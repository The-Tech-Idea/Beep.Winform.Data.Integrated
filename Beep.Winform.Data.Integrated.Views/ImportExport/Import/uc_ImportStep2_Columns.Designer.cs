using System.ComponentModel;

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
            lblTitle = new BeepLabel();
            lblPreview = new BeepLabel();
            btnSelectAll = new BeepButton();
            btnSelectNone = new BeepButton();
            previewGrid = new BeepGridPro();
            colSelectionGrid = new DataGridView();
            _rootPanel.SuspendLayout();
            SuspendLayout();

            // _rootPanel
            _rootPanel.Controls.Add(colSelectionGrid);
            _rootPanel.Controls.Add(previewGrid);
            _rootPanel.Controls.Add(btnSelectNone);
            _rootPanel.Controls.Add(btnSelectAll);
            _rootPanel.Controls.Add(lblPreview);
            _rootPanel.Controls.Add(lblTitle);
            _rootPanel.Dock = DockStyle.Fill;
            _rootPanel.Location = new Point(0, 0);
            _rootPanel.Name = "_rootPanel";
            _rootPanel.Size = new Size(900, 600);

            // lblTitle
            lblTitle.Text = "Step 2: Select Columns";
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(860, 35);
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // lblPreview
            lblPreview.Text = "Preview";
            lblPreview.Location = new Point(20, 60);
            lblPreview.Name = "lblPreview";
            lblPreview.Size = new Size(420, 24);
            lblPreview.TextAlign = ContentAlignment.MiddleLeft;
            lblPreview.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // btnSelectAll
            btnSelectAll.Text = "Select All";
            btnSelectAll.Location = new Point(460, 60);
            btnSelectAll.Name = "btnSelectAll";
            btnSelectAll.Size = new Size(100, 30);
            btnSelectAll.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // btnSelectNone
            btnSelectNone.Text = "Select None";
            btnSelectNone.Location = new Point(570, 60);
            btnSelectNone.Name = "btnSelectNone";
            btnSelectNone.Size = new Size(100, 30);
            btnSelectNone.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // previewGrid
            previewGrid.Location = new Point(20, 88);
            previewGrid.Name = "previewGrid";
            previewGrid.Size = new Size(420, 492);
            previewGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;

            // colSelectionGrid
            colSelectionGrid.Location = new Point(460, 95);
            colSelectionGrid.Name = "colSelectionGrid";
            colSelectionGrid.Size = new Size(420, 485);
            colSelectionGrid.AllowUserToAddRows = false;
            colSelectionGrid.RowHeadersVisible = false;
            colSelectionGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // uc_ImportStep2_Columns
            Controls.Add(_rootPanel);
            Name = "uc_ImportStep2_Columns";
            Size = new Size(900, 600);
            _rootPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private BeepPanel _rootPanel;
        private BeepLabel lblTitle;
        private BeepLabel lblPreview;
        private BeepButton btnSelectAll;
        private BeepButton btnSelectNone;
        private BeepGridPro previewGrid;
        private DataGridView colSelectionGrid;
    }
}
