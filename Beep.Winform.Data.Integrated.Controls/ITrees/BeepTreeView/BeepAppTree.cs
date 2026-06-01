using System.ComponentModel;
using System.Reflection;
using System.Xml.Linq;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Desktop.Common.Helpers;
using TheTechIdea.Beep.Desktop.Common.Util;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Services;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;

using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls.Helpers;
using TheTechIdea.Beep.Winform.Controls.Models;

using static TheTechIdea.Beep.Winform.Controls.Helpers.ControlExtensions;


namespace TheTechIdea.Beep.Winform.Controls.ITrees.BeepTreeView
{
    [ToolboxItem(true)]
    [DisplayName("Beep App Tree")]
    [Category("Beep Controls")]
    [Description("A control that displays App hierarchical data in a tree format.")]
    public partial class BeepAppTree : BeepTree,ITree
    {
        public BeepAppTree()
        {
            // In design mode, add sample nodes so the tree is visible
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                //AddDesignTimeSampleNodes();
                // Force rebuild/paint so nodes actually show in the designer
                base.RefreshTree();
                this.Update();
                TreeStyle = Trees.Models.TreeStyle.AntDesign;
            }
        }
        public BeepAppTree(IBeepService service)
        {
            BeepService = service;
           
          
        }

        public void init(IBeepService service,IAppManager vis)
        {
            VisManager = vis;
            VisManager.Tree = this;
            DMEEditor = service.DMEEditor;
            Treebranchhandler = new BeepTreeBranchHandler(service, this);
            DropHandler = new BeepTreeNodeDragandDropHandler(service, this);
            this.NodeRightClicked += BeepTreeControl_NodeRightClicked;
            this.NodeSelected += BeepTreeControl_NodeClicked;
            this.MenuItemSelected += BeepTreeControl_MenuItemSelected;
         
            ExtensionsHelpers = new FunctionandExtensionsHelpers( DMEEditor, VisManager, this);
          
        }
        #region "Properties"
        private IFunctionandExtensionsHelpers _extensionsHelpers;
        public  IFunctionandExtensionsHelpers ExtensionsHelpers
        {
            get { return _extensionsHelpers; }
            set { _extensionsHelpers = value; }
        }
        public bool IsCheckBoxon { get; set; } = false;
        public ITreeBranchHandler Treebranchhandler { get; set; }
        public BeepTreeNodeDragandDropHandler DropHandler { get; set; }
  
