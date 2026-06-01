using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Base;
using TheTechIdea.Beep.Winform.Controls.ListBoxs;
using TheTechIdea.Beep.Winform.Controls.Models;

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
                Height = 30,
                Text = initialValue ?? string.Empty,
                Margin = new Padding(0, 0, 0, 8)
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
                clientSize: new Size(520, 208),
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
                clientSize: new Size(560, 360),
                configureSurface: surface =>
                {
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
                clientSize: new Size(520, 228),
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
                    clientSize: new Size(560, 360),
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

    internal enum BeepFormsDialogTone
    {
        None,
        Info,
        Success,
        Warning,
        Error,
        Question
    }

    internal sealed class BeepFormsDialogSurface : BeepPanel
    {
        private readonly BeepLabel _captionLabel;
        private readonly BeepLabel _detailsLabel;
        private readonly BeepLabel _toneLabel;
        private readonly Panel _headerHost;
        private readonly Panel _contentHost;
        private readonly FlowLayoutPanel _buttonPanel;
        private readonly BeepButton _primaryButton;
        private readonly BeepButton _secondaryButton;
        private readonly BeepButton _tertiaryButton;

        public BeepFormsDialogSurface()
        {
            Dock = DockStyle.Fill;
            Padding = new Padding(12);
            ShowTitle = false;
            ShowTitleLine = true;
            UseThemeColors = true;

            _headerHost = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            _captionLabel = new BeepLabel
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                WordWrap = true,
                TextAlign = ContentAlignment.MiddleLeft,
                UseThemeColors = true
            };

            _toneLabel = new BeepLabel
            {
                Dock = DockStyle.Right,
                Width = 110,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(6, 0, 6, 0),
                UseThemeColors = false,
                Visible = false
            };

            _detailsLabel = new BeepLabel
            {
                Dock = DockStyle.Top,
                Height = 24,
                Multiline = true,
                WordWrap = true,
                TextAlign = ContentAlignment.MiddleLeft,
                UseThemeColors = true,
                Visible = false
            };

            _contentHost = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 8, 0, 8)
            };

            _buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 42,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(0, 6, 0, 0)
            };

            _primaryButton = CreateButton();
            _secondaryButton = CreateButton();
            _tertiaryButton = CreateButton();

            _primaryButton.Click += (_, _) => PrimaryActionRequested?.Invoke(this, EventArgs.Empty);
            _secondaryButton.Click += (_, _) => SecondaryActionRequested?.Invoke(this, EventArgs.Empty);
            _tertiaryButton.Click += (_, _) => TertiaryActionRequested?.Invoke(this, EventArgs.Empty);

            _buttonPanel.Controls.Add(_primaryButton);
            _buttonPanel.Controls.Add(_secondaryButton);
            _buttonPanel.Controls.Add(_tertiaryButton);

            _headerHost.Controls.Add(_captionLabel);
            _headerHost.Controls.Add(_toneLabel);

            Controls.Add(_contentHost);
            Controls.Add(_buttonPanel);
            Controls.Add(_detailsLabel);
            Controls.Add(_headerHost);
        }

        public event EventHandler? PrimaryActionRequested;
        public event EventHandler? SecondaryActionRequested;
        public event EventHandler? TertiaryActionRequested;

        public string CaptionText
        {
            get => _captionLabel.Text;
            set => _captionLabel.Text = value ?? string.Empty;
        }

        public string DetailsText
        {
            get => _detailsLabel.Text;
            set
            {
                string normalized = value?.Trim() ?? string.Empty;
                _detailsLabel.Text = normalized;
                _detailsLabel.Visible = !string.IsNullOrWhiteSpace(normalized);
            }
        }

        public Control? Content
        {
            get => _contentHost.Controls.Count > 0 ? _contentHost.Controls[0] : null;
            set
            {
                _contentHost.Controls.Clear();
                if (value == null)
                {
                    return;
                }

                value.Dock = DockStyle.Fill;
                _contentHost.Controls.Add(value);
            }
        }

        public string PrimaryButtonText
        {
            get => _primaryButton.Text;
            set => ApplyButtonText(_primaryButton, value);
        }

        public string? SecondaryButtonText
        {
            get => _secondaryButton.Visible ? _secondaryButton.Text : null;
            set => ApplyButtonText(_secondaryButton, value);
        }

        public string? TertiaryButtonText
        {
            get => _tertiaryButton.Visible ? _tertiaryButton.Text : null;
            set => ApplyButtonText(_tertiaryButton, value);
        }

        public bool PrimaryButtonEnabled
        {
            get => _primaryButton.Enabled;
            set => _primaryButton.Enabled = value;
        }

        public void SetTone(BeepFormsDialogTone tone, string? badgeText = null)
        {
            if (tone == BeepFormsDialogTone.None)
            {
                _toneLabel.Text = string.Empty;
                _toneLabel.Visible = false;
                TitleLineColor = Color.SeaGreen;
                return;
            }

            string normalizedBadge = string.IsNullOrWhiteSpace(badgeText)
                ? GetDefaultToneBadgeText(tone)
                : badgeText.Trim().ToUpperInvariant();

            _toneLabel.Text = normalizedBadge;
            _toneLabel.BackColor = GetToneBackgroundColor(tone);
            _toneLabel.ForeColor = GetToneForegroundColor(tone);
            _toneLabel.Visible = true;
            TitleLineColor = GetToneLineColor(tone);
        }

        public void SetSeverity(AlertStyle style)
        {
            SetTone(style switch
            {
                AlertStyle.Stop => BeepFormsDialogTone.Error,
                AlertStyle.Caution => BeepFormsDialogTone.Warning,
                AlertStyle.Question => BeepFormsDialogTone.Question,
                _ => BeepFormsDialogTone.Info
            });
        }

        private static BeepButton CreateButton()
        {
            return new BeepButton
            {
                Width = 96,
                ShowShadow = false,
                Visible = false,
                Margin = new Padding(8, 0, 0, 0)
            };
        }

        private static void ApplyButtonText(BeepButton button, string? value)
        {
            string normalized = value?.Trim() ?? string.Empty;
            button.Text = normalized;
            button.Visible = !string.IsNullOrWhiteSpace(normalized);
        }

        private static string GetDefaultToneBadgeText(BeepFormsDialogTone tone)
        {
            return tone switch
            {
                BeepFormsDialogTone.Success => "SUCCESS",
                BeepFormsDialogTone.Warning => "WARNING",
                BeepFormsDialogTone.Error => "ERROR",
                BeepFormsDialogTone.Question => "QUESTION",
                _ => "INFO"
            };
        }

        private static Color GetToneLineColor(BeepFormsDialogTone tone)
        {
            return tone switch
            {
                BeepFormsDialogTone.Success => Color.SeaGreen,
                BeepFormsDialogTone.Warning => Color.DarkOrange,
                BeepFormsDialogTone.Error => Color.Firebrick,
                BeepFormsDialogTone.Question => Color.SteelBlue,
                _ => Color.SteelBlue
            };
        }

        private static Color GetToneBackgroundColor(BeepFormsDialogTone tone)
        {
            return tone switch
            {
                BeepFormsDialogTone.Success => Color.FromArgb(221, 244, 232),
                BeepFormsDialogTone.Warning => Color.FromArgb(255, 237, 204),
                BeepFormsDialogTone.Error => Color.FromArgb(255, 222, 222),
                BeepFormsDialogTone.Question => Color.FromArgb(220, 235, 255),
                _ => Color.FromArgb(229, 239, 255)
            };
        }

        private static Color GetToneForegroundColor(BeepFormsDialogTone tone)
        {
            return tone switch
            {
                BeepFormsDialogTone.Success => Color.FromArgb(21, 87, 36),
                BeepFormsDialogTone.Warning => Color.FromArgb(128, 73, 0),
                BeepFormsDialogTone.Error => Color.FromArgb(145, 32, 32),
                BeepFormsDialogTone.Question => Color.FromArgb(21, 79, 140),
                _ => Color.FromArgb(21, 79, 140)
            };
        }
    }
}