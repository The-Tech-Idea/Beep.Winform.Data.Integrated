using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    public class NuGetRepositoryForm : Form
    {
        private static readonly string ConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "nuget-repositories.json");

        private List<NuGetRepoItem> _repositories = new();
        private ListBox _listBox = null!;
        private Button _btnAdd = null!;
        private Button _btnRemove = null!;
        private Button _btnSetDefault = null!;
        private Button _btnTest = null!;
        private Button _btnEnable = null!;
        private Button _btnDisable = null!;
        private BeepTextBox _txtName = null!;
        private BeepTextBox _txtUrl = null!;
        private CheckBox _chkEnabled = null!;
        private CheckBox _chkDefault = null!;
        private Button _btnSave = null!;
        private Button _btnCancel = null!;
        private BeepLabel _lblStatus = null!;
        private TableLayoutPanel _layout = null!;

        public NuGetRepositoryForm()
        {
            InitializeComponent();
            this.Load += (_, _) => LoadFromConfig();
        }

        private void InitializeComponent()
        {
            Text = "NuGet Repository Manager";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(570, 480);
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimizeBox = false;
            MaximizeBox = false;

            _layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(12)
            };
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));

            var header = new BeepLabel
            {
                Text = "NuGet Package Sources",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Fill,
                UseThemeColors = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            _layout.Controls.Add(header, 0, 0);

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 2, 0, 2)
            };

            _btnAdd = new Button { Text = "Add", Width = 55, Height = 26, Margin = new Padding(0, 0, 4, 0) };
            _btnAdd.Click += (_, _) => AddRepo();

            _btnRemove = new Button { Text = "Remove", Width = 65, Height = 26, Margin = new Padding(0, 0, 4, 0) };
            _btnRemove.Click += (_, _) => RemoveRepo();

            _btnSetDefault = new Button { Text = "★ Set Default", Width = 95, Height = 26, Margin = new Padding(0, 0, 4, 0) };
            _btnSetDefault.Click += (_, _) => SetDefaultRepo();

            _btnTest = new Button { Text = "Test", Width = 55, Height = 26, Margin = new Padding(0, 0, 4, 0) };
            _btnTest.Click += (_, _) => TestRepo();

            _btnEnable = new Button { Text = "Enable", Width = 60, Height = 26, Margin = new Padding(0, 0, 4, 0) };
            _btnEnable.Click += (_, _) => ToggleRepo(true);

            _btnDisable = new Button { Text = "Disable", Width = 65, Height = 26 };
            _btnDisable.Click += (_, _) => ToggleRepo(false);

            toolbar.Controls.Add(_btnAdd);
            toolbar.Controls.Add(_btnRemove);
            toolbar.Controls.Add(new Label { Text = "", Width = 12 });
            toolbar.Controls.Add(_btnSetDefault);
            toolbar.Controls.Add(_btnTest);
            toolbar.Controls.Add(new Label { Text = "", Width = 12 });
            toolbar.Controls.Add(_btnEnable);
            toolbar.Controls.Add(_btnDisable);

            _layout.Controls.Add(toolbar, 0, 1);

            _listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                IntegralHeight = false,
                Margin = new Padding(0, 0, 0, 6)
            };
            _listBox.SelectedIndexChanged += (_, _) => SyncEditFields();
            _listBox.DrawMode = DrawMode.OwnerDrawFixed;
            _listBox.ItemHeight = 32;
            _listBox.DrawItem += DrawListItem;
            _layout.Controls.Add(_listBox, 0, 2);

            var detailGroup = new GroupBox
            {
                Text = "Repository Details",
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                Margin = new Padding(0, 0, 0, 6)
            };

            var detailLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4
            };
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
            detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            detailLayout.Controls.Add(new Label { Text = "Name:", TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            _txtName = new BeepTextBox { Dock = DockStyle.Fill, UseThemeColors = true };
            detailLayout.Controls.Add(_txtName, 1, 0);

            detailLayout.Controls.Add(new Label { Text = "URL:", TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            _txtUrl = new BeepTextBox { Dock = DockStyle.Fill, UseThemeColors = true };
            detailLayout.Controls.Add(_txtUrl, 1, 1);

            _chkEnabled = new CheckBox { Text = "Enabled", Checked = true, Dock = DockStyle.Fill };
            detailLayout.Controls.Add(_chkEnabled, 1, 2);

            _chkDefault = new CheckBox { Text = "Set as Default", Dock = DockStyle.Fill };
            detailLayout.Controls.Add(_chkDefault, 1, 3);

            detailGroup.Controls.Add(detailLayout);
            _layout.Controls.Add(detailGroup, 0, 3);

            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 4, 0, 0)
            };

            _btnCancel = new Button { Text = "Cancel", Width = 70, Height = 28, Margin = new Padding(8, 0, 0, 0) };
            _btnCancel.Click += (_, _) => Close();

            _btnSave = new Button { Text = "Save", Width = 70, Height = 28, Margin = new Padding(0, 0, 4, 0) };
            _btnSave.Click += (_, _) => SaveRepos();

            _lblStatus = new BeepLabel
            {
                Text = string.Empty,
                AutoSize = true,
                UseThemeColors = true,
                Margin = new Padding(0, 4, 12, 0)
            };

            footer.Controls.Add(_btnCancel);
            footer.Controls.Add(_btnSave);
            footer.Controls.Add(_lblStatus);

            _layout.Controls.Add(footer, 0, 4);
            Controls.Add(_layout);
        }

        private void DrawListItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _repositories.Count) return;
            var repo = _repositories[e.Index];
            e.DrawBackground();

            string prefix = repo.IsDefault ? "★ " : "  ";
            string status = repo.IsEnabled ? "Enabled" : "Disabled";
            Color statusColor = repo.IsEnabled ? Color.FromArgb(46, 125, 50) : Color.FromArgb(198, 40, 40);

            using var nameFont = new Font("Segoe UI", 9, FontStyle.Bold);
            using var urlFont = new Font("Segoe UI", 8);
            using var statusFont = new Font("Segoe UI", 8);

            e.Graphics.DrawString($"{prefix}{repo.Name}", nameFont, Brushes.Black, e.Bounds.X + 4, e.Bounds.Y + 2);
            e.Graphics.DrawString(repo.Url, urlFont, Brushes.Gray, e.Bounds.X + 4, e.Bounds.Y + 18);
            using var statusBrush = new SolidBrush(statusColor);
            e.Graphics.DrawString(status, statusFont, statusBrush, e.Bounds.Right - 70, e.Bounds.Y + 8);
        }

        private void LoadFromConfig()
        {
            try
            {
                if (File.Exists(ConfigFile))
                {
                    var json = File.ReadAllText(ConfigFile);
                    var items = JsonSerializer.Deserialize<List<NuGetRepoItem>>(json);
                    if (items != null) _repositories = items;
                }
            }
            catch { }

            if (_repositories.Count == 0)
                _repositories.Add(new NuGetRepoItem { Name = "nuget.org", Url = "https://api.nuget.org/v3/index.json", IsEnabled = true, IsDefault = true });

            RefreshList();
        }

        private void SaveToConfig()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigFile)!);
                var json = JsonSerializer.Serialize(_repositories, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigFile, json);
            }
            catch (Exception ex) { _lblStatus.Text = $"Save error: {ex.Message}"; }
        }

        private void RefreshList()
        {
            _listBox.DataSource = null;
            _listBox.DataSource = _repositories;
            _listBox.DisplayMember = "Name";
            _listBox.SelectedIndex = -1;
            if (_repositories.Count > 0)
                _listBox.SelectedIndex = _repositories.FindIndex(r => r.IsDefault);
            if (_listBox.SelectedIndex < 0) _listBox.SelectedIndex = 0;
        }

        private void SyncEditFields()
        {
            var repo = _listBox.SelectedItem as NuGetRepoItem;
            if (repo == null) return;
            _txtName.Text = repo.Name;
            _txtUrl.Text = repo.Url;
            _chkEnabled.Checked = repo.IsEnabled;
            _chkDefault.Checked = repo.IsDefault;
        }

        private void AddRepo()
        {
            var repo = new NuGetRepoItem { Name = "New Repository", Url = "", IsEnabled = true, IsDefault = !_repositories.Any(r => r.IsDefault) };
            _repositories.Add(repo);
            RefreshList();
            _listBox.SelectedIndex = _repositories.Count - 1;
        }

        private void RemoveRepo()
        {
            var repo = _listBox.SelectedItem as NuGetRepoItem;
            if (repo == null) return;
            if (repo.IsDefault) { _lblStatus.Text = "Cannot remove default repository."; return; }
            _repositories.Remove(repo);
            RefreshList();
        }

        private void SetDefaultRepo()
        {
            var repo = _listBox.SelectedItem as NuGetRepoItem;
            if (repo == null) return;
            foreach (var r in _repositories) r.IsDefault = false;
            repo.IsDefault = true;
            repo.IsEnabled = true;
            _lblStatus.Text = $"Default: {repo.Name}";
            RefreshList();
            SyncEditFields();
        }

        private async void TestRepo()
        {
            var repo = _listBox.SelectedItem as NuGetRepoItem;
            if (repo == null) return;
            _lblStatus.Text = "Testing...";
            try
            {
                using var http = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                var resp = await http.GetAsync(repo.Url);
                if (IsDisposed) return;
                _lblStatus.Text = resp.IsSuccessStatusCode ? "✅ Reachable" : $"⚠️ HTTP {(int)resp.StatusCode}";
            }
            catch (Exception ex) { if (!IsDisposed) _lblStatus.Text = $"❌ {ex.Message}"; }
        }

        private void ToggleRepo(bool enable)
        {
            var repo = _listBox.SelectedItem as NuGetRepoItem;
            if (repo == null) return;
            if (!enable && repo.IsDefault) { _lblStatus.Text = "Cannot disable default repository."; return; }
            repo.IsEnabled = enable;
            _chkEnabled.Checked = enable;
            _listBox.Refresh();
        }

        private void SaveRepos()
        {
            var repo = _listBox.SelectedItem as NuGetRepoItem;
            if (repo != null)
            {
                repo.Name = _txtName.Text?.Trim() ?? string.Empty;
                repo.Url = _txtUrl.Text?.Trim() ?? string.Empty;
                repo.IsEnabled = _chkEnabled.Checked;
                if (_chkDefault.Checked)
                {
                    foreach (var r in _repositories) r.IsDefault = false;
                    repo.IsDefault = true;
                }
            }

            if (_repositories.Any(r => string.IsNullOrWhiteSpace(r.Name)))
            { _lblStatus.Text = "All repositories must have a name."; return; }
            if (_repositories.Any(r => string.IsNullOrWhiteSpace(r.Url)))
            { _lblStatus.Text = "All repositories must have a URL."; return; }
            if (!_repositories.Any(r => r.IsEnabled))
            { _lblStatus.Text = "At least one repository must be enabled."; return; }
            if (!_repositories.Any(r => r.IsDefault))
            { _lblStatus.Text = "One repository must be set as default."; return; }

            SaveToConfig();
            _lblStatus.Text = "Saved.";
            RefreshList();
        }
    }

    public class NuGetRepoItem
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public bool IsDefault { get; set; }
    }
}