        private IBranch _selectedbranch;
        public IBranch SelectedBranch {
            get { 
                if(base.SelectedNodes != null && base.SelectedNodes.Count > 0)
                {
                    var br = GetBranchByGuidID(base.SelectedNodes[0].GuidId);
                    if (br != null)
                    {
                        _selectedbranch = br;
                    }
                }
                else
                {
                    _selectedbranch = null;
                }

                return _selectedbranch; }
            set
            {
                _selectedbranch = value;
                if (SelectedBranch != null)
                {
                    SelectedBranch.DMEEditor = DMEEditor;
                    SelectedBranch.Visutil = VisManager;
                    SelectedBranch.TreeEditor = this;
                }
            }

        }
        public string ObjectType { get; set; }= "Beep";
        public string CategoryIcon { get; set; }= "Category.svg";
        public string SelectIcon { get; set; } = "Select.svg";
        public string TreeType { get; set; } = "Beep";
        private IBranch _currentbranch;
        public IBranch CurrentBranch { get; set; }
        public IBeepService BeepService { get; set; }
        public IDMEEditor DMEEditor { get; set; }
        private List<int> _selectedBranchs = new List<int>();
        public List<int> SelectedBranchs
        {
            get {

                if (base.SelectedNodes != null)
                {
                    _selectedBranchs = new List<int>();
                    foreach (var item in base.SelectedNodes)
                    {
                        var br = GetBranchByGuidID(item.GuidId);
                        if (br != null)
                        {
                            _selectedBranchs.Add(br.ID);
                        }
                    }
                }
                return _selectedBranchs; }
            set
            {
                _selectedBranchs = value;
              
            }
        }
        public PassedArgs args { get; set; }
        int _seq = 0;
        public int SeqID { get { return _seq++; } set { } }
        public List<IBranch> Branches { get; set; }=new List<IBranch>();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<Tuple<IBranch, string>> GenerBranchs { get; set; } = new List<Tuple<IBranch, string>>();
        public List<MenuList> Menus { get; set; } = new List<MenuList>();
        public IAppManager VisManager { get; set; }
        private int _selectedBranchID;  
        public int SelectedBranchID {
            get { return _selectedBranchID; }
            set
            {
                _selectedBranchID = value;
              
            }
        }
        public string Filterstring { get; set; }
        #endregion "Properties"
        #region "Events"
        public event EventHandler<IPassedArgs> PreCallModule;
        public event EventHandler<IPassedArgs> PreShowItem;
        public event EventHandler<IBranch> RefreshBranch;
        public event EventHandler<IBranch> RefreshChildBranchs;
        public event EventHandler<IBranch> RefreshParentBranch;
        public event EventHandler<IBranch> RefreshBranchIcon;
        public event EventHandler<IBranch> RefreshBranchText;
        #endregion "Events"
        #region "Node Clicks Handlers"
        private void BeepTreeControl_MenuItemSelected(object? sender, SelectedItemChangedEventArgs e)
        {
            if (CurrentBranch == null)
            {
               // Console.WriteLine("Selected Branch is null");
                return;
            }
            AssemblyClassDefinition methodclass = DMEEditor.ConfigEditor.GlobalFunctions.Where(x => x.GuidID== e.SelectedItem.AssemblyClassDefinitionID).FirstOrDefault();
            AssemblyClassDefinition cls = DMEEditor.ConfigEditor.BranchesClasses.Where(x => x.PackageName == CurrentBranch.ToString()).FirstOrDefault();
            if (methodclass != null || cls != null)
            {
                if (methodclass != null)
                {
                    if (!DynamicFunctionCallingManager.IsMethodApplicabletoNode(methodclass, CurrentBranch)) return;
                    RunMethodFromGlobalFunctions(e.SelectedItem, e.SelectedItem.Text);
                }
                else if (cls != null)
                {
                    if (!DynamicFunctionCallingManager.IsMethodApplicabletoNode(cls, CurrentBranch)) return;
                    RunMethod(CurrentBranch, e.SelectedItem.Text);
                }
            }
        }
        public IErrorsInfo RunMethod(object branch, string MethodName)
        {
            try
            {
                ControlExtensions.RunMethodFromBranch(this, branch, MethodName);

            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", ex.Message, DateTime.Now, 0, "", Errors.Failed);
            }
            return DMEEditor.ErrorObject;
           
        }
        public IErrorsInfo RunMethodFromGlobalFunctions(SimpleItem branch, string MethodName)
        {
            try
            {
                DynamicFunctionCallingManager.RunFunctionFromExtensions( branch, MethodName);

            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", ex.Message, DateTime.Now, 0, "", Errors.Failed);
            }
            return DMEEditor.ErrorObject;

        }
        private void BeepTreeControl_NodeClicked(object? sender, BeepMouseEventArgs e)
        {
            //base.NodeSelected(sender, e);   
            //ClickedNode = sender as BeepTreeNode;
            //if (ClickedNode == null) return;
                 CurrentBranch = GetBranchByGuidID(SelectedNode.GuidId);
          //  CurrentBranch = ClickedNode;
            //IBranch br =Branches.FirstOrDefault(c => c.GuidID == ClickedNode.GuidID);
            //AssemblyClassDefinition cls = Editor.ConfigEditor.BranchesClasses.Where(x => x.PackageName == br.Name && x.Methods.Where(y => y.DoubleClick == true || y.Click == true).Any()).FirstOrDefault();
            //if (cls != null)
            //{
            //    if (!DynamicFunctionCallingManager.IsMethodApplicabletoNode(cls, br)) return;

            //    RunMethod(br, br.BranchText);

            //}
        }
        private void BeepTreeControl_NodeRightClicked(object? sender, BeepMouseEventArgs e)
        {
            //ClickedNode = sender as BeepTreeNode;
            //if (ClickedNode != null)
            //{
            //          CurrentBranch = GetBranchByGuidID(ClickedNode.GuidID);
            //   // CurrentBranch = ClickedNode;

            //}
            CurrentBranch = GetBranchByGuidID(ClickedNode.GuidId);

        }
        #endregion "Node Clicks Handlers"
        #region "Change Branch Properties"

