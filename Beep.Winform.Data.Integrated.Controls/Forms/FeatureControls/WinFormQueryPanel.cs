using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;

public sealed class WinFormQueryPanel : WinFormFormsFeatureControl
{
    public WinFormQueryPanel(IBeepFormsHost host, IBlockView block)
        : base(host, block?.BlockName)
    {
        Block = block ?? throw new ArgumentNullException(nameof(block));
    }

    public IBlockView Block { get; }

    public void EnterQuery() => Block.EnterQueryMode();

    public void CancelQuery() => Block.ExitQueryMode();

    public Task<bool> ExecuteAsync(CancellationToken ct = default) =>
        Block.ExecuteQueryAsync(ct);

    public void SaveTemplate(
        string name,
        IReadOnlyDictionary<string, QueryCriterion> criteria) =>
        Host.SaveQueryTemplate(RequireBlockName(), name, criteria);

    public IReadOnlyList<QueryTemplateInfo> GetTemplates() =>
        Host.GetQueryTemplates(RequireBlockName());
}
