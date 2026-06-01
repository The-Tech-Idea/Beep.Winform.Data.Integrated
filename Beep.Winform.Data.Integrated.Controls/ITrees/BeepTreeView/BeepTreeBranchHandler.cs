
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.DataView;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls.Helpers;
using TheTechIdea.Beep.Services;

namespace TheTechIdea.Beep.Winform.Controls.ITrees.BeepTreeView
{
    public partial class BeepTreeBranchHandler : ITreeBranchHandler
    {
        private IBeepService service;
        private BeepAppTree beepTreeControl;
        public BeepTreeBranchHandler(IBeepService service, BeepAppTree beepTreeControl)
        {
            this.service = service;
            this.beepTreeControl = beepTreeControl;
            Tree= (ITree)beepTreeControl;
            DMEEditor = service.DMEEditor;
        }
        public IDMEEditor DMEEditor { get ; set ; }
        public ITree Tree { get; set; }
        public IErrorsInfo AddBranch(IBranch ParentBranch, IBranch Branch)
        {
           
            try
            {
                beepTreeControl.AddBranch(ParentBranch, Branch);

            }
            catch (Exception ex)
            {
                string mes = "Could not Add Branch to " + ParentBranch.BranchText;
                DMEEditor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
            };
            return DMEEditor.ErrorObject;
        }
        
        public IErrorsInfo AddCategory(IBranch Rootbr, string foldername)
        {
            try
            {
                if (DMEEditor.Passedarguments == null)
                {
                    DMEEditor.Passedarguments = new PassedArgs();
                }
                if (foldername != null)
                {
                    if (foldername.Length > 0)
                    {
                        if (!DMEEditor.ConfigEditor.CategoryFolders.Where(p => p.RootName.Equals(Rootbr.BranchClass, StringComparison.InvariantCultureIgnoreCase) && p.ParentName.Equals(Rootbr.BranchText, StringComparison.InvariantCultureIgnoreCase) && p.FolderName.Equals(foldername, StringComparison.InvariantCultureIgnoreCase)).Any())
                        {
                            CategoryFolder x = DMEEditor.ConfigEditor.AddFolderCategory(foldername, Rootbr.BranchClass, Rootbr.BranchText);
                            IBranch br=  Rootbr.CreateCategoryNode(x);
                            if (br != null) {
                                SimpleItem parent = beepTreeControl.GetNodeByGuidID(Rootbr.GuidID);
                                if (parent != null) {
                                    beepTreeControl.AddBranch(Rootbr, br);
                                }
                            }
                            DMEEditor.ConfigEditor.SaveCategoryFoldersValues();
                        }
                    }
                }
                DMEEditor.AddLogMessage("Success", "Added Category", DateTime.Now, 0, null, Errors.Failed);
            }
            catch (Exception ex)
            {
                string mes = "Could not Add Category";
                DMEEditor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
            };
            return DMEEditor.ErrorObject;
        }
        public string CheckifBranchExistinCategory(string BranchName, string pRootName)
        {
            List<CategoryFolder> ls = DMEEditor.ConfigEditor.CategoryFolders.Where(x => x.RootName == pRootName).ToList();
            foreach (CategoryFolder item in ls)
            {
                foreach (string f in item.items)
                {
                    if (f == BranchName)
                    {
                        return item.FolderName;
                    }
                }
            }
            return null;
        }
        public IErrorsInfo CreateBranch(IBranch Branch)
        {
            beepTreeControl.AddBranch(Branch);
            return DMEEditor.ErrorObject;
        }

        #region "Find Branch"
        public IBranch GetBranch(int pID)
        {
            IBranch FindBranchByID(IEnumerable<IBranch> branches, int id)
            {
                foreach (var branch in branches)
                {
                    // Check if the current branch matches
                    if (branch.BranchID == id)
                        return branch;
                    IBranch found = null;
                    if (branch.ChildBranchs != null)
                    {
                        found = FindBranchByID(branch.ChildBranchs, id);
                    }
                    // Recursively check child branches
                 
                    if (found != null)
                        return found;
                }
                return null; // No match found
            }

            return FindBranchByID(Tree.Branches, pID);
        }

        public IBranch GetBranchByMiscID(int pID)
        {
            IBranch FindBranchByMiscID(IEnumerable<IBranch> branches, int id)
            {
                foreach (var branch in branches)
                {
                    // Check if the current branch matches
                    if (branch.MiscID == id)
                        return branch;

                    // Recursively check child branches
                    var found = FindBranchByMiscID(branch.ChildBranchs, id);
                    if (found != null)
                        return found;
                }
                return null; // No match found
            }

            return FindBranchByMiscID(Tree.Branches, pID);
        }

