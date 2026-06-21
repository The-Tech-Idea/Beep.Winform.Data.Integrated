using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.Forms.Helpers;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FieldHost;

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
}
