# uc_EntityEditor Usage Contract

## Purpose

`uc_EntityEditor` is a datasource-agnostic entity schema editor for framework users.  
It separates:

- create-table flow through `IDataSource.CreateEntityAs(...)`
- update-schema flow through `IDataSourceHelper` capability-driven DDL generation

## Core Rules

- `CreateEntityAs` is treated as **create-only**.  
  If the entity already exists, create is blocked.
- Update operations are only enabled when datasource helper capabilities support schema evolution.
- UI never assumes RDBMS-only behavior; command enablement is capability-driven.

## Runtime Modes

- `CreateNew`
- `UpdateExisting`

Mode is derived from:

- selected datasource
- selected/typed entity name
- `SourceConnection.CheckEntityExist(entityName)`

## Command Behavior

- `Create Entity`:
- validates draft structure
- invokes `SourceConnection.CreateEntityAs(draft)`
- refreshes entity list and editor bindings on success

- `Update Schema`:
- validates draft structure
- loads current structure with `GetEntityStructure(entityName, true)`
- computes diff: add/alter/drop column
- generates statements via helper:
  - `GenerateAddColumnSql`
  - `GenerateAlterColumnSql`
  - `GenerateDropColumnSql`
- shows preview confirmation
- executes generated SQL sequentially with stop-on-failure semantics
- refreshes entity list and editor bindings on success

## Validation Pipeline

Before create/update execution:

- entity name is required
- at least one field is required
- duplicate field names are blocked
- helper validation is invoked when available:
  - `IDataSourceHelper.ValidateEntity(entity)`

## Capability Rules

- `GetDataSourceHelper(datasourceType)` resolves helper features.
- If helper is missing:
- create may still run through `CreateEntityAs`
- update is blocked
- If helper exists but `Capabilities.SupportsSchemaEvolution == false`:
- update is disabled
- create remains available

## Telemetry And Logging

- all outcomes are routed through `DMEEditor.AddLogMessage(...)`
- operation summary is tracked in control state (`_lastSummary`)
- in-flight guard (`_isApplyingSchema`) blocks overlapping apply actions

## Notes

- Rename detection is not auto-inferred in diff logic; rename operations are not executed implicitly.
- Update path currently applies add/alter/drop only for deterministic safety.
