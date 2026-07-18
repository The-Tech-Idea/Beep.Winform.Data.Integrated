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
using TheTechIdea.Beep.Editor.Schema;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Controls.VerticalTables.Models;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    [AddinAttribute(Caption = "Schema Compare & Sync Preflight", Name = "uc_SchemaManagerWizard",
        misc = "Config", menu = "Configuration", addinType = AddinType.Control,
        displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 10, RootNodeName = "Configuration", Order = 10, ID = 10,
        BranchText = "Schema Compare", BranchType = EnumPointType.Function,
        IconImageName = "schema.svg", BranchClass = "ADDIN",
        BranchDescription = "Compare a source and destination entity, then preflight a sync (no DDL applied here).")]

    public partial class uc_SchemaManagerWizard : TemplateUserControl, IAddinVisSchema
    {
        /// <summary>Scope selection, then preflight/draft results.</summary>
        private enum Stage { Scope = 0, Results = 1 }

        public event EventHandler<WizardCompletedEventArgs>? Completed;

        private Stage _stage = Stage.Scope;
        private bool _busy;

        /// <summary>
        /// Per-combo generation counters for the entity loads — one per target, so a destination
        /// change cannot cancel an in-flight source load.
        /// </summary>
        private readonly Dictionary<BeepComboBox, int> _entityLoadGeneration = new();
        private SchemaPreflightResult? _preflight;
        private CancellationTokenSource? _cts;

        /// <summary>Full field-level diff engine shared with the migration/sync layers. This is the
        /// same <see cref="SchemaComparator"/> the engine uses; the wizard does not re-implement diffing.</summary>
        private readonly ISchemaComparator _comparator = new SchemaComparator();

        /// <summary>Stable schema hash so an operator can tell when a sync draft's shape changed
        /// across runs — computed by the engine's <see cref="SchemaFingerprinter"/>, not here.</summary>
        private readonly ISchemaFingerprinter _fingerprinter = new SchemaFingerprinter();

        /// <summary>
        /// Designer/parameterless ctor. Must not chain to the IServiceProvider overload with null —
        /// that resolves services off a null provider and throws. Everything below is null-safe
        /// without beepService; the control simply reports "No connections are configured."
        /// </summary>
        public uc_SchemaManagerWizard() => InitializeControl();

        public uc_SchemaManagerWizard(IServiceProvider services) : base(services) => InitializeControl();

        private void InitializeControl()
        {
            InitializeComponent();
            Details.AddinName = "Schema Compare & Sync Preflight";
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
        public int Order { get; set; } = 10;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ID { get; set; } = 10;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchText { get; set; } = "Schema Compare";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Level { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BranchID { get; set; } = 10;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconImageName { get; set; } = "schema.svg";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchStatus { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ParentBranchID { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchDescription { get; set; } = "Compare a source and destination entity, then preflight a sync (no DDL applied here).";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchClass { get; set; } = "ADDIN";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string AddinName { get; set; } = "uc_SchemaManagerWizard";
        #endregion

        /// <summary>Result of the last preflight, or null before Run Preflight.</summary>
        public SchemaPreflightResult? Preflight => _preflight;

        private void WireEvents()
        {
            _btnCancel.Click += (_, _) =>
            {
                _cts?.Cancel();
                Completed?.Invoke(this, new WizardCompletedEventArgs { Cancelled = true });
            };
            _btnBack.Click += (_, _) =>
            {
                if (_busy || _stage == Stage.Scope) return;
                _stage = Stage.Scope;
                UpdateStageUi();
            };
            _btnNext.Click += BtnNext_Click;
            // Fire-and-forget is safe only because LoadEntities catches its own failures.
            _cboSourceConn.SelectedItemChanged += (_, _) =>
                _ = LoadEntities(_cboSourceConn, _cboSourceEntity);
            _cboDestConn.SelectedItemChanged += (_, _) =>
                _ = LoadEntities(_cboDestConn, _cboDestEntity);
        }

        protected override void ApplyDpiScaledLayout()
        {
            _rootPanel.Padding = BeepLayoutMetrics.DialogPadding.ScalePadding(this);
            _contentHost.Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this);
            _actionsPanel.Padding = BeepLayoutMetrics.ButtonStripPd.ScalePadding(this);

            int btnH = BeepLayoutMetrics.ButtonStandard.Height.ScaleValue(this);
            int btnLargeH = BeepLayoutMetrics.ButtonLarge.Height.ScaleValue(this);
            _btnCancel.MinimumSize = new System.Drawing.Size(
                BeepLayoutMetrics.ButtonStandard.Width.ScaleValue(this), btnH);
            _btnBack.MinimumSize = new System.Drawing.Size(
                BeepLayoutMetrics.ButtonStandard.Width.ScaleValue(this), btnH);
            _btnNext.MinimumSize = new System.Drawing.Size(
                BeepLayoutMetrics.ButtonLarge.Width.ScaleValue(this), btnLargeH);
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
                _cboSourceConn.ListItems.Add(new SimpleItem
                {
                    Text = $"{conn.ConnectionName} ({conn.DatabaseType})",
                    Value = conn.ConnectionName
                });
                _cboDestConn.ListItems.Add(new SimpleItem
                {
                    Text = $"{conn.ConnectionName} ({conn.DatabaseType})",
                    Value = conn.ConnectionName
                });
            }
        }

        /// <summary>
        /// Fills an entity picker from the datasource's own entity list, so the user picks a
        /// real entity rather than typing a name that may not exist.
        /// </summary>
        /// <remarks>
        /// GetDataSource resolves a driver and GetEntitesList is a metadata round-trip, so both run
        /// off the UI thread. The per-combo generation counter drops a response whose connection the
        /// user has already moved off — the combos stay live across the await, so a slow first
        /// selection could otherwise land after a faster second one and fill the picker with the
        /// wrong connection's entities.
        /// </remarks>
        private async Task LoadEntities(BeepComboBox connectionCombo, BeepComboBox entityCombo)
        {
            entityCombo.ListItems.Clear();

            var editor = beepService?.DMEEditor;
            var name = connectionCombo.SelectedItem?.Value as string;
            if (editor == null || string.IsNullOrWhiteSpace(name)) return;

            int generation = _entityLoadGeneration.TryGetValue(entityCombo, out var g) ? g + 1 : 1;
            _entityLoadGeneration[entityCombo] = generation;

            try
            {
                var entities = await Task.Run(() => editor.GetDataSource(name)?.GetEntitesList())
                    .ConfigureAwait(true);

                if (_entityLoadGeneration[entityCombo] != generation || IsDisposed || entities == null) return;

                foreach (var entity in entities.Where(e => !string.IsNullOrWhiteSpace(e)).OrderBy(e => e))
                    entityCombo.ListItems.Add(new SimpleItem { Text = entity, Value = entity });
            }
            catch (Exception ex)
            {
                if (_entityLoadGeneration[entityCombo] != generation || IsDisposed) return;
                SetStatus($"Could not list entities for '{name}': {ex.Message}");
            }
        }

        private async void BtnNext_Click(object? sender, EventArgs e)
        {
            if (_busy) return;

            try
            {
                if (_stage == Stage.Scope)
                {
                    if (await RunPreflightAsync())
                    {
                        _stage = Stage.Results;
                        UpdateStageUi();
                    }
                }
                else
                {
                    await BuildDraftAsync();
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Schema manager error: {ex.Message}");
            }
        }

        private void UpdateStageUi()
        {
            _stepScope.Visible = _stage == Stage.Scope;
            _stepResults.Visible = _stage == Stage.Results;
            (_stage == Stage.Scope ? (Control)_stepScope : _stepResults).BringToFront();

            (_lblSubtitle.Text, _btnNext.Text) = _stage == Stage.Scope
                ? ("Step 1 of 2 — choose source and destination entities.", "Run Preflight")
                : ("Step 2 of 2 — preflight results; build a sync draft when ready.", "Build Sync Draft");

            _btnBack.Enabled = !_busy && _stage != Stage.Scope;
            _btnNext.Enabled = !_busy && CanAdvance();
        }

        private bool CanAdvance() => _stage switch
        {
            Stage.Scope => _cboSourceConn.SelectedItem != null
                           && _cboSourceEntity.SelectedItem != null
                           && _cboDestConn.SelectedItem != null
                           && _cboDestEntity.SelectedItem != null,
            // A draft is only meaningful once both sides resolved.
            _ => _preflight != null
                 && _preflight.SourceResolved
                 && _preflight.DestinationResolved,
        };

        private SchemaRequest BuildRequest() => new()
        {
            SourceDataSourceName = (string)_cboSourceConn.SelectedItem!.Value!,
            SourceEntityName = (string)_cboSourceEntity.SelectedItem!.Value!,
            DestinationDataSourceName = (string)_cboDestConn.SelectedItem!.Value!,
            DestinationEntityName = (string)_cboDestEntity.SelectedItem!.Value!,
            AddMissingColumns = _chkAddMissingColumns.Checked,
            CreateDestinationIfNotExists = _chkCreateDestination.Checked
        };

        private async Task<bool> RunPreflightAsync()
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

            var request = BuildRequest();
            var log = new List<string>();

            using var busy = BeginBusy("Running schema preflight…");
            try
            {
                _preflight = await SyncSchemaPreflight
                    .RunPreflightAsync(editor, request, log.Add, token)
                    .ConfigureAwait(true);
                BindResults(_preflight, log);
                _lblFingerprint.Text = string.Empty;

                // The preflight resolves the source structure internally but only returns the
                // destination snapshot, so read the source structure once more to run a full
                // field-level diff through the shared SchemaComparator.
                var compareSummary = await BuildComparisonAsync(editor, request, _preflight, token)
                    .ConfigureAwait(true);

                bool ok = _preflight.Status?.Flag == Errors.Ok;
                _lblResultsSummary.Text =
                    $"Source: {(_preflight.SourceConnected ? "connected" : "not connected")}, " +
                    $"destination: {(_preflight.DestinationConnected ? "connected" : "not connected")}, " +
                    $"destination {(_preflight.DestinationExisted ? "exists" : "missing")}, " +
                    $"{_preflight.MissingDestinationFields.Count} missing field(s). {compareSummary}";

                SetStatus(ok ? "Preflight completed." : $"Preflight reported: {_preflight.Status?.Message}");
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

        private void BindResults(SchemaPreflightResult result, IReadOnlyList<string> log)
        {
            _lstResults.ClearItems();
            var items = new List<SimpleItem>
            {
                new() { Text = $"[status] {result.Status?.Message}" },
                new() { Text = $"[source] resolved={result.SourceResolved}, connected={result.SourceConnected}, structure={result.SourceStructureLoaded}" },
                new() { Text = $"[destination] resolved={result.DestinationResolved}, connected={result.DestinationConnected}, structure={result.DestinationStructureLoaded}, existed={result.DestinationExisted}" }
            };

            foreach (var field in result.MissingDestinationFields)
                items.Add(new SimpleItem { Text = $"[missing field] {field}" });

            // The engine captures a baseline snapshot of an existing destination; note it here so
            // the operator knows the comparison grid is anchored on a real read, not an assumption.
            if (result.DestinationSnapshot?.Fields is { Count: > 0 } destFields)
                items.Add(new SimpleItem
                {
                    Text = $"[destination snapshot] {destFields.Count} field(s) captured at {result.DestinationSnapshot.CapturedAt:u}."
                });

            foreach (var line in log)
                items.Add(new SimpleItem { Text = $"[log] {line}" });

            _lstResults.AddItems(items);
            _lstResults.RefreshItems();
        }

        /// <summary>
        /// Builds the source→destination field comparison shown in <c>_tblCompare</c>. Runs the
        /// shared <see cref="SchemaComparator"/> over a snapshot of the source structure and the
        /// destination snapshot already captured by the preflight, so the operator sees exactly which
        /// fields would be added, differ in type, or exist only on the destination — the detail the
        /// preflight's <c>MissingDestinationFields</c> list cannot convey.
        /// </summary>
        /// <returns>A one-line summary suitable for appending to the results header.</returns>
        private async Task<string> BuildComparisonAsync(
            IDMEEditor editor, SchemaRequest request, SchemaPreflightResult preflight, CancellationToken token)
        {
            ClearComparison();

            if (!preflight.SourceStructureLoaded)
                return "Comparison skipped — source structure unavailable.";

            // GetEntityStructure is a metadata round-trip; keep it off the UI thread. Snapshot fields
            // are read exactly as SyncSchemaPreflight reads the destination, so the two sides are
            // apples-to-apples for the comparator.
            var source = await Task.Run(() =>
            {
                var sds = editor.GetDataSource(request.SourceDataSourceName);
                var st = sds?.GetEntityStructure(request.SourceEntityName, false);
                if (st?.Fields == null) return null;
                return new SchemaSnapshot
                {
                    ContextKey     = $"{request.SourceDataSourceName}/{request.SourceEntityName}",
                    DataSourceName = request.SourceDataSourceName,
                    EntityName     = request.SourceEntityName,
                    Fields         = st.Fields.Select(f => new SnapshotField
                    {
                        Name       = f.FieldName,
                        DataType   = f.ColumnTypeName ?? string.Empty,
                        IsNullable = f.AllowDBNull,
                        MaxLength  = f.Size
                    }).ToList()
                };
            }, token).ConfigureAwait(true);

            if (token.IsCancellationRequested || IsDisposed) return string.Empty;

            if (source == null || source.Fields.Count == 0)
                return "Comparison skipped — source has no readable fields.";

            var dest = preflight.DestinationSnapshot;
            var rows = new List<FeatureRow>();

            // Destination does not exist yet → every source field is a create.
            if (dest == null || dest.Fields.Count == 0)
            {
                foreach (var f in source.Fields)
                    rows.Add(MakeRow(f.Name, f.DataType, "—", "Create (new destination)"));
                BindComparison(rows);
                return $"Destination is new — {source.Fields.Count} field(s) would be created.";
            }

            // baseline = destination (what exists), current = source (what we want to move in).
            var report = _comparator.Compare(baseline: dest, current: source);

            var altered = report.AlteredFields
                .GroupBy(a => a.FieldName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            var destByName = dest.Fields
                .GroupBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            int adds = 0, typeDiffs = 0, matches = 0;

            foreach (var f in source.Fields)
            {
                if (!destByName.TryGetValue(f.Name, out var d))
                {
                    adds++;
                    rows.Add(MakeRow(f.Name, f.DataType, "—", "Add to destination"));
                }
                else if (altered.TryGetValue(f.Name, out var drift))
                {
                    typeDiffs++;
                    rows.Add(MakeRow(f.Name, f.DataType, d.DataType, $"Type differs — {drift.Description}"));
                }
                else
                {
                    matches++;
                    rows.Add(MakeRow(f.Name, f.DataType, d.DataType, "Match"));
                }
            }

            // Fields the destination has but the source does not — sync never drops these, so flag
            // them as informational (mirrors the engine's additive-only stance).
            foreach (var f in report.RemovedFields)
                rows.Add(MakeRow(f.Name, "—", f.DataType, "Destination only (not synced)"));

            BindComparison(rows);
            return $"Fields — {matches} match, {adds} to add, {typeDiffs} type diff(s), " +
                   $"{report.RemovedFields.Count} destination-only.";
        }

        /// <summary>Projects one field into a <see cref="FeatureRow"/> — Source/Destination/Status
        /// columns, with the verdict doubling as the group category so rows cluster by outcome.</summary>
        private static FeatureRow MakeRow(string field, string sourceType, string destType, string status)
        {
            var category = status.StartsWith("Type differs", StringComparison.Ordinal) ? "Type differs" : status;
            return new FeatureRow
            {
                Name     = field,
                Category = category,
                IconType = FeatureIconType.Text,
                Tooltip  = status,
                Values   = new Dictionary<int, object?>
                {
                    [0] = sourceType,
                    [1] = destType,
                    [2] = status
                }
            };
        }

        /// <summary>Binds the field rows as a side-by-side Source/Destination/Status comparison.
        /// Every column shares the same row list; each cell reads its own <c>Values[columnIndex]</c>.</summary>
        private void BindComparison(List<FeatureRow> rows)
        {
            if (IsDisposed) return;
            _tblCompare.SetComparisonData(
                ("Source", rows),
                ("Destination", rows),
                ("Status", rows));
        }

        /// <summary>Empties the comparison table between runs.</summary>
        private void ClearComparison()
        {
            if (IsDisposed) return;
            _tblCompare.Columns.Clear();
        }

        private async Task BuildDraftAsync()
        {
            var editor = beepService?.DMEEditor;
            // _preflight stands in for the old "_schema was constructed" guard: both mean
            // preflight has run. A draft before preflight would build against unresolved sources.
            if (editor == null || _preflight == null) return;

            using var busy = BeginBusy("Building sync draft…");
            try
            {
                var draft = await SyncSchemaPreflight
                    .BuildSyncDraftAsync(editor, BuildRequest(), _cts?.Token ?? CancellationToken.None)
                    .ConfigureAwait(true);
                bool ok = draft.Status?.Flag == Errors.Ok && draft.Draft != null;

                // A stable fingerprint lets an operator tell at a glance whether the draft's shape
                // changed from a previous run — the same hash the sync governance layer stores.
                string shortHash = string.Empty;
                if (ok)
                {
                    var hash = _fingerprinter.ComputeSchemaHash(draft.Draft!);
                    shortHash = hash[..Math.Min(16, hash.Length)];
                    _lblFingerprint.Text =
                        $"Draft fingerprint: {shortHash}  ·  {draft.Draft!.MappedFields.Count} mapped field(s)";
                }

                _lstResults.AddItem(new SimpleItem
                {
                    Text = ok
                        ? $"[draft] Built sync draft '{draft.Draft!.Id}' — fingerprint {shortHash}."
                        : $"[draft] Failed: {draft.Status?.Message}"
                });
                _lstResults.RefreshItems();

                SetStatus(ok ? "Sync draft built." : $"Draft failed: {draft.Status?.Message}");
                Completed?.Invoke(this, new WizardCompletedEventArgs
                {
                    Succeeded = ok,
                    Summary = _lblResultsSummary.Text
                });
            }
            catch (Exception ex)
            {
                SetStatus($"Draft failed: {ex.Message}");
            }
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
            private readonly uc_SchemaManagerWizard _owner;
            public BusyScope(uc_SchemaManagerWizard owner) => _owner = owner;

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
