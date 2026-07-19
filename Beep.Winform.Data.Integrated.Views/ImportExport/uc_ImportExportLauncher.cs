using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Editor.Importing;
using TheTechIdea.Beep.Editor.Importing.Factories;
using TheTechIdea.Beep.Winform.Controls.Forms.ModernForm;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport.Models;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport.Import;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport.Export;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    [AddinAttribute(Caption = "Import/Export Wizard", Name = "uc_ImportExportLauncher",
        misc = "Data", menu = "DataOperations", addinType = AddinType.Control,
        displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 20, RootNodeName = "Data Operations", Order = 20, ID = 20,
        BranchText = "Import/Export", BranchType = EnumPointType.Function,
        IconImageName = "fileconnections.svg", BranchClass = "ADDIN",
        BranchDescription = "Import and Export data wizard")]
    public partial class uc_ImportExportLauncher : TemplateUserControl, IAddinVisSchema
    {
        private readonly IServiceProvider? _services;
        private IImportRunHistoryStore? _historyStore;

        /// <summary>
        /// Designer/parameterless ctor. Deliberately does not chain to the IServiceProvider overload
        /// with null — that would resolve services off a null provider and throw. Without services the
        /// hub still renders; the wizard buttons report that they can't launch.
        /// </summary>
        public uc_ImportExportLauncher()
        {
            _services = null;
            InitializeControl();
        }

        public uc_ImportExportLauncher(IServiceProvider services) : base(services)
        {
            _services = services;
            InitializeControl();
        }

        private void InitializeControl()
        {
            InitializeComponent();
            btnImport.Click += (_, _) => LaunchImportWizard();
            btnExport.Click += (_, _) => LaunchExportWizard();
            btnRefreshHistory.Click += async (_, _) => await LoadHistoryAsync().ConfigureAwait(true);
            Load += async (_, _) => await LoadHistoryAsync().ConfigureAwait(true);
        }

        // ── Run history ───────────────────────────────────────────────────────
        //
        // IImportRunHistoryStore is strictly per-context: SaveRunAsync files a record under
        // record.ContextKey, and GetRunsAsync only ever returns one key's records — there is no way
        // to enumerate contexts. This hub wants the opposite: a feed across every entity.
        //
        // So the hub records under two fixed keys of its own rather than under "{datasource}.{entity}".
        // That also fixes the reason this grid was always empty: runs were SAVED under
        // "{datasource}.{entity}" and READ back with the literal key "import", which can never match.
        // The real source lives in the record's Summary, and the key itself distinguishes the two
        // operations — so no string has to be parsed back out.
        //
        // Nothing else consumes these records: DataImportManager never reads
        // DataImportConfiguration.RunHistoryStore, so the launcher is the only writer and reader and
        // is free to choose the convention.
        private const string ImportHistoryKey = "ImportExportHub.Import";
        private const string ExportHistoryKey = "ImportExportHub.Export";

        /// <summary>
        /// Guards against overlapping history loads. Load and the Refresh button both trigger one,
        /// and a slow first read could otherwise land after a newer one and show stale rows.
        /// </summary>
        private int _historyGeneration;

        /// <summary>
        /// Shows the import/export hub as a modal popup, matching how the setup wizard is launched.
        /// </summary>
        /// <remarks>
        /// The hub — not a single wizard — is what opens, because it owns both the Import and Export
        /// entry points plus run history, and the routed action id ("uc_importexportlauncher")
        /// contains both words, so picking one would be a guess.
        /// </remarks>
        public static DialogResult ShowAsDialog(IServiceProvider? services, IWin32Window? owner)
        {
            var content = services != null
                ? new uc_ImportExportLauncher(services)
                : new uc_ImportExportLauncher();
            content.Dock = DockStyle.Fill;

            using var host = new BeepiFormPro
            {
                Text = "Import / Export",
                ClientSize = new Size(900, 600),
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
            };
            host.Controls.Add(content);
            return owner != null ? host.ShowDialog(owner) : host.ShowDialog();
        }

        #region IAddinVisSchema
        public string RootNodeName { get; set; } = "Data Operations";
        public string CatgoryName { get; set; } = string.Empty;
        public string AddinName { get; set; } = "uc_ImportExportLauncher";
        public int Order { get; set; } = 20;
        public int ID { get; set; } = 20;
        public string Name { get; set; } = "uc_ImportExportLauncher";
        public string BranchText { get; set; } = "Import/Export";
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        public int BranchID { get; set; } = 20;
        public string IconImageName { get; set; } = "fileconnections.svg";
        public string BranchStatus { get; set; } = string.Empty;
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; } = "Import and Export data wizard";
        public string BranchClass { get; set; } = "ADDIN";
        #endregion

        private void LaunchImportWizard()
        {
            if (_services == null)
            {
                MessageBox.Show(this,
                    "The import wizard needs application services, which are not available on this instance.",
                    "Import", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var step1 = new uc_ImportStep1_Configure(_services);
            var step2 = new uc_ImportStep2_Columns(_services);
            var step3 = new uc_ImportStep3_Mapping(_services);
            var step4 = new uc_ImportStep4_Options(_services);
            var step5 = new uc_ImportStep5_Run(_services);

            var config = new WizardConfig
            {
                Title = "Import Wizard",
                Description = "Import data from source to destination",
                // Skill § 1: use BeepLayoutMetrics tokens for dialog sizes; ScaleSize() DPI-scales the value.
                Size = BeepLayoutMetrics.DialogLarge.ScaleSize(this),
                Style = WizardStyle.HorizontalStepper,
            };

            config.Steps.Add(new WizardStep { Key = "configure", Title = "Configure", Description = "Source & destination", Content = step1 });
            config.Steps.Add(new WizardStep { Key = "columns", Title = "Columns", Description = "Select columns", Content = step2 });
            config.Steps.Add(new WizardStep { Key = "mapping", Title = "Mapping", Description = "Map fields", Content = step3 });
            config.Steps.Add(new WizardStep { Key = "options", Title = "Options", Description = "Batch size, quality rules", Content = step4 });
            config.Steps.Add(new WizardStep { Key = "run", Title = "Run", Description = "Execute import", Content = step5 });

            config.OnComplete = async ctx => await OnImportCompleteAsync(ctx).ConfigureAwait(true);

            var instance = WizardManager.CreateWizard(config);
            instance.ShowDialog(this);
        }

        private void LaunchExportWizard()
        {
            if (_services == null)
            {
                MessageBox.Show(this,
                    "The export wizard needs application services, which are not available on this instance.",
                    "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var step1 = new uc_ExportStep1_Select(_services);
            var step2 = new uc_ExportStep2_Columns(_services);
            var step3 = new uc_ExportStep3_Run(_services);

            var config = new WizardConfig
            {
                Title = "Export Wizard",
                Description = "Export data to file or another data source",
                // Skill § 1: use BeepLayoutMetrics tokens for dialog sizes; ScaleSize() DPI-scales the value.
                Size = BeepLayoutMetrics.DialogLarge.ScaleSize(this),
                Style = WizardStyle.HorizontalStepper,
            };

            config.Steps.Add(new WizardStep { Key = "select", Title = "Select", Description = "Source & target", Content = step1 });
            config.Steps.Add(new WizardStep { Key = "columns", Title = "Columns", Description = "Select columns", Content = step2 });
            config.Steps.Add(new WizardStep { Key = "run", Title = "Run", Description = "Execute export", Content = step3 });

            // The import wizard had this and the export wizard did not, so export runs were never
            // recorded and the history feed silently showed imports only.
            config.OnComplete = async ctx => await OnExportCompleteAsync(ctx).ConfigureAwait(true);

            var instance = WizardManager.CreateWizard(config);
            instance.ShowDialog(this);
        }

        private async Task OnImportCompleteAsync(WizardContext context)
        {
            var config = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null);
            var summary = context.GetValue<ImportRunSummary?>(WizardKeys.RunSummary, null);

            if (config != null && Editor != null && summary != null)
            {
                var record = new ImportRunRecord
                {
                    ContextKey = ImportHistoryKey,
                    StartedAt = DateTime.UtcNow - summary.Duration,
                    FinishedAt = DateTime.UtcNow,
                    FinalState = summary.FailedRows == 0 ? ImportState.Completed : ImportState.Faulted,
                    SyncMode = config.SyncMode,
                    RecordsRead = summary.TotalRows,
                    RecordsWritten = summary.TotalRows - summary.FailedRows,
                    RecordsBlocked = summary.FailedRows,
                    Summary = $"{config.SourceDataSourceName}.{config.SourceEntityName} → " +
                              $"{config.DestDataSourceName}.{config.DestEntityName} — " +
                              $"Added:{summary.AddedRows} Updated:{summary.UpdatedRows} Failed:{summary.FailedRows}",
                };
                await SaveRunAsync(record, "import").ConfigureAwait(true);
            }

            await LoadHistoryAsync().ConfigureAwait(true);
        }

        private async Task OnExportCompleteAsync(WizardContext context)
        {
            var config = context.GetValue<ExportConfiguration?>(WizardKeys.ExportConfig, null);
            var summary = context.GetValue<ExportRunSummary?>(WizardKeys.ExportRunSummary, null);

            if (config != null && Editor != null && summary != null)
            {
                // Destination is a file for Csv/Json/Xml and an entity otherwise, so the label
                // follows whichever the run actually wrote to.
                var target = !string.IsNullOrWhiteSpace(summary.FilePath) ? summary.FilePath
                    : !string.IsNullOrWhiteSpace(config.FilePath) ? config.FilePath
                    : $"{config.DestDataSourceName}.{config.DestEntityName}";

                var record = new ImportRunRecord
                {
                    ContextKey = ExportHistoryKey,
                    StartedAt = DateTime.UtcNow - summary.Duration,
                    FinishedAt = DateTime.UtcNow,
                    FinalState = summary.FailedRows == 0 ? ImportState.Completed : ImportState.Faulted,
                    RecordsRead = summary.TotalRows,
                    RecordsWritten = summary.ExportedRows,
                    RecordsBlocked = summary.FailedRows,
                    Summary = $"{config.SourceDataSourceName}.{config.SourceEntityName} → {target} " +
                              $"({config.Format}) — Exported:{summary.ExportedRows} Failed:{summary.FailedRows}",
                };
                await SaveRunAsync(record, "export").ConfigureAwait(true);
            }

            await LoadHistoryAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Persists one run record. Awaited, not fire-and-forget: callers reload the grid from the
        /// same store immediately after, so an unawaited save would race the read and the run that
        /// just finished could be missing from its own history.
        /// </summary>
        private async Task SaveRunAsync(ImportRunRecord record, string kind)
        {
            if (Editor == null) return;
            try
            {
                _historyStore ??= LocalStoreFactory.CreateHistoryStore(Editor);
                if (_historyStore == null) return;
                await _historyStore.SaveRunAsync(record).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                // History is a convenience surface — a failure must not break a run that succeeded.
                Editor?.AddLogMessage("ImportExportLauncher",
                    $"Saving {kind} run history failed: {ex.Message}",
                    DateTime.Now, 0, null, Errors.Warning);
            }
        }

        private async Task LoadHistoryAsync()
        {
            if (Editor == null) return;
            int generation = ++_historyGeneration;
            try
            {
                _historyStore ??= LocalStoreFactory.CreateHistoryStore(Editor);
                if (_historyStore == null) return;

                // Both keys, merged: the store answers one context per call, and the hub's feed spans
                // imports and exports.
                //
                // Awaited rather than .GetAwaiter().GetResult(): this runs from Load, so blocking here
                // froze the UI every time the hub opened. ConfigureAwait(true) returns us to the UI
                // thread, which is why the old InvokeRequired dance is gone.
                var imports = await _historyStore.GetRunsAsync(ImportHistoryKey).ConfigureAwait(true);
                var exports = await _historyStore.GetRunsAsync(ExportHistoryKey).ConfigureAwait(true);

                if (IsDisposed || generation != _historyGeneration) return;

                var runs = (imports ?? Array.Empty<ImportRunRecord>())
                    .Select(r => (Kind: "Import", Run: r))
                    .Concat((exports ?? Array.Empty<ImportRunRecord>()).Select(r => (Kind: "Export", Run: r)))
                    .OrderByDescending(x => x.Run.StartedAt)
                    .ToList();

                PopulateHistoryGrid(runs);
            }
            catch (Exception ex)
            {
                if (generation != _historyGeneration) return;
                // History is a convenience surface — a failure must not break the hub — but report it.
                Editor?.AddLogMessage("ImportExportLauncher",
                    $"Loading run history failed: {ex.Message}",
                    DateTime.Now, 0, null, Errors.Warning);
            }
        }

        private void PopulateHistoryGrid(IReadOnlyList<(string Kind, ImportRunRecord Run)> runs)
        {
            var table = new System.Data.DataTable("History");
            table.Columns.Add("Timestamp", typeof(string));
            table.Columns.Add("Operation", typeof(string));
            table.Columns.Add("Details", typeof(string));
            table.Columns.Add("Rows Written", typeof(long));
            table.Columns.Add("Blocked", typeof(long));
            table.Columns.Add("Status", typeof(string));

            foreach (var (kind, run) in runs.Take(20))
            {
                table.Rows.Add(
                    run.StartedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                    kind,
                    // The source/target, which the fixed hub ContextKey no longer carries.
                    run.Summary ?? string.Empty,
                    run.RecordsWritten,
                    run.RecordsBlocked,
                    run.FinalState.ToString());
            }

            historyGrid.DataSource = table;
        }
    }
}
