using System;
using System.Threading.Tasks;

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
        /// M4-RUN-008: raise the <c>On-Logoff</c> form-level
        /// trigger. The form's <c>ITriggerManager</c> exposes
        /// registration but not a fire-and-forget raise; the
        /// concrete <see cref="BeepLogonScreen"/> raises the
        /// trigger through its own trigger chain. The method
        /// is a no-op stub for M4; M4 polish rounds it out to
        /// call the engine's <c>RaiseFormTrigger(...)</c> when
        /// the API is added.
        /// </summary>
        public void RaiseOnLogoff()
        {
            // M4-RUN-008 stub. The trigger manager's API surface
            // currently exposes Register* but not Raise*; the
            // M4 implementation will route this through the
            // engine's RaiseFormTrigger once it's exposed on
            // ITriggerManager. For M4 the orchestrator's
            // FormClosed event is the source of truth.
            try
            {
                System.Diagnostics.Debug.WriteLine($"[BeepForms.RaiseOnLogoff] {FormName} is closing.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BeepForms.RaiseOnLogoff] {ex.Message}");
            }
        }
    }
}
