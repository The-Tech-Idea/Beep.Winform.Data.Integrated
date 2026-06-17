
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;
using TheTechIdea.Beep.ConfigUtil;

using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;

using TheTechIdea.Beep.Vis.Modules;
using MenuItem = TheTechIdea.Beep.Vis.Modules.MenuItem;

using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Controls.TextFields;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;
using TheTechIdea.Beep.Winform.Controls.Images;
using TheTechIdea.Beep.Shared;






namespace TheTechIdea.Beep.Winform.Controls.Helpers;
public static partial class ControlExtensions
{
    public delegate T ObjectActivator<T>(params object[] args);
    public static ObjectActivator<T> GetActivator<T>(ConstructorInfo ctor)
    {
        Type type = ctor.DeclaringType;
        ParameterInfo[] paramsInfo = ctor.GetParameters();

        //create a single param of type object[]
        ParameterExpression param =
            Expression.Parameter(typeof(object[]), "args");

        Expression[] argsExp =
            new Expression[paramsInfo.Length];

        //pick each arg from the params array 
        //and create a typed expression of them
        for (int i = 0; i < paramsInfo.Length; i++)
        {
            Expression index = Expression.Constant(i);
            Type paramType = paramsInfo[i].ParameterType;

            Expression paramAccessorExp =
                Expression.ArrayIndex(param, index);

            Expression paramCastExp =
                Expression.Convert(paramAccessorExp, paramType);

            argsExp[i] = paramCastExp;
        }

        //make a NewExpression that calls the
        //ctor with the args we just created
        NewExpression newExp = Expression.New(ctor, argsExp);

        //create a lambda with the New
        //Expression as body and our param object[] as arg
        LambdaExpression lambda =
            Expression.Lambda(typeof(ObjectActivator<T>), newExp, param);

        //compile it
        ObjectActivator<T> compiled = (ObjectActivator<T>)lambda.Compile();
        return compiled;
    }
    #region "BeepTree Extensions"
    public static BindingList<SimpleItem> GetBranchs(this ITree tree,IDMEEditor DMEEditor)
    {
        BindingList<SimpleItem> simpleItems = new BindingList<SimpleItem>();
        var res=  tree.CreateTreeTuple(DMEEditor);
        tree.GenerBranchs = res.Item2;
        tree.Branches = res.Item1;
        foreach (var item in res.Item2)
        {
            SimpleItem node = new SimpleItem();
            IBranch br = item.Item1;
            node.Text = br.BranchText;
            node.Name = br.Name;
            node.ID = br.ID; 
            node.ImagePath = ImageListHelper.GetImagePathFromName(br.IconImageName);
            node.GuidId = br.GuidID;
            node.PointType = br.BranchType;
            node.ParentID = 0;
            node.IsVisible = true;
            node.Guid = Guid.TryParse(br.GuidID.ToString(), out Guid guid) ? guid : Guid.Empty;
            node.Children = new BindingList<SimpleItem>();
            node.Children = GetChildBranch(tree, br);
            DynamicMenuManager.CreateMenuMethods(tree.DMEEditor,br);
            if (br.ObjectType != null && br.BranchClass != null)
            {
                DynamicMenuManager.CreateGlobalMenu( br);
            }
            simpleItems.Add(node);
        }
        foreach (var item in tree.Branches.Where(p=>p.BranchType== EnumPointType.Root))
        {
            SimpleItem node = new SimpleItem();
            IBranch br = item;
            node.Text = br.BranchText;
            node.Name = br.Name;
            node.ID = br.ID;
            node.ImagePath = ImageListHelper.GetImagePathFromName(br.IconImageName);
            node.GuidId = br.GuidID;
            node.ParentID = 0;
            node.IsVisible = true;
            node.Guid = Guid.TryParse(br.GuidID.ToString(), out Guid guid) ? guid : Guid.Empty;
            node.Children = new BindingList<SimpleItem>();
            node.Children = GetChildBranch(tree, br);
            DynamicMenuManager.CreateMenuMethods(tree.DMEEditor, br);
            if (br.ObjectType != null && br.BranchClass != null)
            {
                DynamicMenuManager.CreateGlobalMenu( br);
            }
            simpleItems.Add(node);
        }
        return simpleItems;
    }
    public static BindingList<SimpleItem> GetBranchs(this ITree tree, Tuple<List<IBranch>, List<Tuple<IBranch, string>>> res)
    {
        BindingList<SimpleItem> simpleItems = new BindingList<SimpleItem>();
        foreach (var item in res.Item2)
        {
            SimpleItem node = new SimpleItem();
            IBranch br = item.Item1;
            node.Text = br.BranchText;
            node.Name = br.Name;
            node.ID = br.ID;
            node.ImagePath = ImageListHelper.GetImagePathFromName(br.IconImageName);
            node.GuidId = br.GuidID;
            node.ParentID = 0;
            node.ObjectType = br.ObjectType;
            node.BranchClass = br.BranchClass;
            node.Guid = Guid.TryParse(br.GuidID.ToString(), out Guid guid) ? guid : Guid.Empty;
            node.PointType = br.BranchType;
            node.AssemblyClassDefinitionID = br.MiscStringID; ;
            node.Children = new BindingList<SimpleItem>();
            node.IsVisible = true;
            node.Children = GetChildBranch(tree, br);
            DynamicMenuManager.CreateMenuMethods(tree.DMEEditor,br);
            if (br.ObjectType != null && br.BranchClass != null)
            {
                DynamicMenuManager.CreateGlobalMenu(    br);
            }
            simpleItems.Add(node);
        }
        foreach (var item in tree.Branches.Where(p => p.BranchType == EnumPointType.Root && p.ParentBranch==null))
        {
            SimpleItem node = new SimpleItem();
            IBranch br = item;
            node.Text = br.BranchText;
            node.Name = br.Name;
            node.ID = br.ID;
            node.ImagePath = ImageListHelper.GetImagePathFromName(br.IconImageName);
            node.GuidId = br.GuidID;
            node.PointType = br.BranchType;
            node.ParentID = 0;
            node.IsVisible = true;
            node.Guid = Guid.TryParse(br.GuidID.ToString(), out Guid guid) ? guid : Guid.Empty;
            node.Children = new BindingList<SimpleItem>();
            node.Children = GetChildBranch(tree, br);
            DynamicMenuManager.CreateMenuMethods(tree.DMEEditor, br);
            if (br.ObjectType != null && br.BranchClass != null)
            {
                DynamicMenuManager.CreateGlobalMenu(br);
            }
            simpleItems.Add(node);
        }
        
        return simpleItems;
    }
    public static BindingList<SimpleItem> GetChildBranch(this ITree tree,IBranch br)
    {
        BindingList<SimpleItem> Childitems = new BindingList<SimpleItem>();
        if (br.ChildBranchs == null) return Childitems;
        foreach (var item1 in br.ChildBranchs)
        {
            SimpleItem node1 = new SimpleItem();
            node1.Text = item1.BranchText;
            node1.Name = item1.Name;
            node1.ID = item1.ID;
            node1.ImagePath = ImageListHelper.GetImagePathFromName(item1.IconImageName);
            node1.GuidId = item1.GuidID;
            node1.Guid = Guid.TryParse(item1.GuidID.ToString(), out Guid guid) ? guid : Guid.Empty;
            node1.ObjectType = item1.ObjectType;
            node1.BranchClass = item1.BranchClass;
            node1.PointType = item1.BranchType;
            node1.AssemblyClassDefinitionID = item1.MiscStringID; ;
            node1.ParentID = br.ID;
            node1.IsVisible = true;
            node1.Children = new BindingList<SimpleItem>();
            node1.Children = GetChildBranch(tree, item1);
            DynamicMenuManager.CreateMenuMethods(tree.DMEEditor, item1);
            if (br.ObjectType != null && br.BranchClass != null)
            {
                DynamicMenuManager.CreateGlobalMenu(    item1);
            }
            Childitems.Add(node1);
        }
        return Childitems;
    }
    public static BindingList<SimpleItem> GetSimpleItemsFromExecuteCreateChildsMethods(this ITree tree, IBranch br)
    {
        BindingList<SimpleItem> Childitems = new BindingList<SimpleItem>();
        IErrorsInfo retval = new ErrorsInfo();
        try
        { 
            br.CreateChildNodes();
            Childitems= GetChildBranch(tree, br);
            retval.Flag = Errors.Ok;
            retval.Message = "Childs Created";
        }
        catch (Exception ex)
        {
            retval.Flag = Errors.Failed;
            retval.Message = ex.Message;
        }
        return Childitems;
    }
    public static IErrorsInfo CreateFunctionExtensions(this ITree tree, MethodsClass item)
    {
        ContextMenuStrip nodemenu = new ContextMenuStrip();
        try
        {

            ToolStripItem st = nodemenu.Items.Add(item.Caption);
            foreach (IBranch br in tree.Branches)
            {
                if (br.BranchType == item.PointType)
                {
                    nodemenu.Name = br.ToString();
                    if (item.iconimage != null)
                    {
                        //st.ImageIndex = VisManager.visHelper.GetImageIndex(item.iconimage);
                    }
                    //nodemenu.ItemClicked += Nodemenu_ItemClicked;
                    nodemenu.Tag = br;
                }
            }
        }
        catch (Exception ex)
        {
            string mes = $"Could not add method from Extension {item.Name} to menu ";
            MiscFunctions.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
        }
        ;
        return tree.DMEEditor.ErrorObject;

    }
    public static BindingList<SimpleItem> AddBranchToTree(this ITree tree, SimpleItem parent,SimpleItem child,IBranch br)
    {

        DynamicMenuManager.CreateMenuMethods(tree.DMEEditor, br);
        DynamicMenuManager.CreateGlobalMenu( br);
        parent.Children.Add(child);
        return parent.Children;
    }
    public static BindingList<SimpleItem> AddBranchToTree(this ITree tree, SimpleItem parent, IBranch br)
    {
        br.CreateChildNodes();
        SimpleItem node = new SimpleItem();
        node.Text = br.BranchText;
        node.Name = br.Name;
        node.ID = br.ID;
        node.ImagePath = ImageListHelper.GetImagePathFromName(br.IconImageName);
        node.GuidId = br.GuidID;
        node.ParentID = parent.ID;
        node.ObjectType = br.ObjectType;
        node.BranchClass = br.BranchClass;
        node.PointType = br.BranchType;
        node.AssemblyClassDefinitionID = br.MiscStringID; ;
        node.Children = new BindingList<SimpleItem>();
        node.Children = GetChildBranch(tree, br);
        DynamicMenuManager.CreateMenuMethods(tree.DMEEditor, br);
        if (br.ObjectType != null && br.BranchClass != null)
        {
            DynamicMenuManager.CreateGlobalMenu( br);
        }
        parent.Children.Add(node);
        return parent.Children;
    }
    public static Tuple <List<IBranch>,List<Tuple<IBranch, string>>> CreateTreeTuple(this ITree tree, IDMEEditor DMEEditor)
    {
        var Branches = new List<IBranch>();
        var GenerBranchs = new List<Tuple<IBranch, string>>();
        
        string packagename = "";
        try
        {
           
            int SeqID = 0;
            //tree. = new TreeNodeDragandDropHandler(Editor, this);
            //tree.Treebranchhandler = new TreeBranchHandler(Editor, this);
          
            IBranch Genrebr = null;
            // AssemblyClassDefinition GenreBrAssembly = Editor.ConfigEditor.BranchesClasses.Where(p => p.classProperties != null && p.VisSchema != null && p.VisSchema.BranchType == EnumPointType.Genre).FirstOrDefault()!;
            foreach (AssemblyClassDefinition GenreBrAssembly in DMEEditor.ConfigEditor.BranchesClasses.Where(p => p.classProperties != null && p.VisSchema != null && p.VisSchema.BranchType == EnumPointType.Genre).OrderBy(x => x.Order))
            {
                SeqID++;
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
                                int id = SeqID;
                                Genrebr.Name = GenreBrAssembly.PackageName;
                                packagename = GenreBrAssembly.PackageName;
                                Genrebr.ID = id;
                                Genrebr.BranchID = id;
                                Genrebr.BranchText = GenreBrAssembly.classProperties.Caption;
                                Genrebr.DMEEditor = DMEEditor;
                               // CreateNode(id, Genrebr);
                                //else CreateNode(id, Genrebr);
                                Genrebr.MiscStringID = GenreBrAssembly.GuidID;
                           
                                try
                                {
                                    Genrebr.SetConfig(tree, tree.DMEEditor, Genrebr.ParentBranch, Genrebr.BranchText, Genrebr.ID, Genrebr.BranchType, null);
                                }
                                catch (Exception ex)
                                {

                                }
                              //  Genrebr.CreateChildNodes();
                                GenerBranchs.Add(new Tuple<IBranch, string>(Genrebr, GenreBrAssembly.classProperties.menu));
                            }
                        }
                        catch (Exception ex)
                        {
                            MiscFunctions.AddLogMessage("Error", $"Creating StandardTree Root Node {GenreBrAssembly.PackageName} {ex.Message} ", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }

            foreach (AssemblyClassDefinition cls in DMEEditor.ConfigEditor.BranchesClasses.Where(p => p.classProperties != null).OrderBy(x => x.Order))
            {
                Type adc = DMEEditor.assemblyHandler.GetType(cls.PackageName);
                ConstructorInfo ctor = adc.GetConstructors().Where(o => o.GetParameters().Length == 0).FirstOrDefault()!;
                SeqID++;
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
                            br.BranchText = cls.classProperties.Caption;
                            br.DMEEditor = DMEEditor;
                            br.MiscStringID = cls.GuidID;
                            if (cls.classProperties.ObjectType != null)
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
                                    try
                                    {
                                        br.ParentBranch = Genrebr;
                                        br.SetConfig(tree, tree.DMEEditor, Genrebr, br.BranchText, br.ID, br.BranchType, null);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                    Genrebr.ChildBranchs.Add(br);
                                }
                                else
                                {
                                    br.ParentBranch = null;
                                    br.ParentBranchID = -1;
                                    br.ParentGuidID = string.Empty;
                                    try
                                    {
                                        br.ParentBranch = Genrebr;
                                        br.SetConfig(tree, tree.DMEEditor, br.ParentBranch, br.BranchText, br.ID, br.BranchType, null);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }

                            }
                          //  br.CreateChildNodes();
                            Branches.Add(br);
                        }
                    }
                    catch (Exception ex)
                    {
                        MiscFunctions.AddLogMessage("Error", $"Creating StandardTree Root Node {cls.PackageName} {ex.Message} ", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            DMEEditor.ErrorObject.Ex = ex;
            DMEEditor.ErrorObject.Flag = Errors.Failed;
            MiscFunctions.AddLogMessage("Error", $"Creating StandardTree Root Node {packagename} - {ex.Message} ", DateTime.Now, 0, null, Errors.Failed);

        };

        Tuple<List<IBranch>, List<Tuple<IBranch, string>>> retval = new(Branches, GenerBranchs);
        return  retval;
    }
    #endregion "BeepTree Extensions"
    #region "ITree Extensions"
    
    private static bool IsMethodApplicabletoNode(AssemblyClassDefinition cls, IBranch br)
    {
        if (cls.classProperties == null)
        {
            return true;
        }
        if (cls.classProperties.ObjectType != null)
        {
            //if (!cls.classProperties.ObjectType.Equals(br.ObjectType, StringComparison.InvariantCultureIgnoreCase))
            //{
            //    return false ;
            //}
        }
        return true;
    }
    //public static IErrorsInfo RunMethodFromObject(this ITree tree, object branch, string MethodName)
    //{
    //    try
    //    {
    //        Type t = branch.GetType();
    //        AssemblyClassDefinition cls = tree.Editor.ConfigEditor.BranchesClasses.Where(x => x.className == t.Name).FirstOrDefault();
    //        MethodInfo method = null;
    //        MethodsClass methodsClass;
    //        try
    //        {
    //            methodsClass = cls.Methods.Where(x => x.Caption == MethodName).FirstOrDefault();
    //        }
    //        catch (Exception)
    //        {
    //            methodsClass = null;
    //        }
    //        if (methodsClass != null)
    //        {
    //            if (!IsMethodApplicabletoNode(cls, (IBranch)branch)) return tree.Editor.ErrorObject;
    //            //PassedArgs args = new PassedArgs();
    //            //args.ObjectName = MethodName;
    //            //args.ObjectType = methodsClass.ObjectType;
    //            //args.Cancel = false;
    //            //PreCallModule?.Invoke(this, args);
    //            //if (args.Cancel)
    //            //{
    //            //    MiscFunctions.AddLogMessage("Beep", $"You dont have Access Privilige on {MethodName}", DateTime.Now, 0, MethodName, Errors.Failed);
    //            //    ErrorsandMesseges.Flag = Errors.Failed;
    //            //    ErrorsandMesseges.Message = $"Function Access Denied";
    //            //    return ErrorsandMesseges;
    //            //}

    //            method = methodsClass.Info;
    //            if (method.GetParameters().Length > 0)
    //            {
    //                method.Invoke(branch, new object[] { tree.Editor.Passedarguments.Objects[0].obj });
    //            }
    //            else
    //                method.Invoke(branch, null);


    //            //  MiscFunctions.AddLogMessage("Success", "Running method", DateTime.Now, 0, null, Errors.OK);
    //        }

    //    }
    //    catch (Exception ex)
    //    {
    //        string mes = "Could not Run Method " + MethodName;
    //        MiscFunctions.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
    //    };
    //    return tree.Editor.ErrorObject;
    //}
    public static List<IBranch> CreateTree(this ITree tree)
    {
        string packagename = "";
        try
        {
            tree.SeqID = 0;
            //tree. = new TreeNodeDragandDropHandler(Editor, this);
            //tree.Treebranchhandler = new TreeBranchHandler(Editor, this);
            tree.Branches = new List<IBranch>();
            tree.GenerBranchs = new List<Tuple<IBranch, string>>();
            IBranch Genrebr = null;
            foreach (AssemblyClassDefinition GenreBrAssembly in tree.DMEEditor.ConfigEditor.BranchesClasses.Where(p => p.classProperties != null && p.VisSchema != null && p.VisSchema.BranchType == EnumPointType.Genre).OrderBy(x => x.Order))
            {
                tree.SeqID++;
                if (GenreBrAssembly != null)
                {
                    Type adc = tree.DMEEditor.assemblyHandler.GetType(GenreBrAssembly.PackageName);
                    ConstructorInfo ctor = adc.GetConstructors().Where(o => o.GetParameters().Length == 0).FirstOrDefault()!;
                    if (ctor != null)
                    {
                        ObjectActivator<IBranch> createdActivator = GetActivator<IBranch>(ctor);
                        try
                        {
                            Genrebr = createdActivator();
                            if (Genrebr.BranchType == EnumPointType.Genre)
                            {
                                int id = tree.SeqID;
                                Genrebr.Name = GenreBrAssembly.PackageName;
                                packagename = GenreBrAssembly.PackageName;
                                Genrebr.ID = id;
                                Genrebr.BranchID = id;
                                Genrebr.BranchText = GenreBrAssembly.classProperties.Caption;
                                Genrebr.DMEEditor = tree.DMEEditor;
                                tree.CreateNode(id, Genrebr);
                                //else CreateNode(id, Genrebr);
                                Genrebr.MiscStringID = GenreBrAssembly.GuidID;
                                tree.GenerBranchs.Add(new Tuple<IBranch, string>(Genrebr, GenreBrAssembly.classProperties.menu));
                            }
                        }
                        catch (Exception ex)
                        {
                            MiscFunctions.AddLogMessage("Error", $"Creating StandardTree Root Node {GenreBrAssembly.PackageName} {ex.Message} ", DateTime.Now, 0, null, Errors.Failed);
                        }
                    }
                }
            }

            foreach (AssemblyClassDefinition cls in tree.DMEEditor.ConfigEditor.BranchesClasses.Where(p => p.classProperties != null).OrderBy(x => x.Order))
            {
                Type adc = tree.DMEEditor.assemblyHandler.GetType(cls.PackageName);
                ConstructorInfo ctor = adc.GetConstructors().Where(o => o.GetParameters().Length == 0).FirstOrDefault()!;
                tree.SeqID++;
                if (ctor != null)
                {
                    ObjectActivator<IBranch> createdActivator = GetActivator<IBranch>(ctor);
                    try
                    {
                        IBranch br = createdActivator();
                        if (br.BranchType == EnumPointType.Root)
                        {
                            int id = tree.SeqID;
                            br.Name = cls.PackageName;
                            packagename = cls.PackageName;
                            br.ID = id;
                            br.BranchID = id;
                            br.BranchText = cls.classProperties.Caption;
                            br.DMEEditor = tree.DMEEditor;
                            Genrebr.MiscStringID = cls.GuidID;
                            if (cls.classProperties.ObjectType != null)
                            {

                                var tr = tree.GenerBranchs.FirstOrDefault(p => p.Item2.Equals(cls.classProperties.menu, StringComparison.OrdinalIgnoreCase));
                                if (tr != null)
                                {
                                    Genrebr = tr.Item1;
                                }
                                else
                                    Genrebr = null;
                                if (Genrebr != null)
                                {
                                    Genrebr.ChildBranchs.Add(br);
                                    if (br.ObjectType != null && br.BranchClass != null)
                                    {
                                        //// Console.WriteLine($"{CreateNode}- br.BranchText");
                                        tree.CreateMenuMethods(br);
                                        tree.CreateGlobalMenu(br);

                                    }
                                    br.CreateChildNodes();

                                }
                                else
                                {
                                    br.ParentBranch = null;
                                    br.ParentBranchID = -1;
                                    br.ParentGuidID = string.Empty;
                                    tree.CreateNode(id, br);
                                    br.CreateChildNodes();
                                }

                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        MiscFunctions.AddLogMessage("Error", $"Creating StandardTree Root Node {cls.PackageName} {ex.Message} ", DateTime.Now, 0, null, Errors.Failed);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            tree.DMEEditor.ErrorObject.Ex = ex;
            tree.DMEEditor.ErrorObject.Flag = Errors.Failed;
            MiscFunctions.AddLogMessage("Error", $"Creating StandardTree Root Node {packagename} - {ex.Message} ", DateTime.Now, 0, null, Errors.Failed);

        };


        return tree.Branches;
    }
    public static SimpleItem CreateNodeAndChilds(this ITree tree,  int id, IBranch br)
    {
       
        try
        {
            SimpleItem node = new SimpleItem();
            node.Text = br.BranchText;
            node.Name = br.Name;
            node.ID = id;
            node.BranchClass = br.BranchClass;
            node.BranchName = br.Name;
            node.PointType = br.BranchType;
            node.ObjectType = br.ObjectType;
            node.AssemblyClassDefinitionID = br.MiscStringID;
            node.ClassDefinitionID = br.MiscStringID;
            node.ImagePath = ImageListHelper.GetImagePathFromName(br.IconImageName);
            node.GuidId = br.GuidID;
            node.ParentID = 0;
            node.Children = new BindingList<SimpleItem>();
            node.Children = GetChildBranch(tree, br);
            DynamicMenuManager.CreateMenuMethods(tree.DMEEditor, br);
            if (br.ObjectType != null && br.BranchClass != null)
            {
                DynamicMenuManager.CreateGlobalMenu( br);
            }
            br.DMEEditor = tree.DMEEditor;
            if (!tree.DMEEditor.ConfigEditor.objectTypes.Any(i => i.ObjectType == br.BranchClass && i.ObjectName == br.BranchType.ToString() + "_" + br.BranchClass))
            {
                tree.DMEEditor.ConfigEditor.objectTypes.Add(new TheTechIdea.Beep.Workflow.ObjectTypes { ObjectType = br.BranchClass, ObjectName = br.BranchType.ToString() + "_" + br.BranchClass });
            }
            try
            {
                br.SetConfig(tree, tree.DMEEditor, br.ParentBranch, br.BranchText, br.ID, br.BranchType, null);
            }
            catch (Exception ex)
            {

            }
            tree.Branches.Add(br);
            br.CreateChildNodes();
            return node;
        }
        catch (Exception ex)
        {
            tree.DMEEditor.ErrorObject.Ex = ex;
            tree.DMEEditor.ErrorObject.Flag = Errors.Failed;
            MiscFunctions.AddLogMessage("Error", $"Creating Branch Node {br.BranchText} - {ex.Message} ", DateTime.Now, 0, null, Errors.Failed);
            return null;
        }


    }
    public static SimpleItem CreateNode(this ITree tree, int id, IBranch br)
    {
      
        try
        {
            SimpleItem node = new SimpleItem();
            node.Text = br.BranchText;
            node.Name = br.Name;
            node.ID = id;
            node.BranchClass = br.BranchClass;
            node.BranchName = br.Name;
            node.ObjectType = br.ObjectType;
            node.PointType = br.BranchType;
            node.BranchType = br.BranchType;
            node.AssemblyClassDefinitionID = br.MiscStringID;
            node.ClassDefinitionID = br.MiscStringID;
            node.ImagePath = ImageListHelper.GetImagePathFromName(br.IconImageName);
            node.GuidId = br.GuidID;
            node.Guid = Guid.TryParse(br.GuidID.ToString(), out Guid guid) ? guid : Guid.Empty;
            node.ContainerGuidID = br.GuidID;
            node.ParentID = 0;
            node.IsVisible = true;
            node.Children = new BindingList<SimpleItem>();
            node.Children = GetChildBranch(tree, br);
            DynamicMenuManager.CreateMenuMethods(tree.DMEEditor, br);
            if (br.ObjectType != null && br.BranchClass != null)
            {
                DynamicMenuManager.CreateGlobalMenu(br);
            }
            br.DMEEditor = tree.DMEEditor;
            if (!tree.DMEEditor.ConfigEditor.objectTypes.Any(i => i.ObjectType == br.BranchClass && i.ObjectName == br.BranchType.ToString() + "_" + br.BranchClass))
            {
                tree.DMEEditor.ConfigEditor.objectTypes.Add(new TheTechIdea.Beep.Workflow.ObjectTypes { ObjectType = br.BranchClass, ObjectName = br.BranchType.ToString() + "_" + br.BranchClass });
            }
            try
            {
                br.SetConfig(tree, tree.DMEEditor, br.ParentBranch, br.BranchText, br.ID, br.BranchType, null);
            }
            catch (Exception ex)
            {

            }
     //       tree.Branches.Add(br);
           // br.CreateChildNodes();
            return node;
        }
        catch (Exception ex)
        {
            tree.DMEEditor.ErrorObject.Ex = ex;
            tree.DMEEditor.ErrorObject.Flag = Errors.Failed;
            MiscFunctions.AddLogMessage("Error", $"Creating Branch Node {br.BranchText} - {ex.Message} ", DateTime.Now, 0, null, Errors.Failed);
            return null;
        }


    }
    public static bool IsMenuCreated(this ITree tree, IBranch br)
    {
        if (br.ObjectType != null)
        {
            return tree.Menus.Where(p => p.ObjectType != null && p.BranchClass.Equals(br.BranchClass, StringComparison.InvariantCultureIgnoreCase)
            && p.ObjectType.Equals(br.ObjectType, StringComparison.InvariantCultureIgnoreCase)
            && p.PointType == br.BranchType).Any();
        }
        return
            false;
    }
    public static void UpdateMenuItems(this ITree tree, MenuList menu, MenuItem menuItem)
    {
        int menuidx = tree.Menus.FindIndex(p => p.ObjectType != null && p.BranchClass.Equals(menu.BranchClass, StringComparison.InvariantCultureIgnoreCase)
                                  && p.ObjectType.Equals(menu.ObjectType, StringComparison.InvariantCultureIgnoreCase)
                                                                           && p.PointType == menu.PointType);
        if (menuidx > -1)
        {
            int menuitesidx = tree.Menus[menuidx].Items.FindIndex(p => p.Name.Equals(menuItem.Name, StringComparison.InvariantCultureIgnoreCase));
            if (menuitesidx > -1)
            {
                tree.Menus[menuidx].Items[menuitesidx] = menuItem;
            }

        }
    }
    public static void UpdateMenuList(this ITree tree, MenuList menu)
    {
        int menuidx = tree.Menus.FindIndex(p => p.ObjectType != null && p.BranchClass.Equals(menu.BranchClass, StringComparison.InvariantCultureIgnoreCase)
                       && p.ObjectType.Equals(menu.ObjectType, StringComparison.InvariantCultureIgnoreCase)
                                      && p.PointType == menu.PointType);

        if (menuidx > -1)
        {
            tree.Menus[menuidx] = menu;
        }
    }
    public static MenuList GetMenuList(this ITree tree, IBranch br)
    {

        return tree.Menus.Where(p => p.ObjectType != null && p.BranchClass.Equals(br.BranchClass, StringComparison.InvariantCultureIgnoreCase)
            && p.ObjectType.Equals(br.ObjectType, StringComparison.InvariantCultureIgnoreCase)
            && p.PointType == br.BranchType).FirstOrDefault();
    }
    public static List<MenuItem> GetMenuItemsList(this ITree tree, IBranch br)
    {
        List<MenuItem> retval = new List<MenuItem>();
        var ls = tree.Menus.Where(p => p.ObjectType != null && p.BranchClass.Equals(br.BranchClass, StringComparison.InvariantCultureIgnoreCase)
            && p.ObjectType.Equals(br.ObjectType, StringComparison.InvariantCultureIgnoreCase)
            && p.PointType == br.BranchType).FirstOrDefault();
        if (ls != null)
        {
            retval = ls.Items;
        }

        return retval;
    }
    public static List<SimpleItem> GetMenuItemsList(this ITree tree, string brGuidID)
    {
        IBranch br = tree.Branches.Where(p => p.GuidID == brGuidID).FirstOrDefault();
        List<SimpleItem> retval = new List<SimpleItem>();
        var ls = tree.Menus.Where(p => p.ObjectType != null && p.BranchClass.Equals(br.BranchClass, StringComparison.InvariantCultureIgnoreCase)
            && p.ObjectType.Equals(br.ObjectType, StringComparison.InvariantCultureIgnoreCase)
            && p.PointType == br.BranchType).FirstOrDefault();
       
        foreach (var item1 in ls.Items)
        {
            SimpleItem listitem = new SimpleItem { Text = item1.Text, ImagePath = item1.imagename, AssemblyClassDefinitionID = item1.ClassDefinitionID, GuidId = item1.ID };
            retval.Add(listitem);
        }
        return retval;
    }
    public static IErrorsInfo CreateGlobalMenu(this ITree tree,IBranch br)
    {
        try
        {
            MenuList menuList = new MenuList();
            if (!tree.IsMenuCreated(br))
            {
                menuList = new MenuList(br.ObjectType, br.BranchClass, br.BranchType);
                menuList.branchname = br.BranchText;
                tree.Menus.Add(menuList);
                menuList.ObjectType = br.ObjectType;
                menuList.BranchClass = br.BranchClass;
            }
            else
                menuList = tree.GetMenuList(br);
            List<AssemblyClassDefinition> extentions = tree.DMEEditor.ConfigEditor.GlobalFunctions.Where(o => o.classProperties != null && o.classProperties.ObjectType != null && o.classProperties.ObjectType.Equals(br.ObjectType, StringComparison.InvariantCultureIgnoreCase)).OrderBy(p => p.Order).ToList(); //&&  o.classProperties.menu.Equals(br.BranchClass, StringComparison.InvariantCultureIgnoreCase)
            foreach (AssemblyClassDefinition cls in extentions)
            {
                if (!menuList.classDefinitions.Any(p => p.PackageName.Equals(cls.PackageName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    menuList.classDefinitions.Add(cls);
                    foreach (var item in cls.Methods)
                    {
                        if (string.IsNullOrEmpty(item.ClassType))
                        {
                            if (item.PointType == br.BranchType)
                            {
                                MenuItem mi = new MenuItem();
                                mi.Name = item.Caption;
                                mi.MethodName = item.Caption;
                                mi.Text = item.Caption;
                                mi.ObjectType = item.ObjectType;
                                mi.BranchClass = item.ClassType;
                                mi.PointType = item.PointType;
                                mi.ClassDefinition = cls;
                                mi.MethodAttribute = item.CommandAttr;
                                mi.imagename = item.iconimage;
                                menuList.Items.Add(mi);
                            }
                        }
                        else
                        {
                            if ((item.PointType == br.BranchType) && (br.BranchClass.Equals(item.ClassType, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                MenuItem mi = new MenuItem();
                                mi.Name = item.Caption;
                                mi.MethodName = item.Name;
                                mi.Text = item.Caption;
                                mi.ObjectType = item.ObjectType;
                                mi.BranchClass = item.ClassType;
                                mi.PointType = item.PointType;
                                mi.ClassDefinition = cls;
                                mi.imagename = item.iconimage;
                                mi.MethodAttribute = item.CommandAttr;
                                menuList.Items.Add(mi);
                            }
                        }
                    }
                }
            }
            return tree.DMEEditor.ErrorObject;
        }
        catch (Exception ex)
        {
            return tree.DMEEditor.ErrorObject;
        }
    }
    public static List<MenuItem> CreateMenuMethods(this ITree tree, IBranch branch)
    {
        AssemblyClassDefinition cls = tree.DMEEditor.ConfigEditor.BranchesClasses.Where(x => x.PackageName == branch.ToString()).FirstOrDefault();
        MenuList menuList = new MenuList();
        if (!tree.IsMenuCreated(branch))
        {
            menuList = new MenuList(branch.ObjectType, branch.BranchClass, branch.BranchType);
            menuList.branchname = branch.BranchText;
            tree.Menus.Add(menuList);
            menuList.ObjectType = branch.ObjectType;
            menuList.BranchClass = branch.BranchClass;

            menuList.Items = new List<MenuItem>();
        }
        else
            menuList = tree.GetMenuList(branch);
        try
        {

            if (!menuList.classDefinitions.Any(p => p.PackageName.Equals(cls.PackageName, StringComparison.InvariantCultureIgnoreCase)))
            {
                menuList.classDefinitions.Add(cls);
                foreach (var item in cls.Methods.Where(y => y.Hidden == false))
                {
                    MenuItem mi = new MenuItem();
                    mi.Name = item.Caption;
                    mi.MethodName = item.Caption;
                    mi.Text = item.Caption;
                    mi.ObjectType = item.ObjectType;
                    mi.BranchClass = item.ClassType;
                    mi.PointType = item.PointType;
                    mi.ClassDefinition = cls;
                    mi.Category = item.Category;
                    mi.imagename = item.iconimage;
                    mi.MethodAttribute = item.CommandAttr;

                    menuList.Items.Add(mi);
                }
            }
        }
        catch (Exception ex)
        {
            string mes = "Could not add method to menu " + branch.BranchText;
            MiscFunctions.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
        };
        return menuList.Items;
    }
    public static void Nodemenu_ItemClicked(this ITree tree, SimpleItem item,  string MethodName)
    {
        AssemblyClassDefinition cls = AssemblyClassDefinitionManager.GetAssemblyBranchsClassDefinitionByGuid(tree.DMEEditor, item.AssemblyClassDefinitionID);
        IBranch br = tree.Branches.Where(p => p.GuidID == item.GuidId).FirstOrDefault();
        if (cls != null)
        {
            if (!IsMethodApplicabletoNode(cls, br)) return;
            if (cls.componentType == "IFunctionExtension")
            {
                HandlersFactory.RunMethodFromObjectHandler(item,MethodName);

            }
            else
            {

                HandlersFactory.RunMethodFromExtensionWithTreeHandler(br, item.Text);
            };

        }
    }
    public static Tuple<MenuList, bool> Nodemenu_MouseClick(this ITree tree, IBranch br, BeepMouseEventArgs e)
    {

        MenuList menuList = null;
        bool runmethod = false;
        if (br != null)
        {
            string clicks = "";
            if (e.Button == BeepMouseEventArgs.BeepMouseButtons.Right)
            {
                if (tree.IsMenuCreated(br))
                {
                    menuList = tree.GetMenuList(br);

                }
            }
            else
            {
                switch (e.Clicks)
                {
                    case 1:
                        clicks = "SingleClick";
                        break;
                    case 2:
                        clicks = "DoubleClick";
                        break;

                    default:
                        break;
                }
                AssemblyClassDefinition cls = tree.DMEEditor.ConfigEditor.BranchesClasses.Where(x => x.PackageName == br.Name && x.Methods.Where(y => y.DoubleClick == true || y.Click == true).Any()).FirstOrDefault();
                if (cls != null)
                {
                    if (!IsMethodApplicabletoNode(cls, br)) runmethod = true;
                    tree.RunMethodFromBranch(br, clicks);

                }
            }

        }
        return new Tuple<MenuList, bool>(menuList, runmethod);
    }
    public static IErrorsInfo RunMethodFromBranch(this ITree tree, object branch, string MethodName)
    {

        try
        {
            Type t = branch.GetType();
            AssemblyClassDefinition cls = tree.DMEEditor.ConfigEditor.BranchesClasses.Where(x => x.className == t.Name).FirstOrDefault();
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

                if (!IsMethodApplicabletoNode(cls, (IBranch)branch)) return tree.DMEEditor.ErrorObject;
                //PassedArgs args = new PassedArgs();
                //args.ObjectName = MethodName;
                //args.ObjectType = methodsClass.ObjectType;
                //args.Cancel = false;
                //PreCallModule?.Invoke(this, args);
                //if (args.Cancel)
                //{
                //    MiscFunctions.AddLogMessage("Beep", $"You dont have Access Privilige on {MethodName}", DateTime.Now, 0, MethodName, Errors.Failed);
                //    ErrorsandMesseges.Flag = Errors.Failed;
                //    ErrorsandMesseges.Message = $"Function Access Denied";
                //    return ErrorsandMesseges;
                //}

                method = methodsClass.Info;
                if (method.GetParameters().Length > 0)
                {
                    method.Invoke(branch, new object[] { tree.DMEEditor.Passedarguments.Objects[0].obj });
                }
                else
                    method.Invoke(branch, null);


                //  MiscFunctions.AddLogMessage("Success", "Running method", DateTime.Now, 0, null, Errors.OK);
            }

        }
        catch (Exception ex)
        {
            string mes = "Could not Run Method " + MethodName;
            MiscFunctions.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
        };
        return tree.DMEEditor.ErrorObject;
    }
    public static ContextMenuStrip CreateContextMenu(this ITree tree, IBranch br)
    {
        ContextMenuStrip nodemenu = new ContextMenuStrip();
        try
        {
            if (br != null)
            {
                if (tree.IsMenuCreated(br))
                {
                    MenuList menuList = tree.GetMenuList(br);
                    foreach (MenuItem item in menuList.Items)
                    {
                        ToolStripItem st = nodemenu.Items.Add(item.Text);
                        st.Tag = item;
                       // st.Click += tree.Nodemenu_ItemClicked;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            string mes = $"Could not add method from Extension {br.Name} to menu ";
            MiscFunctions.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
        };
        return nodemenu;
    }
    #endregion "ITree Extensions"
    public static Type GetDefaultControlType(DbFieldCategory category)
    {
        switch (category)
        {
            case DbFieldCategory.String:
                return typeof(BeepTextBox);
            case DbFieldCategory.Numeric:
                return typeof(BeepTextBox);
            case DbFieldCategory.Date:
                return typeof(BeepDatePicker);
            case DbFieldCategory.Boolean:
                return typeof(BeepCheckBoxBool);
            case DbFieldCategory.Binary:
                return typeof(BeepImage);
            case DbFieldCategory.Guid:
                return typeof(BeepTextBox);
            case DbFieldCategory.Json:
                return typeof(BeepTextBox);
            case DbFieldCategory.Xml:
                return typeof(BeepTextBox);
            case DbFieldCategory.Geography:
                return typeof(BeepTextBox);
            case DbFieldCategory.Currency:
                return typeof(BeepTextBox);
            case DbFieldCategory.Enum:
                return typeof(BeepComboBox);
            case DbFieldCategory.Timestamp:
                return typeof(BeepTextBox);
            case DbFieldCategory.Complex:
                return typeof(BeepTextBox);
            default:
                return typeof(BeepTextBox);
        }
    }


}


