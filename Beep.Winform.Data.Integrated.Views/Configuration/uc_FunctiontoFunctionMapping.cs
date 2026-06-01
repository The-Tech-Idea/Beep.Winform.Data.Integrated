
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.MVVM.ViewModels;
using TheTechIdea.Beep.Services;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;

using TheTechIdea.Beep.Winform.Default.Views.Template;


namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    [AddinAttribute(Caption = "Function to Function Mapping", Name = "uc_FunctiontoFunctionMapping", misc = "Config", menu = "Configuration", addinType = AddinType.Control, displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 1, RootNodeName = "Configuration", Order = 1, ID = 1, BranchText = "Function to Function Mapping", BranchType = EnumPointType.Function, IconImageName = "functiontofunctionmapping.svg", BranchClass = "ADDIN", BranchDescription = "Function to Function Mapping Setup Screen")]

    public partial class uc_FunctiontoFunctionMapping : TemplateUserControl, IAddinVisSchema
    {
        public uc_FunctiontoFunctionMapping(IServiceProvider services): base(services)
        {
            InitializeComponent();
          
            Details.AddinName = "Function to Function Mapping";
        }
       
        #region "IAddinVisSchema"
        public string RootNodeName { get; set; } = "Configuration";
        public string CatgoryName { get; set; }
        public int Order { get; set; } = 1;
        public int ID { get; set; } = 1;
        public string BranchText { get; set; } = "Function to Function Mapping";
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        public int BranchID { get; set; } = 1;
        public string IconImageName { get; set; } = "functiontofunctionmapping.svg";
        public string BranchStatus { get; set; }
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; } = "Function to Function Mapping Setup Screen";
        public string BranchClass { get; set; } = "ADDIN";
        public string AddinName { get; set; }
        #endregion "IAddinVisSchema"
        FunctionToFunctionMappingViewModel viewModel;
        private IBeepService beepservice;
        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);
            //viewModel = new FunctionToFunctionMappingViewModel(beepservice.DMEEditor, appManager);
           
        }
        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            base.OnNavigatedTo(parameters);
       //     viewModel.LoadData();
       //     beepGridPro1.DataSource = viewModel.DBWork.Units;


        }
    }
}
