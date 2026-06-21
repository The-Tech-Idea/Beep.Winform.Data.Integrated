using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;

public sealed class WinFormTimerPanel : WinFormFormsFeatureControl
{
    public WinFormTimerPanel(IBeepFormsHost host) : base(host)
    {
    }

    public TimerDefinition Create(
        string name,
        TimeSpan interval,
        bool repeating = false) =>
        Host.CreateTimer(name, interval, repeating);

    public bool Delete(string name) => Host.DeleteTimer(name);

    public IReadOnlyList<TimerDefinition> GetTimers() => Host.GetTimers();
}
