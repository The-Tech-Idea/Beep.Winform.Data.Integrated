namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Export
{
    partial class uc_ExportStep1_Select
    {
        private System.ComponentModel.IContainer? components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _rootPanel = new BeepPanel();
            _headerPanel = new BeepPanel();
            _formPanel = new BeepPanel();
            lblTitle = new BeepLabel();
            lblSourceDS = new BeepLabel();
            cmbSourceDS = new BeepComboBox();
            lblSourceEntity = new BeepLabel();
            cmbSourceEntity = new BeepComboBox();
            lblDestMode = new BeepLabel();
            radioDestMode = new BeepRadioGroup();
            lblFormat = new BeepLabel();
            cmbFormat = new BeepComboBox();
            lblFilePath = new BeepLabel();
            txtFilePath = new BeepTextBox();
            btnBrowse = new BeepButton();
            lblDestDS = new BeepLabel();
            cmbDestDS = new BeepComboBox();
            lblDestEntity = new BeepLabel();
            cmbDestEntity = new BeepComboBox();

            _rootPanel.SuspendLayout();
            _formPanel.SuspendLayout();
            SuspendLayout();

            _rootPanel.Controls.Add(_headerPanel);
            _rootPanel.Controls.Add(_formPanel);
            _rootPanel.Dock = DockStyle.Fill;

            _headerPanel.Controls.Add(lblTitle);
            _headerPanel.Dock = DockStyle.Top;
            _headerPanel.Height = 50;
            lblTitle.Text = "Export: Select Source & Target";
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            _formPanel.Dock = DockStyle.Fill;
            _formPanel.AutoScroll = true;

            int y = 20;
            int labelX = 30, comboX = 200, comboW = 350, labelW = 160;

            lblSourceDS.Text = "Source Data Source:"; lblSourceDS.Location = new Point(labelX, y + 3); lblSourceDS.Size = new Size(labelW, 24);
            cmbSourceDS.Location = new Point(comboX, y); cmbSourceDS.Size = new Size(comboW, 30); y += 40;

            lblSourceEntity.Text = "Source Entity:"; lblSourceEntity.Location = new Point(labelX, y + 3); lblSourceEntity.Size = new Size(labelW, 24);
            cmbSourceEntity.Location = new Point(comboX, y); cmbSourceEntity.Size = new Size(comboW, 30); y += 45;

            lblDestMode.Text = "Destination:"; lblDestMode.Location = new Point(labelX, y + 3); lblDestMode.Size = new Size(labelW, 24);
            radioDestMode.Location = new Point(comboX, y); radioDestMode.Size = new Size(300, 30); y += 40;

            lblFormat.Text = "Format:"; lblFormat.Location = new Point(labelX, y + 3); lblFormat.Size = new Size(labelW, 24);
            cmbFormat.Location = new Point(comboX, y); cmbFormat.Size = new Size(150, 30); y += 40;

            lblFilePath.Text = "File Path:"; lblFilePath.Location = new Point(labelX, y + 3); lblFilePath.Size = new Size(labelW, 24);
            txtFilePath.Location = new Point(comboX, y); txtFilePath.Size = new Size(comboW - 40, 30);
            btnBrowse.Text = "..."; btnBrowse.Location = new Point(comboX + comboW - 30, y); btnBrowse.Size = new Size(30, 30); y += 40;

            lblDestDS.Text = "Dest Data Source:"; lblDestDS.Location = new Point(labelX, y + 3); lblDestDS.Size = new Size(labelW, 24);
            cmbDestDS.Location = new Point(comboX, y); cmbDestDS.Size = new Size(comboW, 30); y += 40;

            lblDestEntity.Text = "Dest Entity:"; lblDestEntity.Location = new Point(labelX, y + 3); lblDestEntity.Size = new Size(labelW, 24);
            cmbDestEntity.Location = new Point(comboX, y); cmbDestEntity.Size = new Size(comboW, 30);

            _formPanel.Controls.Add(lblSourceDS);
            _formPanel.Controls.Add(cmbSourceDS);
            _formPanel.Controls.Add(lblSourceEntity);
            _formPanel.Controls.Add(cmbSourceEntity);
            _formPanel.Controls.Add(lblDestMode);
            _formPanel.Controls.Add(radioDestMode);
            _formPanel.Controls.Add(lblFormat);
            _formPanel.Controls.Add(cmbFormat);
            _formPanel.Controls.Add(lblFilePath);
            _formPanel.Controls.Add(txtFilePath);
            _formPanel.Controls.Add(btnBrowse);
            _formPanel.Controls.Add(lblDestDS);
            _formPanel.Controls.Add(cmbDestDS);
            _formPanel.Controls.Add(lblDestEntity);
            _formPanel.Controls.Add(cmbDestEntity);

            Controls.Add(_rootPanel);
            _rootPanel.ResumeLayout(false);
            _formPanel.ResumeLayout(false);
            SuspendLayout();
        }

        private BeepPanel _rootPanel;
        private BeepPanel _headerPanel;
        private BeepPanel _formPanel;
        private BeepLabel lblTitle;
        private BeepLabel lblSourceDS;
        private BeepComboBox cmbSourceDS;
        private BeepLabel lblSourceEntity;
        private BeepComboBox cmbSourceEntity;
        private BeepLabel lblDestMode;
        private BeepRadioGroup radioDestMode;
        private BeepLabel lblFormat;
        private BeepComboBox cmbFormat;
        private BeepLabel lblFilePath;
        private BeepTextBox txtFilePath;
        private BeepButton btnBrowse;
        private BeepLabel lblDestDS;
        private BeepComboBox cmbDestDS;
        private BeepLabel lblDestEntity;
        private BeepComboBox cmbDestEntity;
    }
}
