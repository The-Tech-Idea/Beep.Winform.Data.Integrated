using System.Threading;
using System.Threading.Tasks;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    public partial class BeepForms
    {
        private IBeepFormsCommandRouter _commandRouter = new Services.BeepFormsCommandRouter();

        public IBeepFormsCommandRouter CommandRouter
        {
            get => _commandRouter;
            set
            {
                _commandRouter = value ?? new Services.BeepFormsCommandRouter();
                _commandRouter.FormsManager = _formsManager;
            }
        }

        public async Task<bool> EnterQueryAsync(string? blockName = null)
        {
            string targetBlockName = ResolveTargetBlockName(blockName);
            var messageSnapshot = CaptureMessageSnapshot();
            bool success = await _commandRouter.EnterQueryAsync(targetBlockName).ConfigureAwait(true);
            SyncFromManager();
            UpdateMasterDetailShellContext(targetBlockName);

            PublishOperationFeedback(
                messageSnapshot,
                targetBlockName,
                success
                    ? $"Block '{targetBlockName}' entered query mode."
                    : $"Unable to enter query mode for '{targetBlockName}'.",
                success ? BeepFormsMessageSeverity.Info : BeepFormsMessageSeverity.Warning);

            return success;
        }

        public async Task<bool> ExecuteQueryAsync(string? blockName = null, System.Collections.Generic.List<AppFilter>? filters = null)
        {
            string targetBlockName = ResolveTargetBlockName(blockName);
            var messageSnapshot = CaptureMessageSnapshot();
            bool success = await _commandRouter.ExecuteQueryAsync(targetBlockName, filters).ConfigureAwait(true);
            SyncFromManager();
            if (success)
            {
                await RefreshMasterDetailShellAsync(targetBlockName, "query execution").ConfigureAwait(true);
            }
            else
            {
                UpdateMasterDetailShellContext(targetBlockName);
            }

            PublishOperationFeedback(
                messageSnapshot,
                targetBlockName,
                success
                    ? $"Query executed for '{targetBlockName}' — {GetQueryResultCount(targetBlockName)} records returned."
                    : $"Query execution stopped for '{targetBlockName}'.",
                success ? BeepFormsMessageSeverity.Success : BeepFormsMessageSeverity.Warning);

            return success;
        }

        private string GetQueryResultCount(string blockName)
        {
            try
            {
                int count = _formsManager?.GetBlockCount(blockName) ?? 0;
                return count.ToString();
            }
            catch { return "?"; }
        }

        public async Task<IErrorsInfo> CommitFormAsync()
        {
            var messageSnapshot = CaptureMessageSnapshot();

            var crossBlockErrors = _formsManager?.ValidateCrossBlock();
            if (crossBlockErrors != null && crossBlockErrors.Count > 0)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Cross-block validation failed:");
                foreach (var err in crossBlockErrors)
                    sb.AppendLine($"  • {err}");

                bool proceed = await ConfirmAsync("Validation Errors", sb.ToString()).ConfigureAwait(true);
                if (!proceed)
                    return new ErrorsInfo { Flag = Errors.Failed, Message = "Commit cancelled due to cross-block validation errors." };
            }

            var result = await _commandRouter.CommitFormAsync().ConfigureAwait(true);
            SyncFromManager();
            UpdateMasterDetailShellContext();
            PublishCommandResult(messageSnapshot, result, "Changes committed.", "Commit failed.");
            return result;
        }

        public async Task<IErrorsInfo> RollbackFormAsync()
        {
            var messageSnapshot = CaptureMessageSnapshot();
            var result = await _commandRouter.RollbackFormAsync().ConfigureAwait(true);
            SyncFromManager();
            UpdateMasterDetailShellContext();
            PublishCommandResult(messageSnapshot, result, "Changes rolled back.", "Rollback failed.");
            PublishWorkflowState(
                BuildRollbackWorkflowText(
                    "Form",
                    null,
                    result,
                    "Form rollback completed. Manager and hosted blocks were synchronized to the rolled-back state."),
                ResolveRollbackWorkflowSeverity(result));
            return result;
        }

        public async Task<bool> SwitchToBlockAsync(string blockName)
        {
            var messageSnapshot = CaptureMessageSnapshot();
            bool success = await _commandRouter.SwitchToBlockAsync(blockName).ConfigureAwait(true);
            SyncFromManager();
            UpdateMasterDetailShellContext(ResolveTargetBlockName(blockName));

            if (!success)
            {
                PublishOperationFeedback(
                    messageSnapshot,
                    ResolveTargetBlockName(blockName),
                    $"Unable to switch to block '{ResolveTargetBlockName(blockName)}'.",
                    BeepFormsMessageSeverity.Warning);
            }

            return success;
        }

        public async Task<bool> InsertRecordAsync(string? blockName = null)
        {
            string targetBlockName = ResolveTargetBlockName(blockName);
            var messageSnapshot = CaptureMessageSnapshot();
            bool success = await _commandRouter.InsertRecordAsync(targetBlockName).ConfigureAwait(true);
            SyncFromManager();
            UpdateMasterDetailShellContext(targetBlockName);

            PublishOperationFeedback(
                messageSnapshot,
                targetBlockName,
                success
                    ? $"New record created in '{targetBlockName}'."
                    : $"Unable to create new record in '{targetBlockName}'.",
                success ? BeepFormsMessageSeverity.Success : BeepFormsMessageSeverity.Warning);

            return success;
        }

        public async Task<bool> DeleteCurrentRecordAsync(string? blockName = null)
        {
            string targetBlockName = ResolveTargetBlockName(blockName);
            bool confirmed = await ConfirmAsync("Delete Record",
                $"Are you sure you want to delete the current record from '{targetBlockName}'?").ConfigureAwait(true);
            if (!confirmed) return false;

            var messageSnapshot = CaptureMessageSnapshot();
            bool success = await _commandRouter.DeleteCurrentRecordAsync(targetBlockName).ConfigureAwait(true);
            SyncFromManager();
            UpdateMasterDetailShellContext(targetBlockName);

            PublishOperationFeedback(
                messageSnapshot,
                targetBlockName,
                success
                    ? $"Record deleted from '{targetBlockName}'."
                    : $"Unable to delete record from '{targetBlockName}'.",
                success ? BeepFormsMessageSeverity.Success : BeepFormsMessageSeverity.Warning);

            return success;
        }

        public async Task<bool> DuplicateCurrentRecordAsync(string? blockName = null)
        {
            string targetBlockName = ResolveTargetBlockName(blockName);
            var messageSnapshot = CaptureMessageSnapshot();
            bool success = await _commandRouter.DuplicateCurrentRecordAsync(targetBlockName).ConfigureAwait(true);
            SyncFromManager();
            UpdateMasterDetailShellContext(targetBlockName);

            PublishOperationFeedback(
                messageSnapshot,
                targetBlockName,
                success
                    ? $"Record duplicated in '{targetBlockName}'."
                    : $"Unable to duplicate record in '{targetBlockName}'.",
                success ? BeepFormsMessageSeverity.Success : BeepFormsMessageSeverity.Warning);

            return success;
        }

        public async Task<bool> ClearBlockAsync(string? blockName = null)
        {
            string targetBlockName = ResolveTargetBlockName(blockName);
            bool confirmed = await ConfirmAsync("Clear Block",
                $"Clear all records from '{targetBlockName}'? This cannot be undone.").ConfigureAwait(true);
            if (!confirmed) return false;

            var messageSnapshot = CaptureMessageSnapshot();
            bool success = await _commandRouter.ClearBlockAsync(targetBlockName).ConfigureAwait(true);
            SyncFromManager();
            UpdateMasterDetailShellContext(targetBlockName);

            PublishOperationFeedback(
                messageSnapshot,
                targetBlockName,
                success
                    ? $"Block '{targetBlockName}' cleared."
                    : $"Unable to clear block '{targetBlockName}'.",
                success ? BeepFormsMessageSeverity.Success : BeepFormsMessageSeverity.Warning);

            return success;
        }

        public async Task<bool> ClearRecordAsync(string? blockName = null)
        {
            string targetBlockName = ResolveTargetBlockName(blockName);
            var messageSnapshot = CaptureMessageSnapshot();
            bool success = await _commandRouter.ClearRecordAsync(targetBlockName).ConfigureAwait(true);
            SyncFromManager();
            UpdateMasterDetailShellContext(targetBlockName);

            PublishOperationFeedback(
                messageSnapshot,
                targetBlockName,
                success
                    ? $"Record cleared in '{targetBlockName}'."
                    : $"Unable to clear record in '{targetBlockName}'.",
                success ? BeepFormsMessageSeverity.Success : BeepFormsMessageSeverity.Warning);

            return success;
        }

        public async Task<bool> ExitQueryAsync(string? blockName = null)
        {
            string targetBlockName = ResolveTargetBlockName(blockName);
            Builtins?.ExitQuery();
            SyncFromManager();
            UpdateMasterDetailShellContext(targetBlockName);
            ShowInfo($"Exited query mode for '{targetBlockName}'.");
            return true;
        }

        private string ResolveTargetBlockName(string? blockName)
        {
            if (!string.IsNullOrWhiteSpace(blockName))
            {
                return blockName;
            }

            return _viewState.ActiveBlockName ?? string.Empty;
        }

        private void PublishCommandResult((string Message, BeepFormsMessageSeverity Severity) snapshot, IErrorsInfo? result, string successFallback, string failureFallback)
        {
            if (HasMessageChanged(snapshot) || _managerAdapter.TryGetCurrentMessage(_viewState.ActiveBlockName, out _, out _))
            {
                ApplyShellStateToUi();
                return;
            }

            bool isSuccess = result?.Flag == Errors.Ok;
            string message = string.IsNullOrWhiteSpace(result?.Message)
                ? (isSuccess ? successFallback : failureFallback)
                : result.Message;

            ShowMessage(message, ResolveCommandResultSeverity(result));
        }
    }
}