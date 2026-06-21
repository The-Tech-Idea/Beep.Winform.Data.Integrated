# Remaining WinForms Forms Parity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Complete the remaining WinForms surfaces for audit, undo/redo, cross-block validation, item properties, and dirty-state workflows.

**Architecture:** Extend `IBeepFormsHost` with platform-neutral operations already owned by `FormsManager`. Implement those operations only in `WinFormFormHost`; feature controls continue to depend solely on `IBeepFormsHost`. `WinFormBlockHost.SyncFromManager` applies engine item metadata to presenters.

**Tech Stack:** C# 12, .NET 10 WinForms, xUnit, Moq, BeepDM FormsManager.

---

### Task 1: Platform-neutral contracts

**Files:**
- Modify: `BeepDM/DataManagementModelsStandard/Editor/Forms/Hosts/IBeepFormsHost.cs`
- Modify: `BeepDM/DataManagementModelsStandard/Editor/Forms/Interfaces/IUnitofWorksManager.cs`
- Test: `BeepDM/DataManagementEngineStandard/Editor/Forms.Tests/FormsHostContractTests.cs`

- [x] Add contract tests for audit, undo/redo, cross-block validation, item properties, and dirty-state methods.
- [x] Run the contract test and confirm it fails because methods are absent.
- [x] Add signatures matching existing `FormsManager` APIs.
- [x] Run the contract test and confirm it passes.

### Task 2: WinFormFormHost delegation

**Files:**
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.Audit.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.DataState.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.ItemProperties.cs`
- Test: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostAdvancedTests.cs`

- [x] Add failing tests proving delegation and refresh behavior.
- [x] Implement thin manager delegation with block refresh after state-changing operations.
- [x] Run focused tests and confirm they pass.

### Task 3: Manager-free feature controls

**Files:**
- Create: `Forms/FeatureControls/WinFormAuditPanel.cs`
- Create: `Forms/FeatureControls/WinFormUndoRedoPanel.cs`
- Create: `Forms/FeatureControls/WinFormCrossBlockValidationPanel.cs`
- Create: `Forms/FeatureControls/WinFormItemPropertyPanel.cs`
- Create: `Forms/FeatureControls/WinFormDirtyStatePanel.cs`
- Test: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFeatureControlTests.cs`

- [x] Add failing tests that mock only `IBeepFormsHost`.
- [x] Implement controls as thin command/query wrappers.
- [x] Run focused tests and confirm they pass.

### Task 4: Presenter synchronization and documentation

**Files:**
- Modify: `Forms/BlockHost/WinFormBlockHost.cs`
- Modify: `Forms/ENGINE-GAP-ANALYSIS.md`
- Modify: `Forms/README.md`
- Test: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormBlockHostBindingTests.cs`

- [x] Add a failing test for prompt, visibility, required, enabled, and read-only state.
- [x] Apply `ItemInfo` during synchronization.
- [x] Document the five completed surfaces.

### Task 5: Verification and commits

- [x] Run all BeepDM Forms tests.
- [x] Build BeepDM for `net10.0`.
- [x] Run all WinForms Forms tests.
- [x] Build the WinForms integrated controls project.
- [x] Run the architecture scan ensuring manager/data-source references remain under `Forms/FormHost`.
- [ ] Commit scoped BeepDM changes on `master`.
- [ ] Commit scoped WinForms changes on `master`.
