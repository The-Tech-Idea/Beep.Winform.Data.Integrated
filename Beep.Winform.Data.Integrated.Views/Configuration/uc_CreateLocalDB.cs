using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;

using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Default.Views.Template;
using TheTechIdea.Beep.Container.Services;

using TheTechIdea.Beep.MVVM.ViewModels.BeepConfig;
using TheTechIdea.Beep.Desktop.Common.Util;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Logger;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    [AddinAttribute(Caption = "Create Local DB",ScopeCreateType = AddinScopeCreateType.Multiple , Name = "uc_CreateLocalDB", misc = "Config", menu = "Configuration", addinType = AddinType.Control, displayType = DisplayType.Popup, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 1, RootNodeName = "Configuration", Order = 1, ID = 1, BranchText = "Create Local DB", BranchType = EnumPointType.Function, IconImageName = "localconnections.svg", BranchClass = "ADDIN", BranchDescription = "Create Local DB Screen")]

    public partial class uc_CreateLocalDB : TemplateUserControl, IAddinVisSchema
    {
        public uc_CreateLocalDB(IServiceProvider services) : base(services)
        {
            InitializeComponent();

            Details.AddinName = "Create Local DB";

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
        public string BranchText { get; set; } = "Create Local DB";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Level { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BranchID { get; set; } = 1;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconImageName { get; set; } = "localconnections.svg";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchStatus { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ParentBranchID { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchDescription { get; set; } = "Create Local DB";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchClass { get; set; } = "ADDIN";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string AddinName { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ITree TreeObject { get; set; }
        #endregion "IAddinVisSchema"

     
        List<SimpleItem> Drivers = new List<SimpleItem>();
        List<SimpleItem> InstallationFolders = new List<SimpleItem>();
        private DataConnectionViewModel viewModel;
        public void SetConfig(IDMEEditor pDMEEditor, IDMLogger plogger, IUtil putil, string[] args, IPassedArgs e, IErrorsInfo per)
        {
            
        }
        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);
            if (appManager.Tree != null)
            {
                TreeObject = (ITree)appManager.Tree;
            }
            viewModel = new DataConnectionViewModel(beepService.DMEEditor, appManager);


            foreach (var item in beepService.Config_editor.DataConnections)
            {
                SimpleItem conn = new SimpleItem();
                conn.DisplayField = item.ConnectionName;
                conn.Text = item.ConnectionName;
                conn.Name = item.ConnectionName;
                conn.Value = item.ConnectionName;
                conn.GuidId = item.GuidID;
                conn.ParentItem = null;
                conn.ContainerGuidID = item.GuidID;
               // DatasourcebeepComboBox.ListItems.Add(conn);
            }
            List<SimpleItem> versions = new List<SimpleItem>();
            foreach (var item in viewModel.EmbeddedDatabaseTypes)
            {
                SimpleItem driveritem = new SimpleItem();
               // driveritem.IsDisplayField =item.classHandler +" - " +item.DriverClass +" - " + item.version;
                driveritem.Text = item.classHandler + " - " + item.DriverClass + " - " + item.version;
                driveritem.Name = item.PackageName;
                driveritem.Value = item;
                driveritem.Item = item;
                foreach (var DriversClasse in viewModel.EmbeddedDatabaseTypes)
                {
                    SimpleItem itemversion = new SimpleItem();
                    itemversion.DisplayField = DriversClasse.version;
                    itemversion.Value = DriversClasse.version;
                    itemversion.Text = DriversClasse.version;
                    itemversion.Name = DriversClasse.version;
                    itemversion.ParentItem = driveritem;
                    itemversion.ParentValue = item.PackageName;
                    versions.Add(itemversion);
                }
                Drivers.Add(driveritem);
            }
            // Get installation folders from config foders
            foreach (var item in beepService.Config_editor.Config.Folders.Where(x => x.FolderFilesType == FolderFileTypes.DataFiles || x.FolderFilesType == FolderFileTypes.ProjectData).ToList())
            {
                string foldername=Path.GetFileName(item.FolderPath);
                SimpleItem folderitem = new SimpleItem();
                folderitem.DisplayField = foldername;
                folderitem.Text = foldername;
                folderitem.Name = item.FolderPath;
                folderitem.Value = item.FolderPath;
                folderitem.Item = item;
                InstallationFolders.Add(folderitem);
            }
            // Get System folders and documents folders (from .net environment)and others  add them to the list installation folders
            string programfilesfolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string commonapplicationdatafolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string commonprogramfilesfolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);
            string localapplicationdatafolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appdatafolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string desktopfolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string personalfolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string documentsfolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string downloadsfolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // now add them to the list
            SimpleItem programfiles = new SimpleItem();
            programfiles.DisplayField = "Program Files";
            programfiles.Text = "Program Files";
            programfiles.Name = programfilesfolder;
            programfiles.Value = programfilesfolder;
            InstallationFolders.Add(programfiles);
            SimpleItem commonapplicationdata = new SimpleItem();
            commonapplicationdata.DisplayField = "Common Application Data";
            commonapplicationdata.Text = "Common Application Data";
            commonapplicationdata.Name = commonapplicationdatafolder;
            commonapplicationdata.Value = commonapplicationdatafolder;
            InstallationFolders.Add(commonapplicationdata);
            SimpleItem commonprogramfiles = new SimpleItem();
            commonprogramfiles.DisplayField = "Common Program Files";
            commonprogramfiles.Text = "Common Program Files";
            commonprogramfiles.Name = commonprogramfilesfolder;
            commonprogramfiles.Value = commonprogramfilesfolder;
            InstallationFolders.Add(commonprogramfiles);
            SimpleItem localapplicationdata = new SimpleItem();
            localapplicationdata.DisplayField = "Local Application Data";
            localapplicationdata.Text = "Local Application Data";
            localapplicationdata.Name = localapplicationdatafolder;
            localapplicationdata.Value = localapplicationdatafolder;
            InstallationFolders.Add(localapplicationdata);
            SimpleItem appdata = new SimpleItem();
            appdata.DisplayField = "Application Data";
            appdata.Text = "Application Data";
            appdata.Name = appdatafolder;
            appdata.Value = appdatafolder;
            InstallationFolders.Add(appdata);
            SimpleItem desktop = new SimpleItem();
            desktop.DisplayField = "Desktop";
            desktop.Text = "Desktop";
            desktop.Name = desktopfolder;
            desktop.Value = desktopfolder;
            InstallationFolders.Add(desktop);
            SimpleItem personal = new SimpleItem();
            personal.DisplayField = "Personal";
            personal.Text = "Personal";
            personal.Name = personalfolder;
            personal.Value = personalfolder;
            InstallationFolders.Add(personal);
            SimpleItem documents = new SimpleItem();
            documents.DisplayField = "Documents";
            documents.Text = "Documents";
            documents.Name = documentsfolder;
            documents.Value = documentsfolder;
            InstallationFolders.Add(documents);
            SimpleItem downloads = new SimpleItem();
            downloads.DisplayField = "Downloads";
            downloads.Text = "Downloads";
            downloads.Name = downloadsfolder;
            downloads.Value = downloadsfolder;
            InstallationFolders.Add(downloads);
            // Add the drivers to the combo box

            this.databaseTextBox.DataBindings.Add("Text", viewModel, "DatabaseName", true, DataSourceUpdateMode.OnPropertyChanged);
            this.PasswordbeepTextBox.DataBindings.Add("Text", viewModel, "Password", true, DataSourceUpdateMode.OnPropertyChanged);
         //   this.InstallFoldercomboBox.DataBindings.Add("SelectedMenuItem", ViewModel, "selectedFolder", true, DataSourceUpdateMode.OnPropertyChanged);

            this.SavebeepButton.Click += CreateDBbutton;
            LocalDbTypebeepComboBox.ListItems.AddRange( Drivers);
            SystemFolderbeepComboBox.ListItems.AddRange(InstallationFolders);
            LocalDbTypebeepComboBox.SelectedItemChanged += LocalDbTypebeepComboBox_SelectedItemChanged;
            SystemFolderbeepComboBox.SelectedItemChanged += SystemFolderbeepComboBox_SelectedItemChanged;
        }
        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            base.OnNavigatedTo(parameters);
            //  ViewModel.CreateLocalConnection();  
        }
        private void SystemFolderbeepComboBox_SelectedItemChanged(object? sender, SelectedItemChangedEventArgs e)
        {
           if(e.SelectedItem != null)
            {
                SimpleItem selectedItem = (SimpleItem)e.SelectedItem;
                if (selectedItem.Item != null)
                {
                    viewModel.InstallFolderPath = (string)selectedItem.Value;
                    OtherFolderbeepTextBox.Text = viewModel.InstallFolderPath;
                }
                else
                {
                    viewModel.InstallFolderPath = null;
                }
            }
        }

        private void LocalDbTypebeepComboBox_SelectedItemChanged(object? sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                SimpleItem selectedItem = (SimpleItem)e.SelectedItem;
              if (selectedItem.Item != null)
                {
                    viewModel.SelectedEmbeddedDatabaseType = (DriversConfigurations.ConnectionDriversConfig)selectedItem.Item;
                    viewModel.Extension = viewModel.SelectedEmbeddedDatabaseType.extensionstoHandle;

                }
                else
                {
                    viewModel.SelectedEmbeddedDatabaseType = null;
                }

            }
        }

        
        private void CreateDBbutton(object sender, EventArgs e)
        {
            try

            {
                // check if the database name is not empty
                if (string.IsNullOrEmpty(databaseTextBox.Text))
                {
                    this.ValidateChildren();
                    MessageBox.Show("Please enter a database name", "Beep");
                    return;
                }
                // check if folder path is not empty or Special folder selected if none then error
                if (string.IsNullOrEmpty(SystemFolderbeepComboBox.Text) && string.IsNullOrEmpty(OtherFolderbeepTextBox.Text))
                {
                    this.ValidateChildren();
                    MessageBox.Show("Please select a folder path", "Beep");
                    return;
                }
               
               
                if(string.IsNullOrEmpty(LocalDbTypebeepComboBox.Text))
                {
                    this.ValidateChildren();
                    MessageBox.Show("Please select a Database driver", "Beep");
                    return;
                }
                if(SystemFolderbeepComboBox.Text == "Other")
                {
                    if (string.IsNullOrEmpty(OtherFolderbeepTextBox.Text))
                    {
                        this.ValidateChildren();
                        MessageBox.Show("Please select a folder path", "Beep");
                        return;
                    }
                }
               
             //   viewModel.InstallFolderPath = OtherFolderbeepTextBox.Text;
                if (!Editor.ConfigEditor.DataConnectionExist(databaseTextBox.Text))
                {
                  //  viewModel.SelectedEmbeddedDatabaseType=
                    this.ValidateChildren();
                    viewModel.CreateLocalConnection();
                    if (viewModel.IsCreated)
                    {
                        TreeObject.ExtensionsHelpers.GetValues();
                        TreeObject.ExtensionsHelpers.RDBMSRootBranch.CreateChildNodes();
                        Editor.AddLogMessage("Beep", $"Database Created Successfully", DateTime.Now, -1, null, Errors.Ok);
                        MessageBox.Show("Database Created Successfully", "Beep");
                    }
                    else
                    {
                        Editor.AddLogMessage("Beep", $"Error creating Database", DateTime.Now, -1, null, Errors.Failed);
                        MessageBox.Show("Error creating Database", "Beep");
                    }

                }
                else
                {
                    Editor.AddLogMessage("Beep", $"Database Already Exist by this name please try another name ", DateTime.Now, -1, null, Errors.Failed);
                    MessageBox.Show("Database Already Exist by this name please try another name ", "Beep");
                }




            }
            catch (Exception ex)
            {

                Editor.ErrorObject.Flag = Errors.Failed;
                string errmsg = "Error creating Database";
                MessageBox.Show(errmsg, "Beep");
                Editor.ErrorObject.Message = $"{errmsg}:{ex.Message}";
                //Logger.WriteLog($" {errmsg} :{ex.Message}");
                Editor.AddLogMessage("Beep", $"Error creating Local DB - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
            }
        }
    }

}
