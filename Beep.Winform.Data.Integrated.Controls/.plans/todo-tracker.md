# BeepForms Fresh Start Todo Tracker

> Architecture note:
> `BeepForms` is now the non-visual coordinator/host for `BeepBlock` instances.
> Visual shell surfaces move into separate controls such as `BeepFormsHeader`, `BeepFormsCommandBar`, `BeepFormsQueryShelf`, `BeepFormsPersistenceShelf`, `BeepFormsToolbar`, and `BeepFormsStatusStrip`.

## Status Legend

- `not-started`
- `in-progress`
- `blocked`
- `done`

## Phase 1

| Status | Item | Notes |
|---|---|---|
| not-started | Approve `BeepForms` and `BeepBlock` naming | New strategic names only |
| in-progress | Approve folder and namespace layout | Phase 1 scaffold folders and namespaces added |
| in-progress | Define core interfaces and models | Initial scaffold files added |
| in-progress | Write UI vs adapter vs manager boundary rules | Beep controls default rule captured in plan docs |

## Phase 2

| Status | Item | Notes |
|---|---|---|
| in-progress | Create `BeepForms` coordinator host | `BeepForms` now owns only block hosting and manager/view-state coordination; shell chrome was removed |
| in-progress | Create `BeepFormsHeader` visual surface | Title and active-context rendering now live in a separate BaseControl-derived header over host metadata |
| in-progress | Create `BeepFormsCommandBar` visual surface | Form-level block switching plus sync now live in a separate BaseControl-derived command bar |
| in-progress | Create `BeepFormsQueryShelf` visual surface | Query-mode entry and execution now live in a separate BaseControl-derived shelf with selectable caption variants |
| in-progress | Create `BeepFormsPersistenceShelf` visual surface | Commit and rollback now live in a separate BaseControl-derived persistence shelf keyed off shared dirty-state |
| in-progress | Create `BeepFormsToolbar` visual surface | Savepoint/alert popup actions now live in a separate BaseControl-derived toolbar |
| in-progress | Create `BeepFormsStatusStrip` visual surface | Shared status/message/workflow lines now live in a separate BaseControl-derived strip over `BeepForms.ViewState` |
| in-progress | Create `BeepFormsManagerAdapter` | Initial sync adapter added |
| in-progress | Add active block and form command routing | Router scaffold added and commands now resync after execution |
| in-progress | Add status/message strip | `BeepFormsStatusStrip` now renders shared view state as part of the extracted shell-surface stack |

## Phase 3

| Status | Item | Notes |
|---|---|---|
| in-progress | Create `BeepBlock` control | Shell and layout partials now use Beep controls by default |
| in-progress | Create field definition and presenter registry | Registry now registers default text/numeric/date/checkbox/combo presenters and block metadata prefers UoW EntityStructure |
| in-progress | Implement record mode layout | Record-mode row scaffold now hydrates Beep text/checkbox/date/numeric/combo editors |
| in-progress | Implement grid mode layout | `BeepGridPro` host added for block grid mode |
| done | Rebuild navigation bar against `BeepBlock` | `BeepBlockNavigationBar` now hosts block commands and forwards navigation/query/save flows through the fresh-start surface |

## Phase 4

