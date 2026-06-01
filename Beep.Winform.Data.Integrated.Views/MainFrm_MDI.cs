using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Winform.Controls.DisplayContainers;
using TheTechIdea.Beep.Winform.Controls.Integrated.NuggetsManage;
using TheTechIdea.Beep.Winform.Controls.ITrees.BeepTreeView;
using TheTechIdea.Beep.Winform.Controls.MenuBar;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport;
using TheTechIdea.Beep.Winform.Default.Views.Setup;

namespace TheTechIdea.Beep.Winform.Default.Views
{
    [AddinAttribute(Caption = "Home", Name = "MainFrm_MDI", misc = "MainFrm_MDI", menu = "Main", addinType = AddinType.Page, displayType = DisplayType.Popup, ObjectType = "Beep")]

    public partial class MainFrm_MDI : TemplateForm
    {
        private uc_SetupWizard? _setupWizard;
        private uc_ImportExportWizardLauncher? _importExportWizard;


        IServiceProvider _serviceprovider;
        public IDMEEditor Editor { get; }
        public MainFrm_MDI()
        {
            InitializeComponent();
            Theme = BeepThemesManager.CurrentThemeName;
            FormStyle = BeepThemesManager.CurrentStyle;
            ApplyTheme();
        }
        public MainFrm_MDI(IServiceProvider services) : base(services)
        {

            InitializeComponent();
            _serviceprovider = services;

           // appManager.Container = beepDisplayContainer1;
            appManager.Container.ContainerType = ContainerTypeEnum.TabbedPanel;

            //beepAppTree1.init(beepService, appManager);
            //beepAppTree1.CreateRootTree();
            //beepAppTree1.NodeSelected += BeepAppTree1_NodeSelected;
            //FormStyle = BeepThemesManager.CurrentStyle;

            //beepMenuAppBar1.beepServices = beepService;
            //beepMenuAppBar1.CreateMenuItems();
            //beepMenuAppBar1.SelectedItemChanged += BeepMenuAppBar1_SelectedItemChanged;

            //InitializeWorkspaceControls();

            if (beepService?.DMEEditor != null)
            {
                NuggetsStartupBootstrapper.TryRestore(beepService.DMEEditor);
            }

        }
    }
}
