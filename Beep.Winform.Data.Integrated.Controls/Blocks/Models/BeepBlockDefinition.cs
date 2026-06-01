using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models
{
    [TypeConverter("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepBlockDefinitionTypeConverter, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public sealed class BeepBlockDefinition
    {
        [NotifyParentProperty(true)]
        public string Id { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public string BlockName { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public string ManagerBlockName { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public string Caption { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public BeepBlockPresentationMode PresentationMode { get; set; } = BeepBlockPresentationMode.Record;

        [NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepFieldDefinitionCollectionEditor, TheTechIdea.Beep.Winform.Controls.Design.Server", typeof(UITypeEditor))]
        public List<BeepFieldDefinition> Fields { get; set; } = new();

        [NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepBlockEntityDefinitionEditor, TheTechIdea.Beep.Winform.Controls.Design.Server", typeof(UITypeEditor))]
        public BeepBlockEntityDefinition Entity { get; set; } = new();

        [NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepBlockNavigationDefinitionEditor, TheTechIdea.Beep.Winform.Controls.Design.Server", typeof(UITypeEditor))]
        public BeepBlockNavigationDefinition Navigation { get; set; } = new();

        [NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepStringDictionaryEditor, TheTechIdea.Beep.Winform.Controls.Design.Server", typeof(UITypeEditor))]
        public Dictionary<string, string> Metadata { get; set; } = new();

        public BeepBlockDefinition Clone()
        {
            var clone = new BeepBlockDefinition
            {
                Id = Id,
                BlockName = BlockName,
                ManagerBlockName = ManagerBlockName,
                Caption = Caption,
                PresentationMode = PresentationMode,
                Entity = Entity?.Clone() ?? new BeepBlockEntityDefinition(),
                Navigation = Navigation?.Clone() ?? new BeepBlockNavigationDefinition()
            };

            foreach (var field in Fields)
            {
                if (field != null)
                {
                    clone.Fields.Add(field.Clone());
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
            string name = string.IsNullOrWhiteSpace(Caption) ? BlockName : Caption;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Block Definition";
            }

            return $"{name} [{PresentationMode}]";
        }
    }

    public enum BeepBlockPresentationMode
    {
        Record,
        Grid,
        /// <summary>
        /// Controls were created by the IDE extension and persisted in Designer.cs.
        /// BeepBlock skips its own internal control-creation pass and uses only the
        /// controls registered via <c>BindControl</c>.
        /// </summary>
        DesignerGenerated
    }

    [TypeConverter("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepBlockNavigationDefinitionTypeConverter, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public sealed class BeepBlockNavigationDefinition
    {
        [NotifyParentProperty(true)]
        public bool? Enabled { get; set; }

        [NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public BeepBlockNavigationCommandDefinition First { get; set; } = new();

        [NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public BeepBlockNavigationCommandDefinition Previous { get; set; } = new();

        [NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public BeepBlockNavigationCommandDefinition Next { get; set; } = new();

        [NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public BeepBlockNavigationCommandDefinition Last { get; set; } = new();

        [NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public BeepBlockNavigationCommandDefinition NewRecord { get; set; } = new();

        [NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public BeepBlockNavigationCommandDefinition Delete { get; set; } = new();

        [NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public BeepBlockNavigationCommandDefinition Query { get; set; } = new();

        [NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public BeepBlockNavigationCommandDefinition Execute { get; set; } = new();

        [NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public BeepBlockNavigationCommandDefinition Save { get; set; } = new();

        [NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public BeepBlockNavigationCommandDefinition Rollback { get; set; } = new();

        public BeepBlockNavigationDefinition Clone()
        {
            return new BeepBlockNavigationDefinition
            {
                Enabled = Enabled,
                First = First?.Clone() ?? new BeepBlockNavigationCommandDefinition(),
                Previous = Previous?.Clone() ?? new BeepBlockNavigationCommandDefinition(),
                Next = Next?.Clone() ?? new BeepBlockNavigationCommandDefinition(),
                Last = Last?.Clone() ?? new BeepBlockNavigationCommandDefinition(),
                NewRecord = NewRecord?.Clone() ?? new BeepBlockNavigationCommandDefinition(),
                Delete = Delete?.Clone() ?? new BeepBlockNavigationCommandDefinition(),
                Query = Query?.Clone() ?? new BeepBlockNavigationCommandDefinition(),
                Execute = Execute?.Clone() ?? new BeepBlockNavigationCommandDefinition(),
                Save = Save?.Clone() ?? new BeepBlockNavigationCommandDefinition(),
                Rollback = Rollback?.Clone() ?? new BeepBlockNavigationCommandDefinition()
            };
        }

        public bool TryResolveFlag(string key, out bool value)
        {
            value = false;
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            if (string.Equals(key, "navigation.enabled", StringComparison.OrdinalIgnoreCase))
            {
                if (Enabled.HasValue)
                {
                    value = Enabled.Value;
                    return true;
                }

                return false;
            }

            const string navigationPrefix = "navigation.";
            if (!key.StartsWith(navigationPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string remainder = key.Substring(navigationPrefix.Length);
            int separatorIndex = remainder.IndexOf('.');
            if (separatorIndex <= 0 || separatorIndex >= remainder.Length - 1)
            {
                return false;
            }

            string commandName = remainder.Substring(0, separatorIndex);
            string flagName = remainder.Substring(separatorIndex + 1);

            BeepBlockNavigationCommandDefinition? command = commandName.ToLowerInvariant() switch
            {
                "first" => First,
                "previous" => Previous,
                "next" => Next,
                "last" => Last,
                "new" => NewRecord,
                "delete" => Delete,
                "query" => Query,
                "execute" => Execute,
                "save" => Save,
                "rollback" => Rollback,
                _ => null
            };

            return command?.TryResolveFlag(flagName, out value) == true;
        }

        public override string ToString()
        {
            int overrideCount = 0;
            if (Enabled.HasValue)
            {
                overrideCount++;
            }

            overrideCount += CountOverrides(First);
            overrideCount += CountOverrides(Previous);
            overrideCount += CountOverrides(Next);
            overrideCount += CountOverrides(Last);
            overrideCount += CountOverrides(NewRecord);
            overrideCount += CountOverrides(Delete);
            overrideCount += CountOverrides(Query);
            overrideCount += CountOverrides(Execute);
            overrideCount += CountOverrides(Save);
            overrideCount += CountOverrides(Rollback);

            return overrideCount == 0 ? "Navigation Defaults" : $"Navigation Overrides ({overrideCount})";
        }

        private static int CountOverrides(BeepBlockNavigationCommandDefinition? command)
        {
            if (command == null)
            {
                return 0;
            }

            int count = 0;
            if (command.Visible.HasValue)
            {
                count++;
            }

            if (command.Enabled.HasValue)
            {
                count++;
            }

            return count;
        }
    }

    [TypeConverter("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepBlockNavigationCommandDefinitionTypeConverter, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public sealed class BeepBlockNavigationCommandDefinition
    {
        [NotifyParentProperty(true)]
        public bool? Visible { get; set; }

        [NotifyParentProperty(true)]
        public bool? Enabled { get; set; }

        public BeepBlockNavigationCommandDefinition Clone()
        {
            return new BeepBlockNavigationCommandDefinition
            {
                Visible = Visible,
                Enabled = Enabled
            };
        }

        public bool TryResolveFlag(string flagName, out bool value)
        {
            value = false;
            if (string.IsNullOrWhiteSpace(flagName))
            {
                return false;
            }

            if (string.Equals(flagName, "visible", StringComparison.OrdinalIgnoreCase) && Visible.HasValue)
            {
                value = Visible.Value;
                return true;
            }

            if (string.Equals(flagName, "enabled", StringComparison.OrdinalIgnoreCase) && Enabled.HasValue)
            {
                value = Enabled.Value;
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            if (!Visible.HasValue && !Enabled.HasValue)
            {
                return "Inherit defaults";
            }

            string visibleText = Visible.HasValue ? (Visible.Value ? "Visible" : "Hidden") : "Visible: inherit";
            string enabledText = Enabled.HasValue ? (Enabled.Value ? "Enabled" : "Disabled") : "Enabled: inherit";
            return $"{visibleText}, {enabledText}";
        }
    }
}