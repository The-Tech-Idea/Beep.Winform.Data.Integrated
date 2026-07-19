# Setup & Migration WinForms UI — Enhancement Plan

## Goal

Surface the BeepDM engine's new **versioning** (Setup Phase 9) and **class-reader** (Migration Phase 7)
capabilities in the Integrated WinForms wizards, so an operator can see and drive them — not just the
engine. The wizards already exercise most of the pipeline; this closes the gap where recently-added
engine behavior is invisible or unwired in the UI.

## Scope

**In scope**
- `Setup/uc_SetupWizard.cs` (+ `.Designer.cs`, `.resx`)
- `Setup/SetupWizardLauncher.cs`
- `Setup/uc_SetupSchemaStep.cs` / `Setup/uc_SetupReviewRunStep.cs` (+ designers)
- `Configuration/uc_MigrationWizard.cs` (+ `.Designer.cs`, `.resx`)
- New user-facing controls (checkboxes / text / labels) via Designer edits — the chosen approach.

**Out of scope**
- Breaking `IDMEEditor` / `IDataSource` / `ISetupStep` / `IMigrationManager` signatures (additive only).
- Datasource-specific SQL in the view (stays in the engine/helper layers).
- Re-theming or restructuring the wizards beyond the controls named here.

## Baseline assessment (current gaps, verified)

| # | Gap | Evidence |
|---|---|---|
| G1 | Setup never sets the Phase 9 options — `MigrateOnStartup` / `DeclaredSchemaVersion` are absent from the built `SetupOptions`. | `uc_SetupWizard.cs:410-417` (SetupOptions initializer); properties at `:159-182` have no equivalents. |
| G2 | Migrate-on-startup upgrade pass is never wired — the launcher builds `BeepBootstrapper` with no `upgradeWizardFactory`, so a later launch does not version-check/migrate. | `SetupWizardLauncher.cs:89-90`. |
| G3 | The Migration wizard never stamps the DB version after a successful run — it records idempotency history but not `DatabaseVersion` (the engine's `DbSchemaVersionStore` / `IVersionManagementService`). | `uc_MigrationWizard.cs:694-716` (post-success block records migration, exports artifacts; no version stamp). |
| G4 | No datasource version is ever displayed — neither wizard shows "DB is at vX" even though the engine can now read it from the target DB. | `DbSchemaVersionStore.Read` unused in the UI project. |
| G5 | The Migration wizard can't tune the reader — enum storage, NRT nullability, and discovery markers (`EntityReadOptions`) are not reachable, because `BuildMigrationPlan` uses the reader with defaults. | `uc_MigrationWizard.cs:318-319`; engine `MigrationManager` calls `ConvertToEntityStructure(Type)` with no options. |

## Thin-UI rule (load-bearing)

The views must stay **thin**: no version computation, no replicated engine behavior, no new UI-layer
classes for logic. Every versioning action goes through an engine entry point:

- `MigrationTrackingService.GetCurrentDatabaseVersion(datasource)` — read the recorded DB version.
- `MigrationTrackingService.StampDatabaseVersion(datasource, plan, declaredVersion)` — record it after a
  UI-driven run (owns the version-computation the UI must never duplicate).
- `BeepBootstrapper` + `DefaultSetupWizardFactory.CreateUpgrade` — the startup upgrade pass.

These are shared by every UI (WinForms, WPF, …). A view that hand-rolls a patch-bump or news up
`DbSchemaVersionStore` directly is a bug to fix, not a pattern to copy.

## Design principles

- Datasource-agnostic orchestration in the UI; version/DDL work stays in the engine (`DbSchemaVersionStore`,
  `VersionManagementService`, `MigrationTrackingService`).
- Additive, non-breaking: new option properties default to the engine's defaults (`MigrateOnStartup=true`,
  reader defaults), so existing behavior is unchanged until an operator opts in.
- Non-throwing UX: version reads/stamps are best-effort — a failure logs to status, never breaks a run.
- Beep controls only, matching each wizard's existing look; new controls sit in the natural stage.

## Phase A — Setup: surface Phase 9 versioning

**Files:** `uc_SetupWizard.{cs,Designer.cs}`, `uc_SetupReviewRunStep.{cs,Designer.cs}`, `SetupWizardLauncher.cs`

- **A1 (G1)** Add public properties `MigrateOnStartup` (default true) and `DeclaredSchemaVersion` (string,
  optional) to `uc_SetupWizard`; set them on the `SetupOptions` built at `:410`. Also pass entity scope to
  `SetupOptions.EntityAssemblies`/`EntityTypeNames` so the upgrade pass can resolve the model.
- **A2 (controls)** On the Review/Run step, add a **"Migrate database on startup"** checkbox and a
  **"Declared schema version"** textbox, bound to A1's properties. Add a read-only **"Current DB version"**
  label populated via `DbSchemaVersionStore.Read(datasource)` once a connection is chosen.
