using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;

public sealed class WinFormMultiFormPanel : WinFormFormsFeatureControl
{
    public WinFormMultiFormPanel(IBeepFormsHost host) : base(host)
    {
    }

    public Task<bool> OpenAsync(
        string formName,
        Dictionary<string, object>? parameters = null,
        FormCallMode mode = FormCallMode.Modeless,
        CancellationToken ct = default) =>
        Host.CallFormAsync(formName, parameters, mode, ct);

    public Task<bool> ReturnAsync(
        object? returnData = null,
        CancellationToken ct = default) =>
        Host.ReturnToCallerAsync(returnData, ct);

    public void Post(
        string targetForm,
        string messageType,
        object? payload = null) =>
        Host.PostMessage(targetForm, messageType, payload);
}
