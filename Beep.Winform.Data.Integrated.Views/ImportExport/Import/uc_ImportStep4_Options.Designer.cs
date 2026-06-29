using System.ComponentModel;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Import
{
    partial class uc_ImportStep4_Options
    {
        private System.ComponentModel.IContainer? components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _rootPanel = new BeepPanel();
            lblTitle = new BeepLabel();
            lblBatchSize = new BeepLabel();
            numBatchSize = new BeepNumericUpDown();
            lblDriftPolicy = new BeepLabel();
            cmbDriftPolicy = new BeepComboBox();
            chkPreflight = new BeepCheckBoxBool();
            chkAddMissing = new BeepCheckBoxBool();
            chkSyncDraft = new BeepCheckBoxBool();
            chkUpdateEmpty = new BeepCheckBoxBool();
            chkRunValidation = new BeepCheckBoxBool();
            chkStaging = new BeepCheckBoxBool();
            lblRuleType = new BeepLabel();
            cmbRuleType = new BeepComboBox();
            lblRuleField = new BeepLabel();
            cmbRuleField = new BeepComboBox();
            lblRuleAction = new BeepLabel();
            cmbRuleAction = new BeepComboBox();
            lblRuleParams = new BeepLabel();
            txtRuleParams = new BeepTextBox();
            btnAddRule = new BeepButton();
            btnRemoveRule = new BeepButton();
            qualityRulesGrid = new DataGridView();
            chkDryRun = new BeepCheckBoxBool();
            numDryRunRows = new BeepNumericUpDown();
            btnDryRun = new BeepButton();
            lblDryRunResult = new BeepLabel();
            _rootPanel.SuspendLayout();
            SuspendLayout();

            // _rootPanel
            _rootPanel.Controls.Add(lblDryRunResult);
            _rootPanel.Controls.Add(btnDryRun);
            _rootPanel.Controls.Add(numDryRunRows);
            _rootPanel.Controls.Add(chkDryRun);
            _rootPanel.Controls.Add(qualityRulesGrid);
            _rootPanel.Controls.Add(btnRemoveRule);
            _rootPanel.Controls.Add(btnAddRule);
            _rootPanel.Controls.Add(txtRuleParams);
            _rootPanel.Controls.Add(lblRuleParams);
            _rootPanel.Controls.Add(cmbRuleAction);
            _rootPanel.Controls.Add(lblRuleAction);
            _rootPanel.Controls.Add(cmbRuleField);
            _rootPanel.Controls.Add(lblRuleField);
            _rootPanel.Controls.Add(cmbRuleType);
            _rootPanel.Controls.Add(lblRuleType);
            _rootPanel.Controls.Add(chkStaging);
            _rootPanel.Controls.Add(chkRunValidation);
            _rootPanel.Controls.Add(chkUpdateEmpty);
            _rootPanel.Controls.Add(chkSyncDraft);
            _rootPanel.Controls.Add(chkAddMissing);
            _rootPanel.Controls.Add(chkPreflight);
            _rootPanel.Controls.Add(cmbDriftPolicy);
            _rootPanel.Controls.Add(lblDriftPolicy);
            _rootPanel.Controls.Add(numBatchSize);
            _rootPanel.Controls.Add(lblBatchSize);
            _rootPanel.Controls.Add(lblTitle);
            _rootPanel.Dock = DockStyle.Fill;
            _rootPanel.Location = new Point(0, 0);
            _rootPanel.Name = "_rootPanel";
            _rootPanel.Size = new Size(900, 600);

            // lblTitle
            lblTitle.Text = "Step 4: Options & Pre-flight";
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(860, 35);
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // lblBatchSize
            lblBatchSize.Text = "Batch Size:";
            lblBatchSize.Location = new Point(30, 60);
            lblBatchSize.Name = "lblBatchSize";
            lblBatchSize.Size = new Size(150, 24);
            lblBatchSize.TextAlign = ContentAlignment.MiddleLeft;
            lblBatchSize.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // numBatchSize
            numBatchSize.Location = new Point(200, 57);
            numBatchSize.Name = "numBatchSize";
            numBatchSize.Size = new Size(100, 30);
            numBatchSize.MinimumValue = 1;
            numBatchSize.MaximumValue = 50000;
            numBatchSize.Value = 50;
            numBatchSize.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // lblDriftPolicy
            lblDriftPolicy.Text = "Schema Drift Policy:";
            lblDriftPolicy.Location = new Point(30, 95);
            lblDriftPolicy.Name = "lblDriftPolicy";
            lblDriftPolicy.Size = new Size(150, 24);
            lblDriftPolicy.TextAlign = ContentAlignment.MiddleLeft;
            lblDriftPolicy.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // cmbDriftPolicy
            cmbDriftPolicy.Location = new Point(200, 92);
            cmbDriftPolicy.Name = "cmbDriftPolicy";
            cmbDriftPolicy.Size = new Size(250, 30);
            cmbDriftPolicy.ListItems = new BindingList<SimpleItem>(new List<SimpleItem>
            {
                new() { Text = "Auto Add Columns", Value = "AutoAddColumns" },
                new() { Text = "Abort on Drift", Value = "AbortOnDrift" },
                new() { Text = "Ignore", Value = "Ignore" },
            });
            cmbDriftPolicy.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // chkPreflight
            chkPreflight.Text = "Run migration preflight check";
            chkPreflight.Location = new Point(480, 60);
            chkPreflight.Name = "chkPreflight";
            chkPreflight.Size = new Size(200, 24);
            chkPreflight.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // chkAddMissing
            chkAddMissing.Text = "Add missing columns";
            chkAddMissing.Location = new Point(480, 88);
            chkAddMissing.Name = "chkAddMissing";
            chkAddMissing.Size = new Size(200, 24);
            chkAddMissing.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // chkSyncDraft
            chkSyncDraft.Text = "Save sync profile draft";
            chkSyncDraft.Location = new Point(480, 116);
            chkSyncDraft.Name = "chkSyncDraft";
            chkSyncDraft.Size = new Size(200, 24);
            chkSyncDraft.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // chkUpdateEmpty
            chkUpdateEmpty.Text = "Overwrite empty fields";
            chkUpdateEmpty.Location = new Point(480, 144);
            chkUpdateEmpty.Name = "chkUpdateEmpty";
            chkUpdateEmpty.Size = new Size(200, 24);
            chkUpdateEmpty.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // chkRunValidation
            chkRunValidation.Text = "Run validation rules";
            chkRunValidation.Location = new Point(480, 172);
            chkRunValidation.Name = "chkRunValidation";
            chkRunValidation.Size = new Size(200, 24);
            chkRunValidation.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // chkStaging
            chkStaging.Text = "Enable staging table";
            chkStaging.Location = new Point(480, 200);
            chkStaging.Name = "chkStaging";
            chkStaging.Size = new Size(200, 24);
            chkStaging.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // lblRuleType
            lblRuleType.Text = "Rule:";
            lblRuleType.Location = new Point(30, 238);
            lblRuleType.Name = "lblRuleType";
            lblRuleType.Size = new Size(50, 24);
            lblRuleType.TextAlign = ContentAlignment.MiddleLeft;
            lblRuleType.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // cmbRuleType
            cmbRuleType.Location = new Point(85, 235);
            cmbRuleType.Name = "cmbRuleType";
            cmbRuleType.Size = new Size(120, 30);
            cmbRuleType.ListItems = new BindingList<SimpleItem>(new List<SimpleItem>
            {
                new() { Text = "NotNull", Value = "NotNull" },
                new() { Text = "Unique", Value = "Unique" },
                new() { Text = "Regex", Value = "Regex" },
            });
            cmbRuleType.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // lblRuleField
            lblRuleField.Text = "Field:";
            lblRuleField.Location = new Point(215, 238);
            lblRuleField.Name = "lblRuleField";
            lblRuleField.Size = new Size(45, 24);
            lblRuleField.TextAlign = ContentAlignment.MiddleLeft;
            lblRuleField.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // cmbRuleField
            cmbRuleField.Location = new Point(265, 235);
            cmbRuleField.Name = "cmbRuleField";
            cmbRuleField.Size = new Size(120, 30);
            cmbRuleField.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // lblRuleAction
            lblRuleAction.Text = "Action:";
            lblRuleAction.Location = new Point(395, 238);
            lblRuleAction.Name = "lblRuleAction";
            lblRuleAction.Size = new Size(55, 24);
            lblRuleAction.TextAlign = ContentAlignment.MiddleLeft;
            lblRuleAction.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // cmbRuleAction
            cmbRuleAction.Location = new Point(455, 235);
            cmbRuleAction.Name = "cmbRuleAction";
            cmbRuleAction.Size = new Size(100, 30);
            cmbRuleAction.ListItems = new BindingList<SimpleItem>(new List<SimpleItem>
            {
                new() { Text = "Block", Value = "Block" },
                new() { Text = "Quarantine", Value = "Quarantine" },
                new() { Text = "Warn", Value = "Warn" },
            });
            cmbRuleAction.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // lblRuleParams
            lblRuleParams.Text = "Params:";
            lblRuleParams.Location = new Point(565, 238);
            lblRuleParams.Name = "lblRuleParams";
            lblRuleParams.Size = new Size(50, 24);
            lblRuleParams.TextAlign = ContentAlignment.MiddleLeft;
            lblRuleParams.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // txtRuleParams
            txtRuleParams.Location = new Point(620, 235);
            txtRuleParams.Name = "txtRuleParams";
            txtRuleParams.Size = new Size(100, 26);
            txtRuleParams.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // btnAddRule
            btnAddRule.Text = "Add";
            btnAddRule.Location = new Point(730, 233);
            btnAddRule.Name = "btnAddRule";
            btnAddRule.Size = new Size(60, 30);
            btnAddRule.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // btnRemoveRule
            btnRemoveRule.Text = "Remove";
            btnRemoveRule.Location = new Point(800, 233);
            btnRemoveRule.Name = "btnRemoveRule";
            btnRemoveRule.Size = new Size(75, 30);
            btnRemoveRule.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // qualityRulesGrid
            qualityRulesGrid.Location = new Point(30, 270);
            qualityRulesGrid.Name = "qualityRulesGrid";
            qualityRulesGrid.Size = new Size(845, 220);
            qualityRulesGrid.AllowUserToAddRows = false;
            qualityRulesGrid.RowHeadersVisible = false;
            qualityRulesGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // chkDryRun
            chkDryRun.Text = "Dry Run (first N rows):";
            chkDryRun.Location = new Point(30, 503);
            chkDryRun.Name = "chkDryRun";
            chkDryRun.Size = new Size(180, 24);
            chkDryRun.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            // numDryRunRows
            numDryRunRows.Location = new Point(215, 500);
            numDryRunRows.Name = "numDryRunRows";
            numDryRunRows.Size = new Size(60, 30);
            numDryRunRows.MinimumValue = 1;
            numDryRunRows.MaximumValue = 1000;
            numDryRunRows.Value = 10;
            numDryRunRows.Enabled = false;
            numDryRunRows.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            // btnDryRun
            btnDryRun.Text = "Run";
            btnDryRun.Location = new Point(285, 500);
            btnDryRun.Name = "btnDryRun";
            btnDryRun.Size = new Size(60, 30);
            btnDryRun.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            // lblDryRunResult
            lblDryRunResult.Text = "";
            lblDryRunResult.Location = new Point(355, 503);
            lblDryRunResult.Name = "lblDryRunResult";
            lblDryRunResult.Size = new Size(520, 24);
            lblDryRunResult.TextAlign = ContentAlignment.MiddleLeft;
            lblDryRunResult.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // uc_ImportStep4_Options
            Controls.Add(_rootPanel);
            Name = "uc_ImportStep4_Options";
            Size = new Size(900, 600);
            _rootPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private BeepPanel _rootPanel;
        private BeepLabel lblTitle;
        private BeepLabel lblBatchSize;
        private BeepNumericUpDown numBatchSize;
        private BeepLabel lblDriftPolicy;
        private BeepComboBox cmbDriftPolicy;
        private BeepCheckBoxBool chkPreflight;
        private BeepCheckBoxBool chkAddMissing;
        private BeepCheckBoxBool chkSyncDraft;
        private BeepCheckBoxBool chkUpdateEmpty;
        private BeepCheckBoxBool chkRunValidation;
        private BeepCheckBoxBool chkStaging;
        private BeepLabel lblRuleType;
        private BeepComboBox cmbRuleType;
        private BeepLabel lblRuleField;
        private BeepComboBox cmbRuleField;
        private BeepLabel lblRuleAction;
        private BeepComboBox cmbRuleAction;
        private BeepLabel lblRuleParams;
        private BeepTextBox txtRuleParams;
        private BeepButton btnAddRule;
        private BeepButton btnRemoveRule;
        private DataGridView qualityRulesGrid;
        private BeepCheckBoxBool chkDryRun;
        private BeepNumericUpDown numDryRunRows;
        private BeepButton btnDryRun;
        private BeepLabel lblDryRunResult;
    }
}
