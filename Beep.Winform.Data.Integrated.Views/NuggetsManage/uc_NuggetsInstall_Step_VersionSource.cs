using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Controls.Wizards.Forms;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    public partial class uc_NuggetsInstall_Step_VersionSource : WizardPage
    {
        private CancellationTokenSource? _versionCts;

        public uc_NuggetsInstall_Step_VersionSource()
        {
            InitializeComponent();
            Title = "Version & Source";
            Description = "Choose the version and NuGet source";
        }

        public override void OnStepEnter(WizardContext context)
        {
            base.OnStepEnter(context);

            _versionCts?.Cancel();
            _versionCts?.Dispose();
            _versionCts = new CancellationTokenSource();

            var pkg = context.GetValue(NuggetWizardKeys.PackageId, string.Empty);
            var pre = context.GetValue(NuggetWizardKeys.IncludePrerelease, false);

            _lblPackageId.Text = $"Package:  {pkg}";
            _chkPrerelease.CurrentValue = pre;

            var svc = context.GetValue<NuggetsManageService>(NuggetWizardKeys.Service, null!);
            var sources = svc.GetAllSources().Where(s => s.IsEnabled).ToList();
            _cmbSource.ListItems = new BindingList<SimpleItem>(
                sources.Select(s => new SimpleItem { Text = s.Name, Item = s.Url }).ToList());
            if (_cmbSource.ListItems.Count > 0) _cmbSource.SelectedIndex = 0;

            _ = LoadVersionsAsync(svc, pkg, pre, _versionCts.Token);
        }

        public override void OnStepLeave(WizardContext context)
        {
            _versionCts?.Cancel();
            context.SetValue(NuggetWizardKeys.SelectedVersion, _cmbVersion.SelectedItem?.Text ?? string.Empty);
            context.SetValue(NuggetWizardKeys.SelectedSourceUrl, _cmbSource.SelectedItem?.Item?.ToString() ?? string.Empty);
            context.SetValue(NuggetWizardKeys.IncludePrerelease, _chkPrerelease.CurrentValue);
            base.OnStepLeave(context);
        }

        public override WizardValidationResult Validate()
        {
            if (_cmbVersion.SelectedItem == null) return WizardValidationResult.Error("Please select a version.");
            if (_cmbSource.SelectedItem == null) return WizardValidationResult.Error("Please select a source.");
            return WizardValidationResult.Success();
        }

        private async Task LoadVersionsAsync(NuggetsManageService svc, string pkg, bool pre, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(pkg)) return;
            try
            {
                var versions = await svc.GetVersionsAsync(pkg, pre, ct);
                ct.ThrowIfCancellationRequested();
                _cmbVersion.ListItems = new BindingList<SimpleItem>(
                    versions.Select(v => new SimpleItem { Text = v, Item = v }).ToList());
                if (_cmbVersion.ListItems.Count > 0) _cmbVersion.SelectedIndex = 0;
                IsComplete = true;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { RaiseValidationStateChanged(false, ex.Message); }
        }

        private void ChkPrerelease_StateChanged(object? sender, EventArgs e)
        {
            _versionCts?.Cancel();
            _versionCts?.Dispose();
            _versionCts = new CancellationTokenSource();
            if (Context == null) return;
            var svc = Context.GetValue<NuggetsManageService>(NuggetWizardKeys.Service, null!);
            var pkg = Context.GetValue(NuggetWizardKeys.PackageId, string.Empty);
            _ = LoadVersionsAsync(svc, pkg, _chkPrerelease.CurrentValue, _versionCts.Token);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _versionCts?.Cancel(); _versionCts?.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
