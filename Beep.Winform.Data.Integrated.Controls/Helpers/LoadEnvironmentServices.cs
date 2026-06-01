using ExCSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Winform.Controls.Helpers;


namespace TheTechIdea.Beep.Winform.Controls.Helpers
{
    public static partial class LoadEnvironmentServices
    {
        public static void ConfigureControlExtendedServices(this IBeepService service)
        {
            // Load the services you need here
            // For example:
            AssemblyClassDefinitionManager.TreeStructures=service.Config_editor.AddinTreeStructure;
            AssemblyClassDefinitionManager.BranchesClasses = service.Config_editor.BranchesClasses;
            AssemblyClassDefinitionManager.GlobalFunctions=service.Config_editor.GlobalFunctions;
            LoadHandlers(service);
        }
        private static void LoadHandlers(this IBeepService service)
        {
            // Inject shared references
            DynamicFunctionCallingManager.DMEEditor = service.DMEEditor;
            DynamicFunctionCallingManager.Vismanager = service.vis;
            DynamicFunctionCallingManager.TreeEditor = (Vis.Modules.ITree)service.vis.Tree;

           
            // Assign delegates
            HandlersFactory.GlobalMenuItemsProvider = DynamicMenuManager.GetMenuItemsList; // Set this in the main form if needed

            HandlersFactory.RunFunctionHandler = DynamicFunctionCallingManager.RunFunctionFromExtensions;

            HandlersFactory.RunFunctionWithTreeHandler = ( item, method) =>
                DynamicFunctionCallingManager.RunFunctionFromExtensions( item, method);

            HandlersFactory.RunMethodFromObjectHandler = ( branch, method) =>
                DynamicFunctionCallingManager.RunMethodFromObject(branch, method);

            HandlersFactory.RunMethodFromExtensionHandler = (branch, def, method) =>
                DynamicFunctionCallingManager.RunMethodFromExtension(branch, def, method);

            HandlersFactory.RunMethodFromExtensionWithTreeHandler = ( branch, method) =>
                DynamicFunctionCallingManager.RunMethodFromExtension(branch, method);

        }
    }
}
