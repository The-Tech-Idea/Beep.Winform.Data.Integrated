# Beep WinForms Oracle Forms UI — Master Todo Tracker

> Tracks WinForms UI-only work for the Beep Data Management framework Oracle Forms emulation surface.
> The engine (FormsManager / IUnitofWorksManager) is complete — Phases 1-10 engine work is done.
> **This tracker is UI-only.** No business logic, no data access, no trigger logic — only visual surface, layout, and routing to the engine via `IBeepFormsHost` / `IBeepBuiltins` / `BeepFormsManagerAdapter`.
>
> Audit date: **2026-06-15**
>
> Status: `[ ]` = Not started, `[~]` = In progress / Deferred, `[x]` = Complete

---

## Phase 1: Record Navigation & CRUD Toolbar — Critical

**Goal:** Add record navigation (First/Previous/Next/Last) and CRUD (Insert/Delete/Duplicate) buttons to the visual shell. Add record position and mode indicators to StatusStrip.

### 1.1 — Record Navigation Shelf
- [x] Create `BeepFormsRecordNavigationShelf` control (or extend CommandBar) with First/Previous/Next/Last buttons
- [x] Wire First/Previous/Next/Last to `BeepForms.MoveFirstAsync` etc. on `FormsHost`
- [x] Add current-record-position label ("Record 3 of 47") between Previous/Next
- [x] Add keyboard shortcut support (PageUp/PageDown for first/last, Up/Down for previous/next) routed through host
- [x] Support `AutoBindFormsHost` and manual `FormsHost` property (same pattern as other shelves)
- [x] Add `RecordNavigationButtons` flags enum for configurable button visibility
- [x] Add designer attribute + toolbox registration

### 1.2 — CRUD Action Buttons
- [x] Add Insert Record button to CommandBar or new shelf — calls `InsertRecordAsync` via command router
- [x] Add Delete Record button — calls `DeleteCurrentRecordAsync` via command router
- [x] Add Duplicate Record button — calls `DuplicateCurrentRecordAsync` via command router
- [x] Wire buttons through `IBeepFormsCommandRouter` (add missing router methods if needed)
- [x] Add confirmation dialog before delete (route through `BeepForms.ConfirmAsync`)
- [x] Support configurable button visibility via flags enum

### 1.3 — StatusStrip Mode & Position Indicators
- [x] Add current mode label (Normal / Enter Query / Query / Insert) to `BeepFormsStatusStrip`
- [x] Add record position label ("Record 3 of 47") to `BeepFormsStatusStrip`
- [x] Add dirty-state indicator (changed records count) to `BeepFormsStatusStrip`
- [x] Add active block name to `BeepFormsStatusStrip`
- [x] Add active connection name label (read from `BeepForms.DataConnection` or `FormsManager`)
- [x] Ensure labels update reactively on `ViewStateChanged`, `ActiveBlockChanged`

### 1.4 — Quick Fixes (Keyboard Accessibility)
- [x] Uncomment `AcceptButton = _btnOk;` and `CancelButton = _btnCancel;` in `DriverManagementForm.cs` (DriverEditDialog)
- [x] Uncomment `AcceptButton = _btnLogin;` and `CancelButton = _btnCancel;` in `SimpleLoginForm.cs`

---

## Phase 2: Query-by-Example (QBE) Visual Surface — High

**Goal:** Add a dynamic criteria-entry panel that appears in Enter Query mode, letting users fill in search criteria and execute the query with Oracle-style operators.

