using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.GridX;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    partial class uc_MappingEditor
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

            _selectorPanel = new BeepPanel();
            _selectorTable = new System.Windows.Forms.TableLayoutPanel();
            _lblSourceConn = new BeepLabel();
            _cboSourceConn = new BeepComboBox();
            _lblSourceEntity = new BeepLabel();
            _cboSourceEntity = new BeepComboBox();
            _lblDestConn = new BeepLabel();
            _cboDestConn = new BeepComboBox();
            _lblDestEntity = new BeepLabel();
            _cboDestEntity = new BeepComboBox();

            _toolbarPanel = new BeepPanel();
            _toolbarFlow = new System.Windows.Forms.FlowLayoutPanel();
            _btnCreate = new BeepButton();
            _btnLoad = new BeepButton();

            _contentHost = new BeepPanel();
            _gridFields = new BeepGridPro();

            _actionsPanel = new BeepPanel();
            _actionsFlow = new System.Windows.Forms.FlowLayoutPanel();
            _btnSave = new BeepButton();
            _btnValidate = new BeepButton();
            _lblStatus = new BeepLabel();

            _rootPanel.SuspendLayout();
            _headerPanel.SuspendLayout();
            _selectorPanel.SuspendLayout();
            _selectorTable.SuspendLayout();
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
            _lblSubtitle.Text = "Map source fields onto a destination entity.";

            _lblTitle.UseThemeColors = true;
            _lblTitle.IsFrameless = true;
            _lblTitle.AutoEllipsis = true;
            _lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            _lblTitle.Height = 40;
            _lblTitle.Padding = new System.Windows.Forms.Padding(16, 12, 16, 0);
            _lblTitle.Text = "Mapping Editor";

            _headerPanel.Controls.Add(_lblSubtitle);
            _headerPanel.Controls.Add(_lblTitle);

            // ── selector ──
            _selectorPanel.IsFrameless = true;
            _selectorPanel.ShowTitle = false;
            _selectorPanel.ShowTitleLine = false;
            _selectorPanel.UseThemeColors = true;
            _selectorPanel.Dock = System.Windows.Forms.DockStyle.Top;
            _selectorPanel.Height = 84;
            _selectorPanel.Padding = new System.Windows.Forms.Padding(8);

            _selectorTable.Dock = System.Windows.Forms.DockStyle.Fill;
            _selectorTable.ColumnCount = 4;
            _selectorTable.RowCount = 2;
            _selectorTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            _selectorTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            _selectorTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            _selectorTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            _selectorTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            _selectorTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));

            _lblSourceConn.UseThemeColors = true;
            _lblSourceConn.IsFrameless = true;
            _lblSourceConn.Text = "Source connection";
            _lblSourceConn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblSourceConn.Dock = System.Windows.Forms.DockStyle.Fill;
            _cboSourceConn.UseThemeColors = true;
            _cboSourceConn.Dock = System.Windows.Forms.DockStyle.Fill;

            _lblSourceEntity.UseThemeColors = true;
            _lblSourceEntity.IsFrameless = true;
            _lblSourceEntity.Text = "Source entity";
            _lblSourceEntity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblSourceEntity.Dock = System.Windows.Forms.DockStyle.Fill;
            _cboSourceEntity.UseThemeColors = true;
            _cboSourceEntity.Dock = System.Windows.Forms.DockStyle.Fill;

            _lblDestConn.UseThemeColors = true;
            _lblDestConn.IsFrameless = true;
            _lblDestConn.Text = "Destination connection";
            _lblDestConn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblDestConn.Dock = System.Windows.Forms.DockStyle.Fill;
            _cboDestConn.UseThemeColors = true;
            _cboDestConn.Dock = System.Windows.Forms.DockStyle.Fill;

            _lblDestEntity.UseThemeColors = true;
            _lblDestEntity.IsFrameless = true;
            _lblDestEntity.Text = "Destination entity";
            _lblDestEntity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblDestEntity.Dock = System.Windows.Forms.DockStyle.Fill;
            _cboDestEntity.UseThemeColors = true;
            _cboDestEntity.Dock = System.Windows.Forms.DockStyle.Fill;

            _selectorTable.Controls.Add(_lblSourceConn, 0, 0);
            _selectorTable.Controls.Add(_cboSourceConn, 1, 0);
            _selectorTable.Controls.Add(_lblSourceEntity, 2, 0);
            _selectorTable.Controls.Add(_cboSourceEntity, 3, 0);
            _selectorTable.Controls.Add(_lblDestConn, 0, 1);
            _selectorTable.Controls.Add(_cboDestConn, 1, 1);
            _selectorTable.Controls.Add(_lblDestEntity, 2, 1);
            _selectorTable.Controls.Add(_cboDestEntity, 3, 1);
            _selectorPanel.Controls.Add(_selectorTable);

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

            _btnCreate.UseThemeColors = true;
            _btnCreate.Text = "Create Map";
            _btnCreate.MinimumSize = new System.Drawing.Size(110, 32);

            _btnLoad.UseThemeColors = true;
            _btnLoad.Text = "Load Saved";
            _btnLoad.MinimumSize = new System.Drawing.Size(110, 32);

            _toolbarFlow.Controls.Add(_btnCreate);
            _toolbarFlow.Controls.Add(_btnLoad);
            _toolbarPanel.Controls.Add(_toolbarFlow);

            // ── content ──
            _contentHost.IsFrameless = true;
            _contentHost.ShowTitle = false;
            _contentHost.ShowTitleLine = false;
            _contentHost.UseThemeColors = true;
            _contentHost.Dock = System.Windows.Forms.DockStyle.Fill;
            _contentHost.Padding = new System.Windows.Forms.Padding(8);

            _gridFields.Dock = System.Windows.Forms.DockStyle.Fill;
            _contentHost.Controls.Add(_gridFields);

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
            _btnSave.Text = "Save Mapping";
            _btnSave.MinimumSize = new System.Drawing.Size(130, 36);

            _btnValidate.UseThemeColors = true;
            _btnValidate.Text = "Validate";
            _btnValidate.MinimumSize = new System.Drawing.Size(100, 32);

            _lblStatus.UseThemeColors = true;
            _lblStatus.IsFrameless = true;
            _lblStatus.AutoEllipsis = true;
            _lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblStatus.Dock = System.Windows.Forms.DockStyle.Left;
            _lblStatus.Width = 360;
            _lblStatus.Text = string.Empty;

            _actionsFlow.Controls.Add(_btnSave);
            _actionsFlow.Controls.Add(_btnValidate);
            _actionsPanel.Controls.Add(_actionsFlow);
            _actionsPanel.Controls.Add(_lblStatus);

            // Docked children resolve in reverse z-order: Fill first, edges last.
            _rootPanel.Controls.Add(_contentHost);
            _rootPanel.Controls.Add(_toolbarPanel);
            _rootPanel.Controls.Add(_selectorPanel);
            _rootPanel.Controls.Add(_headerPanel);
            _rootPanel.Controls.Add(_actionsPanel);

            // ── uc_MappingEditor ──
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(_rootPanel);
            Name = "uc_MappingEditor";
            Size = new System.Drawing.Size(840, 560);

            _actionsFlow.ResumeLayout(false);
            _actionsPanel.ResumeLayout(false);
            _contentHost.ResumeLayout(false);
            _toolbarFlow.ResumeLayout(false);
            _toolbarPanel.ResumeLayout(false);
            _selectorTable.ResumeLayout(false);
            _selectorPanel.ResumeLayout(false);
            _headerPanel.ResumeLayout(false);
            _rootPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private BeepPanel _rootPanel;
        private BeepPanel _headerPanel;
        private BeepLabel _lblTitle;
        private BeepLabel _lblSubtitle;

        private BeepPanel _selectorPanel;
        private System.Windows.Forms.TableLayoutPanel _selectorTable;
        private BeepLabel _lblSourceConn;
        private BeepComboBox _cboSourceConn;
        private BeepLabel _lblSourceEntity;
        private BeepComboBox _cboSourceEntity;
        private BeepLabel _lblDestConn;
        private BeepComboBox _cboDestConn;
        private BeepLabel _lblDestEntity;
        private BeepComboBox _cboDestEntity;

        private BeepPanel _toolbarPanel;
        private System.Windows.Forms.FlowLayoutPanel _toolbarFlow;
        private BeepButton _btnCreate;
        private BeepButton _btnLoad;

        private BeepPanel _contentHost;
        private BeepGridPro _gridFields;

        private BeepPanel _actionsPanel;
        private System.Windows.Forms.FlowLayoutPanel _actionsFlow;
        private BeepButton _btnSave;
        private BeepButton _btnValidate;
        private BeepLabel _lblStatus;
    }
}
