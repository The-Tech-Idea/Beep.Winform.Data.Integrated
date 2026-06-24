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
            _headerPanel = new BeepPanel();
            _toolbarPanel = new BeepPanel();
            _gridPanel = new BeepPanel();
            lblTitle = new BeepLabel();
            lblMappingStatus = new BeepLabel();
            lblTemplateLoad = new BeepLabel();
            cmbTemplateLoad = new BeepComboBox();
            btnTemplateSave = new BeepButton();
            btnTemplateLoad = new BeepButton();
            mappingGrid = new DataGridView();

            _rootPanel.SuspendLayout();
            _toolbarPanel.SuspendLayout();
            _gridPanel.SuspendLayout();
            SuspendLayout();

            _rootPanel.Controls.Add(_headerPanel);
            _rootPanel.Controls.Add(_toolbarPanel);
            _rootPanel.Controls.Add(_gridPanel);
            _rootPanel.Dock = DockStyle.Fill;

            _headerPanel.Controls.Add(lblTitle);
            _headerPanel.Dock = DockStyle.Top;
            _headerPanel.Height = 50;
            lblTitle.Text = "Step 3: Map Fields";
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            _toolbarPanel.Controls.Add(lblMappingStatus);
            _toolbarPanel.Controls.Add(lblTemplateLoad);
            _toolbarPanel.Controls.Add(cmbTemplateLoad);
            _toolbarPanel.Controls.Add(btnTemplateLoad);
            _toolbarPanel.Controls.Add(btnTemplateSave);
            _toolbarPanel.Dock = DockStyle.Top;
            _toolbarPanel.Height = 40;

            lblTemplateLoad.Text = "Template:";
            lblTemplateLoad.Location = new Point(10, 10);
            lblTemplateLoad.Size = new Size(60, 20);
            cmbTemplateLoad.Location = new Point(75, 7);
            cmbTemplateLoad.Size = new Size(200, 26);
            btnTemplateLoad.Text = "Load";
            btnTemplateLoad.Location = new Point(285, 5);
            btnTemplateLoad.Size = new Size(70, 28);
            btnTemplateSave.Text = "Save As";
            btnTemplateSave.Location = new Point(365, 5);
            btnTemplateSave.Size = new Size(70, 28);
            lblMappingStatus.Text = "";
            lblMappingStatus.Location = new Point(450, 10);
            lblMappingStatus.Size = new Size(300, 20);

            _gridPanel.Controls.Add(mappingGrid);
            _gridPanel.Dock = DockStyle.Fill;
            mappingGrid.Dock = DockStyle.Fill;
            mappingGrid.AllowUserToAddRows = false;
            mappingGrid.AllowUserToDeleteRows = false;
            mappingGrid.RowHeadersVisible = false;
            mappingGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            Controls.Add(_rootPanel);
            _rootPanel.ResumeLayout(false);
            _toolbarPanel.ResumeLayout(false);
            _gridPanel.ResumeLayout(false);
            SuspendLayout();
        }

        private BeepPanel _rootPanel;
        private BeepPanel _headerPanel;
        private BeepPanel _toolbarPanel;
        private BeepPanel _gridPanel;
        private BeepLabel lblTitle;
        private BeepLabel lblMappingStatus;
        private BeepLabel lblTemplateLoad;
        private BeepComboBox cmbTemplateLoad;
        private BeepButton btnTemplateSave;
        private BeepButton btnTemplateLoad;
        private DataGridView mappingGrid;
    }
}
