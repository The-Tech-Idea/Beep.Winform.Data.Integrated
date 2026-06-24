using System.Data;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport.Models;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Export
{
    public partial class uc_ExportStep3_Run : TemplateUserControl, IWizardStepContent, IDisposable
    {
        private ExportConfiguration? _config;
        private CancellationTokenSource? _cts;
        private bool _isRunning;
        private bool _disposed;

        public uc_ExportStep3_Run(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            btnStart.Click += (_, _) => _ = StartExportAsync();
            btnCancel.Click += (_, _) => CancelExport();
        }

        public bool IsComplete => !_isRunning && progressBar.Value > 0;
        public string NextButtonText => "Finish";
        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;

        public void OnStepEnter(WizardContext context)
        {
            _config = context.GetValue<ExportConfiguration?>(WizardKeys.ExportConfig, null);
        }

        public void OnStepLeave(WizardContext context)
        {
            var summary = context.GetValue<ExportRunSummary?>(WizardKeys.ExportRunSummary, null);
        }

        public WizardValidationResult Validate()
        {
            if (_isRunning)
                return WizardValidationResult.Error("Export is still running.");
            return WizardValidationResult.Success();
        }

        public Task<WizardValidationResult> ValidateAsync() => Task.FromResult(Validate());

        private async Task StartExportAsync()
        {
            if (_config == null || Editor == null || _isRunning) return;

            _isRunning = true;
            btnStart.Enabled = false;
            btnCancel.Enabled = true;
            _cts = new CancellationTokenSource();

            var sw = System.Diagnostics.Stopwatch.StartNew();
            AppendLog("Starting export...");
            progressBar.Value = 0;

            try
            {
                var ds = Editor.GetDataSource(_config.SourceDataSourceName);
                if (ds == null) { AppendLog("Error: Source data source not found."); return; }

                var data = await Task.Run(() => ds.GetEntity(_config.SourceEntityName, _config.Filters), _cts.Token);
                var table = ExportFormatWriter.ConvertToDataTable(data, _config.SourceEntityName);

                if (_config.SelectedFields?.Count > 0)
                {
                    for (int i = table.Columns.Count - 1; i >= 0; i--)
                    {
                        if (!_config.SelectedFields.Contains(table.Columns[i].ColumnName))
                            table.Columns.RemoveAt(i);
                    }
                }

                var progress = new Progress<int>(rows => { progressBar.Value = Math.Min(100, (rows * 100 / Math.Max(1, table.Rows.Count))); });

                if (!string.IsNullOrEmpty(_config.FilePath))
                {
                    await ExportToFileAsync(table, progress);
                }
                else if (!string.IsNullOrEmpty(_config.DestDataSourceName))
                {
                    await ExportToDataSourceAsync(table);
                }

                sw.Stop();
                var summary = new ExportRunSummary
                {
                    TotalRows = table.Rows.Count,
                    ExportedRows = table.Rows.Count,
                    Duration = sw.Elapsed,
                    FilePath = _config.FilePath,
                };

                lblResult.Text = $"Exported {summary.ExportedRows:N0} rows in {summary.Duration.TotalSeconds:F1}s ({summary.RowsPerSecond:F0} rows/s)";
                AppendLog($"Export completed: {summary.ExportedRows} rows → {_config.FilePath}");
                ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(true));
            }
            catch (OperationCanceledException)
            {
                AppendLog("Export cancelled.");
            }
            catch (Exception ex)
            {
                AppendLog($"Export error: {ex.Message}");
            }
            finally
            {
                sw.Stop();
                _isRunning = false;
                btnStart.Enabled = true;
                btnCancel.Enabled = false;
            }
        }

        private async Task ExportToFileAsync(DataTable table, IProgress<int> progress)
        {
            switch (_config!.Format)
            {
                case ExportFormat.Csv:
                    await ExportFormatWriter.WriteCsvAsync(table, _config.FilePath, _config.CsvDelimiter.FirstOrDefault(), _config.IncludeHeaders, progress, _cts!.Token);
                    break;
                case ExportFormat.Json:
                    await ExportFormatWriter.WriteJsonAsync(table, _config.FilePath, true, progress, _cts!.Token);
                    break;
                case ExportFormat.Xml:
                    await ExportFormatWriter.WriteXmlAsync(table, _config.FilePath, progress, _cts!.Token);
                    break;
            }
        }

        private async Task ExportToDataSourceAsync(DataTable table)
        {
            if (Editor == null || _config == null) return;
            var destDs = Editor.GetDataSource(_config.DestDataSourceName);
            if (destDs == null) { AppendLog("Error: Destination data source not found."); return; }

            int exported = 0;
            foreach (DataRow row in table.Rows)
            {
                _cts?.Token.ThrowIfCancellationRequested();
                var record = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                    record[col.ColumnName] = row[col] == DBNull.Value ? null! : row[col];
                await Task.Run(() => destDs.InsertEntity(_config.DestEntityName, record), _cts!.Token);
                exported++;
                if (exported % 100 == 0)
                {
                    progressBar.Value = Math.Min(100, exported * 100 / Math.Max(1, table.Rows.Count));
                    AppendLog($"Exported {exported}/{table.Rows.Count} rows...");
                }
            }
            progressBar.Value = 100;
        }

        private void CancelExport() => _cts?.Cancel();

        private void AppendLog(string message)
        {
            if (InvokeRequired) { Invoke(() => AppendLog(message)); return; }
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
            rtbLog.ScrollToCaret();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cts?.Cancel();
                    _cts?.Dispose();
                    components?.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}
