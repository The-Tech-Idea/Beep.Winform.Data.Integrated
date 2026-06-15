# Phase 1: Record Navigation & CRUD Toolbar

**Priority:** Critical | **Est. Tasks:** 19

## Goal

Add record navigation (First/Previous/Next/Last) and CRUD (Insert/Delete/Duplicate) buttons to the visual shell. Add record position and mode indicators to StatusStrip. Fix keyboard accessibility in existing dialogs.

## Dependencies

- Engine: `FormsManager.Navigation.cs`, `FormsManager.EnhancedOperations.cs`, `FormsManager.BasicDataOps.cs` (all complete)
- UI: `BeepForms.Navigation.cs`, `BeepForms.Commands.cs`, `IBeepFormsCommandRouter` (all exist)

## Engine Methods Used (No New Engine Code)

| UI Action | Engine Method | Status |
|-----------|---------------|--------|
| First Record | `FormsManager.FirstRecordAsync(blockName)` | ✅ |
| Previous Record | `FormsManager.PreviousRecordAsync(blockName)` | ✅ |
| Next Record | `FormsManager.NextRecordAsync(blockName)` | ✅ |
| Last Record | `FormsManager.LastRecordAsync(blockName)` | ✅ |
| Insert Record | `FormsManager.InsertRecordAsync(blockName)` | ✅ |
| Delete Record | `FormsManager.DeleteCurrentRecordAsync(blockName)` | ✅ |
| Duplicate Record | `FormsManager.DuplicateCurrentRecordAsync(blockName)` | ✅ |
| Record Count | `FormsManager.GetBlock(blockName)?.UnitOfWork?.Count` | ✅ |
| Current Index | `FormsManager.GetBlock(blockName)?.UnitOfWork?.CurrentItem` | ✅ |
| Block Mode | `FormsManager.GetBlock(blockName)?.Mode` | ✅ |
| Record Position | `FormsManager.GetCurrentRecordInfo(blockName)` | ✅ |

## Implementation Seam

### 1.1 — BeepFormsRecordNavigationShelf (New Control)

Create a new shelf control following the same pattern as `BeepFormsQueryShelf`:
- Derive from `BaseControl`
- Support `FormsHost` + `AutoBindFormsHost` properties
- Subscribe to `ActiveBlockChanged`, `FormsManagerChanged`, `ViewStateChanged`
- Use `BeepFormsHostResolver.Find(this)` for auto-discovery
- Use `BeepButton` controls with theme support

```
BeepFormsRecordNavigationShelf
├── FlowLayoutPanel _navPanel
│   ├── BeepButton _firstButton  → MoveFirstAsync(blockName)
│   ├── BeepButton _prevButton   → MovePreviousAsync(blockName)
│   ├── BeepLabel  _positionLabel → "Record 3 of 47"
│   ├── BeepButton _nextButton   → MoveNextAsync(blockName)
│   └── BeepButton _lastButton   → MoveLastAsync(blockName)
```

**Properties:**
- `RecordNavigationShelfButtons NavigationButtons` — flags enum (First, Previous, Next, Last, PositionLabel)
- `FlowDirection NavigationFlowDirection` — layout direction
- `bool ShowPositionLabel` — toggle record position label

**Button behaviors:**
- First: enabled when not already at first record
- Previous: enabled when previous record exists
- Next: enabled when next record exists
- Last: enabled when not already at last record
- All update on `ViewStateChanged`

**Keyboard shortcuts:**
- Route `KeyDown` events through host: `PageUp`→First, `PageDown`→Last, `Up`→Previous, `Down`→Next
- Delegate to `BeepForms` which checks focus context

### 1.2 — CRUD Buttons

Add to `BeepFormsCommandBar` (or create `BeepFormsCRUDShelf`):

```
Additions to CommandBar:
├── BeepButton _insertButton    → InsertRecordAsync(blockName)
├── BeepButton _deleteButton    → DeleteCurrentRecordAsync(blockName)  [with confirmation]
└── BeepButton _duplicateButton → DuplicateCurrentRecordAsync(blockName)
```

**Confirmation for Delete:**
```csharp
bool confirmed = await _formsHost.ConfirmAsync("Delete Record", "Are you sure you want to delete this record?");
if (confirmed) { await _formsHost.DeleteCurrentRecordAsync(blockName); }
```

**Enable/disable rules:**
- Insert: enabled when block allows inserts (`InsertAllowed`)
- Delete: enabled when block allows deletes and has records
- Duplicate: enabled when block has a current record

### 1.3 — StatusStrip Enhancements

Add to `BeepFormsStatusStrip`:

| Label | Source | Example |
|-------|--------|---------|
| Mode indicator | `ViewState.IsQueryMode`, block mode | "Normal", "Enter Query", "Query" |
| Record position | `FormsManager.GetCurrentRecordInfo(blockName)` | "Record 3 of 47" |
| Dirty status | `ViewState.IsDirty` | "2 changed records" |
| Block name | `ViewState.ActiveBlockName` | "Block: CUSTOMERS" |
| Connection name | `BeepForms.DataConnection?.ConnectionName` | "Conn: MainDB" |

**Configurable via `BeepFormsStatusStripLinePresets` enum flags** so users can choose which lines appear.

### 1.4 — Quick Fixes

**DriverManagementForm.cs:455-456** — DriverEditDialog inner class:
```csharp
// Uncomment these:
AcceptButton = _btnOk;
CancelButton = _btnCancel;
```

**Logon/SimpleLoginForm.cs:149-150:**
```csharp
// Uncomment these:
AcceptButton = _btnLogin;
CancelButton = _btnCancel;
```

## Verification

1. Drag `BeepFormsRecordNavigationShelf` from toolbox onto a form with `BeepForms`
2. Verify auto-bind discovers the host
3. Click First/Previous/Next/Last — verify record changes and master-detail syncs
4. Verify position label updates ("Record 1 of 10", "Record 2 of 10", etc.)
5. Click Insert — verify new blank record appears
6. Click Delete — verify confirmation dialog, then record removed
7. Click Duplicate — verify record is cloned
8. Verify StatusStrip shows mode, position, dirty state, block name
9. Verify Enter/Esc work in DriverEditDialog and SimpleLoginForm

## New Files

- `BeepFormsRecordNavigationShelf/BeepFormsRecordNavigationShelf.cs` (new control)
- `BeepFormsRecordNavigationShelf/README.md` (optional)
- `Models/BeepFormsRecordNavigationShelfButtons.cs` (new flags enum, or add to existing models)

## Modified Files

- `BeepFormsCommandBar/BeepFormsCommandBar.cs` — add CRUD buttons
- `BeepFormsStatusStrip/BeepFormsStatusStrip.cs` — add indicator labels
- `BeepForms/BeepForms.Commands.cs` — add CRUD routing methods if missing from router
- `DriverManagementForm.cs` — uncomment AcceptButton/CancelButton
- `Logon/SimpleLoginForm.cs` — uncomment AcceptButton/CancelButton
- `Models/BeepFormsCommandBarButtons.cs` — add CRUD flags
- `Models/BeepFormsStatusStripLinePresets.cs` — add new line flags (or new enum)
- `Contracts/IBeepFormsCommandRouter.cs` — add CRUD method signatures if missing

## Risks

- **Low:** All engine methods already exist. UI-only routing work.
- Record position calculation may need fallback if block has no records (show "No records").
- Ensure confirmation dialog doesn't fire for programmatic deletes.
