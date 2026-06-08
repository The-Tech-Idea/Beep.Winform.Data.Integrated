using System.Collections.Generic;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models
{
    /// <summary>
    /// M2-RUN-015: a block-level visual attribute group with four
    /// sub-attributes (header, current record, query mode, changed
    /// record). Each sub-attribute is itself a
    /// <see cref="BeepFieldVisualAttribute"/>; the block applies the
    /// matching sub-attribute to the focused field on the
    /// corresponding state transition.
    /// </summary>
    public sealed class BeepBlockVisualAttributeGroup
    {
        /// <summary>Default attribute for the block's "header" state (no focus / new record).</summary>
        public BeepFieldVisualAttribute? Header { get; set; }

        /// <summary>Override applied when a record is the "current" record (focus is on a field in the block).</summary>
        public BeepFieldVisualAttribute? CurrentRecord { get; set; }

        /// <summary>Override applied when the block is in query / find mode.</summary>
        public BeepFieldVisualAttribute? QueryMode { get; set; }

        /// <summary>Override applied when the record is dirty (changed in memory but not committed).</summary>
        public BeepFieldVisualAttribute? ChangedRecord { get; set; }

        public bool IsEmpty =>
            Header == null
            && CurrentRecord == null
            && QueryMode == null
            && ChangedRecord == null;
    }
}
