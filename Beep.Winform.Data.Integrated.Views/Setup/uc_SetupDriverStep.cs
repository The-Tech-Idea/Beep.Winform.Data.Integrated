using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Default.Views.NuggetsManage;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    public partial class uc_SetupDriverStep : UserControl
    {
        private Control? _embeddedControl;

        public event EventHandler<DriverPackageInstalledEventArgs>? DriverPackageInstalled;

        public uc_SetupDriverStep()
        {
            InitializeComponent();
        }

        public void InitializeStep(IServiceProvider? services, string theme)
        {
            _contentHost.Controls.Clear();
            _embeddedControl = null;

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
                _lblFallback1.Visible = true;
                _lblFallback2.Visible = true;
                _lblFallback3.Visible = true;
            }

            ApplyTheme(theme);
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
                return "Driver Provision: Nuggets manager loaded (interactive mode).";

            return "Driver Provision: Checklist mode (service provider unavailable).";
        }

        private static void ApplyThemeToControl(Control control, string theme)
        {
            if (control is IBeepUIComponent beepComponent)
                beepComponent.Theme = theme;
        }

        private void Nuggets_PackageInstallCompleted(object? sender, uc_NuggetsManage.NuggetInstallCompletedEventArgs e)
        {
            DriverPackageInstalled?.Invoke(this,
                new DriverPackageInstalledEventArgs(e.PackageId, e.Version, e.Success, e.Message));
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
