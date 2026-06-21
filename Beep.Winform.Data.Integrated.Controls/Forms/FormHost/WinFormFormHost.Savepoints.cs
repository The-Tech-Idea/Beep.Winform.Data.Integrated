using TheTechIdea.Beep.Editor.Forms.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public partial class WinFormFormHost
{
    public string CreateSavepoint(
        string blockName,
        string? savepointName = null) =>
        RequireManager().Savepoints.CreateSavepoint(
            NormalizeBlockName(blockName),
            savepointName);

    public async Task<bool> RollbackToSavepointAsync(
        string blockName,
        string savepointName,
        CancellationToken ct = default)
    {
        var name = NormalizeBlockName(blockName);
        var result = await RequireManager().Savepoints
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
