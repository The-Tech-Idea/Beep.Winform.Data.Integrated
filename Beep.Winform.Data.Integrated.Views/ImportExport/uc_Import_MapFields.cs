using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.Importing;
using TheTechIdea.Beep.Editor.Importing.Interfaces;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Default.Views.Template;
using TheTechIdea.Beep.Workflow;            // Mapping_rep_fields
using TheTechIdea.Beep.Workflow.Mapping;    // EntityDataMap, EntityDataMap_DTL

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
   
    public partial class uc_Import_MapFields : TemplateUserControl, IWizardStepContent
    {
        private BindingList<ImportFieldMapRow> _rows = new();
        private DataImportConfiguration?      _config;

        public uc_Import_MapFields(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            AttachGridEvents();

            // Template bar wiring
            btnTemplateSave.Click   += TemplateSave_Click;
            btnTemplateDelete.Click += TemplateDelete_Click;
            cmbTemplateLoad.SelectedItemChanged += TemplateLoad_Changed;
            LoadTemplateNames();
        }

        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;
        public bool   IsComplete     => _rows.Any(r => r.Selected);
        public string NextButtonText => string.Empty;

        public override void OnNavigatedTo(Dictionary<string, object> parameters)  { base.OnNavigatedTo(parameters); RaiseValidationState(); }
        public override void Configure(Dictionary<string, object> settings)        { base.Configure(settings);       RaiseValidationState(); }

        public void OnStepEnter(WizardContext context)
        {
            _config = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null);
            if (_config == null) return;
            LoadTemplateNames();
            LoadFieldMapping();
        }

        public void OnStepLeave(WizardContext context)
        {
            if (_config == null) return;
            _config.Mapping = BuildMappingFromRows();
            context.SetValue(WizardKeys.ImportConfig, _config);
        }

        WizardValidationResult IWizardStepContent.Validate() => ValidateStep();
        public Task<WizardValidationResult> ValidateAsync()  => Task.FromResult(ValidateStep());

        // ── Private helpers ───────────────────────────────────────────────

        private WizardValidationResult ValidateStep()
        {
            if (!_rows.Any(r => r.Selected))
                return WizardValidationResult.Error("Select at least one field to map.");
            return WizardValidationResult.Success();
        }

        private void LoadFieldMapping()
        {
            if (_config == null || Editor == null) return;
            _rows.Clear();

            var srcFields  = GetFields(_config.SourceDataSourceName,  _config.SourceEntityName);
            var destFields = GetFields(_config.DestDataSourceName,     _config.DestEntityName);

            // Restore previously saved mapping if present (from EntityDataMap.MappedEntities[0].FieldMapping)
            var existingFieldMappings = _config.Mapping?.MappedEntities
                ?.SelectMany(d => d.FieldMapping ?? new List<Mapping_rep_fields>())
                .ToList() ?? new List<Mapping_rep_fields>();

            foreach (var sf in srcFields)
            {
                var saved = existingFieldMappings.FirstOrDefault(m =>
                    string.Equals(m.FromFieldName, sf.FieldName, StringComparison.OrdinalIgnoreCase));

                var matchedDest = saved != null
                    ? destFields.FirstOrDefault(d => string.Equals(d.FieldName, saved.ToFieldName, StringComparison.OrdinalIgnoreCase))
                    : destFields.FirstOrDefault(d => string.Equals(d.FieldName, sf.FieldName, StringComparison.OrdinalIgnoreCase));

                _rows.Add(new ImportFieldMapRow
                {
                    Selected         = saved != null || matchedDest != null,
                    SourceField      = sf.FieldName,
                    SourceType       = sf.Fieldtype ?? string.Empty,
                    DestinationField = matchedDest?.FieldName ?? (destFields.Count > 0 ? destFields[0].FieldName : string.Empty),
                    DestinationType  = matchedDest?.Fieldtype ?? string.Empty,
                    Transform        = saved?.Rules ?? string.Empty
                });
            }

            BindGrid();
            RaiseValidationState();
        }

        private EntityDataMap BuildMappingFromRows()
        {
            var fieldMappings = _rows
                .Where(r => r.Selected && !string.IsNullOrWhiteSpace(r.DestinationField))
                .Select(r => new Mapping_rep_fields
                {
                    FromEntityName = _config?.SourceEntityName ?? string.Empty,
                    FromFieldName  = r.SourceField,
                    FromFieldType  = r.SourceType,
                    ToEntityName   = _config?.DestEntityName   ?? string.Empty,
                    ToFieldName    = r.DestinationField,
                    ToFieldType    = r.DestinationType,
                    Rules          = r.Transform
                })
                .ToList();

            var dtl = new EntityDataMap_DTL
            {
                EntityName       = _config?.DestEntityName       ?? string.Empty,
                EntityDataSource = _config?.DestDataSourceName   ?? string.Empty,
                FieldMapping     = fieldMappings
            };

            return new EntityDataMap
            {
                MappingName      = $"{_config?.SourceEntityName}_to_{_config?.DestEntityName}",
                EntityName       = _config?.SourceEntityName     ?? string.Empty,
                EntityDataSource = _config?.SourceDataSourceName ?? string.Empty,
                MappedEntities   = new List<EntityDataMap_DTL> { dtl }
            };
        }

        private IReadOnlyList<EntityField> GetFields(string dataSourceName, string entityName)
        {
            if (Editor == null || string.IsNullOrWhiteSpace(dataSourceName) || string.IsNullOrWhiteSpace(entityName))
                return Array.Empty<EntityField>();
            try
            {
                var ds = Editor.GetDataSource(dataSourceName);
                if (ds == null) return Array.Empty<EntityField>();
                if (ds.ConnectionStatus != System.Data.ConnectionState.Open) ds.Openconnection();
                var entity = ds.GetEntityStructure(entityName, true);
                return entity?.Fields?.Cast<EntityField>().ToList() ?? (IReadOnlyList<EntityField>)Array.Empty<EntityField>();
            }
            catch (Exception ex)
            {
                Editor?.AddLogMessage("ImportExport", $"Error loading fields for '{entityName}': {ex.Message}",
                    DateTime.Now, 0, null, Errors.Failed);
                return Array.Empty<EntityField>();
            }
        }

        private void BindGrid()
        {
            beepDataGrid1.DataSource = null;
            beepDataGrid1.DataSource = _rows;
        }

        private void AttachGridEvents()
        {
            beepDataGrid1.CellValueChanged += (_, e) =>
            {
                if (e.ColumnIndex == 0) RaiseValidationState();
            };
            // Type-mismatch colour coding
            beepDataGrid1.CellFormatting += (_, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= _rows.Count) return;
                var row = _rows[e.RowIndex];
                int mismatch = HasTypeMismatch(row.SourceType, row.DestinationType);
                if (mismatch == 0) return;
                e.CellStyle!.BackColor = mismatch == 1
                    ? Color.FromArgb(255, 255, 200)   // yellow — warning
                    : Color.FromArgb(255, 200, 200);  // red — error
            };
        }

        private void RaiseValidationState()
        {
            var result = ValidateStep();
            ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(result.IsValid, result.ErrorMessage));
            UpdateMappingStatus();
        }

        private void UpdateMappingStatus()
        {
            int mapped   = _rows.Count(r => r.Selected && !string.IsNullOrWhiteSpace(r.DestinationField));
            int total    = _rows.Count;
            int warnings = _rows.Count(r => r.Selected && HasTypeMismatch(r.SourceType, r.DestinationType) == 1);
            int errors   = _rows.Count(r => r.Selected && HasTypeMismatch(r.SourceType, r.DestinationType) == 2);
            lblMappingStatus.Text = $"{mapped} of {total} fields mapped" +
                (warnings > 0 ? $" | {warnings} warnings" : "") +
                (errors   > 0 ? $" | {errors} errors"     : "");
        }

        // Returns 0=ok, 1=warning(compatible), 2=error(incompatible)
        private static int HasTypeMismatch(string srcType, string dstType)
        {
            if (string.IsNullOrWhiteSpace(srcType) || string.IsNullOrWhiteSpace(dstType)) return 0;
            if (string.Equals(srcType, dstType, StringComparison.OrdinalIgnoreCase)) return 0;
            // Numeric family
            var numericTypes = new[] { "int", "integer", "long", "short", "byte", "decimal", "float", "double", "numeric", "number" };
            bool srcNum = numericTypes.Any(n => srcType.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0);
            bool dstNum = numericTypes.Any(n => dstType.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0);
            if (srcNum && dstNum) return 0; // compatible numerics
            // Date family
            var dateTypes = new[] { "date", "datetime", "timestamp", "time" };
            bool srcDate = dateTypes.Any(n => srcType.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0);
            bool dstDate = dateTypes.Any(n => dstType.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0);
            if (srcDate && dstDate) return 0;
            // String can receive almost anything
            var strTypes = new[] { "varchar", "nvarchar", "text", "string", "char" };
            bool dstStr = strTypes.Any(n => dstType.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0);
            if (dstStr) return 1; // warning — may truncate
            return 2; // error — incompatible
        }

        // ── Template methods ───────────────────────────────────────────────────

        private void LoadTemplateNames()
        {
            var names = ImportTemplateManager.ListAll();
            var items = new BindingList<SimpleItem>(
                names.Select(n => new SimpleItem { Text = n, Item = n }).ToList());
            cmbTemplateLoad.ListItems = items;
        }

        private void TemplateLoad_Changed(object? sender, SelectedItemChangedEventArgs e)
        {
            var name = cmbTemplateLoad.SelectedItem?.Text;
            if (string.IsNullOrWhiteSpace(name)) return;
            var dto = ImportTemplateManager.Load(name);
            if (dto == null) return;
            ImportTemplateManager.ApplyToRows(_rows.ToList(), dto);
            BindGrid();
            RaiseValidationState();
        }

        private void TemplateSave_Click(object? sender, EventArgs e)
        {
            var name = PromptForTemplateName();
            if (string.IsNullOrWhiteSpace(name)) return;

            ImportTemplateManager.Save(name, _rows.ToList());
            LoadTemplateNames();
            cmbTemplateLoad.SelectItemByText(name);
        }

        private void TemplateDelete_Click(object? sender, EventArgs e)
        {
            var name = cmbTemplateLoad.SelectedItem?.Text;
            if (string.IsNullOrWhiteSpace(name)) return;
            if (MessageBox.Show($"Delete template '{name}'?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            ImportTemplateManager.Delete(name);
            LoadTemplateNames();
        }

        private static string? PromptForTemplateName()
        {
            using var form = new Form
            {
                Text            = "Save Template",
                Size            = new System.Drawing.Size(360, 130),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition   = FormStartPosition.CenterParent,
                MinimizeBox     = false,
                MaximizeBox     = false
            };
            var lbl = new Label { Text = "Template name:", Location = new System.Drawing.Point(12, 12), AutoSize = true };
            var txt = new TextBox { Location = new System.Drawing.Point(12, 32), Width = 320, Text = "My Template" };
            var ok  = new Button  { Text = "OK",     DialogResult = DialogResult.OK,     Location = new System.Drawing.Point(175, 62), Width = 80 };
            var can = new Button  { Text = "Cancel", DialogResult = DialogResult.Cancel,  Location = new System.Drawing.Point(262, 62), Width = 80 };
            form.Controls.AddRange(new Control[] { lbl, txt, ok, can });
            form.AcceptButton = ok;
            form.CancelButton = can;
            return form.ShowDialog() == DialogResult.OK ? txt.Text.Trim() : null;
        }

        // Toolbar / select-all helpers
        private void SelectAll_Click(object? sender, EventArgs e)   { foreach (var r in _rows) r.Selected = true;  BindGrid(); RaiseValidationState(); }
        private void SelectNone_Click(object? sender, EventArgs e)  { foreach (var r in _rows) r.Selected = false; BindGrid(); RaiseValidationState(); }
        private void AutoMatch_Click(object? sender, EventArgs e)
        {
            if (_config == null) return;
            var dest = GetFields(_config.DestDataSourceName, _config.DestEntityName);
            foreach (var r in _rows)
            {
                var matched = dest.FirstOrDefault(d => string.Equals(d.FieldName, r.SourceField, StringComparison.OrdinalIgnoreCase));
                if (matched != null)
                {
                    r.DestinationField = matched.FieldName;
                    r.DestinationType  = matched.Fieldtype ?? string.Empty;
                    r.Selected         = true;
                }
            }
            BindGrid();
            RaiseValidationState();
        }
    }
}
