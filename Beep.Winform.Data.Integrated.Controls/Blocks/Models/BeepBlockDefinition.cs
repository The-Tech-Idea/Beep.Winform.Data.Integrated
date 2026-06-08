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

        // ── Oracle Forms block-level properties ───────────────────────────

        /// <summary>
        /// Whether the end user is allowed to enter Enter-Query mode on this
        /// block. Maps to the Oracle Forms block property
        /// <c>QUERY_ALLOWED</c>. Defaults to <c>true</c>.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Whether the user can enter Enter-Query mode on this block (Oracle Forms QUERY_ALLOWED).")]
        [DefaultValue(true)]
        public bool QueryAllowed { get; set; } = true;

        /// <summary>
        /// Whether new records can be inserted into this block.
        /// Maps to <c>INSERT_ALLOWED</c>.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Whether new records can be inserted into this block (Oracle Forms INSERT_ALLOWED).")]
        [DefaultValue(true)]
        public bool InsertAllowed { get; set; } = true;

        /// <summary>
        /// Whether existing records can be updated.
        /// Maps to <c>UPDATE_ALLOWED</c>.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Whether existing records can be updated (Oracle Forms UPDATE_ALLOWED).")]
        [DefaultValue(true)]
        public bool UpdateAllowed { get; set; } = true;

        /// <summary>
        /// Whether the user can delete records in this block.
        /// Maps to <c>DELETE_ALLOWED</c>.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Whether records can be deleted in this block (Oracle Forms DELETE_ALLOWED).")]
        [DefaultValue(true)]
        public bool DeleteAllowed { get; set; } = true;

        /// <summary>
        /// Number of records the Forms runtime should buffer at once. Oracle
        /// Forms default is 10. The Beep block uses this as a soft hint for
        /// how many rows to fetch in a single Execute-Query round-trip.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Number of records the block should buffer at once (Oracle Forms RECORDS BUFFERED / RECORDS_DISPLAYED).")]
        [DefaultValue(10)]
        public int RecordsDisplayed { get; set; } = 10;

        /// <summary>
        /// How the block decides which row of the result set is "current" after
        /// an Execute-Query. Maps to <c>KEY_MODE</c> in Forms. Beep follows the
        /// Forms rule: <see cref="BeepBlockKeyMode.Automatic"/> is the only
        /// mode currently implemented; <see cref="BeepBlockKeyMode.NonAutomatic"/>
        /// is honored in the sense that the Forms-style "no current record
        /// warning" is raised by the navigation bar if a developer tries to
        /// commit when no row is current.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("How the block determines the current record (Oracle Forms KEY_MODE).")]
        [DefaultValue(BeepBlockKeyMode.Automatic)]
        public BeepBlockKeyMode KeyMode { get; set; } = BeepBlockKeyMode.Automatic;

        /// <summary>
        /// How the block processes DML — the equivalent of Forms
        /// <c>ENFORCE_PRIMARY_KEY</c> / <c>ENFORPE_REF_UNQ</c> combined with
        /// trigger-only DML. When <see cref="BeepBlockEnforcement.TransactionalTriggers"/>,
        /// Beep never issues an INSERT/UPDATE/DELETE statement directly; the
        /// caller must handle the persistence from a <c>Pre-Insert</c> /
        /// <c>Pre-Update</c> / <c>Pre-Delete</c> trigger.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("How the block processes DML (Oracle Forms ENFORCE / transactional trigger mode).")]
        [DefaultValue(BeepBlockEnforcement.None)]
        public BeepBlockEnforcement Enforcement { get; set; } = BeepBlockEnforcement.None;

        /// <summary>
        /// When <c>false</c>, the block treats its rows as non-database items
        /// (like a control block) — INSERT/UPDATE/DELETE statements are
        /// suppressed. Maps to <c>DATABASE_ITEM</c> at the block level.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Whether rows in this block are database items (Oracle Forms DATABASE_ITEM).")]
        [DefaultValue(true)]
        public bool DatabaseItem { get; set; } = true;

        /// <summary>
        /// Optional pre-defined WHERE clause appended to every Execute-Query.
        /// Maps to <c>WHERE_CLAUSE</c>.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Pre-defined WHERE clause appended to every Execute-Query (Oracle Forms WHERE_CLAUSE).")]
        [DefaultValue("")]
        public string WhereClause { get; set; } = string.Empty;

        /// <summary>
        /// Default ORDER BY for Execute-Query. Maps to <c>ORDER_BY</c>.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Default ORDER BY for Execute-Query (Oracle Forms ORDER_BY).")]
        [DefaultValue("")]
        public string OrderByClause { get; set; } = string.Empty;

        /// <summary>
        /// Whether the navigation bar shows a visual record-status indicator
        /// (asterisk for NEW/INSERT/CHANGED records). Maps to the visual cue
        /// Forms developers use to signal "this row has unsaved changes".
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Whether the navigation bar shows a record-status indicator (asterisk for unsaved rows).")]
        [DefaultValue(true)]
        public bool ShowRecordStatusIndicator { get; set; } = true;

        // ── End Oracle Forms block-level properties ───────────────────────

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

        // M2-RUN-016: visual attribute group carried by the
        // block. The runtime applies the matching sub-attribute
        // (header / current record / query mode / changed
        // record) to the focused field on each state
        // transition. The IDE edits it through
        // BlockMetadataEditorDialog.
        public BeepBlockVisualAttributeGroup? VisualAttributeGroup { get; set; }

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
                Navigation = Navigation?.Clone() ?? new BeepBlockNavigationDefinition(),
                QueryAllowed = QueryAllowed,
                InsertAllowed = InsertAllowed,
                UpdateAllowed = UpdateAllowed,
                DeleteAllowed = DeleteAllowed,
                RecordsDisplayed = RecordsDisplayed,
                KeyMode = KeyMode,
                Enforcement = Enforcement,
                DatabaseItem = DatabaseItem,
                WhereClause = WhereClause,
                OrderByClause = OrderByClause,
                ShowRecordStatusIndicator = ShowRecordStatusIndicator,
                VisualAttributeGroup = VisualAttributeGroup
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

    /// <summary>
    /// Oracle Forms <c>KEY_MODE</c> values. Beep honors the Forms contract:
    /// in <see cref="Automatic"/> mode the Forms runtime moves to the first
    /// row after Execute-Query (matching Beep's current behavior); in
    /// <see cref="NonAutomatic"/> mode the developer is expected to call
    /// <c>GO_RECORD</c> themselves and the navigation bar shows a
    /// "no current record" state when no row is selected.
    /// </summary>
    public enum BeepBlockKeyMode
    {
        Automatic = 0,
        NonAutomatic = 1
    }

    /// <summary>
    /// How the block translates INSERT/UPDATE/DELETE to the database. Mirrors
    /// Forms' <c>ENFORCE</c> attributes and "Transactional Triggers" mode.
    /// </summary>
    public enum BeepBlockEnforcement
    {
        /// <summary>Default. Beep issues SQL DML directly when the user commits.</summary>
        None = 0,
        /// <summary>DML is suppressed; the caller's <c>Pre-Insert/Update/Delete</c> triggers handle persistence.</summary>
        TransactionalTriggers = 1,
        /// <summary>DML is allowed but primary-key uniqueness is enforced via a Pre-Insert trigger.</summary>
        PreInsertTrigger = 2
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