# uc_EntityEditor Enhancement Plan

## Goal

Evolve `uc_EntityEditor` into a framework-grade entity authoring surface that reliably drives `IDataSource.CreateEntityAs(EntityStructure)` across datasource implementations (RDBMS as the reference example), with strong validation, predictable UX, and clear operation telemetry.

## Scope

- In scope:
- `uc_EntityEditor.cs`
- `uc_EntityEditor.Designer.cs`
- `uc_EntityEditor.resx`
- supporting view-model wiring and datasource-safe behaviors exposed through existing services

- Out of scope:
- breaking `IDataSource` interface changes
- datasource-specific SQL authoring inside the view (must delegate to datasource/helper layers)

## Baseline Assessment (Current Gaps)

- Current UI is functional but minimal; create/update intent is ambiguous for users.
- Entity creation relies on `SaveEntity()` path and does not expose a clear, explicit `CreateEntityAs` workflow contract in UX.
- Validation is mostly implicit; no preflight summary before create/update execution.
- Limited operation telemetry (status states, warnings, outcome detail, duration, post-action cache refresh visibility).
- No explicit guardrails for common schema authoring mistakes (missing PK, duplicate names, invalid datatype selections, reserved names, empty field list).
- No staged workflow for "new entity design -> validate -> preview -> create -> refresh".

## Design Principles

- Datasource-agnostic orchestration in UI; datasource-specific work in `IDataSource`/helpers.
- Explicit command intent (`Create`, `Update Structure`, `Validate`, `Preview`, `Reset`).
- Safe-first defaults and preflight checks before executing `CreateEntityAs`.
- Non-throwing UX: all failures routed to status/log with actionable messages.
- Strong visual hierarchy and predictable control states using Beep controls only.

## Target UX Architecture

1. Header + context strip
- Datasource selector
- Entity selector/name input
- mode badge (`New`/`Existing`)
- status badge (`Idle`, `Validating`, `Ready`, `Creating`, `Created`, `Failed`)

2. Command bar
- `Validate`
- `Preview Script` (if datasource can provide script via helper)
- `Create Entity`
- `Update Entity`
- `Reload Entities`
- `Reset Draft`

3. Field designer area (`BeepGridPro`)
- deterministic column policies for `EntityField` editing
- datatype combo sourced from `DatatypeMappings`
- readonly/visibility rules for system fields
- row-level validation indicators

4. Footer telemetry
- last operation
- warning/error count
- duration
- last created/updated entity + datasource

## IDataSource / CreateEntityAs Contract Alignment

The editor must explicitly align to `IDataSource` responsibilities:

- Resolve datasource and ensure connection is open.
- Build normalized `EntityStructure` from UI draft.
- Run preflight validation (`IDataSourceHelper.ValidateEntity(entity)` where available).
- Execute `CreateEntityAs(entity)` for create mode.
- For update mode, execute existing save/update path while keeping operation intent explicit.
- Refresh `Entities` and `EntitiesNames` after schema changes.
- Refresh UI selectors and bindings after success.
- Log all outcomes through `DMEEditor.AddLogMessage(...)` with `Errors` severity.

## Phase-by-Phase Execution Checklist

## Phase 1 - UX Shell and Intent Clarity

- [ ] Add explicit editor mode model (`Create` vs `Update`).
- [ ] Split primary action into dedicated `Create Entity` and `Update Entity`.
- [ ] Add status label/badge and last-operation label.
- [ ] Normalize command naming and button text for consistency.
- [ ] Add empty-state guidance when no datasource/entity is selected.

Acceptance:
- Users can identify current mode and primary action without ambiguity.

## Phase 2 - Field Grid Authoring Standards

- [ ] Define fixed `BeepGridPro` column ordering for key `EntityField` properties.
- [ ] Configure datatype column as combo with favorites-first ordering.
- [ ] Mark technical/system-only columns as readonly/hidden.
- [ ] Add default widths + sort/filter policy for field metadata columns.
- [ ] Enforce stable row add/delete/edit behavior and selection handling.

Acceptance:
- Field designer behaves consistently and remains readable with larger schemas.

## Phase 3 - Preflight Validation Pipeline

