using System.ComponentModel;

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
            SuspendLayout();

            // _rootPanel
            _rootPanel.Controls.Add(cmbDestEntity);
            _rootPanel.Controls.Add(lblDestEntity);
            _rootPanel.Controls.Add(cmbDestDS);
            _rootPanel.Controls.Add(lblDestDS);
            _rootPanel.Controls.Add(btnBrowse);
            _rootPanel.Controls.Add(txtFilePath);
            _rootPanel.Controls.Add(lblFilePath);
            _rootPanel.Controls.Add(cmbFormat);
            _rootPanel.Controls.Add(lblFormat);
            _rootPanel.Controls.Add(radioDestMode);
            _rootPanel.Controls.Add(lblDestMode);
            _rootPanel.Controls.Add(cmbSourceEntity);
            _rootPanel.Controls.Add(lblSourceEntity);
            _rootPanel.Controls.Add(cmbSourceDS);
            _rootPanel.Controls.Add(lblSourceDS);
            _rootPanel.Controls.Add(lblTitle);
            _rootPanel.Dock = DockStyle.Fill;
            _rootPanel.Location = new Point(0, 0);
            _rootPanel.Name = "_rootPanel";
            _rootPanel.Size = new Size(900, 600);

            // lblTitle
            lblTitle.Text = "Export: Select Source & Target";
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(860, 35);
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // lblSourceDS
            lblSourceDS.Text = "Source Data Source:";
            lblSourceDS.Location = new Point(30, 68);
            lblSourceDS.Name = "lblSourceDS";
            lblSourceDS.Size = new Size(160, 24);
            lblSourceDS.TextAlign = ContentAlignment.MiddleLeft;
            lblSourceDS.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // cmbSourceDS
            cmbSourceDS.Location = new Point(200, 65);
            cmbSourceDS.Name = "cmbSourceDS";
            cmbSourceDS.Size = new Size(350, 30);
            cmbSourceDS.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // lblSourceEntity
            lblSourceEntity.Text = "Source Entity:";
            lblSourceEntity.Location = new Point(30, 108);
            lblSourceEntity.Name = "lblSourceEntity";
            lblSourceEntity.Size = new Size(160, 24);
            lblSourceEntity.TextAlign = ContentAlignment.MiddleLeft;
            lblSourceEntity.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // cmbSourceEntity
            cmbSourceEntity.Location = new Point(200, 105);
            cmbSourceEntity.Name = "cmbSourceEntity";
            cmbSourceEntity.Size = new Size(350, 30);
            cmbSourceEntity.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // lblDestMode
            lblDestMode.Text = "Destination:";
            lblDestMode.Location = new Point(30, 153);
            lblDestMode.Name = "lblDestMode";
            lblDestMode.Size = new Size(160, 24);
            lblDestMode.TextAlign = ContentAlignment.MiddleLeft;
            lblDestMode.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // radioDestMode
            radioDestMode.Location = new Point(200, 150);
            radioDestMode.Name = "radioDestMode";
            radioDestMode.Size = new Size(300, 30);
            radioDestMode.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // lblFormat
            lblFormat.Text = "Format:";
            lblFormat.Location = new Point(30, 198);
            lblFormat.Name = "lblFormat";
            lblFormat.Size = new Size(160, 24);
            lblFormat.TextAlign = ContentAlignment.MiddleLeft;
            lblFormat.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // cmbFormat
            cmbFormat.Location = new Point(200, 195);
            cmbFormat.Name = "cmbFormat";
            cmbFormat.Size = new Size(150, 30);
            cmbFormat.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // lblFilePath
            lblFilePath.Text = "File Path:";
            lblFilePath.Location = new Point(30, 238);
            lblFilePath.Name = "lblFilePath";
            lblFilePath.Size = new Size(160, 24);
            lblFilePath.TextAlign = ContentAlignment.MiddleLeft;
            lblFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // txtFilePath
            txtFilePath.Location = new Point(200, 235);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.Size = new Size(310, 30);
            txtFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // btnBrowse
            btnBrowse.Text = "...";
            btnBrowse.Location = new Point(520, 235);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(30, 30);
            btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // lblDestDS
            lblDestDS.Text = "Dest Data Source:";
            lblDestDS.Location = new Point(30, 283);
            lblDestDS.Name = "lblDestDS";
            lblDestDS.Size = new Size(160, 24);
            lblDestDS.TextAlign = ContentAlignment.MiddleLeft;
            lblDestDS.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // cmbDestDS
            cmbDestDS.Location = new Point(200, 280);
            cmbDestDS.Name = "cmbDestDS";
            cmbDestDS.Size = new Size(350, 30);
            cmbDestDS.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // lblDestEntity
            lblDestEntity.Text = "Dest Entity:";
            lblDestEntity.Location = new Point(30, 323);
            lblDestEntity.Name = "lblDestEntity";
            lblDestEntity.Size = new Size(160, 24);
            lblDestEntity.TextAlign = ContentAlignment.MiddleLeft;
            lblDestEntity.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // cmbDestEntity
            cmbDestEntity.Location = new Point(200, 320);
            cmbDestEntity.Name = "cmbDestEntity";
            cmbDestEntity.Size = new Size(350, 30);
            cmbDestEntity.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // uc_ExportStep1_Select
            Controls.Add(_rootPanel);
            Name = "uc_ExportStep1_Select";
            Size = new Size(900, 600);
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
