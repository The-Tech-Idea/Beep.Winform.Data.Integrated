using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Base;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Helpers;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms;

[ToolboxItem(true)]
[ToolboxBitmap(typeof(BeepFormsCommandBar))]
[Category("Beep Controls")]
[DisplayName("Beep Forms Command Bar")]
[Description("Standalone form-level command surface for BeepForms block switching and sync.")]
[Designer("TheTechIdea.Beep.Winform.Controls.Design.Server.Designers.BeepFormsCommandBarDesigner, TheTechIdea.Beep.Winform.Controls.Design.Server")]
public sealed class BeepFormsCommandBar : BeepFormsShelfBase
{
    private readonly FlowLayoutPanel _commandPanel;
    private readonly Dictionary<BeepFormsCommandBarButtons, BeepButton> _buttons = new();
    private BeepButton? _blockSelectorButton;
    private BeepFormsCommandBarButtons _commandButtons = BeepFormsCommandBarButtons.All;
    private FlowDirection _commandFlowDirection = FlowDirection.LeftToRight;

    public BeepFormsCommandBar()
    {
        UseThemeColors = true;
        Padding = new Padding(0);
        MinimumSize = new Size(0, BeepLayoutMetrics.ButtonToolbar.ScaleSize(this).Height + 4);
        Height = BeepLayoutMetrics.ButtonToolbar.ScaleSize(this).Height + 4;

        _commandPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            AutoScroll = true,
            Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this),
            FlowDirection = FlowDirection.LeftToRight
        };

        Controls.Add(_commandPanel);
        RefreshCommandStrip();
    }

    [Browsable(true), Category("Command Bar")]
    [Description("Select which top-level form command buttons are shown.")]
    [DefaultValue(BeepFormsCommandBarButtons.All)]
    public BeepFormsCommandBarButtons CommandButtons
    {
        get => _commandButtons;
        set { if (_commandButtons == value) return; _commandButtons = value; RefreshCommandStrip(); }
    }

    [Browsable(true), Category("Command Bar")]
    [Description("Controls the flow direction used by the form command button row.")]
    [DefaultValue(FlowDirection.LeftToRight)]
    public FlowDirection CommandFlowDirection
    {
        get => _commandFlowDirection;
        set { if (_commandFlowDirection == value) return; _commandFlowDirection = value; RefreshCommandStrip(); }
    }

    private void RefreshCommandStrip()
    {
        _commandPanel.SuspendLayout();
        _commandPanel.Controls.Clear();
        _buttons.Clear();
        _commandPanel.FlowDirection = CommandFlowDirection;
        BuildBlockSelectorButton();

        foreach (var s in BeepFormsCommandBarStrategies.All)
        {
            if (!CommandButtons.HasFlag(s.Flag)) continue;

            var btn = new BeepButton
            {
                Width = GetButtonWidth(s.Caption, s.MinWidth),
                Height = 28,
                Margin = new Padding(0, 0, 8, 0),
                Text = s.Caption,
                Theme = Theme,
                ShowShadow = false,
                UseThemeColors = true
            };
            btn.Click += (_, _) => ExecuteStrategy(s);
            _buttons[s.Flag] = btn;
            _commandPanel.Controls.Add(btn);
        }

        _commandPanel.Visible = _commandPanel.Controls.Count > 0;
        _commandPanel.ResumeLayout(false);
        OnFormsHostChanged();
    }

    private static int GetButtonWidth(string caption, int minWidth)
    {
        int w = TextRenderer.MeasureText(caption, SystemFonts.DefaultFont).Width + 28;
        return Math.Max(minWidth, w);
    }

    private async void ExecuteStrategy(CommandButtonStrategy s)
    {
        if (FormsHost == null || !s.CanExecute(FormsHost)) return;
        try { await s.ExecuteAsync(FormsHost); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[CommandBar.{s.Flag}] {ex.Message}"); }
    }

    private void BuildBlockSelectorButton()
    {
        _blockSelectorButton = null;
        if (!CommandButtons.HasFlag(BeepFormsCommandBarButtons.BlockSelector)) return;

        _blockSelectorButton = new BeepButton
        {
            Width = 150, Height = 28, Margin = new Padding(0, 0, 8, 0),
            Text = "Select Block", Theme = Theme, ShowShadow = false,
            PopupMode = true, UseThemeColors = true
        };
        _blockSelectorButton.SelectedItemChanged += (_, e) =>
        {
            if (FormsHost == null) return;
            string? name = e.SelectedItem?.Item?.ToString() ?? e.SelectedItem?.Value?.ToString() ?? e.SelectedItem?.Text;
            if (!string.IsNullOrWhiteSpace(name)) FormsHost.SwitchToBlockAsync(name);
        };
        _commandPanel.Controls.Add(_blockSelectorButton);
    }

    protected override void OnFormsHostChanged()
    {
        base.OnFormsHostChanged();
        bool hasHost = FormsHost != null;
        bool hasActiveBlock = hasHost && !string.IsNullOrWhiteSpace(FormsHost!.ActiveBlockName);

        if (_blockSelectorButton != null)
        {
            var items = BuildBlockSelectorItems();
            _blockSelectorButton.ListItems = items;
            _blockSelectorButton.Enabled = hasHost && items.Count > 0;
            _blockSelectorButton.Text = hasActiveBlock ? $"Block: {FormsHost!.ActiveBlockName}" : "Select Block";
        }

        foreach (var (flag, btn) in _buttons)
        {
            var s = BeepFormsCommandBarStrategies.All.FirstOrDefault(x => x.Flag == flag);
            btn.Enabled = s != null && s.CanExecute(FormsHost!);

            if (flag == BeepFormsCommandBarButtons.ShowLOV)
            {
                var lovTitle = FormsHost?.ActiveItemName != null && FormsHost.ActiveBlockName != null
                    ? FormsHost.GetLov(FormsHost.ActiveBlockName, FormsHost.ActiveItemName)?.Title : null;
                btn.ToolTipText = !string.IsNullOrWhiteSpace(lovTitle) ? $"Show LOV: {lovTitle}" : "Show LOV";
            }

            if (flag == BeepFormsCommandBarButtons.ToggleGridView)
                btn.Text = BeepFormsCommandBarStrategies.GetGridToggleCaption(FormsHost!);
        }
    }

    private BindingList<SimpleItem> BuildBlockSelectorItems()
    {
        var items = new BindingList<SimpleItem>();
        if (FormsHost == null) return items;
        foreach (var b in FormsHost.Blocks)
            if (!string.IsNullOrWhiteSpace(b.BlockName))
                items.Add(new SimpleItem { Text = b.BlockName, Name = b.BlockName, Value = b.BlockName, Item = b.BlockName });
        return items;
    }
}
