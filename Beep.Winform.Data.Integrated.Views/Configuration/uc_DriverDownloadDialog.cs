using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Helpers;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    /// <summary>Download database driver NuGet packages. Matches WPF BeepWpfDriverDownloadDialog patterns.</summary>
    [AddinAttribute(Caption = "Driver Download", Name = "uc_DriverDownloadDialog", misc = "Config",
        menu = "Configuration", addinType = AddinType.Control, displayType = DisplayType.InControl,
        ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 4, RootNodeName = "Configuration", Order = 4, ID = 4,
        BranchText = "Driver Download", BranchType = EnumPointType.Function, IconImageName = "drivers.svg",
        BranchClass = "ADDIN", BranchDescription = "Download database driver packages")]
    public partial class uc_DriverDownloadDialog : TemplateUserControl, IAddinVisSchema
    {
        private IDMEEditor _editor = null!;
        private string _driversDirectory = "";
        private CancellationTokenSource? _cts;
        private List<ConnectionDriversConfig> _allDrivers = new();

        // Controls
        private BeepPanel _mainPanel = null!;
        private BeepLabel _headerLabel = null!;
        private BeepLabel _subtitleLabel = null!;
        private BeepComboBox _repoCombo = null!;
        private BeepTextBox _filterBox = null!;
        private BeepListBox _driverListBox = null!;
        private BeepLabel _statusLabel = null!;
        private BeepButton _downloadBtn = null!;
        private BeepProgressBar _progressBar = null!;

        /// <summary>
        /// Designer/parameterless ctor. Must not chain to the IServiceProvider overload with null —
        /// that resolves services off a null provider and throws.
        /// </summary>
        /// <remarks>
        /// For the designer only. An instance built this way has no beepService, and Configure()
        /// dereferences it — so the runtime must always construct through the IServiceProvider
        /// overload. RoutingManager's Activator fallback is what makes that so: it now prefers this
        /// type's (IServiceProvider) constructor rather than calling the parameterless one.
        /// </remarks>
        public uc_DriverDownloadDialog() => InitializeControl();

        public uc_DriverDownloadDialog(IServiceProvider services) : base(services) => InitializeControl();

        private void InitializeControl()
        {
            InitializeComponent();
            Details.AddinName = "Driver Download";
            Size = BeepLayoutMetrics.DialogLarge.ScaleSize(this);
        }

        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);
            _editor = beepService.DMEEditor;
            _driversDirectory = string.IsNullOrEmpty(_editor.ConfigEditor?.Config?.ConnectionDriversPath)
                ? Path.Combine(_editor.ConfigEditor?.ExePath ?? ".", "ConnectionDrivers")
                : _editor.ConfigEditor.Config.ConnectionDriversPath;
            if (!Directory.Exists(_driversDirectory)) Directory.CreateDirectory(_driversDirectory);

            BuildUi();
            LoadRepositories();
            LoadDrivers();
        }

        private void BuildUi()
        {
            _mainPanel = new BeepPanel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            Controls.Add(_mainPanel);

            var headerPanel = new BeepPanel { Dock = DockStyle.Top, Height = 60 };
            _headerLabel = new BeepLabel { Text = "Download Database Drivers", Location = new Point(0, 4),
                Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true };
            _subtitleLabel = new BeepLabel { Text = "Select a driver and download its NuGet package.",
                Location = new Point(0, 30), Font = new Font("Segoe UI", 9), AutoSize = true };
            headerPanel.Controls.Add(_headerLabel);
            headerPanel.Controls.Add(_subtitleLabel);
            _mainPanel.Controls.Add(headerPanel);

            var filterPanel = new BeepPanel { Dock = DockStyle.Top, Height = 32 };
            _repoCombo = new BeepComboBox { Location = new Point(0, 4), Width = 160, PlaceholderText = "Repository" };
            _filterBox = new BeepTextBox { Location = new Point(170, 4), Width = 250, PlaceholderText = "Filter drivers..." };
            _filterBox.TextChanged += (_, _) => ApplyFilter();
            filterPanel.Controls.Add(_repoCombo);
            filterPanel.Controls.Add(_filterBox);
            _mainPanel.Controls.Add(filterPanel);

            var splitPanel = new BeepPanel { Dock = DockStyle.Fill };
            _driverListBox = new BeepListBox { Dock = DockStyle.Fill, Width = 300 };
            splitPanel.Controls.Add(_driverListBox);

            _mainPanel.Controls.Add(splitPanel);

            var bottomPanel = new BeepPanel { Dock = DockStyle.Bottom, Height = 36 };
            _statusLabel = new BeepLabel { Text = "Ready", Location = new Point(0, 10), AutoSize = true,
                Font = new Font("Segoe UI", 9) };
            _progressBar = new BeepProgressBar { Location = new Point(0, 30), Width = 200, Height = 4, Visible = false };
            _downloadBtn = new BeepButton { Text = "Download", Location = new Point(350, 4), Width = 90, Height = 28,
                Enabled = false };
            _downloadBtn.Click += async (_, _) => await DownloadDriverAsync();
            bottomPanel.Controls.Add(_statusLabel);
            bottomPanel.Controls.Add(_progressBar);
            bottomPanel.Controls.Add(_downloadBtn);
            _mainPanel.Controls.Add(bottomPanel);
        }

        private void LoadRepositories()
        {
            var items = new List<SimpleItem> { new() { Text = "All (NuGet.org)", Value = null, Name = "All" } };
            _repoCombo.ListItems = items.ToBindingList();
            if (items.Count > 0) _repoCombo.SelectedValue = null;
        }

        private void LoadDrivers()
        {
            _editor.ConfigEditor!.LoadConnectionDriversConfigValues();
            if (_editor.ConfigEditor.DataDriversClasses == null || !_editor.ConfigEditor.DataDriversClasses.Any())
                _editor.ConfigEditor.DataDriversClasses = ConnectionHelper.GetAllConnectionConfigs();

            _allDrivers = _editor.ConfigEditor.DataDriversClasses
                .Where(d => d.NeedDrivers && !d.InMemory && !string.IsNullOrWhiteSpace(d.PackageName))
                .OrderBy(d => d.DatasourceType.ToString()).ToList();

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var filter = _filterBox?.Text?.Trim() ?? "";
            var filtered = string.IsNullOrEmpty(filter)
                ? _allDrivers
                : _allDrivers.Where(d =>
                    d.DatasourceType.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    (d.PackageName ?? "").Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();

            var items = filtered.Select(d => new SimpleItem
            {
                DisplayField = $"{d.DatasourceType} — {d.PackageName}",
                Text = $"{d.DatasourceType} — {d.PackageName}",
                Name = d.PackageName ?? "",
                Value = d
            }).ToBindingList();

            _driverListBox.ListItems = items;
            _statusLabel.Text = $"{filtered.Count} driver(s)";
        }

        private bool CheckInstalled(ConnectionDriversConfig d)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(d.dllname)) return false;
                var folder = Path.Combine(_driversDirectory, d.DatasourceType.ToString());
                return File.Exists(Path.Combine(folder, d.dllname));
            }
            catch { return false; }
        }

        private async Task DownloadDriverAsync()
        {
            if (_driverListBox.SelectedItem?.Value is not ConnectionDriversConfig d) return;
            if (CheckInstalled(d))
            {
                if (MessageBox.Show($"{d.DatasourceType} driver is already installed. Reinstall?",
                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            }

            _downloadBtn.Enabled = false;
            _progressBar.Visible = true;
            _progressBar.IsIndeterminate = true;
            _statusLabel.Text = $"Downloading {d.PackageName}...";

            try
            {
                var folder = Path.Combine(_driversDirectory, d.DatasourceType.ToString());
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                await Task.Run(() =>
                {
                    _editor.ConfigEditor.LoadConnectionDriversConfigValues();
                    _editor.ConfigEditor.SaveDataconnectionsValues();
                });

                _statusLabel.Text = $"✅ {d.PackageName} installed to {folder}";
                LoadDrivers();
            }
            catch (Exception ex) { _statusLabel.Text = $"❌ Error: {ex.Message}"; }
            finally
            {
                _downloadBtn.Enabled = true;
                _progressBar.Visible = false;
            }
        }

        #region IAddinVisSchema
        public string RootNodeName { get; set; } = "Configuration";
        public string CatgoryName { get; set; } = "";
        public int Order { get; set; } = 4;
        public int ID { get; set; } = 4;
        public string BranchText { get; set; } = "Driver Download";
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        public int BranchID { get; set; } = 4;
        public string IconImageName { get; set; } = "drivers.svg";
        public string BranchStatus { get; set; } = "";
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; } = "Download database driver packages";
        public string BranchClass { get; set; } = "ADDIN";
        public string AddinName { get; set; } = "uc_DriverDownloadDialog";
        #endregion
    }
}