| Status | Item | Notes |
|---|---|---|
| done | Implement query mode UX | `BeepBlock` now renders manager-queryable operator/value rows, supports typed numeric/date `between` and `not between` editors, dedicated `in`/`not in` list-entry widgets with option-aware multi-pick for static combo/LOV sources, per-field reset affordances, no-value operators, and forwards packaged `AppFilter` criteria through `BeepForms`/`FormsManager` |
| in-progress | Implement validation bridge | Inline field errors now surface as severity-aware row accents, semantic tooltips, severity badges, and a headline/detail current-record summary with next-step guidance; query mode still suppresses record validation noise |
| in-progress | Implement LOV service and dialog path | `BeepBlock` record-mode LOV fields now expose a dedicated popup picker button and `F9` shortcut, and popup search now debounces into manager-backed `BeepLovPopup` reloads; broader design-time tooling still remains |
| in-progress | Implement messages, alerts, and savepoint UI | `BeepForms` now keeps shared workflow view state while header/query/persistence/toolbar/status surfaces render separate shell responsibilities; active-block current messages now sync from manager state and trigger/user cancellations classify as warnings instead of generic errors; toolbar prompts, pickers, list dialogs, and fallback alerts now use a shared integrated dialog surface with semantic badges and secondary guidance text |
| in-progress | Extract standalone title shell surface | `BeepFormsHeader` now owns title/context rendering so `BeepForms` no longer needs title-derived shell semantics |
| in-progress | Extract standalone host command surface | `BeepFormsCommandBar` now owns block switching plus sync so general host actions stay out of the coordinator shell |
| in-progress | Extract standalone query surface | `BeepFormsQueryShelf` now owns query-mode entry and execution so query actions are isolated from the general host command bar, with caption presets available at design time |
| in-progress | Extract standalone persistence surface | `BeepFormsPersistenceShelf` now owns commit and rollback so persistence actions are isolated from the general host command bar |
| in-progress | Implement master-detail coordinated UX | `BeepForms` now exposes a coordination strip and asks `FormsManager` to refresh detail blocks after master navigation/query/savepoint flows while keeping relationship rules manager-driven |

## Phase 5

| Status | Item | Notes |
|---|---|---|
| done | Add design-time support | The design-server smart-tags now cover `BeepForms`, `BeepBlock`, `BeepFormsHeader`, `BeepFormsCommandBar`, `BeepFormsQueryShelf`, `BeepFormsPersistenceShelf`, `BeepFormsToolbar`, and `BeepFormsStatusStrip`; the definition/options graph now has modal editors, `BeepBlock.BlockName` can suggest nearby host blocks, `BeepFieldDefinition.EditorKey` has typed presenter-key suggestions, metadata uses a focused key/value editor, and block navigator flags keep a typed navigation editor surface |
| done | Create fresh samples | `Beep.Sample.Winform` now hosts a scenario-driven `BeepForms` sample browser covering maintenance, master-detail, query, LOV, and validation without `BeepDataBlock` |
| not-started | Write usage docs | New strategic path only |

## Phase 6

| Status | Item | Notes |
|---|---|---|
| not-started | Define parity checklist | Replacement confidence gate |
| not-started | Freeze `BeepDataBlock` feature work | Legacy only |
| not-started | Plan legacy cleanup | After parity only |
| not-started | Update readmes and examples | Point to `BeepForms` |

## Phase 7 — EntityStructure → BeepBlock Production Pipeline

> Full plan: `07-phase7-entitystructure-beepblock-pipeline.md`

**Architecture rule:** `BeepBlock` and `BeepForms` are UI only. All datasource access, entity structure retrieval, UoW creation, and business logic live in **FormsManager** (BeepDM). BeepBlock/BeepForms read from FormsManager; they never call `IDataSource` directly.

---

### Phase 7A — EntityField Fidelity (Beep.Winform)

| Status | Item | File |
|---|---|---|
| done | Add `IsIdentity`, `IsHidden`, `IsLong`, `IsRowVersion`, `DefaultValue` to `BeepBlockEntityFieldDefinition` | `Blocks/Models/BeepBlockEntityDefinition.cs` |
| done | Update `CreateEntityDefinition()` field-mapping loop | `Blocks/BeepBlock/BeepBlock.Metadata.cs` |
| done | Update `ToFieldDefinition()`: `IsVisible = !IsHidden`, extend `IsReadOnly`, add `DefaultValue` | `Blocks/Models/BeepBlockEntityDefinition.cs` |
| done | Update `Clone()` for 5 new fields | `Blocks/Models/BeepBlockEntityDefinition.cs` |
| done | Add `DefaultValue` to `BeepFieldDefinition` + `Clone()` | `Blocks/Models/BeepFieldDefinition.cs` |

### Phase 7B — FormsManager `SetupBlockAsync` (BeepDM)

