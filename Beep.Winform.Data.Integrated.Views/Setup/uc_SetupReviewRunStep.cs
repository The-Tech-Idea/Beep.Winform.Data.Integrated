using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.SetUp;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.ProgressBars;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    // Full refactor: the review panel binds to the canonical ISetupStep + SetupContext.
    // The Run button invokes step.Validate(context) and step.Execute(context, progress)
    // on the canonical framework — no local SetupExecutor, no legacy wizard types.
    public partial class uc_SetupReviewRunStep : TemplateUserControl
    {
        public event EventHandler? RunSetupRequested;

        private ISetupWizard? _canonicalWizard;
        private SetupContext? _canonicalContext;

        /// <summary>Phase 9 version info line, created in code and docked at the top of the content panel.</summary>
        private readonly BeepLabel _lblVersionInfo = new BeepLabel
        {
            Dock = DockStyle.Top,
            Text = "Versioning: —",
            AutoSize = false,
            Height = 24
        };

        public uc_SetupReviewRunStep()
        {
            InitializeComponent();
            _btnRunSetup.Click += BtnRunSetup_Click;

            // Add the version-info line above the existing summary. The content panel docks its
            // children Top/Fill, so a Top-docked label reflows cleanly; BringToFront keeps it topmost.
            _contentPanel.Controls.Add(_lblVersionInfo);
            _lblVersionInfo.BringToFront();
        }

        /// <summary>
        /// Phase 9: shows the versioning posture for this run — whether the app migrates on startup, the
        /// declared schema version (or "auto" when none), and the version currently recorded in the target
        /// database ("(unversioned)" when the marker is absent).
        /// </summary>
        public void SetVersionInfo(bool migrateOnStartup, string? declaredVersion, string? currentDbVersion)
        {
            var declared = string.IsNullOrWhiteSpace(declaredVersion) ? "auto (entity-diff)" : declaredVersion;
            var current = string.IsNullOrWhiteSpace(currentDbVersion) ? "(unversioned)" : currentDbVersion;
            _lblVersionInfo.Text =
                $"Migrate on startup: {(migrateOnStartup ? "yes" : "no")}   •   " +
                $"Declared version: {declared}   •   Current DB version: {current}";
        }

        /// <summary>
        /// Bind the review panel to the canonical <see cref="ISetupWizard"/> + <see cref="SetupContext"/>,
        /// and summarise every step the run will perform.
        /// </summary>
        /// <remarks>
        /// The panel deliberately does not execute anything itself. The Run button raises
        /// <see cref="RunSetupRequested"/> so the host runs the whole pipeline through
        /// <c>SetupWizard.Run</c> — which owns the state lease, authorization, audit trail, and
        /// checkpoint persistence. Executing a step directly here would skip all of that.
        /// </remarks>
        public void BindToWizard(ISetupWizard wizard, SetupContext context)
        {
            _canonicalWizard = wizard;
            _canonicalContext = context;

            var steps = wizard?.Steps ?? (IReadOnlyList<ISetupStep>)Array.Empty<ISetupStep>();
            _lblSummary.Text = steps.Count > 0
                ? $"{steps.Count} step(s) will run: {string.Join(" → ", steps.Select(s => s.StepName))}"
                : "No steps are registered for this setup.";
        }

        public void SetSummary(string summary) => _lblSummary.Text = summary;
        public void SetProgress(int value, string status)
        {
            _progressBar.Value = value < 0 ? 0 : value > 100 ? 100 : value;
            _lblProgressStatus.Text = status;
        }
        public void SetExecutionPath(string executionPath) => _lblExecutionPath.Text = $"Execution Path: {executionPath}";
        public void SetLastRunSummary(string summary) => _lblLastRunSummary.Text = $"Last Run: {summary}";
        public void SetRunningState(bool isRunning) => _btnRunSetup.Enabled = !isRunning;

        private void BtnRunSetup_Click(object? sender, EventArgs e)
        {
            if (_canonicalWizard == null || _canonicalContext == null)
            {
                SetLastRunSummary("Setup wizard / context not bound. The host should call BindToWizard before showing the review step.");
                return;
            }

            if (RunSetupRequested == null)
            {
                // Loud rather than silent: without a host handler nothing would run, and previously
                // this panel quietly executed only the last step instead of the pipeline.
                SetLastRunSummary("No host is listening for RunSetupRequested; setup was not started.");
                return;
            }

            SetRunningState(true);
            _canonicalContext.State.StartedAt = DateTimeOffset.UtcNow;
            // The host owns execution and calls SetRunningState(false) when the report arrives.
            RunSetupRequested.Invoke(this, EventArgs.Empty);
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
            _contentPanel.Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this);
            _btnRunSetup.MinimumSize = new System.Drawing.Size(
                BeepLayoutMetrics.ButtonLarge.Width.ScaleValue(this),
                BeepLayoutMetrics.ButtonLarge.Height.ScaleValue(this));
        }

    }
}
