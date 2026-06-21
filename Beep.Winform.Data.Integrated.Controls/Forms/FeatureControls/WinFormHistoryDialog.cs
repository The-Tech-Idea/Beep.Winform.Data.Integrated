using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.UOW.Models;
using TheTechIdea.Beep.Editor.UOWManager.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;

public sealed class WinFormHistoryDialog : Form
{
    private readonly IBeepFormsHost _host;
    private readonly string _blockName;

    public WinFormHistoryDialog(IBeepFormsHost host, string blockName)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _blockName = blockName?.Trim() ?? throw new ArgumentNullException(nameof(blockName));
        Text = $"{_blockName} History";
        Width = 640;
        Height = 420;
        StartPosition = FormStartPosition.CenterParent;
    }

    public IReadOnlyList<NavigationHistoryEntry> GetNavigationHistory() =>
        _host.GetNavigationHistory(_blockName);

    public IReadOnlyList<QueryHistoryEntry> GetQueryHistory() =>
        _host.GetQueryHistory(_blockName);

    public Task<bool> BackAsync() => _host.NavigateBackAsync(_blockName);

    public Task<bool> ForwardAsync() => _host.NavigateForwardAsync(_blockName);
}
