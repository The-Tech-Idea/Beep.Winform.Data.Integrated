using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.SetUp;
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

        private ISetupStep? _canonicalStep;
        private SetupContext? _canonicalContext;
        private CancellationTokenSource? _runCts;

        public uc_SetupReviewRunStep()
        {
            InitializeComponent();
            _btnRunSetup.Click += BtnRunSetup_Click;
        }

        /// <summary>
        /// Bind the review panel to a canonical <see cref="ISetupStep"/> + <see cref="SetupContext"/>.
        /// The Run button will invoke step.Validate(context) then step.Execute(context, progress).
        /// </summary>
        public void BindToCanonicalStep(ISetupStep step, SetupContext context)
        {
            _canonicalStep = step;
            _canonicalContext = context;
            _lblSummary.Text = $"Step: {step.StepName} — {step.Description}";
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
            if (_canonicalStep == null || _canonicalContext == null)
            {
                SetLastRunSummary("Canonical step / context not bound. The wizard host should call BindToCanonicalStep before showing the review step.");
                return;
            }

            // Fire-and-forget but exceptions are routed to status instead of crashing the host.
            _ = RunSetupSafelyAsync();
        }

        private async Task RunSetupSafelyAsync()
        {
            _runCts?.Cancel();
            _runCts?.Dispose();
            _runCts = new CancellationTokenSource();
            var token = _runCts.Token;

            try
            {
                SetRunningState(true);
                if (_canonicalContext != null)
                    _canonicalContext.State.StartedAt = DateTimeOffset.UtcNow;

                var progress = new Progress<PassedArgs>(args =>
                {
                    if (args.ParameterInt1 > 0)
                        SetProgress(args.ParameterInt1, args.Messege ?? string.Empty);
                    _lblProgressStatus.Text = args.Messege ?? string.Empty;
                });

                var result = _canonicalStep.Validate(_canonicalContext!);
                if (result.Flag == Errors.Failed)
                {
                    SetLastRunSummary($"Validation failed: {result.Message}");
                    return;
                }

                result = _canonicalStep.Execute(_canonicalContext!, progress);
                if (result.Flag == Errors.Ok)
                    _canonicalContext?.State.CompletedStepIds.Add(_canonicalStep.StepId);

                SetLastRunSummary(result.Flag == Errors.Ok
                    ? "Setup step completed successfully."
                    : $"Setup step failed: {result.Message}");
                SetProgress(result.Flag == Errors.Ok ? 100 : 0, _lblProgressStatus.Text);
            }
            catch (Exception ex)
            {
                SetLastRunSummary($"Setup step crashed: {ex.Message}");
            }
            finally
            {
                SetRunningState(false);
            }
        }

    }
}
