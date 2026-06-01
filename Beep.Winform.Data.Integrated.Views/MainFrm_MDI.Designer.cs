namespace TheTechIdea.Beep.Winform.Default.Views
{
    partial class MainFrm_MDI
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
            beepDockingManager1 = new TheTechIdea.Beep.Winform.Controls.Docking.BeepDockingManager(components);
            dockPanel1 = new TheTechIdea.Beep.Winform.Controls.Docking.Models.DockPanel();
            beepDockspace1 = new TheTechIdea.Beep.Winform.Controls.Docking.BeepDockspace();
            dockPanel2 = new TheTechIdea.Beep.Winform.Controls.Docking.Models.DockPanel();
            beepDockspace1.SuspendLayout();
            SuspendLayout();
            // 
            // beepDockingManager1
            // 
            beepDockingManager1.HostForm = this;
            beepDockingManager1.Style = BeepControlStyle.DiscordStyle;
            beepDockingManager1.Theme = "GalaxyTheme";
            // 
            // dockPanel1
            // 
            dockPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dockPanel1.BackColor = Color.FromArgb(31, 25, 57);
            dockPanel1.ForeColor = Color.White;
            dockPanel1.Key = "dockPanel1";
            dockPanel1.Location = new Point(0, 26);
            dockPanel1.Manager = beepDockingManager1;
            dockPanel1.Name = "dockPanel1";
            dockPanel1.Size = new Size(250, 410);
            dockPanel1.TabIndex = 0;
            dockPanel1.Title = "dockPanel1";
            // 
            // beepDockspace1
            // 
            beepDockspace1.ActivePanelKey = "dockPanel2";
            beepDockspace1.BackColor = Color.FromArgb(31, 25, 57);
            beepDockspace1.Controls.Add(dockPanel2);
            beepDockspace1.Controls.Add(dockPanel1);
            beepDockspace1.ForeColor = Color.White;
            beepDockspace1.Location = new Point(4, 48);
            beepDockspace1.Manager = beepDockingManager1;
            beepDockspace1.MinimumSize = new Size(150, 150);
            beepDockspace1.Name = "beepDockspace1";
            beepDockspace1.Size = new Size(250, 436);
            beepDockspace1.TabIndex = 22;
            // 
            // dockPanel2
            // 
            dockPanel2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dockPanel2.BackColor = Color.FromArgb(31, 25, 57);
            dockPanel2.ForeColor = Color.White;
            dockPanel2.Key = "dockPanel2";
            dockPanel2.Location = new Point(0, 26);
            dockPanel2.Manager = beepDockingManager1;
            dockPanel2.Name = "dockPanel2";
            dockPanel2.Size = new Size(250, 410);
            dockPanel2.TabIndex = 1;
            dockPanel2.Title = "dockPanel2";
            // 
            // MainFrm_MDI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(798, 488);
            Controls.Add(beepDockspace1);
            IsMdiContainer = true;
            KeyPreview = true;
            Name = "MainFrm_MDI";
            Text = "`";
            beepDockspace1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion


        private Controls.Docking.BeepDockingManager beepDockingManager1;
        private Controls.Docking.BeepDockspace beepDockspace1;
        private Controls.Docking.Models.DockPanel dockPanel2;
        private Controls.Docking.Models.DockPanel dockPanel1;
    }
}