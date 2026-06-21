using TheTechIdea.Beep.Editor.Forms.Hosts;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;

public sealed class WinFormSequencePanel : WinFormFormsFeatureControl
{
    public WinFormSequencePanel(IBeepFormsHost host) : base(host)
    {
    }

    public void Create(
        string name,
        long startValue = 1,
        long incrementBy = 1) =>
        Host.CreateSequence(name, startValue, incrementBy);

    public long NextValue(string name) => Host.GetNextSequence(name);

    public long PeekNextValue(string name) => Host.PeekNextSequence(name);

    public void Reset(string name, long startValue = 1) =>
        Host.ResetSequence(name, startValue);

    public bool Drop(string name) =>
        Host.DropSequence(name);
}
