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
            SuspendLayout();

            // _rootPanel
            _rootPanel.Controls.Add(chkUpdateEmpty);
            _rootPanel.Controls.Add(cmbMatchBy);
            _rootPanel.Controls.Add(lblMatchBy);
            _rootPanel.Controls.Add(cmbPurpose);
            _rootPanel.Controls.Add(lblPurpose);
            _rootPanel.Controls.Add(chkCreateDest);
            _rootPanel.Controls.Add(cmbDestEntity);
            _rootPanel.Controls.Add(lblDestEntity);
            _rootPanel.Controls.Add(cmbDestDS);
            _rootPanel.Controls.Add(lblDestDS);
            _rootPanel.Controls.Add(btnRefreshCount);
            _rootPanel.Controls.Add(lblRowCount);
            _rootPanel.Controls.Add(cmbSourceEntity);
            _rootPanel.Controls.Add(lblSourceEntity);
            _rootPanel.Controls.Add(cmbSourceDS);
            _rootPanel.Controls.Add(lblSourceDS);
            _rootPanel.Controls.Add(lblTitle);
            _rootPanel.Dock = DockStyle.Fill;
            _rootPanel.Location = new Point(0, 0);
            _rootPanel.Name = "_rootPanel";
            _rootPanel.Size = new Size(900, 500);

            // lblTitle
            lblTitle.Text = "Step 1: Configure Source & Destination";
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(400, 35);
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            // lblSourceDS
            lblSourceDS.Text = "Source Data Source:";
            lblSourceDS.Location = new Point(30, 65);
            lblSourceDS.Name = "lblSourceDS";
            lblSourceDS.Size = new Size(160, 24);
            lblSourceDS.TextAlign = ContentAlignment.MiddleLeft;

            // cmbSourceDS
            cmbSourceDS.Location = new Point(200, 62);
            cmbSourceDS.Name = "cmbSourceDS";
            cmbSourceDS.Size = new Size(350, 30);

            // lblSourceEntity
            lblSourceEntity.Text = "Source Entity:";
            lblSourceEntity.Location = new Point(30, 105);
            lblSourceEntity.Name = "lblSourceEntity";
            lblSourceEntity.Size = new Size(160, 24);
            lblSourceEntity.TextAlign = ContentAlignment.MiddleLeft;

            // cmbSourceEntity
            cmbSourceEntity.Location = new Point(200, 102);
            cmbSourceEntity.Name = "cmbSourceEntity";
            cmbSourceEntity.Size = new Size(260, 30);

            // lblRowCount
            lblRowCount.Text = "N/A";
            lblRowCount.Location = new Point(470, 105);
            lblRowCount.Name = "lblRowCount";
            lblRowCount.Size = new Size(80, 24);
            lblRowCount.TextAlign = ContentAlignment.MiddleLeft;

            // btnRefreshCount
            btnRefreshCount.Text = "Refresh";
            btnRefreshCount.Location = new Point(560, 102);
            btnRefreshCount.Name = "btnRefreshCount";
            btnRefreshCount.Size = new Size(60, 30);

            // lblDestDS
            lblDestDS.Text = "Destination Data Source:";
            lblDestDS.Location = new Point(30, 145);
            lblDestDS.Name = "lblDestDS";
            lblDestDS.Size = new Size(160, 24);
            lblDestDS.TextAlign = ContentAlignment.MiddleLeft;

            // cmbDestDS
            cmbDestDS.Location = new Point(200, 142);
            cmbDestDS.Name = "cmbDestDS";
            cmbDestDS.Size = new Size(350, 30);

            // lblDestEntity
            lblDestEntity.Text = "Destination Entity:";
            lblDestEntity.Location = new Point(30, 185);
            lblDestEntity.Name = "lblDestEntity";
            lblDestEntity.Size = new Size(160, 24);
            lblDestEntity.TextAlign = ContentAlignment.MiddleLeft;

            // cmbDestEntity
            cmbDestEntity.Location = new Point(200, 182);
            cmbDestEntity.Name = "cmbDestEntity";
            cmbDestEntity.Size = new Size(350, 30);

            // chkCreateDest
            chkCreateDest.Text = "Create destination if not exists";
            chkCreateDest.Location = new Point(200, 222);
            chkCreateDest.Name = "chkCreateDest";
            chkCreateDest.Size = new Size(250, 24);
            chkCreateDest.Checked = true;

            // lblPurpose
            lblPurpose.Text = "Purpose:";
            lblPurpose.Location = new Point(30, 262);
            lblPurpose.Name = "lblPurpose";
            lblPurpose.Size = new Size(160, 24);
            lblPurpose.TextAlign = ContentAlignment.MiddleLeft;

            // cmbPurpose
            cmbPurpose.Location = new Point(200, 259);
            cmbPurpose.Name = "cmbPurpose";
            cmbPurpose.Size = new Size(350, 30);

            // lblMatchBy
            lblMatchBy.Text = "Match-by field:";
            lblMatchBy.Location = new Point(30, 302);
            lblMatchBy.Name = "lblMatchBy";
            lblMatchBy.Size = new Size(160, 24);
            lblMatchBy.TextAlign = ContentAlignment.MiddleLeft;
            lblMatchBy.Visible = false;

            // cmbMatchBy
            cmbMatchBy.Location = new Point(200, 299);
            cmbMatchBy.Name = "cmbMatchBy";
            cmbMatchBy.Size = new Size(350, 30);
            cmbMatchBy.Visible = false;

            // chkUpdateEmpty
            chkUpdateEmpty.Text = "Overwrite empty fields on update";
            chkUpdateEmpty.Location = new Point(200, 342);
            chkUpdateEmpty.Name = "chkUpdateEmpty";
            chkUpdateEmpty.Size = new Size(250, 24);
            chkUpdateEmpty.Visible = false;

            // uc_ImportStep1_Configure
            Controls.Add(_rootPanel);
            Name = "uc_ImportStep1_Configure";
            Size = new Size(900, 500);
            _rootPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private BeepPanel _rootPanel;
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
