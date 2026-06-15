using System;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models
{
    [Flags]
    public enum BeepFormsQueryShelfButtons
    {
        None = 0,
        EnterQuery = 1,
        ExecuteQuery = 2,
        ExitQuery = 4,
        All = EnterQuery | ExecuteQuery | ExitQuery,
        Core = EnterQuery | ExecuteQuery
    }
}