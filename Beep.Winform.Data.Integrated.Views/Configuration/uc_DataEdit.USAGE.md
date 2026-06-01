# uc_DataEdit Usage Contract

## Navigation And Runtime Parameters

- `CurrentEntity` (required): target entity for CRUD, mapping, and import destination.
- `CurrentDataSource` (required): target datasource for CRUD, mapping, and import destination.
- `ImportSourceEntity` (optional): import source entity; if missing, destination entity is reused.
- `ImportSourceDataSource` (optional): import source datasource; if missing, destination datasource is reused.
- `UowAutoValidate` (optional bool): runtime override for `IsAutoValidateEnabled`.
- `UowBlockCommitOnValidationError` (optional bool): runtime override for `BlockCommitOnValidationError`.
- `UowUndoEnabled` (optional bool): runtime override for `IsUndoEnabled`.
- `UowMaxUndoDepth` (optional int): runtime override for undo stack depth.
- `UowCommitOrder` (optional object): best-effort runtime assignment through reflection when `CommitOrder` is exposed.
- `uow` from `TemplateUserControl` is required for active CRUD; without it, control stays in safe read-only status mode.

## Command Matrix

- `New`: calls `BeepGridPro.InsertNew()` and enters edit mode.
- `Edit`: requires row selection, then enables edit mode.
- `Delete`: requires row selection and confirmation, then calls `BeepGridPro.DeleteCurrent()`.
- `Save`: validates (when enabled), commits via `UnitOfWorkWrapper.Commit()`, then reloads.
- `Cancel` (normal mode): confirms then executes rollback flow (`Rollback() -> grid cancel -> reload`).
- `Refresh`: guarded `UnitOfWorkWrapper.Get()` and grid rebind.
- `Undo` / `Redo`: enabled only when UOW undo support is enabled and available.
- `Mapping`: runs `MappingManager.CreateEntityMap(...)` for current entity/datasource context.
- `Import`: runs full import execution (`TestImportConfigurationAsync` then `RunImportAsync`) and refreshes data on completion.

## Import Lifecycle Rules

- Import runs with live progress updates on `lblState`.
- Import uses transactional preflight: if pending edits exist, save is requested first and execution continues only when state is clean.
- Import safety gate blocks source=destination runs; caller should pass `ImportSourceEntity` + `ImportSourceDataSource`.
- While import is running, command surface is busy and overlapping save/rollback/external operations are blocked.
- `Cancel` is dual-mode:
- During import: requests import cancellation.
- Outside import: performs rollback for pending edits.
- During cancellation wind-down, status includes `CancelRequested`.
- After cancellation is requested, `Cancel` is disabled to prevent duplicate requests.

## Import End-State Semantics

- Cancel paths are normalized to `Import cancelled` (including `OperationCanceledException` paths).
- Warning completions are surfaced as `Import completed with warnings`.
- Hard failures remain `Import failed`.
- Successful and warning completions clear stale `LastError`.
- Import run summary includes:
- `Processed/Total`
- `Errors`
- `Quarantined`
- `Blocked`
- `Duration` (`mm:ss` or `hh:mm:ss` for longer runs)
- The latest import summary is persisted and shown in later status lines until a new run replaces it.

## Grid Defaults (BeepGridPro)

- Grid host is `BeepGridPro` only (no `BeepSimpleGrid`).
- Baseline technical columns:
- `Sel` width `36`, no sort/filter.
- `RowNum` width `56`, read-only, no sort/filter.
- `RowID` width `80`, read-only, hidden by default.
- Entity defaults:
- boolean `90`
- numeric `120`
- date/time `140`
- enum/status `130`
- foreign key lookup `160`
- text fallback `180`
- Identity/technical fields enforce read-only behavior.
- Editor defaults:
- bool -> `CheckBoxBool`
- numeric -> `NumericUpDown`
- date/time -> `DateTime`
- foreign key -> `ListOfValue`
- enum/status -> `ComboBox`

## Extension Points

- `ApplyEntityColumnPolicies()` for entity-specific column/editor policy.
- `PrepareMappingAsync()` for custom mapping UI routing.
- `PrepareImportAsync()` for custom batch size/defaults/progress/cancellation behavior.
- `UpdateStatus(...)` for richer UX telemetry.
- `RefreshCommandStates()` for custom enable/disable policies.

## UnitOfWork Lifecycle

- UI handlers route through internal UOW adapter (`Uow*` properties/methods).
- `OnNavigatedTo(...)` resolves context, applies runtime UOW options, then loads.
- `LoadDataFromUnitOfWorkAsync()` is the single safe refresh entry point.
- `SaveChangesAsync()` flow: `validate -> commit -> reload`.
- `CancelChangesAsync()` flow: `confirm -> rollback -> grid cancel -> reload`.
- `Edit` and `Delete` require a current row selection.
- Failures are non-throwing at UI level and reported via status + `Editor.AddLogMessage`.
