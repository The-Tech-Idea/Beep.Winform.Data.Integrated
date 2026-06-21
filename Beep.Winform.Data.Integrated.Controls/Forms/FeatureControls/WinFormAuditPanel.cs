using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;

public sealed class WinFormAuditPanel : WinFormFormsFeatureControl
{
    public WinFormAuditPanel(
        IBeepFormsHost host,
        string? blockName = null)
        : base(host, blockName)
    {
    }

    public void SetUser(string userName) =>
        Host.SetAuditUser(userName);

    public IReadOnlyList<AuditEntry> GetEntries(
        AuditOperation? operation = null,
        DateTime? from = null,
        DateTime? to = null) =>
        Host.GetAuditLog(BlockName, operation, from, to) ?? [];

    public IReadOnlyList<AuditFieldChange> GetFieldHistory(
        string recordKey,
        string fieldName) =>
        Host.GetFieldHistory(
            RequireBlockName(),
            recordKey,
            fieldName) ?? [];

    public Task ExportCsvAsync(string filePath) =>
        Host.ExportAuditToCsvAsync(filePath, BlockName);

    public Task ExportJsonAsync(string filePath) =>
        Host.ExportAuditToJsonAsync(filePath, BlockName);

    public void Purge(int olderThanDays) =>
        Host.PurgeAudit(olderThanDays);

    public void Clear() =>
        Host.ClearAudit();
}
