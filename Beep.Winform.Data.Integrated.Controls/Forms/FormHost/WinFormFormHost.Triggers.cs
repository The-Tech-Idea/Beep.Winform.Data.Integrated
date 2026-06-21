using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public partial class WinFormFormHost
{
    public IReadOnlyList<TriggerDefinition> GetBlockTriggers(string blockName) =>
        RequireManager().Triggers.GetBlockTriggers(NormalizeBlockName(blockName));

    public TriggerStatisticsInfo GetTriggerStatistics(string blockName) =>
        RequireManager().Triggers.GetTriggerStatistics(NormalizeBlockName(blockName));

    public Task<TriggerResult> FireBlockTriggerAsync(
        TriggerType type,
        string blockName,
        TriggerContext? context = null,
        CancellationToken ct = default) =>
        RequireManager().Triggers.FireBlockTriggerAsync(
            type,
            NormalizeBlockName(blockName),
            context,
            ct);

    public Task<TriggerResult> FireKeyTriggerAsync(
        KeyTriggerType keyType,
        string blockName) =>
        RequireManager().FireKeyTriggerAsync(
            keyType,
            NormalizeBlockName(blockName));

    public void EnableTrigger(string triggerId) =>
        RequireManager().Triggers.EnableTrigger(triggerId);

    public void DisableTrigger(string triggerId) =>
        RequireManager().Triggers.DisableTrigger(triggerId);

    public void SuspendTriggers() =>
        RequireManager().Triggers.SuspendTriggers();

    public void ResumeTriggers() =>
        RequireManager().Triggers.ResumeTriggers();
}
