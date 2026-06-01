using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Helpers;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    public partial class BeepForms
    {
        private const int WorkflowHistoryCapacity = 6;

        public string? CreateSavepoint(string? blockName = null, string? savepointName = null)
        {
            if (!TryResolveWorkflowBlockName(blockName, out string targetBlockName, out string unavailableMessage))
            {
                SetSavepointState(unavailableMessage, BeepFormsMessageSeverity.Warning);
                ApplyShellStateToUi();
                return null;
            }

            string? createdSavepoint = TryCreateBlockSavepoint(targetBlockName, savepointName);
            if (string.IsNullOrWhiteSpace(createdSavepoint))
            {
                SetSavepointState($"Savepoint creation failed for '{targetBlockName}'.", BeepFormsMessageSeverity.Warning);
                ApplyShellStateToUi();
                return null;
            }

            UpdateSavepointStateFromManager(targetBlockName, $"Savepoint '{createdSavepoint}' captured", BeepFormsMessageSeverity.Success);
            ApplyShellStateToUi();
            return createdSavepoint;
        }

        public IReadOnlyList<SavepointInfo> ListSavepoints(string? blockName = null)
        {
            if (!TryResolveWorkflowBlockName(blockName, out string targetBlockName, out string unavailableMessage))
            {
                SetSavepointState(unavailableMessage, BeepFormsMessageSeverity.Warning);
                ApplyShellStateToUi();
                return Array.Empty<SavepointInfo>();
            }

            IReadOnlyList<SavepointInfo> savepoints = _formsManager?.Savepoints.ListSavepoints(targetBlockName) ?? Array.Empty<SavepointInfo>();
            if (savepoints.Count == 0)
            {
                SetSavepointState($"No savepoints are available for '{targetBlockName}'.", BeepFormsMessageSeverity.Info);
            }
            else
            {
                UpdateSavepointStateFromManager(targetBlockName, "Savepoint inventory", BeepFormsMessageSeverity.Info);
            }

            ApplyShellStateToUi();
            return savepoints;
        }

        public bool ReleaseSavepoint(string savepointName, string? blockName = null)
        {
            if (!TryResolveWorkflowBlockName(blockName, out string targetBlockName, out string unavailableMessage))
            {
                SetSavepointState(unavailableMessage, BeepFormsMessageSeverity.Warning);
                ApplyShellStateToUi();
                return false;
            }

            if (string.IsNullOrWhiteSpace(savepointName))
            {
                SetSavepointState($"Savepoint release skipped for '{targetBlockName}': a savepoint name is required.", BeepFormsMessageSeverity.Warning);
                ApplyShellStateToUi();
                return false;
            }

            bool released = _formsManager?.Savepoints.ReleaseSavepoint(targetBlockName, savepointName) == true;
            if (released)
            {
                UpdateSavepointStateFromManager(targetBlockName, $"Released savepoint '{savepointName}'", BeepFormsMessageSeverity.Info);
            }
            else
            {
                SetSavepointState($"Savepoint '{savepointName}' was not found for '{targetBlockName}'.", BeepFormsMessageSeverity.Warning);
            }

            ApplyShellStateToUi();
            return released;
        }

        public void ReleaseAllSavepoints(string? blockName = null)
        {
            if (!TryResolveWorkflowBlockName(blockName, out string targetBlockName, out string unavailableMessage))
            {
                SetSavepointState(unavailableMessage, BeepFormsMessageSeverity.Warning);
                ApplyShellStateToUi();
                return;
            }

            _formsManager?.Savepoints.ReleaseAllSavepoints(targetBlockName);
            SetSavepointState($"All savepoints were released for '{targetBlockName}'.", BeepFormsMessageSeverity.Info);
            ApplyShellStateToUi();
        }

        public async Task<bool> RollbackToSavepointAsync(string savepointName, string? blockName = null, CancellationToken cancellationToken = default)
        {
            if (!TryResolveWorkflowBlockName(blockName, out string targetBlockName, out string unavailableMessage))
            {
                SetSavepointState(unavailableMessage, BeepFormsMessageSeverity.Warning);
                SetWorkflowState(unavailableMessage, BeepFormsMessageSeverity.Warning);
                ApplyShellStateToUi();
                return false;
            }

            if (string.IsNullOrWhiteSpace(savepointName))
            {
                SetSavepointState($"Savepoint rollback skipped for '{targetBlockName}': a savepoint name is required.", BeepFormsMessageSeverity.Warning);
                SetWorkflowState($"Savepoint rollback skipped for '{targetBlockName}': a savepoint name is required.", BeepFormsMessageSeverity.Warning);
                ApplyShellStateToUi();
                return false;
            }

            if (_formsManager?.Savepoints.SavepointExists(targetBlockName, savepointName) != true)
            {
                SetSavepointState($"Savepoint '{savepointName}' does not exist for '{targetBlockName}'.", BeepFormsMessageSeverity.Warning);
                SetWorkflowState($"Savepoint rollback is unavailable because '{savepointName}' does not exist for '{targetBlockName}'.", BeepFormsMessageSeverity.Warning);
                ApplyShellStateToUi();
                return false;
            }

            bool success = await TryRollbackToSavepointViaManagerAsync(targetBlockName, savepointName, cancellationToken).ConfigureAwait(true);
            if (!success)
            {
                SetSavepointState($"Savepoint rollback failed for '{targetBlockName}' -> '{savepointName}'.", BeepFormsMessageSeverity.Warning);
                SetWorkflowState($"Savepoint rollback failed for '{targetBlockName}' -> '{savepointName}'.", BeepFormsMessageSeverity.Error);
                ApplyShellStateToUi();
                return false;
            }

            SyncFromManager();
            await RefreshMasterDetailShellAsync(targetBlockName, $"savepoint rollback '{savepointName}'").ConfigureAwait(true);
            UpdateSavepointStateFromManager(targetBlockName, $"Rolled back to savepoint '{savepointName}'", BeepFormsMessageSeverity.Success);
            SetWorkflowState($"Savepoint rollback completed for '{targetBlockName}' -> '{savepointName}'. Hosted blocks were refreshed from manager state.", BeepFormsMessageSeverity.Success);
            ApplyShellStateToUi();
            return true;
        }

        public async Task<AlertResult> ShowAlertAsync(
            string title,
            string message,
            AlertStyle style = AlertStyle.None,
            string button1Text = "OK",
            string? button2Text = null,
            string? button3Text = null,
            CancellationToken cancellationToken = default)
        {
            if (_formsManager?.AlertProvider == null)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    SetAlertState($"Alert '{title}' was canceled before it was shown.", BeepFormsMessageSeverity.Warning);
                    ApplyShellStateToUi();
                    return AlertResult.None;
                }

                AlertResult fallbackResult = BeepFormsDialogService.ShowAlert(
                    FindForm(),
                    title,
                    message,
                    style,
                    button1Text,
                    button2Text,
                    button3Text);

                SetAlertState(BuildAlertStateText(title, style, fallbackResult), MapAlertSeverity(style, fallbackResult));
                ApplyShellStateToUi();
                return fallbackResult;
            }

            try
            {
                AlertResult result = await _formsManager.AlertProvider.ShowAlertAsync(
                    title,
                    message,
                    style,
                    button1Text,
                    button2Text,
                    button3Text,
                    cancellationToken).ConfigureAwait(true);

                SetAlertState(BuildAlertStateText(title, style, result), MapAlertSeverity(style, result));
                ApplyShellStateToUi();
                return result;
            }
            catch (Exception ex)
            {
                SetAlertState($"Alert '{title}' failed: {ex.Message}", BeepFormsMessageSeverity.Error);
                ApplyShellStateToUi();
                return AlertResult.None;
            }
        }

        public Task<AlertResult> ShowInfoAlertAsync(string title, string message, CancellationToken cancellationToken = default)
        {
            return ShowAlertAsync(title, message, AlertStyle.Information, "OK", null, null, cancellationToken);
        }

        public async Task<bool> ConfirmAsync(string title, string message, CancellationToken cancellationToken = default)
        {
            AlertResult result = await ShowAlertAsync(title, message, AlertStyle.Question, "Yes", "No", null, cancellationToken).ConfigureAwait(true);
            return result == AlertResult.Button1;
        }

        private bool TryResolveWorkflowBlockName(string? blockName, out string targetBlockName, out string unavailableMessage)
        {
            targetBlockName = ResolveWorkflowTargetBlockName(blockName);
            unavailableMessage = string.Empty;

            if (_formsManager == null)
            {
                unavailableMessage = "Workflow action is unavailable because no FormsManager is attached.";
                targetBlockName = string.Empty;
                return false;
            }

            if (string.IsNullOrWhiteSpace(targetBlockName) || !_formsManager.BlockExists(targetBlockName))
            {
                unavailableMessage = "Workflow action is unavailable because there is no active block context.";
                targetBlockName = string.Empty;
                return false;
            }

            return true;
        }

        private string ResolveWorkflowTargetBlockName(string? blockName)
        {
            if (!string.IsNullOrWhiteSpace(blockName))
            {
                return blockName;
            }

            if (!string.IsNullOrWhiteSpace(_viewState.ActiveBlockName))
            {
                return _viewState.ActiveBlockName;
            }

            return _formsManager?.CurrentBlockName ?? string.Empty;
        }

        private string? TryCreateBlockSavepoint(string blockName, string? savepointName)
        {
            if (_formsManager == null)
            {
                return null;
            }

            MethodInfo? wrapperMethod = _formsManager.GetType().GetMethod(
                "CreateBlockSavepoint",
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                types: new[] { typeof(string), typeof(string) },
                modifiers: null);

            if (wrapperMethod != null)
            {
                return wrapperMethod.Invoke(_formsManager, new object?[] { blockName, savepointName }) as string;
            }

            return _formsManager.Savepoints.CreateSavepoint(blockName, savepointName);
        }

        private async Task<bool> TryRollbackToSavepointViaManagerAsync(string blockName, string savepointName, CancellationToken cancellationToken)
        {
            if (_formsManager == null)
            {
                return false;
            }

            MethodInfo? wrapperMethod = _formsManager.GetType().GetMethod(
                "RollbackToSavepointAsync",
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                types: new[] { typeof(string), typeof(string), typeof(CancellationToken) },
                modifiers: null);

            if (wrapperMethod == null)
            {
                return false;
            }

            object? taskObject = wrapperMethod.Invoke(_formsManager, new object?[] { blockName, savepointName, cancellationToken });
            if (taskObject is Task<bool> typedTask)
            {
                return await typedTask.ConfigureAwait(true);
            }

            return false;
        }

        private void UpdateSavepointStateFromManager(string blockName, string leadText, BeepFormsMessageSeverity severity)
        {
            IReadOnlyList<SavepointInfo> savepoints = _formsManager?.Savepoints.ListSavepoints(blockName) ?? Array.Empty<SavepointInfo>();
            if (savepoints.Count == 0)
            {
                SetSavepointState($"{leadText} for '{blockName}', but no savepoints remain available.", BeepFormsMessageSeverity.Info);
                return;
            }

            SavepointInfo latest = savepoints[savepoints.Count - 1];
            string recordContext = latest.RecordCount > 0
                ? $"record {Math.Min(latest.RecordIndex + 1, latest.RecordCount)}/{latest.RecordCount}"
                : "no record snapshot";
            string dirtyContext = latest.WasDirty ? "dirty" : "clean";

            SetSavepointState(
                $"{leadText} for '{blockName}'. Latest savepoint '{latest.Name}' tracks {recordContext} and was {dirtyContext}. {savepoints.Count} savepoint(s) available.",
                severity);
        }

        private static string BuildAlertStateText(string title, AlertStyle style, AlertResult result)
        {
            string normalizedTitle = string.IsNullOrWhiteSpace(title) ? "Untitled alert" : title.Trim();
            return $"Alert '{normalizedTitle}' ({style}) returned {FormatAlertResult(result)}.";
        }

        private static string FormatAlertResult(AlertResult result)
        {
            return result switch
            {
                AlertResult.Button1 => "button 1",
                AlertResult.Button2 => "button 2",
                AlertResult.Button3 => "button 3",
                _ => "no selection"
            };
        }

        private static BeepFormsMessageSeverity MapAlertSeverity(AlertStyle style, AlertResult result)
        {
            if (result == AlertResult.None)
            {
                return BeepFormsMessageSeverity.Warning;
            }

            return style switch
            {
                AlertStyle.Stop => BeepFormsMessageSeverity.Error,
                AlertStyle.Caution => BeepFormsMessageSeverity.Warning,
                _ => BeepFormsMessageSeverity.Info
            };
        }

        internal string ResolveToolbarWorkflowBlockName(string? blockName = null)
        {
            return ResolveWorkflowTargetBlockName(blockName);
        }

        internal void PublishSavepointState(string text, BeepFormsMessageSeverity severity)
        {
            SetSavepointState(text, severity);
            ApplyShellStateToUi();
        }

        internal void PublishWorkflowState(string text, BeepFormsMessageSeverity severity)
        {
            SetWorkflowState(text, severity);
            ApplyShellStateToUi();
        }

        internal void PublishAlertState(string text, BeepFormsMessageSeverity severity)
        {
            SetAlertState(text, severity);
            ApplyShellStateToUi();
        }

        private void SetWorkflowState(string text, BeepFormsMessageSeverity severity)
        {
            string normalizedText = (text ?? string.Empty).Trim();
            _viewState.WorkflowText = normalizedText;
            _viewState.WorkflowSeverity = string.IsNullOrWhiteSpace(normalizedText)
                ? BeepFormsMessageSeverity.None
                : severity;

            if (!string.IsNullOrWhiteSpace(normalizedText))
            {
                _viewState.WorkflowHistoryItems.Insert(0, new BeepFormsWorkflowEntry
                {
                    Timestamp = DateTime.Now,
                    Text = normalizedText,
                    Severity = severity
                });

                while (_viewState.WorkflowHistoryItems.Count > WorkflowHistoryCapacity)
                {
                    _viewState.WorkflowHistoryItems.RemoveAt(_viewState.WorkflowHistoryItems.Count - 1);
                }
            }
        }

        private string BuildRollbackWorkflowText(string scopeName, string? targetName, IErrorsInfo? result, string successText)
        {
            if (result?.Flag == Errors.Ok)
            {
                return successText;
            }

            string scope = string.IsNullOrWhiteSpace(targetName)
                ? scopeName
                : $"{scopeName} '{targetName}'";
            string detail = string.IsNullOrWhiteSpace(result?.Message)
                ? "."
                : $": {result!.Message}";

            return ResolveRollbackWorkflowSeverity(result) == BeepFormsMessageSeverity.Warning
                ? $"{scope} rollback was blocked{detail}"
                : $"{scope} rollback failed{detail}";
        }

        private BeepFormsMessageSeverity ResolveRollbackWorkflowSeverity(IErrorsInfo? result)
        {
            if (result == null)
            {
                return BeepFormsMessageSeverity.Error;
            }

            if (result.Flag == Errors.Ok)
            {
                return BeepFormsMessageSeverity.Success;
            }

            if (result.Flag is Errors.Warning or Errors.Information)
            {
                return BeepFormsMessageSeverity.Warning;
            }

            return ClassifyMessageText(result.Message, BeepFormsMessageSeverity.Error) == BeepFormsMessageSeverity.Warning
                ? BeepFormsMessageSeverity.Warning
                : BeepFormsMessageSeverity.Error;
        }

        private void SetSavepointState(string text, BeepFormsMessageSeverity severity)
        {
            _viewState.SavepointText = text ?? string.Empty;
            _viewState.SavepointSeverity = string.IsNullOrWhiteSpace(text)
                ? BeepFormsMessageSeverity.None
                : severity;
        }

        private void SetAlertState(string text, BeepFormsMessageSeverity severity)
        {
            _viewState.AlertText = text ?? string.Empty;
            _viewState.AlertSeverity = string.IsNullOrWhiteSpace(text)
                ? BeepFormsMessageSeverity.None
                : severity;
        }
    }
}