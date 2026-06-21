using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;

public sealed class WinFormParameterListPanel : WinFormFormsFeatureControl
{
    public WinFormParameterListPanel(IBeepFormsHost host) : base(host)
    {
    }

    public ParameterList Create(string name) => Host.CreateParameterList(name);

    public void Set(string listName, string parameterName, object? value) =>
        Host.SetParameter(listName, parameterName, value);

    public object? Get(string listName, string parameterName) =>
        Host.GetParameter(listName, parameterName);

    public bool Remove(string listName, string parameterName) =>
        Host.RemoveParameter(listName, parameterName);

    public bool Destroy(string name) => Host.DestroyParameterList(name);
}
