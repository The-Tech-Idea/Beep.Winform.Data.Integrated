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
using TheTechIdea.Beep.SetUp.Seeding;
using TheTechIdea.Beep.SetUp.Steps;
using TheTechIdea.Beep.Vis.Modules;
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
    public partial class uc_SetupWizard : TemplateUserControl, IAddinVisSchema
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

        // The canonical wizard the host built on Launch. Held in memory so the review step's
        // Run button can call pieces.Wizard.RunAsync(pieces.Context, progress, token).
        private SetupPieces? _pieces;
        private CancellationTokenSource? _runCts;

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

        public IReadOnlyList<Type>? EntityTypes
        {
            get => _entityTypes;
            set
            {
                _entityTypes = value;
                if (_pieces != null) _pieces.SchemaOpts.EntityTypes = value;
            }
        }

        public IReadOnlyList<Assembly>? ExtraAssemblies
        {
            get => _extraAssemblies;
            set
            {
                _extraAssemblies = value;
                if (_pieces != null) _pieces.SchemaOpts.ExtraAssemblies = value;
            }
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

                var lastCanonical = pieces.Wizard.Steps.LastOrDefault();
                if (lastCanonical != null && _reviewStepControl != null)
                {
                    _reviewStepControl.BindToCanonicalStep(lastCanonical, pieces.Context);
                    _reviewStepControl.RunSetupRequested -= OnReviewRunSetupRequested;
                    _reviewStepControl.RunSetupRequested += OnReviewRunSetupRequested;
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
        private sealed record SetupPieces(
            ISetupWizard Wizard,
            SetupContext Context,
            DriverProvisionStepOptions DriverOpts,
            ConnectionConfigStepOptions ConnectionOpts,
            SchemaSetupStepOptions SchemaOpts,
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
            var driverOpts = driverPackageNames.Count == 1
                ? new DriverProvisionStepOptions { PackageName = driverPackageNames[0] }
                : new DriverProvisionStepOptions { PackageName = driverPackageNames.FirstOrDefault() ?? string.Empty };
            var connectionOpts = new ConnectionConfigStepOptions
            {
                ConnectionProperties = conn,
                OpenConnection = true
            };
            var schemaOpts = new SchemaSetupStepOptions
            {
                EntityTypes = _entityTypes,
                ExtraAssemblies = _extraAssemblies
            };
            var seedingOpts = new SeedingStepOptions
            {
                Registry = _seederRegistry
            };

            // 3. Build the canonical wizard with those options. The Steps take the Options
            //    in their ctor and read from them at Execute.
            var context = new SetupContext
            {
                Editor = editor
            };
            if (beepService != null)
                context.Properties["beepService"] = beepService;
            context.ConnectionProperties = conn;

            var factory = new DefaultSetupWizardFactory();
            var (wizard, ctx) = factory.Create(editor, new SetupOptions
            {
                Environment = "Production"
            }, builder =>
            {
                builder
                    .WithId("platform-setup-winform")
                    .WithEnvironment("Production");

                // One DriverProvisionStep per distinct package — mirrors Blazor
                // BeepSetupWizardRunner's bridge.DriverPackageNames loop.
                foreach (var pkg in driverPackageNames)
                {
                    builder.AddStep(new DriverProvisionStep(new DriverProvisionStepOptions
                    {
                        PackageName = pkg,
                        NuGetSources = new List<string>()
                    }));
                }

                builder
                    .AddStep(new ConnectionConfigStep(connectionOpts))
                    .AddStep(new SchemaSetupStep(schemaOpts))
                    .AddStep(new SeedingStep(seedingOpts));
            });

            return new SetupPieces(wizard, ctx, driverOpts, connectionOpts, schemaOpts, seedingOpts);
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

        // The review step's Run button → canonical Wizard.RunAsync (the framework's pipeline).
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

            try
            {
                UpdateStatus("Running canonical setup pipeline…");

                var progress = new Progress<PassedArgs>(args =>
                {
                    if (_reviewStepControl != null && args.ParameterInt1 > 0)
                        _reviewStepControl.SetProgress(args.ParameterInt1, args.Messege ?? string.Empty);
                    UpdateStatus(args.Messege ?? string.Empty);
                });

                var result = await Task.Run(() => _pieces.Wizard.Run(_pieces.Context, progress), token);

                var report = _pieces.Wizard.GetReport();
                var results = report?.StepResults ?? (IReadOnlyList<SetupStepResult>)Array.Empty<SetupStepResult>();
                var summary = results.Count > 0
                    ? string.Join(", ", results.Select(r => $"{r.StepName}: {(r.Succeeded ? "OK" : "FAIL")}"))
                    : (report?.Succeeded == true ? "All steps passed" : "No steps executed");
                var path = string.Join(" → ", results.Where(s => s.Succeeded).Select(s => s.StepName));
                var firstError = results.FirstOrDefault(r => !r.Succeeded);

                // Surface the staged drivers from the UI control so the review run
                // reflects the same packages the wizard read from ConfigEditor at
                // Launch time. Same data path the Blazor runner uses.
                var staged = _driverStepControl?.StagedPackages ?? Array.Empty<string>();
                if (staged.Count > 0)
                {
                    summary += $" | drivers staged: {string.Join(", ", staged)}";
                }

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
            catch (OperationCanceledException)
            {
                UpdateStatus("Setup cancelled.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Setup crashed: {ex.Message}");
            }
        }

        private void UpdateStatus(string message)
        {
            if (_lblStatus != null)
                _lblStatus.Text = message;
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
