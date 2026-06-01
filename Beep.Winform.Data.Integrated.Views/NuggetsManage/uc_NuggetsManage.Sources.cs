using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.NuGet;
using TheTechIdea.Beep.Winform.Controls.GridX;
using TheTechIdea.Beep.Winform.Controls.Models;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    public partial class uc_NuggetsManage
    {
        private DataTable? _dtSources;
        private NuGetSourceConfig? _editingSource;

        private void InitializeSourcesData()
        {
            _dtSources = new DataTable("Sources");
            _dtSources.Columns.Add("Name", typeof(string));
            _dtSources.Columns.Add("Url", typeof(string));
            _dtSources.Columns.Add("Enabled", typeof(string));
            _dtSources.Columns.Add("Type", typeof(string));

            _gridSources.Columns.Add(new BeepColumnConfig { ColumnName = "Name", ColumnCaption = "Name", Width = 150 });
            _gridSources.Columns.Add(new BeepColumnConfig { ColumnName = "Url", ColumnCaption = "URL / Path", Width = 300 });
            _gridSources.Columns.Add(new BeepColumnConfig { ColumnName = "Enabled", ColumnCaption = "Enabled", Width = 70 });
            _gridSources.Columns.Add(new BeepColumnConfig { ColumnName = "Type", ColumnCaption = "Type", Width = 80 });
            _gridSources.DataSource = _dtSources;
        }

        // ---- Event handlers wired in Designer ----

        private void BtnSourceAdd_Click(object? sender, EventArgs e) => BeginAddSource();
        private void BtnSourceEdit_Click(object? sender, EventArgs e) => BeginEditSource();
        private void BtnSourceRemove_Click(object? sender, EventArgs e) => RemoveSource();

        private async void BtnSourceTest_Click(object? sender, EventArgs e)
        {
            try { await TestSelectedSource(); }
            catch (Exception ex) { _lblSourceStatus.Text = $"Test error: {ex.Message}"; }
        }

        private void BtnSourceSave_Click(object? sender, EventArgs e) => SaveSource();
        private void BtnSourceCancel_Click(object? sender, EventArgs e) => CancelSourceEdit();
        private void TabSources_Enter(object? sender, EventArgs e) => RefreshSources();

        // ---- Logic ----

        private void RefreshSources()
        {
            if (_dtSources == null) return;
            _dtSources.Rows.Clear();
            try
            {
                var sources = GetService().GetAllSources();
                foreach (var source in sources)
                    _dtSources.Rows.Add(source.Name, source.Url, source.IsEnabled ? "Yes" : "No", source.IsLocal ? "Local" : "NuGet");
                _lblSourceStatus.Text = $"{sources.Count} sources";
            }
            catch (Exception ex) { _lblSourceStatus.Text = $"Refresh failed: {ex.Message}"; }
        }

        private void BeginAddSource()
        {
            _editingSource = null;
            _txtSourceName.Text = string.Empty;
            _txtSourceUrl.Text = string.Empty;
            _chkSourceEnabled.CurrentValue = true;
            _txtSourceName.Focus();
        }

        private void BeginEditSource()
        {
            if (_gridSources?.CurrentRow == null) return;
            var name = _gridSources.CurrentRow.Cells["Name"]?.Value?.ToString();
            if (string.IsNullOrWhiteSpace(name)) return;
            try
            {
                var source = GetService().GetAllSources().FirstOrDefault(s => s.Name == name);
                if (source == null) return;
                _editingSource = source;
                _txtSourceName.Text = source.Name;
                _txtSourceUrl.Text = source.Url;
                _chkSourceEnabled.CurrentValue = source.IsEnabled;
            }
            catch (Exception ex) { _lblSourceStatus.Text = $"Edit failed: {ex.Message}"; }
        }

        private void SaveSource()
        {
            var name = _txtSourceName.Text?.Trim() ?? string.Empty;
            var url = _txtSourceUrl.Text?.Trim() ?? string.Empty;
            var enabled = _chkSourceEnabled.CurrentValue;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url))
            {
                _lblSourceStatus.Text = "Name and URL are required.";
                return;
            }
            try
            {
                GetService().AddSource(name, url, enabled);
                _lblSourceStatus.Text = $"Source '{name}' saved.";
                RefreshSources();
                _editingSource = null;
                _txtSourceName.Text = string.Empty;
                _txtSourceUrl.Text = string.Empty;
            }
            catch (Exception ex) { _lblSourceStatus.Text = $"Save failed: {ex.Message}"; }
        }

        private void CancelSourceEdit()
        {
            _editingSource = null;
            _txtSourceName.Text = string.Empty;
            _txtSourceUrl.Text = string.Empty;
        }

        private void RemoveSource()
        {
            if (_gridSources?.CurrentRow == null) return;
            var name = _gridSources.CurrentRow.Cells["Name"]?.Value?.ToString();
            if (string.IsNullOrWhiteSpace(name)) return;
            try
            {
                GetService().RemoveSource(name);
                _lblSourceStatus.Text = $"Source '{name}' removed.";
                RefreshSources();
            }
            catch (Exception ex) { _lblSourceStatus.Text = $"Remove failed: {ex.Message}"; }
        }

        private async System.Threading.Tasks.Task TestSelectedSource()
        {
            if (_gridSources?.CurrentRow == null) return;
            var name = _gridSources.CurrentRow.Cells["Name"]?.Value?.ToString();
            if (string.IsNullOrWhiteSpace(name)) return;
            try
            {
                var source = GetService().GetAllSources().FirstOrDefault(s => s.Name == name);
                if (source == null) return;
                _lblSourceStatus.Text = $"Testing '{name}'...";
                var healthy = await GetService().TestSourceAsync(source);
                _lblSourceStatus.Text = $"Source '{name}' is {(healthy ? "healthy" : "unreachable")}.";
            }
            catch (Exception ex) { _lblSourceStatus.Text = $"Test failed: {ex.Message}"; }
        }
    }
}

