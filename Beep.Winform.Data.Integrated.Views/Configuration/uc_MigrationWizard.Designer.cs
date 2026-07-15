using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;
using TheTechIdea.Beep.Winform.Controls.GridX;
using TheTechIdea.Beep.Winform.Controls.ProgressBars;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    partial class uc_MigrationWizard
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
            _lblConnection = new BeepLabel();
            _cboConnection = new BeepComboBox();
            _lblNamespace = new BeepLabel();
            _txtNamespace = new BeepTextBox();
            _lblEnvironment = new BeepLabel();
            _cboEnvironment = new BeepComboBox();
            _chkDetectRelationships = new BeepCheckBoxBool();
            _chkApplyForeignKeys = new BeepCheckBoxBool();
            _chkApplyIndexes = new BeepCheckBoxBool();

            _stepPlan = new BeepPanel();
            _lblPlanSummary = new BeepLabel();
            _gridPlan = new BeepGridPro();

            _stepSafety = new BeepPanel();
            _lblSafetySummary = new BeepLabel();
            _lstFindings = new BeepListBox();

            _stepRun = new BeepPanel();
            _progress = new BeepProgressBar();
            _lblRunStatus = new BeepLabel();
            _lstRunLog = new BeepListBox();

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
            _stepPlan.SuspendLayout();
            _stepSafety.SuspendLayout();
            _stepRun.SuspendLayout();
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
            _lblSubtitle.Text = "Select a target connection and scope.";

            _lblTitle.UseThemeColors = true;
            _lblTitle.IsFrameless = true;
            _lblTitle.AutoEllipsis = true;
            _lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            _lblTitle.Height = 40;
            _lblTitle.Padding = new System.Windows.Forms.Padding(16, 12, 16, 0);
            _lblTitle.Text = "Migration Wizard";

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
            _scopeTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            _scopeTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            for (int i = 0; i < 6; i++)
                _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            _lblConnection.UseThemeColors = true;
            _lblConnection.IsFrameless = true;
            _lblConnection.Text = "Connection";
            _lblConnection.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblConnection.Dock = System.Windows.Forms.DockStyle.Fill;

            _cboConnection.UseThemeColors = true;
            _cboConnection.Dock = System.Windows.Forms.DockStyle.Fill;

            _lblNamespace.UseThemeColors = true;
            _lblNamespace.IsFrameless = true;
            _lblNamespace.Text = "Namespace filter";
            _lblNamespace.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblNamespace.Dock = System.Windows.Forms.DockStyle.Fill;

            _txtNamespace.UseThemeColors = true;
            _txtNamespace.PlaceholderText = "(blank = discover all entity types)";
            _txtNamespace.Dock = System.Windows.Forms.DockStyle.Fill;

            _lblEnvironment.UseThemeColors = true;
            _lblEnvironment.IsFrameless = true;
            _lblEnvironment.Text = "Environment";
            _lblEnvironment.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblEnvironment.Dock = System.Windows.Forms.DockStyle.Fill;

            _cboEnvironment.UseThemeColors = true;
            _cboEnvironment.Dock = System.Windows.Forms.DockStyle.Fill;

            _chkDetectRelationships.UseThemeColors = true;
            _chkDetectRelationships.Text = "Detect relationships";
            _chkDetectRelationships.Checked = true;
            _chkDetectRelationships.Dock = System.Windows.Forms.DockStyle.Fill;

            _chkApplyForeignKeys.UseThemeColors = true;
            _chkApplyForeignKeys.Text = "Apply foreign keys";
            _chkApplyForeignKeys.Dock = System.Windows.Forms.DockStyle.Fill;

            _chkApplyIndexes.UseThemeColors = true;
            _chkApplyIndexes.Text = "Apply indexes";
            _chkApplyIndexes.Dock = System.Windows.Forms.DockStyle.Fill;

            _scopeTable.Controls.Add(_lblConnection, 0, 0);
            _scopeTable.Controls.Add(_cboConnection, 1, 0);
            _scopeTable.Controls.Add(_lblNamespace, 0, 1);
            _scopeTable.Controls.Add(_txtNamespace, 1, 1);
            _scopeTable.Controls.Add(_lblEnvironment, 0, 2);
            _scopeTable.Controls.Add(_cboEnvironment, 1, 2);
            _scopeTable.Controls.Add(_chkDetectRelationships, 1, 3);
            _scopeTable.Controls.Add(_chkApplyForeignKeys, 1, 4);
            _scopeTable.Controls.Add(_chkApplyIndexes, 1, 5);
            _stepScope.Controls.Add(_scopeTable);

            // ── step 2: plan ──
            _stepPlan.IsFrameless = true;
            _stepPlan.ShowTitle = false;
            _stepPlan.ShowTitleLine = false;
            _stepPlan.UseThemeColors = true;
            _stepPlan.Dock = System.Windows.Forms.DockStyle.Fill;
            _stepPlan.Visible = false;

            _gridPlan.Dock = System.Windows.Forms.DockStyle.Fill;

            _lblPlanSummary.UseThemeColors = true;
            _lblPlanSummary.IsFrameless = true;
            _lblPlanSummary.AutoEllipsis = true;
            _lblPlanSummary.Dock = System.Windows.Forms.DockStyle.Top;
            _lblPlanSummary.Height = 44;
            _lblPlanSummary.Text = "No plan built yet.";

            _stepPlan.Controls.Add(_gridPlan);
            _stepPlan.Controls.Add(_lblPlanSummary);

            // ── step 3: safety ──
            _stepSafety.IsFrameless = true;
            _stepSafety.ShowTitle = false;
            _stepSafety.ShowTitleLine = false;
            _stepSafety.UseThemeColors = true;
            _stepSafety.Dock = System.Windows.Forms.DockStyle.Fill;
            _stepSafety.Visible = false;

            _lstFindings.UseThemeColors = true;
            _lstFindings.ShowSearch = false;
            _lstFindings.Dock = System.Windows.Forms.DockStyle.Fill;

            _lblSafetySummary.UseThemeColors = true;
            _lblSafetySummary.IsFrameless = true;
            _lblSafetySummary.AutoEllipsis = true;
            _lblSafetySummary.Dock = System.Windows.Forms.DockStyle.Top;
            _lblSafetySummary.Height = 44;
            _lblSafetySummary.Text = "Not validated yet.";

            _stepSafety.Controls.Add(_lstFindings);
            _stepSafety.Controls.Add(_lblSafetySummary);

            // ── step 4: run ──
            _stepRun.IsFrameless = true;
            _stepRun.ShowTitle = false;
            _stepRun.ShowTitleLine = false;
            _stepRun.UseThemeColors = true;
            _stepRun.Dock = System.Windows.Forms.DockStyle.Fill;
            _stepRun.Visible = false;

            _lstRunLog.UseThemeColors = true;
            _lstRunLog.ShowSearch = false;
            _lstRunLog.Dock = System.Windows.Forms.DockStyle.Fill;

            _lblRunStatus.UseThemeColors = true;
            _lblRunStatus.IsFrameless = true;
            _lblRunStatus.AutoEllipsis = true;
            _lblRunStatus.Dock = System.Windows.Forms.DockStyle.Top;
            _lblRunStatus.Height = 28;
            _lblRunStatus.Text = "Not started.";

            _progress.UseThemeColors = true;
            _progress.Dock = System.Windows.Forms.DockStyle.Top;
            _progress.Height = 24;
            _progress.Minimum = 0;
            _progress.Maximum = 100;

            _stepRun.Controls.Add(_lstRunLog);
            _stepRun.Controls.Add(_lblRunStatus);
            _stepRun.Controls.Add(_progress);

            // Fill-docked siblings resolve in reverse z-order; the active step is
            // brought to front at runtime.
            _contentHost.Controls.Add(_stepRun);
            _contentHost.Controls.Add(_stepSafety);
            _contentHost.Controls.Add(_stepPlan);
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
            _btnNext.Text = "Build Plan";
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

            // ── uc_MigrationWizard ──
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(_rootPanel);
            Name = "uc_MigrationWizard";
            Size = new System.Drawing.Size(840, 560);

            _actionsFlow.ResumeLayout(false);
            _actionsPanel.ResumeLayout(false);
            _stepRun.ResumeLayout(false);
            _stepSafety.ResumeLayout(false);
            _stepPlan.ResumeLayout(false);
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
        private BeepLabel _lblConnection;
        private BeepComboBox _cboConnection;
        private BeepLabel _lblNamespace;
        private BeepTextBox _txtNamespace;
        private BeepLabel _lblEnvironment;
        private BeepComboBox _cboEnvironment;
        private BeepCheckBoxBool _chkDetectRelationships;
        private BeepCheckBoxBool _chkApplyForeignKeys;
        private BeepCheckBoxBool _chkApplyIndexes;

        private BeepPanel _stepPlan;
        private BeepLabel _lblPlanSummary;
        private BeepGridPro _gridPlan;

        private BeepPanel _stepSafety;
        private BeepLabel _lblSafetySummary;
        private BeepListBox _lstFindings;

        private BeepPanel _stepRun;
        private BeepProgressBar _progress;
        private BeepLabel _lblRunStatus;
        private BeepListBox _lstRunLog;

        private BeepPanel _actionsPanel;
        private System.Windows.Forms.FlowLayoutPanel _actionsFlow;
        private BeepButton _btnNext;
        private BeepButton _btnBack;
        private BeepButton _btnCancel;
        private BeepLabel _lblStatus;
    }
}