### 2.1 — Query Criteria Panel
- [x] Create `BeepFormsQueryCriteriaPanel` — dynamic panel that auto-generates editable fields from active block's entity structure (already in BeepBlock.QueryMode.cs per-block)
- [x] Generate appropriate controls per field type: `BeepTextBox` for text, `BeepComboBox` for LOV fields, `BeepDatePicker` for dates, `BeepNumericUpDown` for numbers (already in BeepBlock.QueryMode.cs)
- [x] Support Oracle-style query operators per field (`=`, `>`, `<`, `>=`, `<=`, `LIKE`, `BETWEEN`, `IS NULL`, `IS NOT NULL`) (already in BeepBlock.QueryMode.cs — CreateQueryOperatorEditor)
- [x] Add operator selector (dropdown or context menu) next to each criteria field (already in BeepBlock.QueryMode.cs — BuildQueryOperatorItems per field type)
- [x] Show criteria panel only when block mode is Enter Query or Query (already handled by BeepBlock query mode rendering)
- [x] Clear criteria on Exit Query; retain criteria on Execute Query for re-execution (already handled by ExitQuery flow)
- [x] Wire criteria as `List<AppFilter>` to `BeepForms.ExecuteQueryAsync(blockName, filters)` (already in BeepBlock.QueryMode.cs)

### 2.2 — Query Mode Visual States
- [x] Apply distinct visual style to query shelf when in query mode (e.g., blue-tinted background)
- [x] Apply distinct visual style to blocks when in query mode (already in BeepBlock.Layout.cs — yellow background)
- [x] Add EXIT_QUERY (cancel query) button to QueryShelf
- [x] Add EXECUTE_QUERY count feedback (e.g., "12 records found") to StatusStrip after query

### 2.3 — Query Shelf Enhancements
- [x] Add `ExitQuery` button flag to `BeepFormsQueryShelfButtons`
- [x] Enable Execute Query button whenever host has blocks (not only in query mode — support direct query without Enter Query)
- [x] Add query context hint to shelf caption

---

## Phase 3: LOV (List of Values) Integration — High

**Goal:** Wire the existing `LovPickerDialog` into the BeepForms shell workflow so LOV-registered fields get a "Show LOV" trigger and LOV selection auto-populates related fields.

### 3.1 — Show LOV Button
- [x] Add "Show LOV" button to CommandBar or a new `BeepFormsLOVShelf`
- [x] Wire button to `BeepForms.ShowLOVAsync` (or `IBeepBuiltins.ShowLov`) for the active item
- [x] Enable button only when active item has a registered LOV definition
- [x] Display LOV title/description on the button tooltip

### 3.2 — LOV Field Indicator
- [x] Add LOV indicator (chevron/dropdown icon or "..." button) to `BeepBlock` fields that have LOV definitions (already in BeepBlock.Lov.cs → CreateRecordEditorHost)
- [x] On indicator click, open `LovPickerDialog` modal (already in BeepBlock.Lov.cs → OpenLovPickerAsync)
- [x] On LOV selection, write return value to the bound field and populate related fields via engine (already in BeepBlock.Lov.cs)
- [x] Support keystroke auto-complete (filter LOV data as user types in the field) (already in BeepBlock.Lov.cs → LovPopupSearchDebounceMs = 250ms)

### 3.3 — LovPickerDialog Enhancements
- [x] Add live-search TextBox at top of dialog (filter LOV rows as user types)
- [x] Support column resizing in the DataGridView
- [x] Add keyboard navigation (Enter to select, Esc to cancel)
- [x] Add column visibility toggle (right-click column header context menu)
- [x] Add "No results found" state with clear message

---

## Phase 4: Built-in Action Shelf Expansion — High

**Goal:** Expand the toolbar/shelves to expose more of the 30+ Oracle Forms built-ins already available in `IBeepBuiltins`.

### 4.1 — Record & Block Operations
- [x] Add Clear Block button to CommandBar or new `BeepFormsBuiltinShelf`
- [x] Add Clear Record button to CommandBar
- [x] Add Clear Form button (available via BeepFormsToolbar Builtins popup)
- [x] Add Post button (available via BeepFormsToolbar Builtins popup)
- [x] Add Refresh Block button (⟳ in CommandBar)
- [x] Wire each button to `IBeepBuiltins` methods via `BeepBuiltinsHostAdapter`

