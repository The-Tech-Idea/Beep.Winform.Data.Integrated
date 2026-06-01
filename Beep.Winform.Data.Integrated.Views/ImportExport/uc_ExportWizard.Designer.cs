using TheTechIdea.Beep.Winform.Controls.Numerics;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    partial class uc_ExportWizard
    {
        private System.ComponentModel.IContainer components = null;
        private Panel mainPanel;
        private Panel sourcePanel;
        private Panel destPanel;
        private Panel buttonPanel;
        private Panel logPanel;
        private TheTechIdea.Beep.Winform.Controls.BeepComboBox cmbSourceDS;
        private TheTechIdea.Beep.Winform.Controls.BeepComboBox cmbSourceEntity;
        private TheTechIdea.Beep.Winform.Controls.BeepComboBox cmbFormat;
        private TheTechIdea.Beep.Winform.Controls.BeepLabel lblSourceDS;
        private TheTechIdea.Beep.Winform.Controls.BeepLabel lblSourceEntity;
        private TheTechIdea.Beep.Winform.Controls.BeepLabel lblFormat;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox txtFilePath;
        private TheTechIdea.Beep.Winform.Controls.BeepButton btnBrowseFile;
        private TheTechIdea.Beep.Winform.Controls.BeepButton btnLaunch;
        private TheTechIdea.Beep.Winform.Controls.BeepButton btnClearLog;
        private BeepCheckBoxBool chkHeaders;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox txtDelimiter;
        private BeepNumericUpDown numBatchSize;
        private RichTextBox txtLog;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.mainPanel = new Panel();
            this.sourcePanel = new Panel();
            this.destPanel = new Panel();
            this.buttonPanel = new Panel();
            this.logPanel = new Panel();
            this.cmbSourceDS = new TheTechIdea.Beep.Winform.Controls.BeepComboBox();
            this.cmbSourceEntity = new TheTechIdea.Beep.Winform.Controls.BeepComboBox();
            this.cmbFormat = new TheTechIdea.Beep.Winform.Controls.BeepComboBox();
            this.lblSourceDS = new TheTechIdea.Beep.Winform.Controls.BeepLabel();
            this.lblSourceEntity = new TheTechIdea.Beep.Winform.Controls.BeepLabel();
            this.lblFormat = new TheTechIdea.Beep.Winform.Controls.BeepLabel();
            this.txtFilePath = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.btnBrowseFile = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            this.btnLaunch = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            this.btnClearLog = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            this.chkHeaders = new BeepCheckBoxBool();
            this.txtDelimiter = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.numBatchSize = new BeepNumericUpDown();
            this.txtLog = new RichTextBox();

            this.SuspendLayout();

            // Main panel
            this.mainPanel.Dock = DockStyle.Fill;
            this.mainPanel.Padding = new System.Windows.Forms.Padding(8);

            // Source panel
            this.sourcePanel.Dock = DockStyle.Top;
            this.sourcePanel.Height = 80;
            this.sourcePanel.Controls.Add(this.lblSourceDS);
            this.sourcePanel.Controls.Add(this.cmbSourceDS);
            this.sourcePanel.Controls.Add(this.lblSourceEntity);
            this.sourcePanel.Controls.Add(this.cmbSourceEntity);

            this.lblSourceDS.Text = "Source Data Source:";
            this.lblSourceDS.Location = new System.Drawing.Point(8, 8);
            this.lblSourceDS.Size = new System.Drawing.Size(120, 20);

            this.cmbSourceDS.Location = new System.Drawing.Point(140, 8);
            this.cmbSourceDS.Size = new System.Drawing.Size(200, 24);

            this.lblSourceEntity.Text = "Source Entity:";
            this.lblSourceEntity.Location = new System.Drawing.Point(8, 40);
            this.lblSourceEntity.Size = new System.Drawing.Size(120, 20);

            this.cmbSourceEntity.Location = new System.Drawing.Point(140, 40);
            this.cmbSourceEntity.Size = new System.Drawing.Size(200, 24);

            // Destination panel
            this.destPanel.Dock = DockStyle.Top;
            this.destPanel.Height = 120;
            this.destPanel.Controls.Add(this.lblFormat);
            this.destPanel.Controls.Add(this.cmbFormat);
            this.destPanel.Controls.Add(this.txtFilePath);
            this.destPanel.Controls.Add(this.btnBrowseFile);
            this.destPanel.Controls.Add(this.chkHeaders);
            this.destPanel.Controls.Add(this.txtDelimiter);
            this.destPanel.Controls.Add(this.numBatchSize);

            this.lblFormat.Text = "Export Format:";
            this.lblFormat.Location = new System.Drawing.Point(8, 8);
            this.lblFormat.Size = new System.Drawing.Size(120, 20);

            this.cmbFormat.Location = new System.Drawing.Point(140, 8);
            this.cmbFormat.Size = new System.Drawing.Size(120, 24);

            this.txtFilePath.Location = new System.Drawing.Point(8, 40);
            this.txtFilePath.Size = new System.Drawing.Size(300, 24);
            this.txtFilePath.PlaceholderText = "File path...";

            this.btnBrowseFile.Text = "Browse...";
            this.btnBrowseFile.Location = new System.Drawing.Point(320, 40);
            this.btnBrowseFile.Size = new System.Drawing.Size(80, 24);

            this.chkHeaders.Text = "Include Headers";
            this.chkHeaders.Location = new System.Drawing.Point(8, 72);
            this.chkHeaders.Size = new System.Drawing.Size(120, 20);
            this.chkHeaders.CurrentValue = true;

            this.txtDelimiter.Text = ",";
            this.txtDelimiter.Location = new System.Drawing.Point(140, 72);
            this.txtDelimiter.Size = new System.Drawing.Size(40, 24);
            this.txtDelimiter.PlaceholderText = "Delimiter";

            this.numBatchSize.Value = 1000;
            this.numBatchSize.Location = new System.Drawing.Point(200, 72);
            this.numBatchSize.Size = new System.Drawing.Size(80, 24);

            // Button panel
            this.buttonPanel.Dock = DockStyle.Top;
            this.buttonPanel.Height = 40;
            this.buttonPanel.Controls.Add(this.btnLaunch);
            this.buttonPanel.Controls.Add(this.btnClearLog);

            this.btnLaunch.Text = "Launch Export Wizard";
            this.btnLaunch.Location = new System.Drawing.Point(8, 8);
            this.btnLaunch.Size = new System.Drawing.Size(150, 28);

            this.btnClearLog.Text = "Clear Log";
            this.btnClearLog.Location = new System.Drawing.Point(170, 8);
            this.btnClearLog.Size = new System.Drawing.Size(80, 28);

            // Log panel
            this.logPanel.Dock = DockStyle.Fill;
            this.logPanel.Controls.Add(this.txtLog);

            this.txtLog.Dock = DockStyle.Fill;
            this.txtLog.Multiline = true;
            this.txtLog.ReadOnly = true;
            this.txtLog.BackColor = System.Drawing.Color.WhiteSmoke;

            // Assemble
            this.mainPanel.Controls.Add(this.logPanel);
            this.mainPanel.Controls.Add(this.buttonPanel);
            this.mainPanel.Controls.Add(this.destPanel);
            this.mainPanel.Controls.Add(this.sourcePanel);

            this.Controls.Add(this.mainPanel);

            this.Size = new System.Drawing.Size(800, 600);
            this.ResumeLayout(false);
        }
    }
}
