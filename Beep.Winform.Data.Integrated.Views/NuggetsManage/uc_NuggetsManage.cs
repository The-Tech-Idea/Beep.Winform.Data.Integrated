using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    [AddinAttribute(Caption = "Nugget Manager", Name = "uc_NuggetsManage",
        misc = "Config", menu = "Configuration", addinType = AddinType.Control,
        displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 4, RootNodeName = "Configuration", Order = 4, ID = 4,
        BranchText = "Nugget Manager", BranchType = EnumPointType.Function,
        IconImageName = "drivers.svg", BranchClass = "ADDIN",
        BranchDescription = "NuGet package search, install, and management")]
    public partial class uc_NuggetsManage : TemplateUserControl, IAddinVisSchema, IDisposable
    {
        private readonly IServiceProvider _services;
        private NuggetsManageService? _service;
        private bool _disposed;

        public event EventHandler<NuggetInstallCompletedEventArgs>? PackageInstallCompleted;

        private NuggetsManageService GetService()
        {
            if (_service == null)
            {
                if (Editor == null)
                    throw new InvalidOperationException("Editor is not available.");
                _service = new NuggetsManageService(Editor);
            }
            return _service;
        }

        public uc_NuggetsManage(IServiceProvider services) : base(services)
        {
            _services = services;
            InitializeComponent();
           
            Details.AddinName = "Nugget Manager";
        }

        #region IAddinVisSchema
        public string RootNodeName { get; set; } = "Configuration";
        public string CatgoryName { get; set; } = string.Empty;
        public int Order { get; set; } = 4;
        public int ID { get; set; } = 4;
        public string BranchText { get; set; } = "Nugget Manager";
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        public int BranchID { get; set; } = 4;
        public string IconImageName { get; set; } = "drivers.svg";
        public string BranchStatus { get; set; } = string.Empty;
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; } = "NuGet package search, install, and management";
        public string BranchClass { get; set; } = "ADDIN";
        public string AddinName { get; set; } = "uc_NuggetsManage";
        #endregion
        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);
            InitializeSearchData();
            InitializeInstalledData();
            InitializeSourcesData();
            InitializeActivityData();
        }
        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            base.OnNavigatedTo(parameters);
            if (Editor == null)
            {
                MessageBox.Show("Editor is not available. Cannot initialize Nugget Manager.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Wire grid double-click to install wizard
            _gridSearchResults.DoubleClick -= GridSearchResults_DoubleClick;
            _gridSearchResults.DoubleClick += GridSearchResults_DoubleClick;

            RestoreLastTab();
            LoadSearchSources();
            RefreshInstalled();
            RefreshSources();
            RefreshLogs();
        }

        private void GridSearchResults_DoubleClick(object? sender, EventArgs e)
        {
            var packageId = _gridSearchResults?.CurrentRow?.Cells["PackageId"]?.Value?.ToString();
            if (!string.IsNullOrWhiteSpace(packageId))
                LaunchInstallWizard(packageId);
        }

        protected override void InitLayout()
        {
            base.InitLayout();
        }

        public override void ApplyTheme()
        {
            base.ApplyTheme();
            if(_tabs!=null)   _tabs.Theme = Theme;
        }

        private void RestoreLastTab()
        {
            try
            {
                var state = GetService().LoadState();
                if (state.LastActiveTabIndex >= 0 && state.LastActiveTabIndex < _tabs.TabCount)
                    _tabs.SelectedIndex = state.LastActiveTabIndex;
            }
            catch { /* ignore restore errors */ }
        }

        private void Tabs_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                var state = GetService().LoadState();
                state.LastActiveTabIndex = _tabs.SelectedIndex;
                GetService().SaveState(state);
            }
            catch { /* ignore save errors */ }
        }

        internal void RaisePackageInstallCompleted(string packageId, string version, bool success, string message)
        {
            PackageInstallCompleted?.Invoke(this, new NuggetInstallCompletedEventArgs(packageId, version, success, message));
        }

        public sealed class NuggetInstallCompletedEventArgs : EventArgs
        {
            public NuggetInstallCompletedEventArgs(string packageId, string version, bool success, string message)
            {
                PackageId = packageId;
                Version = version;
                Success = success;
                Message = message;
            }

            public string PackageId { get; }
            public string Version { get; }
            public bool Success { get; }
            public string Message { get; }
        }
    }
}
