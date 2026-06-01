namespace TheTechIdea.Beep.Winform.Default.Views.Template
{
    partial class TemplateForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            beepFormuiManager1 = new TheTechIdea.Beep.Winform.Controls.Managers.BeepFormUIManager(components);
            SuspendLayout();
            // 
            // beepFormuiManager1
            // 
            beepFormuiManager1.ApplyBeepFormStyle = false;
            beepFormuiManager1.ApplyThemeOnImage = false;
            beepFormuiManager1.Backdrop = null;
            beepFormuiManager1.BeepAppBar = null;
            beepFormuiManager1.BeepiForm = null;
            beepFormuiManager1.BeepMenuBar = null;
            beepFormuiManager1.BeepSideMenu = null;
            beepFormuiManager1.BorderRadius = null;
            beepFormuiManager1.BorderThickness = null;
            beepFormuiManager1.CaptionHeight = null;
            beepFormuiManager1.CaptionPadding = null;
            beepFormuiManager1.DisplayContainer = null;
            beepFormuiManager1.EnableCaptionGradient = null;
            beepFormuiManager1.IsRounded = false;
            beepFormuiManager1.LogoImage = "";
            beepFormuiManager1.ShowBorder = true;
            beepFormuiManager1.ShowCaptionBar = null;
            beepFormuiManager1.ShowIconInCaption = null;
            beepFormuiManager1.ShowShadow = false;
            beepFormuiManager1.ShowSystemButtons = null;
            beepFormuiManager1.Theme = "DefaultType";
            beepFormuiManager1.Title = "Beep Form";
            beepFormuiManager1.UseImmersiveDarkMode = null;
            // 
            // TemplateForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CaptionHeight = 44;
            ClientSize = new Size(844, 520);
            ForeColor = Color.Black;
            Name = "TemplateForm";
            Padding = new Padding(3);
            Text = "TemplateForm";
            ResumeLayout(false);
        }

        #endregion

        private Controls.Managers.BeepFormUIManager beepFormuiManager1;
    }
}