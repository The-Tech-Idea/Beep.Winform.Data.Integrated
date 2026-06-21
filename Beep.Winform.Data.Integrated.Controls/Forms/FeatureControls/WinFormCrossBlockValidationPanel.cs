using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.UOWManager.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;

public sealed class WinFormCrossBlockValidationPanel : WinFormFormsFeatureControl
{
    public WinFormCrossBlockValidationPanel(IBeepFormsHost host)
        : base(host)
    {
    }

    public void Register(CrossBlockValidationRule rule) =>
        Host.RegisterCrossBlockRule(rule);

    public bool Unregister(string ruleName) =>
        Host.UnregisterCrossBlockRule(ruleName);

    public IReadOnlyList<string> ValidateRules() =>
        Host.ValidateCrossBlock() ?? [];
}
