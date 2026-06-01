using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.NuGet;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Controls.Wizards;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    public partial class uc_NuggetsManage
    {
        private DataTable? _dtSearchResults;
        private CancellationTokenSource? _searchCts;
        private List<NuGetSearchResult>? _lastSearchResults;

        private void InitializeSearchData()
        {
            _dtSearchResults = new DataTable("SearchResults");
            _dtSearchResults.Columns.Add("PackageId", typeof(string));
            _dtSearchResults.Columns.Add("Version", typeof(string));
            _dtSearchResults.Columns.Add("Downloads", typeof(string));
            _dtSearchResults.Columns.Add("Description", typeof(string));

            _gridSearchResults.Columns.Add(new BeepColumnConfig { ColumnName = "PackageId", ColumnCaption = "Package", Width = 200 });
            _gridSearchResults.Columns.Add(new BeepColumnConfig { ColumnName = "Version", ColumnCaption = "Version", Width = 80 });
            _gridSearchResults.Columns.Add(new BeepColumnConfig { ColumnName = "Downloads", ColumnCaption = "Downloads", Width = 90 });
            _gridSearchResults.Columns.Add(new BeepColumnConfig { ColumnName = "Description", ColumnCaption = "Description", Width = 250 });
            _gridSearchResults.DataSource = _dtSearchResults;
        }

        // ---- Event handlers wired in Designer ----

        private async void TxtSearch_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try { await ExecuteSearchAsync(); }
                catch (Exception ex) { if (_lblSearchStatus != null) _lblSearchStatus.Text = $"Search error: {ex.Message}"; }
            }
        }

        private async void BtnSearch_Click(object? sender, EventArgs e)
        {
            try { await ExecuteSearchAsync(); }
            catch (Exception ex) { if (_lblSearchStatus != null) _lblSearchStatus.Text = $"Search error: {ex.Message}"; }
        }

        private void GridSearchResults_SelectionChanged(object? sender, EventArgs e)
        {
            // No inline detail panel; Install is launched via double-click or context menu.
        }

        private void TabSearch_Enter(object? sender, EventArgs e) => LoadSearchSources();

        // ---- Logic ----

        private void LoadSearchSources()
        {
            try
            {
                var sources = GetService().GetAllSources().Where(s => s.IsEnabled).ToList();
                var items = new BindingList<SimpleItem>(sources.Select(s => new SimpleItem { Text = s.Name, Item = s.Url }).ToList());
                _cmbSearchSource.ListItems = items;
                if (items.Count > 0 && _cmbSearchSource.SelectedItem == null)
                    _cmbSearchSource.SelectedItem = items[0];

                var state = GetService().LoadState();
                _chkPrerelease.CurrentValue = state.IncludePrerelease;
                if (!string.IsNullOrWhiteSpace(state.LastSearchTerm))
                    _txtSearch.Text = state.LastSearchTerm;
            }
            catch (Exception ex)
            {
                _lblSearchStatus.Text = $"Failed to load sources: {ex.Message}";
            }
        }

        private async Task ExecuteSearchAsync()
        {
            if (_dtSearchResults == null) return;

            var searchTerm = _txtSearch.Text?.Trim();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                _lblSearchStatus.Text = "Please enter a search term";
                return;
            }

            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = new CancellationTokenSource();

            _btnSearch.Enabled = false;
            _lblSearchStatus.Text = "Searching…";
            _progressBar.Visible = true;
            _dtSearchResults.Rows.Clear();
            _gridSearchResults.DataSource = null;
            _gridSearchResults.DataSource = _dtSearchResults;

            try
            {
                var results = await GetService().SearchAsync(searchTerm, _chkPrerelease.CurrentValue, _searchCts.Token);
                _lastSearchResults = results;
                foreach (var item in results)
                    _dtSearchResults.Rows.Add(item.PackageId, item.Version, item.TotalDownloads.ToString("N0"), Truncate(item.Description, 120));

                _gridSearchResults.DataSource = null;
                _gridSearchResults.DataSource = _dtSearchResults;
                _lblSearchStatus.Text = $"{results.Count} package{(results.Count == 1 ? "" : "s")} found — double-click to install";

                var state = GetService().LoadState();
                state.LastSearchTerm = searchTerm;
                state.IncludePrerelease = _chkPrerelease.CurrentValue;
                GetService().SaveState(state);
            }
            catch (OperationCanceledException)
            {
                _lblSearchStatus.Text = "Search cancelled.";
            }
            catch (Exception ex)
            {
                _lblSearchStatus.Text = $"Search failed: {ex.Message}";
                Editor?.AddLogMessage("NuggetsManage", $"Search error: {ex}", DateTime.Now, 0, null, Errors.Failed);
            }
            finally
            {
                _btnSearch.Enabled = true;
                _progressBar.Visible = false;
            }
        }

        internal void LaunchInstallWizard(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId)) return;

            var config = new WizardConfig
            {
                Key = "NuggetInstall",
                Title = $"Install — {packageId}",
                Size = new System.Drawing.Size(700, 520),
                FinishButtonText = "Install",
                Steps =
                {
                    new WizardStep { Key = "v", Title = "Version & Source", Content = new uc_NuggetsInstall_Step_VersionSource() },
                    new WizardStep { Key = "o", Title = "Options", Content = new uc_NuggetsInstall_Step_Options() },
                    new WizardStep { Key = "r", Title = "Install", Content = new uc_NuggetsInstall_Step_Run() }
                },
                OnComplete = ctx =>
                {
                    var result = ctx.GetValue<NuggetInstallResult?>(NuggetWizardKeys.InstallResult, null);
                    var version = ctx.GetValue(NuggetWizardKeys.SelectedVersion, string.Empty);
                    var success = result?.Success ?? false;
                    var message = result?.Message ?? (success ? "Done" : "Install did not complete");
                    _lblSearchStatus.Text = message;
                    RaisePackageInstallCompleted(packageId, version, success, message);
                    if (success) RefreshInstalled();
                }
            };

            config.Steps[0].OnEnter = ctx =>
            {
                ctx.SetValue(NuggetWizardKeys.Service, GetService());
                ctx.SetValue(NuggetWizardKeys.PackageId, packageId);
                ctx.SetValue(NuggetWizardKeys.IncludePrerelease, _chkPrerelease.CurrentValue);
            };

            _ = WizardManager.ShowWizardAsync(config, FindForm());
        }

        private static string Truncate(string? text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            return text.Length <= maxLength ? text : text[..maxLength] + "…";
        }
    }
}