# Beep Winform Data Integrated

## Components

| Component | Location | Purpose |
|---|---|---|
| **BeepForms** | `Controls/Forms/BeepForms/` | Application shell with toolbar, command bar, query shelf, header, persistence, status strip |
| **BeepBlock** | `Controls/Blocks/BeepBlock/` | Data-bound content panel: Record/Grid/Query presentation modes |
| **BeepAppTree** | `Controls/ITrees/BeepTreeView/` | Branch-driven application navigation tree |
| **BeepDataConnection** | `Controls/DataConnection/` | Design-time component for connection management |
| **NuggetsManage** | `Controls/NuggetsManage/` | NuGet package management UI |
| **DynamicMenuManager** | `Controls/Helpers/` | Dynamic context menu generation from metadata |

## Documentation

- [Browser Help](./Help/integrated-controls.html) — Full HTML documentation
- [Block Definition Model](./Help/integrated-controls.html#block-definition) — BeepBlockDefinition, BeepBlockEntityDefinition, BeepFieldDefinition

## Architecture

```
BeepForms (application shell)
  ├── BeepFormsToolbar          ← Save, Refresh, Actions
  ├── BeepFormsCommandBar       ← Undo/Redo, Cancel
  ├── BeepFormsQueryShelf       ← Filter/search query builder
  ├── BeepFormsHeader           ← Title, breadcrumb
  ├── BeepFormsPersistenceShelf ← Savepoint management
  ├── BeepFormsStatusStrip      ← Status messages, progress
  └── BeepBlock (content panel)
        ├── RecordMode            ← Single-record CRUD form
        ├── GridMode              ← Multi-row data grid
        └── QueryMode             ← Query builder + results
```

## Recent Changes

### SimpleItem Cleanup & Serialization Fix (2026)
- Removed unused `INotifyPropertyChanged`, `ParentValueChanged` event chain, LOV subsystem
- `Color BadgeBackColor` → `BeepColor BadgeBackColor` (portable, implicit `Color` conversion)
- Added `[DesignerSerializationVisibility(Hidden)]` on non-serializable properties (Value, Item, ParentItem, Data, Children)
- SimpleItem now serializes correctly in WinForms designer

### Global ImagePath Editor (2026)
- `GlobalImagePathEditorRegistration` auto-applies `BeepImagePathEditor` to any `ImagePath` string property
- Covers `SimpleItem` and all `BaseControl` subclasses
- Activated via `[ModuleInitializer]` — zero code changes in controls
