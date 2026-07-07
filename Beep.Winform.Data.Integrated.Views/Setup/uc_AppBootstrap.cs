using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.SetUp;
using TheTechIdea.Beep.SetUp.Adapters;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    [AddinAttribute(Caption = "App Bootstrap", Name = "uc_AppBootstrap",
        misc = "Configuration", menu = "Configuration", addinType = AddinType.Control,
        displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 4, RootNodeName = "Configuration", Order = 4, ID = 4,
        BranchText = "App Bootstrap", BranchType = EnumPointType.Function,
        IconImageName = "rocket.svg", BranchClass = "ADDIN",
        BranchDescription = "First-run setup gate: detects new install and runs the setup wizard")]
    public partial class uc_AppBootstrap : TemplateUserControl, IAddinVisSchema
    {
        private readonly IServiceProvider? _services;
        private readonly CancellationTokenSource _cts = new();
        private BeepBootstrapper? _bootstrapper;
        private IFirstRunDetector? _firstRunDetector;
        private bool _bootstrapCompleted;

        private TableLayoutPanel? _rootLayout;
        private BeepLabel? _titleLabel;
        private BeepLabel? _subtitleLabel;
        private BeepLabel? _statusLabel;
        private BeepButton? _quickSetupButton;
        private BeepButton? _skipButton;
        private BeepButton? _enterAppButton;
        private BeepButton? _resetButton;

        public event EventHandler<BootstrapCompletedEventArgs>? BootstrapCompleted;

        public uc_AppBootstrap()
        {
            BuildUi();
        }

        public uc_AppBootstrap(IServiceProvider services) : base(services)
        {
            _services = services;
            BuildUi();
        }

        #region IAddinVisSchema
        public string RootNodeName { get; set; } = "Configuration";
        public string CatgoryName { get; set; } = string.Empty;
        public int Order { get; set; } = 4;
        public int ID { get; set; } = 4;
        public string BranchText { get; set; } = "App Bootstrap";
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        public int BranchID { get; set; } = 4;
        public string IconImageName { get; set; } = "rocket.svg";
        public string BranchStatus { get; set; } = string.Empty;
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; } = "First-run setup gate";
        public string BranchClass { get; set; } = "ADDIN";
        public string AddinName { get; set; } = "uc_AppBootstrap";
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _ = InitializeAsync(_cts.Token);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_bootstrapper != null)
                    _bootstrapper.ProgressChanged -= Bootstrapper_ProgressChanged;
                _cts.Cancel();
                _cts.Dispose();
            }
            base.Dispose(disposing);
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested) return;

                _firstRunDetector = ResolveFirstRunDetector();
                if (_firstRunDetector == null)
                {
                    SetStatus("Cannot detect first-run state. Entering application.");
                    RaiseBootstrapCompleted(true, "No first-run detector available.");
                    return;
                }

                bool isFirstRun = await _firstRunDetector.IsFirstRunAsync();
                if (cancellationToken.IsCancellationRequested) return;

                if (!isFirstRun)
                {
                    SetStatus("Setup already completed. Loading application...");
                    await Task.Delay(500, cancellationToken);
                    RaiseBootstrapCompleted(true, "Not first run.");
                    return;
                }

                _bootstrapper = ResolveBootstrapper();
                if (_bootstrapper != null)
                    _bootstrapper.ProgressChanged += Bootstrapper_ProgressChanged;

                SetStatus("First run detected. Choose how to set up the data platform.");
                ShowWelcome();
            }
            catch (OperationCanceledException) { /* shutdown */ }
            catch (Exception ex)
            {
                SetStatus($"Bootstrap error: {ex.Message}");
                ShowWelcome();
            }
        }

        public async Task ResetAsync()
        {
            if (_firstRunDetector != null)
                await _firstRunDetector.ClearSetupFlagAsync();
            SetStatus("Bootstrap reset. Restart the application to re-run setup.");
        }

        private IFirstRunDetector? ResolveFirstRunDetector()
        {
            if (_services != null)
            {
                var resolved = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions
                    .GetService<IFirstRunDetector>(_services);
                if (resolved != null) return resolved;
            }

            if (Editor == null) return null;
            return new FileBasedFirstRunDetector(Editor);
        }

        private BeepBootstrapper? ResolveBootstrapper()
        {
            if (Editor == null) return null;

            var detector = _firstRunDetector ?? new FileBasedFirstRunDetector(Editor);
            var factory = new DefaultSetupWizardFactory();
            var adapter = new DesktopSetupWizardAdapter(
                progressCallback: _ => { },
                completedCallback: _ => { });

            return new BeepBootstrapper(
                detector,
                factory,
                () => Editor!,
                adapter);
        }

        private void Bootstrapper_ProgressChanged(string message, BootstrapPhase phase)
        {
            SetStatus($"[{phase}] {message}");
        }

        private void SetStatus(string text)
        {
            if (IsDisposed || Disposing) return;
            if (InvokeRequired)
            {
                if (IsHandleCreated) BeginInvoke(() => SetStatus(text));
                return;
            }
            if (_statusLabel != null) _statusLabel.Text = text;
        }

        private void BuildUi()
        {
            // Skill § 10.9: building a single UserControl programmatically (not as a wizard step)
            // is allowed when each instance is rendered as one cohesive layout — the parent
            // Control IS the visual surface. We still bracket with SuspendLayout/ResumeLayout(true).
            SuspendLayout();
            Dock = DockStyle.Fill;
            BackColor = Color.White;
            // Skill § 5.6: dialog padding scales with DPI through BeepLayoutMetrics tokens.
            Padding = BeepLayoutMetrics.DialogPadding.ScalePadding(this);

            _rootLayout = new TableLayoutPanel
            {
                ColumnCount = 1,
                RowCount = 7,
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this)
            };
            _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            // Skill § 1: a single row height token drives every spacer row — change once, update all.
            int spacerRowHeight = BeepLayoutMetrics.ButtonToolbar.Height.ScaleValue(this);
            for (int i = 0; i < 6; i++)
                _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, spacerRowHeight));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _titleLabel = new BeepLabel
            {
                Text = "Welcome to BeepWeb",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point),
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _subtitleLabel = new BeepLabel
            {
                Text = "Looks like this is your first run. Let's set up your data platform.",
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DimGray
            };
            _statusLabel = new BeepLabel
            {
                Text = "Initializing...",
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray
            };
            // Skill § default-size tokens: ButtonLarge for primary CTA, ButtonStandard for secondary, ButtonSmall for utility.
            _quickSetupButton = new BeepButton
            {
                Text = "Quick Setup (5 steps)",
                Dock = DockStyle.Fill,
                Height = BeepLayoutMetrics.ButtonLarge.Height.ScaleValue(this)
            };
            _quickSetupButton.Click += QuickSetupButton_Click;

            _skipButton = new BeepButton
            {
                Text = "Skip for Now",
                Dock = DockStyle.Fill,
                Height = BeepLayoutMetrics.ButtonLarge.Height.ScaleValue(this)
            };
            _skipButton.Click += async (_, _) => await SkipAsync(_cts.Token);

            _resetButton = new BeepButton
            {
                Text = "Reset Setup Flag",
                Dock = DockStyle.Fill,
                Height = BeepLayoutMetrics.ButtonSmall.Height.ScaleValue(this),
                Visible = false
            };
            _resetButton.Click += async (_, _) => await ResetAsync();

            _enterAppButton = new BeepButton
            {
                Text = "Enter Application",
                Dock = DockStyle.Fill,
                Height = BeepLayoutMetrics.ButtonLarge.Height.ScaleValue(this),
                Visible = false
            };
            _enterAppButton.Click += (_, _) => RaiseBootstrapCompleted(true, "User chose to enter app.");

            var buttons = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 1,
                Dock = DockStyle.Fill,
                AutoSize = true
            };
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            buttons.RowStyles.Add(new RowStyle(SizeType.Absolute, BeepLayoutMetrics.ButtonLarge.Height.ScaleValue(this) + BeepLayoutMetrics.ButtonGap.ScaleValue(this)));
            buttons.Controls.Add(_quickSetupButton, 0, 0);
            buttons.Controls.Add(_skipButton, 1, 0);
            buttons.Controls.Add(_enterAppButton, 2, 0);

            _rootLayout.Controls.Add(_titleLabel, 0, 0);
            _rootLayout.Controls.Add(_subtitleLabel, 0, 1);
            _rootLayout.Controls.Add(_statusLabel, 0, 2);
            _rootLayout.Controls.Add(buttons, 0, 3);
            _rootLayout.Controls.Add(_resetButton, 0, 4);

            Controls.Add(_rootLayout);
            ResumeLayout(true);

            // Skill § "Sizing tokens": after the chrome is built, apply DPI-scaled overrides
            // from BeepLayoutMetrics. The usercontrol's own Size stays in sync with the
            // dialog size token; the inner label heights and row gaps follow the design-skill
            // row-height token so they scale with the host's display DPI.
            ApplyDpiScaledLayout();
        }

        /// <summary>
        /// Skill § "Sizing tokens": apply DPI-scaled <see cref="BeepLayoutMetrics"/> values to
        /// chrome created by <see cref="BuildUi"/>. The design-time chrome is built in code;
        /// this overlay keeps heights and padding tracking the host display scale.
        /// </summary>
        private void ApplyDpiScaledLayout()
        {
            int titleHeight   = (int)BeepLayoutMetrics.TitleFontSize + 8;
            int statusHeight  = (int)BeepLayoutMetrics.SubtitleFontSize + 6;
            int buttonHeight  = BeepLayoutMetrics.ButtonLarge.Height.ScaleValue(this);
            int rowHeight     = BeepLayoutMetrics.TextRowHeight.ScaleValue(this);
            int spacerHeight  = BeepLayoutMetrics.InterRowSpacing.ScaleValue(this);

            if (_titleLabel != null)    _titleLabel.Height   = titleHeight;
            if (_subtitleLabel != null) _subtitleLabel.Height = statusHeight;
            if (_statusLabel != null)   _statusLabel.Height  = statusHeight;
            if (_quickSetupButton != null) _quickSetupButton.Height = Math.Max(_quickSetupButton.Height, buttonHeight);
            if (_skipButton != null)       _skipButton.Height       = Math.Max(_skipButton.Height, buttonHeight);
            if (_enterAppButton != null)   _enterAppButton.Height   = Math.Max(_enterAppButton.Height, buttonHeight);
            if (_resetButton != null)      _resetButton.Height      = Math.Max(_resetButton.Height, buttonHeight);

            // Apply consistent row heights to the TableLayoutPanel so the layout scales.
            for (int i = 0; i < _rootLayout.RowStyles.Count; i++)
            {
                if (i < 3) _rootLayout.RowStyles[i].Height = rowHeight;
                else if (i < 5) _rootLayout.RowStyles[i].Height = (float)spacerHeight;
            }
        }

        private void ShowWelcome()
        {
            if (IsDisposed || Disposing) return;
            if (InvokeRequired)
            {
                if (IsHandleCreated) BeginInvoke(ShowWelcome);
                return;
            }
            _quickSetupButton!.Visible = true;
            _skipButton!.Visible = true;
            _enterAppButton!.Visible = false;
            _resetButton!.Visible = true;
        }

        private void ShowDone()
        {
            if (IsDisposed || Disposing) return;
            if (InvokeRequired)
            {
                if (IsHandleCreated) BeginInvoke(ShowDone);
                return;
            }
            _quickSetupButton!.Visible = false;
            _skipButton!.Visible = false;
            _enterAppButton!.Visible = true;
            _resetButton!.Visible = false;
        }

        private async void QuickSetupButton_Click(object? sender, EventArgs e)
        {
            try
            {
                _quickSetupButton!.Enabled = false;
                _skipButton!.Enabled = false;
                SetStatus("Running setup wizard...");

                if (_bootstrapper == null)
                {
                    SetStatus("Bootstrapper not initialised.");
                    _quickSetupButton.Enabled = true;
                    _skipButton.Enabled = true;
                    return;
                }

                var result = await _bootstrapper.BootstrapAsync(_cts.Token);
                if (result.Succeeded)
                {
                    SetStatus($"Setup complete in {result.TotalElapsed.TotalSeconds:F1}s.");
                    ShowDone();
                    await Task.Delay(800, _cts.Token);
                    RaiseBootstrapCompleted(true, "Quick setup completed.");
                }
                else
                {
                    SetStatus($"Setup failed: {result.FailureMessage}");
                    _quickSetupButton.Enabled = true;
                    _skipButton.Enabled = true;
                }
            }
            catch (OperationCanceledException) { /* shutdown */ }
            catch (Exception ex)
            {
                SetStatus($"Setup error: {ex.Message}");
                _quickSetupButton!.Enabled = true;
                _skipButton!.Enabled = true;
            }
        }

        private async Task SkipAsync(CancellationToken cancellationToken)
        {
            try
            {
                SetStatus("Marking setup complete...");
                if (_firstRunDetector != null)
                    await _firstRunDetector.MarkSetupCompleteAsync();
                if (!cancellationToken.IsCancellationRequested)
                    RaiseBootstrapCompleted(true, "User skipped setup.");
            }
            catch (OperationCanceledException) { /* shutdown */ }
        }

        private void RaiseBootstrapCompleted(bool success, string message)
        {
            if (_bootstrapCompleted) return;
            _bootstrapCompleted = true;
            BootstrapCompleted?.Invoke(this, new BootstrapCompletedEventArgs(success, message));
        }

        public override void Configure(Dictionary<string, object> settings) { base.Configure(settings); }
    }

    public class BootstrapCompletedEventArgs : EventArgs
    {
        public bool Success { get; }
        public string Message { get; }

        public BootstrapCompletedEventArgs(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}