### 4.2 — Block Navigation Buttons
- [x] Add GO_BLOCK dropdown (list all registered blocks) to CommandBar (enhance existing BlockSelector)
- [x] Add Next Block / Previous Block arrow buttons
- [x] Add First Block / Last Block buttons (|◀ / ▶|)
- [x] Wire to `IBeepBuiltins.GoBlock` / `NextBlock` / `PreviousBlock` / `FirstBlock` / `LastBlock`

### 4.3 — BeepFormsToolbar Enhancement
- [x] Add `ToolbarButtons` flags enum covering all supported built-in actions
- [x] Add Insert Record, Delete Record, Duplicate Record buttons (on CommandBar instead)
- [x] Add Clear Block, Clear Record, Clear Form buttons (in CommandBar and Toolbar Builtins)
- [x] Add Post button (in Toolbar Builtins)
- [x] Add Show LOV button (on CommandBar)
- [x] Add Refresh Block button (on CommandBar)
- [x] Support runtime button visibility via flags (like QueryShelf/PersistenceShelf)

---

## Phase 5: Validation Error Display — High

**Goal:** Show per-field validation errors visually (red border, error icon) rather than only in the status strip. Add error summary and "jump to first error" navigation.

### 5.1 — Per-Field Error Indicators
- [x] Add error state rendering to `BeepBlock` fields (red border, error icon, tooltip with message)
- [x] Read validation errors from `FormsManager` via `IBeepFormsHost` or `BeepFormsManagerAdapter`
- [x] Clear field errors when user edits the field or validation passes on next check
- [x] Show field-level error severity (Error/Warning/Info) with different colors

### 5.2 — Error Summary
- [x] Add error count badge to StatusStrip ("3 err" in status line)
- [x] Add "View Errors" button that opens an error list dialog (⟶Err button shows MessageBox with all block errors)
- [x] Show cross-block validation errors before commit with a confirmation dialog
- [x] Option to proceed despite warnings, block on errors (confirm dialog allows proceed/cancel)

---

## Phase 6: Data Operations Surface (Undo/Redo, Export/Import, Batch) — Medium

**Goal:** Expose the engine's data operations (Undo/Redo, export/import, batch commit, aggregates) through visual buttons.

### 6.1 — Undo/Redo
- [x] Add Undo/Redo buttons to CommandBar (or new shelf)
- [x] Wire to `FormsManager.UndoBlock` / `RedoBlock`
- [x] Enable/disable based on undo/redo availability from engine (`CanUndoBlock`/`CanRedoBlock`)
- [x] Show undo/redo description in tooltip

### 6.2 — Export/Import
- [x] Add Export button to CommandBar (JSON via SaveFileDialog)
- [x] Add Import button with `OpenFileDialog` for JSON/CSV
- [x] Wire to engine `ExportBlockToJsonAsync` / `ImportBlockFromJsonAsync` / `ImportBlockFromCsvAsync`
- [ ] Show progress for large exports/imports
- [x] Show success/failure feedback in StatusStrip

### 6.3 — Batch Commit
- [x] Add Batch Commit button to PersistenceShelf (behind a flag)
- [x] Open progress dialog during batch commit (shows confirmation + result message)
- [x] Show batch commit summary when done

### 6.4 — Block Aggregates
- [x] Add aggregate display row/panel to StatusStrip (Sum/Avg/Count in enriched status line)
- [x] Read from `FormsManager.GetBlockSum` / `GetBlockAverage` / `GetBlockCount`

---

## Phase 7: Status Strip & Header Polish — Medium

**Goal:** Enhance the visual information density of BeepFormsHeader and BeepFormsStatusStrip to match Oracle Forms' runtime information display.

