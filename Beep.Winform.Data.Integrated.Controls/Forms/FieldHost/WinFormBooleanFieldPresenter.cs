using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.Forms.Helpers;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FieldHost;

public sealed class WinFormBooleanFieldPresenter : WinFormFieldPresenterBase
{
    public WinFormBooleanFieldPresenter(EntityField field) : base(field, new BeepCheckBoxBool()) { }
    public override string Key => "Boolean";
    public override bool CanPresent(object definition) =>
        definition is EntityField field &&
        FieldTypeMapper.GetCanonicalFieldType(field) is "Boolean" or "Checkbox";
    public override object CreateEditor(object definition) => new BeepCheckBoxBool();
}
