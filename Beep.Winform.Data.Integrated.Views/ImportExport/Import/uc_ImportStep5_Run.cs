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

        public uc_ImportStep5_Run(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            btnStart.Click += (_, _) => _ = StartImportAsync();
            btnPause.Click += (_, _) => _importManager?.PauseImport();
            btnResume.Click += (_, _) => _importManager?.ResumeImport();
            btnCancel.Click += (_, _) => CancelImport();
            btnExportErrors.Click += (_, _) => ExportErrorsToCsv();
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

            _cts = new CancellationTokenSource();
            _importManager = new DataImportManager(Editor);

            var progress = new Progress<IPassedArgs>(OnProgress);
            _statusTimer = new Timer { Interval = 500 };
            _statusTimer.Tick += (_, _) => OnStatusTick();
            _statusTimer.Start();

            var sw = System.Diagnostics.Stopwatch.StartNew();
            AppendLog("Starting import...");
            try
            {
                var result = await _importManager.RunImportAsync(_config, progress, _cts.Token);
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
                _statusTimer?.Stop();
                _isRunning = false;
                btnStart.Enabled = true;
                btnPause.Enabled = false;
                btnCancel.Enabled = false;
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

        private void OnStatusTick()
        {
            if (_importManager == null) return;
            try
            {
                var status = _importManager.GetImportStatus();
                if (status.IsPaused) AppendLog("Paused.");
                if (status.IsCancelled) AppendLog("Cancelled.");
                if (status.IsCompleted && _isRunning)
                {
                    AppendLog("Completed.");
                    progressBar.Value = 100;
                }

                var logData = _importManager.ImportLogData;
                if (logData != null && logData.Count > 0)
                {
                    var last = logData.Last();
                    if (last != null && last.RecordNumber > 0)
                    {
                        var pct = status.TotalRecords > 0
                            ? Math.Min(100, (last.RecordNumber * 100) / status.TotalRecords)
                            : 0;
                        if (InvokeRequired)
                            Invoke(() => progressBar.Value = pct);
                        else
                            progressBar.Value = pct;
                    }
                }
            }
            catch { }
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
            catch { }
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

        private void CancelImport()
        {
            _cts?.Cancel();
            _importManager?.CancelImport();
        }

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

        private void ExportErrorsToCsv()
        {
            if (_errorRows.Count == 0) return;
            using var dlg = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv", FileName = $"import_errors_{DateTime.Now:yyyyMMdd_HHmmss}.csv" };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("RowIndex,Field,Value,ErrorMessage");
            foreach (var err in _errorRows)
                sb.AppendLine($"{err.RowIndex},\"{err.Field}\",\"{err.Value}\",\"{err.ErrorMessage}\"");
            File.WriteAllText(dlg.FileName, sb.ToString());
            AppendLog($"Errors exported to {dlg.FileName}");
        }

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
