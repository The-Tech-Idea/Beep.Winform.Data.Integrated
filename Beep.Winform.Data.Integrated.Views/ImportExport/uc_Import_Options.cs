using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Editor.Importing;
using TheTechIdea.Beep.Editor.Importing.Interfaces;
using TheTechIdea.Beep.Editor.Importing.Quality;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Default.Views.Template;
using TheTechIdea.Beep.Workflow;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    
    public partial class uc_Import_Options : TemplateUserControl, IWizardStepContent
    {
        private DataImportConfiguration? _config;
        private BindingList<QualityRuleRow> _qualityRows = new();

        public uc_Import_Options(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            numBatchSize.ValueChanged  += (_, _) => { /* live update could go here */ };
            numDryRunRows.ValueChanged += (_, _) => { /* live update could go here */ };
            WireQualityRuleEvents();
        }

        private void WireQualityRuleEvents()
        {
            btnAddQualityRule.Click += AddQualityRule_Click;
            btnRemoveQualityRule.Click += RemoveQualityRule_Click;
            btnClearQualityRules.Click += ClearQualityRules_Click;
        }

        // ── IWizardStepContent ───────────────────────────────────────────────

        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;

        /// <summary>Options step is always passable — no required fields.</summary>
        public bool   IsComplete     => true;
        public string NextButtonText => "Run Import";

        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            base.OnNavigatedTo(parameters);
            RaiseValidationState();
        }

        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);
            RaiseValidationState();
        }

        public void OnStepEnter(WizardContext context)
        {
            _config = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null);

            // ── Validate upstream step data integrity ──
            if (!ImportExportWizardValidation.ValidateContextIntegrity(context, out var integrityIssues))
            {
                // Log warnings but don't block — user may still correct in earlier steps
                foreach (var issue in integrityIssues)
                    System.Diagnostics.Debug.WriteLine($"[Wizard Validation] {issue}");
            }
            if (!ImportExportWizardValidation.ValidateMappingRoundtrip(context, out var mappingIssues))
            {
                foreach (var issue in mappingIssues)
                    System.Diagnostics.Debug.WriteLine($"[Wizard Mapping] {issue}");
            }

            // Restore persisted values (or sensible defaults)
            numBatchSize.Value     = context.GetValue<int>(WizardKeys.BatchSize, 50);
            chkDryRun.CurrentValue = context.GetValue<int>(WizardKeys.DryRunRowCount, 0) > 0;
            numDryRunRows.Value    = Math.Max(1, context.GetValue<int>(WizardKeys.DryRunRowCount, 100));
            chkRunValidation.CurrentValue = context.GetValue<bool>(WizardKeys.RunValidation, true);
            chkSkipBlanks.CurrentValue    = context.GetValue<bool>(WizardKeys.SkipBlanks, false);
            chkPreflight.CurrentValue     = context.GetValue<bool>(WizardKeys.RunSchemaPreflight, false);
            chkSyncDraft.CurrentValue     = context.GetValue<bool>(WizardKeys.SaveSyncDraft, false);

            // Show SkipBlanks only when mode can update existing rows
            var purpose = context.GetValue<ImportPurpose>(WizardKeys.Purpose, ImportPurpose.AddOnly);
            chkSkipBlanks.Visible = purpose != ImportPurpose.AddOnly;

            UpdateDryRunRowsVisibility();
            LoadQualityRules();
            RenderPreflight(context);
            RaiseValidationState();
        }

        public void OnStepLeave(WizardContext context)
        {
            // Commit option values back to context + config
            int batchSize    = (int)numBatchSize.Value;
            int dryRunRows   = chkDryRun.CurrentValue ? (int)numDryRunRows.Value : 0;
            bool runValidate = chkRunValidation.CurrentValue;

            context.SetValue(WizardKeys.BatchSize, batchSize);
            context.SetValue(WizardKeys.DryRunRowCount, dryRunRows);
            context.SetValue(WizardKeys.RunValidation, runValidate);
            context.SetValue(WizardKeys.SkipBlanks,         chkSkipBlanks.CurrentValue);
            context.SetValue(WizardKeys.RunSchemaPreflight, chkPreflight.CurrentValue);
            context.SetValue(WizardKeys.SaveSyncDraft,      chkSyncDraft.CurrentValue);

            if (_config != null)
            {
                _config.BatchSize = batchSize;
                _config.SkipBlanks = chkSkipBlanks.CurrentValue;
                _config.QualityRules = BuildQualityRulesFromRows();
            }
        }

        // ── Data Quality Rules ────────────────────────────────────────────────

        private void LoadQualityRules()
        {
            _qualityRows.Clear();
            if (_config?.QualityRules == null) return;

            foreach (var rule in _config.QualityRules)
            {
                _qualityRows.Add(new QualityRuleRow
                {
                    Selected = true,
                    RuleType = GetRuleTypeName(rule),
                    FieldName = rule.FieldName,
                    Parameters = GetRuleParameters(rule),
                    OnFailure = rule.OnFailure.ToString()
                });
            }
            BindQualityGrid();
        }

        private void BindQualityGrid()
        {
            if (qualityRulesGrid != null)
            {
                qualityRulesGrid.DataSource = null;
                qualityRulesGrid.DataSource = _qualityRows;
            }
        }

        private void AddQualityRule_Click(object? sender, EventArgs e)
        {
            using var dlg = new Form
            {
                Text = "Add Quality Rule",
                Size = new System.Drawing.Size(400, 280),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false
            };

            var lblType = new Label { Text = "Rule Type:", Location = new System.Drawing.Point(12, 12), AutoSize = true };
            var cmbType = new ComboBox { Location = new System.Drawing.Point(120, 12), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbType.Items.AddRange(new[] { "Not Null", "Unique", "Range", "Regex", "Accepted Values" });
            cmbType.SelectedIndex = 0;

            var lblField = new Label { Text = "Field Name:", Location = new System.Drawing.Point(12, 44), AutoSize = true };
            var txtField = new TextBox { Location = new System.Drawing.Point(120, 44), Width = 250 };

            var lblParams = new Label { Text = "Parameters:", Location = new System.Drawing.Point(12, 76), AutoSize = true };
            var txtParams = new TextBox { Location = new System.Drawing.Point(120, 76), Width = 250, Text = "(optional - e.g., min,max for Range)" };

            var lblAction = new Label { Text = "On Failure:", Location = new System.Drawing.Point(12, 108), AutoSize = true };
            var cmbAction = new ComboBox { Location = new System.Drawing.Point(120, 108), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbAction.Items.AddRange(new[] { "Block", "Quarantine", "Warn" });
            cmbAction.SelectedIndex = 0;

            var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new System.Drawing.Point(200, 180), Width = 80 };
            var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new System.Drawing.Point(290, 180), Width = 80 };

            dlg.Controls.AddRange(new Control[] { lblType, cmbType, lblField, txtField, lblParams, txtParams, lblAction, cmbAction, btnOk, btnCancel });
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            // Validate field name
            if (string.IsNullOrWhiteSpace(txtField.Text))
            {
                MessageBox.Show("Field name is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Clear placeholder text if used as-is
            var parameters = txtParams.Text == "(optional - e.g., min,max for Range)" ? "" : txtParams.Text;

            var row = new QualityRuleRow
            {
                Selected = true,
                RuleType = cmbType.Text,
                FieldName = txtField.Text.Trim(),
                Parameters = parameters?.Trim() ?? "",
                OnFailure = cmbAction.Text
            };
            _qualityRows.Add(row);
            BindQualityGrid();
        }

        private void RemoveQualityRule_Click(object? sender, EventArgs e)
        {
            var selected = _qualityRows.Where(r => r.Selected).ToList();
            foreach (var row in selected)
                _qualityRows.Remove(row);
            BindQualityGrid();
        }

        private void ClearQualityRules_Click(object? sender, EventArgs e)
        {
            _qualityRows.Clear();
            BindQualityGrid();
        }

        private List<IDataQualityRule> BuildQualityRulesFromRows()
        {
            var rules = new List<IDataQualityRule>();
            foreach (var row in _qualityRows.Where(r => r.Selected))
            {
                var action = Enum.TryParse<DataQualityAction>(row.OnFailure, out var a) ? a : DataQualityAction.Block;
                switch (row.RuleType)
                {
                    case "Not Null":
                        rules.Add(new NotNullRule(row.FieldName, action));
                        break;
                    case "Unique":
                        rules.Add(new UniqueRule(row.FieldName, action));
                        break;
                    case "Range":
                        var rangeParts = row.Parameters?.Split(',') ?? Array.Empty<string>();
                        if (rangeParts.Length >= 2 && double.TryParse(rangeParts[0], out var min) && double.TryParse(rangeParts[1], out var max))
                            rules.Add(new RangeRule(row.FieldName, min, max, action));
                        break;
                    case "Regex":
                        if (!string.IsNullOrWhiteSpace(row.Parameters))
                            rules.Add(new RegexRule(row.FieldName, row.Parameters, action));
                        break;
                    case "Accepted Values":
                        var values = row.Parameters?.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)) ?? Array.Empty<string>();
                        rules.Add(new AcceptedValuesRule(row.FieldName, values, action));
                        break;
                }
            }
            return rules;
        }

        private static string GetRuleTypeName(IDataQualityRule rule) => rule switch
        {
            NotNullRule => "Not Null",
            UniqueRule => "Unique",
            RangeRule => "Range",
            RegexRule => "Regex",
            AcceptedValuesRule => "Accepted Values",
            _ => "Custom"
        };

        private static string GetRuleParameters(IDataQualityRule rule)
        {
            try
            {
                return rule switch
                {
                    RangeRule r => $"{r.Min},{r.Max}",
                    RegexRule r => r.GetType().GetField("_regex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(r)?.ToString() ?? "",
                    AcceptedValuesRule r => string.Join(",", r.GetType().GetField("_accepted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(r) as System.Collections.Generic.HashSet<string> ?? new()),
                    _ => ""
                };
            }
            catch
            {
                return "";
            }
        }

        // ── Dry-run toggle ────────────────────────────────────────────────────

        private void ChkDryRun_Changed(object? sender, EventArgs e)
        {
            UpdateDryRunRowsVisibility();
        }

        private void ChkPreflight_Changed(object? sender, EventArgs e)
        {
            if (!chkPreflight.CurrentValue) {
                lblPreflightStatus.Text = string.Empty;
                return;
            }
            // Run a quick schema check and report result in lblPreflightStatus
            try
            {
                if (_config?.Mapping == null)
                {
                    lblPreflightStatus.Text = "⚠ No field mapping defined";
                    lblPreflightStatus.ForeColor = System.Drawing.Color.DarkOrange;
                    return;
                }
                int totalFields  = _config.Mapping.MappedEntities?.Sum(d => d.FieldMapping?.Count ?? 0) ?? 0;
                int unknownTypes = _config.Mapping.MappedEntities?
                    .SelectMany(d => d.FieldMapping ?? new List<Mapping_rep_fields>())
                    .Count(f => string.IsNullOrWhiteSpace(f.FromFieldType) || string.IsNullOrWhiteSpace(f.ToFieldType))
                    ?? 0;

                lblPreflightStatus.Text = unknownTypes == 0
                    ? $"✓ {totalFields} fields mapped — types OK"
                    : $"⚠ {unknownTypes}/{totalFields} fields have unknown types";
                lblPreflightStatus.ForeColor = unknownTypes == 0
                    ? System.Drawing.Color.DarkGreen
                    : System.Drawing.Color.DarkOrange;
            }
            catch (Exception ex)
            {
                lblPreflightStatus.Text = $"✕ {ex.Message}";
                lblPreflightStatus.ForeColor = System.Drawing.Color.DarkRed;
            }
        }

        private void UpdateDryRunRowsVisibility()
        {
            lblDryRunRows.Visible = chkDryRun.CurrentValue;
            numDryRunRows.Visible = chkDryRun.CurrentValue;
        }

        // ── Pre-flight summary ────────────────────────────────────────────────

        private void RenderPreflight(WizardContext context)
        {
            if (_config == null)
            {
                txtPreflight.Text = "(No configuration loaded — return to Step 1 to set up the import.)";
                return;
            }

            var sb = new StringBuilder();

            sb.AppendLine("─── Import Configuration Summary ───────────────────────────");
            sb.AppendLine();

            // Source / destination
            sb.AppendLine($"  Source      : {_config.SourceDataSourceName ?? "—"} / {_config.SourceEntityName ?? "—"}");
            sb.AppendLine($"  Destination : {_config.DestDataSourceName ?? "—"} / {_config.DestEntityName ?? "—"}");
            sb.AppendLine();

            // Purpose
            var purpose = context.GetValue<ImportPurpose>(WizardKeys.Purpose, ImportPurpose.AddOnly);
            var matchBy = context.GetValue<string?>(WizardKeys.MatchByField, null);
            var updEmpty= context.GetValue<bool>(WizardKeys.UpdateEmptyFields, false);

            sb.AppendLine($"  Mode        : {purpose}");
            if (purpose != ImportPurpose.AddOnly && !string.IsNullOrWhiteSpace(matchBy))
                sb.AppendLine($"  Match by    : {matchBy}");
            if (purpose == ImportPurpose.AddOrUpdate)
                sb.AppendLine($"  Update empty: {(updEmpty ? "Yes" : "No")}");
            sb.AppendLine();

            // Column selection
            var selectedCols = context.GetValue<List<string>?>(WizardKeys.SelectedColumns, null);
            int colCount = selectedCols?.Count ?? 0;
            sb.AppendLine($"  Columns     : {(colCount > 0 ? colCount.ToString() + " selected" : "all")}");
            sb.AppendLine();

            // Field mapping
            int mappedFields = _config.Mapping?.MappedEntities?.Count ?? 0;
            sb.AppendLine($"  Mapped dest : {mappedFields} destination entities");
            sb.AppendLine();

            // Run options
            sb.AppendLine($"  Batch size  : {(int)numBatchSize.Value:N0} rows / commit");
            if (chkDryRun.CurrentValue)
                sb.AppendLine($"  Dry run     : Yes — first {(int)numDryRunRows.Value:N0} rows only");
            else
                sb.AppendLine($"  Dry run     : No (full import)");
            sb.AppendLine($"  Validate    : {(chkRunValidation.CurrentValue ? "Yes" : "No")}");
            sb.AppendLine();

            // Template name if any
            var templateName = context.GetValue<string?>(WizardKeys.TemplateName, null);
            if (!string.IsNullOrWhiteSpace(templateName))
                sb.AppendLine($"  Template    : {templateName}");

            sb.AppendLine("────────────────────────────────────────────────────────────");
            sb.AppendLine();
            sb.AppendLine("Review the summary above and click  Run Import  to proceed.");

            txtPreflight.Text = sb.ToString();
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        WizardValidationResult IWizardStepContent.Validate()
            => WizardValidationResult.Success();

        public Task<WizardValidationResult> ValidateAsync()
            => Task.FromResult(WizardValidationResult.Success());

        private void RaiseValidationState()
        {
            ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(IsComplete));
        }
    }

    public class QualityRuleRow
    {
        public bool Selected { get; set; } = true;
        public string RuleType { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string Parameters { get; set; } = string.Empty;
        public string OnFailure { get; set; } = "Block";
    }
}
