using System.ComponentModel;
using System.Windows.Forms;
using TheTechIdea.Beep.Icons;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;
using TheTechIdea.Beep.Winform.Controls.GridX;
using TheTechIdea.Beep.Winform.Controls.ProgressBars;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    partial class uc_NuggetsManage
    {
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _service?.Dispose();
                    _searchCts?.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            // ── Tabs ──────────────────────────────────────────────────────────
            _tabSearch    = new BeepTabPage();
            _tabInstalled = new BeepTabPage();
            _tabSources   = new BeepTabPage();
            _tabActivity  = new BeepTabPage();
            _tabs         = new BeepTabs();

            // ── Search tab controls ───────────────────────────────────────────
            _tlpSearchBar      = new TableLayoutPanel();
            _lblSearchSource   = new BeepLabel();
            _cmbSearchSource   = new BeepComboBox();
            _txtSearch         = new BeepTextBox();
            _chkPrerelease     = new BeepCheckBoxBool();
            _btnSearch         = new BeepButton();
            _gridSearchResults = new BeepGridPro();
            _pnlSearchStatus   = new Panel();
            _progressBar       = new BeepProgressBar();
            _lblSearchStatus   = new BeepLabel();

            // ── Installed tab controls ────────────────────────────────────────
            _tlpInstalledBar      = new TableLayoutPanel();
            _txtInstalledFilter   = new BeepTextBox();
            _btnInstalledRefresh  = new BeepButton();
            _btnInstalledLoad     = new BeepButton();
            _btnInstalledUnload   = new BeepButton();
            _btnInstalledRemove   = new BeepButton();
            _btnInstalledUpdate   = new BeepButton();
            _splitInstalled       = new SplitContainer();
            _gridInstalled        = new BeepGridPro();
            _tlpInstalledDetail   = new TableLayoutPanel();
            _lblInstalledDetailPackage = new BeepLabel();
            _lblInstalledDetailVersion = new BeepLabel();
            _lblInstalledDetailStatus  = new BeepLabel();
            _lblInstalledDetailSource  = new BeepLabel();
            _lblInstalledDetailPath    = new BeepLabel();
            _chkInstalledStartup       = new BeepCheckBoxBool();
            _pnlInstalledStatus        = new Panel();
            _lblInstalledStatus        = new BeepLabel();

            // ── Sources tab controls ──────────────────────────────────────────
            _tlpSourcesBar  = new TableLayoutPanel();
            _btnSourceAdd   = new BeepButton();
            _btnSourceEdit  = new BeepButton();
            _btnSourceRemove = new BeepButton();
            _btnSourceTest  = new BeepButton();
            _gridSources    = new BeepGridPro();
            _tlpSourceEdit  = new TableLayoutPanel();
            _txtSourceName  = new BeepTextBox();
            _txtSourceUrl   = new BeepTextBox();
            _chkSourceEnabled = new BeepCheckBoxBool();
            _btnSourceSave   = new BeepButton();
            _btnSourceCancel = new BeepButton();
            _pnlSourceStatus = new Panel();
            _lblSourceStatus = new BeepLabel();

            // ── Activity tab controls ─────────────────────────────────────────
            _tlpActivityBar = new TableLayoutPanel();
            _cmbLogFilter   = new BeepComboBox();
            _btnLogClear    = new BeepButton();
            _btnLogCopy     = new BeepButton();
            _btnLogExport   = new BeepButton();
            _gridLogs       = new BeepGridPro();
            _pnlLogStatus   = new Panel();
            _lblLogStatus   = new BeepLabel();

            ((ISupportInitialize)_splitInstalled).BeginInit();
            _splitInstalled.Panel1.SuspendLayout();
            _splitInstalled.Panel2.SuspendLayout();
            _splitInstalled.SuspendLayout();
            SuspendLayout();

            // ══════════════════════ SEARCH TAB ══════════════════════════════

            // _tlpSearchBar – 1 row / 5 cols: Source-label | Source-combo | Search-text | Prerelease | Search-btn
            _tlpSearchBar.Dock = DockStyle.Top;
            _tlpSearchBar.Height = 40;
            _tlpSearchBar.ColumnCount = 5;
            _tlpSearchBar.RowCount = 1;
            _tlpSearchBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _tlpSearchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            _tlpSearchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _tlpSearchBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _tlpSearchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            _tlpSearchBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _tlpSearchBar.Padding = new Padding(6, 6, 6, 0);
            _tlpSearchBar.Name = "_tlpSearchBar";

            _lblSearchSource.Text = "Source:";
            _lblSearchSource.AutoSize = true;
            _lblSearchSource.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _lblSearchSource.Margin = new Padding(0, 0, 4, 0);
            _lblSearchSource.Name = "_lblSearchSource";

            _cmbSearchSource.Dock = DockStyle.Fill;
            _cmbSearchSource.Margin = new Padding(0, 0, 6, 0);
            _cmbSearchSource.Name = "_cmbSearchSource";

            _txtSearch.Dock = DockStyle.Fill;
            _txtSearch.PlaceholderText = "Search packages…";
            _txtSearch.Margin = new Padding(0, 0, 6, 0);
            _txtSearch.Name = "_txtSearch";
            _txtSearch.KeyDown += TxtSearch_KeyDown;

            _chkPrerelease.Text = "Prerelease";
            _chkPrerelease.AutoSize = true;
            _chkPrerelease.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _chkPrerelease.Margin = new Padding(0, 0, 6, 0);
            _chkPrerelease.Name = "_chkPrerelease";

            _btnSearch.Text = "Search";
            _btnSearch.ImagePath = SvgsUIcons.Common.Search;
            _btnSearch.Dock = DockStyle.Fill;
            _btnSearch.Name = "_btnSearch";
            _btnSearch.Click += BtnSearch_Click;

            _tlpSearchBar.Controls.Add(_lblSearchSource, 0, 0);
            _tlpSearchBar.Controls.Add(_cmbSearchSource, 1, 0);
            _tlpSearchBar.Controls.Add(_txtSearch, 2, 0);
            _tlpSearchBar.Controls.Add(_chkPrerelease, 3, 0);
            _tlpSearchBar.Controls.Add(_btnSearch, 4, 0);

            _gridSearchResults.Dock = DockStyle.Fill;
            _gridSearchResults.Name = "_gridSearchResults";
            _gridSearchResults.SelectionChanged += GridSearchResults_SelectionChanged;

            _pnlSearchStatus.Dock = DockStyle.Bottom;
            _pnlSearchStatus.Height = 28;
            _pnlSearchStatus.Name = "_pnlSearchStatus";

            _progressBar.Dock = DockStyle.Bottom;
            _progressBar.Height = 4;
            _progressBar.Visible = false;
            _progressBar.Name = "_progressBar";

            _lblSearchStatus.Dock = DockStyle.Fill;
            _lblSearchStatus.Text = "Ready";
            _lblSearchStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblSearchStatus.Padding = new Padding(4, 0, 0, 0);
            _lblSearchStatus.Name = "_lblSearchStatus";
            _pnlSearchStatus.Controls.Add(_lblSearchStatus);

            _tabSearch.Text = "Search & Install";
            _tabSearch.Name = "tabSearch";
            _tabSearch.Controls.Add(_gridSearchResults);
            _tabSearch.Controls.Add(_tlpSearchBar);
            _tabSearch.Controls.Add(_pnlSearchStatus);
            _tabSearch.Controls.Add(_progressBar);
            _tabSearch.Enter += TabSearch_Enter;

            // ══════════════════════ INSTALLED TAB ═══════════════════════════

            _tlpInstalledBar.Dock = DockStyle.Top;
            _tlpInstalledBar.Height = 40;
            _tlpInstalledBar.ColumnCount = 6;
            _tlpInstalledBar.RowCount = 1;
            _tlpInstalledBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _tlpInstalledBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            _tlpInstalledBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            _tlpInstalledBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            _tlpInstalledBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            _tlpInstalledBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            _tlpInstalledBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _tlpInstalledBar.Padding = new Padding(6, 6, 6, 0);
            _tlpInstalledBar.Name = "_tlpInstalledBar";

            _txtInstalledFilter.Dock = DockStyle.Fill;
            _txtInstalledFilter.PlaceholderText = "Filter…";
            _txtInstalledFilter.Margin = new Padding(0, 0, 6, 0);
            _txtInstalledFilter.Name = "_txtInstalledFilter";
            _txtInstalledFilter.TextChanged += TxtInstalledFilter_TextChanged;

            _btnInstalledRefresh.Text = "Refresh";
            _btnInstalledRefresh.ImagePath = SvgsUIcons.Common.Refresh;
            _btnInstalledRefresh.Dock = DockStyle.Fill;
            _btnInstalledRefresh.Margin = new Padding(0, 0, 4, 0);
            _btnInstalledRefresh.Name = "_btnInstalledRefresh";
            _btnInstalledRefresh.Click += BtnInstalledRefresh_Click;

            _btnInstalledLoad.Text = "Load";
            _btnInstalledLoad.ImagePath = SvgsUIcons.Common.Play;
            _btnInstalledLoad.Dock = DockStyle.Fill;
            _btnInstalledLoad.Margin = new Padding(0, 0, 4, 0);
            _btnInstalledLoad.Name = "_btnInstalledLoad";
            _btnInstalledLoad.Click += BtnInstalledLoad_Click;

            _btnInstalledUnload.Text = "Unload";
            _btnInstalledUnload.ImagePath = SvgsUIcons.Common.Stop;
            _btnInstalledUnload.Dock = DockStyle.Fill;
            _btnInstalledUnload.Margin = new Padding(0, 0, 4, 0);
            _btnInstalledUnload.Name = "_btnInstalledUnload";
            _btnInstalledUnload.Click += BtnInstalledUnload_Click;

            _btnInstalledRemove.Text = "Remove";
            _btnInstalledRemove.ImagePath = SvgsUIcons.Common.Delete;
            _btnInstalledRemove.Dock = DockStyle.Fill;
            _btnInstalledRemove.Margin = new Padding(0, 0, 4, 0);
            _btnInstalledRemove.Name = "_btnInstalledRemove";
            _btnInstalledRemove.Click += BtnInstalledRemove_Click;

            _btnInstalledUpdate.Text = "Update";
            _btnInstalledUpdate.ImagePath = SvgsUIcons.Common.Download;
            _btnInstalledUpdate.Dock = DockStyle.Fill;
            _btnInstalledUpdate.Name = "_btnInstalledUpdate";
            _btnInstalledUpdate.Click += BtnInstalledUpdate_Click;

            _tlpInstalledBar.Controls.Add(_txtInstalledFilter, 0, 0);
            _tlpInstalledBar.Controls.Add(_btnInstalledRefresh, 1, 0);
            _tlpInstalledBar.Controls.Add(_btnInstalledLoad, 2, 0);
            _tlpInstalledBar.Controls.Add(_btnInstalledUnload, 3, 0);
            _tlpInstalledBar.Controls.Add(_btnInstalledRemove, 4, 0);
            _tlpInstalledBar.Controls.Add(_btnInstalledUpdate, 5, 0);

            _splitInstalled.Dock = DockStyle.Fill;
            _splitInstalled.SplitterDistance = 420;
            _splitInstalled.Name = "_splitInstalled";

            _gridInstalled.Dock = DockStyle.Fill;
            _gridInstalled.Name = "_gridInstalled";
            _gridInstalled.SelectionChanged += GridInstalled_SelectionChanged;
            _splitInstalled.Panel1.Controls.Add(_gridInstalled);

            _tlpInstalledDetail.Dock = DockStyle.Fill;
            _tlpInstalledDetail.ColumnCount = 2;
            _tlpInstalledDetail.RowCount = 6;
            _tlpInstalledDetail.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _tlpInstalledDetail.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _tlpInstalledDetail.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _tlpInstalledDetail.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _tlpInstalledDetail.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _tlpInstalledDetail.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _tlpInstalledDetail.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _tlpInstalledDetail.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _tlpInstalledDetail.Padding = new Padding(8);
            _tlpInstalledDetail.Name = "_tlpInstalledDetail";

            _lblInstalledDetailPackage.Text = "(none selected)";
            _lblInstalledDetailPackage.AutoSize = true;
            _lblInstalledDetailPackage.Anchor = AnchorStyles.Left;
            _tlpInstalledDetail.SetColumnSpan(_lblInstalledDetailPackage, 2);

            _lblInstalledDetailVersion.Text = string.Empty;
            _lblInstalledDetailVersion.AutoSize = true;
            _lblInstalledDetailVersion.Anchor = AnchorStyles.Left;
            _tlpInstalledDetail.SetColumnSpan(_lblInstalledDetailVersion, 2);

            _lblInstalledDetailStatus.Text = string.Empty;
            _lblInstalledDetailStatus.AutoSize = true;
            _lblInstalledDetailStatus.Anchor = AnchorStyles.Left;
            _tlpInstalledDetail.SetColumnSpan(_lblInstalledDetailStatus, 2);

            _lblInstalledDetailSource.Text = string.Empty;
            _lblInstalledDetailSource.AutoSize = true;
            _lblInstalledDetailSource.Anchor = AnchorStyles.Left;
            _tlpInstalledDetail.SetColumnSpan(_lblInstalledDetailSource, 2);

            _lblInstalledDetailPath.Text = string.Empty;
            _lblInstalledDetailPath.AutoSize = true;
            _lblInstalledDetailPath.Anchor = AnchorStyles.Left;
            _tlpInstalledDetail.SetColumnSpan(_lblInstalledDetailPath, 2);

            _chkInstalledStartup.Text = "Enable at startup";
            _chkInstalledStartup.AutoSize = true;
            _chkInstalledStartup.Anchor = AnchorStyles.Left;
            _chkInstalledStartup.Name = "_chkInstalledStartup";
            _chkInstalledStartup.StateChanged += ChkInstalledStartup_StateChanged;
            _tlpInstalledDetail.SetColumnSpan(_chkInstalledStartup, 2);

            _tlpInstalledDetail.Controls.Add(_lblInstalledDetailPackage, 0, 0);
            _tlpInstalledDetail.Controls.Add(_lblInstalledDetailVersion, 0, 1);
            _tlpInstalledDetail.Controls.Add(_lblInstalledDetailStatus, 0, 2);
            _tlpInstalledDetail.Controls.Add(_lblInstalledDetailSource, 0, 3);
            _tlpInstalledDetail.Controls.Add(_lblInstalledDetailPath, 0, 4);
            _tlpInstalledDetail.Controls.Add(_chkInstalledStartup, 0, 5);
            _splitInstalled.Panel2.Controls.Add(_tlpInstalledDetail);

            _pnlInstalledStatus.Dock = DockStyle.Bottom;
            _pnlInstalledStatus.Height = 28;
            _pnlInstalledStatus.Name = "_pnlInstalledStatus";

            _lblInstalledStatus.Dock = DockStyle.Fill;
            _lblInstalledStatus.Text = "Ready";
            _lblInstalledStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblInstalledStatus.Padding = new Padding(4, 0, 0, 0);
            _lblInstalledStatus.Name = "_lblInstalledStatus";
            _pnlInstalledStatus.Controls.Add(_lblInstalledStatus);

            _tabInstalled.Text = "Installed";
            _tabInstalled.Name = "tabInstalled";
            _tabInstalled.Controls.Add(_splitInstalled);
            _tabInstalled.Controls.Add(_tlpInstalledBar);
            _tabInstalled.Controls.Add(_pnlInstalledStatus);
            _tabInstalled.Enter += TabInstalled_Enter;

            // ══════════════════════ SOURCES TAB ═════════════════════════════

            _tlpSourcesBar.Dock = DockStyle.Top;
            _tlpSourcesBar.Height = 40;
            _tlpSourcesBar.ColumnCount = 4;
            _tlpSourcesBar.RowCount = 1;
            _tlpSourcesBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            _tlpSourcesBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            _tlpSourcesBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            _tlpSourcesBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            _tlpSourcesBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _tlpSourcesBar.Padding = new Padding(6, 6, 6, 0);
            _tlpSourcesBar.Name = "_tlpSourcesBar";

            _btnSourceAdd.Text = "Add";
            _btnSourceAdd.ImagePath = SvgsUIcons.Common.Add;
            _btnSourceAdd.Dock = DockStyle.Fill;
            _btnSourceAdd.Margin = new Padding(0, 0, 4, 0);
            _btnSourceAdd.Name = "_btnSourceAdd";
            _btnSourceAdd.Click += BtnSourceAdd_Click;

            _btnSourceEdit.Text = "Edit";
            _btnSourceEdit.ImagePath = SvgsUIcons.Common.Edit;
            _btnSourceEdit.Dock = DockStyle.Fill;
            _btnSourceEdit.Margin = new Padding(0, 0, 4, 0);
            _btnSourceEdit.Name = "_btnSourceEdit";
            _btnSourceEdit.Click += BtnSourceEdit_Click;

            _btnSourceRemove.Text = "Remove";
            _btnSourceRemove.ImagePath = SvgsUIcons.Common.Delete;
            _btnSourceRemove.Dock = DockStyle.Fill;
            _btnSourceRemove.Margin = new Padding(0, 0, 4, 0);
            _btnSourceRemove.Name = "_btnSourceRemove";
            _btnSourceRemove.Click += BtnSourceRemove_Click;

            _btnSourceTest.Text = "Test";
            _btnSourceTest.ImagePath = SvgsUIcons.NetworkCloud.Wifi;
            _btnSourceTest.Dock = DockStyle.Fill;
            _btnSourceTest.Name = "_btnSourceTest";
            _btnSourceTest.Click += BtnSourceTest_Click;

            _tlpSourcesBar.Controls.Add(_btnSourceAdd, 0, 0);
            _tlpSourcesBar.Controls.Add(_btnSourceEdit, 1, 0);
            _tlpSourcesBar.Controls.Add(_btnSourceRemove, 2, 0);
            _tlpSourcesBar.Controls.Add(_btnSourceTest, 3, 0);

            _gridSources.Dock = DockStyle.Fill;
            _gridSources.Name = "_gridSources";

            _tlpSourceEdit.Dock = DockStyle.Bottom;
            _tlpSourceEdit.Height = 50;
            _tlpSourceEdit.ColumnCount = 5;
            _tlpSourceEdit.RowCount = 1;
            _tlpSourceEdit.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            _tlpSourceEdit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _tlpSourceEdit.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _tlpSourceEdit.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _tlpSourceEdit.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _tlpSourceEdit.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _tlpSourceEdit.Padding = new Padding(6);
            _tlpSourceEdit.Name = "_tlpSourceEdit";

            _txtSourceName.Dock = DockStyle.Fill;
            _txtSourceName.PlaceholderText = "Source name";
            _txtSourceName.Margin = new Padding(0, 0, 6, 0);
            _txtSourceName.Name = "_txtSourceName";

            _txtSourceUrl.Dock = DockStyle.Fill;
            _txtSourceUrl.PlaceholderText = "URL or local path";
            _txtSourceUrl.Margin = new Padding(0, 0, 6, 0);
            _txtSourceUrl.Name = "_txtSourceUrl";

            _chkSourceEnabled.Text = "Enabled";
            _chkSourceEnabled.AutoSize = true;
            _chkSourceEnabled.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _chkSourceEnabled.CurrentValue = true;
            _chkSourceEnabled.Margin = new Padding(0, 0, 6, 0);
            _chkSourceEnabled.Name = "_chkSourceEnabled";

            _btnSourceSave.Text = "Save";
            _btnSourceSave.ImagePath = SvgsUIcons.Common.Save;
            _btnSourceSave.AutoSize = true;
            _btnSourceSave.Margin = new Padding(0, 0, 4, 0);
            _btnSourceSave.Name = "_btnSourceSave";
            _btnSourceSave.Click += BtnSourceSave_Click;

            _btnSourceCancel.Text = "Cancel";
            _btnSourceCancel.ImagePath = SvgsUIcons.Common.Cancel;
            _btnSourceCancel.AutoSize = true;
            _btnSourceCancel.Name = "_btnSourceCancel";
            _btnSourceCancel.Click += BtnSourceCancel_Click;

            _tlpSourceEdit.Controls.Add(_txtSourceName, 0, 0);
            _tlpSourceEdit.Controls.Add(_txtSourceUrl, 1, 0);
            _tlpSourceEdit.Controls.Add(_chkSourceEnabled, 2, 0);
            _tlpSourceEdit.Controls.Add(_btnSourceSave, 3, 0);
            _tlpSourceEdit.Controls.Add(_btnSourceCancel, 4, 0);

            _pnlSourceStatus.Dock = DockStyle.Bottom;
            _pnlSourceStatus.Height = 28;
            _pnlSourceStatus.Name = "_pnlSourceStatus";

            _lblSourceStatus.Dock = DockStyle.Fill;
            _lblSourceStatus.Text = "Ready";
            _lblSourceStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblSourceStatus.Padding = new Padding(4, 0, 0, 0);
            _lblSourceStatus.Name = "_lblSourceStatus";
            _pnlSourceStatus.Controls.Add(_lblSourceStatus);

            _tabSources.Text = "Sources";
            _tabSources.Name = "tabSources";
            _tabSources.Controls.Add(_gridSources);
            _tabSources.Controls.Add(_tlpSourcesBar);
            _tabSources.Controls.Add(_tlpSourceEdit);
            _tabSources.Controls.Add(_pnlSourceStatus);
            _tabSources.Enter += TabSources_Enter;

            // ══════════════════════ ACTIVITY TAB ════════════════════════════

            _tlpActivityBar.Dock = DockStyle.Top;
            _tlpActivityBar.Height = 40;
            _tlpActivityBar.ColumnCount = 4;
            _tlpActivityBar.RowCount = 1;
            _tlpActivityBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            _tlpActivityBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            _tlpActivityBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            _tlpActivityBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            _tlpActivityBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _tlpActivityBar.Padding = new Padding(6, 6, 6, 0);
            _tlpActivityBar.Name = "_tlpActivityBar";

            _cmbLogFilter.Dock = DockStyle.Fill;
            _cmbLogFilter.Margin = new Padding(0, 0, 6, 0);
            _cmbLogFilter.Name = "_cmbLogFilter";
            _cmbLogFilter.SelectedItemChanged += CmbLogFilter_SelectedItemChanged;

            _btnLogClear.Text = "Clear";
            _btnLogClear.ImagePath = SvgsUIcons.Documents.RecycleBin;
            _btnLogClear.Dock = DockStyle.Fill;
            _btnLogClear.Margin = new Padding(0, 0, 4, 0);
            _btnLogClear.Name = "_btnLogClear";
            _btnLogClear.Click += BtnLogClear_Click;

            _btnLogCopy.Text = "Copy";
            _btnLogCopy.ImagePath = SvgsUIcons.Common.Copy;
            _btnLogCopy.Dock = DockStyle.Fill;
            _btnLogCopy.Margin = new Padding(0, 0, 4, 0);
            _btnLogCopy.Name = "_btnLogCopy";
            _btnLogCopy.Click += BtnLogCopy_Click;

            _btnLogExport.Text = "Export";
            _btnLogExport.ImagePath = SvgsUIcons.DataTable.Export;
            _btnLogExport.Dock = DockStyle.Fill;
            _btnLogExport.Name = "_btnLogExport";
            _btnLogExport.Click += BtnLogExport_Click;

            _tlpActivityBar.Controls.Add(_cmbLogFilter, 0, 0);
            _tlpActivityBar.Controls.Add(_btnLogClear, 1, 0);
            _tlpActivityBar.Controls.Add(_btnLogCopy, 2, 0);
            _tlpActivityBar.Controls.Add(_btnLogExport, 3, 0);

            _gridLogs.Dock = DockStyle.Fill;
            _gridLogs.Name = "_gridLogs";

            _pnlLogStatus.Dock = DockStyle.Bottom;
            _pnlLogStatus.Height = 28;
            _pnlLogStatus.Name = "_pnlLogStatus";

            _lblLogStatus.Dock = DockStyle.Fill;
            _lblLogStatus.Text = "Ready";
            _lblLogStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblLogStatus.Padding = new Padding(4, 0, 0, 0);
            _lblLogStatus.Name = "_lblLogStatus";
            _pnlLogStatus.Controls.Add(_lblLogStatus);

            _tabActivity.Text = "Activity";
            _tabActivity.Name = "tabActivity";
            _tabActivity.Controls.Add(_gridLogs);
            _tabActivity.Controls.Add(_tlpActivityBar);
            _tabActivity.Controls.Add(_pnlLogStatus);
            _tabActivity.Enter += TabActivity_Enter;

            // ══════════════════════ MAIN TABS ════════════════════════════════

            _tabs.Dock = DockStyle.Fill;
            _tabs.ShowCloseButtons = false;
            _tabs.HeaderHeight = 30;
            _tabs.HeaderPosition = TabHeaderPosition.Top;
            _tabs.TabStyle = TabStyle.Classic;
            _tabs.Name = "_tabs";
            _tabs.SelectedIndexChanged += Tabs_SelectedIndexChanged;

            _tabs.AddTab(_tabSearch);
            _tabs.AddTab(_tabInstalled);
            _tabs.AddTab(_tabSources);
            _tabs.AddTab(_tabActivity);

            // ══════════════════════ USER CONTROL ════════════════════════════

            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(_tabs);
            Name = "uc_NuggetsManage";

            _splitInstalled.Panel1.ResumeLayout(false);
            _splitInstalled.Panel2.ResumeLayout(false);
            ((ISupportInitialize)_splitInstalled).EndInit();
            _splitInstalled.ResumeLayout(false);

            ResumeLayout(true);
        }

        // ── Backing fields ────────────────────────────────────────────────────

        private BeepTabs _tabs;
        private BeepTabPage _tabSearch;
        private BeepTabPage _tabInstalled;
        private BeepTabPage _tabSources;
        private BeepTabPage _tabActivity;

        // Search tab
        private TableLayoutPanel _tlpSearchBar;
        private BeepLabel _lblSearchSource;
        private BeepComboBox _cmbSearchSource;
        private BeepTextBox _txtSearch;
        private BeepCheckBoxBool _chkPrerelease;
        private BeepButton _btnSearch;
        private BeepGridPro _gridSearchResults;
        private Panel _pnlSearchStatus;
        private BeepProgressBar _progressBar;
        private BeepLabel _lblSearchStatus;

        // Installed tab
        private TableLayoutPanel _tlpInstalledBar;
        private BeepTextBox _txtInstalledFilter;
        private BeepButton _btnInstalledRefresh;
        private BeepButton _btnInstalledLoad;
        private BeepButton _btnInstalledUnload;
        private BeepButton _btnInstalledRemove;
        private BeepButton _btnInstalledUpdate;
        private SplitContainer _splitInstalled;
        private BeepGridPro _gridInstalled;
        private TableLayoutPanel _tlpInstalledDetail;
        private BeepLabel _lblInstalledDetailPackage;
        private BeepLabel _lblInstalledDetailVersion;
        private BeepLabel _lblInstalledDetailStatus;
        private BeepLabel _lblInstalledDetailSource;
        private BeepLabel _lblInstalledDetailPath;
        private BeepCheckBoxBool _chkInstalledStartup;
        private Panel _pnlInstalledStatus;
        private BeepLabel _lblInstalledStatus;

        // Sources tab
        private TableLayoutPanel _tlpSourcesBar;
        private BeepButton _btnSourceAdd;
        private BeepButton _btnSourceEdit;
        private BeepButton _btnSourceRemove;
        private BeepButton _btnSourceTest;
        private BeepGridPro _gridSources;
        private TableLayoutPanel _tlpSourceEdit;
        private BeepTextBox _txtSourceName;
        private BeepTextBox _txtSourceUrl;
        private BeepCheckBoxBool _chkSourceEnabled;
        private BeepButton _btnSourceSave;
        private BeepButton _btnSourceCancel;
        private Panel _pnlSourceStatus;
        private BeepLabel _lblSourceStatus;

        // Activity tab
        private TableLayoutPanel _tlpActivityBar;
        private BeepComboBox _cmbLogFilter;
        private BeepButton _btnLogClear;
        private BeepButton _btnLogCopy;
        private BeepButton _btnLogExport;
        private BeepGridPro _gridLogs;
        private Panel _pnlLogStatus;
        private BeepLabel _lblLogStatus;
    }
}
