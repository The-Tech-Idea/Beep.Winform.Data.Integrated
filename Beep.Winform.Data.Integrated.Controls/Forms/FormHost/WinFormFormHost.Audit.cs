using TheTechIdea.Beep.Editor.Forms.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public partial class WinFormFormHost
{
    public void SetAuditUser(string userName) =>
        RequireManager().SetAuditUser(userName);

    public IReadOnlyList<AuditEntry> GetAuditLog(
        string? blockName = null,
        AuditOperation? operation = null,
        DateTime? from = null,
        DateTime? to = null) =>
        TryReadManager(
            manager => manager.GetAuditLog(blockName, operation, from, to),
            []);

    public IReadOnlyList<AuditFieldChange> GetFieldHistory(
        string blockName,
        string recordKey,
        string fieldName) =>
        TryReadManager(
            manager => manager.GetFieldHistory(
                NormalizeBlockName(blockName),
                recordKey,
                fieldName),
            []);

    public Task ExportAuditToCsvAsync(
        string filePath,
        string? blockName = null) =>
        RequireManager().ExportAuditToCsvAsync(filePath, blockName);

    public Task ExportAuditToJsonAsync(
        string filePath,
        string? blockName = null) =>
        RequireManager().ExportAuditToJsonAsync(filePath, blockName);

    public void PurgeAudit(int olderThanDays) =>
        RequireManager().PurgeAudit(olderThanDays);

    public void ClearAudit() =>
        RequireManager().ClearAudit();
}
