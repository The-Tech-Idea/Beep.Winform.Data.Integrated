using System;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models
{
    [Flags]
    public enum BeepFormsPersistenceShelfButtons
    {
        None = 0,
        Commit = 1,
        Rollback = 2,
        All = Commit | Rollback
    }
}