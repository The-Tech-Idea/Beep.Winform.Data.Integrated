
using System;
using System.Reflection;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls.Helpers;
using TheTechIdea.Beep.Winform.Controls.Models;
using static TheTechIdea.Beep.Winform.Controls.Helpers.ControlExtensions;

namespace TheTechIdea.Beep.Winform.Controls.Helpers
{
    public static class MenusandToolbarGenerators
    {
        public static List<MenuList> Menus { get; set; } = new List<MenuList>();
        public static List<SimpleItem> GetMethods(IDMEEditor DMEEditor, string ObjectType,bool IsHorizentalBar)
        {
            List < SimpleItem > Methods = new List<SimpleItem>();
            try
            {
                
                List<AssemblyClassDefinition> classes = new List<AssemblyClassDefinition>();
                if (!IsHorizentalBar)
                {
                    classes = DMEEditor.ConfigEditor.GlobalFunctions.Where(x => x.componentType == "IFunctionExtension" && x.classProperties != null && x.classProperties.ObjectType != null && (x.classProperties.Showin == ShowinType.Toolbar || x.classProperties.Showin == ShowinType.Both) && x.classProperties.ObjectType.Equals(ObjectType, StringComparison.InvariantCultureIgnoreCase)).OrderBy(p => p.Order).ToList();

                }
                else
                {
                    classes = DMEEditor.ConfigEditor.GlobalFunctions.Where(x => x.componentType == "IFunctionExtension" && x.classProperties != null && x.classProperties.ObjectType != null && (x.classProperties.Showin == ShowinType.HorZToolbar) && x.classProperties.ObjectType.Equals(ObjectType, StringComparison.InvariantCultureIgnoreCase)).OrderBy(p => p.Order).ToList();

                }

                foreach (AssemblyClassDefinition cls in classes)
                {

                    foreach (var method in cls.Methods)
                    {
                        SimpleItem item = new SimpleItem();
                        item.Name = method.Name;
                        item.Text = method.Caption;
                        item.ImagePath = method.iconimage;
                        item.DisplayField = method.Caption;
                        item.ObjectType = method.ObjectType;
                        item.MethodName = method.Name;
                        item.AssemblyClassDefinitionID = cls.GuidID;
                        item.ClassDefinitionID= method.GuidID;
                        item.ActionID = method.GuidID;
                        
                        //ToolStripButton toolStripButton1 = new ToolStripButton();
                        //if (item.iconimage != null)
                        //{

                        //    toolStripButton1.ImageIndex = vismanager.visHelper.GetImageIndex(item.iconimage);
                        //    toolStripButton1.ImageKey = item.iconimage;
                        //}
                        //toolStripButton1.Alignment = item.CommandAttr.IsLeftAligned ? ToolStripItemAlignment.Left : ToolStripItemAlignment.Right;
                        //toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
                        //toolStripButton1.TextAlign = ContentAlignment.BottomLeft;
                        ////toolStripButton1.ImageAlign = ContentAlignment.TopRight;
                        //toolStripButton1.Name = item.Name;
                        //toolStripButton1.Value = new System.Drawing.Value(24, 24);
                        //toolStripButton1.Text = item.Caption;
                        //toolStripButton1.ToolTipText = item.Caption;
                        //toolStripButton1.Click += RunFunction;
                        //toolStripButton1.Tag = cls;
                        //toolStripButton1.AutoSize = true;

                        //toolStripButton1.Width = 32;
                        //toolStripButton1.Font = new Font("Arial", 8, FontStyle.Regular);
                        //toolStripButton1.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.SizeToFit;
                        //ToolStrip.Items.Add(toolStripButton1);

                        //ToolStripSeparator stripSeparator = new ToolStripSeparator();
                        // ToolStrip.Items.Add(stripSeparator);
                    }

                }
                ////-------------------------------------------------------------------------------------------

               
            }
            catch (Exception ex)
            {

               
            }
            return Methods;
        }

        #region "Tree Generators"
        public static IErrorsInfo CreateRootTree(IDMEEditor DMEEditor, TreeView TreeV,  ITree treeeditor,ITreeBranchHandler treeBranchHandler, string TreeType)
        {
            int SeqID = 0;
            string packagename = "";
            var GenerBranchs = new List<Tuple<IBranch, string>>();
            var Branches = new List<IBranch>();
            try
            {
                //bool HasConstructor=false;


                IBranch Genrebr = null;
                // AssemblyClassDefinition GenreBrAssembly = Editor.ConfigEditor.BranchesClasses.Where(p => p.classProperties != null && p.VisSchema != null && p.VisSchema.BranchType == EnumPointType.Genre).FirstOrDefault()!;
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
                                                    CreateNode(DMEEditor, id, Genrebr, TreeV,treeeditor );
                                                }

                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (Genrebr.Visible)
                                        {
                                            CreateNode(DMEEditor, id, Genrebr, TreeV, treeeditor);
                                        }
                                    }

