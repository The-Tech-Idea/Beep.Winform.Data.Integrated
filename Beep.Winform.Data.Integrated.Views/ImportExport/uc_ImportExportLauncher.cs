using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Editor.Importing;
using TheTechIdea.Beep.Editor.Importing.Factories;
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
        private readonly IServiceProvider _services;
        private IImportRunHistoryStore? _historyStore;

        public uc_ImportExportLauncher(IServiceProvider services) : base(services)
        {
            _services = services;
            InitializeComponent();
            btnImport.Click += (_, _) => LaunchImportWizard();
            btnExport.Click += (_, _) => LaunchExportWizard();
            btnRefreshHistory.Click += (_, _) => LoadHistory();
            Load += (_, _) => LoadHistory();
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

            config.OnComplete = ctx => OnImportComplete(ctx);

            var instance = WizardManager.CreateWizard(config);
            instance.ShowDialog(this);
        }

        private void LaunchExportWizard()
        {
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

            var instance = WizardManager.CreateWizard(config);
            instance.ShowDialog(this);
        }

        private void OnImportComplete(WizardContext context)
        {
            var config = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null);
            var summary = context.GetValue<ImportRunSummary?>(WizardKeys.RunSummary, null);

            if (config != null && Editor != null)
            {
                _historyStore ??= LocalStoreFactory.CreateHistoryStore(Editor);
                if (_historyStore != null && summary != null)
                {
                    var contextKey = $"{config.SourceDataSourceName}.{config.SourceEntityName}";
                    var record = new ImportRunRecord
                    {
                        ContextKey = contextKey,
                        StartedAt = DateTime.UtcNow - summary.Duration,
                        FinishedAt = DateTime.UtcNow,
                        FinalState = summary.FailedRows == 0 ? ImportState.Completed : ImportState.Faulted,
                        SyncMode = config.SyncMode,
                        RecordsRead = summary.TotalRows,
                        RecordsWritten = summary.TotalRows - summary.FailedRows,
                        RecordsBlocked = summary.FailedRows,
                        Summary = $"Added:{summary.AddedRows} Updated:{summary.UpdatedRows} Failed:{summary.FailedRows}",
                    };
                    _ = _historyStore.SaveRunAsync(record);
                }
            }

            LoadHistory();
        }

        private void LoadHistory()
        {
            if (Editor == null) return;
            try
            {
                _historyStore ??= LocalStoreFactory.CreateHistoryStore(Editor);
                var runs = _historyStore.GetRunsAsync("import").GetAwaiter().GetResult();
                if (InvokeRequired)
                    Invoke(() => PopulateHistoryGrid(runs));
                else
                    PopulateHistoryGrid(runs);
            }
            catch { }
        }

        private void PopulateHistoryGrid(IReadOnlyList<ImportRunRecord> runs)
        {
            var table = new System.Data.DataTable("History");
            table.Columns.Add("Timestamp", typeof(string));
            table.Columns.Add("Source", typeof(string));
            table.Columns.Add("Rows Written", typeof(long));
            table.Columns.Add("Blocked", typeof(long));
            table.Columns.Add("Status", typeof(string));

            foreach (var run in runs.Take(20))
            {
                table.Rows.Add(
                    run.StartedAt.ToString("yyyy-MM-dd HH:mm"),
                    run.ContextKey,
                    run.RecordsWritten,
                    run.RecordsBlocked,
                    run.FinalState.ToString());
            }

            historyGrid.DataSource = table;
        }
    }
}
