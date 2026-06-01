using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
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
        public uc_DataConnections(IServiceProvider services) : base(services)
        {
            InitializeComponent();

            Details.AddinName = "Data Connections";
        }

        #region "IAddinVisSchema"
        public string RootNodeName { get; set; } = "Configuration";
        public string CatgoryName { get; set; }
        public int Order { get; set; } = 1;
        public int ID { get; set; } = 1;
        public string BranchText { get; set; } = "Data Connections";
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        public int BranchID { get; set; } = 1;
        public string IconImageName { get; set; } = "dataconnections.svg";
        public string BranchStatus { get; set; }
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; } = "Data Connections Setup Screen";
        public string BranchClass { get; set; } = "ADDIN";
        public string AddinName { get; set; }
        #endregion "IAddinVisSchema"
        DataConnectionViewModel viewModel;


        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);
            viewModel = new DataConnectionViewModel(beepService.DMEEditor, appManager);
            //viewModel.DBWork.Units.Filter = "Category = " + DatasourceCategory.RDBMS;
            BeepColumnConfig drivername = beepSimpleGrid1.GetColumnByName("DriverName");
            beepSimpleGrid1.CellValueChanged += BeepGridPro1_CellValueChanged;
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
            beepSimpleGrid1.SaveCalled += BeepGridPro1_SaveCalled;
            beepSimpleGrid1.ShowCheckBox = true;
            // idx = 0;
            //foreach (var item in viewModel.PackageVersions)
            //{
            //    SimpleItem driveritem = new SimpleItem();
            //    driveritem.IsDisplayField = item;
            //    driveritem.Value = idx++;
            //    driveritem.Text = item;
            //    driveritem.Name = item;
            //    driverversion.Items.Add(driveritem);
            //}
        }

        private void BeepGridPro1_SaveCalled(object? sender, EventArgs e)
        {
            viewModel.Save();
        }

        private void BeepGridPro1_CellValueChanged(object? sender, BeepCellEventArgs e)
        {
            //BeepColumnConfig beepColumnConfig = beepGridPro1.GetColumnByName("DriverName");
            //BeepColumnConfig currentcolumn = beepGridPro1.GetColumnByIndex(e.Cell.ColumnIndex);
            //if (currentcolumn.ColumnName == "DriverName")
            //{
            //    BeepColumnConfig driverversion = beepGridPro1.GetColumnByName("DriverVersion");
            //    driverversion.Items.Clear();
            //    e.Cell.FilterdList = new List<SimpleItem>();
            //    foreach (var DriversClasse in beepservice.Config_editor.DataDriversClasses.Where(x => x.PackageName == e.Cell.CellValue.ToString()))
            //    {
            //        SimpleItem itemversion = new SimpleItem();
            //        itemversion.IsDisplayField = DriversClasse.version;
            //        itemversion.Value = DriversClasse.version;
            //        itemversion.Text = DriversClasse.version;
            //        itemversion.Name = DriversClasse.version;

            //        driverversion.FilterdList.Add(itemversion);


            //    }
            //}
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            base.OnNavigatedTo(parameters);
            beepSimpleGrid1.DataSource = viewModel.DBWork.Units;


        }

        private void uc_DataConnections_Load(object sender, EventArgs e)
        {

        }
    }
}