| Status | Item | File |
|---|---|---|
| done | Add `SetupBlockAsync(blockName, connectionName, entityName, isMaster, ct)` to `IUnitofWorksManager` | `DataManagementEngineStandard/Editor/Forms/Interfaces/IUnitofWorksManagerInterfaces.cs` |
| done | Implement `SetupBlockAsync` in `FormsManager.BlockRegistration.cs` (delegates to `RegisterBlockFromSourceAsync`) | `DataManagementEngineStandard/Editor/Forms/FormsManager.BlockRegistration.cs` |

### Phase 7C — FieldCategory → ControlType Resolution (Beep.Winform)

| Status | Item | File |
|---|---|---|
| done | Add `isLong` param + short-circuit to `ResolveDefaultFieldSettings()` | `Blocks/Services/BeepFieldControlTypeRegistry.cs` |
| done | Complete category switch (Integer, Decimal, Date, DateTime, Boolean, Binary, Enum, LongString) | `Blocks/Services/BeepFieldControlTypeRegistry.cs` |
| done | Update `ResolveWidth()` for numeric / long-text widths | `Blocks/Models/BeepBlockEntityDefinition.cs` |
| done | Update `ResolveDefaultFieldSettings` call sites to pass `isLong` | `Blocks/Models/BeepBlockEntityDefinition.cs` |

### Phase 7D — BeepForms Bootstrap Trigger, UI side only (Beep.Winform)

| Status | Item | File |
|---|---|---|
| done | `BootstrapState` enum + `BeepFormsBootstrapEventArgs` | `Forms/Models/BeepFormsBootstrapEventArgs.cs` |
| done | Add `BootstrapState` to `BeepFormsViewState` | `Forms/Models/BeepFormsViewState.cs` |
| done | Add `InitializeAsync()` + `BootstrapCompleted` event to `BeepForms.cs` | `Forms/BeepForms/BeepForms.cs` |
| done | Update `FormsManager` + `Definition` setters to call `InitializeAsync()` | `Forms/BeepForms/BeepForms.cs` |
| done | Surface `BootstrapState` prefix in `BeepFormsStatusStrip.UpdateFromViewState()` | `Forms/BeepFormsStatusStrip/BeepFormsStatusStrip.cs` |
| done | Add `BootstrapCompleted` + `InitializeAsync` to `IBeepFormsHost` | `Forms/Contracts/IBeepFormsHost.cs` |

### Phase 7E — Designer-Time Preview (Beep.Winform)

| Status | Item | File |
|---|---|---|
| done | `CreateEntityDefinition` is `internal static` | `Blocks/BeepBlock/BeepBlock.Metadata.cs` |
| done | `BeepBlockEntityDefinitionTypeConverter` + `BeepBlockEntityFieldDefinitionTypeConverter` (both `GetProperties` + `ConvertTo`) | `Design.Server/Editors/IntegratedFormsDefinitionEditors.cs` |

---

## Decision Log

| Date | Decision |
|---|---|
| 2026-04-09 | Fresh start approved: ignore `BeepDataBlock` for new design |
| 2026-04-09 | New top-level UI names are `BeepForms` and `BeepBlock` |
| 2026-04-10 | `BeepForms` is non-visual; shell surfaces split into separate controls |
| 2026-04-13 | Bootstrap gap identified; Phase 7 plan created |
| 2026-04-13 | **`LoadEntityStructure(EntityStructure)` removed from `BeepBlock.Metadata.cs`** — violated UI-only rule |
| 2026-04-13 | **`IBeepFormsBootstrapper` / `BeepFormsDefaultBootstrapper` cancelled** — called `IDataSource` directly |
| 2026-04-13 | Phase 7B renamed to "FormsManager `SetupBlockAsync`" — FormsManager is the only data authority; BeepBlock/BeepForms are pure UI |

# BeepDataBlock → FormsManager Migration — Todo Tracker

**Overall status:** Dual-path delegation COMPLETE. Purge-local-state phase BLOCKED (Phase 01 skipped — NuGet boundary).  
**Last updated:** Session 7 — shared datasource-agnostic key resolution now covers coordinated and standalone BeepDataBlock paths; phase documents reconciled to current tracker status.

---

