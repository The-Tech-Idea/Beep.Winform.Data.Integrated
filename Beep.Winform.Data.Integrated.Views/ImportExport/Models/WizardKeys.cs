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
    }

    public enum ImportPurpose { AddOnly, AddOrUpdate, ReplaceAll }

    public enum ExportDestMode { File, DataSource }
}
