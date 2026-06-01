namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    partial class uc_Export_ColumnSelection
    {
        private System.ComponentModel.IContainer components = null;
        private Panel mainPanel;
        private Panel gridPanel;
        private Panel previewPanel;
        private TheTechIdea.Beep.Winform.Controls.GridX.BeepGridPro colGrid;
        private TheTechIdea.Beep.Winform.Controls.GridX.BeepGridPro previewGrid;
        private TheTechIdea.Beep.Winform.Controls.BeepButton btnSelectAll;
        private TheTechIdea.Beep.Winform.Controls.BeepButton btnSelectNone;
        private TheTechIdea.Beep.Winform.Controls.BeepLabel lblPreview;
        private Splitter splitter;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.mainPanel = new Panel();
            this.gridPanel = new Panel();
            this.previewPanel = new Panel();
            this.colGrid = new TheTechIdea.Beep.Winform.Controls.GridX.BeepGridPro();
            this.previewGrid = new TheTechIdea.Beep.Winform.Controls.GridX.BeepGridPro();
            this.btnSelectAll = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            this.btnSelectNone = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            this.lblPreview = new TheTechIdea.Beep.Winform.Controls.BeepLabel();
            this.splitter = new Splitter();

            this.SuspendLayout();

            // Main panel
            this.mainPanel.Dock = DockStyle.Fill;

            // Grid panel (left)
            this.gridPanel.Dock = DockStyle.Left;
            this.gridPanel.Width = 350;
            this.gridPanel.Controls.Add(this.btnSelectNone);
            this.gridPanel.Controls.Add(this.btnSelectAll);
            this.gridPanel.Controls.Add(this.colGrid);

            this.btnSelectAll.Text = "Select All";
            this.btnSelectAll.Location = new System.Drawing.Point(8, 8);
            this.btnSelectAll.Size = new System.Drawing.Size(80, 24);
            this.btnSelectAll.Click += new System.EventHandler(this.SelectAll_Click);

            this.btnSelectNone.Text = "Select None";
            this.btnSelectNone.Location = new System.Drawing.Point(96, 8);
            this.btnSelectNone.Size = new System.Drawing.Size(80, 24);
            this.btnSelectNone.Click += new System.EventHandler(this.SelectNone_Click);

            this.colGrid.Dock = DockStyle.Bottom;
            this.colGrid.Height = 400;
            this.colGrid.Columns.Add(new TheTechIdea.Beep.Winform.Controls.Models.BeepColumnConfig { ColumnName = "Selected", ColumnCaption = "", Width = 30, IsSelectionCheckBox = true });
            this.colGrid.Columns.Add(new TheTechIdea.Beep.Winform.Controls.Models.BeepColumnConfig { ColumnName = "ColumnName", ColumnCaption = "Column", Width = 120 });
            this.colGrid.Columns.Add(new TheTechIdea.Beep.Winform.Controls.Models.BeepColumnConfig { ColumnName = "DataType", ColumnCaption = "Type", Width = 80 });
            this.colGrid.Columns.Add(new TheTechIdea.Beep.Winform.Controls.Models.BeepColumnConfig { ColumnName = "SampleValue", ColumnCaption = "Sample", Width = 120 });

            // Preview panel (right)
            this.previewPanel.Dock = DockStyle.Fill;
            this.previewPanel.Controls.Add(this.previewGrid);
            this.previewPanel.Controls.Add(this.lblPreview);

            this.lblPreview.Text = "Preview (first 5 rows)";
            this.lblPreview.Dock = DockStyle.Top;
            this.lblPreview.Height = 24;

            this.previewGrid.Dock = DockStyle.Fill;
            this.previewGrid.ReadOnly = true;

            // Splitter
            this.splitter.Dock = DockStyle.Left;
            this.splitter.Width = 4;
            this.splitter.BackColor = System.Drawing.Color.LightGray;

            // Assemble
            this.mainPanel.Controls.Add(this.previewPanel);
            this.mainPanel.Controls.Add(this.splitter);
            this.mainPanel.Controls.Add(this.gridPanel);

            this.Controls.Add(this.mainPanel);

            this.Size = new System.Drawing.Size(800, 500);
            this.ResumeLayout(false);
        }
    }
}
