using System.ComponentModel;

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
            lblTitle = new BeepLabel();
            btnSelectAll = new BeepButton();
            btnSelectNone = new BeepButton();
            previewGrid = new BeepGridPro();
            colGrid = new DataGridView();
            _rootPanel.SuspendLayout();
            SuspendLayout();

            // _rootPanel
            _rootPanel.Controls.Add(colGrid);
            _rootPanel.Controls.Add(previewGrid);
            _rootPanel.Controls.Add(btnSelectNone);
            _rootPanel.Controls.Add(btnSelectAll);
            _rootPanel.Controls.Add(lblTitle);
            _rootPanel.Dock = DockStyle.Fill;
            _rootPanel.Location = new Point(0, 0);
            _rootPanel.Name = "_rootPanel";
            _rootPanel.Size = new Size(900, 600);

            // lblTitle
            lblTitle.Text = "Export: Select Columns";
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(860, 35);
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // btnSelectAll
            btnSelectAll.Text = "Select All";
            btnSelectAll.Location = new Point(20, 58);
            btnSelectAll.Name = "btnSelectAll";
            btnSelectAll.Size = new Size(100, 30);
            btnSelectAll.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // btnSelectNone
            btnSelectNone.Text = "Select None";
            btnSelectNone.Location = new Point(130, 58);
            btnSelectNone.Name = "btnSelectNone";
            btnSelectNone.Size = new Size(100, 30);
            btnSelectNone.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // previewGrid
            previewGrid.Location = new Point(20, 95);
            previewGrid.Name = "previewGrid";
            previewGrid.Size = new Size(420, 485);
            previewGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;

            // colGrid
            colGrid.Location = new Point(460, 95);
            colGrid.Name = "colGrid";
            colGrid.Size = new Size(420, 485);
            colGrid.AllowUserToAddRows = false;
            colGrid.RowHeadersVisible = false;
            colGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // uc_ExportStep2_Columns
            Controls.Add(_rootPanel);
            Name = "uc_ExportStep2_Columns";
            Size = new Size(900, 600);
            _rootPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private BeepPanel _rootPanel;
        private BeepLabel lblTitle;
        private BeepButton btnSelectAll;
        private BeepButton btnSelectNone;
        private BeepGridPro previewGrid;
        private DataGridView colGrid;
    }
}
