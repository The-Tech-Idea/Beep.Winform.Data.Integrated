using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.ListBoxs;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Helpers
{
    internal static class BeepFormsDialogService
    {
        public static string? PromptText(
            IWin32Window? owner,
            string title,
            string caption,
            string initialValue,
            bool allowEmpty)
        {
            string? result = null;

            using var inputBox = new BeepTextBox
            {
                Dock = DockStyle.Top,
                Height = BeepLayoutMetrics.TextRowHeight.ScaleValue((Control)owner),
                Text = initialValue ?? string.Empty,
                Margin = new Padding(0, 0, 0, BeepLayoutMetrics.ButtonGap)
            };

            using var contentHost = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };
            contentHost.Controls.Add(inputBox);

            bool accepted = ShowDialog(
                owner,
                title,
                caption,
                contentHost,
                primaryText: "OK",
                secondaryText: "Cancel",
                tertiaryText: null,
                clientSize: BeepLayoutMetrics.DialogSmall.ScaleSize((Control)owner),
                onShown: () => inputBox.Focus(),
                configureSurface: surface =>
                {
                    surface.SetTone(allowEmpty ? BeepFormsDialogTone.Info : BeepFormsDialogTone.Warning, allowEmpty ? "INPUT" : "REQUIRED");
                    surface.DetailsText = allowEmpty
                        ? "Leave the value blank to use the default behavior."
                        : "A value is required before the dialog can continue.";
                    surface.PrimaryButtonEnabled = allowEmpty || !string.IsNullOrWhiteSpace(inputBox.Text);
                    inputBox.TextChanged += (_, _) => surface.PrimaryButtonEnabled = allowEmpty || !string.IsNullOrWhiteSpace(inputBox.Text);
                },
                handlePrimary: () =>
                {
                    result = inputBox.Text;
                    return true;
                });

            return accepted ? result : null;
        }

        public static T? PickItem<T>(
            IWin32Window? owner,
            string title,
            string caption,
            IReadOnlyList<T> items,
            Func<T, string> displayText,
            string primaryText = "Select")
            where T : class
        {
            if (items == null || items.Count == 0)
            {
                return null;
            }

            T? selected = null;

            using var listBox = new BeepListBox
            {
                Dock = DockStyle.Fill,
                UseThemeColors = true,
                ListItems = new BindingList<SimpleItem>(items.Select(item => new SimpleItem
                {
                    Text = displayText(item),
                    Name = displayText(item),
                    Value = displayText(item),
                    Item = item
                }).ToList())
            };

            bool accepted = ShowDialog(
                owner,
                title,
                caption,
                listBox,
                primaryText,
                "Cancel",
                tertiaryText: null,
                clientSize: BeepLayoutMetrics.DialogMedium.ScaleSize((Control)owner),
                configureSurface: surface =>                {
                    surface.SetTone(BeepFormsDialogTone.Info, "SELECT");
                    surface.DetailsText = "Pick a single item to continue with the current workflow action.";
                    surface.PrimaryButtonEnabled = false;
                    listBox.SelectedItemChanged += (_, args) =>
                    {
                        selected = args.SelectedItem?.Item as T;
                        surface.PrimaryButtonEnabled = selected != null;
                    };
                },
                handlePrimary: () => selected != null);

            return accepted ? selected : null;
        }

        public static AlertResult ShowAlert(
            IWin32Window? owner,
            string title,
            string message,
            AlertStyle style,
            string button1Text,
            string? button2Text,
            string? button3Text)
        {
            AlertResult result = AlertResult.None;

            using var messageLabel = new BeepLabel
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                WordWrap = true,
                Text = message,
                UseThemeColors = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            ShowDialog(
                owner,
                title,
                BuildAlertCaption(style),
                messageLabel,
                primaryText: NormalizeButtonText(button1Text, "OK"),
                secondaryText: string.IsNullOrWhiteSpace(button2Text) ? null : button2Text,
                tertiaryText: string.IsNullOrWhiteSpace(button3Text) ? null : button3Text,
                clientSize: BeepLayoutMetrics.DialogSmall.ScaleSize((Control)owner),
                configureSurface: surface =>
                {
                    surface.SetSeverity(style);
                    surface.DetailsText = BuildAlertHint(style);
                },
                handlePrimary: () =>
                {
                    result = AlertResult.Button1;
                    return true;
                },
                handleSecondary: button2Text == null
                    ? null
                    : () =>
                    {
                        result = AlertResult.Button2;
                        return true;
                    },
                handleTertiary: button3Text == null
                    ? null
                    : () =>
                    {
                        result = AlertResult.Button3;
                        return true;
                    });

            return result;
        }

        public static void ShowList<T>(
            IWin32Window? owner,
            string title,
            string caption,
            IReadOnlyList<T> items,
            Func<T, string> displayText,
            string primaryText = "Close")
        {
            if (items == null)
            {
                return;
            }

            Control content;
            if (items.Count == 0)
            {
                content = new BeepLabel
                {
                    Dock = DockStyle.Fill,
                    Multiline = true,
                    WordWrap = true,
                    Text = "No items are currently available.",
                    TextAlign = ContentAlignment.MiddleLeft,
                    UseThemeColors = true
                };
            }
            else
            {
                content = new BeepListBox
                {
                    Dock = DockStyle.Fill,
                    UseThemeColors = true,
                    ListItems = new BindingList<SimpleItem>(items.Select(item => new SimpleItem
                    {
                        Text = displayText(item),
                        Name = displayText(item),
                        Value = displayText(item),
                        Item = item
                    }).ToList())
                };
            }

            using (content)
            {
                ShowDialog(
                    owner,
                    title,
                    caption,
                    content,
                    primaryText: NormalizeButtonText(primaryText, "Close"),
                    secondaryText: null,
                    tertiaryText: null,
                    clientSize: BeepLayoutMetrics.DialogMedium.ScaleSize((Control)owner),
                    configureSurface: surface =>
                    {
                        surface.SetTone(BeepFormsDialogTone.Info, "LIST");
                        surface.DetailsText = items.Count == 0
                            ? "The workflow does not have any items to show right now."
                            : $"Showing {items.Count} item(s) from the current workflow context.";
                    },
                    handlePrimary: () => true);
            }
        }

        private static bool ShowDialog(
            IWin32Window? owner,
            string title,
            string caption,
            Control content,
            string primaryText,
            string? secondaryText,
            string? tertiaryText,
            Size clientSize,
            Action? onShown = null,
            Action<BeepFormsDialogSurface>? configureSurface = null,
            Func<bool>? handlePrimary = null,
            Func<bool>? handleSecondary = null,
            Func<bool>? handleTertiary = null)
        {
            bool accepted = false;

            using Form dialog = new()
            {
                Text = string.IsNullOrWhiteSpace(title) ? "Workflow Dialog" : title,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false,
                ClientSize = clientSize,
                ShowInTaskbar = false
            };

            using var surface = new BeepFormsDialogSurface
            {
                Dock = DockStyle.Fill,
                CaptionText = caption,
                Content = content,
                PrimaryButtonText = primaryText,
                SecondaryButtonText = secondaryText,
                TertiaryButtonText = tertiaryText
            };

            surface.PrimaryActionRequested += (_, _) =>
            {
                if (handlePrimary?.Invoke() != false)
                {
                    accepted = true;
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                }
            };

            surface.SecondaryActionRequested += (_, _) =>
            {
                if (handleSecondary == null)
                {
                    dialog.DialogResult = DialogResult.Cancel;
                    dialog.Close();
                    return;
                }

                if (handleSecondary())
                {
                    accepted = true;
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                }
            };

            surface.TertiaryActionRequested += (_, _) =>
            {
                if (handleTertiary == null)
                {
                    dialog.DialogResult = DialogResult.Cancel;
                    dialog.Close();
                    return;
                }

                if (handleTertiary())
                {
                    accepted = true;
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                }
            };

            dialog.Controls.Add(surface);
            configureSurface?.Invoke(surface);
            dialog.Shown += (_, _) => onShown?.Invoke();

            DialogResult result = owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
            return accepted || result == DialogResult.OK;
        }

        private static string BuildAlertCaption(AlertStyle style)
        {
            return style switch
            {
                AlertStyle.Stop => "Blocking workflow alert",
                AlertStyle.Caution => "Review the current workflow state",
                AlertStyle.Question => "Choose how to continue",
                _ => "Workflow notification"
            };
        }

        private static string BuildAlertHint(AlertStyle style)
        {
            return style switch
            {
                AlertStyle.Stop => "Resolve the blocking condition before retrying the workflow step.",
                AlertStyle.Caution => "Review the warning carefully before you continue.",
                AlertStyle.Question => "Choose how the workflow should continue.",
                _ => "Review the message and continue when ready."
            };
        }

        private static string NormalizeButtonText(string? value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }
}

// BeepFormsDialogTone and BeepFormsDialogSurface moved to BeepFormsDialogSurface.cs