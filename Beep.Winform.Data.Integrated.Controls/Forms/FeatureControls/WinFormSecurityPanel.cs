using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;

public sealed class WinFormSecurityPanel : WinFormFormsFeatureControl
{
    public WinFormSecurityPanel(
        IBeepFormsHost host,
        string? blockName = null)
        : base(host, blockName)
    {
    }

    public void ApplyContext(SecurityContext context) =>
        Host.SetSecurityContext(context);

    public SecurityContext? GetContext() =>
        Host.GetSecurityContext();

    public void SetBlockPolicy(BlockSecurity security) =>
        Host.SetBlockSecurity(RequireBlockName(), security);

    public BlockSecurity? GetBlockPolicy() =>
        Host.GetBlockSecurity(RequireBlockName());

    public bool IsAllowed(SecurityPermission permission) =>
        Host.IsBlockAllowed(RequireBlockName(), permission);

    public void SetFieldPolicy(string fieldName, FieldSecurity security) =>
        Host.SetFieldSecurity(RequireBlockName(), fieldName, security);

    public FieldSecurity? GetFieldPolicy(string fieldName) =>
        Host.GetFieldSecurity(RequireBlockName(), fieldName);

    public IReadOnlyList<SecurityViolationEventArgs> GetViolations() =>
        Host.GetSecurityViolations();

    public void ClearBlockPolicy() =>
        Host.ClearBlockSecurity(RequireBlockName());
}
