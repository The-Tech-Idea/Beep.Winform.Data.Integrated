using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.ITrees.BeepTreeView;
using TheTechIdea.Beep.Winform.Controls.MenuBar;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views
{
    public partial class MainFrm_SideBar : TemplateForm
    {
      
      

        public MainFrm_SideBar()
        {
            InitializeComponent();

        }
        public MainFrm_SideBar(IServiceProvider service) : base(service)
        {
            InitializeComponent();
            appManager.Container = beepDisplayContainer1;
            appManager.Container.ContainerType = ContainerTypeEnum.TabbedPanel;


     

        }
    }
}
