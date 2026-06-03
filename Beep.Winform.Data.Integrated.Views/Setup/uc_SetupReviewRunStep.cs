using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.SetUp;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.ProgressBars;
using TheTechIdea.Beep.Winform.Controls.Wizards;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    public partial class uc_SetupReviewRunStep : UserControl, IWizardStepContent
    {
        public event EventHandler? RunSetupRequested;

        private SetupReport? _lastReport;
        private WizardContext? _wizardContext;
        private bool _isComplete;

        public uc_SetupReviewRunStep()
        {
            InitializeComponent();
            _btnRunSetup.Click += BtnRunSetup_Click;
        }

        public bool IsComplete
        {
            get => _isComplete;
            private set
            {
                if (_isComplete == value) return;
                _isComplete = value;
                ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(_isComplete, _isComplete ? string.Empty : "Review not ready"));
            }
        }

        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;

        public string NextButtonText { get; set; } = string.Empty;

        public void SetSummary(string summary)
        {
            _lblSummary.Text = summary;
        }

        public void SetProgress(int value, string status)
        {
            _progressBar.Value = value < 0 ? 0 : value > 100 ? 100 : value;
            _lblProgressStatus.Text = status;
        }

        public void SetExecutionPath(string executionPath)
        {
            _lblExecutionPath.Text = $"Execution Path: {executionPath}";
        }

        public void SetLastRunSummary(string summary)
        {
            _lblLastRunSummary.Text = $"Last Run: {summary}";
        }

        public void SetRunningState(bool isRunning)
        {
            _btnRunSetup.Enabled = !isRunning;
        }

        public void SetReport(SetupReport? report)
        {
            _lastReport = report;
            if (report == null)
                return;

            var sb = new StringBuilder();
            sb.AppendLine($"Wizard: {report.WizardId} (Run {report.RunId})");
            sb.AppendLine($"Environment: {report.Environment}");
            sb.AppendLine($"Started: {report.StartedAt.LocalDateTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Finished: {report.FinishedAt.LocalDateTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Total: {report.TotalElapsed}");
            sb.AppendLine($"Succeeded: {report.Succeeded}");
            sb.AppendLine($"Content hash: {report.ContentHash}");
            sb.AppendLine();
            sb.AppendLine("Step results:");

            if (report.StepResults == null || report.StepResults.Count == 0)
            {
                sb.AppendLine("  (no steps reported)");
            }
            else
            {
                int idx = 1;
                foreach (var result in report.StepResults)
                {
                    var status = result.Succeeded
                        ? (result.Skipped ? "SKIPPED" : "OK")
                        : "FAILED";
                    sb.AppendLine($"  {idx++}. [{status}] {result.StepName} ({result.StepId}) - {result.Message} ({result.Elapsed})");
                }
            }

            _lblLastRunSummary.Text = sb.ToString();
        }

        public SetupReport? LastReport => _lastReport;

        public void ApplyTheme(string theme)
        {
            ApplyThemeToControl(_rootPanel, theme);
            ApplyThemeToControl(_headerPanel, theme);
            ApplyThemeToControl(_contentPanel, theme);
            ApplyThemeToControl(_lblTitle, theme);
            ApplyThemeToControl(_lblDescription, theme);
            ApplyThemeToControl(_lblSummary, theme);
            ApplyThemeToControl(_lblExecutionPath, theme);
            ApplyThemeToControl(_lblLastRunSummary, theme);
            ApplyThemeToControl(_lblProgressStatus, theme);
            ApplyThemeToControl(_progressBar, theme);
            ApplyThemeToControl(_btnRunSetup, theme);
        }

        private void BtnRunSetup_Click(object? sender, System.EventArgs e)
        {
            RunSetupRequested?.Invoke(this, System.EventArgs.Empty);
        }

        private static void ApplyThemeToControl(Control control, string theme)
        {
            if (control is IBeepUIComponent beepComponent)
                beepComponent.Theme = theme;
        }

        void IWizardStepContent.OnStepEnter(WizardContext context)
        {
            _wizardContext = context;
            IsComplete = true;
        }

        void IWizardStepContent.OnStepLeave(WizardContext context)
        {
        }

        WizardValidationResult IWizardStepContent.Validate()
        {
            return WizardValidationResult.Success();
        }

        Task<WizardValidationResult> IWizardStepContent.ValidateAsync()
        {
            return Task.FromResult(WizardValidationResult.Success());
        }
    }
}
