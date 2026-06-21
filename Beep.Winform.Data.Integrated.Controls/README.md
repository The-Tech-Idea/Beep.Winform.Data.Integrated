# WinForms Integrated UI — Oracle Forms Runtime Layer

> **Root:** `Beep.Winform.Data.Integrated\Beep.Winform.Data.Integrated.Controls\`
> **Scope:** `Blocks\` + `Forms\` + `DataConnection\`
> **Date:** 2026-06-18

All business logic lives in the Forms Engine (`BeepDM\DataManagementEngineStandard\Editor\forms\`).
This WinForms layer is a pure UI shell — field controls, layout, drag-drop, keyboard shortcuts, 
WinForms-specific rendering. Every navigation, CRUD, query, and validation operation delegates
to the engine via `IBeepFormsHost` → `IUnitofWorksManager`.

---

## File Inventory

### Blocks/ (38 files)

| Subdir | Files | Purpose |
|---|---|---|
| `BeepBlock/` | 14 partials | Main block control — grid/form, navigation, query, operations, LOV, validation, context menus |
| `Contracts/` | 2 | `IBeepBlockView`, `IBeepFieldPresenter` |
| `Models/` | 7 | Block definitions, visual attributes, view state, field definitions |
| `Navigation/` | 1 | `BeepBlockNavigationBar` |
| `Services/` | 5 | Presenter registry, calculator, control type policy/registry, format mask, master-detail coordinator |
| `Services/Presenters/` | 6 | Text, Combo, Date, Numeric, Checkbox, ReflectiveControl field presenters |

### Forms/ (50 files)

| Subdir | Files | Purpose |
|---|---|---|
| `BeepForms/` | 14 partials | Main form control — IBeepFormsHost, block proxy, commands, navigation, events, trigger proxy, master-detail, messages, workflow shell, drag-drop, keyboard shortcuts, layout, definition |
| `BeepFormsCommandBar/` | 1 | Command bar UI component |
| `BeepFormsHeader/` | 1 | Form header |
| `BeepFormsPersistenceShelf/` | 1 | Persistence state shelf |
| `BeepFormsQueryShelf/` | 1 | Query shelf |
| `BeepFormsRecordNavigationShelf/` | 1 | Record navigation shelf |
| `BeepFormsStatusStrip/` | 1 | Status strip |
| `BeepFormsToolbar/` | 2 | Toolbar + actions |
| `Contracts/` | 3 | `IBeepFormsHost`, `IBeepFormsBootstrapper`, `IBeepFormsNotificationService` |
| `Helpers/` | 4 | Dialog service, dialog surface, display text resolver, host resolver |
| `Logon/` | 5 | Logon dialog, types, simple login form |
| `Lov/` | 1 | `LovPickerDialog` |
| `Models/` | 8 | Command bar, keyboard shortcuts, toolbars, definitions, query shelf models |
| `Services/` | 3 | BeepFormsManagerAdapter, BeepFormsMessageService, BeepFormsKeyboardShortcutProvider |

### DataConnection/ (4 files)

| File | Purpose |
|---|---|
| `BeepDataConnection.cs` | Connection management WinForms component |
| `ConnectionPackageManager.cs` | Connection package management |
| `ConnectionState.cs` | Connection state tracking |
| `EnvironmentConnectionManager.cs` | Environment-specific connection manager |

---

## Engine Delegation

Every `BeepBlock` operation flows through the engine:
```
BeepBlock → IBeepFormsHost._formsHost.MethodAsync() → BlockProxy → _formsManager.Method() → IDataSource
```

BlockProxy (431 lines) exposes 40+ proxy methods — all delegate to `_formsManager.*`. 
BeepBlock never accesses `FormsManager` directly.

### Severity Classification (engine-only)

All message/error/alert severity mapping is centralized in the engine's 
`MessageClassifier` class. No UI file contains local severity mapping methods:
- `MessageClassifier.MapMessageLevel(MessageLevel)` → `BeepMessageSeverity`
- `MessageClassifier.MapErrorSeverity(ErrorSeverity)` → `BeepMessageSeverity`
- `MessageClassifier.ClassifyFromMessageType(string)` → `BeepMessageSeverity`
- `MessageClassifier.ClassifyCommandResult(IErrorsInfo)` → `BeepMessageSeverity`
- `MessageClassifier.MapAlertSeverity(AlertStyle, AlertResult)` → `BeepMessageSeverity`
- `MessageClassifier.ClassifyFromText(string, default)` → `BeepMessageSeverity`
- `MessageClassifier.IsNeutralStatus(string)` → `bool`

### Field Type Mapping (engine-only)

`FieldTypeMapper.GetCanonicalFieldType(EntityField)` maps database types to UI presenter types.
The WinForms `BeepBlockPresenterRegistry` uses this for field → control creation.

---

## Related Docs

- Engine: `BeepDM\DataManagementEngineStandard\Editor\forms\` — Forms engine core
- IDE: `Beep.Desktop\TheTechIdea.Beep.Desktop.IDE.Extensions\` — VS IDE navigator
- Engine contract: `ENGINE-CONTRACT.md` in IDE directory
- WPF layer: `Beep.WPF\TheTechIdea.Beep.Wpf.Data.Integrated\` — WPF equivalent
