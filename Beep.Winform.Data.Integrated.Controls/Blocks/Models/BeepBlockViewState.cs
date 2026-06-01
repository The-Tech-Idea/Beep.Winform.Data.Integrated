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
        public DataBlockMode Mode { get; set; } = DataBlockMode.Query;
    }
}