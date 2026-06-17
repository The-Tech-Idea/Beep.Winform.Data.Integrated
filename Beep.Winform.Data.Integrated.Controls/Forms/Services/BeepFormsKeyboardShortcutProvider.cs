using System.Windows.Forms;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms;

public sealed class BeepFormsKeyboardShortcutProvider
{
    private readonly Dictionary<Keys, BeepFormsKeyboardShortcut> _shortcuts = new();
    private BeepForms? _forms;

    public IReadOnlyList<BeepFormsKeyboardShortcut> Shortcuts => _shortcuts.Values.ToList();

    public void Register(BeepFormsKeyboardShortcut shortcut)
    {
        _shortcuts[shortcut.KeyData] = shortcut;
    }

    public void Unregister(Keys keyData)
    {
        _shortcuts.Remove(keyData);
    }

    public void SetForm(BeepForms? form)
    {
        _forms = form;
    }

    public bool ProcessCmdKey(Message msg, Keys keyData)
    {
        if (_forms == null) return false;
        if (!_shortcuts.TryGetValue(keyData, out var shortcut)) return false;

        if (shortcut.RequiresManager && _forms.FormsManager == null) return false;
        if (shortcut.RequiresActiveBlock && string.IsNullOrWhiteSpace(_forms.ActiveBlockName)) return false;

        if (shortcut.ExecuteAsync != null)
        {
            _ = shortcut.ExecuteAsync(_forms);
            return true;
        }
        return false;
    }
}