        public void ChangeBranchIcon(int branchId, string iconName)
        {
            var branch = GetBranchById(branchId);
            if (branch != null)
            {
                branch.IconImageName = iconName;

                // Update the corresponding SimpleItem in Nodes
                var simpleItem = GetNodeByGuidID(branch.GuidID);
                if (simpleItem != null)
                {
                    simpleItem.ImagePath = ImageListHelper.GetImagePathFromName(iconName);

                    // Check if the corresponding node is drawn in NodesControls
                    var node = GetNodeByGuidID(branch.GuidID);
                    if (node != null)
                    {
                        node.ImagePath = simpleItem.ImagePath;
                        //node.Invalidate(); // Redraw if necessary
                    }
                }
            }
        }

        public void ChangeBranchIcon(string branchName, string iconName)
        {
            var branch = GetBranchByName(branchName);
            if (branch != null)
            {
                branch.IconImageName = iconName;

                // Update the corresponding SimpleItem in Nodes
                var simpleItem = GetNodeByGuidID(branch.GuidID);
                if (simpleItem != null)
                {
                    simpleItem.ImagePath = ImageListHelper.GetImagePathFromName(iconName);

                    // Check if the corresponding node is drawn in NodesControls
                    var node = GetNodeByGuidID(branch.GuidID);
                    if (node != null)
                    {
                        node.ImagePath = simpleItem.ImagePath;
                       // node.Invalidate(); // Redraw if necessary
                    }
                }
            }
        }

        public void ChangeBranchIcon(IBranch branch, string iconName)
        {
            if (branch != null)
            {
                branch.IconImageName = iconName;

                // Update the corresponding SimpleItem in Nodes
                var simpleItem = GetNodeByGuidID(branch.GuidID);
                if (simpleItem != null)
                {
                    simpleItem.ImagePath = ImageListHelper.GetImagePathFromName(iconName);

                    // Check if the corresponding node is drawn in NodesControls
                    var node = GetNodeByGuidID(branch.GuidID);
                    if (node != null)
                    {
                        node.ImagePath = simpleItem.ImagePath;
                      //  node.Invalidate(); // Redraw if necessary
                    }
                }
            }
        }

        public void ChangeBranchText(int branchId, string text)
        {
            var branch = GetBranchById(branchId);
            if (branch != null)
            {
                branch.BranchText = text;

                // Update the corresponding SimpleItem in Nodes
                var simpleItem = GetNodeByGuidID(branch.GuidID);
                if (simpleItem != null)
                {
                    simpleItem.Text = text;

                    // Check if the corresponding node is drawn in NodesControls
                    var node = GetNodeByGuidID(branch.GuidID);
                    if (node != null)
                    {
                        node.Text = simpleItem.Text;
                       // node.Invalidate(); // Redraw if necessary
                    }
                }
            }
        }

        public void ChangeBranchText(string branchName, string text)
        {
            var branch = GetBranchByName(branchName);
            if (branch != null)
            {
                branch.BranchText = text;

                // Update the corresponding SimpleItem in Nodes
                var simpleItem = GetNodeByGuidID(branch.GuidID);
                if (simpleItem != null)
                {
                    simpleItem.Text = text;

                    // Check if the corresponding node is drawn in NodesControls
                    var node = GetNodeByGuidID(branch.GuidID);
                    if (node != null)
                    {
                        node.Text = simpleItem.Text;
                       // node.Invalidate(); // Redraw if necessary
                    }
                }
            }
        }

        public void ChangeBranchText(IBranch branch, string text)
        {
            if (branch != null)
            {
                branch.BranchText = text;

                // Update the corresponding SimpleItem in Nodes
                var simpleItem = GetNodeByGuidID(branch.GuidID);
                if (simpleItem != null)
                {
                    simpleItem.Text = text;

                    // Check if the corresponding node is drawn in NodesControls
                    var node = GetNodeByGuidID(branch.GuidID);
                    if (node != null)
                    {
                        node.Text = simpleItem.Text;
                       // node.Invalidate(); // Redraw if necessary
                    }
                }
            }
        }

