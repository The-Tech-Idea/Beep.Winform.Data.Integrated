# Phase 7: EntityStructure → BeepBlock Pipeline (Full Production Path)

> Replaces the original single-goal plan. This document covers five ordered sub-phases that together make BeepBlock a production-ready, zero-manual-wiring UI surface backed by any BeepDM datasource.

---

## Sub-Phase Map

| Sub-Phase | Title | Depends On |
|---|---|---|
| **7A** | EntityField Fidelity — complete the field snapshot | none |
| **7B** | Direct EntityStructure Feed on BeepBlock | 7A |
| **7C** | FieldCategory → ControlType Resolution | 7A |
| **7D** | FormsManager Bootstrap From Definition | 7A, 7B, 7C |
| **7E** | Designer-Time EntityStructure Preview | 7A, 7D |

---

# ORIGINAL PHASE 7 HEADER (renamed to 7D below)

## Background — The Bootstrap Gap

The current runtime wiring is architecturally correct **once FormsManager already has registered blocks**:

- `BeepBlock.SyncFromManager()` reads `manager.GetBlock(blockName).UnitOfWork.EntityStructure` → builds runtime field definitions → scaffolds editors   ✅
- `BeepForms.SyncFromManager()` syncs ViewState + calls SyncFromManager on each block ✅
- `BeepForms.Events.cs` subscribes to manager events ✅

**What is missing** is the initiation path:

```
BeepFormsDefinition
  └── Block[0].Entity.ConnectionName = "NorthwindDB"
  └── Block[0].Entity.EntityName    = "Customers"
        ↓ NOBODY converts this into ↓
FormsManager.RegisterBlock("customers", datasource, entityStructure)
        ↓ which would trigger ↓
BeepBlock.SyncFromManager()  →  schema flows down to UI
```

Today a developer must:
1. Manually instantiate a datasource (open connection, get entity structure)
2. Manually call FormsManager to register blocks
3. Then assign FormsManager to BeepForms

That breaks the goal of "set `Definition` + `FormsManager` and have the UI populate itself."

There is also **no wiring** between the `BeepDataConnection` component and `BeepForms`. They are independent components with no glue.

---

## Goal

Provide a bootstrap path so that:
1. Setting `BeepForms.Definition` (or assigning `BeepForms.FormsManager`) triggers FormsManager to connect to datasources, register blocks, and load entity schemas — using the connection/entity info from `BeepFormsDefinition.Blocks[].Entity`.
2. `BeepDataConnection` can be assigned to `BeepForms` as the connection provider.
3. Developers can opt out of auto-bootstrap when they prefer to wire FormsManager manually.

---

## Architectural Rules (must not be broken)

1. **FormsManager owns all datasource operations.** BeepForms/BeepBlock must never call `IDataSource` directly.
2. **BeepBlock never registers itself.** Block registration is a form-level / application-level concern.
3. **`BeepFormsDefinition` is UI-side only.** It carries UI metadata (connection name, entity name, field overrides). It does not own datasource objects.
4. **Bootstrap is optional / replaceable.** Advanced scenarios (multi-step init, explicit external wiring) must still work without auto-bootstrap.

---

## Deliverables

### D1 — `IBeepFormsBootstrapper` contract
Encapsulates the block registration sequence so it is swappable and testable. Default implementation uses `BeepDataConnection`.

```csharp
public interface IBeepFormsBootstrapper
{
    /// <summary>
    /// Called by BeepForms when a Definition is assigned and FormsManager is available.
    /// Implementations register each block with FormsManager using connection/entity info
    /// from the supplied block definitions.
    /// Returns true if all blocks were registered successfully.
    /// </summary>
    Task<bool> BootstrapAsync(
        IUnitofWorksManager formsManager,
        IReadOnlyList<BeepBlockDefinition> blocks,
        CancellationToken cancellationToken = default);
}
```

### D2 — `BeepFormsDefaultBootstrapper`
Default implementation backed by `IBeepService` (from `BeepDataConnection` or injected):

1. For each `BeepBlockDefinition` with a non-empty `Entity.ConnectionName` + `Entity.EntityName`:
   - Call `formsManager.RegisterBlockAsync(blockName, connectionName, entityName)` — FormsManager opens the datasource, retrieves `EntityStructure`, creates UoW.
