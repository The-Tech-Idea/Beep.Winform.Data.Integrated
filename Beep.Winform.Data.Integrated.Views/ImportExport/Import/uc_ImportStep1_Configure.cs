using System.ComponentModel;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport.Models;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Import
{
    public partial class uc_ImportStep1_Configure : TemplateUserControl, IWizardStepContent
    {
        private DataImportConfiguration? _config;
        private bool _isComplete;

        public uc_ImportStep1_Configure(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            SetupEvents();
        }

        public bool IsComplete => _isComplete;
        public string NextButtonText => "Next";

        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;

        public void OnStepEnter(WizardContext context)
        {
            LoadDataSources();
            var existing = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null);
            if (existing != null)
            {
                _config = existing;
                RestoreSelections();
            }
            else
            {
                _config = new DataImportConfiguration();
            }
        }

        public void OnStepLeave(WizardContext context)
        {
            if (_config == null) return;

            ImportTemplateManager.ApplyPurpose(_config, GetSelectedPurpose());
            _config.UpsertKeyColumns = GetSelectedPurpose() == ImportPurpose.AddOrUpdate
                ? new List<string> { cmbMatchBy.SelectedItem?.Value?.ToString() ?? string.Empty }
                : new List<string>();
            _config.CreateDestinationIfNotExists = chkCreateDest.Checked;

            context.SetValue(WizardKeys.ImportConfig, _config);
            context.SetValue(WizardKeys.Purpose, GetSelectedPurpose());
            context.SetValue(WizardKeys.MatchByField, cmbMatchBy.SelectedItem?.Value?.ToString() ?? string.Empty);
            context.SetValue(WizardKeys.UpdateEmptyFields, chkUpdateEmpty.Checked);
        }

        public WizardValidationResult Validate()
        {
            if (_config == null || string.IsNullOrWhiteSpace(_config.SourceDataSourceName))
                return WizardValidationResult.Error("Please select a source data source.");
            if (string.IsNullOrWhiteSpace(_config.SourceEntityName))
                return WizardValidationResult.Error("Please select a source entity.");
            if (string.IsNullOrWhiteSpace(_config.DestDataSourceName))
                return WizardValidationResult.Error("Please select a destination data source.");
            if (string.IsNullOrWhiteSpace(_config.DestEntityName))
                return WizardValidationResult.Error("Please select a destination entity.");
            return WizardValidationResult.Success();
        }

        public Task<WizardValidationResult> ValidateAsync() => Task.FromResult(Validate());

        private void SetupEvents()
        {
            cmbSourceDS.SelectedItemChanged += (_, _) => OnSourceDSChanged();
            cmbDestDS.SelectedItemChanged += (_, _) => OnDestDSChanged();
            cmbPurpose.SelectedItemChanged += (_, _) => OnPurposeChanged();
            cmbSourceEntity.SelectedItemChanged += (_, _) => OnSourceEntityChanged();
            btnRefreshCount.Click += (_, _) => _ = RefreshRowCountAsync();
        }

        private void LoadDataSources()
        {
            if (Editor?.ConfigEditor?.DataConnections == null) return;

            var dsNames = Editor.ConfigEditor.DataConnections
                .Select(c => c.ConnectionName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .ToList();

            var items = dsNames.Select(n => new SimpleItem { Text = n, Value = n }).ToList();
            cmbSourceDS.ListItems = new BindingList<SimpleItem>(items);
            cmbDestDS.ListItems = new BindingList<SimpleItem>(items);

            cmbPurpose.ListItems = new BindingList<SimpleItem>(new List<SimpleItem>
            {
                new() { Text = "Add Only", Value = ImportPurpose.AddOnly },
                new() { Text = "Add or Update", Value = ImportPurpose.AddOrUpdate },
                new() { Text = "Replace All", Value = ImportPurpose.ReplaceAll },
            });
            cmbPurpose.SelectedIndex = 0;
        }

        private void OnSourceDSChanged()
        {
            var dsName = cmbSourceDS.SelectedItem?.Value?.ToString();
            if (string.IsNullOrEmpty(dsName)) return;
            LoadEntities(dsName, cmbSourceEntity);
        }

        private void OnDestDSChanged()
        {
            var dsName = cmbDestDS.SelectedItem?.Value?.ToString();
            if (string.IsNullOrEmpty(dsName)) return;
            LoadEntities(dsName, cmbDestEntity);
        }

        private void LoadEntities(string dsName, BeepComboBox combo)
        {
            if (Editor == null) return;
            var ds = Editor.GetDataSource(dsName);
            if (ds == null) return;
            var entities = ds.GetEntitesList();
            if (entities == null) return;

            var items = entities.Select(e => new SimpleItem { Text = e, Value = e }).ToList();
            combo.ListItems = new BindingList<SimpleItem>(items);
        }

        private void OnPurposeChanged()
        {
            var purpose = GetSelectedPurpose();
            bool isUpsert = purpose == ImportPurpose.AddOrUpdate;
            lblMatchBy.Visible = isUpsert;
            cmbMatchBy.Visible = isUpsert;
            chkUpdateEmpty.Visible = isUpsert;

            if (isUpsert && cmbMatchBy.ListItems.Count == 0)
                PopulateMatchByFields();
        }

        private void OnSourceEntityChanged()
        {
            _config ??= new DataImportConfiguration();
            _config.SourceDataSourceName = cmbSourceDS.SelectedItem?.Value?.ToString() ?? string.Empty;
            _config.SourceEntityName = cmbSourceEntity.SelectedItem?.Value?.ToString() ?? string.Empty;
            _config.DestDataSourceName = cmbDestDS.SelectedItem?.Value?.ToString() ?? string.Empty;
            _config.DestEntityName = cmbDestEntity.SelectedItem?.Value?.ToString() ?? string.Empty;

            PopulateMatchByFields();
            _ = RefreshRowCountAsync();
            UpdateCompleteness();
        }

        private void PopulateMatchByFields()
        {
            if (Editor == null || _config == null) return;
            var ds = Editor.GetDataSource(_config.SourceDataSourceName);
            if (ds == null) return;
            var structure = ds.GetEntityStructure(_config.SourceEntityName, false);
            if (structure?.Fields == null) return;

            cmbMatchBy.ListItems = new BindingList<SimpleItem>(structure.Fields
                .Select(f => new SimpleItem { Text = f.FieldName, Value = f.FieldName })
                .ToList());
        }

        private async Task RefreshRowCountAsync()
        {
            if (Editor == null || _config == null || string.IsNullOrEmpty(_config.SourceEntityName)) return;
            lblRowCount.Text = "Counting...";
            try
            {
                var ds = Editor.GetDataSource(_config.SourceDataSourceName);
                if (ds == null) { lblRowCount.Text = "N/A"; return; }
                var data = await Task.Run(() => ds.GetEntity(_config.SourceEntityName, null));
                lblRowCount.Text = $"~{data?.Count():N0} rows";
            }
            catch
            {
                lblRowCount.Text = "N/A";
            }
        }

        private ImportPurpose GetSelectedPurpose()
        {
            var item = cmbPurpose.SelectedItem?.Value;
            return item is ImportPurpose p ? p : ImportPurpose.AddOnly;
        }

        private void UpdateCompleteness()
        {
            if (_config == null) return;
            bool wasComplete = _isComplete;
            _isComplete = !string.IsNullOrWhiteSpace(_config.SourceDataSourceName)
                       && !string.IsNullOrWhiteSpace(_config.SourceEntityName)
                       && !string.IsNullOrWhiteSpace(_config.DestDataSourceName)
                       && !string.IsNullOrWhiteSpace(_config.DestEntityName);
            if (wasComplete != _isComplete)
                ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(_isComplete));
        }

        private void RestoreSelections()
        {
            if (_config == null) return;
            SelectComboItem(cmbSourceDS, _config.SourceDataSourceName);
            SelectComboItem(cmbDestDS, _config.DestDataSourceName);
            if (!string.IsNullOrEmpty(_config.SourceDataSourceName))
            {
                LoadEntities(_config.SourceDataSourceName, cmbSourceEntity);
                SelectComboItem(cmbSourceEntity, _config.SourceEntityName);
            }
            if (!string.IsNullOrEmpty(_config.DestDataSourceName))
            {
                LoadEntities(_config.DestDataSourceName, cmbDestEntity);
                SelectComboItem(cmbDestEntity, _config.DestEntityName);
            }
            chkCreateDest.Checked = _config.CreateDestinationIfNotExists;
        }

        private static void SelectComboItem(BeepComboBox combo, string? value)
        {
            if (string.IsNullOrEmpty(value)) return;
            for (int i = 0; i < combo.ListItems?.Count; i++)
            {
                if (combo.ListItems[i]?.Value?.ToString() == value)
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
        }
    }
}
