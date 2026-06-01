using System;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    public partial class BeepForms
    {
        private IBeepFormsNotificationService _notificationService = new Services.BeepFormsMessageService();

        public IBeepFormsNotificationService NotificationService
        {
            get => _notificationService;
            set => _notificationService = value ?? new Services.BeepFormsMessageService();
        }

        public void ShowInfo(string message)
        {
            ShowMessage(message, BeepFormsMessageSeverity.Info);
        }

        public void ShowSuccess(string message)
        {
            ShowMessage(message, BeepFormsMessageSeverity.Success);
        }

        public void ShowWarning(string message)
        {
            ShowMessage(message, BeepFormsMessageSeverity.Warning);
        }

        public void ShowError(string message)
        {
            ShowMessage(message, BeepFormsMessageSeverity.Error);
        }

        public void ClearMessages()
        {
            _notificationService.Clear(_viewState);
            ApplyShellStateToUi();
        }

        private void ShowMessage(string message, BeepFormsMessageSeverity severity)
        {
            _notificationService.Publish(_viewState, message, severity);
            ApplyShellStateToUi();
        }

        private static BeepFormsMessageSeverity MapMessageLevel(MessageLevel level)
        {
            return level switch
            {
                MessageLevel.Success => BeepFormsMessageSeverity.Success,
                MessageLevel.Warning => BeepFormsMessageSeverity.Warning,
                MessageLevel.Error => BeepFormsMessageSeverity.Error,
                _ => BeepFormsMessageSeverity.Info
            };
        }

        private static BeepFormsMessageSeverity MapErrorSeverity(ErrorSeverity severity)
        {
            return severity switch
            {
                ErrorSeverity.Info => BeepFormsMessageSeverity.Info,
                ErrorSeverity.Warning => BeepFormsMessageSeverity.Warning,
                ErrorSeverity.Critical => BeepFormsMessageSeverity.Error,
                ErrorSeverity.Error => BeepFormsMessageSeverity.Error,
                _ => BeepFormsMessageSeverity.Info
            };
        }

        private static BeepFormsMessageSeverity MapFormMessageSeverity(string? messageType)
        {
            if (string.IsNullOrWhiteSpace(messageType))
            {
                return BeepFormsMessageSeverity.Info;
            }

            if (messageType.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0 ||
                messageType.IndexOf("fail", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return BeepFormsMessageSeverity.Error;
            }

            if (messageType.IndexOf("warn", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return BeepFormsMessageSeverity.Warning;
            }

            if (messageType.IndexOf("success", StringComparison.OrdinalIgnoreCase) >= 0 ||
                messageType.IndexOf("ok", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return BeepFormsMessageSeverity.Success;
            }

            return BeepFormsMessageSeverity.Info;
        }

        private (string Message, BeepFormsMessageSeverity Severity) CaptureMessageSnapshot()
        {
            return (_viewState.CurrentMessage ?? string.Empty, _viewState.MessageSeverity);
        }

        private bool HasMessageChanged((string Message, BeepFormsMessageSeverity Severity) snapshot)
        {
            return !string.Equals(snapshot.Message, _viewState.CurrentMessage ?? string.Empty, StringComparison.Ordinal) ||
                   snapshot.Severity != _viewState.MessageSeverity;
        }

        private void PublishOperationFeedback(
            (string Message, BeepFormsMessageSeverity Severity) snapshot,
            string? preferredBlockName,
            string fallbackMessage,
            BeepFormsMessageSeverity fallbackSeverity)
        {
            if (HasMessageChanged(snapshot) || _managerAdapter.TryGetCurrentMessage(preferredBlockName ?? _viewState.ActiveBlockName, out _, out _))
            {
                ApplyShellStateToUi();
                return;
            }

            if (!string.IsNullOrWhiteSpace(_formsManager?.Status) && !IsNeutralStatus(_formsManager.Status))
            {
                ShowMessage(_formsManager.Status, ClassifyMessageText(_formsManager.Status, fallbackSeverity));
                return;
            }

            if (!string.IsNullOrWhiteSpace(fallbackMessage))
            {
                ShowMessage(fallbackMessage, fallbackSeverity);
            }
        }

        private static BeepFormsMessageSeverity ResolveCommandResultSeverity(IErrorsInfo? result, BeepFormsMessageSeverity successSeverity = BeepFormsMessageSeverity.Success)
        {
            if (result == null)
            {
                return BeepFormsMessageSeverity.Error;
            }

            return result.Flag switch
            {
                Errors.Ok => ClassifyMessageText(result.Message, successSeverity),
                Errors.Warning => BeepFormsMessageSeverity.Warning,
                Errors.Information => BeepFormsMessageSeverity.Info,
                Errors.Critical => BeepFormsMessageSeverity.Error,
                Errors.Exception => BeepFormsMessageSeverity.Error,
                Errors.Error => BeepFormsMessageSeverity.Error,
                Errors.Fatal => BeepFormsMessageSeverity.Error,
                _ => ClassifyMessageText(result.Message, BeepFormsMessageSeverity.Error)
            };
        }

        private static BeepFormsMessageSeverity ClassifyMessageText(string? message, BeepFormsMessageSeverity defaultSeverity)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return defaultSeverity;
            }

            if (ContainsMessageToken(message,
                    "cancelled",
                    "canceled",
                    "blocked",
                    "not allowed",
                    "not permitted",
                    "validation failed",
                    "duplicate",
                    "no changes",
                    "must be in query mode",
                    "already in query mode",
                    "cannot enter query mode"))
            {
                return BeepFormsMessageSeverity.Warning;
            }

            if (ContainsMessageToken(message, "error", "exception", "fatal", "critical", "failed"))
            {
                return BeepFormsMessageSeverity.Error;
            }

            if (ContainsMessageToken(message, "warning"))
            {
                return BeepFormsMessageSeverity.Warning;
            }

            if (ContainsMessageToken(message,
                    "success",
                    "completed",
                    "committed",
                    "rolled back",
                    "executed successfully",
                    "entered query mode",
                    "navigated",
                    "switched to block"))
            {
                return defaultSeverity;
            }

            if (ContainsMessageToken(message, "info", "ready"))
            {
                return BeepFormsMessageSeverity.Info;
            }

            return defaultSeverity;
        }

        private static bool ContainsMessageToken(string message, params string[] tokens)
        {
            foreach (string token in tokens)
            {
                if (message.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsNeutralStatus(string? status)
        {
            return string.IsNullOrWhiteSpace(status) ||
                   string.Equals(status.Trim(), "Ready", StringComparison.OrdinalIgnoreCase);
        }
    }
}