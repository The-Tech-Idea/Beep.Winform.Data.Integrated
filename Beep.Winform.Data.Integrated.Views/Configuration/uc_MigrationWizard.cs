using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Migration;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Default.Views.Template;

// Two unrelated types are named SchemaDriftReport — this is the one MigrationManager.InspectDrift
// returns; the other lives in TheTechIdea.Beep.Distributed.Schema. Aliased so it can't silently
// bind to the wrong one.
using SchemaDriftReport = TheTechIdea.Beep.Editor.Schema.SchemaDriftReport;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    [AddinAttribute(Caption = "Migration Wizard", Name = "uc_MigrationWizard",
        misc = "Config", menu = "Configuration", addinType = AddinType.Control,
        displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 12, RootNodeName = "Configuration", Order = 12, ID = 12,
        BranchText = "Migration Wizard", BranchType = EnumPointType.Function,
        IconImageName = "migration.svg", BranchClass = "ADDIN",
        BranchDescription = "Plan, validate, dry-run, apply, and verify migrations.")]

    public partial class uc_MigrationWizard : TemplateUserControl, IAddinVisSchema
    {
        /// <summary>The four stages of the migration lifecycle this wizard walks.</summary>
        private enum Stage
        {
            Scope = 0,
            Plan = 1,
            Safety = 2,
            Run = 3
        }

        public event EventHandler<WizardCompletedEventArgs>? Completed;

        private Stage _stage = Stage.Scope;
        private bool _busy;

        private IDataSource? _dataSource;
        private IMigrationManager? _migration;
        private MigrationPlanArtifact? _plan;
        private MigrationPolicyOptions _policyOptions = new();
        private CancellationTokenSource? _runCts;

        /// <summary>Phase 9: version currently recorded in the target DB (null = unversioned/unknown).</summary>
        private string? _currentDbVersion;

        /// <summary>
        /// Optional declared schema version to stamp after a successful run. When null the recorded
        /// version is patch-bumped (or seeded at 1.0.0). Mirrors Setup's <c>DeclaredSchemaVersion</c>.
        /// </summary>
        public string? DeclaredVersion { get; set; }

        // Phase 7 (C1): reader-option checkboxes added to the Scope-stage table in code.
        private BeepCheckBoxBool? _chkEnumAsString;
        private BeepCheckBoxBool? _chkHonorNrt;

        /// <summary>Entity types behind the current plan — needed for InspectDrift, which is type-based.</summary>
        private IReadOnlyList<Type> _planTypes = Array.Empty<Type>();

        /// <summary>Per-entity drift (.NET model vs live DB), keyed by Type.Name. The plan diff is
        /// additive-only, so this is the only place drops and alters become visible.</summary>
        private Dictionary<string, SchemaDriftReport> _drift = new();

        // Every other report lives on MigrationPlanArtifact itself (CompensationPlan,
        // RollbackReadinessReport, PerformancePlan, CiValidationReport, RolloutGovernanceReport,
        // Diagnostics, AuditTrail, …). The artifact is the aggregate the engine is built around —
        // holding private copies here would just be a second source of truth.

        private readonly BindingList<MigrationPlanRow> _planRows = new();

        /// <summary>
        /// Designer/parameterless ctor. Must not chain to the IServiceProvider overload with null —
        /// that resolves services off a null provider and throws. Everything below is null-safe
        /// without beepService; the control simply lists no connections.
        /// </summary>
        public uc_MigrationWizard() => InitializeControl();

        public uc_MigrationWizard(IServiceProvider services) : base(services) => InitializeControl();

        private void InitializeControl()
        {
            InitializeComponent();
            Details.AddinName = "Migration Wizard";
            AddReaderOptionControls();
            WireEvents();
            PopulateChoices();
            _gridPlan.DataSource = _planRows;
            UpdateStageUi();
        }

        /// <summary>
        /// Phase 7 (C1): adds the reader-option checkboxes (enum-as-text, honor NRT) to the Scope-stage
        /// table. Done in code — inserting two 35px rows before the table's 100% filler row — so the
        /// generated Designer file stays untouched. The values feed <c>MigrationManager.ReadOptions</c>
        /// when the plan is built.
        /// </summary>
        private void AddReaderOptionControls()
        {
            _chkEnumAsString = new BeepCheckBoxBool
            {
                UseThemeColors = true,
                Text = "Store enums as text",
                Dock = DockStyle.Fill
            };
            _chkHonorNrt = new BeepCheckBoxBool
            {
                UseThemeColors = true,
                Text = "Honor nullable reference types",
                Dock = DockStyle.Fill,
                Checked = true
            };

            // The last RowStyle is the 100% filler; insert two absolute rows before it so the new
            // checkboxes sit with the existing option checkboxes and the filler stays last.
            int fillerIndex = _scopeTable.RowStyles.Count - 1;
            _scopeTable.RowStyles.Insert(fillerIndex, new RowStyle(SizeType.Absolute, 35F));
            _scopeTable.RowStyles.Insert(fillerIndex, new RowStyle(SizeType.Absolute, 35F));
            _scopeTable.RowCount += 2;

            // Existing option checkboxes occupy column 1, rows 3–5; the old filler was row 6.
            _scopeTable.Controls.Add(_chkEnumAsString, 1, 6);
            _scopeTable.Controls.Add(_chkHonorNrt, 1, 7);
        }

        /// <summary>Reader options selected in the Scope stage, applied to the manager before planning.</summary>
        private EntityReadOptions BuildReadOptions() => new EntityReadOptions
        {
            EnumStorage = _chkEnumAsString?.Checked == true ? EnumStorageStrategy.String : EnumStorageStrategy.Int,
            HonorNullableReferenceTypes = _chkHonorNrt?.Checked ?? true
        };

        #region "IAddinVisSchema"
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string RootNodeName { get; set; } = "Configuration";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CatgoryName { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Order { get; set; } = 12;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ID { get; set; } = 12;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchText { get; set; } = "Migration Wizard";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Level { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BranchID { get; set; } = 12;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconImageName { get; set; } = "migration.svg";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchStatus { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ParentBranchID { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchDescription { get; set; } = "Plan, validate, dry-run, apply, and verify migrations.";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchClass { get; set; } = "ADDIN";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string AddinName { get; set; } = "uc_MigrationWizard";
        #endregion

        /// <summary>The plan built at the Plan stage, or null before Build Plan runs.</summary>
        public MigrationPlanArtifact? Plan => _plan;

        private void WireEvents()
        {
            _btnCancel.Click += BtnCancel_Click;
            _btnBack.Click += BtnBack_Click;
            _btnNext.Click += BtnNext_Click;
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

        /// <summary>
        /// Fills the connection list from ConfigEditor (the persisted connection store) and
        /// the environment tier list from the policy enum.
        /// </summary>
        private void PopulateChoices()
        {
            foreach (var tier in Enum.GetValues<MigrationEnvironmentTier>())
            {
                _cboEnvironment.ListItems.Add(new SimpleItem
                {
                    Text = tier.ToString(),
                    Value = tier
                });
            }
            _cboEnvironment.SelectedItem = _cboEnvironment.ListItems
                .FirstOrDefault(i => (MigrationEnvironmentTier)i.Value! == MigrationEnvironmentTier.Development);

            var connections = beepService?.DMEEditor?.ConfigEditor?.DataConnections;
            if (connections == null || connections.Count == 0)
            {
                SetStatus("No connections are configured. Add a connection before migrating.");
                return;
            }

            foreach (var conn in connections.Where(c => !string.IsNullOrWhiteSpace(c?.ConnectionName)))
            {
                _cboConnection.ListItems.Add(new SimpleItem
                {
                    Text = $"{conn.ConnectionName} ({conn.DatabaseType})",
                    Value = conn.ConnectionName
                });
            }
        }

        // ── navigation ────────────────────────────────────────────────────────

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            _runCts?.Cancel();
            Completed?.Invoke(this, new WizardCompletedEventArgs { Cancelled = true });
        }

        private void BtnBack_Click(object? sender, EventArgs e)
        {
            if (_busy || _stage == Stage.Scope) return;
            _stage = (Stage)((int)_stage - 1);
            UpdateStageUi();
        }

        private async void BtnNext_Click(object? sender, EventArgs e)
        {
            if (_busy) return;

            try
            {
                switch (_stage)
                {
                    case Stage.Scope:
                        if (await BuildPlanAsync())
                        {
                            _stage = Stage.Plan;
                            UpdateStageUi();
                        }
                        break;

                    case Stage.Plan:
                        if (await ValidatePlanAsync())
                        {
                            _stage = Stage.Safety;
                            UpdateStageUi();
                        }
                        break;

                    case Stage.Safety:
                        _stage = Stage.Run;
                        UpdateStageUi();
                        await ExecutePlanAsync();
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
                SetStatus($"Migration wizard error: {ex.Message}");
            }
        }

        private void UpdateStageUi()
        {
            _stepScope.Visible = _stage == Stage.Scope;
            _stepPlan.Visible = _stage == Stage.Plan;
            _stepSafety.Visible = _stage == Stage.Safety;
            _stepRun.Visible = _stage == Stage.Run;

            var active = _stage switch
            {
                Stage.Scope => (Control)_stepScope,
                Stage.Plan => _stepPlan,
                Stage.Safety => _stepSafety,
                _ => _stepRun
            };
            active.BringToFront();

            (_lblSubtitle.Text, _btnNext.Text) = _stage switch
            {
                Stage.Scope => ("Step 1 of 4 — choose the target connection and what to migrate.", "Build Plan"),
                Stage.Plan => ("Step 2 of 4 — review the operations this migration will perform.", "Validate"),
                Stage.Safety => ("Step 3 of 4 — policy, preflight, and dry-run results.", "Run Migration"),
                _ => ("Step 4 of 4 — execution progress and result.", "Finish")
            };

            _btnBack.Enabled = !_busy && _stage != Stage.Scope && _stage != Stage.Run;
            _btnNext.Enabled = !_busy && CanAdvance();
        }

        /// <summary>Gates Next on the state the current stage actually requires.</summary>
        private bool CanAdvance() => _stage switch
        {
            Stage.Scope => _cboConnection.SelectedItem != null,
            Stage.Plan => _plan != null && _plan.PendingOperationCount > 0,
            // A blocking policy finding or a failed preflight must not be executable.
            Stage.Safety => _plan != null
                            && !_plan.PolicyEvaluation.HasBlockingFindings
                            && _plan.PreflightReport.CanApply,
            _ => true
        };

        // ── stage 1 → plan ────────────────────────────────────────────────────

        private async Task<bool> BuildPlanAsync()
        {
            var editor = beepService?.DMEEditor;
            if (editor == null)
            {
                SetStatus("IDMEEditor is not available; cannot build a migration plan.");
                return false;
            }

            var connectionName = _cboConnection.SelectedItem?.Value as string;
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                SetStatus("Select a connection first.");
                return false;
            }

            var ns = string.IsNullOrWhiteSpace(_txtNamespace.Text) ? null : _txtNamespace.Text.Trim();
            bool detect = _chkDetectRelationships.Checked;
            bool fks = _chkApplyForeignKeys.Checked;
            bool indexes = _chkApplyIndexes.Checked;

            using var busy = BeginBusy("Building migration plan…");
            try
            {
                _dataSource = await Task.Run(() => editor.GetDataSource(connectionName)).ConfigureAwait(true);
                if (_dataSource == null)
                {
                    SetStatus($"Could not open datasource '{connectionName}'.");
                    return false;
                }

                var migration = new MigrationManager(editor, _dataSource)
                {
                    // Phase 7 (C1): honour the Scope-stage reader options (enum storage, NRT) so the
                    // plan is built from the model the way the operator chose.
                    ReadOptions = BuildReadOptions()
                };
                _migration = migration;

                _plan = await Task.Run(() =>
                    migration.BuildMigrationPlan(ns, null, detect, fks, indexes)).ConfigureAwait(true);

                // Resolve the same scope as concrete types so the Safety stage can inspect drift,
                // which is type-based rather than namespace-based.
                _planTypes = await Task.Run(() =>
                    (IReadOnlyList<Type>)migration.DiscoverEntityTypes(ns, null, true)).ConfigureAwait(true);

                // Drift is the one report held outside the artifact (InspectDrift is type-based, not
                // plan-based), so it needs an explicit reset; every other report ships fresh on the
                // new plan artifact.
                _drift = new Dictionary<string, SchemaDriftReport>();

                BindPlanRows(_plan);

                // Phase 9 (B1): show the version the target DB is currently at, alongside the plan.
                _currentDbVersion = TryReadDbVersion(editor, connectionName);

                _lblPlanSummary.Text =
                    $"Plan {_plan.PlanId[..Math.Min(8, _plan.PlanId.Length)]} — {_plan.PendingOperationCount} pending " +
                    $"operation(s) across {_plan.EntityTypeCount} entity type(s) on " +
                    $"{_plan.DataSourceName} ({_plan.DataSourceType}). " +
                    $"DB version: {_currentDbVersion ?? "(unversioned)"}.";

                SetStatus(_plan.PendingOperationCount == 0
                    ? "Nothing to migrate — the schema already matches the entity model."
                    : $"Plan built: {_plan.PendingOperationCount} pending operation(s).");

                return true;
            }
            catch (Exception ex)
            {
                SetStatus($"Plan build failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>Display-only DB version read — delegates straight to the engine's tracking service.</summary>
        private static string? TryReadDbVersion(IDMEEditor? editor, string? datasourceName)
            => editor == null || string.IsNullOrWhiteSpace(datasourceName)
                ? null
                : new MigrationTrackingService(editor).GetCurrentDatabaseVersion(datasourceName!)?.VersionString;

        /// <summary>
        /// Phase 9 (B2): records the DB version after a successful run. The engine owns the
        /// version-computation and persistence (<see cref="MigrationTrackingService.StampDatabaseVersion"/>);
        /// the view only forwards the declared version and shows the result.
        /// </summary>
        private void StampVersion(IMigrationManager migration, MigrationPlanArtifact plan)
        {
            var editor = migration.DMEEditor;
            if (editor == null || string.IsNullOrWhiteSpace(plan.DataSourceName)) return;

            var version = new MigrationTrackingService(editor)
                .StampDatabaseVersion(plan.DataSourceName, plan, DeclaredVersion);
            if (version != null)
            {
                _currentDbVersion = version.VersionString;
                AppendLog($"[version] '{plan.DataSourceName}' recorded at v{version.VersionString}.");
            }
        }

        private void BindPlanRows(MigrationPlanArtifact plan)
        {
            _planRows.Clear();
            foreach (var op in plan.Operations)
            {
                _planRows.Add(new MigrationPlanRow
                {
                    Entity = op.EntityName,
                    Operation = op.Kind.ToString(),
                    Risk = op.RiskLevel.ToString(),
                    Target = op.TargetName,
                    Destructive = op.IsDestructive,
                    Note = op.Note
                });
            }
        }

        // ── stage 2 → safety ──────────────────────────────────────────────────

        private async Task<bool> ValidatePlanAsync()
        {
            if (_migration == null || _plan == null)
            {
                SetStatus("Build a plan first.");
                return false;
            }

            var migration = _migration;
            var plan = _plan;
            _policyOptions = new MigrationPolicyOptions
            {
                EnvironmentTier = (MigrationEnvironmentTier)(_cboEnvironment.SelectedItem?.Value
                                                             ?? MigrationEnvironmentTier.Development)
            };
            var options = _policyOptions;
            var types = _planTypes;

            using var busy = BeginBusy("Running policy, dry-run, preflight, CI and governance gates…");
            try
            {
                // The engine writes each report back onto the plan artifact, so the
                // Safety gate reads them straight off _plan.
                await Task.Run(() =>
                {
                    plan.PolicyEvaluation = migration.EvaluateMigrationPlanPolicy(plan, options);
                    plan.DryRunReport = migration.GenerateDryRunReport(plan);
                    plan.PreflightReport = migration.RunPreflightChecks(plan, options);
                    plan.ImpactReport = migration.BuildImpactReport(plan);

                    // Drift shows drops/alters the additive-only plan diff cannot.
                    _drift = types.Count > 0
                        ? migration.InspectDrift(types)
                        : new Dictionary<string, SchemaDriftReport>();

                    // Needed before Run so a failure can tell the user whether an automatic
                    // rollback is actually possible for this plan.
                    plan.CompensationPlan = migration.BuildCompensationPlan(plan);

                    // Readiness is evidence-based: nothing here has confirmed a backup or a restore
                    // test, so both are reported false rather than assumed. The engine decides what
                    // that means for IsReady.
                    plan.RollbackReadinessReport = migration.CheckRollbackReadiness(
                        plan, backupConfirmed: false, restoreTestEvidenceProvided: false);

                    // Lock/runtime strategy and maintenance-window guidance.
                    plan.PerformancePlan = migration.BuildPerformancePlan(plan);

                    // The four documented CI gates: plan-lint, policy-check, dry-run-validation,
                    // portability-warning. Surfaced here so the desktop operator sees the same
                    // verdict a pipeline would.
                    plan.CiValidationReport = migration.ValidatePlanForCi(plan, options);

                    // Wave promotion + KPI/hard-stop evaluation.
                    plan.RolloutGovernanceReport = migration.EvaluateRolloutGovernance(plan);
                }).ConfigureAwait(true);

                BindFindings(plan);

                var blocking = plan.PolicyEvaluation.Findings.Count(f => f.Decision == MigrationPolicyDecision.Block);
                var warnings = plan.PolicyEvaluation.Findings.Count(f => f.Decision == MigrationPolicyDecision.Warn);

                var driftedEntities = _drift.Count(k => k.Value?.HasDrift == true);

                _lblSafetySummary.Text =
                    $"Policy: {plan.PolicyEvaluation.Decision} ({blocking} blocking, {warnings} warning). " +
                    $"Preflight: {(plan.PreflightReport.CanApply ? "can apply" : "blocked")}" +
                    $"{(plan.PreflightReport.SchemaDriftDetected ? ", schema drift detected" : string.Empty)}. " +
                    $"Drift: {driftedEntities} of {_drift.Count} entity(ies). " +
                    $"CI: {(plan.CiValidationReport.CanMerge ? "pass" : "block")}. " +
                    $"Governance: {(plan.RolloutGovernanceReport.CanPromote ? "promotable" : "blocked")}. " +
                    $"Rollback: {plan.CompensationPlan?.Actions?.Count ?? 0} compensation action(s), " +
                    $"ready={plan.RollbackReadinessReport.IsReady}.";

                SetStatus(blocking > 0
                    ? $"Blocked by {blocking} policy finding(s) — resolve before running."
                    : plan.PreflightReport.CanApply
                        ? "Validation passed."
                        : "Preflight blocked this plan.");

                return true;
            }
            catch (Exception ex)
            {
                SetStatus($"Validation failed: {ex.Message}");
                return false;
            }
        }

        private void BindFindings(MigrationPlanArtifact plan)
        {
            _lstFindings.ClearItems();
            var items = new List<SimpleItem>();

            // Provider capability profile + assumptions — BuildMigrationPlan already populates these
            // on the artifact; they explain *why* an operation is unsupported or risky here.
            var caps = plan.ProviderCapabilities;
            if (caps != null)
            {
                items.Add(new SimpleItem
                {
                    Text = $"[Capability] {caps.DataSourceType}/{caps.DataSourceCategory} — " +
                           $"alter={caps.SupportsAlterColumn}, renameEntity={caps.SupportsRenameEntity}, " +
                           $"renameColumn={caps.SupportsRenameColumn}, txDdl={caps.SupportsTransactionalDdl}, " +
                           $"fk={caps.SupportsForeignKeys}, idx={caps.SupportsIndexes}, " +
                           $"offlineWindow={caps.RequiresOfflineWindowForSchemaChanges}",
                    Value = caps
                });

                if (!string.IsNullOrWhiteSpace(caps.PortabilityWarning))
                    items.Add(new SimpleItem { Text = $"[Portability] {caps.PortabilityWarning}", Value = caps });

                foreach (var c in caps.Constraints)
                    items.Add(new SimpleItem { Text = $"[Constraint] {c}", Value = caps });
            }

            foreach (var a in plan.ProviderAssumptions)
                items.Add(new SimpleItem { Text = $"[Assumption] {a}" });

            foreach (var i in plan.ReadinessIssues)
                items.Add(new SimpleItem
                {
                    Text = $"[Readiness/{i.Severity}] {i.Code} — {i.Message}" +
                           (string.IsNullOrWhiteSpace(i.EntityName) ? "" : $" ({i.EntityName})") +
                           (string.IsNullOrWhiteSpace(i.Recommendation) ? "" : $" → {i.Recommendation}"),
                    Value = i
                });

            foreach (var f in plan.PolicyEvaluation.Findings)
            {
                items.Add(new SimpleItem
                {
                    Text = $"[{f.Decision}] {f.RuleId} — {f.Message}" +
                           (string.IsNullOrWhiteSpace(f.Recommendation) ? "" : $" → {f.Recommendation}"),
                    Value = f
                });
            }

            foreach (var c in plan.PreflightReport.Checks)
            {
                items.Add(new SimpleItem
                {
                    Text = $"[Preflight/{c.Decision}] {c.Code} — {c.Message}" +
                           (string.IsNullOrWhiteSpace(c.Recommendation) ? "" : $" → {c.Recommendation}"),
                    Value = c
                });
            }

            foreach (var op in plan.DryRunReport.Operations)
            {
                foreach (var ddl in op.DdlPreview)
                    items.Add(new SimpleItem { Text = $"[DDL/{op.EntityName}] {ddl}", Value = op });
            }

            // Impact was already computed but never shown. It is advisory: the engine derives
            // sensitivity from hardcoded heuristics and does not probe row counts or table size.
            foreach (var e in plan.ImpactReport?.Entries ?? new List<MigrationImpactEntry>())
            {
                var hints = e.UsageHints.Concat(e.DataVolumeIndicators).ToList();
                items.Add(new SimpleItem
                {
                    Text = $"[Impact/{e.Sensitivity}] {e.EntityName} {e.Kind}" +
                           (string.IsNullOrWhiteSpace(e.TargetName) ? "" : $" ({e.TargetName})") +
                           (hints.Count > 0 ? $" — {string.Join("; ", hints)}" : "") +
                           " (advisory — heuristic, no row-count probe)",
                    Value = e
                });
            }

            // Drift: the only view that shows removed/altered columns, since the plan diff is additive.
            foreach (var kv in _drift.Where(k => k.Value?.HasDrift == true))
            {
                var d = kv.Value;
                foreach (var f in d.AddedFields)
                    items.Add(new SimpleItem { Text = $"[Drift/{kv.Key}] model adds '{f.Name}' ({f.DataType}) — missing in database", Value = f });
                foreach (var f in d.RemovedFields)
                    items.Add(new SimpleItem { Text = $"[Drift/{kv.Key}] database has '{f.Name}' ({f.DataType}) — absent from model (not auto-dropped)", Value = f });
                foreach (var f in d.AlteredFields)
                    items.Add(new SimpleItem { Text = $"[Drift/{kv.Key}] '{f.FieldName}' type differs: model={f.BaselineType} vs database={f.CurrentType}. {f.Description} (AlterColumn is never auto-planned)", Value = f });
            }

            // CI gates — plan-lint, policy-check, dry-run-validation, portability-warning.
            var ci = plan.CiValidationReport;
            if (ci != null)
            {
                items.Add(new SimpleItem
                {
                    Text = $"[CI] {(ci.CanMerge ? "Would merge" : "Would BLOCK merge")} — {ci.Gates.Count} gate(s).",
                    Value = ci
                });
                foreach (var g in ci.Gates)
                    items.Add(new SimpleItem { Text = $"[CI/{g.Decision}] {g.Gate} — {g.Message}", Value = g });
            }

            // Rollout governance — wave promotion, KPI thresholds, hard stops.
            var gov = plan.RolloutGovernanceReport;
            if (gov != null)
            {
                items.Add(new SimpleItem
                {
                    Text = $"[Governance] wave={gov.Wave}, canPromote={gov.CanPromote}" +
                           (gov.HardStopTriggered ? $", HARD STOP: {gov.HardStopReason}" : ""),
                    Value = gov
                });
                foreach (var g in gov.Gates)
                    items.Add(new SimpleItem
                    {
                        Text = $"[Governance/{g.Decision}] {g.Gate} — observed {g.Observed} vs threshold {g.Threshold}. {g.Message}",
                        Value = g
                    });
            }

            // Rollback readiness — the engine's verdict, not a guess. Backup/restore evidence was
            // reported as absent, so IsReady reflects that honestly.
            var readiness = plan.RollbackReadinessReport;
            if (readiness != null)
            {
                items.Add(new SimpleItem
                {
                    Text = $"[Readiness] rollbackReady={readiness.IsReady}, " +
                           $"backupConfirmed={readiness.BackupConfirmed}, " +
                           $"restoreTested={readiness.RestoreTestEvidenceProvided}",
                    Value = readiness
                });
                foreach (var c in readiness.Checks)
                    items.Add(new SimpleItem
                    {
                        Text = $"[Readiness/{c.Decision}] {c.Code} — {c.Message}" +
                               (string.IsNullOrWhiteSpace(c.Recommendation) ? "" : $" → {c.Recommendation}"),
                        Value = c
                    });
            }

            // Performance/lock strategy. Advisory: the engine derives this from heuristics, not from
            // probing row counts or table sizes.
            var perf = plan.PerformancePlan;
            if (perf != null)
            {
                foreach (var g in perf.MaintenanceWindowGuidance)
                    items.Add(new SimpleItem { Text = $"[Performance] {g} (advisory — heuristic)", Value = perf });
                foreach (var g in perf.TimeoutGuidance)
                    items.Add(new SimpleItem { Text = $"[Timeout] {g} (advisory — heuristic)", Value = perf });
            }

            // Say plainly whether an automatic rollback exists for this plan.
            var compensationActions = plan.CompensationPlan?.Actions?.Count ?? 0;
            items.Add(new SimpleItem
            {
                Text = compensationActions > 0
                    ? $"[Rollback] {compensationActions} compensation action(s) available if execution fails."
                    : "[Rollback] No compensation actions — this plan is additive, so a failed run cannot be undone automatically. Fix forward."
            });

            // Datasource-specific guidance straight from the engine's recommendation profile.
            foreach (var p in _migration?.GetMigrationBestPractices(plan.DataSourceType, plan.DataSourceCategory)
                              ?? (IReadOnlyList<string>)Array.Empty<string>())
                items.Add(new SimpleItem { Text = $"[Best practice] {p}" });

            if (items.Count == 0)
                items.Add(new SimpleItem { Text = "No findings — nothing to report." });

            _lstFindings.AddItems(items);
            _lstFindings.RefreshItems();
        }

        // ── stage 3 → run ─────────────────────────────────────────────────────

        private async Task ExecutePlanAsync()
        {
            if (_migration == null || _plan == null) return;

            var migration = _migration;
            var plan = _plan;

            _runCts?.Cancel();
            _runCts?.Dispose();
            _runCts = new CancellationTokenSource();
            var token = _runCts.Token;

            _lstRunLog.ClearItems();
            _progress.Value = 0;

            using var busy = BeginBusy("Running migration…");
            try
            {
                var progress = new Progress<PassedArgs>(args =>
                {
                    if (args.ParameterInt1 > 0)
                        _progress.Value = Math.Clamp(args.ParameterInt1, 0, 100);
                    AppendLog(args.Messege ?? string.Empty);
                    _lblRunStatus.Text = args.Messege ?? _lblRunStatus.Text;
                });

                // Idempotency: a named migration is recorded once and gated on re-run. Only
                // successful records count, so a previous failure does not block a retry.
                var migrationName = $"{plan.DataSourceName}:{plan.PlanId}";
                if (migration.IsMigrationApplied(migrationName))
                {
                    AppendLog($"[idempotency] '{migrationName}' is already recorded as applied — nothing to do.");
                    _lblRunStatus.Text = "Already applied.";
                    SetStatus(_lblRunStatus.Text);
                    return;
                }

                var approved = await Task.Run(() =>
                    migration.ApproveMigrationPlan(plan, Environment.UserName, "Approved from Migration Wizard"),
                    token).ConfigureAwait(true);
                _plan = approved;

                // Retry transient faults rather than failing the whole run on the first blip, and
                // stop for a human on a hard fail. Passing null here meant the engine's defaults
                // were never tuned for an interactive operator.
                var execPolicy = new MigrationExecutionPolicy
                {
                    MaxTransientRetries = 3,
                    RetryDelayMilliseconds = 500,
                    RequireOperatorInterventionOnHardFail = true
                };

                // An explicit checkpoint token makes the run resumable after a crash — the engine
                // re-hydrates checkpoints from persisted history.
                var checkpoint = migration.CreateExecutionCheckpoint(approved);
                AppendLog($"[checkpoint] execution token {checkpoint.ExecutionToken}");

                var result = await migration
                    .ExecuteMigrationPlanAsync(approved, execPolicy, checkpoint.ExecutionToken, progress, token,
                        policyOptions: _policyOptions)
                    .ConfigureAwait(true);

                _progress.Value = result.Success ? 100 : _progress.Value;
                _lblRunStatus.Text = result.Success
                    ? $"Migration completed — {result.AppliedCount} operation(s) applied."
                    : $"Migration failed: {result.Message}";
                AppendLog(_lblRunStatus.Text);

                BindRunObservability(migration, result.ExecutionToken);

                if (result.Success)
                {
                    // Record it so a re-run is gated. Only successful records count.
                    var record = migration.RecordMigration(migrationName, success: true,
                        notes: $"Applied from Migration Wizard by {Environment.UserName}.");
                    AppendLog(record?.Flag == Errors.Ok
                        ? $"[idempotency] recorded '{migrationName}'."
                        : $"[idempotency] could not record '{migrationName}': {record?.Message}");

                    // Phase 9 (B2): stamp the new DB version — in the target database and the JSON mirror.
                    StampVersion(migration, plan);

                    ExportArtifacts(migration, _plan!);
                }
                else
                {
                    AppendLog($"Failed step(s): {string.Join(", ", result.FailedSteps)}");
                    await OfferResumeAsync(migration, result, execPolicy, progress).ConfigureAwait(true);
                    await OfferRollbackAsync(migration, result).ConfigureAwait(true);
                }

                SetStatus(_lblRunStatus.Text);
                Completed?.Invoke(this, new WizardCompletedEventArgs
                {
                    Succeeded = result.Success,
                    Summary = _lblRunStatus.Text
                });
            }
            catch (OperationCanceledException)
            {
                _lblRunStatus.Text = "Migration cancelled.";
                AppendLog(_lblRunStatus.Text);
                SetStatus(_lblRunStatus.Text);
            }
            catch (Exception ex)
            {
                _lblRunStatus.Text = $"Migration crashed: {ex.Message}";
                AppendLog(_lblRunStatus.Text);
                SetStatus(_lblRunStatus.Text);
            }
        }

        /// <summary>
        /// Surfaces the engine's own diagnostics and telemetry for a finished run.
        /// </summary>
        private void BindRunObservability(IMigrationManager migration, string executionToken)
        {
            foreach (var d in migration.GetMigrationDiagnostics(executionToken))
                AppendLog($"[{d.Severity}] {d.OperationCode} {d.EntityName} — {d.Message}" +
                          (string.IsNullOrWhiteSpace(d.Recommendation) ? "" : $" → {d.Recommendation}"));

            var telemetry = migration.GetMigrationTelemetrySnapshot(executionToken);
            if (telemetry != null)
                AppendLog($"[telemetry] {System.Text.Json.JsonSerializer.Serialize(telemetry)}");
        }

        /// <summary>
        /// Writes the engine's approval-evidence bundle next to the Beep config folder.
        /// </summary>
        /// <remarks>
        /// The bundle (plan JSON, dry-run, CI validation, approval markdown, performance,
        /// compensation, rollback readiness) is produced by <c>ExportMigrationArtifacts</c>; this
        /// only chooses where to put it.
        /// </remarks>
        private void ExportArtifacts(IMigrationManager migration, MigrationPlanArtifact plan)
        {
            try
            {
                var root = beepService?.DMEEditor?.ConfigEditor?.ConfigPath;
                if (string.IsNullOrWhiteSpace(root)) return;

                var bundle = migration.ExportMigrationArtifacts(plan, plan.CiValidationReport);
                if (bundle == null) return;

                var dir = Path.Combine(root, "migration", plan.PlanId);
                Directory.CreateDirectory(dir);

                WriteIfPresent(dir, "migration-plan.json", bundle.PlanJson);
                WriteIfPresent(dir, "migration-dryrun.json", bundle.DryRunJson);
                WriteIfPresent(dir, "migration-ci.json", bundle.CiValidationJson);
                WriteIfPresent(dir, "migration-performance.json", bundle.PerformancePlanJson);
                WriteIfPresent(dir, "migration-compensation.json", bundle.CompensationPlanJson);
                WriteIfPresent(dir, "migration-rollback-readiness.json", bundle.RollbackReadinessJson);
                WriteIfPresent(dir, "approval-report.md", bundle.ApprovalReportMarkdown);

                AppendLog($"[artifacts] wrote approval evidence to {dir}");
            }
            catch (Exception ex)
            {
                // Evidence export must never fail a migration that already succeeded.
                AppendLog($"[artifacts] export failed: {ex.Message}");
                beepService?.DMEEditor?.AddLogMessage("MigrationWizard",
                    $"Artifact export failed: {ex.Message}", DateTime.Now, 0, null, Errors.Warning);
            }
        }

        private static void WriteIfPresent(string dir, string name, string content)
        {
            if (!string.IsNullOrWhiteSpace(content))
                File.WriteAllText(Path.Combine(dir, name), content);
        }

        /// <summary>
        /// After a failed run, offers to resume from the last completed step rather than re-running
        /// the whole plan.
        /// </summary>
        /// <remarks>
        /// Checkpoints are re-hydrated from persisted history, so this survives a process restart.
        /// </remarks>
        private async Task OfferResumeAsync(
            IMigrationManager migration,
            MigrationExecutionResult result,
            MigrationExecutionPolicy execPolicy,
            IProgress<PassedArgs> progress)
        {
            if (string.IsNullOrWhiteSpace(result.ExecutionToken)) return;

            var checkpoint = migration.GetExecutionCheckpoint(result.ExecutionToken);
            if (checkpoint == null) return;

            AppendLog($"[checkpoint] last completed step: {checkpoint.LastCompletedStep}" +
                      (string.IsNullOrWhiteSpace(checkpoint.FailureCategory) ? "" : $", failure: {checkpoint.FailureCategory}"));

            if (MessageBox.Show(
                    $"The migration failed after '{checkpoint.LastCompletedStep}'.\n\n" +
                    "Resume from the last completed step? Choose No to leave it and decide on rollback.",
                    "Resume Migration", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var resumed = await Task.Run(() =>
                migration.ResumeMigrationPlan(result.ExecutionToken, execPolicy, progress, _policyOptions))
                .ConfigureAwait(true);

            AppendLog(resumed.Success
                ? $"[resume] completed — {resumed.AppliedCount} operation(s) applied."
                : $"[resume] failed: {resumed.Message}");

            if (resumed.Success)
            {
                _progress.Value = 100;
                _lblRunStatus.Text = "Migration completed after resume.";
            }
        }

        /// <summary>
        /// After a failed run, previews the rollback and — when there is actually something to undo —
        /// offers to execute it, leaving the decision with the user.
        /// </summary>
        /// <remarks>
        /// <c>BuildCompensationPlan</c> only emits actions for high-risk or relational operations, so
        /// a purely additive plan (CreateEntity / AddMissingColumns) yields an empty compensation
        /// plan and <c>RollbackFailedExecution</c> would report success having done nothing. Rather
        /// than offer a no-op, that case says so and stops. Note also that the preview defaults to
        /// dryRun: true — the real undo must pass dryRun: false explicitly.
        /// </remarks>
        private async Task OfferRollbackAsync(IMigrationManager migration, MigrationExecutionResult result)
        {
            if (string.IsNullOrWhiteSpace(result.ExecutionToken))
            {
                AppendLog("[rollback] No execution token was recorded — cannot roll back automatically.");
                return;
            }

            if ((_plan?.CompensationPlan?.Actions?.Count ?? 0) == 0)
            {
                AppendLog("[rollback] Compensation plan is empty — this plan is additive, so there is " +
                          "nothing to undo automatically. Resolve the failure and re-run (fix forward).");
                return;
            }

            var preview = await Task.Run(() =>
                migration.RollbackFailedExecution(result.ExecutionToken, dryRun: true)).ConfigureAwait(true);

            foreach (var action in preview.ExecutedActions)
                AppendLog($"[rollback preview] {action}");

            var confirm = MessageBox.Show(
                $"The migration failed and left a partial apply.\n\n" +
                $"Roll back now? {preview.ExecutedActions.Count} action(s) would run:\n\n" +
                string.Join(Environment.NewLine, preview.ExecutedActions.Take(8).Select(a => $"- {a}")) +
                (preview.ExecutedActions.Count > 8 ? $"{Environment.NewLine}… +{preview.ExecutedActions.Count - 8} more" : ""),
                "Roll Back Migration",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
            {
                AppendLog("[rollback] Declined — the partial apply is still in place.");
                return;
            }

            var rollback = await Task.Run(() =>
                migration.RollbackFailedExecution(result.ExecutionToken, dryRun: false)).ConfigureAwait(true);

            foreach (var action in rollback.ExecutedActions)
                AppendLog($"[rollback] {action}");

            AppendLog(rollback.Success
                ? "[rollback] Completed."
                : $"[rollback] Failed: {rollback.Message}");
        }

        private void AppendLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            _lstRunLog.AddItem(new SimpleItem { Text = message });
            _lstRunLog.RefreshItems();
        }

        // ── helpers ───────────────────────────────────────────────────────────

        private void SetStatus(string message) => _lblStatus.Text = message;

        /// <summary>
        /// Marks the wizard busy for the lifetime of an operation so Next/Back cannot
        /// re-enter it, and restores button state on dispose.
        /// </summary>
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
            private readonly uc_MigrationWizard _owner;
            public BusyScope(uc_MigrationWizard owner) => _owner = owner;

            public void Dispose()
            {
                _owner._busy = false;
                _owner.Cursor = Cursors.Default;
                _owner.UpdateStageUi();
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            _runCts?.Cancel();
            base.OnHandleDestroyed(e);
        }

        /// <summary>Grid-shaped projection of a <see cref="MigrationPlanOperation"/>.</summary>
        public sealed class MigrationPlanRow
        {
            public string Entity { get; set; } = string.Empty;
            public string Operation { get; set; } = string.Empty;
            public string Risk { get; set; } = string.Empty;
            public string Target { get; set; } = string.Empty;
            public bool Destructive { get; set; }
            public string Note { get; set; } = string.Empty;
        }

    }
}
