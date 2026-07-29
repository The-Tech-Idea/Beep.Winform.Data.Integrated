namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Models
{
    public static class WizardKeys
    {
        public const string ImportConfig       = "ImportConfig";
        public const string Purpose            = "Purpose";
        public const string MatchByField       = "MatchByField";
        public const string UpdateEmptyFields  = "UpdateEmptyFields";
        public const string SelectedColumns    = "SelectedColumns";
        public const string TemplateName       = "TemplateName";
        public const string BatchSize          = "BatchSize";
        public const string DryRunRowCount     = "DryRunRowCount";
        public const string RunValidation      = "RunValidation";
        public const string RunSummary         = "RunSummary";
        public const string LastRunSucceeded   = "LastRunSucceeded";

        public const string ExportConfig       = "ExportConfig";
        public const string ExportSelectedCols = "ExportSelectedCols";
        public const string ExportRunSummary   = "ExportRunSummary";

        // ── run-history context keys ──────────────────────────────────────────
        //
        // IImportRunHistoryStore is strictly per-context: SaveRunAsync files a record under
        // record.ContextKey and GetRunsAsync only ever returns one key's records, with no way to
        // enumerate contexts. The history feed wants the opposite — everything, across entities —
        // so runs are filed under these two fixed keys rather than "{datasource}.{entity}".
        //
        // Shared: uc_DataImportWizard writes import runs, uc_ImportExportLauncher writes export
        // runs and reads both back for its grid.
        public const string ImportHistoryContext = "ImportExportHub.Import";
        public const string ExportHistoryContext = "ImportExportHub.Export";
    }

    public enum ImportPurpose { AddOnly, AddOrUpdate, ReplaceAll }

    public enum ExportDestMode { File, DataSource }
}
