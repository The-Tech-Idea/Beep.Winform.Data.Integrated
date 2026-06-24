namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    partial class uc_ImportExportLauncher
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
            _buttonPanel = new BeepPanel();
            _historyPanel = new BeepPanel();
            lblTitle = new BeepLabel();
            btnImport = new BeepButton();
            btnExport = new BeepButton();
            btnRefreshHistory = new BeepButton();
            lblHistory = new BeepLabel();
            historyGrid = new BeepGridPro();

            _rootPanel.SuspendLayout();
            _buttonPanel.SuspendLayout();
            _historyPanel.SuspendLayout();
            SuspendLayout();

            _rootPanel.Controls.Add(_headerPanel);
            _rootPanel.Controls.Add(_buttonPanel);
            _rootPanel.Controls.Add(_historyPanel);
            _rootPanel.Dock = DockStyle.Fill;

            _headerPanel.Controls.Add(lblTitle);
            _headerPanel.Dock = DockStyle.Top;
            _headerPanel.Height = 60;
            lblTitle.Text = "Import / Export";
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            _buttonPanel.Controls.Add(btnImport);
            _buttonPanel.Controls.Add(btnExport);
            _buttonPanel.Dock = DockStyle.Top;
            _buttonPanel.Height = 50;
            btnImport.Text = "Import Data";
            btnImport.Location = new Point(20, 10);
            btnImport.Size = new Size(150, 35);
            btnImport.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnExport.Text = "Export Data";
            btnExport.Location = new Point(180, 10);
            btnExport.Size = new Size(150, 35);
            btnExport.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            _historyPanel.Controls.Add(lblHistory);
            _historyPanel.Controls.Add(btnRefreshHistory);
            _historyPanel.Controls.Add(historyGrid);
            _historyPanel.Dock = DockStyle.Fill;

            lblHistory.Text = "Run History";
            lblHistory.Dock = DockStyle.Top;
            lblHistory.Height = 30;
            lblHistory.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblHistory.TextAlign = ContentAlignment.BottomLeft;

            btnRefreshHistory.Text = "Refresh";
            btnRefreshHistory.Dock = DockStyle.Top;
            btnRefreshHistory.Height = 30;

            historyGrid.Dock = DockStyle.Fill;

            Controls.Add(_rootPanel);
            _rootPanel.ResumeLayout(false);
            _buttonPanel.ResumeLayout(false);
            _historyPanel.ResumeLayout(false);
            SuspendLayout();
        }

        private BeepPanel _rootPanel;
        private BeepPanel _headerPanel;
        private BeepPanel _buttonPanel;
        private BeepPanel _historyPanel;
        private BeepLabel lblTitle;
        private BeepButton btnImport;
        private BeepButton btnExport;
        private BeepButton btnRefreshHistory;
        private BeepLabel lblHistory;
        private BeepGridPro historyGrid;
    }
}
