using TheTechIdea.Beep.Editor.Forms.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public partial class WinFormFormHost
{
    public void SetMessage(string message, MessageLevel level = MessageLevel.Info) =>
        RequireManager().SetMessage(message, level);

    public void ClearMessage() =>
        RequireManager().ClearMessage();

    public Task<AlertResult> ShowAlertAsync(
        string title,
        string message,
        AlertStyle style = AlertStyle.None,
        string button1Text = "OK",
        string? button2Text = null,
        string? button3Text = null,
        CancellationToken ct = default) =>
        RequireManager().ShowAlertAsync(
            title,
            message,
            style,
            button1Text,
            button2Text,
            button3Text,
            ct);
}
