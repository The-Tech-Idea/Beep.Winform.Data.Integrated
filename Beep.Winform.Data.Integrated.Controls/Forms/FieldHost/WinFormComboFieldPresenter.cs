using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Winform.Controls.ComboBoxes;
using TheTechIdea.Beep.Winform.Controls.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FieldHost;

public sealed class WinFormComboFieldPresenter : WinFormFieldPresenterBase
{
    public WinFormComboFieldPresenter(EntityField field) : base(field, new BeepComboBox()) { }
    private BeepComboBox ComboEditor => (BeepComboBox)Editor;
    public override string Key => "Combo";
    public void SetLovSelection(object? value, object? displayValue)
    {
        var item = ComboEditor.ListItems.FirstOrDefault(candidate =>
            Equals(candidate.Value, value) || Equals(candidate.Item, value));
        if (item is null)
        {
            item = new SimpleItem
            {
                Text = displayValue?.ToString() ?? value?.ToString() ?? string.Empty,
                Value = value,
                Item = value,
            };
            ComboEditor.ListItems.Add(item);
        }
        else
        {
            item.Text = displayValue?.ToString() ?? item.Text;
        }

        SetValue(value);
    }
    public override bool CanPresent(object definition) => definition is EntityField;
    public override object CreateEditor(object definition) => new BeepComboBox();
}
