using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Services;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Forms;
using TheTechIdea.Beep.Winform.Controls.Forms.ModernForm;

namespace TheTechIdea.Beep.Winform.Default.Views.Template
{
    public partial class TemplateForm: BeepiFormPro,IDM_Addin
    {

        protected readonly IBeepService? beepService;
        protected readonly IAppManager? appManager;
        private IDMEEditor Editor { get; }
        private bool _themeEventsRegistered;

        public TemplateForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            InitializeComponent();
            InitializeThemeSync();
        }

        public TemplateForm(IServiceProvider services) : base()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            InitializeComponent();
            InitializeThemeSync();
            beepService = ServiceProviderServiceExtensions.GetService<IBeepService>(services);
            appManager= ServiceProviderServiceExtensions.GetService<IAppManager>(services);
            if (beepService != null)
            {
                Dependencies.DMEEditor = beepService.DMEEditor;

                beepFormuiManager1.OnThemeChanged += BeepuiManager1_OnThemeChanged;
                beepFormuiManager1.LogoImage = appManager.LogoUrl;
                beepFormuiManager1.Title = appManager.Title;
            }
        }

        private void BeepuiManager1_OnThemeChanged(string obj)
        {
           Invalidate();
        }

        private void InitializeThemeSync()
        {
            RegisterThemeEvents();
            ApplyCurrentThemeAndStyle();
        }

        private void RegisterThemeEvents()
        {
            if (_themeEventsRegistered) return;

            BeepThemesManager.ThemeChanged += BeepThemesManager_ThemeChanged;
            BeepThemesManager.FormStyleChanged += BeepThemesManager_FormStyleChanged;
            _themeEventsRegistered = true;
        }

        private void UnregisterThemeEvents()
        {
            if (!_themeEventsRegistered) return;

            BeepThemesManager.ThemeChanged -= BeepThemesManager_ThemeChanged;
            BeepThemesManager.FormStyleChanged -= BeepThemesManager_FormStyleChanged;
            _themeEventsRegistered = false;
        }

        private void BeepThemesManager_ThemeChanged(object? sender, ThemeChangeEventArgs e)
        {
            ApplyCurrentThemeAndStyleSafe();
        }

        private void BeepThemesManager_FormStyleChanged(object? sender, StyleChangeEventArgs e)
        {
            ApplyCurrentThemeAndStyleSafe();
        }

        private void ApplyCurrentThemeAndStyleSafe()
        {
            if (IsDisposed || Disposing) return;

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke((System.Windows.Forms.MethodInvoker)ApplyCurrentThemeAndStyle);
                }
                catch
                {
                    // Best effort only; form may be shutting down.
                }
                return;
            }

            ApplyCurrentThemeAndStyle();
        }

        private void ApplyCurrentThemeAndStyle()
        {
            Theme = BeepThemesManager.CurrentThemeName;
            FormStyle = BeepThemesManager.CurrentStyle;

            if (beepFormuiManager1 != null)
            {
                beepFormuiManager1.Theme = Theme;
            }

            ApplyTheme();
            Invalidate(true);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            UnregisterThemeEvents();
            base.OnFormClosed(e);
        }
        #region "IDM_Addin"
        public event EventHandler OnStart;
        public event EventHandler OnStop;
        public event EventHandler<ErrorEventArgs> OnError;
        public AddinDetails Details { get; set; } = new AddinDetails();
        public Dependencies Dependencies { get; set; } = new Dependencies();
        public string GuidID { get; set; }
        public bool IsConfigured { get ; set ; }
        public bool IsRunning { get ; set ; }
        public bool IsSuspended { get ; set ; }
        public bool IsStarted { get ; set ; }

        public virtual void Initialize()
        {
          


        }

        public virtual void Suspend()
        {

        }

        public virtual void Resume()
        {

        }

        public virtual string GetErrorDetails()
        {
            // if error occured return the error details
            // create error messege sring 
            string errormessage = "";
            if (Editor.ErrorObject != null)
            {
                if (Editor.ErrorObject.Errors.Count >0)
                {
                    foreach (var item in Editor.ErrorObject.Errors)
                    {
                        errormessage += item.Message + "\n";
                    }
                }
            }

            return errormessage;
        }

        public void Run(IPassedArgs pPassedarg)
        {

        }

        public virtual void Run(params object[] args)
        {

        }

        public virtual Task<IErrorsInfo> RunAsync(IPassedArgs pPassedarg)
        {
            try
            {
                // use the view router to navigate back

            }
            catch (Exception ex)
            {
                string methodName = MethodBase.GetCurrentMethod().Name; // Retrieves "PrintGrid"
                Dependencies.DMEEditor.AddLogMessage("Beep", $"in {methodName} Error : {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
            }
            return Task.FromResult(Dependencies.DMEEditor.ErrorObject);
        }

        public virtual Task<IErrorsInfo> RunAsync(params object[] args)
        {
            try
            {
                // use the view router to navigate back

            }
            catch (Exception ex)
            {
                string methodName = MethodBase.GetCurrentMethod().Name; // Retrieves "PrintGrid"
                Dependencies.DMEEditor.AddLogMessage("Beep", $"in {methodName} Error : {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
            }
            return Task.FromResult(Dependencies.DMEEditor.ErrorObject);

        }

        public virtual void Configure(Dictionary<string, object> settings)
        {

        }

        public virtual void OnNavigatedTo(Dictionary<string, object> parameters)
        {

        }

        public virtual void SetError(string message)
        {

        }
        #endregion "IDM_Addin"
    }
}
