using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Controls.Base;
using TheTechIdea.Beep.Winform.Controls.ToolTips;
using ValidationSeverity = TheTechIdea.Beep.Editor.ValidationSeverity;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks
{
    public partial class BeepBlock
    {
        private readonly Dictionary<string, ValidationSurfaceMessage> _fieldValidationStates = new(StringComparer.OrdinalIgnoreCase);
        private IUnitofWorksManager? _validationOwner;

        private sealed class ValidationSurfaceMessage
        {
            public ValidationSurfaceMessage(string message, ValidationSeverity severity)
            {
                Message = message ?? string.Empty;
                Severity = severity;
            }

            public string Message { get; }
            public ValidationSeverity Severity { get; }
        }

        private readonly struct ValidationSummaryInfo
        {
            public ValidationSummaryInfo(string headline, string details, string hint, string severityBadge, ValidationSeverity severity)
            {
                Headline = headline;
                Details = details;
                Hint = hint;
                SeverityBadge = severityBadge;
                Severity = severity;
            }

            public string Headline { get; }
            public string Details { get; }
            public string Hint { get; }
            public string SeverityBadge { get; }
            public ValidationSeverity Severity { get; }
            public bool HasContent =>
                !string.IsNullOrWhiteSpace(Headline) ||
                !string.IsNullOrWhiteSpace(Details) ||
                !string.IsNullOrWhiteSpace(Hint);
        }

        private void SyncValidationSubscriptions(IUnitofWorksManager? manager)
        {
            if (ReferenceEquals(_validationOwner, manager))
            {
                return;
            }

            if (_validationOwner != null)
            {
                _validationOwner.Validation.ValidationFailed -= Validation_ValidationFailed;
                _validationOwner.Validation.ValidationCompleted -= Validation_ValidationCompleted;
                _validationOwner.LOV.LOVValidationFailed -= Lov_LOVValidationFailed;
            }

            _validationOwner = manager;
            if (_validationOwner != null)
            {
                _validationOwner.Validation.ValidationFailed += Validation_ValidationFailed;
                _validationOwner.Validation.ValidationCompleted += Validation_ValidationCompleted;
                _validationOwner.LOV.LOVValidationFailed += Lov_LOVValidationFailed;
            }
        }

        private void ResetValidationState()
        {
            _fieldValidationStates.Clear();
            ClearFieldErrors();
            UpdateValidationSummarySurface();
        }

        private void RefreshValidationState()
        {
            if (ViewState.IsQueryMode)
            {
                ResetValidationState();
                return;
            }

            if (_formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName) || !_formsHost.IsBlockRegistered(ManagerBlockName))
            {
                ResetValidationState();
                return;
            }

            var record = BuildValidationRecord(GetCurrentRecord());
            if (record == null)
            {
                ResetValidationState();
                return;
            }

            ApplyRecordValidationResult(_formsHost!.ValidateBlockRecord(ManagerBlockName, record, ValidationTiming.Manual));
        }

        private void ApplyRecordValidationResult(RecordValidationResult? result)
        {
            _fieldValidationStates.Clear();

            foreach (var itemResult in result?.ItemResults ?? Enumerable.Empty<KeyValuePair<string, ItemValidationResult>>())
            {
                ValidationSurfaceMessage? message = BuildValidationSurfaceMessage(itemResult.Value);
                if (message != null)
                {
                    _fieldValidationStates[itemResult.Key] = message;
                }
            }

            ShowFieldErrors(_fieldValidationStates);
            UpdateValidationSummarySurface(result);
        }

        private static Dictionary<string, object>? BuildValidationRecord(object? record)
        {
            if (record == null)
            {
                return null;
            }

            return record.GetType()
                .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                .Where(property => property.CanRead)
                .ToDictionary(property => property.Name, property => property.GetValue(record), StringComparer.OrdinalIgnoreCase);
        }

        private void Validation_ValidationFailed(object? sender, ValidationFailedEventArgs e)
        {
            if (!string.Equals(e.BlockName, ManagerBlockName, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(e.ItemName))
            {
                return;
            }

            SetFieldError(
                e.ItemName,
                e.Result?.ErrorMessage ?? "Validation failed.",
                e.Result?.Severity ?? ValidationSeverity.Error);
        }

        private void Validation_ValidationCompleted(object? sender, ValidationCompletedEventArgs e)
        {
            if (!string.Equals(e.BlockName, ManagerBlockName, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(e.ItemName))
            {
                return;
            }

            if (e.RulesFailed == 0)
            {
                ClearFieldError(e.ItemName);
            }
        }

        private void Lov_LOVValidationFailed(object? sender, LOVValidationEventArgs e)
        {
            if (!string.Equals(e.BlockName, ManagerBlockName, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(e.FieldName))
            {
                return;
            }

            SetFieldError(e.FieldName, e.ErrorMessage, ValidationSeverity.Error);
        }

        private void SetFieldError(string fieldName, string? message, ValidationSeverity severity = ValidationSeverity.Error)
        {
            if (string.IsNullOrWhiteSpace(fieldName) || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            _fieldValidationStates[fieldName] = new ValidationSurfaceMessage(message, severity);
            ShowFieldErrors(_fieldValidationStates);
            UpdateValidationSummarySurface();
        }

        private void ClearFieldError(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                return;
            }

            if (_fieldValidationStates.Remove(fieldName))
            {
                ShowFieldErrors(_fieldValidationStates);
                UpdateValidationSummarySurface();
            }
        }

        public void ShowFieldErrors(IReadOnlyDictionary<string, string> errors)
        {
            var mapped = errors?.ToDictionary(
                item => item.Key,
                item => new ValidationSurfaceMessage(item.Value, ValidationSeverity.Error),
                StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, ValidationSurfaceMessage>(StringComparer.OrdinalIgnoreCase);

            ShowFieldErrors(mapped);
        }

        private void ShowFieldErrors(IReadOnlyDictionary<string, ValidationSurfaceMessage> errors)
        {
            foreach (var editor in _fieldEditors)
            {
                ValidationSurfaceMessage? state = errors != null && errors.TryGetValue(editor.Key, out var value) ? value : null;
                ApplyFieldState(editor.Key, editor.Value, state);
            }
        }

        public void ClearFieldErrors()
        {
            foreach (var editor in _fieldEditors)
            {
                ApplyFieldState(editor.Key, editor.Value, null);
            }
        }

        private void ApplyFieldState(string fieldName, Control editor, ValidationSurfaceMessage? state)
        {
            ApplyFieldError(editor, state);

            if (_fieldRows.TryGetValue(fieldName, out var rowPanel))
            {
                rowPanel.ShowTitleLine = state != null;
                rowPanel.TitleLineColor = state == null ? Color.Gray : GetValidationSummaryColor(state.Severity);
                rowPanel.ToolTipText = state?.Message ?? string.Empty;
                rowPanel.TooltipType = state == null ? ToolTipType.Default : MapValidationToolTipType(state.Severity);
                rowPanel.Invalidate();
            }

            if (_fieldLabels.TryGetValue(fieldName, out var label))
            {
                label.UseThemeColors = state == null;
                label.ForeColor = state == null ? ForeColor : GetValidationSummaryColor(state.Severity);
                label.ToolTipText = state?.Message ?? string.Empty;
                label.TooltipType = state == null ? ToolTipType.Default : MapValidationToolTipType(state.Severity);
                label.Text = string.IsNullOrWhiteSpace(label.Text)
                    ? fieldName
                    : label.Text;
            }

            if (_fieldStateLabels.TryGetValue(fieldName, out var stateLabel))
            {
                stateLabel.Visible = state != null;
                stateLabel.UseThemeColors = state == null;
                stateLabel.ForeColor = state == null ? ForeColor : GetValidationChipTextColor(state.Severity);
                stateLabel.BackColor = state == null ? SystemColors.Control : GetValidationChipBackColor(state.Severity);
                stateLabel.ToolTipText = state?.Message ?? string.Empty;
                stateLabel.TooltipType = state == null ? ToolTipType.Default : MapValidationToolTipType(state.Severity);
                stateLabel.Text = state == null ? string.Empty : GetValidationSeverityBadgeText(state.Severity);
            }
        }

        private static void ApplyFieldError(Control editor, ValidationSurfaceMessage? state)
        {
            bool hasError = state != null && (state.Severity == ValidationSeverity.Error || state.Severity == ValidationSeverity.Critical);
            string message = state?.Message ?? string.Empty;

            if (editor is BaseControl baseControl)
            {
                baseControl.ErrorText = hasError ? message : string.Empty;
                baseControl.HasError = hasError;
                baseControl.ToolTipText = message;
                baseControl.TooltipType = state == null ? ToolTipType.Default : MapValidationToolTipType(state.Severity);
                return;
            }

            editor.BackColor = state == null
                ? System.Drawing.SystemColors.Window
                : GetValidationBackgroundColor(state.Severity);
        }

        private void UpdateValidationSummarySurface(RecordValidationResult? result = null)
        {
            if (_validationSummaryPanel == null ||
                _validationSummaryHeadlineLabel == null ||
                _validationSummaryDetailsLabel == null ||
                _validationSummarySeverityLabel == null ||
                _validationSummaryHintLabel == null)
            {
                return;
            }

            if (ViewState.IsQueryMode)
            {
                _validationSummaryPanel.Visible = false;
                _validationSummaryHeadlineLabel.Text = string.Empty;
                _validationSummaryDetailsLabel.Text = string.Empty;
                _validationSummarySeverityLabel.Text = string.Empty;
                _validationSummarySeverityLabel.Visible = false;
                _validationSummaryHintLabel.Text = string.Empty;
                _validationSummaryHintLabel.Visible = false;
                return;
            }

            var summary = BuildValidationSummary(result);
            _validationSummaryHeadlineLabel.Text = summary.Headline;
            _validationSummaryDetailsLabel.Text = summary.Details;
            _validationSummaryHintLabel.Text = summary.Hint;
            _validationSummaryHeadlineLabel.UseThemeColors = false;
            _validationSummaryHeadlineLabel.ForeColor = GetValidationSummaryColor(summary.Severity);
            _validationSummaryHeadlineLabel.TooltipType = MapValidationToolTipType(summary.Severity);
            _validationSummaryDetailsLabel.UseThemeColors = false;
            _validationSummaryDetailsLabel.ForeColor = summary.HasContent ? SystemColors.ControlText : ForeColor;
            _validationSummaryDetailsLabel.TooltipType = MapValidationToolTipType(summary.Severity);
            _validationSummaryHintLabel.UseThemeColors = false;
            _validationSummaryHintLabel.ForeColor = GetValidationSummaryColor(summary.Severity);
            _validationSummaryHintLabel.TooltipType = MapValidationToolTipType(summary.Severity);
            _validationSummaryHintLabel.Visible = !string.IsNullOrWhiteSpace(summary.Hint);
            _validationSummarySeverityLabel.Text = summary.SeverityBadge;
            _validationSummarySeverityLabel.BackColor = GetValidationChipBackColor(summary.Severity);
            _validationSummarySeverityLabel.ForeColor = GetValidationChipTextColor(summary.Severity);
            _validationSummarySeverityLabel.TooltipType = MapValidationToolTipType(summary.Severity);
            _validationSummarySeverityLabel.Visible = !string.IsNullOrWhiteSpace(summary.SeverityBadge);
            _validationSummaryPanel.UseThemeColors = !summary.HasContent;
            _validationSummaryPanel.BackColor = summary.HasContent ? GetValidationSummaryBackgroundColor(summary.Severity) : SystemColors.Control;
            _validationSummaryPanel.ToolTipText = summary.Details;
            _validationSummaryPanel.TooltipType = MapValidationToolTipType(summary.Severity);
            _validationSummaryPanel.ShowTitleLine = summary.HasContent;
            _validationSummaryPanel.TitleLineColor = GetValidationSummaryColor(summary.Severity);
            _validationSummaryPanel.Visible = summary.HasContent;
        }

        private ValidationSummaryInfo BuildValidationSummary(RecordValidationResult? result)
        {
            var messages = new List<string>();
            int errorCount = 0;
            int warningCount = 0;
            int infoCount = 0;
            ValidationSeverity summarySeverity = ValidationSeverity.Info;

            if (result?.ItemResults != null)
            {
                foreach (var itemResult in result.ItemResults)
                {
                    foreach (var ruleResult in itemResult.Value.RuleResults ?? Enumerable.Empty<ValidationRuleResult>())
                    {
                        if (string.IsNullOrWhiteSpace(ruleResult.ErrorMessage))
                        {
                            continue;
                        }

                        if (ruleResult.Severity == ValidationSeverity.Warning)
                        {
                            warningCount++;
                            if (summarySeverity != ValidationSeverity.Error && summarySeverity != ValidationSeverity.Critical)
                            {
                                summarySeverity = ValidationSeverity.Warning;
                            }
                        }
                        else if (ruleResult.Severity == ValidationSeverity.Error || ruleResult.Severity == ValidationSeverity.Critical)
                        {
                            errorCount++;
                            summarySeverity = ValidationSeverity.Error;
                        }
                        else
                        {
                            infoCount++;
                        }

                        messages.Add($"- {GetValidationSeverityLabel(ruleResult.Severity)} | {itemResult.Key}: {ruleResult.ErrorMessage}");
                    }
                }
            }

            if (messages.Count == 0 && _fieldValidationStates.Count > 0)
            {
                foreach (var item in _fieldValidationStates)
                {
                    if (item.Value.Severity == ValidationSeverity.Warning)
                    {
                        warningCount++;
                        if (summarySeverity != ValidationSeverity.Error && summarySeverity != ValidationSeverity.Critical)
                        {
                            summarySeverity = ValidationSeverity.Warning;
                        }
                    }
                    else if (item.Value.Severity == ValidationSeverity.Error || item.Value.Severity == ValidationSeverity.Critical)
                    {
                        errorCount++;
                        summarySeverity = ValidationSeverity.Error;
                    }
                    else
                    {
                        infoCount++;
                    }

                    messages.Add($"- {GetValidationSeverityLabel(item.Value.Severity)} | {item.Key}: {item.Value.Message}");
                }
            }

            if (messages.Count == 0)
            {
                return new ValidationSummaryInfo(string.Empty, string.Empty, string.Empty, string.Empty, ValidationSeverity.Info);
            }

            string headline = BuildValidationHeadline(errorCount, warningCount, infoCount);
            string details = string.Join(Environment.NewLine, messages.Take(5));
            if (messages.Count > 5)
            {
                details += Environment.NewLine + $"- {messages.Count - 5} additional validation message(s) hidden.";
            }

            return new ValidationSummaryInfo(
                headline,
                details,
                BuildValidationHint(errorCount, warningCount, infoCount),
                GetValidationSeverityBadgeText(summarySeverity),
                summarySeverity);
        }

        private static ValidationSurfaceMessage? BuildValidationSurfaceMessage(ItemValidationResult? itemResult)
        {
            if (itemResult?.RuleResults == null)
            {
                return null;
            }

            ValidationRuleResult? bestResult = itemResult.RuleResults
                .Where(rule => !rule.IsValid && !string.IsNullOrWhiteSpace(rule.ErrorMessage))
                .OrderByDescending(rule => GetValidationSeverityRank(rule.Severity))
                .ThenBy(rule => rule.RuleName, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            return bestResult == null
                ? null
                : new ValidationSurfaceMessage(bestResult.ErrorMessage, bestResult.Severity);
        }

        private static int GetValidationSeverityRank(ValidationSeverity severity)
        {
            return severity switch
            {
                ValidationSeverity.Critical => 4,
                ValidationSeverity.Error => 3,
                ValidationSeverity.Warning => 2,
                ValidationSeverity.Info => 1,
                _ => 0
            };
        }

        private static string BuildValidationHeadline(int errorCount, int warningCount, int infoCount)
        {
            var parts = new List<string>();

            if (errorCount > 0)
            {
                parts.Add($"{errorCount} blocking issue(s)");
            }

            if (warningCount > 0)
            {
                parts.Add($"{warningCount} warning(s)");
            }

            if (infoCount > 0)
            {
                parts.Add($"{infoCount} note(s)");
            }

            return string.Join(", ", parts) + " on the current record.";
        }

        private static string GetValidationSeverityLabel(ValidationSeverity severity)
        {
            return severity switch
            {
                ValidationSeverity.Warning => "Warning",
                ValidationSeverity.Critical => "Critical",
                ValidationSeverity.Info => "Info",
                _ => "Error"
            };
        }

        private static string GetValidationSeverityBadgeText(ValidationSeverity severity)
        {
            return severity switch
            {
                ValidationSeverity.Warning => "WARNING",
                ValidationSeverity.Critical => "CRITICAL",
                ValidationSeverity.Info => "INFO",
                _ => "ERROR"
            };
        }

        private static string BuildValidationHint(int errorCount, int warningCount, int infoCount)
        {
            if (errorCount > 0)
            {
                return "Fix the blocking issues before commit or record navigation continues.";
            }

            if (warningCount > 0)
            {
                return "Warnings do not always block the workflow, but they should be reviewed before you continue.";
            }

            if (infoCount > 0)
            {
                return "Review the validation notes for extra context on the current record.";
            }

            return string.Empty;
        }

        private static System.Drawing.Color GetValidationSummaryColor(ValidationSeverity severity)
        {
            return severity switch
            {
                ValidationSeverity.Warning => System.Drawing.Color.DarkOrange,
                ValidationSeverity.Critical => System.Drawing.Color.Firebrick,
                ValidationSeverity.Error => System.Drawing.Color.Firebrick,
                ValidationSeverity.Info => System.Drawing.Color.SteelBlue,
                _ => System.Drawing.Color.Black
            };
        }

        private static Color GetValidationSummaryBackgroundColor(ValidationSeverity severity)
        {
            return severity switch
            {
                ValidationSeverity.Warning => Color.FromArgb(255, 247, 230),
                ValidationSeverity.Critical => Color.FromArgb(255, 233, 233),
                ValidationSeverity.Error => Color.FromArgb(255, 239, 239),
                ValidationSeverity.Info => Color.FromArgb(236, 245, 255),
                _ => SystemColors.Control
            };
        }

        private static Color GetValidationChipBackColor(ValidationSeverity severity)
        {
            return severity switch
            {
                ValidationSeverity.Warning => Color.FromArgb(255, 237, 204),
                ValidationSeverity.Critical => Color.FromArgb(245, 214, 214),
                ValidationSeverity.Error => Color.FromArgb(255, 222, 222),
                ValidationSeverity.Info => Color.FromArgb(215, 232, 250),
                _ => SystemColors.Control
            };
        }

        private static Color GetValidationChipTextColor(ValidationSeverity severity)
        {
            return severity switch
            {
                ValidationSeverity.Warning => Color.FromArgb(128, 73, 0),
                ValidationSeverity.Critical => Color.FromArgb(120, 20, 20),
                ValidationSeverity.Error => Color.FromArgb(145, 32, 32),
                ValidationSeverity.Info => Color.FromArgb(21, 79, 140),
                _ => SystemColors.ControlText
            };
        }

        private static ToolTipType MapValidationToolTipType(ValidationSeverity severity)
        {
            return severity switch
            {
                ValidationSeverity.Warning => ToolTipType.Warning,
                ValidationSeverity.Critical => ToolTipType.Error,
                ValidationSeverity.Error => ToolTipType.Validation,
                ValidationSeverity.Info => ToolTipType.Info,
                _ => ToolTipType.Default
            };
        }

        private static Color GetValidationBackgroundColor(ValidationSeverity severity)
        {
            return severity switch
            {
                ValidationSeverity.Warning => Color.FromArgb(255, 248, 225),
                ValidationSeverity.Info => Color.FromArgb(232, 244, 252),
                ValidationSeverity.Critical => Color.FromArgb(255, 235, 235),
                ValidationSeverity.Error => Color.MistyRose,
                _ => SystemColors.Window
            };
        }
    }
}