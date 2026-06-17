using System;
using System.Drawing;
using System.Windows.Forms;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Winform.Controls.Base;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Helpers;

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
        Padding = BeepLayoutMetrics.DialogPadding;
        ShowTitle = false;
        ShowTitleLine = true;
        UseThemeColors = true;

        _headerHost = new Panel
        {
            Dock = DockStyle.Top,
            Height = BeepLayoutMetrics.TextRowHeight + 9,
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
            Width = BeepLayoutMetrics.ButtonSmall.Width + 30,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = BeepLayoutMetrics.ContainerPadding,
            UseThemeColors = false,
            Visible = false
        };

        _detailsLabel = new BeepLabel
        {
            Dock = DockStyle.Top,
            Height = BeepLayoutMetrics.TextRowHeight - 11,
            Multiline = true,
            WordWrap = true,
            TextAlign = ContentAlignment.MiddleLeft,
            UseThemeColors = true,
            Visible = false
        };

        _contentHost = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, BeepLayoutMetrics.ButtonGap, 0, BeepLayoutMetrics.ButtonGap)
        };

        _buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = BeepLayoutMetrics.ButtonToolbar.Height + 10,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, BeepLayoutMetrics.SmallGap + 2, 0, 0)
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
            if (value == null) return;
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
