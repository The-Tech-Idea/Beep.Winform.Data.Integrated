using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    public partial class BeepForms
    {
        private bool _masterDetailRefreshInProgress;

        private void UpdateMasterDetailShellContext(string? blockName = null)
        {
            if (_formsManager == null)
            {
                SetCoordinationState(string.Empty, BeepFormsMessageSeverity.None);
                ApplyShellStateToUi();
                return;
            }

            string targetBlockName = !string.IsNullOrWhiteSpace(blockName)
                ? blockName
                : (_viewState.ActiveBlockName ?? _formsManager.CurrentBlockName ?? string.Empty);

            if (string.IsNullOrWhiteSpace(targetBlockName) || !_formsManager.BlockExists(targetBlockName))
            {
                SetCoordinationState(string.Empty, BeepFormsMessageSeverity.None);
                ApplyShellStateToUi();
                return;
            }

            List<string> detailBlocks = _formsManager.GetDetailBlocks(targetBlockName) ?? new List<string>();
            string masterBlockName = _formsManager.GetMasterBlock(targetBlockName) ?? string.Empty;

            if (detailBlocks.Count > 0)
            {
                SetCoordinationState(
                    $"Master '{targetBlockName}' coordinates detail block(s): {string.Join(", ", detailBlocks)}.",
                    BeepFormsMessageSeverity.Info);
            }
            else if (!string.IsNullOrWhiteSpace(masterBlockName))
            {
                SetCoordinationState(
                    $"Detail '{targetBlockName}' is coordinated by master '{masterBlockName}'.",
                    BeepFormsMessageSeverity.Info);
            }
            else
            {
                SetCoordinationState(string.Empty, BeepFormsMessageSeverity.None);
            }

            ApplyShellStateToUi();
        }

        private async Task RefreshMasterDetailShellAsync(string blockName, string reason)
        {
            if (_formsManager == null || string.IsNullOrWhiteSpace(blockName) || !_formsManager.BlockExists(blockName))
            {
                UpdateMasterDetailShellContext(blockName);
                return;
            }

            List<string> detailBlocks = _formsManager.GetDetailBlocks(blockName) ?? new List<string>();
            if (detailBlocks.Count == 0)
            {
                UpdateMasterDetailShellContext(blockName);
                return;
            }

            if (_masterDetailRefreshInProgress)
            {
                UpdateMasterDetailShellContext(blockName);
                return;
            }

            try
            {
                _masterDetailRefreshInProgress = true;
                await _formsManager.SynchronizeDetailBlocksAsync(blockName).ConfigureAwait(true);

                SyncBlockView(blockName);
                foreach (string detailBlockName in detailBlocks)
                {
                    SyncBlockView(detailBlockName);
                }

                _managerAdapter.Sync(_viewState);
                SetCoordinationState(
                    $"Master '{blockName}' refreshed detail block(s) after {reason}: {string.Join(", ", detailBlocks)}.",
                    BeepFormsMessageSeverity.Success);
            }
            catch (Exception ex)
            {
                SetCoordinationState(
                    $"Master '{blockName}' detail refresh failed after {reason}: {ex.Message}",
                    BeepFormsMessageSeverity.Warning);
            }
            finally
            {
                _masterDetailRefreshInProgress = false;
                ApplyShellStateToUi();
                Invalidate();
            }
        }

        private void QueueMasterDetailRefreshFromFieldChange(string blockName, BlockFieldChangedEventArgs e)
        {
            if (!ShouldSynchronizeDetailBlocksOnFieldChange(blockName, e))
            {
                return;
            }

            _ = RefreshMasterDetailShellAsync(blockName, $"'{e.FieldName}' change");
        }

        private bool ShouldSynchronizeDetailBlocksOnFieldChange(string blockName, BlockFieldChangedEventArgs e)
        {
            if (_formsManager == null || string.IsNullOrWhiteSpace(blockName) || !_formsManager.BlockExists(blockName))
            {
                return false;
            }

            List<string> detailBlocks = _formsManager.GetDetailBlocks(blockName) ?? new List<string>();
            if (detailBlocks.Count == 0)
            {
                return false;
            }

            string masterKeyField = _formsManager.GetBlock(blockName)?.MasterKeyField ?? string.Empty;
            return string.IsNullOrWhiteSpace(masterKeyField) ||
                   string.IsNullOrWhiteSpace(e.FieldName) ||
                   string.Equals(masterKeyField, e.FieldName, StringComparison.OrdinalIgnoreCase);
        }

        private void SetCoordinationState(string text, BeepFormsMessageSeverity severity)
        {
            _viewState.CoordinationText = text ?? string.Empty;
            _viewState.CoordinationSeverity = string.IsNullOrWhiteSpace(text)
                ? BeepFormsMessageSeverity.None
                : severity;
        }
    }
}