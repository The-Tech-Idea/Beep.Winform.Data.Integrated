using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.UOW.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;

public sealed class WinFormDirtyStatePanel : WinFormFormsFeatureControl
{
    public WinFormDirtyStatePanel(IBeepFormsHost host)
        : base(host)
    {
    }

    public IReadOnlyList<string> GetDirtyBlocks() =>
        Host.GetDirtyBlocks() ?? [];

    public IReadOnlyDictionary<string, ChangeSummary> GetChangeSummary() =>
        Host.GetFormChangeSummary();

    public Task<bool> SaveAsync(CancellationToken ct = default) =>
        Host.SaveDirtyBlocksAsync(ct);

    public Task<bool> RollbackAsync(CancellationToken ct = default) =>
        Host.RollbackDirtyBlocksAsync(ct);
}
