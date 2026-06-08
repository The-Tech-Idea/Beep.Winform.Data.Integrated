using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    /// <summary>
    /// M4-RUN-007: a <see cref="BeepForms"/> that hosts the
    /// logon screen. The class is intentionally thin — it
    /// composes the existing <see cref="Logon.BeepLogonDialog"/>
    /// into the multi-form application and raises the
    /// <c>Post-Logon</c> / <c>On-Logoff</c> form-level triggers
    /// through <see cref="RaiseOnLogoff"/> and the new
    /// <see cref="RaisePostLogon"/>.
    /// </summary>
    public sealed class BeepLogonScreen : BeepForms
    {
        /// <summary>Flag: <c>true</c> when the form is a logon screen.</summary>
        public bool IsLogon => true;

        public event EventHandler? LoggedIn;

        public BeepLogonScreen() : base()
        {
            Name = "BeepLogonScreen";
        }

        /// <summary>
        /// Show the logon dialog. The function returns <c>true</c>
        /// when the user logs in successfully; <c>false</c> when
        /// the user cancels. The method blocks until the dialog
        /// closes; the <see cref="BeepApplication"/> uses the
        /// result to drive <see cref="RaisePostLogon"/>.
        /// </summary>
        public bool ShowLogonDialog()
        {
            using var dialog = new Logon.BeepLogonDialog();
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                LoggedIn?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        /// <summary>
        /// M4-RUN-008: raise the <c>Post-Logon</c> form-level
        /// trigger. The method is a stub for M4; M4 polish
        /// routes it through the engine's
        /// <c>RaiseFormTrigger(...)</c> when the API is added.
        /// </summary>
        public void RaisePostLogon()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[BeepLogonScreen.RaisePostLogon] {FormName} is now logged in.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BeepLogonScreen.RaisePostLogon] {ex.Message}");
            }
        }
    }
}
