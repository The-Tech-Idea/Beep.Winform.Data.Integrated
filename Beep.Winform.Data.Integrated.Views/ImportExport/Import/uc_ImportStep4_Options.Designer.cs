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
            _headerPanel = new BeepPanel();
            _optionsPanel = new BeepPanel();
            _qualityPanel = new BeepPanel();
            _dryRunPanel = new BeepPanel();
            lblTitle = new BeepLabel();
            lblBatchSize = new BeepLabel();
            numBatchSize = new BeepNumericUpDown();
            chkPreflight = new BeepCheckBoxBool();
            chkAddMissing = new BeepCheckBoxBool();
            chkSyncDraft = new BeepCheckBoxBool();
            chkUpdateEmpty = new BeepCheckBoxBool();
            chkRunValidation = new BeepCheckBoxBool();
            chkStaging = new BeepCheckBoxBool();
            lblDriftPolicy = new BeepLabel();
            cmbDriftPolicy = new BeepComboBox();
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
            _optionsPanel.SuspendLayout();
            _qualityPanel.SuspendLayout();
            _dryRunPanel.SuspendLayout();
            SuspendLayout();

            _rootPanel.Controls.Add(_headerPanel);
            _rootPanel.Controls.Add(_optionsPanel);
            _rootPanel.Controls.Add(_qualityPanel);
            _rootPanel.Controls.Add(_dryRunPanel);
            _rootPanel.Dock = DockStyle.Fill;

            _headerPanel.Controls.Add(lblTitle);
            _headerPanel.Dock = DockStyle.Top;
            _headerPanel.Height = 50;
            lblTitle.Text = "Step 4: Options & Pre-flight";
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;

            _optionsPanel.Dock = DockStyle.Top;
            _optionsPanel.Height = 200;
            int y = 10;
            int labelX = 20, comboX = 200, comboW = 250;

            lblBatchSize.Text = "Batch Size:";
            lblBatchSize.Location = new Point(labelX, y + 3);
            lblBatchSize.Size = new Size(150, 24);
            numBatchSize.Location = new Point(comboX, y);
            numBatchSize.Size = new Size(100, 30);
            numBatchSize.MinimumValue = 1;
            numBatchSize.MaximumValue = 50000;
            numBatchSize.Value = 50;
            y += 35;

            lblDriftPolicy.Text = "Schema Drift Policy:";
            lblDriftPolicy.Location = new Point(labelX, y + 3);
            lblDriftPolicy.Size = new Size(150, 24);
            cmbDriftPolicy.Location = new Point(comboX, y);
            cmbDriftPolicy.Size = new Size(comboW, 30);
            cmbDriftPolicy.ListItems = new BindingList<SimpleItem>(new List<SimpleItem>
            {
                new() { Text = "Auto Add Columns", Value = "AutoAddColumns" },
                new() { Text = "Abort on Drift", Value = "AbortOnDrift" },
                new() { Text = "Ignore", Value = "Ignore" },
            });
            y += 35;

            chkPreflight.Text = "Run migration preflight check";
            chkPreflight.Location = new Point(labelX, y); chkPreflight.Size = new Size(250, 24); y += 30;
            chkAddMissing.Text = "Add missing columns";
            chkAddMissing.Location = new Point(labelX, y); chkAddMissing.Size = new Size(250, 24); y += 30;
            chkSyncDraft.Text = "Save sync profile draft";
            chkSyncDraft.Location = new Point(labelX, y); chkSyncDraft.Size = new Size(250, 24); y += 30;
            chkUpdateEmpty.Text = "Overwrite empty fields";
            chkUpdateEmpty.Location = new Point(labelX, y); chkUpdateEmpty.Size = new Size(250, 24); y += 30;
            chkRunValidation.Text = "Run validation rules";
            chkRunValidation.Location = new Point(labelX, y); chkRunValidation.Size = new Size(250, 24); y += 30;
            chkStaging.Text = "Enable staging table";
            chkStaging.Location = new Point(labelX, y); chkStaging.Size = new Size(250, 24);

            _optionsPanel.Controls.Add(lblBatchSize);
            _optionsPanel.Controls.Add(numBatchSize);
            _optionsPanel.Controls.Add(lblDriftPolicy);
            _optionsPanel.Controls.Add(cmbDriftPolicy);
            _optionsPanel.Controls.Add(chkPreflight);
            _optionsPanel.Controls.Add(chkAddMissing);
            _optionsPanel.Controls.Add(chkSyncDraft);
            _optionsPanel.Controls.Add(chkUpdateEmpty);
            _optionsPanel.Controls.Add(chkRunValidation);
            _optionsPanel.Controls.Add(chkStaging);

            _qualityPanel.Dock = DockStyle.Top;
            _qualityPanel.Height = 200;
            _qualityPanel.Controls.Add(lblRuleType);
            _qualityPanel.Controls.Add(cmbRuleType);
            _qualityPanel.Controls.Add(lblRuleField);
            _qualityPanel.Controls.Add(cmbRuleField);
            _qualityPanel.Controls.Add(lblRuleAction);
            _qualityPanel.Controls.Add(cmbRuleAction);
            _qualityPanel.Controls.Add(lblRuleParams);
            _qualityPanel.Controls.Add(txtRuleParams);
            _qualityPanel.Controls.Add(btnAddRule);
            _qualityPanel.Controls.Add(btnRemoveRule);
            _qualityPanel.Controls.Add(qualityRulesGrid);

            y = 10;
            lblRuleType.Text = "Rule:"; lblRuleType.Location = new Point(labelX, y + 3); lblRuleType.Size = new Size(60, 24);
            cmbRuleType.Location = new Point(80, y); cmbRuleType.Size = new Size(120, 30);
            cmbRuleType.ListItems = new BindingList<SimpleItem>(new List<SimpleItem>
            {
                new() { Text = "NotNull", Value = "NotNull" },
                new() { Text = "Unique", Value = "Unique" },
                new() { Text = "Regex", Value = "Regex" },
            });
            lblRuleField.Text = "Field:"; lblRuleField.Location = new Point(210, y + 3); lblRuleField.Size = new Size(50, 24);
            cmbRuleField.Location = new Point(265, y); cmbRuleField.Size = new Size(120, 30);
            lblRuleAction.Text = "Action:"; lblRuleAction.Location = new Point(395, y + 3); lblRuleAction.Size = new Size(50, 24);
            cmbRuleAction.Location = new Point(450, y); cmbRuleAction.Size = new Size(100, 30);
            cmbRuleAction.ListItems = new BindingList<SimpleItem>(new List<SimpleItem>
            {
                new() { Text = "Block", Value = "Block" },
                new() { Text = "Quarantine", Value = "Quarantine" },
                new() { Text = "Warn", Value = "Warn" },
            });
            lblRuleParams.Text = "Params:"; lblRuleParams.Location = new Point(560, y + 3); lblRuleParams.Size = new Size(50, 24);
            txtRuleParams.Location = new Point(615, y); txtRuleParams.Size = new Size(100, 26);
            btnAddRule.Text = "Add"; btnAddRule.Location = new Point(725, y - 2); btnAddRule.Size = new Size(60, 30);
            btnRemoveRule.Text = "Remove"; btnRemoveRule.Location = new Point(795, y - 2); btnRemoveRule.Size = new Size(70, 30);
            y += 35;
            qualityRulesGrid.Location = new Point(labelX, y);
            qualityRulesGrid.Size = new Size(830, 150);
            qualityRulesGrid.AllowUserToAddRows = false;
            qualityRulesGrid.RowHeadersVisible = false;

            _dryRunPanel.Dock = DockStyle.Top;
            _dryRunPanel.Height = 80;
            _dryRunPanel.Controls.Add(chkDryRun);
            _dryRunPanel.Controls.Add(numDryRunRows);
            _dryRunPanel.Controls.Add(btnDryRun);
            _dryRunPanel.Controls.Add(lblDryRunResult);

            chkDryRun.Text = "Dry Run (first N rows):"; chkDryRun.Location = new Point(labelX, 10); chkDryRun.Size = new Size(180, 24);
            numDryRunRows.Location = new Point(200, 8); numDryRunRows.Size = new Size(60, 30);
            numDryRunRows.MinimumValue = 1; numDryRunRows.MaximumValue = 1000; numDryRunRows.Value = 10;
            numDryRunRows.Enabled = false;
            btnDryRun.Text = "Run"; btnDryRun.Location = new Point(270, 5); btnDryRun.Size = new Size(60, 30);
            lblDryRunResult.Text = ""; lblDryRunResult.Location = new Point(340, 12); lblDryRunResult.Size = new Size(400, 24);

            Controls.Add(_rootPanel);
            _rootPanel.ResumeLayout(false);
            _optionsPanel.ResumeLayout(false);
            _qualityPanel.ResumeLayout(false);
            _dryRunPanel.ResumeLayout(false);
            SuspendLayout();
        }

        private BeepPanel _rootPanel;
        private BeepPanel _headerPanel;
        private BeepPanel _optionsPanel;
        private BeepPanel _qualityPanel;
        private BeepPanel _dryRunPanel;
        private BeepLabel lblTitle;
        private BeepLabel lblBatchSize;
        private BeepNumericUpDown numBatchSize;
        private BeepCheckBoxBool chkPreflight;
        private BeepCheckBoxBool chkAddMissing;
        private BeepCheckBoxBool chkSyncDraft;
        private BeepCheckBoxBool chkUpdateEmpty;
        private BeepCheckBoxBool chkRunValidation;
        private BeepCheckBoxBool chkStaging;
        private BeepLabel lblDriftPolicy;
        private BeepComboBox cmbDriftPolicy;
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
