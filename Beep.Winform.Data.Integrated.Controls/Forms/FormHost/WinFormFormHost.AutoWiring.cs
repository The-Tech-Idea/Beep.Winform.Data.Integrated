using System.Windows.Forms;
using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Winform.Data.Integrated.Forms.StatusBar;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

/// <summary>
/// Finds the block views and status surfaces on the form and wires them up,
/// so a developer never writes the wiring by hand.
/// </summary>
/// <remarks>
/// <para>
/// Until 2026-08-01 every block view had to be passed to <c>RegisterBlock</c>
/// and every status bar to <c>Bind</c>, by hand, in the right order, after the
/// manager was assigned. Nothing did that automatically — which meant the
/// runtime only worked for someone who already knew the sequence, and the
/// integration harness had to perform it too. If a harness has to hand-wire a
/// form, so does every user of the product.
/// </para>
/// <para>
/// Discovery is idempotent and re-runs whenever the tree changes, so a control
/// dropped on the designer surface, added at runtime, or created before the
/// manager is assigned all end up wired the same way.
/// </para>
/// </remarks>
public partial class WinFormFormHost
{
    private readonly List<WinFormFormStatusBar> _statusSurfaces = [];
    private readonly HashSet<Control> _watchedContainers = [];

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        AutoWireSurfaces();
    }

    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);
        AutoWireSurfaces();
    }

    /// <summary>
    /// Walks the form and attaches every block view and status surface it finds.
    /// Safe to call repeatedly; anything already attached is left alone.
    /// </summary>
    public void AutoWireSurfaces()
    {
        if (IsDisposed || Disposing) return;

        RunOnUi(() =>
        {
            // Search from the top-level form when there is one — block views are
            // usually siblings of the host, not children of it.
            Control root = TopLevelControl as Control ?? this;
            AttachTree(root);
        });
    }

    private void AttachTree(Control control)
    {
        if (control is null) return;

        Watch(control);

        foreach (Control child in control.Controls)
        {
            switch (child)
            {
                case IBlockView block:
                    TryAdoptBlock(block);
                    break;
                case WinFormFormStatusBar status:
                    TryAdoptStatusSurface(status);
                    break;
            }

            AttachTree(child);
        }
    }

    private void Watch(Control container)
    {
        if (!_watchedContainers.Add(container)) return;
        container.ControlAdded += ContainerControlAdded;
        container.Disposed += ContainerDisposed;
    }

    private void ContainerControlAdded(object sender, ControlEventArgs e) =>
        AutoWireSurfaces();

    private void ContainerDisposed(object sender, EventArgs e)
    {
        if (sender is not Control container) return;
        container.ControlAdded -= ContainerControlAdded;
        container.Disposed -= ContainerDisposed;
        _watchedContainers.Remove(container);
    }

    private void TryAdoptBlock(IBlockView block)
    {
        if (string.IsNullOrWhiteSpace(block.BlockName)) return;

        var blockName = NormalizeBlockName(block.BlockName);

        // Already ours, or a different view already claims the name — either
        // way discovery must not throw. RegisterBlock does throw on a duplicate,
        // and that is right for an explicit call; a sweep of the control tree is
        // not an explicit call.
        if (_blocks.ContainsKey(blockName)) return;

        RegisterBlockCore(block);
    }

    private void TryAdoptStatusSurface(WinFormFormStatusBar status)
    {
        if (!_statusSurfaces.Contains(status))
        {
            _statusSurfaces.Add(status);
            status.Disposed += StatusSurfaceDisposed;
        }

        BindStatusSurfaceIfReady(status);
    }

    /// <summary>
    /// Binds a status surface once there is a manager to read.
    /// </summary>
    /// <remarks>
    /// Discovery runs as soon as a control appears on the form, which is often
    /// before the generated code assigns a manager. Binding then would have the
    /// surface immediately read block status, and the host throws on that by
    /// design for explicit callers. So adoption and binding are separate steps,
    /// and <c>ReplaceFormsManager</c> sweeps again once a manager exists.
    /// </remarks>
    private void BindStatusSurfaceIfReady(WinFormFormStatusBar status)
    {
        if (_formsManager is null || status.IsBound) return;
        status.Bind(this);
    }

    private void StatusSurfaceDisposed(object sender, EventArgs e)
    {
        if (sender is not WinFormFormStatusBar status) return;
        status.Disposed -= StatusSurfaceDisposed;
        _statusSurfaces.Remove(status);
    }

    /// <summary>
    /// Refreshes every discovered status surface. Called wherever registered
    /// blocks are refreshed, so a status line tracks the engine without the
    /// caller remembering to poke it.
    /// </summary>
    private void RefreshStatusSurfaces()
    {
        foreach (var status in _statusSurfaces)
        {
            status.SyncFromManager();
        }
    }
}
