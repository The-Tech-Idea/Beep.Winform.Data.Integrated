namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public sealed class FormsNotificationEventArgs(string message, FormsNotificationKind kind) : EventArgs
{
    public string Message { get; } = message;
    public FormsNotificationKind Kind { get; } = kind;
}

public enum FormsNotificationKind
{
    Info,
    Warning,
    Error
}
