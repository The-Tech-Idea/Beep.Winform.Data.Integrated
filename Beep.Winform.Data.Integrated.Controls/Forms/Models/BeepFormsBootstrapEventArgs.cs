using System;
using System.Threading;
using System.Threading.Tasks;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models
{
    /// <summary>Lifecycle states for the async bootstrap operation in <see cref="BeepForms"/>.</summary>
    public enum BootstrapState
    {
        /// <summary>No bootstrap attempt has been made yet.</summary>
        Idle,
        /// <summary>Bootstrap is currently in progress.</summary>
        Running,
        /// <summary>Bootstrap completed successfully.</summary>
        Succeeded,
        /// <summary>Bootstrap completed but one or more blocks could not be populated.</summary>
        PartialSuccess,
        /// <summary>Bootstrap failed entirely.</summary>
        Failed
    }

    /// <summary>Event arguments raised when the bootstrap operation on <see cref="BeepForms"/> completes.</summary>
    public sealed class BeepFormsBootstrapEventArgs : EventArgs
    {
        public BootstrapState State { get; }
        public string? ErrorMessage { get; }

        public BeepFormsBootstrapEventArgs(BootstrapState state, string? errorMessage = null)
        {
            State = state;
            ErrorMessage = errorMessage;
        }
    }
}
