using System;
using System.Linq;
using System.Threading.Tasks;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Helpers;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    public partial class BeepFormsToolbar
    {
        private async void SavepointActionsButton_SelectedItemChanged(object? sender, SelectedItemChangedEventArgs e)
        {
            string action = e.SelectedItem?.Value?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(action) || _formsHost == null)
            {
                ResetToolbarSelection(sender);
                return;
            }

            try
            {
                switch (action)
                {
                    case "capture":
                        await CaptureSavepointFromToolbarAsync().ConfigureAwait(true);
                        break;
                    case "list":
                        ShowSavepointInventoryDialog();
                        break;
                    case "rollback":
                        await RollbackSavepointFromToolbarAsync().ConfigureAwait(true);
                        break;
                    case "release":
                        await ReleaseSavepointFromToolbarAsync().ConfigureAwait(true);
                        break;
                    case "release_all":
                        await ReleaseAllSavepointsFromToolbarAsync().ConfigureAwait(true);
                        break;
                }
            }
            finally
            {
                ResetToolbarSelection(sender);
                UpdateCommandStripState();
            }
        }

        private async void AlertPresetsButton_SelectedItemChanged(object? sender, SelectedItemChangedEventArgs e)
        {
            string action = e.SelectedItem?.Value?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(action) || _formsHost == null)
            {
                ResetToolbarSelection(sender);
                return;
            }

            try
            {
                (string Title, string Message, AlertStyle Style) preset = BuildAlertPreset(action);
                if (string.Equals(action, "question", StringComparison.OrdinalIgnoreCase))
                {
                    await _formsHost.ConfirmAsync(preset.Title, preset.Message).ConfigureAwait(true);
                }
                else
                {
                    await _formsHost.ShowAlertAsync(preset.Title, preset.Message, preset.Style).ConfigureAwait(true);
                }
            }
            finally
            {
                ResetToolbarSelection(sender);
                UpdateCommandStripState();
            }
        }

        private static void ResetToolbarSelection(object? sender)
        {
            if (sender is BeepButton button)
            {
                button.SelectedItem = null;
            }
        }

        private async Task CaptureSavepointFromToolbarAsync()
        {
            if (_formsHost == null)
            {
                return;
            }

            string? blockName = ResolveWorkflowTargetBlockName();
            string? enteredName = BeepFormsDialogService.PromptText(
                FindForm(),
                "Capture Savepoint",
                string.IsNullOrWhiteSpace(blockName)
                    ? "Enter an optional savepoint name. Leave blank to let FormsManager generate one."
                    : $"Enter an optional savepoint name for '{blockName}'. Leave blank to let FormsManager generate one.",
                string.Empty,
                allowEmpty: true);

            if (enteredName == null)
            {
                return;
            }

            _formsHost.CreateSavepoint(blockName, string.IsNullOrWhiteSpace(enteredName) ? null : enteredName.Trim());
            await Task.CompletedTask.ConfigureAwait(true);
        }

        private async Task RollbackSavepointFromToolbarAsync()
        {
            if (_formsHost == null)
            {
                return;
            }

            string blockName = ResolveWorkflowTargetBlockName();
            SavepointInfo? selectedSavepoint = ShowSavepointPickerDialog(blockName, "Rollback To Savepoint", "Choose the savepoint that should restore the active block state.");
            if (selectedSavepoint == null)
            {
                return;
            }

            await _formsHost.RollbackToSavepointAsync(selectedSavepoint.Name, blockName).ConfigureAwait(true);
        }

        private async Task ReleaseSavepointFromToolbarAsync()
        {
            if (_formsHost == null)
            {
                return;
            }

            string blockName = ResolveWorkflowTargetBlockName();
            SavepointInfo? selectedSavepoint = ShowSavepointPickerDialog(blockName, "Release Savepoint", "Choose the savepoint that should be removed from the active block.");
            if (selectedSavepoint == null)
            {
                return;
            }

            _formsHost.ReleaseSavepoint(selectedSavepoint.Name, blockName);
            await Task.CompletedTask.ConfigureAwait(true);
        }

        private async Task ReleaseAllSavepointsFromToolbarAsync()
        {
            if (_formsHost == null)
            {
                return;
            }

            string blockName = ResolveWorkflowTargetBlockName();
            if (string.IsNullOrWhiteSpace(blockName))
            {
                _formsHost.PublishSavepointState("Release-all is unavailable because there is no active block context.", BeepFormsMessageSeverity.Warning);
                return;
            }

            bool confirmed = await _formsHost.ConfirmAsync(
                "Release All Savepoints",
                $"Release every savepoint for '{blockName}'? This cannot be undone.").ConfigureAwait(true);
            if (!confirmed)
            {
                _formsHost.PublishSavepointState($"Release-all savepoints was canceled for '{blockName}'.", BeepFormsMessageSeverity.Info);
                return;
            }

            _formsHost.ReleaseAllSavepoints(blockName);
        }

        private SavepointInfo? ShowSavepointPickerDialog(string blockName, string title, string caption)
        {
            if (_formsHost == null)
            {
                return null;
            }

            var savepoints = _formsHost.ListSavepoints(blockName);
            if (savepoints.Count == 0)
            {
                return null;
            }

            return BeepFormsDialogService.PickItem(
                FindForm(),
                title,
                caption,
                savepoints,
                FormatSavepointListText,
                primaryText: "Select");
        }

        private void ShowSavepointInventoryDialog()
        {
            if (_formsHost == null)
            {
                return;
            }

            string blockName = ResolveWorkflowTargetBlockName();
            var savepoints = _formsHost.ListSavepoints(blockName);
            if (savepoints.Count == 0)
            {
                return;
            }

            BeepFormsDialogService.ShowList(
                FindForm(),
                string.IsNullOrWhiteSpace(blockName) ? "Savepoint Inventory" : $"Savepoints - {blockName}",
                "Review the savepoints currently available for the active workflow block.",
                savepoints,
                FormatSavepointListText,
                primaryText: "Close");
        }

        private static string FormatSavepointListText(SavepointInfo savepoint)
        {
            string recordText = savepoint.RecordCount > 0
                ? $"record {Math.Min(savepoint.RecordIndex + 1, savepoint.RecordCount)}/{savepoint.RecordCount}"
                : "no record snapshot";
            string dirtyText = savepoint.WasDirty ? "dirty" : "clean";
            string ageText = savepoint.Age.TotalHours >= 1
                ? savepoint.Age.ToString(@"hh\:mm\:ss")
                : savepoint.Age.ToString(@"mm\:ss");

            return $"{savepoint.Name}  |  {recordText}  |  {dirtyText}  |  age {ageText}";
        }

        private (string Title, string Message, AlertStyle Style) BuildAlertPreset(string action)
        {
            string blockName = ResolveWorkflowTargetBlockName();
            string contextName = string.IsNullOrWhiteSpace(blockName)
                ? (_formsHost?.FormName ?? string.Empty)
                : blockName;
            if (string.IsNullOrWhiteSpace(contextName))
            {
                contextName = "Current form";
            }

            BeepFormsViewState? viewState = _formsHost?.ViewState;
            string fallbackStatus = string.IsNullOrWhiteSpace(viewState?.StatusText)
                ? $"'{contextName}' is ready."
                : viewState.StatusText;

            return action switch
            {
                "caution" => (
                    $"Review {contextName}",
                    viewState?.IsDirty == true
                        ? $"'{contextName}' has pending changes. Review the current state before continuing."
                        : $"Review '{contextName}' before continuing.",
                    AlertStyle.Caution),
                "stop" => (
                    $"Attention Required - {contextName}",
                    FirstNonEmpty(viewState?.CurrentMessage, viewState?.AlertText, viewState?.CoordinationText, $"'{contextName}' needs attention before the next operation."),
                    AlertStyle.Stop),
                "question" => (
                    $"Continue With {contextName}?",
                    $"Do you want to continue working with '{contextName}'?",
                    AlertStyle.Question),
                _ => (
                    $"Status - {contextName}",
                    FirstNonEmpty(viewState?.CurrentMessage, viewState?.CoordinationText, fallbackStatus),
                    AlertStyle.Information)
            };
        }

        private static string FirstNonEmpty(params string?[] values)
        {
            foreach (string? value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return string.Empty;
        }
    }
}