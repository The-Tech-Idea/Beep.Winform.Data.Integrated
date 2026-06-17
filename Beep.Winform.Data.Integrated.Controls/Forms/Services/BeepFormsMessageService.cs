using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Services
{
    public sealed class BeepFormsMessageService : IBeepFormsNotificationService
    {
        public string CurrentMessage { get; private set; } = string.Empty;

        public BeepMessageSeverity CurrentSeverity { get; private set; } = BeepMessageSeverity.None;

        public void Publish(BeepViewState viewState, string message, BeepMessageSeverity severity = BeepMessageSeverity.Info)
        {
            CurrentMessage = message ?? string.Empty;
            CurrentSeverity = severity;
            viewState.CurrentMessage = CurrentMessage;
            viewState.MessageSeverity = severity;
        }

        public void Clear(BeepViewState viewState)
        {
            CurrentMessage = string.Empty;
            CurrentSeverity = BeepMessageSeverity.None;
            viewState.CurrentMessage = string.Empty;
            viewState.MessageSeverity = BeepMessageSeverity.None;
        }
    }
}