using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Services;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models
{
    [TypeConverter("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepBlockEntityDefinitionTypeConverter, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public sealed class BeepBlockEntityDefinition
    {
        [NotifyParentProperty(true)]
        public string ConnectionName { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public string EntityName { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public string DatasourceEntityName { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public string Caption { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public string Description { get; set; } = string.Empty;

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
        public string DataSourceId { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public bool IsMasterBlock { get; set; }

        [NotifyParentProperty(true)]
        public string MasterBlockName { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public string MasterKeyField { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public string ForeignKeyField { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepBlockEntityFieldDefinitionCollectionEditor, TheTechIdea.Beep.Winform.Controls.Design.Server", typeof(UITypeEditor))]
        public List<BeepBlockEntityFieldDefinition> Fields { get; set; } = new();

        public List<BeepFieldDefinition> CreateFieldDefinitions()
        {
            if (Fields == null || Fields.Count == 0)
            {
                return new List<BeepFieldDefinition>();
            }

            return Fields
                .Where(field => field != null && !string.IsNullOrWhiteSpace(field.FieldName))
                .OrderBy(field => field!.Order)
                .Select((field, index) => field!.ToFieldDefinition(index))
                .ToList();
        }

        public BeepBlockEntityDefinition Clone()
        {
            var clone = new BeepBlockEntityDefinition
            {
                ConnectionName = ConnectionName,
                EntityName = EntityName,
                DatasourceEntityName = DatasourceEntityName,
                Caption = Caption,
                Description = Description,
                EditorKey = EditorKey,
                ControlType = ControlType,
                BindingProperty = BindingProperty,
                DataSourceId = DataSourceId,
                IsMasterBlock = IsMasterBlock,
                MasterBlockName = MasterBlockName,
                MasterKeyField = MasterKeyField,
                ForeignKeyField = ForeignKeyField
            };

            foreach (var field in Fields)
            {
                if (field != null)
                {
                    clone.Fields.Add(field.Clone());
                }
            }

            return clone;
        }

        public override string ToString()
        {
            string name = string.IsNullOrWhiteSpace(Caption) ? EntityName : Caption;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Entity Snapshot";
            }

            return Fields.Count == 0 ? name : $"{name} ({Fields.Count} fields)";
        }
    }

    [TypeConverter("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepBlockEntityFieldDefinitionTypeConverter, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public sealed class BeepBlockEntityFieldDefinition
    {
        [NotifyParentProperty(true)]
        public string FieldName { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public string Label { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public string Description { get; set; } = string.Empty;

        [NotifyParentProperty(true)]
        public string DataType { get; set; } = string.Empty;

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
        public DbFieldCategory Category { get; set; } = DbFieldCategory.String;

        [NotifyParentProperty(true)]
        public int Order { get; set; }

        [NotifyParentProperty(true)]
        public int Size { get; set; }

        [NotifyParentProperty(true)]
        public short NumericPrecision { get; set; }

        [NotifyParentProperty(true)]
        public short NumericScale { get; set; }

        [NotifyParentProperty(true)]
        public bool IsRequired { get; set; }

        [NotifyParentProperty(true)]
        public bool AllowDBNull { get; set; } = true;

        [NotifyParentProperty(true)]
        public bool IsPrimaryKey { get; set; }

        [NotifyParentProperty(true)]
        public bool IsUnique { get; set; }

        [NotifyParentProperty(true)]
        public bool IsIndexed { get; set; }

        [NotifyParentProperty(true)]
        public bool IsAutoIncrement { get; set; }

        [NotifyParentProperty(true)]
        public bool IsReadOnly { get; set; }

        [NotifyParentProperty(true)]
        public bool IsCheck { get; set; }

        // Phase 7A additions — lossless snapshot of EntityField
        [NotifyParentProperty(true)]
        public bool IsIdentity { get; set; }

        [NotifyParentProperty(true)]
        public bool IsHidden { get; set; }

        [NotifyParentProperty(true)]
        public bool IsLong { get; set; }

        [NotifyParentProperty(true)]
        public bool IsRowVersion { get; set; }

        [NotifyParentProperty(true)]
        public string DefaultValue { get; set; } = string.Empty;

        public BeepFieldDefinition ToFieldDefinition(int order)
        {
            var defaults = BeepFieldControlTypeRegistry.ResolveDefaultFieldSettings(Category, DataType, IsCheck, IsLong);
            string editorKey = string.IsNullOrWhiteSpace(EditorKey) ? defaults.EditorKey : EditorKey;
            string controlType = string.IsNullOrWhiteSpace(ControlType) ? defaults.ControlType : ControlType;
            string bindingProperty = string.IsNullOrWhiteSpace(BindingProperty)
                ? BeepFieldControlTypeRegistry.ResolveDefaultBindingProperty(controlType, editorKey)
                : BindingProperty;

            return new BeepFieldDefinition
            {
                FieldName = FieldName,
                Label = string.IsNullOrWhiteSpace(Label)
                    ? (string.IsNullOrWhiteSpace(Description) ? FieldName : Description)
                    : Label,
                EditorKey = editorKey,
                ControlType = controlType,
                BindingProperty = bindingProperty,
                Order = order,
                Width = ResolveWidth(),
                IsVisible = !IsHidden,
                IsReadOnly = IsReadOnly || IsAutoIncrement || IsIdentity || IsRowVersion,
                DefaultValue = DefaultValue
            };
        }

        public BeepBlockEntityFieldDefinition Clone()
        {
            return new BeepBlockEntityFieldDefinition
            {
                FieldName = FieldName,
                Label = Label,
                Description = Description,
                EditorKey = EditorKey,
                ControlType = ControlType,
                BindingProperty = BindingProperty,
                DataType = DataType,
                Category = Category,
                Order = Order,
                Size = Size,
                NumericPrecision = NumericPrecision,
                NumericScale = NumericScale,
                IsRequired = IsRequired,
                AllowDBNull = AllowDBNull,
                IsPrimaryKey = IsPrimaryKey,
                IsUnique = IsUnique,
                IsIndexed = IsIndexed,
                IsAutoIncrement = IsAutoIncrement,
                IsReadOnly = IsReadOnly,
                IsCheck = IsCheck,
                IsIdentity = IsIdentity,
                IsHidden = IsHidden,
                IsLong = IsLong,
                IsRowVersion = IsRowVersion,
                DefaultValue = DefaultValue
            };
        }

        public override string ToString()
        {
            string name = string.IsNullOrWhiteSpace(Label) ? FieldName : Label;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Field Snapshot";
            }

            return string.IsNullOrWhiteSpace(DataType) ? name : $"{name} ({DataType})";
        }

        private int ResolveWidth()
        {
            if (IsCheck || Category == DbFieldCategory.Boolean)
                return 120;

            if (Category == DbFieldCategory.Date || Category == DbFieldCategory.DateTime)
                return 180;

            if (Category == DbFieldCategory.Enum)
                return 220;

            if (Category == DbFieldCategory.Integer)
                return 120;

            if (Category == DbFieldCategory.Decimal ||
                Category == DbFieldCategory.Double ||
                Category == DbFieldCategory.Float ||
                Category == DbFieldCategory.Numeric ||
                Category == DbFieldCategory.Currency)
                return 160;

            if (IsLong)
                return 320;

            int raw = Size > 0 ? Size : 160;
            int width = raw <= 24 ? raw * 12 : raw * 2;
            return System.Math.Max(120, System.Math.Min(320, width));
        }
    }
}