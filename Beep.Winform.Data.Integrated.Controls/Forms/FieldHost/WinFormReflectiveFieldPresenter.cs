using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls.TextFields;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FieldHost;

public sealed class WinFormReflectiveFieldPresenter : WinFormFieldPresenterBase
{
    public WinFormReflectiveFieldPresenter(EntityField field, IBeepUIComponent? editor = null)
        : base(field, editor ?? new BeepTextBox()) { }
    public override string Key => "Fallback";
    public override bool CanPresent(object definition) => definition is EntityField;
    public override object CreateEditor(object definition) => new BeepTextBox();
}
