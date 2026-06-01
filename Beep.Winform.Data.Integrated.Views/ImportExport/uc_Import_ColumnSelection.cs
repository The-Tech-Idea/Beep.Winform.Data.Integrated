using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Editor.Importing;
using TheTechIdea.Beep.Editor.Importing.Interfaces;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
        public partial class uc_Import_ColumnSelection : TemplateUserControl, IWizardStepContent
    {
        private DataImportConfiguration? _config;

        // ── Column metadata loaded from entity structure ─────────────────────
        private sealed class ColRow
        {
            public bool   Selected  { get; set; } = true;
            public string Name      { get; set; } = string.Empty;
            public string FieldType { get; set; } = string.Empty;
            public string Sample    { get; set; } = string.Empty;
        }

        private readonly List<ColRow> _colRows = new();

        public uc_Import_ColumnSelection(IServiceProvider services) : base(services)
        {
            InitializeComponent();
        }

        // ── IWizardStepContent ───────────────────────────────────────────────

        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;
        public bool   IsComplete     => _colRows.Any(r => r.Selected);
        public string NextButtonText => string.Empty;

        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            base.OnNavigatedTo(parameters);
            RaiseValidationState();
        }

        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);
            btnSelectAll.Click += (_, _) => ToggleAll(true);
            btnSelectNone.Click += (_, _) => ToggleAll(false);
            btnRefreshPreview.Click += (_, _) => _ = LoadPreviewAsync();
            RaiseValidationState();
        }

        public void OnStepEnter(WizardContext context)
        {
            _config = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null);
            if (_config == null) return;

            LoadEntityColumns(context);

            // Restore previous selection if any
            var prevSelected = context.GetValue<List<string>?>(WizardKeys.SelectedColumns, null);
            if (prevSelected != null && prevSelected.Count > 0)
                ApplySavedSelection(prevSelected);

            _ = LoadPreviewAsync();
            RaiseValidationState();
        }

        public void OnStepLeave(WizardContext context)
        {
            // Commit checked columns back to config
            var selected = _colRows.Where(r => r.Selected).Select(r => r.Name).ToList();
            context.SetValue(WizardKeys.SelectedColumns, selected);

            if (_config != null)
                _config.SelectedFields = selected;
        }

        // ── Column grid population ───────────────────────────────────────────

        private void LoadEntityColumns(WizardContext context)
        {
            _colRows.Clear();
            colSelectionGrid.Rows.Clear();

            if (_config == null) return;

            // Prefer field info from the existing mapping if already set
            var mappingFields = _config.Mapping?.MappedEntities?
                .SelectMany(d => d.EntityFields ?? new List<TheTechIdea.Beep.DataBase.EntityField>())
                .ToList();

            // Fall back to fetching from data source directly
            if (mappingFields == null || mappingFields.Count == 0)
            {
                var srcDs     = _config.SourceDataSourceName;
                var srcEntity = _config.SourceEntityName;
                if (!string.IsNullOrWhiteSpace(srcDs) && !string.IsNullOrWhiteSpace(srcEntity))
                {
                    try
                    {
                        var ds = Editor?.GetDataSource(srcDs);
                        if (ds != null)
                        {
                            if (ds.ConnectionStatus != System.Data.ConnectionState.Open)
                                ds.Openconnection();
                            var entity = ds.GetEntityStructure(srcEntity, false);
                            mappingFields = entity?.Fields?.ToList();
                        }
                    }
                    catch { /* ignore — grid stays empty */ }
                }
            }

            if (mappingFields == null) return;

            foreach (var f in mappingFields.Where(f => !string.IsNullOrWhiteSpace(f.FieldName)))
            {
                _colRows.Add(new ColRow
                {
                    Selected  = true,
                    Name      = f.FieldName,
                    FieldType = f.Fieldtype ?? string.Empty,
                    Sample    = string.Empty
                });
            }

            RefreshSelectionGrid();
        }

        private void RefreshSelectionGrid()
        {
            colSelectionGrid.SuspendLayout();
            colSelectionGrid.Rows.Clear();
            foreach (var row in _colRows)
                colSelectionGrid.Rows.Add(row.Selected, row.Name, row.FieldType, row.Sample);
            colSelectionGrid.ResumeLayout();

            UpdateStatusLabel();
            RaiseValidationState();
        }

        private void ApplySavedSelection(List<string> selectedNames)
        {
            var set = new HashSet<string>(selectedNames, StringComparer.OrdinalIgnoreCase);
            foreach (var r in _colRows)
                r.Selected = set.Contains(r.Name);
            RefreshSelectionGrid();
        }

        // ── Preview grid ─────────────────────────────────────────────────────

        internal async Task LoadPreviewAsync()
        {
            if (_config == null) return;

            var srcDs     = _config.SourceDataSourceName;
            var srcEntity = _config.SourceEntityName;
            if (string.IsNullOrWhiteSpace(srcDs) || string.IsNullOrWhiteSpace(srcEntity)) return;

            lblPreviewStatus.Text = "Loading preview…";
            previewGrid.DataSource = null;

            try
            {
                var result = await Task.Run(() =>
                {
                    var ds = Editor?.GetDataSource(srcDs);
                    if (ds == null) return null;
                    if (ds.ConnectionStatus != ConnectionState.Open) ds.Openconnection();
                    // Retrieve first page (5 rows) to populate preview
                    var paged = ds.GetEntity(srcEntity, null, 1, 5);
                    return paged;
                });

                var listData = result?.Data as System.Collections.IList;
                if (listData != null && listData.Count > 0)
                {
                    BuildPreviewGrid(listData);
                    lblPreviewStatus.Text = $"Preview — first {listData.Count} row(s) of ~{result!.TotalRecords:N0}";
                    PopulateSampleValues(listData);
                    RefreshSelectionGrid();
                }
                else
                {
                    lblPreviewStatus.Text = "Preview unavailable";
                }
            }
            catch (Exception ex)
            {
                lblPreviewStatus.Text = $"Preview error: {ex.Message}";
            }
        }

        private void BuildPreviewGrid(global::System.Collections.IList rows)
        {
            if (rows.Count == 0) return;

            var dt = new DataTable();

            // Build DataTable from dynamic row objects
            if (rows[0] is System.Dynamic.ExpandoObject)
            {
                var first = (IDictionary<string, object?>)rows[0]!;
                foreach (var key in first.Keys)
                    dt.Columns.Add(key, typeof(string));

                foreach (IDictionary<string, object?> row in rows)
                    dt.Rows.Add(row.Values.Select(v => v?.ToString() ?? "").ToArray<object>());
            }
            else if (rows[0] is DataRow dr)
            {
                // Already a DataRow — grab its table schema
                var srcTable = dr.Table;
                foreach (DataColumn c in srcTable.Columns)
                    dt.Columns.Add(c.ColumnName, typeof(string));

                foreach (DataRow r in rows)
                    dt.Rows.Add(srcTable.Columns.Cast<DataColumn>()
                        .Select(c => r[c]?.ToString() ?? "").ToArray<object>());
            }

            previewGrid.DataSource = dt;
        }

        private void PopulateSampleValues(global::System.Collections.IList rows)
        {
            if (rows.Count == 0) return;

            // Try extract field sample from first row
            try
            {
                if (rows[0] is IDictionary<string, object?> expando)
                {
                    foreach (var colRow in _colRows)
                    {
                        if (expando.TryGetValue(colRow.Name, out var val))
                            colRow.Sample = val?.ToString() ?? string.Empty;
                    }
                }
                else if (rows[0] is DataRow dr)
                {
                    foreach (var colRow in _colRows)
                    {
                        if (dr.Table.Columns.Contains(colRow.Name))
                            colRow.Sample = dr[colRow.Name]?.ToString() ?? string.Empty;
                    }
                }
            }
            catch { /* ignore */ }
        }

        // ── Toolbar actions ──────────────────────────────────────────────────

        internal void ToggleAll(bool selected)
        {
            foreach (var r in _colRows)
                r.Selected = selected;
            RefreshSelectionGrid();
        }

        // ── Grid events ──────────────────────────────────────────────────────

        private void ColGrid_DirtyChanged(object? sender, EventArgs e)
        {
            if (colSelectionGrid.IsCurrentCellDirty)
                colSelectionGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void ColGrid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != 0 || e.RowIndex < 0 || e.RowIndex >= _colRows.Count) return;

            var cell = colSelectionGrid.Rows[e.RowIndex].Cells[0];
            _colRows[e.RowIndex].Selected = cell.Value is bool b && b;
            UpdateStatusLabel();
            RaiseValidationState();
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private void UpdateStatusLabel()
        {
            int selected = _colRows.Count(r => r.Selected);
            int total    = _colRows.Count;
            lblPreviewStatus2.Text = $"{selected} of {total} columns selected";
        }

        WizardValidationResult IWizardStepContent.Validate()
            => _colRows.Any(r => r.Selected)
                ? WizardValidationResult.Success()
                : WizardValidationResult.Error("Select at least one column.");

        public Task<WizardValidationResult> ValidateAsync()
            => Task.FromResult(((IWizardStepContent)this).Validate());

        private void RaiseValidationState()
        {
            ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(IsComplete));
        }
    }
}
