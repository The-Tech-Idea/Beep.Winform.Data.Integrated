using TheTechIdea.Beep.Editor.Forms.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public partial class WinFormFormHost
{
    // Goes through the manager's data-aware wrappers, NOT through
    // RequireManager().Savepoints.
    //
    // The savepoint manager is a store: its two-argument CreateSavepoint
    // captures no field values, and its RollbackToSavepointAsync only prunes
    // later savepoints - its own docs say restoring the data is the caller's
    // job. This host called the store directly until 2026-08-02, so it created
    // savepoints that snapshotted nothing and rolled back to them without
    // restoring anything, reporting success both times. A savepoint that does
    // not restore is not a savepoint.
    public string CreateSavepoint(
        string blockName,
        string? savepointName = null) =>
        RequireManager().CreateBlockSavepoint(
            NormalizeBlockName(blockName),
            savepointName);

    public async Task<bool> RollbackToSavepointAsync(
        string blockName,
        string savepointName,
        CancellationToken ct = default)
    {
        var name = NormalizeBlockName(blockName);
        var result = await RequireManager()
            .RollbackToSavepointAsync(name, savepointName, ct)
            .ConfigureAwait(false);
        if (result)
        {
            RefreshBlockAndDetails(name);
        }

        return result;
    }

    public bool ReleaseSavepoint(string blockName, string savepointName) =>
        RequireManager().Savepoints.ReleaseSavepoint(
            NormalizeBlockName(blockName),
            savepointName);

    public void ReleaseAllSavepoints(string blockName) =>
        RequireManager().Savepoints.ReleaseAllSavepoints(
            NormalizeBlockName(blockName));

    public IReadOnlyList<SavepointInfo> GetSavepoints(string blockName) =>
        RequireManager().Savepoints.ListSavepoints(
            NormalizeBlockName(blockName));
}
