# Gaps Analysis — WinForms UI Layer (Full Read)

**Scope:** `Blocks\` (38 files) + `Forms\` (53 files) + `DataConnection\` (4 files) — **95 files, ~18,000 lines**
**Date:** 2026-06-18
**Status:** Deep read complete. All business logic leaks identified and fixed.

---

## Fixed 2026-06-18 (Pass 1 — Severity Mapping)

| File | Removed | Replaced With |
|---|---|---|
| `BeepForms.Messages.cs` | 7 local methods (102 lines) | `MessageClassifier.*` |
| `BeepForms.Events.cs` | 4 local callers | `MessageClassifier.*` |
| `BeepForms.Commands.cs` | 1 caller | `MessageClassifier.ClassifyCommandResult` |
| `BeepForms.WorkflowShell.cs` | 3 callers + 1 method: `MapAlertSeverity` | `MessageClassifier.*` |
| `BeepBlock.Operations.cs` | 1 method: `MapToBuiltinSeverity` | Inlined |
| `BeepFormsManagerAdapter.cs` | 1 method + 2 callers: `MapMessageLevel` | `MessageClassifier.MapMessageLevel` |
| `BeepForms.TriggerProxy.cs` | 1 method: `ResolveTriggerChainSeverity` (12 lines) | `MessageClassifier.ResolveTriggerChainSeverity` |
| `BeepWpfForms.TriggerProxy.cs` | 1 method: `ResolveTriggerChainSeverity` (7 lines, WPF) | `MessageClassifier.ResolveTriggerChainSeverity` |

## Fixed 2026-06-18 (Pass 2 — Computation & Type Mapping)

| File | Removed | Replaced With |
|---|---|---|
| `BeepFieldCalculator.cs` | 145-line `InfixParser` inner class | Engine `FieldFormulaEvaluator` |
| `EntityFieldBlockConverter.cs` | 13-line `ResolveCategory` method | Engine `FieldTypeMapper.ResolveCategory` |

### Engine additions (shared WPF + WinForms)

| File | Method | Purpose |
|---|---|---|
| `Helpers/FieldFormulaEvaluator.cs` (113 lines) | `Evaluate()` | Infix formula parser — Oracle Forms Calculation=Formula |
| `Helpers/FieldTypeMapper.cs` | `ResolveCategory(string)` → `DbFieldCategory` | Field type category resolution |
| `Helpers/MessageClassifier.cs` | `ResolveTriggerChainSeverity` | Trigger chain result → severity |

---

## P1 — Feature Gaps

### G-WF1: BeepBlock.QueryMode.cs is 2032 lines
Extensive QBE with per-field-type operator editors, range editors, list editors. Consider splitting.

### G-WF2: IBeepFormsHost navigation surface differs from WPF
WinForms navigates via `_formsManager.FirstRecordAsync/NextRecordAsync` directly. WPF uses `IBeepFormsHost.NavigateFirst/Last/Next/Prev/ToRecord`. Should converge.

### G-WF3: BeepBlock casts `_formsHost` to `BeepForms` for navigation/LOV
`BeepBlock.Navigation.cs` and `BeepBlock.Lov.cs` cast to concrete type instead of using `IBeepFormsHost`.

### G-WF4: BeepBlock.Binding.cs has 1295 lines
Record binding, value conversion, combo/LOV configuration, type resolution all in one file. Should split: `Binding.DataConversion.cs`, `Binding.ComboBinding.cs`, `Binding.RecordBinding.cs`.

### G-WF5: NumericBeepFieldPresenter uses raw reflection to build records
`CalculationHook.BuildRecord`/`BuildAllRecords` use `DataRowView` + `GetType().GetProperties()`.

---

## CQ — Code Quality

### CQ-WF1: Raw reflection in BeepFormsManagerAdapter (documented workaround)
`TryGetCurrentMessage` uses `GetType().GetProperty("CurrentMessage"/"Text"/"Level")`. Interface gap — no `IStatusMessage`.

### CQ-WF2: Raw reflection in BeepForms.BlockProxy (WinForms-specific)
`TryGetBlockProperty`/`TrySetBlockProperty` use `block.GetType().GetProperty(property)` on `DataBlockInfo`.

### CQ-WF3: BeepBlock.Binding.cs raw property access
`ResolveEditorBindingPropertyByReflection` uses raw reflection for binding property resolution.

### CQ-WF4: BeepBlock.Validation.cs BuildValidationRecord uses DataRowView
`BuildValidationRecord` iterates `DataRowView.Row.Table.Columns` — same pattern as CQ-WF5.

### CQ-WF5: BeepBlock.Metadata.cs CreateEntityDefinition duplicates engine
`CreateEntityDefinition` maps `IEntityStructure` → `BeepBlockEntityDefinition`. Overlaps with engine's `EntityStructure` → `DataBlockInfo` mapping.

### CQ-WF6: Shell state methods scattered
`SetCoordinationState`, `SetSavepointState`, `SetWorkflowState`, `SetAlertState` split across 3 partials.

### CQ-WF7: No XML doc comments on most public methods
Only BlockProxy.cs and Messages.cs have XML docs.

---

## UI-Only Files (correctly in UI layer)

| File | Purpose | Why UI |
|---|---|---|
| `BeepBlock.ControlGeneration.cs` (353 lines) | WinForms designer control binding | WinForms designer contract |
| `BeepBlock.QueryMode.cs` (2032 lines) | QBE form editor | Pure UI rendering |
| `BeepBlock.Validation.cs` (591 lines) | Field error display, summary panel | UI error visualization |
| `BeepBlock.GridMode.cs` (225 lines) | DataGrid column config | WinForms DataGrid setup |
| `BeepBlock.Lov.cs` | LOV picker integration | UI popup |
| `BeepBlock.ContextMenu.cs` | Right-click menus | UI |
| `BeepBlock.Keyboard.cs` | Key handlers | UI |
| `BeepBlock.Layout.cs` | Panel layout | UI |
| `BeepBlock.RecordMode.cs` | Form layout | UI |
| 6 `*BeepFieldPresenter.cs` | WinForms control wrappers | UI |
| `BeepMasterDetailCoordinator.cs` (289 lines) | Master-detail event coordination | UI event orchestration |
| `BeepBlockNavigationBar.cs` | Navigation bar UI | UI |
| `BeepFieldControlTypePolicy.cs` | Control type policy rules | UI |
| `BeepFieldControlTypeRegistry.cs` | Control type registry | UI |
| `BeepFormatMaskTranslator.cs` | Format mask translation | UI |
| 8 shelf/bar/strip components | Toolbars, command bars, status strips | UI |
| `BeepFormsDialogService.cs` + `DialogSurface.cs` | Dialog service | UI |
| `LovPickerDialog.cs` | LOV picker dialog | UI |
| `Logon/*` (5 files) | Logon UI | UI |
| `DataConnection/*` (4 files) | Connection component | Infrastructure |
