using System;
using System.Windows.Forms;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models
{
    [Flags]
    public enum BeepFormsShellToolbarButtons
    {
        None = 0,
        Savepoints = 1,
        Alerts = 2,
        Builtins = 4,
        All = Savepoints | Alerts | Builtins
    }

    [Flags]
    public enum BeepFormsSavepointToolbarActions
    {
        None = 0,
        Capture = 1,
        List = 2,
        Rollback = 4,
        Release = 8,
        ReleaseAll = 16,
        All = Capture | List | Rollback | Release | ReleaseAll
    }

    [Flags]
    public enum BeepFormsAlertToolbarPresets
    {
        None = 0,
        Info = 1,
        Caution = 2,
        Stop = 4,
        Question = 8,
        All = Info | Caution | Stop | Question
    }

    /// <summary>
    /// Built-in actions exposed by the toolbar popup when
    /// <see cref="BeepFormsShellToolbarButtons.Builtins"/> is selected.
    /// Each flag maps to a single <see cref="TheTechIdea.Beep.Editor.Forms.Builtins.IBeepBuiltins"/>
    /// method (Oracle Forms GO_BLOCK, NEXT_RECORD, COMMIT_FORM, etc.).
    /// </summary>
    [Flags]
    public enum BeepFormsBuiltinToolbarActions
    {
        None = 0,
        FirstBlock = 1,
        LastBlock = 2,
        NextBlock = 4,
        PreviousBlock = 8,
        EnterQuery = 16,
        ExecuteQuery = 32,
        ExitQuery = 64,
        Commit = 128,
        Rollback = 256,
        Post = 512,
        ClearBlock = 1024,
        ClearForm = 2048,
        ClearRecord = 4096,
        All = FirstBlock | LastBlock | NextBlock | PreviousBlock
              | EnterQuery | ExecuteQuery | ExitQuery
              | Commit | Rollback | Post
              | ClearBlock | ClearForm | ClearRecord
    }

    public enum BeepFormsShellToolbarOrder
    {
        SavepointsFirst = 0,
        AlertsFirst = 1,
        BuiltinsFirst = 2
    }
}