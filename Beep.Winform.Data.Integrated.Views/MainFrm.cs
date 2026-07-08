
using System;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Integrated.NuggetsManage;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Default.Views.Setup;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views
{
    [AddinAttribute(Caption = "Home", Name = "MainForm", misc = "Main", menu = "Main", addinType = AddinType.Page, displayType = DisplayType.Popup, ObjectType = "Beep")]

    public partial class MainFrm : TemplateForm
    {
        private Setup.SetupWizardLauncher? _launcher;

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
            beepAppTree1.CollapseAll();
            beepAppTree1.NodeSelected += BeepAppTree1_NodeSelected;
            FormStyle = BeepThemesManager.CurrentStyle;

            beepMenuAppBar1.beepServices = beepService;
            beepMenuAppBar1.CreateMenuItems();
            beepMenuAppBar1.SelectedItemChanged += BeepMenuAppBar1_SelectedItemChanged;

            if (beepService?.DMEEditor != null)
            {
                NuggetsStartupBootstrapper.TryRestore(beepService.DMEEditor);
            }

        }

        private async void MainFrm_Load(object sender, EventArgs e)
        {
            _launcher = new Setup.SetupWizardLauncher(_serviceprovider, beepService?.DMEEditor, this);
            await _launcher.TryShowFirstRunAsync();
        }

        private void BeepAppTree1_NodeSelected(object? sender, BeepMouseEventArgs e)
        {
            if (e?.Data is SimpleItem item)
                RouteAction(item.ActionID ?? item.MenuID ?? "");
        }

        private void BeepMenuAppBar1_SelectedItemChanged(object? sender, SelectedItemChangedEventArgs e)
        {
            if (e?.SelectedItem is SimpleItem item)
                RouteAction(item.ActionID ?? item.MenuID ?? "");
        }

        // Keep for Designer.cs reference
        private void beepMenuAppBar1_Click(object sender, EventArgs e) { }

        private void RouteAction(string actionId)
        {
            if (string.IsNullOrEmpty(actionId)) return;
            var id = actionId.ToLowerInvariant();
            if (id.Contains("setup")) _launcher?.ShowSetupWizard();
            else if (id.Contains("import") || id.Contains("export")) _launcher?.ShowImportExport();
        }
    }
}