2. Call `formsManager.OpenFormAsync(formName)` once all blocks are registered.
3. Raise `BootstrapCompleted` event on `BeepForms` (succeeded or failed per-block).

### D3 — `BeepForms.Bootstrapper` property
```csharp
[Browsable(false)]
public IBeepFormsBootstrapper? Bootstrapper { get; set; }
```
Defaults to `BeepFormsDefaultBootstrapper`. Set to `null` to disable auto-bootstrap.

### D4 — Bootstrap trigger in `BeepForms`
Called when both conditions are met: `FormsManager != null` AND `Definition != null` (with at least one block def that has connection/entity info). Sequence:

```
if (Bootstrapper != null && FormsManager != null && Definition?.Blocks.Any() == true)
    await Bootstrapper.BootstrapAsync(FormsManager, Definition.Blocks);
// then:
SyncFromManager();
```

Triggered from:
- `FormsManager` property setter (if `Definition` already assigned)
- `Definition` property setter (if `FormsManager` already assigned)
- Explicit `BeepForms.InitializeAsync()` for forms that need async completion feedback

### D5 — `BeepForms.DataConnection` property
```csharp
[Browsable(true)]
[Category("Data")]
[Description("BeepDataConnection component that provides the IBeepService used to resolve datasources during bootstrap.")]
public BeepDataConnection? DataConnection { get; set; }
```

When `DataConnection` is set, `BeepFormsDefaultBootstrapper` uses its `BeepService` to resolve datasources. This is the design-time drag-and-wire path.

### D6 — `BeepForms.InitializeAsync()` public method
```csharp
public Task<bool> InitializeAsync(CancellationToken cancellationToken = default);
```
Allows programmatic bootstrap trigger from `Form.Load` or application startup without depending on property-setter side-effects.

### D7 — No-bootstrap path remains valid
If `Bootstrapper = null` OR if FormsManager already has registered blocks before `BeepForms.FormsManager` is set:
- Skip auto-bootstrap
- Call `SyncFromManager()` directly (current behavior)

---

## Work Items

1. Define `IBeepFormsBootstrapper` interface in `Forms/Contracts/`.
2. Implement `BeepFormsDefaultBootstrapper` in `Forms/Services/`.
3. Add `DataConnection` property to `BeepForms.cs`.
4. Add `Bootstrapper` property to `BeepForms.cs`.
5. Add bootstrap trigger logic in `BeepForms.cs` (property setters + `InitializeAsync`).
6. Add `BootstrapCompleted` event and `BootstrapState` view-state flag to `BeepFormsViewState`.
7. Surface bootstrap status in `BeepFormsStatusStrip` (loading indicator while bootstrap runs).
8. Update `BeepForms` designer smart tag to expose `DataConnection` and `Bootstrapper` options.
9. Add `FormsManager.RegisterBlockAsync(blockName, connectionName, entityName)` check — verify if this method already exists on `IUnitofWorksManager`; if not, create a gap issue in BeepDM.
10. Update phase-5 samples to demonstrate the bootstrap path end-to-end.

---

## FormsManager API Gap Check

Before D3 can be implemented, verify that `IUnitofWorksManager` (in BeepDM) exposes:
- `RegisterBlockAsync(blockName, connectionName, entityName)` — or equivalent
- `OpenFormAsync(formName)` — or equivalent

If these do not exist as async methods, the bootstrapper must use the synchronous equivalents on a background thread, or a BeepDM gap issue must be filed.

> **Action item:** Check `IUnitofWorksManager` interface + `FormsManager.cs` in BeepDM for these methods.

---

## Exit Criteria

- A developer can drag `BeepForms` + `BeepDataConnection` onto a form, set `Definition` with connection/entity info, and without any other manual wiring the blocks populate from FormsManager.
- Developers who prefer manual registration still receive `SyncFromManager()` calls and nothing breaks.
- Bootstrap errors are visible in `BeepFormsStatusStrip` and do not crash the application.

---

## Risks

- `IUnitofWorksManager` may not expose async block-registration from just a connection name + entity name string → requires BeepDM API addition
- Bootstrap on property setter can be confusing in designer (VS calls setters at design time) → guard with `DesignMode` check
- Race conditions if `FormsManager` is set multiple times before first bootstrap completes → use cancellation token and cancel previous bootstrap
