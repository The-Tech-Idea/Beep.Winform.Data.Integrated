using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.SetUp;
using TheTechIdea.Beep.SetUp.Steps;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Default.Views.NuggetsManage;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    // Full refactor: UI step wraps a typed DriverProvisionStepOptions directly.
    // The canonical DriverProvisionStep reads options.PackageName / options.Version
    // during Execute, so UI mutations take effect immediately.
    public partial class uc_SetupDriverStep : TemplateUserControl
    {
        private Control? _embeddedControl;
        private IDMEEditor? _editor;
        private DriverProvisionStepOptions? _options;
        private SetupContext? _context;

        private string _lastPackageId = string.Empty;
        private string _lastPackageVersion = string.Empty;
        private bool _lastInstallSuccess;
        private string _lastInstallMessage = string.Empty;

        /// <summary>
        /// Packages the wizard staged from <c>ConfigEditor.DataDriversClasses</c>
        /// (one <c>DriverProvisionStep</c> per package). Surfaced to the UI so the
        /// user can see which drivers are queued for install without manually
        /// searching the nuggets manager.
        /// </summary>
        private IReadOnlyList<string> _stagedPackages = Array.Empty<string>();

        public event EventHandler<DriverPackageInstalledEventArgs>? DriverPackageInstalled;

        public sealed class DriverPackageInstalledEventArgs : EventArgs
        {
            public DriverPackageInstalledEventArgs(string packageId, string version, bool success, string message)
            {
                PackageId = packageId;
                Version = version;
                Success = success;
                Message = message;
            }

            public string PackageId { get; }
            public string Version { get; }
            public bool Success { get; }
            public string Message { get; }
        }

        public uc_SetupDriverStep()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Bind the nuggets UI to the typed <see cref="DriverProvisionStepOptions"/>.
        /// The wizard host passes the same options object that will be handed to the canonical
        /// <see cref="DriverProvisionStep"/> at Execute time.
        /// </summary>
        public void InitializeStep(DriverProvisionStepOptions options, SetupContext context, IDMEEditor? editor = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            if (editor != null) _editor = editor;

            DisposeEmbeddedControl();

            // Build an IServiceProvider the nuggets manager can consume.
            IServiceProvider? services = null;
            if (_editor?.assemblyHandler != null)
            {
                var collection = new ServiceCollection();
                collection.AddSingleton(_editor.assemblyHandler);
                services = collection.BuildServiceProvider();
            }

            if (services != null)
            {
                var nuggets = new uc_NuggetsManage(services) { Dock = DockStyle.Fill };
                nuggets.PackageInstallCompleted += Nuggets_PackageInstallCompleted;
                nuggets.OnNavigatedTo(new Dictionary<string, object>());
                _embeddedControl = nuggets;
                _contentHost.Controls.Add(nuggets);

                _lblFallback1.Visible = false;
                _lblFallback2.Visible = false;
                _lblFallback3.Visible = false;
            }
            else
            {
                _embeddedControl = null;
                _lblFallback1.Visible = true;
                _lblFallback2.Visible = true;
                _lblFallback3.Visible = true;
            }
        }

        public string LastPackageId => _lastPackageId;
        public string LastPackageVersion => _lastPackageVersion;
        public IReadOnlyList<string> StagedPackages => _stagedPackages;

        /// <summary>
        /// Sets the list of packages the wizard staged for this step.
        /// Mirrors Blazor <c>BeepSetupWizardRunner</c>'s <c>DriverPackageNames</c> loop —
        /// the wizard builds one <c>DriverProvisionStep</c> per package and this UI
        /// surfaces the same list so the user knows what will be installed.
        /// </summary>
        public void SetStagedPackages(IReadOnlyList<string> packages)
        {
            _stagedPackages = packages ?? Array.Empty<string>();
            UpdateStagedPackagesDisplay();
        }

        private void UpdateStagedPackagesDisplay()
        {
            if (_lblStagedPackages == null) return;
            if (_stagedPackages.Count == 0)
            {
                _lblStagedPackages.Text = "No packages staged from ConfigEditor.";
                return;
            }
            _lblStagedPackages.Text =
                "Staged drivers (from ConfigEditor.DataDriversClasses): "
                + string.Join(", ", _stagedPackages);
        }

        private void DisposeEmbeddedControl()
        {
            if (_embeddedControl == null) return;

            _contentHost.Controls.Clear();
            if (_embeddedControl is uc_NuggetsManage nuggets)
                nuggets.PackageInstallCompleted -= Nuggets_PackageInstallCompleted;

            _embeddedControl.Dispose();
            _embeddedControl = null;
        }

        private void Nuggets_PackageInstallCompleted(object? sender, NuggetInstallCompletedEventArgs e)
        {
            _lastPackageId = e.PackageId ?? string.Empty;
            _lastPackageVersion = e.Version ?? string.Empty;
            _lastInstallSuccess = e.Success;
            _lastInstallMessage = e.Message ?? string.Empty;

            // Skill: write the installed package DIRECTLY into the typed options, so the
            // canonical DriverProvisionStep reads it during Execute. No dictionary, no reflection.
            if (_options != null)
            {
                _options.PackageName = _lastPackageId;
                _options.Version = _lastPackageVersion;
            }

            DriverPackageInstalled?.Invoke(this,
                new DriverPackageInstalledEventArgs(_lastPackageId, _lastPackageVersion, _lastInstallSuccess, _lastInstallMessage));
        }

        /// <summary>
        /// Overlays DPI-scaled padding on the Designer's design-time pixels.
        /// </summary>
        /// <remarks>
        /// Invoked by TemplateUserControl from OnHandleCreated and OnDpiChangedAfterParent — never
        /// from the ctor, where DpiScalingHelper reports a scale of 1.0 because the handle does not
        /// exist yet and nothing would actually scale.
        /// <para>
        /// Only the docked panels' padding is scaled. Their children are all Dock=Top/Fill, so the
        /// layout reflows from the padding alone; pushing size tokens onto individual controls is
        /// what broke uc_ImportStep5_Run, whose Designer positions its row absolutely.
        /// </para>
        /// </remarks>
        protected override void ApplyDpiScaledLayout()
        {
            _rootPanel.Padding = BeepLayoutMetrics.DialogPadding.ScalePadding(this);
            _contentHost.Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this);
        }

    }
}
