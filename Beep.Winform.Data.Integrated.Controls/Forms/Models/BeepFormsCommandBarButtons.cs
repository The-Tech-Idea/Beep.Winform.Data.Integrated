using System;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models
{
    [Flags]
    public enum BeepFormsCommandBarButtons
    {
        None = 0,
        BlockSelector = 1,
        Sync = 2,
        All = BlockSelector | Sync
    }
}