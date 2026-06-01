using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;
using TheTechIdea.Beep.Winform.Controls.Numerics;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Services.Presenters
{
    public sealed class NumericBeepFieldPresenter : IBeepFieldPresenter
    {
        public string Key => "numeric";

        public bool CanPresent(BeepFieldDefinition fieldDefinition)
        {
            return string.Equals(fieldDefinition?.EditorKey, Key, System.StringComparison.OrdinalIgnoreCase);
        }

        public Control CreateEditor(BeepFieldDefinition fieldDefinition)
        {
            return new BeepNumericUpDown
            {
                Name = string.IsNullOrWhiteSpace(fieldDefinition.FieldName) ? "FieldEditor" : $"{fieldDefinition.FieldName}_Editor",
                Width = fieldDefinition.Width,
                ReadOnly = fieldDefinition.IsReadOnly,
                UseThemeColors = true
            };
        }

        public void ApplyMetadata(Control editor, BeepFieldDefinition fieldDefinition)
        {
            editor.Enabled = !fieldDefinition.IsReadOnly;
            editor.Visible = fieldDefinition.IsVisible;
            editor.Width = fieldDefinition.Width;
        }
    }
}