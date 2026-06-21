using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOW.Models;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Editor.UOWManager.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public partial class WinFormFormHost
{
    public void SetBlockUndoEnabled(
        string blockName,
        bool enabled,
        int maxDepth = 50) =>
        RequireManager().SetBlockUndoEnabled(
            NormalizeBlockName(blockName),
            enabled,
            maxDepth);

    public bool UndoBlock(string blockName) =>
        ExecuteStateChange(blockName, manager =>
            manager.UndoBlock(NormalizeBlockName(blockName)));

    public bool RedoBlock(string blockName) =>
        ExecuteStateChange(blockName, manager =>
            manager.RedoBlock(NormalizeBlockName(blockName)));

    public bool CanUndoBlock(string blockName) =>
        TryReadManager(
            manager => manager.CanUndoBlock(NormalizeBlockName(blockName)),
            false);

    public bool CanRedoBlock(string blockName) =>
        TryReadManager(
            manager => manager.CanRedoBlock(NormalizeBlockName(blockName)),
            false);

    public IReadOnlyDictionary<string, ChangeSummary> GetFormChangeSummary() =>
        TryReadManager(
            manager => manager.GetFormChangeSummary(),
            new Dictionary<string, ChangeSummary>());

    public void RegisterCrossBlockRule(CrossBlockValidationRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        RequireManager().RegisterCrossBlockRule(rule);
    }

    public bool UnregisterCrossBlockRule(string ruleName) =>
        RequireManager().UnregisterCrossBlockRule(ruleName);

    public IReadOnlyList<string> ValidateCrossBlock() =>
        TryReadManager(manager => manager.ValidateCrossBlock(), []);

    public IReadOnlyList<string> GetDirtyBlocks() =>
        TryReadManager(
            manager => (IReadOnlyList<string>)manager.GetDirtyBlocks(),
            []);

    public async Task<bool> SaveDirtyBlocksAsync(
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return false;
        }

        var result = await RequireManager().SaveDirtyBlocksAsync()
            .ConfigureAwait(false);
        if (result && !ct.IsCancellationRequested)
        {
            RefreshAllRegisteredBlocks();
        }
        return result && !ct.IsCancellationRequested;
    }

    public async Task<bool> RollbackDirtyBlocksAsync(
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return false;
        }

        var result = await RequireManager().RollbackDirtyBlocksAsync()
            .ConfigureAwait(false);
        if (result && !ct.IsCancellationRequested)
        {
            RefreshAllRegisteredBlocks();
        }
        return result && !ct.IsCancellationRequested;
    }

    private bool ExecuteStateChange(
        string blockName,
        Func<IUnitofWorksManager, bool> change)
    {
        var result = change(RequireManager());
        if (result)
        {
            RefreshBlockAndDetails(NormalizeBlockName(blockName));
        }
        return result;
    }
}
