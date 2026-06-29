using System.ComponentModel;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Import
{
    partial class uc_ImportStep5_Run
    {
        private void InitializeComponent()
        {
            _rootPanel = new BeepPanel();
            lblTitle = new BeepLabel();
            lblSummary = new BeepLabel();
            progressBar = new BeepProgressBar();
            lblResult = new BeepLabel();
            btnStart = new BeepButton();
            btnPause = new BeepButton();
            btnResume = new BeepButton();
            btnCancel = new BeepButton();
            btnExportErrors = new BeepButton();
            rtbLog = new RichTextBox();
            errorGrid = new DataGridView();
            _rootPanel.SuspendLayout();
            SuspendLayout();

            // _rootPanel
            _rootPanel.Controls.Add(errorGrid);
            _rootPanel.Controls.Add(rtbLog);
            _rootPanel.Controls.Add(btnExportErrors);
            _rootPanel.Controls.Add(btnCancel);
            _rootPanel.Controls.Add(btnResume);
            _rootPanel.Controls.Add(btnPause);
            _rootPanel.Controls.Add(btnStart);
            _rootPanel.Controls.Add(lblResult);
            _rootPanel.Controls.Add(progressBar);
            _rootPanel.Controls.Add(lblSummary);
            _rootPanel.Controls.Add(lblTitle);
            _rootPanel.Dock = DockStyle.Fill;
            _rootPanel.Location = new Point(0, 0);
            _rootPanel.Name = "_rootPanel";
            _rootPanel.Size = new Size(900, 600);

            // lblTitle
            lblTitle.Text = "Step 5: Review, Run & Summary";
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(860, 35);
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // lblSummary
            lblSummary.Text = "Configuration summary will appear here.";
            lblSummary.Font = new Font("Segoe UI", 9F);
            lblSummary.Location = new Point(20, 55);
            lblSummary.Name = "lblSummary";
            lblSummary.Size = new Size(860, 70);
            lblSummary.TextAlign = ContentAlignment.TopLeft;
            lblSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // progressBar
            progressBar.Location = new Point(20, 135);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(860, 22);
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // lblResult
            lblResult.Text = "";
            lblResult.Location = new Point(20, 162);
            lblResult.Name = "lblResult";
            lblResult.Size = new Size(860, 20);
            lblResult.TextAlign = ContentAlignment.MiddleLeft;
            lblResult.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // btnStart
            btnStart.Text = "Start Import";
            btnStart.Location = new Point(20, 190);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(100, 30);
            btnStart.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // btnPause
            btnPause.Text = "Pause";
            btnPause.Location = new Point(130, 190);
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(70, 30);
            btnPause.Enabled = false;
            btnPause.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // btnResume
            btnResume.Text = "Resume";
            btnResume.Location = new Point(210, 190);
            btnResume.Name = "btnResume";
            btnResume.Size = new Size(80, 30);
            btnResume.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // btnCancel
            btnCancel.Text = "Cancel";
            btnCancel.Location = new Point(300, 190);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 30);
            btnCancel.Enabled = false;
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // btnExportErrors
            btnExportErrors.Text = "Export Errors";
            btnExportErrors.Location = new Point(390, 190);
            btnExportErrors.Name = "btnExportErrors";
            btnExportErrors.Size = new Size(110, 30);
            btnExportErrors.Enabled = false;
            btnExportErrors.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // rtbLog
            rtbLog.Location = new Point(20, 235);
            rtbLog.Name = "rtbLog";
            rtbLog.Size = new Size(420, 345);
            rtbLog.ReadOnly = true;
            rtbLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;

            // errorGrid
            errorGrid.Location = new Point(455, 235);
            errorGrid.Name = "errorGrid";
            errorGrid.Size = new Size(425, 345);
            errorGrid.AllowUserToAddRows = false;
            errorGrid.RowHeadersVisible = false;
            errorGrid.Columns.Add("rowIdx", "Row");
            errorGrid.Columns.Add("field", "Field");
            errorGrid.Columns.Add("value", "Value");
            errorGrid.Columns.Add("error", "Error");
            errorGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // uc_ImportStep5_Run
            Controls.Add(_rootPanel);
            Name = "uc_ImportStep5_Run";
            Size = new Size(900, 600);
            _rootPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private BeepPanel _rootPanel;
        private BeepLabel lblTitle;
        private BeepLabel lblSummary;
        private BeepProgressBar progressBar;
        private BeepLabel lblResult;
        private BeepButton btnStart;
        private BeepButton btnPause;
        private BeepButton btnResume;
        private BeepButton btnCancel;
        private BeepButton btnExportErrors;
        private RichTextBox rtbLog;
        private DataGridView errorGrid;
    }
}
