using System.Collections;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Editor.UOWManager.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public partial class WinFormFormHost
{
    public DataBlockInfo? GetBlockInfo(string blockName) =>
        TryReadManager(
            manager => manager.GetBlock(NormalizeBlockName(blockName)),
            default(DataBlockInfo));

    public IReadOnlyList<EntityField>? GetBlockFields(string blockName) =>
        GetBlockInfo(blockName)?.EntityStructure?.Fields;

    public IEnumerable? GetBlockData(string blockName) =>
        TryReadManager(
            manager => manager.GetUnitOfWork(NormalizeBlockName(blockName))?.Units as IEnumerable,
            default(IEnumerable));

    public IEnumerable<string> GetDetailBlockNames(string blockName) =>
        TryReadManager(
            manager => (IEnumerable<string>)(manager.GetDetailBlocks(
                NormalizeBlockName(blockName)) ?? []),
            []);

    public int GetBlockRecordCount(string blockName) =>
        TryReadManager(
            manager => manager.GetBlockCount(
                NormalizeBlockName(blockName)),
            0);

    public DataBlockMode GetBlockMode(string blockName) =>
        GetBlockInfo(blockName)?.Mode ?? DataBlockMode.Query;

    public bool IsBlockQueryAllowed(string blockName) =>
        GetBlockInfo(blockName)?.QueryAllowed == true;

    public bool IsFieldQueryAllowed(string blockName, string fieldName) =>
        TryReadManager(
            manager => manager.ItemProperties.IsItemQueryAllowed(
                NormalizeBlockName(blockName),
                fieldName),
            false);

    public bool IsBlockDirty(string blockName) =>
        TryReadManager(
            manager => manager.GetUnitOfWork(
                NormalizeBlockName(blockName))?.IsDirty == true,
            false);

    public Task<bool> MoveFirstAsync(string blockName) =>
        ExecuteAndRefreshAsync(
            blockName,
            (manager, name) => manager.FirstRecordAsync(name));

    public Task<bool> MovePreviousAsync(string blockName) =>
        ExecuteAndRefreshAsync(
            blockName,
            (manager, name) => manager.PreviousRecordAsync(name));

    public Task<bool> MoveNextAsync(string blockName) =>
        ExecuteAndRefreshAsync(
            blockName,
            (manager, name) => manager.NextRecordAsync(name));

    public Task<bool> MoveLastAsync(string blockName) =>
        ExecuteAndRefreshAsync(
            blockName,
            (manager, name) => manager.LastRecordAsync(name));

    public Task<bool> MoveToRecordAsync(string blockName, int index)
    {
        if (index < 0)
        {
            return Task.FromResult(false);
        }

        return ExecuteAndRefreshAsync(
            blockName,
            (manager, name) =>
            {
                manager.Locking.SetCurrentRecordIndex(name, index);
                return Task.FromResult(true);
            });
    }

    public Task<bool> InsertBlockRecordAsync(
        string blockName,
        CancellationToken ct = default) =>
        ExecuteAndRefreshAsync(
            blockName,
            (manager, name) => manager.InsertRecordAsync(name),
            ct);

    public Task<bool> DeleteBlockCurrentRecordAsync(
        string blockName,
        CancellationToken ct = default) =>
        ExecuteAndRefreshAsync(
            blockName,
            (manager, name) => manager.DeleteCurrentRecordAsync(name),
            ct);

    public Task<bool> DuplicateCurrentRecordAsync(
        string blockName,
        CancellationToken ct = default) =>
        ExecuteAndRefreshAsync(
            blockName,
            (manager, name) => manager.DuplicateCurrentRecordAsync(name, ct),
            ct);

    public Task<bool> ExecuteQueryAsync(
        string blockName,
        CancellationToken ct = default) =>
        ExecuteAndRefreshAsync(
            blockName,
            (manager, name) => manager.ExecuteQueryAsync(name),
            ct);

    public Task<bool> SaveBlockAsync(
        string blockName,
        CancellationToken ct = default) =>
        ExecuteAndRefreshAsync(
            blockName,
            async (manager, name) =>
            {
                var unitOfWork = manager.GetUnitOfWork(name);
                if (unitOfWork is null)
                {
                    return false;
                }

                var result = await unitOfWork.Commit().ConfigureAwait(false);
                return result?.Flag == Errors.Ok;
            },
            ct);

    public Task<bool> RollbackBlockAsync(
        string blockName,
        CancellationToken ct = default) =>
        ExecuteAndRefreshAsync(
            blockName,
            async (manager, name) =>
            {
                var unitOfWork = manager.GetUnitOfWork(name);
                if (unitOfWork is null)
                {
                    return false;
                }

                var result = await unitOfWork.Rollback().ConfigureAwait(false);
                return result?.Flag == Errors.Ok;
            },
            ct);

    public Task<bool> ClearBlockAsync(
        string blockName,
        CancellationToken ct = default) =>
        ExecuteAndRefreshAsync(
            blockName,
            async (manager, name) =>
            {
                await manager.ClearBlockAsync(name).ConfigureAwait(false);
                return true;
            },
            ct);

    public Task<bool> ClearRecordAsync(
        string blockName,
        CancellationToken ct = default) =>
        ExecuteAndRefreshAsync(
            blockName,
            (manager, name) =>
            {
                var unitOfWork = manager.GetUnitOfWork(name);
                if (unitOfWork is null)
                {
                    return Task.FromResult(false);
                }

                unitOfWork.New();
                return Task.FromResult(true);
            },
            ct);

    public Task<bool> EnterQueryModeAsync(string blockName) =>
        ExecuteAndRefreshAsync(
            blockName,
            (manager, name) => manager.EnterQueryAsync(name));

    public Task<bool> ExitQueryModeAsync(string blockName) =>
        ExecuteAndRefreshAsync(
            blockName,
            (manager, name) =>
            {
                manager.ExitingQueryModeAsync(name);
                return Task.FromResult(true);
            });

    public bool HasLov(string blockName, string fieldName) =>
        TryReadManager(
            manager => manager.LOV.HasLOV(
                NormalizeBlockName(blockName),
                fieldName),
            false);

    public LOVDefinition? GetLov(string blockName, string fieldName) =>
        TryReadManager(
            manager => manager.LOV.GetLOV(
                NormalizeBlockName(blockName),
                fieldName),
            default(LOVDefinition));

    public async Task<LOVResult> LoadLovDataAsync(
        string blockName,
        string fieldName,
        string? searchText = null)
    {
        var manager = _formsManager;
        if (manager is null)
        {
            return LOVResult.Fail("A Forms manager is not assigned.");
        }

        try
        {
            return await manager.LOV.LoadLOVDataAsync(
                NormalizeBlockName(blockName),
                fieldName,
                searchText).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
            return LOVResult.Fail(ex.Message);
        }
    }

    public async Task<LOVResult> ShowLovAsync(
        string blockName,
        string fieldName,
        string? searchText = null,
        CancellationToken ct = default)
    {
        var manager = _formsManager;
        if (manager is null)
        {
            return LOVResult.Fail("A Forms manager is not assigned.");
        }

        try
        {
            return await manager.ShowLOVAsync(
                NormalizeBlockName(blockName),
                fieldName,
                searchText,
                null,
                ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
            return LOVResult.Fail(ex.Message);
        }
    }

    public async Task<LOVValidationResult> ValidateLovValueAsync(
        string blockName,
        string fieldName,
        object value)
    {
        var manager = _formsManager;
        if (manager is null)
        {
            return LOVValidationResult.Invalid(
                "A Forms manager is not assigned.");
        }

        try
        {
            return await manager.LOV.ValidateLOVValueAsync(
                NormalizeBlockName(blockName),
                fieldName,
                value).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
            return LOVValidationResult.Invalid(ex.Message);
        }
    }

    public Dictionary<string, object>? GetLovRelatedFieldValues(
        LOVDefinition lov,
        object? selectedItem)
    {
        if (selectedItem is null)
        {
            return null;
        }

        return TryReadManager(
            manager => manager.LOV.GetRelatedFieldValues(lov, selectedItem),
            default(Dictionary<string, object>));
    }

    public RecordValidationResult? ValidateBlockRecord(
        string blockName,
        IDictionary<string, object> record,
        ValidationTiming timing) =>
        TryReadManager(
            manager => manager.Validation.ValidateRecord(
                NormalizeBlockName(blockName),
                record,
                timing),
            default(RecordValidationResult));

    public void ShowInfo(string message)
    {
        RunOnUi(() => NotificationRaised?.Invoke(
            this, new FormsNotificationEventArgs(message, FormsNotificationKind.Info)));
    }

    public void ShowWarning(string message)
    {
        RunOnUi(() => NotificationRaised?.Invoke(
            this, new FormsNotificationEventArgs(message, FormsNotificationKind.Warning)));
    }

    public void ShowError(string message)
    {
        RunOnUi(() => NotificationRaised?.Invoke(
            this, new FormsNotificationEventArgs(message, FormsNotificationKind.Error)));
    }

    private async Task<bool> ExecuteAndRefreshAsync(
        string blockName,
        Func<IUnitofWorksManager, string, Task<bool>> operation,
        CancellationToken ct = default)
    {
        var manager = _formsManager;
        if (manager is null || ct.IsCancellationRequested)
        {
            return false;
        }

        string normalizedBlockName;
        try
        {
            normalizedBlockName = NormalizeBlockName(blockName);
            var succeeded = await operation(manager, normalizedBlockName)
                .ConfigureAwait(false);
            if (!succeeded || ct.IsCancellationRequested)
            {
                return false;
            }

            RefreshBlockAndDetails(normalizedBlockName);
            return true;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
            return false;
        }
    }

    private void RefreshBlockAndDetails(string blockName)
    {
        var blockNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            blockName
        };

        foreach (var detailName in GetDetailBlockNames(blockName))
        {
            if (!string.IsNullOrWhiteSpace(detailName))
            {
                blockNames.Add(detailName.Trim());
            }
        }

        RunOnUi(() =>
        {
            foreach (var name in blockNames)
            {
                if (_blocks.TryGetValue(name, out var block))
                {
                    block.SyncFromManager();
                }
            }
        });
    }

    private T TryReadManager<T>(
        Func<IUnitofWorksManager, T> read,
        T fallback)
    {
        var manager = _formsManager;
        if (manager is null)
        {
            return fallback;
        }

        try
        {
            return read(manager);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
            return fallback;
        }
    }
}