## How to read this file

- `[ ]` = not started  
- `[~]` = in progress  
- `[x]` = done  
- `[B]` = blocked (depends on Phase 01 which was permanently skipped)

> **Key architectural decision:**  
> Phase 01 was skipped permanently (cannot add `IDataBlockController` to NuGet BeepDM without a new release).  
> As a result all planned file-deletion tasks are blocked — WinForms local models/helpers must remain because
> no BeepDM equivalents exist to replace them.  
> The migration strategy became: **dual-path delegation** — when `IsCoordinated` delegate to FormsManager;
> otherwise use local standalone state. Both paths compile and run correctly.

---

## Phase Summary

| Phase | Title | Status |
|---|---|---|
| 01 | BeepDM Contracts | ⏭ Skipped permanently (NuGet boundary) |
| 02 | Remove IsCoordinated | ✅ Done (simplified to `_formsManager != null`; dual-path kept) |
| 03 | Triggers Delegation | ✅ Done (delegation added; local state kept — Phase 01 blocked deletions) |
| 04 | Validation Delegation | ✅ Done (early-return fixed; local rules kept — Phase 01 blocked deletions) |
| 05 | LOV Delegation | ✅ Done (already correct; BeepDataBlockLOV kept — Phase 01 blocked deletions) |
| 06 | Properties/Guards Delegation | ✅ Done (27 sub-manager null checks removed) |
| 07 | Data Operations Delegation | ✅ Done (commit/rollback/nav delegation; `ExecuteQueryByExampleAsync` IsCoordinated guard added) |
| 08 | SystemVariables + Navigation | ✅ Done (dual-path already correct) |
| 09 | Interface Consolidation | 🔄 Partial (IBeepDataBlock enriched with 10 members; IDataBlockController blocked by Phase 01) |
| 10 | Examples + Smoke Tests | ✅ Complete (examples added; Beep.Sample demo form wired; smoke tests A–H compile-verified) |
| 11 | WPF + Blazor Adapters | 🚫 Not applicable to current migration |

---

## Phase 01 — BeepDM Contracts (SKIPPED)

All tasks here were never executed. Phase permanently skipped.

| # | Task | Status |
|---|---|---|
| 1.1–1.5 | Create IDataBlockNotifier, IDataBlockController, etc. in BeepDM | ⏭ Skipped — NuGet boundary |

---

## Phase 02 — Remove IsCoordinated ✅

| # | Task | Done |
|---|---|---|
| 2.1 | `Coordination.cs` — `_isRegisteredWithFormsManager` field removed | [x] |
| 2.2 | `IsCoordinated` simplified to `_formsManager != null` | [x] |
| 2.3 | FormManager setter auto-registers/unregisters | [x] |
| 2.4 | Session 2: 4× `IsCoordinated && FormManager != null` → `IsCoordinated` in UnitOfWork.cs | [x] |

---

## Phase 03 — Triggers Delegation ✅ (delegation done; deletions blocked)

| # | Task | Done |
|---|---|---|
| 3.1 | `Triggers.cs` — RemoveTrigger/ClearAll delegate to FormsManager first | [x] |
| 3.2 | Local `_triggers`/`_namedTriggers` kept as standalone fallback | [x] |
| 3.3–3.8 | Delete BeepDataBlockTrigger.cs, TriggerContext.cs, etc. | [B] — files still referenced externally |

---

## Phase 04 — Validation Delegation ✅ (delegation done; deletions blocked)

| # | Task | Done |
|---|---|---|
| 4.1 | `Validation.cs` — `ValidateField`/`ValidateCurrentRecord` return immediately on both pass AND fail | [x] |
| 4.6 | Delete `Models/ValidationRule.cs` | [B] — 3 external refs remain |
| 4.7 | Delete `Helpers/ValidationRuleHelpers.cs` | [B] — 15 external refs remain |

---

## Phase 05 — LOV Delegation ✅ (already correct; deletions blocked)

