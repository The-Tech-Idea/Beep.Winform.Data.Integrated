
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Winform.Default.Views.Template;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Winform.Controls;


namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    [AddinAttribute(Caption = "Filter", Name = "uc_FilterForm", misc = "Config", menu = "Configuration", addinType = AddinType.Control, displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 2, RootNodeName = "Configuration", Order = 2, ID = 2, BranchText = "Filter", BranchType = EnumPointType.Function, IconImageName = "driversconfig.png", BranchClass = "ADDIN", BranchDescription = "Data Sources Connection Drivers Setup Screen")]

    public partial class uc_FilterForm: TemplateUserControl, IAddinVisSchema
    {
        public uc_FilterForm(IServiceProvider services): base(services)
        {
            InitializeComponent();
         
          

            Details.AddinName = "Filter";
        }
        #region "IAddinVisSchema"
        public string RootNodeName { get; set; } = "Configuration";
        public string CatgoryName { get; set; }
        public int Order { get; set; } = 2;
        public int ID { get; set; } = 2;
        public string BranchText { get; set; } = "Filter ";
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.Entity;
        public int BranchID { get; set; } = 3;
        public string IconImageName { get; set; } = "connectiondrivers.ico";
        public string BranchStatus { get; set; }
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; } = "Data Sources Connection Drivers Setup Screen";
        public string BranchClass { get; set; } = "ADDIN";
        public string AddinName { get; set; }
        #endregion "IAddinVisSchema"

       
        
        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);
      
        }
        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
           
            base.OnNavigatedTo(parameters);
            beepSimpleGrid1.DataSource =  beepService.Config_editor.DataSourcesClasses;
            //beepFilter1.DataSource = beepservice.Editor.ConfigEditor.DataSourcesClasses;

        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
        }
    }
}
