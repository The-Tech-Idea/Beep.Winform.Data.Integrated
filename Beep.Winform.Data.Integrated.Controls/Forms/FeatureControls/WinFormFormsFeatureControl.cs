using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Winform.Controls;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;

public abstract class WinFormFormsFeatureControl : UserControl
{
    protected WinFormFormsFeatureControl(
        IBeepFormsHost host,
        string? blockName = null)
    {
        Host = host ?? throw new ArgumentNullException(nameof(host));
        BlockName = string.IsNullOrWhiteSpace(blockName)
            ? null
            : blockName.Trim();
        Dock = DockStyle.Fill;
    }

    protected IBeepFormsHost Host { get; }
    protected string? BlockName { get; }

    protected string RequireBlockName() =>
        BlockName ?? throw new InvalidOperationException(
            "This feature control requires a block name.");

    protected static BeepButton CreateButton(string text, EventHandler handler)
    {
        var button = new BeepButton
        {
            Text = text,
            Width = 100,
            Height = 34,
            UseThemeColors = true
        };
        button.Click += handler;
        return button;
    }
}
