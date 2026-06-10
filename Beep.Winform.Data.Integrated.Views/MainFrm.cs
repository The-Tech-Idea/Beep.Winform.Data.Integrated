
using System;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Integrated.NuggetsManage;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport;
using TheTechIdea.Beep.Winform.Default.Views.Setup;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views
{
    [AddinAttribute(Caption = "Home", Name = "MainForm", misc = "Main", menu = "Main", addinType = AddinType.Page, displayType = DisplayType.Popup, ObjectType = "Beep")]

    public partial class MainFrm : TemplateForm
    {
        private uc_SetupWizard? _setupWizard;
        private uc_ImportExportWizardLauncher? _importExportWizard;
        private uc_AppBootstrap? _appBootstrap;
        private BootstrapState? _bootstrapState;
        private System.Threading.CancellationTokenSource? _bootstrapCts;


        IServiceProvider _serviceprovider;
        public IDMEEditor Editor { get; }

        public MainFrm()
        {
            InitializeComponent();
            Theme = BeepThemesManager.CurrentThemeName;
            FormStyle = BeepThemesManager.CurrentStyle;
            ApplyTheme();
        }
        public MainFrm(IServiceProvider services) : base(services)
        {

            InitializeComponent();
            _serviceprovider = services;

            appManager.Container = beepDisplayContainer1;
            appManager.Container.ContainerType = ContainerTypeEnum.TabbedPanel;

            beepAppTree1.init(beepService,appManager);
            beepAppTree1.CreateRootTree();
            beepAppTree1.NodeSelected += BeepAppTree1_NodeSelected;
            FormStyle = BeepThemesManager.CurrentStyle;

            beepMenuAppBar1.beepServices = beepService;
            beepMenuAppBar1.CreateMenuItems();
            beepMenuAppBar1.SelectedItemChanged += BeepMenuAppBar1_SelectedItemChanged;

            InitializeWorkspaceControls();

            if (beepService?.DMEEditor != null)
            {
                NuggetsStartupBootstrapper.TryRestore(beepService.DMEEditor);
            }

        }

        private void MainFrm_Load(object sender, EventArgs e)
        {
            _ = TryShowBootstrapAsync();
        }

        private async Task TryShowBootstrapAsync()
        {
            if (beepService?.DMEEditor == null) return;
            if (IsDisposed || Disposing) return;

            _bootstrapCts ??= new System.Threading.CancellationTokenSource();
            _bootstrapState ??= BootstrapState.Resolve(_serviceprovider, beepService.DMEEditor);

            try
            {
                await _bootstrapState.InitializeAsync();
                if (IsDisposed || Disposing) return;

                if (!_bootstrapState.IsFirstRun)
                {
                    return;
                }

                if (InvokeRequired)
                {
                    BeginInvoke(ShowBootstrapControl);
                }
                else
                {
                    ShowBootstrapControl();
                }
            }
            catch (Exception ex)
            {
                beepService.DMEEditor.AddLogMessage("MainFrm",
                    $"Bootstrap init failed: {ex.Message}",
                    DateTime.Now, -1, null, TheTechIdea.Beep.ConfigUtil.Errors.Failed);
            }
        }

        private void ShowBootstrapControl()
        {
            if (IsDisposed || Disposing) return;
            if (_appBootstrap != null) return;

            _appBootstrap = new uc_AppBootstrap(_serviceprovider);
            _appBootstrap.BootstrapCompleted -= AppBootstrap_BootstrapCompleted;
            _appBootstrap.BootstrapCompleted += AppBootstrap_BootstrapCompleted;
            _appBootstrap.Dock = DockStyle.Fill;
            Controls.Add(_appBootstrap);
            _appBootstrap.BringToFront();
        }

        private async void AppBootstrap_BootstrapCompleted(object? sender, BootstrapCompletedEventArgs e)
        {
            if (_appBootstrap != null)
            {
                if (InvokeRequired) BeginInvoke(RemoveBootstrapControl);
                else RemoveBootstrapControl();
            }
        }

        private void RemoveBootstrapControl()
        {
            if (_appBootstrap == null) return;
            _appBootstrap.BootstrapCompleted -= AppBootstrap_BootstrapCompleted;
            Controls.Remove(_appBootstrap);
            _appBootstrap.Dispose();
            _appBootstrap = null;

            InitializeWorkspaceControls();
        }

        private void InitializeWorkspaceControls()
        {
            if (beepDisplayContainer1 == null)
                return;

            _setupWizard ??= new uc_SetupWizard(_serviceprovider);
            _setupWizard.SetupCompleted -= SetupWizard_SetupCompleted;
            _setupWizard.SetupCompleted += SetupWizard_SetupCompleted;

            if (!beepDisplayContainer1.IsControlExit(_setupWizard))
            {
                beepDisplayContainer1.AddControl(_setupWizard.BranchText, _setupWizard, ContainerTypeEnum.TabbedPanel);
            }

            beepDisplayContainer1.ShowControl(_setupWizard.BranchText, _setupWizard);
        }

        private void SetupWizard_SetupCompleted(object? sender, uc_SetupWizard.SetupCompletedEventArgs e)
        {
            if (beepDisplayContainer1 == null)
                return;

            _importExportWizard ??= new uc_ImportExportWizardLauncher(_serviceprovider);
            _importExportWizard.ApplySetupSnapshot(e.Snapshot);

            if (!beepDisplayContainer1.IsControlExit(_importExportWizard))
            {
                beepDisplayContainer1.AddControl(_importExportWizard.BranchText, _importExportWizard, ContainerTypeEnum.TabbedPanel);
            }

            beepDisplayContainer1.ShowControl(_importExportWizard.BranchText, _importExportWizard);
        }

        private void BeepAppTree1_NodeSelected(object? sender, BeepMouseEventArgs e)
        {
            var item = e?.Data as SimpleItem;
            if (item == null)
                return;

            var branchText = item.Text ?? item.BranchName ?? string.Empty;
            if (branchText.Contains("Setup", StringComparison.OrdinalIgnoreCase))
            {
                ShowHostedControl(_setupWizard ??= new uc_SetupWizard(_serviceprovider), _setupWizard.BranchText);
                return;
            }

            if (branchText.Contains("Import", StringComparison.OrdinalIgnoreCase) ||
                branchText.Contains("Export", StringComparison.OrdinalIgnoreCase))
            {
                ShowHostedControl(_importExportWizard ??= new uc_ImportExportWizardLauncher(_serviceprovider), _importExportWizard.BranchText);
            }
        }

        private void ShowHostedControl(Control control, string title)
        {
            if (beepDisplayContainer1 == null || control == null)
                return;

            if (!beepDisplayContainer1.IsControlExit(control as IDM_Addin))
            {
                beepDisplayContainer1.AddControl(title, control as IDM_Addin, ContainerTypeEnum.TabbedPanel);
            }

            beepDisplayContainer1.ShowControl(title, control as IDM_Addin);
        }

        private void beepMenuAppBar1_Click(object sender, EventArgs e)
        {

        }

        private void BeepMenuAppBar1_SelectedItemChanged(object? sender, SelectedItemChangedEventArgs e)
        {
            var selectedItem = e?.SelectedItem;
            if (selectedItem == null)
                return;

            var itemText = selectedItem.Text ?? selectedItem.BranchName ?? selectedItem.MenuName ?? string.Empty;
            if (itemText.Contains("Setup", StringComparison.OrdinalIgnoreCase))
            {
                ShowHostedControl(_setupWizard ??= new uc_SetupWizard(_serviceprovider), _setupWizard.BranchText);
                return;
            }

            if (itemText.Contains("Import", StringComparison.OrdinalIgnoreCase) ||
                itemText.Contains("Export", StringComparison.OrdinalIgnoreCase))
            {
                ShowHostedControl(_importExportWizard ??= new uc_ImportExportWizardLauncher(_serviceprovider), _importExportWizard.BranchText);
            }
        }
    }
}
