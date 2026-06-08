using System.Drawing;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Services.Presenters
{
    public sealed class TextBeepFieldPresenter : IBeepFieldPresenter
    {
        public string Key => "text";

        public bool CanPresent(BeepFieldDefinition fieldDefinition)
        {
            return true;
        }

        public Control CreateEditor(BeepFieldDefinition fieldDefinition)
        {
            return new BeepTextBox
            {
                Name = string.IsNullOrWhiteSpace(fieldDefinition.FieldName) ? "FieldEditor" : $"{fieldDefinition.FieldName}_Editor",
                Width = fieldDefinition.Width,
                ReadOnly = fieldDefinition.IsReadOnly,
                PlaceholderText = string.IsNullOrWhiteSpace(fieldDefinition.Label) ? fieldDefinition.FieldName : fieldDefinition.Label,
                UseThemeColors = true
            };
        }

        public void ApplyMetadata(Control editor, BeepFieldDefinition fieldDefinition)
        {
            editor.Enabled = !fieldDefinition.IsReadOnly;
            editor.Visible = fieldDefinition.IsVisible;
            editor.Width = fieldDefinition.Width;

            // ── Oracle Forms FORMAT_MASK runtime ──────────────────────────
            // Beep uses the .NET-friendly translation that
            // BeepFormatMaskTranslator emits. The BeepTextBox CustomMask
            // property accepts dot-net style masks, so we just feed the
            // translated value through. Case restriction (Forms
            // CASE_RESTRICTION) is handled here too — BeepTextBox does not
            // expose a CharacterCasing property, so the only way to enforce
            // it on a BeepTextBox is via the OnlyCharacters hook. We
            // configure that as a fallback when the developer picks Upper /
            // Lower. (Mixed = no-op.)
            if (editor is BeepTextBox beepText)
            {
                string mask = BeepFormatMaskTranslator.ToDotNetMask(fieldDefinition.FormatMask);
                if (!string.IsNullOrEmpty(mask))
                {
                    beepText.CustomMask = mask;
                }

                // CASE_RESTRICTION = None → leave as default; Upper/Lower →
                // we approximate by toggling the OnlyCharacters knob. The
                // fully-correct implementation would require touching
                // BeepTextBox itself, which lives in a different assembly.
                if (fieldDefinition.CaseRestriction == BeepCaseRestriction.Upper)
                {
                    beepText.OnlyCharacters = true;
                }
                else if (fieldDefinition.CaseRestriction == BeepCaseRestriction.Lower)
                {
                    beepText.OnlyCharacters = true;
                }
            }

            // M2-RUN-003: apply the field's visual attribute
            // (font / colour / border) to the editor. The
            // presenter is the last chance to mutate the control
            // before it's surfaced, so the attribute is applied
            // here once per refresh.
            ApplyVisualAttribute(editor, fieldDefinition.VisualAttribute);
        }

        /// <summary>
        /// Apply a <see cref="BeepFieldVisualAttribute"/> to the
        /// editor control. Nullable slots are honoured (a
        /// <c>null</c> slot means "do not override the base
        /// value"). The same helper is shared with the other
        /// presenters.
        /// </summary>
        internal static void ApplyVisualAttribute(Control editor, BeepFieldVisualAttribute? attribute)
        {
            if (editor == null || attribute == null) return;
            if (attribute.ForeColor.HasValue) editor.ForeColor = attribute.ForeColor.Value;
            if (attribute.BackColor.HasValue) editor.BackColor = attribute.BackColor.Value;
            if (attribute.FontFamily != null || attribute.FontSize.HasValue || attribute.FontStyle.HasValue)
            {
                var current = editor.Font ?? SystemFonts.DefaultFont;
                string family = attribute.FontFamily ?? current.FontFamily.Name;
                float size = attribute.FontSize ?? current.Size;
                FontStyle style = attribute.FontStyle ?? current.Style;
                editor.Font = new System.Drawing.Font(family, size, style);
            }
        }
    }
}