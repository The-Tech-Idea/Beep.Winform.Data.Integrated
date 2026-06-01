using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Services.Presenters
{
    public sealed class ReflectiveControlBeepFieldPresenter : IBeepFieldPresenter
    {
        public string Key => "control";

        public bool CanPresent(BeepFieldDefinition fieldDefinition)
        {
            return !string.IsNullOrWhiteSpace(fieldDefinition?.ControlType);
        }

        public Control CreateEditor(BeepFieldDefinition fieldDefinition)
        {
            var controlType = BeepFieldControlTypeRegistry.ResolveControlType(fieldDefinition.ControlType) ?? typeof(BeepTextBox);
            var editor = Activator.CreateInstance(controlType) as Control ?? new BeepTextBox();
            editor.Name = string.IsNullOrWhiteSpace(fieldDefinition.FieldName) ? "FieldEditor" : $"{fieldDefinition.FieldName}_Editor";
            return editor;
        }

        public void ApplyMetadata(Control editor, BeepFieldDefinition fieldDefinition)
        {
            editor.Visible = fieldDefinition.IsVisible;
            editor.Enabled = !fieldDefinition.IsReadOnly;
            editor.Width = fieldDefinition.Width > 0 ? fieldDefinition.Width : editor.Width;

            ApplyBooleanProperty(editor, nameof(BeepTextBox.UseThemeColors), true);
            ApplyBooleanProperty(editor, nameof(TextBoxBase.ReadOnly), fieldDefinition.IsReadOnly);

            if (!string.IsNullOrWhiteSpace(fieldDefinition.Label))
            {
                ApplyStringProperty(editor, nameof(BeepTextBox.PlaceholderText), fieldDefinition.Label);
            }

            if (editor is BeepComboBox comboBox)
            {
                comboBox.AllowFreeText = true;
                comboBox.ListItems ??= new BindingList<SimpleItem>();
                comboBox.PlaceholderText = string.IsNullOrWhiteSpace(fieldDefinition.Label) ? fieldDefinition.FieldName : fieldDefinition.Label;
            }

            if (editor is BeepCheckBoxBool checkBox)
            {
                checkBox.HideText = true;
            }
        }

        private static void ApplyBooleanProperty(Control editor, string propertyName, bool value)
        {
            var property = editor.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property?.CanWrite == true && property.PropertyType == typeof(bool))
            {
                property.SetValue(editor, value);
            }
        }

        private static void ApplyStringProperty(Control editor, string propertyName, string value)
        {
            var property = editor.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property?.CanWrite == true && property.PropertyType == typeof(string))
            {
                property.SetValue(editor, value);
            }
        }
    }
}