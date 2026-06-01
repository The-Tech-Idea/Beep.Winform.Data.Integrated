# Beep Winform Data Integrated

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/nuget/v/TheTechIdea.Beep.Winform.Controls.Integrated.svg)](https://www.nuget.org/packages/TheTechIdea.Beep.Winform.Controls.Integrated)

Data-bound application framework for the BeepDM ecosystem — provides zero-code CRUD forms, branch-driven navigation, connection management, and NuGet package discovery at design time.

## Components

### Core

| Component | Description |
|---|---|
| **BeepForms** | Application shell with toolbar, command bar, query shelf, header, persistence shelf, and status strip. Renders BeepBlock as content. |
| **BeepBlock** | Data-bound content panel. Three presentation modes: **Record** (single-record form), **Grid** (multi-row data grid), **Query** (query builder + results). |
| **BeepAppTree** | Branch-driven application navigation tree. Auto-builds from `BranchesClasses` discovered by AssemblyHandler's `ILoaderExtention` system. |

### Design-Time

| Component | Description |
|---|---|
| **BeepDataConnection** | Tray component for design-time connection management. Auto-populates from BeepService configuration. Supports Project/User/Machine storage scopes. |
| **NuggetsManage** | Integrated NuGet package manager UI — search, install, uninstall, and manage packages from within the designer. |

### Infrastructure

| Component | Description |
|---|---|
| **DynamicMenuManager** | Builds context menus from `AssemblyClassDefinition` metadata. Scans for `[CommandAttribute]` methods. |
| **DynamicFunctionCallingManager** | Executes methods on branches and global function extensions. |
| **ControlExtensions** | Tree node creation, branch → SimpleItem conversion, method execution from tree nodes. |

## Architecture

```
BeepForms (Application Shell)
  ├── BeepFormsToolbar          ← Save, Refresh, Delete, New, Print, Export
  ├── BeepFormsCommandBar       ← Undo/Redo, Cancel Edit
  ├── BeepFormsQueryShelf       ← Filter/search query builder
  ├── BeepFormsHeader           ← Title, breadcrumb, metadata
  ├── BeepFormsPersistenceShelf ← Savepoint management
  ├── BeepFormsStatusStrip      ← Status messages, progress, validation
  └── BeepBlock (Content Panel)
        ├── RecordMode            ← Single-record CRUD form
        ├── GridMode              ← Multi-row data grid
        └── QueryMode             ← Query builder + grid results
             └── Field Presenters  ← Text, Combo, Date, CheckBox, Numeric
```

## Quick Start

```csharp
// Create a data-bound form from a connection + entity name
var forms = new BeepForms();
forms.SetDefinition(new BeepBlockDefinition
{
    BlockName = "CustomerForm",
    PresentationMode = BeepBlockPresentationMode.Record,
    Entity = new BeepBlockEntityDefinition
    {
        DataSourceName = "northwind.db",
        EntityName = "Customers"
    },
    Fields = new List<BeepFieldDefinition>
    {
        new() { FieldName = "CustomerId",  ControlType = "BeepTextBox" },
        new() { FieldName = "CompanyName", ControlType = "BeepTextBox" },
        new() { FieldName = "Country",     ControlType = "BeepComboBox",
                LOVDataSource = "northwind.db", LOVEntity = "Countries" }
    }
});

// One call generates all controls, opens the datasource, and binds data
forms.Initialize();
```

## Block Definition Model

```csharp
BeepBlockDefinition            ← Top-level config: Id, Name, PresentationMode, Fields, Entity
  ├── BeepBlockEntityDefinition  ← Data source binding: DataSourceName, EntityName, Triggers
  ├── BeepFieldDefinition        ← Per-field config: ControlType, Label, Required, LOV
  └── BeepBlockNavigationDefinition ← Navigation mode, paging, breadcrumbs
```

Field presenters auto-create and bind controls based on `ControlType`:
- `"BeepTextBox"` → `TextBeepFieldPresenter`
- `"BeepComboBox"` → `ComboBeepFieldPresenter` (with LOV datasource)
- `"BeepDatePicker"` → `DateBeepFieldPresenter`
- `"BeepCheckBox"` → `CheckboxBeepFieldPresenter`
- `"BeepNumericUpDown"` → `NumericBeepFieldPresenter`

## BeepAppTree — Navigation

`BeepAppTree` extends `BeepTree` with `IBranch`-driven navigation. It reads `ConfigEditor.BranchesClasses` and builds a hierarchical tree:

```
Genre (category)                          ← "Data Sources"
  └── Root (navigation item)              ← "Northwind Database"
        └── Child (entity)                ← "Customers" table
              └── Child (action)          ← "New Record", "Reports"
```

Each branch is discovered as an `IBranch` implementation via `AssemblyHandler.ScanExtensions()`. The tree auto-builds by instantiating each branch via reflection and calling `CreateChildNodes()`.

## Design-Time Integration

### BeepDataConnection (Tray Component)

Drop on a form at design time for automatic connection configuration:

```csharp
// In designer.cs after dropping BeepDataConnection on a form:
this.beepDataConnection1 = new BeepDataConnection();
// DataConnections auto-populated from BeepService config
// Connection properties appear in PropertyGrid
// Serializes to designer.cs for form-level connection management
```

Storage backends:
- `JsonConnectionStorageProvider` — JSON files in Beep app repo
- `IConnectionStorageProvider` — pluggable interface
- `ConnectionSecretProtector` — encrypted sensitive parameters

### NuggetsManage

Integrated NuGet package management:
- Search and browse NuGet packages within the designer
- Install/uninstall drivers and extensions
- Version selection and dependency resolution
- Auto-discovery via `NuggetsStartupBootstrapper`

## Dependencies

| Package | Version | Purpose |
|---|---|---|
| `TheTechIdea.Beep.DataManagementModels` | 3.0.0 | Core data models (`IDMEEditor`, `IDataSource`, `EntityStructure`) |
| `TheTechIdea.Beep.DataManagementEngine` | 2.0.70 | Runtime engine (conditional) |
| `TheTechIdea.Beep.Vis.Modules` | 2.0.43 | Visualization interfaces (`IBranch`, `ITree`, `IAppManager`, `SimpleItem`) |
| `TheTechIdea.Beep.Winform.Controls` | — | Base controls (ProjectReference) |
| `TheTechIdea.Beep.Winform.Controls.Design.Server` | — | Design-time infrastructure (ProjectReference, via Views) |

## Documentation

- [Browser Help](./Help/index.html) — Full HTML documentation with sidebar and theme toggle
- [Full Reference](./Help/integrated-controls.html) — BeepForms, BeepBlock, BeepAppTree, Data Connection, Block Definition Model

## Recent Changes

### SimpleItem Cleanup &amp; Serialization Fix (2026)
- `SimpleItem` now serializes correctly in WinForms designer
- Removed unused `INotifyPropertyChanged`, `ParentValueChanged` event chain, dead LOV subsystem
- `Color BadgeBackColor` → `BeepColor BadgeBackColor` (portable, implicit `Color` conversion)
- Added `[DesignerSerializationVisibility(Hidden)]` on non-serializable properties
- ~200 lines removed, 0 breaking changes to runtime behavior

### Global ImagePath Editor (2026)
- `GlobalImagePathEditorRegistration` in Design.Server auto-applies `BeepImagePathEditor` to any `ImagePath` string property
- Covers `SimpleItem` and all `BaseControl` subclasses
- Activated via `[ModuleInitializer]` — zero code changes in controls or models

### Project Structure Cleanup (2026)
- Integrated Controls documentation now self-contained in `Help/` with local CSS
- Added `docs/README.md` for quick architectural reference
- Design.Server now references Integrated.Controls via ProjectReference

## Project Status

- **Alpha Phase** — Core features functional, APIs may evolve.
- **Contributions** — Welcome! See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

Licensed under the [MIT License](LICENSE).
