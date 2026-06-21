using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Report;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;

public sealed class WinFormRecordGroupPanel : WinFormFormsFeatureControl
{
    public WinFormRecordGroupPanel(IBeepFormsHost host) : base(host)
    {
    }

    public void Create(
        string name,
        string dataSourceName,
        string entityName,
        List<AppFilter>? filters = null) =>
        Host.CreateRecordGroup(name, dataSourceName, entityName, filters);

    public Task<bool> PopulateAsync(
        string name,
        CancellationToken ct = default) =>
        Host.PopulateRecordGroupAsync(name, ct);

    public IReadOnlyList<RecordGroup> GetGroups() => Host.GetRecordGroups();

    public bool Remove(string name) => Host.RemoveRecordGroup(name);
}
