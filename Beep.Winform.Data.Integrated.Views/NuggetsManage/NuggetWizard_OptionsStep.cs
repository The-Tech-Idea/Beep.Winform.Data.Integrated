using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Controls.Wizards.Forms;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    /// <summary>
    /// Step 2: Version and install options.
    /// All work goes through <see cref="IAssemblyHandler"/>.
    /// </summary>
    public class NuggetWizard_OptionsStep : WizardPage
    {
        private BeepLabel _lblPackage; private BeepLabel _lblVersion; private BeepComboBox _cmbVersion;
        private BeepLabel _lblPath;    private BeepTextBox _txtPath;  private BeepButton _btnBrowse;
        private BeepCheckBoxBool _chkLoad; private BeepCheckBoxBool _chkShared; private BeepCheckBoxBool _chkProcess;
        private BeepLabel _lblStatus;

        private CancellationTokenSource? _cts;

        public NuggetWizard_OptionsStep()
        {
            Title = "Version & Options";
            Description = "Pick a version and configure how the package will be loaded.";
            BuildLayout();
        }

        private IAssemblyHandler? Handler
            => Context?.GetValue<IAssemblyHandler>(NuggetWizardKeys.Handler, null!);
        private NuggetActivityLog? Log
            => Context?.GetValue<NuggetActivityLog>(NuggetWizardKeys.Log, null!);

        private void BuildLayout()
        {
            Dock = DockStyle.Fill;
            Padding = new Padding(12);
            var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6 };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            _lblPackage = new BeepLabel { Text = "Package:", AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 6, 6, 6) };
            t.SetColumnSpan(_lblPackage, 2);

            _lblVersion = new BeepLabel { Text = "Version:", AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 6) };
            _cmbVersion  = new BeepComboBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 6) };
            _cmbVersion.SelectedIndexChanged += (s, e) => IsComplete = _cmbVersion.SelectedItem != null;
            t.Controls.Add(_lblVersion, 0, 1);
            t.Controls.Add(_cmbVersion,  1, 1);

            _lblPath = new BeepLabel { Text = "Install path:", AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 6) };
            var pathPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            pathPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pathPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            _txtPath  = new BeepTextBox { Dock = DockStyle.Fill, PlaceholderText = "(default)", Margin = new Padding(0, 0, 4, 0) };
            _btnBrowse = new BeepButton { Text = "Browse…", Dock = DockStyle.Fill };
            _btnBrowse.Click += (s, e) =>
            {
                using var dlg = new FolderBrowserDialog { Description = "Select install folder" };
                if (dlg.ShowDialog(this) == DialogResult.OK) _txtPath.Text = dlg.SelectedPath;
            };
            pathPanel.Controls.Add(_txtPath,  0, 0);
            pathPanel.Controls.Add(_btnBrowse, 1, 0);
            t.Controls.Add(_lblPath, 0, 2);
            t.Controls.Add(pathPanel, 1, 2);

            _chkLoad    = new BeepCheckBoxBool { Text = "Load after install",  AutoSize = true, Dock = DockStyle.Fill, CurrentValue = true,  Margin = new Padding(0, 4, 0, 4) };
            _chkShared  = new BeepCheckBoxBool { Text = "Use single shared context", AutoSize = true, Dock = DockStyle.Fill, CurrentValue = true,  Margin = new Padding(0, 0, 0, 4) };
            _chkProcess = new BeepCheckBoxBool { Text = "Use process host",   AutoSize = true, Dock = DockStyle.Fill, CurrentValue = false, Margin = new Padding(0, 0, 0, 4) };
            t.SetColumnSpan(_chkLoad, 2);    t.Controls.Add(_chkLoad,    0, 3);
            t.SetColumnSpan(_chkShared, 2);  t.Controls.Add(_chkShared,  0, 4);
            t.SetColumnSpan(_chkProcess, 2); t.Controls.Add(_chkProcess, 0, 5);

            _lblStatus = new BeepLabel { Text = string.Empty, AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 0) };
            // Status bar pinned at bottom
            _lblStatus.Dock = DockStyle.Bottom;
            _lblStatus.Height = 22;
            _lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblStatus.Padding = new Padding(4, 0, 0, 0);

            Controls.Add(t);
            Controls.Add(_lblStatus);
        }

        public override void OnStepEnter(WizardContext context)
        {
            base.OnStepEnter(context);
            var h = Handler; if (h == null) return;
            var pkg = context.GetValue(NuggetWizardKeys.PackageId, string.Empty);
            var pre = context.GetValue(NuggetWizardKeys.IncludePre, false);

            _lblPackage.Text = $"Package:  {pkg}";

            _txtPath.Text         = context.GetValue(NuggetWizardKeys.InstallPath, string.Empty);
            _chkLoad.CurrentValue    = context.GetValue(NuggetWizardKeys.LoadAfter,    true);
            _chkShared.CurrentValue  = context.GetValue(NuggetWizardKeys.SharedCtx,    true);
            _chkProcess.CurrentValue = context.GetValue(NuggetWizardKeys.ProcessHost,  false);

            _cts?.Cancel(); _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _ = LoadVersionsAsync(h, pkg, pre, _cts.Token);
        }

        public override void OnStepLeave(WizardContext context)
        {
            base.OnStepLeave(context);
            context.SetValue(NuggetWizardKeys.Version,     _cmbVersion.SelectedItem?.Text ?? string.Empty);
            context.SetValue(NuggetWizardKeys.InstallPath, _txtPath.Text?.Trim() ?? string.Empty);
            context.SetValue(NuggetWizardKeys.LoadAfter,   _chkLoad.CurrentValue);
            context.SetValue(NuggetWizardKeys.SharedCtx,   _chkShared.CurrentValue);
            context.SetValue(NuggetWizardKeys.ProcessHost, _chkProcess.CurrentValue);
        }

        public override WizardValidationResult Validate()
        {
            if (_cmbVersion.SelectedItem == null)
                return WizardValidationResult.Error("Please select a version.");
            return WizardValidationResult.Success();
        }

        private async Task LoadVersionsAsync(IAssemblyHandler? h, string pkg, bool pre, CancellationToken ct)
        {
            if (h == null || string.IsNullOrWhiteSpace(pkg)) return;
            if (IsDisposed || Disposing) return;
            var preservedVersion = Context?.GetValue(NuggetWizardKeys.Version, string.Empty) ?? string.Empty;
            _lblStatus.Text = "Loading versions…";
            try
            {
                var versions = await h.GetNuGetPackageVersionsAsync(pkg, pre, ct);
                ct.ThrowIfCancellationRequested();
                if (IsDisposed || Disposing) return;
                var items = versions.Select(v => new SimpleItem { Text = v, Item = v }).ToList();
                _cmbVersion.ListItems = new BindingList<SimpleItem>(items);
                if (items.Count > 0)
                {
                    // Restore the version the user had selected in a previous visit
                    // (back/forward navigation or re-entry). Fall back to the first
                    // (typically latest) version when nothing is preserved.
                    int idx = -1;
                    if (!string.IsNullOrWhiteSpace(preservedVersion))
                    {
                        idx = items.FindIndex(i =>
                            string.Equals(i.Text, preservedVersion, StringComparison.OrdinalIgnoreCase));
                    }
                    _cmbVersion.SelectedIndex = idx >= 0 ? idx : 0;
                }
                IsComplete = _cmbVersion.SelectedItem != null;
                _lblStatus.Text = $"{versions.Count} versions available.";
                Log?.Info($"Loaded {versions.Count} versions for {pkg}.", pkg);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing)
                    _lblStatus.Text = $"Failed to load versions: {ex.Message}";
                Log?.Error($"Get versions failed for {pkg}: {ex.Message}", pkg);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _cts?.Cancel(); _cts?.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
