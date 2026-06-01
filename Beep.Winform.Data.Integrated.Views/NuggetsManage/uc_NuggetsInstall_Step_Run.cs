using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Controls.Wizards.Forms;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    public partial class uc_NuggetsInstall_Step_Run : WizardPage
    {
        public uc_NuggetsInstall_Step_Run()
        {
            InitializeComponent();
            Title = "Install";
            Description = "Review and install";
            NextButtonText = "Install";
        }

        public override void OnStepEnter(WizardContext context)
        {
            base.OnStepEnter(context);
            IsComplete = false;

            _lblSummary.Text =
                $"Package:   {context.GetValue(NuggetWizardKeys.PackageId, string.Empty)}\n" +
                $"Version:   {context.GetValue(NuggetWizardKeys.SelectedVersion, string.Empty)}\n" +
                $"Source:    {context.GetValue(NuggetWizardKeys.SelectedSourceUrl, string.Empty)}\n" +
                $"Load now:  {(context.GetValue(NuggetWizardKeys.LoadAfterInstall, true) ? "Yes" : "No")}\n" +
                $"Shared ctx:{(context.GetValue(NuggetWizardKeys.SharedContext, true) ? "Yes" : "No")}\n" +
                $"Path:      {context.GetValue(NuggetWizardKeys.InstallPath, string.Empty)}";
            _lblStatus.Text = "Click Install to proceed.";
        }

        public override async Task<WizardValidationResult> ValidateAsync()
        {
            if (IsComplete) return WizardValidationResult.Success();
            return await RunInstallAsync();
        }

        private async Task<WizardValidationResult> RunInstallAsync()
        {
            var svc = Context?.GetValue<NuggetsManageService>(NuggetWizardKeys.Service, null!);
            if (svc == null || Context == null) return WizardValidationResult.Error("Service not available.");

            var request = new NuggetInstallRequest
            {
                PackageId = Context.GetValue(NuggetWizardKeys.PackageId, string.Empty),
                Version = Context.GetValue(NuggetWizardKeys.SelectedVersion, string.Empty),
                Sources = new List<string> { Context.GetValue(NuggetWizardKeys.SelectedSourceUrl, string.Empty) },
                LoadAfterInstall = Context.GetValue(NuggetWizardKeys.LoadAfterInstall, true),
                UseSingleSharedContext = Context.GetValue(NuggetWizardKeys.SharedContext, true),
                UseProcessHost = Context.GetValue(NuggetWizardKeys.UseProcessHost, false),
                AppInstallPath = Context.GetValue(NuggetWizardKeys.InstallPath, string.Empty)
            };

            _progressBar.Visible = true;
            _lblStatus.Text = $"Installing {request.PackageId} {request.Version}...";
            var progress = new Progress<string>(msg => _lblStatus.Text = msg);

            try
            {
                var result = await svc.InstallAsync(request, progress);
                IsComplete = result.Success;
                _lblStatus.Text = result.Message;
                Context.SetValue(NuggetWizardKeys.InstallResult, result);
                return result.Success ? WizardValidationResult.Success() : WizardValidationResult.Error(result.Message);
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"Install failed: {ex.Message}";
                RaiseValidationStateChanged(false, ex.Message);
                return WizardValidationResult.Error(ex.Message);
            }
            finally { _progressBar.Visible = false; }
        }
    }
}
