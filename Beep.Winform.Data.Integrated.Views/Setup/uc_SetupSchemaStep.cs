using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Migration;
using TheTechIdea.Beep.SetUp;
using TheTechIdea.Beep.SetUp.Steps;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Default.Views.Template;

// TheTechIdea.Beep.SetUp also publishes a MigrationSummary — a 4-property DTO for setup
// events. This step reports real database state, so it binds to the migration engine's
// summary (the type IMigrationManager.GetMigrationSummaryForTypes actually returns).
using MigrationSummary = TheTechIdea.Beep.Editor.Migration.MigrationSummary;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    // Full refactor: UI step wraps a typed SchemaSetupStepOptions directly.
    // The canonical SchemaSetupStep reads options.EntityTypeNames / options.ExtraAssemblies
    // during Execute. This shell keeps the CLR Types alongside, because its own survey
    // (GetMigrationSummaryForTypes) is type-based; the host supplies them.
    public partial class uc_SetupSchemaStep : TemplateUserControl
    {
        private IReadOnlyList<Type>? _entityTypes;
        private IReadOnlyList<Assembly>? _extraAssemblies;
        private IDMEEditor? _editor;
        private IDataSource? _dataSource;
        private MigrationSummary? _lastSummary;
        private SchemaSetupStepOptions? _options;
        private SetupContext? _context;
        private CancellationTokenSource? _refreshCts;

        public event EventHandler<SchemaSummaryEventArgs>? SummaryChanged;

        public uc_SetupSchemaStep()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Entity types this step will survey. Writing them also refreshes the options as portable
        /// <c>EntityTypeNames</c> — <c>SchemaSetupStepOptions.EntityTypes</c> is <c>[Obsolete]</c>
        /// because CLR Types can't be serialised into a SetupDefinition.
        /// </summary>
        public IReadOnlyList<Type>? EntityTypes
        {
            get => _entityTypes;
            set
            {
                _entityTypes = value;
                if (_options != null) uc_SetupWizard.ApplyEntityTypes(_options, value, _extraAssemblies);
                _ = TryRefreshSummaryAsync();
            }
        }

        public IReadOnlyList<Assembly>? ExtraAssemblies
        {
            get => _extraAssemblies;
            set
            {
                _extraAssemblies = value;
                if (_options != null) uc_SetupWizard.ApplyEntityTypes(_options, _entityTypes, value);
                _ = TryRefreshSummaryAsync();
            }
        }

        /// <summary>
        /// The last summary returned by <see cref="IMigrationManager.GetMigrationSummaryForTypes"/>,
        /// or null when no summary has been produced yet (no open datasource, no entity types,
        /// or the survey failed).
        /// </summary>
        public MigrationSummary? LastSummary => _lastSummary;

        public bool IsReadyForSetup()
        {
            return _editor != null
                && _dataSource != null
                && _dataSource.ConnectionStatus == ConnectionState.Open
                && _entityTypes != null
                && _entityTypes.Count > 0;
        }

        /// <summary>One-line human-readable summary for the review step.</summary>
        public string GetStepSummary()
        {
            if (_lastSummary == null) return "Schema: not summarized yet.";

            var summary = $"Schema: {_lastSummary.TotalPendingMigrations} pending migration(s) — " +
                          $"{_lastSummary.EntitiesToCreate.Count} to create, " +
                          $"{_lastSummary.EntitiesToUpdate.Count} to update, " +
                          $"{_lastSummary.EntitiesUpToDate.Count} up to date.";

            if (_lastSummary.Errors.Count > 0)
                summary += $" {_lastSummary.Errors.Count} error(s): {_lastSummary.Errors[0]}";

            return summary;
        }

        /// <summary>
        /// Bind the migration-summary UI to the typed <see cref="SchemaSetupStepOptions"/>.
        /// The wizard host passes the same options that will be handed to the canonical
        /// <see cref="SchemaSetupStep"/> at Execute time. Entity types and extra assemblies
        /// can be set directly on the options or via the legacy setters (which mirror back).
        /// </summary>
        public void InitializeStep(SchemaSetupStepOptions options, SetupContext context, IDMEEditor? editor = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            if (editor != null) _editor = editor;

            // Deliberately not read back from options: the options carry the portable
            // EntityTypeNames now, and resolving those to Types here would duplicate
            // SchemaSetupStep's private probe order (assemblyHandler → CLR → ExtraAssemblies scan).
            // The host sets EntityTypes straight after InitializeStep, since it already holds them.
            _extraAssemblies = options.ExtraAssemblies;
            _lastSummary = null;

            // Try to read the connection properties written by the previous (connection) step
            // from the canonical context — that's the live datasource.
            var cp = context.ConnectionProperties;
            if (cp != null)
            {
                _dataSource = _editor?.GetDataSource(cp.ConnectionName);
            }

            if (_dataSource != null && _dataSource.ConnectionStatus == ConnectionState.Open)
            {
                _ = TryRefreshSummaryAsync();
            }
            else
            {
                ShowTasks(
                    "- Build migration plan from entities (no open datasource).",
                    "- Run policy and preflight checks (requires open datasource).",
                    "- Execute migration and save checkpoint token.");
                SummaryChanged?.Invoke(this, new SchemaSummaryEventArgs(0, 0, "Schema step is not connected to an open datasource yet."));
            }
        }

        /// <summary>
        /// Surveys the migration datasource through the canonical
        /// <see cref="MigrationManager"/> and reports what the schema step will actually do.
        /// The survey compares the entity types against live database state, so it runs off
        /// the UI thread.
        /// </summary>
        private async Task TryRefreshSummaryAsync()
        {
            if (_editor == null || _dataSource == null || _entityTypes == null || _entityTypes.Count == 0)
                return;

            // Property setters fire this without awaiting, so a newer survey must win.
            _refreshCts?.Cancel();
            var cts = new CancellationTokenSource();
            _refreshCts = cts;
            var token = cts.Token;

            var editor = _editor;
            var dataSource = _dataSource;
            var entityTypes = _entityTypes.ToList();

            try
            {
                ShowTasks(
                    $"- Surveying {entityTypes.Count} entit{(entityTypes.Count == 1 ? "y" : "ies")} against {dataSource.DatasourceName}…",
                    "- Run policy and preflight checks.",
                    "- Execute migration and save checkpoint token.");

                var summary = await Task.Run(() =>
                {
                    var migration = new MigrationManager(editor, dataSource);
                    return migration.GetMigrationSummaryForTypes(entityTypes, detectRelationships: true);
                }, token).ConfigureAwait(true);

                if (token.IsCancellationRequested) return;

                _lastSummary = summary;

                var create = summary.EntitiesToCreate.Count;
                var update = summary.EntitiesToUpdate.Count;
                var upToDate = summary.EntitiesUpToDate.Count;

                var task2 = summary.Errors.Count > 0
                    ? $"- Policy and preflight blocked: {summary.Errors[0]}"
                    : summary.HasPendingMigrations
                        ? "- Run policy and preflight checks before applying."
                        : "- Nothing to apply; schema already matches the entity model.";

                ShowTasks(
                    $"- Build migration plan: {create} to create, {update} to update, {upToDate} up to date.",
                    task2,
                    "- Execute migration and save checkpoint token.");

                SummaryChanged?.Invoke(this,
                    new SchemaSummaryEventArgs(summary.TotalPendingMigrations, entityTypes.Count, GetStepSummary()));
            }
            catch (OperationCanceledException)
            {
                // Superseded by a newer survey — leave the newer one's output in place.
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested) return;

                _lastSummary = null;
                ShowTasks(
                    "- Build migration plan: " + ex.Message,
                    "- Policy checks unavailable.",
                    "- Execution blocked until errors are fixed.");
                SummaryChanged?.Invoke(this, new SchemaSummaryEventArgs(0, entityTypes.Count, $"Schema survey failed: {ex.Message}"));
            }
            finally
            {
                if (ReferenceEquals(_refreshCts, cts)) _refreshCts = null;
                cts.Dispose();
            }
        }

        /// <summary>
        /// Applies the three task lines. InitializeStep runs before the control is parented,
        /// so the handle usually does not exist yet — assigning directly is correct on the UI
        /// thread and only a created handle needs marshalling.
        /// </summary>
        private void ShowTasks(string task1, string task2, string task3)
        {
            if (IsDisposed || Disposing) return;

            if (IsHandleCreated && InvokeRequired)
            {
                BeginInvoke(new Action(() => ShowTasks(task1, task2, task3)));
                return;
            }

            _lblTask1.Text = task1;
            _lblTask2.Text = task2;
            _lblTask3.Text = task3;
        }

        // Dispose(bool) belongs to the designer file; cancel an in-flight survey here so a
        // late continuation cannot touch a torn-down control.
        protected override void OnHandleDestroyed(EventArgs e)
        {
            _refreshCts?.Cancel();
            base.OnHandleDestroyed(e);
        }

        /// <summary>
        /// Overlays DPI-scaled padding on the Designer's design-time pixels.
        /// </summary>
        /// <remarks>
        /// Invoked by TemplateUserControl from OnHandleCreated and OnDpiChangedAfterParent — never
        /// from the ctor, where DpiScalingHelper reports a scale of 1.0 because the handle does not
        /// exist yet and nothing would actually scale.
        /// <para>
        /// Only the docked panels' padding is scaled. Their children are all Dock=Top/Fill, so the
        /// layout reflows from the padding alone; pushing size tokens onto individual controls is
        /// what broke uc_ImportStep5_Run, whose Designer positions its row absolutely.
        /// </para>
        /// </remarks>
        protected override void ApplyDpiScaledLayout()
        {
            _rootPanel.Padding = BeepLayoutMetrics.DialogPadding.ScalePadding(this);
        }

    }

    public sealed class SchemaSummaryEventArgs : EventArgs
    {
        public SchemaSummaryEventArgs(int pendingMigrations, int entityTypeCount, string message)
        {
            PendingMigrations = pendingMigrations;
            EntityTypeCount = entityTypeCount;
            Message = message;
        }

        public int PendingMigrations { get; }
        public int EntityTypeCount { get; }
        public string Message { get; }


    }
}
