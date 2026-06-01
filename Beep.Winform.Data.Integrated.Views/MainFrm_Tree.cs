using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport;
using TheTechIdea.Beep.Winform.Default.Views.Setup;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views
{
    public partial class MainFrm_Tree : TemplateForm
    {
        private uc_SetupWizard? _setupWizard;
        private uc_ImportExportWizardLauncher? _importExportWizard;
        IServiceProvider _serviceProvider;

        public MainFrm_Tree()
        {
            InitializeComponent();

        }
        public MainFrm_Tree(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            _serviceProvider = services;
            appManager.Container = beepDisplayContainer1;
            appManager.Container.ContainerType = ContainerTypeEnum.TabbedPanel;

            beepAppTree1.init(beepService,appManager);
            beepAppTree1.CreateRootTree();
            beepAppTree1.NodeSelected += BeepAppTree1_NodeSelected;

            InitializeWorkspaceControls();


            //beepMenuAppBar1.beepServices = beepService;
            //beepMenuAppBar1.CreateMenuItems();


        }

        private void MainFrm_Tree_Load(object sender, EventArgs e)
        {

        }

        private void InitializeWorkspaceControls()
        {
            if (beepDisplayContainer1 == null)
                return;

            _setupWizard ??= new uc_SetupWizard(_serviceProvider);
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

            _importExportWizard ??= new uc_ImportExportWizardLauncher(_serviceProvider);
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
                ShowHostedControl(_setupWizard ??= new uc_SetupWizard(_serviceProvider), _setupWizard.BranchText);
                return;
            }

            if (branchText.Contains("Import", StringComparison.OrdinalIgnoreCase) ||
                branchText.Contains("Export", StringComparison.OrdinalIgnoreCase))
            {
                ShowHostedControl(_importExportWizard ??= new uc_ImportExportWizardLauncher(_serviceProvider), _importExportWizard.BranchText);
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
    }
}
