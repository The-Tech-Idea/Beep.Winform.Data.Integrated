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
using TheTechIdea.Beep.SetUp.Steps;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Wizards;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    public partial class uc_SetupSchemaStep : UserControl, IWizardStepContent
    {
        private IReadOnlyList<Type>? _entityTypes;
        private IReadOnlyList<Assembly>? _extraAssemblies;
        private IDMEEditor? _editor;
        private IDataSource? _dataSource;
        private MigrationSummary? _lastSummary;
        private WizardContext? _wizardContext;
        private bool _isComplete;

        public event EventHandler<SchemaSummaryEventArgs>? SummaryChanged;

        public uc_SetupSchemaStep()
        {
            InitializeComponent();
        }

        public bool IsComplete
        {
            get => _isComplete;
            private set
            {
                if (_isComplete == value) return;
                _isComplete = value;
                ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(_isComplete, _isComplete ? string.Empty : "Schema summary not ready"));
            }
        }

        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;

        public string NextButtonText { get; set; } = string.Empty;

        public void InitializeStep(IDMEEditor? editor, IDataSource? dataSource,
            IReadOnlyList<Type>? entityTypes = null,
            IReadOnlyList<Assembly>? extraAssemblies = null)
        {
            _editor = editor;
            _dataSource = dataSource;
            _entityTypes = entityTypes;
            _extraAssemblies = extraAssemblies;
            _lastSummary = null;

            if (editor != null && dataSource != null && dataSource.ConnectionStatus == ConnectionState.Open)
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

        public IReadOnlyList<Type>? EntityTypes
        {
            get => _entityTypes;
            set { _entityTypes = value; _ = TryRefreshSummaryAsync(); }
        }

        public IReadOnlyList<Assembly>? ExtraAssemblies
        {
            get => _extraAssemblies;
            set { _extraAssemblies = value; _ = TryRefreshSummaryAsync(); }
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

        public SchemaSetupStepOptions BuildStepOptions()
        {
            if (!IsReadyForSetup() || _editor == null)
                throw new InvalidOperationException("Schema step is not ready. Connect a datasource and ensure entity types are set.");

            return new SchemaSetupStepOptions
            {
                EntityTypes = _entityTypes,
                ExtraAssemblies = _extraAssemblies,
                DetectRelationships = true
            };
        }

        public string GetStepSummary()
        {
            if (!IsReadyForSetup())
                return "Schema: Waiting for an open datasource.";

            if (_lastSummary == null)
                return "Schema: Pending migration summary.";

            if (!_lastSummary.HasPendingMigrations)
                return "Schema: Already up-to-date (no pending migrations).";

            return $"Schema: {_lastSummary.TotalPendingMigrations} pending migration(s) will be applied.";
        }

        private async System.Threading.Tasks.Task TryRefreshSummaryAsync()
        {
            if (!IsReadyForSetup() || _editor == null || _dataSource == null)
                return;

            try
            {
                var migration = new MigrationManager(_editor, _dataSource);
                if (_extraAssemblies != null)
                {
                    foreach (var asm in _extraAssemblies)
                        migration.RegisterAssembly(asm);
                }

                _lastSummary = await System.Threading.Tasks.Task.Run(() =>
                    BuildSummarySafely(migration, _entityTypes)).ConfigureAwait(true);

                if (IsDisposed) return;
                if (InvokeRequired)
                    BeginInvoke(new Action(SafeUpdateTaskLabels));
                else
                    SafeUpdateTaskLabels();
            }
            catch (Exception ex)
            {
                if (IsDisposed) return;
                if (InvokeRequired)
                    BeginInvoke(new Action(() => UpdateTaskLabelsForError(ex.Message)));
                else
                    UpdateTaskLabelsForError(ex.Message);
            }
        }

        private void SafeUpdateTaskLabels()
        {
            try { UpdateTaskLabels(); }
            catch (Exception ex) { UpdateTaskLabelsForError(ex.Message); }
        }

        /// <summary>
        /// Build a migration summary in a way that is robust across
        /// different versions of the underlying
        /// <c>DataManagementEngineStandard</c> assembly. The schema step
        /// must compile even when the referenced BCL does not yet expose
        /// <c>GetMigrationSummaryForTypes</c>.
        /// </summary>
        private static MigrationSummary BuildSummarySafely(MigrationManager migration, IReadOnlyList<Type>? entityTypes)
        {
            // Try the most specific API first via reflection so a newer
            // BCL can use it; fall back to the always-present discovery
            // API otherwise. Reflection is used here purely as a version
            // bridge — the call site is deterministic at runtime.
            try
            {
                if (entityTypes != null && entityTypes.Count > 0)
                {
                    var method = typeof(MigrationManager).GetMethod(
                        "GetMigrationSummaryForTypes",
                        new[] { typeof(System.Collections.Generic.IEnumerable<Type>) });
                    if (method != null)
                    {
                        var result = method.Invoke(migration, new object[] { entityTypes });
                        if (result is MigrationSummary ms) return ms;
                    }
                }
            }
            catch
            {
                // fall through
            }

            return migration.GetMigrationSummary();
        }

        private void UpdateTaskLabels()
        {
            if (_lastSummary == null)
                return;

            if (!_lastSummary.HasPendingMigrations)
            {
                _lblTask1.Text = "- No migration plan needed (schema up-to-date).";
                _lblTask2.Text = "- Policy and preflight checks skipped (no changes).";
                _lblTask3.Text = "- No execution required.";
            }
            else
            {
                _lblTask1.Text = $"- Build migration plan for {_entityTypes?.Count ?? 0} entity type(s). {_lastSummary.TotalPendingMigrations} change(s) detected.";
                _lblTask2.Text = "- Run policy and preflight checks against the active environment tier.";
                _lblTask3.Text = "- Execute migration plan and persist the resulting checkpoint token.";
            }

            SummaryChanged?.Invoke(this, new SchemaSummaryEventArgs(
                _lastSummary.TotalPendingMigrations,
                _entityTypes?.Count ?? 0,
                _lastSummary.HasPendingMigrations ? "Schema has pending changes." : "Schema is up-to-date."));
        }

        private void UpdateTaskLabelsForError(string error)
        {
            _lblTask1.Text = "- Migration summary could not be generated.";
            _lblTask2.Text = "- Verify the datasource is open and entity types are reachable.";
            _lblTask3.Text = $"- Last error: {error}";
            SummaryChanged?.Invoke(this, new SchemaSummaryEventArgs(0, 0, error));
        }

        public void ApplyTheme(string theme)
        {
            ApplyThemeToControl(_rootPanel, theme);
            ApplyThemeToControl(_lblTitle, theme);
            ApplyThemeToControl(_lblDescription, theme);
            ApplyThemeToControl(_lblTask1, theme);
            ApplyThemeToControl(_lblTask2, theme);
            ApplyThemeToControl(_lblTask3, theme);
        }

        private static void ApplyThemeToControl(Control control, string theme)
        {
            if (control is IBeepUIComponent beepComponent)
                beepComponent.Theme = theme;
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

        async void IWizardStepContent.OnStepEnter(WizardContext context)
        {
            _wizardContext = context;

            try
            {
                // Try to get an open datasource from the connection step via context
                if (_dataSource == null || _dataSource.ConnectionStatus != ConnectionState.Open)
                {
                    var connName = context.GetValue<string>("connectionName");
                    if (!string.IsNullOrWhiteSpace(connName) && _editor != null)
                    {
                        try
                        {
                            var ds = _editor.GetDataSource(connName);
                            if (ds != null && ds.ConnectionStatus == ConnectionState.Open)
                                _dataSource = ds;
                        }
                        catch { }
                    }
                }

                // Also try to get entity types from context
                if (_entityTypes == null || _entityTypes.Count == 0)
                {
                    _entityTypes = context.GetValue<IReadOnlyList<Type>>("entityTypes");
                }

                if (_dataSource != null && _dataSource.ConnectionStatus == ConnectionState.Open)
                {
                    await TryRefreshSummaryAsync().ConfigureAwait(true);
                    SafeSetIsComplete(_lastSummary != null);
                }
                else
                {
                    SafeSetIsComplete(false);
                }
            }
            catch (Exception ex)
            {
                // The interface signature is `void OnStepEnter` — an unhandled
                // throw would crash the wizard. Route to the error label and
                // mark the step incomplete so the user can move on / retry.
                if (InvokeRequired)
                    BeginInvoke(new Action(() => UpdateTaskLabelsForError(ex.Message)));
                else
                    UpdateTaskLabelsForError(ex.Message);

                SafeSetIsComplete(false);
            }
        }

        private void SafeSetIsComplete(bool value)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => IsComplete = value));
            }
            else
            {
                IsComplete = value;
            }
        }

        void IWizardStepContent.OnStepLeave(WizardContext context)
        {
            if (_lastSummary != null)
            {
                context.SetValue("schemaSummary", _lastSummary);
                context.SetValue("schemaHasPendingMigrations", _lastSummary.HasPendingMigrations);
                context.SetValue("schemaPendingCount", _lastSummary.TotalPendingMigrations);
            }
        }

        WizardValidationResult IWizardStepContent.Validate()
        {
            if (!IsReadyForSetup())
                return WizardValidationResult.Error("Schema step is not ready. Connect a datasource and ensure entity types are set.");

            if (_lastSummary == null)
                return WizardValidationResult.Error("Migration summary has not been generated. Verify the datasource is open.");

            return WizardValidationResult.Success();
        }

        Task<WizardValidationResult> IWizardStepContent.ValidateAsync()
        {
            return Task.FromResult(((IWizardStepContent)this).Validate());
        }
    }
}
