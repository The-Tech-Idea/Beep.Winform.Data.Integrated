using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.UOWManager.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;

public sealed class WinFormItemPropertyPanel : WinFormFormsFeatureControl
{
    public WinFormItemPropertyPanel(
        IBeepFormsHost host,
        string blockName)
        : base(host, blockName)
    {
    }

    public IReadOnlyList<ItemInfo> GetItems() =>
        Host.GetItems(RequireBlockName());

    public void SetProperty(
        string fieldName,
        string propertyName,
        object? value) =>
        Host.SetItemProperty(
            RequireBlockName(),
            fieldName,
            propertyName,
            value);

    public object? GetProperty(
        string fieldName,
        string propertyName) =>
        Host.GetItemProperty(
            RequireBlockName(),
            fieldName,
            propertyName);

    public void SetValue(string fieldName, object? value) =>
        Host.SetItemValue(RequireBlockName(), fieldName, value);

    public object? GetValue(string fieldName) =>
        Host.GetItemValue(RequireBlockName(), fieldName);

    public IReadOnlyDictionary<string, object> GetValues() =>
        Host.GetAllItemValues(RequireBlockName());

    public void SetValues(IReadOnlyDictionary<string, object> values) =>
        Host.SetAllItemValues(RequireBlockName(), values);

    public IReadOnlyList<string> GetDirtyItems() =>
        Host.GetDirtyItems(RequireBlockName());

    public IReadOnlyList<ItemInfo> GetItemsWithErrors() =>
        Host.GetItemsWithErrors(RequireBlockName());

    public void SetError(string fieldName, string message) =>
        Host.SetItemError(RequireBlockName(), fieldName, message);

    public void ClearError(string fieldName) =>
        Host.ClearItemError(RequireBlockName(), fieldName);

    public void SetTabOrder(IReadOnlyList<string> fieldNames) =>
        Host.SetTabOrder(RequireBlockName(), fieldNames);

    public IReadOnlyList<string> GetTabOrder() =>
        Host.GetTabOrder(RequireBlockName());

    public string? GetNextItem(string currentFieldName) =>
        Host.GetNextItem(RequireBlockName(), currentFieldName);

    public string? GetPreviousItem(string currentFieldName) =>
        Host.GetPreviousItem(RequireBlockName(), currentFieldName);

    public IReadOnlyList<ItemInfo> GetEditableItems(FormMode mode) =>
        Host.GetEditableItems(RequireBlockName(), mode);
}
