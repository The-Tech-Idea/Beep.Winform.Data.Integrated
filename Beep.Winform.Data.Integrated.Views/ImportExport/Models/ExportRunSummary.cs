namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Models
{
    public sealed class ExportRunSummary
    {
        public int TotalRows { get; set; }
        public int ExportedRows { get; set; }
        public int FailedRows { get; set; }
        public TimeSpan Duration { get; set; }
        public double RowsPerSecond => Duration.TotalSeconds > 0 ? TotalRows / Duration.TotalSeconds : 0;
        public string FilePath { get; set; } = string.Empty;
    }
}
