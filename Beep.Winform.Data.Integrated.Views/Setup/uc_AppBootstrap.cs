using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.SetUp;
using TheTechIdea.Beep.SetUp.Adapters;
using TheTechIdea.Beep.SetUp.Steps;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    /// <summary>
    /// First-run bootstrap user control.
    /// On a fresh install, shows a Welcome screen with three options:
    ///   1. Quick Setup    - delegates to <see cref="ApplicationBootstrapper"/> which runs the 5-step
    ///                       setup wizard (Driver → Connection → Schema → Seed → Run).
    ///   2. Skip for Now   - marks the setup complete via the IFirstRunDetector and raises BootstrapCompleted.
    ///   3. Enter App      - same as Skip.
    /// On subsequent launches, the marker exists, the wizard is bypassed, and the host
    /// form continues normal startup immediately.
    ///
    /// All UI updates from async/background work are marshalled to the UI thread via
    /// <see cref="Control.InvokeRequired"/> + <see cref="Control.BeginInvoke(Action)"/>.
    /// </summary>
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
        private readonly BootstrapState _state;
        private readonly CancellationTokenSource _cts = new();
        private ApplicationBootstrapper? _bootstrapper;
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
            _state = new BootstrapState(new FileBasedFirstRunDetector(Editor ?? throw new InvalidOperationException(
                "uc_AppBootstrap must be constructed with an IServiceProvider or after Editor is set.")));
            BuildUi();
        }

        public uc_AppBootstrap(IServiceProvider services) : base(services)
        {
            _services = services;
            _state = BootstrapState.Resolve(services, Editor!);
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
                _state.Dispose();
            }
            base.Dispose(disposing);
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested) return;

                _bootstrapper = ResolveBootstrapper();
                _bootstrapper.ProgressChanged += Bootstrapper_ProgressChanged;

                await _state.InitializeAsync();
                if (cancellationToken.IsCancellationRequested) return;

                if (!_state.IsFirstRun)
                {
                    SetStatus("Setup already completed. Loading application...");
                    await Task.Delay(500, cancellationToken);
                    RaiseBootstrapCompleted(true, "Not first run.");
                    return;
                }

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
            await _state.ResetAsync();
            SetStatus("Bootstrap reset. Restart the application to re-run setup.");
        }

        private ApplicationBootstrapper ResolveBootstrapper()
        {
            if (_services != null)
            {
                var resolved = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions
                    .GetService<ApplicationBootstrapper>(_services);
                if (resolved != null) return resolved;
            }

            if (Editor == null)
                throw new InvalidOperationException("Cannot resolve bootstrapper: no Editor and no IServiceProvider.");

            var detector = new FileBasedFirstRunDetector(Editor);
            var factory = new DefaultSetupWizardFactory();
            var (wizard, ctx) = BootstrapWizardBuilder.BuildForFirstRun(factory, Editor);
            var adapter = new DesktopSetupWizardAdapter(
                progressCallback: args => { /* progress UI handled by ProgressChanged event */ },
                completedCallback: _ => { /* completion handled by ApplicationBootstrapper */ });
            return new ApplicationBootstrapper(detector, wizard, ctx, adapter);
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
            SuspendLayout();
            Dock = DockStyle.Fill;
            BackColor = Color.White;
            Padding = new Padding(24);

            _rootLayout = new TableLayoutPanel
            {
                ColumnCount = 1,
                RowCount = 7,
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(20)
            };
            _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 6; i++)
                _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
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
            _quickSetupButton = new BeepButton
            {
                Text = "Quick Setup (5 steps)",
                Dock = DockStyle.Fill,
                Height = 40
            };
            _quickSetupButton.Click += QuickSetupButton_Click;

            _skipButton = new BeepButton
            {
                Text = "Skip for Now",
                Dock = DockStyle.Fill,
                Height = 40
            };
            _skipButton.Click += async (_, _) => await SkipAsync(_cts.Token);

            _resetButton = new BeepButton
            {
                Text = "Reset Setup Flag",
                Dock = DockStyle.Fill,
                Height = 30,
                Visible = false
            };
            _resetButton.Click += async (_, _) => await ResetAsync();

            _enterAppButton = new BeepButton
            {
                Text = "Enter Application",
                Dock = DockStyle.Fill,
                Height = 40,
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
            buttons.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
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
                await _state.MarkSetupCompleteAsync();
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
