# Phase 10: Grid-Driven Multi-Record Block View

**Priority:** Medium | **Est. Tasks:** 12

## Goal

Support Oracle Forms multi-record block style where records are displayed in a grid (tabular) view alongside the single-record form view. Add Form/Grid view toggle and inline editing with full validation/trigger pipeline.

## Dependencies

- Engine: `IUnitofWork` — all CRUD, navigation, events (complete)
- Engine: `FormsManager` — validation, triggers, LOV (complete)
- UI: `BeepBlock` (exists), `BeepGridPro` (exists)

## Engine Methods Used (No New Engine Code)

The grid reads/writes through the same `IBeepFormsHost` interface. No new engine code.

| Grid Action | Engine Path | Status |
|-------------|-------------|--------|
| Load Records | `IUnitofWork.Get(filters)` | ✅ |
| Navigate Record | `IUnitofWork.MoveTo(index)` | ✅ |
| Edit Cell Value | `IUnitofWork` item property setter | ✅ |
| Validate Cell | `ValidationManager.ValidateItem` | ✅ |
| Insert Row | `FormsManager.InsertRecordAsync` | ✅ |
| Delete Row | `FormsManager.DeleteCurrentRecordAsync` | ✅ |
| Show LOV | `FormsManager.ShowLOVAsync` | ✅ |

## Implementation Seam

### 10.1 — Grid View Mode on BeepBlock

Add a view mode toggle to `BeepBlock`:

```csharp
public enum BeepBlockViewMode
{
    Form,   // Single-record form view (current behavior)
    Grid    // Multi-record grid view (new)
}
```

**Grid rendering:**
```csharp
// In BeepBlock, when ViewMode == Grid:
private BeepGridPro? _gridView;

private void ShowGridView()
{
    if (_gridView == null)
    {
        _gridView = new BeepGridPro
        {
            Dock = DockStyle.Fill,
            // Bind to the block's IUnitofWork
            DataSource = _host?.GetBlockUnitOfWork(BlockName)
        };
        _gridView.CellValueChanged += GridView_CellValueChanged;
        _gridView.SelectionChanged += GridView_SelectionChanged;
        _gridView.CellClick += GridView_CellClick;
    }
    
    _formViewPanel?.Hide();
    _gridView.Show();
}
```

**Form/Grid synchronization:**
```csharp
// When toggling from Form to Grid:
// Set grid's current row to the form's current record index

// When toggling from Grid to Form:
// Set form's current record to the grid's selected row index

// On grid row selection change:
// Sync the block's current record position via IBeepFormsHost
```

#### Grid Column Configuration
- Auto-generate columns from entity structure (same as form fields)
- LOV fields show as dropdown columns
- Date fields show with date formatting
- Read-only fields show as non-editable columns
- Hidden fields (per field security) are excluded

#### Grid Inline Editing
- Cell edit starts on double-click or F2
- On cell value change:
  1. Set property on the current `IUnitofWork` item
  2. Fire `ItemChanged` event (triggers validation)
  3. Fire `WHEN-VALIDATE-ITEM` trigger via FormsManager
  4. Update dirty state
- On validation failure:
  - Show error tooltip on the cell
  - Prevent leaving the cell (or highlight in red)

#### Grid Features
- **Sorting:** Click column header to sort (sorts the UoW's in-memory list or re-queries with ORDER BY)
- **Filtering:** Per-column filter row (optional, configurable)
- **Column reordering:** Drag columns to reorder
- **Column resizing:** Drag column edges
- **Multi-select:** Ctrl+Click or Shift+Click for batch operations

#### View Toggle Button
Add a toggle button to `BeepFormsCommandBar`:

```
[Form View ◫] ← toggles to → [Grid View ⊞]
```

Button shows current mode and toggles on click. View mode is per-block and persisted during session.

### 10.2 — Grid-Specific Behaviors

#### Record Selector
- Current record is highlighted with a row selector triangle (▶) similar to Oracle Forms
- Selected row for multi-select operations has a different highlight

#### Current Record Sync
```csharp
// When user clicks a row in the grid:
private void GridView_SelectionChanged(object? sender, EventArgs e)
{
    if (_gridView?.CurrentRow?.Index is int rowIndex && rowIndex >= 0)
    {
        // Notify the host that the current record changed
        _host?.NotifyCurrentRecordChanged(BlockName, rowIndex);
    }
}
```

#### Inline LOV
- For LOV fields in grid mode, a dropdown button appears in the cell
- Clicking it opens the LOV picker for that field
- Selection populates the cell value

#### Validation in Grid
- Cell-level validation on edit (red border on invalid cells)
- Row-level validation on row change (before leaving the row)
- Error tooltip on invalid cells

### 10.3 — Multi-Select Batch Operations

```csharp
// Multi-select mode:
// - Ctrl+A: Select all rows
// - Ctrl+Click: Toggle row selection
// - Shift+Click: Select range
// Batch operations on selected rows:
// - Delete selected
// - Export selected
// - Set field value on selected
```

## Verification

1. Click "Grid View" toggle — verify block switches from form to grid
2. Verify grid shows all records from the block's UoW
3. Click a row — verify form view would navigate to that record (if toggled back)
4. Double-click a cell — verify inline editing works
5. Edit a value — verify dirty state updates and validation fires
6. Click "Form View" toggle — verify switches back with correct current record
7. Multi-select rows and delete — verify all selected are deleted
8. Verify LOV columns show dropdown button
9. Verify read-only fields cannot be edited in the grid

## New Files

- None (enhancements to existing `BeepBlock`)

## Modified Files

- `BeepBlock/` — add `ViewMode` property, grid rendering, form/grid sync
- `BeepFormsCommandBar/BeepFormsCommandBar.cs` — add View Toggle button
- `Models/BeepFormsCommandBarButtons.cs` — add ViewToggle flag

## Risks

- **Medium:** Grid integration with the block's form view requires careful synchronization of current record state.
- Grid column generation from entity structure may need the same dynamic control logic as the query criteria panel (Phase 2).
- Inline editing in grid must route through the same validation/trigger pipeline as form editing — ensure consistency.
- Performance: Large datasets (10K+ records) in grid mode may need virtual scrolling (BeepGridPro likely already supports this).
