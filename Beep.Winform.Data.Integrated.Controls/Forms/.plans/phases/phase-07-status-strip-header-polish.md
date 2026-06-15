# Phase 7: Status Strip & Header Polish

**Priority:** Medium | **Est. Tasks:** 11

## Goal

Enhance the visual information density of `BeepFormsHeader` and `BeepFormsStatusStrip`. Add configurable line presets, severity-based coloring, auto-clear timeout, and workflow history tooltips.

## Dependencies

- Engine: `FormsManager` state properties (complete)
- UI: `BeepFormsHeader`, `BeepFormsStatusStrip`, `BeepFormsViewState` (exist)

## Engine Methods Used (No New Engine Code)

| UI Display | Engine Source | Status |
|------------|---------------|--------|
| Block Count | `_formsManager.Blocks.Count` (or via adapter) | ✅ |
| Form Name | `BeepForms.FormName` | ✅ |
| Connection Status | `IDataSource?.ConnectionStatus` | ✅ |
| All other state | `BeepFormsViewState` (already synced by adapter) | ✅ |

## Implementation Seam

### 7.1 — StatusStrip Enhancements

**Current State:**
The StatusStrip renders shared `BeepFormsViewState` with configurable line presets and a compact rolling workflow history row.

**Enhancements:**

#### Line Presets System
```csharp
[Flags]
public enum BeepFormsStatusStripLines
{
    None = 0,
    Mode = 1,           // "Normal" / "Enter Query" / "Query"
    BlockName = 2,      // "Block: CUSTOMERS"
    RecordPosition = 4, // "Record 3 of 47"
    DirtyState = 8,     // "2 changes" or empty when clean
    Message = 16,       // Current message text
    Connection = 32,    // "Conn: MainDB"
    Aggregate = 64,     // "Sum: $45K | Avg: $962"
    ErrorCount = 128,   // "⚠ 3 errors"
    WorkflowHistory = 256, // Rolling workflow row
    All = ~None
}
```

**Line layout:**
```
┌──────────────────────────────────────────────────────────────────────┐
│ Normal │ CUSTOMERS │ Rec 3/47 │ ⚠ 2 │ $45K │ Conn: MainDB          │
│ ⚠ Name is required                                                  │
│ Last workflow: Rollback completed at 10:23 AM                        │
└──────────────────────────────────────────────────────────────────────┘
  Line 1: Mode | Block | Position | Errors | Aggregates | Connection
  Line 2: Current message (severity-colored)
  Line 3: Workflow history (rolling)
```

#### Severity-Based Coloring

| Severity | Text Color | Background Hint | Auto-Clear |
|----------|-----------|-----------------|------------|
| None | Transparent/inherit | None | N/A |
| Info | DimGray | None | After 10 seconds |
| Success | ForestGreen | Very light green | After 5 seconds |
| Warning | DarkOrange | Very light orange | Manual only |
| Error | Firebrick | Very light red | Manual only |

```csharp
private Color GetMessageColor(BeepFormsMessageSeverity severity)
{
    return severity switch
    {
        BeepFormsMessageSeverity.Success => Color.ForestGreen,
        BeepFormsMessageSeverity.Warning => Color.DarkOrange,
        BeepFormsMessageSeverity.Error => Color.Firebrick,
        _ => SystemColors.GrayText
    };
}
```

#### Auto-Clear Timer
```csharp
private System.Windows.Forms.Timer? _messageClearTimer;

private void ShowMessageWithAutoClear(string text, BeepFormsMessageSeverity severity)
{
    SetMessage(text, severity);
    
    int timeout = severity switch
    {
        BeepFormsMessageSeverity.Success => 5000,
        BeepFormsMessageSeverity.Info => 10000,
        _ => 0 // Warning/Error: no auto-clear
    };
    
    if (timeout > 0)
    {
        _messageClearTimer?.Stop();
        _messageClearTimer = new System.Windows.Forms.Timer { Interval = timeout };
        _messageClearTimer.Tick += (s, e) =>
        {
            _messageClearTimer.Stop();
            ClearMessage();
        };
        _messageClearTimer.Start();
    }
}
```

#### Workflow History Tooltip
- Hovering over the workflow history line shows a tooltip with the last N workflow entries
- Each entry shows: timestamp, action text, severity icon

### 7.2 — BeepFormsHeader Enhancements

**Current State:**
The Header is a separate title/context surface for shared host metadata.

**Enhancements:**

#### Visual Structure
```
┌──────────────────────────────────────────────────────────┐
│ 🗗 Customer Management                    [Normal] [3] ⚠2 │
│   Connection: MainDB · 3 blocks · Last saved: 10:23 AM   │
└──────────────────────────────────────────────────────────┘
  Title text: FormName or "Untitled Form"
  Badges: Mode indicator, block count, dirty count
  Subtitle: Connection name, block count detail, last saved time
```

#### Configurable Elements
```csharp
[Flags]
public enum BeepFormsHeaderElements
{
    None = 0,
    Title = 1,           // Form name text
    Icon = 2,            // Form icon
    ModeBadge = 4,       // Mode indicator badge
    BlockCount = 8,      // "3 blocks" badge
    DirtyBadge = 16,     // "2 changes" badge
    ConnectionStatus = 32, // Connection name and state
    LastSavedTime = 64,  // "Last saved: 10:23 AM"
    SubtitleLine = 128,  // Show the subtitle row at all
    CollapseMode = 256,  // Allow collapsing to small bar
    All = ~None
}
```

#### Mode Badge Styling
| Mode | Badge Text | Color |
|------|-----------|-------|
| Normal | "Normal" | Green |
| Enter Query | "Enter Query" | Blue |
| Query | "Query" | Blue |
| Insert | "Insert" | Orange |

#### Collapse Mode
- When collapsed, header shows only a thin bar (4-6px) with the form name
- Click to expand, or auto-expand on mouse hover
- Configurable via `CollapseMode` flag

## Verification

1. Set `StatusStripLines = All` — verify all lines appear with correct labels
2. Change `ViewState.Mode` to "Enter Query" — verify mode label updates in StatusStrip
3. Show an error message — verify it appears in red and does NOT auto-clear
4. Show a success message — verify it appears in green and auto-clears after 5 seconds
5. Show a warning message — verify it appears in orange and persists
6. Hover over workflow history — verify tooltip shows last entries
7. Set `HeaderElements = All` — verify header shows title, badges, subtitle
8. Set `HeaderElements = Title | ModeBadge` — verify only title and mode badge appear
9. Verify collapse/expand works on header

## New Files

- None (enhancements to existing files)

## Modified Files

- `BeepFormsStatusStrip/BeepFormsStatusStrip.cs` — add line presets, coloring, auto-clear, tooltips
- `BeepFormsHeader/BeepFormsHeader.cs` — add badges, subtitle, collapse mode
- `Models/BeepFormsViewState.cs` — add fields for connection name, last saved time (or read from FormsManager)

## Risks

- **Low:** Pure UI rendering work. No engine changes.
- Auto-clear timer must be handled carefully to avoid timer leaks on control disposal.
- Header collapse mode must not interfere with form layout.