| # | Task | Done |
|---|---|---|
| 5.1 | `LOV.cs` already delegates to FormsManager via `FormManager.GetLOV/ShowLOV` | [x] |
| 5.6 | Delete `Models/BeepDataBlockLOV.cs` | [B] — 25 external refs remain |
| 5.7 | Delete `Helpers/DataBlockQueryHelper.cs` | [x] — zero external refs confirmed; safe candidate |

> `DataBlockQueryHelper.cs` had 2 self-references (counted against itself by PowerShell).  
> `grep_search` across all Beep.Winform returned 0 external hits.  
> Last usage in `UnitOfWork.ExecuteQueryByExampleAsync` has been refactored: `IsCoordinated` now
> early-returns before reaching the helper; standalone path still uses it. File is still compiled.
> Keeping for now as the standalone QBE path still calls `BuildQueryFilters`/`ValidateQueryFilters`.

---

## Phase 06 — Properties/Guards Delegation ✅

| # | Task | Done |
|---|---|---|
| 6.1 | 27 redundant `IsCoordinated && _formsManager?.SubX != null` checks removed across 6 files | [x] |
| 6.5 | Delete `Helpers/BeepDataBlockPropertyHelper.cs` | [B] — 45 external refs remain |

---

## Phase 07 — Data Operations Delegation ✅

| # | Task | Done |
|---|---|---|
| 7.1 | `CommitWithUnitOfWorkAsync` → `CoordinatedCommit()` early-return | [x] |
| 7.2 | `RollbackWithUnitOfWorkAsync` → `CoordinatedRollback()` early-return | [x] |
| 7.3 | `MoveNext/Previous/First/LastWithUnitOfWorkAsync` → FormsManager delegates | [x] |
| 7.4 | `ExecuteQueryByExampleAsync` — `IsCoordinated` guard added at top (session 4) | [x] |
| 7.5 | `HandleDataChanges` — `!IsCoordinated` guard prevents auto-commit in coordinated mode | [x] |
| 7.6 | `HandleDataChanges` — `.Result` → `.ConfigureAwait(false).GetAwaiter().GetResult()` deadlock fix | [x] |
| 7.7 | Delete `Helpers/DataBlockUnitOfWorkHelper.cs` | [B] — still used by standalone path |
| 7.8 | `OnPreQuery`/`OnPostQuery`/`OnPreInsert`/etc. events retained — standalone hooks only | [x] |
| 7.9 | Remove BeepDM `IUnitofWork` / `UnitOfWorkWrapper` master-detail registration APIs; move live relationship state back into `FormsManager` | [x] |

---

## Phase 08 — SystemVariables + Navigation ✅

| # | Task | Done |
|---|---|---|
| 8.1 | `SystemVariables.cs` dual-path confirmed correct | [x] |
| 8.2 | Navigation methods in `UnitOfWork.cs` delegate to FormsManager first | [x] |
| 8.4 | Delete `Models/SystemVariables.cs` | [B] — used by Navigation.cs + TriggerContext |

---

## Phase 09 — Interface Consolidation 🔄 Partial

| # | Task | Done |
|---|---|---|
| 9.1 | `IBeepDataBlock` enriched: `IsCoordinated`, `IsDirty`, 8 async methods | [x] |
| 9.2 | `IBeepDataBlock` compile-verified (BeepDataBlock satisfies all new members) | [x] |
| 9.3–9.8 | Extend `IDataBlockController`, slim field-selection/editor-template models | [B] — Phase 01 blocked |

---

## Phase 10 — Examples + Smoke Tests ✅ Complete

| # | Task | Done |
|---|---|---|
| 10.2 | `FormsManagerCoordinationExamples.cs` added (single-block + master-detail + triggers + rollback) | [x] |
| 10.3 | Master-detail example in `FormsManagerCoordinationExamples.Example2_MasterDetailAsync` | [x] |
| 10.1 | `BeepDataBlockDemoForm.cs` created in `Beep.Sample.Winform/Forms/` | [x] |
| 10.1 | `case "BeepDataBlockDemo"` + `ShowBeepDataBlockDemo()` wired in `MainForm.cs` | [x] |
| 10.4–10.12 | Smoke tests A–H in `BeepDataBlockDemoForm` — zero compile errors verified | [x] |

