# BeepForms WinForms UI — Plans

This directory contains the enhancement roadmap and master todo tracker for the WinForms UI layer of the Beep Data Management framework's Oracle Forms emulation surface.

## Design Rule

The WinForms layer is **UI-only**. It routes user actions to the engine (`FormsManager` / `IUnitofWorksManager`) and renders engine state. No business logic, data access, trigger logic, or validation rules belong in the UI layer.

## Documents

| File | Description |
|------|-------------|
| [enhancement-plan.md](enhancement-plan.md) | High-level enhancement roadmap with architecture overview, gap analysis, and phase summary |
| [MASTER-TODO-TRACKER.md](MASTER-TODO-TRACKER.md) | Operational task tracker with 126 tasks across 11 phases |
| [phases/](phases/) | Detailed implementation guides for each phase |

## Phases

| Phase | Priority | Description | Tasks |
|-------|----------|-------------|-------|
| [01](phases/phase-01-record-navigation-crud-toolbar.md) | Critical | Record nav buttons, CRUD buttons, StatusStrip indicators | 19 |
| [02](phases/phase-02-qbe-visual-surface.md) | High | Query-by-example criteria panel, query mode visuals | 13 |
| [03](phases/phase-03-lov-integration.md) | High | Show LOV button, field indicators, dialog enhancements | 12 |
| [04](phases/phase-04-builtin-action-shelf-expansion.md) | High | Clear/Post/Refresh buttons, block nav arrows | 13 |
| [05](phases/phase-05-validation-error-display.md) | High | Per-field error indicators, error summary | 8 |
| [06](phases/phase-06-data-operations-surface.md) | Medium | Undo/Redo, export/import, batch commit, aggregates | 13 |
| [07](phases/phase-07-status-strip-header-polish.md) | Medium | StatusStrip line presets, coloring, header badges | 11 |
| [08](phases/phase-08-multi-form-shell-integration.md) | Medium | Form switcher, globals viewer, message log | 9 |
| [09](phases/phase-09-reflection-cleanup-hardening.md) | Medium | Replace 2 reflection calls with direct interface calls | 5 |
| [10](phases/phase-10-grid-multi-record-block-view.md) | Medium | Grid view mode, Form/Grid toggle, inline editing | 12 |
| [11](phases/phase-11-designer-experience-polish.md) | Low | Designer smart-tags, auto-generate shelves, validation | 11 |

## Related Documents (Engine Side)

- `BeepDM/DataManagementEngineStandard/Editor/Forms/.plans/enhancement-plan.md` — Engine enhancement plan (Phases 1-9 complete)
- `BeepDM/DataManagementEngineStandard/Editor/Forms/.plans/todo-tracker.md` — Engine todo tracker (190/190 tasks, all complete)
- `BeepDM/DataManagementEngineStandard/Editor/Forms/ORACLE-FORMS-MAPPING.md` — Oracle Forms concept mapping
- `BeepDM/DataManagementEngineStandard/Editor/Forms/gaps.md` — Engine CRUD gaps analysis
- `BeepDM/DataManagementEngineStandard/Editor/Forms/enhancements.md` — Engine enhancement opportunities

---

*Last updated: 2026-06-15*
