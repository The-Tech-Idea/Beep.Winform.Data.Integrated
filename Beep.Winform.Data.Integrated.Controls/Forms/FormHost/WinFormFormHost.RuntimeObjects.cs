using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Report;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public partial class WinFormFormHost
{
    public TimerDefinition CreateTimer(
        string timerName,
        TimeSpan interval,
        bool repeating = false) =>
        RequireManager().Timers.CreateTimer(
            timerName,
            interval,
            repeating);

    public bool DeleteTimer(string timerName) =>
        RequireManager().Timers.DeleteTimer(timerName);

    public TimerDefinition? GetTimer(string timerName) =>
        RequireManager().Timers.GetTimer(timerName);

    public IReadOnlyList<TimerDefinition> GetTimers() =>
        RequireManager().Timers.GetAllTimers();

    public long GetNextSequence(string sequenceName) =>
        RequireManager().Sequences.GetNextSequence(sequenceName);

    public long PeekNextSequence(string sequenceName) =>
        RequireManager().Sequences.PeekNextSequence(sequenceName);

    public void CreateSequence(
        string sequenceName,
        long startValue = 1,
        long incrementBy = 1) =>
        RequireManager().Sequences.CreateSequence(
            sequenceName,
            startValue,
            incrementBy);

    public void ResetSequence(string sequenceName, long startValue = 1) =>
        RequireManager().Sequences.ResetSequence(
            sequenceName,
            startValue);

    public bool DropSequence(string sequenceName) =>
        RequireManager().Sequences.DropSequence(sequenceName);

    public void CreateRecordGroup(
        string name,
        string dataSourceName,
        string entityName,
        List<AppFilter>? filters = null) =>
        RequireManager().CreateRecordGroup(
            name,
            dataSourceName,
            entityName,
            filters);

    public Task<bool> PopulateRecordGroupAsync(
        string name,
        CancellationToken ct = default) =>
        RequireManager().PopulateRecordGroupAsync(name, ct);

    public RecordGroup? GetRecordGroup(string name) =>
        RequireManager().GetRecordGroup(name);

    public IReadOnlyList<RecordGroup> GetRecordGroups() =>
        RequireManager().GetAllRecordGroups();

    public bool RemoveRecordGroup(string name) =>
        RequireManager().RemoveRecordGroup(name);

    public void ClearRecordGroups() =>
        RequireManager().ClearAllRecordGroups();

    public ParameterList CreateParameterList(string name) =>
        RequireManager().CreateParameterList(name);

    public bool DestroyParameterList(string name) =>
        RequireManager().DestroyParameterList(name);

    public void SetParameter(
        string listName,
        string parameterName,
        object? value) =>
        RequireManager().AddParameter(
            listName,
            parameterName,
            value!);

    public object? GetParameter(string listName, string parameterName) =>
        RequireManager().GetParameter(listName, parameterName);

    public bool RemoveParameter(string listName, string parameterName) =>
        RequireManager().RemoveParameter(listName, parameterName);

    public IReadOnlyList<ParameterList> GetParameterLists() =>
        RequireManager().GetAllParameterLists();

    public void ClearParameterList(string listName) =>
        RequireManager().ClearParameterList(listName);
}
