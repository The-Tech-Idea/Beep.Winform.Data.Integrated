using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.Forms.Helpers;
using TheTechIdea.Beep.Winform.Controls.Numerics;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FieldHost;

public sealed class WinFormNumericFieldPresenter : WinFormFieldPresenterBase
{
    public WinFormNumericFieldPresenter(EntityField field) : base(field, new BeepNumericUpDown()) { }
    public override string Key => "Numeric";
    public override bool CanPresent(object definition) =>
        definition is EntityField field && FieldTypeMapper.GetCanonicalFieldType(field) == "Numeric";
    public override object CreateEditor(object definition) => new BeepNumericUpDown();
}