        public IBranch GetNodeByGuidID(string guidID)
        {
            if (string.IsNullOrEmpty(guidID))
            {
               // Console.WriteLine("Invalid GuidID provided.");
                return null;
            }

            IBranch FindNodeByGuid(IEnumerable<IBranch> nodes, string guid)
            {
                foreach (var node in nodes)
                {
                   // Console.WriteLine($"Checking node: {node.Name}");

                    if (node.GuidID == guid)
                    {
                       // Console.WriteLine($"Match found: {node.Name}");
                        return node;
                    }

                    var found = FindNodeByGuid(node.ChildBranchs, guid);
                    if (found != null)
                        return found;
                }
                return null;
            }

            var result = FindNodeByGuid(Tree.Branches, guidID);
            if (result == null)
            {
               // Console.WriteLine($"Node with GuidID {guidID} not found.");
            }
            return result;
        }

        /// <summary>
        /// Recursively searches for a node in the provided list of SimpleItems.
        /// </summary>
        /// <param name="items">The list of SimpleItems to search.</param>
        /// <param name="predicate">The condition to match the node.</param>
        /// <returns>The matching SimpleItem, or null if no match is found.</returns>
        private IBranch FindNode(IEnumerable<IBranch> items, Func<IBranch, bool> predicate)
        {
            foreach (var item in items)
            {
                // Check the current node
                if (predicate(item))
                    return item;

                // Recursively check child nodes
                var childResult = FindNode(item.ChildBranchs, predicate);
                if (childResult != null)
                    return childResult;
            }

            return null; // No match found
        }
        public IBranch GetNode(string nodeName)
        {
            return FindNode(Tree.Branches, item => item.Name == nodeName);
        }

