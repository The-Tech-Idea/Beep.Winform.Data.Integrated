using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.SetUp;
using TheTechIdea.Beep.SetUp.Adapters;
using TheTechIdea.Beep.SetUp.Security;
using TheTechIdea.Beep.SetUp.Seeding;
using TheTechIdea.Beep.SetUp.State;
using TheTechIdea.Beep.SetUp.Steps;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    [AddinAttribute(Caption = "Setup Wizard", Name = "uc_SetupWizard",
        misc = "Configuration", menu = "Configuration", addinType = AddinType.Control,
        displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 5, RootNodeName = "Configuration", Order = 5, ID = 5,
        BranchText = "Setup Wizard", BranchType = EnumPointType.Function,
        IconImageName = "settings.svg", BranchClass = "ADDIN",
        BranchDescription = "Guided platform setup for driver, connection, schema, and seeding")]
    /// <summary>
    /// WinForms shell for the canonical <c>TheTechIdea.Beep.SetUp</c> wizard.
    /// </summary>
    /// <remarks>
    /// Implements <see cref="ISetupWizardFactory"/> so <c>BeepBootstrapper</c> — the framework's
    /// first-run orchestrator — can run <em>this</em> configured wizard rather than
    /// <c>DefaultSetupWizardFactory.CreateDefault</c>, which deliberately builds steps with empty
    /// options and would fail validation.
    /// </remarks>
    public partial class uc_SetupWizard : TemplateUserControl, IAddinVisSchema, ISetupWizardFactory
    {
        // ── IAddinVisSchema (unchanged) ─────────────────────────────────────
        public string RootNodeName { get; set; } = "Configuration";
        public string CatgoryName { get; set; } = string.Empty;
        public int Order { get; set; } = 5;
        public int ID { get; set; } = 5;
        public string BranchText { get; set; } = "Setup Wizard";
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        public int BranchID { get; set; } = 5;
        public string IconImageName { get; set; } = "settings.svg";
        public string BranchStatus { get; set; } = string.Empty;
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; } =
            "Guided platform setup for driver, connection, schema, and seeding";
        public string BranchClass { get; set; } = "ADDIN";
        public string AddinName { get; set; } = "uc_SetupWizard";

        // ── View state (no fallback paths, no external executor) ──────────────
        private readonly SetupWizardViewModel _viewModel = new();
        private readonly IServiceProvider? _services;

        private uc_SetupDriverStep? _driverStepControl;
        private uc_SetupConnectionStep? _connectionStepControl;
        private uc_SetupSchemaStep? _schemaStepControl;
        private uc_SetupSeedingStep? _seedingStepControl;
        private uc_SetupReviewRunStep? _reviewStepControl;

        // The canonical wizard the host built on Launch.
        private SetupPieces? _pieces;
        private CancellationTokenSource? _runCts;

        /// <summary>
        /// The framework's WinForms/WPF bridge. Owns the run loop (worker thread, progress plumbing,
        /// uniform cancel/exception handling, GetReport) and, once handed to the builder via
        /// <c>WithAdapter</c>, receives ShowStep per step from <c>SetupWizard.Run</c>.
        /// </summary>
        private readonly DesktopSetupWizardAdapter _adapter = new();

        /// <summary>
        /// The framework adapter this shell drives. Hand it to <c>BeepBootstrapper</c> so the run
        /// reports progress through this control.
        /// </summary>
        public ISetupWizardAdapter Adapter => _adapter;

        // ── ISetupWizardFactory ──────────────────────────────────────────────

        /// <summary>
        /// Returns the wizard this shell configured, building it on first use.
        /// </summary>
        /// <remarks>
        /// Returns the cached pieces when present — rebuilding would create fresh options objects
        /// and silently discard everything the user typed into the step UIs, which hold references
        /// to the existing instances.
        /// </remarks>
        public (ISetupWizard wizard, SetupContext context) CreateDefault(IDMEEditor editor)
        {
            _pieces ??= BuildSetupPieces();
            return (_pieces.Wizard, _pieces.Context);
        }

        /// <inheritdoc/>
        public (ISetupWizard wizard, SetupContext context) Create(
            IDMEEditor editor, SetupOptions options, Action<ISetupWizardBuilder> configure)
            => new DefaultSetupWizardFactory().Create(editor, options, configure);

        /// <summary>Raised for every progress report the adapter forwards, on a worker thread.</summary>
        private event Action<PassedArgs>? AdapterProgress;

        private ISeederRegistry? _seederRegistry;
        private IReadOnlyList<Type>? _entityTypes;
        private IReadOnlyList<Assembly>? _extraAssemblies;

        // Fired when the canonical Wizard.RunAsync completes (success or failure). MainFrm and
        // MainFrm_Tree listen to this to react to setup completion.
        public event EventHandler<SetupCompletedEventArgs>? SetupCompleted;

        public ISeederRegistry? SeederRegistry
        {
            get => _seederRegistry;
            set
            {
                _seederRegistry = value;
                if (_pieces != null) _pieces.SeedingOpts.Registry = value;
            }
        }

        /// <summary>
        /// Entity types whose schema setup will create. Converted to
        /// <see cref="SchemaSetupStepOptions.EntityTypeNames"/> when the wizard is built.
        /// </summary>
        /// <remarks>
        /// <c>SchemaSetupStepOptions.EntityTypes</c> is <c>[Obsolete]</c> — CLR Types can't be
        /// serialised into a SetupDefinition, which blocks versioning, CLI/CI use, and remote
        /// storage. This property stays (it is the convenient in-process API and the designer-facing
        /// surface) but it now feeds the portable <c>EntityTypeNames</c> instead of the legacy list.
        /// </remarks>
        public IReadOnlyList<Type>? EntityTypes
        {
            get => _entityTypes;
            set
            {
                _entityTypes = value;
                if (_pieces != null) ApplyEntityTypes(_pieces.SchemaOpts, value, _extraAssemblies);
            }
        }

        /// <summary>
        /// Target environment label. Drives <c>MigrationEnvironmentTier</c> inside
        /// <c>SchemaSetupStep</c>, which decides whether high-risk changes need approval and whether
        /// destructive ones are blocked outright.
        /// </summary>
        /// <remarks>
        /// Defaults to "Production" to preserve this wizard's long-standing behaviour — that is the
        /// strictest tier, so lowering it is a deliberate act by the host, never an accident here.
        /// (<see cref="SetupOptions.Environment"/>'s own default is "Development".)
        /// </remarks>
        public string Environment { get; set; } = "Production";

        /// <summary>
        /// When true, a failed run automatically rolls back its completed steps in reverse.
        /// </summary>
        /// <remarks>
        /// Opt-in by design: undoing a partial setup can destroy the state needed to diagnose the
        /// failure. The outcome is written to <c>SetupReport.RollbackReportJson</c> either way.
        /// </remarks>
        public bool AutoRollbackOnFailure { get; set; }

        /// <summary>
        /// When true, plans and dry-run reports are produced but nothing is applied.
        /// </summary>
        /// <remarks>
        /// Only <c>SchemaSetupStep</c> does real dry-run work (it stores a DDL preview on the
        /// context); every other step simply reports itself skippable.
        /// </remarks>
        public bool DryRun { get; set; }

        /// <summary>
        /// Requires rollback readiness (confirmed backup + restore evidence) before schema changes.
        /// </summary>
        public bool StrictPolicyMode { get; set; }

        /// <summary>
        /// When true (default), later launches run the version-gate upgrade pass — comparing the
        /// declared schema version and the entity model against the version recorded in the target
        /// database, and applying pending migrations. Maps to <see cref="SetupOptions.MigrateOnStartup"/>.
        /// </summary>
        public bool MigrateOnStartup { get; set; } = true;

        /// <summary>
        /// Optional declared schema version (e.g. "2.3.0"). Wins over any <c>[AppSchemaVersion]</c>
        /// assembly attribute; when null the version gate falls back to entity-diff only. Maps to
        /// <see cref="SetupOptions.DeclaredSchemaVersion"/>.
        /// </summary>
        public string? DeclaredSchemaVersion { get; set; }

        public IReadOnlyList<Assembly>? ExtraAssemblies
        {
            get => _extraAssemblies;
            set
            {
                _extraAssemblies = value;
                if (_pieces != null) ApplyEntityTypes(_pieces.SchemaOpts, _entityTypes, value);
            }
        }

        /// <summary>
        /// Writes entity types onto the options as portable <c>EntityTypeNames</c>, and makes sure
        /// the assemblies declaring them are probe-able.
        /// </summary>
        /// <remarks>
        /// Assembly-qualified names are used because <c>SchemaSetupStep</c> resolves through
        /// <c>assemblyHandler.GetType(name)</c> → <c>Type.GetType(name)</c> → a scan of
        /// <c>ExtraAssemblies</c>; a bare FullName only resolves via the last of those. The declaring
        /// assemblies are unioned into <c>ExtraAssemblies</c> so the scan can still find them by name
        /// if the qualified lookup misses. An unresolvable name fails the step loudly — it is never
        /// silently dropped — so it is worth being generous here.
        /// </remarks>
        internal static void ApplyEntityTypes(
            SchemaSetupStepOptions opts,
            IReadOnlyList<Type>? types,
            IReadOnlyList<Assembly>? extraAssemblies)
        {
            if (opts == null) return;

            var names = types?
                .Where(t => t != null)
                .Select(t => t.AssemblyQualifiedName ?? t.FullName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();

            opts.EntityTypeNames = names is { Count: > 0 } ? names! : null;

            var assemblies = new List<Assembly>();
            if (extraAssemblies != null) assemblies.AddRange(extraAssemblies.Where(a => a != null));
            if (types != null)
                foreach (var asm in types.Where(t => t != null).Select(t => t.Assembly).Distinct())
                    if (!assemblies.Contains(asm)) assemblies.Add(asm);

            opts.ExtraAssemblies = assemblies.Count > 0 ? assemblies : null;
        }

        public uc_SetupWizard()
        {
            InitializeComponent();
            BindDefaults();
            Details.AddinName = "Setup Wizard";
        }

        public uc_SetupWizard(IServiceProvider services) : base(services)
        {
            _services = services;
            InitializeComponent();
            BindDefaults();
            Details.AddinName = "Setup Wizard";
        }

        private void BindDefaults()
        {
            _viewModel.Style = WizardStyle.HorizontalStepper;
            _viewModel.AllowCancel = true;
            _viewModel.AllowSkip = false;

            // DesktopSetupWizardAdapter invokes its callbacks on the thread-pool thread the run
            // happens on. Marshal once here so every AdapterProgress subscriber is on the UI thread
            // and none of them has to think about it.
            _adapter.OnProgress += RaiseProgressOnUiThread;
        }

        private void RaiseProgressOnUiThread(PassedArgs args)
        {
            if (args == null || IsDisposed || !IsHandleCreated) return;
            try
            {
                if (InvokeRequired) BeginInvoke(() => AdapterProgress?.Invoke(args));
                else AdapterProgress?.Invoke(args);
            }
            catch (ObjectDisposedException)
            {
                // The control went away mid-run; dropping a progress tick is the correct outcome.
            }
            catch (InvalidOperationException)
            {
                // Handle destroyed between the check and the BeginInvoke — same reasoning.
            }
        }

        private void BtnReset_Click(object? sender, EventArgs e)
        {
            _viewModel.Style = WizardStyle.HorizontalStepper;
            _viewModel.AllowCancel = true;
            _viewModel.AllowSkip = false;

            BindDefaults();
            UpdateStatus("Defaults restored. Wizard style: HorizontalStepper.");
        }

        // ── Launch: build the canonical wizard and wire UI shells to its typed options ──

        private void BtnLaunch_Click(object? sender, EventArgs e)
        {
            SyncModelFromUi();

            try
            {
                var pieces = BuildSetupPieces();
                _pieces = pieces;
                PopulateUi(pieces);

                if (_reviewStepControl != null)
                {
                    // Bind the whole wizard: the review step summarises and runs the full pipeline,
                    // not just its last step.
                    _reviewStepControl.BindToWizard(pieces.Wizard, pieces.Context);
                    _reviewStepControl.RunSetupRequested -= OnReviewRunSetupRequested;
                    _reviewStepControl.RunSetupRequested += OnReviewRunSetupRequested;
                    UpdateReviewVersionInfo(pieces.Context);
                }

                UpdateStatus("Setup wizard initialized. Fill each step's UI, then click Run in the review step.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Setup failed to start: {ex.Message}");
            }
        }

        private void SyncModelFromUi()
        {
            // _viewModel.Style / AllowCancel / AllowSkip are read from the option panel
            // before BtnLaunch_Click; the actual Settings propagation happens inside
            // BuildSetupPieces (see setup options assembly).
        }

        // ── Canonical framework integration ──────────────────────────────────

        /// <summary>
        /// Bundles the canonical wizard with the typed options it was built from. The options
        /// are the single source of truth — UI shells mutate them in place, and the canonical
        /// Steps read from them at Execute.
        /// </summary>
        /// <summary>
        /// Phase 9 (A2): populates the review step's version-info line — the migrate-on-startup posture,
        /// the declared version, and the version currently recorded in the target database (best-effort;
        /// null/unversioned on a first run where the DB doesn't exist yet).
        /// </summary>
        private void UpdateReviewVersionInfo(SetupContext? ctx)
        {
            if (_reviewStepControl == null) return;

            string? currentDbVersion = null;
            try
            {
                var connName = ctx?.ConnectionProperties?.ConnectionName;
                var editor = ctx?.Editor;
                if (editor != null && !string.IsNullOrWhiteSpace(connName))
                    currentDbVersion = new global::TheTechIdea.Beep.Editor.Migration.MigrationTrackingService(editor)
                        .GetCurrentDatabaseVersion(connName!)?.VersionString;
            }
            catch
            {
                // Display-only: never let a version probe break wizard setup.
            }

            _reviewStepControl.SetVersionInfo(MigrateOnStartup, DeclaredSchemaVersion, currentDbVersion);
        }

        private sealed record SetupPieces(
            ISetupWizard Wizard,
            SetupContext Context,
            DriverProvisionStepOptions DriverOpts,
            ConnectionConfigStepOptions ConnectionOpts,
            SchemaSetupStepOptions SchemaOpts,
            DefaultsSetupStepOptions DefaultsOpts,
            SeedingStepOptions SeedingOpts);

        private SetupPieces BuildSetupPieces()
        {
            if (beepService?.DMEEditor == null)
                throw new InvalidOperationException(
                    "IDMEEditor is not available. Cannot build a setup wizard without a configuration store.");

            var editor = beepService.DMEEditor;
            var configEditor = editor.ConfigEditor;

            // 1. Resolve defaults from the editor — same lookup pattern as the Blazor
            //    Beep.Razor.Components BeepSetupWizardRunner. Read driver configs and
            //    the existing first connection straight from ConfigEditor (no ad-hoc
            //    in-memory caches). The Blazor runner also stages a missing driver
            //    for "use existing connection" — we mirror that here.
            var conn = configEditor?.DataConnections?.FirstOrDefault();
            if (conn == null)
            {
                conn = new ConnectionProperties
                {
                    ConnectionName = "MainDB",
                    DatabaseType = DataSourceType.SqlLite,
                    ConnectionString = $"Data Source={Path.Combine(AppContext.BaseDirectory, "beep-bootstrap.db")}",
                    Category = DatasourceCategory.RDBMS
                };
            }
            var driverPackageNames = ResolveDriverPackageNames(editor, conn);

            // 2. Build typed options (single source of truth).
            //    DriverProvisionStepOptions.PackageName holds ONE package per step; we
            //    create one step per package so each can CanSkip independently.
            //    Build the per-package options ONCE and hand these same instances to the steps
            //    below. Constructing separate options here and again in the builder would leave
            //    the UI bound to an object no step reads — the user's driver edits would be
            //    silently discarded at Execute.
            var driverOptsList = driverPackageNames
                .Select(pkg => new DriverProvisionStepOptions
                {
                    PackageName = pkg,
                    NuGetSources = new List<string>()
                })
                .ToList();

            //    The driver shell edits one options instance, so bind it to the first step's.
            //    With no packages there are no driver steps at all, so the fallback is inert.
            var driverOpts = driverOptsList.FirstOrDefault()
                ?? new DriverProvisionStepOptions { PackageName = string.Empty, NuGetSources = new List<string>() };
            var connectionOpts = new ConnectionConfigStepOptions
            {
                ConnectionProperties = conn,
                OpenConnection = true
            };
            var schemaOpts = new SchemaSetupStepOptions
            {
                StrictPolicyMode = StrictPolicyMode
            };
            // Portable EntityTypeNames rather than the [Obsolete] EntityTypes, which cannot be
            // serialised into a SetupDefinition.
            ApplyEntityTypes(schemaOpts, _entityTypes, _extraAssemblies);

            var defaultsOpts = new DefaultsSetupStepOptions();
            var seedingOpts = new SeedingStepOptions
            {
                Registry = _seederRegistry
            };

            // 3. Build the canonical wizard with those options. The Steps take the Options
            //    in their ctor and read from them at Execute.
            //
            //    ReportOutputPath is what turns on the framework's default append-only JSONL audit
            //    sink and makes SetupReport.DryRunReportJson / RollbackReportJson get written;
            //    unset, auditing is a silent no-op.
            var reportPath = ResolveSetupPath(configEditor, "reports");

            var options = new SetupOptions
            {
                Environment = Environment,
                DryRun = DryRun,
                StrictPolicyMode = StrictPolicyMode,
                AutoRollbackOnFailure = AutoRollbackOnFailure,
                ReportOutputPath = reportPath,
                // Phase 9: version-gate on later launches, and the model scope it resolves from.
                MigrateOnStartup = MigrateOnStartup,
                DeclaredSchemaVersion = string.IsNullOrWhiteSpace(DeclaredSchemaVersion) ? null : DeclaredSchemaVersion,
                EntityTypeNames = schemaOpts.EntityTypeNames,
                EntityAssemblies = schemaOpts.ExtraAssemblies?
                    .Select(a => a.GetName().Name)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToList()!
            };

            // The concrete SetupWizardBuilder, not DefaultSetupWizardFactory.Create: the factory
            // hands its callback an ISetupWizardBuilder, which is a deliberately minimal marker
            // (WithId/WithEnvironment/AddStep/Build) so the Models project needn't reference
            // engine-only types. Adapter, state store and security live on the concrete builder.
            // Casting inside the callback would compile but silently skip all three if it ever
            // failed, so bind to the real type instead. The factory does nothing else we need —
            // every step here is supplied explicitly.
            var builder = new SetupWizardBuilder()
                .WithOptions(options)
                .WithId("platform-setup-winform")
                .WithEnvironment(Environment)
                // The framework's desktop bridge: gives the wizard a display surface
                // (ShowStep per step) and owns the run loop used by RunPipelineAsync.
                .WithAdapter(_adapter)
                // Without a store the framework disables checkpointing and warns that the run
                // cannot be resumed. Key it under the Beep config folder so a crashed first-run
                // can pick up where it left off.
                .WithStateStore(BuildStateStore(configEditor))
                // Records who ran setup. The solo default is anonymous (IsAuthenticated=false),
                // which is honest for a desktop run — but pass it explicitly so the value
                // reaches SchemaSetupStep's approval provider instead of arriving null.
                .WithSecurity(new AnonymousSetupPrincipal(), null);

            // One DriverProvisionStep per distinct package — mirrors Blazor
            // BeepSetupWizardRunner's bridge.DriverPackageNames loop. These are the same
            // options instances the UI shell binds to, so edits reach Execute.
            foreach (var opts in driverOptsList)
                builder.AddStep(new DriverProvisionStep(opts));

            // Approval and backup confirmation are the only points where SchemaSetupStep pauses
            // for a human. Without providers it self-approves and reports "no backup" — so wire
            // the WinForms prompts in, otherwise the safety gates are invisible to the operator.
            // DefaultsSetupStep matches the framework's canonical order
            // (schema → defaults → seeding); omitting it meant the UTCNOW audit-column defaults
            // it writes were never configured.
            builder
                .AddStep(new ConnectionConfigStep(connectionOpts))
                .AddStep(new SchemaSetupStep(
                    schemaOpts,
                    backupConfirmation: new WinFormsBackupConfirmationProvider(this),
                    approvalProvider: new WinFormsSetupApprovalProvider(this),
                    principal: new AnonymousSetupPrincipal()))
                .AddStep(new DefaultsSetupStep(defaultsOpts));

            // SeedingStep.Validate hard-fails without a registry, so adding it unconditionally
            // would ship a wizard that can never run — same rule DefaultSetupWizardFactory uses.
            if (_seederRegistry != null)
                builder.AddStep(new SeedingStep(seedingOpts));

            var wizard = builder.Build();

            // SetupWizard.Run does `context.Options ?? Options` — the context wins outright. Sharing
            // the one instance is what keeps DryRun honest; a context with its own default
            // SetupOptions would silently turn a dry run into a live one.
            var ctx = new SetupContext { Editor = editor, Options = options };

            // The context the steps actually see is the one the factory returns. An earlier version
            // built a second SetupContext here, set beepService/ConnectionProperties on it, and threw
            // it away — so neither value ever reached the run.
            if (beepService != null)
                ctx.Properties["beepService"] = beepService;
            ctx.ConnectionProperties = conn;

            return new SetupPieces(wizard, ctx, driverOpts, connectionOpts, schemaOpts, defaultsOpts, seedingOpts);
        }

        /// <summary>
        /// Builds the local checkpoint store, rooted in the Beep config folder.
        /// </summary>
        /// <remarks>
        /// Returns null when no config path is known, which leaves the framework in its documented
        /// no-store mode: checkpointing disabled, run not resumable, and it warns. Better to inherit
        /// that behaviour than to invent a path somewhere arbitrary on the user's disk.
        /// </remarks>
        private static LocalJsonSetupStateStore? BuildStateStore(IConfigEditor? configEditor)
        {
            var root = configEditor?.ConfigPath;
            return string.IsNullOrWhiteSpace(root) ? null : new LocalJsonSetupStateStore(root);
        }

        private static string? ResolveSetupPath(IConfigEditor? configEditor, string leaf)
        {
            var root = configEditor?.ConfigPath;
            return string.IsNullOrWhiteSpace(root) ? null : Path.Combine(root, "setup", leaf);
        }

        /// <summary>
        /// Resolve driver packages from <c>ConfigEditor.DataDriversClasses</c> using
        /// the same lookup strategy as
        /// <c>Beep.Razor.Components.BeepSetupWizardRunner.StageExistingConnectionDriver</c>:
        /// prefer AutoLoad drivers; if none, fall back to every distinct package.
        /// Always includes the driver matching the connection's <c>DatabaseType</c>
        /// (the "use existing" path).
        /// </summary>
        private static List<string> ResolveDriverPackageNames(IDMEEditor editor, ConnectionProperties connection)
        {
            var drivers = editor?.ConfigEditor?.DataDriversClasses?
                .Where(d => d != null && !string.IsNullOrWhiteSpace(d.PackageName))
                .ToList();

            if (drivers == null || drivers.Count == 0)
                return new List<string>();

            var selected = drivers.Where(d => d.AutoLoad).ToList();
            if (selected.Count == 0)
                selected = drivers;

            // Mirror Blazor "use existing connection" staging: if the selected
            // set doesn't include the driver for connection.DatabaseType, add it.
            if (connection != null)
            {
                var matching = drivers.FirstOrDefault(d =>
                    d.DatasourceType == connection.DatabaseType &&
                    !string.IsNullOrWhiteSpace(d.PackageName));
                if (matching != null && !selected.Any(d =>
                        string.Equals(d.PackageName, matching.PackageName,
                            StringComparison.OrdinalIgnoreCase)))
                {
                    selected.Add(matching);
                }
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new List<string>();
            foreach (var d in selected)
            {
                if (seen.Add(d.PackageName))
                    result.Add(d.PackageName);
            }
            return result;
        }

        // Bind the UI shells to the typed options (the source of truth). The shells mutate
        // the options in-place as the user edits; the canonical Steps already hold references
        // to these same options, so Execute picks up the latest values with no copying.
        private void PopulateUi(SetupPieces pieces)
        {
            _driverStepControl = new uc_SetupDriverStep { Dock = DockStyle.Fill };
            _driverStepControl.InitializeStep(pieces.DriverOpts, pieces.Context, beepService?.DMEEditor);
            // Mirror Blazor BeepSetupWizardRunner — wizard builds one DriverProvisionStep per
            // staged package; surface that list on the UI driver step so the user can see
            // which packages will be (re)loaded and skip the interactive nuggets manager
            // when there's nothing to install interactively.
            _driverStepControl.SetStagedPackages(ResolveStagedPackagesForUi());

            _connectionStepControl = new uc_SetupConnectionStep { Dock = DockStyle.Fill };
            _connectionStepControl.InitializeStep(pieces.ConnectionOpts, pieces.Context);
            _connectionStepControl.ConnectionSaved += (s, e) =>
                UpdateStatus($"Connection saved: {e.ConnectionProperties.ConnectionName}");

            _schemaStepControl = new uc_SetupSchemaStep { Dock = DockStyle.Fill };
            _schemaStepControl.InitializeStep(pieces.SchemaOpts, pieces.Context, beepService?.DMEEditor);
            // The options carry portable EntityTypeNames; the shell's survey is type-based, so hand
            // it the Types the host already holds rather than making it re-resolve the names.
            _schemaStepControl.EntityTypes = _entityTypes;

            _seedingStepControl = new uc_SetupSeedingStep { Dock = DockStyle.Fill };
            _seedingStepControl.InitializeStep(pieces.SeedingOpts, pieces.Context);
        }

        /// <summary>
        /// Returns the list of staged driver package names for the UI to display.
        /// Pulled directly from <c>ConfigEditor.DataDriversClasses</c> using the
        /// same AutoLoad-first lookup the factory uses.
        /// </summary>
        private List<string> ResolveStagedPackagesForUi()
        {
            var editor = beepService?.DMEEditor;
            if (editor?.ConfigEditor?.DataDriversClasses == null)
                return new List<string>();

            var drivers = editor.ConfigEditor.DataDriversClasses
                .Where(d => d != null && !string.IsNullOrWhiteSpace(d.PackageName))
                .ToList();

            var selected = drivers.Where(d => d.AutoLoad).ToList();
            if (selected.Count == 0)
                selected = drivers;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new List<string>();
            foreach (var d in selected)
            {
                if (seen.Add(d.PackageName))
                    result.Add(d.PackageName);
            }
            return result;
        }

        /// <summary>
        /// The one place the canonical pipeline is run. Both entry points — the review step's Run
        /// button and the wizard's Finish (OnComplete) — go through here, so they can't drift apart
        /// in what they execute or what they report.
        /// </summary>
        /// <remarks>
        /// Delegates to the framework's <see cref="DesktopSetupWizardAdapter"/> rather than calling
        /// <c>Wizard.Run</c> directly. <c>SetupWizardAdapterBase.RunAsync</c> already owns the
        /// worker-thread hop, the <c>IProgress&lt;PassedArgs&gt;</c> plumbing, uniform
        /// cancellation/exception handling (an unexpected throw yields the partial report rather than
        /// escaping), and <c>GetReport()</c>. Re-implementing that here meant a second run mechanism
        /// that could drift from the framework's.
        ///
        /// Cancellation is still only observed *between* steps — <c>ISetupStep.Execute</c> takes no
        /// CancellationToken — so <paramref name="token"/> cannot interrupt a step already underway.
        /// </remarks>
        private Task<SetupReport?> RunPipelineAsync(CancellationToken token)
        {
            var pieces = _pieces ?? throw new InvalidOperationException(
                "Setup wizard not initialized; call BuildWizardConfig/Launch first.");

            return RunViaAdapterAsync(pieces, token);
        }

        private async Task<SetupReport?> RunViaAdapterAsync(SetupPieces pieces, CancellationToken token)
            => await _adapter.RunAsync(pieces.Wizard, pieces.Context, token).ConfigureAwait(true);

        /// <summary>
        /// Renders a finished run: review-step summary, status line, and the SetupCompleted event.
        /// </summary>
        private void ApplyRunReport(SetupReport? report)
        {
            var results = report?.StepResults ?? (IReadOnlyList<SetupStepResult>)Array.Empty<SetupStepResult>();
            var summary = results.Count > 0
                ? string.Join(", ", results.Select(r => $"{r.StepName}: {(r.Succeeded ? "OK" : "FAIL")}"))
                : (report?.Succeeded == true ? "All steps passed" : "No steps executed");
            var path = string.Join(" → ", results.Where(s => s.Succeeded).Select(s => s.StepName));
            var firstError = results.FirstOrDefault(r => !r.Succeeded);

            // Surface the staged drivers from the UI control so the run reflects the same packages
            // the wizard read from ConfigEditor at Launch time. Same data path the Blazor runner uses.
            var staged = _driverStepControl?.StagedPackages ?? Array.Empty<string>();
            if (staged.Count > 0)
                summary += $" | drivers staged: {string.Join(", ", staged)}";

            if (_reviewStepControl != null)
            {
                _reviewStepControl.SetLastRunSummary(summary);
                _reviewStepControl.SetExecutionPath(path);
                _reviewStepControl.SetProgress(report?.Succeeded == true ? 100 : 0, summary);
                _reviewStepControl.SetRunningState(false);
            }

            UpdateStatus(report?.Succeeded == true
                ? $"Setup completed in {report!.TotalElapsed.TotalSeconds:0.0}s."
                : $"Setup failed: {firstError?.Message ?? "Unknown error"}");

            SetupCompleted?.Invoke(this, new SetupCompletedEventArgs
            {
                Succeeded = report?.Succeeded == true,
                Summary = summary,
                ExecutionPath = path,
                Report = report,
                StagedDrivers = staged.ToList()
            });
        }

        // The review step's Run button → the shared canonical pipeline.
        private async void OnReviewRunSetupRequested(object? sender, EventArgs e)
        {
            if (_pieces == null)
            {
                UpdateStatus("Setup wizard not initialized. Click Launch first.");
                return;
            }

            _runCts?.Cancel();
            _runCts?.Dispose();
            _runCts = new CancellationTokenSource();
            var token = _runCts.Token;

            // Already marshalled to the UI thread by RaiseProgressOnUiThread.
            void OnProgress(PassedArgs args)
            {
                if (_reviewStepControl != null && args.ParameterInt1 > 0)
                    _reviewStepControl.SetProgress(args.ParameterInt1, args.Messege ?? string.Empty);
                UpdateStatus(args.Messege ?? string.Empty);
            }

            AdapterProgress += OnProgress;
            try
            {
                UpdateStatus("Running canonical setup pipeline…");
                ApplyRunReport(await RunPipelineAsync(token).ConfigureAwait(true));
            }
            catch (OperationCanceledException)
            {
                UpdateStatus("Setup cancelled.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Setup crashed: {ex.Message}");
            }
            finally
            {
                AdapterProgress -= OnProgress;
                // The review step disables its Run button and waits for the host to re-enable it.
                // ApplyRunReport does that on the success path; without this the button would stay
                // dead after a cancel or a crash.
                _reviewStepControl?.SetRunningState(false);
            }
        }

        private void UpdateStatus(string message)
        {
            if (_lblStatus != null)
                _lblStatus.Text = message;
        }

        /// <summary>
        /// Builds a WizardConfig suitable for WizardManager.ShowWizard().
        /// Maps the Beep.SetUp framework wizard steps to WinForms UserControl content.
        /// </summary>
        public WizardConfig BuildWizardConfig()
        {
            if (beepService?.DMEEditor == null)
                return new WizardConfig { Title = "Setup Wizard" };

            var pieces = BuildSetupPieces();
            _pieces = pieces;
            PopulateUi(pieces);

            var config = new WizardConfig
            {
                Key = "platform-setup-winform",
                Title = "Platform Setup",
                Style = WizardStyle.HorizontalStepper,
                TransitionType = TransitionType.Fade,
                TransitionDurationMs = 200,
                ShowProgressBar = true,
                AllowCancel = true,
                ShowStepList = true,
                NextButtonText = "Next",
                BackButtonText = "Back",
                FinishButtonText = "Run Setup",
            };

            // Map framework steps to WizardSteps with WinForms controls as content
            foreach (var step in pieces.Wizard.Steps)
            {
                Control? content = step switch
                {
                    DriverProvisionStep => _driverStepControl,
                    ConnectionConfigStep => _connectionStepControl,
                    SchemaSetupStep => _schemaStepControl,
                    SeedingStep => _seedingStepControl,
                    _ => null
                };

                config.Steps.Add(new WizardStep
                {
                    Key = step.GetType().Name,
                    Title = step.GetType().Name.Replace("Step", ""),
                    Description = "",
                    Content = content
                });
            }

            // Add review step as the final step
            if (_reviewStepControl != null)
            {
                _reviewStepControl.BindToWizard(pieces.Wizard, pieces.Context);
                _reviewStepControl.RunSetupRequested -= OnReviewRunSetupRequested;
                _reviewStepControl.RunSetupRequested += OnReviewRunSetupRequested;
                UpdateReviewVersionInfo(pieces.Context);

                config.Steps.Add(new WizardStep
                {
                    Key = "ReviewAndRun",
                    Title = "Review & Run",
                    Description = "Review your configuration and run the setup",
                    Content = _reviewStepControl
                });
            }

            // Deliberately no OnComplete run hook.
            //
            // This WizardConfig is the *configuration* surface — it collects driver, connection,
            // schema and seeding settings into the typed options. Executing the pipeline is
            // BeepBootstrapper's job: it runs through ISetupWizardAdapter.RunAsync and marks
            // first-run complete only on success. Running here as well would execute setup twice,
            // and doing it inside OnComplete (an Action invoked just before the wizard closes)
            // is what forced the whole run onto the UI thread in the first place.
            return config;
        }

        private sealed class SetupWizardViewModel
        {
            public WizardStyle Style { get; set; } = WizardStyle.HorizontalStepper;
            public bool AllowCancel { get; set; } = true;
            public bool AllowSkip { get; set; } = false;
        }

        public sealed class SetupCompletedEventArgs : EventArgs
        {
            public bool Succeeded { get; init; }
            public string Summary { get; init; } = string.Empty;
            public string ExecutionPath { get; init; } = string.Empty;
            public SetupReport? Report { get; init; }
            public IReadOnlyList<string> StagedDrivers { get; init; } = Array.Empty<string>();
        }
    }
}