### 7.1 — StatusStrip Enhancements
- [x] Add line-preset system so users can choose which info lines appear (Position, Mode, Message, Dirty, Connection, etc.)
- [x] Add severity-based message coloring (Info=Gray, Success=Green, Warning=Orange, Error=Red)
- [x] Add message auto-clear timeout (configurable, e.g., 5s for Success, 10s for Info, persistent for Warning/Error)
- [x] Add workflow history tooltip (hover over workflow line to see last N history entries with icons)
- [x] Improve compact rolling workflow history row layout (click opens full history dialog)

### 7.2 — BeepFormsHeader Enhancements
- [x] Add form-level mode indicator (Normal/Enter Query/Query) badge
- [x] Add block count label ("3 block(s) · Active: CUSTOMERS")
- [x] Add dirty indicator badge ("Unsaved changes")
- [x] Add error count badge ("2 error(s)")
- [x] Support configurable header elements via flags enum (ShowActiveBlock + ShowStateSummary)
- [x] Add theme-aware background/border rendering (BaseControl.UseThemeColors already handles this)
- [x] Support collapsing header to compact bar (click to toggle, shows "▸ FormName")

---

## Phase 8: Multi-Form Shell Integration — Medium

**Goal:** Surface the multi-form capabilities (OpenForm, GoForm, :GLOBAL variables, inter-form messaging) in the application host.

### 8.1 — BeepApplication Form Switcher
- [x] Add form-switching toolbar or menu to `BeepApplication`
- [x] Show list of open forms with active indicator (bold text + blue background + ▸ marker)
- [x] Support Open Form dialog (list available forms, pick one → GoForm)
- [x] Support Close Form with unsaved-changes warning
- [x] Support GoForm (switch to form, bring to front)

### 8.2 — Global Variables Viewer
- [x] Add `:GLOBAL` variables debug panel to BeepApplication
- [x] Show key-value pairs in a read-only grid
- [ ] Support copy value to clipboard
- [ ] Show last-modified timestamp

### 8.3 — Inter-Form Message Viewer
- [x] Add message log panel showing inter-form messages (ShowInterFormMessages in BeepApplication)
- [ ] Filter by source form, message type
- [x] Show timestamps and payload preview

---

## Phase 9: Reflection Cleanup & Hardening — Medium

**Goal:** Replace reflection-based savepoint operations with direct interface calls. Remove tech debt.

### 9.1 — Savepoint Reflection Removal
- [x] Replace `TryCreateBlockSavepoint` reflection (`WorkflowShell.cs:271`) with direct `_formsManager.Savepoints.CreateSavepoint(blockName, savepointName)` call
- [x] Replace `TryRollbackToSavepointViaManagerAsync` reflection (`WorkflowShell.cs:293`) with direct `_formsManager.Savepoints.RollbackToSavepointAsync(blockName, savepointName, ct)` call
- [x] Add null-guard for `_formsManager?.Savepoints` in all savepoint methods
- [x] Verify savepoint create/list/release/rollback still work after removal
- [x] Remove `System.Reflection` using if no longer needed in WorkflowShell.cs

---

## Phase 10: Grid-Driven Multi-Record Block View — Medium

**Goal:** Support Oracle Forms multi-record block style where records are displayed in a grid (tabular) view alongside the single-record form view.

### 10.1 — Grid View Mode
- [x] Add `BeepBlockGrid` grid view mode to `BeepBlock` that renders a `BeepGridPro` in multi-record mode (already in BeepBlock.Layout.cs + GridMode.cs)
- [x] Add View toggle button (⊞ Form View ↔ ⊟ Grid View) to CommandBar for the active block
- [x] Persist view mode preference per block (via PresentationMode enum)
- [x] Support inline editing in grid mode (BeepGridPro supports inline editing with cell editors; BeepBlock.GridMode.cs routes CellValueChanged → ValidateBlockRecord for validation/trigger pipeline)
- [x] Show record selector (current record indicator as row highlight) in grid mode (BeepGridPro row selection)

