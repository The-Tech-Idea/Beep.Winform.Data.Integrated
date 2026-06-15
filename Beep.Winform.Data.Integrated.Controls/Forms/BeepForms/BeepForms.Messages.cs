using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor.Forms.Builtins;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    public partial class BeepForms
    {
        private IBeepFormsNotificationService _notificationService = new Services.BeepFormsMessageService();
        private readonly object _messageLock = new();

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
            lock (_messageLock)
            {
                _notificationService.Clear(_viewState);
            }
            ApplyShellStateToUi();
        }

        private void ShowMessage(string message, BeepFormsMessageSeverity severity)
        {
            lock (_messageLock)
            {
                _notificationService.Publish(_viewState, message, severity);
            }
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

            if (Regex.IsMatch(messageType, @"\bsuccess\b", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(messageType, @"\bok\b", RegexOptions.IgnoreCase))
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

        // ── Oracle Forms MESSAGE / ALERT built-ins ─────────────────────────────

        /// <summary>
        /// Implementation of the Oracle Forms <c>MESSAGE</c> built-in.
        /// Translates the Forms severity levels (0/5/10/15) into the
        /// Beep message service's severity and publishes through the same
        /// notification service the rest of the shell uses.
        /// </summary>
        public void PublishBuiltinMessage(string message, int messageLevel, BeepBuiltinMessageSeverity severity)
        {
            if (string.IsNullOrEmpty(message))
            {
                ClearBuiltinMessage();
                return;
            }

            BeepFormsMessageSeverity mapped = severity switch
            {
                BeepBuiltinMessageSeverity.Hint => BeepFormsMessageSeverity.Info,
                BeepBuiltinMessageSeverity.Warning => BeepFormsMessageSeverity.Warning,
                BeepBuiltinMessageSeverity.Error => BeepFormsMessageSeverity.Error,
                _ => BeepFormsMessageSeverity.Info
            };

            // Forms treats messageLevel >= 25 as "no message" — honor that.
            if (messageLevel >= 25)
            {
                return;
            }

            ShowMessage(message, mapped);
        }

        public void ClearBuiltinMessage()
        {
            ClearMessages();
        }

        public void ShowErrorSummary()
        {
            if (_formsManager == null)
            {
                ShowInfo("No FormsManager attached.");
                return;
            }

            var lines = new System.Text.StringBuilder();
            int totalErrors = 0;
            int blocksWithErrors = 0;

            foreach (var block in _blocks)
            {
                string? blockName = block.BlockName;
                if (string.IsNullOrWhiteSpace(blockName)) continue;
                if (!_formsManager.BlockExists(blockName)) continue;

                var errors = _formsManager.ErrorLog.GetErrorLog(blockName);
                if (errors.Count == 0) continue;

                blocksWithErrors++;
                totalErrors += errors.Count;
                lines.AppendLine($"── {blockName} ({errors.Count} error(s)) ──");
                foreach (var err in errors.Take(10))
                {
                    string severity = err.Severity.ToString().ToUpperInvariant();
                    lines.AppendLine($"  [{severity}] {err.Message}");
                    if (!string.IsNullOrWhiteSpace(err.Context) && err.Context != err.Message)
                        lines.AppendLine($"           Context: {err.Context}");
                }
                if (errors.Count > 10)
                    lines.AppendLine($"  ... and {errors.Count - 10} more");
                lines.AppendLine();
            }

            if (totalErrors == 0)
            {
                ShowSuccess("No errors recorded.");
                return;
            }

            string summary = $"{totalErrors} error(s) across {blocksWithErrors} block(s)";
            MessageBox.Show(lines.ToString(), $"Error Summary — {summary}",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Implementation of the Oracle Forms <c>ALERT</c> built-in. Maps
        /// the Forms <c>ALERT_MESSAGE</c> style constants to a WinForms
        /// <see cref="MessageBox"/> icon and returns the Forms-style
        /// 1-based button index.
        /// </summary>
        public Task<int> ShowBuiltinAlertAsync(
            string title,
            string message,
            BeepBuiltinAlertStyle style,
            string button1,
            string? button2,
            string? button3,
            CancellationToken ct = default)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                MessageBoxIcon icon = style switch
                {
                    BeepBuiltinAlertStyle.Caution => MessageBoxIcon.Warning,
                    BeepBuiltinAlertStyle.Stop => MessageBoxIcon.Stop,
                    BeepBuiltinAlertStyle.Note => MessageBoxIcon.Information,
                    _ => MessageBoxIcon.Information
                };

                // Build the button list. Forms accepted up to 3 buttons.
                string buttonText = string.IsNullOrEmpty(button1) ? "OK" : button1;
                if (!string.IsNullOrEmpty(button2) && !string.IsNullOrEmpty(button3))
                {
                    DialogResult r = MessageBox.Show(
                        message,
                        string.IsNullOrEmpty(title) ? "Alert" : title,
                        MessageBoxButtons.YesNoCancel,
                        icon);
                    if (r == DialogResult.Yes) return 1;
                    if (r == DialogResult.No) return 2;
                    return 3;
                }
                if (!string.IsNullOrEmpty(button2))
                {
                    DialogResult r = MessageBox.Show(
                        message,
                        string.IsNullOrEmpty(title) ? "Alert" : title,
                        MessageBoxButtons.YesNo,
                        icon);
                    return r == DialogResult.Yes ? 1 : 2;
                }
                MessageBox.Show(
                    message,
                    string.IsNullOrEmpty(title) ? "Alert" : title,
                    MessageBoxButtons.OK,
                    icon);
                return 1;
            }, ct);
        }
    }
}