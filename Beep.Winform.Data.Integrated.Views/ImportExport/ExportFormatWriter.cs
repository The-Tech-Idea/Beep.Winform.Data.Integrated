using System.Data;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    public static class ExportFormatWriter
    {
        public static async Task WriteCsvAsync(DataTable table, string filePath, char delimiter = ',', bool includeHeaders = true, IProgress<int>? progress = null, CancellationToken token = default)
        {
            var sb = new StringBuilder();
            if (includeHeaders)
            {
                var headers = table.Columns.Cast<DataColumn>()
                    .Select(c => EscapeCsv(c.ColumnName, delimiter));
                sb.AppendLine(string.Join(delimiter, headers));
            }

            int rowIdx = 0;
            foreach (DataRow row in table.Rows)
            {
                token.ThrowIfCancellationRequested();
                var cells = row.ItemArray.Select(v => EscapeCsv(v?.ToString() ?? string.Empty, delimiter));
                sb.AppendLine(string.Join(delimiter, cells));
                rowIdx++;
                if (rowIdx % 100 == 0)
                {
                    progress?.Report(rowIdx);
                    await Task.Yield();
                }
            }

            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8, token);
            progress?.Report(table.Rows.Count);
        }

        public static async Task WriteJsonAsync(DataTable table, string filePath, bool indented = true, IProgress<int>? progress = null, CancellationToken token = default)
        {
            var list = new List<Dictionary<string, object?>>();
            foreach (DataRow row in table.Rows)
            {
                token.ThrowIfCancellationRequested();
                var dict = new Dictionary<string, object?>();
                foreach (DataColumn col in table.Columns)
                    dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                list.Add(dict);
            }

            var options = new JsonSerializerOptions { WriteIndented = indented };
            var json = JsonSerializer.Serialize(list, options);
            await File.WriteAllTextAsync(filePath, json, Encoding.UTF8, token);
            progress?.Report(table.Rows.Count);
        }

        public static async Task WriteXmlAsync(DataTable table, string filePath, IProgress<int>? progress = null, CancellationToken token = default)
        {
            var root = new XElement("Export",
                new XAttribute("Source", table.TableName),
                new XAttribute("Date", DateTime.UtcNow.ToString("O")));

            int rowIdx = 0;
            foreach (DataRow row in table.Rows)
            {
                token.ThrowIfCancellationRequested();
                var record = new XElement("Record");
                foreach (DataColumn col in table.Columns)
                    record.Add(new XElement(col.ColumnName, row[col] == DBNull.Value ? string.Empty : row[col]));
                root.Add(record);
                rowIdx++;
                if (rowIdx % 100 == 0)
                {
                    progress?.Report(rowIdx);
                    await Task.Yield();
                }
            }

            await File.WriteAllTextAsync(filePath, root.ToString(), Encoding.UTF8, token);
            progress?.Report(table.Rows.Count);
        }

        public static DataTable ConvertToDataTable(IEnumerable<object> data, string tableName = "Export")
        {
            var table = new DataTable(tableName);
            if (data == null)
                return table;

            var first = data.FirstOrDefault();
            if (first == null)
                return table;

            var props = first.GetType().GetProperties();
            foreach (var p in props)
                table.Columns.Add(p.Name, Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType);

            foreach (var item in data)
            {
                var row = table.NewRow();
                foreach (var p in props)
                    row[p.Name] = p.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }

            return table;
        }

        public static string GetFileFilter(Models.ExportFormat format)
        {
            return format switch
            {
                Models.ExportFormat.Csv => "CSV files (*.csv)|*.csv",
                Models.ExportFormat.Json => "JSON files (*.json)|*.json",
                Models.ExportFormat.Xml => "XML files (*.xml)|*.xml",
                _ => "All files (*.*)|*.*",
            };
        }

        public static string GetExtension(Models.ExportFormat format)
        {
            return format switch
            {
                Models.ExportFormat.Csv => ".csv",
                Models.ExportFormat.Json => ".json",
                Models.ExportFormat.Xml => ".xml",
                _ => ".dat",
            };
        }

        private static string EscapeCsv(string value, char delimiter)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            if (value.Contains(delimiter) || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }
}
