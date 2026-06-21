using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.Forms.Helpers;
using TheTechIdea.Beep.Winform.Controls;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FieldHost;

public sealed class WinFormDateFieldPresenter : WinFormFieldPresenterBase
{
    public WinFormDateFieldPresenter(EntityField field) : base(field, new BeepDatePicker()) { }
    public override string Key => "Date";
    public override bool CanPresent(object definition) =>
        definition is EntityField field && FieldTypeMapper.GetCanonicalFieldType(field) == "Date";
    public override object CreateEditor(object definition) => new BeepDatePicker();
}
