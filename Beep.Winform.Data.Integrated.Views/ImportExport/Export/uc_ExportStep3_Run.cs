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

        /// <summary>
        /// Held so the run can publish its ExportRunSummary. The summary is produced inside
        /// StartExportAsync, which the wizard invokes from a button rather than through
        /// IWizardStepContent, so this is the only reference to the context available at that point.
        /// </summary>
        private WizardContext? _context;

        /// <summary>
        /// Rows written so far by the current run. A field rather than a local because cancellation
        /// throws out of the write loop, so the running total has to survive the stack unwind —
        /// otherwise a run cancelled after 30,000 of 50,000 inserts reports zero rows exported while
        /// the destination actually holds 30,000.
        /// </summary>
        private int _exportedRows;

        /// <summary>
        /// Designer/parameterless ctor. Must not chain to the IServiceProvider overload with null —
        /// that resolves services off a null provider and throws. For the designer only; the runtime
        /// must construct through the IServiceProvider overload.
        /// </summary>
        public uc_ExportStep3_Run() => InitializeControl();

        public uc_ExportStep3_Run(IServiceProvider services) : base(services) => InitializeControl();

        private void InitializeControl()
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
            _context = context;
            _config = context.GetValue<ExportConfiguration?>(WizardKeys.ExportConfig, null);
        }

        // Nothing to save on leave: StartExportAsync publishes the run summary to the context as
        // soon as the run finishes, because the wizard's OnComplete reads it and the user can finish
        // from here. This used to read that key into an unused local, which did nothing.
        public void OnStepLeave(WizardContext context) { }

        /// <summary>
        /// Publishes a summary for a run that did not complete, so a cancelled or failed export is
        /// recorded in run history rather than vanishing — the successful ones are the least
        /// interesting entries to keep.
        /// </summary>
        private void PublishFailedSummary(System.Diagnostics.Stopwatch sw, string reason)
        {
            sw.Stop();
            _context?.SetValue(WizardKeys.ExportRunSummary, new ExportRunSummary
            {
                // Reports the rows that actually landed before the run stopped, not zero. A cancel
                // throws out of the write loop, so without the surviving _exportedRows count this
                // said "Exported: 0" for a destination that really did receive them — leaving the
                // operator to reconcile a partial write against a record claiming nothing happened.
                TotalRows = _exportedRows,
                ExportedRows = _exportedRows,
                // FailedRows > 0 is what marks the record Faulted for the history grid; 1 stands for
                // "this run did not finish", not for a row tally.
                FailedRows = 1,
                Duration = sw.Elapsed,
                FilePath = _config?.FilePath ?? string.Empty,
            });
            AppendLog($"Recorded an incomplete export run ({reason}) — {_exportedRows} row(s) had already been written.");
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
            _exportedRows = 0;
            _cts = new CancellationTokenSource();

            var sw = System.Diagnostics.Stopwatch.StartNew();
            AppendLog("Starting export...");
            progressBar.Value = 0;

            try
            {
                var ds = Editor.GetDataSource(_config.SourceDataSourceName);
                // Thrown rather than logged-and-returned: a bare return fell through to the success
                // summary below, which now reaches run history — so a resolve failure would have
                // been recorded as a completed export.
                if (ds == null)
                    throw new InvalidOperationException($"Source data source '{_config.SourceDataSourceName}' could not be resolved.");

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

                // Tracks rows as the writer reports them, so a cancelled file export can still say
                // how far it got — the file on disk is partial either way.
                var progress = new Progress<int>(rows =>
                {
                    _exportedRows = rows;
                    progressBar.Value = Math.Min(100, (rows * 100 / Math.Max(1, table.Rows.Count)));
                });

                // Every branch must either write or throw. Previously, a config with neither a file
                // path nor a destination datasource matched no branch at all and fell straight
                // through to the success summary — reporting an export that never wrote a row.
                int exportedRows;
                if (!string.IsNullOrEmpty(_config.FilePath))
                {
                    await ExportToFileAsync(table, progress);
                    exportedRows = table.Rows.Count;
                }
                else if (!string.IsNullOrEmpty(_config.DestDataSourceName))
                {
                    exportedRows = await ExportToDataSourceAsync(table);
                }
                else
                {
                    throw new InvalidOperationException(
                        "No export destination is configured — set a file path or a destination data source.");
                }

                sw.Stop();
                var summary = new ExportRunSummary
                {
                    TotalRows = table.Rows.Count,
                    // What was actually written, not what was read. For a datasource export these
                    // differ whenever a row fails to insert.
                    ExportedRows = exportedRows,
                    FailedRows = table.Rows.Count - exportedRows,
                    Duration = sw.Elapsed,
                    FilePath = _config.FilePath,
                };

                // Publish it. The summary used to be built here and then only formatted into a label,
                // never written to the context — so OnStepLeave and the wizard's OnComplete both read
                // this key back as null, and an export could not be recorded to run history at all.
                _context?.SetValue(WizardKeys.ExportRunSummary, summary);

                var target = string.IsNullOrWhiteSpace(_config.FilePath)
                    ? $"{_config.DestDataSourceName}.{_config.DestEntityName}"
                    : _config.FilePath;
                lblResult.Text = $"Exported {summary.ExportedRows:N0} rows in {summary.Duration.TotalSeconds:F1}s ({summary.RowsPerSecond:F0} rows/s)";
                AppendLog($"Export completed: {summary.ExportedRows} rows → {target}");
                ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(true));
            }
            catch (OperationCanceledException)
            {
                AppendLog("Export cancelled.");
                PublishFailedSummary(sw, "cancelled");
            }
            catch (Exception ex)
            {
                AppendLog($"Export error: {ex.Message}");
                PublishFailedSummary(sw, ex.Message);
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

        /// <summary>Writes the table to the destination datasource and returns the rows written.</summary>
        /// <remarks>
        /// Throws rather than logging-and-returning when the destination cannot be resolved: the
        /// caller builds the run summary from the return value and a silent return read as "wrote
        /// every row" — recording a completed export that never happened.
        /// </remarks>
        private async Task<int> ExportToDataSourceAsync(DataTable table)
        {
            if (Editor == null || _config == null)
                throw new InvalidOperationException("Export services are not available.");
            var destDs = Editor.GetDataSource(_config.DestDataSourceName);
            if (destDs == null)
                throw new InvalidOperationException($"Destination data source '{_config.DestDataSourceName}' could not be resolved.");

            int failed = 0;
            int attempted = 0;
            foreach (DataRow row in table.Rows)
            {
                _cts?.Token.ThrowIfCancellationRequested();
                var record = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                    record[col.ColumnName] = row[col] == DBNull.Value ? null! : row[col];

                // InsertEntity reports failure through IErrorsInfo — it does NOT throw — so the
                // result has to be inspected. Counting every call as written made FailedRows always
                // zero and reported a run where every insert was rejected as fully Completed.
                var result = await Task.Run(() => destDs.InsertEntity(_config.DestEntityName, record), _cts!.Token);
                attempted++;

                if (result != null && result.Flag != Errors.Ok)
                {
                    failed++;
                    // Only the first few, so a wholly-rejected table cannot flood the log.
                    if (failed <= 10)
                        AppendLog($"Row {attempted} rejected: {result.Message}");
                    else if (failed == 11)
                        AppendLog("Further row rejections suppressed — see the run summary for the total.");
                }
                else
                {
                    _exportedRows++;
                }

                if (attempted % 100 == 0)
                {
                    progressBar.Value = Math.Min(100, attempted * 100 / Math.Max(1, table.Rows.Count));
                    AppendLog($"Processed {attempted}/{table.Rows.Count} rows ({_exportedRows} written, {failed} rejected)...");
                }
            }
            progressBar.Value = 100;
            return _exportedRows;
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
