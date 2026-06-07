using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;
using TheTechIdea.Beep.Winform.Controls.ComboBoxes;
using TheTechIdea.Beep.Winform.Controls.GridX;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    public class DriverManagementForm : Form
    {
        private readonly IDMEEditor _editor;
        private BeepGridPro? _grid;
        private BeepButton _btnAdd = null!;
        private BeepButton _btnSave = null!;
        private BeepButton _btnSyncStatus = null!;
        private BeepLabel _lblStatus = null!;
        private FlowLayoutPanel _toolbar = null!;
        private TableLayoutPanel _layout = null!;

        private List<ConnectionDriversConfig> _drivers = new();
        private bool _hasChanges;
        private CancellationTokenSource? _cts;

        public DriverManagementForm(IDMEEditor editor)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            InitializeComponent();
            LoadDrivers();
        }

        private void InitializeComponent()
        {
            Text = "Driver Management";
            StartPosition = FormStartPosition.CenterParent;
            Size = BeepLayoutMetrics.DialogLarge.ScaleSize(this);

            _layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(8)
            };
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            _btnAdd = CreateToolbarButton("Add Driver", "➕");
            _btnAdd.Click += (_, _) => ShowAddEditDialog(null);

            _btnSyncStatus = CreateToolbarButton("Sync Status", "🔄");
            _btnSyncStatus.Click += (_, _) => _ = SyncDriverStatusAsync();

            _btnSave = CreateToolbarButton("Save", "💾");
            _btnSave.Click += (_, _) => SaveDrivers();

            _lblStatus = new BeepLabel
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                UseThemeColors = true,
                AutoSize = true
            };

            _toolbar.Controls.Add(_btnAdd);
            _toolbar.Controls.Add(_btnSyncStatus);
            _toolbar.Controls.Add(_btnSave);
            _toolbar.Controls.Add(_lblStatus);

            _layout.Controls.Add(_toolbar, 0, 0);
            Controls.Add(_layout);
        }

        private BeepButton CreateToolbarButton(string text, string icon)
        {
            var btn = new BeepButton
            {
                Text = $"{icon} {text}",
                Width = BeepLayoutMetrics.ButtonToolbar.ScaleSize(this).Width,
                Height = BeepLayoutMetrics.ButtonToolbar.ScaleSize(this).Height,
                UseThemeColors = true,
                Margin = new Padding(0, 0, 8, 0)
            };
            return btn;
        }

        private void LoadDrivers()
        {
            _drivers = _editor.ConfigEditor?.DataDriversClasses?
                .Where(d => d.NeedDrivers && !d.InMemory)
                .ToList() ?? new List<ConnectionDriversConfig>();

            BuildGrid();
        }

        private void BuildGrid()
        {
            _grid?.Dispose();

            _grid = new BeepGridPro
            {
                Dock = DockStyle.Fill,
                UseThemeColors = true
            };

            var dt = new System.Data.DataTable();
            dt.Columns.Add("PackageName", typeof(string));
            dt.Columns.Add("DatasourceType", typeof(string));
            dt.Columns.Add("Category", typeof(string));
            dt.Columns.Add("Version", typeof(string));
            dt.Columns.Add("ClassHandler", typeof(string));
            dt.Columns.Add("Installed", typeof(string));
            dt.Columns.Add("AutoLoad", typeof(string));
            dt.Columns.Add("RowIndex", typeof(int));

            for (int i = 0; i < _drivers.Count; i++)
            {
                var d = _drivers[i];
                dt.Rows.Add(
                    d.PackageName ?? string.Empty,
                    d.DatasourceType.ToString(),
                    d.DatasourceCategory.ToString(),
                    d.NuggetVersion ?? d.version ?? string.Empty,
                    d.classHandler ?? string.Empty,
                    d.IsMissing ? (d.NuggetMissing ? "Not Installed" : "Downloaded, Not Loaded") : "Installed ✓",
                    d.AutoLoad ? "Yes" : "No",
                    i);
            }

            _grid.DataSource = dt;
            _grid.ContextMenuStrip = BuildContextMenu();
            

            _layout.Controls.Remove(_layout.GetControlFromPosition(0, 1));
            _layout.Controls.Add(_grid, 0, 1);
        }

        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Edit Driver", null, (_, _) => EditSelected());
            menu.Items.Add("-");
            menu.Items.Add("Download NuGet Package", null, (_, _) => DownloadNuGetSelected());
            menu.Items.Add("Load From Cache", null, (_, _) => LoadSelectedFromCache());
            menu.Items.Add("-");
            menu.Items.Add("Delete Driver", null, (_, _) => DeleteSelected());
            return menu;
        }

        //private void Grid_MouseDown(object? sender, MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Right)
        //    {
        //        var hit = _grid?.HitTest(e.X, e.Y);
        //        if (hit?.RowIndex >= 0)
        //        {
        //            _grid?.ClearSelection();
        //            _grid?.SelectRow(hit.RowIndex);
        //        }
        //    }
        //}

        private int GetSelectedRowIndex()
        {
            if (_grid?.SelectedRows == null || _grid.SelectedRows.Count == 0) return -1;
            var row = _grid.SelectedRows[0];
            return _grid.CurrentRowIndex;
        }

        private void EditSelected()
        {
            var idx = GetSelectedRowIndex();
            if (idx >= 0 && idx < _drivers.Count)
                ShowAddEditDialog(_drivers[idx]);
        }

        private void DeleteSelected()
        {
            var idx = GetSelectedRowIndex();
            if (idx < 0) return;

            if (MessageBox.Show($"Delete driver '{_drivers[idx].PackageName}'?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _drivers.RemoveAt(idx);
            _editor.ConfigEditor!.DataDriversClasses!.Remove(_drivers[idx]);
            _hasChanges = true;
            BuildGrid();
        }

        private void ShowAddEditDialog(ConnectionDriversConfig? existing)
        {
            using var dlg = new DriverEditDialog(_editor, existing);
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
            {
                if (existing != null)
                {
                    var idx = _drivers.IndexOf(existing);
                    if (idx >= 0) _drivers[idx] = dlg.Result;
                }
                else
                {
                    _drivers.Add(dlg.Result);
                    _editor.ConfigEditor?.DataDriversClasses?.Add(dlg.Result);
                }
                _hasChanges = true;
                BuildGrid();
            }
        }

        private async Task SyncDriverStatusAsync()
        {
            _lblStatus.Text = "Syncing driver status...";
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                var updated = 0;
                foreach (var driver in _drivers)
                {
                    if (_cts.IsCancellationRequested) break;

                    var handler = _editor.assemblyHandler;
                    bool loaded = handler != null && handler.IsDriverClassLoaded(driver.classHandler, driver.dllname);
                    bool downloaded = loaded || (handler?.HasLocalPackage(driver) ?? false);

                    if (driver.IsMissing != !loaded || driver.NuggetMissing != !downloaded)
                        updated++;

                    driver.IsMissing = !loaded;
                    driver.NuggetMissing = !downloaded;
                }

                _hasChanges = true;
                BuildGrid();
                _lblStatus.Text = $"Status synced — {updated} driver(s) updated.";
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"Sync failed: {ex.Message}";
            }
        }

        private async void DownloadNuGetSelected()
        {
            var idx = GetSelectedRowIndex();
            if (idx < 0 || idx >= _drivers.Count) return;
            var driver = _drivers[idx];

            using var dlg = new NuGetDownloadDialog(_editor, driver);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                driver.NuggetMissing = false;
                driver.IsMissing = false;
                driver.NuggetVersion = dlg.SelectedVersion;
                driver.AutoLoad = true;
                _hasChanges = true;
                BuildGrid();
            }
        }

        private void LoadSelectedFromCache()
        {
            var idx = GetSelectedRowIndex();
            if (idx < 0 || idx >= _drivers.Count) return;
            var driver = _drivers[idx];

            try
            {
                if (_editor.assemblyHandler?.LoadDriverFromLocalPackage(driver, out _) == true)
                {
                    driver.IsMissing = false;
                    _hasChanges = true;
                    BuildGrid();
                    _lblStatus.Text = $"'{driver.PackageName}' loaded successfully.";
                }
                else
                {
                    _lblStatus.Text = $"'{driver.PackageName}' not available locally. Use Download.";
                }
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"Load failed: {ex.Message}";
            }
        }

        private void SaveDrivers()
        {
            try
            {
                _editor.ConfigEditor?.SaveConnectionDriversConfigValues();
                _hasChanges = false;
                _lblStatus.Text = "Drivers saved.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _grid?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    internal class DriverEditDialog : Form
    {
        private readonly IDMEEditor _editor;
        private BeepTextBox _txtPackageName = null!;
        private BeepTextBox _txtClassHandler = null!;
        private BeepTextBox _txtConnectionString = null!;
        private BeepTextBox _txtNuggetVersion = null!;
        private BeepCheckBoxBool _chkAutoLoad = null!;
        private BeepButton _btnOk = null!;
        private BeepButton _btnCancel = null!;
        private TableLayoutPanel _layout = null!;

        public ConnectionDriversConfig? Result { get; private set; }
        public ConnectionDriversConfig? Original { get; }

        public DriverEditDialog(IDMEEditor editor, ConnectionDriversConfig? existing)
        {
            _editor = editor;
            Original = existing;
            InitializeComponent();

            if (existing != null)
            {
                Text = "Edit Driver";
                _txtPackageName.Text = existing.PackageName ?? string.Empty;
                _txtPackageName.ReadOnly = true;
                _txtClassHandler.Text = existing.classHandler ?? string.Empty;
                _txtConnectionString.Text = existing.ConnectionString ?? string.Empty;
                _txtNuggetVersion.Text = existing.NuggetVersion ?? string.Empty;
                _chkAutoLoad.CurrentValue = existing.AutoLoad;
            }
            else
            {
                Text = "Add Driver";
            }
        }

        private void InitializeComponent()
        {
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            Size = BeepLayoutMetrics.DialogMedium.ScaleSize(this);
            MaximizeBox = false;
            MinimizeBox = false;

            _layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 8,
                Padding = BeepLayoutMetrics.HeaderPadding.ScalePadding(this)
            };
            for (int i = 0; i < 7; i++) _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _txtPackageName = CreateField("Package Name");
            _txtClassHandler = CreateField("Class Handler");
            _txtConnectionString = CreateField("Connection String");
            _txtNuggetVersion = CreateField("Nugget Version");

            _chkAutoLoad = new BeepCheckBoxBool
            {
                Text = "Auto-load on startup",
                Dock = DockStyle.Fill,
                Height = BeepLayoutMetrics.TextRowHeight.ScaleValue(this),
                CurrentValue = true
            };

            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this)
            };

            _btnOk = new BeepButton { Text = "Save", Width = BeepLayoutMetrics.ButtonStandard.ScaleSize(this).Width, Height = BeepLayoutMetrics.ButtonStandard.ScaleSize(this).Height };
            _btnOk.Click += (_, _) => { Commit(); DialogResult = DialogResult.OK; Close(); };

            _btnCancel = new BeepButton { Text = "Cancel", Width = BeepLayoutMetrics.ButtonStandard.ScaleSize(this).Width, Height = BeepLayoutMetrics.ButtonStandard.ScaleSize(this).Height };
            _btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

            btnPanel.Controls.Add(_btnOk);
            btnPanel.Controls.Add(_btnCancel);

            _layout.Controls.Add(_txtPackageName, 0, 0);
            _layout.Controls.Add(_txtClassHandler, 0, 1);
            _layout.Controls.Add(_txtConnectionString, 0, 2);
            _layout.Controls.Add(_txtNuggetVersion, 0, 3);
            _layout.Controls.Add(_chkAutoLoad, 0, 4);
            _layout.Controls.Add(btnPanel, 0, 5);
            _layout.Controls.Add(new BeepLabel { Text = string.Empty, Dock = DockStyle.Fill }, 0, 6);

            Controls.Add(_layout);
            //AcceptButton = _btnOk;
            //CancelButton = _btnCancel;
        }

        private BeepTextBox CreateField(string label)
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var lbl = new BeepLabel { Text = label, AutoSize = true, UseThemeColors = true };
            var txt = new BeepTextBox { Dock = DockStyle.Fill, UseThemeColors = true };
            panel.Controls.Add(lbl, 0, 0);
            panel.Controls.Add(txt, 0, 1);

            var wrapper = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 0, BeepLayoutMetrics.SmallGap.ScaleValue(this)) };
            wrapper.Controls.Add(panel);
            _layout?.Controls.Add(wrapper);
            return txt;
        }

        private void Commit()
        {
            Result = new ConnectionDriversConfig
            {
                PackageName = _txtPackageName.Text?.Trim() ?? string.Empty,
                classHandler = _txtClassHandler.Text?.Trim() ?? string.Empty,
                ConnectionString = _txtConnectionString.Text?.Trim() ?? string.Empty,
                NuggetVersion = _txtNuggetVersion.Text?.Trim() ?? string.Empty,
                AutoLoad = _chkAutoLoad.CurrentValue,
                NeedDrivers = true,
                DatasourceType = Original?.DatasourceType ?? DataSourceType.NONE,
                DatasourceCategory = Original?.DatasourceCategory ?? DatasourceCategory.RDBMS
            };
        }
    }

    internal class NuGetDownloadDialog : Form
    {
        private readonly IDMEEditor _editor;
        private readonly ConnectionDriversConfig _driver;
        private BeepLabel _lblPackage = null!;
        private BeepComboBox _cmbVersion = null!;
        private BeepButton _btnDownload = null!;
        private BeepButton _btnClose = null!;
        private BeepLabel _lblStatus = null!;
        private BeepLabel _lblResult = null!;
        private TableLayoutPanel _layout = null!;
        private bool _downloading;

        public string? SelectedVersion { get; private set; }

        public NuGetDownloadDialog(IDMEEditor editor, ConnectionDriversConfig driver)
        {
            _editor = editor;
            _driver = driver;
            InitializeComponent();
            _ = LoadVersionsAsync();
        }

        private void InitializeComponent()
        {
            Text = $"Download: {_driver.PackageName}";
            StartPosition = FormStartPosition.CenterParent;
            Size = BeepLayoutMetrics.DialogMedium.ScaleSize(this);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            _layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(16)
            };
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _lblPackage = new BeepLabel
            {
                Text = $"Package: {_driver.PackageName}",
                Dock = DockStyle.Fill,
                UseThemeColors = true
            };

            _cmbVersion = new BeepComboBox
            {
                Dock = DockStyle.Fill,
                IsEditable = false,
                PlaceholderText = "Loading versions..."
            };

            _lblStatus = new BeepLabel
            {
                Text = "Loading versions...",
                Dock = DockStyle.Fill,
                UseThemeColors = true
            };

            _lblResult = new BeepLabel
            {
                Text = string.Empty,
                Dock = DockStyle.Fill,
                ForeColor = Color.Green,
                UseThemeColors = false
            };

            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft
            };

            _btnDownload = new BeepButton { Text = "⬇ Download", Width = BeepLayoutMetrics.ButtonStandard.ScaleSize(this).Width, Height = BeepLayoutMetrics.ButtonStandard.ScaleSize(this).Height, Enabled = false };
            _btnDownload.Click += (_, _) => _ = DownloadAsync();

            _btnClose = new BeepButton { Text = "Close", Width = BeepLayoutMetrics.ButtonSmall.ScaleSize(this).Width, Height = BeepLayoutMetrics.ButtonSmall.ScaleSize(this).Width };
            _btnClose.Click += (_, _) => Close();

            btnPanel.Controls.Add(_btnDownload);
            btnPanel.Controls.Add(_btnClose);

            _layout.Controls.Add(_lblPackage, 0, 0);
            _layout.Controls.Add(_cmbVersion, 0, 1);
            _layout.Controls.Add(_lblStatus, 0, 2);
            _layout.Controls.Add(_lblResult, 0, 3);
            _layout.Controls.Add(btnPanel, 0, 4);

            Controls.Add(_layout);
        }

        private async Task LoadVersionsAsync()
        {
            try
            {
                var versions = await _editor.assemblyHandler!.GetNuGetPackageVersionsAsync(
                    _driver.PackageName, includePrerelease: false);

                if (versions != null && versions.Count > 0)
                {
                    foreach (var v in versions)
                        _cmbVersion.ListItems?.Add(new SimpleItem { Text = v, Value = v });

                    var preSelect = !string.IsNullOrEmpty(_driver.NuggetVersion) && versions.Contains(_driver.NuggetVersion)
                        ? _driver.NuggetVersion : versions[0];

                    for (int i = 0; i < _cmbVersion.ListItems?.Count; i++)
                    {
                        if (string.Equals(_cmbVersion.ListItems[i].Value?.ToString(), preSelect, StringComparison.OrdinalIgnoreCase))
                        {
                            _cmbVersion.SelectedIndex = i;
                            break;
                        }
                    }

                    _lblStatus.Text = $"{versions.Count} versions available.";
                    _btnDownload.Enabled = true;
                }
                else
                {
                    _lblStatus.Text = "No versions found on NuGet.";
                }
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"Error loading versions: {ex.Message}";
            }
        }

        private async Task DownloadAsync()
        {
            var selectedVer = _cmbVersion.SelectedItem?.Value?.ToString();
            if (string.IsNullOrEmpty(selectedVer)) return;

            _downloading = true;
            _btnDownload.Enabled = false;
            _lblStatus.Text = $"Downloading {_driver.PackageName} {selectedVer}...";

            try
            {
                var assemblies = await _editor.assemblyHandler!.LoadNuggetFromNuGetAsync(
                    _driver.PackageName, selectedVer);

                if (assemblies != null && assemblies.Count > 0)
                {
                    _lblResult.Text = $"Downloaded {assemblies.Count} assemblies. Driver ready.";
                    _lblResult.ForeColor = Color.Green;
                    SelectedVersion = selectedVer;
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    _lblResult.Text = "Download failed — no assemblies returned.";
                    _lblResult.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                _lblResult.Text = $"Error: {ex.Message}";
                _lblResult.ForeColor = Color.Red;
            }
            finally
            {
                _downloading = false;
                _lblStatus.Text = string.Empty;
                _btnClose.Text = "Close";
                _btnDownload.Enabled = !_downloading;
            }
        }
    }
}
