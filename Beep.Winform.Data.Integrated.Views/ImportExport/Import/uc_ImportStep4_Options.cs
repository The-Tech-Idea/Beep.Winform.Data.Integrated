using System.ComponentModel;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport.Models;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Import
{
    public partial class uc_ImportStep4_Options : TemplateUserControl, IWizardStepContent
    {
        private DataImportConfiguration? _config;

        /// <summary>
        /// Designer/parameterless ctor. Must not chain to the IServiceProvider overload with null —
        /// that resolves services off a null provider and throws.
        /// </summary>
        public uc_ImportStep4_Options() => InitializeControl();

        public uc_ImportStep4_Options(IServiceProvider services) : base(services) => InitializeControl();

        private void InitializeControl()
        {
            InitializeComponent();
            SetupEvents();
        }

        public bool IsComplete => true;
        public string NextButtonText => "Next";
        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;

        public void OnStepEnter(WizardContext context)
        {
            _config = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null);
            if (_config == null) return;
            RestoreOptions();
            LoadQualityRules();
            PopulateFieldCombo();
        }

        public void OnStepLeave(WizardContext context)
        {
            if (_config == null) return;
            _config.BatchSize = (int)numBatchSize.Value;
            _config.RunMigrationPreflight = chkPreflight.Checked;
            _config.AddMissingColumns = chkAddMissing.Checked;
            _config.CreateSyncProfileDraft = chkSyncDraft.Checked;
            _config.SkipBlanks = !chkUpdateEmpty.Checked;
            _config.DriftPolicy = GetSelectedDriftPolicy();
            _config.QualityRules = BuildQualityRules();
            _config.Staging = chkStaging.Checked
                ? new StagingOptions { Enabled = true, StagingEntitySuffix = "_raw" }
                : null;

            context.SetValue(WizardKeys.BatchSize, _config.BatchSize);
            context.SetValue(WizardKeys.RunValidation, chkRunValidation.Checked);
            context.SetValue(WizardKeys.DryRunRowCount, chkDryRun.Checked ? (int)numDryRunRows.Value : 0);
            context.SetValue(WizardKeys.ImportConfig, _config);
        }

        public WizardValidationResult Validate() => WizardValidationResult.Success();
        public Task<WizardValidationResult> ValidateAsync() => Task.FromResult(Validate());

        private void SetupEvents()
        {
            chkDryRun.CheckedChanged += (_, _) => { numDryRunRows.Enabled = chkDryRun.Checked; };
            btnAddRule.Click += (_, _) => AddQualityRule();
            btnRemoveRule.Click += (_, _) => RemoveQualityRule();
            btnDryRun.Click += (_, _) => _ = ExecuteDryRunAsync();
        }

        private void RestoreOptions()
        {
            if (_config == null) return;
            numBatchSize.Value = Math.Max(1, Math.Min(_config.BatchSize, (int)numBatchSize.MaximumValue));
            chkPreflight.Checked = _config.RunMigrationPreflight;
            chkAddMissing.Checked = _config.AddMissingColumns;
            chkSyncDraft.Checked = _config.CreateSyncProfileDraft;
            chkUpdateEmpty.Checked = !_config.SkipBlanks;
            chkRunValidation.Checked = true;
            chkDryRun.Checked = false;
            numDryRunRows.Value = 10;
            numDryRunRows.Enabled = false;
        }

        private void PopulateFieldCombo()
        {
            if (Editor == null || _config == null) return;
            var ds = Editor.GetDataSource(_config.SourceDataSourceName);
            if (ds == null) return;
            var structure = ds.GetEntityStructure(_config.SourceEntityName, false);
            if (structure?.Fields == null) return;

            cmbRuleField.ListItems = new BindingList<SimpleItem>(
                structure.Fields.Select(f => new SimpleItem { Text = f.FieldName, Value = f.FieldName }).ToList());
        }

        private void LoadQualityRules()
        {
            qualityRulesGrid.Rows.Clear();
            qualityRulesGrid.Columns.Clear();
            qualityRulesGrid.Columns.Add("ruleType", "Rule Type");
            qualityRulesGrid.Columns.Add("fieldName", "Field");
            qualityRulesGrid.Columns.Add("action", "Action");
            qualityRulesGrid.Columns.Add("params", "Parameters");

            if (_config?.QualityRules != null)
            {
                foreach (var rule in _config.QualityRules)
                {
                    qualityRulesGrid.Rows.Add(rule.RuleName, rule.FieldName, rule.OnFailure.ToString(), "");
                }
            }
        }

        private void AddQualityRule()
        {
            var ruleType = cmbRuleType.SelectedItem?.Value?.ToString() ?? "NotNull";
            var fieldName = cmbRuleField.SelectedItem?.Value?.ToString() ?? string.Empty;
            var action = GetSelectedAction();
            var parameters = txtRuleParams.Text;

            if (string.IsNullOrEmpty(fieldName))
            {
                MessageBox.Show("Please select a field for the quality rule.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            qualityRulesGrid.Rows.Add(ruleType, fieldName, action.ToString(), parameters);
            txtRuleParams.Text = string.Empty;
        }

        private void RemoveQualityRule()
        {
            if (qualityRulesGrid.SelectedRows.Count > 0)
                qualityRulesGrid.Rows.RemoveAt(qualityRulesGrid.SelectedRows[0].Index);
        }

        private List<IDataQualityRule> BuildQualityRules()
        {
            var rules = new List<IDataQualityRule>();
            foreach (DataGridViewRow row in qualityRulesGrid.Rows)
            {
                if (row.IsNewRow) continue;
                var type = row.Cells["ruleType"].Value?.ToString();
                var field = row.Cells["fieldName"].Value?.ToString();
                if (string.IsNullOrEmpty(field)) continue;
                var action = Enum.TryParse<DataQualityAction>(row.Cells["action"].Value?.ToString(), out var a) ? a : DataQualityAction.Warn;
                var parameters = row.Cells["params"].Value?.ToString() ?? "";

                IDataQualityRule? rule = type switch
                {
                    "NotNull" => new NotNullRule(field, action),
                    "Unique" => new UniqueRule(field, action),
                    "Regex" when !string.IsNullOrEmpty(parameters) => new RegexRule(field, parameters, action),
                    _ => null,
                };
                if (rule != null) rules.Add(rule);
            }
            return rules;
        }

        private async Task ExecuteDryRunAsync()
        {
            if (_config == null || Editor == null) return;
            btnDryRun.Enabled = false;
            lblDryRunResult.Text = "Running dry-run...";

            try
            {
                var mgr = new DataImportManager(Editor);
                var config = new DataImportConfiguration
                {
                    SourceEntityName = _config.SourceEntityName,
                    SourceDataSourceName = _config.SourceDataSourceName,
                    DestEntityName = _config.DestEntityName,
                    DestDataSourceName = _config.DestDataSourceName,
                    Mapping = _config.Mapping,
                    SelectedFields = _config.SelectedFields,
                    QualityRules = _config.QualityRules,
                    DriftPolicy = _config.DriftPolicy,
                    CreateDestinationIfNotExists = _config.CreateDestinationIfNotExists,
                    AddMissingColumns = _config.AddMissingColumns,
                };
                config.BatchSize = (int)numDryRunRows.Value;

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var result = await mgr.RunImportAsync(config, new Progress<IPassedArgs>(_ => { }), cts.Token);

                if (result.Flag == Errors.Ok)
                    lblDryRunResult.Text = "Dry-run completed — no errors.";
                else
                    lblDryRunResult.Text = $"Dry-run: {result.Message}";
            }
            catch (Exception ex)
            {
                lblDryRunResult.Text = $"Dry-run error: {ex.Message}";
            }
            finally
            {
                btnDryRun.Enabled = true;
            }
        }

        private SchemaDriftPolicy GetSelectedDriftPolicy()
        {
            var item = cmbDriftPolicy.SelectedItem?.Value?.ToString();
            return item switch
            {
                "AbortOnDrift" => SchemaDriftPolicy.AbortOnDrift,
                "Ignore" => SchemaDriftPolicy.Ignore,
                _ => SchemaDriftPolicy.AutoAddColumns,
            };
        }

        private DataQualityAction GetSelectedAction()
        {
            var item = cmbRuleAction.SelectedItem?.Value?.ToString();
            return item switch
            {
                "Block" => DataQualityAction.Block,
                "Quarantine" => DataQualityAction.Quarantine,
                _ => DataQualityAction.Warn,
            };
        }
    }
}
