namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Import
{
    partial class uc_ImportStep2_Columns
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
            _splitPanel = new SplitContainer();
            _leftPanel = new BeepPanel();
            _rightPanel = new BeepPanel();
            lblTitle = new BeepLabel();
            lblPreview = new BeepLabel();
            previewGrid = new BeepGridPro();
            colSelectionGrid = new DataGridView();
            btnSelectAll = new BeepButton();
            btnSelectNone = new BeepButton();

            _splitPanel.Panel1.SuspendLayout();
            _splitPanel.Panel2.SuspendLayout();
            _splitPanel.SuspendLayout();
            _rootPanel.SuspendLayout();
            SuspendLayout();

            _rootPanel.Controls.Add(_headerPanel);
            _rootPanel.Controls.Add(_splitPanel);
            _rootPanel.Dock = DockStyle.Fill;

            _headerPanel.Controls.Add(lblTitle);
            _headerPanel.Dock = DockStyle.Top;
            _headerPanel.Height = 50;
            lblTitle.Text = "Step 2: Select Columns";
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            _splitPanel.Dock = DockStyle.Fill;
            _splitPanel.SplitterDistance = 300;
            _splitPanel.Orientation = Orientation.Vertical;

            _leftPanel.Controls.Add(lblPreview);
            _leftPanel.Controls.Add(previewGrid);
            _leftPanel.Dock = DockStyle.Fill;
            lblPreview.Dock = DockStyle.Top;
            lblPreview.Text = "Preview";
            lblPreview.Height = 25;
            previewGrid.Dock = DockStyle.Fill;

            _rightPanel.Controls.Add(btnSelectAll);
            _rightPanel.Controls.Add(btnSelectNone);
            _rightPanel.Controls.Add(colSelectionGrid);
            _rightPanel.Dock = DockStyle.Fill;
            btnSelectAll.Text = "Select All";
            btnSelectAll.Dock = DockStyle.Top;
            btnSelectNone.Text = "Select None";
            btnSelectNone.Dock = DockStyle.Top;
            colSelectionGrid.Dock = DockStyle.Fill;
            colSelectionGrid.AllowUserToAddRows = false;
            colSelectionGrid.RowHeadersVisible = false;

            _splitPanel.Panel1.Controls.Add(_leftPanel);
            _splitPanel.Panel2.Controls.Add(_rightPanel);

            Controls.Add(_rootPanel);
            _splitPanel.Panel1.ResumeLayout(false);
            _splitPanel.Panel2.ResumeLayout(false);
            _splitPanel.ResumeLayout(false);
            _rootPanel.ResumeLayout(false);
            SuspendLayout();
        }

        private BeepPanel _rootPanel;
        private BeepPanel _headerPanel;
        private SplitContainer _splitPanel;
        private BeepPanel _leftPanel;
        private BeepPanel _rightPanel;
        private BeepLabel lblTitle;
        private BeepLabel lblPreview;
        private BeepGridPro previewGrid;
        private DataGridView colSelectionGrid;
        private BeepButton btnSelectAll;
        private BeepButton btnSelectNone;
    }
}
