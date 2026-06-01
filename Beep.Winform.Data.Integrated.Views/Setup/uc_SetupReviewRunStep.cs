using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.ProgressBars;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    public partial class uc_SetupReviewRunStep : UserControl
    {
        public event EventHandler? RunSetupRequested;

        public uc_SetupReviewRunStep()
        {
            InitializeComponent();
            _btnRunSetup.Click += BtnRunSetup_Click;
        }

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
    }
}
