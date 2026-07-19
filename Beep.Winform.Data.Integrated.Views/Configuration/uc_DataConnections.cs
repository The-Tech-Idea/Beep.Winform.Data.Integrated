using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Default.Views.Template;
using TheTechIdea.Beep.MVVM.ViewModels.BeepConfig;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Vis.Modules;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    [AddinAttribute(Caption = "Data Connections", Name = "uc_DataConnections", misc = "Config", menu = "Configuration", addinType = AddinType.Control, displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 1, RootNodeName = "Configuration", Order = 1, ID = 1, BranchText = "Data Connections", BranchType = EnumPointType.Function, IconImageName = "rdbmsconnections.svg", BranchClass = "ADDIN", BranchDescription = "Data Connections Setup Screen")]
    public partial class uc_DataConnections : TemplateUserControl, IAddinVisSchema
    {
        /// <summary>
        /// Designer/parameterless ctor. Must not chain to the IServiceProvider overload with null —
        /// that resolves services off a null provider and throws.
        /// </summary>
        public uc_DataConnections() => InitializeControl();

        public uc_DataConnections(IServiceProvider services) : base(services) => InitializeControl();

        private void InitializeControl()
        {
            InitializeComponent();
            Details.AddinName = "Data Connections";
        }

        protected override void ApplyDpiScaledLayout()
        {
            Size = BeepLayoutMetrics.DialogLarge.ScaleSize(this);
        }

        #region "IAddinVisSchema"
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string RootNodeName { get; set; } = "Configuration";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CatgoryName { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Order { get; set; } = 1;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ID { get; set; } = 1;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchText { get; set; } = "Data Connections";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Level { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BranchID { get; set; } = 1;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconImageName { get; set; } = "dataconnections.svg";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchStatus { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ParentBranchID { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchDescription { get; set; } = "Data Connections Setup Screen";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchClass { get; set; } = "ADDIN";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string AddinName { get; set; }
        #endregion "IAddinVisSchema"

        DataConnectionViewModel viewModel;

        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);
            viewModel = new DataConnectionViewModel(beepService.DMEEditor, appManager);

            // Load driver catalog for ClassHandler column (matches WPF LoadClassHandlers pattern)
            beepService.Config_editor.LoadConnectionDriversConfigValues();
            if (beepService.Config_editor.DataDriversClasses == null || !beepService.Config_editor.DataDriversClasses.Any())
                beepService.Config_editor.DataDriversClasses = Beep.Helpers.ConnectionHelper.GetAllConnectionConfigs();

            BeepColumnConfig drivername = beepSimpleGrid1.GetColumnByName("DriverName");
            beepSimpleGrid1.CellValueChanged -= BeepGridPro1_CellValueChanged;
            beepSimpleGrid1.CellValueChanged += BeepGridPro1_CellValueChanged;

            // Populate driver packages with versions (cascading: Package → Version)
            List<SimpleItem> versions = new List<SimpleItem>();
            foreach (var item in viewModel.PackageNames)
            {
                SimpleItem driveritem = new SimpleItem();
                driveritem.DisplayField = item;
                driveritem.Text = item;
                driveritem.Name = item;
                driveritem.Value = item;
                foreach (var DriversClasse in beepService.Config_editor.DataDriversClasses.Where(x => x.PackageName == item))
                {
                    SimpleItem itemversion = new SimpleItem();
                    itemversion.DisplayField = DriversClasse.version;
                    itemversion.Value = DriversClasse.version;
                    itemversion.Text = DriversClasse.version;
                    itemversion.Name = DriversClasse.version;
                    itemversion.ParentItem = driveritem;
                    itemversion.ParentValue = item;
                    versions.Add(itemversion);
                }
                drivername.Items.Add(driveritem);
            }

            BeepColumnConfig driverversion = beepSimpleGrid1.GetColumnByName("DriverVersion");
            driverversion.ParentColumnName = "DriverName";
            driverversion.Items = versions;
            beepSimpleGrid1.SaveCalled -= BeepGridPro1_SaveCalled;
            beepSimpleGrid1.SaveCalled += BeepGridPro1_SaveCalled;
            beepSimpleGrid1.ShowCheckBox = true;
        }

        private void BeepGridPro1_SaveCalled(object? sender, EventArgs e)
        {
            // Ensure GuidID for new connections (matches WPF SaveToResult pattern)
            foreach (var unit in viewModel.DBWork.Units)
            {
                var conn = unit as ConnectionProperties;
                if (conn != null && string.IsNullOrEmpty(conn.GuidID))
                    conn.GuidID = Guid.NewGuid().ToString();
            }
            viewModel.Save();
        }

        private void BeepGridPro1_CellValueChanged(object? sender, BeepCellEventArgs e)
        {
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            base.OnNavigatedTo(parameters);
            beepSimpleGrid1.DataSource = viewModel.DBWork.Units;
        }
    }
}
