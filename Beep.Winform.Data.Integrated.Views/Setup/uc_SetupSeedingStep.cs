using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    public partial class uc_SetupSeedingStep : UserControl
    {
        public uc_SetupSeedingStep()
        {
            InitializeComponent();
        }

        public string GetStepSummary()
        {
            return "Seeding: Manual seeding review step configured.";
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
    }
}
