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

        // ── Oracle Forms item-level properties ───────────────────────────

        /// <summary>
        /// Whether the user must supply a value before commit. Maps to
        /// <c>REQUIRED</c> in Oracle Forms. Beep feeds this into the
        /// <see cref="BlockValidationManager"/> so missing values raise the
        /// <c>FRM-40400</c>-style "field is required" error during
        /// <c>WHEN-VALIDATE-RECORD</c>.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Whether the user must supply a value before commit (Oracle Forms REQUIRED).")]
        [DefaultValue(false)]
        public bool IsRequired { get; set; }

        /// <summary>
        /// Whether the item can be edited when the block is in query mode.
        /// Maps to <c>QUERY_ALLOWED</c> at the item level. Default
        /// <c>true</c> matches the Forms default.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Whether the item can be edited in query mode (Oracle Forms QUERY_ALLOWED).")]
        [DefaultValue(true)]
        public bool QueryAllowed { get; set; } = true;

        /// <summary>
        /// Whether the item is sent to the database during INSERT.
        /// Maps to <c>INSERT_ALLOWED</c> at the item level.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Whether the item is included in INSERT statements (Oracle Forms INSERT_ALLOWED).")]
        [DefaultValue(true)]
        public bool InsertAllowed { get; set; } = true;

        /// <summary>
        /// Whether the item is sent to the database during UPDATE.
        /// Maps to <c>UPDATE_ALLOWED</c> at the item level.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Whether the item is included in UPDATE statements (Oracle Forms UPDATE_ALLOWED).")]
        [DefaultValue(true)]
        public bool UpdateAllowed { get; set; } = true;

        /// <summary>
        /// Case restriction applied as the user types. Maps to
        /// <c>CASE_RESTRICTION</c> in Oracle Forms.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Case restriction applied to user input (Oracle Forms CASE_RESTRICTION).")]
        [DefaultValue(BeepCaseRestriction.None)]
        public BeepCaseRestriction CaseRestriction { get; set; } = BeepCaseRestriction.None;

        /// <summary>
        /// Format mask used to display and parse the value. Maps to
        /// <c>FORMAT_MASK</c> in Oracle Forms (e.g. <c>DD-MON-YYYY</c>,
        /// <c>$999,999.00</c>, <c>999G999G999D99</c>).
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Format mask for display and parse (Oracle Forms FORMAT_MASK, e.g. 'DD-MON-YYYY').")]
        [DefaultValue("")]
        public string FormatMask { get; set; } = string.Empty;

        /// <summary>
        /// Multi-line help text shown when the field is focused. Maps to
        /// <c>HINT_TEXT</c> (or the "Help" property in older Forms
        /// versions).
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Multi-line help text shown when the field is focused (Oracle Forms HINT).")]
        [DefaultValue("")]
        public string HelpText { get; set; } = string.Empty;

        /// <summary>
        /// Optional calculation formula. Maps to <c>CALCULATION</c> in
        /// Oracle Forms (e.g. <c>SUMMARY</c>, <c>FORMULA</c>, <c>SUM</c>).
        /// Beep stores the raw expression; presentation layers can render
        /// the field read-only with the calculation applied at runtime.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Calculation formula reference or expression (Oracle Forms CALCULATION).")]
        [DefaultValue(BeepFieldCalculation.None)]
        public BeepFieldCalculation Calculation { get; set; } = BeepFieldCalculation.None;

        // M3-RUN-016: the formula expression when Calculation =
        // Formula. The runtime's BeepFieldCalculator.EvaluateFormula
        // evaluates this string against the current record's
        // fields. Examples: "Quantity * UnitPrice",
        // "(SubTotal + Tax) * Discount".
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Formula expression used when Calculation = Formula (e.g. 'Quantity * UnitPrice').")]
        [DefaultValue("")]
        public string CalculationFormula { get; set; } = string.Empty;

        /// <summary>
        /// Summary operation applied to the column across the active
        /// record set. Maps to <c>SUMMARY</c> in Oracle Forms.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Summary operation across the block (Oracle Forms SUMMARY).")]
        [DefaultValue(BeepSummaryOperation.None)]
        public BeepSummaryOperation Summary { get; set; } = BeepSummaryOperation.None;

        /// <summary>
        /// Whether the item holds a database value (true) or is a control
        /// item (false). Maps to <c>DATABASE_ITEM</c> at the item level.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Whether the item stores a database value (Oracle Forms DATABASE_ITEM).")]
        [DefaultValue(true)]
        public bool DatabaseItem { get; set; } = true;

        /// <summary>
        /// Whether the field accepts null values. Forms default is
        /// <c>true</c> (matches SQL <c>NULL</c> semantics). Maps to
        /// <c>ALLOW_NULL</c> / <c>REQUIRED</c>.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Whether null values are accepted (Oracle Forms ALLOW_NULL).")]
        [DefaultValue(true)]
        public bool AllowNull { get; set; } = true;

        /// <summary>
        /// Whether the user can copy the value out of the field.
        /// Maps to <c>COPYABLE</c> in Oracle Forms.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Whether the user can copy the value to the clipboard (Oracle Forms COPYABLE).")]
        [DefaultValue(true)]
        public bool Copyable { get; set; } = true;

        /// <summary>
        /// Whether the user can paste into the field.
        /// Maps to <c>PASTEABLE</c> in Oracle Forms.
        /// </summary>
        [NotifyParentProperty(true)]
        [Category("Oracle Forms")]
        [Description("Whether the user can paste into the field (Oracle Forms PASTEABLE).")]
        [DefaultValue(true)]
        public bool Pasteable { get; set; } = true;

        // ── End Oracle Forms item-level properties ───────────────────────

        [NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepFieldOptionDefinitionCollectionEditor, TheTechIdea.Beep.Winform.Controls.Design.Server", typeof(UITypeEditor))]
        public List<BeepFieldOptionDefinition> Options { get; set; } = new();

        // M2-RUN-002: visual attribute carried by the field. The
        // runtime applies it via the field presenter; the IDE
        // edits it through the new VisualAttributeEditorDialog.
        public BeepFieldVisualAttribute? VisualAttribute { get; set; }

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
                IsRequired = IsRequired,
                QueryAllowed = QueryAllowed,
                InsertAllowed = InsertAllowed,
                UpdateAllowed = UpdateAllowed,
                DatabaseItem = DatabaseItem,
                AllowNull = AllowNull,
                Copyable = Copyable,
                Pasteable = Pasteable,
                CaseRestriction = CaseRestriction,
                FormatMask = FormatMask,
                HelpText = HelpText,
                Calculation = Calculation,
                CalculationFormula = CalculationFormula,
                Summary = Summary,
                DefaultValue = DefaultValue,
                VisualAttribute = VisualAttribute
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

    /// <summary>
    /// Mirrors Oracle Forms <c>CASE_RESTRICTION</c> for item input.
    /// </summary>
    public enum BeepCaseRestriction
    {
        None = 0,
        Upper = 1,
        Lower = 2,
        Mixed = 3
    }

    /// <summary>
    /// Mirrors Oracle Forms <c>CALCULATION</c>. Beep stores the type; the
    /// field presenter is responsible for applying the calculation
    /// (Formula / Average / Count / etc.) at display time.
    /// </summary>
    public enum BeepFieldCalculation
    {
        None = 0,
        Formula = 1,
        Summary = 2,
        Count = 3
    }

    /// <summary>
    /// Mirrors Oracle Forms <c>SUMMARY</c> aggregation operation.
    /// </summary>
    public enum BeepSummaryOperation
    {
        None = 0,
        Sum = 1,
        Average = 2,
        Count = 3,
        Minimum = 4,
        Maximum = 5,
        StandardDeviation = 6,
        Variance = 7
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