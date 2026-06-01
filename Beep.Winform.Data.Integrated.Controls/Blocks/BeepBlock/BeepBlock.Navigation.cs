using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks
{
    public partial class BeepBlock
    {
        internal bool IsNavigatorCommandVisible(string commandName)
        {
            if (!GetNavigatorFlag("navigation.enabled", true))
            {
                return false;
            }

            return GetNavigatorFlag($"navigation.{commandName}.visible", true);
        }

        internal bool IsNavigatorCommandEnabled(string commandName)
        {
            if (!IsNavigatorCommandVisible(commandName) || !GetNavigatorFlag($"navigation.{commandName}.enabled", true))
            {
                return false;
            }

            bool hasRecords = ViewState.RecordCount > 0 && ViewState.CurrentRecordIndex >= 0;
            bool hasLocalCommands = _boundUnitOfWork != null;
            bool hasFormsHostCommands = _formsHost is BeepForms;

            return commandName.ToLowerInvariant() switch
            {
                "first" => !ViewState.IsQueryMode && hasRecords && ViewState.CurrentRecordIndex > 0,
                "previous" => !ViewState.IsQueryMode && hasRecords && ViewState.CurrentRecordIndex > 0,
                "next" => !ViewState.IsQueryMode && hasRecords && ViewState.CurrentRecordIndex < ViewState.RecordCount - 1,
                "last" => !ViewState.IsQueryMode && hasRecords && ViewState.CurrentRecordIndex < ViewState.RecordCount - 1,
                "new" => !ViewState.IsQueryMode && hasLocalCommands,
                "delete" => !ViewState.IsQueryMode && hasLocalCommands && hasRecords,
                "query" => hasFormsHostCommands && !ViewState.IsQueryMode && IsManagerQueryAllowed(),
                "execute" => hasFormsHostCommands && ViewState.IsQueryMode && IsManagerQueryAllowed(),
                "save" => !ViewState.IsQueryMode && (hasFormsHostCommands || hasLocalCommands) && ViewState.IsDirty,
                "rollback" => (hasFormsHostCommands || hasLocalCommands) && ViewState.IsDirty,
                _ => false
            };
        }

        internal string GetNavigatorPositionText()
        {
            int currentRecord = ViewState.CurrentRecordIndex >= 0 ? ViewState.CurrentRecordIndex + 1 : 0;
            return $"{currentRecord} / {ViewState.RecordCount}";
        }

        public async Task<bool> MoveFirstAsync()
        {
            return await ExecuteNavigationAsync(forms => forms.MoveFirstAsync(ManagerBlockName), () => TryMoveBindingSourceTo(0)).ConfigureAwait(true);
        }

        public async Task<bool> MovePreviousAsync()
        {
            return await ExecuteNavigationAsync(forms => forms.MovePreviousAsync(ManagerBlockName), () => TryMoveBindingSourceBy(-1)).ConfigureAwait(true);
        }

        public async Task<bool> MoveNextAsync()
        {
            return await ExecuteNavigationAsync(forms => forms.MoveNextAsync(ManagerBlockName), () => TryMoveBindingSourceBy(1)).ConfigureAwait(true);
        }

        public async Task<bool> MoveLastAsync()
        {
            return await ExecuteNavigationAsync(forms => forms.MoveLastAsync(ManagerBlockName), () => TryMoveBindingSourceTo((_recordBindingSource?.Count ?? 1) - 1)).ConfigureAwait(true);
        }

        public async Task<bool> EnterQueryAsync()
        {
            if (_formsHost is BeepForms forms)
            {
                _formsHost.TrySetActiveBlock(BlockName);
                return await forms.EnterQueryAsync(ManagerBlockName).ConfigureAwait(true);
            }

            EnterQueryMode();
            return true;
        }

        public async Task<bool> ExecuteQueryAsync(List<AppFilter>? filters = null)
        {
            List<AppFilter> effectiveFilters;
            if (filters != null)
            {
                effectiveFilters = filters;
            }
            else if (!TryBuildQueryFiltersFromEditors(out effectiveFilters, out string validationMessage))
            {
                if (_formsHost is BeepForms warningHost && !string.IsNullOrWhiteSpace(validationMessage))
                {
                    warningHost.ShowWarning(validationMessage);
                }

                return false;
            }

            if (_formsHost is BeepForms forms)
            {
                _formsHost.TrySetActiveBlock(BlockName);
                return await forms.ExecuteQueryAsync(ManagerBlockName, effectiveFilters).ConfigureAwait(true);
            }

            return false;
        }

        public async Task<bool> CommitAsync()
        {
            if (_formsHost is BeepForms forms)
            {
                _formsHost.TrySetActiveBlock(BlockName);
                var result = await forms.CommitFormAsync().ConfigureAwait(true);
                return !IsFailure(result);
            }

            // Route through BeepForms host — never access FormsManager from BeepBlock.
            if (_formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName))
            {
                return false;
            }

            bool committed = await _formsHost.SaveBlockAsync(ManagerBlockName).ConfigureAwait(true);
            SyncFromManager();
            return committed;
        }

        public async Task<bool> RollbackAsync()
        {
            if (_formsHost is BeepForms forms)
            {
                _formsHost.TrySetActiveBlock(BlockName);
                var result = await forms.RollbackFormAsync().ConfigureAwait(true);
                return !IsFailure(result);
            }

            // Route through BeepForms host — never access FormsManager from BeepBlock.
            if (_formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName))
            {
                return false;
            }

            bool rolledBack = await _formsHost.RollbackBlockAsync(ManagerBlockName).ConfigureAwait(true);
            SyncFromManager();
            return rolledBack;
        }

        public async Task<bool> NewRecordAsync()
        {
            if (ViewState.IsQueryMode)
            {
                return false;
            }

            if (_formsHost is BeepForms forms)
            {
                _formsHost.TrySetActiveBlock(BlockName);
            }

            // Route through BeepForms host — never access FormsManager from BeepBlock.
            if (_formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName))
            {
                return false;
            }

            bool ok = await _formsHost.InsertBlockRecordAsync(ManagerBlockName).ConfigureAwait(true);
            SyncFromManager();
            return ok;
        }

        public async Task<bool> DeleteCurrentRecordAsync()
        {
            if (ViewState.IsQueryMode || ViewState.RecordCount == 0)
            {
                return false;
            }

            if (_formsHost is BeepForms forms)
            {
                _formsHost.TrySetActiveBlock(BlockName);
            }

            // Route through BeepForms host — never access FormsManager from BeepBlock.
            if (_formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName))
            {
                return false;
            }

            bool ok = await _formsHost.DeleteBlockCurrentRecordAsync(ManagerBlockName).ConfigureAwait(true);
            SyncFromManager();
            return ok;
        }

        private bool GetNavigatorFlag(string key, bool defaultValue)
        {
            var navigation = EffectiveDefinition?.Navigation;
            if (navigation != null && navigation.TryResolveFlag(key, out bool typedValue))
            {
                return typedValue;
            }

            var metadata = EffectiveDefinition?.Metadata;
            if (metadata != null && metadata.TryGetValue(key, out var rawValue) && bool.TryParse(rawValue, out bool enabled))
            {
                return enabled;
            }

            return defaultValue;
        }

        private async Task<bool> ExecuteNavigationAsync(Func<BeepForms, Task<bool>> formsAction, Func<bool> fallbackAction)
        {
            if (_formsHost is BeepForms forms)
            {
                _formsHost.TrySetActiveBlock(BlockName);
                return await formsAction(forms).ConfigureAwait(true);
            }

            return fallbackAction();
        }

        private bool TryMoveBindingSourceTo(int targetPosition)
        {
            if (_recordBindingSource == null || _recordBindingSource.Count == 0)
            {
                return false;
            }

            if (targetPosition < 0 || targetPosition >= _recordBindingSource.Count)
            {
                return false;
            }

            _recordBindingSource.Position = targetPosition;
            UpdateRecordViewState(_boundUnitOfWork);
            RefreshValidationState();
            return true;
        }

        private bool TryMoveBindingSourceBy(int offset)
        {
            if (_recordBindingSource == null || _recordBindingSource.Count == 0)
            {
                return false;
            }

            return TryMoveBindingSourceTo(_recordBindingSource.Position + offset);
        }

        private static bool IsFailure(IErrorsInfo? result)
        {
            string flagText = result?.Flag.ToString() ?? string.Empty;
            return string.IsNullOrWhiteSpace(flagText) ||
                   flagText.IndexOf("fail", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   flagText.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}