using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Services
{
    public sealed class BeepFormsMessageService : IBeepFormsNotificationService
    {
        public string CurrentMessage { get; private set; } = string.Empty;

        public BeepFormsMessageSeverity CurrentSeverity { get; private set; } = BeepFormsMessageSeverity.None;

        public void Publish(BeepFormsViewState viewState, string message, BeepFormsMessageSeverity severity = BeepFormsMessageSeverity.Info)
        {
            CurrentMessage = message ?? string.Empty;
            CurrentSeverity = severity;
            viewState.CurrentMessage = CurrentMessage;
            viewState.MessageSeverity = severity;
        }

        public void Clear(BeepFormsViewState viewState)
        {
            CurrentMessage = string.Empty;
            CurrentSeverity = BeepFormsMessageSeverity.None;
            viewState.CurrentMessage = string.Empty;
            viewState.MessageSeverity = BeepFormsMessageSeverity.None;
        }
    }
}