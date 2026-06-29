using System.ComponentModel;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Import
{
    partial class uc_ImportStep3_Mapping
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
            lblTemplateLoad = new BeepLabel();
            cmbTemplateLoad = new BeepComboBox();
            btnTemplateLoad = new BeepButton();
            btnTemplateSave = new BeepButton();
            lblMappingStatus = new BeepLabel();
            mappingGrid = new DataGridView();
            _rootPanel.SuspendLayout();
            SuspendLayout();

            // _rootPanel
            _rootPanel.Controls.Add(mappingGrid);
            _rootPanel.Controls.Add(lblMappingStatus);
            _rootPanel.Controls.Add(btnTemplateSave);
            _rootPanel.Controls.Add(btnTemplateLoad);
            _rootPanel.Controls.Add(cmbTemplateLoad);
            _rootPanel.Controls.Add(lblTemplateLoad);
            _rootPanel.Controls.Add(lblTitle);
            _rootPanel.Dock = DockStyle.Fill;
            _rootPanel.Location = new Point(0, 0);
            _rootPanel.Name = "_rootPanel";
            _rootPanel.Size = new Size(900, 600);

            // lblTitle
            lblTitle.Text = "Step 3: Map Fields";
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(860, 35);
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // lblTemplateLoad
            lblTemplateLoad.Text = "Template:";
            lblTemplateLoad.Location = new Point(20, 68);
            lblTemplateLoad.Name = "lblTemplateLoad";
            lblTemplateLoad.Size = new Size(70, 24);
            lblTemplateLoad.TextAlign = ContentAlignment.MiddleLeft;
            lblTemplateLoad.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // cmbTemplateLoad
            cmbTemplateLoad.Location = new Point(95, 65);
            cmbTemplateLoad.Name = "cmbTemplateLoad";
            cmbTemplateLoad.Size = new Size(220, 30);
            cmbTemplateLoad.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // btnTemplateLoad
            btnTemplateLoad.Text = "Load";
            btnTemplateLoad.Location = new Point(325, 63);
            btnTemplateLoad.Name = "btnTemplateLoad";
            btnTemplateLoad.Size = new Size(75, 30);
            btnTemplateLoad.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // btnTemplateSave
            btnTemplateSave.Text = "Save As";
            btnTemplateSave.Location = new Point(410, 63);
            btnTemplateSave.Name = "btnTemplateSave";
            btnTemplateSave.Size = new Size(80, 30);
            btnTemplateSave.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // lblMappingStatus
            lblMappingStatus.Text = "";
            lblMappingStatus.Location = new Point(505, 68);
            lblMappingStatus.Name = "lblMappingStatus";
            lblMappingStatus.Size = new Size(375, 24);
            lblMappingStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblMappingStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // mappingGrid
            mappingGrid.Location = new Point(20, 105);
            mappingGrid.Name = "mappingGrid";
            mappingGrid.Size = new Size(860, 475);
            mappingGrid.AllowUserToAddRows = false;
            mappingGrid.AllowUserToDeleteRows = false;
            mappingGrid.RowHeadersVisible = false;
            mappingGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            mappingGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // uc_ImportStep3_Mapping
            Controls.Add(_rootPanel);
            Name = "uc_ImportStep3_Mapping";
            Size = new Size(900, 600);
            _rootPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private BeepPanel _rootPanel;
        private BeepLabel lblTitle;
        private BeepLabel lblTemplateLoad;
        private BeepComboBox cmbTemplateLoad;
        private BeepButton btnTemplateLoad;
        private BeepButton btnTemplateSave;
        private BeepLabel lblMappingStatus;
        private DataGridView mappingGrid;
    }
}
