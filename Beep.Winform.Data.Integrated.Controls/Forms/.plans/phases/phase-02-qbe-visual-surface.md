# Phase 2: Query-by-Example (QBE) Visual Surface

**Priority:** High | **Est. Tasks:** 13

## Goal

Add a dynamic criteria-entry panel that appears in Enter Query mode, letting users fill in search criteria and execute the query with Oracle-style operators. This is the visual counterpart to the engine's `ENTER_QUERY` / `EXECUTE_QUERY` workflow.

## Dependencies

- Engine: `FormsManager.ModeTransitions.cs`, `FormsManager.BasicDataOps.cs` (complete)
- UI: `BeepForms.Commands.cs`, `BeepFormsQueryShelf` (exists)

## Engine Methods Used (No New Engine Code)

| UI Action | Engine Method | Status |
|-----------|---------------|--------|
| Enter Query Mode | `FormsManager.EnterQueryModeAsync(blockName)` | ✅ |
| Execute Query | `FormsManager.ExecuteQueryAsync(blockName, filters)` | ✅ |
| Execute + CRUD | `FormsManager.ExecuteQueryAndEnterCrudModeAsync(blockName, filters)` | ✅ |
| Exit Query | `IBeepBuiltins.ExitQuery()` | ✅ |
| Get Entity Structure | `FormsManager.GetBlock(blockName)?.EntityStructure` | ✅ |
| Block Query Allowed | `FormsManager.GetBlock(blockName)?.QueryAllowed` | ✅ |
| Get LOV Fields | `FormsManager.GetLOVDefinitions(blockName)` | ✅ |

## Implementation Seam

### 2.1 — BeepFormsQueryCriteriaPanel (New Control)

Create a new panel control that appears when the active block enters query mode. It auto-generates one criteria control per queryable field in the block's entity structure.

```
BeepFormsQueryCriteriaPanel (BaseControl)
├── TableLayoutPanel _criteriaGrid
│   ├── Row 0: BeepLabel "Field" | BeepLabel "Operator" | BeepLabel "Value"
│   ├── Row 1: BeepLabel "CustomerId" | BeepComboBox _opCustId | BeepTextBox _valCustId
│   ├── Row 2: BeepLabel "CustomerName" | BeepComboBox _opCustName | BeepTextBox _valCustName
│   ├── Row 3: BeepLabel "CreatedDate" | BeepComboBox _opCreatedDate | BeepDatePicker _valCreatedDate
│   └── ... (one row per queryable field)
├── FlowLayoutPanel _actionPanel
│   ├── BeepButton _clearCriteriaButton
│   ├── BeepButton _executeQueryButton
│   └── BeepButton _exitQueryButton
```

**Field type → Control mapping:**
| Entity Field Type | Criteria Control | Operators |
|-------------------|------------------|-----------|
| `string` | `BeepTextBox` | =, LIKE, NOT LIKE, IS NULL, IS NOT NULL |
| `int/long/decimal` | `BeepNumericUpDown` or `BeepTextBox` | =, <>, >, <, >=, <=, BETWEEN, IS NULL |
| `DateTime/DateOnly` | `BeepDatePicker` × 2 (for BETWEEN) | =, >, <, >=, <=, BETWEEN, IS NULL |
| `bool` | `BeepComboBox` (True/False/Any) | = |
| LOV field | `BeepComboBox` with LOV values | =, IS NULL |
| FK field | `BeepComboBox` with lookup values | =, IS NULL |

**Visibility rules:**
- Show panel when `ViewState.IsQueryMode == true`
- Hide panel when `ViewState.IsQueryMode == false`
- Only generate rows for fields where `IsQueryAllowed == true`
- Skip computed/read-only fields

**Building filters:**
```csharp
public List<AppFilter> BuildFilters()
{
    var filters = new List<AppFilter>();
    foreach (var row in _criteriaRows)
    {
        if (row.HasValue)
        {
            filters.Add(new AppFilter
            {
                FieldName = row.FieldName,
                Operator = row.Operator,  // "=", "LIKE", ">", etc.
                FilterValue = row.Value
            });
        }
    }
    return filters;
}
```

**Clear criteria:**
- Resets all criteria controls to empty/default
- Does NOT exit query mode

**Execute Query:**
- Builds filters from all non-empty criteria rows
- Calls `_formsHost.ExecuteQueryAsync(blockName, filters)`
- If successful, transitions to CRUD mode (engine handles this)
- Shows query result count in StatusStrip

**Exit Query:**
- Calls `_formsHost.ExitQueryAsync()` (add to router → calls `IBeepBuiltins.ExitQuery()`)
- Clears criteria
- Hides the criteria panel

### 2.2 — Query Mode Visual States

**QueryShelf background:**
- Normal mode: default theme background
- Query mode: light blue tint (`Color.FromArgb(230, 240, 255)` or theme equivalent)

**Block visual state:**
- In query mode, highlight block border with query-mode color
- Show "Enter Query" watermark text in the active block

**BeepFormsQueryShelf additions:**
- Add `ExitQuery` flag to `BeepFormsQueryShelfButtons` enum
- Add `_exitQueryButton` that calls `ExitQueryAsync`
- Add count label after Execute Query: "N records found"

### 2.3 — QueryShelf Enhancements

**Enable Execute Query without Enter Query:**
Currently ExecuteQueryButton requires `isQueryMode == true`. Also enable when:
- Block exists (has blocks)
- Block allows queries (`QueryAllowed`)
This lets users execute a direct query with default filters without entering query mode first.

**Exit Query button:**
- Only visible when `isQueryMode == true`
- Calls `IBeepBuiltins.ExitQuery()` via host
- Clears all query criteria
- Returns block to Normal/CRUD mode

## Verification

1. Click "Enter Query" — verify criteria panel appears with one row per queryable field
2. Enter value "SMITH%" in CustomerName with operator "LIKE"
3. Click "Execute Query" — verify results load and panel hides
4. Verify "12 records found" appears in StatusStrip
5. Click "Enter Query" again — verify criteria are cleared (fresh start)
6. Enter criteria, click "Exit Query" — verify panel hides without executing
7. Verify operator dropdown shows correct operators per field type
8. Verify date fields show two date pickers when operator is BETWEEN

## New Files

- `BeepFormsQueryCriteriaPanel/BeepFormsQueryCriteriaPanel.cs` (new control)
- `BeepFormsQueryCriteriaPanel/README.md` (optional)

## Modified Files

- `BeepFormsQueryShelf/BeepFormsQueryShelf.cs` — add ExitQuery button, count label, enable rules
- `BeepForms/BeepForms.Commands.cs` — add `ExitQueryAsync` method
- `BeepForms/BeepForms.Messages.cs` — add query count feedback
- `Models/BeepFormsQueryShelfButtons.cs` — add ExitQuery flag
- `Services/BeepFormsCommandRouter.cs` — add ExitQuery routing
- `BeepForms/BeepForms.Layout.cs` — optionally host criteria panel

## Risks

- **Medium:** Dynamic control generation from entity structure — must handle all common field types gracefully. Unknown field types should show a read-only label "Unsupported type".
- Entity structure availability — `GetBlock(blockName)?.EntityStructure` may be null during design time. Handle with a design-time placeholder.
- BETWEEN operator for dates needs two date pickers; careful layout management needed.
