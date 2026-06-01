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
        All = Savepoints | Alerts
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

    public enum BeepFormsShellToolbarOrder
    {
        SavepointsFirst = 0,
        AlertsFirst = 1
    }
}