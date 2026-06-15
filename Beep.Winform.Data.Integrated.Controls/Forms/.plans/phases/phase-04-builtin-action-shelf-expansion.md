# Phase 4: Built-in Action Shelf Expansion

**Priority:** High | **Est. Tasks:** 13

## Goal

Expose more of the 30+ Oracle Forms built-ins already available in `IBeepBuiltins`. Add block navigation arrows, record/block clearing, Post, Refresh, and Duplicate buttons. Expand `BeepFormsToolbar` with configurable button visibility.

## Dependencies

- Engine: `IBeepBuiltins` interface (complete, 30+ methods)
- UI: `BeepBuiltinsHostAdapter`, `BeepFormsToolbar` (exist)

## Engine Methods Used (No New Engine Code)

All methods below are on `IBeepBuiltins` and route through `BeepBuiltinsHostAdapter` → `FormsManager`:

| UI Button | Engine Method | Category |
|-----------|---------------|----------|
| Clear Block | `IBeepBuiltins.ClearBlock()` | Record/Block Ops |
| Clear Record | `IBeepBuiltins.ClearRecord()` | Record/Block Ops |
| Clear Form | `IBeepBuiltins.ClearForm()` | Record/Block Ops |
| Post | `IBeepBuiltins.Post()` | Transaction |
| Refresh Block | `IBeepBuiltins.RefreshBlock()` | Data Ops |
| Next Block | `IBeepBuiltins.NextBlock()` | Block Navigation |
| Previous Block | `IBeepBuiltins.PreviousBlock()` | Block Navigation |
| First Block | `IBeepBuiltins.FirstBlock()` | Block Navigation |
| Last Block | `IBeepBuiltins.LastBlock()` | Block Navigation |
| Go Block | `IBeepBuiltins.GoBlock(blockName)` | Block Navigation |
| Show LOV | `IBeepBuiltins.ShowLov()` | LOV |
| Enter Query | `IBeepBuiltins.EnterQuery()` | Query |
| Execute Query | `IBeepBuiltins.ExecuteQuery()` | Query |
| Exit Query | `IBeepBuiltins.ExitQuery()` | Query |
| Commit | `IBeepBuiltins.Commit()` | Transaction |
| Rollback | `IBeepBuiltins.Rollback()` | Transaction |

## Implementation Seam

### 4.1 — Record & Block Operations on Toolbar

Expand `BeepFormsToolbar` to include new buttons:

```
Current BeepFormsToolbar:
├── [Savepoints section]
│   ├── Create Savepoint
│   ├── List Savepoints
│   ├── Rollback to Savepoint
│   └── Release All Savepoints
├── [Alerts section]
│   ├── Info Alert
│   ├── Warning Alert
│   └── Error Alert

Additions:
├── [Record Operations section]
│   ├── Clear Record    → _builtins.ClearRecord()
│   ├── Clear Block     → _builtins.ClearBlock()
│   ├── Clear Form      → _builtins.ClearForm()
│   ├── Post            → _builtins.Post()
│   └── Refresh Block   → _builtins.RefreshBlock() [or via CommandRouter]
├── [Data Operations section]
│   ├── Duplicate Record → via CommandRouter
│   ├── Insert Record    → via CommandRouter
│   └── Delete Record    → via CommandRouter
```

**Button flags enum:**
```csharp
[Flags]
public enum BeepFormsToolbarButtons
{
    // Savepoints
    SavepointCreate = 1,
    SavepointList = 2,
    SavepointRollback = 4,
    SavepointReleaseAll = 8,
    
    // Alerts
    AlertInfo = 16,
    AlertWarning = 32,
    AlertError = 64,
    
    // Record Operations (NEW)
    ClearRecord = 128,
    ClearBlock = 256,
    ClearForm = 512,
    Post = 1024,
    RefreshBlock = 2048,
    
    // Data Operations (NEW)
    DuplicateRecord = 4096,
    InsertRecord = 8192,
    DeleteRecord = 16384,
    
    // Presets
    Savepoints = SavepointCreate | SavepointList | SavepointRollback | SavepointReleaseAll,
    Alerts = AlertInfo | AlertWarning | AlertError,
    RecordOps = ClearRecord | ClearBlock | ClearForm | Post | RefreshBlock,
    DataOps = DuplicateRecord | InsertRecord | DeleteRecord,
    All = Savepoints | Alerts | RecordOps | DataOps
}
```

