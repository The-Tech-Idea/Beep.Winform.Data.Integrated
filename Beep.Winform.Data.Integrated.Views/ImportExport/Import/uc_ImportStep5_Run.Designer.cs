namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Import
{
    partial class uc_ImportStep5_Run
    {
        private System.ComponentModel.IContainer? components = null;

        private void InitializeComponent()
        {
            _rootPanel = new BeepPanel();
            _headerPanel = new BeepPanel();
            _summaryPanel = new BeepPanel();
            _actionPanel = new BeepPanel();
            _logPanel = new BeepPanel();
            _errorPanel = new BeepPanel();
            _progressPanel = new BeepPanel();
            _splitLog = new SplitContainer();
            lblTitle = new BeepLabel();
            lblSummary = new BeepLabel();
            lblResult = new BeepLabel();
            btnStart = new BeepButton();
            btnPause = new BeepButton();
            btnResume = new BeepButton();
            btnCancel = new BeepButton();
            btnExportErrors = new BeepButton();
            progressBar = new BeepProgressBar();
            rtbLog = new RichTextBox();
            errorGrid = new DataGridView();

            _splitLog.Panel1.SuspendLayout();
            _splitLog.Panel2.SuspendLayout();
            _splitLog.SuspendLayout();
            _rootPanel.SuspendLayout();
            SuspendLayout();

            _rootPanel.Controls.Add(_headerPanel);
            _rootPanel.Controls.Add(_summaryPanel);
            _rootPanel.Controls.Add(_progressPanel);
            _rootPanel.Controls.Add(_actionPanel);
            _rootPanel.Controls.Add(_splitLog);
            _rootPanel.Dock = DockStyle.Fill;

            _headerPanel.Controls.Add(lblTitle);
            _headerPanel.Dock = DockStyle.Top;
            _headerPanel.Height = 50;
            lblTitle.Text = "Step 5: Review, Run & Summary";
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            _summaryPanel.Controls.Add(lblSummary);
            _summaryPanel.Dock = DockStyle.Top;
            _summaryPanel.Height = 100;
            lblSummary.Text = "Configuration summary will appear here.";
            lblSummary.Dock = DockStyle.Fill;
            lblSummary.Font = new Font("Segoe UI", 9F);

            _progressPanel.Controls.Add(progressBar);
            _progressPanel.Controls.Add(lblResult);
            _progressPanel.Dock = DockStyle.Top;
            _progressPanel.Height = 50;
            progressBar.Dock = DockStyle.Top;
            progressBar.Height = 20;
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            lblResult.Text = "";
            lblResult.Dock = DockStyle.Fill;
            lblResult.TextAlign = ContentAlignment.MiddleLeft;

            _actionPanel.Controls.Add(btnStart);
            _actionPanel.Controls.Add(btnPause);
            _actionPanel.Controls.Add(btnResume);
            _actionPanel.Controls.Add(btnCancel);
            _actionPanel.Controls.Add(btnExportErrors);
            _actionPanel.Dock = DockStyle.Top;
            _actionPanel.Height = 40;

            btnStart.Text = "Start Import"; btnStart.Location = new Point(10, 5); btnStart.Size = new Size(100, 30);
            btnPause.Text = "Pause"; btnPause.Location = new Point(120, 5); btnPause.Size = new Size(70, 30); btnPause.Enabled = false;
            btnResume.Text = "Resume"; btnResume.Location = new Point(200, 5); btnResume.Size = new Size(80, 30);
            btnCancel.Text = "Cancel"; btnCancel.Location = new Point(290, 5); btnCancel.Size = new Size(80, 30); btnCancel.Enabled = false;
            btnExportErrors.Text = "Export Errors"; btnExportErrors.Location = new Point(380, 5); btnExportErrors.Size = new Size(110, 30); btnExportErrors.Enabled = false;

            _splitLog.Dock = DockStyle.Fill;
            _splitLog.Orientation = Orientation.Vertical;
            _splitLog.SplitterDistance = 400;

            _logPanel.Controls.Add(rtbLog);
            _logPanel.Dock = DockStyle.Fill;
            rtbLog.Dock = DockStyle.Fill;
            rtbLog.ReadOnly = true;

            _errorPanel.Controls.Add(errorGrid);
            _errorPanel.Dock = DockStyle.Fill;
            errorGrid.Dock = DockStyle.Fill;
            errorGrid.AllowUserToAddRows = false;
            errorGrid.RowHeadersVisible = false;
            errorGrid.Columns.Add("rowIdx", "Row");
            errorGrid.Columns.Add("field", "Field");
            errorGrid.Columns.Add("value", "Value");
            errorGrid.Columns.Add("error", "Error");

            _splitLog.Panel1.Controls.Add(_logPanel);
            _splitLog.Panel2.Controls.Add(_errorPanel);

            Controls.Add(_rootPanel);
            _splitLog.Panel1.ResumeLayout(false);
            _splitLog.Panel2.ResumeLayout(false);
            _splitLog.ResumeLayout(false);
            _rootPanel.ResumeLayout(false);
            SuspendLayout();
        }

        private BeepPanel _rootPanel;
        private BeepPanel _headerPanel;
        private BeepPanel _summaryPanel;
        private BeepPanel _actionPanel;
        private BeepPanel _logPanel;
        private BeepPanel _errorPanel;
        private BeepPanel _progressPanel;
        private SplitContainer _splitLog;
        private BeepLabel lblTitle;
        private BeepLabel lblSummary;
        private BeepLabel lblResult;
        private BeepButton btnStart;
        private BeepButton btnPause;
        private BeepButton btnResume;
        private BeepButton btnCancel;
        private BeepButton btnExportErrors;
        private BeepProgressBar progressBar;
        private RichTextBox rtbLog;
        private DataGridView errorGrid;
    }
}
