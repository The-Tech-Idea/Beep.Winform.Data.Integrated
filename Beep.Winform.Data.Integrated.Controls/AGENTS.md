# Beep.Winform.Data.Integrated.Controls — Agent Instructions

## 1. Three Object Types Only
The UI emulates Oracle Forms with exactly 3 engine interfaces. NO other interfaces should exist.

| Interface | WinForms Implements |
|---|---|
| `IBeepFormsHost` | `BeepForms` |
| `IBlockView` | `BeepBlock` |
| `IFieldPresenter` | 6 presenters (Text, Combo, Checkbox, Date, Numeric, ReflectiveControl) |

## 2. Architecture
```
Engine (BeepDM) — ALL interfaces in `Editor/Forms/Hosts/`
    │
WinForms UI (this project) — PURE rendering, no business logic
    BeepForms → implements IBeepFormsHost directly
    BeepBlock → implements IBlockView directly
    *BeepFieldPresenter → implements IFieldPresenter directly
```

## 3. Critical Rules

### No interfaces in WinForms
ALL contracts in `BeepDM/Editor/Forms/Hosts/`. WinForms implements them directly.
Deleted: `IBeepBlockView`, `IBeepFieldPresenter`, `IBeepFormsHost`, `IBeepFormsNotificationService`, `IBeepFormsBootstrapper`.

### Block layer — no direct engine access
- NO `_boundUnitOfWork` field — DELETED
- NO `_formsManager` access from BeepBlock partials
- ALL operations through `_formsHost.*` (IBeepFormsHost)
- `BindToHostData()` — hosted data binding using `_formsHost.GetBlockData()`
- `_recordBindingSource` (BindingSource) — WinForms data-binding infrastructure only

### No business logic
- NO DataTable, DataView, DataRowView
- NO raw reflection — use `RecordPropertyAccessor`
- NO severity duplication — use `MessageClassifier`
- NO field type mapping — use `FieldTypeMapper`
- NO formula evaluation — use `FieldFormulaEvaluator`

### No builtins
- `IBeepBuiltins`, `IBuiltinHost`, `BeepBuiltins`, `BeepBuiltinsHostAdapter` — DELETED
- Engine operations through `_formsManager` directly
- No adapters, no bridges

### Single implementation
- `BeepForms : IBeepFormsHost` — direct implementation
- `BeepBlock : IBlockView` — direct implementation
- BlockProxy delegates to `_formsManager.*` — thin delegation
- Commands call `_formsManager.*` directly

## 4. Engine Helpers (shared with WPF)
| Helper | Usage |
|---|---|
| `RecordPropertyAccessor` | All record field access |
| `MessageClassifier` | All severity classification |
| `FieldTypeMapper` | Field type → presenter type |
| `FieldFormulaEvaluator` | Formula evaluation |