        public IBranch GetNode(int nodeIndex)
        {
            int currentIndex = 0;
            return FindNode(Tree.Branches, _ =>
            {
                if (currentIndex == nodeIndex)
                    return true;

                currentIndex++;
                return false;
            });
        }
        #endregion "Find Branch"
        public IErrorsInfo MoveBranchToCategory(IBranch CategoryBranch, IBranch CurrentBranch)
        {
            try
            {
                IBranch CategoryBranchNode = GetNodeByGuidID(CategoryBranch.GuidID);
                IBranch ParentBranchNode = GetNodeByGuidID(CurrentBranch.ParentBranch.GuidID);
                IBranch CurrentBranchNode = GetNodeByGuidID(CurrentBranch.GuidID);
                string currentParentFolder = CheckifBranchExistinCategory(CurrentBranch.BranchText, CurrentBranch.BranchClass);
                
                if (currentParentFolder != null)
                {

                    RemoveEntityFromCategory(ParentBranchNode.BranchClass, currentParentFolder, CurrentBranch.BranchText);
                }
                SimpleItem CurrentNode = beepTreeControl.GetNodeByGuidID(CurrentBranch.GuidID);
                Tree.RemoveNode(CurrentNode.ID);
                CategoryFolder CurFodler = DMEEditor.ConfigEditor.CategoryFolders.Where(y => y.RootName == CategoryBranch.BranchClass && y.FolderName == CategoryBranch.BranchText).FirstOrDefault();
                if (CurFodler != null)
                {
                    if (CurFodler.items.Contains(CurrentBranch.BranchText) == false)
                    {
                        CurFodler.items.Remove(CurrentBranch.BranchText);
                    }
                }

                CategoryFolder NewFolder = DMEEditor.ConfigEditor.CategoryFolders.Where(y => y.FolderName == CategoryBranch.BranchText && y.RootName == CategoryBranch.BranchClass).FirstOrDefault();
                if (NewFolder != null)
                {
                    if (NewFolder.items.Contains(CurrentBranch.BranchText) == false)
                    {
                        NewFolder.items.Add(CurrentBranch.BranchText);
                    }
                }
                //if (CategoryBranch.BranchType == EnumPointType.Entity && CategoryBranch.BranchClass == "VIEW" && CurrentBranch.BranchClass == "VIEW" && CategoryBranch.DataSourceName == CurrentBranch.DataSourceName)
                //{
                //    IDataSource vds = DMEEditor.GetDataSource(CurrentBranch.DataSourceName);
                //    if (vds.Entities[vds.EntityListIndex(CategoryBranch.MiscID)].ID == vds.Entities[vds.EntityListIndex(CurrentBranch.MiscID)].ParentId)
                //    {

                //    }
                //    else
                //    {
                //        vds.Entities[vds.EntityListIndex(CurrentBranch.MiscID)].ParentId = vds.Entities[vds.EntityListIndex(CategoryBranch.MiscID)].ID;
                //    }


                //}
                SimpleItem simpleItem = ControlExtensions.CreateNode(Tree, CurrentBranch.ID, CurrentBranch);
                SimpleItem parentnode = beepTreeControl.GetNodeByGuidID(CategoryBranch.GuidID);
                parentnode.Children.Add(simpleItem);
                

                DMEEditor.ConfigEditor.SaveCategoryFoldersValues();


                DMEEditor.AddLogMessage("Success", "Moved Branch successfully", DateTime.Now, 0, null, Errors.Ok);
            }
            catch (Exception ex)
            {
                string mes = "Could not Moved Branch";
                DMEEditor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
            };
            return DMEEditor.ErrorObject;
        }
        public IErrorsInfo MoveBranchToParent(IBranch ParentBranch, IBranch CurrentBranch)
        {

            try
            {
                IBranch ParentBranchNode = GetNodeByGuidID(ParentBranch.GuidID );
                SimpleItem parentitem = beepTreeControl.GetNodeByGuidID(ParentBranch.GuidID);
                IBranch CurrentBranchNode = GetNodeByGuidID(CurrentBranch.GuidID);
                SimpleItem currentitem = beepTreeControl.GetNodeByGuidID(CurrentBranch.GuidID);
                string foldername = CheckifBranchExistinCategory(CurrentBranch.BranchText, CurrentBranch.BranchClass);
                if (foldername != null)
                {
                    RemoveEntityFromCategory(ParentBranch.BranchClass, foldername, CurrentBranch.BranchText);
                }
                if (CurrentBranchNode != null)
                {
                    Tree.RemoveNode(currentitem.ID);

                }

                CategoryFolder CurFodler = DMEEditor.ConfigEditor.CategoryFolders.Where(y => y.RootName == ParentBranch.BranchClass).FirstOrDefault();
                if (CurFodler != null)
                {
                    if (CurFodler.items.Contains(CurrentBranch.BranchText) == false)
                    {
                        CurFodler.items.Remove(CurrentBranch.BranchText);
                    }
                }

                CategoryFolder NewFolder = DMEEditor.ConfigEditor.CategoryFolders.Where(y => y.FolderName == ParentBranch.BranchText && y.RootName == ParentBranch.BranchClass).FirstOrDefault();
                if (NewFolder != null)
                {
                    if (NewFolder.items.Contains(CurrentBranch.BranchText) == false)
                    {
                        NewFolder.items.Add(CurrentBranch.BranchText);
                    }
                }
                //if (ParentBranch.BranchType == EnumPointType.Entity && ParentBranch.BranchClass == "VIEW" && CurrentBranch.BranchClass == "VIEW" && ParentBranch.DataSourceName == CurrentBranch.DataSourceName)
                //{
                //    IDataSource vds = DMEEditor.GetDataSource(CurrentBranch.DataSourceName);
                //    if (vds.Entities[vds.EntityListIndex(ParentBranch.MiscID)].ID == vds.Entities[vds.EntityListIndex(CurrentBranch.MiscID)].ParentId)
                //    {

                //    }
                //    else
                //    {
                //        vds.Entities[vds.EntityListIndex(CurrentBranch.MiscID)].ParentId = vds.Entities[vds.EntityListIndex(ParentBranch.MiscID)].ID;
                //    }


                //}
                SimpleItem simpleItem = ControlExtensions.CreateNode(Tree, CurrentBranch.ID, CurrentBranch);
              
                parentitem.Children.Add(simpleItem);

               // ParentBranchNode.Nodes.Add(CurrentBranchNode);

                DMEEditor.ConfigEditor.SaveCategoryFoldersValues();

                DMEEditor.AddLogMessage("Success", "Moved Branch successfully", DateTime.Now, 0, null, Errors.Ok);
            }
            catch (Exception ex)
            {
                string mes = "Could not Moved Branch";
                DMEEditor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
            };
            return DMEEditor.ErrorObject;
        }
        public IErrorsInfo RemoveBranch(IBranch Branch)
        {
            try
            {
             //   BeepTreeNode BranchNode = (BeepTreeNode)beepTreeControl.GetBranchByGuidID(Branch.GuidID);
                SimpleItem branchitem = beepTreeControl.GetNodeByGuidID(Branch.GuidID);
                SimpleItem parentitem = beepTreeControl.GetNodeByGuidID(Branch.ParentBranch.GuidID);
                string foldername = CheckifBranchExistinCategory(Branch.BranchText, Branch.BranchClass);
                if (foldername != null)
                {
                    RemoveEntityFromCategory(Branch.BranchClass, foldername, Branch.BranchText);
                }
                if (Branch.ChildBranchs != null){
                    if (Branch.ChildBranchs.Count > 0)
                    {
                        RemoveChildBranchs(Branch);
                    }
                }
            
                Tree.Branches.Remove(Branch);
                if (Tree.SelectedBranchs.Contains(Branch.BranchID))
                {
                    Tree.SelectedBranchs.Remove(Branch.BranchID);
                }
                IBranch parentbranch = Branch.ParentBranch;
                parentbranch.ChildBranchs.Remove(Branch);
               if (branchitem!=null)     beepTreeControl.RemoveNode(branchitem.ID);

                // Editor.AddLogMessage("Success", "removed node and childs", DateTime.Now, 0, null, Errors.OK);
            }
            catch (Exception ex)
            {
                string mes = "Could not  remove node and childs";
                DMEEditor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
            };
            return DMEEditor.ErrorObject;
        }
        public IErrorsInfo RemoveBranch(int id)
        {
            try
            {
                RemoveBranch(Tree.Branches.Where(x => x.BranchID == id).FirstOrDefault());
            }
            catch (Exception ex)
            {
                string mes = "Could not  remove node and childs";
                DMEEditor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
            };
            return DMEEditor.ErrorObject;

        }
        public IErrorsInfo RemoveCategoryBranch(int id)
        {
            try
            {
                IBranch CategoryBranch = GetBranch(id);
                IBranch RootBranch = GetBranch(CategoryBranch.ParentBranchID);
                SimpleItem categoryitem = beepTreeControl.GetNodeByGuidID(CategoryBranch.GuidID);
                SimpleItem parentitem = beepTreeControl.GetNodeByGuidID(RootBranch.GuidID);
                var ls = Tree.Branches.Where(x => x.ParentBranchID == id).ToList();
                if (ls.Count() > 0)
                {
                    foreach (IBranch f in ls)
                    {
                        MoveBranchToParent(RootBranch, f);
                    }
                }

                //parentitem.Children.Remove(categoryitem);
                //Tree.Branches.Remove(CategoryBranch);
                RemoveBranch(categoryitem.ID);
                CategoryFolder Folder = DMEEditor.ConfigEditor.CategoryFolders.Where(y => y.FolderName == CategoryBranch.BranchText && y.RootName == CategoryBranch.BranchClass).FirstOrDefault();
                DMEEditor.ConfigEditor.CategoryFolders.Remove(Folder);

                DMEEditor.ConfigEditor.SaveCategoryFoldersValues();
                DMEEditor.AddLogMessage("Success", "Removed Branch successfully", DateTime.Now, 0, null, Errors.Ok);

            }
            catch (Exception ex)
            {
                string mes = "";
                DMEEditor.AddLogMessage(ex.Message, "Could not remove category" + mes, DateTime.Now, -1, mes, Errors.Failed);

            };
            return DMEEditor.ErrorObject;
        }
        public IErrorsInfo RemoveChildBranchs(IBranch branch)
        {
            try
            {
                SimpleItem branchitem = beepTreeControl.GetNodeByGuidID(branch.GuidID);
                SimpleItem parentitem = null;
                if (branch.ParentBranch != null)
                {
                    parentitem = beepTreeControl.GetNodeByGuidID(branch.ParentBranch.GuidID);
                }
                
                if (branch.ChildBranchs != null)
                {
                   
                    foreach (IBranch item in branch.ChildBranchs)
                    {
                        if (item.ChildBranchs != null)
                        {
                            if (item.ChildBranchs.Count > 0)
                            {
                                RemoveChildBranchs(item);
                            }
                        }
                
                        RemoveBranch(item);
                    }
                }
            }
            catch (Exception ex)
            {
                string mes = "Could not  remove   childs";
                DMEEditor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
            };
            return DMEEditor.ErrorObject;
        }
        public bool RemoveEntityFromCategory(string root, string foldername, string entityname)
        {

            try
            {
                CategoryFolder f = DMEEditor.ConfigEditor.CategoryFolders.Where(x => x.RootName == root && x.FolderName == foldername).FirstOrDefault();
                if (f != null)
                {
                    f.items.Remove(entityname);
                }

                return true;
            }
            catch (Exception ex)
            {
                string mes = "";
                DMEEditor.AddLogMessage(ex.Message, "Could not remove entity from category" + mes, DateTime.Now, -1, mes, Errors.Failed);
                return false;
            };

        }
        public IErrorsInfo SendActionFromBranchToBranch(IBranch ToBranch, IBranch CurrentBranch, string ActionType)
        {
            string targetBranchClass = ToBranch.GetType().Name;
            string dragedBranchClass = CurrentBranch.GetType().Name;


            try
            {

                var functionAction = DMEEditor.ConfigEditor.Function2Functions.Where(x => x.FromClass == dragedBranchClass && x.ToClass == targetBranchClass && x.ToMethod == ActionType).FirstOrDefault();
                if (functionAction != null)
                {
                    Tree.RunMethod(ToBranch, ActionType);
                }
                //   Editor.AddLogMessage("Success", "Added Database Connection", DateTime.Now, 0, null, Errors.OK);
            }
            catch (Exception ex)
            {
                string mes = "Could not send action to branch";
                DMEEditor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
            };
            return DMEEditor.ErrorObject;
        }
    }
}
