using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private readonly BindingList<MigrationPlanRow> _planRows = new();

        public uc_MigrationWizard() : this(null) { }
        public uc_MigrationWizard(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            Details.AddinName = "Migration Wizard";
            WireEvents();
            ApplyDpiScaledLayout();
            PopulateChoices();
            _gridPlan.DataSource = _planRows;
            UpdateStageUi();
        }

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

        private void ApplyDpiScaledLayout()
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

                var migration = new MigrationManager(editor, _dataSource);
                _migration = migration;

                _plan = await Task.Run(() =>
                    migration.BuildMigrationPlan(ns, null, detect, fks, indexes)).ConfigureAwait(true);

                BindPlanRows(_plan);

                _lblPlanSummary.Text =
                    $"Plan {_plan.PlanId[..Math.Min(8, _plan.PlanId.Length)]} — {_plan.PendingOperationCount} pending " +
                    $"operation(s) across {_plan.EntityTypeCount} entity type(s) on " +
                    $"{_plan.DataSourceName} ({_plan.DataSourceType}).";

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

            using var busy = BeginBusy("Running policy, dry-run, and preflight…");
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
                }).ConfigureAwait(true);

                BindFindings(plan);

                var blocking = plan.PolicyEvaluation.Findings.Count(f => f.Decision == MigrationPolicyDecision.Block);
                var warnings = plan.PolicyEvaluation.Findings.Count(f => f.Decision == MigrationPolicyDecision.Warn);

                _lblSafetySummary.Text =
                    $"Policy: {plan.PolicyEvaluation.Decision} ({blocking} blocking, {warnings} warning). " +
                    $"Preflight: {(plan.PreflightReport.CanApply ? "can apply" : "blocked")}" +
                    $"{(plan.PreflightReport.SchemaDriftDetected ? ", schema drift detected" : string.Empty)}.";

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

                var approved = await Task.Run(() =>
                    migration.ApproveMigrationPlan(plan, Environment.UserName, "Approved from Migration Wizard"),
                    token).ConfigureAwait(true);
                _plan = approved;

                var result = await migration
                    .ExecuteMigrationPlanAsync(approved, null, null, progress, token)
                    .ConfigureAwait(true);

                _progress.Value = result.Success ? 100 : _progress.Value;
                _lblRunStatus.Text = result.Success
                    ? $"Migration completed — {result.AppliedCount} operation(s) applied."
                    : $"Migration failed: {result.Message}";
                AppendLog(_lblRunStatus.Text);

                if (!result.Success)
                {
                    // Surface what rollback would do rather than silently leaving a partial apply.
                    var rollback = await Task.Run(() =>
                        migration.RollbackFailedExecution(result.ExecutionToken, dryRun: true)).ConfigureAwait(true);
                    foreach (var action in rollback.ExecutedActions)
                        AppendLog($"[rollback preview] {action}");
                    AppendLog($"Failed step(s): {string.Join(", ", result.FailedSteps)}");
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

        public sealed class WizardCompletedEventArgs : EventArgs
        {
            public bool Succeeded { get; init; }
            public bool Cancelled { get; init; }
            public string Summary { get; init; } = string.Empty;
        }
    }
}