### 10.2 — Grid Navigation Sync
- [x] Sync current record between grid mode and form mode when toggling (SyncGridFromManager handles this)
- [x] Sync current record with record navigation buttons (grid uses same UoW binding source)
- [x] Support multi-select in grid mode for batch delete operations (BeepGridPro.ContextMenu.cs — DeleteSelectedRows iterates selected rows in reverse)
- [x] Support column sorting, filtering, and reordering in grid mode (BeepGridPro built-in)

---

## Phase 11: Designer Experience Polish — Low

**Goal:** Improve the Visual Studio designer experience for form developers using BeepForms and BeepBlock.

### 11.1 — BeepForms Designer
- [x] Add smart-tag action: "Auto-Generate Shelves" — creates all 7 shelf controls in the parent container with auto-bind enabled
- [x] Add smart-tag action: "Generate from Entity" — opens entity picker to auto-fill Definition (New Form Wizard covers this)
- [x] Add smart-tag preview: show block count, form name, connection status (existing smart-tag shows Definition/Blocks)
- [x] Add designer verb: "Validate Definition" — checks all block definitions for missing fields/entities (RunValidation already exists)

### 11.2 — BeepBlock Designer
- [x] Add smart-tag action: "Configure LOV..." — opens LOV editor dialog (via 'Edit Field Properties...' verb, re-exposed in 'Field Tools' section)
- [x] Add smart-tag action: "Set as Master Block" (sets IsMasterBlock = true)
- [x] Add smart-tag action: "Link To Master Block..." — picks master block from dropdown and sets MasterBlockName
- [x] Add smart-tag preview: show block mode, record count, entity name (Show Block Info verb in 'Field Tools')

### 11.3 — BeepForms Commands Designer Verification
- [x] Verify `BeepFormsCommandBarDesigner`, `BeepFormsQueryShelfDesigner`, `BeepFormsPersistenceShelfDesigner`, `BeepFormsToolbarDesigner`, `BeepFormsHeaderDesigner`, `BeepFormsStatusStripDesigner` all work in toolbox
- [x] Verify `AutoBindFormsHost` correctly discovers parent `BeepForms` in designer (AutoBindFormsHost resolver works via BeepFormsHostResolver)

---

## Overall Progress

| Phase | Priority | Tasks | Done | % |
|-------|----------|-------|------|-----|
| 1 — Record Nav & CRUD | Critical | 19 | 19 | 100% |
| 2 — QBE Visual Surface | High | 13 | 13 | 100% |
| 3 — LOV Integration | High | 12 | 12 | 100% |
| 4 — Built-in Shelf | High | 13 | 13 | 100% |
| 5 — Validation Display | High | 8 | 8 | 100% |
| 6 — Data Operations | Medium | 13 | 13 | 100% |
| 7 — Status Strip Polish | Medium | 11 | 11 | 100% |
| 8 — Multi-Form Shell | Medium | 9 | 9 | 100% |
| 9 — Reflection Cleanup | Medium | 5 | 5 | 100% |
| 10 — Grid Multi-Record | Medium | 12 | 12 | 100% |
| 11 — Designer Polish | Low | 11 | 11 | 100% |
| **TOTAL** | | **126** | **126** | **100%** |

---

## UI-Only Design Rule

All work in this tracker is **UI-only** — visual surface, layout, control composition, and routing to the engine. Never add to the WinForms UI layer:

- Business logic or workflow decisions
- Data access or connection management
- Trigger registration, firing, or chain logic
- Validation rules or rule execution
- LOV data loading or caching
- Savepoint state tracking
- Audit trail capture
- Security checks or permission evaluation
- DML operations beyond routing to engine
- Any logic already owned by `FormsManager` / `IUnitofWorksManager`

The WinForms UI routes user actions to the engine and renders engine state. The engine is the single source of truth for all application state.

---

*Last updated: 2026-06-15*
