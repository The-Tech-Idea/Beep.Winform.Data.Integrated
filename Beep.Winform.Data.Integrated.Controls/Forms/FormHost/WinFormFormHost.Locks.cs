using TheTechIdea.Beep.Editor.Forms.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public partial class WinFormFormHost
{
    public Task<bool> LockCurrentRecordAsync(
        string blockName,
        CancellationToken ct = default) =>
        RequireManager().Locking.LockCurrentRecordAsync(
            NormalizeBlockName(blockName),
            ct);

    public bool UnlockCurrentRecord(string blockName) =>
        RequireManager().Locking.UnlockCurrentRecord(
            NormalizeBlockName(blockName));

    public void UnlockAllRecords(string blockName) =>
        RequireManager().Locking.UnlockAllRecords(
            NormalizeBlockName(blockName));

    public bool IsCurrentRecordLocked(string blockName) =>
        RequireManager().Locking.IsCurrentRecordLocked(
            NormalizeBlockName(blockName));

    public RecordLockInfo? GetCurrentRecordLockInfo(string blockName)
    {
        var name = NormalizeBlockName(blockName);
        var index = GetCurrentBlockRecordIndex(name);
        return index < 0
            ? null
            : RequireManager().Locking.GetLockInfo(name, index);
    }

    public IReadOnlyList<RecordLockInfo> GetAllLocks(string blockName) =>
        RequireManager().Locking.GetAllLocks(
            NormalizeBlockName(blockName));

    public LockMode GetLockMode(string blockName) =>
        RequireManager().Locking.GetLockMode(
            NormalizeBlockName(blockName));

    public void SetLockMode(string blockName, LockMode mode) =>
        RequireManager().Locking.SetLockMode(
            NormalizeBlockName(blockName),
            mode);

    public bool GetLockOnEdit(string blockName) =>
        RequireManager().Locking.GetLockOnEdit(
            NormalizeBlockName(blockName));

    public void SetLockOnEdit(string blockName, bool value) =>
        RequireManager().Locking.SetLockOnEdit(
            NormalizeBlockName(blockName),
            value);
}
