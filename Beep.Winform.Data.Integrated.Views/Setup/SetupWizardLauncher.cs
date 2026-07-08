using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.SetUp;
using TheTechIdea.Beep.Winform.Controls.Wizards;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    /// <summary>
    /// Coordinates first-run detection, setup wizard, and import/export.
    /// All wizards open as modal popups via WizardManager — never embedded as tabs.
    /// </summary>
    public class SetupWizardLauncher
    {
        private readonly IServiceProvider? _services;
        private readonly IDMEEditor? _editor;
        private readonly IWin32Window _owner;
        private IFirstRunDetector? _detector;

        public SetupWizardLauncher(IServiceProvider? services, IDMEEditor? editor, IWin32Window owner)
        {
            _services = services;
            _editor = editor;
            _owner = owner;
        }

        /// <summary>
        /// Detects first run and shows the setup wizard as a modal popup if needed.
        /// Returns true if the wizard was shown and completed successfully.
        /// </summary>
        public async Task<bool> TryShowFirstRunAsync()
        {
            if (_editor == null) return false;

            _detector ??= _services != null
                ? _services.GetService(typeof(IFirstRunDetector)) as IFirstRunDetector
                    ?? new FileBasedFirstRunDetector(_editor)
                : new FileBasedFirstRunDetector(_editor);

            try
            {
                if (!await _detector.IsFirstRunAsync())
                    return false;
            }
            catch { return false; }

            // Ask user directly — no intermediate welcome page
            var result = MessageBox.Show(
                _owner as IWin32Window,
                "Welcome! This appears to be your first run.\n\nWould you like to run the setup wizard to configure your data platform?",
                "First Run Setup",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ShowSetupWizard();
                await _detector.MarkSetupCompleteAsync();
                return true;
            }

            // User chose No — mark complete so we don't ask again
            await _detector.MarkSetupCompleteAsync();
            return false;
        }

        /// <summary>
        /// Shows the setup wizard as a modal popup using the Beep WinForms Wizard framework.
        /// </summary>
        public void ShowSetupWizard()
        {
            var wizard = new uc_SetupWizard(_services);
            // uc_SetupWizard now exposes a WizardConfig that WizardManager can show
            var config = wizard.BuildWizardConfig();
            if (config != null)
            {
                config.Key = "platform-setup-winform";
                config.Title = "Platform Setup";
                WizardManager.ShowWizard(config, _owner);
            }
        }

        /// <summary>
        /// Shows the import/export launcher as a modal popup.
        /// </summary>
        public void ShowImportExport()
        {
            // Import/Export will use the same WizardManager.ShowWizard pattern
            // once uc_ImportExportLauncher is refactored to expose BuildWizardConfig().
            MessageBox.Show(_owner as IWin32Window,
                "Import/Export will open as a wizard popup.",
                "Import / Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
