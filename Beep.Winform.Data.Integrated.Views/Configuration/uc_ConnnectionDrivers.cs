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
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Default.Views.Template;
using System.ComponentModel;



namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    [AddinAttribute(Caption = "Connection Drivers", Name = "uc_ConnectionDrivers", misc = "Config", menu = "Configuration", addinType = AddinType.Control, displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 3, RootNodeName = "Configuration", Order = 3, ID = 3, BranchText = "Connection Drivers", BranchType = EnumPointType.Function, IconImageName = "drivers.svg", BranchClass = "ADDIN", BranchDescription = "Data Sources Connection Drivers Setup Screen")]

    public partial class uc_ConnnectionDrivers : TemplateUserControl, IAddinVisSchema
    {
        /// <summary>
        /// Designer/parameterless ctor. Must not chain to the IServiceProvider overload with null —
        /// that resolves services off a null provider and throws.
        /// </summary>
        public uc_ConnnectionDrivers() => InitializeControl();

        public uc_ConnnectionDrivers(IServiceProvider services) : base(services) => InitializeControl();

        private void InitializeControl()
        {
            InitializeComponent();

            Details.AddinName = "Connection Drivers";
        }

        /// <summary>
        /// Skill § "Sizing tokens": apply DPI-scaled <see cref="BeepLayoutMetrics"/> values to
        /// chrome that the Designer serialized as static pixels. The Designer is the source of
        /// truth for layout; this method overlays DPI-scaled dimensions on top so the surface
        /// tracks the host display scale.
        /// </summary>
        protected override void ApplyDpiScaledLayout()
        {
            // Usercontrol chrome: design-time size is in Designer; overlay DPI-scaled dialog size.
            Size = BeepLayoutMetrics.DialogLarge.ScaleSize(this);
        }
        #region "IAddinVisSchema"
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string RootNodeName { get; set; } = "Configuration";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CatgoryName { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Order { get; set; } = 3;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ID { get; set; } = 3;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchText { get; set; } = "Connection Drivers";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Level { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BranchID { get; set; } = 3;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconImageName { get; set; } = "drivers.svg";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchStatus { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ParentBranchID { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchDescription { get; set; } = "Data Sources Connection Drivers Setup Screen";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchClass { get; set; } = "ADDIN";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

            // Ensure driver catalog is loaded (matches WPF LoadDrivers pattern)
            beepService.Config_editor.LoadConnectionDriversConfigValues();
            if (beepService.Config_editor.DataDriversClasses == null
                || !beepService.Config_editor.DataDriversClasses.Any())
            {
                beepService.Config_editor.DataDriversClasses =
                    Beep.Helpers.ConnectionHelper.GetAllConnectionConfigs();
            }

            // Populate ClassHandler column with drivers filtered by DataSourceType (matches WPF)
            BeepColumnConfig classhandlers = beepGridPro1.GetColumnByName("ClassHandler");
            classhandlers.CellEditor = BeepColumnType.ListOfValue;
            int idx = 0;
            var driverCatalog = beepService.Config_editor.DataDriversClasses
                ?.Where(d => !string.IsNullOrWhiteSpace(d.classHandler))
                .GroupBy(d => d.classHandler ?? "", StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(d => d.classHandler, StringComparer.OrdinalIgnoreCase)
                ?? Enumerable.Empty<TheTechIdea.Beep.DriversConfigurations.ConnectionDriversConfig>();

            foreach (var item in driverCatalog)
            {
                SimpleItem item1 = new SimpleItem();
                item1.DisplayField = $"{item.classHandler} ({item.PackageName})";
                item1.Value = idx++;
                item1.Text = item.classHandler;
                item1.Name = item.classHandler;
                classhandlers.Items.Add(item1);
            }
            beepGridPro1.DataSource = viewModel.DBWork.Units;
        }
       
    }
}
