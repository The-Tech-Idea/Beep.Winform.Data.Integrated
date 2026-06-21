using TheTechIdea.Beep.Editor.UOWManager.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public partial class WinFormFormHost
{
    public IReadOnlyList<ItemInfo> GetItems(string blockName) =>
        TryReadManager(
            manager => manager.ItemProperties.GetAllItems(
                NormalizeBlockName(blockName)),
            []);

    public void SetItemProperty(
        string blockName,
        string fieldName,
        string propertyName,
        object? value)
    {
        var name = NormalizeBlockName(blockName);
        RequireManager().ItemProperties.SetItemProperty(
            name,
            fieldName,
            propertyName,
            value!);
        RefreshBlockAndDetails(name);
    }

    public object? GetItemProperty(
        string blockName,
        string fieldName,
        string propertyName) =>
        TryReadManager(
            manager => manager.ItemProperties.GetItemProperty(
                NormalizeBlockName(blockName),
                fieldName,
                propertyName),
            default(object));

    public void SetItemValue(
        string blockName,
        string fieldName,
        object? value)
    {
        var name = NormalizeBlockName(blockName);
        RequireManager().ItemProperties.SetItemValue(name, fieldName, value!);
        RefreshBlockAndDetails(name);
    }

    public object? GetItemValue(string blockName, string fieldName) =>
        TryReadManager(
            manager => manager.ItemProperties.GetItemValue(
                NormalizeBlockName(blockName),
                fieldName),
            default(object));

    public IReadOnlyDictionary<string, object> GetAllItemValues(string blockName) =>
        TryReadManager(
            manager => (IReadOnlyDictionary<string, object>)
                manager.ItemProperties.GetAllItemValues(
                    NormalizeBlockName(blockName)),
            new Dictionary<string, object>());

    public void SetAllItemValues(
        string blockName,
        IReadOnlyDictionary<string, object> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var name = NormalizeBlockName(blockName);
        RequireManager().ItemProperties.SetAllItemValues(
            name,
            new Dictionary<string, object>(values));
        RefreshBlockAndDetails(name);
    }

    public IReadOnlyList<string> GetDirtyItems(string blockName) =>
        TryReadManager(
            manager => manager.ItemProperties.GetDirtyItems(
                NormalizeBlockName(blockName)),
            []);

    public void ClearItemDirty(string blockName, string fieldName) =>
        RequireManager().ItemProperties.ClearItemDirty(
            NormalizeBlockName(blockName),
            fieldName);

    public void ClearAllItemDirtyFlags(string blockName) =>
        RequireManager().ItemProperties.ClearAllDirtyFlags(
            NormalizeBlockName(blockName));

    public void SetItemError(
        string blockName,
        string fieldName,
        string message)
    {
        var name = NormalizeBlockName(blockName);
        RequireManager().ItemProperties.SetItemError(name, fieldName, message);
        RefreshBlockAndDetails(name);
    }

    public void ClearItemError(string blockName, string fieldName)
    {
        var name = NormalizeBlockName(blockName);
        RequireManager().ItemProperties.ClearItemError(name, fieldName);
        RefreshBlockAndDetails(name);
    }

    public IReadOnlyList<ItemInfo> GetItemsWithErrors(string blockName) =>
        TryReadManager(
            manager => manager.ItemProperties.GetItemsWithErrors(
                NormalizeBlockName(blockName)),
            []);

    public void SetTabOrder(
        string blockName,
        IReadOnlyList<string> fieldNames) =>
        RequireManager().ItemProperties.SetTabOrder(
            NormalizeBlockName(blockName),
            fieldNames);

    public IReadOnlyList<string> GetTabOrder(string blockName) =>
        TryReadManager(
            manager => manager.ItemProperties.GetTabOrder(
                NormalizeBlockName(blockName)),
            []);

    public string? GetNextItem(
        string blockName,
        string currentFieldName) =>
        TryReadManager(
            manager => manager.ItemProperties.GetNextItem(
                NormalizeBlockName(blockName),
                currentFieldName),
            default(string));

    public string? GetPreviousItem(
        string blockName,
        string currentFieldName) =>
        TryReadManager(
            manager => manager.ItemProperties.GetPreviousItem(
                NormalizeBlockName(blockName),
                currentFieldName),
            default(string));

    public IReadOnlyList<ItemInfo> GetEditableItems(
        string blockName,
        FormMode mode) =>
        TryReadManager(
            manager => manager.ItemProperties.GetEditableItems(
                NormalizeBlockName(blockName),
                mode),
            []);
}
