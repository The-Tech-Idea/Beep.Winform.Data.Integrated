using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    partial class uc_SchemaManagerWizard
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
            _contentHost = new BeepPanel();

            _stepScope = new BeepPanel();
            _scopeTable = new System.Windows.Forms.TableLayoutPanel();
            _lblSourceConn = new BeepLabel();
            _cboSourceConn = new BeepComboBox();
            _lblSourceEntity = new BeepLabel();
            _cboSourceEntity = new BeepComboBox();
            _lblDestConn = new BeepLabel();
            _cboDestConn = new BeepComboBox();
            _lblDestEntity = new BeepLabel();
            _cboDestEntity = new BeepComboBox();
            _chkAddMissingColumns = new BeepCheckBoxBool();
            _chkCreateDestination = new BeepCheckBoxBool();

            _stepResults = new BeepPanel();
            _lblResultsSummary = new BeepLabel();
            _lstResults = new BeepListBox();

            _actionsPanel = new BeepPanel();
            _actionsFlow = new System.Windows.Forms.FlowLayoutPanel();
            _btnNext = new BeepButton();
            _btnBack = new BeepButton();
            _btnCancel = new BeepButton();
            _lblStatus = new BeepLabel();

            _rootPanel.SuspendLayout();
            _headerPanel.SuspendLayout();
            _contentHost.SuspendLayout();
            _stepScope.SuspendLayout();
            _scopeTable.SuspendLayout();
            _stepResults.SuspendLayout();
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
            _lblSubtitle.Text = "Compare a source and destination entity before syncing.";

            _lblTitle.UseThemeColors = true;
            _lblTitle.IsFrameless = true;
            _lblTitle.AutoEllipsis = true;
            _lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            _lblTitle.Height = 40;
            _lblTitle.Padding = new System.Windows.Forms.Padding(16, 12, 16, 0);
            _lblTitle.Text = "Schema Manager";

            _headerPanel.Controls.Add(_lblSubtitle);
            _headerPanel.Controls.Add(_lblTitle);

            // ── content host ──
            _contentHost.IsFrameless = true;
            _contentHost.ShowTitle = false;
            _contentHost.ShowTitleLine = false;
            _contentHost.UseThemeColors = true;
            _contentHost.Dock = System.Windows.Forms.DockStyle.Fill;
            _contentHost.Padding = new System.Windows.Forms.Padding(8);

            // ── step 1: scope ──
            _stepScope.IsFrameless = true;
            _stepScope.ShowTitle = false;
            _stepScope.ShowTitleLine = false;
            _stepScope.UseThemeColors = true;
            _stepScope.Dock = System.Windows.Forms.DockStyle.Fill;

            _scopeTable.Dock = System.Windows.Forms.DockStyle.Fill;
            _scopeTable.ColumnCount = 2;
            _scopeTable.RowCount = 7;
            _scopeTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            _scopeTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            for (int i = 0; i < 6; i++)
                _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

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

            _chkAddMissingColumns.UseThemeColors = true;
            _chkAddMissingColumns.Text = "Add missing destination columns";
            _chkAddMissingColumns.Checked = true;
            _chkAddMissingColumns.Dock = System.Windows.Forms.DockStyle.Fill;

            _chkCreateDestination.UseThemeColors = true;
            _chkCreateDestination.Text = "Create destination entity if it does not exist";
            _chkCreateDestination.Checked = true;
            _chkCreateDestination.Dock = System.Windows.Forms.DockStyle.Fill;

            _scopeTable.Controls.Add(_lblSourceConn, 0, 0);
            _scopeTable.Controls.Add(_cboSourceConn, 1, 0);
            _scopeTable.Controls.Add(_lblSourceEntity, 0, 1);
            _scopeTable.Controls.Add(_cboSourceEntity, 1, 1);
            _scopeTable.Controls.Add(_lblDestConn, 0, 2);
            _scopeTable.Controls.Add(_cboDestConn, 1, 2);
            _scopeTable.Controls.Add(_lblDestEntity, 0, 3);
            _scopeTable.Controls.Add(_cboDestEntity, 1, 3);
            _scopeTable.Controls.Add(_chkAddMissingColumns, 1, 4);
            _scopeTable.Controls.Add(_chkCreateDestination, 1, 5);
            _stepScope.Controls.Add(_scopeTable);

            // ── step 2: results ──
            _stepResults.IsFrameless = true;
            _stepResults.ShowTitle = false;
            _stepResults.ShowTitleLine = false;
            _stepResults.UseThemeColors = true;
            _stepResults.Dock = System.Windows.Forms.DockStyle.Fill;
            _stepResults.Visible = false;

            _lstResults.UseThemeColors = true;
            _lstResults.ShowSearch = false;
            _lstResults.Dock = System.Windows.Forms.DockStyle.Fill;

            _lblResultsSummary.UseThemeColors = true;
            _lblResultsSummary.IsFrameless = true;
            _lblResultsSummary.AutoEllipsis = true;
            _lblResultsSummary.Dock = System.Windows.Forms.DockStyle.Top;
            _lblResultsSummary.Height = 44;
            _lblResultsSummary.Text = "Preflight has not run yet.";

            _stepResults.Controls.Add(_lstResults);
            _stepResults.Controls.Add(_lblResultsSummary);

            _contentHost.Controls.Add(_stepResults);
            _contentHost.Controls.Add(_stepScope);

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
            _btnNext.UseThemeColors = true;
            _btnNext.Text = "Run Preflight";
            _btnNext.MinimumSize = new System.Drawing.Size(130, 36);

            _btnBack.UseThemeColors = true;
            _btnBack.Text = "Back";
            _btnBack.MinimumSize = new System.Drawing.Size(100, 32);
            _btnBack.Enabled = false;

            _btnCancel.UseThemeColors = true;
            _btnCancel.Text = "Cancel";
            _btnCancel.MinimumSize = new System.Drawing.Size(100, 32);

            _lblStatus.UseThemeColors = true;
            _lblStatus.IsFrameless = true;
            _lblStatus.AutoEllipsis = true;
            _lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblStatus.Dock = System.Windows.Forms.DockStyle.Left;
            _lblStatus.Width = 320;
            _lblStatus.Text = string.Empty;

            _actionsFlow.Controls.Add(_btnNext);
            _actionsFlow.Controls.Add(_btnBack);
            _actionsFlow.Controls.Add(_btnCancel);
            _actionsPanel.Controls.Add(_actionsFlow);
            _actionsPanel.Controls.Add(_lblStatus);

            _rootPanel.Controls.Add(_contentHost);
            _rootPanel.Controls.Add(_headerPanel);
            _rootPanel.Controls.Add(_actionsPanel);

            // ── uc_SchemaManagerWizard ──
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(_rootPanel);
            Name = "uc_SchemaManagerWizard";
            Size = new System.Drawing.Size(840, 560);

            _actionsFlow.ResumeLayout(false);
            _actionsPanel.ResumeLayout(false);
            _stepResults.ResumeLayout(false);
            _scopeTable.ResumeLayout(false);
            _stepScope.ResumeLayout(false);
            _contentHost.ResumeLayout(false);
            _headerPanel.ResumeLayout(false);
            _rootPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private BeepPanel _rootPanel;
        private BeepPanel _headerPanel;
        private BeepLabel _lblTitle;
        private BeepLabel _lblSubtitle;
        private BeepPanel _contentHost;

        private BeepPanel _stepScope;
        private System.Windows.Forms.TableLayoutPanel _scopeTable;
        private BeepLabel _lblSourceConn;
        private BeepComboBox _cboSourceConn;
        private BeepLabel _lblSourceEntity;
        private BeepComboBox _cboSourceEntity;
        private BeepLabel _lblDestConn;
        private BeepComboBox _cboDestConn;
        private BeepLabel _lblDestEntity;
        private BeepComboBox _cboDestEntity;
        private BeepCheckBoxBool _chkAddMissingColumns;
        private BeepCheckBoxBool _chkCreateDestination;

        private BeepPanel _stepResults;
        private BeepLabel _lblResultsSummary;
        private BeepListBox _lstResults;

        private BeepPanel _actionsPanel;
        private System.Windows.Forms.FlowLayoutPanel _actionsFlow;
        private BeepButton _btnNext;
        private BeepButton _btnBack;
        private BeepButton _btnCancel;
        private BeepLabel _lblStatus;
    }
}
