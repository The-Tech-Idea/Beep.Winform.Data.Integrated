using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.Forms.Helpers;
using TheTechIdea.Beep.Winform.Controls.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FieldHost;

/// <summary>
/// Decides which Beep control renders a field, for both single-record and
/// multi-record (grid) mode.
/// </summary>
/// <remarks>
/// <b>This is the only place that decision is made on WinForms.</b> What a
/// field *is* comes from BeepDM — <see cref="FieldTypeMapper"/> — so the IDE,
/// WinForms and WPF all agree on it. Binding that to a concrete control is
/// necessarily per-platform, and it happens here once, for both modes.
/// <para>
/// Grid mode briefly carried its own switch over the same canonical values.
/// Two mappings drift: a date field would be a picker on the form and a text
/// box in the grid.
/// </para>
/// </remarks>
public sealed class WinFormFieldPresenterRegistry
{
    public WinFormFieldPresenterBase Create(EntityField field, bool hasLov = false)
    {
        ArgumentNullException.ThrowIfNull(field);
        if (hasLov) return new WinFormComboFieldPresenter(field);
        return FieldTypeMapper.GetCanonicalFieldType(field) switch
        {
            "Numeric" => new WinFormNumericFieldPresenter(field),
            "Date" => new WinFormDateFieldPresenter(field),
            "Boolean" or "Checkbox" => new WinFormBooleanFieldPresenter(field),
            "ReadOnly" => new WinFormTextBoxFieldPresenter(field) { IsReadOnly = true },
            _ => new WinFormTextBoxFieldPresenter(field)
        };
    }

    /// <summary>
    /// The grid cell editor for a field — the multi-record counterpart of
    /// <see cref="Create"/>, resolved from the same canonical type so the two
    /// modes cannot disagree.
    /// </summary>
    public BeepColumnType ResolveColumnType(EntityField field, bool hasLov = false)
    {
        ArgumentNullException.ThrowIfNull(field);
        if (hasLov) return BeepColumnType.ListOfValue;
        return FieldTypeMapper.GetCanonicalFieldType(field) switch
        {
            "Numeric" => BeepColumnType.NumericUpDown,
            "Date" => BeepColumnType.DateTime,
            "Boolean" or "Checkbox" => BeepColumnType.CheckBoxBool,
            _ => BeepColumnType.Text,
        };
    }
}
