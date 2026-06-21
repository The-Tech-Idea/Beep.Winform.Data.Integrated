namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public interface IWinFormFormsFactory
{
    Task<bool> ShowModalAsync(
        string formName,
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken ct = default);

    Task<bool> ShowModelessAsync(
        string formName,
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken ct = default);

    Task<bool> ReplaceCurrentAsync(
        string formName,
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken ct = default);

    Task<bool> ReturnToCallerAsync(
        object? returnData,
        CancellationToken ct = default);
}
