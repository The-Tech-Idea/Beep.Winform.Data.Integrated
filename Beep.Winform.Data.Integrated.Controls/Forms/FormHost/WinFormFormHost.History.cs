using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Models;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public partial class WinFormFormHost
{
    public Task<bool> NavigateBackAsync(string blockName) =>
        ExecuteAndRefreshAsync(
            blockName,
            (manager, name) => manager.NavigateBackAsync(name));

    public Task<bool> NavigateForwardAsync(string blockName) =>
        ExecuteAndRefreshAsync(
            blockName,
            (manager, name) => manager.NavigateForwardAsync(name));

    public bool CanNavigateBack(string blockName) =>
        RequireManager().CanNavigateBack(NormalizeBlockName(blockName));

    public bool CanNavigateForward(string blockName) =>
        RequireManager().CanNavigateForward(NormalizeBlockName(blockName));

    public IReadOnlyList<NavigationHistoryEntry> GetNavigationHistory(string blockName) =>
        RequireManager().GetNavigationHistory(NormalizeBlockName(blockName));

    public void ClearNavigationHistory(string blockName) =>
        RequireManager().ClearNavigationHistory(NormalizeBlockName(blockName));

    public void SetBookmark(string blockName, string bookmarkName) =>
        RequireManager().SetBlockBookmark(
            NormalizeBlockName(blockName),
            bookmarkName);

    public bool GoToBookmark(string blockName, string bookmarkName)
    {
        var name = NormalizeBlockName(blockName);
        var result = RequireManager().GoToBlockBookmark(name, bookmarkName);
        if (result)
        {
            RefreshBlockAndDetails(name);
        }
        return result;
    }

    public void RemoveBookmark(string blockName, string bookmarkName) =>
        RequireManager().RemoveBlockBookmark(
            NormalizeBlockName(blockName),
            bookmarkName);

    public void ClearBookmarks(string blockName) =>
        RequireManager().ClearBlockBookmarks(
            NormalizeBlockName(blockName));
}
