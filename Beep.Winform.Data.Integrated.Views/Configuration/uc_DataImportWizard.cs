using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor.Importing;
using TheTechIdea.Beep.Editor.Schema;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    [AddinAttribute(Caption = "Data Import Wizard", Name = "uc_DataImportWizard",
        misc = "Config", menu = "Configuration", addinType = AddinType.Control,
        displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 15, RootNodeName = "Configuration", Order = 15, ID = 15,
        BranchText = "Data Import Wizard", BranchType = EnumPointType.Function,
        IconImageName = "import.svg", BranchClass = "ADDIN",
        BranchDescription = "Profile, transform, and run data imports.")]

    public partial class uc_DataImportWizard : TemplateUserControl, IAddinVisSchema
    {
        /// <summary>The three stages of the import lifecycle this wizard walks.</summary>
        private enum Stage { Scope = 0, Preflight = 1, Run = 2 }

        public event EventHandler<WizardCompletedEventArgs>? Completed;

        private Stage _stage = Stage.Scope;
        private bool _busy;
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Held across Preflight and Run so the pause/resume controls and the post-run
        /// <see cref="DataImportManager.ImportLogData"/> belong to the manager that ran the import.
        /// Disposed by the control.
        /// </summary>
        private DataImportManager? _manager;

        /// <summary>The config preflight approved. Run must use this exact instance, not a rebuilt one.</summary>
        private DataImportConfiguration? _config;

        /// <summary>
        /// Destination-acceptance verdict. The import itself does not preflight — RunImportAsync
        /// never reads <c>RunMigrationPreflight</c> — so this wizard runs the check explicitly and
        /// gates on it, rather than letting a mismatch surface mid-run with rows already written.
        /// </summary>
        private SchemaPreflightResult? _preflight;

        /// <summary>
        /// Narration from the preflight's log callback. Buffered because that callback fires from
        /// inside SyncSchemaPreflight's own Task.Run — writing to the list there would be a
        /// cross-thread call. Rendered by BindFindings after the await resumes on the UI thread.
        /// </summary>
        private readonly List<string> _preflightLog = new();

        /// <summary>Per-combo generation counters guarding <see cref="LoadEntitiesAsync"/> against stale responses.</summary>
        private readonly Dictionary<BeepComboBox, int> _entityLoadGeneration = new();

        private bool _paused;

        /// <summary>
        /// Designer/parameterless ctor. Must not chain to the IServiceProvider overload with null —
        /// that resolves services off a null provider and throws.
        /// </summary>
        public uc_DataImportWizard() => InitializeControl();

        public uc_DataImportWizard(IServiceProvider services) : base(services) => InitializeControl();

        private void InitializeControl()
        {
            InitializeComponent();
            Details.AddinName = "Data Import Wizard";
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
        public int Order { get; set; } = 15;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ID { get; set; } = 15;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchText { get; set; } = "Data Import Wizard";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Level { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BranchID { get; set; } = 15;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconImageName { get; set; } = "import.svg";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchStatus { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ParentBranchID { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchDescription { get; set; } = "Profile, transform, and run data imports.";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchClass { get; set; } = "ADDIN";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string AddinName { get; set; } = "uc_DataImportWizard";
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
            _btnPause.Click += BtnPause_Click;
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
            // resumes. The connection combo stays live during the await, so a user switching
            // connections mid-fetch would otherwise get the slower response landing last.
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
        /// Drops a preflight verdict that no longer describes the current selection, so a user
        /// cannot pass preflight, change the destination, and then run against the stale approval.
        /// </summary>
        private void InvalidatePreflight()
        {
            _preflight = null;
            _config = null;
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
                        await RunImportAsync();
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
                SetStatus($"Import wizard error: {ex.Message}");
            }
        }

        /// <summary>
        /// Pause/Resume is genuinely honoured mid-run: PauseImport resets the ManualResetEventSlim
        /// that RunImportAsync's batch loop waits on before each batch, so the run stops at the next
        /// batch boundary rather than immediately.
        /// </summary>
        private void BtnPause_Click(object? sender, EventArgs e)
        {
            var manager = _manager;
            if (manager == null) return;

            if (_paused)
            {
                manager.ResumeImport();
                _paused = false;
                _btnPause.Text = "Pause";
                AppendLog("[control] Resumed.");
            }
            else
            {
                manager.PauseImport();
                _paused = true;
                _btnPause.Text = "Resume";
                AppendLog("[control] Paused — the run stops at the next batch boundary.");
            }
            SetStatus(_paused ? "Import paused." : "Import running…");
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
                Stage.Preflight => ("Step 2 of 3 — preflight findings. Nothing has been written yet.", "Run Import"),
                _ => ("Step 3 of 3 — import progress and result.", "Finish")
            };

            _btnBack.Enabled = !_busy && _stage == Stage.Preflight;
            _btnNext.Enabled = !_busy && CanAdvance();
            // Only meaningful while a run is actually in flight — pausing a finished run does nothing.
            _btnPause.Visible = _stage == Stage.Run && _busy;
        }

        /// <summary>Gates Next on the state the current stage actually requires.</summary>
        private bool CanAdvance() => _stage switch
        {
            Stage.Scope => SourceConn != null && SourceEntity != null && DestConn != null && DestEntity != null,

            // A destination that cannot accept the source must not be runnable. The import will not
            // re-check this itself, so this gate is the only thing standing between a mismatch and
            // a partially-written destination.
            Stage.Preflight => _config != null && _preflight?.Status?.Flag != Errors.Failed,

            _ => true
        };

        private DataImportConfiguration BuildConfig() => new()
        {
            SourceDataSourceName = SourceConn!,
            SourceEntityName = SourceEntity!,
            DestDataSourceName = DestConn!,
            DestEntityName = DestEntity!,
            BatchSize = int.TryParse(_txtBatchSize.Text, out var b) && b > 0 ? b : 50,
            CreateDestinationIfNotExists = _chkCreateDestination.Checked,
            ApplyDefaults = _chkApplyDefaults.Checked,

            // Pinned false, and given no control on the Scope step. This must be explicit: the
            // property defaults to TRUE, and true means "skip the missing-column check" to
            // SyncSchemaPreflight (`if (!request.AddMissingColumns && destExists ...)`) — leaving it
            // unset would silently disable the very guard the Preflight stage exists to apply.
            // Nothing would then add the columns either: the import's only schema-mutating step,
            // EnsureDestinationEntityExists, creates whole entities and never alters one. So rows
            // would move into a destination that cannot hold them. BeepSyncManager.Sync.cs pins it
            // false for the same reason.
            AddMissingColumns = false,

            // RunMigrationPreflight is likewise left alone: RunImportAsync never reads it — it only
            // feeds the back-compat RunMigrationPreflightAsync shim — so setting it would preflight
            // nothing. This wizard runs that check itself, as its own stage.
        };

        // ── stage 1 → preflight ───────────────────────────────────────────────

        /// <summary>
        /// Checks that the destination will accept the source, without writing anything.
        /// </summary>
        /// <remarks>
        /// Calls <see cref="SyncSchemaPreflight"/> directly rather than the manager's
        /// <c>RunMigrationPreflightAsync</c> shim: the shim returns only an <c>IErrorsInfo</c>
        /// status, discarding the structured result — including MissingDestinationFields, which is
        /// the one thing an operator needs to act on.
        /// </remarks>
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
                _manager = new DataImportManager(editor);
                var config = BuildConfig();
                _preflightLog.Clear();

                _preflight = await SyncSchemaPreflight.RunPreflightAsync(editor, new SchemaRequest
                {
                    SourceDataSourceName = config.SourceDataSourceName,
                    SourceEntityName = config.SourceEntityName,
                    DestinationDataSourceName = config.DestDataSourceName,
                    DestinationEntityName = config.DestEntityName,
                    // Matches what the config carries, so this verdict is the one the run will get.
                    AddMissingColumns = config.AddMissingColumns,
                    CreateDestinationIfNotExists = config.CreateDestinationIfNotExists
                }, msg => _preflightLog.Add(msg), token).ConfigureAwait(true);

                _config = config;
                BindFindings(config);
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

        private void BindFindings(DataImportConfiguration config)
        {
            _lstFindings.ClearItems();
            var items = new List<SimpleItem>();

            foreach (var line in _preflightLog)
                items.Add(new SimpleItem { Text = $"[Destination] {line}" });

            var p = _preflight;
            if (p != null)
            {
                items.Add(new SimpleItem
                {
                    Text = $"[Destination] source resolved={p.SourceResolved}/connected={p.SourceConnected}, " +
                           $"destination resolved={p.DestinationResolved}/connected={p.DestinationConnected}, " +
                           $"destination exists={p.DestinationExisted}",
                    Value = p
                });

                foreach (var f in p.MissingDestinationFields ?? Array.Empty<string>())
                    items.Add(new SimpleItem { Text = $"[Destination] '{f}' is missing from the destination entity.", Value = f });

                items.Add(new SimpleItem { Text = $"[Destination/{p.Status?.Flag}] {p.Status?.Message}", Value = p.Status });
            }

            items.Add(new SimpleItem
            {
                Text = $"[Scope] Batch size {config.BatchSize}, create destination if missing=" +
                       $"{config.CreateDestinationIfNotExists}, apply defaults={config.ApplyDefaults}."
            });

            items.Add(new SimpleItem
            {
                Text = "[Scope] A destination missing any source column blocks this import. The engine " +
                       "can create a whole destination entity but cannot add a column to an existing " +
                       "one, so there is no option to continue past that — align the destination first."
            });

            if (items.Count == 0)
                items.Add(new SimpleItem { Text = "No findings — nothing to report." });

            _lstFindings.AddItems(items);
            _lstFindings.RefreshItems();

            bool blocked = p?.Status?.Flag == Errors.Failed;
            int missing = p?.MissingDestinationFields?.Count ?? 0;

            _lblPreflightSummary.Text =
                $"Preflight: {(blocked ? "BLOCKED" : "approved")} — destination " +
                $"{(p?.DestinationExisted == true ? "exists" : "does not exist yet")}, " +
                $"{missing} missing column(s).";

            SetStatus(blocked
                ? "Destination rejected this import — resolve before running."
                : "Preflight approved — nothing has been written yet.");
        }

        // ── stage 2 → run ─────────────────────────────────────────────────────

        private async Task RunImportAsync()
        {
            var manager = _manager;
            var config = _config;
            if (manager == null || config == null)
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
            _paused = false;
            _btnPause.Text = "Pause";

            using var busy = BeginBusy("Running import…");
            try
            {
                var progress = new Progress<IPassedArgs>(args =>
                {
                    if (args.ParameterInt1 > 0)
                        _progress.Value = Math.Clamp(args.ParameterInt1, 0, 100);
                    AppendLog(args.Messege ?? string.Empty);
                    if (!string.IsNullOrWhiteSpace(args.Messege))
                        _lblRunStatus.Text = args.Messege;
                });

                var result = await manager.RunImportAsync(config, progress, token).ConfigureAwait(true);

                // A cancelled run is not a successful one. The engine swallows
                // OperationCanceledException and returns Errors.Ok for it, so Flag alone would
                // report "completed" for an import the user stopped a few batches in — and would
                // tell the host Succeeded=true. The tracked state is the honest signal, so it wins.
                //
                // State alone, deliberately — not `|| token.IsCancellationRequested`. Every
                // cancellation the run actually observed funnels into State=Cancelled, whereas the
                // token can be set after the run already finished: the Cancel click and this
                // continuation are both queued on the message loop, so a click landing first would
                // relabel a fully-written import as cancelled. State=Completed means it finished
                // before the cancel was noticed, and nothing was lost.
                var state = manager.GetImportStatus().State;
                bool cancelled = state == ImportState.Cancelled;
                bool ok = result?.Flag == Errors.Ok && !cancelled;

                _progress.Value = ok ? 100 : _progress.Value;
                _lblRunStatus.Text = cancelled
                    ? "Import cancelled — the destination holds whatever was written before it stopped."
                    : ok
                        ? "Import completed."
                        : $"Import failed: {result?.Message}";
                AppendLog(_lblRunStatus.Text);

                BindRunReports(manager);
                SetStatus(_lblRunStatus.Text);

                Completed?.Invoke(this, new WizardCompletedEventArgs
                {
                    Succeeded = ok,
                    Cancelled = cancelled,
                    Summary = _lblRunStatus.Text
                });
            }
            catch (OperationCanceledException)
            {
                _lblRunStatus.Text = "Import cancelled.";
                AppendLog(_lblRunStatus.Text);
                SetStatus(_lblRunStatus.Text);
            }
            catch (Exception ex)
            {
                _lblRunStatus.Text = $"Import crashed: {ex.Message}";
                AppendLog(_lblRunStatus.Text);
                SetStatus(_lblRunStatus.Text);
            }
        }

        /// <summary>
        /// Surfaces the counts the manager tracked during the run, plus its own log.
        /// </summary>
        /// <remarks>
        /// Only the fields DataImportManager genuinely maintains are reported. Its
        /// <c>CancelImport()</c> is not offered as a control at all: it cancels an
        /// <c>_internalCancellationTokenSource</c> that is never assigned, so it does nothing —
        /// the wizard's own token, wired to the Cancel button, is the real cancellation path.
        /// </remarks>
        private void BindRunReports(DataImportManager manager)
        {
            var status = manager.GetImportStatus();
            AppendLog($"[status] {status.State} — {status.RecordsProcessed}/{status.TotalRecords} record(s), " +
                      $"batch {status.CurrentBatch}/{status.TotalBatches}, {status.PercentComplete:F1}% complete.");

            if (status.StartedAt != null)
                AppendLog($"[status] started {status.StartedAt:u}" +
                          (status.FinishedAt != null
                              ? $", finished {status.FinishedAt:u} (took {(status.FinishedAt - status.StartedAt)?.TotalSeconds:F1}s)."
                              : "."));

            if (status.RecordsBlocked > 0 || status.RecordsQuarantined > 0 || status.RecordsWarned > 0)
                AppendLog($"[quality] blocked={status.RecordsBlocked}, quarantined={status.RecordsQuarantined}, " +
                          $"warned={status.RecordsWarned}.");

            // The engine's own log — the only place per-batch errors and skips are recorded.
            foreach (var log in manager.ImportLogData ?? new List<Importlogdata>())
            {
                if (log.Level == ImportLogLevel.Info || log.Level == ImportLogLevel.Debug) continue;
                AppendLog($"[{log.Level}/{log.Category}] {log.Message}" +
                          (log.RecordNumber > 0 ? $" (record {log.RecordNumber})" : ""));
            }
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
            Cursor = Cursors.WaitCursor;
            // Re-evaluate rather than hardcoding the buttons off: UpdateStageUi already derives
            // every button from _busy + stage, and it is what reveals Pause once a run is in flight.
            UpdateStageUi();
            return new BusyScope(this);
        }

        private sealed class BusyScope : IDisposable
        {
            private readonly uc_DataImportWizard _owner;
            public BusyScope(uc_DataImportWizard owner) => _owner = owner;

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