        public void TurnonOffCheckBox(bool val)
        {
            // Update the checkbox visibility for all NodesControls
            ShowCheckBox = val;
            IsCheckBoxon = val;
            Invalidate();
        }

        #endregion "Change Branch Properties"
        #region "Create Branches"
        public IErrorsInfo CreateFunctionExtensions(MethodsClass item)
        {
            try
            {
                ControlExtensions.CreateFunctionExtensions(this,item);

            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage("Error", ex.Message, DateTime.Now, 0, "", Errors.Failed);
            }
            return DMEEditor.ErrorObject;
           
        }
        public IErrorsInfo CreateRootTree()
        {
            string packagename = "";
            try
            {
                Branches = new List<IBranch>();
                GenerBranchs = new List<Tuple<IBranch, string>>();
                
                // Clear nodes properly
                Nodes.Clear();
                // Call base BeepTree's RefreshTree to clear visible nodes cache
                base.RefreshTree();
               
                IBranch Genrebr = null;
                foreach (AssemblyClassDefinition GenreBrAssembly in DMEEditor.ConfigEditor.BranchesClasses.Where(p => p.classProperties != null && p.VisSchema != null && p.VisSchema.BranchType == EnumPointType.Genre).OrderBy(x => x.Order))
                {
                    if (GenreBrAssembly != null)
                    {

                        Type adc = DMEEditor.assemblyHandler.GetType(GenreBrAssembly.PackageName);
                        ConstructorInfo ctor = adc.GetConstructors().Where(o => o.GetParameters().Length == 0).FirstOrDefault()!;
                        if (ctor != null)
                        {
                            ObjectActivator<IBranch> createdActivator = GetActivator<IBranch>(ctor);
                            try
                            {
                                Genrebr = createdActivator();
                                if (Genrebr.BranchType == EnumPointType.Genre)
                                {
                                    if (GenreBrAssembly.PackageName != null)
                                    {
                                        PassedArgs x = new PassedArgs { ObjectType = GenreBrAssembly.PackageName };
                                        x.Cancel = false;
                                        x.AddinName = GenreBrAssembly.PackageName;
                                        x.AddinType = GenreBrAssembly.componentType;
                                        x.CurrentEntity = GenreBrAssembly.PackageName;
                                        x.ObjectName = GenreBrAssembly.className;
                                        x.ObjectType = GenreBrAssembly.classProperties.ClassType;
                                        PreShowItem?.Invoke(this, x);
                                        //   VisManager.Addins.Where(p => p.ObjectName == GenreBrAssembly.className).FirstOrDefault().Run();
                                        if (x.Cancel)
                                        {
                                            Genrebr.Visible = false;
                                        }
                                    }
                                    int id = SeqID;
                                    Genrebr.Name = GenreBrAssembly.PackageName;
                                    packagename = GenreBrAssembly.PackageName;
                                    Genrebr.ID = id;
                                    Genrebr.BranchID = id;
                                    if (TreeType != null)
                                    {
                                        if (GenreBrAssembly.classProperties.ObjectType != null)
                                        {
                                            if (GenreBrAssembly.classProperties.ObjectType.Equals(TreeType, StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                if (Genrebr.Visible)
                                                {
                                                    CreateNode(id, Genrebr);
                                                }

                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (Genrebr.Visible)
                                        {
                                            CreateNode(id, Genrebr);
                                        }
                                    }

                                    GenerBranchs.Add(new Tuple<IBranch, string>(Genrebr, GenreBrAssembly.classProperties.menu));
                                }
                            }
                            catch (Exception ex)
                            {
                                DMEEditor.AddLogMessage("Error", $"Creating StandardTree Root Node {GenreBrAssembly.PackageName} {ex.Message} ", DateTime.Now, 0, null, Errors.Failed);
                            }
                        }
                    }
                }

                foreach (AssemblyClassDefinition cls in DMEEditor.ConfigEditor.BranchesClasses.Where(p => p.classProperties != null).OrderBy(x => x.Order))
                {
                    Type adc = DMEEditor.assemblyHandler.GetType(cls.PackageName);
                    ConstructorInfo ctor = adc.GetConstructors().Where(o => o.GetParameters().Length == 0).FirstOrDefault()!;
                    if (ctor == null)
                    {
                        ctor = adc.GetConstructors().FirstOrDefault();
                    }
                    if (ctor != null)
                    {
                        ObjectActivator<IBranch> createdActivator = GetActivator<IBranch>(ctor);

                        try
                        {
                            IBranch br = createdActivator();
                            if (br.BranchType == EnumPointType.Root)
                            {
                                int id = SeqID;
                                br.Name = cls.PackageName;
                                packagename = cls.PackageName;
                                br.ID = id;
                                br.BranchID = id;
                                br.DMEEditor= DMEEditor;
                                br.TreeEditor= this;
                                if (cls.PackageName != null)
                                {
                                    PassedArgs x = new PassedArgs { ObjectType = cls.PackageName };
                                    x.Cancel = false;
                                    x.AddinName = cls.PackageName;
                                    x.AddinType = cls.componentType;
                                    x.CurrentEntity = cls.PackageName;
                                    x.ObjectName = cls.className;
                                    x.ObjectType = cls.classProperties.ClassType;
                                    PreShowItem?.Invoke(this, x);
                                    //   VisManager.Addins.Where(p => p.ObjectName == GenreBrAssembly.className).FirstOrDefault().Run();
                                    if (x.Cancel)
                                    {
                                        br.Visible = false;
                                    }
                                }
                                if (TreeType != null)
                                {
                                    if (cls.classProperties.ObjectType != null)
                                    {
                                        if (cls.classProperties.ObjectType.Equals(TreeType, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            var tr = GenerBranchs.FirstOrDefault(p => p.Item2.Equals(cls.classProperties.menu, StringComparison.OrdinalIgnoreCase));
                                            if (tr != null)
                                            {
                                                Genrebr = tr.Item1;
                                            }
                                            else
                                                Genrebr = null;

                                            if (Genrebr != null)
                                            {
                                                if (Genrebr.Visible)
                                                {
                                                    AddBranch(Genrebr, br);
                                                    //br!.CreateChildNodes();
                                                }

                                            }
                                            else
                                            {
                                                if (br.Visible)
                                                {
                                                    CreateNode(id, br);
                                                }

                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (cls.classProperties.Category != DatasourceCategory.NONE)
                                    {
                                        AddBranch(Genrebr, br);
                                        //br!.CreateChildNodes();
                                    }
                                    else
                                    {
                                        if (br.Visible)
                                        {
                                            CreateNode(id, br);
                                        }

                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            DMEEditor.AddLogMessage("Error", $"Creating StandardTree Root Node {cls.PackageName} {ex.Message} ", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
                
                // If no nodes were created, check if there's any data available
                if (Nodes.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: No nodes created. DMEEditor.ConfigEditor.BranchesClasses may be empty or filtered out.");
                    System.Diagnostics.Debug.WriteLine($"Total BranchesClasses available: {DMEEditor.ConfigEditor.BranchesClasses?.Count ?? 0}");
                }
                
                // Call base BeepTree's RefreshTree to rebuild visible nodes and update display
                base.RefreshTree();
                // Ensure the tree is expanded so users can see the content
                ExpandAll();
                
                // CRITICAL FIX: Force immediate paint update
                Update();
                
                // Debug: Log how many nodes were created
                System.Diagnostics.Debug.WriteLine($"CreateRootTree completed: {Nodes.Count} root nodes, {Branches.Count} branches");
            }
            catch (Exception ex)
            {
                DMEEditor.ErrorObject.Ex = ex;
                DMEEditor.ErrorObject.Flag = Errors.Failed;
                DMEEditor.AddLogMessage("Error", $"Creating StandardTree Root Node {packagename} - {ex.Message} ", DateTime.Now, 0, null, Errors.Failed);
                System.Diagnostics.Debug.WriteLine($"CreateRootTree ERROR: {ex.Message}");
            };
            return DMEEditor.ErrorObject;
        }
        public IErrorsInfo AddBranch(IBranch ParentBranch, IBranch br)
        {
            SimpleItem parentnode = new SimpleItem();
          
            try
            {
                if (br.GuidID == null)
                {
                    br.GuidID = Guid.NewGuid().ToString();
                }
                int id = SeqID;
                if (ParentBranch.ChildBranchs.Where(x => x.BranchText == br.BranchText).Any())
                {
                    DMEEditor.AddLogMessage("Error", "Branch already exist", DateTime.Now, -1, null, Errors.Failed);
                    return DMEEditor.ErrorObject;
                }
               // Console.WriteLine($"Adding Branch {br.BranchText} to {ParentBranch.BranchText}");
                parentnode = GetNode(ParentBranch.BranchText);
       
                if (parentnode == null)
                {
                    DMEEditor.AddLogMessage("Error", $"Parent Node not found {ParentBranch.BranchText}", DateTime.Now, -1, null, Errors.Failed);
                    return DMEEditor.ErrorObject;
                }
              
                br.ParentBranch = ParentBranch;
                br.ParentBranchID = ParentBranch.ID;
                br.ParentGuidID = ParentBranch.GuidID;
                SimpleItem item = ControlExtensions.CreateNode(this, id, br);
                item.GuidId = br.GuidID;
                item.ContainerGuidID = br.GuidID;
                item.IsDrawn = false;
                // Make newly added branches expanded by default so content is visible
                item.IsExpanded = true;

               // Console.WriteLine(br.BranchText);
                DynamicMenuManager.CreateMenuMethods(DMEEditor, br);
                if (br.ObjectType != null && br.BranchClass != null)
                {
                    DynamicMenuManager.CreateGlobalMenu( br);
                }


                br.DMEEditor = DMEEditor;
                br.Visutil = VisManager;
                br.TreeEditor = this;
                if (!DMEEditor.ConfigEditor.objectTypes.Any(i => i.ObjectType == br.BranchClass && i.ObjectName == br.BranchType.ToString() + "_" + br.BranchClass))
                {
                    DMEEditor.ConfigEditor.objectTypes.Add(new Workflow.ObjectTypes { ObjectType = br.BranchClass, ObjectName = br.BranchType.ToString() + "_" + br.BranchClass });
                }
                try
                {
                    br.SetConfig(this, DMEEditor, br.ParentBranch, br.BranchText, br.ID, br.BranchType, null);
                }
                catch (Exception ex)
                {

                }
               
                // add to Simpleitem child nodes
                AddNodeWithBranch(item, parentnode.GuidId);
                // add to Child Branchs
                ParentBranch.ChildBranchs.Add(br);
             //   AddBranchToParentInBranchsOnly(ParentBranch, br);
             if(br.ChildBranchs!=null)
                {
                    if (br.ChildBranchs.Count == 0)
                    {
                        br.CreateChildNodes();
                        if (br.ChildBranchs.Count > 0)
                        {
                            foreach (var childbr in br.ChildBranchs)
                            {
                                AddBranch(br, childbr);
                                // br.ChildBranchs.Add(childbr);
                            }

                        }
                    }
                }
          
                

            }
            catch (Exception ex)
            {
                string mes = "Could not Add Branch to " + ParentBranch.BranchText;
                DMEEditor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
            };
            return DMEEditor.ErrorObject;
        }
        public bool AddBranchToParentInBranchsOnly(IBranch ParentBranch, IBranch br)
        {
            if (ParentBranch != null)
            {
                IBranch parent = Branches.FirstOrDefault(c => c.ID == ParentBranch.ID);
                if (parent != null)
                {
                    if (parent.ChildBranchs.Where(x => x.BranchText == br.BranchText).Any())
                    {
                        DMEEditor.AddLogMessage("Error", "Branch already exist", DateTime.Now, -1, null, Errors.Failed);
                        return false;
                    }
                    parent.ChildBranchs.Add(br);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        public void AddBranch( IBranch br)
        {
            if (br.GuidID == null)
            {
                br.GuidID = Guid.NewGuid().ToString();
            }
            int id = SeqID;
            SimpleItem item = ControlExtensions.CreateNode(this, id, br);
            item.GuidId = br.GuidID;
            item.ContainerGuidID = br.GuidID;
            // Expand new root by default
            item.IsExpanded = true;
            //n.ContextMenuStrip = 
           // Console.WriteLine(br.BranchText);
            DynamicMenuManager.CreateMenuMethods(DMEEditor, br);
            if (br.ObjectType != null && br.BranchClass != null)
            {
                DynamicMenuManager.CreateGlobalMenu( br);
            }

            br.DMEEditor = DMEEditor;
            br.Visutil = VisManager;
            br.TreeEditor = this;
            if (!DMEEditor.ConfigEditor.objectTypes.Any(i => i.ObjectType == br.BranchClass && i.ObjectName == br.BranchType.ToString() + "_" + br.BranchClass))
            {
                DMEEditor.ConfigEditor.objectTypes.Add(new Workflow.ObjectTypes { ObjectType = br.BranchClass, ObjectName = br.BranchType.ToString() + "_" + br.BranchClass });
            }
            try
            {
                br.SetConfig(this, DMEEditor, br.ParentBranch, br.BranchText, br.ID, br.BranchType, null);
            }
            catch (Exception ex)
            {

            }
            
           // Nodes.Add(item);
            AddNodeWithBranch(item);
            Branches.Add(br);
            br.CreateChildNodes();
        }
        public void CreateNode(int id, IBranch br)
        {
            if(br.GuidID == null)
            {
                br.GuidID = Guid.NewGuid().ToString();
            }
            SimpleItem item=ControlExtensions.CreateNode(this,id, br);
            br.GuidID = item.GuidId;
            
            // CRITICAL FIX: Expand root nodes by default so their children are visible
            if (br.ParentBranch == null)
            {
                item.IsExpanded = true;
            }
            
            //n.ContextMenuStrip = 
           // Console.WriteLine(br.BranchText);
            DynamicMenuManager.CreateMenuMethods(DMEEditor, br);
            if (br.ObjectType != null && br.BranchClass != null)
            {
                DynamicMenuManager.CreateGlobalMenu(br);
            }

            br.DMEEditor = DMEEditor;
            br.Visutil = VisManager;
            br.TreeEditor = this;
            if (!DMEEditor.ConfigEditor.objectTypes.Any(i => i.ObjectType == br.BranchClass && i.ObjectName == br.BranchType.ToString() + "_" + br.BranchClass))
            {
                DMEEditor.ConfigEditor.objectTypes.Add(new Workflow.ObjectTypes { ObjectType = br.BranchClass, ObjectName = br.BranchType.ToString() + "_" + br.BranchClass });
            }
            try
            {
                br.SetConfig(this, DMEEditor, br.ParentBranch, br.BranchText, br.ID, br.BranchType, null);
            }
            catch (Exception ex)
            {

            }
           
           //ee Nodes.Add(item);
            AddNodeWithBranch(item);
            Branches.Add(br);
            br.CreateChildNodes();
        }
       
        #endregion "Create Branches"
        #region "Branch Retrieval"

        /// <summary>
        /// Retrieves a branch by its ID, searching recursively through all branches.
        /// </summary>
        /// <param name="branchId">The ID of the branch to find.</param>
        /// <returns>The matching branch, or null if not found.</returns>
        private IBranch GetBranchById(int branchId)
        {
            return FindBranch(Branches, b => b.ID == branchId);
        }

        /// <summary>
        /// Retrieves a branch by its name, searching recursively through all branches.
        /// </summary>
        /// <param name="branchName">The name of the branch to find.</param>
        /// <returns>The matching branch, or null if not found.</returns>
        private IBranch GetBranchByName(string branchName)
        {
            return FindBranch(Branches, b => b.Name.Equals(branchName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Recursively searches for a branch matching the specified predicate.
        /// </summary>
        /// <param name="branches">The collection of branches to search through.</param>
        /// <param name="predicate">The condition to match the branch.</param>
        /// <returns>The matching branch, or null if not found.</returns>
        private IBranch FindBranch(IEnumerable<IBranch> branches, Func<IBranch, bool> predicate)
        {
            foreach (var branch in branches)
            {
                if (predicate(branch))
                {
                    return branch;
                }

                // Recursively search child branches
                var result = FindBranch(branch.ChildBranchs, predicate);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

    
        public IBranch GetBranch(string text, EnumPointType branchtype)
        {
            return FindBranchRecursive(Branches, branch => branch.BranchText == text && branch.BranchType == branchtype);
        }
        public IBranch GetBranch(string text)
        {
            return FindBranchRecursive(Branches, branch => branch.BranchText == text);
        }
        
        public IBranch GetBranch(int id)
        {
            return FindBranchRecursive(Branches, branch => branch.ID == id);
        }
        public IBranch GetBranch(string text, string parent)
        {
            return FindBranchRecursive(Branches, branch => branch.BranchText == text && branch.ParentBranch.Name == parent);
        }

        public IBranch GetBranchByEntityGuidID(string guidid)
        {
            return FindBranchRecursive(Branches, branch => branch.EntityGuidID == guidid);
        }

        public IBranch GetBranchByGuidID(string guidid)
        {
            return FindBranchRecursive(Branches, branch => branch.GuidID == guidid);
        }

        public IBranch GetBranchByMiscGuidID(string guidid)
        {
            return FindBranchRecursive(Branches, branch => branch.MiscStringID == guidid);
        }

        public object GetTreeNodeByID(int id)
        {
            return FindBranchRecursive(Branches, branch => branch.ID == id);
        }

        private IBranch FindBranchRecursive(IEnumerable<IBranch> branches, Func<IBranch, bool> predicate)
        {
            if (branches == null) return null;
            foreach (var branch in branches)
            {
                if (predicate(branch))
                {
                    return branch;
                }

                var childResult = FindBranchRecursive(branch.ChildBranchs, predicate);
                if (childResult != null)
                {
                    return childResult;
                }
            }

            return null;
        }
        #endregion "Branch Retrieval"
        #region "Refresh Tree"
        public void RefreshTree()
        {
           
            CreateRootTree();
            Invalidate();
        }
        public void RefreshTree(IBranch branch)
        {
            if (branch == null) return;

            var simpleItem = GetNodeByGuidID(branch.GuidID);
            if (simpleItem != null)
            {
                simpleItem.Text = branch.Name;
                simpleItem.ImagePath = ImageListHelper.GetImagePathFromName( branch.IconImageName);

                var node = GetNodeByGuidID(branch.GuidID);
                if (node != null)
                {
                    node.Text = branch.Name;
                    node.ImagePath = ImageListHelper.GetImagePathFromName(branch.IconImageName);
                   
                }
            }
            Invalidate();
        }
        public void RefreshTree(int branchid)
        {
            var branch = GetBranchById(branchid);
            if (branch != null)
            {
                RefreshTree(branch);
            }
        }
        public void RefreshTree(string branchname)
        {
            var branch = GetBranchByName(branchname);
            if (branch != null)
            {
                RefreshTree(branch);
            }
        }
        #endregion "Refresh Tree"
        #region "Remove Node"
        public void RemoveNode(int id)
        {
            // Remove from Nodes (recursively) if it exists
            var simpleItem = TraverseAllItems(Nodes).FirstOrDefault(n => n.ID == id);
            if (simpleItem != null)
            {
                RemoveNode(simpleItem); // This will handle child nodes and remove controls if needed
            }

            // Remove from Branches if it exists
            var branch = Branches.FirstOrDefault(n => n.ID == id);
            if (branch != null)
            {
                Branches.Remove(branch);
            }
        }

        public void RemoveNode(IBranch br)
        {
            if (br == null) return;

            // Find the corresponding SimpleItem and remove it
            var simpleItem = TraverseAllItems(Nodes).FirstOrDefault(n => n.GuidId == br.GuidID);
            if (simpleItem != null)
            {
                RemoveNode(simpleItem);
            }

            // Remove the branch from Branches
            Branches.Remove(br);
        }

        public void RemoveNode(string branchName)
        {
            if (string.IsNullOrWhiteSpace(branchName)) return;

            // Find the corresponding Branch
            var branch = Branches.FirstOrDefault(b => b.Name.Equals(branchName, StringComparison.OrdinalIgnoreCase));
            if (branch != null)
            {
                RemoveNode(branch); // Reuse the `RemoveNode(IBranch br)` method
            }
        }

        public void RemoveNodeByGuidID(string guidID)
        {
            if (string.IsNullOrWhiteSpace(guidID)) return;

            // Find the corresponding Branch
            var branch = Branches.FirstOrDefault(b => b.GuidID == guidID);
            if (branch != null)
            {
                RemoveNode(branch); // Reuse the `RemoveNode(IBranch br)` method
            }
        }
        #endregion "Remove Node"
        #region "Select Branch "
        #endregion "Select Branch "
        #region "Mouse Events"

        #endregion "Mouse Events"
    }
}