                                    GenerBranchs.Add(new Tuple<IBranch, string>(Genrebr, GenreBrAssembly.classProperties.menu));
                                }
                            }
                            catch (Exception ex)
                            {
                                MiscFunctions.AddLogMessage("Error", $"Creating Tree Root Node {GenreBrAssembly.PackageName} {ex.Message} ", DateTime.Now, 0, null, Errors.Failed);
                            }
                        }
                    }
                }

                foreach (AssemblyClassDefinition cls in DMEEditor.ConfigEditor.BranchesClasses.Where(p => p.classProperties != null).OrderBy(x => x.Order))
                {
                    Type adc = DMEEditor.assemblyHandler.GetType(cls.PackageName);
                    ConstructorInfo ctor = adc.GetConstructors().Where(o => o.GetParameters().Length == 0).FirstOrDefault()!;

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
                                if (cls.PackageName != null)
                                {
                                    PassedArgs x = new PassedArgs { ObjectType = cls.PackageName };
                                    x.Cancel = false;
                                    x.AddinName = cls.PackageName;
                                    x.AddinType = cls.componentType;
                                    x.CurrentEntity = cls.PackageName;
                                    x.ObjectName = cls.className;
                                    x.ObjectType = cls.classProperties.ClassType;
                                    //  PreShowItem?.Invoke(this, x);
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
                                                    treeBranchHandler.AddBranch(Genrebr, br);
                                                    br!.CreateChildNodes();
                                                }

                                            }
                                            else
                                            {
                                                if (br.Visible)
                                                {
                                                    CreateNode(DMEEditor, id, br, TreeV, treeeditor);
                                                    
                                                }

                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (cls.classProperties.Category != DatasourceCategory.NONE)
                                    {
                                        treeBranchHandler.AddBranch(Genrebr, br);
                                        br!.CreateChildNodes();
                                    }
                                    else
                                    {
                                        if (br.Visible)
                                        {
                                            CreateNode(DMEEditor, id, br, TreeV, treeeditor);
                                            
                                        }

                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MiscFunctions.AddLogMessage("Error", $"Creating Tree Root Node {cls.PackageName} {ex.Message} ", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.ErrorObject.Ex = ex;
                DMEEditor.ErrorObject.Flag = Errors.Failed;
                MiscFunctions.AddLogMessage("Error", $"Creating Tree Root Node {packagename} - {ex.Message} ", DateTime.Now, 0, null, Errors.Failed);

            };
            return DMEEditor.ErrorObject;
        }
        private static void CreateNode(IDMEEditor DMEEditor, int id, IBranch br, TreeView tree,ITree treeeditor)
        {
            TreeNode n = tree.Nodes.Add(br.BranchText);
            n.Tag = br;
            br.TreeEditor = treeeditor;
            br.BranchID = id;
            br.ID = id;
            n.Name = br.ID.ToString();
         
            //br.ParentBranch = n;
            int imgidx = ImageListHelper.GetImageIndex(br.IconImageName);
            if (imgidx == -1)
            {
                imgidx = ImageListHelper.GetImageIndexFromConnectioName(br.BranchText);
            }
            if (imgidx == -1)
            {
                n.ImageKey = br.IconImageName;
                n.SelectedImageKey = br.IconImageName;

            }
            else
            {
                n.ImageIndex = imgidx;
                n.SelectedImageIndex = imgidx;

            }
            //n.ContextMenuStrip = 
            Console.WriteLine(br.BranchText);
            //CreateMenuMethods(Editor, br);
            if (br.ObjectType != null && br.BranchClass != null)
            {
             //   CreateGlobalMenu(Editor,br);
            }

            br.DMEEditor = DMEEditor;
            if (!DMEEditor.ConfigEditor.objectTypes.Any(i => i.ObjectType == br.BranchClass && i.ObjectName == br.BranchType.ToString() + "_" + br.BranchClass))
            {
                DMEEditor.ConfigEditor.objectTypes.Add(new TheTechIdea.Beep.Workflow.ObjectTypes { ObjectType = br.BranchClass, ObjectName = br.BranchType.ToString() + "_" + br.BranchClass });
            }
            try
            {
                br.SetConfig(treeeditor, DMEEditor, br.ParentBranch, br.BranchText, br.ID, br.BranchType, null);
            }
            catch (Exception ex)
            {

            }

          
            br.CreateChildNodes();
        }
        #endregion "Tree Generators"

        #region "Run Functions"

        public static void RunFunction(IDMEEditor DMEEditor, object sender, EventArgs e)
        {
            IBranch br = null;
            AssemblyClassDefinition assemblydef = new AssemblyClassDefinition();
            MethodInfo method = null;
            MethodsClass methodsClass;
            string MethodName = "";
            if (sender == null) { return; }
            if (sender.GetType() == typeof(ToolStripButton))
            {
                ToolStripButton item = (ToolStripButton)sender;
                assemblydef = (AssemblyClassDefinition)item.Tag;
                MethodName = item.Text;
            }
            if (sender.GetType() == typeof(ToolStripMenuItem))
            {
                ToolStripMenuItem item = (ToolStripMenuItem)sender;
                assemblydef = (AssemblyClassDefinition)item.Tag;
                MethodName = item.Text;
            }
            dynamic fc = DMEEditor.assemblyHandler.CreateInstanceFromString(assemblydef.dllname, assemblydef.type.ToString(), new object[] {DMEEditor });
            //  dynamic fc = Editor.assemblyHandler.CreateInstanceFromString(assemblydef.type.ToString(), new object[] { Editor, Vismanager, this });
            if (fc == null)
            {
                return;
            }

            Type t = ((IFunctionExtension)fc).GetType();
            AssemblyClassDefinition cls = DMEEditor.ConfigEditor.GlobalFunctions.Where(x => x.className == t.Name).FirstOrDefault();
          
            methodsClass = cls.Methods.Where(x => x.Caption == MethodName).FirstOrDefault();

            if (DMEEditor.Passedarguments == null)
            {
                DMEEditor.Passedarguments = new PassedArgs();
            }
           
            if (methodsClass != null)
            {
              
                method = methodsClass.Info;
                if (method.GetParameters().Length > 0)
                {
                    method.Invoke(fc, new object[] { DMEEditor.Passedarguments });
                }
                else
                    method.Invoke(fc, null);
            }
        }
        public static void RunFunction(IDMEEditor DMEEditor, IBranch br, ToolStripItem item)
        {
            if (br != null)
            {
                if (DMEEditor.Passedarguments == null)
                {
                    DMEEditor.Passedarguments = new PassedArgs();
                }

                AssemblyClassDefinition assemblydef = (AssemblyClassDefinition)item.Tag;
                dynamic fc = DMEEditor.assemblyHandler.CreateInstanceFromString(assemblydef.dllname, assemblydef.type.ToString(), new object[] { DMEEditor });
                Type t = ((IFunctionExtension)fc).GetType();
                //dynamic fc = Activator.CreateInstance(assemblydef.type, new object[] { Editor, Vismanager, this });
                AssemblyClassDefinition cls = DMEEditor.ConfigEditor.GlobalFunctions.Where(x => x.className == t.Name).FirstOrDefault();
                MethodInfo method = null;
                MethodsClass methodsClass;
               

                try
                {
                    if (br.BranchType != EnumPointType.Global)
                    {
                        methodsClass = cls.Methods.Where(x => x.Caption == item.Text).FirstOrDefault();
                    }
                    else
                    {
                        methodsClass = cls.Methods.Where(x => x.Caption == item.Text && x.PointType == br.BranchType).FirstOrDefault();
                    }

                }
                catch (Exception)
                {

                    methodsClass = null;
                }
                if (methodsClass != null)
                {
                   
                    method = methodsClass.Info;
                    if (method.GetParameters().Length > 0)
                    {
                        method.Invoke(fc, new object[] { DMEEditor.Passedarguments });
                    }
                    else
                        method.Invoke(fc, null);
                }
            }


        }
        public static IErrorsInfo RunMethod(IDMEEditor DMEEditor ,Object branch, string MethodName)
        {

            try
            {
                Type t = branch.GetType();
                AssemblyClassDefinition cls = DMEEditor.ConfigEditor.BranchesClasses.Where(x => x.className == t.Name).FirstOrDefault();
                MethodInfo method = null;
                MethodsClass methodsClass;
                try
                {
                    methodsClass = cls.Methods.Where(x => x.Caption == MethodName).FirstOrDefault();
                }
                catch (Exception)
                {

                    methodsClass = null;
                }
                if (methodsClass != null)
                {


                    method = methodsClass.Info;
                    if (method.GetParameters().Length > 0)
                    {
                        method.Invoke(branch, new object[] { DMEEditor.Passedarguments.Objects[0].obj });
                    }
                    else
                        method.Invoke(branch, null);


                    //  MiscFunctions.AddLogMessage("Success", "Running method", DateTime.Now, 0, null, Errors.Ok);
                }

            }
            catch (Exception ex)
            {
                string mes = "Could not Run Method " + MethodName;
                MiscFunctions.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
            };
            return DMEEditor.ErrorObject;
        }

        #endregion "Run Functions"

    }
}
