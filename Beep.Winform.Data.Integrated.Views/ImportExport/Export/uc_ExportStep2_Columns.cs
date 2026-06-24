using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport.Models;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Export
{
    public partial class uc_ExportStep2_Columns : TemplateUserControl, IWizardStepContent
    {
        private ExportConfiguration? _config;
        private List<ExportColumnRow> _rows = new();
        private bool _isComplete;

        public uc_ExportStep2_Columns(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            btnSelectAll.Click += (_, _) => SelectAll(true);
            btnSelectNone.Click += (_, _) => SelectAll(false);
            colGrid.CellValueChanged += OnCellValueChanged;
            colGrid.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (colGrid.CurrentCell is DataGridViewCheckBoxCell)
                    colGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
        }

        public bool IsComplete => _isComplete;
        public string NextButtonText => "Next";
        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;

        public void OnStepEnter(WizardContext context)
        {
            _config = context.GetValue<ExportConfiguration?>(WizardKeys.ExportConfig, null);
            if (_config == null) return;
            LoadColumns();
            LoadPreview();
            var saved = context.GetValue<List<string>?>(WizardKeys.ExportSelectedCols, null);
            if (saved != null)
                _rows.ForEach(r => r.Selected = saved.Contains(r.FieldName));
            RefreshGrid();
        }

        public void OnStepLeave(WizardContext context)
        {
            SyncRowsFromGrid();
            _config ??= new ExportConfiguration();
            _config.SelectedFields = _rows.Where(r => r.Selected).Select(r => r.FieldName).ToList();
            context.SetValue(WizardKeys.ExportSelectedCols, _config.SelectedFields);
            context.SetValue(WizardKeys.ExportConfig, _config);
        }

        public WizardValidationResult Validate()
        {
            if (!_rows.Any(r => r.Selected))
                return WizardValidationResult.Error("Please select at least one column.");
            return WizardValidationResult.Success();
        }

        public Task<WizardValidationResult> ValidateAsync() => Task.FromResult(Validate());

        private void OnCellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != 0 || e.RowIndex < 0 || e.RowIndex >= _rows.Count) return;
            var cellValue = colGrid.Rows[e.RowIndex].Cells[0].Value;
            _rows[e.RowIndex].Selected = cellValue is bool b && b;
            _isComplete = _rows.Any(r => r.Selected);
            ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(_isComplete));
        }

        private void SyncRowsFromGrid()
        {
            for (int i = 0; i < _rows.Count && i < colGrid.Rows.Count; i++)
            {
                var val = colGrid.Rows[i].Cells[0].Value;
                _rows[i].Selected = val is bool b && b;
            }
        }

        private void LoadColumns()
        {
            if (Editor == null || _config == null) return;
            var ds = Editor.GetDataSource(_config.SourceDataSourceName);
            if (ds == null) return;
            var structure = ds.GetEntityStructure(_config.SourceEntityName, false);
            if (structure?.Fields == null) return;
            _rows = structure.Fields.Select(f => new ExportColumnRow
            {
                FieldName = f.FieldName,
                FieldType = f.Fieldtype,
                Selected = true,
            }).ToList();
        }

        private void LoadPreview()
        {
            if (Editor == null || _config == null) return;
            var ds = Editor.GetDataSource(_config.SourceDataSourceName);
            if (ds == null) return;
            try { previewGrid.DataSource = ds.GetEntity(_config.SourceEntityName, null)?.ToList(); }
            catch { }
        }

        private void SelectAll(bool value)
        {
            _rows.ForEach(r => r.Selected = value);
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            colGrid.Rows.Clear();
            colGrid.Columns.Clear();
            var chkCol = new DataGridViewCheckBoxColumn { Name = "chk", HeaderText = "Select", TrueValue = true, FalseValue = false };
            colGrid.Columns.Add(chkCol);
            colGrid.Columns.Add("name", "Column");
            colGrid.Columns.Add("type", "Type");

            foreach (var row in _rows)
            {
                int idx = colGrid.Rows.Add();
                colGrid.Rows[idx].Cells["chk"].Value = row.Selected;
                colGrid.Rows[idx].Cells["name"].Value = row.FieldName;
                colGrid.Rows[idx].Cells["type"].Value = row.FieldType;
            }

            _isComplete = _rows.Any(r => r.Selected);
            ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(_isComplete));
        }

        public sealed class ExportColumnRow
        {
            public bool Selected { get; set; } = true;
            public string FieldName { get; set; } = string.Empty;
            public string FieldType { get; set; } = string.Empty;
        }
    }
}
