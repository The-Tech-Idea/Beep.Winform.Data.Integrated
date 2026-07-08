using System;
using System.Windows.Forms;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Winform.Controls.DisplayContainers;
using TheTechIdea.Beep.Winform.Controls.Integrated.NuggetsManage;
using TheTechIdea.Beep.Winform.Default.Views.Setup;

namespace TheTechIdea.Beep.Winform.Default.Views
{
    [AddinAttribute(Caption = "Home", Name = "MainFrm_MDI", misc = "MainFrm_MDI", menu = "Main", addinType = AddinType.Page, displayType = DisplayType.Popup, ObjectType = "Beep")]

    public partial class MainFrm_MDI : TemplateForm
    {
        private SetupWizardLauncher? _launcher;

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
            _launcher = new SetupWizardLauncher(_serviceprovider, beepService?.DMEEditor, this);
            appManager.Container.ContainerType = ContainerTypeEnum.TabbedPanel;

            if (beepService?.DMEEditor != null)
                NuggetsStartupBootstrapper.TryRestore(beepService.DMEEditor);
        }
    }
}
