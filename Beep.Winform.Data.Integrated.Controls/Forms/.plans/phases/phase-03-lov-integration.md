# Phase 3: LOV (List of Values) Integration

**Priority:** High | **Est. Tasks:** 12

## Goal

Wire the existing `LovPickerDialog` into the BeepForms shell workflow. Add a "Show LOV" button and LOV field indicators. Enhance the picker dialog with live search and better UX.

## Dependencies

- Engine: `FormsManager` LOV methods, `LOVManager`, `ILOVManager` (complete)
- UI: `LovPickerDialog` (exists), `BeepForms.WorkflowShell.cs` (exists)

## Engine Methods Used (No New Engine Code)

| UI Action | Engine Method | Status |
|-----------|---------------|--------|
| Show LOV | `IBeepBuiltins.ShowLov(blockName, fieldName)` | ✅ |
| Popup LOV | `IBeepBuiltins.PopupLov(blockName, fieldName)` | ✅ |
| List Values | `IBeepBuiltins.ListValues(blockName, fieldName)` | ✅ |
| Get LOV Definition | `FormsManager.GetLOVDefinitions(blockName)` | ✅ |
| Get Related Fields | LOVDefinition.RelatedFields | ✅ |
| Validate LOV Value | `LOVManager.ValidateLOVValueAsync(...)` | ✅ |
| LOV Data | `LOVManager.GetLOVData(definition)` | ✅ |

## Implementation Seam

### 3.1 — Show LOV Button on CommandBar

Add a "Show LOV" button to `BeepFormsCommandBar`:

```csharp
BeepButton _showLovButton → Click: async {
    string blockName = _formsHost.ActiveBlockName;
    string itemName = _formsHost.ActiveItemName;
    // Route through IBeepBuiltins
    _formsHost.Builtins?.ShowLov(blockName, itemName);
}
```

**Enable rules:**
- Button enabled when:
  - Active block exists
  - Active item has a registered LOV definition
  - Block is NOT in query mode (LOV is for data entry, not query criteria)
- Disabled otherwise (grayed out)
- Tooltip shows LOV title: "Show LOV: Customer Type"

**Add to `BeepFormsCommandBarButtons` enum:**
```csharp
[Flags]
public enum BeepFormsCommandBarButtons
{
    BlockSelector = 1,
    Sync = 2,
    ShowLOV = 4,    // NEW
    // ... existing flags
    All = BlockSelector | Sync | ShowLOV | ...
}
```

### 3.2 — LOV Field Indicator on BeepBlock

Add a visual indicator to `BeepBlock` fields that have LOV definitions:

**Indicator style:** Small chevron button ("⋮" or "…" or dropdown arrow) at the right edge of the text box.

```csharp
// In BeepBlock field rendering:
if (HasLOVDefinition(fieldName))
{
    // Add a small button (16×16) at the right edge
    Button lovIndicator = new BeepButton
    {
        Width = 20,
        Text = "…",
        ToolTip = $"Show LOV: {lovDefinition.Title}"
    };
    lovIndicator.Click += (s, e) => ShowLOVForField(fieldName);
    // Position at right edge of the text box
}
```

**Click handler:**
```csharp
private async void ShowLOVForField(string fieldName)
{
    string blockName = _formsHost?.ActiveBlockName;
    if (blockName == null) return;
    
    // Get current field value for pre-selection
    // Open LovPickerDialog
    // On selection, set field value and populate related fields
    await _formsHost.ShowLOVAsync(blockName, fieldName);
}
```

**Keystroke auto-complete:**
- When user types in an LOV field, filter LOV data and show a dropdown suggestion list
- This can be a simpler inline dropdown (not the full LovPickerDialog)
- Use `LOVManager.FilterLOVData(searchText)` to get filtered results
- Show top N results in a popup below the field
- On selection, populate the field value

### 3.3 — LovPickerDialog Enhancements

Enhance the existing `LovPickerDialog`:

#### Live Search
```csharp
// Add TextBox at top of dialog
BeepTextBox _searchBox → TextChanged: {
    string search = _searchBox.Text;
    if (string.IsNullOrWhiteSpace(search))
    {
        _grid.DataSource = _allRows;
    }
    else
    {
        // Filter DataView
        DataView dv = _allRows.DefaultView;
        dv.RowFilter = BuildSearchFilter(search); // "Column1 LIKE '%search%' OR Column2 LIKE '%search%'"
        _grid.DataSource = dv;
    }
}
```

#### Column Resizing
```csharp
// Enable column resize
_grid.AllowUserToResizeColumns = true;
_grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
```

#### Keyboard Navigation
```csharp
// Handle key events
protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
{
    if (keyData == Keys.Enter)
    {
        SelectCurrentRow();
        DialogResult = DialogResult.OK;
        return true;
    }
    if (keyData == Keys.Escape)
    {
        DialogResult = DialogResult.Cancel;
        return true;
    }
    return base.ProcessCmdKey(ref msg, keyData);
}
```

#### Column Visibility Toggle
- Add a context menu on the grid header to show/hide columns
- Persist column visibility per LOV definition

#### Empty State
- When no rows match the search, show "No matching values found" in the grid area

## Verification

1. Register an LOV for "CustomerType" field on a block
2. Verify "Show LOV" button appears and is enabled in CommandBar when customer type field is active
3. Verify LOV indicator ("…" button) appears next to the CustomerType text box in BeepBlock
4. Click LOV indicator — verify LovPickerDialog opens
5. Type in search box — verify grid filters in real time
6. Select a row, press Enter — verify value is set in the field and related fields update
7. Verify the LOV button is disabled when no field has an LOV
8. Verify the LOV button is disabled in query mode (if intentional)

## New Files

- None (enhancements to existing files)

## Modified Files

- `BeepFormsCommandBar/BeepFormsCommandBar.cs` — add Show LOV button
- `Models/BeepFormsCommandBarButtons.cs` — add ShowLOV flag
- `BeepBlock/` (the BeepBlock control) — add LOV field indicator
- `Lov/LovPickerDialog.cs` — add live search, column resize, keyboard nav, empty state
- `Contracts/IBeepFormsHost.cs` — add `ShowLOVAsync(blockName, fieldName)` if missing
- `BeepForms/BeepForms.WorkflowShell.cs` — add `ShowLOVAsync` method

## Risks

- **Low:** LOV subsystem already exists in both engine and UI. This is wiring and UX polish.
- LOV field indicator placement must work with the dynamic layout of BeepBlock fields — careful positioning needed.
- Auto-complete dropdown must not conflict with the BeepBlock's existing input handling.
