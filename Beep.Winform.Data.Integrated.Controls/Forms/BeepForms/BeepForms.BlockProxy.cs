using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Editor.UOWManager.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    /// <summary>
    /// IBeepFormsHost proxy implementations.
    /// These are the ONLY entry points BeepBlock is allowed to use for FormsManager access.
    /// BeepBlock → BeepForms (here) → _formsManager → IDataSource.
    /// </summary>
    public partial class BeepForms
    {
        // ── Block / UoW query proxies ─────────────────────────────────────────────────

        public bool IsBlockRegistered(string blockName)
        {
            if (string.IsNullOrWhiteSpace(blockName)) return false;
            return _formsManager?.BlockExists(blockName) ?? false;
        }

        public DataBlockInfo? GetBlockInfo(string blockName)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null) return null;
            if (!_formsManager.BlockExists(blockName)) return null;
            return _formsManager.GetBlock(blockName);
        }

        public IUnitofWork? GetBlockUnitOfWork(string blockName)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null) return null;
            return _formsManager.GetUnitOfWork(blockName);
        }

        public object? GetCurrentBlockItem(string blockName)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null) return null;
            return _formsManager.GetUnitOfWork(blockName)?.CurrentItem;
        }

        public IEnumerable<string> GetDetailBlockNames(string blockName)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null)
                return Enumerable.Empty<string>();
            return _formsManager.GetDetailBlocks(blockName) ?? Enumerable.Empty<string>();
        }

        public void SetBlockCurrentRecordIndex(string blockName, int index)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null) return;
            _formsManager.Locking.SetCurrentRecordIndex(blockName, index);
        }

        public bool IsBlockQueryAllowed(string blockName)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null) return false;
            return _formsManager.GetBlock(blockName)?.QueryAllowed ?? false;
        }

        public TriggerStatisticsInfo? GetTriggerStatistics(string blockName)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null)
                return null;

            return _formsManager.Triggers.GetTriggerStatistics(blockName);
        }

        public IReadOnlyList<TriggerDefinition> GetFormLevelTriggers(string blockName)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null)
                return Array.Empty<TriggerDefinition>();

            return _formsManager.Triggers.GetFormLevelTriggers(blockName) ?? Array.Empty<TriggerDefinition>();
        }

        public IReadOnlyList<TriggerDefinition> GetBlockLevelTriggers(string blockName)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null)
                return Array.Empty<TriggerDefinition>();

            return _formsManager.Triggers.GetBlockLevelTriggers(blockName) ?? Array.Empty<TriggerDefinition>();
        }

        public IReadOnlyList<TriggerDefinition> GetRecordLevelTriggers(string blockName)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null)
                return Array.Empty<TriggerDefinition>();

            return _formsManager.Triggers.GetRecordLevelTriggers(blockName) ?? Array.Empty<TriggerDefinition>();
        }

        public IReadOnlyList<TriggerDefinition> GetItemLevelTriggers(string blockName)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null)
                return Array.Empty<TriggerDefinition>();

            return _formsManager.Triggers.GetItemLevelTriggers(blockName) ?? Array.Empty<TriggerDefinition>();
        }

        public IEnumerable<string> GetAvailableConnectionNames()
        {
            var connections = _formsManager?.DMEEditor?.ConfigEditor?.DataConnections;
            if (connections == null) return Enumerable.Empty<string>();
            return connections
                .Where(c => !string.IsNullOrWhiteSpace(c?.ConnectionName))
                .Select(c => c!.ConnectionName!);
        }

        // ── LOV proxies ───────────────────────────────────────────────────────────────

        public bool HasLov(string blockName, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(fieldName) || _formsManager == null)
                return false;
            return _formsManager.LOV.HasLOV(blockName, fieldName);
        }

        public LOVDefinition? GetLov(string blockName, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(fieldName) || _formsManager == null)
                return null;
            return _formsManager.LOV.GetLOV(blockName, fieldName);
        }

        public Task<LOVResult> LoadLovDataAsync(string blockName, string fieldName, string? searchText = null)
        {
            if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(fieldName) || _formsManager == null)
                return Task.FromResult(new LOVResult { Success = false });
            return _formsManager.LOV.LoadLOVDataAsync(blockName, fieldName, searchText);
        }

        public Task<LOVValidationResult> ValidateLovValueAsync(string blockName, string fieldName, object? value)
        {
            if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(fieldName) || _formsManager == null)
                return Task.FromResult(new LOVValidationResult { IsValid = false });
            return _formsManager.LOV.ValidateLOVValueAsync(blockName, fieldName, value);
        }

        public Dictionary<string, object>? GetLovRelatedFieldValues(LOVDefinition lov, object? selectedItem)
        {
            if (lov == null || selectedItem == null || _formsManager == null) return null;
            return _formsManager.LOV.GetRelatedFieldValues(lov, selectedItem);
        }

        public Task<LOVResult> ShowLovAsync(string blockName, string fieldName, string? searchText = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(fieldName) || _formsManager == null)
                return Task.FromResult(new LOVResult { Success = false });
            return _formsManager.ShowLOVAsync(blockName, fieldName, searchText, ct: ct);
        }

        // ── Validation proxy ──────────────────────────────────────────────────────────

        public RecordValidationResult? ValidateBlockRecord(string blockName, IDictionary<string, object> record, ValidationTiming timing)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null) return null;
            return _formsManager.Validation.ValidateRecord(blockName, record, timing);
        }

        // ── Item-properties proxy ─────────────────────────────────────────────────────

        public bool IsFieldQueryAllowed(string blockName, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(fieldName) || _formsManager == null)
                return true; // default allow
            if (!_formsManager.BlockExists(blockName)) return true;
            try
            {
                return _formsManager.ItemProperties.IsItemQueryAllowed(blockName, fieldName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BeepForms.BlockProxy.IsFieldQueryAllowed] {blockName}.{fieldName}: {ex.GetType().Name} - {ex.Message}");
                return _formsManager.GetBlock(blockName)
                    ?.FieldMetadata
                    ?.FirstOrDefault(f => string.Equals(f.FieldName, fieldName, StringComparison.OrdinalIgnoreCase))
                    ?.IsQueryable ?? true;
            }
        }

        // ── Mutation proxies ──────────────────────────────────────────────────────────

        public async Task<bool> SaveBlockAsync(string blockName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null) return false;
            var uow = _formsManager.GetUnitOfWork(blockName);
            if (uow == null) return false;
            var result = await uow.Commit().ConfigureAwait(false);
            return result?.Flag == TheTechIdea.Beep.ConfigUtil.Errors.Ok;
        }

        public async Task<bool> RollbackBlockAsync(string blockName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null) return false;
            var uow = _formsManager.GetUnitOfWork(blockName);
            if (uow == null) return false;
            var result = await uow.Rollback().ConfigureAwait(false);
            bool success = result?.Flag == TheTechIdea.Beep.ConfigUtil.Errors.Ok;
            PublishWorkflowState(
                BuildRollbackWorkflowText(
                    "Block",
                    blockName,
                    result,
                    $"Block '{blockName}' rollback completed through the host proxy."),
                ResolveRollbackWorkflowSeverity(result));
            return success;
        }

        public async Task<bool> InsertBlockRecordAsync(string blockName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null) return false;
            return await _formsManager.InsertRecordAsync(blockName).ConfigureAwait(false);
        }

        public async Task<bool> DeleteBlockCurrentRecordAsync(string blockName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null) return false;
            return await _formsManager.DeleteCurrentRecordAsync(blockName).ConfigureAwait(false);
        }

        // ── Built-in supporting proxies (Phase 1) ──────────────────────────────────────

        public bool IsItemRegistered(string blockName, string itemName)
        {
            if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(itemName)) return false;
            return _formsManager?.ItemProperties?.ItemExists(blockName, itemName) ?? false;
        }

        public IReadOnlyList<string> GetRegisteredBlockNames()
        {
            return _blocks
                .Select(b => b.BlockName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();
        }

        public IReadOnlyList<string> GetRegisteredItemNames(string blockName)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null)
                return Array.Empty<string>();
            try
            {
                var items = _formsManager.ItemProperties?.GetAllItems(blockName);
                if (items == null) return Array.Empty<string>();
                return items.Select(i => i.ItemName).Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BeepForms.BlockProxy.GetRegisteredItemNames] {blockName}: {ex.GetType().Name} - {ex.Message}");
                return Array.Empty<string>();
            }
        }

        public int GetBlockRecordCount(string blockName)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null) return 0;
            try
            {
                var uow = _formsManager.GetUnitOfWork(blockName);
                if (uow == null) return 0;
                if (uow.TotalItemCount > 0) return uow.TotalItemCount;
                var dyn = uow.Units as System.Collections.ICollection;
                return dyn?.Count ?? 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BeepForms.BlockProxy.GetBlockRecordCount] {blockName}: {ex.GetType().Name} - {ex.Message}");
                return 0;
            }
        }

        public DataBlockMode GetBlockMode(string blockName)
        {
            return GetBlockInfo(blockName)?.Mode ?? DataBlockMode.Query;
        }

        public void SetBlockMode(string blockName, DataBlockMode mode)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null) return;
            var info = _formsManager.GetBlock(blockName);
            if (info == null) return;
            info.Mode = mode;
        }

        public bool TryGetItemProperty(string blockName, string itemName, string property, out object? value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(itemName) || _formsManager == null)
                return false;
            try
            {
                value = _formsManager.ItemProperties.GetItemProperty(blockName, itemName, property);
                return value != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BeepForms.BlockProxy.TryGetItemProperty] {blockName}.{itemName}: {ex.GetType().Name} - {ex.Message}");
                value = null;
                return false;
            }
        }

        public bool TrySetItemProperty(string blockName, string itemName, string property, object? value)
        {
            if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(itemName) || _formsManager == null)
                return false;
            try
            {
                _formsManager.ItemProperties.SetItemProperty(blockName, itemName, property, value);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BeepForms.BlockProxy.TrySetItemProperty] {blockName}.{itemName}: {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        public bool TryGetBlockProperty(string blockName, string property, out object? value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null) return false;
            try
            {
                var block = _formsManager.GetBlock(blockName);
                if (block == null) return false;
                var prop = block.GetType().GetProperty(property);
                if (prop == null) return false;
                value = prop.GetValue(block);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BeepForms.BlockProxy.TryGetBlockProperty] {blockName}.{property}: {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        public bool TrySetBlockProperty(string blockName, string property, object? value)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null) return false;
            try
            {
                var block = _formsManager.GetBlock(blockName);
                if (block == null) return false;
                var prop = block.GetType().GetProperty(property);
                if (prop == null || !prop.CanWrite) return false;
                prop.SetValue(block, value);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BeepForms.BlockProxy.TrySetBlockProperty] {blockName}.{property}: {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ExecuteQueryAsync(string blockName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null) return false;
            try
            {
                var uow = _formsManager.GetUnitOfWork(blockName);
                if (uow == null) return false;
                var data = await uow.Get().ConfigureAwait(false);
                ct.ThrowIfCancellationRequested();
                return data != null;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BeepForms.BlockProxy.ExecuteQueryAsync] {blockName}: {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ClearBlockAsync(string blockName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null) return false;
            try
            {
                var uow = _formsManager.GetUnitOfWork(blockName);
                if (uow == null) return false;
                uow.Clear();
                _formsManager.Locking.SetCurrentRecordIndex(blockName, -1);
                ct.ThrowIfCancellationRequested();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BeepForms.BlockProxy.ClearBlockAsync] {blockName}: {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ClearRecordAsync(string blockName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null) return false;
            ct.ThrowIfCancellationRequested();
            try
            {
                var uow = _formsManager.GetUnitOfWork(blockName);
                if (uow == null) return false;
                uow.New();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[BeepForms.BlockProxy.ClearRecordAsync] {blockName}: {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        public async Task<bool> PostBlockAsync(string blockName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(blockName) || _formsManager == null) return false;
            return await _formsManager.PostBlockAsync(blockName, ct).ConfigureAwait(false);
        }
    }
}
