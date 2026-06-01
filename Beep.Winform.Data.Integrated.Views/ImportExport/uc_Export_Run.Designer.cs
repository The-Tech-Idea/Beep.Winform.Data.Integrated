using TheTechIdea.Beep.Winform.Controls.ProgressBars;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    partial class uc_Export_Run
    {
        private System.ComponentModel.IContainer components = null;
        private Panel mainPanel;
        private Panel summaryPanel;
        private Panel runPanel;
        private Panel buttonPanel;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox txtSummary;
        private RichTextBox rtbLog;
        private TheTechIdea.Beep.Winform.Controls.BeepButton beepButton_Run;
        private TheTechIdea.Beep.Winform.Controls.BeepButton beepButton_Cancel;
        private BeepCheckBoxBool beepCheckBoxLastRun;
        private BeepProgressBar statusProgressBar;
        private BeepLabel statusLabelRows;

     

        private void InitializeComponent()
        {
            this.mainPanel = new Panel();
            this.summaryPanel = new Panel();
            this.runPanel = new Panel();
            this.buttonPanel = new Panel();
            this.txtSummary = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.rtbLog = new RichTextBox();
            this.beepButton_Run = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            this.beepButton_Cancel = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            this.beepCheckBoxLastRun = new BeepCheckBoxBool();
            this.statusProgressBar = new BeepProgressBar();
            this.statusLabelRows = new BeepLabel();

            this.SuspendLayout();

            // Main panel
            this.mainPanel.Dock = DockStyle.Fill;
            this.mainPanel.Padding = new System.Windows.Forms.Padding(8);

            // Summary panel
            this.summaryPanel.Dock = DockStyle.Top;
            this.summaryPanel.Height = 120;
            this.summaryPanel.Controls.Add(this.txtSummary);

            this.txtSummary.Dock = DockStyle.Fill;
            this.txtSummary.Multiline = true;
            this.txtSummary.ReadOnly = true;
            this.txtSummary.BackColor = System.Drawing.Color.WhiteSmoke;

            // Button panel
            this.buttonPanel.Dock = DockStyle.Top;
            this.buttonPanel.Height = 40;
            this.buttonPanel.Controls.Add(this.beepButton_Run);
            this.buttonPanel.Controls.Add(this.beepButton_Cancel);
            this.buttonPanel.Controls.Add(this.beepCheckBoxLastRun);

            this.beepButton_Run.Text = "Run Export";
            this.beepButton_Run.Location = new System.Drawing.Point(8, 8);
            this.beepButton_Run.Size = new System.Drawing.Size(100, 28);

            this.beepButton_Cancel.Text = "Cancel";
            this.beepButton_Cancel.Location = new System.Drawing.Point(120, 8);
            this.beepButton_Cancel.Size = new System.Drawing.Size(80, 28);
            this.beepButton_Cancel.Enabled = false;

            this.beepCheckBoxLastRun.Text = "Export completed successfully";
            this.beepCheckBoxLastRun.Location = new System.Drawing.Point(220, 8);
            this.beepCheckBoxLastRun.Size = new System.Drawing.Size(200, 24);
            this.beepCheckBoxLastRun.CurrentValue = false;
            this.beepCheckBoxLastRun.Enabled = false;

            // Run panel (log + progress)
            this.runPanel.Dock = DockStyle.Fill;
            this.runPanel.Controls.Add(this.rtbLog);
            this.runPanel.Controls.Add(this.statusLabelRows);
            this.runPanel.Controls.Add(this.statusProgressBar);

            this.statusProgressBar.Dock = DockStyle.Bottom;
            this.statusProgressBar.Height = 20;
            this.statusProgressBar.Value = 0;

            this.statusLabelRows.Dock = DockStyle.Bottom;
            this.statusLabelRows.Height = 20;
            this.statusLabelRows.Text = "Ready";

            this.rtbLog.Dock = DockStyle.Fill;
            this.rtbLog.BackColor = System.Drawing.Color.White;
            this.rtbLog.Multiline = true;
            this.rtbLog.ReadOnly = true;

            // Assemble
            this.mainPanel.Controls.Add(this.runPanel);
            this.mainPanel.Controls.Add(this.buttonPanel);
            this.mainPanel.Controls.Add(this.summaryPanel);

            this.Controls.Add(this.mainPanel);

            this.Size = new System.Drawing.Size(800, 500);
            this.ResumeLayout(false);
        }
    }
}
