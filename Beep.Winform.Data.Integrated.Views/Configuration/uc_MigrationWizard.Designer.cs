using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;
using TheTechIdea.Beep.Winform.Controls.GridX;
using TheTechIdea.Beep.Winform.Controls.ProgressBars;
using TheTechIdea.Beep.Winform.Controls.Wizards.Forms;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    partial class uc_MigrationWizard
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                _runCts?.Cancel();
                _runCts?.Dispose();
                _runCts = null;
            }
            base.Dispose(disposing);
        }

        // The designer owns the four step pages and a status line only. Wizard chrome — stepper,
        // Back/Next/Cancel, progress, validation display — comes from the Wizards framework form
        // that StartWizard embeds into _hostPanel.
        private void InitializeComponent()
        {
            _hostPanel = new System.Windows.Forms.Panel();
            _lblStatus = new BeepLabel();

            _pageScope = new WizardPage();
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

            _pagePlan = new WizardPage();
            _lblPlanSummary = new BeepLabel();
            _gridPlan = new BeepGridPro();

            _pageSafety = new WizardPage();
            _lblSafetySummary = new BeepLabel();
            _lstFindings = new BeepListBox();

            _pageRun = new WizardPage();
            _progress = new BeepProgressBar();
            _lblRunStatus = new BeepLabel();
            _lstRunLog = new BeepListBox();

            _hostPanel.SuspendLayout();
            _pageScope.SuspendLayout();
            _scopeTable.SuspendLayout();
            _pagePlan.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_gridPlan).BeginInit();
            _pageSafety.SuspendLayout();
            _pageRun.SuspendLayout();
            SuspendLayout();

            // ── host for the embedded wizard form ──
            _hostPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _hostPanel.Name = "_hostPanel";

            _lblStatus.AutoEllipsis = true;
            _lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            _lblStatus.Height = 28;
            _lblStatus.IsFrameless = true;
            _lblStatus.Name = "_lblStatus";
            _lblStatus.Padding = new System.Windows.Forms.Padding(12, 4, 12, 4);
            _lblStatus.Text = "";
            _lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblStatus.UseThemeColors = true;

            // ── step 1: scope ──
            _pageScope.Description = "Choose the target connection and what to migrate.";
            _pageScope.Dock = System.Windows.Forms.DockStyle.Fill;
            _pageScope.Name = "_pageScope";
            _pageScope.NextButtonText = "Build Plan";
            _pageScope.Title = "Scope";

            _scopeTable.ColumnCount = 2;
            _scopeTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            _scopeTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _scopeTable.Dock = System.Windows.Forms.DockStyle.Fill;
            _scopeTable.Name = "_scopeTable";
            _scopeTable.RowCount = 7;
            // Six rows written out one at a time: the designer's code parser only understands
            // assignments, object creation and method calls, so a loop here breaks the design view.
            // AddReaderOptionControls() inserts two more absolute rows before the filler at runtime.
            _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            _lblConnection.Dock = System.Windows.Forms.DockStyle.Fill;
            _lblConnection.IsFrameless = true;
            _lblConnection.Name = "_lblConnection";
            _lblConnection.Text = "Connection";
            _lblConnection.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblConnection.UseThemeColors = true;

            _cboConnection.Dock = System.Windows.Forms.DockStyle.Fill;
            _cboConnection.Name = "_cboConnection";
            _cboConnection.UseThemeColors = true;

            _lblNamespace.Dock = System.Windows.Forms.DockStyle.Fill;
            _lblNamespace.IsFrameless = true;
            _lblNamespace.Name = "_lblNamespace";
            _lblNamespace.Text = "Namespace filter";
            _lblNamespace.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblNamespace.UseThemeColors = true;

            _txtNamespace.Dock = System.Windows.Forms.DockStyle.Fill;
            _txtNamespace.Name = "_txtNamespace";
            _txtNamespace.PlaceholderText = "(blank = discover all entity types)";
            _txtNamespace.UseThemeColors = true;

            _lblEnvironment.Dock = System.Windows.Forms.DockStyle.Fill;
            _lblEnvironment.IsFrameless = true;
            _lblEnvironment.Name = "_lblEnvironment";
            _lblEnvironment.Text = "Environment";
            _lblEnvironment.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblEnvironment.UseThemeColors = true;

            _cboEnvironment.Dock = System.Windows.Forms.DockStyle.Fill;
            _cboEnvironment.Name = "_cboEnvironment";
            _cboEnvironment.UseThemeColors = true;

            _chkDetectRelationships.Checked = true;
            _chkDetectRelationships.Dock = System.Windows.Forms.DockStyle.Fill;
            _chkDetectRelationships.Name = "_chkDetectRelationships";
            _chkDetectRelationships.Text = "Detect relationships";
            _chkDetectRelationships.UseThemeColors = true;

            _chkApplyForeignKeys.Dock = System.Windows.Forms.DockStyle.Fill;
            _chkApplyForeignKeys.Name = "_chkApplyForeignKeys";
            _chkApplyForeignKeys.Text = "Apply foreign keys";
            _chkApplyForeignKeys.UseThemeColors = true;

            _chkApplyIndexes.Dock = System.Windows.Forms.DockStyle.Fill;
            _chkApplyIndexes.Name = "_chkApplyIndexes";
            _chkApplyIndexes.Text = "Apply indexes";
            _chkApplyIndexes.UseThemeColors = true;

            _scopeTable.Controls.Add(_lblConnection, 0, 0);
            _scopeTable.Controls.Add(_cboConnection, 1, 0);
            _scopeTable.Controls.Add(_lblNamespace, 0, 1);
            _scopeTable.Controls.Add(_txtNamespace, 1, 1);
            _scopeTable.Controls.Add(_lblEnvironment, 0, 2);
            _scopeTable.Controls.Add(_cboEnvironment, 1, 2);
            _scopeTable.Controls.Add(_chkDetectRelationships, 1, 3);
            _scopeTable.Controls.Add(_chkApplyForeignKeys, 1, 4);
            _scopeTable.Controls.Add(_chkApplyIndexes, 1, 5);
            _pageScope.Controls.Add(_scopeTable);

            // ── step 2: plan ──
            _pagePlan.Description = "Review the operations this migration will perform.";
            _pagePlan.Dock = System.Windows.Forms.DockStyle.Fill;
            _pagePlan.Name = "_pagePlan";
            _pagePlan.NextButtonText = "Validate";
            _pagePlan.Title = "Plan";
            _pagePlan.Visible = false;

            _gridPlan.Dock = System.Windows.Forms.DockStyle.Fill;
            _gridPlan.Name = "_gridPlan";

            _lblPlanSummary.AutoEllipsis = true;
            _lblPlanSummary.Dock = System.Windows.Forms.DockStyle.Top;
            _lblPlanSummary.Height = 44;
            _lblPlanSummary.IsFrameless = true;
            _lblPlanSummary.Name = "_lblPlanSummary";
            _lblPlanSummary.Text = "No plan built yet.";
            _lblPlanSummary.UseThemeColors = true;

            _pagePlan.Controls.Add(_gridPlan);
            _pagePlan.Controls.Add(_lblPlanSummary);

            // ── step 3: safety ──
            _pageSafety.Description = "Policy, preflight, and dry-run results.";
            _pageSafety.Dock = System.Windows.Forms.DockStyle.Fill;
            _pageSafety.Name = "_pageSafety";
            _pageSafety.NextButtonText = "Run Migration";
            _pageSafety.Title = "Safety";
            _pageSafety.Visible = false;

            _lstFindings.Dock = System.Windows.Forms.DockStyle.Fill;
            _lstFindings.Name = "_lstFindings";
            _lstFindings.ShowSearch = false;
            _lstFindings.UseThemeColors = true;

            _lblSafetySummary.AutoEllipsis = true;
            _lblSafetySummary.Dock = System.Windows.Forms.DockStyle.Top;
            _lblSafetySummary.Height = 44;
            _lblSafetySummary.IsFrameless = true;
            _lblSafetySummary.Name = "_lblSafetySummary";
            _lblSafetySummary.Text = "Not validated yet.";
            _lblSafetySummary.UseThemeColors = true;

            _pageSafety.Controls.Add(_lstFindings);
            _pageSafety.Controls.Add(_lblSafetySummary);

            // ── step 4: run ──
            _pageRun.Description = "Execution progress and result.";
            _pageRun.Dock = System.Windows.Forms.DockStyle.Fill;
            _pageRun.Name = "_pageRun";
            _pageRun.Title = "Run";
            _pageRun.Visible = false;

            _lstRunLog.Dock = System.Windows.Forms.DockStyle.Fill;
            _lstRunLog.Name = "_lstRunLog";
            _lstRunLog.ShowSearch = false;
            _lstRunLog.UseThemeColors = true;

            _lblRunStatus.AutoEllipsis = true;
            _lblRunStatus.Dock = System.Windows.Forms.DockStyle.Top;
            _lblRunStatus.Height = 28;
            _lblRunStatus.IsFrameless = true;
            _lblRunStatus.Name = "_lblRunStatus";
            _lblRunStatus.Text = "Not started.";
            _lblRunStatus.UseThemeColors = true;

            _progress.Dock = System.Windows.Forms.DockStyle.Top;
            _progress.Height = 24;
            _progress.Maximum = 100;
            _progress.Minimum = 0;
            _progress.Name = "_progress";
            _progress.UseThemeColors = true;

            _pageRun.Controls.Add(_lstRunLog);
            _pageRun.Controls.Add(_lblRunStatus);
            _pageRun.Controls.Add(_progress);

            // Parented here so they are owned (and disposed) even if the wizard never starts; the
            // wizard form re-parents whichever page is current into its own content panel.
            _hostPanel.Controls.Add(_pageRun);
            _hostPanel.Controls.Add(_pageSafety);
            _hostPanel.Controls.Add(_pagePlan);
            _hostPanel.Controls.Add(_pageScope);

            // ── uc_MigrationWizard ──
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(_hostPanel);
            Controls.Add(_lblStatus);
            Name = "uc_MigrationWizard";
            Size = new System.Drawing.Size(900, 600);

            _pageRun.ResumeLayout(false);
            _pageSafety.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_gridPlan).EndInit();
            _pagePlan.ResumeLayout(false);
            _scopeTable.ResumeLayout(false);
            _pageScope.ResumeLayout(false);
            _hostPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private System.Windows.Forms.Panel _hostPanel;
        private BeepLabel _lblStatus;

        private WizardPage _pageScope;
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

        private WizardPage _pagePlan;
        private BeepLabel _lblPlanSummary;
        private BeepGridPro _gridPlan;

        private WizardPage _pageSafety;
        private BeepLabel _lblSafetySummary;
        private BeepListBox _lstFindings;

        private WizardPage _pageRun;
        private BeepProgressBar _progress;
        private BeepLabel _lblRunStatus;
        private BeepListBox _lstRunLog;
    }
}
