namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Export
{
    partial class uc_ExportStep3_Run
    {
        private System.ComponentModel.IContainer? components = null;

        private void InitializeComponent()
        {
            _rootPanel = new BeepPanel();
            _headerPanel = new BeepPanel();
            _actionPanel = new BeepPanel();
            _progressPanel = new BeepPanel();
            _logPanel = new BeepPanel();
            lblTitle = new BeepLabel();
            lblResult = new BeepLabel();
            btnStart = new BeepButton();
            btnCancel = new BeepButton();
            progressBar = new BeepProgressBar();
            rtbLog = new RichTextBox();

            _rootPanel.SuspendLayout();
            SuspendLayout();

            _rootPanel.Controls.Add(_headerPanel);
            _rootPanel.Controls.Add(_actionPanel);
            _rootPanel.Controls.Add(_progressPanel);
            _rootPanel.Controls.Add(_logPanel);
            _rootPanel.Dock = DockStyle.Fill;

            _headerPanel.Controls.Add(lblTitle);
            _headerPanel.Dock = DockStyle.Top;
            _headerPanel.Height = 50;
            lblTitle.Text = "Export: Run";
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            _actionPanel.Controls.Add(btnStart);
            _actionPanel.Controls.Add(btnCancel);
            _actionPanel.Dock = DockStyle.Top;
            _actionPanel.Height = 40;
            btnStart.Text = "Start Export"; btnStart.Location = new Point(10, 5); btnStart.Size = new Size(100, 30);
            btnCancel.Text = "Cancel"; btnCancel.Location = new Point(120, 5); btnCancel.Size = new Size(80, 30); btnCancel.Enabled = false;

            _progressPanel.Controls.Add(progressBar);
            _progressPanel.Controls.Add(lblResult);
            _progressPanel.Dock = DockStyle.Top;
            _progressPanel.Height = 50;
            progressBar.Dock = DockStyle.Top;
            progressBar.Height = 20;
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            lblResult.Text = ""; lblResult.Dock = DockStyle.Fill;
            lblResult.TextAlign = ContentAlignment.MiddleLeft;

            _logPanel.Controls.Add(rtbLog);
            _logPanel.Dock = DockStyle.Fill;
            rtbLog.Dock = DockStyle.Fill;
            rtbLog.ReadOnly = true;

            Controls.Add(_rootPanel);
            _rootPanel.ResumeLayout(false);
            SuspendLayout();
        }

        private BeepPanel _rootPanel;
        private BeepPanel _headerPanel;
        private BeepPanel _actionPanel;
        private BeepPanel _progressPanel;
        private BeepPanel _logPanel;
        private BeepLabel lblTitle;
        private BeepLabel lblResult;
        private BeepButton btnStart;
        private BeepButton btnCancel;
        private BeepProgressBar progressBar;
        private RichTextBox rtbLog;
    }
}
