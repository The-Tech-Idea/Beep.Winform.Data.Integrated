using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Services;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models
{
    [TypeConverter("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepFieldDefinitionTypeConverter, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public sealed class BeepFieldDefinition
    {
        [NotifyParentProperty(true)]
        public string FieldName { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public string Label { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        [TypeConverter("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepFieldEditorKeyTypeConverter, TheTechIdea.Beep.Winform.Controls.Design.Server")]
        public string EditorKey { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        [TypeConverter("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepFieldControlTypeTypeConverter, TheTechIdea.Beep.Winform.Controls.Design.Server")]
        public string ControlType { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        [TypeConverter("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepFieldBindingPropertyTypeConverter, TheTechIdea.Beep.Winform.Controls.Design.Server")]
        public string BindingProperty { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public int Order { get; set; }

        [NotifyParentProperty(true)]
        public int Width { get; set; } = 160;

        [NotifyParentProperty(true)]
        public bool IsVisible { get; set; } = true;

        [NotifyParentProperty(true)]
        public bool IsReadOnly { get; set; }

        [NotifyParentProperty(true)]
        public string DefaultValue { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepFieldOptionDefinitionCollectionEditor, TheTechIdea.Beep.Winform.Controls.Design.Server", typeof(UITypeEditor))]
        public List<BeepFieldOptionDefinition> Options { get; set; } = new();

        public BeepFieldDefinition Clone()
        {
            var clone = new BeepFieldDefinition
            {
                FieldName = FieldName,
                Label = Label,
                EditorKey = EditorKey,
                ControlType = ControlType,
                BindingProperty = BindingProperty,
                Order = Order,
                Width = Width,
                IsVisible = IsVisible,
                IsReadOnly = IsReadOnly,
                DefaultValue = DefaultValue
            };

            if (Options != null)
            {
                foreach (var option in Options)
                {
                    if (option != null)
                    {
                        clone.Options.Add(option.Clone());
                    }
                }
            }

            return clone;
        }

        public override string ToString()
        {
            string name = string.IsNullOrWhiteSpace(Label) ? FieldName : Label;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Field Definition";
            }

            string editorDescriptor = !string.IsNullOrWhiteSpace(ControlType)
                ? BeepFieldControlTypeRegistry.SimplifyControlTypeName(ControlType)
                : EditorKey;

            return string.IsNullOrWhiteSpace(editorDescriptor)
                ? name
                : $"{name} ({editorDescriptor})";
        }
    }

    [TypeConverter("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepFieldOptionDefinitionTypeConverter, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public sealed class BeepFieldOptionDefinition
    {
        [NotifyParentProperty(true)]
        public string Text { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public string Name { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public string Description { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public object? Value { get; set; }

        public BeepFieldOptionDefinition Clone()
        {
            return new BeepFieldOptionDefinition
            {
                Text = Text,
                Name = Name,
                Description = Description,
                Value = Value
            };
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Text))
            {
                return Text;
            }

            if (!string.IsNullOrWhiteSpace(Name))
            {
                return Name;
            }

            return "Option";
        }
    }
}