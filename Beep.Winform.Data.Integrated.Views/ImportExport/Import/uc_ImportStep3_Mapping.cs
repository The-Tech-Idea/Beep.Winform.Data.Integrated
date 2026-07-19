using System.ComponentModel;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport.Models;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Import
{
    public partial class uc_ImportStep3_Mapping : TemplateUserControl, IWizardStepContent
    {
        private DataImportConfiguration? _config;
        private List<EntityField>? _sourceFields;
        private List<EntityField>? _destFields;

        /// <summary>
        /// Designer/parameterless ctor. Must not chain to the IServiceProvider overload with null —
        /// that resolves services off a null provider and throws.
        /// </summary>
        public uc_ImportStep3_Mapping() => InitializeControl();

        public uc_ImportStep3_Mapping(IServiceProvider services) : base(services) => InitializeControl();

        private void InitializeControl()
        {
            InitializeComponent();
            btnTemplateSave.Click += (_, _) => SaveTemplate();
            btnTemplateLoad.Click += (_, _) => LoadTemplate();
        }

        public bool IsComplete => true;
        public string NextButtonText => "Next";
        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;

        public void OnStepEnter(WizardContext context)
        {
            _config = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null);
            if (_config == null) return;
            LoadFields();
            BuildMappingGrid();
            var templateName = context.GetValue<string?>(WizardKeys.TemplateName, null);
            if (!string.IsNullOrEmpty(templateName))
                cmbTemplateLoad.Text = templateName;
        }

        public void OnStepLeave(WizardContext context)
        {
            if (_config == null) return;
            _config.Mapping = BuildMappingFromGrid();
            context.SetValue(WizardKeys.ImportConfig, _config);
        }

        public WizardValidationResult Validate()
        {
            if (_config?.Mapping == null)
                return WizardValidationResult.Error("Field mapping is not configured.");
            var mapped = _config.Mapping.MappedEntities?
                .SelectMany(d => d.FieldMapping ?? new List<Mapping_rep_fields>())
                .Count() ?? 0;
            if (mapped == 0)
                return WizardValidationResult.Error("At least one field must be mapped.");
            return WizardValidationResult.Success();
        }

        public Task<WizardValidationResult> ValidateAsync() => Task.FromResult(Validate());

        private void LoadFields()
        {
            if (Editor == null || _config == null) return;
            var srcDs = Editor.GetDataSource(_config.SourceDataSourceName);
            var destDs = Editor.GetDataSource(_config.DestDataSourceName);
            var srcStruct = srcDs?.GetEntityStructure(_config.SourceEntityName, false);
            var destStruct = destDs?.GetEntityStructure(_config.DestEntityName, false);
            _sourceFields = srcStruct?.Fields;
            _destFields = destStruct?.Fields;
            PopulateTemplateCombo();
        }

        private void BuildMappingGrid()
        {
            mappingGrid.Rows.Clear();
            mappingGrid.Columns.Clear();
            mappingGrid.Columns.Add("srcName", "Source Field");
            mappingGrid.Columns.Add("srcType", "Source Type");
            mappingGrid.Columns.Add("destName", "Destination Field");
            mappingGrid.Columns.Add("destType", "Destination Type");
            mappingGrid.Columns.Add("status", "Match");

            var destCombo = new DataGridViewComboBoxColumn { Name = "destCombo", HeaderText = "Destination Field" };

            if (_sourceFields != null && _destFields != null)
            {
                foreach (var sf in _sourceFields)
                {
                    destCombo.Items.Clear();
                    foreach (var df in _destFields)
                        destCombo.Items.Add(df.FieldName);

                    var autoMatch = _destFields.FirstOrDefault(d =>
                        string.Equals(d.FieldName, sf.FieldName, StringComparison.OrdinalIgnoreCase));
                    int rowIdx = mappingGrid.Rows.Add(sf.FieldName, sf.Fieldtype, autoMatch?.FieldName ?? "", autoMatch?.Fieldtype ?? "", "");
                    mappingGrid.Rows[rowIdx].Cells["destName"] = new DataGridViewComboBoxCell();
                    ((DataGridViewComboBoxCell)mappingGrid.Rows[rowIdx].Cells["destName"]).Items.Clear();
                    foreach (var df in _destFields)
                        ((DataGridViewComboBoxCell)mappingGrid.Rows[rowIdx].Cells["destName"]).Items.Add(df.FieldName);
                    mappingGrid.Rows[rowIdx].Cells["destName"].Value = autoMatch?.FieldName ?? "";

                    UpdateTypeMatchStatus(rowIdx, sf.Fieldtype, autoMatch?.Fieldtype);
                }
            }
            lblMappingStatus.Text = $"{mappingGrid.Rows.Count} source fields loaded";
        }

        private void UpdateTypeMatchStatus(int rowIdx, string? srcType, string? destType)
        {
            if (string.IsNullOrEmpty(destType))
            {
                mappingGrid.Rows[rowIdx].Cells["status"].Value = "—";
                return;
            }
            if (string.Equals(srcType, destType, StringComparison.OrdinalIgnoreCase))
                mappingGrid.Rows[rowIdx].Cells["status"].Value = "✓";
            else
                mappingGrid.Rows[rowIdx].Cells["status"].Value = "⚠";
        }

        private EntityDataMap BuildMappingFromGrid()
        {
            var map = new EntityDataMap
            {
                MappingName = $"{_config?.SourceEntityName}_to_{_config?.DestEntityName}",
                EntityName = _config?.SourceEntityName ?? string.Empty,
                EntityDataSource = _config?.SourceDataSourceName ?? string.Empty,
                EntityFields = _sourceFields ?? new List<EntityField>(),
                MappedEntities = new List<EntityDataMap_DTL>(),
            };

            var dtl = new EntityDataMap_DTL
            {
                EntityName = _config?.DestEntityName ?? string.Empty,
                EntityDataSource = _config?.DestDataSourceName ?? string.Empty,
                EntityFields = _destFields ?? new List<EntityField>(),
                SelectedDestFields = _destFields ?? new List<EntityField>(),
                FieldMapping = new List<Mapping_rep_fields>(),
            };

            foreach (DataGridViewRow row in mappingGrid.Rows)
            {
                var srcField = row.Cells["srcName"].Value?.ToString();
                var destField = row.Cells["destName"].Value?.ToString();
                if (string.IsNullOrEmpty(srcField) || string.IsNullOrEmpty(destField)) continue;

                dtl.FieldMapping.Add(new Mapping_rep_fields
                {
                    FromEntityName = _config?.SourceEntityName,
                    FromFieldName = srcField,
                    FromFieldType = row.Cells["srcType"].Value?.ToString(),
                    ToEntityName = _config?.DestEntityName,
                    ToFieldName = destField,
                    ToFieldType = row.Cells["destType"].Value?.ToString(),
                });
            }

            map.MappedEntities.Add(dtl);
            return map;
        }

        private void PopulateTemplateCombo()
        {
            if (Editor == null) return;
            var names = ImportTemplateManager.ListAll(Editor);
            cmbTemplateLoad.ListItems = new BindingList<SimpleItem>(
                names.Select(n => new SimpleItem { Text = n, Value = n }).ToList());
        }

        private void SaveTemplate()
        {
            if (_config == null || Editor == null) return;
            var name = cmbTemplateLoad.Text;
            if (string.IsNullOrWhiteSpace(name)) return;
            _config.Mapping = BuildMappingFromGrid();
            ImportTemplateManager.Save(Editor, name, _config);
            lblMappingStatus.Text = $"Template '{name}' saved.";
        }

        private void LoadTemplate()
        {
            if (Editor == null) return;
            var name = cmbTemplateLoad.SelectedItem?.Value?.ToString();
            if (string.IsNullOrEmpty(name)) return;
            var loaded = ImportTemplateManager.Load(Editor, name);
            if (loaded?.Mapping?.MappedEntities != null)
            {
                _config = loaded;
                BuildMappingGrid();
                lblMappingStatus.Text = $"Template '{name}' loaded.";
            }
        }
    }
}
