using System;
using System.Windows.Forms;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Default.Views.Setup;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views
{
    public partial class MainFrm_Tree : TemplateForm
    {
        private SetupWizardLauncher? _launcher;
        IServiceProvider _serviceProvider;

        public MainFrm_Tree() { InitializeComponent(); }

        public MainFrm_Tree(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            _serviceProvider = services;
            _launcher = new SetupWizardLauncher(_serviceProvider, beepService?.DMEEditor, this);

            appManager.Container = beepDisplayContainer1;
            appManager.Container.ContainerType = ContainerTypeEnum.TabbedPanel;

            beepAppTree1.init(beepService, appManager);
            beepAppTree1.CreateRootTree();
            beepAppTree1.NodeSelected += BeepAppTree1_NodeSelected;
        }

        private void MainFrm_Tree_Load(object sender, EventArgs e) { }

        private void BeepAppTree1_NodeSelected(object? sender, BeepMouseEventArgs e)
        {
            if (e?.Data is SimpleItem item)
                RouteAction(item.ActionID ?? item.MenuID ?? "");
        }

        private void RouteAction(string actionId)
        {
            if (string.IsNullOrEmpty(actionId)) return;
            var id = actionId.ToLowerInvariant();
            if (id.Contains("setup")) _launcher?.ShowSetupWizard();
        }
    }
}
