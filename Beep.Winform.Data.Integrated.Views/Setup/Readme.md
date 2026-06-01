# Setup Controls Integration Guide

This guide explains how the Setup controls, Nuggets Manager, Connection controls, and Import/Export launcher work together.

## Controls and Roles

- `uc_SetupWizard`
  - Orchestrates setup flow (Driver -> Connection -> Schema -> Seeding -> Review/Run).
  - Supports three execution paths:
    - External executor (`SetupExecutor` delegate)
    - BeepDM setup framework (`ISetupWizardFactory` resolved from DI at runtime)
    - Fallback connection persistence/open path
  - Exposes snapshot APIs for interoperability:
    - `GetSnapshot()`
    - `ApplySnapshot(SetupWizardSnapshot)`

- `uc_SetupDriverStep`
  - Hosts `uc_NuggetsManage` when service provider is available.
  - Raises `DriverPackageInstalled` for package install telemetry.

- `uc_SetupConnectionStep`
  - Hosts `uc_DataConnectionBase`.
  - Exposes:
    - `GetConnectionProperties()`
    - `SetConnectionProperties(ConnectionProperties)`
  - Raises:
    - `ConnectionSaved`
    - `ConnectionCancelled`
    - `ConnectionTestCompleted`

- `uc_DataConnectionBase`
  - Full connection editor with validation and test.
  - Raises lifecycle events:
    - `ConnectionSaved`
    - `ConnectionCancelled`
    - `ConnectionTestCompleted`

- `uc_NuggetsManage`
  - Package/source lifecycle manager.
  - Raises `PackageInstallCompleted` when install finishes.

- `uc_ImportExportWizardLauncher`
  - Import/Export workflow launcher.
  - Exposes state exchange APIs:
    - `GetSelectionSnapshot()`
    - `ApplySelectionSnapshot(ImportExportSelectionSnapshot)`
  - Raises `SelectionSnapshotChanged` when direction/source/destination selection changes.

## Recommended Cross-Control Wiring

Use the controls in this order for a smooth first-run workflow:

1. Driver provisioning
2. Connection configuration and test
3. Setup execution
4. Import/Export workflow prefilled from setup outputs

### Example: Setup -> Import/Export handoff

```csharp
// setupWizard and importExportLauncher are controls hosted by your shell
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

`uc_SetupWizard` already consumes driver package events from `uc_SetupDriverStep` and updates review status. If you host `uc_NuggetsManage` directly, wire this event the same way:

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

## Current Execution Behavior in Setup Review/Run

At runtime, `uc_SetupWizard` executes in this priority order:

1. `SetupExecutor` delegate if provided
2. Dynamic BeepDM setup framework path via `ISetupWizardFactory` from DI
3. Built-in fallback connection save/open path

The review step displays:

- Current execution path used
- Last run summary
- Progress/status updates