**Confirmation dialogs for destructive operations:**
```csharp
// Clear Block
if (await _formsHost.ConfirmAsync("Clear Block", "Clear all records in this block?"))
    _builtins.ClearBlock();

// Clear Form
if (await _formsHost.ConfirmAsync("Clear Form", "Clear all records in all blocks?"))
    _builtins.ClearForm();

// Clear Record
// No confirmation needed (undo-able)
```

### 4.2 — Block Navigation Buttons

Add block navigation to `BeepFormsCommandBar`:

```
Current CommandBar:
├── [Block Selector] dropdown
└── [Sync] button

Additions:
├── ◀ (Previous Block)    → _builtins.PreviousBlock()
├── [Block Selector] enhanced with GoBlock
├── ▶ (Next Block)        → _builtins.NextBlock()
├── |◀ (First Block)      → _builtins.FirstBlock()  [optional]
└── ▶| (Last Block)       → _builtins.LastBlock()   [optional]
```

**Button behavior:**
- Previous Block: cycle to previous block in registration order, wrap to last
- Next Block: cycle to next block in registration order, wrap to first
- Both buttons use `<` and `>` or `◀` and `▶` Unicode arrows
- Tooltip shows target block name on hover

**Block selector enhancement:**
- Current block selector already shows a dropdown of registered blocks
- Selecting a block already calls `SwitchToBlockAsync` (which is `GoBlock`)
- Enhance with: visual indicator for dirty blocks (asterisk), master/detail indent

### 4.3 — Button Routing Pattern

All new toolbar buttons route through the same pattern:

```csharp
private async void ClearBlockButton_Click(object? sender, EventArgs e)
{
    if (_formsHost == null) return;
    try
    {
        bool confirmed = await _formsHost.ConfirmAsync(
            "Clear Block",
            "This will clear all records in the current block. Continue?").ConfigureAwait(true);
        if (!confirmed) return;
        
        _formsHost.Builtins?.ClearBlock();
        _formsHost.SyncFromManager();
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"[BeepFormsToolbar.ClearBlock] {ex.Message}");
        _formsHost.ShowError($"Failed to clear block: {ex.Message}");
    }
}
```

**Key pattern:** The shelf only calls `_formsHost.Builtins?.X()` or `_formsHost.X()`. Never calls `FormsManager` directly.

## Verification

1. Add all new toolbar buttons — verify they appear in correct sections
2. Click Clear Record — verify current record is cleared, form resets to blank
3. Click Clear Block — verify confirmation dialog, then all records cleared
4. Click Clear Form — verify confirmation dialog, then all blocks cleared
5. Click Post — verify record is saved to database without committing transaction
6. Click Refresh Block — verify block re-queries from data source
7. Click Previous Block / Next Block — verify active block changes
8. Verify block selector shows dirty indicator (*) on blocks with unsaved changes
9. Verify button visibility respects flags enum (set `ClearBlock | ClearForm | ClearRecord` only)

## New Files

- None (enhancements to existing files)

## Modified Files

- `BeepFormsToolbar/BeepFormsToolbar.cs` — add new buttons, expand flags enum
- `BeepFormsToolbar/BeepFormsToolbar.Actions.cs` — add action handlers
- `BeepFormsCommandBar/BeepFormsCommandBar.cs` — add block navigation arrows
- `Models/BeepFormsToolbarConfiguration.cs` — expand configuration (if used)
- `Models/BeepFormsCommandBarButtons.cs` — add block navigation flags

## Risks

- **Low:** All engine methods exist. Pure UI button work.
- Destructive operations (Clear Block, Clear Form, Clear Record) need confirmation dialogs.
- Post button may be confusing to users unfamiliar with Oracle Forms — tooltip and documentation needed.
- Ensure toolbar doesn't overflow — consider wrapping or scrolling for many buttons.
