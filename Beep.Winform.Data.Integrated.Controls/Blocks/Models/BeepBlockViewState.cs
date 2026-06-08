using TheTechIdea.Beep.Editor.UOWManager.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models
{
    public sealed class BeepBlockViewState
    {
        public bool IsBound { get; set; }
        public bool IsDirty { get; set; }
        public bool IsQueryMode { get; set; }
        public int CurrentRecordIndex { get; set; } = -1;
        public int RecordCount { get; set; }
        public string ManagerBlockName { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string ConnectionName { get; set; } = string.Empty;
        public int FieldCount { get; set; }
        public int TriggerCount { get; set; }
        public int FormTriggerCount { get; set; }
        public int BlockTriggerCount { get; set; }
        public int RecordTriggerCount { get; set; }
        public int ItemTriggerCount { get; set; }
        public string LastTriggerText { get; set; } = string.Empty;
        public string LastUnitOfWorkActivityText { get; set; } = string.Empty;
        public BeepBlockEntityDefinition Entity { get; set; } = new();
        public DataBlockMode Mode { get; set; } = DataBlockMode.EnterQuery;

        /// <summary>
        /// Oracle Forms <c>:SYSTEM.RECORD_STATUS</c> for the current record. The
        /// block view updates this whenever the underlying unit-of-work
        /// reports a new status, and the navigation bar renders the visual
        /// indicator (asterisk for NEW/CHANGED) from this value.
        /// </summary>
        public BeepRecordStatus RecordStatus { get; set; } = BeepRecordStatus.Query;

        /// <summary>
        /// The Oracle Forms <c>:SYSTEM.CURSOR_BLOCK</c> value for this view.
        /// Defaults to the block's <see cref="BlockName"/> (or
        /// <see cref="ManagerBlockName"/> if set) when bound.
        /// </summary>
        public string CursorBlock { get; set; } = string.Empty;

        /// <summary>
        /// The Oracle Forms <c>:SYSTEM.CURSOR_ITEM</c> value for this view.
        /// Updated by the field editor when navigation moves between items.
        /// </summary>
        public string CursorItem { get; set; } = string.Empty;

        /// <summary>
        /// The 1-based record number of the current record the way
        /// <c>:SYSTEM.CURSOR_RECORD</c> reports it. <c>0</c> means "no current
        /// record" (e.g. before the first successful query).
        /// </summary>
        public int CursorRecord { get; set; }

        /// <summary>
        /// Effective Oracle Forms <c>:SYSTEM.MESSAGE_LEVEL</c> value for this
        /// view. 5 = suppress everything, 0 = all messages, 10 = highest
        /// detail. Mirrors the Forms runtime constants.
        /// </summary>
        public int MessageLevel { get; set; }
    }
}