namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Models
{
    public sealed class ImportRunSummary
    {
        public int TotalRows { get; set; }
        public int AddedRows { get; set; }
        public int UpdatedRows { get; set; }
        public int SkippedRows { get; set; }
        public int FailedRows { get; set; }
        public TimeSpan Duration { get; set; }
        public double RowsPerSecond => Duration.TotalSeconds > 0 ? TotalRows / Duration.TotalSeconds : 0;
        public List<ImportRowError> Errors { get; set; } = new();
    }

    public sealed class ImportRowError
    {
        public int RowIndex { get; set; }
        public string Field { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
