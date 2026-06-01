using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls.GridX;
using TheTechIdea.Beep.Winform.Controls.Models;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    public partial class uc_NuggetsManage
    {
        private DataTable? _dtInstalled;

        private void InitializeInstalledData()
        {
            _dtInstalled = new DataTable("InstalledNuggets");
            _dtInstalled.Columns.Add("PackageId", typeof(string));
            _dtInstalled.Columns.Add("Version", typeof(string));
            _dtInstalled.Columns.Add("Status", typeof(string));
            _dtInstalled.Columns.Add("Startup", typeof(string));
            _dtInstalled.Columns.Add("Source", typeof(string));
            _dtInstalled.Columns.Add("InstallPath", typeof(string));

            _gridInstalled.Columns.Add(new BeepColumnConfig { ColumnName = "PackageId", ColumnCaption = "Package", Width = 180 });
            _gridInstalled.Columns.Add(new BeepColumnConfig { ColumnName = "Version", ColumnCaption = "Version", Width = 80 });
            _gridInstalled.Columns.Add(new BeepColumnConfig { ColumnName = "Status", ColumnCaption = "Status", Width = 80 });
            _gridInstalled.Columns.Add(new BeepColumnConfig { ColumnName = "Startup", ColumnCaption = "Startup", Width = 70 });
            _gridInstalled.Columns.Add(new BeepColumnConfig { ColumnName = "Source", ColumnCaption = "Source", Width = 150 });
            _gridInstalled.Columns.Add(new BeepColumnConfig { ColumnName = "InstallPath", ColumnCaption = "Path", Width = 200 });
            _gridInstalled.DataSource = _dtInstalled;
        }

        // ---- Event handlers wired in Designer ----

        private void TxtInstalledFilter_TextChanged(object? sender, EventArgs e) => FilterInstalled();
        private void BtnInstalledRefresh_Click(object? sender, EventArgs e) => RefreshInstalled();
        private void BtnInstalledLoad_Click(object? sender, EventArgs e) => LoadSelectedNugget();
        private void BtnInstalledUnload_Click(object? sender, EventArgs e) => UnloadSelectedNugget();

        private async void BtnInstalledRemove_Click(object? sender, EventArgs e)
        {
            try { await RemoveSelectedNuggetAsync(); }
            catch (Exception ex) { _lblInstalledStatus.Text = $"Remove error: {ex.Message}"; }
        }

        private async void BtnInstalledUpdate_Click(object? sender, EventArgs e)
        {
            try { await UpdateSelectedNuggetAsync(); }
            catch (Exception ex) { _lblInstalledStatus.Text = $"Update error: {ex.Message}"; }
        }

        private void GridInstalled_SelectionChanged(object? sender, EventArgs e) => OnInstalledSelected();
        private void ChkInstalledStartup_StateChanged(object? sender, EventArgs e) => SetStartupFlag();
        private void TabInstalled_Enter(object? sender, EventArgs e) => RefreshInstalled();

        // ---- Logic ----

        private void RefreshInstalled()
        {
            if (_dtInstalled == null) return;
            _dtInstalled.Rows.Clear();
            try
            {
                var states = GetService().GetInstalledStates();
                foreach (var state in states)
                {
                    var isLoaded = GetService().IsAssemblyLoaded(state.PackageId);
                    var status = isLoaded ? "Loaded" : (string.IsNullOrWhiteSpace(state.InstallPath) || !System.IO.Directory.Exists(state.InstallPath) ? "Missing" : "Unloaded");
                    _dtInstalled.Rows.Add(state.PackageId, state.Version, status, state.IsEnabledAtStartup ? "Yes" : "No", state.Source, state.InstallPath);
                }
                _lblInstalledStatus.Text = $"{states.Count} nuggets";
            }
            catch (Exception ex)
            {
                _lblInstalledStatus.Text = $"Refresh failed: {ex.Message}";
            }
        }

        private void FilterInstalled()
        {
            if (_dtInstalled == null) return;
            var filter = _txtInstalledFilter.Text?.ToLowerInvariant() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(filter)) { RefreshInstalled(); return; }

            _dtInstalled.Rows.Clear();
            try
            {
                var states = GetService().GetInstalledStates().Where(s => s.PackageId.ToLowerInvariant().Contains(filter)).ToList();
                foreach (var state in states)
                {
                    var isLoaded = GetService().IsAssemblyLoaded(state.PackageId);
                    var status = isLoaded ? "Loaded" : (string.IsNullOrWhiteSpace(state.InstallPath) || !System.IO.Directory.Exists(state.InstallPath) ? "Missing" : "Unloaded");
                    _dtInstalled.Rows.Add(state.PackageId, state.Version, status, state.IsEnabledAtStartup ? "Yes" : "No", state.Source, state.InstallPath);
                }
            }
            catch (Exception ex)
            {
                _lblInstalledStatus.Text = $"Filter failed: {ex.Message}";
            }
        }

        private void OnInstalledSelected()
        {
            if (_gridInstalled?.CurrentRow == null)
            {
                _lblInstalledDetailPackage.Text = "Package: (none selected)";
                _lblInstalledDetailVersion.Text = "Version:";
                _lblInstalledDetailStatus.Text = "Status:";
                _lblInstalledDetailSource.Text = "Source:";
                _lblInstalledDetailPath.Text = "Path:";
                _chkInstalledStartup.CurrentValue = false;
                return;
            }

            var packageId = _gridInstalled.CurrentRow.Cells["PackageId"]?.Value?.ToString() ?? string.Empty;
            var version = _gridInstalled.CurrentRow.Cells["Version"]?.Value?.ToString() ?? string.Empty;
            var status = _gridInstalled.CurrentRow.Cells["Status"]?.Value?.ToString() ?? string.Empty;
            var source = _gridInstalled.CurrentRow.Cells["Source"]?.Value?.ToString() ?? string.Empty;
            var path = _gridInstalled.CurrentRow.Cells["InstallPath"]?.Value?.ToString() ?? string.Empty;
            var startup = _gridInstalled.CurrentRow.Cells["Startup"]?.Value?.ToString() ?? "No";

            _lblInstalledDetailPackage.Text = $"Package: {packageId}";
            _lblInstalledDetailVersion.Text = $"Version: {version}";
            _lblInstalledDetailStatus.Text = $"Status: {status}";
            _lblInstalledDetailSource.Text = $"Source: {source}";
            _lblInstalledDetailPath.Text = $"Path: {path}";
            _chkInstalledStartup.CurrentValue = string.Equals(startup, "Yes", StringComparison.OrdinalIgnoreCase);
        }

        private void LoadSelectedNugget()
        {
            var packageId = GetSelectedInstalledPackageId();
            if (string.IsNullOrWhiteSpace(packageId)) return;
            try
            {
                var result = GetService().LoadNugget(packageId);
                _lblInstalledStatus.Text = result ? $"Loaded '{packageId}'" : $"Failed to load '{packageId}'";
                RefreshInstalled();
            }
            catch (Exception ex) { _lblInstalledStatus.Text = $"Load error: {ex.Message}"; }
        }

        private void UnloadSelectedNugget()
        {
            var packageId = GetSelectedInstalledPackageId();
            if (string.IsNullOrWhiteSpace(packageId)) return;
            try
            {
                var result = GetService().UnloadNugget(packageId);
                _lblInstalledStatus.Text = result ? $"Unloaded '{packageId}'" : $"Failed to unload '{packageId}'";
                RefreshInstalled();
            }
            catch (Exception ex) { _lblInstalledStatus.Text = $"Unload error: {ex.Message}"; }
        }

        private async System.Threading.Tasks.Task RemoveSelectedNuggetAsync()
        {
            var packageId = GetSelectedInstalledPackageId();
            if (string.IsNullOrWhiteSpace(packageId)) return;
            if (MessageBox.Show($"Remove '{packageId}'? This will unload and delete files.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                await GetService().RemoveAsync(packageId);
                _lblInstalledStatus.Text = $"Removed '{packageId}'";
                RefreshInstalled();
            }
            catch (Exception ex) { _lblInstalledStatus.Text = $"Remove error: {ex.Message}"; }
        }

        private async System.Threading.Tasks.Task UpdateSelectedNuggetAsync()
        {
            var packageId = GetSelectedInstalledPackageId();
            if (string.IsNullOrWhiteSpace(packageId)) return;
            _lblInstalledStatus.Text = $"Updating '{packageId}'...";
            try
            {
                var installResult = await GetService().UpdateAsync(packageId);
                _lblInstalledStatus.Text = installResult.Message;
                RefreshInstalled();
            }
            catch (Exception ex) { _lblInstalledStatus.Text = $"Update error: {ex.Message}"; }
        }

        private void SetStartupFlag()
        {
            var packageId = GetSelectedInstalledPackageId();
            if (string.IsNullOrWhiteSpace(packageId)) return;
            try
            {
                GetService().SetStartupEnabled(packageId, _chkInstalledStartup.CurrentValue);
                RefreshInstalled();
            }
            catch (Exception ex) { _lblInstalledStatus.Text = $"Startup flag error: {ex.Message}"; }
        }

        private string? GetSelectedInstalledPackageId()
            => _gridInstalled?.CurrentRow?.Cells["PackageId"]?.Value?.ToString();
    }
}

