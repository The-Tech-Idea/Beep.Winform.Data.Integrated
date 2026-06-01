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
        }
    }
}