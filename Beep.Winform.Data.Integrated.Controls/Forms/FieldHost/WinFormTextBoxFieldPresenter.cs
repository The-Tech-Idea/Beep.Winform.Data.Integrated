using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.Forms.Helpers;
using TheTechIdea.Beep.Winform.Controls.TextFields;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FieldHost;

public sealed class WinFormTextBoxFieldPresenter : WinFormFieldPresenterBase
{
    public WinFormTextBoxFieldPresenter(EntityField field) : base(field, Create(field)) { }
    public override string Key => "Text";
    public override bool CanPresent(object definition) =>
        definition is EntityField field && FieldTypeMapper.GetCanonicalFieldType(field) is "Text" or "ReadOnly";
    public override object CreateEditor(object definition) => Create((EntityField)definition);
    private static BeepTextBox Create(EntityField field) => new()
    {
        PlaceholderText = field.Description ?? field.Caption ?? field.FieldName,
        MaxLength = field.MaxLength > 0 ? field.MaxLength : field.Size,
        ReadOnly = field.IsReadOnly || field.IsIdentity
    };
}