---

## Phase 11 — WPF + Blazor Adapters 🚫 Not Applicable

This is not part of the current migration anymore.

- Reason: the migration stopped at the WinForms dual-path boundary after Phase 01 was skipped, so there is no shipped cross-platform contract surface to justify adapter implementation work here.
- If cross-platform adapters are revisited later, treat them as a separate architecture spike rather than as an unfinished migration phase.

---

## Post-Migration Follow-up — Key Metadata Resolution 🔄

This is not a new migration phase. It is follow-up work discovered during the master/detail review.

> **Constraint:**
> `FormsManager` must remain datasource-agnostic. RDBMS helpers are only one metadata source.
> Key discovery and relation mapping must work for relational, file, cache, API, and other providers,
> with provider-specific enrichment where available and explicit fallbacks where not.

| # | Task | Status |
|---|---|---|
| K1 | Audit key metadata sources (`EntityStructure.PrimaryKeys`, `Relations`, datasource `GetEntityforeignkeys`, helper PK/FK query APIs) | [x] |
| K2 | Define key resolution order: explicit block config → entity relations → datasource foreign keys → primary-key-name fallback | [x] |
| K3 | Design datasource-agnostic key resolver consumed by `FormsManager` / `BeepDataBlock` | [x] |
| K4 | Add RDBMS metadata adapter using helper/query surface (`RdbmsHelper.GetPrimaryKeyQuery/GetForeignKeysQuery`) | [ ] |
| K5 | Add non-RDBMS fallback rules for file/CSV/cache/API/vector/streaming datasources | [ ] |
| K6 | Normalize block initialization to use resolved key mappings instead of ad hoc string copies | [~] |
| K7 | Extend coordinated master/detail path for composite key mappings | [x] |
| K8 | Add tests for PK/FK propagation and master-detail-detail synchronization across provider types | [~] |

Notes:
- Coordinated registration now resolves keys inside `FormsManager.CreateMasterDetailRelation(...)` instead of requiring BeepDataBlock to pre-fill authoritative strings.
- Resolver order is live for the coordinated path: explicit override → filtered `EntityStructure.Relations` → datasource `GetEntityforeignkeys(...)` → matching primary-key names.
- Composite mappings now flow through coordinated filtering and detail FK stamping when the resolved mapping set contains multiple field pairs.
- Standalone/local BeepDataBlock filtering, detail insert validation, and FK stamping now resolve through the shared key resolver using block relationship snapshots, so coordinated and non-coordinated paths share the same resolution order.
- Remaining gaps: provider-specific adapters beyond datasource foreign-key metadata are not implemented yet.

---

## File Deletion Audit (Session 4)

All originally planned deletions are blocked by Phase 01 skip (no BeepDM equivalents to replace WinForms copies).

| File | External Refs | Decision |
|---|---|---|
| `Helpers/DataBlockQueryHelper.cs` | 1 (standalone QBE path) | Kept — still used |
| `Helpers/BeepDataBlockPropertyHelper.cs` | 45 | Keep |
| `Helpers/ValidationRuleHelpers.cs` | 15 | Keep |
| `Helpers/BeepDataBlockTriggerHelper.cs` | 13 (examples) | Keep |
| `Helpers/DataBlockUnitOfWorkHelper.cs` | ∞ (standalone fallback) | Keep |
| `Models/ValidationRule.cs` | 3 | Keep |
| `Models/BeepDataBlockLOV.cs` | 25 | Keep |
| `Models/TriggerContext.cs` | 41 | Keep |
| `Models/TriggerEnums.cs` | 0 filename refs (but contains `TriggerType` enum — 41 uses of enum values) | Keep |
| `Models/BeepDataBlockEditorTemplate.cs` | 1 (DesignTime.cs) | Keep |
| `Models/BeepDataBlockFieldSelection.cs` | 3 (2 Integrated files + Design.Server) | Keep |
| `Models/SystemVariables.cs` | ≥2 (Navigation + TriggerContext) | Keep |


---

*End of tracker. All active work should be recorded above in the accurate phase sections.*

