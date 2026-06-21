using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Winform.Controls;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;

public sealed class WinFormLockPanel : WinFormFormsFeatureControl
{
    private readonly BeepLabel _status = new()
    {
        Dock = DockStyle.Top,
        Height = 28,
        UseThemeColors = true
    };

    public WinFormLockPanel(IBeepFormsHost host, string blockName)
        : base(host, blockName)
    {
        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44 };
        actions.Controls.Add(CreateButton("Lock", async (_, _) => await LockAsync()));
        actions.Controls.Add(CreateButton("Unlock", (_, _) => Unlock()));
        actions.Controls.Add(CreateButton("Unlock All", (_, _) => UnlockAll()));
        Controls.Add(actions);
        Controls.Add(_status);
        RefreshState();
    }

    public Task<bool> LockAsync(CancellationToken ct = default) =>
        Host.LockCurrentRecordAsync(RequireBlockName(), ct);

    public bool Unlock() => Host.UnlockCurrentRecord(RequireBlockName());

    public void UnlockAll() => Host.UnlockAllRecords(RequireBlockName());

    public IReadOnlyList<RecordLockInfo> GetLocks() =>
        Host.GetAllLocks(RequireBlockName());

    public void RefreshState() =>
        _status.Text = Host.IsCurrentRecordLocked(RequireBlockName())
            ? "Current record locked"
            : "Current record unlocked";
}
