using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Default.Views.NuggetsManage;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    public partial class uc_SetupDriverStep : UserControl
    {
        private Control? _embeddedControl;
        private IDMEEditor? _editor;
        private string _lastPackageId = string.Empty;
        private string _lastPackageVersion = string.Empty;
        private bool _lastInstallSuccess;
        private string _lastInstallMessage = string.Empty;

        public event EventHandler<DriverPackageInstalledEventArgs>? DriverPackageInstalled;
        public event EventHandler<DatasourceSetupResult>? DatasourceSetupCompleted;

        public uc_SetupDriverStep()
        {
            InitializeComponent();
        }

        public void InitializeStep(IServiceProvider? services, string theme)
        {
            DisposeEmbeddedControl();

            if (services != null)
            {
                var nuggets = new uc_NuggetsManage(services)
                {
                    Dock = DockStyle.Fill
                };

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

            ApplyTheme(theme);
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

        public void ApplyTheme(string theme)
        {
            ApplyThemeToControl(_rootPanel, theme);
            ApplyThemeToControl(_headerPanel, theme);
            ApplyThemeToControl(_contentHost, theme);
            ApplyThemeToControl(_lblTitle, theme);
            ApplyThemeToControl(_lblDescription, theme);
            ApplyThemeToControl(_lblFallback1, theme);
            ApplyThemeToControl(_lblFallback2, theme);
            ApplyThemeToControl(_lblFallback3, theme);

            if (_embeddedControl is IBeepUIComponent beepControl)
                beepControl.Theme = theme;
        }

        public string GetStepSummary()
        {
            if (_embeddedControl is uc_NuggetsManage)
            {
                if (!string.IsNullOrWhiteSpace(_lastPackageId))
                {
                    return _lastInstallSuccess
                        ? $"Driver Provision: Installed {_lastPackageId} {_lastPackageVersion}."
                        : $"Driver Provision: Failed to install {_lastPackageId} {_lastPackageVersion} - {_lastInstallMessage}.";
                }

                return "Driver Provision: Nuggets manager loaded (interactive mode).";
            }

            return "Driver Provision: Checklist mode (service provider unavailable).";
        }

        public string LastPackageId => _lastPackageId;
        public string LastPackageVersion => _lastPackageVersion;

        /// <summary>
        /// Run the datasource setup pipeline through <see cref="DatasourceSetupHandler"/>
        /// using the connection properties provided by the connection step.
        /// </summary>
        public async Task<DatasourceSetupResult> SetupDatasourceAsync(
            IDMEEditor editor,
            ConnectionProperties connectionProperties,
            IProgress<(int Percent, string Message)>? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (editor == null) throw new ArgumentNullException(nameof(editor));
            if (connectionProperties == null) throw new ArgumentNullException(nameof(connectionProperties));

            _editor = editor;

            var handler = new DatasourceSetupHandler(editor);
            var options = new DatasourceSetupOptions
            {
                ConnectionProperties = connectionProperties,
                ProvisionDriverIfMissing = true,
                OpenConnection = true
            };

            var result = await handler.SetupAsync(options, progress, cancellationToken).ConfigureAwait(true);
            DatasourceSetupCompleted?.Invoke(this, result);
            return result;
        }

        public void ResetPackageState()
        {
            _lastPackageId = string.Empty;
            _lastPackageVersion = string.Empty;
            _lastInstallSuccess = false;
            _lastInstallMessage = string.Empty;
        }

        private static void ApplyThemeToControl(Control control, string theme)
        {
            if (control is IBeepUIComponent beepComponent)
                beepComponent.Theme = theme;
        }

        private void Nuggets_PackageInstallCompleted(object? sender, NuggetInstallCompletedEventArgs e)
        {
            _lastPackageId = e.PackageId ?? string.Empty;
            _lastPackageVersion = e.Version ?? string.Empty;
            _lastInstallSuccess = e.Success;
            _lastInstallMessage = e.Message ?? string.Empty;

            DriverPackageInstalled?.Invoke(this,
                new DriverPackageInstalledEventArgs(_lastPackageId, _lastPackageVersion, _lastInstallSuccess, _lastInstallMessage));
        }

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
    }
}
