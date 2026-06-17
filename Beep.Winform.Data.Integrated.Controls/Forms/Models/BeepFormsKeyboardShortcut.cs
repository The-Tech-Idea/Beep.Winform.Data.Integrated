using System.Windows.Forms;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms;

public sealed class BeepFormsKeyboardShortcut
{
    public Keys KeyData { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string Category { get; init; } = "General";
    public bool RequiresActiveBlock { get; init; } = true;
    public bool RequiresManager { get; init; } = true;
    public Func<BeepForms, Task<bool>>? ExecuteAsync { get; init; }
}
