using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.GridX;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Controls.Wizards.Forms;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    /// <summary>
    /// Step 4: Manage installed packages and view the activity log.
    /// All work goes through <see cref="IAssemblyHandler"/>; the activity log
    /// comes from the shared <see cref="NuggetActivityLog"/> in the wizard context.
    /// </summary>
    public class NuggetWizard_InstalledStep : WizardPage
    {
        // Left: installed
        private DataTable _dtInstalled;
        private BeepButton _btnRefresh, _btnLoad, _btnUnload, _btnRemove, _btnUpdate;
        private BeepGridPro _gridInstalled;
        private BeepLabel _lblSelectedDetail;

        // Right: activity log
        private BeepComboBox _cmbFilter;
        private BeepButton _btnClearLog, _btnCopyLog, _btnExportLog;
        private DataTable _dtLogs;
        private BeepGridPro _gridLogs;

        public NuggetWizard_InstalledStep()
        {
            Title = "Installed & Activity";
            Description = "Manage installed packages and review the activity log.";
            NextButtonText = "Finish";
            BuildLayout();
        }

        private IAssemblyHandler? Handler
            => Context?.GetValue<IAssemblyHandler>(NuggetWizardKeys.Handler, null!);
        private NuggetActivityLog? Log
            => Context?.GetValue<NuggetActivityLog>(NuggetWizardKeys.Log, null!);

        private void BuildLayout()
        {
            Dock = DockStyle.Fill;
            // Skill § 5.6: Token-based padding scales with DPI.
            Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                // Skill § 5.3: Use BeepLayoutMetrics.Sidebar.Width for the initial splitter distance
                // so the left pane stays in proportion at any DPI.
                SplitterDistance = (int)(BeepLayoutMetrics.Sidebar.Width.ScaleValue(this) * 1.6)
            };

            // ── Left: Installed packages ──────────────────────────────────────
            var left = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));

            var smallGap = BeepLayoutMetrics.SmallGap.ScaleValue(this);
            var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false, Padding = new Padding(0, 0, 0, smallGap) };
            _btnRefresh = new BeepButton { Text = "Refresh", AutoSize = true, Margin = new Padding(0, smallGap, smallGap, 0) };
            _btnLoad    = new BeepButton { Text = "Load",    AutoSize = true, Margin = new Padding(0, smallGap, smallGap, 0) };
            _btnUnload  = new BeepButton { Text = "Unload",  AutoSize = true, Margin = new Padding(0, smallGap, smallGap, 0) };
            _btnUpdate  = new BeepButton { Text = "Update",  AutoSize = true, Margin = new Padding(0, smallGap, smallGap, 0) };
            _btnRemove  = new BeepButton { Text = "Remove",  AutoSize = true, Margin = new Padding(0, smallGap, 0, 0) };
            _btnRefresh.Click += async (s, e) => await RefreshInstalledAsync();
            _btnLoad.Click    += (s, e) => LoadSelected();
            _btnUnload.Click  += (s, e) => UnloadSelected();
            _btnUpdate.Click  += async (s, e) => await UpdateSelectedAsync();
            _btnRemove.Click  += async (s, e) => await RemoveSelectedAsync();
            bar.Controls.AddRange(new Control[] { _btnRefresh, _btnLoad, _btnUnload, _btnUpdate, _btnRemove });

            _dtInstalled = new DataTable();
            _dtInstalled.Columns.Add("PackageId", typeof(string));
            _dtInstalled.Columns.Add("Version", typeof(string));
            _dtInstalled.Columns.Add("Status", typeof(string));
            _dtInstalled.Columns.Add("Source", typeof(string));
            _dtInstalled.Columns.Add("InstallPath", typeof(string));

            _gridInstalled = new BeepGridPro { Dock = DockStyle.Fill };
            _gridInstalled.Columns.Add(new BeepColumnConfig { ColumnName = "PackageId",   ColumnCaption = "Package",   Width = 180 });
            _gridInstalled.Columns.Add(new BeepColumnConfig { ColumnName = "Version",     ColumnCaption = "Version",   Width = 80  });
            _gridInstalled.Columns.Add(new BeepColumnConfig { ColumnName = "Status",      ColumnCaption = "Status",    Width = 80  });
            _gridInstalled.Columns.Add(new BeepColumnConfig { ColumnName = "Source",      ColumnCaption = "Source",    Width = 130 });
            _gridInstalled.Columns.Add(new BeepColumnConfig { ColumnName = "InstallPath", ColumnCaption = "Path",      Width = 200 });
            _gridInstalled.DataSource = _dtInstalled;
            _gridInstalled.SelectionChanged += (s, e) => UpdateSelectedDetail();

            _lblSelectedDetail = new BeepLabel
            {
                Dock = DockStyle.Fill,
                Text = "(none selected)",
                Font = new System.Drawing.Font("Consolas", 9F),
                Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this),
                BorderStyle = BorderStyle.FixedSingle
            };

            left.Controls.Add(bar, 0, 0);
            left.Controls.Add(_gridInstalled, 0, 1);
            left.Controls.Add(_lblSelectedDetail, 0, 2);

            split.Panel1.Controls.Add(left);

            // ── Right: Activity log ───────────────────────────────────────────
            var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var logBar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false, Padding = new Padding(0, 0, 0, smallGap) };
            _cmbFilter  = new BeepComboBox { Width = (int)(BeepLayoutMetrics.FieldStandard.Width.ScaleValue(this) * 0.6), Margin = new Padding(0, smallGap, smallGap, 0) };
            _cmbFilter.ListItems = new BindingList<SimpleItem>
            {
                new SimpleItem { Text = "All",     Item = "All" },
                new SimpleItem { Text = "Info",    Item = "Info" },
                new SimpleItem { Text = "Success", Item = "Success" },
                new SimpleItem { Text = "Warning", Item = "Warning" },
                new SimpleItem { Text = "Error",   Item = "Error" }
            };
            _cmbFilter.SelectedIndex = 0;
            _cmbFilter.SelectedItemChanged += (s, e) => RefreshLogs();
            _btnClearLog  = new BeepButton { Text = "Clear",  AutoSize = true, Margin = new Padding(0, smallGap, smallGap, 0) };
            _btnCopyLog   = new BeepButton { Text = "Copy",   AutoSize = true, Margin = new Padding(0, smallGap, smallGap, 0) };
            _btnExportLog = new BeepButton { Text = "Export", AutoSize = true, Margin = new Padding(0, smallGap, 0, 0) };
            _btnClearLog.Click  += (s, e) => { Log?.Clear(); RefreshLogs(); };
            _btnCopyLog.Click   += (s, e) => CopyLogs();
            _btnExportLog.Click += (s, e) => ExportLogs();
            logBar.Controls.AddRange(new Control[] { _cmbFilter, _btnClearLog, _btnCopyLog, _btnExportLog });

            _dtLogs = new DataTable();
            _dtLogs.Columns.Add("Time", typeof(string));
            _dtLogs.Columns.Add("Severity", typeof(string));
            _dtLogs.Columns.Add("Message", typeof(string));
            _dtLogs.Columns.Add("Package", typeof(string));

            _gridLogs = new BeepGridPro { Dock = DockStyle.Fill };
            _gridLogs.Columns.Add(new BeepColumnConfig { ColumnName = "Time",     ColumnCaption = "Time",     Width = 80  });
            _gridLogs.Columns.Add(new BeepColumnConfig { ColumnName = "Severity", ColumnCaption = "Severity", Width = 80  });
            _gridLogs.Columns.Add(new BeepColumnConfig { ColumnName = "Message",  ColumnCaption = "Message",  Width = 350 });
            _gridLogs.Columns.Add(new BeepColumnConfig { ColumnName = "Package",  ColumnCaption = "Package",  Width = 130 });
            _gridLogs.DataSource = _dtLogs;

            right.Controls.Add(logBar, 0, 0);
            right.Controls.Add(_gridLogs, 0, 1);
            right.Controls.Add(new BeepLabel { Text = "Operation Log", AutoSize = true, Dock = DockStyle.Left }, 0, 2);

            split.Panel2.Controls.Add(right);
            Controls.Add(split);

            IsComplete = true;  // last step is always "complete"
        }

        public override void OnStepEnter(WizardContext context)
        {
            base.OnStepEnter(context);
            _ = RefreshInstalledAsync();
            RefreshLogs();
        }

        // ---- Installed operations ----

        private async System.Threading.Tasks.Task RefreshInstalledAsync()
        {
            var h = Handler; if (h == null || _dtInstalled == null) return;
            _btnRefresh.Enabled = false;
            try
            {
                var infos = await h.GetInstalledNuGetPackagesAsync();
                _dtInstalled.Rows.Clear();
                foreach (var info in infos)
                {
                    string status = info.IsLoaded
                        ? "Loaded"
                        : (string.IsNullOrWhiteSpace(info.InstallPath) || !Directory.Exists(info.InstallPath)
                            ? "Missing"
                            : "Unloaded");
                    _dtInstalled.Rows.Add(
                        info.PackageId,
                        info.Version,
                        status,
                        info.Source,
                        info.InstallPath);
                }
                Log?.Info($"Refreshed: {infos.Count} installed package(s).");
            }
            catch (Exception ex)
            {
                Log?.Error($"Refresh installed failed: {ex.Message}");
            }
            finally { _btnRefresh.Enabled = true; }
        }

        private void UpdateSelectedDetail()
        {
            if (_gridInstalled.CurrentRow == null)
            { _lblSelectedDetail.Text = "(none selected)"; return; }
            var id  = _gridInstalled.CurrentRow.Cells["PackageId"]?.Value?.ToString() ?? string.Empty;
            var ver = _gridInstalled.CurrentRow.Cells["Version"]?.Value?.ToString() ?? string.Empty;
            var st  = _gridInstalled.CurrentRow.Cells["Status"]?.Value?.ToString() ?? string.Empty;
            var src = _gridInstalled.CurrentRow.Cells["Source"]?.Value?.ToString() ?? string.Empty;
            var pth = _gridInstalled.CurrentRow.Cells["InstallPath"]?.Value?.ToString() ?? string.Empty;
            _lblSelectedDetail.Text =
                $"Package:  {id}\n" +
                $"Version:  {ver}\n" +
                $"Status:   {st}\n" +
                $"Source:   {src}\n" +
                $"Path:     {pth}";
        }

        private (string id, string installPath)? GetSelected()
        {
            if (_gridInstalled.CurrentRow == null) return null;
            var id  = _gridInstalled.CurrentRow.Cells["PackageId"]?.Value?.ToString();
            var pth = _gridInstalled.CurrentRow.Cells["InstallPath"]?.Value?.ToString();
            if (string.IsNullOrWhiteSpace(id)) return null;
            return (id, pth ?? string.Empty);
        }

        private void LoadSelected()
        {
            var h = Handler; var sel = GetSelected();
            if (h == null || sel == null) return;
            if (string.IsNullOrWhiteSpace(sel.Value.installPath))
            {
                Log?.Warn($"Cannot load '{sel.Value.id}': no install path recorded.");
                return;
            }
            try
            {
                bool ok = h.LoadNugget(sel.Value.installPath);
                Log?.Info(ok
                    ? $"Load '{sel.Value.id}' requested."
                    : $"Load '{sel.Value.id}' returned false.", sel.Value.id);
                _ = RefreshInstalledAsync();
                RefreshLogs();
            }
            catch (Exception ex)
            {
                Log?.Error($"Load '{sel.Value.id}' failed: {ex.Message}", sel.Value.id);
            }
        }

        private void UnloadSelected()
        {
            var h = Handler; var sel = GetSelected();
            if (h == null || sel == null) return;
            try
            {
                bool ok = h.UnloadNugget(sel.Value.id);
                Log?.Info(ok
                    ? $"Unload '{sel.Value.id}' requested."
                    : $"Unload '{sel.Value.id}' returned false.", sel.Value.id);
                _ = RefreshInstalledAsync();
                RefreshLogs();
            }
            catch (Exception ex)
            {
                Log?.Error($"Unload '{sel.Value.id}' failed: {ex.Message}", sel.Value.id);
            }
        }

        private async System.Threading.Tasks.Task UpdateSelectedAsync()
        {
            var h = Handler; var sel = GetSelected();
            if (h == null || sel == null) return;
            try
            {
                var result = await h.UpdateNuGetPackageAsync(sel.Value.id);
                if (result != null)
                {
                    if (result.Success && result.WasUpdated)
                        Log?.Success($"Updated '{sel.Value.id}': {result.OldVersion} → {result.NewVersion}.", sel.Value.id);
                    else if (result.Success)
                        Log?.Info($"'{sel.Value.id}' is already at the latest version ({result.NewVersion}).", sel.Value.id);
                    else
                        Log?.Error($"Update '{sel.Value.id}' failed: {result.Error}", sel.Value.id);
                }
                else
                {
                    Log?.Info($"Update '{sel.Value.id}' returned no result.", sel.Value.id);
                }
                await RefreshInstalledAsync();
                RefreshLogs();
            }
            catch (Exception ex)
            {
                Log?.Error($"Update '{sel.Value.id}' failed: {ex.Message}", sel.Value.id);
            }
        }

        private async System.Threading.Tasks.Task RemoveSelectedAsync()
        {
            var h = Handler; var sel = GetSelected();
            if (h == null || sel == null) return;
            if (MessageBox.Show($"Remove '{sel.Value.id}'? This will unload and delete files.",
                                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                bool ok = await h.UninstallNuGetPackageAsync(sel.Value.id, removeDependencies: true);
                Log?.Info(ok
                    ? $"Removed '{sel.Value.id}'."
                    : $"Remove '{sel.Value.id}' returned false.", sel.Value.id);
                await RefreshInstalledAsync();
                RefreshLogs();
            }
            catch (Exception ex)
            {
                Log?.Error($"Remove '{sel.Value.id}' failed: {ex.Message}", sel.Value.id);
            }
        }

        // ---- Log operations ----

        private void RefreshLogs()
        {
            var log = Log; if (log == null || _dtLogs == null) return;
            _dtLogs.Rows.Clear();
            var filter = _cmbFilter?.SelectedItem?.Item?.ToString() ?? "All";
            var entries = log.Entries
                .Cast<NuggetLogEntry>()
                .Where(e => filter == "All" || string.Equals(e.Severity, filter, StringComparison.OrdinalIgnoreCase));
            foreach (var e in entries)
                _dtLogs.Rows.Add(e.Timestamp.ToString("HH:mm:ss"), e.Severity, e.Message, e.Package);
        }

        private void CopyLogs()
        {
            var log = Log; if (log == null) return;
            try
            {
                var text = string.Join(Environment.NewLine,
                    log.Entries.Cast<NuggetLogEntry>().Select(e =>
                        $"[{e.Timestamp:HH:mm:ss}] {e.Severity}: {e.Message}"));
                Clipboard.SetText(text);
            }
            catch { /* ignore clipboard errors */ }
        }

        private void ExportLogs()
        {
            var log = Log; if (log == null) return;
            using var dlg = new SaveFileDialog
            {
                Filter = "Log files (*.log)|*.log|All files (*.*)|*.*",
                FileName = $"NuggetsActivity_{DateTime.Now:yyyyMMdd_HHmmss}.log"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                var text = string.Join(Environment.NewLine,
                    log.Entries.Cast<NuggetLogEntry>().Select(e =>
                        $"[{e.Timestamp:yyyy-MM-dd HH:mm:ss}] {e.Severity}: {e.Message}"));
                File.WriteAllText(dlg.FileName, text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
