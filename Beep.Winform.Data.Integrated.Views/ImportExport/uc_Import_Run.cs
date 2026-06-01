using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Editor.Importing;
using TheTechIdea.Beep.Editor.Importing.Interfaces;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    
    public partial class uc_Import_Run : TemplateUserControl, IWizardStepContent, IDisposable
    {
        private DataImportConfiguration? _config;
        private DataImportManager?       _importManager;
        private CancellationTokenSource? _cts;
        private bool _isRunning;
        private ImportRunSummary?        _lastSummary;
        private readonly System.Collections.Generic.List<ImportRowError> _errorRows = new();
        private bool _disposed;

        public uc_Import_Run(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            beepButton_Run.Click    += RunButton_Click;
            beepButton_Pause.Click  += PauseButton_Click;
            beepButton_Resume.Click += ResumeButton_Click;
            beepButton_Cancel.Click += CancelButton_Click;
            statusTimer.Tick        += StatusTimer_Tick;
            btnToggleSummary.Click  += (_, _) => ToggleSummaryCard();
            btnExportErrors.Click   += ExportErrors_Click;
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
                    _importManager?.Dispose();
                    statusTimer?.Stop();
                    statusTimer?.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;
        public bool   IsComplete     => beepCheckBoxLastRun?.CurrentValue == true;
        public string NextButtonText => "Finish";

        public override void OnNavigatedTo(Dictionary<string, object> parameters)  { base.OnNavigatedTo(parameters); RaiseValidationState(); }
        public override void Configure(Dictionary<string, object> settings)        { base.Configure(settings);       RaiseValidationState(); }

        public void OnStepEnter(WizardContext context)
        {
            _config = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null);
            if (_config == null) return;

            // ── Validate auto-create / destination readiness ──
            if (!ImportExportWizardValidation.ValidateAutoCreateSettings(context, out var autoCreateIssues))
            {
                foreach (var issue in autoCreateIssues)
                    AppendLog($"[Pre-flight] {issue}");
            }

            // Restore option checkboxes from config
            cbPreflight.Checked        = _config.RunMigrationPreflight;
            cbAddMissing.Checked       = _config.AddMissingColumns;
            cbSyncDraft.Checked        = _config.CreateSyncProfileDraft;
            beepCheckBoxLastRun.CurrentValue = false;
            beepLogBox.Clear();
            ClearErrorGrid();
            RenderSummaryCard(context);
            SetRunningState(false);
            RaiseValidationState();
        }

        public void OnStepLeave(WizardContext context)
        {
            if (_config == null) return;
            _config.RunMigrationPreflight  = cbPreflight.Checked;
            _config.AddMissingColumns      = cbAddMissing.Checked;
            _config.CreateSyncProfileDraft = cbSyncDraft.Checked;
            context.SetValue(WizardKeys.ImportConfig,      _config);
            context.SetValue(WizardKeys.LastRunSucceeded,  beepCheckBoxLastRun.CurrentValue);
            if (_lastSummary != null)
                context.SetValue(WizardKeys.RunSummary, _lastSummary);
        }

        WizardValidationResult IWizardStepContent.Validate() => ValidateStep();
        public Task<WizardValidationResult> ValidateAsync()  => Task.FromResult(ValidateStep());

        // ── Button handlers ───────────────────────────────────────────────

        private void RunButton_Click(object? sender, EventArgs e)
        {
            if (_config == null) { AppendLog("No import configuration found."); return; }
            _ = ExecuteImportAsyncWithErrorHandling();
        }

        private async Task ExecuteImportAsyncWithErrorHandling()
        {
            try
            {
                await ExecuteImportAsync();
            }
            catch (Exception ex)
            {
                AppendLog($"Unexpected error: {ex.Message}");
                Editor?.AddLogMessage("ImportWizard", $"Unexpected error: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                SetRunningState(false);
                RaiseValidationState();
            }
        }

        private void PauseButton_Click(object? sender, EventArgs e)
        {
            _importManager?.PauseImport();
            AppendLog("Import paused.");
        }

        private void ResumeButton_Click(object? sender, EventArgs e)
        {
            _importManager?.ResumeImport();
            AppendLog("Import resumed.");
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            _cts?.Cancel();
            _importManager?.CancelImport();
            AppendLog("Import cancelled.");
            SetRunningState(false);
        }

        // ── Core execution ────────────────────────────────────────────────

        private async Task ExecuteImportAsync()
        {
            if (_config == null || Editor == null) return;
            _config.RunMigrationPreflight  = cbPreflight.Checked;
            _config.AddMissingColumns      = cbAddMissing.Checked;
            _config.CreateSyncProfileDraft = cbSyncDraft.Checked;

            SetRunningState(true);
            beepCheckBoxLastRun.CurrentValue = false;
            beepLogBox.Clear();
            _cts            = new CancellationTokenSource();
            _importManager  = new DataImportManager(Editor);

            var progress = new Progress<IPassedArgs>(iargs =>
            {
                var args = iargs as PassedArgs ?? new PassedArgs { Messege = iargs?.Messege, ParameterInt1 = iargs?.ParameterInt1 ?? 0 };
                if (!IsDisposed && InvokeRequired)
                    BeginInvoke(() => UpdateProgress(args));
                else
                    UpdateProgress(args);
            });

            try
            {
                // Optional preflight
                if (_config.RunMigrationPreflight)
                {
                    AppendLog("Running migration preflight…");
                    var pre = await _importManager.RunMigrationPreflightAsync(_config, AppendLog);
                    if (pre?.Flag == Errors.Failed)
                    {
                        AppendLog($"Preflight failed: {pre.Message}");
                        SetRunningState(false);
                        return;
                    }
                    AppendLog("Preflight passed.");
                }

                // Validate
                var validation = _importManager.ValidationHelper.ValidateImportConfiguration(_config);
                if (validation?.Flag == Errors.Failed)
                {
                    AppendLog($"Validation failed: {validation.Message}");
                    SetRunningState(false);
                    return;
                }

                // Run
                AppendLog("Starting import…");
                statusTimer.Start();
                var result = await _importManager.RunImportAsync(_config, progress, _cts.Token);
                statusTimer.Stop();

                // Optional sync-draft
                if (_config.CreateSyncProfileDraft && result?.Flag != Errors.Failed)
                {
                    AppendLog("Building sync profile draft…");
                    await _importManager.BuildSyncDraftAsync(_config);
                }

                bool succeeded = result?.Flag != Errors.Failed;
                AppendLog(succeeded ? "Import completed successfully." : $"Import finished with errors: {result?.Message}");
                beepCheckBoxLastRun.CurrentValue = succeeded;

                // Build run summary
                var status = _importManager.GetImportStatus();
                var elapsed = (status?.FinishedAt ?? DateTime.UtcNow) - (status?.StartedAt ?? DateTime.UtcNow);
                _lastSummary = new ImportRunSummary
                {
                    TotalRows  = status?.TotalRecords     ?? 0,
                    AddedRows  = status?.RecordsProcessed ?? 0,
                    Duration   = elapsed,
                    RowsPerSec = elapsed.TotalSeconds > 0
                                     ? ((status?.RecordsProcessed ?? 0) / elapsed.TotalSeconds) : 0,
                    Errors     = new System.Collections.Generic.List<ImportRowError>(_errorRows)
                };
                _lastSummary.FailedRows = _errorRows.Count;
                UpdateSummaryCard(_lastSummary);
                btnExportErrors.Enabled = _errorRows.Count > 0;
            }
            catch (OperationCanceledException)
            {
                AppendLog("Import was cancelled.");
            }
            catch (Exception ex)
            {
                AppendLog($"Unexpected error: {ex.Message}");
                Editor?.AddLogMessage("ImportExport", ex.Message, DateTime.Now, 0, null, Errors.Failed);
            }
            finally
            {
                statusTimer.Stop();
                SetRunningState(false);
                RaiseValidationState();
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private void StatusTimer_Tick(object? sender, EventArgs e)
        {
            if (_importManager == null) return;
            var status = _importManager.GetImportStatus();
            if (status == null) return;
            statusProgressBar.Value  = Math.Min(100, (int)Math.Round(status.PercentComplete));
            var el = (DateTime.UtcNow) - (status.StartedAt ?? DateTime.UtcNow);
            statusLabelRows.Text     = $"Rows: {status.RecordsProcessed:N0} / {status.TotalRecords:N0}";
            statusLabelElapsed.Text  = $"Elapsed: {el:hh\\:mm\\:ss}";
            // Throughput
            double elapsed = el.TotalSeconds;
            if (elapsed > 0 && status.RecordsProcessed > 0)
                lblThroughput.Text = $"{status.RecordsProcessed / elapsed:N0} rows/sec";
            // Batch info
            if (status.TotalRecords > 0)
                lblBatchInfo.Text = $"Batch {status.RecordsProcessed:N0} / {status.TotalRecords:N0}";
            else
                lblBatchInfo.Text = string.Empty;
        }

        private void UpdateProgress(PassedArgs args)
        {
            if (args.ParameterInt1 >= 0 && args.ParameterInt1 <= 100)
                statusProgressBar.Value = args.ParameterInt1;
            if (!string.IsNullOrWhiteSpace(args.Messege))
                AppendLog(args.Messege);
        }

        private void AppendLog(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                beepLogBox.AppendText(message + Environment.NewLine);
        }

        private void SetRunningState(bool running)
        {
            _isRunning                  = running;
            beepButton_Run.Enabled      = !running;
            beepButton_Pause.Enabled    = running;
            beepButton_Resume.Enabled   = running;
            beepButton_Cancel.Enabled   = running;
            cbPreflight.Enabled         = !running;
            cbAddMissing.Enabled        = !running;
            cbSyncDraft.Enabled         = !running;
        }

        private WizardValidationResult ValidateStep()
        {
            if (!beepCheckBoxLastRun.CurrentValue)
                return WizardValidationResult.Error("Run the import first.");
            return WizardValidationResult.Success();
        }

        private void RaiseValidationState()
        {
            var result = ValidateStep();
            ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(result.IsValid, result.ErrorMessage));
        }

        // ── Summary card helpers ────────────────────────────────────────────────

        private void RenderSummaryCard(WizardContext context)
        {
            if (_config == null) { lblSummaryCard.Text = "(configure settings first)"; return; }
            var purpose  = context.GetValue(WizardKeys.Purpose, ImportPurpose.AddOnly).ToString();
            var mapped   = _config.Mapping?.MappedEntities?.Count ?? 0;
            var batchSz  = context.GetValue(WizardKeys.BatchSize, _config.BatchSize);
            lblSummaryCard.Text =
                $"Source: {_config.SourceDataSourceName}/{_config.SourceEntityName}  →  " +
                $"Dest: {_config.DestDataSourceName}/{_config.DestEntityName}\r\n" +
                $"Purpose: {purpose}   Fields mapped: {mapped}   Batch size: {batchSz}";
        }

        private void UpdateSummaryCard(ImportRunSummary s)
        {
            lblSummaryCard.Text += $"\r\nResult: Added {s.AddedRows} | Updated {s.UpdatedRows} | " +
                                   $"Skipped {s.SkippedRows} | Failed {s.FailedRows} | " +
                                   $"Duration: {s.Duration:mm\\:ss} | {s.RowsPerSec:N0} rows/sec";
        }

        private void ToggleSummaryCard()
        {
            bool expanded = summaryCardPanel.Height > 30;
            summaryCardPanel.Height  = expanded ? 28 : 70;
            btnToggleSummary.Text    = expanded ? "▶" : "▼";
        }

        private void ClearErrorGrid()
        {
            _errorRows.Clear();
            errorGrid.Rows.Clear();
            btnExportErrors.Enabled = false;
        }

        private void AddErrorRow(ImportRowError err)
        {
            _errorRows.Add(err);
            errorGrid.Rows.Add(err.RowIndex, err.FieldName, err.Value, err.ErrorMessage);
        }

        private void ExportErrors_Click(object? sender, EventArgs e)
        {
            if (_errorRows.Count == 0) return;
            using var dlg = new SaveFileDialog
            {
                Title      = "Export Errors",
                Filter     = "CSV files (*.csv)|*.csv",
                FileName   = $"import_errors_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };
            if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
            try
            {
                var sb = new StringBuilder("Row,Field,Value,Message\r\n");
                foreach (var err in _errorRows)
                    sb.AppendLine($"{err.RowIndex},\"{err.FieldName}\",\"{err.Value}\",\"{err.ErrorMessage}\"");
                System.IO.File.WriteAllText(dlg.FileName, sb.ToString());
                AppendLog($"Errors exported to: {dlg.FileName}");
            }
            catch (Exception ex)
            {
                AppendLog($"Export failed: {ex.Message}");
            }
        }


    }
}
