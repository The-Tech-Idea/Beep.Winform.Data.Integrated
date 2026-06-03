# Setup Controls Integration Guide

This guide explains how the Setup controls, Nuggets Manager, Connection controls, and Import/Export launcher work together with the BeepDM setup framework.

## Architecture Overview

The Setup module is composed of five user controls that are hosted by `WizardConfig` (from `TheTechIdea.Beep.Winform.Controls.Wizards`):

```
uc_SetupWizard  (orchestrator)
├── Driver Provisioning   -> uc_SetupDriverStep   (embeds uc_NuggetsManage)
├── Connection            -> uc_SetupConnectionStep (embeds uc_DataConnectionBase)
├── Schema Migration      -> uc_SetupSchemaStep   (uses MigrationManager)
├── Seeding               -> uc_SetupSeedingStep   (uses ISeederRegistry)
└── Review and Run        -> uc_SetupReviewRunStep (uses SetupReport)
```

Each step control is a self-contained `UserControl` that knows how to:

- Initialize from the host (`InitializeStep(...)`)
- Expose a framework-friendly options object via `BuildStepOptions()` (where applicable)
- Expose a `GetStepSummary()` for the review step
- Expose an `IsReadyForSetup()` readiness check

## BeepDM Setup Framework Integration

The wizard runs steps through the BeepDM setup framework (`TheTechIdea.Beep.SetUp`):

- `ISetupWizardFactory` — produces `(ISetupWizard, SetupContext)`
- `ISetupWizard` — runs a sequence of `ISetupStep`s
- `ISetupStep` — atomic unit of work (driver provision, connection, schema, seeding)
- `SetupContext` — shared mutable state (editor, datasource, options, state, properties)
- `SetupState` — checkpoint/resume state (completed/skipped step IDs, schema hash, completed seeder IDs)
- `SetupOptions` — dry-run, skip flags, environment, strict policy, state file, report path
- `SetupReport` — per-step results, timing, SHA-256 content hash

### Step Pipeline (Built by `uc_SetupWizard.BuildFrameworkWizard`)

When the user clicks "Run Setup", the orchestrator composes a wizard from `SetupWizardBuilder`:

1. `ConnectionConfigStep` — persists and opens the connection (uses `ConnectionHelper.GetBestMatchingDriver`).
2. `SchemaSetupStep` — added only when `uc_SetupSchemaStep.IsReadyForSetup()` returns true (editor, open datasource, entity types).
3. `SeedingStep` — added only when `uc_SetupSeedingStep.IsReadyForSetup()` returns true (non-null registry with at least one seeder).

The schema and seeding steps are added only when their respective step controls are ready. The connection step is always added when a connection name is available. The wizard then calls `Run(context, progress)`, captures the `SetupReport`, and forwards it to `uc_SetupReviewRunStep.SetReport(...)`.

### Context Seeding

Before running, the wizard seeds `SetupContext` with:

- `Editor = beepService.DMEEditor`
- `Options.SkipSeeding = _seederRegistry == null` (auto-skip if no registry)
- `State = new SetupState()` (fresh or supplied)
- `ConnectionProperties = cp` (when available)
- `DataSource = editor.GetDataSource(cp.ConnectionName)` when the datasource is already open

If any of `Editor`, `ConnectionName`, or `DataSource` is missing, the orchestrator returns `null` from `BuildFrameworkWizard` and the `Run` falls back to a plain connection-persist path.

## Controls and Roles

### `uc_SetupWizard`

Orchestrates the setup flow.

- Builds a `WizardConfig` with `WizardStyle`, `AllowCancel`, `AllowSkip`, theme, and five steps.
- Exposes:
  - `SeederRegistry` (set by host to enable seeding)
  - `EntityTypes` (set by host to enable schema step)
  - `ExtraAssemblies` (optional assemblies for `MigrationManager` discovery)
  - `SetupExecutor` (optional external delegate)
  - `GetSnapshot()` / `ApplySnapshot(...)`
- Fires `SetupCompleted` after a successful run.

Execution priority at "Run Setup":

1. `SetupExecutor` delegate (if provided)
2. BeepDM setup framework (preferred when a connection is available)
3. Built-in fallback connection save/open path

### `uc_SetupDriverStep`

Hosts `uc_NuggetsManage` when a service provider is available.

- `InitializeStep(IServiceProvider?, string theme)`
- Tracks the latest package install operation (id, version, success, message) so the summary reflects actual state.
- Raises `DriverPackageInstalled` for downstream consumers.

### `uc_SetupConnectionStep`

Hosts `uc_DataConnectionBase`.

- `InitializeStep(IBeepService?, string theme)`
- `GetConnectionPropertiesForStep()` returns the connection snapshot seeded with the best-matching driver via `ConnectionHelper.GetBestMatchingDriver`.
- `IsReadyForSetup()` validates `ConnectionName`, `DatabaseType`, and `ConnectionString`.
- `GetStepSummary()` includes driver name and readiness status.
- Raises `ConnectionSaved`, `ConnectionCancelled`, `ConnectionTestCompleted`.

### `uc_SetupSchemaStep`

Drives `MigrationManager` to produce a migration summary.

- `InitializeStep(editor, dataSource, entityTypes?, extraAssemblies?)`
- `IsReadyForSetup()` requires an editor, an open datasource, and at least one entity type.
- `BuildStepOptions()` returns a `SchemaSetupStepOptions` ready to feed a `SchemaSetupStep`.
- `GetStepSummary()` reflects `MigrationSummary.HasPendingMigrations` and `TotalPendingMigrations`.
- Fires `SummaryChanged` with `SchemaSummaryEventArgs` whenever the summary refreshes.
- Uses `MigrationManager.GetMigrationSummaryForTypes(entityTypes)` for a type-based summary.

