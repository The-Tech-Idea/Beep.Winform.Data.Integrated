using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Logger;
using TheTechIdea.Beep.MVVM.ViewModels.BeepConfig;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls.GridX;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Default.Views.Template;



namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    [AddinAttribute(Caption = "Connection Drivers", Name = "uc_ConnectionDrivers", misc = "Config", menu = "Configuration", addinType = AddinType.Control, displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 3, RootNodeName = "Configuration", Order = 3, ID = 3, BranchText = "Connection Drivers", BranchType = EnumPointType.Function, IconImageName = "drivers.svg", BranchClass = "ADDIN", BranchDescription = "Data Sources Connection Drivers Setup Screen")]

    public partial class uc_ConnnectionDrivers : TemplateUserControl, IAddinVisSchema
    {
        public uc_ConnnectionDrivers(IServiceProvider services): base(services)
        {
            InitializeComponent();
  
            Details.AddinName = "Connection Drivers";
        }
        #region "IAddinVisSchema"
        public string RootNodeName { get; set; } = "Configuration";
        public string CatgoryName { get; set; }
        public int Order { get; set; } = 3;
        public int ID { get; set; } = 3;
        public string BranchText { get; set; } = "Connection Drivers";
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        public int BranchID { get; set; } = 3;
        public string IconImageName { get; set; } = "drivers.svg";
        public string BranchStatus { get; set; }
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; } = "Data Sources Connection Drivers Setup Screen";
        public string BranchClass { get; set; } = "ADDIN";
        public string AddinName { get ; set ; }
        #endregion "IAddinVisSchema"

        DriversConfigViewModel viewModel;
     
      
        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);
            viewModel = new DriversConfigViewModel(beepService.DMEEditor, appManager);
            beepGridPro1.ReadOnly=true;
            beepGridPro1.SaveCalled += BeepGridPro1_SaveCalled;
        }

        private void BeepGridPro1_SaveCalled(object? sender, EventArgs e)
        {
            viewModel.Save();
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            base.OnNavigatedTo(parameters);
            BeepColumnConfig classhandlers = beepGridPro1.GetColumnByName("ClassHandler");
            classhandlers.CellEditor = BeepColumnType.ListOfValue;
            int idx = 0;
            foreach (var item in viewModel.DBAssemblyClasses)
            {
                SimpleItem item1 = new SimpleItem();
                item1.DisplayField = item.className;
                item1.Value = idx++;
                item1.Text = item.className;
                item1.Name = item.className;
                classhandlers.Items.Add(item1);
            }
            beepGridPro1.DataSource = viewModel.DBWork.Units;


        }
       
    }
}
