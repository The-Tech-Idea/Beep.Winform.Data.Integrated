using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    public partial class uc_Login : TemplateUserControl
    {
        public uc_Login(IServiceProvider services):base(services) 
        {
            InitializeComponent();

        }

        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);
           beepLabel1.Text = settings["Title"].ToString();
           beepLabel1.SubHeaderText = settings["SubTitle"].ToString();
           beepLogin1.LoginClick+= (s, e) =>
           {
            
                   GetLoginClick();
           };
            CancelbeepButton.Click+= (s, e) =>
            {
         
                    GetCancelClick();
            };
        }

        private void GetCancelClick()
        {
           
        }

        private void  GetLoginClick()
        {
           
        }


        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            base.OnNavigatedTo(parameters);

          

        }
        public void SetTitle(string title)
        {
            beepLabel1.Text = title;
        }
        public void SetTitleandSubTitle(string title,string subtitle)
        {
            beepLabel1.Text = title;
            beepLabel1.SubHeaderText = subtitle;
        }
        public void ApplyTheme()
        {
            if (Theme == null) { }
            else
            {
                this.BackColor = _currentTheme.BackgroundColor;
                this.ForeColor = _currentTheme.ForeColor;
                beepLogin1.Theme = Theme;
                beepLabel1.Theme = Theme;

            }
        }
    }
}
