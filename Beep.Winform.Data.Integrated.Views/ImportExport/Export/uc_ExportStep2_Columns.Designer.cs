namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Export
{
    partial class uc_ExportStep2_Columns
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
            _split = new SplitContainer();
            _leftPanel = new BeepPanel();
            _rightPanel = new BeepPanel();
            lblTitle = new BeepLabel();
            previewGrid = new BeepGridPro();
            colGrid = new DataGridView();
            btnSelectAll = new BeepButton();
            btnSelectNone = new BeepButton();

            _split.Panel1.SuspendLayout();
            _split.Panel2.SuspendLayout();
            _split.SuspendLayout();
            _rootPanel.SuspendLayout();
            SuspendLayout();

            _rootPanel.Controls.Add(_headerPanel);
            _rootPanel.Controls.Add(_split);
            _rootPanel.Dock = DockStyle.Fill;

            _headerPanel.Controls.Add(lblTitle);
            _headerPanel.Dock = DockStyle.Top;
            _headerPanel.Height = 50;
            lblTitle.Text = "Export: Select Columns";
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            _split.Dock = DockStyle.Fill;
            _split.SplitterDistance = 300;

            _leftPanel.Controls.Add(previewGrid);
            _leftPanel.Dock = DockStyle.Fill;
            previewGrid.Dock = DockStyle.Fill;

            _rightPanel.Controls.Add(btnSelectAll);
            _rightPanel.Controls.Add(btnSelectNone);
            _rightPanel.Controls.Add(colGrid);
            _rightPanel.Dock = DockStyle.Fill;
            btnSelectAll.Text = "Select All"; btnSelectAll.Dock = DockStyle.Top;
            btnSelectNone.Text = "Select None"; btnSelectNone.Dock = DockStyle.Top;
            colGrid.Dock = DockStyle.Fill;
            colGrid.AllowUserToAddRows = false;
            colGrid.RowHeadersVisible = false;

            _split.Panel1.Controls.Add(_leftPanel);
            _split.Panel2.Controls.Add(_rightPanel);

            Controls.Add(_rootPanel);
            _split.Panel1.ResumeLayout(false);
            _split.Panel2.ResumeLayout(false);
            _split.ResumeLayout(false);
            _rootPanel.ResumeLayout(false);
            SuspendLayout();
        }

        private BeepPanel _rootPanel;
        private BeepPanel _headerPanel;
        private SplitContainer _split;
        private BeepPanel _leftPanel;
        private BeepPanel _rightPanel;
        private BeepLabel lblTitle;
        private BeepGridPro previewGrid;
        private DataGridView colGrid;
        private BeepButton btnSelectAll;
        private BeepButton btnSelectNone;
    }
}
