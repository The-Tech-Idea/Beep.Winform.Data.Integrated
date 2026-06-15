using System;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models
{
    [Flags]
    public enum BeepFormsCommandBarButtons
    {
        None = 0,
        BlockSelector = 1,
        Sync = 2,
        PreviousBlock = 4,
        NextBlock = 8,
        InsertRecord = 16,
        DeleteRecord = 32,
        DuplicateRecord = 64,
        ClearRecord = 128,
        ClearBlock = 256,
        ShowLOV = 512,
        FirstBlock = 1024,
        LastBlock = 2048,
        RefreshBlock = 4096,
        Undo = 8192,
        Redo = 16384,
        ExportJson = 32768,
        ImportJson = 131072,
        JumpToFirstError = 65536,
        ToggleGridView = 262144,
        All = BlockSelector | Sync | PreviousBlock | NextBlock | FirstBlock | LastBlock
            | InsertRecord | DeleteRecord | DuplicateRecord
            | ClearRecord | ClearBlock | ShowLOV | RefreshBlock
            | Undo | Redo | ExportJson | JumpToFirstError | ImportJson | ToggleGridView,
        BlockNav = PreviousBlock | NextBlock | FirstBlock | LastBlock,
        Crud = InsertRecord | DeleteRecord | DuplicateRecord,
        Clear = ClearRecord | ClearBlock,
        UndoRedo = Undo | Redo,
        Core = BlockSelector | Sync | BlockNav | Crud | Clear | ShowLOV | RefreshBlock | UndoRedo | ExportJson | ImportJson | ToggleGridView
    }
}