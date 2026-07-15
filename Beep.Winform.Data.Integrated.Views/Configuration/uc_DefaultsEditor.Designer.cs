using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.GridX;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    partial class uc_DefaultsEditor
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) { components.Dispose(); }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _rootPanel = new BeepPanel();
            _headerPanel = new BeepPanel();
            _lblTitle = new BeepLabel();
            _lblSubtitle = new BeepLabel();

            _toolbarPanel = new BeepPanel();
            _toolbarFlow = new System.Windows.Forms.FlowLayoutPanel();
            _lblConnection = new BeepLabel();
            _cboConnection = new BeepComboBox();
            _btnReload = new BeepButton();
            _btnAdd = new BeepButton();
            _btnRemove = new BeepButton();

            _contentHost = new BeepPanel();
            _gridDefaults = new BeepGridPro();

            _actionsPanel = new BeepPanel();
            _actionsFlow = new System.Windows.Forms.FlowLayoutPanel();
            _btnSave = new BeepButton();
            _btnTest = new BeepButton();
            _btnValidate = new BeepButton();
            _lblStatus = new BeepLabel();

            _rootPanel.SuspendLayout();
            _headerPanel.SuspendLayout();
            _toolbarPanel.SuspendLayout();
            _toolbarFlow.SuspendLayout();
            _contentHost.SuspendLayout();
            _actionsPanel.SuspendLayout();
            _actionsFlow.SuspendLayout();
            SuspendLayout();

            // ── root ──
            _rootPanel.ControlStyle = TheTechIdea.Beep.Winform.Controls.Common.BeepControlStyle.Material3;
            _rootPanel.IsFrameless = true;
            _rootPanel.ShowTitle = false;
            _rootPanel.ShowTitleLine = false;
            _rootPanel.UseThemeColors = true;
            _rootPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _rootPanel.Padding = new System.Windows.Forms.Padding(12);

            // ── header ──
            _headerPanel.IsFrameless = true;
            _headerPanel.ShowTitle = false;
            _headerPanel.ShowTitleLine = false;
            _headerPanel.UseThemeColors = true;
            _headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            _headerPanel.Height = 76;

            _lblSubtitle.UseThemeColors = true;
            _lblSubtitle.IsFrameless = true;
            _lblSubtitle.AutoEllipsis = true;
            _lblSubtitle.Dock = System.Windows.Forms.DockStyle.Top;
            _lblSubtitle.Height = 24;
            _lblSubtitle.Padding = new System.Windows.Forms.Padding(16, 2, 16, 4);
            _lblSubtitle.Text = "Edit field-level default-value rules for a connection.";

            _lblTitle.UseThemeColors = true;
            _lblTitle.IsFrameless = true;
            _lblTitle.AutoEllipsis = true;
            _lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            _lblTitle.Height = 40;
            _lblTitle.Padding = new System.Windows.Forms.Padding(16, 12, 16, 0);
            _lblTitle.Text = "Defaults Editor";

            _headerPanel.Controls.Add(_lblSubtitle);
            _headerPanel.Controls.Add(_lblTitle);

            // ── toolbar ──
            _toolbarPanel.IsFrameless = true;
            _toolbarPanel.ShowTitle = false;
            _toolbarPanel.ShowTitleLine = false;
            _toolbarPanel.UseThemeColors = true;
            _toolbarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            _toolbarPanel.Height = 48;
            _toolbarPanel.Padding = new System.Windows.Forms.Padding(10);

            _toolbarFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            _toolbarFlow.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            _toolbarFlow.WrapContents = false;

            _lblConnection.UseThemeColors = true;
            _lblConnection.IsFrameless = true;
            _lblConnection.Text = "Connection";
            _lblConnection.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblConnection.Size = new System.Drawing.Size(80, 28);

            _cboConnection.UseThemeColors = true;
            _cboConnection.Size = new System.Drawing.Size(260, 28);

            _btnReload.UseThemeColors = true;
            _btnReload.Text = "Reload";
            _btnReload.MinimumSize = new System.Drawing.Size(100, 32);

            _btnAdd.UseThemeColors = true;
            _btnAdd.Text = "Add";
            _btnAdd.MinimumSize = new System.Drawing.Size(80, 28);

            _btnRemove.UseThemeColors = true;
            _btnRemove.Text = "Remove";
            _btnRemove.MinimumSize = new System.Drawing.Size(80, 28);

            _toolbarFlow.Controls.Add(_lblConnection);
            _toolbarFlow.Controls.Add(_cboConnection);
            _toolbarFlow.Controls.Add(_btnReload);
            _toolbarFlow.Controls.Add(_btnAdd);
            _toolbarFlow.Controls.Add(_btnRemove);
            _toolbarPanel.Controls.Add(_toolbarFlow);

            // ── content ──
            _contentHost.IsFrameless = true;
            _contentHost.ShowTitle = false;
            _contentHost.ShowTitleLine = false;
            _contentHost.UseThemeColors = true;
            _contentHost.Dock = System.Windows.Forms.DockStyle.Fill;
            _contentHost.Padding = new System.Windows.Forms.Padding(8);

            _gridDefaults.Dock = System.Windows.Forms.DockStyle.Fill;
            _contentHost.Controls.Add(_gridDefaults);

            // ── actions ──
            _actionsPanel.IsFrameless = true;
            _actionsPanel.ShowTitle = false;
            _actionsPanel.ShowTitleLine = false;
            _actionsPanel.UseThemeColors = true;
            _actionsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            _actionsPanel.Height = 52;
            _actionsPanel.Padding = new System.Windows.Forms.Padding(10);

            _actionsFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            _actionsFlow.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            _actionsFlow.WrapContents = false;

            // Flow is RightToLeft: the first child is rightmost.
            _btnSave.UseThemeColors = true;
            _btnSave.Text = "Save";
            _btnSave.MinimumSize = new System.Drawing.Size(130, 36);

            _btnTest.UseThemeColors = true;
            _btnTest.Text = "Test Rule";
            _btnTest.MinimumSize = new System.Drawing.Size(100, 32);

            _btnValidate.UseThemeColors = true;
            _btnValidate.Text = "Validate All";
            _btnValidate.MinimumSize = new System.Drawing.Size(100, 32);

            _lblStatus.UseThemeColors = true;
            _lblStatus.IsFrameless = true;
            _lblStatus.AutoEllipsis = true;
            _lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblStatus.Dock = System.Windows.Forms.DockStyle.Left;
            _lblStatus.Width = 340;
            _lblStatus.Text = string.Empty;

            _actionsFlow.Controls.Add(_btnSave);
            _actionsFlow.Controls.Add(_btnTest);
            _actionsFlow.Controls.Add(_btnValidate);
            _actionsPanel.Controls.Add(_actionsFlow);
            _actionsPanel.Controls.Add(_lblStatus);

            // Docked children resolve in reverse z-order: Fill first, edges last.
            _rootPanel.Controls.Add(_contentHost);
            _rootPanel.Controls.Add(_toolbarPanel);
            _rootPanel.Controls.Add(_headerPanel);
            _rootPanel.Controls.Add(_actionsPanel);

            // ── uc_DefaultsEditor ──
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(_rootPanel);
            Name = "uc_DefaultsEditor";
            Size = new System.Drawing.Size(840, 560);

            _actionsFlow.ResumeLayout(false);
            _actionsPanel.ResumeLayout(false);
            _contentHost.ResumeLayout(false);
            _toolbarFlow.ResumeLayout(false);
            _toolbarPanel.ResumeLayout(false);
            _headerPanel.ResumeLayout(false);
            _rootPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private BeepPanel _rootPanel;
        private BeepPanel _headerPanel;
        private BeepLabel _lblTitle;
        private BeepLabel _lblSubtitle;

        private BeepPanel _toolbarPanel;
        private System.Windows.Forms.FlowLayoutPanel _toolbarFlow;
        private BeepLabel _lblConnection;
        private BeepComboBox _cboConnection;
        private BeepButton _btnReload;
        private BeepButton _btnAdd;
        private BeepButton _btnRemove;

        private BeepPanel _contentHost;
        private BeepGridPro _gridDefaults;

        private BeepPanel _actionsPanel;
        private System.Windows.Forms.FlowLayoutPanel _actionsFlow;
        private BeepButton _btnSave;
        private BeepButton _btnTest;
        private BeepButton _btnValidate;
        private BeepLabel _lblStatus;
    }
}
