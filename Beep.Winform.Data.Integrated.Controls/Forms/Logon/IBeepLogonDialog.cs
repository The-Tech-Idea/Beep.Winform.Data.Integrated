using System;
using System.Threading.Tasks;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Logon
{
    public interface IBeepLogonDialog
    {
        event EventHandler<BeepLogonContext>? LoggedIn;

        /// <summary>
        /// Raised when the user clicks the embedded <c>BeepLogin</c> button
        /// (i.e. presses the form's primary login affordance). The event carries
        /// the in-flight <see cref="BeepLogonContext"/> assembled from the
        /// current dialog values; subscribers may cancel the click to keep the
        /// dialog open.
        /// </summary>
        event EventHandler<BeepLogonEventArgs>? OnLogin;

        BeepLogonContext? LastContext { get; }

        Task<BeepLogonContext> PromptAsync(BeepLogonRequest request);
    }
}