- [ ] Implement a dedicated `ValidateDraft()` pipeline before create/update.
- [ ] Validate entity name (required, legal format, not whitespace).
- [ ] Validate field set (non-empty, unique names, legal datatype).
- [ ] Validate key constraints (PK recommendation/rules by datasource category).
- [ ] Invoke datasource/helper validation when available.
- [ ] Surface validation report in status + log (blocking vs warning categories).

Acceptance:
- Invalid drafts are blocked before `CreateEntityAs` execution with actionable errors.

## Phase 4 - CreateEntityAs Execution Hardening

- [ ] Build a normalized `EntityStructure` payload from current draft.
- [ ] Execute `IDataSource.CreateEntityAs(entity)` explicitly in create mode.
- [ ] Add in-flight guard (`_isCreating`) and block overlapping commands.
- [ ] Add confirmation for destructive or overwrite-like operations.
- [ ] Capture duration and structured result summary.
- [ ] On success: refresh entity cache/list and rebind editor.

Acceptance:
- Create workflow is deterministic, guarded, and observable end-to-end.

## Phase 5 - Update/Existing Entity Flow

- [ ] Separate update semantics from create semantics in UI and logs.
- [ ] Add pre-update compatibility checks (field rename/type-change warnings).
- [ ] Add optional "safe update" mode (warn-only vs block).
- [ ] Keep entity selector synchronized after update operations.

Acceptance:
- Existing entity updates are explicit and safer, with no accidental create behavior.

## Phase 6 - Script Preview and Explainability

- [ ] Add `Preview Script` command (when helper/script generation exists).
- [ ] Show script preview in a modal/editor panel with copy support.
- [ ] Tag preview with datasource type and target entity metadata.
- [ ] Ensure preview does not mutate schema/state.

Acceptance:
- Users can inspect generated DDL intent before execution.

## Phase 7 - Telemetry, Logging, and Recovery

- [ ] Standardize operation states (`Idle`, `Validating`, `Creating`, `Updating`, `Failed`, `Completed`).
- [ ] Persist last operation summary in control state.
- [ ] Add retry-safe behavior for transient datasource failures.
- [ ] Improve errors with cause + next-step guidance.
- [ ] Ensure all failures remain non-throwing at UI surface.

Acceptance:
- Operational transparency and supportability are significantly improved.

## Phase 8 - Accessibility and UI Polish

- [ ] Ensure all command controls have clear text/tooltips.
- [ ] Improve tab order and keyboard navigation through form and grid.
- [ ] Add visual emphasis for primary action and validation states.
- [ ] Ensure responsive layout behavior for lower widths.

Acceptance:
- Editor remains usable and clear across common resolutions and input methods.

## Validation Matrix

- Create new entity against RDBMS datasource (`CreateEntityAs` success path).
- Create with invalid draft (name/fields/datatypes) -> blocked with clear errors.
- Update existing entity -> explicit update path and refreshed state.
- Cancel/reset draft -> no schema mutation.
- Datasource disconnected -> open/connect guard and safe failure.
- Cache refresh check -> new entity appears in `GetEntitesList()` after create.

## Risks and Mitigations

- Risk: datasource behavior differences for create/update.
- Mitigation: keep UI orchestration generic and rely on datasource/helper capabilities, with capability checks.

- Risk: breaking existing save behavior.
- Mitigation: run create/update split behind clear mode checks and keep backward-compatible save path until stabilized.

- Risk: UI complexity growth.
- Mitigation: phase delivery, enforce command-state matrix, and document behavior contract.

## Deliverables

- Updated `uc_EntityEditor.cs` with explicit create/update/validate pipelines.
- Updated `uc_EntityEditor.Designer.cs` with improved command/status surfaces.
- Updated `uc_EntityEditor.resx` for new control resources/text.
- `uc_EntityEditor.USAGE.md` (recommended follow-up) documenting parameter contract, command matrix, and validation rules.

## Recommended Implementation Order

1. Phase 1 + 2 (UX clarity and grid standards)
2. Phase 3 + 4 (validation + hardened create execution)
3. Phase 5 + 6 (update safety + script preview)
4. Phase 7 + 8 (telemetry and polish)
