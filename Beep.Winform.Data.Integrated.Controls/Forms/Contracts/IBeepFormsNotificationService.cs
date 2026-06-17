using TheTechIdea.Beep.Editor.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts
{
    public interface IBeepFormsNotificationService
    {
        string CurrentMessage { get; }
        BeepMessageSeverity CurrentSeverity { get; }

        void Publish(BeepViewState viewState, string message, BeepMessageSeverity severity = BeepMessageSeverity.Info);
        void Clear(BeepViewState viewState);
    }
}