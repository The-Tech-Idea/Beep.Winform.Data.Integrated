using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.BeepSync;
using TheTechIdea.Beep.Editor.Schema;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    [AddinAttribute(Caption = "Sync Wizard", Name = "uc_SyncWizard",
        misc = "Config", menu = "Configuration", addinType = AddinType.Control,
        displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 11, RootNodeName = "Configuration", Order = 11, ID = 11,
        BranchText = "Sync Wizard", BranchType = EnumPointType.Function,
        IconImageName = "sync.svg", BranchClass = "ADDIN",
        BranchDescription = "Configure, validate, and run datasource synchronisation.")]

    public partial class uc_SyncWizard : TemplateUserControl, IAddinVisSchema
    {
        /// <summary>The three stages of the sync lifecycle this wizard walks.</summary>
        private enum Stage { Scope = 0, Preflight = 1, Run = 2 }

        public event EventHandler<WizardCompletedEventArgs>? Completed;

        private Stage _stage = Stage.Scope;
        private bool _busy;
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Held across Preflight and Run so the run reports (<see cref="BeepSyncManager.LastRunCheckpoint"/>,
        /// <see cref="BeepSyncManager.LastRunConflicts"/>, <see cref="BeepSyncManager.LastRunReconciliationReport"/>)
        /// belong to the same manager that produced them. Disposed by the control.
        /// </summary>
        private BeepSyncManager? _manager;

        /// <summary>The schema preflight approved. Run must use this exact instance, not a rebuilt one.</summary>
        private DataSyncSchema? _schema;

        /// <summary>Structural/rules/defaults/mapping verdict from <c>BeepSyncManager.RunPreflightAsync</c>.</summary>
        private SyncPreflightReport? _preflight;

        /// <summary>
        /// Destination-acceptance verdict from <c>SyncSchemaPreflight</c>. This is the gate that
        /// actually blocks a run: <c>SyncDataAsync</c> runs the same check internally and returns
        /// Failed on it before any data moves, so the wizard runs it here to fail in the UI rather
        /// than a third of the way through a run.
        /// </summary>
        private SchemaPreflightResult? _schemaPreflight;

        /// <summary>
        /// Narration collected from SyncSchemaPreflight's log callback. Buffered rather than written
        /// straight to the list: that callback is invoked from inside the helper's own Task.Run, so
        /// touching the control there would be a cross-thread call. Rendered by BindFindings once the
        /// await has resumed on the UI thread.
        /// </summary>
        private readonly List<string> _preflightLog = new();

        /// <summary>
        /// Per-combo generation counters for <see cref="LoadEntitiesAsync"/>. The connection combo
        /// stays live while its entity list is being fetched off-thread, so a user can switch
        /// connections mid-flight and the slower first response would otherwise land last and
        /// repopulate the combo with the wrong connection's entities.
        /// </summary>
        private readonly Dictionary<BeepComboBox, int> _entityLoadGeneration = new();

        /// <summary>
        /// Designer/parameterless ctor. Must not chain to the IServiceProvider overload with null —
        /// that resolves services off a null provider and throws.
        /// </summary>
        public uc_SyncWizard() => InitializeControl();

        public uc_SyncWizard(IServiceProvider services) : base(services) => InitializeControl();

        private void InitializeControl()
        {
            InitializeComponent();
            Details.AddinName = "Sync Wizard";
            WireEvents();
            PopulateConnections();
            UpdateStageUi();
        }

        #region "IAddinVisSchema"
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string RootNodeName { get; set; } = "Configuration";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CatgoryName { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Order { get; set; } = 11;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ID { get; set; } = 11;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchText { get; set; } = "Sync Wizard";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Level { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BranchID { get; set; } = 11;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconImageName { get; set; } = "sync.svg";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchStatus { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ParentBranchID { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchDescription { get; set; } = "Configure, validate, and run datasource synchronisation.";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchClass { get; set; } = "ADDIN";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string AddinName { get; set; } = "uc_SyncWizard";
        #endregion

        private void WireEvents()
        {
            _btnCancel.Click += (_, _) =>
            {
                _cts?.Cancel();
                Completed?.Invoke(this, new WizardCompletedEventArgs { Cancelled = true });
            };
            _btnBack.Click += (_, _) =>
            {
                if (_busy || _stage == Stage.Scope || _stage == Stage.Run) return;
                _stage = (Stage)((int)_stage - 1);
                UpdateStageUi();
            };
            _btnNext.Click += BtnNext_Click;
            // Fire-and-forget is safe here only because LoadEntitiesAsync catches its own failures;
            // it is never awaited, so an escaping exception would go unobserved.
            _cboSourceConn.SelectedItemChanged += (_, _) => _ = LoadEntitiesAsync(_cboSourceConn, _cboSourceEntity);
            _cboDestConn.SelectedItemChanged += (_, _) => _ = LoadEntitiesAsync(_cboDestConn, _cboDestEntity);
            _cboSourceEntity.SelectedItemChanged += (_, _) => InvalidatePreflight();
            _cboDestEntity.SelectedItemChanged += (_, _) => InvalidatePreflight();
        }

        protected override void ApplyDpiScaledLayout()
        {
            _rootPanel.Padding = BeepLayoutMetrics.DialogPadding.ScalePadding(this);
            _contentHost.Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this);
            _actionsPanel.Padding = BeepLayoutMetrics.ButtonStripPd.ScalePadding(this);

            int btnH = BeepLayoutMetrics.ButtonStandard.Height.ScaleValue(this);
            _btnCancel.MinimumSize = new System.Drawing.Size(
                BeepLayoutMetrics.ButtonStandard.Width.ScaleValue(this), btnH);
            _btnBack.MinimumSize = new System.Drawing.Size(
                BeepLayoutMetrics.ButtonStandard.Width.ScaleValue(this), btnH);
            _btnNext.MinimumSize = new System.Drawing.Size(
                BeepLayoutMetrics.ButtonLarge.Width.ScaleValue(this),
                BeepLayoutMetrics.ButtonLarge.Height.ScaleValue(this));
        }

        private void PopulateConnections()
        {
            var connections = beepService?.DMEEditor?.ConfigEditor?.DataConnections;
            if (connections == null || connections.Count == 0)
            {
                SetStatus("No connections are configured.");
                return;
            }

            foreach (var conn in connections.Where(c => !string.IsNullOrWhiteSpace(c?.ConnectionName)))
            {
                var text = $"{conn.ConnectionName} ({conn.DatabaseType})";
                _cboSourceConn.ListItems.Add(new SimpleItem { Text = text, Value = conn.ConnectionName });
                _cboDestConn.ListItems.Add(new SimpleItem { Text = text, Value = conn.ConnectionName });
            }
        }

        /// <summary>
        /// Lists the entities on a connection. Both GetDataSource and GetEntitesList open a
        /// connection and round-trip to the database, so they run off the UI thread.
        /// </summary>
        private async Task LoadEntitiesAsync(BeepComboBox connectionCombo, BeepComboBox entityCombo)
        {
            entityCombo.ListItems.Clear();
            InvalidatePreflight();

            // Claim a generation before awaiting; anything older is a stale response by the time it
            // resumes. Incrementing on every call also cancels an in-flight load whose connection
            // has since been cleared.
            int generation = _entityLoadGeneration.TryGetValue(entityCombo, out var g) ? g + 1 : 1;
            _entityLoadGeneration[entityCombo] = generation;

            var editor = beepService?.DMEEditor;
            var name = connectionCombo.SelectedItem?.Value as string;
            if (editor == null || string.IsNullOrWhiteSpace(name)) { UpdateStageUi(); return; }

            try
            {
                SetStatus($"Listing entities on '{name}'…");
                var entities = await Task.Run(() =>
                    editor.GetDataSource(name)?.GetEntitesList()).ConfigureAwait(true);

                // A newer selection superseded this one while it was in flight — its results are
                // already on screen, so drop these rather than overwrite them.
                if (_entityLoadGeneration[entityCombo] != generation) return;

                if (entities != null)
                {
                    foreach (var entity in entities.Where(e => !string.IsNullOrWhiteSpace(e)).OrderBy(e => e))
                        entityCombo.ListItems.Add(new SimpleItem { Text = entity, Value = entity });
                }

                SetStatus(entities == null || !entities.Any()
                    ? $"'{name}' reported no entities."
                    : $"{entities.Count(e => !string.IsNullOrWhiteSpace(e))} entity(ies) on '{name}'.");
            }
            catch (Exception ex)
            {
                if (_entityLoadGeneration[entityCombo] != generation) return;
                SetStatus($"Could not list entities for '{name}': {ex.Message}");
            }
            UpdateStageUi();
        }

        /// <summary>
        /// Drops a preflight verdict that no longer describes the current selection. Without this a
        /// user could pass preflight, change the destination, and run against the stale approval.
        /// </summary>
        private void InvalidatePreflight()
        {
            _preflight = null;
            _schemaPreflight = null;
            _schema = null;
            UpdateStageUi();
        }

        private string? SourceConn => _cboSourceConn.SelectedItem?.Value as string;
        private string? SourceEntity => _cboSourceEntity.SelectedItem?.Value as string;
        private string? DestConn => _cboDestConn.SelectedItem?.Value as string;
        private string? DestEntity => _cboDestEntity.SelectedItem?.Value as string;

        private async void BtnNext_Click(object? sender, EventArgs e)
        {
            if (_busy) return;

            try
            {
                switch (_stage)
                {
                    case Stage.Scope:
                        if (await RunPreflightStageAsync())
                        {
                            _stage = Stage.Preflight;
                            UpdateStageUi();
                        }
                        break;

                    case Stage.Preflight:
                        _stage = Stage.Run;
                        UpdateStageUi();
                        await RunSyncAsync();
                        break;

                    case Stage.Run:
                        Completed?.Invoke(this, new WizardCompletedEventArgs
                        {
                            Succeeded = true,
                            Summary = _lblRunStatus.Text
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Sync wizard error: {ex.Message}");
            }
        }

        private void UpdateStageUi()
        {
            _stepScope.Visible = _stage == Stage.Scope;
            _stepPreflight.Visible = _stage == Stage.Preflight;
            _stepRun.Visible = _stage == Stage.Run;

            var active = _stage switch
            {
                Stage.Scope => (Control)_stepScope,
                Stage.Preflight => _stepPreflight,
                _ => _stepRun
            };
            active.BringToFront();

            (_lblSubtitle.Text, _btnNext.Text) = _stage switch
            {
                Stage.Scope => ("Step 1 of 3 — choose source, destination, and options.", "Run Preflight"),
                Stage.Preflight => ("Step 2 of 3 — preflight findings. Nothing has moved yet.", "Run Sync"),
                _ => ("Step 3 of 3 — sync progress and result.", "Finish")
            };

            _btnBack.Enabled = !_busy && _stage == Stage.Preflight;
            _btnNext.Enabled = !_busy && CanAdvance();
        }

        /// <summary>Gates Next on the state the current stage actually requires.</summary>
        private bool CanAdvance() => _stage switch
        {
            Stage.Scope => SourceConn != null && SourceEntity != null && DestConn != null && DestEntity != null,

            // Both gates must pass. SyncDataAsync re-runs the destination-acceptance check itself
            // and returns Failed on it, so letting a failed preflight through would only move the
            // same failure later, after the approval UI implied it was safe.
            Stage.Preflight => _schema != null
                               && _preflight?.IsApproved == true
                               && _schemaPreflight?.Status?.Flag != Errors.Failed,

            _ => true
        };

        private DataSyncSchema BuildSchema() => new()
        {
            SourceDataSourceName = SourceConn!,
            SourceEntityName = SourceEntity!,
            DestinationDataSourceName = DestConn!,
            DestinationEntityName = DestEntity!,
            EntityName = SourceEntity!,
            BatchSize = int.TryParse(_txtBatchSize.Text, out var b) && b > 0 ? b : 50,
            CreateDestinationIfNotExists = _chkCreateDestination.Checked,

            // AddMissingColumns is deliberately left false and has no control on the Scope step.
            // Nothing in the sync path can add a column: the import's only schema-mutating step
            // creates whole entities (DataImportManager.EnsureDestinationEntityExists). Setting it
            // true would merely suppress the preflight's missing-column veto and let rows move into
            // a destination that cannot hold them, so the wizard does not offer the choice.

            // Enables the engine's internal rule-engine preflight gate. It only fires when an
            // IntegrationContext with a RuleEngine is wired and RulePolicy.Enabled is true
            // (BeepSyncManager.Sync.cs) — neither of which this wizard configures. Set true anyway
            // so the gate engages for a host that does supply a context; the wizard's own Preflight
            // stage is what the operator actually sees.
            RunPreflight = true
        };

        // ── stage 1 → preflight ───────────────────────────────────────────────

        /// <summary>
        /// Runs both preflight gates without moving any data, and binds their findings.
        /// </summary>
        private async Task<bool> RunPreflightStageAsync()
        {
            var editor = beepService?.DMEEditor;
            if (editor == null)
            {
                SetStatus("IDMEEditor is not available; cannot run preflight.");
                return false;
            }

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            using var busy = BeginBusy("Running preflight…");
            try
            {
                _manager?.Dispose();
                _manager = new BeepSyncManager(editor);
                var schema = BuildSchema();
                _preflightLog.Clear();

                // Destination acceptance: does the destination exist, and will it take every source
                // field? Mirrors the check SyncDataAsync runs internally, using the same schema flags
                // so the verdict shown here is the one the run will get.
                _schemaPreflight = await SyncSchemaPreflight.RunPreflightAsync(editor, new SchemaRequest
                {
                    SourceDataSourceName = schema.SourceDataSourceName,
                    SourceEntityName = schema.SourceEntityName,
                    DestinationDataSourceName = schema.DestinationDataSourceName,
                    DestinationEntityName = schema.DestinationEntityName,
                    AddMissingColumns = schema.AddMissingColumns,
                    CreateDestinationIfNotExists = schema.CreateDestinationIfNotExists
                }, msg => _preflightLog.Add(msg), token).ConfigureAwait(true);

                // Structural validation plus whichever integration channels are wired.
                _preflight = await _manager.RunPreflightAsync(schema, token).ConfigureAwait(true);

                _schema = schema;
                BindFindings(schema);
                return true;
            }
            catch (OperationCanceledException)
            {
                SetStatus("Preflight cancelled.");
                return false;
            }
            catch (Exception ex)
            {
                SetStatus($"Preflight failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Binds every preflight verdict the engine produced, plus the limits of what it checked.
        /// </summary>
        private void BindFindings(DataSyncSchema schema)
        {
            _lstFindings.ClearItems();
            var items = new List<SimpleItem>();

            foreach (var line in _preflightLog)
                items.Add(new SimpleItem { Text = $"[Destination] {line}" });

            // ── destination acceptance ──
            var sp = _schemaPreflight;
            if (sp != null)
            {
                items.Add(new SimpleItem
                {
                    Text = $"[Destination] source resolved={sp.SourceResolved}/connected={sp.SourceConnected}, " +
                           $"destination resolved={sp.DestinationResolved}/connected={sp.DestinationConnected}, " +
                           $"destination exists={sp.DestinationExisted}",
                    Value = sp
                });

                foreach (var f in sp.MissingDestinationFields ?? Array.Empty<string>())
                    items.Add(new SimpleItem { Text = $"[Destination] '{f}' is missing from the destination entity.", Value = f });

                items.Add(new SimpleItem
                {
                    Text = $"[Destination/{sp.Status?.Flag}] {sp.Status?.Message}",
                    Value = sp.Status
                });
            }

            // ── structural / rules / defaults / mapping ──
            var pf = _preflight;
            if (pf != null)
            {
                foreach (var i in pf.Issues ?? new List<SyncPreflightIssue>())
                    items.Add(new SimpleItem { Text = $"[{i.Channel}/{i.Severity}] {i.Code} — {i.Message}", Value = i });

                if (pf.MappingScore >= 0)
                    items.Add(new SimpleItem { Text = $"[Mapping] quality score {pf.MappingScore}/100.", Value = pf });
            }

            // The Rules, Defaults and Mapping channels of RunPreflightAsync are all conditional on
            // an IntegrationContext that this wizard does not build, so silence from them means
            // "not evaluated", not "passed". Say so rather than letting a clean list imply approval.
            if (_manager?.IntegrationContext == null)
                items.Add(new SimpleItem
                {
                    Text = "[Scope] No integration context is configured, so the Rule Engine, Defaults and " +
                           "Mapping-quality checks were skipped — only structural and destination checks ran."
                });

            // The wizard collects no sync key and no sync mode, so the translator maps this to a
            // full refresh. Worth stating: an operator seeing "sync" may expect an incremental one.
            items.Add(new SimpleItem
            {
                Text = "[Scope] Sync mode is a full refresh — this wizard does not expose sync-mode or " +
                       "sync-key selection, so rows are matched by neither watermark nor upsert key."
            });

            items.Add(new SimpleItem
            {
                Text = $"[Scope] Batch size {schema.BatchSize}, create destination if missing=" +
                       $"{schema.CreateDestinationIfNotExists}."
            });

            // The destination check above is strict about columns and cannot be relaxed, so say why
            // rather than leave the operator hunting for the option that would allow it.
            items.Add(new SimpleItem
            {
                Text = "[Scope] A destination missing any source column blocks this sync. The engine " +
                       "can create a whole destination entity but cannot add a column to an existing " +
                       "one, so there is no option to continue past that — align the destination first."
            });

            if (items.Count == 0)
                items.Add(new SimpleItem { Text = "No findings — nothing to report." });

            _lstFindings.AddItems(items);
            _lstFindings.RefreshItems();

            bool destBlocked = sp?.Status?.Flag == Errors.Failed;
            bool approved = pf?.IsApproved == true && !destBlocked;
            int errors = pf?.Issues?.Count(i => i.Severity == "Error") ?? 0;
            int warnings = pf?.Issues?.Count(i => i.Severity == "Warning") ?? 0;

            _lblPreflightSummary.Text =
                $"Preflight: {(approved ? "approved" : "BLOCKED")} — {errors} error(s), {warnings} warning(s). " +
                $"Destination: {(destBlocked ? "rejected" : "accepts")}. " +
                $"Rules passed={pf?.RulesPassed}, defaults ready={pf?.DefaultsReady}.";

            SetStatus(approved
                ? "Preflight approved — nothing has moved yet."
                : destBlocked
                    ? "Destination rejected this sync — resolve before running."
                    : $"Blocked by {errors} preflight error(s) — resolve before running.");
        }

        // ── stage 2 → run ─────────────────────────────────────────────────────

        private async Task RunSyncAsync()
        {
            var manager = _manager;
            var schema = _schema;
            if (manager == null || schema == null)
            {
                SetStatus("Run preflight first.");
                return;
            }

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _lstRunLog.ClearItems();
            _progress.Value = 0;

            using var busy = BeginBusy("Running sync…");
            try
            {
                var progress = new Progress<PassedArgs>(args =>
                {
                    if (args.ParameterInt1 > 0)
                        _progress.Value = Math.Clamp(args.ParameterInt1, 0, 100);
                    AppendLog(args.Messege ?? string.Empty);
                    if (!string.IsNullOrWhiteSpace(args.Messege))
                        _lblRunStatus.Text = args.Messege;
                });

                var result = await manager.SyncDataAsync(schema, token, progress).ConfigureAwait(true);
                bool ok = result?.Flag == Errors.Ok;

                _progress.Value = ok ? 100 : _progress.Value;
                _lblRunStatus.Text = ok ? "Sync completed." : $"Sync failed: {result?.Message}";
                AppendLog(_lblRunStatus.Text);

                BindRunReports(manager, schema);
                SetStatus(_lblRunStatus.Text);

                Completed?.Invoke(this, new WizardCompletedEventArgs
                {
                    Succeeded = ok,
                    Summary = _lblRunStatus.Text
                });
            }
            catch (OperationCanceledException)
            {
                _lblRunStatus.Text = "Sync cancelled.";
                AppendLog(_lblRunStatus.Text);
                SetStatus(_lblRunStatus.Text);
            }
            catch (Exception ex)
            {
                _lblRunStatus.Text = $"Sync crashed: {ex.Message}";
                AppendLog(_lblRunStatus.Text);
                SetStatus(_lblRunStatus.Text);
            }
        }

        /// <summary>
        /// Surfaces the state the manager and schema carry after a run.
        /// </summary>
        /// <remarks>
        /// Deliberately does not report the reconciliation report's row counts. BeepSyncManager
        /// builds that report with SourceRowsScanned/DestRowsWritten/Inserted/Updated/Skipped
        /// hardcoded to 0 (BeepSyncManager.Sync.cs BuildReconReport), because the row movement
        /// happens inside DataImportManager and is never counted back. Showing "0 rows written"
        /// after a successful sync would be worse than showing nothing — and RejectRate is derived
        /// from SourceRowsScanned, so it is always 0.0 for the same reason.
        /// </remarks>
        private void BindRunReports(BeepSyncManager manager, DataSyncSchema schema)
        {
            AppendLog($"[status] {schema.SyncStatus}: {schema.SyncStatusMessage}");

            var recon = manager.LastRunReconciliationReport;
            if (recon != null)
            {
                AppendLog($"[reconciliation] run {recon.RunId} by {recon.GeneratedBy} at {recon.GeneratedAt:u}.");
                AppendLog($"[reconciliation] rejects={recon.RejectCount}, quarantined={recon.QuarantineCount}, " +
                          $"conflicts={recon.ConflictCount}, defaults filled={recon.DefaultsFillCount}" +
                          (recon.RunAbortedByThreshold ? ", ABORTED by DQ threshold" : "") +
                          " (row counts are not tracked by the engine and are omitted).");

                if (recon.MappingQualityScore >= 0)
                    AppendLog($"[reconciliation] mapping quality {recon.MappingQualityScore}/100 ({recon.MappingQualityBand}).");

                foreach (var f in recon.UnmappedRequiredFields ?? new List<string>())
                    AppendLog($"[reconciliation] '{f}' received neither a mapped value nor a default — written as null.");

                foreach (var d in recon.DqFailures ?? new List<DqGateResult>())
                    AppendLog($"[dq/{d.ReasonCode}] {d.RuleKey} on {d.EntityName}" +
                              (string.IsNullOrWhiteSpace(d.FieldName) ? "" : $".{d.FieldName}") +
                              $" — {d.Message}");
            }

            var cp = manager.LastRunCheckpoint;
            if (cp != null)
                AppendLog($"[checkpoint] run {cp.RunId} — status={cp.Status}, attempts={cp.AttemptCount}" +
                          (string.IsNullOrWhiteSpace(cp.LastErrorCategory) ? "" : $", last error category={cp.LastErrorCategory}"));

            // Only populated for bidirectional runs with ConflictPolicy.CaptureEvidence enabled.
            foreach (var c in manager.LastRunConflicts ?? new List<ConflictEvidence>())
                AppendLog($"[conflict/{c.ReasonCode}] {c.EntityName} key={c.RecordKey} — winner={c.Winner} " +
                          $"(rule {c.RuleKey}, {c.RuleElapsed.TotalMilliseconds:F0}ms).");

            foreach (var a in schema.LastRunAlerts ?? new List<SyncAlertRecord>())
                AppendLog($"[alert/{a.Severity}] {a.RuleKey} — {a.Reason}" +
                          (string.IsNullOrWhiteSpace(a.RemediationHint) ? "" : $" → {a.RemediationHint}"));
        }

        private void AppendLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            _lstRunLog.AddItem(new SimpleItem { Text = message });
            _lstRunLog.RefreshItems();
        }

        private void SetStatus(string message) => _lblStatus.Text = message;

        private IDisposable BeginBusy(string message)
        {
            _busy = true;
            SetStatus(message);
            _btnNext.Enabled = false;
            _btnBack.Enabled = false;
            Cursor = Cursors.WaitCursor;
            return new BusyScope(this);
        }

        private sealed class BusyScope : IDisposable
        {
            private readonly uc_SyncWizard _owner;
            public BusyScope(uc_SyncWizard owner) => _owner = owner;

            public void Dispose()
            {
                _owner._busy = false;
                _owner.Cursor = Cursors.Default;
                _owner.UpdateStageUi();
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            _cts?.Cancel();
            base.OnHandleDestroyed(e);
        }

    }
}
