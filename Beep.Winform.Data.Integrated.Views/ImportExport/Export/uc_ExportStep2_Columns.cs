using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport.Models;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Export
{
    public partial class uc_ExportStep2_Columns : TemplateUserControl, IWizardStepContent
    {
        /// <summary>
        /// Rows fetched for the preview grid. The point of the preview is to show the shape of the
        /// data, which the first page does as well as the whole table.
        /// </summary>
        private const int PreviewPageSize = 100;

        private ExportConfiguration? _config;
        private List<ExportColumnRow> _rows = new();
        private bool _isComplete;

        /// <summary>
        /// Bumped on every OnStepEnter. A step can be re-entered by navigating back and forward
        /// faster than a load completes, and the later entry's grid must not be overwritten by the
        /// earlier entry's rows.
        /// </summary>
        private int _loadGeneration;

        /// <summary>
        /// Designer/parameterless ctor. Must not chain to the IServiceProvider overload with null —
        /// that resolves services off a null provider and throws.
        /// </summary>
        public uc_ExportStep2_Columns() => InitializeControl();

        public uc_ExportStep2_Columns(IServiceProvider services) : base(services) => InitializeControl();

        private void InitializeControl()
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

        // IWizardStepContent.OnStepEnter is void and has no async counterpart, so the loads continue
        // after it returns. Safe here: the wizard treats it as a notification, and the step gates
        // Next on ValidationStateChanged, which RefreshGrid raises once the rows have landed.
        public void OnStepEnter(WizardContext context) => _ = OnStepEnterAsync(context);

        private async Task OnStepEnterAsync(WizardContext context)
        {
            _config = context.GetValue<ExportConfiguration?>(WizardKeys.ExportConfig, null);
            if (_config == null) return;

            int generation = ++_loadGeneration;

            await LoadColumnsAsync(generation).ConfigureAwait(true);
            // IsDisposed matters here specifically: RefreshGrid touches the grid, and closing the
            // wizard mid-load would otherwise throw ObjectDisposedException into this unobserved task.
            if (generation != _loadGeneration || IsDisposed) return;

            var saved = context.GetValue<List<string>?>(WizardKeys.ExportSelectedCols, null);
            if (saved != null)
                _rows.ForEach(r => r.Selected = saved.Contains(r.FieldName));
            RefreshGrid();

            // After the columns: the grid is usable without the preview, so it must not wait on it.
            await LoadPreviewAsync(generation).ConfigureAwait(true);
        }

        public void OnStepLeave(WizardContext context)
        {
            // Nothing loaded means nothing to save. OnStepEnter is now async, so leaving before the
            // columns land (Back skips validation entirely) would otherwise persist an EMPTY
            // selection — and on the next entry that empty-but-non-null list is replayed over the
            // freshly loaded rows, deselecting every column and wedging the step on "select at least
            // one column". Leaving the previous selection in place is the honest no-op.
            if (_rows.Count == 0) return;

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

        /// <summary>
        /// Loads the column list. GetDataSource and GetEntityStructure are blocking round-trips.
        /// </summary>
        private async Task LoadColumnsAsync(int generation)
        {
            // Captured before the await: _config can be replaced by a re-entry while this is in
            // flight, and the catch below would then dereference a null field.
            var editor = Editor;
            var config = _config;
            if (editor == null || config == null) return;

            try
            {
                var structure = await Task.Run(() =>
                    editor.GetDataSource(config.SourceDataSourceName)
                          ?.GetEntityStructure(config.SourceEntityName, false)).ConfigureAwait(true);

                if (generation != _loadGeneration || IsDisposed) return;
                if (structure?.Fields == null) return;

                _rows = structure.Fields.Select(f => new ExportColumnRow
                {
                    FieldName = f.FieldName,
                    FieldType = f.Fieldtype,
                    Selected = true,
                }).ToList();
            }
            catch (Exception ex)
            {
                if (generation != _loadGeneration || IsDisposed) return;
                editor.AddLogMessage("ExportStep2",
                    $"Could not read the structure of '{config.SourceEntityName}': {ex.Message}",
                    DateTime.Now, 0, null, Errors.Failed);
            }
        }

        /// <summary>
        /// Loads the first page of source rows for the preview grid.
        /// </summary>
        /// <remarks>
        /// Uses IDataSource's paged GetEntity overload rather than the unbounded one this used to
        /// call: <c>GetEntity(entity, null)</c> pulled the ENTIRE table across the wire, on the UI
        /// thread, to populate a grid the user only glances at — so previewing a large table froze
        /// the wizard for as long as the read took. How much a provider saves depends on the
        /// provider: those that push paging down to the query fetch only this page, while some
        /// (e.g. the in-memory/cache datasources) still read everything and page in memory. Bounded
        /// rendering is the floor; a bounded fetch is the win where the provider supports it.
        /// </remarks>
        private async Task LoadPreviewAsync(int generation)
        {
            // Captured before the await — see LoadColumnsAsync.
            var editor = Editor;
            var config = _config;
            if (editor == null || config == null) return;

            try
            {
                var paged = await Task.Run(() =>
                    editor.GetDataSource(config.SourceDataSourceName)
                          ?.GetEntity(config.SourceEntityName, null, 1, PreviewPageSize)).ConfigureAwait(true);

                if (generation != _loadGeneration || IsDisposed) return;
                previewGrid.DataSource = paged?.Data;
            }
            catch (Exception ex)
            {
                if (generation != _loadGeneration || IsDisposed) return;
                // Preview is a convenience — a failure must not block column selection — but a
                // silent empty grid looks like "no data" rather than "the read failed".
                editor.AddLogMessage("ExportStep2",
                    $"Preview of '{config.SourceEntityName}' failed: {ex.Message}",
                    DateTime.Now, 0, null, Errors.Warning);
            }
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
