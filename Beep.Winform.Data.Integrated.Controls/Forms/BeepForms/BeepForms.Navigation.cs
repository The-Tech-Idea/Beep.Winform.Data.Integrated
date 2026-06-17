using System.Threading.Tasks;
using TheTechIdea.Beep.Editor.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms;

public partial class BeepForms
{
    public async Task<bool> MoveFirstAsync(string? blockName = null)
    {
        string targetBlockName = ResolveTargetBlockName(blockName);
        var messageSnapshot = CaptureMessageSnapshot();
        bool success = _formsManager != null
            ? await _formsManager.FirstRecordAsync(targetBlockName).ConfigureAwait(true)
            : false;
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
            PublishOperationFeedback(messageSnapshot, targetBlockName, $"Navigation stopped for '{targetBlockName}'.", BeepMessageSeverity.Warning);
        }

        return success;
    }

    public async Task<bool> MovePreviousAsync(string? blockName = null)
    {
        string targetBlockName = ResolveTargetBlockName(blockName);
        var messageSnapshot = CaptureMessageSnapshot();
        bool success = _formsManager != null
            ? await _formsManager.PreviousRecordAsync(targetBlockName).ConfigureAwait(true)
            : false;
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
            PublishOperationFeedback(messageSnapshot, targetBlockName, $"Navigation stopped for '{targetBlockName}'.", BeepMessageSeverity.Warning);
        }

        return success;
    }

    public async Task<bool> MoveNextAsync(string? blockName = null)
    {
        string targetBlockName = ResolveTargetBlockName(blockName);
        var messageSnapshot = CaptureMessageSnapshot();
        bool success = _formsManager != null
            ? await _formsManager.NextRecordAsync(targetBlockName).ConfigureAwait(true)
            : false;
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
            PublishOperationFeedback(messageSnapshot, targetBlockName, $"Navigation stopped for '{targetBlockName}'.", BeepMessageSeverity.Warning);
        }

        return success;
    }

    public async Task<bool> MoveLastAsync(string? blockName = null)
    {
        string targetBlockName = ResolveTargetBlockName(blockName);
        var messageSnapshot = CaptureMessageSnapshot();
        bool success = _formsManager != null
            ? await _formsManager.LastRecordAsync(targetBlockName).ConfigureAwait(true)
            : false;
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
            PublishOperationFeedback(messageSnapshot, targetBlockName, $"Navigation stopped for '{targetBlockName}'.", BeepMessageSeverity.Warning);
        }

        return success;
    }
}
