# Integrated Controls Documentation

See [Help/index.html](../Help/integrated-controls.html) for full browser documentation.

## Components

| Component | Purpose |
|---|---|
| **BeepForms** | Application shell: toolbar, command bar, query shelf, header, persistence shelf, status strip |
| **BeepBlock** | Data-bound content panel: Record mode (single CRUD), Grid mode (data grid), Query mode |
| **BeepAppTree** | Branch-driven application navigation tree from `BranchesClasses` |
| **BeepDataConnection** | Design-time tray component for connection management |
| **NuggetsManage** | NuGet package management UI |
| **DynamicMenuManager** | Dynamic context menu generation from `AssemblyClassDefinition` metadata |
| **DynamicFunctionCallingManager** | Function/method execution from tree nodes |

## Recent Changes

### SimpleItem Cleanup & Serialization Fix
- `SimpleItem` serialization in WinForms designer is now fixed
- Removed unused `INotifyPropertyChanged`, `ParentValueChanged` event chain, LOV subsystem
- Color properties (`BadgeBackColor`, `BadgeForeColor`) replaced with portable `BeepColor` (implicit `Color` conversion)
- Added `[DesignerSerializationVisibility(Hidden)]` on non-serializable properties

### Global ImagePath Editor
- `GlobalImagePathEditorRegistration` in Design.Server auto-applies `BeepImagePathEditor` to any `ImagePath` string property
- Covers `SimpleItem` and all `BaseControl` subclasses
- Activated via `[ModuleInitializer]` — zero code changes in controls
