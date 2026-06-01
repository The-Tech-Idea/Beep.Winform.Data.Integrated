using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts
{
    public interface IBeepFormsNotificationService
    {
        string CurrentMessage { get; }
        BeepFormsMessageSeverity CurrentSeverity { get; }

        void Publish(BeepFormsViewState viewState, string message, BeepFormsMessageSeverity severity = BeepFormsMessageSeverity.Info);
        void Clear(BeepFormsViewState viewState);
    }
}