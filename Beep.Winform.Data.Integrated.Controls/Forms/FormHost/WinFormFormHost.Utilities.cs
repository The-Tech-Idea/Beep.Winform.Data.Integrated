using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.UOW.Models;
using TheTechIdea.Beep.Editor.UOWManager.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public partial class WinFormFormHost
{
    public FormStateSnapshot SaveFormState() =>
        RequireManager().SaveFormState();

    public async Task<bool> RestoreFormStateAsync(
        FormStateSnapshot snapshot,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var result = await RequireManager().RestoreFormStateAsync(snapshot, ct)
            .ConfigureAwait(false);
        if (result)
        {
            RefreshAllRegisteredBlocks();
        }
        return result;
    }

    public IReadOnlyDictionary<string, object> GetComputedValues(string blockName) =>
        RequireManager().GetAllBlockComputedValues(NormalizeBlockName(blockName));

    public void FreezeBlock(string blockName) =>
        RequireManager().FreezeBlock(NormalizeBlockName(blockName));

    public void UnfreezeBlock(string blockName)
    {
        var name = NormalizeBlockName(blockName);
        RequireManager().UnfreezeBlock(name);
        RefreshBlockAndDetails(name);
    }

    public void BeginBlockBatchUpdate(string blockName) =>
        RequireManager().BeginBlockBatchUpdate(NormalizeBlockName(blockName));

    public bool RevertCurrentRecord(string blockName)
    {
        var name = NormalizeBlockName(blockName);
        var result = RequireManager().RevertCurrentRecord(name);
        if (result)
        {
            RefreshBlockAndDetails(name);
        }
        return result;
    }

    public async Task<bool> RefreshBlockAsync(
        string blockName,
        ConflictMode mode = ConflictMode.ServerWins,
        CancellationToken ct = default)
    {
        var name = NormalizeBlockName(blockName);
        var result = await RequireManager().RefreshBlockAsync(
            name, null, mode, ct).ConfigureAwait(false);
        if (result)
        {
            RefreshBlockAndDetails(name);
        }
        return result;
    }

    public ChangeSummary GetBlockChangeSummary(string blockName) =>
        RequireManager().GetBlockChangeSummary(NormalizeBlockName(blockName));

    public IReadOnlyList<object> GetDetailedChangeLog(string blockName) =>
        RequireManager().GetBlockDetailedChangeLog(NormalizeBlockName(blockName));

    public decimal GetBlockSum(string blockName, string fieldName) =>
        RequireManager().GetBlockSum(NormalizeBlockName(blockName), fieldName);

    public decimal GetBlockAverage(string blockName, string fieldName) =>
        RequireManager().GetBlockAverage(NormalizeBlockName(blockName), fieldName);

    public Task<double> GetBlockAggregateScalarAsync(
        string blockName,
        string aggregateExpression,
        CancellationToken ct = default) =>
        RequireManager().GetBlockAggregateScalarAsync(
            NormalizeBlockName(blockName), aggregateExpression, ct);

    public Task ExportBlockToJsonAsync(
        string blockName,
        Stream stream,
        CancellationToken ct = default) =>
        RequireManager().ExportBlockToJsonAsync(
            NormalizeBlockName(blockName), stream, ct);

    public Task ExportBlockToCsvAsync(
        string blockName,
        Stream stream,
        char delimiter = ',',
        CancellationToken ct = default) =>
        RequireManager().ExportBlockToCsvAsync(
            NormalizeBlockName(blockName), stream, delimiter, ct);

    public async Task<int> ImportBlockFromJsonAsync(
        string blockName,
        Stream stream,
        bool clearFirst = true,
        CancellationToken ct = default)
    {
        var name = NormalizeBlockName(blockName);
        var count = await RequireManager().ImportBlockFromJsonAsync(
            name, stream, clearFirst, ct).ConfigureAwait(false);
        RefreshBlockAndDetails(name);
        return count;
    }

    public async Task<int> ImportBlockFromCsvAsync(
        string blockName,
        Stream stream,
        char delimiter = ',',
        bool clearFirst = true,
        bool hasHeaderRow = true,
        CancellationToken ct = default)
    {
        var name = NormalizeBlockName(blockName);
        var count = await RequireManager().ImportBlockFromCsvAsync(
            name, stream, delimiter, clearFirst, hasHeaderRow, ct)
            .ConfigureAwait(false);
        RefreshBlockAndDetails(name);
        return count;
    }

    public async Task GoToPageAsync(
        string blockName,
        int page,
        CancellationToken ct = default)
    {
        var name = NormalizeBlockName(blockName);
        await RequireManager().GoToBlockPageAsync(name, page, ct)
            .ConfigureAwait(false);
        RefreshBlockAndDetails(name);
    }

    public Task PrefetchAdjacentPagesAsync(
        string blockName,
        CancellationToken ct = default) =>
        RequireManager().PrefetchBlockAdjacentPagesAsync(
            NormalizeBlockName(blockName), ct);

    public Task<string> ReadTextFileAsync(
        string path,
        CancellationToken ct = default) =>
        RequireManager().ReadTextFileAsync(path, ct);

    public Task WriteTextFileAsync(
        string path,
        string content,
        CancellationToken ct = default) =>
        RequireManager().WriteTextFileAsync(path, content, ct);

    public Task AppendTextFileAsync(
        string path,
        string content,
        CancellationToken ct = default) =>
        RequireManager().AppendTextFileAsync(path, content, ct);

    public void SetClientInfo(string clientInfo) =>
        RequireManager().SetClientInfo(clientInfo);

    public string GetClientInfo() =>
        RequireManager().GetClientInfo();

    public void SetApplicationProperty(string name, object? value) =>
        RequireManager().SetApplicationProperty(name, value!);

    public object? GetApplicationProperty(string name) =>
        RequireManager().GetApplicationProperty(name);

    public bool BeginFormTransaction() =>
        RequireManager().BeginFormTransaction();

    public bool CommitFormTransaction() =>
        RequireManager().CommitFormTransaction();

    public void EndFormTransaction() =>
        RequireManager().EndFormTransaction();

    public Task<bool> PostBlockAsync(
        string blockName,
        CancellationToken ct = default) =>
        ExecuteAndRefreshAsync(
            blockName,
            (manager, name) => manager.PostBlockAsync(name, ct),
            ct);

    public BlockStatus GetBlockStatus(string blockName) =>
        RequireManager().GetBlockStatus(NormalizeBlockName(blockName));

    private void RefreshAllRegisteredBlocks() =>
        RunOnUi(() =>
        {
            foreach (var block in _blocks.Values)
            {
                block.SyncFromManager();
            }
        });
}
