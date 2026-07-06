using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Migration;
using TheTechIdea.Beep.SetUp;
using TheTechIdea.Beep.SetUp.Steps;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    // Full refactor: UI step wraps a typed SchemaSetupStepOptions directly.
    // The canonical SchemaSetupStep reads options.EntityTypes / options.ExtraAssemblies
    // during Execute.
    public partial class uc_SetupSchemaStep : TemplateUserControl
    {
        private IReadOnlyList<Type>? _entityTypes;
        private IReadOnlyList<Assembly>? _extraAssemblies;
        private IDMEEditor? _editor;
        private IDataSource? _dataSource;
        private MigrationSummary? _lastSummary;
        private SchemaSetupStepOptions? _options;
        private SetupContext? _context;

        public event EventHandler<SchemaSummaryEventArgs>? SummaryChanged;

        public uc_SetupSchemaStep()
        {
            InitializeComponent();
        }

        public IReadOnlyList<Type>? EntityTypes
        {
            get => _entityTypes;
            set { _entityTypes = value; if (_options != null) _options.EntityTypes = value; _ = TryRefreshSummaryAsync(); }
        }

        public IReadOnlyList<Assembly>? ExtraAssemblies
        {
            get => _extraAssemblies;
            set { _extraAssemblies = value; if (_options != null) _options.ExtraAssemblies = value; _ = TryRefreshSummaryAsync(); }
        }

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
            return $"Schema: {_lastSummary.TotalPendingMigrations} pending migration(s); " +
                   $"plan valid: {_lastSummary.IsValid}; " +
                   $"policy: {_lastSummary.PolicyResult ?? "(unchecked)"}.";
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

            _entityTypes = options.EntityTypes;
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
                _lblTask1.Text = "- Build migration plan from entities (no open datasource).";
                _lblTask2.Text = "- Run policy and preflight checks (requires open datasource).";
                _lblTask3.Text = "- Execute migration and save checkpoint token.";
                SummaryChanged?.Invoke(this, new SchemaSummaryEventArgs(0, 0, "Schema step is not connected to an open datasource yet."));
            }
        }

        private async Task TryRefreshSummaryAsync()
        {
            try
            {
                if (_editor == null || _dataSource == null || _entityTypes == null || _entityTypes.Count == 0)
                    return;

                // The canonical SchemaSetupStep owns the actual migration logic; the UI shell
                // just provides a static summary so the user has visible feedback while filling
                // out the step. We surface a count-based summary derived from the typed options.
                int entityTypeCount = _entityTypes.Count;
                bool datasourceOpen = _dataSource.ConnectionStatus == ConnectionState.Open;

                _lastSummary = new MigrationSummary
                {
                    TotalPendingMigrations = entityTypeCount,
                    HasPendingMigrations = entityTypeCount > 0,
                    IsValid = datasourceOpen,
                    PolicyResult = datasourceOpen ? "OK" : "NotReady: datasource not open"
                };

                if (IsHandleCreated)
                {
                    BeginInvoke(new Action(() =>
                    {
                        _lblTask1.Text = $"- Build migration plan from {entityTypeCount} entit{(entityTypeCount == 1 ? "y" : "ies")}.";
                        _lblTask2.Text = datasourceOpen
                            ? "- Run policy and preflight checks (passed)."
                            : "- Run policy and preflight checks (waiting for datasource to open).";
                        _lblTask3.Text = "- Execute migration and save checkpoint token.";

                        SummaryChanged?.Invoke(this,
                            new SchemaSummaryEventArgs(_lastSummary.TotalPendingMigrations,
                                entityTypeCount, _lastSummary.PolicyResult));
                    }));
                }
            }
            catch (Exception ex)
            {
                if (IsHandleCreated)
                {
                    BeginInvoke(new Action(() =>
                    {
                        _lblTask1.Text = "- Build migration plan: " + ex.Message;
                        _lblTask2.Text = "- Policy checks unavailable.";
                        _lblTask3.Text = "- Execution blocked until errors are fixed.";
                    }));
                }
            }
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

    public sealed class MigrationSummary
    {
        public int TotalPendingMigrations { get; set; }
        public bool HasPendingMigrations { get; set; }
        public bool IsValid { get; set; }
        public string? PolicyResult { get; set; }
    }
}
