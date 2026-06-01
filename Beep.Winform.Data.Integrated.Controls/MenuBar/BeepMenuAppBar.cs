
using System.ComponentModel;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Desktop.Common.Helpers;
using TheTechIdea.Beep.Desktop.Common.Util;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Services;
using TheTechIdea.Beep.Winform.Controls.Helpers;
//


namespace TheTechIdea.Beep.Winform.Controls.MenuBar
{
    [ToolboxItem(true)]
    [DisplayName("Beep Menu App Bar")]
    [Category("Beep Controls")]
    [Description("A menu bar control that displays a list of items.")]
    public partial class BeepMenuAppBar:BeepMenuBar
    {

        IBeepService _beepServices;
        IDMEEditor DMEEditor;
        string currentMenuName = "Beep";
        public IBeepService beepServices { 
            get { return _beepServices; }
            set { _beepServices = value; }
              
        }

        public string? ObjectType { get; private set; }

        public BeepMenuAppBar(IBeepService beepServices)
        {
            _beepServices = beepServices;
            DMEEditor = _beepServices.DMEEditor;
        }

        public BeepMenuAppBar():base()
        {
            
        }
        public IErrorsInfo CreateMenuItems(string menuname)
        {
            currentMenuName= menuname;
            return CreateMenuItems();
        }
        public IErrorsInfo CreateMenuItems()
        {
            ErrorsInfo errors = new ErrorsInfo();
            errors.Flag = Errors.Ok;
            try
            {
                foreach (var item in DynamicMenuManager.CreateCombinedMenuItems(currentMenuName))
                {
                    MenuItems.Add(item);
                }
                //InitMenu();
                this.Invalidate();
                return errors;
            }
            catch (Exception ex)
            {
                errors.Ex = ex;
                errors.Flag = Errors.Failed;
                errors.Message = ex.Message;

                return errors;
            }
        }
      
    }
}
