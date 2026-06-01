using System.Data;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor.Importing;
using TheTechIdea.Beep.Editor.Importing.Interfaces;
using TheTechIdea.Beep.Editor.Mapping;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Winform.Controls.GridX;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    [AddinAttribute(Caption = "Data Edit", Name = "uc_DataEdit", ScopeCreateType = AddinScopeCreateType.Multiple, misc = "Config", menu = "Configuration", addinType = AddinType.Control, displayType = DisplayType.InControl, ObjectType = "Beep")]
    public partial class uc_DataEdit : TemplateUserControl
    {
        private sealed class ColumnLayoutProfile
        {
            public int Width { get; set; }
            public bool Visible { get; set; }
        }

        private sealed class EntityLayoutProfile
        {
            public string SchemaSignature { get; set; } = string.Empty;
            public Dictionary<string, ColumnLayoutProfile> Columns { get; } = new(StringComparer.OrdinalIgnoreCase);
        }

        private static readonly Dictionary<string, EntityLayoutProfile> LayoutProfiles = new(StringComparer.OrdinalIgnoreCase);
        private bool _eventsWired;
        private bool _isLoading;
        private bool _isSaving;
        private bool _isRollingBack;
        private bool _isImporting;
        private bool _isEditMode;
        private bool _hasPendingChanges;
        private string _currentEntityName = string.Empty;
        private string _currentDataSourceName = string.Empty;
        private string _importSourceEntityName = string.Empty;
        private string _importSourceDataSourceName = string.Empty;
        private string _lastError = string.Empty;
        private string _lastOperation = "None";
        private string _lastImportSummary = string.Empty;
        private DateTime? _lastImportCompletedAt;
        private DataImportManager? _activeImportManager;
        private CancellationTokenSource? _activeImportCancellation;
        private bool _isImportCancelRequested;

        public uc_DataEdit(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            Details.AddinName = "Data Edit";
            WireEvents();
            ConfigureGridDefaults();
            UpdateStatus("Ready");
            RefreshCommandStates();
        }

        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);
            WireEvents();
            ConfigureGridDefaults();
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            base.OnNavigatedTo(parameters);
            CaptureCurrentLayout();
            ResolveNavigationContext(parameters);
            _ = LoadDataFromUnitOfWorkAsync();
        }

        private void ResolveNavigationContext(Dictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return;
            }

            if (parameters.TryGetValue("CurrentEntity", out var entityObj))
            {
                _currentEntityName = entityObj?.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(_currentEntityName))
                {
                    Details.AddinName = _currentEntityName;
                    lblEntityName.Text = $"Entity: {_currentEntityName}";
                }
            }

            if (parameters.TryGetValue("CurrentDataSource", out var dsObj))
            {
                _currentDataSourceName = dsObj?.ToString() ?? string.Empty;
            }

            if (parameters.TryGetValue("ImportSourceEntity", out var sourceEntityObj))
            {
                _importSourceEntityName = sourceEntityObj?.ToString() ?? string.Empty;
            }

            if (parameters.TryGetValue("ImportSourceDataSource", out var sourceDataSourceObj))
            {
                _importSourceDataSourceName = sourceDataSourceObj?.ToString() ?? string.Empty;
            }

            ApplyRuntimeUowOptions(parameters);
            RefreshCommandStates();
        }

        private void WireEvents()
        {
            if (_eventsWired)
            {
                return;
            }

            _eventsWired = true;

            btnNew.Click += (_, _) => StartNewRecord();
            btnEdit.Click += (_, _) => EnterEditMode();
            btnDelete.Click += (_, _) => DeleteCurrentRecord();
            btnSave.Click += async (_, _) => await SaveChangesAsync();
            btnCancel.Click += async (_, _) => await CancelChangesAsync();
            btnRefresh.Click += async (_, _) => await LoadDataFromUnitOfWorkAsync();
            btnUndo.Click += (_, _) => UndoLastChange();
            btnRedo.Click += (_, _) => RedoLastChange();
            btnMap.Click += async (_, _) => await PrepareMappingAsync();
            btnImport.Click += async (_, _) => await PrepareImportAsync();

            beepGridPro1.SaveCalled += async (_, _) => await SaveChangesAsync();
            beepGridPro1.CellValueChanged += (_, _) => MarkDirty("Cell value changed");
            beepGridPro1.RowSelectionChanged += (_, _) =>
            {
                UpdateStatus($"Selection changed (row {beepGridPro1.CurrentRowIndex + 1})");
                RefreshCommandStates();
            };
            beepGridPro1.ColumnReordered += (_, _) => CaptureCurrentLayout();
        }

        private void ConfigureGridDefaults()
        {
            beepGridPro1.ShowNavigator = true;
            beepGridPro1.ShowTopFilterPanel = true;
            beepGridPro1.TopFilterPanelHeight = 34;
            beepGridPro1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            beepGridPro1.AutoSizeTriggerMode = AutoSizeTriggerMode.OnDataBind;
            beepGridPro1.SortIconVisibility = HeaderIconVisibility.Always;
            beepGridPro1.FilterIconVisibility = HeaderIconVisibility.Always;
            beepGridPro1.MultiSelect = false;
            beepGridPro1.SelectionMode = BeepGridSelectionMode.FullRowSelect;
            beepGridPro1.ReadOnly = false;
            ConfigureBaselineColumns();
        }

        private void ConfigureBaselineColumns()
        {
            beepGridPro1.Columns.Clear();
            beepGridPro1.Columns.Add(new BeepColumnConfig
            {
                ColumnName = "Sel",
                ColumnCaption = "Sel",
                IsSelectionCheckBox = true,
                CellEditor = BeepColumnType.CheckBoxBool,
                Width = 36,
                ReadOnly = false,
                AllowSort = false,
                ShowFilterIcon = false,
                ShowSortIcon = false
            });
            beepGridPro1.Columns.Add(new BeepColumnConfig
            {
                ColumnName = "RowNum",
                ColumnCaption = "#",
                IsRowNumColumn = true,
                Width = 56,
                ReadOnly = true,
                AllowSort = false,
                ShowFilterIcon = false
            });
            beepGridPro1.Columns.Add(new BeepColumnConfig
            {
                ColumnName = "RowID",
                ColumnCaption = "RowID",
                IsRowID = true,
                Width = 80,
                ReadOnly = true,
                Visible = false,
                ShowFilterIcon = false
            });
        }

        private async Task LoadDataFromUnitOfWorkAsync()
        {
            if (uow == null)
            {
                UpdateStatus("No UnitOfWork is attached.");
                return;
            }

            _isLoading = true;
            try
            {
                if (!EnsureDataSourceReady())
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(_currentDataSourceName))
                {
                    _currentDataSourceName = UowDatasourceName;
                }

                if (string.IsNullOrWhiteSpace(_currentEntityName))
                {
                    _currentEntityName = UowEntityName;
                }

                if (!string.IsNullOrWhiteSpace(_currentEntityName))
                {
                    lblEntityName.Text = $"Entity: {_currentEntityName}";
                }

                await UowGetAsync();
                beepGridPro1.Uow = uow;
                beepGridPro1.DataSource = UowUnits;
                ApplyEntityColumnPolicies();
                RestoreCurrentLayout();
                ApplyDefaultSort();
                _isEditMode = false;
                _hasPendingChanges = false;
                _lastError = string.Empty;
                _lastOperation = "Load";
                UpdateStatus("Data loaded");
                Log("Data loaded successfully.");
            }
            catch (Exception ex)
            {
                SetLastError($"Load failed: {ex.Message}");
                Log(_lastError, Errors.Failed);
            }
            finally
            {
                _isLoading = false;
                RefreshCommandStates();
            }
        }

        private void ApplyEntityColumnPolicies()
        {
            foreach (var column in beepGridPro1.Columns)
            {
                if (column == null || string.IsNullOrWhiteSpace(column.ColumnName))
                {
                    continue;
                }

                var columnName = column.ColumnName;
                if (string.Equals(columnName, "Sel", StringComparison.OrdinalIgnoreCase))
                {
                    column.Width = 36;
                    column.AllowSort = false;
                    column.ShowFilterIcon = false;
                    column.ShowSortIcon = false;
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                    continue;
                }

                if (string.Equals(columnName, "RowNum", StringComparison.OrdinalIgnoreCase))
                {
                    column.Width = 56;
                    column.ReadOnly = true;
                    column.AllowSort = false;
                    column.ShowFilterIcon = false;
                    column.ShowSortIcon = false;
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                    column.CellEditor = BeepColumnType.NumericUpDown;
                    continue;
                }

                if (string.Equals(columnName, "RowID", StringComparison.OrdinalIgnoreCase))
                {
                    column.Width = 80;
                    column.ReadOnly = true;
                    column.Visible = false;
                    column.ShowFilterIcon = false;
                    column.ShowSortIcon = false;
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                    column.CellEditor = BeepColumnType.NumericUpDown;
                    continue;
                }

                column.Width = ResolveDefaultWidth(column);
                column.AllowSort = !IsTechnicalOrComputed(column);
                column.ShowFilterIcon = !IsTechnicalOrComputed(column);
                column.ShowSortIcon = true;
                column.ReadOnly = ResolveReadOnly(column);
                column.SortMode = column.AllowSort ? DataGridViewColumnSortMode.Automatic : DataGridViewColumnSortMode.NotSortable;
                column.CellEditor = ResolveEditorType(column);
                if (column.IsForeignKey && !HasLookupSource(column))
                {
                    // No valid lookup source configured: avoid unsafe free-text edits.
                    column.ReadOnly = true;
                    column.CellEditor = BeepColumnType.Text;
                }
            }
        }

        private void ApplyDefaultSort()
        {
            var hasExistingSort = beepGridPro1.Columns.Any(c => c?.IsSorted == true);
            if (hasExistingSort)
            {
                return;
            }

            var sortColumnIndex = -1;
            for (var index = 0; index < beepGridPro1.Columns.Count; index++)
            {
                var column = beepGridPro1.Columns[index];
                if (column == null || !column.AllowSort)
                {
                    continue;
                }

                if (column.IsPrimaryKey)
                {
                    sortColumnIndex = index;
                    break;
                }

                if (sortColumnIndex < 0 && string.Equals(column.ColumnName, "RowNum", StringComparison.OrdinalIgnoreCase))
                {
                    sortColumnIndex = index;
                }
            }

            if (sortColumnIndex >= 0)
            {
                beepGridPro1.ToggleColumnSort(sortColumnIndex);
            }
        }

        private static int ResolveDefaultWidth(BeepColumnConfig column)
        {
            if (column == null)
            {
                return 180;
            }

            if (column.IsPrimaryKey)
            {
                return 140;
            }

            if (column.IsForeignKey)
            {
                return 160;
            }

            if (column.ColumnType == DbFieldCategory.Boolean)
            {
                return 90;
            }

            if (column.ColumnType == DbFieldCategory.Numeric)
            {
                return 120;
            }

            if (column.ColumnType == DbFieldCategory.Date || column.ColumnType == DbFieldCategory.DateTime)
            {
                return 140;
            }

            if (IsEnumOrStatusColumn(column))
            {
                return 130;
            }

            if (IsLargeTextColumn(column))
            {
                return 260;
            }

            return 180;
        }

        private static bool ResolveReadOnly(BeepColumnConfig column)
        {
            if (column == null)
            {
                return true;
            }

            if (column.IsRowID || column.IsRowNumColumn)
            {
                return true;
            }

            if (column.IsPrimaryKey && !column.IsAutoIncrement)
            {
                return false;
            }

            if (column.IsAutoIncrement)
            {
                return true;
            }

            if (IsComputedColumn(column))
            {
                return true;
            }

            return column.ReadOnly;
        }

        private static BeepColumnType ResolveEditorType(BeepColumnConfig column)
        {
            if (column == null || column.ReadOnly || IsComputedColumn(column))
            {
                return BeepColumnType.Text;
            }

            if (column.IsForeignKey)
            {
                return BeepColumnType.ListOfValue;
            }

            if (column.ColumnType == DbFieldCategory.Boolean)
            {
                return BeepColumnType.CheckBoxBool;
            }

            if (column.ColumnType == DbFieldCategory.Date || column.ColumnType == DbFieldCategory.DateTime)
            {
                return BeepColumnType.DateTime;
            }

            if (column.ColumnType == DbFieldCategory.Numeric)
            {
                return BeepColumnType.NumericUpDown;
            }

            if (IsEnumOrStatusColumn(column))
            {
                return BeepColumnType.ComboBox;
            }

            if (IsLargeTextColumn(column))
            {
                return BeepColumnType.Text;
            }

            return BeepColumnType.Text;
        }

        private static bool IsTechnicalOrComputed(BeepColumnConfig column)
        {
            if (column == null)
            {
                return true;
            }

            return column.IsSelectionCheckBox || column.IsRowID || column.IsRowNumColumn || IsComputedColumn(column);
        }

        private static bool IsComputedColumn(BeepColumnConfig column)
        {
            if (column == null || string.IsNullOrWhiteSpace(column.ColumnName))
            {
                return false;
            }

            var name = column.ColumnName;
            return name.Contains("computed", StringComparison.OrdinalIgnoreCase) ||
                   name.Contains("calculated", StringComparison.OrdinalIgnoreCase) ||
                   name.Contains("formula", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsEnumOrStatusColumn(BeepColumnConfig column)
        {
            if (column == null || string.IsNullOrWhiteSpace(column.ColumnName))
            {
                return false;
            }

            var name = column.ColumnName;
            return name.Contains("status", StringComparison.OrdinalIgnoreCase) ||
                   name.Contains("state", StringComparison.OrdinalIgnoreCase) ||
                   name.Contains("type", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsLargeTextColumn(BeepColumnConfig column)
        {
            if (column == null || string.IsNullOrWhiteSpace(column.ColumnName))
            {
                return false;
            }

            var name = column.ColumnName;
            return name.Contains("description", StringComparison.OrdinalIgnoreCase) ||
                   name.Contains("notes", StringComparison.OrdinalIgnoreCase) ||
                   name.Contains("comment", StringComparison.OrdinalIgnoreCase);
        }

        private void StartNewRecord()
        {
            if (uow == null)
            {
                UpdateStatus("Cannot create record: UnitOfWork is not available.");
                return;
            }

            try
            {
                Log("New operation started.");
                beepGridPro1.InsertNew();
                _isEditMode = true;
                _lastOperation = "New";
                MarkDirty("New record started.");
                Log("New operation completed.");
            }
            catch (Exception ex)
            {
                SetLastError($"New record failed: {ex.Message}");
                Log(_lastError, Errors.Failed);
            }
        }

        private void EnterEditMode()
        {
            if (uow == null)
            {
                UpdateStatus("Edit skipped: UnitOfWork is not available.");
                return;
            }

            if (!HasRowSelection())
            {
                UpdateStatus("Edit requires a selected row.");
                return;
            }

            _isEditMode = true;
            beepGridPro1.ReadOnly = false;
            _lastOperation = "Edit";
            UpdateStatus("Edit mode enabled.");
        }

        private void DeleteCurrentRecord()
        {
            if (uow == null)
            {
                UpdateStatus("Cannot delete: UnitOfWork is not available.");
                return;
            }

            if (!HasRowSelection())
            {
                UpdateStatus("Delete requires a selected row.");
                return;
            }

            var confirm = MessageBox.Show("Delete selected record?", "Delete Record", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                Log("Delete operation started.");
                beepGridPro1.DeleteCurrent();
                _isEditMode = true;
                _lastOperation = "Delete";
                MarkDirty("Record marked for delete.");
                Log("Delete operation completed.");
            }
            catch (Exception ex)
            {
                SetLastError($"Delete failed: {ex.Message}");
                Log(_lastError, Errors.Failed);
            }
        }

        private async Task SaveChangesAsync()
        {
            if (uow == null)
            {
                UpdateStatus("Save skipped: UnitOfWork is not available.");
                return;
            }

            if (_isImporting)
            {
                UpdateStatus("Save blocked while import is running.");
                return;
            }

            try
            {
                if (_isSaving)
                {
                    UpdateStatus("Save is already running.");
                    return;
                }

                _isSaving = true;
                Log("Save operation started.");
                if (!_hasPendingChanges && UowIsDirty == false)
                {
                    UpdateStatus("No pending changes.");
                    return;
                }

                if (!EnsureDataSourceReady())
                {
                    return;
                }

                if (UowIsAutoValidateEnabled)
                {
                    var validation = UowValidateAll();
                    if (validation != null && !validation.IsValid && UowBlockCommitOnValidationError)
                    {
                        SetLastError($"Validation failed ({validation.Errors?.Count ?? 0} errors).");
                        Log("Save blocked by validation.", Errors.Failed);
                        return;
                    }
                }

                var result = await UowCommitAsync();
                if (result == null || result.Flag != Errors.Ok)
                {
                    SetLastError($"Save failed: {result?.Message ?? "Unknown error"}");
                    Log(_lastError, Errors.Failed);
                    return;
                }

                _isEditMode = false;
                _hasPendingChanges = false;
                _lastError = string.Empty;
                _lastOperation = "Save";
                UpdateStatus("Save completed.");
                Log("Save completed.");
                RefreshEntityStructure();
                await LoadDataFromUnitOfWorkAsync();
            }
            catch (Exception ex)
            {
                SetLastError($"Save failed: {ex.Message}");
                Log(_lastError, Errors.Failed);
            }
            finally
            {
                _isSaving = false;
                RefreshCommandStates();
            }
        }

        private async Task CancelChangesAsync()
        {
            if (uow == null)
            {
                UpdateStatus("Cancel skipped: UnitOfWork is not available.");
                return;
            }

            if (_isImporting)
            {
                if (_isImportCancelRequested)
                {
                    UpdateStatus("Import cancellation already requested. Waiting for shutdown.");
                    return;
                }

                var stopImport = MessageBox.Show("Import is running. Cancel import now?", "Cancel Import", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (stopImport == DialogResult.Yes)
                {
                    try
                    {
                        _isImportCancelRequested = true;
                        _activeImportManager?.CancelImport();
                        _activeImportCancellation?.Cancel();
                        UpdateStatus("Import cancellation requested.");
                        Log("Import cancellation requested.");
                    }
                    catch (Exception ex)
                    {
                        SetLastError($"Import cancel failed: {ex.Message}");
                        Log(_lastError, Errors.Failed);
                    }
                }
                else
                {
                    UpdateStatus("Import cancel was not requested.");
                }
                return;
            }

            var confirm = MessageBox.Show("Rollback pending changes?", "Cancel Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (_isRollingBack)
                {
                    UpdateStatus("Rollback is already running.");
                    return;
                }

                _isRollingBack = true;
                Log("Rollback operation started.");
                await UowRollbackAsync();
                beepGridPro1.Cancel();
                _isEditMode = false;
                _hasPendingChanges = false;
                _lastError = string.Empty;
                _lastOperation = "Rollback";
                UpdateStatus("Changes rolled back.");
                Log("Rollback completed.");
                await LoadDataFromUnitOfWorkAsync();
            }
            catch (Exception ex)
            {
                SetLastError($"Rollback failed: {ex.Message}");
                Log(_lastError, Errors.Failed);
            }
            finally
            {
                _isRollingBack = false;
                RefreshCommandStates();
            }
        }

        private void UndoLastChange()
        {
            if (uow == null || !UowIsUndoEnabled || !UowCanUndo)
            {
                UpdateStatus("Undo unavailable.");
                return;
            }

            var success = UowUndo();
            UpdateStatus(success ? "Undo applied." : "Undo failed.");
        }

        private void RedoLastChange()
        {
            if (uow == null || !UowIsUndoEnabled || !UowCanRedo)
            {
                UpdateStatus("Redo unavailable.");
                return;
            }

            var success = UowRedo();
            UpdateStatus(success ? "Redo applied." : "Redo failed.");
        }

        private async Task PrepareMappingAsync()
        {
            if (Editor == null || string.IsNullOrWhiteSpace(_currentEntityName) || string.IsNullOrWhiteSpace(_currentDataSourceName))
            {
                UpdateStatus("Mapping prep requires entity and datasource context.");
                return;
            }

            try
            {
                if (!await CanExecuteExternalOperationAsync("Mapping"))
                {
                    return;
                }

                Log("Mapping preparation started.");
                var dataSource = Editor.GetDataSource(_currentDataSourceName);
                if (dataSource == null)
                {
                    SetLastError($"Mapping preflight failed: datasource '{_currentDataSourceName}' was not found.");
                    Log(_lastError, Errors.Failed);
                    return;
                }

                if (dataSource.ConnectionStatus != ConnectionState.Open)
                {
                    dataSource.Openconnection();
                }

                if (!EnsureEntityContextAvailable(dataSource, "Mapping"))
                {
                    return;
                }

                var result = MappingManager.CreateEntityMap(Editor, _currentEntityName, _currentDataSourceName);
                if (result.Item1.Flag == Errors.Ok)
                {
                    UpdateStatus("Mapping prepared and saved.");
                    _lastOperation = "Mapping";
                    Log("Mapping prepared.");
                    await LoadDataFromUnitOfWorkAsync();
                }
                else
                {
                    SetLastError($"Mapping failed: {result.Item1.Message}");
                    Log(_lastError, Errors.Failed);
                }
            }
            catch (Exception ex)
            {
                SetLastError($"Mapping error: {ex.Message}");
                Log(_lastError, Errors.Failed);
            }
        }

        private async Task PrepareImportAsync()
        {
            if (Editor == null || string.IsNullOrWhiteSpace(_currentEntityName) || string.IsNullOrWhiteSpace(_currentDataSourceName))
            {
                UpdateStatus("Import prep requires entity and datasource context.");
                return;
            }

            try
            {
                if (!await CanExecuteExternalOperationAsync("Import"))
                {
                    return;
                }

                if (_isImporting)
                {
                    UpdateStatus("Import is already running.");
                    return;
                }

                _isImporting = true;
                _isImportCancelRequested = false;
                _lastOperation = "Import";
                UpdateStatus("Import preflight started.");
                Log("Import preparation started.");
                var dataSource = Editor.GetDataSource(_currentDataSourceName);
                if (dataSource == null)
                {
                    SetLastError($"Import preflight failed: datasource '{_currentDataSourceName}' was not found.");
                    Log(_lastError, Errors.Failed);
                    return;
                }

                if (dataSource.ConnectionStatus != ConnectionState.Open)
                {
                    dataSource.Openconnection();
                }

                if (!EnsureEntityContextAvailable(dataSource, "Import"))
                {
                    return;
                }

                var sourceEntity = string.IsNullOrWhiteSpace(_importSourceEntityName) ? _currentEntityName : _importSourceEntityName;
                var sourceDataSource = string.IsNullOrWhiteSpace(_importSourceDataSourceName) ? _currentDataSourceName : _importSourceDataSourceName;
                if (string.Equals(sourceEntity, _currentEntityName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(sourceDataSource, _currentDataSourceName, StringComparison.OrdinalIgnoreCase))
                {
                    SetLastError("Import preflight failed: source and destination are the same. Provide ImportSourceEntity/ImportSourceDataSource navigation parameters.");
                    Log(_lastError, Errors.Failed);
                    return;
                }

                var manager = new DataImportManager(Editor);
                _activeImportManager = manager;
                var config = manager.CreateImportConfiguration(
                    sourceEntity,
                    sourceDataSource,
                    _currentEntityName,
                    _currentDataSourceName);

                config.BatchSize = 100;
                config.ApplyDefaults = true;
                var preflight = await manager.TestImportConfigurationAsync(config);
                if (preflight == null || preflight.Flag == Errors.Failed)
                {
                    SetLastError($"Import preflight failed: {preflight?.Message ?? "Unknown error"}");
                    Log(_lastError, Errors.Failed);
                    return;
                }

                var confirm = MessageBox.Show(
                    $"Execute import from '{sourceDataSource}.{sourceEntity}' into '{_currentDataSourceName}.{_currentEntityName}'?",
                    "Run Import",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes)
                {
                    UpdateStatus("Import canceled.");
                    return;
                }

                UpdateStatus($"Import running: {_currentDataSourceName}.{_currentEntityName}");
                var progress = new Progress<IPassedArgs>(args => ReportImportProgress(args, sourceDataSource, sourceEntity));
                _activeImportCancellation = new CancellationTokenSource();
                var result = await manager.RunImportAsync(config, progress, _activeImportCancellation.Token);
                var importSummary = BuildImportSummary(manager);
                _lastImportSummary = importSummary;
                _lastImportCompletedAt = DateTime.Now;
                if (result == null || result.Flag == Errors.Failed)
                {
                    if (IsImportCancelled(manager, result))
                    {
                        _lastOperation = "ImportCancelled";
                        _lastError = string.Empty;
                        UpdateStatus($"Import cancelled. {importSummary}");
                        Log($"Import cancelled. {importSummary}", Errors.Warning);
                        return;
                    }

                    SetLastError($"Import failed: {result?.Message ?? "Unknown error"} ({importSummary})");
                    Log(_lastError, Errors.Failed);
                    return;
                }

                if (result.Flag == Errors.Warning)
                {
                    _lastOperation = "ImportWarning";
                    _lastError = string.Empty;
                    UpdateStatus($"Import completed with warnings. {importSummary}");
                    Log($"Import completed with warnings. {importSummary}", Errors.Warning);
                    await LoadDataFromUnitOfWorkAsync();
                    return;
                }

                _lastOperation = "Import";
                _lastError = string.Empty;
                UpdateStatus($"Import completed and data refreshed. {importSummary}");
                Log($"Import completed. {importSummary}");
                await LoadDataFromUnitOfWorkAsync();
            }
            catch (OperationCanceledException)
            {
                var importSummary = BuildImportSummary(_activeImportManager);
                _lastImportSummary = importSummary;
                _lastImportCompletedAt = DateTime.Now;
                _lastOperation = "ImportCancelled";
                _lastError = string.Empty;
                UpdateStatus($"Import cancelled. {importSummary}");
                Log($"Import cancelled. {importSummary}", Errors.Warning);
            }
            catch (Exception ex)
            {
                SetLastError($"Import prep failed: {ex.Message}");
                Log(_lastError, Errors.Failed);
            }
            finally
            {
                _isImporting = false;
                _isImportCancelRequested = false;
                _activeImportCancellation?.Dispose();
                _activeImportCancellation = null;
                _activeImportManager?.Dispose();
                _activeImportManager = null;
                RefreshCommandStates();
            }
        }

        private void MarkDirty(string message)
        {
            if (_isLoading)
            {
                return;
            }

            _isEditMode = true;
            _hasPendingChanges = true;
            UpdateStatus(message);
            RefreshCommandStates();
        }

        private bool EnsureDataSourceReady()
        {
            if (uow?.DataSource == null)
            {
                UpdateStatus("No datasource attached to UnitOfWork.");
                return false;
            }

            try
            {
                if (UowDataSource.ConnectionStatus != ConnectionState.Open)
                {
                    var state = UowDataSource.Openconnection();
                    if (state != ConnectionState.Open && UowDataSource.ConnectionStatus != ConnectionState.Open)
                    {
                        SetLastError("Datasource is not open.");
                        Log("Datasource open precheck failed.", Errors.Failed);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                SetLastError($"Datasource precheck failed: {ex.Message}");
                Log(_lastError, Errors.Failed);
                return false;
            }
        }

        private bool EnsureEntityContextAvailable(IDataSource dataSource, string operationName)
        {
            try
            {
                var entityStructure = dataSource.GetEntityStructure(_currentEntityName, true);
                if (entityStructure == null)
                {
                    SetLastError($"{operationName} preflight failed: entity '{_currentEntityName}' not found in datasource '{_currentDataSourceName}'.");
                    Log(_lastError, Errors.Failed);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                SetLastError($"{operationName} preflight failed while validating entity context: {ex.Message}");
                Log(_lastError, Errors.Failed);
                return false;
            }
        }

        private void ApplyRuntimeUowOptions(Dictionary<string, object> parameters)
        {
            if (uow == null || parameters == null || parameters.Count == 0)
            {
                return;
            }

            TrySetBoolParameter(parameters, "UowAutoValidate", value => uow.IsAutoValidateEnabled = value);
            TrySetBoolParameter(parameters, "UowBlockCommitOnValidationError", value => uow.BlockCommitOnValidationError = value);
            TrySetBoolParameter(parameters, "UowUndoEnabled", value => uow.IsUndoEnabled = value);
            TrySetIntParameter(parameters, "UowMaxUndoDepth", value => uow.MaxUndoDepth = value);

            if (parameters.TryGetValue("UowCommitOrder", out var commitOrderObj))
            {
                TrySetUowCommitOrder(commitOrderObj);
            }
        }

        private static void TrySetBoolParameter(Dictionary<string, object> parameters, string key, Action<bool> apply)
        {
            if (parameters.TryGetValue(key, out var valueObj) && valueObj != null && bool.TryParse(valueObj.ToString(), out var parsed))
            {
                apply(parsed);
            }
        }

        private static void TrySetIntParameter(Dictionary<string, object> parameters, string key, Action<int> apply)
        {
            if (parameters.TryGetValue(key, out var valueObj) && valueObj != null && int.TryParse(valueObj.ToString(), out var parsed))
            {
                apply(parsed);
            }
        }

        private void TrySetUowCommitOrder(object commitOrderObj)
        {
            if (uow == null || commitOrderObj == null)
            {
                return;
            }

            try
            {
                var prop = uow.GetType().GetProperty("CommitOrder");
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(uow, commitOrderObj);
                    Log("Applied UOW CommitOrder runtime option.");
                }
            }
            catch (Exception ex)
            {
                Log($"Unable to apply CommitOrder runtime option: {ex.Message}", Errors.Failed);
            }
        }

        private void ReportImportProgress(IPassedArgs args, string sourceDataSource, string sourceEntity)
        {
            var progress = args?.Progress ?? 0;
            if (progress < 0) progress = 0;
            if (progress > 100) progress = 100;

            var stepMessage = args?.Messege;
            if (string.IsNullOrWhiteSpace(stepMessage))
            {
                stepMessage = $"Importing from {sourceDataSource}.{sourceEntity}";
            }

            RunOnUiThread(() =>
            {
                UpdateStatus($"Import {progress}% - {stepMessage}");
            });
        }

        private static string BuildImportSummary(DataImportManager? manager)
        {
            if (manager == null)
            {
                return "Processed:0/0, Errors:Unknown";
            }

            var status = manager.GetImportStatus();
            if (status == null)
            {
                return "Processed:0/0, Errors:Unknown";
            }

            var processed = status.RecordsProcessed;
            var total = status.TotalRecords;
            var hasErrors = status.HasErrors ? "Yes" : "No";
            var duration = ResolveImportDuration(status);
            return $"Processed:{processed}/{total}, Errors:{hasErrors}, Quarantined:{status.RecordsQuarantined}, Blocked:{status.RecordsBlocked}, Duration:{duration}";
        }

        private static string ResolveImportDuration(ImportStatus status)
        {
            if (status == null || !status.StartedAt.HasValue)
            {
                return "--:--";
            }

            var end = status.FinishedAt ?? DateTime.UtcNow;
            var span = end - status.StartedAt.Value;
            if (span < TimeSpan.Zero)
            {
                span = TimeSpan.Zero;
            }

            return span.TotalHours >= 1
                ? $"{(int)span.TotalHours:00}:{span.Minutes:00}:{span.Seconds:00}"
                : $"{span.Minutes:00}:{span.Seconds:00}";
        }

        private bool IsImportCancelled(DataImportManager manager, IErrorsInfo result)
        {
            if (_activeImportCancellation?.IsCancellationRequested == true)
            {
                return true;
            }

            var message = result?.Message ?? string.Empty;
            if (message.Contains("cancel", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var status = manager?.GetImportStatus();
            if (status == null)
            {
                return false;
            }

            if (status.IsCancelled)
            {
                return true;
            }

            return status.LastMessage?.Contains("cancel", StringComparison.OrdinalIgnoreCase) == true;
        }

        private void RunOnUiThread(Action action)
        {
            if (action == null || IsDisposed || Disposing)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(action);
                return;
            }

            action();
        }

        private void RefreshEntityStructure()
        {
            if (UowDataSource == null || string.IsNullOrWhiteSpace(_currentEntityName))
            {
                return;
            }

            try
            {
                _ = UowDataSource.GetEntityStructure(_currentEntityName, true);
            }
            catch (Exception ex)
            {
                Log($"Entity structure refresh failed: {ex.Message}", Errors.Failed);
            }
        }

        private void CaptureCurrentLayout()
        {
            if (string.IsNullOrWhiteSpace(_currentEntityName) || beepGridPro1.Columns.Count == 0)
            {
                return;
            }

            var profile = new EntityLayoutProfile
            {
                SchemaSignature = BuildCurrentSchemaSignature()
            };
            foreach (var column in beepGridPro1.Columns)
            {
                if (column == null || string.IsNullOrWhiteSpace(column.ColumnName))
                {
                    continue;
                }

                profile.Columns[column.ColumnName] = new ColumnLayoutProfile
                {
                    Width = column.Width,
                    Visible = column.Visible
                };
            }

            LayoutProfiles[_currentEntityName] = profile;
        }

        private void RestoreCurrentLayout()
        {
            if (string.IsNullOrWhiteSpace(_currentEntityName))
            {
                return;
            }

            if (!LayoutProfiles.TryGetValue(_currentEntityName, out var profile) || profile == null)
            {
                return;
            }

            var currentSignature = BuildCurrentSchemaSignature();
            if (!string.Equals(currentSignature, profile.SchemaSignature, StringComparison.Ordinal))
            {
                // Schema drift detected; keep baseline policy and avoid applying stale layout.
                UpdateStatus("Schema changed. Layout profile reset to defaults.");
                return;
            }

            foreach (var column in beepGridPro1.Columns)
            {
                if (column == null || string.IsNullOrWhiteSpace(column.ColumnName))
                {
                    continue;
                }

                if (!profile.Columns.TryGetValue(column.ColumnName, out var saved))
                {
                    continue;
                }

                column.Width = saved.Width;
                column.Visible = saved.Visible;
            }
        }

        private string BuildCurrentSchemaSignature()
        {
            try
            {
                var parts = new List<string>(beepGridPro1.Columns.Count);
                foreach (var column in beepGridPro1.Columns)
                {
                    if (column == null || string.IsNullOrWhiteSpace(column.ColumnName))
                    {
                        continue;
                    }

                    var descriptor = string.Join("|",
                        column.ColumnName,
                        column.ColumnType.ToString(),
                        column.IsPrimaryKey ? "PK" : "NPK",
                        column.IsForeignKey ? "FK" : "NFK",
                        column.IsAutoIncrement ? "AI" : "NAI",
                        column.IsRowID ? "RID" : "NRID",
                        column.IsRowNumColumn ? "RNUM" : "NRNUM");
                    parts.Add(descriptor);
                }

                return string.Join(";", parts.OrderBy(s => s, StringComparer.OrdinalIgnoreCase));
            }
            catch
            {
                return string.Empty;
            }
        }

        private void UpdateStatus(string message)
        {
            var dirtyState = UowIsDirty ? "Dirty" : "Clean";
            var selected = 0;
            var total = GetUnitCount();
            var validationSummary = GetValidationSummary();
            try
            {
                selected = beepGridPro1.SelectedRows?.Count ?? 0;
            }
            catch
            {
                selected = 0;
            }

            var lastErrTag = string.IsNullOrWhiteSpace(_lastError) ? string.Empty : " | LastError";
            var lastImportTag = GetLastImportTag();
            var cancelReqTag = _isImportCancelRequested ? " | CancelRequested" : string.Empty;
            var busy = _isLoading || _isSaving || _isRollingBack || _isImporting ? "Busy" : "Idle";
            lblState.Text = $"State: {dirtyState} | Pending: {(_hasPendingChanges ? "Yes" : "No")} | Mode: {(_isEditMode ? "Edit" : "Browse")} | Op: {_lastOperation} | Run:{busy} | Sel: {selected}/{total} | {validationSummary}{lastErrTag}{lastImportTag}{cancelReqTag} | {message}";
            RefreshCommandStates();
        }

        private string GetLastImportTag()
        {
            if (string.IsNullOrWhiteSpace(_lastImportSummary))
            {
                return string.Empty;
            }

            var at = _lastImportCompletedAt?.ToString("HH:mm:ss") ?? "--:--:--";
            return $" | LastImport({at}): {_lastImportSummary}";
        }

        private void SetLastError(string message)
        {
            _lastError = message ?? string.Empty;
            _lastOperation = "Error";
            UpdateStatus(_lastError);
        }

        private async Task<bool> CanExecuteExternalOperationAsync(string operationName)
        {
            if (uow == null)
            {
                SetLastError($"{operationName} preflight failed: UnitOfWork is not available.");
                Log(_lastError, Errors.Failed);
                return false;
            }

            if (_isLoading || _isSaving || _isRollingBack || _isImporting)
            {
                UpdateStatus($"{operationName} preflight blocked: another operation is running.");
                return false;
            }

            if (_hasPendingChanges || UowIsDirty)
            {
                var result = MessageBox.Show(
                    $"{operationName} works on persisted context. Save changes first?",
                    $"{operationName} Preflight",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    await SaveChangesAsync();
                    if (_hasPendingChanges || UowIsDirty)
                    {
                        SetLastError($"{operationName} preflight failed: save did not clear pending changes.");
                        Log(_lastError, Errors.Failed);
                        return false;
                    }
                }
                else
                {
                    UpdateStatus($"{operationName} canceled due to pending edits.");
                }

                return false;
            }

            return EnsureDataSourceReady();
        }

        private int GetUnitCount()
        {
            try
            {
                return UowCount;
            }
            catch
            {
                return 0;
            }
        }

        private string GetValidationSummary()
        {
            if (uow == null || !UowIsAutoValidateEnabled)
            {
                return "Validation:Off";
            }

            try
            {
                var invalidCount = UowGetInvalidItems()?.Count ?? 0;
                return invalidCount > 0 ? $"Validation:Err({invalidCount})" : "Validation:Ok";
            }
            catch
            {
                return "Validation:Unknown";
            }
        }

        private void RefreshCommandStates()
        {
            var hasUow = uow != null;
            var isBusy = _isLoading || _isSaving || _isRollingBack || _isImporting;
            var hasSelection = false;

            try
            {
                hasSelection = HasRowSelection();
            }
            catch
            {
                hasSelection = false;
            }

            var hasPending = _hasPendingChanges || (uow?.IsDirty == true);

            btnNew.Enabled = hasUow && !isBusy;
            btnEdit.Enabled = hasUow && !isBusy && hasSelection;
            btnDelete.Enabled = hasUow && !isBusy && hasSelection;
            btnRefresh.Enabled = hasUow && !isBusy;

            btnSave.Enabled = hasUow && !isBusy && hasPending;
            btnCancel.Enabled = hasUow && ((_isImporting && !_isImportCancelRequested) || (!isBusy && hasPending));

            btnUndo.Enabled = hasUow && !isBusy && UowIsUndoEnabled && UowCanUndo;
            btnRedo.Enabled = hasUow && !isBusy && UowIsUndoEnabled && UowCanRedo;

            var canRunExternal = hasUow &&
                                 !isBusy &&
                                 !_hasPendingChanges &&
                                 !UowIsDirty &&
                                 !string.IsNullOrWhiteSpace(_currentEntityName) &&
                                 !string.IsNullOrWhiteSpace(_currentDataSourceName);
            btnMap.Enabled = canRunExternal;
            btnImport.Enabled = canRunExternal;
        }

        private bool HasRowSelection()
        {
            try
            {
                return (beepGridPro1.SelectedRows?.Count ?? 0) > 0 || beepGridPro1.CurrentRowIndex >= 0;
            }
            catch
            {
                return false;
            }
        }

        #region Uow Adapter
        private string UowDatasourceName => uow?.DatasourceName ?? string.Empty;
        private string UowEntityName => uow?.EntityName ?? string.Empty;
        private dynamic UowUnits => uow?.Units;
        private bool UowIsDirty => uow?.IsDirty == true;
        private bool UowIsAutoValidateEnabled => uow?.IsAutoValidateEnabled == true;
        private bool UowBlockCommitOnValidationError => uow?.BlockCommitOnValidationError == true;
        private bool UowIsUndoEnabled => uow?.IsUndoEnabled == true;
        private bool UowCanUndo => uow?.CanUndo == true;
        private bool UowCanRedo => uow?.CanRedo == true;
        private int UowCount => uow?.Count ?? 0;
        private IDataSource? UowDataSource => uow?.DataSource;

        private Task<dynamic> UowGetAsync() => uow?.Get() ?? Task.FromResult((dynamic?)null);
        private Task<IErrorsInfo> UowCommitAsync() => uow?.Commit() ?? Task.FromResult<IErrorsInfo>(new ErrorsInfo { Flag = Errors.Failed, Message = "UnitOfWork is not available." });
        private Task<IErrorsInfo> UowRollbackAsync() => uow?.Rollback() ?? Task.FromResult<IErrorsInfo>(new ErrorsInfo { Flag = Errors.Failed, Message = "UnitOfWork is not available." });
        private TheTechIdea.Beep.Editor.ValidationResult UowValidateAll() => uow?.ValidateAll() ?? new TheTechIdea.Beep.Editor.ValidationResult();
        private List<dynamic> UowGetInvalidItems() => uow?.GetInvalidItems() ?? new List<dynamic>();
        private bool UowUndo() => uow?.Undo() == true;
        private bool UowRedo() => uow?.Redo() == true;
        #endregion

        private static bool HasLookupSource(BeepColumnConfig column)
        {
            if (column == null)
            {
                return false;
            }

            return !string.IsNullOrWhiteSpace(column.QueryToGetValues) ||
                   !string.IsNullOrWhiteSpace(column.EnumSourceType);
        }

        private void Log(string message, Errors flag = Errors.Ok)
        {
            Editor?.AddLogMessage("DataEdit", message, DateTime.Now, 0, null, flag);
        }
    }
}
