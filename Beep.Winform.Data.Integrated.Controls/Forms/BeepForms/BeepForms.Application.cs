using System;
using System.Threading.Tasks;
using TheTechIdea.Beep.Editor.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    /// <summary>
    /// M4-RUN-006: the <see cref="Application"/> property
    /// implementation on <see cref="BeepForms"/>. The property
    /// is a plain auto-property: the setter is exposed for
    /// <see cref="BeepApplication.OpenForm"/> to wire the
    /// back-reference when a form is opened through the
    /// application's <c>OpenForm</c> / <c>GoForm</c> calls.
    /// </summary>
    public partial class BeepForms
    {
        /// <summary>
        /// M4-RUN-005 / M4-RUN-006: the multi-form application
        /// that owns this form. <c>null</c> when the form is
        /// not yet attached to a <see cref="BeepApplication"/>
        /// (design time, single-form tests, or forms opened
        /// outside the application's <c>OpenForm</c>).
        /// </summary>
        public BeepApplication? Application { get; set; }

        /// <summary>
        /// M4-RUN-008 / M5-RUN-002: raise the <c>On-Logoff</c>
        /// form-level trigger through the engine's
        /// <c>ITriggerManager.FireFormTrigger</c>. The method
        /// is no-op when the form has no trigger manager.
        /// </summary>
        public void RaiseOnLogoff()
        {
            try
            {
                var formTriggers = FormsManager?.Triggers;
                if (formTriggers == null) return;
                var result = formTriggers.FireFormTrigger(TriggerType.OnLogoff, FormName);
                System.Diagnostics.Debug.WriteLine($"[BeepForms.RaiseOnLogoff] {FormName} OnLogoff fired, result={result}.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BeepForms.RaiseOnLogoff] {ex.Message}");
            }
        }

        /// <summary>
        /// M5-RUN-002: raise the <c>Post-Logon</c> form-level
        /// trigger through the engine's
        /// <c>ITriggerManager.FireFormTrigger</c>. The method
        /// is no-op when the form has no trigger manager.
        /// </summary>
        public void RaisePostLogon()
        {
            try
            {
                var formTriggers = FormsManager?.Triggers;
                if (formTriggers == null) return;
                var result = formTriggers.FireFormTrigger(TriggerType.PostLogon, FormName);
                System.Diagnostics.Debug.WriteLine($"[BeepForms.RaisePostLogon] {FormName} PostLogon fired, result={result}.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BeepForms.RaisePostLogon] {ex.Message}");
            }
        }
    }
}
