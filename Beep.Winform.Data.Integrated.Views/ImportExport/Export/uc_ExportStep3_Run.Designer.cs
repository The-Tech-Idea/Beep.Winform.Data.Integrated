using System.ComponentModel;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Export
{
    partial class uc_ExportStep3_Run
    {
        private System.ComponentModel.IContainer? components = null;

        private void InitializeComponent()
        {
            _rootPanel = new BeepPanel();
            lblTitle = new BeepLabel();
            btnStart = new BeepButton();
            btnCancel = new BeepButton();
            progressBar = new BeepProgressBar();
            lblResult = new BeepLabel();
            rtbLog = new RichTextBox();
            _rootPanel.SuspendLayout();
            SuspendLayout();

            // _rootPanel
            _rootPanel.Controls.Add(rtbLog);
            _rootPanel.Controls.Add(lblResult);
            _rootPanel.Controls.Add(progressBar);
            _rootPanel.Controls.Add(btnCancel);
            _rootPanel.Controls.Add(btnStart);
            _rootPanel.Controls.Add(lblTitle);
            _rootPanel.Dock = DockStyle.Fill;
            _rootPanel.Location = new Point(0, 0);
            _rootPanel.Name = "_rootPanel";
            _rootPanel.Size = new Size(900, 600);

            // lblTitle
            lblTitle.Text = "Export: Run";
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(860, 35);
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // btnStart
            btnStart.Text = "Start Export";
            btnStart.Location = new Point(20, 60);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(100, 30);
            btnStart.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // btnCancel
            btnCancel.Text = "Cancel";
            btnCancel.Location = new Point(130, 60);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 30);
            btnCancel.Enabled = false;
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // progressBar
            progressBar.Location = new Point(20, 100);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(860, 22);
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // lblResult
            lblResult.Text = "";
            lblResult.Location = new Point(20, 128);
            lblResult.Name = "lblResult";
            lblResult.Size = new Size(860, 20);
            lblResult.TextAlign = ContentAlignment.MiddleLeft;
            lblResult.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // rtbLog
            rtbLog.Location = new Point(20, 155);
            rtbLog.Name = "rtbLog";
            rtbLog.Size = new Size(860, 425);
            rtbLog.ReadOnly = true;
            rtbLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // uc_ExportStep3_Run
            Controls.Add(_rootPanel);
            Name = "uc_ExportStep3_Run";
            Size = new Size(900, 600);
            _rootPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private BeepPanel _rootPanel;
        private BeepLabel lblTitle;
        private BeepButton btnStart;
        private BeepButton btnCancel;
        private BeepProgressBar progressBar;
        private BeepLabel lblResult;
        private RichTextBox rtbLog;
    }
}
