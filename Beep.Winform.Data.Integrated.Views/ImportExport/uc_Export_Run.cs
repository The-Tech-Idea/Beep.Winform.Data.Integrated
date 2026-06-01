using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Editor.Importing;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    public partial class uc_Export_Run : TemplateUserControl, IWizardStepContent, IDisposable
    {
        private ExportConfiguration _config;
        private CancellationTokenSource? _cts;
        private bool _isRunning;
        private ExportRunSummary? _lastSummary;
        private bool _disposed;

        public uc_Export_Run(IServiceProvider services, ExportConfiguration config) : base(services)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            InitializeComponent();
            beepButton_Run.Click += RunButton_Click;
            beepButton_Cancel.Click += CancelButton_Click;
            SetRunningState(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cts?.Cancel();
                    _cts?.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;
        public bool IsComplete => beepCheckBoxLastRun?.CurrentValue == true;
        public string NextButtonText => "Finish";

        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            base.OnNavigatedTo(parameters);
            RaiseValidationState();
        }

        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);
            RaiseValidationState();
        }

        public void OnStepEnter(WizardContext context)
        {
            var config = context.GetValue<ExportConfiguration?>(ExportWizardKeys.ExportConfig, null);
            if (config != null) _config = config;
            if (beepCheckBoxLastRun != null)
                beepCheckBoxLastRun.CurrentValue = false;
            rtbLog?.Clear();
            SetRunningState(false);
            RenderSummary();
            RaiseValidationState();
        }

        public void OnStepLeave(WizardContext context)
        {
            context.SetValue(ExportWizardKeys.LastRunSucceeded, beepCheckBoxLastRun.CurrentValue);
            if (_lastSummary != null)
                context.SetValue(ExportWizardKeys.RunSummary, _lastSummary);
        }

        WizardValidationResult IWizardStepContent.Validate() => ValidateStep();
        public Task<WizardValidationResult> ValidateAsync() => Task.FromResult(ValidateStep());

        private void RunButton_Click(object? sender, EventArgs e)
        {
            _ = ExecuteExportAsyncWithErrorHandling();
        }

        private async Task ExecuteExportAsyncWithErrorHandling()
        {
            try
            {
                await ExecuteExportAsync();
            }
            catch (Exception ex)
            {
                AppendLog($"Unexpected error: {ex.Message}");
                Editor?.AddLogMessage("ExportWizard", $"Unexpected error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                SetRunningState(false);
                RaiseValidationState();
            }
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            _cts?.Cancel();
            AppendLog("Export cancelled.");
            SetRunningState(false);
        }

        private async Task ExecuteExportAsync()
        {
            if (Editor == null) { AppendLog("Editor not available."); return; }
            if (string.IsNullOrWhiteSpace(_config.FilePath) && string.IsNullOrWhiteSpace(_config.DestDataSourceName))
            {
                AppendLog("No destination specified.");
                return;
            }

            // Ensure output directory exists for file export
            if (!string.IsNullOrWhiteSpace(_config.FilePath))
            {
                var dir = Path.GetDirectoryName(_config.FilePath);
                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                {
                    try
                    {
                        Directory.CreateDirectory(dir);
                    }
                    catch (Exception ex)
                    {
                        AppendLog($"Error creating directory: {ex.Message}");
                        return;
                    }
                }
            }

            SetRunningState(true);
            beepCheckBoxLastRun.CurrentValue = false;
            rtbLog.Clear();
            _cts = new CancellationTokenSource();

            var startTime = DateTime.UtcNow;
            int exported = 0;
            int failed = 0;
            bool success = false;

            try
            {
                AppendLog("Starting export…");
                var ds = Editor.GetDataSource(_config.SourceDataSourceName);
                if (ds == null) { AppendLog("Source data source not found."); return; }
                if (ds.ConnectionStatus != System.Data.ConnectionState.Open) ds.Openconnection();

                // Fetch data
                var data = await Task.Run(() => ds.GetEntity(_config.SourceEntityName, _config.Filters));
                var table = ConvertToDataTable(data);
                int totalRows = table?.Rows.Count ?? 0;
                AppendLog($"Loaded {totalRows:N0} rows from source.");

                if (totalRows == 0)
                {
                    AppendLog("No data to export.");
                    return;
                }

                // Filter columns if specified
                var columns = _config.SelectedFields?.Count > 0
                    ? _config.SelectedFields
                    : table?.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList() ?? new List<string>();

                // Export based on format
                if (!string.IsNullOrWhiteSpace(_config.FilePath))
                {
                    switch (_config.Format)
                    {
                        case ExportFormat.Csv:
                            await ExportToCsvAsync(table, columns, totalRows);
                            break;
                        case ExportFormat.Json:
                            await ExportToJsonAsync(table, columns, totalRows);
                            break;
                        case ExportFormat.Xml:
                            await ExportToXmlAsync(table, columns, totalRows);
                            break;
                        default:
                            AppendLog($"Format {_config.Format} not yet supported.");
                            return;
                    }
                    exported = totalRows;
                    success = true;
                }
                else if (!string.IsNullOrWhiteSpace(_config.DestDataSourceName))
                {
                    exported = await ExportToDataSourceAsync(table, columns, totalRows);
                    success = exported > 0;
                }

                var elapsed = DateTime.UtcNow - startTime;
                _lastSummary = new ExportRunSummary
                {
                    TotalRows = totalRows,
                    ExportedRows = exported,
                    FailedRows = failed,
                    Duration = elapsed,
                    FilePath = _config.FilePath
                };

                if (success)
                {
                    beepCheckBoxLastRun.CurrentValue = true;
                    AppendLog($"Export complete: {exported:N0} rows in {elapsed:mm\\:ss} ({_lastSummary.RowsPerSecond:N1} rows/sec)");
                }
            }
            catch (OperationCanceledException)
            {
                AppendLog("Export was cancelled.");
            }
            catch (Exception ex)
            {
                AppendLog($"Export error: {ex.Message}");
                Editor?.AddLogMessage("ExportWizard", ex.Message, DateTime.Now, 0, null, Errors.Failed);
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                SetRunningState(false);
                UpdateProgress(exported, exported > 0 ? exported : 1); // Show 100% or final state
                RaiseValidationState();
            }
        }

        private async Task ExportToCsvAsync(DataTable table, List<string> columns, int totalRows)
        {
            var sb = new StringBuilder();
            if (_config.IncludeHeaders)
            {
                sb.AppendLine(string.Join(_config.CsvDelimiter, columns));
            }

            int processed = 0;
            foreach (DataRow row in table.Rows)
            {
                _cts?.Token.ThrowIfCancellationRequested();
                var values = columns.Select(c =>
                {
                    var val = row[c]?.ToString() ?? string.Empty;
                    // Escape quotes and wrap in quotes if contains delimiter or quotes
                    if (val.Contains(_config.CsvDelimiter) || val.Contains('"') || val.Contains('\n'))
                        val = $"\"{val.Replace("\"", "\"\"")}\"";
                    return val;
                });
                sb.AppendLine(string.Join(_config.CsvDelimiter, values));

                processed++;
                if (processed % 100 == 0)
                {
                    UpdateProgress(processed, totalRows);
                    await Task.Yield();
                }
            }

            var encoding = GetSafeEncoding(_config.Encoding);
            await File.WriteAllTextAsync(_config.FilePath, sb.ToString(), encoding, _cts?.Token ?? default);
            UpdateProgress(totalRows, totalRows); // Ensure 100% progress
            AppendLog($"CSV exported to: {_config.FilePath}");
        }

        private async Task ExportToJsonAsync(DataTable table, List<string> columns, int totalRows)
        {
            var records = new List<Dictionary<string, object>>();
            int processed = 0;
            foreach (DataRow row in table.Rows)
            {
                _cts?.Token.ThrowIfCancellationRequested();
                var record = new Dictionary<string, object>();
                foreach (var col in columns)
                    record[col] = row[col] == DBNull.Value ? null : row[col];
                records.Add(record);

                processed++;
                if (processed % 100 == 0)
                {
                    UpdateProgress(processed, totalRows);
                    await Task.Yield();
                }
            }

            var json = JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_config.FilePath, json, _cts?.Token ?? default);
            UpdateProgress(totalRows, totalRows); // Ensure 100% progress
            AppendLog($"JSON exported to: {_config.FilePath}");
        }

        private async Task ExportToXmlAsync(DataTable table, List<string> columns, int totalRows)
        {
            var root = new XElement("Export",
                new XAttribute("Source", _config.SourceEntityName),
                new XAttribute("Date", DateTime.UtcNow.ToString("O")));

            int processed = 0;
            foreach (DataRow row in table.Rows)
            {
                _cts?.Token.ThrowIfCancellationRequested();
                var record = new XElement("Record");
                foreach (var col in columns)
                {
                    var val = row[col] == DBNull.Value ? null : row[col]?.ToString();
                    record.Add(new XElement(col, val));
                }
                root.Add(record);

                processed++;
                if (processed % 100 == 0)
                {
                    UpdateProgress(processed, totalRows);
                    await Task.Yield();
                }
            }

            await File.WriteAllTextAsync(_config.FilePath, root.ToString(), _cts?.Token ?? default);
            UpdateProgress(totalRows, totalRows); // Ensure 100% progress
            AppendLog($"XML exported to: {_config.FilePath}");
        }

        private async Task<int> ExportToDataSourceAsync(DataTable table, List<string> columns, int totalRows)
        {
            var destDs = Editor?.GetDataSource(_config.DestDataSourceName);
            if (destDs == null) { AppendLog("Destination data source not found."); return 0; }
            if (destDs.ConnectionStatus != System.Data.ConnectionState.Open) destDs.Openconnection();

            // Check if destination entity exists, create if needed
            var destStructure = destDs.GetEntityStructure(_config.DestEntityName, false);
            if (destStructure == null)
            {
                AppendLog($"Destination entity '{_config.DestEntityName}' not found.");
                return 0;
            }

            int processed = 0;
            var errors = new List<string>();
            foreach (DataRow row in table.Rows)
            {
                _cts?.Token.ThrowIfCancellationRequested();
                
                try
                {
                    // Create a dictionary from the row data
                    var record = new Dictionary<string, object>();
                    foreach (var col in columns)
                    {
                        if (row.Table.Columns.Contains(col))
                            record[col] = row[col] == DBNull.Value ? null : row[col];
                    }

                    // Insert the record
                    var result = destDs.InsertEntity(_config.DestEntityName, record);
                    if (result.Flag != Errors.Ok)
                    {
                        errors.Add($"Row {processed}: {result.Message}");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {processed}: {ex.Message}");
                }

                processed++;
                if (processed % 100 == 0)
                {
                    UpdateProgress(processed, totalRows);
                    await Task.Yield();
                }
            }

            if (errors.Count > 0)
            {
                AppendLog($"Export completed with {errors.Count} errors:");
                foreach (var error in errors.Take(10))
                    AppendLog($"  {error}");
            }
            else
            {
                AppendLog($"Exported {processed:N0} rows to {_config.DestDataSourceName}.{_config.DestEntityName}");
            }
            
            UpdateProgress(totalRows, totalRows); // Ensure 100% progress
            return processed;
        }

        private void UpdateProgress(int processed, int totalRows)
        {
            if (statusProgressBar != null)
                statusProgressBar.Value = totalRows > 0 ? Math.Min(100, (int)((double)processed / totalRows * 100)) : 0;
            if (statusLabelRows != null)
                statusLabelRows.Text = $"Rows: {processed:N0} / {totalRows:N0}";
        }

        private static Encoding GetSafeEncoding(string encodingName)
        {
            try
            {
                return Encoding.GetEncoding(encodingName);
            }
            catch
            {
                return Encoding.UTF8;
            }
        }

        private DataTable ConvertToDataTable(object data)
        {
            if (data is DataTable dt) return dt;
            if (data is IEnumerable<object> enumerable)
            {
                var table = new DataTable();
                var first = enumerable.FirstOrDefault();
                if (first == null) return table;

                var props = first.GetType().GetProperties();
                foreach (var prop in props)
                    table.Columns.Add(prop.Name, typeof(string));

                foreach (var item in enumerable)
                {
                    var row = table.NewRow();
                    foreach (var prop in props)
                        row[prop.Name] = prop.GetValue(item)?.ToString() ?? string.Empty;
                    table.Rows.Add(row);
                }
                return table;
            }
            return new DataTable();
        }

        private void RenderSummary()
        {
            if (txtSummary == null) return;
            var sb = new StringBuilder();
            sb.AppendLine("─── Export Configuration Summary ───────────────────────────");
            sb.AppendLine();
            sb.AppendLine($"  Source      : {_config.SourceDataSourceName} / {_config.SourceEntityName}");
            if (!string.IsNullOrWhiteSpace(_config.FilePath))
                sb.AppendLine($"  Destination : File ({_config.Format}) - {_config.FilePath}");
            else
                sb.AppendLine($"  Destination : {_config.DestDataSourceName} / {_config.DestEntityName}");
            sb.AppendLine($"  Columns     : {_config.SelectedFields?.Count ?? 0} selected");
            sb.AppendLine($"  Format      : {_config.Format}");
            sb.AppendLine($"  Batch size  : {_config.BatchSize:N0}");
            sb.AppendLine("──────────────────────────────────────────────────────────");
            txtSummary.Text = sb.ToString();
        }

        private void AppendLog(string message)
        {
            if (!string.IsNullOrWhiteSpace(message) && rtbLog != null)
                rtbLog.AppendText(message + Environment.NewLine);
        }

        private void SetRunningState(bool running)
        {
            _isRunning = running;
            beepButton_Run.Enabled = !running;
            beepButton_Cancel.Enabled = running;
        }

        private WizardValidationResult ValidateStep()
        {
            if (beepCheckBoxLastRun == null || !beepCheckBoxLastRun.CurrentValue)
                return WizardValidationResult.Error("Run the export first.");
            return WizardValidationResult.Success();
        }

        private void RaiseValidationState()
        {
            var result = ValidateStep();
            ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(result.IsValid, result.ErrorMessage));
        }
    }
}
