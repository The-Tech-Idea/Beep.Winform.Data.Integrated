using System;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models
{
    [Flags]
    public enum BeepFormsRecordNavigationShelfButtons
    {
        None = 0,
        First = 1,
        Previous = 2,
        PositionLabel = 4,
        Next = 8,
        Last = 16,
        All = First | Previous | PositionLabel | Next | Last,
        Core = Previous | PositionLabel | Next
    }
}