- **A3 (G2)** In `SetupWizardLauncher`, pass an `upgradeWizardFactory` to `BeepBootstrapper` (via
  `DefaultSetupWizardFactory.CreateUpgrade` or the shell's own gate step) so a completed setup runs the
  **version gate** on later launches; surface any `MigratedFrom→MigratedTo` movement in a toast/log.

## Phase B — Migration: version tracking & display

**Files:** `uc_MigrationWizard.{cs,Designer.cs}`

- **B1 (G4)** In the **Scope** stage, after the datasource is chosen, show a read-only **"DB version"**
  label from `DbSchemaVersionStore.Read`. In the **Run** stage summary, show the version the run will move to.
- **B2 (G3)** After a successful `ExecuteMigrationPlanAsync` (`:694`), stamp the new `DatabaseVersion`
  (in-DB marker + `IVersionManagementService` mirror), reusing the engine's `MigrationTrackingService`
  stamping semantics (declared version when provided, else patch-bump). Log "DB vX → vY".
- **B3 (optional control)** A **"Declared version"** textbox in the Scope stage feeds B2's stamp, mirroring
  Setup's `DeclaredSchemaVersion`.

## Phase C — Migration: reader options (needs engine plumbing)

**Depends on an engine change — flagged for a separate decision.**

- **C0 (engine)** Thread `EntityReadOptions` through `MigrationManager.BuildMigrationPlan(ForTypes)` →
  `TryGetEntityStructure` → `ConvertToEntityStructure(Type, EntityReadOptions)` (the overload already exists
  on `ClassCreator`). Additive optional parameter; default = current behavior.
- **C1 (controls)** In the Scope stage add: **enum storage** (Int / String) dropdown, **"Honor nullable
  reference types"** checkbox, and a note that `[BeepEntity]`/`[BeepIgnore]` markers govern discovery.
  These bind to an `EntityReadOptions` passed into C0.

> Without C0 these controls would be inert, so Phase C ships only if the engine plumbing is approved. Phases
> A and B are fully deliverable against today's engine.

## Task tracker

| # | Task | Phase | Engine dep | Status |
|---|---|---|---|---|
| A1 | `MigrateOnStartup` / `DeclaredSchemaVersion` → SetupOptions | A | no | [x] |
| A2 | Review-step version-info line + current-DB-version | A | no | [x] |
| A3 | `TryRunStartupUpgradeAsync` upgrade-pass in `SetupWizardLauncher` | A | no | [x] |
| B1 | DB-version shown in plan summary | B | no | [x] |
| B2 | Stamp version after successful run | B | no | [x] |
| B3 | Declared-version input (Scope) | B | no | [~] property `DeclaredVersion` added; no visible textbox yet |
| C0 | Thread `EntityReadOptions` through MigrationManager | C | **yes** | [x] |
| C1 | Reader-option controls (enum/NRT) | C | **yes** | [x] |

## Implemented (2026-07-19)

Phases A and B landed; Views project builds clean (`net10.0-windows`, 0 errors).

- **A1** — `uc_SetupWizard.{MigrateOnStartup, DeclaredSchemaVersion}` set on `SetupOptions` (+ `EntityTypeNames`/`EntityAssemblies` for the upgrade pass).
- **A2** — read-only version line on the review step (`uc_SetupReviewRunStep.SetVersionInfo`, added programmatically into the docked content panel), populated by `uc_SetupWizard.UpdateReviewVersionInfo` via `DbSchemaVersionStore.Read`. (Editable toggles deferred: `SetupOptions` is `init`-only, so a checkbox at review time can't feed back without rebuilding options — the values are correctly host/developer-set.)
- **A3** — `SetupWizardLauncher.TryRunStartupUpgradeAsync(datasource, entityTypes, declaredVersion, migrateOnStartup, …)`: builds `DefaultSetupWizardFactory.CreateUpgrade` + `BeepBootstrapper` upgrade delegate, runs the version gate on a completed install, logs any `MigratedFrom→MigratedTo`. Host calls it on startup.
- **B1** — plan summary now shows `DB version: x` via `TryReadDbVersion`.
- **B2** — `StampVersion` records the new `DatabaseVersion` (in-DB marker + `IVersionManagementService`) after a successful run, patch-bumping or using `DeclaredVersion`; logged in the run log.

**Phase C (2026-07-19):** delivered including the engine change.
- **C0 (engine, BeepDM)** — `MigrationManager.ReadOptions` (`EntityReadOptions`, cache-clearing setter);
  `TryGetEntityStructure` uses the concrete `ClassCreator` options overload (no `IClassCreator` change).
  2 new tests (`ReadOptionsThreadingTests`) prove enum-as-text flows through planning; MigrationManagerTests 68/68.
- **C1 (UI)** — two Scope-stage checkboxes ("Store enums as text", "Honor nullable reference types")
  added **programmatically** into the existing `TableLayoutPanel` (two 35px rows inserted before the filler),
  feeding `BuildReadOptions()` → `migration.ReadOptions` before `BuildMigrationPlan`. Generated Designer
  file untouched.

**Verification note:** builds are green (Views `net10.0-windows`, engine net8.0/net9.0; MigrationManagerTests 68/68),
but WinForms visuals (the new review-step line, the two Scope checkboxes, summary text) were not run-verified in
this environment — a manual smoke run is the remaining check. The C1 checkboxes were added in code rather than via
Designer edits so the layout change is easy to review/revert if it needs nudging on a running UI.

## Verification

- `dotnet build TheTechIdea.Beep.Winform.Default.Views.csproj` (0 errors) after each phase.
- Manual smoke via the existing wizards: run Setup once → relaunch and confirm the version gate reports
  up-to-date; run the Migration wizard → confirm the DB-version label populates and updates after a run.
- Regression notes under `docs/regression/` per this repo's convention.