### `uc_SetupSeedingStep`

Drives the seeding framework.

- `InitializeStep(ISeederRegistry?, extraAssemblies?)`
- `IsReadyForSetup()` requires a non-null registry with at least one seeder.
- `BuildStepOptions()` returns a `SeedingStepOptions` ready to feed a `SeedingStep`.
- `GetStepSummary()` reflects the registered seeder count.
- Uses `ISeederRegistry.GetOrderedSeeders()` to verify topological order.
- Fires `SummaryChanged` with `SeedingSummaryEventArgs`.

### `uc_SetupReviewRunStep`

Displays the summary, progress, and run outcome.

- `SetSummary(string)` — initial review text
- `SetProgress(int, string)` — live progress updates
- `SetReport(SetupReport)` — populates the run report (per-step results, timing, content hash, environment)
- `SetExecutionPath(string)` / `SetLastRunSummary(string)` — last run metadata
- `SetRunningState(bool)` — toggles the run button
- Raises `RunSetupRequested` when the user clicks the run button.

## Migration Form (DataManagement) Integration

`uc_SetupSchemaStep` shares the same migration pipeline as the migration form:

- Uses `MigrationManager` from `TheTechIdea.Beep.Editor.Migration`
- Computes a `MigrationSummary` with the same `EntitiesToCreate` / `EntitiesToUpdate` / `EntitiesUpToDate` lists
- Honors `applyForeignKeys` and `applyIndexes` (passed through `SchemaSetupStep` execution)

This means the setup step produces the same plan/policy/dry-run/preflight/compensation/execute results as the standalone migration form.

## Wizard Control (Beep.Winform.Controls.Wizards) Integration

The host wizard (`WizardConfig` / `WizardStep`) lives in `TheTechIdea.Beep.Winform.Controls.Wizards`:

- `WizardConfig` — `Key`, `Title`, `Style`, `AllowCancel`, `AllowSkip`, `Theme`, `ShowProgressBar`, `ShowStepList`, `Steps`, `OnComplete`, `OnCancel`
- `WizardStep` — `Key`, `Title`, `Description`, `Icon`, `Content` (the step control), `IsOptional`, `OnEnter`
- `WizardManager.ShowWizard(config, owner?)` — entry point

Each setup step is hosted as a `WizardStep.Content`. The orchestrator's `OnEnter` callbacks refresh the embedded control with the latest context (e.g., schema step pulls the current datasource, driver step resets its package state).

## Wiring the Host

```csharp
// Optional: feed the schema step with the entity types you intend to migrate.
var entityTypes = new[] { typeof(Customer), typeof(Order) };
var assemblies = new[] { typeof(Customer).Assembly };
var registry = new SeederRegistry();
registry.Register(new RolesSeeder());
registry.DiscoverFromAssemblies(assemblies);

setupWizard.EntityTypes = entityTypes;
setupWizard.ExtraAssemblies = assemblies;
setupWizard.SeederRegistry = registry;
```

When the user clicks "Run Setup", the orchestrator:

1. Resolves the connection properties from `uc_SetupConnectionStep`.
2. Calls `BuildFrameworkWizard(cp)` to build a `SetupWizard` with the eligible steps.
3. Calls `wizard.Run(context, progress)` on a background thread.
4. Captures `wizard.GetReport()` and passes it to `uc_SetupReviewRunStep.SetReport(...)`.
5. Reports success/failure via the `SetupCompleted` event.

## Recommended Cross-Control Wiring

Use the controls in this order for a smooth first-run workflow:

1. Driver provisioning
2. Connection configuration and test
3. Setup execution
4. Import/Export workflow prefilled from setup outputs

### Example: Setup -> Import/Export handoff

```csharp
var setupSnapshot = setupWizard.GetSnapshot();
var cp = setupSnapshot.ConnectionProperties;

if (cp != null && !string.IsNullOrWhiteSpace(cp.ConnectionName))
{
    var selection = new uc_ImportExportWizardLauncher.ImportExportSelectionSnapshot
    {
        Direction = ImportExportDirection.Import,
        SourceDataSourceName = cp.ConnectionName,
        DestinationDataSourceName = cp.ConnectionName
    };

    importExportLauncher.ApplySelectionSnapshot(selection);
}
```

### Example: Nuggets -> Setup status propagation

```csharp
nuggets.PackageInstallCompleted += (_, e) =>
{
    // e.PackageId, e.Version, e.Success, e.Message
    // write status to host shell, logger, or notification panel
};
```

### Example: Connection editor in standalone host

```csharp
var editor = new uc_DataConnectionBase();
editor.InitializeDialog(new ConnectionProperties());

editor.ConnectionSaved += (_, e) =>
{
    var saved = e.ConnectionProperties;
    // propagate into setup/import/export selection
};

editor.ConnectionTestCompleted += (_, e) =>
{
    // e.Success and e.Message for UX feedback
};
```

## Integration Notes

- Keep the controls loosely coupled through events and snapshot DTOs.
- Avoid direct dependencies between Setup and Import/Export internals.
- Prefer pushing only immutable snapshots across control boundaries.
- Let each control remain owner of its own validation and UI state.
- Use the BeepDM setup framework's `SetupReport` for audit-grade run history; the review step renders the per-step outcomes.
- For production deployments, set `SetupOptions.StrictPolicyMode = true` to convert policy warnings into blockers.
- For long-running migrations, configure `SetupOptions.StateFilePath` and `SetupOptions.ReportOutputPath` so the run can resume after interruption and produce a JSON/Markdown report.
