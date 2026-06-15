# Beep WinForms Oracle Forms UI — Enhancement Plan

> **UI-only enhancement roadmap for the Beep Data Management framework Oracle Forms emulation surface.**
>
> **Current Score:** 3/10 (UI) — Engine score: 9.5/10 (FormsManager, Phases 1-9 complete)
> **Target Score:** 9/10 (UI parity with engine capabilities)
> **Audit Date:** 2026-06-15
> **Namespace:** `TheTechIdea.Beep.Winform.Controls.Integrated.Forms`
>
> **Design Rule:** The WinForms layer is UI-only. It routes user actions to the engine (`FormsManager` / `IUnitofWorksManager`) and renders engine state. No business logic, no data access, no trigger logic, no validation rules — these belong to the engine. See [UI-Only Design Rule](#ui-only-design-rule) below.

---

## Architecture Overview (Current State)

```
┌─────────────────────────────────────────────────────────────┐
│                    WinForms UI Layer                         │
│  BeepForms (Panel) — Coordinator + IBeepFormsHost            │
│  ├── BeepFormsHeader        — Title/context surface          │
│  ├── BeepFormsCommandBar    — Block selector + sync          │
│  ├── BeepFormsQueryShelf    — Enter/Execute query buttons    │
│  ├── BeepFormsPersistenceShelf — Commit/rollback buttons     │
│  ├── BeepFormsToolbar       — Savepoints + alerts            │
│  └── BeepFormsStatusStrip   — Shared state reader            │
│                                                              │
│  BeepBlock (Panel) — Single-record form view                 │
│  BeepApplication — Multi-form MDI host                       │
│  LovPickerDialog — Modal LOV picker                          │
│  BeepLogonDialog / SimpleLoginForm — Logon forms             │
├─────────────────────────────────────────────────────────────┤
│                    Manager Adapter                           │
│  BeepFormsManagerAdapter — Bidirectional state sync          │
│  BeepFormsCommandRouter — Command delegation                 │
│  BeepBuiltinsHostAdapter — Built-ins routing                 │
├─────────────────────────────────────────────────────────────┤
│                    Engine (FormsManager)                     │
│  All Oracle Forms built-ins, triggers, LOV, master-detail,   │
│  multi-form, validation, audit, security, performance        │
│  (Phases 1-9 complete, 190/190 tasks)                        │
└─────────────────────────────────────────────────────────────┘
```

---

## What's Working Well (UI Production-Ready)

- ✅ `BeepForms` coordinator with definition-first block materialization
- ✅ 6 visual shell controls with auto-bind to parent `BeepForms`
- ✅ Command routing: EnterQuery, ExecuteQuery, Commit, Rollback
- ✅ Record navigation: MoveFirst, MovePrevious, MoveNext, MoveLast (on coordinator)
- ✅ Master-detail auto-sync on navigation and field change
- ✅ Savepoint CRUD: create, list, rollback, release, release-all
- ✅ Alert dialog with manager-provider fallback
- ✅ Multi-form application host (BeepApplication): OpenForm, GoForm, CloseForm, :GLOBAL variables
- ✅ Trigger lifecycle proxy (TriggerExecuting/TriggerExecuted events)
- ✅ Message/notification system with severity mapping
- ✅ Logon dialog with DataConnection enumeration
- ✅ LOV picker dialog (standalone, not wired to shell)
- ✅ Designer support: smart-tags, UITypeEditors, toolbox integration
- ✅ Configurable button visibility via flags enums (QueryShelf, PersistenceShelf, CommandBar)

---

## Key Gaps (UI Does Not Expose Engine Capabilities)

| Engine Capability | UI Status | Gap |
|-------------------|-----------|-----|
| Record navigation (First/Prev/Next/Last) | Coordinator only | No visual buttons on shelves |
| Insert/Delete/Duplicate Record | Not surfaced | No buttons anywhere |
| Clear Block/Record/Form | Not surfaced | `IBeepBuiltins` has them, no buttons |
| Post (record-level save) | Not surfaced | No Post button |
| LOV show/select | Dialog exists | No "Show LOV" button, no field indicator |
| Query-by-example criteria | Not surfaced | Buttons exist, no criteria entry panel |
| Exit Query / Abort Query | Not surfaced | No cancel-query button |
| Block navigation (Next/Previous Block) | Not surfaced | No arrow buttons |
| Item navigation (Next/Previous Item) | Not surfaced | No buttons |
| Undo/Redo | Not surfaced | No buttons |
| Export/Import (JSON/CSV) | Not surfaced | No buttons |
| Batch commit with progress | Not surfaced | No buttons |
| Block aggregates (Sum/Avg/Count) | Not surfaced | No display |
| Refresh Block | Not surfaced | No button |
| Validation error display per-field | StatusStrip only | No error provider on fields |
| Mode indicator (Normal/Query/CRUD) | Not surfaced | StatusStrip is basic |
| Record position display | Not surfaced | No "Record 3/47" |
| Grid/multi-record view | Not implemented | Only single-record form view |
| Form switching (GoForm/OpenForm) | BeepApplication has it | No visual switcher in BeepFormsHeader |

---

## Phased Enhancement Plan

### Phase 1: Record Navigation & CRUD Toolbar — `Critical`

**Goal:** Add record navigation and CRUD buttons. Add position/mode indicators to StatusStrip.

Add a `BeepFormsRecordNavigationShelf` with First/Previous/Next/Last buttons and a record-position label. Add Insert/Delete/Duplicate buttons to the command surface. Enhance `BeepFormsStatusStrip` with mode, record position, dirty state, and connection name labels.

**Key deliverables:**
- `BeepFormsRecordNavigationShelf` control (new file)
- Extended `BeepFormsCommandBar` with CRUD buttons (or new shelf)
- Enhanced `BeepFormsStatusStrip` with 4 new indicator labels
- Fixed keyboard accessibility in `DriverManagementForm` and `SimpleLoginForm`

**Seam:** All buttons route through `BeepForms` (the coordinator) → `IBeepFormsCommandRouter` → `FormsManager`. No new engine code.

---

### Phase 2: Query-by-Example (QBE) Visual Surface — `High`

**Goal:** Add a dynamic criteria-entry panel that appears in Enter Query mode, letting users fill in search criteria and execute the query with Oracle-style operators.

Create a `BeepFormsQueryCriteriaPanel` that auto-generates editable fields from the active block's entity structure. Support per-field operator selection (`=`, `>`, `LIKE`, `BETWEEN`, `IS NULL`, etc.). Show panel only when in query mode. Clear on exit, retain on execute.

**Key deliverables:**
- `BeepFormsQueryCriteriaPanel` control (new file)
- Enhanced `BeepFormsQueryShelf` with ExitQuery button and count feedback
- Query-mode visual state on blocks and shelves

**Seam:** Criteria are built as `List<AppFilter>` and passed to `BeepForms.ExecuteQueryAsync(blockName, filters)`. The engine handles the rest.

---

### Phase 3: LOV (List of Values) Integration — `High`

**Goal:** Wire the existing `LovPickerDialog` into the BeepForms shell workflow so LOV-registered fields get a "Show LOV" trigger and LOV selection auto-populates related fields.

Add a "Show LOV" button that is enabled only when the active item has a registered LOV. Add LOV indicator icons to `BeepBlock` fields. Enhance `LovPickerDialog` with live search, column resizing, and keyboard navigation.

**Key deliverables:**
- LOV button in CommandBar (or new shelf)
- LOV field indicator on `BeepBlock` fields (chevron/"..." button)
- Enhanced `LovPickerDialog` with live search and column resize
- Auto-complete keystroke filtering

**Seam:** LOV data loading, caching, and validation are engine-owned. The UI only shows the dialog, passes the selection back to `FormsManager.ShowLOVAsync`, and renders the result.

---

### Phase 4: Built-in Action Shelf Expansion — `High`

**Goal:** Expose more of the 30+ Oracle Forms built-ins already available in `IBeepBuiltins`.

Add Clear Block/Record/Form, Post, Refresh Block, Duplicate Record buttons to the toolbar. Add block navigation buttons (Next/Previous Block arrows, GO_BLOCK dropdown). Expand `BeepFormsToolbar` with configurable button visibility flags.

**Key deliverables:**
- Extended `BeepFormsToolbar` with new buttons and flags enum
- Block navigation arrows in `BeepFormsCommandBar`
- All buttons route through `BeepBuiltins` → `BeepBuiltinsHostAdapter` → `FormsManager`

**Seam:** Every button calls a method on `IBeepBuiltins` which routes to the engine. No logic in the UI.

---

### Phase 5: Validation Error Display — `High`

**Goal:** Show per-field validation errors visually (red border, error icon, tooltip) instead of only in the status strip.

Add error state rendering to `BeepBlock` fields. Add error count badge and "Jump to First Error" navigation. Show cross-block validation errors before commit.

**Key deliverables:**
- Per-field error indicators in `BeepBlock` (red border, error icon, tooltip)
- Error count badge in `BeepFormsStatusStrip`
- Error summary dialog
- "Jump to First Error" button

**Seam:** Validation rules and execution are engine-owned. The UI reads validation results from `FormsManager` via `BeepFormsManagerAdapter` and renders them.

---

### Phase 6: Data Operations Surface — `Medium`

**Goal:** Expose Undo/Redo, export/import, batch commit, and block aggregates through visual buttons.

Add Undo/Redo, Export (JSON/CSV/DataTable), Import, Batch Commit, and Refresh buttons. Add aggregate display (Sum/Average/Count) to StatusStrip.

**Key deliverables:**
- Undo/Redo buttons on CommandBar
- Export/Import buttons with file dialogs
- Batch Commit with progress dialog
- Aggregate display row in StatusStrip

**Seam:** All operations delegate to `FormsManager` methods (`UndoBlock`, `ExportBlockToJsonAsync`, `CommitFormBatchAsync`, `GetBlockSum`, etc.). The UI only provides buttons and dialogs.

---

### Phase 7: Status Strip & Header Polish — `Medium`

**Goal:** Enhance the visual information density of BeepFormsHeader and BeepFormsStatusStrip.

Add configurable line presets, severity-based coloring, auto-clear timeout, and workflow history tooltip to StatusStrip. Add mode indicator, block count, and dirty badge to Header.

**Key deliverables:**
- Enhanced `BeepFormsStatusStrip` with line presets and coloring
- Enhanced `BeepFormsHeader` with badges and indicators
- Configurable element visibility via flags enums

---

### Phase 8: Multi-Form Shell Integration — `Medium`

**Goal:** Surface multi-form capabilities (form switching, :GLOBAL variables viewer, inter-form message log).

Add form-switching toolbar to `BeepApplication`. Add debug panels for :GLOBAL variables and inter-form messages.

**Key deliverables:**
- Form switcher in `BeepApplication`
- Global variables viewer panel
- Inter-form message log panel

---

### Phase 9: Reflection Cleanup & Hardening — `Medium`

**Goal:** Replace reflection-based savepoint operations in `BeepForms.WorkflowShell.cs` with direct interface calls.

Two methods (`TryCreateBlockSavepoint`, `TryRollbackToSavepointViaManagerAsync`) use `MethodInfo.Invoke` because the methods were not guaranteed on the interface. They now are. Replace with direct calls.

**Key deliverables:**
- Direct `_formsManager.Savepoints.CreateSavepoint()` call
- Direct `_formsManager.Savepoints.RollbackToSavepointAsync()` call
- Remove `System.Reflection` using from WorkflowShell.cs

---

### Phase 10: Grid-Driven Multi-Record Block View — `Medium`

**Goal:** Support Oracle Forms multi-record block style where records are displayed in a grid view alongside the form view.

Add grid view mode to `BeepBlock` using `BeepGridPro`. Add Form/Grid view toggle button. Support inline editing in grid mode with the same validation/trigger pipeline.

**Key deliverables:**
- `BeepBlockGrid` grid view mode
- View toggle button (Form ↔ Grid)
- Inline editing with trigger/validation pipeline
- Multi-select for batch operations

**Seam:** The grid reads data from the block's `IUnitofWork` via the host. Editing routes through the same `IBeepFormsHost` interface. No new engine code.

---

### Phase 11: Designer Experience Polish — `Low`

**Goal:** Improve the Visual Studio designer experience with auto-generation verbs, entity pickers, and validation commands.

**Key deliverables:**
- "Auto-Generate Shelves" smart-tag action on BeepForms
- Entity picker and validation verbs
- BeepBlock LOV configuration smart-tag

---

## UI-Only Design Rule

The WinForms UI layer is a **thin visual shell** over the Beep Data Management engine. It must never contain:

| Forbidden in UI | Owned By |
|-----------------|----------|
| Business logic or workflow decisions | FormsManager |
| Data access or connection management | IDataSource / IUnitofWork |
| Trigger registration, firing, or chain logic | TriggerManager |
| Validation rules or rule execution | ValidationManager |
| LOV data loading, caching, or validation | LOVManager |
| Savepoint state tracking | SavepointManager |
| Audit trail capture | AuditManager |
| Security checks or permission evaluation | SecurityManager |
| DML operations beyond routing | IUnitofWork |
| Master-detail synchronization logic | FormsManager.Relationships |
| Mode transition logic (EnterQuery/ExecuteQuery) | FormsManager.ModeTransitions |
| Navigation logic beyond calling engine methods | FormsManager.Navigation |

**What the UI does:**
- Renders controls (buttons, labels, text boxes, grids, panels)
- Handles mouse/keyboard input and routes it to the engine
- Subscribes to engine events and updates visual state
- Shows/hides controls based on engine state
- Opens/closes dialogs and windows
- Applies themes and styling
- Handles layout and positioning

**Routing pattern for every user action:**
```
User clicks button → UI event handler → BeepForms coordinator method
    → IBeepFormsCommandRouter / IBeepBuiltins → FormsManager → Engine
```

**State sync pattern for every engine change:**
```
FormsManager event → BeepForms.Events subscription → BeepFormsManagerAdapter.Sync()
    → BeepFormsViewState update → ViewStateChanged event
    → All shelves re-read ViewState → Control.Enabled/Visible/Text updated
```

---

## Cross-Cutting Rules

1. **All new shelves follow the same pattern as existing ones:** derive from `BaseControl`, support `FormsHost` + `AutoBindFormsHost` properties, subscribe to host events, use `BeepFormsHostResolver.Find(this)` for auto-discovery.
2. **Button visibility uses flags enums** (like `BeepFormsQueryShelfButtons`, `BeepFormsPersistenceShelfButtons`). New shelves define their own enum.
3. **All controls use Beep controls** (BeepButton, BeepLabel, etc.) by default. Standard WinForms controls are fallback only.
4. **Designer support is required** for every new public control: `[ToolboxItem(true)]`, `[Designer(...)]` attribute, designer class in `Design.Server`.
5. **All async operations use `ConfigureAwait(true)`** to return to the UI thread.
6. **No direct engine references in shelf controls** — always go through `FormsHost` (the `BeepForms` coordinator) which has the engine reference.
7. **The coordinator (`BeepForms`) is the single entry point** for all engine operations. Shelves never call `FormsManager` directly.

---

## Phase Status Snapshot

| Phase | Current State | Priority | Est. Tasks |
|-------|---------------|----------|------------|
| 1 — Record Nav & CRUD | Not started | Critical | 19 |
| 2 — QBE Visual Surface | Not started | High | 13 |
| 3 — LOV Integration | Not started | High | 12 |
| 4 — Built-in Shelf | Not started | High | 13 |
| 5 — Validation Display | Not started | High | 8 |
| 6 — Data Operations | Not started | Medium | 13 |
| 7 — Status Strip Polish | Not started | Medium | 11 |
| 8 — Multi-Form Shell | Not started | Medium | 9 |
| 9 — Reflection Cleanup | Not started | Medium | 5 |
| 10 — Grid Multi-Record | Not started | Medium | 12 |
| 11 — Designer Polish | Not started | Low | 11 |

---

## Quick Wins (Recommended Starting Point)

1. **Phase 9 first** — Replace 2 reflection calls with direct interface calls. 30 minutes, zero risk.
2. **Phase 1.4** — Uncomment `AcceptButton`/`CancelButton` in 2 files. 5 minutes.
3. **Phase 1.3** — Add mode/position/dirty indicators to StatusStrip. 2 hours.
4. **Phase 1.1** — Add record navigation buttons. 4 hours.
5. **Phase 4.2** — Add block navigation arrows to CommandBar. 2 hours.

These 5 quick wins deliver immediate visible improvement with minimal code changes and no new engine work.

---

## References

- **Engine enhancement plan:** `BeepDM/DataManagementEngineStandard/Editor/Forms/.plans/enhancement-plan.md`
- **Engine todo tracker:** `BeepDM/DataManagementEngineStandard/Editor/Forms/.plans/todo-tracker.md`
- **Oracle Forms mapping:** `BeepDM/DataManagementEngineStandard/Editor/Forms/ORACLE-FORMS-MAPPING.md`
- **Engine gaps:** `BeepDM/DataManagementEngineStandard/Editor/Forms/gaps.md`
- **Engine enhancements:** `BeepDM/DataManagementEngineStandard/Editor/Forms/enhancements.md`
- **UI todo tracker:** [MASTER-TODO-TRACKER.md](MASTER-TODO-TRACKER.md)

---

*Last updated: 2026-06-15*
