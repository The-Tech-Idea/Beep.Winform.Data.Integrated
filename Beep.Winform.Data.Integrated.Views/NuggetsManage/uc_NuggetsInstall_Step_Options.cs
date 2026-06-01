using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Controls.Wizards.Forms;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    public partial class uc_NuggetsInstall_Step_Options : WizardPage
    {
        public uc_NuggetsInstall_Step_Options()
        {
            InitializeComponent();
            Title = "Options";
            Description = "Configure install options";
            IsComplete = true;
        }

        public override void OnStepEnter(WizardContext context)
        {
            base.OnStepEnter(context);
            _chkLoadAfterInstall.CurrentValue = context.GetValue(NuggetWizardKeys.LoadAfterInstall, true);
            _chkSharedContext.CurrentValue = context.GetValue(NuggetWizardKeys.SharedContext, true);
            _chkUseProcessHost.CurrentValue = context.GetValue(NuggetWizardKeys.UseProcessHost, false);
            _txtInstallPath.Text = context.GetValue(NuggetWizardKeys.InstallPath, string.Empty);
        }

        public override void OnStepLeave(WizardContext context)
        {
            context.SetValue(NuggetWizardKeys.LoadAfterInstall, _chkLoadAfterInstall.CurrentValue);
            context.SetValue(NuggetWizardKeys.SharedContext, _chkSharedContext.CurrentValue);
            context.SetValue(NuggetWizardKeys.UseProcessHost, _chkUseProcessHost.CurrentValue);
            context.SetValue(NuggetWizardKeys.InstallPath, _txtInstallPath.Text.Trim());
            base.OnStepLeave(context);
        }

        private void BtnBrowse_Click(object? sender, System.EventArgs e)
        {
            using var dlg = new FolderBrowserDialog { Description = "Select install folder" };
            if (dlg.ShowDialog(this) == DialogResult.OK)
                _txtInstallPath.Text = dlg.SelectedPath;
        }
    }
}
