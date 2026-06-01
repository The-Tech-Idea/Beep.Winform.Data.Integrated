# Data Connection Object Enhancement Plan

## Objective
Enable `BeepDataConnection` components dropped on Forms/UserControls to manage connections in one shared repository so all app modules and projects can discover and reuse saved connections consistently.

## Current State (Observed)
- `BeepDataConnection` initializes its own `BeepService` per component instance.
- Runtime config uses `AppContext.BaseDirectory` + hardcoded container `"RuntimeContainer"`.
- `BeepConnectionRepository` persists through `Config_editor` (`DataConnections.json`) and supports add/update/remove/save.
- Local list synchronization exists (`DataConnections`, `CurrentConnection`, `ConnectionsChanged`), but sharing scope is not explicitly controlled (project/user/machine).
- No explicit bootstrap contract that guarantees all dropped components point to the same logical shared connection store.

## Main Gaps to Fix
- Inconsistent service lifetime: each component can create isolated service context.
- Missing explicit shared-storage strategy and naming contract (`AppRepoName`, path, environment).
- No first-class “connection registry service” for app-wide discovery and synchronization.
- Designer-time and runtime behaviors are coupled but not formally versioned or validated.
- Limited lifecycle events for “saved to shared store”, “reload from store”, “store unavailable”.

## Target Architecture

### 1) Shared Connection Registry (Single Source of Truth)
- Introduce `ISharedConnectionRegistry` (or `IConnectionCatalogService`) with:
  - `GetAll()`, `GetByName()`, `AddOrUpdate()`, `Remove()`, `Save()`, `Reload()`.
  - `ConnectionChanged` / `CatalogChanged` events.
- Backed by `BeepConnectionRepository` + `Config_editor` as initial persistence provider.
- Expose deterministic storage location and scope:
  - **Project scope** (default): shared for all controls inside same app/project.
  - Optional **User** and **Machine** scopes for future extension.

### 2) Service Lifetime and Registration
- Stop creating a fully isolated service per component when app-level service exists.
- Preferred resolution order for `IBeepService`:
  1. Explicitly assigned service instance.
  2. DI/service provider registered app singleton.
  3. Fallback local initialization (design/runtime).
- Align with modern Beep registration (`AddBeepForDesktop`, `AppRepoName`, `DirectoryPath`) so every control uses the same repo contract.

### 3) Connection Identity and Integrity
- Enforce stable identity by `GuidID` and unique `ConnectionName`.
- Add validation before save:
  - Required fields by datasource type.
  - Flag consistency (`IsLocal`, `IsRemote`, `IsFile`, `IsDatabase`, `IsInMemory`).
- Add compatibility migration for legacy entries missing `GuidID`.

### 4) Component Experience (Designer + Runtime)
- Keep `DataConnections` for designer visibility, but treat it as a live projection of shared catalog.
- Add explicit component properties:
  - `ConnectionScope` (Project/User/Machine)
  - `AppRepoName`
  - `StoragePath` (optional override)
  - `AutoReloadOnChange` (default true)
- On component drop/load:
  - Resolve shared registry.
  - Reload and bind local list.
  - Keep `CurrentConnection` synchronized with registry updates.

### 5) Observability and Reliability
- Add structured logs for load/save/remove/reload/failure operations.
- Add recoverable error flow (don’t throw for routine config failures; publish status + error object).
- Add lightweight file-change watch (optional) to refresh when external edits happen.

## Implementation Roadmap

## Phase 1 - Foundation (Registry + Contracts)
- Create shared registry interface and concrete implementation.
- Refactor `BeepConnectionRepository` to be persistence engine behind registry.
- Add configuration model for scope/path/repo naming.
- Acceptance criteria:
  - One registry instance can be reused by multiple `BeepDataConnection` components.
  - Add/update/remove from one component is visible in another after reload/event.

## Phase 2 - Component Refactor
- Update `BeepDataConnection` to consume shared registry instead of owning persistence logic.
- Add new component properties (`ConnectionScope`, `AppRepoName`, `StoragePath`, `AutoReloadOnChange`).
- Preserve backward compatibility with existing `DataConnections` and `CurrentConnection`.
- Acceptance criteria:
  - Existing forms compile without changes.
  - Dropping multiple controls on same form uses shared catalog behavior.

## Phase 3 - Designer/Runtime Alignment
- Implement deterministic initialization paths for:
  - Design-time preview
  - Runtime app host/DI
  - Runtime fallback (no DI)
- Provide clear designer serialization boundaries (avoid duplicating shared data in `.Designer.cs`).
- Acceptance criteria:
  - Opening form in designer does not duplicate or corrupt shared connection store.
  - Runtime load resolves the same shared connections created at design-time configuration.

## Phase 4 - Validation, Security, and Migration
- Add connection validation rules based on provider type.
- Add secure handling strategy for sensitive fields (mask/optional encrypted persistence).
- Add migration logic for old `DataConnections.json` records (missing IDs/flags).
- Acceptance criteria:
  - Invalid connection definitions fail with actionable messages.
  - Existing connection files upgrade without data loss.

## Phase 5 - Testing and Adoption
- Unit tests:
  - Registry CRUD + identity rules.
  - Concurrency and event propagation.
  - Migration paths.
- Integration tests:
  - Multi-control synchronization on one form.
  - Shared store discovery across app modules/projects.
- Documentation + examples:
  - “Drop component and share connection” quick-start.
  - DI registration sample for desktop app.
- Acceptance criteria:
  - Verified end-to-end: create connection from one control, consume from another module immediately.

## Proposed Backlog Items (Actionable)
- `DCONN-01` Add `ISharedConnectionRegistry` contract.
- `DCONN-02` Implement `SharedConnectionRegistry` (ConfigEditor-backed).
- `DCONN-03` Refactor `BeepDataConnection` to registry-first design.
- `DCONN-04` Add scope/repo/path component properties.
- `DCONN-05` Add validation and compatibility migration layer.
- `DCONN-06` Add diagnostics/events for save/reload failures.
- `DCONN-07` Add unit/integration test suite.
- `DCONN-08` Publish usage docs and sample form.

## Example Target Flow
1. User drops `BeepDataConnection` on Form A.
2. Component resolves shared registry using app `IBeepService` + `AppRepoName`.
3. User adds connection in designer/runtime.
4. Registry persists once to shared store.
5. Form B/UserControl C loads and automatically sees the same saved connection catalog.
6. App services open datasource by `ConnectionName` through shared configuration.

## Non-Goals (This Iteration)
- Replacing BeepDM `Config_editor` persistence format.
- Full cloud secret vault integration (can be phase-2 security extension).
- Cross-machine synchronization service.

## Recommended Default Decisions
- Default scope: **Project**.
- Default persistence provider: **Config_editor/DataConnections.json**.
- Default registration: app-level singleton `IBeepService` + shared registry.
- Default behavior: `AutoReloadOnChange = true`.

## Success Metrics
- 0 duplicate connection catalogs per app session.
- 100% consistency of connection list across dropped controls in same app scope.
- Reduced setup friction: one-time connection setup reusable across forms/modules.
- No regression in existing form designer behavior.
