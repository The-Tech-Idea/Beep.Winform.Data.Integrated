using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models
{
    [TypeConverter("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepFormsDefinitionTypeConverter, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public sealed class BeepFormsDefinition
    {
        [NotifyParentProperty(true)]
        public string Id { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public string FormName { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public string Title { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepBlockDefinitionCollectionEditor, TheTechIdea.Beep.Winform.Controls.Design.Server", typeof(UITypeEditor))]
        public List<BeepBlockDefinition> Blocks { get; set; } = new();

        [NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepStringDictionaryEditor, TheTechIdea.Beep.Winform.Controls.Design.Server", typeof(UITypeEditor))]
        public Dictionary<string, string> Metadata { get; set; } = new();

        public BeepFormsDefinition Clone()
        {
            var clone = new BeepFormsDefinition
            {
                Id = Id,
                FormName = FormName,
                Title = Title
            };

            foreach (var block in Blocks)
            {
                if (block != null)
                {
                    clone.Blocks.Add(block.Clone());
                }
            }

            foreach (var item in Metadata)
            {
                clone.Metadata[item.Key] = item.Value;
            }

            return clone;
        }

        public override string ToString()
        {
            string name = string.IsNullOrWhiteSpace(Title) ? FormName : Title;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Form Definition";
            }

            return $"{name} ({Blocks.Count} block(s))";
        }
    }
}