using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.NuGet;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;
using TheTechIdea.Beep.Winform.Controls.GridX;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Controls.Wizards.Forms;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    /// <summary>
    /// Step 1: Search NuGet packages and manage sources.
    /// All work goes through <see cref="IAssemblyHandler"/>.
    /// </summary>
    public class NuggetWizard_SearchStep : WizardPage
    {
        private DataTable? _dtResults;
        private CancellationTokenSource? _cts;

        private BeepLabel _lblSource; private BeepComboBox _cmbSource;
        private BeepTextBox _txtSearch; private BeepCheckBoxBool _chkPrerelease; private BeepButton _btnSearch;
        private BeepGridPro _grid;
        private BeepLabel _lblSourcesTitle;
        private BeepTextBox _txtSourceName; private BeepTextBox _txtSourceUrl;
        private BeepCheckBoxBool _chkSourceEnabled;
        private BeepButton _btnAddSource; private BeepButton _btnRemoveSource; private BeepButton _btnTestSource;
        private BeepLabel _lblStatus;

        public NuggetWizard_SearchStep()
        {
            Title = "Select Package";
            Description = "Search NuGet packages and choose one to install. Manage sources below.";
            BuildLayout();
        }

        private TheTechIdea.Beep.Tools.IAssemblyHandler? Handler
            => Context?.GetValue<TheTechIdea.Beep.Tools.IAssemblyHandler>(NuggetWizardKeys.Handler, null!);
        private NuggetActivityLog? Log
            => Context?.GetValue<NuggetActivityLog>(NuggetWizardKeys.Log, null!);

        private void BuildLayout()
        {
            Dock = DockStyle.Fill;
            // Skill § 5.6: Token-based padding scales with DPI.
            Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, BeepLayoutMetrics.LabelStandard.Height.ScaleValue(this) * 3));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Search bar
            var bar = new TableLayoutPanel { Dock = DockStyle.Top, Height = BeepLayoutMetrics.ButtonStandard.Height.ScaleValue(this), ColumnCount = 5 };
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, BeepLayoutMetrics.ComboBox.Width.ScaleValue(this)));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, BeepLayoutMetrics.ButtonSmall.Width.ScaleValue(this)));
            var smallGap = BeepLayoutMetrics.SmallGap.ScaleValue(this);
            bar.Padding = new Padding(0, 0, 0, smallGap);

            _lblSource    = new BeepLabel { Text = "Source:", AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, smallGap + 2, smallGap, 0) };
            _cmbSource    = new BeepComboBox { Dock = DockStyle.Fill, Margin = new Padding(0, smallGap, smallGap, smallGap) };
            _txtSearch    = new BeepTextBox { Dock = DockStyle.Fill, PlaceholderText = "Search packages…", Margin = new Padding(0, smallGap, smallGap, smallGap) };
            _chkPrerelease= new BeepCheckBoxBool { Text = "Prerelease", AutoSize = true, Dock = DockStyle.Fill, CurrentValue = false, Margin = new Padding(0, smallGap + 4, smallGap, 0) };
            _btnSearch    = new BeepButton { Text = "Search", Dock = DockStyle.Fill, Margin = new Padding(0, smallGap, 0, smallGap) };

            _txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) _ = SearchAsync(); };
            _btnSearch.Click   += (s, e) => _ = SearchAsync();

            bar.Controls.Add(_lblSource,    0, 0);
            bar.Controls.Add(_cmbSource,    1, 0);
            bar.Controls.Add(_txtSearch,    2, 0);
            bar.Controls.Add(_chkPrerelease, 3, 0);
            bar.Controls.Add(_btnSearch,    4, 0);
            root.Controls.Add(bar, 0, 0);

            // Results
            _dtResults = new DataTable();
            _dtResults.Columns.Add("PackageId", typeof(string));
            _dtResults.Columns.Add("Version", typeof(string));
            _dtResults.Columns.Add("Downloads", typeof(string));
            _dtResults.Columns.Add("Description", typeof(string));

            _grid = new BeepGridPro { Dock = DockStyle.Fill };
            _grid.Columns.Add(new BeepColumnConfig { ColumnName = "PackageId",   ColumnCaption = "Package",    Width = 200 });
            _grid.Columns.Add(new BeepColumnConfig { ColumnName = "Version",     ColumnCaption = "Version",    Width = 80  });
            _grid.Columns.Add(new BeepColumnConfig { ColumnName = "Downloads",   ColumnCaption = "Downloads",  Width = 90  });
            _grid.Columns.Add(new BeepColumnConfig { ColumnName = "Description", ColumnCaption = "Description", Width = 350 });
            _grid.DataSource = _dtResults;
            _grid.SelectionChanged += (s, e) =>
            {
                var id = _grid?.CurrentRow?.Cells["PackageId"]?.Value?.ToString();
                IsComplete = !string.IsNullOrWhiteSpace(id);
            };
            root.Controls.Add(_grid, 0, 1);

            // Sources title
            _lblSourcesTitle = new BeepLabel
            {
                Text = "Manage Sources",
                AutoSize = true,
                Dock = DockStyle.Left,
                Font = new System.Drawing.Font(System.Drawing.SystemFonts.MessageBoxFont ?? System.Drawing.SystemFonts.DefaultFont, System.Drawing.FontStyle.Bold),
                Margin = new Padding(0, smallGap, 0, 0)
            };
            root.Controls.Add(_lblSourcesTitle, 0, 2);

            // Sources edit
            var src = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, Height = BeepLayoutMetrics.ButtonStandard.Height.ScaleValue(this) };
            src.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, BeepLayoutMetrics.LabelColumnWidth.ScaleValue(this) + smallGap * 3));
            src.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            src.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            src.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, BeepLayoutMetrics.ButtonSmall.Width.ScaleValue(this)));
            src.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, BeepLayoutMetrics.ButtonSmall.Width.ScaleValue(this)));
            src.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, BeepLayoutMetrics.ButtonSmall.Width.ScaleValue(this)));

            _txtSourceName    = new BeepTextBox { Dock = DockStyle.Fill, PlaceholderText = "Source name",    Margin = new Padding(0, 2, smallGap, 0) };
            _txtSourceUrl     = new BeepTextBox { Dock = DockStyle.Fill, PlaceholderText = "URL or local path", Margin = new Padding(0, 2, smallGap, 0) };
            _chkSourceEnabled = new BeepCheckBoxBool { Text = "Enabled", AutoSize = true, Dock = DockStyle.Fill, CurrentValue = true, Margin = new Padding(0, smallGap + 4, smallGap, 0) };
            _btnAddSource     = new BeepButton { Text = "Add",    Dock = DockStyle.Fill, Margin = new Padding(0, 2, smallGap, 0) };
            _btnRemoveSource  = new BeepButton { Text = "Remove", Dock = DockStyle.Fill, Margin = new Padding(0, 2, smallGap, 0) };
            _btnTestSource    = new BeepButton { Text = "Test",   Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 0) };

            _btnAddSource.Click    += BtnAddSource_Click;
            _btnRemoveSource.Click += BtnRemoveSource_Click;
            _btnTestSource.Click   += async (s, e) => await TestSourceAsync();

            src.Controls.Add(_txtSourceName,    0, 0);
            src.Controls.Add(_txtSourceUrl,     1, 0);
            src.Controls.Add(_chkSourceEnabled, 2, 0);
            src.Controls.Add(_btnAddSource,     3, 0);
            src.Controls.Add(_btnRemoveSource,  4, 0);
            src.Controls.Add(_btnTestSource,    5, 0);
            root.Controls.Add(src, 0, 3);

            // Status
            _lblStatus = new BeepLabel { Text = "Ready", Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0, smallGap, 0, 0) };
            root.Controls.Add(_lblStatus, 0, 4);

            Controls.Add(root);
        }

        public override void OnStepEnter(WizardContext context)
        {
            base.OnStepEnter(context);
            var h = Handler; if (h == null) return;
            var sources = h.GetNuGetSources();
            _cmbSource.ListItems = new BindingList<SimpleItem>(
                sources.Select(s => new SimpleItem { Text = s.Name, Item = s.Url }).ToList());
        }

        public override void OnStepLeave(WizardContext context)
        {
            base.OnStepLeave(context);
            var id = _grid?.CurrentRow?.Cells["PackageId"]?.Value?.ToString() ?? string.Empty;
            var src = _cmbSource?.SelectedItem?.Item?.ToString() ?? string.Empty;
            context.SetValue(NuggetWizardKeys.PackageId, id);
            context.SetValue(NuggetWizardKeys.SourceUrl, src);
            context.SetValue(NuggetWizardKeys.IncludePre, _chkPrerelease.CurrentValue);
        }

        public override WizardValidationResult Validate()
        {
            var id = _grid?.CurrentRow?.Cells["PackageId"]?.Value?.ToString();
            return string.IsNullOrWhiteSpace(id)
                ? WizardValidationResult.Error("Please select a package from the results.")
                : WizardValidationResult.Success();
        }

        private async Task SearchAsync()
        {
            var h = Handler; if (h == null || _dtResults == null) return;
            var term = _txtSearch.Text?.Trim();
            if (string.IsNullOrWhiteSpace(term)) { _lblStatus.Text = "Enter a search term."; return; }

            _cts?.Cancel(); _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _btnSearch.Enabled = false;
            _lblStatus.Text = "Searching…";
            _dtResults.Rows.Clear();

            try
            {
                var results = await h.SearchNuGetPackagesAsync(term, 0, 100, _chkPrerelease.CurrentValue, _cts.Token);
                foreach (var r in results)
                    _dtResults.Rows.Add(r.PackageId, r.Version, r.TotalDownloads.ToString("N0"), Truncate(r.Description, 120));
                _grid.DataSource = null; _grid.DataSource = _dtResults;
                _lblStatus.Text = $"{results.Count} package{(results.Count == 1 ? "" : "s")} found.";
                Log?.Info($"Search '{term}': {results.Count} hits.");
            }
            catch (OperationCanceledException) { _lblStatus.Text = "Search cancelled."; }
            catch (Exception ex) { _lblStatus.Text = $"Search failed: {ex.Message}"; Log?.Error($"Search failed: {ex.Message}"); }
            finally { _btnSearch.Enabled = true; }
        }

        private void BtnAddSource_Click(object? sender, EventArgs e)
        {
            var h = Handler; if (h == null) return;
            var name = _txtSourceName.Text?.Trim() ?? string.Empty;
            var url  = _txtSourceUrl.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url))
            { _lblStatus.Text = "Name and URL are required."; return; }
            try
            {
                h.AddNuGetSource(name, url, _chkSourceEnabled.CurrentValue);
                _lblStatus.Text = $"Source '{name}' added.";
                _txtSourceName.Text = _txtSourceUrl.Text = string.Empty;
                Log?.Success($"Source '{name}' added.");
                OnStepEnter(Context!);
            }
            catch (Exception ex) { _lblStatus.Text = $"Add failed: {ex.Message}"; Log?.Error($"Add source failed: {ex.Message}"); }
        }

        private void BtnRemoveSource_Click(object? sender, EventArgs e)
        {
            var h = Handler; var sel = _cmbSource.SelectedItem;
            if (h == null || sel == null) return;
            try
            {
                h.RemoveNuGetSource(sel.Text ?? string.Empty);
                _lblStatus.Text = "Source removed.";
                Log?.Info($"Source '{sel.Text}' removed.");
                OnStepEnter(Context!);
            }
            catch (Exception ex) { _lblStatus.Text = $"Remove failed: {ex.Message}"; Log?.Error($"Remove source failed: {ex.Message}"); }
        }

        private async Task TestSourceAsync()
        {
            var h = Handler; var sel = _cmbSource.SelectedItem;
            if (h == null || sel == null) return;
            _lblStatus.Text = $"Testing '{sel.Text}'…";
            try
            {
                // A simple probe: do a single-result search against the source.
                var probe = await h.SearchNuGetPackagesAsync(sel.Text ?? string.Empty, 0, 1, false, CancellationToken.None);
                var ok = probe != null;
                _lblStatus.Text = $"Source '{sel.Text}' is {(ok ? "reachable" : "unreachable")}.";
                Log?.Info($"Source '{sel.Text}' test: {(ok ? "reachable" : "unreachable")}.");
            }
            catch (Exception ex) { _lblStatus.Text = $"Test failed: {ex.Message}"; Log?.Error($"Test source failed: {ex.Message}"); }
        }

        private static string Truncate(string? text, int max)
            => string.IsNullOrWhiteSpace(text) ? string.Empty
             : (text.Length <= max ? text : text[..max] + "…");

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _cts?.Cancel(); _cts?.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
