using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public partial class WinFormFormHost
{
    public ItemInfo? GetItemInfo(string blockName, string fieldName) =>
        TryReadManager(
            manager => manager.ItemProperties.GetItem(
                NormalizeBlockName(blockName),
                fieldName),
            default(ItemInfo));

    public void SetSecurityContext(SecurityContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        RequireManager().SetSecurityContext(context);
        RefreshAllRegisteredBlocks();
    }

    public SecurityContext? GetSecurityContext() =>
        TryReadManager(
            manager => manager.SecurityContext,
            default(SecurityContext));

    public void SetBlockSecurity(string blockName, BlockSecurity security)
    {
        ArgumentNullException.ThrowIfNull(security);
        var name = NormalizeBlockName(blockName);
        RequireManager().SetBlockSecurity(name, security);
        RefreshBlockAndDetails(name);
    }

    public BlockSecurity? GetBlockSecurity(string blockName) =>
        TryReadManager(
            manager => manager.GetBlockSecurity(NormalizeBlockName(blockName)),
            default(BlockSecurity));

    public bool IsBlockAllowed(string blockName, SecurityPermission permission) =>
        TryReadManager(
            manager => manager.IsBlockAllowed(
                NormalizeBlockName(blockName),
                permission),
            false);

    public void SetFieldSecurity(
        string blockName,
        string fieldName,
        FieldSecurity security)
    {
        ArgumentNullException.ThrowIfNull(security);
        var name = NormalizeBlockName(blockName);
        RequireManager().SetFieldSecurity(name, fieldName, security);
        RefreshBlockAndDetails(name);
    }

    public FieldSecurity? GetFieldSecurity(string blockName, string fieldName) =>
        TryReadManager(
            manager => manager.GetFieldSecurity(
                NormalizeBlockName(blockName),
                fieldName),
            default(FieldSecurity));

    public object? GetMaskedFieldValue(
        string blockName,
        string fieldName,
        object? rawValue) =>
        TryReadManager(
            manager => manager.GetMaskedFieldValue(
                NormalizeBlockName(blockName),
                fieldName,
                rawValue!),
            rawValue);

    public IReadOnlyList<SecurityViolationEventArgs> GetSecurityViolations() =>
        TryReadManager(
            manager => manager.GetSecurityViolations(),
            []);

    public void ClearBlockSecurity(string blockName)
    {
        var name = NormalizeBlockName(blockName);
        RequireManager().ClearBlockSecurity(name);
        RefreshBlockAndDetails(name);
    }
}
