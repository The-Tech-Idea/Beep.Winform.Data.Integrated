using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;

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
using TheTechIdea.Beep.DriversConfigurations;

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
            ApplyDpiScaledLayout();
        }

        /// <summary>
        /// Skill § "Sizing tokens": apply DPI-scaled <see cref="BeepLayoutMetrics"/> values to
        /// chrome that the Designer serialized as static pixels. The Designer is the source
        /// of truth for layout; this method overlays DPI-scaled dimensions on top.
        /// </summary>
        private void ApplyDpiScaledLayout()
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

        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);
            if (appManager?.Tree != null)
            {
                TreeObject = (ITree)appManager.Tree;
            }
            viewModel = new DataConnectionViewModel(beepService.DMEEditor, appManager);

            if (beepService?.Config_editor?.DataConnections != null)
            {
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
                }
            }

            var versions = new List<SimpleItem>();
            if (viewModel.EmbeddedDatabaseTypes != null)
            {
                foreach (var item in viewModel.EmbeddedDatabaseTypes)
                {
                    SimpleItem driveritem = new SimpleItem();
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
            }

            // Get installation folders from config folders
            if (beepService?.Config_editor?.Config?.Folders != null)
            {
                foreach (var item in beepService.Config_editor.Config.Folders
                    .Where(x => x.FolderFilesType == FolderFileTypes.DataFiles || x.FolderFilesType == FolderFileTypes.ProjectData)
                    .ToList())
                {
                    string foldername = Path.GetFileName(item.FolderPath);
                    SimpleItem folderitem = new SimpleItem();
                    folderitem.DisplayField = foldername;
                    folderitem.Text = foldername;
                    folderitem.Name = item.FolderPath;
                    folderitem.Value = item.FolderPath;
                    folderitem.Item = item;
                    InstallationFolders.Add(folderitem);
                }
            }

            // Standard well-known folders (from .NET environment)
            var wellKnown = new (string Display, Environment.SpecialFolder Folder)[]
            {
                ("Program Files",            Environment.SpecialFolder.ProgramFiles),
                ("Common Application Data", Environment.SpecialFolder.CommonApplicationData),
                ("Common Program Files",    Environment.SpecialFolder.CommonProgramFiles),
                ("Local Application Data",  Environment.SpecialFolder.LocalApplicationData),
                ("Application Data",         Environment.SpecialFolder.ApplicationData),
                ("Desktop",                  Environment.SpecialFolder.Desktop),
                ("Personal",                 Environment.SpecialFolder.Personal),
                ("Documents",                Environment.SpecialFolder.MyDocuments),
                ("Downloads",                Environment.SpecialFolder.UserProfile),
            };
            foreach (var (display, folder) in wellKnown)
            {
                string path = Environment.GetFolderPath(folder);
                InstallationFolders.Add(new SimpleItem
                {
                    DisplayField = display,
                    Text = display,
                    Name = path,
                    Value = path
                });
            }

            if (viewModel != null)
            {
                databaseTextBox.DataBindings.Add("Text", viewModel, "DatabaseName", true, DataSourceUpdateMode.OnPropertyChanged);
                PasswordbeepTextBox.DataBindings.Add("Text", viewModel, "Password", true, DataSourceUpdateMode.OnPropertyChanged);
            }

            // Skill § onConfigure handler accumulation: -= before += so multiple
            // Configure calls do not stack delegates on each event.
            SavebeepButton.Click -= CreateDBbutton;
            SavebeepButton.Click += CreateDBbutton;
            LocalDbTypebeepComboBox.SelectedItemChanged -= LocalDbTypebeepComboBox_SelectedItemChanged;
            LocalDbTypebeepComboBox.SelectedItemChanged += LocalDbTypebeepComboBox_SelectedItemChanged;
            SystemFolderbeepComboBox.SelectedItemChanged -= SystemFolderbeepComboBox_SelectedItemChanged;
            SystemFolderbeepComboBox.SelectedItemChanged += SystemFolderbeepComboBox_SelectedItemChanged;

            LocalDbTypebeepComboBox.ListItems.AddRange(Drivers);
            SystemFolderbeepComboBox.ListItems.AddRange(InstallationFolders);
        }

        private void SystemFolderbeepComboBox_SelectedItemChanged(object? sender, SelectedItemChangedEventArgs e)
        {
            if (e?.SelectedItem is not SimpleItem selectedItem) return;

            if (selectedItem.Item != null && selectedItem.Value is string path)
            {
                viewModel.InstallFolderPath = path;
                OtherFolderbeepTextBox.Text = viewModel.InstallFolderPath;
            }
            else
            {
                viewModel.InstallFolderPath = null;
            }
        }

        private void LocalDbTypebeepComboBox_SelectedItemChanged(object? sender, SelectedItemChangedEventArgs e)
        {
            if (e?.SelectedItem is not SimpleItem selectedItem) return;

            if (selectedItem.Item is ConnectionDriversConfig driver)
            {
                viewModel.SelectedEmbeddedDatabaseType = driver;
                viewModel.Extension = driver.extensionstoHandle;
            }
            else
            {
                viewModel.SelectedEmbeddedDatabaseType = null;
            }
        }

        private void CreateDBbutton(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(databaseTextBox.Text))
                {
                    ValidateChildren();
                    MessageBox.Show("Please enter a database name", "Beep");
                    return;
                }
                if (string.IsNullOrEmpty(SystemFolderbeepComboBox.Text) && string.IsNullOrEmpty(OtherFolderbeepTextBox.Text))
                {
                    ValidateChildren();
                    MessageBox.Show("Please select a folder path", "Beep");
                    return;
                }
                if (string.IsNullOrEmpty(LocalDbTypebeepComboBox.Text))
                {
                    ValidateChildren();
                    MessageBox.Show("Please select a Database driver", "Beep");
                    return;
                }
                if (SystemFolderbeepComboBox.Text == "Other" && string.IsNullOrEmpty(OtherFolderbeepTextBox.Text))
                {
                    ValidateChildren();
                    MessageBox.Show("Please select a folder path", "Beep");
                    return;
                }

                if (Editor.ConfigEditor.DataConnectionExist(databaseTextBox.Text)) return;

                ValidateChildren();
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
            catch (Exception ex)
            {
                Editor.ErrorObject.Flag = Errors.Failed;
                string errmsg = "Error creating Database";
                MessageBox.Show(errmsg, "Beep");
                Editor.ErrorObject.Message = $"{errmsg}:{ex.Message}";
                Editor.AddLogMessage("Beep", $"Error creating Local DB - {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
            }
        }
    }
}
