using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOW.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public partial class WinFormFormHost
{
    public async Task<bool> ExecuteQueryByExampleAsync(
        string blockName,
        IReadOnlyDictionary<string, QueryCriterion> criteria,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        var manager = RequireManager();
        var name = NormalizeBlockName(blockName);
        var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        manager.QueryBuilder.ClearQueryOperators(name);

        foreach (var pair in criteria)
        {
            if (!pair.Value.IsEnabled || pair.Value.Value is null)
            {
                continue;
            }

            manager.QueryBuilder.SetQueryOperator(name, pair.Key, pair.Value.Operator);
            values[pair.Key] = pair.Value.Value;
        }

        var filters = manager.QueryBuilder.BuildFilters(name, values);
        ct.ThrowIfCancellationRequested();
        var result = await manager.ExecuteQueryAsync(name, filters).ConfigureAwait(false);
        if (result)
        {
            RefreshBlockAndDetails(name);
        }

        return result;
    }

    public void SaveQueryTemplate(
        string blockName,
        string templateName,
        IReadOnlyDictionary<string, QueryCriterion> criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        var manager = RequireManager();
        var name = NormalizeBlockName(blockName);
        var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        manager.QueryBuilder.ClearQueryOperators(name);

        foreach (var pair in criteria)
        {
            if (!pair.Value.IsEnabled || pair.Value.Value is null)
            {
                continue;
            }

            manager.QueryBuilder.SetQueryOperator(name, pair.Key, pair.Value.Operator);
            values[pair.Key] = pair.Value.Value;
        }

        manager.QueryBuilder.SaveQueryTemplate(
            name,
            templateName,
            manager.QueryBuilder.BuildFilters(name, values));
    }

    public QueryTemplateInfo? LoadQueryTemplate(string blockName, string templateName) =>
        RequireManager().QueryBuilder.LoadQueryTemplate(
            NormalizeBlockName(blockName),
            templateName);

    public IReadOnlyList<QueryTemplateInfo> GetQueryTemplates(string blockName) =>
        RequireManager().QueryBuilder.GetQueryTemplates(NormalizeBlockName(blockName));

    public bool DeleteQueryTemplate(string blockName, string templateName) =>
        RequireManager().QueryBuilder.DeleteQueryTemplate(
            NormalizeBlockName(blockName),
            templateName);

    public IReadOnlyList<QueryHistoryEntry> GetQueryHistory(string blockName) =>
        RequireManager().GetBlockQueryHistory(NormalizeBlockName(blockName));

    public void ClearQueryHistory(string blockName) =>
        RequireManager().ClearBlockQueryHistory(NormalizeBlockName(blockName));
}
