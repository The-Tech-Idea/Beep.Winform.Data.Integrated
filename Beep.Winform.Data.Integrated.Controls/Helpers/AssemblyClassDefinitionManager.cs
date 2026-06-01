using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;


namespace TheTechIdea.Beep.Winform.Controls.Helpers
{
    public static class AssemblyClassDefinitionManager
    {
        public static IDMEEditor Editor { get; set; }

        public static AssemblyClassDefinition GetAssemblyClassDefinitionByObjectTypeAndBranchClass(IDMEEditor DMEEditor, string ObjectType, string BranchClass)
        {
            return DMEEditor.ConfigEditor.BranchesClasses.Where(x => x.classProperties.ObjectType == ObjectType && x.classProperties.ClassType == BranchClass).FirstOrDefault();
        }
        public static AssemblyClassDefinition GetAssemblyClassDefinitionByObjectTypeAndBranchClassAndDataSourceType(IDMEEditor DMEEditor, string ObjectType, string BranchClass, DatasourceCategory datasourceCategory)
        {
            return DMEEditor.ConfigEditor.BranchesClasses.Where(x => x.classProperties.ObjectType == ObjectType && x.classProperties.ClassType == BranchClass && x.classProperties.Category == datasourceCategory).FirstOrDefault();
        }
        public static AssemblyClassDefinition GetAssemblyClassDefinitionByObjectTypeAndBranchClassAndPointType(IDMEEditor DMEEditor, string ObjectType, string BranchClass, EnumPointType PointType)
        {
            return DMEEditor.ConfigEditor.BranchesClasses.Where(x => x.classProperties.ObjectType == ObjectType && x.classProperties.ClassType == BranchClass && x.VisSchema.BranchType == PointType).FirstOrDefault();
        }
        public static AssemblyClassDefinition GetAssemblyClassDefinitionByObjectTypeAndBranchClassAndPointTypeAndDataSourceType(IDMEEditor DMEEditor, string ObjectType, string BranchClass, EnumPointType PointType, DatasourceCategory datasourceCategory)
        {
            return DMEEditor.ConfigEditor.BranchesClasses.Where(x => x.classProperties.ObjectType == ObjectType && x.classProperties.ClassType == BranchClass && x.VisSchema.BranchType == PointType && x.classProperties.Category == datasourceCategory).FirstOrDefault();
        }
        public static AssemblyClassDefinition GetAssemblyBranchsClassDefinitionByGuid(IDMEEditor DMEEditor, string Guid)
        {
            return DMEEditor.ConfigEditor.BranchesClasses.Where(x => x.GuidID == Guid).FirstOrDefault();
        }
        public static AssemblyClassDefinition GetAssemblyGlobalFunctionsClassDefinitionByGuid(IDMEEditor DMEEditor, string Guid)
        {
            return DMEEditor.ConfigEditor.GlobalFunctions.Where(x => x.GuidID == Guid).FirstOrDefault();
        }


        public static AssemblyClassDefinition GetAssemblyClassDefinition(string PackageName)
        {
            return Editor.ConfigEditor.BranchesClasses.Where(x => x.PackageName == PackageName).FirstOrDefault();
        }
        public static AssemblyClassDefinition GetAssemblyClassDefinitionByClassName(string ClassName)
        {
            return Editor.ConfigEditor.BranchesClasses.Where(x => x.className == ClassName).FirstOrDefault();
        }
        public static AssemblyClassDefinition GetAssemblyClassDefinitionByObjectType(string ObjectType)
        {
            return Editor.ConfigEditor.BranchesClasses.Where(x => x.classProperties.ObjectType == ObjectType).FirstOrDefault();
        }
        public static AssemblyClassDefinition GetAssemblyClassDefinitionByObjectTypeAndBranchClass(string ObjectType, string BranchClass)
        {
            return Editor.ConfigEditor.BranchesClasses.Where(x => x.classProperties.ObjectType == ObjectType && x.classProperties.ClassType == BranchClass).FirstOrDefault();
        }
        public static AssemblyClassDefinition GetAssemblyClassDefinitionByObjectTypeAndBranchClassAndDataSourceType(string ObjectType, string BranchClass, DatasourceCategory datasourceCategory)
        {
            return Editor.ConfigEditor.BranchesClasses.Where(x => x.classProperties.ObjectType == ObjectType && x.classProperties.ClassType == BranchClass && x.classProperties.Category == datasourceCategory).FirstOrDefault();
        }
        public static AssemblyClassDefinition GetAssemblyClassDefinitionByObjectTypeAndBranchClassAndPointType(string ObjectType, string BranchClass, EnumPointType PointType)
        {
            return Editor.ConfigEditor.BranchesClasses.Where(x => x.classProperties.ObjectType == ObjectType && x.classProperties.ClassType == BranchClass && x.VisSchema.BranchType == PointType).FirstOrDefault();
        }
        public static AssemblyClassDefinition GetAssemblyClassDefinitionByObjectTypeAndBranchClassAndPointTypeAndDataSourceType(string ObjectType, string BranchClass, EnumPointType PointType, DatasourceCategory datasourceCategory)
        {
            return Editor.ConfigEditor.BranchesClasses.Where(x => x.classProperties.ObjectType == ObjectType && x.classProperties.ClassType == BranchClass && x.VisSchema.BranchType == PointType && x.classProperties.Category == datasourceCategory).FirstOrDefault();
        }
        public static AssemblyClassDefinition GetAssemblyClassDefinitionByGuid(string Guid)
        {
            AssemblyClassDefinition ret= Editor.ConfigEditor.BranchesClasses.Where(x => x.GuidID.Equals(Guid, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault(); 
            if (ret == null)
            {
                ret= Editor.ConfigEditor.GlobalFunctions.Where(x => x.GuidID.Equals(Guid, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }
            return ret;
        }
        public static List<AssemblyClassDefinition> GetAssemblyClassDefinitionForMenu(string ObjectType="Beep")
        {
            return Editor.ConfigEditor.GlobalFunctions.Where(o => o.classProperties != null && o.classProperties.ObjectType != null && (o.classProperties.Showin == ShowinType.Menu || o.classProperties.Showin == ShowinType.Both) && o.classProperties.ObjectType.Equals(ObjectType, StringComparison.CurrentCultureIgnoreCase)).OrderBy(p => p.Order).ToList();
        }
        public static List<AssemblyClassDefinition> GetAssemblyClassDefinitionVerticalToolbar(string ObjectType = "Beep")
        {
            return Editor.ConfigEditor.GlobalFunctions.Where(x => x.componentType == "IFunctionExtension" && x.classProperties != null && x.classProperties.ObjectType != null && (x.classProperties.Showin == ShowinType.HorZToolbar) && x.classProperties.ObjectType.Equals(ObjectType, StringComparison.InvariantCultureIgnoreCase)).OrderBy(p => p.Order).ToList();
        }
        public static List<AssemblyClassDefinition> GetAssemblyClassDefinitionToolbar(string ObjectType = "Beep")
        {
            return Editor.ConfigEditor.GlobalFunctions.Where(x => x.componentType == "IFunctionExtension" && x.classProperties != null && x.classProperties.ObjectType != null && (x.classProperties.Showin == ShowinType.Toolbar || x.classProperties.Showin == ShowinType.Both) && x.classProperties.ObjectType.Equals(ObjectType, StringComparison.InvariantCultureIgnoreCase)).OrderBy(p => p.Order).ToList();
        }
        public static List<AddinTreeStructure> GetAssemblyClassDefinitionAddins(string ObjectType = "Beep")
        {
            return Editor.ConfigEditor.AddinTreeStructure;
        }
        
    }
}
