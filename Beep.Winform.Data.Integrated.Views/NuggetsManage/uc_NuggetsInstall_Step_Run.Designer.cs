using System.ComponentModel;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.ProgressBars;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    partial class uc_NuggetsInstall_Step_Run
    {
        private void InitializeComponent()
        {
            _tlpRoot = new TableLayoutPanel();
            _lblSummary = new BeepLabel();
            _progressBar = new BeepProgressBar();
            _lblStatus = new BeepLabel();

            _tlpRoot.SuspendLayout();
            SuspendLayout();

            _tlpRoot.Dock = DockStyle.Fill;
            _tlpRoot.ColumnCount = 1;
            _tlpRoot.RowCount = 3;
            _tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _tlpRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            _tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _tlpRoot.Padding = new Padding(12);

            _lblSummary.AutoSize = true;
            _lblSummary.Anchor = AnchorStyles.Left;
            _lblSummary.Margin = new Padding(0, 0, 0, 12);

            _progressBar.Dock = DockStyle.Fill;
            _progressBar.Visible = false;

            _lblStatus.Dock = DockStyle.Fill;
            _lblStatus.Text = string.Empty;
            _lblStatus.TextAlign = System.Drawing.ContentAlignment.TopLeft;

            _tlpRoot.Controls.Add(_lblSummary, 0, 0);
            _tlpRoot.Controls.Add(_progressBar, 0, 1);
            _tlpRoot.Controls.Add(_lblStatus, 0, 2);

            Controls.Add(_tlpRoot);
            Name = "uc_NuggetsInstall_Step_Run";

            _tlpRoot.ResumeLayout(false);
            ResumeLayout(false);
        }

        private TableLayoutPanel _tlpRoot;
        private BeepLabel _lblSummary;
        private BeepProgressBar _progressBar;
        private BeepLabel _lblStatus;
    }
}
