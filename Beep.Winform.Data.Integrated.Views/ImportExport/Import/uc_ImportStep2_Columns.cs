using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport.Models;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Import
{
    public partial class uc_ImportStep2_Columns : TemplateUserControl, IWizardStepContent
    {
        private DataImportConfiguration? _config;
        private List<ColRow> _rows = new();
        private bool _isComplete;

        public uc_ImportStep2_Columns(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            btnSelectAll.Click += (_, _) => SelectAll(true);
            btnSelectNone.Click += (_, _) => SelectAll(false);
            colSelectionGrid.CellValueChanged += OnCellValueChanged;
            colSelectionGrid.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (colSelectionGrid.CurrentCell is DataGridViewCheckBoxCell)
                    colSelectionGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
        }

        public bool IsComplete => _isComplete;
        public string NextButtonText => "Next";
        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;

        public void OnStepEnter(WizardContext context)
        {
            _config = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null);
            if (_config == null) return;
            LoadColumns();
            LoadPreview();
            var saved = context.GetValue<List<string>?>(WizardKeys.SelectedColumns, null);
            if (saved != null)
                _rows.ForEach(r => r.Selected = saved.Contains(r.FieldName));
            RefreshGrid();
        }

        public void OnStepLeave(WizardContext context)
        {
            SyncRowsFromGrid();
            var selected = _rows.Where(r => r.Selected).Select(r => r.FieldName).ToList();
            _config ??= new DataImportConfiguration();
            _config.SelectedFields = selected;
            context.SetValue(WizardKeys.SelectedColumns, selected);
            if (_config.Mapping == null && _config.SourceEntityStructure != null)
                context.SetValue(WizardKeys.ImportConfig, _config);
        }

        public WizardValidationResult Validate()
        {
            if (_rows.Count == 0)
                return WizardValidationResult.Error("No columns available to select.");
            if (!_rows.Any(r => r.Selected))
                return WizardValidationResult.Error("Please select at least one column.");
            return WizardValidationResult.Success();
        }

        public Task<WizardValidationResult> ValidateAsync() => Task.FromResult(Validate());

        private void OnCellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != 0 || e.RowIndex < 0 || e.RowIndex >= _rows.Count) return;
            var cellValue = colSelectionGrid.Rows[e.RowIndex].Cells[0].Value;
            _rows[e.RowIndex].Selected = cellValue is bool b && b;
            _isComplete = _rows.Any(r => r.Selected);
            ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(_isComplete));
        }

        private void SyncRowsFromGrid()
        {
            for (int i = 0; i < _rows.Count && i < colSelectionGrid.Rows.Count; i++)
            {
                var val = colSelectionGrid.Rows[i].Cells[0].Value;
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
            _config.SourceEntityStructure = structure;

            _rows = structure.Fields.Select(f => new ColRow
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
            try
            {
                var data = ds.GetEntity(_config.SourceEntityName, null);
                previewGrid.DataSource = data?.ToList();
            }
            catch { }
            lblPreview.Text = $"Preview: first rows of {_config.SourceEntityName}";
        }

        private void SelectAll(bool value)
        {
            _rows.ForEach(r => r.Selected = value);
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            colSelectionGrid.Rows.Clear();
            colSelectionGrid.Columns.Clear();
            var chkCol = new DataGridViewCheckBoxColumn { Name = "chk", HeaderText = "Select", TrueValue = true, FalseValue = false };
            colSelectionGrid.Columns.Add(chkCol);
            colSelectionGrid.Columns.Add("name", "Column");
            colSelectionGrid.Columns.Add("type", "Type");

            foreach (var row in _rows)
            {
                var idx = colSelectionGrid.Rows.Add();
                colSelectionGrid.Rows[idx].Cells["chk"].Value = row.Selected;
                colSelectionGrid.Rows[idx].Cells["name"].Value = row.FieldName;
                colSelectionGrid.Rows[idx].Cells["type"].Value = row.FieldType;
            }

            _isComplete = _rows.Any(r => r.Selected);
            ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(_isComplete));
        }

        private sealed class ColRow
        {
            public bool Selected { get; set; } = true;
            public string FieldName { get; set; } = string.Empty;
            public string FieldType { get; set; } = string.Empty;
        }
    }
}
