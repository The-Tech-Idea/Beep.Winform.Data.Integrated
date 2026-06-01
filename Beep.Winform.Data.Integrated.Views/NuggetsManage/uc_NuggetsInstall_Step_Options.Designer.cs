using System.ComponentModel;
using System.Windows.Forms;
using TheTechIdea.Beep.Icons;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    partial class uc_NuggetsInstall_Step_Options
    {
        private void InitializeComponent()
        {
            _tlpRoot = new TableLayoutPanel();
            _chkLoadAfterInstall = new BeepCheckBoxBool();
            _chkSharedContext = new BeepCheckBoxBool();
            _chkUseProcessHost = new BeepCheckBoxBool();
            _lblInstallPathCap = new BeepLabel();
            _txtInstallPath = new BeepTextBox();
            _btnBrowse = new BeepButton();

            _tlpRoot.SuspendLayout();
            SuspendLayout();

            _tlpRoot.Dock = DockStyle.Fill;
            _tlpRoot.ColumnCount = 3;
            _tlpRoot.RowCount = 5;
            _tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            _tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36));
            _tlpRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _tlpRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _tlpRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _tlpRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _tlpRoot.Padding = new Padding(12);

            _chkLoadAfterInstall.Text = "Load assemblies after install";
            _chkLoadAfterInstall.AutoSize = true;
            _chkLoadAfterInstall.Anchor = AnchorStyles.Left;
            _chkLoadAfterInstall.CurrentValue = true;
            _chkLoadAfterInstall.Margin = new Padding(0, 0, 0, 8);
            _tlpRoot.SetColumnSpan(_chkLoadAfterInstall, 3);

            _chkSharedContext.Text = "Use single shared assembly context";
            _chkSharedContext.AutoSize = true;
            _chkSharedContext.Anchor = AnchorStyles.Left;
            _chkSharedContext.CurrentValue = true;
            _chkSharedContext.Margin = new Padding(0, 0, 0, 8);
            _tlpRoot.SetColumnSpan(_chkSharedContext, 3);

            _chkUseProcessHost.Text = "Use isolated process host";
            _chkUseProcessHost.AutoSize = true;
            _chkUseProcessHost.Anchor = AnchorStyles.Left;
            _chkUseProcessHost.Margin = new Padding(0, 0, 0, 12);
            _tlpRoot.SetColumnSpan(_chkUseProcessHost, 3);

            _lblInstallPathCap.Text = "Install path:";
            _lblInstallPathCap.AutoSize = true;
            _lblInstallPathCap.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _lblInstallPathCap.Margin = new Padding(0, 0, 6, 0);

            _txtInstallPath.Dock = DockStyle.Fill;
            _txtInstallPath.PlaceholderText = "Default application path";
            _txtInstallPath.Margin = new Padding(0, 0, 6, 0);

            _btnBrowse.Text = "…";
            _btnBrowse.ImagePath = SvgsUIcons.Folders.Open;
            _btnBrowse.AutoSize = true;
            _btnBrowse.Dock = DockStyle.Fill;
            _btnBrowse.Click += BtnBrowse_Click;

            _tlpRoot.Controls.Add(_chkLoadAfterInstall, 0, 0);
            _tlpRoot.Controls.Add(_chkSharedContext, 0, 1);
            _tlpRoot.Controls.Add(_chkUseProcessHost, 0, 2);
            _tlpRoot.Controls.Add(_lblInstallPathCap, 0, 3);
            _tlpRoot.Controls.Add(_txtInstallPath, 1, 3);
            _tlpRoot.Controls.Add(_btnBrowse, 2, 3);

            Controls.Add(_tlpRoot);
            Name = "uc_NuggetsInstall_Step_Options";

            _tlpRoot.ResumeLayout(false);
            ResumeLayout(false);
        }

        private TableLayoutPanel _tlpRoot;
        private BeepCheckBoxBool _chkLoadAfterInstall;
        private BeepCheckBoxBool _chkSharedContext;
        private BeepCheckBoxBool _chkUseProcessHost;
        private BeepLabel _lblInstallPathCap;
        private BeepTextBox _txtInstallPath;
        private BeepButton _btnBrowse;
    }
}
