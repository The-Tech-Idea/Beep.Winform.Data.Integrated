namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    partial class uc_DefaultsEditor
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) { components.Dispose(); }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _rootPanel = new TheTechIdea.Beep.Winform.Controls.BeepPanel();
            _headerPanel = new TheTechIdea.Beep.Winform.Controls.BeepPanel();
            _lblTitle = new TheTechIdea.Beep.Winform.Controls.BeepLabel();
            _lblSubtitle = new TheTechIdea.Beep.Winform.Controls.BeepLabel();
            _contentHost = new TheTechIdea.Beep.Winform.Controls.BeepPanel();
            _actionsPanel = new TheTechIdea.Beep.Winform.Controls.BeepPanel();
            _btnCancel = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            _btnBack = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            _btnNext = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            _rootPanel.SuspendLayout();
            _headerPanel.SuspendLayout();
            _actionsPanel.SuspendLayout();
            SuspendLayout();

            // _rootPanel
            _rootPanel.ControlStyle = TheTechIdea.Beep.Winform.Controls.Common.BeepControlStyle.Material3;
            _rootPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _rootPanel.Padding = new System.Windows.Forms.Padding(12);
            _rootPanel.Controls.Add(_headerPanel);
            _rootPanel.Controls.Add(_contentHost);
            _rootPanel.Controls.Add(_actionsPanel);

            // _headerPanel
            _headerPanel.BackColor = System.Drawing.SystemColors.Control;
            _headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            _headerPanel.Size = new System.Drawing.Size(840, 76);
            _headerPanel.Controls.Add(_lblTitle);
            _headerPanel.Controls.Add(_lblSubtitle);

            // _lblTitle
            _lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            _lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            _lblTitle.Padding = new System.Windows.Forms.Padding(16, 12, 16, 0);
            _lblTitle.Size = new System.Drawing.Size(840, 40);
            _lblTitle.Text = "uc_DefaultsEditor";

            // _lblSubtitle
            _lblSubtitle.Dock = System.Windows.Forms.DockStyle.Top;
            _lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 10F);
            _lblSubtitle.ForeColor = System.Drawing.Color.DimGray;
            _lblSubtitle.Padding = new System.Windows.Forms.Padding(16, 2, 16, 4);
            _lblSubtitle.Size = new System.Drawing.Size(840, 24);
            _lblSubtitle.Text = "uc_DefaultsEditor placeholder.";

            // _contentHost
            _contentHost.Dock = System.Windows.Forms.DockStyle.Fill;
            _contentHost.Padding = new System.Windows.Forms.Padding(8);

            // _actionsPanel
            _actionsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            _actionsPanel.Padding = new System.Windows.Forms.Padding(10);
            _actionsPanel.Size = new System.Drawing.Size(840, 52);
            _actionsPanel.Controls.Add(_btnCancel);
            _actionsPanel.Controls.Add(_btnBack);
            _actionsPanel.Controls.Add(_btnNext);

            // _btnCancel – secondary (leftmost via RightToLeft=Yes)
            _btnCancel.Text = "Cancel";
            _btnCancel.Size = new System.Drawing.Size(100, 32);
            _btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            _btnCancel.Dock = System.Windows.Forms.DockStyle.None;
            _btnCancel.Location = new System.Drawing.Point(12, 10);

            // _btnBack – secondary
            _btnBack.Text = "Back";
            _btnBack.Size = new System.Drawing.Size(100, 32);
            _btnBack.Location = new System.Drawing.Point(120, 10);
            _btnBack.Enabled = false;

            // _btnNext – primary
            _btnNext.Text = "Next";
            _btnNext.Size = new System.Drawing.Size(130, 36);
            _btnNext.Location = new System.Drawing.Point(228, 8);

            // uc_DefaultsEditor
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(_rootPanel);
            Name = "uc_DefaultsEditor";
            Size = new System.Drawing.Size(840, 560);
            _rootPanel.ResumeLayout(false);
            _headerPanel.ResumeLayout(false);
            _actionsPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private TheTechIdea.Beep.Winform.Controls.BeepPanel _rootPanel;
        private TheTechIdea.Beep.Winform.Controls.BeepPanel _headerPanel;
        private TheTechIdea.Beep.Winform.Controls.BeepLabel _lblTitle;
        private TheTechIdea.Beep.Winform.Controls.BeepLabel _lblSubtitle;
        private TheTechIdea.Beep.Winform.Controls.BeepPanel _contentHost;
        private TheTechIdea.Beep.Winform.Controls.BeepPanel _actionsPanel;
        private TheTechIdea.Beep.Winform.Controls.BeepButton _btnCancel;
        private TheTechIdea.Beep.Winform.Controls.BeepButton _btnBack;
        private TheTechIdea.Beep.Winform.Controls.BeepButton _btnNext;
    }
}