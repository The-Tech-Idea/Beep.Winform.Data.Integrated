using System.ComponentModel;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Logon
{
    /// <summary>
    /// Event payload for <see cref="IBeepLogonDialog.OnLogin"/>.
    /// Carries the in-flight <see cref="BeepLogonContext"/> assembled from the
    /// current dialog values; subscribers may set <see cref="CancelEventArgs.Cancel"/>
    /// to <c>true</c> to prevent the dialog from closing.
    /// </summary>
    public class BeepLogonEventArgs : CancelEventArgs
    {
        public BeepLogonEventArgs(BeepLogonContext context)
        {
            Context = context;
        }

        public BeepLogonContext Context { get; }
    }
}
