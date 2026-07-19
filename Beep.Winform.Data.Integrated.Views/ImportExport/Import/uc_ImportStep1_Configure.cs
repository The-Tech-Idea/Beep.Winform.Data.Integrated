using System.ComponentModel;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport.Models;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Import
{
    public partial class uc_ImportStep1_Configure : TemplateUserControl, IWizardStepContent
    {
        private DataImportConfiguration? _config;
        private bool _isComplete;

        /// <summary>
        /// Per-combo generation counters for the entity loads. One counter per target, not one for
        /// the view: a single shared counter meant picking a destination datasource cancelled an
        /// in-flight source-entity load, leaving that combo empty until the user re-picked — and
        /// re-picking the same value raises no SelectedItemChanged, so it never recovered.
        /// </summary>
        private readonly Dictionary<BeepComboBox, int> _entityLoadGeneration = new();

        /// <summary>
        /// Own counters, so these cancel their own predecessors rather than each other's. Sharing
        /// the entity counter meant a dest-combo change stranded the row-count label on
        /// "Counting..." forever, while two rapid entity changes cancelled neither and could paint
        /// the first entity's count beside the second entity's name.
        /// </summary>
        private int _rowCountGeneration;
        private int _matchByGeneration;

        /// <summary>
        /// Designer/parameterless ctor. Must not chain to the IServiceProvider overload with null —
        /// that resolves services off a null provider and throws.
        /// </summary>
        public uc_ImportStep1_Configure() => InitializeControl();

        public uc_ImportStep1_Configure(IServiceProvider services) : base(services) => InitializeControl();

        private void InitializeControl()
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

        // Fire-and-forget: LoadEntities catches its own failures and the handler signature is void.
        private void OnSourceDSChanged()
        {
            var dsName = cmbSourceDS.SelectedItem?.Value?.ToString();
            if (string.IsNullOrEmpty(dsName)) return;
            _ = LoadEntities(dsName, cmbSourceEntity);
        }

        private void OnDestDSChanged()
        {
            var dsName = cmbDestDS.SelectedItem?.Value?.ToString();
            if (string.IsNullOrEmpty(dsName)) return;
            _ = LoadEntities(dsName, cmbDestEntity);
        }

        /// <summary>
        /// Fills an entity picker. GetDataSource resolves a driver and GetEntitesList is a metadata
        /// round-trip, so both run off the UI thread.
        /// </summary>
        /// <param name="selectValue">
        /// Selection to apply once the items exist. It has to be applied here rather than by the
        /// caller: this method now returns at its first await, so a caller that selected straight
        /// afterwards was iterating an empty list and silently selecting nothing.
        /// </param>
        private async Task LoadEntities(string dsName, BeepComboBox combo, string? selectValue = null)
        {
            var editor = Editor;
            if (editor == null) return;

            int generation = _entityLoadGeneration.TryGetValue(combo, out var g) ? g + 1 : 1;
            _entityLoadGeneration[combo] = generation;
            try
            {
                var entities = await Task.Run(() => editor.GetDataSource(dsName)?.GetEntitesList())
                    .ConfigureAwait(true);

                if (_entityLoadGeneration[combo] != generation || IsDisposed || entities == null) return;

                var items = entities.Select(e => new SimpleItem { Text = e, Value = e }).ToList();
                combo.ListItems = new BindingList<SimpleItem>(items);
                SelectComboItem(combo, selectValue);
            }
            catch (Exception ex)
            {
                if (_entityLoadGeneration[combo] != generation || IsDisposed) return;
                editor.AddLogMessage("ImportStep1",
                    $"Could not list entities for '{dsName}': {ex.Message}",
                    DateTime.Now, 0, null, Errors.Warning);
            }
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

        private void PopulateMatchByFields() => _ = PopulateMatchByFieldsAsync();

        /// <summary>Fills the upsert match-by picker. GetEntityStructure is a blocking round-trip.</summary>
        private async Task PopulateMatchByFieldsAsync()
        {
            var editor = Editor;
            var config = _config;
            if (editor == null || config == null || string.IsNullOrWhiteSpace(config.SourceEntityName)) return;

            int generation = ++_matchByGeneration;
            try
            {
                var structure = await Task.Run(() =>
                    editor.GetDataSource(config.SourceDataSourceName)
                          ?.GetEntityStructure(config.SourceEntityName, false)).ConfigureAwait(true);

                if (generation != _matchByGeneration || IsDisposed || structure?.Fields == null) return;

                cmbMatchBy.ListItems = new BindingList<SimpleItem>(structure.Fields
                    .Select(f => new SimpleItem { Text = f.FieldName, Value = f.FieldName })
                    .ToList());
            }
            catch (Exception ex)
            {
                if (generation != _matchByGeneration || IsDisposed) return;
                editor.AddLogMessage("ImportStep1",
                    $"Could not read fields of '{config.SourceEntityName}': {ex.Message}",
                    DateTime.Now, 0, null, Errors.Warning);
            }
        }

        /// <summary>
        /// Shows roughly how many rows the source holds.
        /// </summary>
        /// <remarks>
        /// Asks for a single-row page and reads <c>PagedResult.TotalRecords</c>, rather than what
        /// this used to do: <c>GetEntity(entity, null).Count()</c> — which dragged the ENTIRE table
        /// across the wire and materialised every row, purely to render a count label. On a large
        /// source that is minutes of transfer and a heap full of rows nobody asked for.
        /// <para>
        /// TotalRecords is only reported by providers that compute it — the in-memory/cache sources
        /// discard it and RDBSource swallows a failing COUNT — so a 0 means "not reported", not
        /// "empty". It is shown as unavailable rather than as a zero row count, and the full read is
        /// not attempted as a fallback: the cost is exactly what this exists to avoid.
        /// </para>
        /// </remarks>
        private async Task RefreshRowCountAsync()
        {
            var editor = Editor;
            var config = _config;
            if (editor == null || config == null || string.IsNullOrEmpty(config.SourceEntityName)) return;

            int generation = ++_rowCountGeneration;
            lblRowCount.Text = "Counting...";
            try
            {
                var paged = await Task.Run(() =>
                    editor.GetDataSource(config.SourceDataSourceName)
                          ?.GetEntity(config.SourceEntityName, null, 1, 1)).ConfigureAwait(true);

                if (generation != _rowCountGeneration || IsDisposed) return;

                lblRowCount.Text = paged == null ? "N/A"
                    : paged.TotalRecords > 0 ? $"~{paged.TotalRecords:N0} rows"
                    : "Row count not reported by this source";
            }
            catch (Exception ex)
            {
                if (generation != _rowCountGeneration || IsDisposed) return;
                lblRowCount.Text = "N/A";
                // The bare catch here swallowed the reason entirely, leaving "N/A" to mean both
                // "not supported" and "the datasource is unreachable".
                editor.AddLogMessage("ImportStep1",
                    $"Could not count rows of '{config.SourceEntityName}': {ex.Message}",
                    DateTime.Now, 0, null, Errors.Warning);
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

        /// <summary>
        /// Reapplies the saved selections when the step is re-entered.
        /// </summary>
        /// <remarks>
        /// The entity selections are handed to LoadEntities rather than applied here. Selecting
        /// immediately after kicking off the load raced an empty combo and silently selected
        /// nothing, so returning to this step showed an empty entity picker.
        /// </remarks>
        private void RestoreSelections()
        {
            if (_config == null) return;
            SelectComboItem(cmbSourceDS, _config.SourceDataSourceName);
            SelectComboItem(cmbDestDS, _config.DestDataSourceName);

            if (!string.IsNullOrEmpty(_config.SourceDataSourceName))
                _ = LoadEntities(_config.SourceDataSourceName, cmbSourceEntity, _config.SourceEntityName);

            if (!string.IsNullOrEmpty(_config.DestDataSourceName))
                _ = LoadEntities(_config.DestDataSourceName, cmbDestEntity, _config.DestEntityName);

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
