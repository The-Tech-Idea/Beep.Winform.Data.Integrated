using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    public partial class uc_Login : TemplateUserControl
    {
        public event EventHandler? LoginRequested;
        public event EventHandler? CancelRequested;

        public uc_Login(IServiceProvider services):base(services)
        {
            InitializeComponent();
            // Skill § "Configure" handler accumulation: -= before += so multiple
            // Configure calls do not stack delegates on each event.
            beepLogin1.LoginClick -= OnLoginClick;
            beepLogin1.LoginClick += OnLoginClick;
            CancelbeepButton.Click -= OnCancelClick;
            CancelbeepButton.Click += OnCancelClick;
            ApplyDpiScaledLayout();
        }

        /// <summary>
        /// Skill § "Sizing tokens": apply DPI-scaled <see cref="BeepLayoutMetrics"/> values to
        /// chrome that the Designer serialized as static pixels. The Designer is the source
        /// of truth for layout; this method overlays DPI-scaled dimensions on top.
        /// </summary>
        private void ApplyDpiScaledLayout()
        {
            // Usercontrol chrome: design-time size lives in Designer; overlay DPI-scaled dialog.
            Size = BeepLayoutMetrics.DialogSmall.ScaleSize(this);
            // Login card + cancel button keep the designer's pixel bounds as a floor,
            // but height tracks the design-skill button height so it stays accessible.
            int buttonHeight = BeepLayoutMetrics.ButtonStandard.Height.ScaleValue(this);
            CancelbeepButton.Height = Math.Max(CancelbeepButton.Height, buttonHeight);
        }

        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);

            // Robust null-check pattern: settings may be null or missing keys.
            if (settings != null)
            {
                if (settings.TryGetValue("Title", out var title) && title != null)
                    beepLabel1.Text = title.ToString() ?? string.Empty;
                if (settings.TryGetValue("SubTitle", out var sub) && sub != null)
                    beepLabel1.SubHeaderText = sub.ToString() ?? string.Empty;
            }
        }

        private void OnLoginClick(object? sender, EventArgs e)
        {
            LoginRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnCancelClick(object? sender, EventArgs e)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        public void SetTitle(string title)
        {
            beepLabel1.Text = title;
        }
        public void SetTitleandSubTitle(string title, string subtitle)
        {
            beepLabel1.Text = title;
            beepLabel1.SubHeaderText = subtitle;
        }
    }
}
