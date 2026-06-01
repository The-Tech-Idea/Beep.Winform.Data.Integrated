using System.Threading.Tasks;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    public partial class BeepForms
    {
        public async Task<bool> MoveFirstAsync(string? blockName = null)
        {
            string targetBlockName = ResolveTargetBlockName(blockName);
            var messageSnapshot = CaptureMessageSnapshot();
            bool success = await _commandRouter.FirstRecordAsync(targetBlockName).ConfigureAwait(true);
            SyncFromManager();
            if (success)
            {
                await RefreshMasterDetailShellAsync(targetBlockName, "navigation").ConfigureAwait(true);
            }
            else
            {
                UpdateMasterDetailShellContext(targetBlockName);
            }

            if (!success)
            {
                PublishOperationFeedback(messageSnapshot, targetBlockName, $"Navigation stopped for '{targetBlockName}'.", BeepFormsMessageSeverity.Warning);
            }

            return success;
        }

        public async Task<bool> MovePreviousAsync(string? blockName = null)
        {
            string targetBlockName = ResolveTargetBlockName(blockName);
            var messageSnapshot = CaptureMessageSnapshot();
            bool success = await _commandRouter.PreviousRecordAsync(targetBlockName).ConfigureAwait(true);
            SyncFromManager();
            if (success)
            {
                await RefreshMasterDetailShellAsync(targetBlockName, "navigation").ConfigureAwait(true);
            }
            else
            {
                UpdateMasterDetailShellContext(targetBlockName);
            }

            if (!success)
            {
                PublishOperationFeedback(messageSnapshot, targetBlockName, $"Navigation stopped for '{targetBlockName}'.", BeepFormsMessageSeverity.Warning);
            }

            return success;
        }

        public async Task<bool> MoveNextAsync(string? blockName = null)
        {
            string targetBlockName = ResolveTargetBlockName(blockName);
            var messageSnapshot = CaptureMessageSnapshot();
            bool success = await _commandRouter.NextRecordAsync(targetBlockName).ConfigureAwait(true);
            SyncFromManager();
            if (success)
            {
                await RefreshMasterDetailShellAsync(targetBlockName, "navigation").ConfigureAwait(true);
            }
            else
            {
                UpdateMasterDetailShellContext(targetBlockName);
            }

            if (!success)
            {
                PublishOperationFeedback(messageSnapshot, targetBlockName, $"Navigation stopped for '{targetBlockName}'.", BeepFormsMessageSeverity.Warning);
            }

            return success;
        }

        public async Task<bool> MoveLastAsync(string? blockName = null)
        {
            string targetBlockName = ResolveTargetBlockName(blockName);
            var messageSnapshot = CaptureMessageSnapshot();
            bool success = await _commandRouter.LastRecordAsync(targetBlockName).ConfigureAwait(true);
            SyncFromManager();
            if (success)
            {
                await RefreshMasterDetailShellAsync(targetBlockName, "navigation").ConfigureAwait(true);
            }
            else
            {
                UpdateMasterDetailShellContext(targetBlockName);
            }

            if (!success)
            {
                PublishOperationFeedback(messageSnapshot, targetBlockName, $"Navigation stopped for '{targetBlockName}'.", BeepFormsMessageSeverity.Warning);
            }

            return success;
        }
    }
}