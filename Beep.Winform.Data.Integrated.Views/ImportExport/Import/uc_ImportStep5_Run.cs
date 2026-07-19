using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport.Models;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Import
{
    public partial class uc_ImportStep5_Run : TemplateUserControl, IWizardStepContent, IDisposable
    {
        private DataImportConfiguration? _config;
        private DataImportManager? _importManager;
        private CancellationTokenSource? _cts;
        private bool _isRunning;
        private ImportRunSummary? _lastSummary;
        private readonly List<ImportRowError> _errorRows = new();
        private Timer? _statusTimer;
        private bool _disposed;

        /// <summary>
        /// Last state and pause flag the poll actually reported, so the 500ms tick logs a
        /// TRANSITION rather than the current condition. Logging the condition would append
        /// "Paused." twice a second for as long as the run stays paused.
        /// </summary>
        private ImportState _lastPolledState = ImportState.Idle;
        private bool _lastPolledPaused;

        /// <summary>
        /// Designer/parameterless ctor. Must not chain to the IServiceProvider overload with null —
        /// that resolves services off a null provider and throws.
        /// </summary>
        public uc_ImportStep5_Run() => InitializeControl();

        public uc_ImportStep5_Run(IServiceProvider services) : base(services) => InitializeControl();

        // No ApplyDpiScaledLayout here, deliberately. The sibling views apply BeepLayoutMetrics
        // tokens because their Designers dock their buttons; this one positions the button row
        // absolutely (x=130/210/300, widths 70/80/80), so pushing a MinimumSize token onto each
        // button GROWS it into its neighbour — btnPause would overlap btnResume, and a click near
        // btnPause's right edge would land on Resume. The token pass would also be a no-op even
        // where it fit: DpiScalingHelper returns a factor of 1.0 until the handle exists, and this
        // runs from the constructor. Making this view DPI-aware means converting the Designer's
        // absolute layout to a FlowLayoutPanel first, which is a separate change.
        private void InitializeControl()
        {
            InitializeComponent();
            btnStart.Click += (_, _) => _ = StartImportAsync();
            btnPause.Click += (_, _) => _importManager?.PauseImport();
            btnResume.Click += (_, _) => _importManager?.ResumeImport();
            btnCancel.Click += (_, _) => CancelImport();
            btnExportErrors.Click += (_, _) => _ = ExportErrorsToCsvAsync();
        }

        public bool IsComplete => _lastSummary != null;
        public string NextButtonText => "Finish";
        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;

        public void OnStepEnter(WizardContext context)
        {
            _config = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null);
            if (_config == null) return;
            RenderSummaryCard();

            if (context.GetValue<bool>(WizardKeys.LastRunSucceeded))
            {
                _lastSummary = context.GetValue<ImportRunSummary?>(WizardKeys.RunSummary, null);
                if (_lastSummary != null)
                    RenderRunSummary();
            }
        }

        public void OnStepLeave(WizardContext context)
        {
            context.SetValue(WizardKeys.RunSummary, _lastSummary);
            context.SetValue(WizardKeys.LastRunSucceeded, _lastSummary?.FailedRows == 0);
        }

        public WizardValidationResult Validate()
        {
            if (_lastSummary == null)
                return WizardValidationResult.Error("Import has not been run yet.");
            return WizardValidationResult.Success();
        }

        public Task<WizardValidationResult> ValidateAsync() => Task.FromResult(Validate());

        private void RenderSummaryCard()
        {
            if (_config == null) return;
            var ruleCount = _config.QualityRules?.Count ?? 0;
            var mappedFields = _config.Mapping?.MappedEntities?
                .SelectMany(d => d.FieldMapping ?? new List<Mapping_rep_fields>())
                .Count() ?? 0;

            lblSummary.Text = $"Source: {_config.SourceDataSourceName}.{_config.SourceEntityName}\r\n" +
                              $"Destination: {_config.DestDataSourceName}.{_config.DestEntityName}\r\n" +
                              $"Batch Size: {_config.BatchSize}  |  Sync Mode: {_config.SyncMode}\r\n" +
                              $"Mapped Fields: {mappedFields}  |  Quality Rules: {ruleCount}\r\n" +
                              $"Drift Policy: {_config.DriftPolicy}  |  Preflight: {_config.RunMigrationPreflight}";
        }

        private async Task StartImportAsync()
        {
            if (_config == null || Editor == null || _isRunning) return;

            _isRunning = true;
            _errorRows.Clear();
            errorGrid.Rows.Clear();
            btnStart.Enabled = false;
            btnPause.Enabled = true;
            btnCancel.Enabled = true;
            progressBar.Value = 0;
            rtbLog.Clear();

            // Each run gets a fresh CTS, manager and timer, so the previous ones must go first —
            // they were simply overwritten. The manager is the one that actually leaked: it owns a
            // ManualResetEventSlim and was never disposed per run. (A stopped WinForms Timer holds
            // no native handle and would have been collected; disposing it is correctness, not a
            // leak fix.)
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _importManager?.Dispose();
            _importManager = new DataImportManager(Editor);

            _lastPolledState = ImportState.Idle;
            _lastPolledPaused = false;

            var progress = new Progress<IPassedArgs>(OnProgress);
            _statusTimer?.Dispose();
            _statusTimer = new Timer { Interval = 500 };
            _statusTimer.Tick += (_, _) => OnStatusTick();
            _statusTimer.Start();

            var sw = System.Diagnostics.Stopwatch.StartNew();
            AppendLog("Starting import...");
            try
            {
                var result = await _importManager.RunImportAsync(_config, progress, _cts.Token)
                    .ConfigureAwait(true);
                sw.Stop();

                _lastSummary = new ImportRunSummary
                {
                    Duration = sw.Elapsed,
                    Errors = _errorRows.ToList(),
                };
                ExtractSummaryFromStatus();
                RenderRunSummary();
                AppendLog($"Import completed: {result.Flag} — {result.Message}");
            }
            catch (OperationCanceledException)
            {
                sw.Stop();
                AppendLog("Import cancelled by user.");
                _lastSummary = new ImportRunSummary
                {
                    Duration = sw.Elapsed,
                    Errors = _errorRows.ToList(),
                };
            }
            catch (Exception ex)
            {
                sw.Stop();
                AppendLog($"Import error: {ex.Message}");
            }
            finally
            {
                // One last poll before the timer dies, so the terminal state and final counts land
                // even if the run finished between ticks.
                _statusTimer?.Stop();
                OnStatusTick();

                _isRunning = false;
                if (!IsDisposed)
                {
                    btnStart.Enabled = true;
                    btnPause.Enabled = false;
                    btnCancel.Enabled = false;
                }
                ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(_lastSummary != null));
            }
        }

        private void OnProgress(IPassedArgs args)
        {
            if (args == null) return;

            if (!string.IsNullOrEmpty(args.Messege))
            {
                if (args.ParameterInt1 > 0 || args.ParameterString1 != null)
                {
                    var error = new ImportRowError
                    {
                        RowIndex = args.ParameterInt1,
                        ErrorMessage = args.Messege,
                        Field = args.ParameterString1 ?? string.Empty,
                        Value = args.ParameterString2 ?? string.Empty,
                    };
                    _errorRows.Add(error);

                    if (InvokeRequired)
                        Invoke(() => errorGrid.Rows.Add(error.RowIndex, error.Field, error.Value, error.ErrorMessage));
                    else
                        errorGrid.Rows.Add(error.RowIndex, error.Field, error.Value, error.ErrorMessage);
                }

                AppendLog(args.Messege);
            }

            if (args.Progress > 0)
            {
                var pct = Math.Min(100, args.Progress);
                if (InvokeRequired)
                    Invoke(() => progressBar.Value = pct);
                else
                    progressBar.Value = pct;
            }
        }

        /// <summary>
        /// Polls the manager's live status for the counts IProgress does not carry.
        /// </summary>
        /// <remarks>
        /// Logs transitions only. The status flags used to be permanently false — GetImportStatus
        /// built a throwaway object from two never-assigned fields — so logging the raw condition
        /// was silently harmless. It reports the real state now, and "if (status.IsPaused) log"
        /// would append a line twice a second for as long as the run stayed paused.
        /// </remarks>
        private void OnStatusTick()
        {
            if (_importManager == null) return;
            try
            {
                var status = _importManager.GetImportStatus();
                bool terminal = status.State is ImportState.Completed or ImportState.Cancelled or ImportState.Faulted;

                // Pause is reported only while the run is live. PauseImport resets the pause event
                // and nothing sets it back on cancellation, so a run cancelled while paused still
                // reads IsPaused=true afterwards — announcing "Paused" for a run that has already
                // stopped. The state transition below is the honest report at that point.
                if (!terminal && status.IsPaused != _lastPolledPaused)
                {
                    _lastPolledPaused = status.IsPaused;
                    AppendLog(status.IsPaused
                        ? "Paused — the run stops at the next batch boundary."
                        : "Resumed.");
                }

                if (status.State != _lastPolledState)
                {
                    _lastPolledState = status.State;
                    switch (status.State)
                    {
                        case ImportState.Cancelled: AppendLog("Cancelled."); break;
                        case ImportState.Completed: AppendLog("Completed."); progressBar.Value = 100; break;
                        case ImportState.Faulted: AppendLog($"Faulted: {status.LastMessage}"); break;
                    }
                }

                // Progress straight from the tracked counts. Reading the last log entry's
                // RecordNumber was a workaround for a status object that reported nothing.
                if (status.TotalRecords > 0)
                    progressBar.Value = Math.Clamp((int)status.PercentComplete, 0, 100);
            }
            catch (Exception ex)
            {
                // Polling the status must never take down a running import, but a swallowed fault
                // here shows up as a progress bar that silently stops moving.
                Editor?.AddLogMessage("ImportStep5",
                    $"Reading import status failed: {ex.Message}",
                    DateTime.Now, 0, null, Errors.Warning);
            }
        }

        private void ExtractSummaryFromStatus()
        {
            if (_importManager == null || _lastSummary == null) return;
            try
            {
                var status = _importManager.GetImportStatus();
                _lastSummary.TotalRows = status.TotalRecords;
                _lastSummary.FailedRows = _errorRows.Count;
                _lastSummary.SkippedRows = status.RecordsBlocked;

                var logData = _importManager.ImportLogData;
                if (logData != null)
                {
                    _lastSummary.AddedRows = logData.Count(l => l.Level == ImportLogLevel.Success);
                    _lastSummary.FailedRows = Math.Max(_lastSummary.FailedRows, logData.Count(l => l.Level == ImportLogLevel.Error));
                }
            }
            catch (Exception ex)
            {
                // The summary feeds the run-history record. Failing quietly here would persist a
                // wrong row count as if it were real, so report rather than swallow.
                Editor?.AddLogMessage("ImportStep5",
                    $"Extracting import summary failed; counts may be incomplete: {ex.Message}",
                    DateTime.Now, 0, null, Errors.Warning);
            }
        }

        private void RenderRunSummary()
        {
            if (_lastSummary == null) return;
            lblResult.Text = $"Total: {_lastSummary.TotalRows:N0} | " +
                             $"Failed: {_lastSummary.FailedRows:N0} | " +
                             $"Skipped: {_lastSummary.SkippedRows:N0} | " +
                             $"Duration: {_lastSummary.Duration.TotalSeconds:F1}s | " +
                             $"Throughput: {_lastSummary.RowsPerSecond:F0} rows/s";
            btnExportErrors.Enabled = _errorRows.Count > 0;
        }

        /// <summary>
        /// Cancels the run through the token passed to RunImportAsync.
        /// </summary>
        /// <remarks>
        /// Deliberately does NOT call <c>DataImportManager.CancelImport()</c>, which it used to.
        /// That method cancels an <c>_internalCancellationTokenSource</c> that the engine never
        /// assigns (the compiler flags it CS0649), so it is a no-op — calling it implied a second
        /// cancellation path that does not exist. The token below is the only one the batch loop
        /// observes.
        /// </remarks>
        private void CancelImport() => _cts?.Cancel();

        private void AppendLog(string message)
        {
            if (IsDisposed || Disposing) return;
            if (InvokeRequired)
            {
                if (IsHandleCreated)
                    BeginInvoke(() => AppendLog(message));
                return;
            }
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
            rtbLog.ScrollToCaret();
        }

        /// <summary>
        /// Writes the captured error rows to a CSV the user picks.
        /// </summary>
        /// <remarks>
        /// The whole body is inside the try because this is invoked fire-and-forget: anything that
        /// escaped — including a shell fault out of ShowDialog — would be captured into a discarded
        /// Task and observed by nobody, leaving the click looking like it simply did nothing.
        /// </remarks>
        private async Task ExportErrorsToCsvAsync()
        {
            if (_errorRows.Count == 0) return;

            string path = string.Empty;
            try
            {
                using var dlg = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FileName = $"import_errors_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                if (dlg.ShowDialog() != DialogResult.OK) return;
                path = dlg.FileName;

                // Snapshotted: a run can still be appending to _errorRows while this writes.
                var rows = _errorRows.ToList();

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("RowIndex,Field,Value,ErrorMessage");
                foreach (var err in rows)
                    sb.AppendLine($"{err.RowIndex},{CsvQuote(err.Field)},{CsvQuote(err.Value)},{CsvQuote(err.ErrorMessage)}");

                btnExportErrors.Enabled = false;
                // Off the UI thread: a large error set is a real disk write, and it ran inline.
                await File.WriteAllTextAsync(path, sb.ToString()).ConfigureAwait(true);
                if (IsDisposed) return;
                AppendLog($"Exported {rows.Count} error row(s) to {path}");
            }
            catch (Exception ex)
            {
                if (IsDisposed) return;
                AppendLog($"Could not export errors{(path.Length > 0 ? $" to {path}" : "")}: {ex.Message}");
                Editor?.AddLogMessage("ImportStep5",
                    $"Exporting import errors failed: {ex.Message}",
                    DateTime.Now, 0, null, Errors.Warning);
            }
            finally
            {
                if (!IsDisposed) btnExportErrors.Enabled = _errorRows.Count > 0;
            }
        }

        /// <summary>
        /// Quotes a CSV field per RFC 4180 — doubling any embedded quote.
        /// </summary>
        /// <remarks>
        /// The old writer wrapped values in quotes without escaping the ones inside them, so an
        /// error message containing a quote (SQL errors quote identifiers routinely — <c>column
        /// "Id" does not exist</c>) silently produced a broken row that shifted every later column.
        /// </remarks>
        private static string CsvQuote(string? value) =>
            $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cts?.Cancel();
                    _cts?.Dispose();
                    _importManager?.Dispose();
                    _statusTimer?.Dispose();
                    //components?.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}
