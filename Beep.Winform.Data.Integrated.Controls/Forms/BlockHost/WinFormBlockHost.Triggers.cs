using TheTechIdea.Beep.Editor.Forms.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.BlockHost;

public partial class WinFormBlockHost
{
    public void RaiseTriggerExecuting(TriggerExecutingEventArgs args) =>
        TriggerExecuting?.Invoke(this, args);

    public void RaiseTriggerExecuted(TriggerExecutedEventArgs args) =>
        TriggerExecuted?.Invoke(this, args);

    public void RaiseTriggerRegistered(TriggerRegisteredEventArgs args) =>
        TriggerRegistered?.Invoke(this, args);

    public void RaiseTriggerUnregistered(TriggerUnregisteredEventArgs args) =>
        TriggerUnregistered?.Invoke(this, args);
}
