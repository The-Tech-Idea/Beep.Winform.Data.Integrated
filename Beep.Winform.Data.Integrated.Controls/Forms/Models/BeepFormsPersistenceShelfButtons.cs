using System;
using TheTechIdea.Beep.Editor.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models
{
    [Flags]
    public enum BeepFormsPersistenceShelfButtons
    {
        None = 0,
        Commit = 1,
        Rollback = 2,
        BatchCommit = 4,
        All = Commit | Rollback | BatchCommit,
        Core = Commit | Rollback
    }
}