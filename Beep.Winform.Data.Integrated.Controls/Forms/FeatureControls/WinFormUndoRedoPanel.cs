using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.UOW.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;

public sealed class WinFormUndoRedoPanel : WinFormFormsFeatureControl
{
    public WinFormUndoRedoPanel(
        IBeepFormsHost host,
        string blockName)
        : base(host, blockName)
    {
    }

    public void Configure(bool enabled, int maxDepth = 50) =>
        Host.SetBlockUndoEnabled(RequireBlockName(), enabled, maxDepth);

    public bool Undo() =>
        Host.UndoBlock(RequireBlockName());

    public bool Redo() =>
        Host.RedoBlock(RequireBlockName());

    public bool CanUndo() =>
        Host.CanUndoBlock(RequireBlockName());

    public bool CanRedo() =>
        Host.CanRedoBlock(RequireBlockName());

    public ChangeSummary GetChangeSummary() =>
        Host.GetBlockChangeSummary(RequireBlockName());
}
