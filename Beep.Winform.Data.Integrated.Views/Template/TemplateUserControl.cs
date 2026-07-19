using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Reflection;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;

using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Vis.Modules;

using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Base;
using TheTechIdea.Beep.Winform.Controls.Converters;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.Winform.Controls.ThemeManagement;
using TheTechIdea.Beep.Winform.Controls.Forms.ModernForm;
using TheTechIdea.Beep.Services;

namespace TheTechIdea.Beep.Winform.Default.Views.Template
{
    // Minimal, .NET 8-friendly template.
    public partial class TemplateUserControl : UserControl, IDM_Addin
    {
        // Common services and state expected by derived controls
        protected IBeepService? beepService;
        protected IDMEEditor? Editor { get; set; }
        protected IAppManager? appManager { get; set; }
        protected IBeepTheme _currentTheme = BeepThemesManager.GetDefaultTheme();
        protected IDataSource? ds;
        protected string? DataSourceName;
        protected string? EntityName;
        protected UnitOfWorkWrapper? uow;

        private bool _themeEventsRegistered;

        public TemplateUserControl()
        {
            InitializeComponent();
            Details = new AddinDetails { ObjectType = "UserControl" };
            Dependencies = new Dependencies();
            RegisterThemeEvents();
            
            // Enable double buffering to prevent flickering and ensure proper rendering
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
        }

     


      
        public TemplateUserControl(IServiceProvider services) : this()
        {
            beepService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<IBeepService>(services);
            appManager = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<IAppManager>(services);
            if (beepService != null)
            {
                Dependencies.DMEEditor = beepService.DMEEditor;
                Dependencies.Logger = beepService.lg;
                Editor = beepService.DMEEditor;
            }
            // Ensure the current theme is honored even when services supply theme changes later.
            Theme = BeepThemesManager.CurrentThemeName;
        }

        /// <summary>
        /// Applies DPI-scaled layout values on top of the Designer's design-time pixels. Override in
        /// a derived view; the base calls it once the handle exists and again whenever the host's
        /// DPI changes.
        /// </summary>
        /// <remarks>
        /// Do not call this from a constructor — that is exactly the bug this hook exists to fix.
        /// <see cref="DpiScalingHelper.GetDpiScaleFactor(Control)"/> returns <c>1.0</c> while
        /// <c>IsHandleCreated</c> is false, so every view that scaled from its ctor (all of them)
        /// computed design-time pixels and scaled nothing, on any machine, at any DPI. OnHandleCreated
        /// is the first moment DeviceDpi is authoritative.
        /// <para>
        /// Implementations must be idempotent and must not assume a monotonic scale: this runs again
        /// on every handle recreation (theme change) and on every DPI change, including a move to a
        /// LOWER-DPI monitor.
        /// </para>
        /// </remarks>
        protected virtual void ApplyDpiScaledLayout()
        {
        }

        /// <inheritdoc />
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            // First point at which DeviceDpi is real; see ApplyDpiScaledLayout.
            ApplyDpiScaledLayout();
        }

        /// <inheritdoc />
        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            // Dragged to a monitor with a different scale — re-apply against the new DeviceDpi.
            ApplyDpiScaledLayout();
        }

        private void BeepThemesManager_FormStyleChanged(object? sender, StyleChangeEventArgs e)
        {
            ControlFormStyle = e.NewStyle;
            if (IsDisposed || Disposing) return;
            ApplyTheme();
            Invalidate(true);
        }

        private void BeepThemesManager_ThemeChanged(object? sender, ThemeChangeEventArgs e)
        {
            Theme = e.NewThemeName;
           
        }

        private string _themeName = BeepThemesManager.CurrentThemeName;
        [Browsable(true)]
        [TypeConverter(typeof(ThemeEnumConverter))]
        public string Theme
        {
            get => _themeName;
            set
            {
                _themeName = value;
                _currentTheme = BeepThemesManager.GetTheme(value);
                ApplyTheme();
            }
        }

        public AddinDetails Details { get; set; }
        public Dependencies Dependencies { get; set; }
        public string GuidID { get; set; } = string.Empty;
        public bool IsConfigured { get ; set ; }=false;
        public bool IsRunning { get ; set ; }=false;
        public bool IsSuspended { get ; set ; }=false;
        public bool IsStarted { get ; set ; }=false;
        public FormStyle ControlFormStyle { get; private set; }

        public event EventHandler? OnStart;
        public event EventHandler? OnStop;
        public event EventHandler<ErrorEventArgs>? OnError;

    // Note: All explicit scaling/DPI-blocking logic was removed to rely on default .NET 8 behavior.

        public virtual void Configure(Dictionary<string, object> settings)
        {
            if (Theme != BeepThemesManager.CurrentThemeName)
            {
                Theme =BeepThemesManager.CurrentThemeName;
            }
          
        }

        public virtual void Initialize() { }
        public virtual void OnNavigatedTo(Dictionary<string, object> parameters) { }
        public virtual void Resume() { }
        public virtual void Run(IPassedArgs pPassedarg) { }
        public virtual void Run(params object[] args) { }
        public virtual void Suspend() { }
        public virtual void Dispose() { }

        public virtual string GetErrorDetails()
        {
            if (Editor?.ErrorObject?.Errors?.Count > 0)
            {
                return string.Join("\n", Editor.ErrorObject.Errors.Select(e => e.Message));
            }
            return string.Empty;
        }

        public virtual Task<IErrorsInfo> RunAsync(IPassedArgs pPassedarg)
        {
            try { }
            catch (Exception ex)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? nameof(RunAsync);
                Editor?.AddLogMessage("Beep", $"in {methodName} Error : {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
            }
            return Task.FromResult(Editor?.ErrorObject!);
        }

        public virtual Task<IErrorsInfo> RunAsync(params object[] args)
        {
            try { }
            catch (Exception ex)
            {
                string methodName = MethodBase.GetCurrentMethod()?.Name ?? nameof(RunAsync);
                Editor?.AddLogMessage("Beep", $"in {methodName} Error : {ex.Message}", DateTime.Now, -1, null, Errors.Failed);
            }
            return Task.FromResult(Editor?.ErrorObject!);
        }

        public virtual void SetError(string message) { }

        public virtual void ApplyTheme()
        {
            ApplyThemeToControls(this.Controls);
        }

        private void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control child in controls)
            {
                if (child is IBeepUIComponent ui)
                {
                    ui.Theme = Theme;
                }
                if (child.HasChildren)
                {
                    ApplyThemeToControls(child.Controls);
                }
            }
        }

        private void RegisterThemeEvents()
        {
            if (_themeEventsRegistered) return;
            BeepThemesManager.ThemeChanged += BeepThemesManager_ThemeChanged;
            BeepThemesManager.FormStyleChanged += BeepThemesManager_FormStyleChanged;
            _themeEventsRegistered = true;
            ControlFormStyle = BeepThemesManager.CurrentStyle;
            Theme = BeepThemesManager.CurrentThemeName;
        }

        private void UnregisterThemeEvents()
        {
            if (!_themeEventsRegistered) return;
            BeepThemesManager.ThemeChanged -= BeepThemesManager_ThemeChanged;
            BeepThemesManager.FormStyleChanged -= BeepThemesManager_FormStyleChanged;
            _themeEventsRegistered = false;
        }
        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing)
            {
                UnregisterThemeEvents();
            }
            base.Dispose(disposing);
        }
      
    }
}
