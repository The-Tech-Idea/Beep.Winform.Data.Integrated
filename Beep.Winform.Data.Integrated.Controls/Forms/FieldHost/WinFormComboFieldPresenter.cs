using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Winform.Controls.ComboBoxes;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FieldHost;

public sealed class WinFormComboFieldPresenter : WinFormFieldPresenterBase
{
    public WinFormComboFieldPresenter(EntityField field) : base(field, new BeepComboBox()) { }
    public override string Key => "Combo";
    public override bool CanPresent(object definition) => definition is EntityField;
    public override object CreateEditor(object definition) => new BeepComboBox();
}
