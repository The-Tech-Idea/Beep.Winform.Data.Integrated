using TheTechIdea.Beep.Editor.Forms.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public partial class WinFormFormHost
{
    public async Task<bool> CallFormAsync(
        string formName,
        Dictionary<string, object>? parameters = null,
        FormCallMode mode = FormCallMode.Modal,
        CancellationToken ct = default)
    {
        var factory = FormFactory ?? throw new InvalidOperationException(
            "A WinForms form factory must be assigned before opening another form.");
        var values = parameters ?? new Dictionary<string, object>();
        if (mode != FormCallMode.Modal)
        {
            return mode == FormCallMode.Modeless
                ? await OpenFormModelessAsync(formName, values, ct)
                : await NewFormAsync(formName, values, ct);
        }

        var engineTask = RequireManager().CallFormAsync(
            formName,
            values,
            FormCallMode.Modal,
            ct);
        await Task.Yield();
        if (engineTask.IsCompleted)
        {
            return await engineTask.ConfigureAwait(false);
        }

        if (!await factory.ShowModalAsync(formName, values, ct).ConfigureAwait(false))
        {
            return false;
        }

        return await engineTask.ConfigureAwait(false);
    }

    public async Task<bool> OpenFormModelessAsync(
        string formName,
        Dictionary<string, object>? parameters = null,
        CancellationToken ct = default)
    {
        var factory = FormFactory ?? throw new InvalidOperationException(
            "A WinForms form factory must be assigned before opening another form.");
        var values = parameters ?? new Dictionary<string, object>();
        if (!await RequireManager().OpenFormModelessAsync(formName, values)
                .ConfigureAwait(false))
        {
            return false;
        }

        return await factory.ShowModelessAsync(formName, values, ct)
            .ConfigureAwait(false);
    }

    public async Task<bool> NewFormAsync(
        string formName,
        Dictionary<string, object>? parameters = null,
        CancellationToken ct = default)
    {
        var factory = FormFactory ?? throw new InvalidOperationException(
            "A WinForms form factory must be assigned before replacing the current form.");
        var values = parameters ?? new Dictionary<string, object>();
        if (!await RequireManager().NewFormAsync(formName, values)
                .ConfigureAwait(false))
        {
            return false;
        }

        return await factory.ReplaceCurrentAsync(formName, values, ct)
            .ConfigureAwait(false);
    }

    public async Task<bool> ReturnToCallerAsync(
        object? returnData = null,
        CancellationToken ct = default)
    {
        if (!await RequireManager().ReturnToCallerAsync(returnData)
                .ConfigureAwait(false))
        {
            return false;
        }

        return FormFactory is null ||
               await FormFactory.ReturnToCallerAsync(returnData, ct)
                   .ConfigureAwait(false);
    }

    public void SetGlobalVariable(string name, object? value) =>
        RequireManager().SetGlobalVariable(name, value!);

    public object? GetGlobalVariable(string name) =>
        RequireManager().GetGlobalVariable(name);

    public object? GetFormParameter(string name) =>
        RequireManager().GetFormParameter(name);

    public bool SendParameterToForm(
        string targetFormName,
        string parameterName,
        object? value) =>
        RequireManager().SendParameterToForm(
            targetFormName,
            parameterName,
            value!);

    public void PostMessage(
        string targetForm,
        string messageType,
        object? payload = null) =>
        RequireManager().PostMessage(targetForm, messageType, payload);

    public void BroadcastMessage(
        string messageType,
        object? payload = null) =>
        RequireManager().BroadcastMessage(messageType, payload);
}
