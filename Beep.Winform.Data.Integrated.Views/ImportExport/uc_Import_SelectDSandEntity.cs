using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Editor.Importing;
using TheTechIdea.Beep.Editor.Importing.Interfaces;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.ComboBoxes;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    internal static class WizardKeys
    {
        public const string ImportConfig      = "ImportConfig";
        public const string RunImportOnFinish = "RunImportOnFinish";
        public const string LastRunSucceeded  = "LastRunSucceeded";
        // Phase 2 additions
        public const string Purpose           = "Purpose";           // ImportPurpose enum
        public const string MatchByField      = "MatchByField";      // string
        public const string UpdateEmptyFields = "UpdateEmptyFields"; // bool
        public const string SelectedColumns   = "SelectedColumns";   // List<string>
        public const string TemplateName      = "TemplateName";      // string
        public const string BatchSize         = "BatchSize";         // int
        public const string DryRunRowCount    = "DryRunRowCount";    // int (0 = disabled)
        public const string RunValidation     = "RunValidation";     // bool
        public const string SkipBlanks        = "SkipBlanks";        // bool
        public const string RunSchemaPreflight= "RunSchemaPreflight";// bool
        public const string SaveSyncDraft     = "SaveSyncDraft";     // bool
        public const string RunSummary        = "RunSummary";        // ImportRunSummary
    }

    public enum ImportPurpose
    {
        AddOnly,
        AddOrUpdate,
        ReplaceAll
    }

    public sealed class ImportRunSummary
    {
        public int  TotalRows      { get; set; }
        public int  AddedRows      { get; set; }
        public int  UpdatedRows    { get; set; }
        public int  SkippedRows    { get; set; }
        public int  FailedRows     { get; set; }
        public TimeSpan Duration   { get; set; }
        public double RowsPerSec   { get; set; }
        public List<ImportRowError> Errors { get; set; } = new();
    }

    public sealed class ImportRowError
    {
        public int    RowIndex     { get; set; }
        public string FieldName    { get; set; } = string.Empty;
        public string Value        { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public sealed class ImportFieldMapRow
    {
        public bool   Selected         { get; set; }
        public string SourceField      { get; set; } = string.Empty;
        public string SourceType       { get; set; } = string.Empty;
        public string DestinationField { get; set; } = string.Empty;
        public string DestinationType  { get; set; } = string.Empty;
        /// <summary>
        /// Optional transform expression applied before writing to destination.
        /// Supported tokens: TRIM, UPPER, LOWER, DATE:{format}, or leave empty.
        /// </summary>
        public string Transform        { get; set; } = string.Empty;
        /// <summary>Read-only type-compatibility indicator: ✓ / ⚠ / ✕</summary>
        [System.ComponentModel.Browsable(false)]
        public string TypeStatus
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SourceType) || string.IsNullOrWhiteSpace(DestinationType)) return "✓";
                if (string.Equals(SourceType, DestinationType, StringComparison.OrdinalIgnoreCase)) return "✓";
                var numericTypes = new[] { "int", "integer", "long", "short", "byte", "decimal", "float", "double", "numeric", "number" };
                bool srcNum = numericTypes.Any(n => SourceType.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0);
                bool dstNum = numericTypes.Any(n => DestinationType.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0);
                if (srcNum && dstNum) return "✓";
                var dateTypes = new[] { "date", "datetime", "timestamp", "time" };
                bool srcDate = dateTypes.Any(n => SourceType.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0);
                bool dstDate = dateTypes.Any(n => DestinationType.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0);
                if (srcDate && dstDate) return "✓";
                var strTypes = new[] { "varchar", "nvarchar", "text", "string", "char" };
                bool dstStr = strTypes.Any(n => DestinationType.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0);
                return dstStr ? "⚠" : "✕";
            }
        }
    }

   
    public partial class uc_Import_SelectDSandEntity : TemplateUserControl, IWizardStepContent
    {
        private bool _isInitializing;

        public uc_Import_SelectDSandEntity(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            _sourceEntityCombo.SelectedItemChanged      += EntityCombo_Changed;
            _destinationEntityCombo.SelectedItemChanged += EntityCombo_Changed;
            SourcebeepComboBox.SelectedItemChanged += SourceDS_Changed;
            beepComboBox1.SelectedItemChanged       += DestDS_Changed;
            AddSourcebeepButton.Click               += (_, _) => { LoadDataSources(); RaiseValidationState(); };
            beepCheckBoxBool1.StateChanged          += (_, _) => RaiseValidationState();
            beepCheckBoxBool1.CurrentValue           = true;

            // ── Seed the Purpose combo ────────────────────────────────────────
            var purposeItems = new BindingList<SimpleItem>
            {
                new SimpleItem { Text = "Add Only",       Item = ImportPurpose.AddOnly      },
                new SimpleItem { Text = "Add or Update",  Item = ImportPurpose.AddOrUpdate  },
                new SimpleItem { Text = "Replace All",    Item = ImportPurpose.ReplaceAll   }
            };
            cmbPurpose.ListItems = purposeItems;
            cmbPurpose.SelectItemByText("Add Only");
            cmbPurpose.SelectedItemChanged += PurposeCombo_Changed;

            btnRefreshCount.Click += (_, _) => _ = RefreshRowCountAsync();
        }

        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;
        public bool   IsComplete     => ValidateStep().IsValid;
        public string NextButtonText => string.Empty;

        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            base.OnNavigatedTo(parameters);
            LoadDataSources();
            RaiseValidationState();
        }

        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);
            LoadDataSources();
            RaiseValidationState();
        }

        public void OnStepEnter(WizardContext context)
        {
            _isInitializing = true;
            try
            {
                LoadDataSources();
                var config = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null)
                             ?? new DataImportConfiguration();
                RestoreFromConfig(config);

                // Restore new wizard-level keys
                var purpose = context.GetValue(WizardKeys.Purpose, ImportPurpose.AddOnly);
                SelectPurposeItem(purpose);
                ApplyPurposeVisibility(purpose);

                var matchBy = context.GetValue(WizardKeys.MatchByField, string.Empty);
                if (!string.IsNullOrWhiteSpace(matchBy))
                    cmbMatchBy.SelectItemByText(matchBy);

                chkUpdateEmpty.CurrentValue = context.GetValue(WizardKeys.UpdateEmptyFields, false);
            }
            finally { _isInitializing = false; }
            _ = RefreshRowCountAsync();
            RaiseValidationState();
        }

        public void OnStepLeave(WizardContext context)
        {
            var existing = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null)
                           ?? new DataImportConfiguration();
            ApplySelectionToConfig(existing);
            context.SetValue(WizardKeys.ImportConfig, existing);

            // Persist new fields
            var purpose = GetSelectedPurpose();
            context.SetValue(WizardKeys.Purpose,          purpose);
            context.SetValue(WizardKeys.MatchByField,     cmbMatchBy.SelectedItem?.Text ?? string.Empty);
            context.SetValue(WizardKeys.UpdateEmptyFields, chkUpdateEmpty.CurrentValue);
        }

        WizardValidationResult IWizardStepContent.Validate() => ValidateStep();
        public Task<WizardValidationResult> ValidateAsync()  => Task.FromResult(ValidateStep());

        // ── Private helpers ────────────────────────────────────────────────

        private WizardValidationResult ValidateStep()
        {
            if (string.IsNullOrWhiteSpace(SourcebeepComboBox.SelectedItem?.Text))
                return WizardValidationResult.Error("Select a source data source.");
            if (string.IsNullOrWhiteSpace(_sourceEntityCombo.SelectedItem?.Text))
                return WizardValidationResult.Error("Select a source entity.");
            if (string.IsNullOrWhiteSpace(beepComboBox1.SelectedItem?.Text))
                return WizardValidationResult.Error("Select a destination data source.");
            if (string.IsNullOrWhiteSpace(_destinationEntityCombo.SelectedItem?.Text))
                return WizardValidationResult.Error("Select a destination entity.");
            return WizardValidationResult.Success();
        }

        private void ApplySelectionToConfig(DataImportConfiguration config)
        {
            config.SourceDataSourceName  = SourcebeepComboBox.SelectedItem?.Text ?? string.Empty;
            config.SourceEntityName      = _sourceEntityCombo.SelectedItem?.Text ?? string.Empty;
            config.DestDataSourceName    = beepComboBox1.SelectedItem?.Text ?? string.Empty;
            config.DestEntityName        = _destinationEntityCombo.SelectedItem?.Text ?? string.Empty;
            config.CreateDestinationIfNotExists = beepCheckBoxBool1.CurrentValue;

            // Map purpose → SyncMode + upsert key
            var purpose = GetSelectedPurpose();
            config.SyncMode = purpose switch
            {
                ImportPurpose.AddOrUpdate => SyncMode.Upsert,
                ImportPurpose.ReplaceAll  => SyncMode.FullRefresh,
                _                         => SyncMode.FullRefresh
            };
            if (purpose == ImportPurpose.AddOrUpdate)
                config.WatermarkColumn = cmbMatchBy.SelectedItem?.Text ?? string.Empty;
        }

        private void RestoreFromConfig(DataImportConfiguration config)
        {
            _isInitializing = true;
            try
            {
                if (!string.IsNullOrWhiteSpace(config.SourceDataSourceName))
                    SourcebeepComboBox.SelectItemByText(config.SourceDataSourceName);
                if (!string.IsNullOrWhiteSpace(config.DestDataSourceName))
                    beepComboBox1.SelectItemByText(config.DestDataSourceName);

                LoadEntitiesForSelectedSource();
                LoadEntitiesForSelectedDestination();

                if (!string.IsNullOrWhiteSpace(config.SourceEntityName))
                    _sourceEntityCombo.SelectItemByText(config.SourceEntityName);
                if (!string.IsNullOrWhiteSpace(config.DestEntityName))
                    _destinationEntityCombo.SelectItemByText(config.DestEntityName);

                beepCheckBoxBool1.CurrentValue = config.CreateDestinationIfNotExists;
            }
            finally { _isInitializing = false; }
        }

        private void SourceDS_Changed(object? sender, SelectedItemChangedEventArgs e)
        {
            if (_isInitializing) return;
            LoadEntitiesForSelectedSource();
            RaiseValidationState();
        }

        private void DestDS_Changed(object? sender, SelectedItemChangedEventArgs e)
        {
            if (_isInitializing) return;
            LoadEntitiesForSelectedDestination();
            RaiseValidationState();
        }

        private void EntityCombo_Changed(object? sender, SelectedItemChangedEventArgs e)
        {
            if (!_isInitializing) RaiseValidationState();
        }

        private void LoadDataSources()
        {
            var items = new BindingList<SimpleItem>(
                (Editor?.ConfigEditor?.DataConnections ?? new List<ConnectionProperties>())
                .Select(p => p.ConnectionName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n)
                .Select(n => new SimpleItem { Text = n, Item = n })
                .ToList());
            SourcebeepComboBox.ListItems = items;
            beepComboBox1.ListItems      = new BindingList<SimpleItem>(items.ToList());
        }

        private void LoadEntitiesForSelectedSource()
            => PopulateEntityCombo(_sourceEntityCombo, SourcebeepComboBox.SelectedItem?.Text ?? string.Empty);

        private void LoadEntitiesForSelectedDestination()
            => PopulateEntityCombo(_destinationEntityCombo, beepComboBox1.SelectedItem?.Text ?? string.Empty);

        private void PopulateEntityCombo(BeepComboBox combo, string dataSourceName)
        {
            var prev  = combo.SelectedItem?.Text ?? string.Empty;
            var items = GetEntityNames(dataSourceName)
                .Select(n => new SimpleItem { Text = n, Item = n }).ToList();
            combo.ListItems = new BindingList<SimpleItem>(items);
            if (!string.IsNullOrWhiteSpace(prev))
                combo.SelectItemByText(prev);
            else if (items.Count > 0)
                combo.SelectItemByText(items[0].Text);
        }

        private List<string> GetEntityNames(string dataSourceName)
        {
            if (Editor == null || string.IsNullOrWhiteSpace(dataSourceName)) return new List<string>();
            try
            {
                var ds = Editor.GetDataSource(dataSourceName);
                if (ds == null) return new List<string>();
                if (ds.ConnectionStatus != ConnectionState.Open) ds.Openconnection();
                var list = ds.GetEntitesList()?.ToList() ?? new List<string>();
                if (list.Count == 0 && ds.EntitiesNames != null) list = ds.EntitiesNames.ToList();
                return list.Where(n => !string.IsNullOrWhiteSpace(n))
                           .Distinct(System.StringComparer.OrdinalIgnoreCase)
                           .OrderBy(n => n).ToList();
            }
            catch (System.Exception ex)
            {
                Editor?.AddLogMessage("ImportExport", $"Error loading entities for '{dataSourceName}': {ex.Message}",
                    System.DateTime.Now, 0, null, Errors.Failed);
                return new List<string>();
            }
        }

        private void RaiseValidationState()
        {
            var result = ValidateStep();
            _statusLabel.Text = result.IsValid
                ? "Selection is valid. Continue to mapping."
                : result.ErrorMessage ?? "Selection is incomplete.";
            ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(result.IsValid, result.ErrorMessage));
        }

        // ── Purpose helpers ────────────────────────────────────────────────────

        private void PurposeCombo_Changed(object? sender, SelectedItemChangedEventArgs e)
        {
            if (_isInitializing) return;
            var purpose = GetSelectedPurpose();
            ApplyPurposeVisibility(purpose);
            RaiseValidationState();
        }

        private ImportPurpose GetSelectedPurpose()
        {
            if (cmbPurpose.SelectedItem?.Item is ImportPurpose p) return p;
            return ImportPurpose.AddOnly;
        }

        private void SelectPurposeItem(ImportPurpose purpose)
        {
            var text = purpose switch
            {
                ImportPurpose.AddOrUpdate => "Add or Update",
                ImportPurpose.ReplaceAll  => "Replace All",
                _                         => "Add Only"
            };
            cmbPurpose.SelectItemByText(text);
        }

        private void ApplyPurposeVisibility(ImportPurpose purpose)
        {
            bool isUpsert = purpose == ImportPurpose.AddOrUpdate;
            lblMatchBy.Visible      = isUpsert;
            cmbMatchBy.Visible      = isUpsert;
            chkUpdateEmpty.Visible  = isUpsert;
            if (isUpsert) PopulateMatchByCombo();
        }

        private void PopulateMatchByCombo()
        {
            var srcDs     = SourcebeepComboBox.SelectedItem?.Text ?? string.Empty;
            var srcEntity = _sourceEntityCombo.SelectedItem?.Text ?? string.Empty;
            var prev      = cmbMatchBy.SelectedItem?.Text ?? string.Empty;

            var fields = GetEntityFields(srcDs, srcEntity)
                .Select(f => new SimpleItem { Text = f, Item = f })
                .ToList();
            cmbMatchBy.ListItems = new BindingList<SimpleItem>(fields);
            if (!string.IsNullOrWhiteSpace(prev))
                cmbMatchBy.SelectItemByText(prev);
            else if (fields.Count > 0)
                cmbMatchBy.SelectItemByText(fields[0].Text);
        }

        private async Task RefreshRowCountAsync()
        {
            var srcDs     = SourcebeepComboBox.SelectedItem?.Text ?? string.Empty;
            var srcEntity = _sourceEntityCombo.SelectedItem?.Text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(srcDs) || string.IsNullOrWhiteSpace(srcEntity)) return;

            lblRowCount.Text = "Row count: …";
            try
            {
                var count = await Task.Run(async () =>
                {
                    var ds = Editor?.GetDataSource(srcDs);
                    if (ds == null) return -1L;
                    if (ds.ConnectionStatus != ConnectionState.Open) ds.Openconnection();
                    // Use GetScalarAsync for SQL-capable sources; fall back to -1 if unsupported
                    try
                    {
                        var result = await ds.GetScalarAsync($"SELECT COUNT(*) FROM {srcEntity}");
                        return (long)result;
                    }
                    catch
                    {
                        return -1L;
                    }
                });
                lblRowCount.Text = count >= 0 ? $"Row count: ~{count:N0}" : "Row count: —";
            }
            catch
            {
                lblRowCount.Text = "Row count: error";
            }
        }

        private List<string> GetEntityFields(string dataSourceName, string entityName)
        {
            if (Editor == null || string.IsNullOrWhiteSpace(dataSourceName) || string.IsNullOrWhiteSpace(entityName))
                return new List<string>();
            try
            {
                var ds = Editor.GetDataSource(dataSourceName);
                if (ds == null) return new List<string>();
                if (ds.ConnectionStatus != ConnectionState.Open) ds.Openconnection();
                var entity = ds.GetEntityStructure(entityName, true);
                return entity?.Fields?.Select(f => f.FieldName)
                           .Where(n => !string.IsNullOrWhiteSpace(n)).ToList()
                       ?? new List<string>();
            }
            catch { return new List<string>(); }
        }


    }
}
