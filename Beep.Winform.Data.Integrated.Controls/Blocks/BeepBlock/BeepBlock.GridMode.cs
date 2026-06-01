using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Controls.Numerics;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Services;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks
{
    public partial class BeepBlock
    {
        private void SyncGridFromManager(IUnitofWorksManager? manager)
        {
            if (_gridView == null)
            {
                return;
            }

            if (manager == null || string.IsNullOrWhiteSpace(ManagerBlockName) || !manager.BlockExists(ManagerBlockName))
            {
                _gridView.Uow = null;
                _gridView.DataSource = null;
                return;
            }

            var blockInfo = manager.GetBlock(ManagerBlockName);
            ConfigureGridColumns(ResolveEntityStructure(blockInfo));

            _gridView.Uow = null;
            _gridView.DataSource = _recordBindingSource;
        }

        private void ConfigureGridColumns(IEntityStructure? entityStructure)
        {
            if (_gridView == null)
            {
                return;
            }

            _gridView.Columns.Clear();
            _gridView.EnsureSystemColumns();

            var definition = EffectiveDefinition;
            if (definition?.Fields == null || definition.Fields.Count == 0)
            {
                return;
            }

            var metadataByField = entityStructure?.Fields?
                .Where(x => !string.IsNullOrWhiteSpace(x.FieldName))
                .ToDictionary(x => x.FieldName, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, EntityField>(StringComparer.OrdinalIgnoreCase);

            int index = _gridView.Columns.Count;
            foreach (var fieldDefinition in definition.Fields.Where(x => x.IsVisible).OrderBy(x => x.Order))
            {
                metadataByField.TryGetValue(fieldDefinition.FieldName, out var entityField);
                _gridView.Columns.Add(CreateGridColumn(fieldDefinition, entityField, index++));
            }
        }

        private static BeepColumnConfig CreateGridColumn(Models.BeepFieldDefinition fieldDefinition, EntityField? entityField, int index)
        {
            var columnEditor = ResolveGridCellEditor(fieldDefinition, entityField);
            var columnType = entityField?.FieldCategory ?? ResolveGridColumnType(columnEditor);
            var propertyType = entityField != null ? ResolveClrType(entityField) : ResolveClrType(columnType);

            return new BeepColumnConfig
            {
                ColumnName = fieldDefinition.FieldName,
                ColumnCaption = string.IsNullOrWhiteSpace(fieldDefinition.Label) ? fieldDefinition.FieldName : fieldDefinition.Label,
                Width = fieldDefinition.Width > 0 ? fieldDefinition.Width : 160,
                Index = index,
                DisplayOrder = index,
                Visible = fieldDefinition.IsVisible,
                ReadOnly = fieldDefinition.IsReadOnly || entityField?.IsAutoIncrement == true || entityField?.FieldCategory == DbFieldCategory.Timestamp,
                IsRequired = entityField?.IsRequired ?? false,
                ColumnType = columnType,
                CellEditor = columnEditor,
                PropertyTypeName = propertyType.AssemblyQualifiedName ?? propertyType.FullName ?? typeof(string).FullName ?? "System.String"
            };
        }

        private static BeepColumnType ResolveGridCellEditor(Models.BeepFieldDefinition fieldDefinition, EntityField? entityField)
        {
            if (!string.IsNullOrWhiteSpace(fieldDefinition.ControlType))
            {
                return ResolveGridCellEditorFromControlType(fieldDefinition.ControlType, fieldDefinition.EditorKey);
            }

            string editorKey = string.IsNullOrWhiteSpace(fieldDefinition.EditorKey) && entityField != null
                ? InferEditorKey(entityField)
                : fieldDefinition.EditorKey ?? string.Empty;

            return ResolveGridCellEditorFromEditorKey(editorKey);
        }

        private static BeepColumnType ResolveGridCellEditorFromControlType(string controlType, string editorKey)
        {
            return BeepFieldControlTypeRegistry.SimplifyControlTypeName(controlType) switch
            {
                nameof(BeepCheckBoxBool) or nameof(System.Windows.Forms.CheckBox) => BeepColumnType.CheckBoxBool,
                nameof(BeepDatePicker) or nameof(System.Windows.Forms.DateTimePicker) => BeepColumnType.DateTime,
                nameof(BeepNumericUpDown) or nameof(System.Windows.Forms.NumericUpDown) => BeepColumnType.NumericUpDown,
                nameof(BeepComboBox) or nameof(System.Windows.Forms.ComboBox) => string.Equals(editorKey, "lov", StringComparison.OrdinalIgnoreCase)
                    ? BeepColumnType.ListOfValue
                    : BeepColumnType.ComboBox,
                _ => ResolveGridCellEditorFromEditorKey(editorKey)
            };
        }

        private static BeepColumnType ResolveGridCellEditorFromEditorKey(string editorKey)
        {
            return editorKey.Trim().ToLowerInvariant() switch
            {
                "checkbox" => BeepColumnType.CheckBoxBool,
                "date" => BeepColumnType.DateTime,
                "numeric" => BeepColumnType.NumericUpDown,
                "combo" => BeepColumnType.ComboBox,
                "lov" => BeepColumnType.ListOfValue,
                "option" => BeepColumnType.ComboBox,
                _ => BeepColumnType.Text
            };
        }

        private static DbFieldCategory ResolveGridColumnType(BeepColumnType columnEditor)
        {
            return columnEditor switch
            {
                BeepColumnType.CheckBoxBool => DbFieldCategory.Boolean,
                BeepColumnType.DateTime => DbFieldCategory.DateTime,
                BeepColumnType.NumericUpDown => DbFieldCategory.Numeric,
                BeepColumnType.ComboBox => DbFieldCategory.Enum,
                BeepColumnType.ListOfValue => DbFieldCategory.Enum,
                _ => DbFieldCategory.String
            };
        }

        private static string InferEditorKey(EntityField field)
        {
            string typeName = field.Fieldtype ?? string.Empty;

            if (field.IsCheck ||
                field.FieldCategory == DbFieldCategory.Boolean ||
                typeName.IndexOf("bool", StringComparison.OrdinalIgnoreCase) >= 0 ||
                typeName.IndexOf("bit", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "checkbox";
            }

            if (field.FieldCategory == DbFieldCategory.Enum)
            {
                return "combo";
            }

            if (field.FieldCategory == DbFieldCategory.Date ||
                field.FieldCategory == DbFieldCategory.DateTime ||
                typeName.IndexOf("date", StringComparison.OrdinalIgnoreCase) >= 0 ||
                typeName.IndexOf("time", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "date";
            }

            if (field.FieldCategory == DbFieldCategory.Numeric ||
                field.FieldCategory == DbFieldCategory.Integer ||
                field.FieldCategory == DbFieldCategory.Decimal ||
                field.FieldCategory == DbFieldCategory.Double ||
                field.FieldCategory == DbFieldCategory.Float ||
                field.FieldCategory == DbFieldCategory.Currency ||
                typeName.IndexOf("int", StringComparison.OrdinalIgnoreCase) >= 0 ||
                typeName.IndexOf("decimal", StringComparison.OrdinalIgnoreCase) >= 0 ||
                typeName.IndexOf("double", StringComparison.OrdinalIgnoreCase) >= 0 ||
                typeName.IndexOf("float", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "numeric";
            }

            return "text";
        }

        private static Type ResolveClrType(DbFieldCategory fieldCategory)
        {
            return fieldCategory switch
            {
                DbFieldCategory.Boolean => typeof(bool),
                DbFieldCategory.Date => typeof(DateTime),
                DbFieldCategory.DateTime => typeof(DateTime),
                DbFieldCategory.Integer => typeof(int),
                DbFieldCategory.Numeric => typeof(decimal),
                DbFieldCategory.Decimal => typeof(decimal),
                DbFieldCategory.Double => typeof(double),
                DbFieldCategory.Float => typeof(float),
                DbFieldCategory.Currency => typeof(decimal),
                DbFieldCategory.Enum => typeof(string),
                _ => typeof(string)
            };
        }
    }
}