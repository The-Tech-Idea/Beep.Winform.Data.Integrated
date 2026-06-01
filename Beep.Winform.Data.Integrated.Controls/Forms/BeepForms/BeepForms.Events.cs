using System;
using System.Linq;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Editor.UOWManager.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    public partial class BeepForms
    {
        private void AttachToFormsManager(IUnitofWorksManager? formsManager)
        {
            if (formsManager == null)
            {
                return;
            }

            formsManager.OnBlockFieldChanged += HandleManagerBlockFieldChanged;
            formsManager.OnFormMessage += HandleManagerFormMessage;
            formsManager.Messages.OnMessage += HandleManagerBlockMessage;
            formsManager.Messages.OnMessageCleared += HandleManagerBlockMessageCleared;
            formsManager.ErrorLog.OnError += HandleManagerError;
            formsManager.ErrorLog.OnWarning += HandleManagerWarning;
            AttachTriggerProxy(formsManager);
        }

        private void DetachFromFormsManager(IUnitofWorksManager? formsManager)
        {
            if (formsManager == null)
            {
                return;
            }

            formsManager.OnBlockFieldChanged -= HandleManagerBlockFieldChanged;
            formsManager.OnFormMessage -= HandleManagerFormMessage;
            formsManager.Messages.OnMessage -= HandleManagerBlockMessage;
            formsManager.Messages.OnMessageCleared -= HandleManagerBlockMessageCleared;
            formsManager.ErrorLog.OnError -= HandleManagerError;
            formsManager.ErrorLog.OnWarning -= HandleManagerWarning;
            DetachTriggerProxy(formsManager);
        }

        private void HandleManagerBlockFieldChanged(object? sender, BlockFieldChangedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                string blockName = !string.IsNullOrWhiteSpace(_formsManager?.CurrentBlockName)
                    ? _formsManager.CurrentBlockName
                    : e.BlockName;

                if (!string.IsNullOrWhiteSpace(blockName))
                {
                    SetActiveBlockState(blockName);
                    SyncBlockView(blockName);
                    UpdateMasterDetailShellContext(blockName);
                    QueueMasterDetailRefreshFromFieldChange(blockName, e);
                }

                _managerAdapter.Sync(_viewState);
                ApplyShellStateToUi();
                Invalidate();
            });
        }

        private void HandleManagerFormMessage(object? sender, FormMessageEventArgs e)
        {
            RunOnUiThread(() =>
            {
                string messageText = FormatFormMessage(e);
                if (!string.IsNullOrWhiteSpace(messageText))
                {
                    ShowMessage(messageText, MapFormMessageSeverity(e.Message?.MessageType));
                }
            });
        }

        private void HandleManagerBlockMessage(object? sender, BlockMessageEventArgs e)
        {
            RunOnUiThread(() =>
            {
                string blockName = e.Message?.BlockName ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(blockName))
                {
                    SyncBlockView(blockName);
                    UpdateMasterDetailShellContext(blockName);
                }

                string text = FormatBlockMessage(e);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    ShowMessage(text, MapMessageLevel(e.Message?.Level ?? MessageLevel.Info));
                }
            });
        }

        private void HandleManagerBlockMessageCleared(object? sender, BlockMessageEventArgs e)
        {
            RunOnUiThread(() =>
            {
                if (e.IsClear)
                {
                    ClearMessages();
                }
            });
        }

        private void HandleManagerError(object? sender, BlockErrorEventArgs e)
        {
            RunOnUiThread(() =>
            {
                string blockName = e.ErrorInfo?.BlockName ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(blockName))
                {
                    SyncBlockView(blockName);
                    UpdateMasterDetailShellContext(blockName);
                }

                ShowMessage(FormatErrorMessage(e), MapErrorSeverity(e.ErrorInfo?.Severity ?? ErrorSeverity.Error));
            });
        }

        private void HandleManagerWarning(object? sender, BlockErrorEventArgs e)
        {
            RunOnUiThread(() =>
            {
                string blockName = e.ErrorInfo?.BlockName ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(blockName))
                {
                    SyncBlockView(blockName);
                    UpdateMasterDetailShellContext(blockName);
                }

                ShowMessage(FormatErrorMessage(e), MapErrorSeverity(e.ErrorInfo?.Severity ?? ErrorSeverity.Warning));
            });
        }

        private void SetActiveBlockState(string blockName)
        {
            if (string.IsNullOrWhiteSpace(blockName) ||
                !_blocks.Any(x => string.Equals(x.BlockName, blockName, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            if (!string.Equals(_viewState.ActiveBlockName, blockName, StringComparison.OrdinalIgnoreCase))
            {
                _viewState.ActiveBlockName = blockName;
                ActiveBlockChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SyncBlockView(string blockName)
        {
            var blockView = _blocks.FirstOrDefault(x => string.Equals(x.BlockName, blockName, StringComparison.OrdinalIgnoreCase));
            if (blockView != null)
            {
                _managerAdapter.SyncBlock(blockView);
            }
        }

        private void RunOnUiThread(Action action)
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                if (IsHandleCreated)
                {
                    BeginInvoke(action);
                }
                else
                {
                    action();
                }

                return;
            }

            action();
        }

        private string FormatFormMessage(FormMessageEventArgs e)
        {
            if (e?.Message == null)
            {
                return string.Empty;
            }

            string messageType = e.Message.MessageType ?? string.Empty;
            string payloadText = e.Message.Payload?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(messageType))
            {
                return payloadText;
            }

            if (string.IsNullOrWhiteSpace(payloadText))
            {
                return messageType;
            }

            return $"{messageType}: {payloadText}";
        }

        private string FormatBlockMessage(BlockMessageEventArgs e)
        {
            if (e?.Message == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(e.Message.BlockName))
            {
                return e.Message.Text ?? string.Empty;
            }

            return $"[{e.Message.BlockName}] {e.Message.Text}";
        }

        private string FormatErrorMessage(BlockErrorEventArgs e)
        {
            if (e?.ErrorInfo == null)
            {
                return "Manager error.";
            }

            string blockName = e.ErrorInfo.BlockName ?? string.Empty;
            string context = e.ErrorInfo.Context ?? string.Empty;
            string message = e.ErrorInfo.Message ?? e.ErrorInfo.Exception?.Message ?? "Manager error.";

            if (string.IsNullOrWhiteSpace(blockName) && string.IsNullOrWhiteSpace(context))
            {
                return message;
            }

            if (string.IsNullOrWhiteSpace(context))
            {
                return $"[{blockName}] {message}";
            }

            return $"[{blockName}:{context}] {message}";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DetachFromFormsManager(_formsManager);
            }

            base.Dispose(disposing);
        }
    }
}