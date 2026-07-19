using System;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    /// <summary>
    /// Outcome of a Configuration wizard run, raised on its <c>Completed</c> event.
    /// </summary>
    /// <remarks>
    /// One shared type, deliberately. Six wizards (Migration, Schema Manager, Sync, Data Import,
    /// Defaults, Mapping) each declared a byte-for-byte identical nested copy, so a caller handling
    /// <c>Completed</c> from two of them was dealing with two unrelated types that happened to look
    /// the same. They all live in this namespace, so unqualified uses bind here with no call-site
    /// changes.
    /// </remarks>
    public sealed class WizardCompletedEventArgs : EventArgs
    {
        /// <summary>The wizard finished its work successfully.</summary>
        public bool Succeeded { get; init; }

        /// <summary>The user cancelled before the work completed.</summary>
        public bool Cancelled { get; init; }

        /// <summary>Human-readable outcome, suitable for a status line.</summary>
        public string Summary { get; init; } = string.Empty;
    }
}
