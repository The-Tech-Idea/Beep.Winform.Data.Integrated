using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Editor.Importing;
using TheTechIdea.Beep.Editor.Importing.Factories;
using TheTechIdea.Beep.Editor.Importing.History;
using TheTechIdea.Beep.Editor.Importing.Interfaces;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Default.Views.Template;
using TheTechIdea.Beep.Winform.Default.Views.Setup;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    /// <summary>
    /// Direction for the wizard – controls labels, default pre-fill order, and step titles.
    /// </summary>
    public enum ImportExportDirection { Import, Export }

    [AddinAttribute(Caption = "Import/Export Wizard", Name = "uc_ImportExportWizardLauncher",
        misc = "Config", menu = "Configuration", addinType = AddinType.Control,
        displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 5, RootNodeName = "Configuration", Order = 5, ID = 5, BranchText = "Import/Export Wizard", BranchType = EnumPointType.Function, IconImageName = "drivers.svg", BranchClass = "ADDIN", BranchDescription = "Import and export wizard for data movement workflows")]
    public partial class uc_ImportExportWizardLauncher : TemplateUserControl, IAddinVisSchema
    {
        private readonly IServiceProvider _services;
        private bool _isLoading;

        public event EventHandler<ImportExportSelectionSnapshot>? SelectionSnapshotChanged;

        // ── Backing fields ──────────────────────────────────────────────────────
        private ImportExportDirection _direction = ImportExportDirection.Import;
        private string _sourceDataSourceName = string.Empty;
        private string _sourceEntityName = string.Empty;
        private string _destinationDataSourceName = string.Empty;
        private string _destinationEntityName = string.Empty;

        // ── Phase 4: history + last summary ────────────────────────────────────
        private IImportRunHistoryStore? _historyStore;
        private ImportRunSummary? _lastSummary;
        private DataTable _historyDt;

        #region IAddinVisSchema
        public string RootNodeName { get; set; } = "Configuration";
        public string CatgoryName { get; set; } = string.Empty;
        public int Order { get; set; } = 5;
        public int ID { get; set; } = 5;
        public string BranchText { get; set; } = "Import/Export Wizard";
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        public int BranchID { get; set; } = 5;
        public string IconImageName { get; set; } = "drivers.svg";
        public string BranchStatus { get; set; } = string.Empty;
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; } = "Import and export wizard for data movement workflows";
        public string BranchClass { get; set; } = "ADDIN";
        public string AddinName { get; set; } = "uc_ImportExportWizardLauncher";
        #endregion

        // ── Public configuration properties ────────────────────────────────────
        /// <summary>Import = data flows into this system; Export = data flows out.</summary>
        [Category("Import/Export"), DefaultValue(ImportExportDirection.Import)]
        public ImportExportDirection Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                if (!_isLoading) SyncDirectionUI();
            }
        }

        [Category("Import/Export"), DefaultValue("")]
        public string SourceDataSourceName
        {
            get => _sourceDataSourceName;
            set { _sourceDataSourceName = value ?? string.Empty; if (!_isLoading) cmbSourceDS.SelectItemByText(_sourceDataSourceName); }
        }

        [Category("Import/Export"), DefaultValue("")]
        public string SourceEntityName
        {
            get => _sourceEntityName;
            set { _sourceEntityName = value ?? string.Empty; if (!_isLoading) cmbSourceEntity.SelectItemByText(_sourceEntityName); }
        }

        [Category("Import/Export"), DefaultValue("")]
        public string DestinationDataSourceName
        {
            get => _destinationDataSourceName;
            set { _destinationDataSourceName = value ?? string.Empty; if (!_isLoading) cmbDestDS.SelectItemByText(_destinationDataSourceName); }
        }

        [Category("Import/Export"), DefaultValue("")]
        public string DestinationEntityName
        {
            get => _destinationEntityName;
            set { _destinationEntityName = value ?? string.Empty; if (!_isLoading) cmbDestEntity.SelectItemByText(_destinationEntityName); }
        }

        [Category("Import/Export"), DefaultValue(true)]
        public bool CreateDestinationIfNotExists
        {
            get => chkCreateIfNotExists.CurrentValue;
            set { if (!_isLoading) chkCreateIfNotExists.CurrentValue = value; }
        }

        [Category("Import/Export"), DefaultValue(true)]
        public bool AddMissingColumns
        {
            get => chkAddMissing.CurrentValue;
            set { if (!_isLoading) chkAddMissing.CurrentValue = value; }
        }

        public ImportExportSelectionSnapshot GetSelectionSnapshot()
        {
            return new ImportExportSelectionSnapshot
            {
                Direction = _direction,
                SourceDataSourceName = cmbSourceDS.SelectedItem?.Text ?? _sourceDataSourceName,
                SourceEntityName = cmbSourceEntity.SelectedItem?.Text ?? _sourceEntityName,
                DestinationDataSourceName = cmbDestDS.SelectedItem?.Text ?? _destinationDataSourceName,
                DestinationEntityName = cmbDestEntity.SelectedItem?.Text ?? _destinationEntityName,
                CreateDestinationIfNotExists = chkCreateIfNotExists.CurrentValue,
                AddMissingColumns = chkAddMissing.CurrentValue
            };
        }

        public void ApplySelectionSnapshot(ImportExportSelectionSnapshot snapshot)
        {
            if (snapshot == null)
                return;

            _isLoading = true;
            try
            {
                Direction = snapshot.Direction;
                SourceDataSourceName = snapshot.SourceDataSourceName;
                SourceEntityName = snapshot.SourceEntityName;
                DestinationDataSourceName = snapshot.DestinationDataSourceName;
                DestinationEntityName = snapshot.DestinationEntityName;
                CreateDestinationIfNotExists = snapshot.CreateDestinationIfNotExists;
                AddMissingColumns = snapshot.AddMissingColumns;
            }
            finally
            {
                _isLoading = false;
            }

            SyncDirectionUI();
            LoadDataSources();
            NotifySelectionSnapshotChanged();
        }

        public void ApplySetupSnapshot(uc_SetupWizard.SetupWizardSnapshot snapshot)
        {
            if (snapshot == null)
                return;

            var cp = snapshot.ConnectionProperties;
            var sourceName = cp?.ConnectionName ?? string.Empty;
            var entityName = cp?.Database ?? string.Empty;

            ApplySelectionSnapshot(new ImportExportSelectionSnapshot
            {
                Direction = ImportExportDirection.Import,
                SourceDataSourceName = sourceName,
                SourceEntityName = entityName,
                DestinationDataSourceName = sourceName,
                DestinationEntityName = entityName,
                CreateDestinationIfNotExists = true,
                AddMissingColumns = true
            });
        }

        // ── Constructor ─────────────────────────────────────────────────────────
        public uc_ImportExportWizardLauncher(IServiceProvider services) : base(services)
        {
            _services = services;
            Details.AddinName = "Import / Export Wizard";

            InitializeComponent();

        }

        // ── Programmatic launch overloads ───────────────────────────────────────
        /// <summary>Launch the wizard using whatever is already configured in the UI.</summary>
        public void Launch() => LaunchWizard();

        /// <summary>Pre-fill source and destination, then immediately launch.</summary>
        public void LaunchWithSelection(string srcDs, string srcEntity,
                                        string destDs = null,
                                        string destEntity = null,
                                        ImportExportDirection direction = ImportExportDirection.Import)
        {
            Direction = direction;
            SourceDataSourceName = srcDs;
            SourceEntityName = srcEntity;
            if (destDs != null) DestinationDataSourceName = destDs;
            if (destEntity != null) DestinationEntityName = destEntity;
            LaunchWizard();
        }

        /// <summary>Pre-fill for export: the "owned" side is treated as Source.</summary>
        public void LaunchExport(string srcDs, string srcEntity,
                                 string destDs = null, string destEntity = null)
            => LaunchWithSelection(srcDs, srcEntity, destDs, destEntity, ImportExportDirection.Export);

        // ── Navigation ──────────────────────────────────────────────────────────
        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            base.OnNavigatedTo(parameters);
            LoadDataSources();
            InitHistoryGrid();
            LoadRecentTemplates();
            AppendLog($"Ready. Configure source/destination then click Launch ({_direction}).");
        }

        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);

            // Default option values
            chkCreateIfNotExists.CurrentValue = true;
            chkAddMissing.CurrentValue = true;

            // Wire events
            cmbDirection.SelectedItemChanged += (_, _) =>
            {
                SyncDirectionFromCombo();
                NotifySelectionSnapshotChanged();
            };
            cmbSourceDS.SelectedItemChanged += (_, _) =>
            {
                LoadEntities(cmbSourceDS.SelectedItem?.Text, cmbSourceEntity);
                NotifySelectionSnapshotChanged();
            };
            cmbDestDS.SelectedItemChanged += (_, _) =>
            {
                LoadEntities(cmbDestDS.SelectedItem?.Text, cmbDestEntity);
                NotifySelectionSnapshotChanged();
            };
            cmbSourceEntity.SelectedItemChanged += (_, _) => NotifySelectionSnapshotChanged();
            cmbDestEntity.SelectedItemChanged += (_, _) => NotifySelectionSnapshotChanged();
            btnLaunch.Click += (_, _) => LaunchWizard();
            btnClearLog.Click += (_, _) => txtLog.Clear();
            btnSwap.Click += (_, _) => SwapSourceDest();
            btnQuickImport.Click += (_, _) => QuickImport_Click();
            btnViewLastSummary.Click += (_, _) => ViewLastSummary_Click();

            // Seed direction combo
            var dirItems = new BindingList<SimpleItem>
            {
                new SimpleItem { Text = "Import", Item = ImportExportDirection.Import },
                new SimpleItem { Text = "Export", Item = ImportExportDirection.Export }
            };
            cmbDirection.ListItems = dirItems;
            cmbDirection.SelectItemByText("Import");
            SyncDirectionUI();
            LoadDataSources();
            InitHistoryGrid();
            LoadRecentTemplates();
            NotifySelectionSnapshotChanged();
        }

        // ── Core wizard launch ──────────────────────────────────────────────────
        private void LaunchWizard()
        {
            if (_direction == ImportExportDirection.Export)
            {
                LaunchExportWizard();
                return;
            }

            var config = BuildInitialConfig();

            // ── 5-step import wizard ───────────────────────────────────────────
            var selectStep = new uc_Import_SelectDSandEntity(_services);
            var colStep = new uc_Import_ColumnSelection(_services);
            var mapStep = new uc_Import_MapFields(_services);
            var optionsStep = new uc_Import_Options(_services);
            var runStep = new uc_Import_Run(_services);

            var wizardConfig = new WizardConfig
            {
                Key = $"ImportExportWizard_{Guid.NewGuid():N}",
                Title = "Import Wizard",
                Description = "Select source/destination, choose columns, map fields, set options, then run import.",
                Style = WizardStyle.HorizontalStepper,
                ShowProgressBar = true,
                ShowStepList = true,
                AllowBack = true,
                AllowCancel = true,
                ShowInlineErrors = true,
                Steps = new List<WizardStep>
                {
                    new WizardStep { Key = "select",  Title = "Configure",              Description = "Choose data sources, entities, and import mode.",     Content = selectStep  },
                    new WizardStep { Key = "columns", Title = "Select Columns",         Description = "Choose which source columns to include.",              Content = colStep     },
                    new WizardStep { Key = "map",     Title = "Map Fields",             Description = "Map source fields to destination fields.",             Content = mapStep     },
                    new WizardStep { Key = "options", Title = "Options & Pre-flight",   Description = "Set batch size, dry-run, validation, and review.",     Content = optionsStep },
                    new WizardStep { Key = "run",     Title = "Review & Run",           Description = "Review summary and execute the import.",               Content = runStep     }
                }
            };

            wizardConfig.OnProgress = (cur, tot, t) => AppendLog($"[{cur}/{tot}] {t}");
            wizardConfig.OnComplete = RunImportFromWizardContext;
            wizardConfig.OnCancel = _ => AppendLog("Wizard cancelled.");

            // Pre-inject the config so Step 1 restores from it
            var wizardInstance = WizardManager.CreateWizard(wizardConfig);
            wizardInstance.Context.SetValue(WizardKeys.ImportConfig, config);

            AppendLog("Launching Import Wizard…");
            var owner = FindForm();
            var result = owner == null
                ? wizardInstance.ShowDialog()
                : wizardInstance.ShowDialog(owner);
            AppendLog($"Wizard closed: {result}");
        }

        private void LaunchExportWizard()
        {
            var exportConfig = new ExportConfiguration
            {
                SourceDataSourceName = cmbSourceDS.SelectedItem?.Text ?? string.Empty,
                SourceEntityName     = cmbSourceEntity.SelectedItem?.Text ?? string.Empty,
                DestDataSourceName   = cmbDestDS.SelectedItem?.Text ?? string.Empty,
                DestEntityName       = cmbDestEntity.SelectedItem?.Text ?? string.Empty
            };

            var selectStep = new uc_Export_SelectDSandFile(_services);
            var colStep    = new uc_Export_ColumnSelection(_services, exportConfig);
            var runStep    = new uc_Export_Run(_services, exportConfig);

            var wizardConfig = new WizardConfig
            {
                Key              = $"ExportWizard_{Guid.NewGuid():N}",
                Title            = "Export Wizard",
                Description      = "Choose source, select columns, then export data.",
                Style            = WizardStyle.HorizontalStepper,
                ShowProgressBar  = true,
                ShowStepList     = true,
                AllowBack        = true,
                AllowCancel      = true,
                ShowInlineErrors = true,
                Steps = new List<WizardStep>
                {
                    new WizardStep { Key = "select",  Title = "Configure",      Description = "Choose source entity and export destination.", Content = selectStep },
                    new WizardStep { Key = "columns", Title = "Select Columns", Description = "Choose which columns to export.",               Content = colStep   },
                    new WizardStep { Key = "run",     Title = "Review & Export",Description = "Review summary and execute the export.",        Content = runStep   }
                }
            };

            wizardConfig.OnComplete = ctx =>
            {
                var summary = ctx.GetValue<ExportRunSummary?>(ExportWizardKeys.RunSummary, null);
                if (summary != null)
                    AppendLog($"Export complete: {summary.ExportedRows:N0} rows");
            };
            wizardConfig.OnCancel = _ => AppendLog("Export wizard cancelled.");

            var wizardInstance = WizardManager.CreateWizard(wizardConfig);
            wizardInstance.Context.SetValue(ExportWizardKeys.ExportConfig, exportConfig);

            AppendLog("Launching Export Wizard…");
            var owner = FindForm();
            var result = owner == null
                ? wizardInstance.ShowDialog()
                : wizardInstance.ShowDialog(owner);
            AppendLog($"Export wizard closed: {result}");
        }

        private void RunImportFromWizardContext(WizardContext context)
        {
            if (context.GetValue<bool>(WizardKeys.LastRunSucceeded, false))
            {
                AppendLog("Import already ran inside the wizard step. Done.");
                return;
            }
            if (!context.GetValue<bool>(WizardKeys.RunImportOnFinish, true))
            {
                AppendLog("Run-on-finish disabled."); return;
            }
            if (Editor == null) { AppendLog("Editor not available."); return; }

            var importConfig = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null);
            if (importConfig == null) { AppendLog("No config in wizard context."); return; }
            if ((importConfig.Mapping?.MappedEntities?.Count ?? 0) == 0) { AppendLog("No field mappings — import aborted."); return; }

            AppendLog("Running import…");

            // Capture any summary the Run step may have stored
            _lastSummary = context.GetValue<ImportRunSummary?>(WizardKeys.RunSummary, null);

            var manager = new DataImportManager(Editor);
            var progress = new Progress<IPassedArgs>(args =>
            {
                if (!string.IsNullOrWhiteSpace(args?.Messege)) AppendLog(args.Messege);
            });

            _ = manager.RunImportAsync(importConfig, progress, default)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        var msg = t.Exception?.GetBaseException().Message ?? "Unknown error";
                        AppendLog($"Import failed: {msg}");
                        AddToHistory(importConfig, 0, false);
                        if (IsHandleCreated)
                            BeginInvoke(() => MessageBox.Show($"Import failed:\n{msg}", caption: "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error));
                        return;
                    }
                    var info = t.Result;
                    var success = info?.Flag == Errors.Ok;
                    AppendLog(info?.Message ?? "Completed.");
                    // Capture summary from context if available (Run step may have set it)
                    var rows = _lastSummary?.TotalRows ?? 0;
                    AddToHistory(importConfig, rows, success);
                    if (IsHandleCreated)
                        BeginInvoke(() => MessageBox.Show(
                            success ? "Import completed successfully." : $"Import finished with errors:\n{info?.Message}",
                            success ? "Done" : "Warning",
                            MessageBoxButtons.OK,
                            success ? MessageBoxIcon.Information : MessageBoxIcon.Warning));
                }, TaskScheduler.Default);
        }

        // ── Helpers ─────────────────────────────────────────────────────────────
        private DataImportConfiguration BuildInitialConfig()
        {
            return new DataImportConfiguration
            {
                SourceDataSourceName = cmbSourceDS.SelectedItem?.Text ?? string.Empty,
                SourceEntityName = cmbSourceEntity.SelectedItem?.Text ?? string.Empty,
                DestDataSourceName = cmbDestDS.SelectedItem?.Text ?? string.Empty,
                DestEntityName = cmbDestEntity.SelectedItem?.Text ?? string.Empty,
                CreateDestinationIfNotExists = chkCreateIfNotExists.CurrentValue,
                AddMissingColumns = chkAddMissing.CurrentValue
            };
        }

        private void LoadDataSources()
        {
            _isLoading = true;
            try
            {
                var names = (Editor?.ConfigEditor?.DataConnections ?? new List<ConnectionProperties>())
                    .Select(p => p.ConnectionName)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(n => n)
                    .Select(n => new SimpleItem { Text = n, Item = n })
                    .ToList();

                var list = new BindingList<SimpleItem>(names);
                cmbSourceDS.ListItems = list;
                cmbDestDS.ListItems = new BindingList<SimpleItem>(names);

                if (!string.IsNullOrWhiteSpace(_sourceDataSourceName))
                    cmbSourceDS.SelectItemByText(_sourceDataSourceName);
                if (!string.IsNullOrWhiteSpace(_destinationDataSourceName))
                    cmbDestDS.SelectItemByText(_destinationDataSourceName);
            }
            finally { _isLoading = false; }

            // Cascade entity combos
            LoadEntities(cmbSourceDS.SelectedItem?.Text, cmbSourceEntity, _sourceEntityName);
            LoadEntities(cmbDestDS.SelectedItem?.Text, cmbDestEntity, _destinationEntityName);
            NotifySelectionSnapshotChanged();
        }

        private void LoadEntities(string dataSourceName, BeepComboBox combo, string restoreValue = null)
        {
            if (string.IsNullOrWhiteSpace(dataSourceName) || combo == null) return;
            try
            {
                var ds = Editor?.GetDataSource(dataSourceName);
                if (ds == null) return;
                if (ds.ConnectionStatus != System.Data.ConnectionState.Open) ds.Openconnection();
                var list = ds.GetEntitesList()?.ToList() ?? new List<string>();
                if (list.Count == 0 && ds.EntitiesNames != null) list = ds.EntitiesNames.ToList();

                var items = new BindingList<SimpleItem>(
                    list.Where(n => !string.IsNullOrWhiteSpace(n))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(n => n)
                        .Select(n => new SimpleItem { Text = n, Item = n })
                        .ToList());

                combo.ListItems = items;
                var restore = restoreValue ?? combo.SelectedItem?.Text;
                if (!string.IsNullOrWhiteSpace(restore)) combo.SelectItemByText(restore);
            }
            catch (Exception ex)
            {
                Editor?.AddLogMessage("ImportExport", $"Error loading entities for '{dataSourceName}': {ex.Message}",
                    DateTime.Now, 0, null, Errors.Failed);
            }
        }

        private void SwapSourceDest()
        {
            var srcDs = cmbSourceDS.SelectedItem?.Text ?? string.Empty;
            var srcEntity = cmbSourceEntity.SelectedItem?.Text ?? string.Empty;
            var dstDs = cmbDestDS.SelectedItem?.Text ?? string.Empty;
            var dstEntity = cmbDestEntity.SelectedItem?.Text ?? string.Empty;

            cmbSourceDS.SelectItemByText(dstDs);
            cmbSourceEntity.SelectItemByText(dstEntity);
            cmbDestDS.SelectItemByText(srcDs);
            cmbDestEntity.SelectItemByText(srcEntity);
            AppendLog("Source and destination swapped.");
            NotifySelectionSnapshotChanged();
        }

        private void SyncDirectionFromCombo()
        {
            if (cmbDirection.SelectedItem?.Item is ImportExportDirection d)
                _direction = d;
            SyncDirectionUI();
        }

        private void SyncDirectionUI()
        {
            bool isExport = _direction == ImportExportDirection.Export;
            lblSourceDS.Text = isExport ? "Export From:" : "Source:";
            lblDestDS.Text = isExport ? "Export To:" : "Destination:";
            btnLaunch.Text = isExport ? "Launch Export Wizard" : "Launch Import Wizard";
        }

        private void AppendLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            if (txtLog.InvokeRequired) { txtLog.BeginInvoke(new Action<string>(AppendLog), message); return; }
            txtLog.AppendText($"{DateTime.Now:HH:mm:ss}  {message}{Environment.NewLine}");
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }

        // ── Phase 4: recent templates ────────────────────────────────────────────
        private void LoadRecentTemplates()
        {
            try
            {
                var all = ImportTemplateManager.ListAll() ?? Array.Empty<string>();
                var items = new System.ComponentModel.BindingList<Controls.Models.SimpleItem>(
                    all.Take(5)
                       .Select(n => new Controls.Models.SimpleItem { Text = n, Item = n })
                       .ToList());
                cmbRecentTemplates.ListItems = items;
                btnQuickImport.Enabled = items.Count > 0;
            }
            catch { /* templates are optional */ }
        }

        private void QuickImport_Click()
        {
            var templateName = cmbRecentTemplates.SelectedItem?.Text;
            if (string.IsNullOrWhiteSpace(templateName))
            {
                MessageBox.Show("Please select a template first.", "Quick Import",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                var dto = ImportTemplateManager.Load(templateName);
                if (dto == null) { AppendLog($"Template '{templateName}' not found."); return; }

                var config = BuildInitialConfig();
                ImportTemplateManager.ApplyToConfig(dto, config);

                if ((config.Mapping?.MappedEntities?.Count ?? 0) == 0)
                {
                    AppendLog("Template has no field mappings — launching wizard instead.");
                    LaunchWizard();
                    return;
                }

                AppendLog($"Quick Import using template '{templateName}'…");
                var importManager = new DataImportManager(Editor);
                var progress = new Progress<IPassedArgs>(a =>
                { if (!string.IsNullOrWhiteSpace(a?.Messege)) AppendLog(a.Messege); });

                _ = importManager.RunImportAsync(config, progress, default)
                    .ContinueWith(t =>
                    {
                        var success = !t.IsFaulted && t.Result?.Flag == Errors.Ok;
                        AppendLog(t.IsFaulted
                            ? $"Quick Import failed: {t.Exception?.GetBaseException().Message}"
                            : (t.Result?.Message ?? "Done."));
                        AddToHistory(config, _lastSummary?.TotalRows ?? 0, success);
                    }, TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                AppendLog($"Quick Import error: {ex.Message}");
            }
        }

        private void ViewLastSummary_Click()
        {
            if (_lastSummary == null)
            {
                MessageBox.Show("No recent import summary available.",
                    "Last Summary", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Last Import Summary ===");
            sb.AppendLine($"  Total rows   : {_lastSummary.TotalRows:N0}");
            sb.AppendLine($"  Added        : {_lastSummary.AddedRows:N0}");
            sb.AppendLine($"  Updated      : {_lastSummary.UpdatedRows:N0}");
            sb.AppendLine($"  Skipped      : {_lastSummary.SkippedRows:N0}");
            sb.AppendLine($"  Failed       : {_lastSummary.FailedRows:N0}");
            sb.AppendLine($"  Duration     : {_lastSummary.Duration}");
            sb.AppendLine($"  Rows/sec     : {_lastSummary.RowsPerSec:N1}");
            if (_lastSummary.Errors.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Errors:");
                foreach (var e in _lastSummary.Errors.Take(20))
                    sb.AppendLine($"  Row {e.RowIndex}: {e.ErrorMessage}");
            }
            MessageBox.Show(sb.ToString(), "Last Summary", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Phase 4: history grid ────────────────────────────────────────────────
        private bool _historyInitialized;
        private void InitHistoryGrid()
        {
            if (_historyInitialized) return;
            _historyInitialized = true;

            // Build the backing DataTable
            _historyDt = new DataTable("RunHistory");
            _historyDt.Columns.Add("When", typeof(string));
            _historyDt.Columns.Add("Source", typeof(string));
            _historyDt.Columns.Add("Dest", typeof(string));
            _historyDt.Columns.Add("Rows", typeof(string));
            _historyDt.Columns.Add("Status", typeof(string));

            // Configure BeepGridPro columns
            historyGrid.Columns.Clear();
            historyGrid.Columns.Add(new BeepColumnConfig { ColumnName = "When", ColumnCaption = "Time", Width = 80 });
            historyGrid.Columns.Add(new BeepColumnConfig { ColumnName = "Source", ColumnCaption = "Source", Width = 130 });
            historyGrid.Columns.Add(new BeepColumnConfig { ColumnName = "Dest", ColumnCaption = "Dest", Width = 130 });
            historyGrid.Columns.Add(new BeepColumnConfig { ColumnName = "Rows", ColumnCaption = "Rows", Width = 60 });
            historyGrid.Columns.Add(new BeepColumnConfig { ColumnName = "Status", ColumnCaption = "Status", Width = 70 });

            historyGrid.DataSource = _historyDt;
        }

        private void AddToHistory(DataImportConfiguration config, int rowCount, bool success)
        {
            _ = AddToHistoryAsync(config, rowCount, success);
        }

        private async Task AddToHistoryAsync(DataImportConfiguration config, int rowCount, bool success)
        {
            try
            {
                _historyStore ??= LocalStoreFactory.CreateHistoryStore(Editor);
                var record = new ImportRunRecord
                {
                    ContextKey = $"{config.SourceDataSourceName}/{config.SourceEntityName}/{config.DestDataSourceName}/{config.DestEntityName}",
                    StartedAt = DateTime.UtcNow.AddMinutes(-1),
                    FinishedAt = DateTime.UtcNow,
                    RecordsRead = rowCount,
                    RecordsWritten = rowCount,
                    FinalState = success ? ImportState.Completed : ImportState.Faulted,
                    Summary = $"Source: {config.SourceDataSourceName}/{config.SourceEntityName} -> Dest: {config.DestDataSourceName}/{config.DestEntityName}"
                };
                await _historyStore.SaveRunAsync(record);
                btnViewLastSummary.Enabled = true;
                if (IsHandleCreated)
                    BeginInvoke(() => _ = RefreshHistoryGridAsync());
            }
            catch (Exception ex)
            {
                Editor?.AddLogMessage("ImportExport", $"Error saving history: {ex.Message}",
                    DateTime.Now, 0, null, Errors.Failed);
            }
        }

        private async Task RefreshHistoryGridAsync()
        {
            try
            {
                InitHistoryGrid();
                _historyDt.Rows.Clear();
                _historyStore ??= LocalStoreFactory.CreateHistoryStore(Editor);
                var runs = await _historyStore.GetRunsAsync("*");
                foreach (var run in runs.Take(5))
                {
                    var parts = run.ContextKey?.Split('/') ?? new string[4];
                    var src = parts.Length >= 2 ? $"{parts[0]}/{parts[1]}" : run.ContextKey;
                    var dst = parts.Length >= 4 ? $"{parts[2]}/{parts[3]}" : "";
                    _historyDt.Rows.Add(
                        run.FinishedAt?.ToString("HH:mm:ss") ?? run.StartedAt.ToString("HH:mm:ss"),
                        src,
                        dst,
                        run.RecordsRead.ToString("N0"),
                        run.FinalState == ImportState.Completed ? "✓ OK" : "✕ Failed");
                }
            }
            catch (Exception ex)
            {
                Editor?.AddLogMessage("ImportExport", $"Error loading history: {ex.Message}",
                    DateTime.Now, 0, null, Errors.Failed);
            }
        }

        private void RefreshHistoryGrid()
        {
            _ = RefreshHistoryGridAsync();
        }

        private void beepImage1_Click(object sender, EventArgs e)
        {

        }

        private void NotifySelectionSnapshotChanged()
        {
            if (_isLoading)
                return;

            SelectionSnapshotChanged?.Invoke(this, GetSelectionSnapshot());
        }

        public sealed class ImportExportSelectionSnapshot
        {
            public ImportExportDirection Direction { get; set; } = ImportExportDirection.Import;
            public string SourceDataSourceName { get; set; } = string.Empty;
            public string SourceEntityName { get; set; } = string.Empty;
            public string DestinationDataSourceName { get; set; } = string.Empty;
            public string DestinationEntityName { get; set; } = string.Empty;
            public bool CreateDestinationIfNotExists { get; set; } = true;
            public bool AddMissingColumns { get; set; } = true;
        }
    }
}