# Phase 6: Data Operations Surface (Undo/Redo, Export/Import, Batch)

**Priority:** Medium | **Est. Tasks:** 13

## Goal

Expose the engine's data operations (Undo/Redo, export/import, batch commit, aggregates) through visual buttons and displays.

## Dependencies

- Engine: `FormsManager.DataOperations.cs`, `FormsManager.FormOperations.cs` (complete)
- UI: `BeepFormsCommandBar`, `BeepFormsStatusStrip` (exist)

## Engine Methods Used (No New Engine Code)

| UI Action | Engine Method | Status |
|-----------|---------------|--------|
| Undo | `FormsManager.UndoBlock(blockName)` | ✅ |
| Redo | `FormsManager.RedoBlock(blockName)` | ✅ |
| Export JSON | `FormsManager.ExportBlockToJsonAsync(blockName)` | ✅ |
| Export CSV | `FormsManager.ExportBlockToCsvAsync(blockName)` | ✅ |
| Export DataTable | `FormsManager.GetBlockAsDataTable(blockName)` | ✅ |
| Import JSON | `FormsManager.ImportBlockFromJsonAsync(blockName, json)` | ✅ |
| Import CSV | `FormsManager.ImportBlockFromCsvAsync(blockName, csv)` | ✅ |
| Batch Commit | `FormsManager.CommitFormBatchAsync()` or `CommitBlockBatchAsync(blockName)` | ✅ |
| Block Sum | `FormsManager.GetBlockSum(blockName, fieldName)` | ✅ |
| Block Average | `FormsManager.GetBlockAverage(blockName, fieldName)` | ✅ |
| Block Count | `FormsManager.GetBlockCount(blockName)` | ✅ |
| Undo Available | `IUnitofWorkHistory` or manager equivalent | ✅ |
| Redo Available | `IUnitofWorkHistory` or manager equivalent | ✅ |

## Implementation Seam

### 6.1 — Undo/Redo Buttons

Add to `BeepFormsCommandBar`:

```
├── ↩ Undo    → UndoBlock(blockName)  [enabled when undo stack has entries]
├── ↪ Redo    → RedoBlock(blockName)  [enabled when redo stack has entries]
```

**Enable/disable rules:**
- Undo: enabled when `_formsHost?.CanUndo(blockName) == true`
- Redo: enabled when `_formsHost?.CanRedo(blockName) == true`
- Update enable state on `ViewStateChanged`

**Tooltip:**
- Undo shows last undo action description: "Undo: Changed CustomerName"
- Redo shows last redo action description: "Redo: Changed CustomerName"

### 6.2 — Export/Import Buttons

Add to `BeepFormsCommandBar` or `BeepFormsToolbar`:

```
├── [Export ▼]
│   ├── Export as JSON...
│   ├── Export as CSV...
│   └── Export as DataTable (copy to clipboard)
└── [Import]
    └── Import from file... (JSON or CSV)
```

**Export flow:**
1. User clicks Export → selects format
2. If JSON, open `SaveFileDialog` with `.json` filter
3. Call `ExportBlockToJsonAsync(blockName)` → write to file
4. Show success message with row count

```csharp
private async void ExportJsonButton_Click(object? sender, EventArgs e)
{
    if (_formsHost == null) return;
    string blockName = _formsHost.ActiveBlockName;
    if (string.IsNullOrEmpty(blockName)) return;
    
    using var dialog = new SaveFileDialog
    {
        FileName = $"{blockName}.json",
        Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
        Title = $"Export {blockName} as JSON"
    };
    
    if (dialog.ShowDialog() == DialogResult.OK)
    {
        try
        {
            string json = await _formsHost.ExportBlockToJsonAsync(blockName);
            File.WriteAllText(dialog.FileName, json);
            _formsHost.ShowSuccess($"Exported {CountRows(json)} records to {dialog.FileName}");
        }
        catch (Exception ex)
        {
            _formsHost.ShowError($"Export failed: {ex.Message}");
        }
    }
}
```

**Import flow:**
1. User clicks Import → opens `OpenFileDialog` with `.json`/`.csv` filter
2. Reads file content → calls `ImportBlockFromJsonAsync` or `ImportBlockFromCsvAsync`
3. Shows "N records imported" or error message
4. Refreshes block view

**Warning:** Import can overwrite existing data. Show confirmation if block is not empty.

### 6.3 — Batch Commit

Add to `BeepFormsPersistenceShelf` (behind a flag, not always visible):

```
├── [Batch Commit] → CommitFormBatchAsync()
```

**Batch commit dialog:**
```
┌────────────────────────────────────────────┐
│ Batch Commit Progress                      │
├────────────────────────────────────────────┤
│ Block: CUSTOMERS    ████████████ 100%  OK  │
│ Block: ORDERS       ██████░░░░░░  50%      │
│ Block: ORDER_ITEMS  ░░░░░░░░░░░░   0%      │
│                                            │
│ Committing 3 blocks...                     │
│                                            │
│                     [Cancel]               │
└────────────────────────────────────────────┘
```

### 6.4 — Block Aggregates Display

Add to `BeepFormsStatusStrip` (optional, behind a preset):

```
StatusStrip with aggregates:
┌────────────────────────────────────────────────────────────────────────┐
│ Mode: Normal │ Block: ORDERS │ Rec 1/47 │ Sum: $45,230 │ Avg: $962 │ Dirty │
└────────────────────────────────────────────────────────────────────────┘
```

**Implementation:**
- Aggregates are only shown for numeric fields
- Which field to aggregate can be configured or auto-detected (first numeric field with "Amount"/"Total"/"Price"/"Sum"/"Value" in name)
- Update aggregates on `ViewStateChanged` (after query, navigation, edit)

## Verification

1. Click Undo after editing a field — verify field reverts
2. Click Redo — verify field re-applies the change
3. Click Export as JSON — verify file saves and contains correct data
4. Click Export as CSV — verify file saves and opens correctly in Excel
5. Import a JSON file — verify records appear in the block
6. Import with dirty block — verify confirmation dialog appears
7. Batch commit with 3 blocks — verify progress dialog shows each block
8. Verify aggregate display updates when navigating records

## New Files

- None (enhancements to existing files)

## Modified Files

- `BeepFormsCommandBar/BeepFormsCommandBar.cs` — add Undo/Redo + Export/Import buttons
- `BeepFormsPersistenceShelf/BeepFormsPersistenceShelf.cs` — add Batch Commit button
- `BeepFormsStatusStrip/BeepFormsStatusStrip.cs` — add aggregate display row
- `BeepForms/BeepForms.Commands.cs` — add Undo/Redo/Export/Import/Batch routing
- `Models/BeepFormsCommandBarButtons.cs` — add Undo/Redo/Export/Import flags
- `Contracts/IBeepFormsCommandRouter.cs` — add method signatures

## Risks

- **Low:** All engine methods exist. UI-only routing.
- Export with very large datasets may block UI — consider async with progress for large exports.
- Import overwriting data is destructive — confirmation dialog is critical.
- Aggregate calculation on every navigation may be slow for large datasets — consider caching or debouncing.
