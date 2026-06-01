using System.ComponentModel;
using System.Windows.Forms;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Services.Presenters
{
    public sealed class ComboBeepFieldPresenter : IBeepFieldPresenter
    {
        public string Key => "combo";

        public bool CanPresent(BeepFieldDefinition fieldDefinition)
        {
            return string.Equals(fieldDefinition?.EditorKey, Key, System.StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(fieldDefinition?.EditorKey, "lov", System.StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(fieldDefinition?.EditorKey, "option", System.StringComparison.OrdinalIgnoreCase);
        }

        public Control CreateEditor(BeepFieldDefinition fieldDefinition)
        {
            return new BeepComboBox
            {
                Name = string.IsNullOrWhiteSpace(fieldDefinition.FieldName) ? "FieldEditor" : $"{fieldDefinition.FieldName}_Editor",
                Width = fieldDefinition.Width,
                PlaceholderText = string.IsNullOrWhiteSpace(fieldDefinition.Label) ? fieldDefinition.FieldName : fieldDefinition.Label,
                AllowFreeText = true,
                ListItems = new BindingList<SimpleItem>(),
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