using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    public partial class uc_Export_ColumnSelection : TemplateUserControl, IWizardStepContent
    {
        private readonly ExportConfiguration _config;
        private BindingList<ExportColumnRow> _rows = new();
        private DataTable _previewData = new();

        public uc_Export_ColumnSelection(IServiceProvider services, ExportConfiguration config) : base(services)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            InitializeComponent();
            AttachGridEvents();
        }

        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;
        public bool IsComplete => _rows.Any(r => r.Selected);
        public string NextButtonText => string.Empty;

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
            var config = context.GetValue<ExportConfiguration?>(ExportWizardKeys.ExportConfig, null);
            if (config != null)
            {
                _config.SourceDataSourceName = config.SourceDataSourceName;
                _config.SourceEntityName = config.SourceEntityName;
            }
            _ = LoadColumnsAsync();
        }

        public void OnStepLeave(WizardContext context)
        {
            var selected = _rows.Where(r => r.Selected).Select(r => r.ColumnName).ToList();
            _config.SelectedFields = selected;
            context.SetValue(ExportWizardKeys.SelectedColumns, selected);
            context.SetValue(ExportWizardKeys.ExportConfig, _config);
        }

        WizardValidationResult IWizardStepContent.Validate() => ValidateStep();
        public Task<WizardValidationResult> ValidateAsync() => Task.FromResult(ValidateStep());

        private async Task LoadColumnsAsync()
        {
            _rows.Clear();
            previewGrid.DataSource = null;

            if (Editor == null || string.IsNullOrWhiteSpace(_config.SourceDataSourceName) ||
                string.IsNullOrWhiteSpace(_config.SourceEntityName))
            {
                RaiseValidationState();
                return;
            }

            try
            {
                var ds = Editor.GetDataSource(_config.SourceDataSourceName);
                if (ds == null) 
                { 
                    RaiseValidationState();
                    return; 
                }
                if (ds.ConnectionStatus != System.Data.ConnectionState.Open) ds.Openconnection();

                // Get entity structure for columns
                var structure = ds.GetEntityStructure(_config.SourceEntityName, false);
                if (structure?.Fields == null) 
                { 
                    RaiseValidationState();
                    return; 
                }

                // Load preview data (first 5 rows)
                var data = await Task.Run(() => ds.GetEntity(_config.SourceEntityName, _config.Filters));
                if (data == null)
                {
                    Editor?.AddLogMessage("ExportWizard", "No data returned from source.",
                        DateTime.Now, 0, null, ConfigUtil.Errors.Failed);
                    RaiseValidationState();
                    return;
                }
                _previewData = ConvertToDataTable(data, structure.Fields.Select(f => f.FieldName).ToList());

                // Build column rows
                foreach (var field in structure.Fields)
                {
                    var sampleValue = GetSampleValue(field.FieldName);
                    _rows.Add(new ExportColumnRow
                    {
                        Selected = true,
                        ColumnName = field.FieldName,
                        DataType = field.Fieldtype ?? "string",
                        SampleValue = sampleValue
                    });
                }

                BindGrid();
                BindPreview();
                RaiseValidationState();
            }
            catch (Exception ex)
            {
                Editor?.AddLogMessage("ExportWizard", $"Error loading columns: {ex.Message}",
                    DateTime.Now, 0, null, ConfigUtil.Errors.Failed);
                RaiseValidationState();
            }
        }

        private string GetSampleValue(string columnName)
        {
            if (_previewData == null || _previewData.Rows.Count == 0) return string.Empty;
            try
            {
                var value = _previewData.Rows[0][columnName];
                return value?.ToString() ?? string.Empty;
            }
            catch { return string.Empty; }
        }

        private DataTable ConvertToDataTable(object data, List<string> columnNames)
        {
            var table = new DataTable();
            foreach (var col in columnNames)
                table.Columns.Add(col, typeof(string));

            if (data is DataTable dt)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var newRow = table.NewRow();
                    foreach (var col in columnNames)
                        newRow[col] = row[col]?.ToString() ?? string.Empty;
                    table.Rows.Add(newRow);
                }
            }
            else if (data is IEnumerable<object> enumerable)
            {
                int count = 0;
                foreach (var item in enumerable)
                {
                    if (count++ >= 5) break;
                    var newRow = table.NewRow();
                    foreach (var col in columnNames)
                    {
                        var prop = item.GetType().GetProperty(col);
                        newRow[col] = prop?.GetValue(item)?.ToString() ?? string.Empty;
                    }
                    table.Rows.Add(newRow);
                }
            }
            return table;
        }

        private void BindGrid()
        {
            if (colGrid == null) return;
            colGrid.DataSource = null;
            colGrid.DataSource = _rows;
        }

        private void BindPreview()
        {
            if (previewGrid == null) return;
            previewGrid.DataSource = null;
            previewGrid.DataSource = _previewData;
        }

        private void AttachGridEvents()
        {
            colGrid.CellValueChanged += (_, e) =>
            {
                if (e.Cell?.ColumnIndex == 0) RaiseValidationState();
            };
        }

        private void RaiseValidationState()
        {
            var result = ValidateStep();
            ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(result.IsValid, result.ErrorMessage));
        }

        private WizardValidationResult ValidateStep()
        {
            if (!_rows.Any(r => r.Selected))
                return WizardValidationResult.Error("Select at least one column to export.");
            return WizardValidationResult.Success();
        }

        private void SelectAll_Click(object? sender, EventArgs e)
        {
            foreach (var r in _rows) r.Selected = true;
            BindGrid();
            RaiseValidationState();
        }

        private void SelectNone_Click(object? sender, EventArgs e)
        {
            foreach (var r in _rows) r.Selected = false;
            BindGrid();
            RaiseValidationState();
        }
    }

    public class ExportColumnRow
    {
        public bool Selected { get; set; } = true;
        public string ColumnName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string SampleValue { get; set; } = string.Empty;
    }
}
