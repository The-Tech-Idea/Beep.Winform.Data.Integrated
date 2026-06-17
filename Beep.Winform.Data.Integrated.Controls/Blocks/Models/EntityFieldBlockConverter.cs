using System;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Services;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;

internal static class EntityFieldBlockConverter
{
    public static BeepFieldDefinition ToFieldDefinition(EntityField field, int order)
    {
        var defaults = BeepFieldControlTypeRegistry.ResolveDefaultFieldSettings(
            ResolveCategory(field.Fieldtype),
            field.Fieldtype ?? string.Empty,
            field.IsCheck,
            field.IsLong);

        return new BeepFieldDefinition
        {
            FieldName = field.FieldName ?? string.Empty,
            Label = !string.IsNullOrWhiteSpace(field.Description)
                ? field.Description
                : (field.FieldName ?? string.Empty),
            EditorKey = defaults.EditorKey,
            ControlType = defaults.ControlType,
            BindingProperty = BeepFieldControlTypeRegistry.ResolveDefaultBindingProperty(defaults.ControlType, defaults.EditorKey),
            Order = order,
            Width = ResolveWidth(field),
            IsVisible = !field.IsHidden,
            IsReadOnly = field.IsReadOnly || field.IsAutoIncrement || field.IsIdentity || field.IsRowVersion,
            DefaultValue = field.DefaultValue?.ToString() ?? string.Empty
        };
    }

    private static DbFieldCategory ResolveCategory(string? fieldType)
    {
        if (string.IsNullOrWhiteSpace(fieldType)) return DbFieldCategory.String;
        string t = fieldType.ToLowerInvariant();
        if (t.Contains("int") || t.Contains("bit")) return DbFieldCategory.Integer;
        if (t.Contains("decimal") || t.Contains("double") || t.Contains("float") || t.Contains("numeric") || t.Contains("money")) return DbFieldCategory.Decimal;
        if (t.Contains("date") || t.Contains("time")) return DbFieldCategory.DateTime;
        if (t.Contains("bool")) return DbFieldCategory.Boolean;
        if (t.Contains("text") || t.Contains("char") || t.Contains("varchar") || t.Contains("nchar") || t.Contains("nvarchar")) return DbFieldCategory.String;
        return DbFieldCategory.String;
    }

    private static int ResolveWidth(EntityField field)
    {
        var category = ResolveCategory(field.Fieldtype);
        if (field.IsCheck || category == DbFieldCategory.Boolean)
            return 120;
        if (category == DbFieldCategory.Date || category == DbFieldCategory.DateTime)
            return 180;
        if (category == DbFieldCategory.Enum)
            return 220;
        if (category == DbFieldCategory.Integer)
            return 120;
        if (category is DbFieldCategory.Decimal or DbFieldCategory.Double or DbFieldCategory.Float or DbFieldCategory.Numeric or DbFieldCategory.Currency)
            return 160;
        if (field.IsLong)
            return 320;
        int raw = field.Size > 0 ? field.Size : 160;
        int width = raw <= 24 ? raw * 12 : raw * 2;
        return Math.Max(120, Math.Min(320, width));
    }
}
