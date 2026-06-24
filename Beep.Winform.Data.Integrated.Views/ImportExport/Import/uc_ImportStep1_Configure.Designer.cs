using System.ComponentModel;
namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Import
{
    partial class uc_ImportStep1_Configure
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
            lblRowCount = new BeepLabel();
            btnRefreshCount = new BeepButton();
            lblDestDS = new BeepLabel();
            cmbDestDS = new BeepComboBox();
            lblDestEntity = new BeepLabel();
            cmbDestEntity = new BeepComboBox();
            chkCreateDest = new BeepCheckBoxBool();
            lblPurpose = new BeepLabel();
            cmbPurpose = new BeepComboBox();
            lblMatchBy = new BeepLabel();
            cmbMatchBy = new BeepComboBox();
            chkUpdateEmpty = new BeepCheckBoxBool();

            _rootPanel.SuspendLayout();
            _formPanel.SuspendLayout();
            SuspendLayout();

            // _rootPanel
            _rootPanel.Controls.Add(_headerPanel);
            _rootPanel.Controls.Add(_formPanel);
            _rootPanel.Dock = DockStyle.Fill;

            // _headerPanel
            _headerPanel.Controls.Add(lblTitle);
            _headerPanel.Dock = DockStyle.Top;
            _headerPanel.Height = 50;

            // lblTitle
            lblTitle.Text = "Step 1: Configure Source & Destination";
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            // _formPanel
            _formPanel.Controls.Add(lblSourceDS);
            _formPanel.Controls.Add(cmbSourceDS);
            _formPanel.Controls.Add(lblSourceEntity);
            _formPanel.Controls.Add(cmbSourceEntity);
            _formPanel.Controls.Add(lblRowCount);
            _formPanel.Controls.Add(btnRefreshCount);
            _formPanel.Controls.Add(lblDestDS);
            _formPanel.Controls.Add(cmbDestDS);
            _formPanel.Controls.Add(lblDestEntity);
            _formPanel.Controls.Add(cmbDestEntity);
            _formPanel.Controls.Add(chkCreateDest);
            _formPanel.Controls.Add(lblPurpose);
            _formPanel.Controls.Add(cmbPurpose);
            _formPanel.Controls.Add(lblMatchBy);
            _formPanel.Controls.Add(cmbMatchBy);
            _formPanel.Controls.Add(chkUpdateEmpty);
            _formPanel.Dock = DockStyle.Fill;
            _formPanel.AutoScroll = true;

            // Layout: labels left, combos right
            int y = 20;
            int labelX = 30, comboX = 200, comboW = 350, labelW = 160;

            lblSourceDS.Text = "Source Data Source:";
            lblSourceDS.Location = new Point(labelX, y + 3);
            lblSourceDS.Size = new Size(labelW, 24);
            cmbSourceDS.Location = new Point(comboX, y);
            cmbSourceDS.Size = new Size(comboW, 30);
            y += 40;

            lblSourceEntity.Text = "Source Entity:";
            lblSourceEntity.Location = new Point(labelX, y + 3);
            lblSourceEntity.Size = new Size(labelW, 24);
            cmbSourceEntity.Location = new Point(comboX, y);
            cmbSourceEntity.Size = new Size(comboW - 100, 30);
            lblRowCount.Text = "N/A";
            lblRowCount.Location = new Point(comboX + comboW - 90, y + 3);
            lblRowCount.Size = new Size(80, 24);
            btnRefreshCount.Text = "↻";
            btnRefreshCount.Location = new Point(comboX + comboW, y);
            btnRefreshCount.Size = new Size(30, 30);
            y += 40;

            lblDestDS.Text = "Destination Data Source:";
            lblDestDS.Location = new Point(labelX, y + 3);
            lblDestDS.Size = new Size(labelW, 24);
            cmbDestDS.Location = new Point(comboX, y);
            cmbDestDS.Size = new Size(comboW, 30);
            y += 40;

            lblDestEntity.Text = "Destination Entity:";
            lblDestEntity.Location = new Point(labelX, y + 3);
            lblDestEntity.Size = new Size(labelW, 24);
            cmbDestEntity.Location = new Point(comboX, y);
            cmbDestEntity.Size = new Size(comboW, 30);
            y += 40;

            chkCreateDest.Text = "Create destination if not exists";
            chkCreateDest.Location = new Point(comboX, y);
            chkCreateDest.Size = new Size(250, 24);
            chkCreateDest.Checked = true;
            y += 40;

            lblPurpose.Text = "Purpose:";
            lblPurpose.Location = new Point(labelX, y + 3);
            lblPurpose.Size = new Size(labelW, 24);
            cmbPurpose.Location = new Point(comboX, y);
            cmbPurpose.Size = new Size(comboW, 30);
            y += 40;

            lblMatchBy.Text = "Match-by field:";
            lblMatchBy.Location = new Point(labelX, y + 3);
            lblMatchBy.Size = new Size(labelW, 24);
            cmbMatchBy.Location = new Point(comboX, y);
            cmbMatchBy.Size = new Size(comboW, 30);
            lblMatchBy.Visible = false;
            cmbMatchBy.Visible = false;
            y += 40;

            chkUpdateEmpty.Text = "Overwrite empty fields on update";
            chkUpdateEmpty.Location = new Point(comboX, y);
            chkUpdateEmpty.Size = new Size(250, 24);
            chkUpdateEmpty.Visible = false;

            // uc_ImportStep1_Configure
            Controls.Add(_rootPanel);
            AutoScaleMode = AutoScaleMode.None;

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
        private BeepLabel lblRowCount;
        private BeepButton btnRefreshCount;
        private BeepLabel lblDestDS;
        private BeepComboBox cmbDestDS;
        private BeepLabel lblDestEntity;
        private BeepComboBox cmbDestEntity;
        private BeepCheckBoxBool chkCreateDest;
        private BeepLabel lblPurpose;
        private BeepComboBox cmbPurpose;
        private BeepLabel lblMatchBy;
        private BeepComboBox cmbMatchBy;
        private BeepCheckBoxBool chkUpdateEmpty;
    }
}
