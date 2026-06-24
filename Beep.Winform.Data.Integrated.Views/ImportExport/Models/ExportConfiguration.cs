using TheTechIdea.Beep.Report;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Models
{
    public sealed class ExportConfiguration
    {
        public string SourceDataSourceName { get; set; } = string.Empty;
        public string SourceEntityName { get; set; } = string.Empty;
        public string DestDataSourceName { get; set; } = string.Empty;
        public string DestEntityName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public ExportFormat Format { get; set; } = ExportFormat.Csv;
        public List<string> SelectedFields { get; set; } = new();
        public string CsvDelimiter { get; set; } = ",";
        public bool IncludeHeaders { get; set; } = true;
        public string Encoding { get; set; } = "UTF-8";
        public int BatchSize { get; set; } = 1000;
        public List<AppFilter> Filters { get; set; } = new();
    }

    public enum ExportFormat { Csv, Json, Xml }
}
