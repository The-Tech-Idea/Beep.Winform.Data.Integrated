using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms;

public partial class BeepForms
{
    private BeepFormsKeyboardShortcutProvider _keyboardShortcutProvider = new();

    [System.ComponentModel.Browsable(false)]
    public BeepFormsKeyboardShortcutProvider KeyboardShortcutProvider
    {
        get => _keyboardShortcutProvider;
        set
        {
            _keyboardShortcutProvider = value ?? new BeepFormsKeyboardShortcutProvider();
            _keyboardShortcutProvider.SetForm(this);
            RegisterDefaultShortcuts();
        }
    }

    public IReadOnlyList<BeepFormsKeyboardShortcut> GetKeyboardShortcuts() =>
        _keyboardShortcutProvider.Shortcuts;

    private void RegisterDefaultShortcuts()
    {
        var provider = _keyboardShortcutProvider;
        provider.SetForm(this);

        provider.Register(new BeepFormsKeyboardShortcut
        {
            KeyData = Keys.Control | Keys.S,
            DisplayName = "Commit / Save",
            Category = "Data",
            ExecuteAsync = async (f) => { var r = await f.CommitFormAsync(); return r.Flag == Errors.Ok; }
        });

        provider.Register(new BeepFormsKeyboardShortcut
        {
            KeyData = Keys.F7,
            DisplayName = "Enter Query",
            Category = "Query",
            ExecuteAsync = async (f) => await f.EnterQueryAsync()
        });

        provider.Register(new BeepFormsKeyboardShortcut
        {
            KeyData = Keys.Control | Keys.F,
            DisplayName = "Enter Query (Ctrl+F)",
            Category = "Query",
            ExecuteAsync = async (f) => await f.EnterQueryAsync()
        });

        provider.Register(new BeepFormsKeyboardShortcut
        {
            KeyData = Keys.F8,
            DisplayName = "Execute Query",
            Category = "Query",
            ExecuteAsync = async (f) => await f.ExecuteQueryAsync()
        });

        provider.Register(new BeepFormsKeyboardShortcut
        {
            KeyData = Keys.F9,
            DisplayName = "Show LOV",
            Category = "Navigation",
            RequiresActiveBlock = true,
            ExecuteAsync = async (f) =>
            {
                if (string.IsNullOrWhiteSpace(f.ActiveBlockName)) return false;
                await f.ShowLovAsync(f.ActiveBlockName!, f.ActiveItemName ?? "");
                return true;
            }
        });

        provider.Register(new BeepFormsKeyboardShortcut
        {
            KeyData = Keys.Control | Keys.I,
            DisplayName = "Insert Record",
            Category = "Data",
            ExecuteAsync = async (f) => await f.InsertRecordAsync()
        });

        provider.Register(new BeepFormsKeyboardShortcut
        {
            KeyData = Keys.Control | Keys.N,
            DisplayName = "New Record (Ctrl+N)",
            Category = "Data",
            ExecuteAsync = async (f) => await f.InsertRecordAsync()
        });

        provider.Register(new BeepFormsKeyboardShortcut
        {
            KeyData = Keys.Control | Keys.D,
            DisplayName = "Delete Record",
            Category = "Data",
            ExecuteAsync = async (f) => await f.DeleteCurrentRecordAsync()
        });

        provider.Register(new BeepFormsKeyboardShortcut
        {
            KeyData = Keys.Control | Keys.G,
            DisplayName = "Go Block",
            Category = "Navigation",
            RequiresActiveBlock = true,
            ExecuteAsync = async (f) =>
            {
                if (string.IsNullOrWhiteSpace(f.ActiveBlockName)) return false;
                return await f.SwitchToBlockAsync(f.ActiveBlockName);
            }
        });

        provider.Register(new BeepFormsKeyboardShortcut
        {
            KeyData = Keys.Control | Keys.R,
            DisplayName = "Rollback",
            Category = "Data",
            ExecuteAsync = async (f) => { var r = await f.RollbackFormAsync(); return r.Flag == Errors.Ok; }
        });

        provider.Register(new BeepFormsKeyboardShortcut
        {
            KeyData = Keys.Escape,
            DisplayName = "Cancel Query",
            Category = "Query",
            RequiresActiveBlock = true,
            ExecuteAsync = async (f) =>
            {
                await f.ExitQueryAsync();
                return true;
            }
        });
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (_keyboardShortcutProvider.ProcessCmdKey(msg, keyData))
            return true;
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
