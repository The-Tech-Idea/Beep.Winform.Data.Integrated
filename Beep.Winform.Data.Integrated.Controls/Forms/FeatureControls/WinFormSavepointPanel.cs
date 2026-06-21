using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;

public sealed class WinFormSavepointPanel : WinFormFormsFeatureControl
{
    public WinFormSavepointPanel(IBeepFormsHost host, string blockName)
        : base(host, blockName)
    {
    }

    public string Create(string? name = null) =>
        Host.CreateSavepoint(RequireBlockName(), name);

    public Task<bool> RollbackAsync(
        string name,
        CancellationToken ct = default) =>
        Host.RollbackToSavepointAsync(RequireBlockName(), name, ct);

    public bool Release(string name) =>
        Host.ReleaseSavepoint(RequireBlockName(), name);

    public void ReleaseAll() =>
        Host.ReleaseAllSavepoints(RequireBlockName());

    public IReadOnlyList<SavepointInfo> GetSavepoints() =>
        Host.GetSavepoints(RequireBlockName());
}
