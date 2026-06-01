# TODO - Import/Export Wizard

Date: 2026-02-10 | Last updated: 2026-03-03
Target: Convert ImportExport flow to Beep Wizard with 3 guided steps.

## Completed
- [x] Wizard framework integration (3 steps: SelectDSandEntity / MapFields / Run).
- [x] `IWizardStepContent` implemented in all 3 step controls.
- [x] `ImportExportContextStore` — thread-safe state shared between steps.
- [x] `ImportExportOrchestrator` — drives `DataImportManager.RunImportAsync`.
- [x] Removed duplicate `GetMappingCount` — consolidated to `ImportExportOrchestrator.GetMappingCount` (public static).
- [x] Fixed double-import bug — `uc_Import_Run` saves `LastRunSucceeded` flag in wizard context; launcher skips re-running if already ran via Start button.
- [x] Fixed sync-over-async in launcher — replaced `Task.Run(...).GetAwaiter().GetResult()` (blocked UI thread) with fire-and-forget `ContinueWith`; orchestrator disposed inside continuation.
- [x] Fixed `ValidateRequestAsync` — replaced non-existent `manager.TestImportConfigurationAsync` with `manager.ValidationHelper.ValidateImportConfiguration`; method simplified to synchronous `ValidateRequest`.

## Remaining
- [x] End-to-end validate step-to-step data persistence. — `ImportExportWizardValidation.ValidateContextIntegrity` checks all step keys and cross-step consistency.
- [x] Validate mapping roundtrip between steps. — `ImportExportWizardValidation.ValidateMappingRoundtrip` verifies mapping entities, field mappings, and config alignment.
- [x] Validate import runs and destination entity auto-create behaves correctly. — `ImportExportWizardValidation.ValidateAutoCreateSettings` checks destination readiness and `CreateDestinationIfNotExists` flag flow.
- [x] Tie progress bar in `uc_Import_Run` to real batch count from `DataImportManager`. — `ImportStatus.CurrentBatch` / `TotalBatches` added; `StatusTimer_Tick` displays batch info.

## Notes
- `uc_CopyEntities` is kept untouched — separate legacy ETL-script path.
- Use strong-typed `ConnectionProperties`; reserve `ParameterList` only for extras.
