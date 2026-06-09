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
        /// closes; on success, the <c>Post-Logon</c>
        /// form-level trigger is fired through the engine.
        /// </summary>
        public bool ShowLogonDialog()
        {
            using var dialog = new Logon.BeepLogonDialog();
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                LoggedIn?.Invoke(this, EventArgs.Empty);
                RaisePostLogon();
                return true;
            }
            return false;
        }

        /// <summary>
        /// M4-RUN-008 / M5-RUN-002: raise the <c>Post-Logon</c>
        /// form-level trigger. The method calls the
        /// <c>FireFormTrigger</c> path through the inherited
        /// <see cref="BeepForms.FormsManager"/>.
        /// </summary>
        public new void RaisePostLogon()
        {
            try
            {
                if (FormsManager?.Triggers == null) return;
                var result = FormsManager.Triggers.FireFormTrigger(
                    TheTechIdea.Beep.Editor.Forms.Models.TriggerType.PostLogon,
                    FormName);
                System.Diagnostics.Debug.WriteLine($"[BeepLogonScreen.RaisePostLogon] {FormName} PostLogon fired, result={result}.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BeepLogonScreen.RaisePostLogon] {ex.Message}");
            }
        }
    }
}
