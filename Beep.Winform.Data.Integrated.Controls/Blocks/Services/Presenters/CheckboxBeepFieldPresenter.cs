using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Services.Presenters
{
    public sealed class CheckboxBeepFieldPresenter : IBeepFieldPresenter
    {
        public string Key => "checkbox";

        public bool CanPresent(BeepFieldDefinition fieldDefinition)
        {
            return string.Equals(fieldDefinition?.EditorKey, Key, System.StringComparison.OrdinalIgnoreCase);
        }

        public Control CreateEditor(BeepFieldDefinition fieldDefinition)
        {
            return new BeepCheckBoxBool
            {
                Name = string.IsNullOrWhiteSpace(fieldDefinition.FieldName) ? "FieldEditor" : $"{fieldDefinition.FieldName}_Editor",
                Width = fieldDefinition.Width,
                HideText = true,
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