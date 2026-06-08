using System.Drawing;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models
{
    /// <summary>
    /// M2-RUN-001: visual attribute record carried by
    /// <see cref="BeepFieldDefinition"/>. Mirrors the Oracle Forms
    /// "Visual Attribute" concept: a named bundle of font / colour
    /// / border settings that a runtime can apply to a field on
    /// demand (e.g. when the field enters the "current record" or
    /// the "query mode" state).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="CurrentRecordOverride"/> flag controls whether
    /// the runtime should apply this attribute on top of the
    /// field's base attribute when the field is in the current
    /// record. When the field leaves the current record, the base
    /// attribute is restored.
    /// </para>
    /// <para>
    /// Colour slots are nullable. A <c>null</c> slot means "do not
    /// override the base value". This keeps the partial-override
    /// use case (e.g. only override the back colour) simple to
    /// model.
    /// </para>
    /// </remarks>
    public sealed class BeepFieldVisualAttribute
    {
        public string Name { get; set; } = string.Empty;
        public string? FontFamily { get; set; }
        public float? FontSize { get; set; }
        public FontStyle? FontStyle { get; set; }
        public Color? ForeColor { get; set; }
        public Color? BackColor { get; set; }
        public Color? BorderColor { get; set; }
        public bool CurrentRecordOverride { get; set; }
        public bool QueryModeOverride { get; set; }
        public bool ChangedRecordOverride { get; set; }
    }
}
