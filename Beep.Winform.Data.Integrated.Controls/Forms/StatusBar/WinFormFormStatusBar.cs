using System.ComponentModel;
using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Winform.Controls.Badges;
using TheTechIdea.Beep.Winform.Controls.StatusBars;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.StatusBar;

/// <summary>
/// The form's message line and record-status indicators — Oracle Forms' status
/// line.
/// </summary>
/// <remarks>
/// <para>
/// Thin by construction. Every value shown here is read from the engine:
/// messages arrive on <see cref="IBeepFormsHost.MessageRaised"/> /
/// <see cref="IBeepFormsHost.MessageCleared"/>, which <c>FormsManager</c> raises
/// through its message queue, and the record indicators come from
/// <see cref="IBeepFormsHost.GetBlockStatus"/>. This class decides nothing; it
/// maps engine state onto <see cref="BeepStatusBar"/> segments.
/// </para>
/// <para>
/// The engine end of this was already complete and had no consumer — the
/// interface comment on <c>OnMessage</c>/<c>OnMessageCleared</c> says "UI layers
/// subscribe to display messages", and until 2026-08-01 no UI layer did, so
/// every <c>SetMessage</c> call in the engine went nowhere.
/// </para>
/// </remarks>
[ToolboxItem(true)]
[DisplayName("Beep Form Status Bar")]
[Category("Beep Forms")]
[Description("The form's message line and record-status indicators. Binds itself to the form host on the same form.")]
public class WinFormFormStatusBar : BeepStatusBar
{
    /// <summary>Segment key for "Record n of m".</summary>
    public const string PositionSegment = "position";

    /// <summary>Segment key for the block's mode.</summary>
    public const string ModeSegment = "mode";

    /// <summary>Segment key for the unsaved-changes indicator.</summary>
    public const string ChangedSegment = "changed";

    private IBeepFormsHost? _formsHost;
    private string _blockName = string.Empty;

    /// <summary>
    /// The block whose record status is shown. Empty means "follow whichever
    /// block is active", which is the Oracle Forms behaviour.
    /// </summary>
    [Browsable(true)]
    [Category("Beep")]
    [Description("Block whose record status is shown; empty follows the active block.")]
    [DefaultValue("")]
    public string BlockName
    {
        get => _blockName;
        set
        {
            _blockName = value ?? string.Empty;
            SyncFromManager();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IBeepFormsHost? FormsHost => _formsHost;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsBound => _formsHost is not null;

    public void Bind(IBeepFormsHost host)
    {
        if (host is null) return;

        Unbind();

        _formsHost = host;
        _formsHost.MessageRaised += HostMessageRaised;
        _formsHost.MessageCleared += HostMessageCleared;
        _formsHost.ActiveBlockChanged += HostActiveBlockChanged;

        SyncFromManager();
    }

    public void Unbind()
    {
        if (_formsHost is null) return;

        _formsHost.MessageRaised -= HostMessageRaised;
        _formsHost.MessageCleared -= HostMessageCleared;
        _formsHost.ActiveBlockChanged -= HostActiveBlockChanged;
        _formsHost = null;
    }

    /// <summary>
    /// Re-reads record status from the engine. Call after any operation that
    /// moves the record pointer or changes dirty state.
    /// </summary>
    public void SyncFromManager()
    {
        var block = ResolveBlockName();
        if (_formsHost is null || string.IsNullOrWhiteSpace(block))
        {
            ClearSegments();
            return;
        }

        // Nothing to show for a block this host does not carry. (Note that
        // IsBlockRegistered asks the *host's* view registry, not the engine —
        // it says the host knows the block, not that a manager exists. The host
        // is what defers binding until it has a manager.)
        if (!_formsHost.IsBlockRegistered(block))
        {
            ClearSegments();
            return;
        }

        var status = _formsHost.GetBlockStatus(block);
        if (status is null)
        {
            ClearSegments();
            return;
        }

        // CurrentRecordIndex is zero-based; Oracle Forms counts from 1.
        SetSegment(
            PositionSegment,
            status.RecordCount > 0
                ? $"Record {status.CurrentRecordIndex + 1} of {status.RecordCount}"
                : "No records");

        SetSegment(
            ModeSegment,
            status.IsInQueryMode ? "Enter Query" : status.CurrentMode ?? string.Empty);

        if (status.HasUnsavedChanges)
        {
            SetSegment(ChangedSegment, "Changed", ValidationState.Warning);
        }
        else
        {
            RemoveSegment(ChangedSegment);
        }
    }

    private string ResolveBlockName() =>
        string.IsNullOrWhiteSpace(_blockName)
            ? _formsHost?.ActiveBlockName ?? string.Empty
            : _blockName;

    private void HostMessageRaised(object? sender, FormsHostMessageEventArgs e)
    {
        SetMessage(e.Message, SeverityFor(e.Level));
        SyncFromManager();
    }

    private void HostMessageCleared(object? sender, FormsHostMessageEventArgs e) =>
        ClearMessage();

    private void HostActiveBlockChanged(object? sender, EventArgs e) => SyncFromManager();

    private static ValidationState SeverityFor(MessageLevel level) => level switch
    {
        MessageLevel.Error => ValidationState.Error,
        MessageLevel.Warning => ValidationState.Warning,
        MessageLevel.Success => ValidationState.Success,
        _ => ValidationState.Info
    };

    protected override void Dispose(bool disposing)
    {
        if (disposing) Unbind();
        base.Dispose(disposing);
    }
}
