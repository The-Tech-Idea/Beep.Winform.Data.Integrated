using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Icons;
using TheTechIdea.Beep.SetUp;
using TheTechIdea.Beep.SetUp.Seeding;
using TheTechIdea.Beep.SetUp.Steps;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Models;
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
    public partial class uc_SetupWizard : TemplateUserControl, IAddinVisSchema
    {
        private readonly SetupWizardViewModel _viewModel = new SetupWizardViewModel();
        private readonly IServiceProvider? _services;
        private uc_SetupDriverStep? _driverStepControl;
        private uc_SetupConnectionStep? _connectionStepControl;
        private uc_SetupSchemaStep? _schemaStepControl;
        private uc_SetupSeedingStep? _seedingStepControl;
        private uc_SetupReviewRunStep? _reviewStepControl;
        private bool _isRunInProgress;
        private string _lastExecutionPath = "Not run yet";
        private string _lastRunSummary = "Not executed yet";
        private string _lastDriverProvisionSummary = "No driver package operations recorded.";

        private ISeederRegistry? _seederRegistry;
        private IReadOnlyList<Type>? _entityTypes;
        private IReadOnlyList<Assembly>? _extraAssemblies;

        public Func<SetupExecutionRequest, IProgress<(int Progress, string Message)>?, Task<SetupExecutionResult>>? SetupExecutor { get; set; }
        public event EventHandler<SetupCompletedEventArgs>? SetupCompleted;

        private CancellationTokenSource? _runCts;

        #region IAddinVisSchema
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
        #endregion

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

        public ISeederRegistry? SeederRegistry
        {
            get => _seederRegistry;
            set
            {
                _seederRegistry = value;
                _seedingStepControl?.InitializeStep(_seederRegistry, _extraAssemblies);
                RefreshReviewStep();
            }
        }

        public IReadOnlyList<Type>? EntityTypes
        {
            get => _entityTypes;
            set
            {
                _entityTypes = value;
                if (_schemaStepControl != null)
                    _schemaStepControl.EntityTypes = _entityTypes;
                RefreshReviewStep();
            }
        }

        public IReadOnlyList<Assembly>? ExtraAssemblies
        {
            get => _extraAssemblies;
            set
            {
                _extraAssemblies = value;
                if (_schemaStepControl != null)
                    _schemaStepControl.ExtraAssemblies = _extraAssemblies;
                if (_seedingStepControl != null)
                    _seedingStepControl.ExtraAssemblies = _extraAssemblies;
                RefreshReviewStep();
            }
        }

        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);
            ApplyTheme();
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            base.OnNavigatedTo(parameters);
            UpdateStatus("Ready. Choose options and launch setup wizard.");
        }

        public override void ApplyTheme()
        {
            base.ApplyTheme();

            ApplyThemeToBeep(_rootPanel);
            ApplyThemeToBeep(_headerPanel);
            ApplyThemeToBeep(_optionsPanel);
            ApplyThemeToBeep(_actionsPanel);
            ApplyThemeToBeep(_statusPanel);
            ApplyThemeToBeep(_lblStatus);
            ApplyThemeToBeep(_cmbWizardStyle);
            ApplyThemeToBeep(_chkAllowSkip);
            ApplyThemeToBeep(_chkAllowCancel);
            ApplyThemeToBeep(_btnLaunch);
            ApplyThemeToBeep(_btnReset);

            _driverStepControl?.ApplyTheme(Theme);
            _connectionStepControl?.ApplyTheme(Theme);
            _schemaStepControl?.ApplyTheme(Theme);
            _seedingStepControl?.ApplyTheme(Theme);
            _reviewStepControl?.ApplyTheme(Theme);
        }

        private void BindDefaults()
        {
            if (_cmbWizardStyle != null)
            {
                _cmbWizardStyle.Items.Clear();
                foreach (var name in Enum.GetNames(typeof(WizardStyle)))
                    _cmbWizardStyle.Items.Add(new SimpleItem { Text = name, Name = name });

                var styleText = _viewModel.Style.ToString();
                _cmbWizardStyle.SelectedItem = _cmbWizardStyle.Items
                    .OfType<SimpleItem>()
                    .FirstOrDefault(i => i.Text == styleText);
            }

            if (_chkAllowCancel != null)
                _chkAllowCancel.Checked = _viewModel.AllowCancel;

            if (_chkAllowSkip != null)
                _chkAllowSkip.Checked = _viewModel.AllowSkip;
        }

        private void BtnReset_Click(object? sender, EventArgs e)
        {
            _viewModel.Style = WizardStyle.HorizontalStepper;
            _viewModel.AllowCancel = true;
            _viewModel.AllowSkip = false;

            BindDefaults();
            UpdateStatus("Defaults restored. Wizard style: HorizontalStepper.");
        }

        private void BtnLaunch_Click(object? sender, EventArgs e)
        {
            SyncModelFromUi();

            var config = CreateWizardConfig();
            var owner = FindForm();
            var dialogResult = owner != null
                ? WizardManager.ShowWizard(config, owner)
                : WizardManager.ShowWizard(config);

            if (dialogResult == DialogResult.OK)
            {
                UpdateStatus("Setup wizard completed successfully.");
                return;
            }

            if (dialogResult == DialogResult.Cancel)
            {
                UpdateStatus("Setup wizard cancelled.");
                return;
            }

            UpdateStatus($"Setup wizard ended with result: {dialogResult}.");
        }

        private void SyncModelFromUi()
        {
            if (_cmbWizardStyle?.SelectedItem is SimpleItem selectedStyle &&
                Enum.TryParse(selectedStyle.Text, out WizardStyle parsedStyle))
            {
                _viewModel.Style = parsedStyle;
            }

            _viewModel.AllowCancel = _chkAllowCancel?.Checked ?? true;
            _viewModel.AllowSkip = _chkAllowSkip?.Checked ?? false;
        }

        private WizardConfig CreateWizardConfig()
        {
            var config = new WizardConfig
            {
                Key = "platform-setup-winform",
                Title = "Platform Setup - WinForms",
                Description = "Guided setup for driver, connection, schema, and seed data.",
                Style = _viewModel.Style,
                AllowCancel = _viewModel.AllowCancel,
                AllowSkip = _viewModel.AllowSkip,
                Theme = _currentTheme,
                ShowProgressBar = true,
                ShowStepList = true,
                Steps = BuildSteps()
            };

            config.OnComplete = _ => UpdateStatus("Setup completed. Review connection and schema status.");
            config.OnCancel = _ => UpdateStatus("Setup cancelled by user.");
            config.OnStepChanged = (stepIndex, context) =>
            {
                if (_entityTypes != null)
                    context.SetValue("entityTypes", _entityTypes);
                if (_extraAssemblies != null)
                    context.SetValue("extraAssemblies", _extraAssemblies);
                if (_seederRegistry != null)
                    context.SetValue("seederRegistry", _seederRegistry);
            };

            return config;
        }

        private List<WizardStep> BuildSteps()
        {
            return new List<WizardStep>
            {
                CreateDriverProvisionStep(),
                CreateConnectionStep(),
                CreateSchemaStep(),
                CreateSeedingStep(),
                CreateReviewRunStep()
            };
        }

        private WizardStep CreateDriverProvisionStep()
        {
            _driverStepControl = new uc_SetupDriverStep { Dock = DockStyle.Fill };
            _driverStepControl.InitializeStep(_services, Theme);
            _driverStepControl.DriverPackageInstalled += DriverStepControl_DriverPackageInstalled;
            _driverStepControl.DatasourceSetupCompleted += DriverStepControl_DatasourceSetupCompleted;

            return new WizardStep
            {
                Key = "driver-provision",
                Title = "Driver Provision",
                Description = "Discover or install required drivers.",
                Icon = SvgsUIcons.Logistics.Package,
                IsOptional = false,
                Content = _driverStepControl,
                OnEnter = _ => SyncDriverStepState()
            };
        }

        private WizardStep CreateConnectionStep()
        {
            _connectionStepControl = new uc_SetupConnectionStep { Dock = DockStyle.Fill };
            _connectionStepControl.InitializeStep(beepService, Theme);
            _connectionStepControl.ConnectionSaved += ConnectionStepControl_ConnectionSaved;
            _connectionStepControl.ConnectionTestCompleted += ConnectionStepControl_ConnectionTestCompleted;

            return new WizardStep
            {
                Key = "connection-config",
                Title = "Connection Configuration",
                Description = "Create and verify the target datasource connection.",
                Icon = SvgsUIcons.Storage.Database,
                IsOptional = false,
                Content = _connectionStepControl
            };
        }

        private WizardStep CreateSchemaStep()
        {
            _schemaStepControl = new uc_SetupSchemaStep { Dock = DockStyle.Fill };
            _schemaStepControl.InitializeStep(beepService?.DMEEditor, null, _entityTypes, _extraAssemblies);

            return new WizardStep
            {
                Key = "schema-setup",
                Title = "Schema Setup",
                Description = "Plan and apply schema migration actions.",
                Icon = SvgsUIcons.DataTable.Table,
                IsOptional = false,
                Content = _schemaStepControl,
                OnEnter = _ => RefreshSchemaSummary()
            };
        }

        private WizardStep CreateSeedingStep()
        {
            _seedingStepControl = new uc_SetupSeedingStep { Dock = DockStyle.Fill };
            _seedingStepControl.InitializeStep(_seederRegistry, _extraAssemblies);

            return new WizardStep
            {
                Key = "seeding",
                Title = "Seed Initial Data",
                Description = "Run ordered seeders for baseline data.",
                Icon = SvgsUIcons.Agriculture.Seedling,
                IsOptional = _seederRegistry == null,
                Content = _seedingStepControl
            };
        }

        private WizardStep CreateReviewRunStep()
        {
            _reviewStepControl = new uc_SetupReviewRunStep { Dock = DockStyle.Fill };
            _reviewStepControl.ApplyTheme(Theme);
            _reviewStepControl.RunSetupRequested += OnReviewRunSetupRequested;

            return new WizardStep
            {
                Key = "review-run",
                Title = "Review and Run",
                Description = "Validate your setup selections before execution.",
                Icon = SvgsUIcons.Common.Play,
                IsOptional = false,
                Content = _reviewStepControl,
                OnEnter = _ => RefreshReviewStep()
            };
        }

        private void OnReviewRunSetupRequested(object? sender, EventArgs e)
        {
            // Fire-and-forget but exceptions are routed to status instead of
            // crashing the wizard (the previous `async void` lambda could
            // bring down the host process on an unhandled throw).
            _ = RunSetupSafelyAsync();
        }

        private async Task RunSetupSafelyAsync()
        {
            try
            {
                await RunSetupAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Setup run crashed: {ex.Message}");
                if (_reviewStepControl != null)
                {
                    _reviewStepControl.SetProgress(0, $"Progress: Crashed - {ex.Message}");
                    _reviewStepControl.SetRunningState(false);
                }
            }
            finally
            {
                _isRunInProgress = false;
            }
        }

        private void RefreshSchemaSummary()
        {
            if (_schemaStepControl == null || beepService?.DMEEditor == null)
                return;

            var ds = TryGetOpenDataSource();
            _schemaStepControl.InitializeStep(beepService.DMEEditor, ds, _entityTypes, _extraAssemblies);
        }

        private void SyncDriverStepState()
        {
            _driverStepControl?.ResetPackageState();
        }

        private IDataSource? TryGetOpenDataSource()
        {
            var cp = _connectionStepControl?.GetConnectionProperties();
            if (cp == null || string.IsNullOrWhiteSpace(cp.ConnectionName))
                return null;

            var editor = beepService?.DMEEditor;
            if (editor == null)
                return null;

            try
            {
                var ds = editor.GetDataSource(cp.ConnectionName);
                if (ds != null && ds.ConnectionStatus == ConnectionState.Open)
                    return ds;
            }
            catch
            {
                // datasource not registered yet
            }

            return null;
        }

        private void RefreshReviewStep()
        {
            if (_reviewStepControl == null)
                return;

            var builder = new StringBuilder();
            builder.AppendLine("Please review your current setup selections:");
            builder.AppendLine();
            builder.AppendLine(_driverStepControl?.GetStepSummary() ?? "Driver Provision: Pending.");
            builder.AppendLine($"Driver Activity: {_lastDriverProvisionSummary}");
            builder.AppendLine(_connectionStepControl?.GetStepSummary() ?? "Connection: Pending.");
            builder.AppendLine(_schemaStepControl?.GetStepSummary() ?? "Schema: Pending.");
            builder.AppendLine(_seedingStepControl?.GetStepSummary() ?? "Seeding: Pending.");
            builder.AppendLine(SetupExecutor == null
                ? "Execution: Built-in setup framework (preferred) with connection-persist fallback."
                : "Execution: External setup executor mode.");

            _reviewStepControl.SetSummary(builder.ToString());
            _reviewStepControl.SetExecutionPath(_lastExecutionPath);
            _reviewStepControl.SetLastRunSummary(_lastRunSummary);

            if (_isRunInProgress)
            {
                _reviewStepControl.SetRunningState(true);
                return;
            }

            _reviewStepControl.SetRunningState(false);
            _reviewStepControl.SetProgress(0, "Progress: Ready to execute setup pipeline.");
        }

        private async Task RunSetupAsync()
        {
            if (_isRunInProgress)
                return;

            var review = _reviewStepControl;
            if (review == null)
                return;

            var cp = _connectionStepControl?.GetConnectionPropertiesForStep();

            if (SetupExecutor == null && beepService?.DMEEditor == null)
            {
                review.SetProgress(0, "Progress: Beep service is not available.");
                UpdateStatus("Setup run failed: Beep service is not available.");
                return;
            }

            if (SetupExecutor == null && (cp == null || string.IsNullOrWhiteSpace(cp.ConnectionName)))
            {
                review.SetProgress(0, "Progress: Connection details are incomplete.");
                UpdateStatus("Setup run failed: complete connection configuration first.");
                return;
            }

            _isRunInProgress = true;
            review.SetRunningState(true);
            _lastExecutionPath = "Running";
            _lastRunSummary = "Execution in progress...";
            review.SetExecutionPath(_lastExecutionPath);
            review.SetLastRunSummary(_lastRunSummary);
            review.SetProgress(5, "Progress: Starting setup execution...");
            UpdateStatus("Setup run started.");

            _runCts?.Dispose();
            _runCts = new CancellationTokenSource();

            try
            {
                var progress = new Progress<(int Progress, string Message)>(p => review.SetProgress(p.Progress, p.Message));

                var runResult = SetupExecutor != null
                    ? await ExecuteExternalSetupAsync(cp, progress)
                    : await ExecuteDefaultSetupAsync(cp, progress, _runCts.Token);

                _lastExecutionPath = runResult.ExecutionPath;
                _lastRunSummary = runResult.Message;
                review.SetExecutionPath(_lastExecutionPath);
                review.SetLastRunSummary(_lastRunSummary);
                review.SetProgress(runResult.ProgressPercent, runResult.Message);
                if (runResult.Success)
                {
                    SetupCompleted?.Invoke(this, new SetupCompletedEventArgs(GetSnapshot(), runResult));
                }
                UpdateStatus(runResult.Success
                    ? "Setup run completed successfully."
                    : $"Setup run failed: {runResult.Message}");
            }
            catch (OperationCanceledException)
            {
                review.SetProgress(0, "Progress: Cancelled by user.");
                _lastRunSummary = "Cancelled";
                review.SetLastRunSummary(_lastRunSummary);
                UpdateStatus("Setup run cancelled.");
            }
            finally
            {
                _isRunInProgress = false;
                review.SetRunningState(false);
            }
        }

        private async Task<SetupExecutionResult> ExecuteExternalSetupAsync(
            ConnectionProperties? cp,
            IProgress<(int Progress, string Message)>? progress)
        {
            if (SetupExecutor == null)
                return SetupExecutionResult.Fail(0, "Progress: External setup executor is not configured.", "External Executor");

            try
            {
                var request = new SetupExecutionRequest
                {
                    ConnectionProperties = cp,
                    Theme = Theme,
                    AllowSkip = _viewModel.AllowSkip,
                    AllowCancel = _viewModel.AllowCancel
                };

                var result = await SetupExecutor(request, progress);
                return result ?? SetupExecutionResult.Fail(0, "Progress: External setup executor returned no result.", "External Executor");
            }
            catch (Exception ex)
            {
                return SetupExecutionResult.Fail(0, $"Progress: External setup executor failed - {ex.Message}", "External Executor");
            }
        }

        private async Task<SetupExecutionResult> ExecuteDefaultSetupAsync(
            ConnectionProperties? cp,
            IProgress<(int Progress, string Message)>? progress,
            CancellationToken cancellationToken)
        {
            var frameworkResult = await ExecuteSetupFrameworkRunAsync(cp, progress);
            if (frameworkResult != null)
                return frameworkResult;

            if (cp == null || string.IsNullOrWhiteSpace(cp.ConnectionName))
                return SetupExecutionResult.Fail(0, "Progress: Connection details are incomplete.", "Fallback Connection");

            return await ExecuteDatasourceSetupFallbackAsync(cp, progress, cancellationToken);
        }

        private async Task<SetupExecutionResult?> ExecuteSetupFrameworkRunAsync(
            ConnectionProperties? cp,
            IProgress<(int Progress, string Message)>? progress)
        {
            if (beepService?.DMEEditor == null)
                return null;

            try
            {
                progress?.Report((10, "Progress: Preparing setup framework execution..."));

                var (wizard, context) = BuildFrameworkWizard(cp);
                if (wizard == null || context == null)
                {
                    progress?.Report((15, "Progress: Setup framework unavailable, using fallback."));
                    return null;
                }

                progress?.Report((25, $"Progress: Running setup framework pipeline ({wizard.Steps.Count} step(s))..."));

                var adapterProgress = new Progress<PassedArgs>(args =>
                {
                    var pct = args?.ParameterInt1 ?? 0;
                    var msg = string.IsNullOrWhiteSpace(args?.Messege)
                        ? "Progress: Running setup framework..."
                        : args.Messege;
                    progress?.Report((pct, msg));
                });

                var errorsInfo = await Task.Run(() => wizard.Run(context, adapterProgress));

                var isOk = IsErrorsInfoOk(errorsInfo);
                var message = GetErrorsInfoMessage(errorsInfo);

                var report = wizard.GetReport();
                if (report != null)
                    _reviewStepControl?.SetReport(report);

                if (!isOk)
                {
                    progress?.Report((100, $"Progress: Setup framework failed - {message}"));
                    return SetupExecutionResult.Fail(100, $"Progress: Setup framework failed - {message}", "Setup Framework");
                }

                progress?.Report((100, "Progress: Setup framework execution completed successfully."));
                return SetupExecutionResult.Ok(100,
                    $"Progress: Setup framework execution completed successfully. Run={report?.RunId}",
                    "Setup Framework");
            }
            catch (Exception ex)
            {
                progress?.Report((0, $"Progress: Setup framework execution failed - {ex.Message}"));
                return SetupExecutionResult.Fail(0, $"Progress: Setup framework execution failed - {ex.Message}", "Setup Framework");
            }
        }

        private (ISetupWizard? wizard, SetupContext? context) BuildFrameworkWizard(ConnectionProperties? cp)
        {
            if (beepService?.DMEEditor == null)
                return (null, null);

            var context = new SetupContext
            {
                Editor = beepService.DMEEditor,
                Options = new SetupOptions
                {
                    SkipSeeding = _seederRegistry == null,
                    Environment = "Development"
                },
                State = new SetupState()
            };

            if (cp != null)
            {
                context.ConnectionProperties = cp;
                try
                {
                    var ds = beepService.DMEEditor.GetDataSource(cp.ConnectionName);
                    if (ds != null && ds.ConnectionStatus == ConnectionState.Open)
                        context.DataSource = ds;
                }
                catch
                {
                    // datasource not yet registered — the ConnectionConfigStep will open it
                }
            }

            var builder = new SetupWizardBuilder()
                .WithId("platform-setup-winform")
                .WithOptions(context.Options);

            if (cp != null && !string.IsNullOrWhiteSpace(cp.ConnectionName))
            {
                // If the user pinned a driver name in the connection step,
                // ensure the driver is loaded before we try to open the
                // connection. DriverProvisionStep is a no-op when the
                // driver is already in-process.
                if (!string.IsNullOrWhiteSpace(cp.DriverName))
                {
                    builder.AddStep(new DriverProvisionStep(new DriverProvisionStepOptions
                    {
                        PackageName = cp.DriverName,
                        Version = string.IsNullOrWhiteSpace(cp.DriverVersion) ? null : cp.DriverVersion
                    }));
                }

                builder.AddStep(new ConnectionConfigStep(new ConnectionConfigStepOptions
                {
                    ConnectionProperties = cp,
                    OpenConnection = true
                }));
            }

            if (_schemaStepControl != null && _schemaStepControl.IsReadyForSetup())
            {
                try
                {
                    builder.AddStep(new SchemaSetupStep(_schemaStepControl.BuildStepOptions()));
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Schema step not added: {ex.Message}");
                }
            }

            if (_seedingStepControl != null && _seedingStepControl.IsReadyForSetup())
            {
                try
                {
                    builder.AddStep(new SeedingStep(_seedingStepControl.BuildStepOptions()));
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Seeding step not added: {ex.Message}");
                }
            }

            if (string.IsNullOrWhiteSpace(cp?.ConnectionName))
                return (null, context);

            return (builder.Build(), context);
        }

        private static bool IsErrorsInfoOk(object? errorsInfo)
        {
            if (errorsInfo == null)
                return false;

            var flagProp = errorsInfo.GetType().GetProperty("Flag");
            var flagVal = flagProp?.GetValue(errorsInfo);
            return string.Equals(flagVal?.ToString(), "Ok", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetErrorsInfoMessage(object? errorsInfo)
        {
            if (errorsInfo == null)
                return "Unknown setup error.";

            var msgProp = errorsInfo.GetType().GetProperty("Message");
            var message = msgProp?.GetValue(errorsInfo)?.ToString();
            return string.IsNullOrWhiteSpace(message) ? "Unknown setup error." : message;
        }

        private async Task<SetupExecutionResult> ExecuteDatasourceSetupFallbackAsync(
            ConnectionProperties cp,
            IProgress<(int Progress, string Message)>? progress,
            CancellationToken cancellationToken)
        {
            var editor = beepService?.DMEEditor;
            if (editor == null)
                return SetupExecutionResult.Fail(0, "Progress: Editor is not available.", "Fallback Connection");

            try
            {
                var handler = new DatasourceSetupHandler(editor);
                var options = new DatasourceSetupOptions
                {
                    ConnectionProperties = cp,
                    ProvisionDriverIfMissing = false,
                    OpenConnection = true,
                    SkipWhenAlreadyOpen = false
                };

                var result = await handler.SetupAsync(options, progress, cancellationToken).ConfigureAwait(true);

                if (result.Success)
                {
                    var driverNote = result.DriverProvisioned ? " Driver was provisioned." : string.Empty;
                    return SetupExecutionResult.Ok(100,
                        $"Progress: Setup execution completed successfully ({result.Duration.TotalSeconds:0.00}s).{driverNote}",
                        "Fallback Connection");
                }

                return SetupExecutionResult.Fail(100,
                    $"Progress: Setup execution failed - {result.Message}",
                    "Fallback Connection");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return SetupExecutionResult.Fail(0, $"Progress: Setup execution failed - {ex.Message}", "Fallback Connection");
            }
        }

        private void UpdateStatus(string message)
        {
            if (_lblStatus != null)
                _lblStatus.Text = message;
        }

        public SetupWizardSnapshot GetSnapshot()
        {
            return new SetupWizardSnapshot
            {
                ConnectionProperties = _connectionStepControl?.GetConnectionProperties(),
                AllowCancel = _viewModel.AllowCancel,
                AllowSkip = _viewModel.AllowSkip,
                Style = _viewModel.Style,
                LastExecutionPath = _lastExecutionPath,
                LastRunSummary = _lastRunSummary,
                LastDriverProvisionSummary = _lastDriverProvisionSummary,
            };
        }

        /// <summary>
        /// Public entry-point that runs the full datasource setup pipeline
        /// through <see cref="DatasourceSetupHandler"/>: resolve driver →
        /// provision (if missing) → persist connection → open datasource.
        /// </summary>
        /// <remarks>
        /// Callers that want headless / programmatic setup without driving
        /// the wizard UI should prefer this method over the framework
        /// wizard pipeline — it has no UI dependencies and always returns
        /// a structured <see cref="DatasourceSetupResult"/>.
        /// </remarks>
        public async Task<DatasourceSetupResult> RunDatasourceSetupAsync(
            ConnectionProperties connectionProperties,
            DatasourceSetupOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (connectionProperties == null) throw new ArgumentNullException(nameof(connectionProperties));
            var editor = beepService?.DMEEditor;
            if (editor == null)
            {
                return DatasourceSetupResult.Fail(
                    "Editor is not available.",
                    Array.Empty<DatasourceSetupStep>(),
                    TimeSpan.Zero);
            }

            var handler = new DatasourceSetupHandler(editor);
            options ??= new DatasourceSetupOptions { ConnectionProperties = connectionProperties };
            options.ConnectionProperties ??= connectionProperties;

            // Use the review-step progress reporter if the user is watching.
            var review = _reviewStepControl;
            var progress = new Progress<(int Percent, string Message)>(p =>
            {
                if (review != null) review.SetProgress(p.Percent, $"Progress: {p.Message}");
            });

            return await handler.SetupAsync(options, progress, cancellationToken).ConfigureAwait(true);
        }

        public void ApplySnapshot(SetupWizardSnapshot snapshot)
        {
            if (snapshot == null)
                return;

            _viewModel.AllowCancel = snapshot.AllowCancel;
            _viewModel.AllowSkip = snapshot.AllowSkip;
            _viewModel.Style = snapshot.Style;
            _lastExecutionPath = string.IsNullOrWhiteSpace(snapshot.LastExecutionPath)
                ? _lastExecutionPath
                : snapshot.LastExecutionPath;
            _lastRunSummary = string.IsNullOrWhiteSpace(snapshot.LastRunSummary)
                ? _lastRunSummary
                : snapshot.LastRunSummary;
            _lastDriverProvisionSummary = string.IsNullOrWhiteSpace(snapshot.LastDriverProvisionSummary)
                ? _lastDriverProvisionSummary
                : snapshot.LastDriverProvisionSummary;

            BindDefaults();
            if (snapshot.ConnectionProperties != null)
            {
                _connectionStepControl?.SetConnectionProperties(snapshot.ConnectionProperties);
            }

            RefreshReviewStep();
        }

        private void ApplyThemeToBeep(Control? control)
        {
            if (control is IBeepUIComponent component)
                component.Theme = Theme;
        }

        private void DriverStepControl_DriverPackageInstalled(object? sender, uc_SetupDriverStep.DriverPackageInstalledEventArgs e)
        {
            _lastDriverProvisionSummary = e.Success
                ? $"Installed {e.PackageId} {e.Version}"
                : $"Failed install {e.PackageId} {e.Version}: {e.Message}";

            UpdateStatus($"Driver package operation: {_lastDriverProvisionSummary}");
            RefreshReviewStep();
        }

        private void DriverStepControl_DatasourceSetupCompleted(object? sender, DatasourceSetupResult result)
        {
            var details = result.Steps != null && result.Steps.Count > 0
                ? string.Join(" | ", result.Steps.Select(s => $"{(s.Success ? "OK" : "FAIL")}:{s.Name}"))
                : "(no steps)";

            _lastDriverProvisionSummary = result.Success
                ? $"Datasource ready in {result.Duration.TotalSeconds:0.00}s. Steps: {details}"
                : $"Datasource setup failed: {result.Message}. Steps: {details}";

            UpdateStatus(result.Success
                ? $"Datasource '{result.DataSource?.DatasourceName ?? "(unknown)"}' ready."
                : $"Datasource setup failed: {result.Message}");

            RefreshReviewStep();
        }

        private void ConnectionStepControl_ConnectionSaved(object? sender, uc_SetupConnectionStep.ConnectionSavedEventArgs e)
        {
            UpdateStatus($"Connection saved: {e.ConnectionProperties.ConnectionName}");
            RefreshReviewStep();
        }

        private void ConnectionStepControl_ConnectionTestCompleted(object? sender, uc_SetupConnectionStep.ConnectionTestCompletedEventArgs e)
        {
            var stateText = e.Success ? "passed" : "failed";
            UpdateStatus($"Connection test {stateText}: {e.Message}");
            RefreshReviewStep();
        }

        private sealed class SetupWizardViewModel
        {
            public WizardStyle Style { get; set; } = WizardStyle.HorizontalStepper;
            public bool AllowCancel { get; set; } = true;
            public bool AllowSkip { get; set; } = false;
        }

        public sealed class SetupExecutionRequest
        {
            public ConnectionProperties? ConnectionProperties { get; set; }
            public string Theme { get; set; } = string.Empty;
            public bool AllowSkip { get; set; }
            public bool AllowCancel { get; set; }
        }

        public sealed class SetupExecutionResult
        {
            public bool Success { get; }
            public int ProgressPercent { get; }
            public string Message { get; }
            public string ExecutionPath { get; }

            private SetupExecutionResult(bool success, int progressPercent, string message, string executionPath)
            {
                Success = success;
                ProgressPercent = progressPercent;
                Message = message;
                ExecutionPath = executionPath;
            }

            public static SetupExecutionResult Ok(int progressPercent, string message, string executionPath = "Unknown") =>
                new SetupExecutionResult(true, progressPercent, message, executionPath);

            public static SetupExecutionResult Fail(int progressPercent, string message, string executionPath = "Unknown") =>
                new SetupExecutionResult(false, progressPercent, message, executionPath);
        }

        public sealed class SetupCompletedEventArgs : EventArgs
        {
            public SetupCompletedEventArgs(SetupWizardSnapshot snapshot, SetupExecutionResult result)
            {
                Snapshot = snapshot;
                Result = result;
            }

            public SetupWizardSnapshot Snapshot { get; }
            public SetupExecutionResult Result { get; }
        }

        public sealed class SetupWizardSnapshot
        {
            public ConnectionProperties? ConnectionProperties { get; set; }
            public bool AllowCancel { get; set; } = true;
            public bool AllowSkip { get; set; }
            public WizardStyle Style { get; set; } = WizardStyle.HorizontalStepper;
            public string LastExecutionPath { get; set; } = string.Empty;
            public string LastRunSummary { get; set; } = string.Empty;
            public string LastDriverProvisionSummary { get; set; } = string.Empty;
        }
    }
}
