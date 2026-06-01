# Phase 7: EntityStructure → BeepBlock Pipeline

> **Architecture rule — non-negotiable:**
> `BeepBlock` and `BeepForms` are **UI only**.
> All datasource access, entity structure retrieval, field metadata, business logic,
> and unit-of-work creation live exclusively in **FormsManager** (BeepDM).
> BeepBlock and BeepForms **read** from FormsManager. They never call `IDataSource`
> directly and never accept `EntityStructure` as a parameter.

---

## Data Flow (invariant)

```
IDataSource.GetEntityStructure()     ← FormsManager only
      ↓
FormsManager.RegisterBlock(blockName, unitOfWork, entityStructure, connectionName)
      ↓
FormsManager.GetBlock(blockName) → DataBlockInfo.UnitOfWork.EntityStructure
      ↓
BeepBlock.SyncFromManager()
  └─ CreateEntityDefinition(blockInfo, entityStructure)
  └─ BeepBlockEntityFieldDefinition[]  ← UI snapshot (read-only)
      ↓
BeepFieldControlTypeRegistry.ResolveDefaultFieldSettings(Category, DataType, IsLong)
      ↓
WinForms controls rendered in BeepBlock
```

---

## Sub-Phase Map

| # | Title | Owner | Depends On |
|---|---|---|---|
| **7A** | EntityField Fidelity — add 5 missing fields to snapshot | BeepBlock (UI) | — |
| **7B** | `FormsManager.SetupBlockAsync` — new BeepDM API | **FormsManager (BeepDM)** | — |
| **7C** | FieldCategory → ControlType resolution | BeepBlock (UI) | 7A |
| **7D** | BeepForms bootstrap trigger (UI side only) | BeepForms (UI) | 7A, 7B, 7C |
| **7E** | Designer-time preview via IDE extension | Design.Server | 7A, 7B |

---

# Sub-Phase 7A — EntityField Fidelity

**Goal:** `BeepBlockEntityFieldDefinition` is a read-only UI snapshot of `EntityField`.
Five properties are currently missing, causing wrong rendering.

| Missing field | UI impact |
|---|---|
| `IsIdentity` | Identity columns appear editable; included in INSERT |
| `IsHidden` | Provider-hidden fields appear in UI |
| `IsLong` | LOB/TEXT/CLOB fields get a single-line TextBox |
| `IsRowVersion` | Concurrency timestamps appear editable |
| `DefaultValue` | No placeholder/hint on new-record editors |

### `Blocks/Models/BeepBlockEntityDefinition.cs`

Add after existing `IsCheck` property in `BeepBlockEntityFieldDefinition`:

```csharp
[NotifyParentProperty(true)]
public bool IsIdentity { get; set; }

[NotifyParentProperty(true)]
public bool IsHidden { get; set; }

[NotifyParentProperty(true)]
public bool IsLong { get; set; }

[NotifyParentProperty(true)]
public bool IsRowVersion { get; set; }

[NotifyParentProperty(true)]
public string DefaultValue { get; set; } = string.Empty;
```

Update `ToFieldDefinition()`:

```csharp
IsVisible  = !IsHidden,
IsReadOnly = IsReadOnly || IsAutoIncrement || IsIdentity || IsRowVersion,
DefaultValue = DefaultValue,
```

Update `Clone()` to include the 5 new fields.

### `Blocks/Models/BeepFieldDefinition.cs`

Add:

```csharp
[NotifyParentProperty(true)]
public string DefaultValue { get; set; } = string.Empty;
```

### `Blocks/BeepBlock/BeepBlock.Metadata.cs`

Extend the field-mapping loop in `CreateEntityDefinition()`:

```csharp
IsIdentity   = field.IsIdentity,
IsHidden     = field.IsHidden,
IsLong       = field.IsLong,
IsRowVersion = field.IsRowVersion,
DefaultValue = field.DefaultValue ?? string.Empty
```

**Exit criteria:** Identity/hidden/rowversion columns render correctly; LOB columns flagged with `IsLong = true`.

---

# Sub-Phase 7B — FormsManager `SetupBlockAsync` (BeepDM gap)

**Goal:** FormsManager exposes one method that does the full datasource-open → EntityStructure → UoW → RegisterBlock pipeline internally. UI layers pass only strings; they never touch a datasource object.

### `IUnitofWorksManagerInterfaces.cs` (BeepDM)

```csharp
/// <summary>
/// Opens the named datasource, fetches EntityStructure, creates UnitOfWork,
/// and registers the block. All datasource work happens inside FormsManager.
/// UI layers (BeepForms, BeepBlock) must never call IDataSource directly.
/// </summary>
Task<bool> SetupBlockAsync(
    string blockName,
    string connectionName,
    string entityName,
    bool isMasterBlock = false,
    CancellationToken cancellationToken = default);
```

### `FormsManager.cs` (BeepDM)

```csharp
public async Task<bool> SetupBlockAsync(
    string blockName, string connectionName, string entityName,
    bool isMasterBlock = false, CancellationToken ct = default)
{
    var ds = DMEEditor.GetDataSource(connectionName);
    if (ds == null) return false;

    if (ds.ConnectionStatus != ConnectionState.Open)
        ds.Openconnection();

    var entityStructure = await Task.Run(
        () => ds.GetEntityStructure(entityName, false), ct);
    if (entityStructure == null) return false;

    var uow = DMEEditor.CreateUnitOfWork(entityName, ds);
    RegisterBlock(blockName, uow, entityStructure, connectionName, isMasterBlock);
    return true;
}
```

**Exit criteria:** `await formsManager.SetupBlockAsync("customers", "NorthwindDB", "Customers")` → block registered → `BeepBlock.SyncFromManager()` populates fields. Zero datasource calls in BeepBlock or BeepForms.

---

# Sub-Phase 7C — FieldCategory → ControlType Resolution

**Goal:** `BeepFieldControlTypeRegistry.ResolveDefaultFieldSettings()` returns correct control type for every `DbFieldCategory` and for `IsLong = true`. Pure UI decision.

### `Blocks/Services/BeepFieldControlTypeRegistry.cs`

Add `isLong` parameter:

```csharp
public static (string EditorKey, string ControlType) ResolveDefaultFieldSettings(
    DbFieldCategory category,
    string dataType,
    bool isCheck,
    bool isLong = false)
```

Short-circuit at top:

```csharp
if (isLong || category == DbFieldCategory.LongString)
    return ("memo", "TheTechIdea.Beep.Winform.Controls.BeepRichTextBox");

if (isCheck || category == DbFieldCategory.Boolean)
    return ("checkbox", "TheTechIdea.Beep.Winform.Controls.BeepCheckBox");
```

Complete the category switch:

```csharp
return category switch
{
    DbFieldCategory.Integer
        => ("integer",  "TheTechIdea.Beep.Winform.Controls.BeepNumericBox"),
    DbFieldCategory.Decimal or DbFieldCategory.Float
        => ("decimal",  "TheTechIdea.Beep.Winform.Controls.BeepNumericBox"),
    DbFieldCategory.Date
        => ("date",     "TheTechIdea.Beep.Winform.Controls.BeepDatePicker"),
    DbFieldCategory.DateTime
        => ("datetime", "TheTechIdea.Beep.Winform.Controls.BeepDateTimePicker"),
    DbFieldCategory.Enum
        => ("combobox", "TheTechIdea.Beep.Winform.Controls.BeepComboBox"),
    DbFieldCategory.Binary
        => ("blob",     "TheTechIdea.Beep.Winform.Controls.BeepLabel"),
    _ => ("text",       "TheTechIdea.Beep.Winform.Controls.BeepTextBox")
};
```

Update `ResolveWidth()` in `BeepBlockEntityFieldDefinition`:

```csharp
if (IsLong)                               return 320;
if (Category == DbFieldCategory.Integer)  return 120;
if (Category is DbFieldCategory.Decimal
             or DbFieldCategory.Float)    return 160;
```

Update all `ResolveDefaultFieldSettings` call sites to pass `isLong: IsLong`.

**Exit criteria:** LOB → memo-style; Date → date picker; Decimal → numeric box; Bool → checkbox; Binary → read-only label.

---

# Sub-Phase 7D — BeepForms Bootstrap Trigger (UI side only)

**Goal:** `BeepForms` has one method, `InitializeAsync()`, that passes `(blockName, connectionName, entityName)` strings to `FormsManager.SetupBlockAsync()` per block definition, then calls `SyncFromManager()`. That is the full extent of what BeepForms does — no datasource, no UoW, no EntityStructure.

> **Prerequisite:** Phase 7B must be in BeepDM first.

### `Forms/Models/BeepFormsBootstrapModels.cs` (new file)

```csharp
public enum BootstrapState { Idle, Loading, Ready, Error }

public sealed class BeepFormsBootstrapEventArgs : EventArgs
{
    public bool Success { get; }
    public BeepFormsBootstrapEventArgs(bool success) => Success = success;
}
```

### `Forms/Models/BeepFormsViewState.cs`

Add:

```csharp
public BootstrapState BootstrapState { get; set; } = BootstrapState.Idle;
```

### `Forms/BeepForms/BeepForms.cs`

```csharp
public event EventHandler<BeepFormsBootstrapEventArgs>? BootstrapCompleted;

public async Task<bool> InitializeAsync(CancellationToken cancellationToken = default)
{
    if (DesignMode || _formsManager == null || _definition?.Blocks == null
        || _definition.Blocks.Count == 0)
        return false;

    _viewState.BootstrapState = BootstrapState.Loading;
    bool allOk = true;

    foreach (var blockDef in _definition.Blocks)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string blockName  = blockDef.ManagerBlockName ?? blockDef.BlockName;
        string connName   = blockDef.Entity?.ConnectionName ?? string.Empty;
        string entityName = blockDef.Entity?.EntityName ?? string.Empty;

        if (string.IsNullOrWhiteSpace(connName) || string.IsNullOrWhiteSpace(entityName))
            continue;   // no datasource info — skip; developer pre-registered manually

        // FormsManager does everything: open connection, GetEntityStructure, create UoW, RegisterBlock
        bool ok = await _formsManager.SetupBlockAsync(
            blockName, connName, entityName,
            blockDef.Entity?.IsMasterBlock ?? false,
            cancellationToken);

        if (!ok) allOk = false;
    }

    _viewState.BootstrapState = allOk ? BootstrapState.Ready : BootstrapState.Error;
    SyncFromManager();   // reads from FormsManager — no datasource calls here
    BootstrapCompleted?.Invoke(this, new BeepFormsBootstrapEventArgs(allOk));
    return allOk;
}
```

Trigger from property setters (guard `DesignMode`):

```csharp
// In FormsManager setter and Definition setter:
if (_formsManager != null && _definition?.Blocks?.Count > 0)
    _ = InitializeAsync();
else
    SyncFromManager();
```

**What is NOT created:**
- No `IBeepFormsBootstrapper` interface.
- No `BeepFormsDefaultBootstrapper` service. Those called `IDataSource` — architectural violation.

**Exit criteria:** Set `Definition` (with `ConnectionName` + `EntityName`) then `FormsManager` → blocks populate automatically. Pre-registered blocks continue to work unchanged. No `IDataSource` call in `BeepForms.cs` or `BeepBlock.cs`.

---

# Sub-Phase 7E — Designer-Time Preview

**Goal:** Setting `ConnectionName` + `EntityName` in the VS designer Property Grid auto-populates the field list.

### `Design.Server/Editors/BeepBlockEntityDefinitionTypeConverter.cs`

```csharp
public override PropertyDescriptorCollection GetProperties(
    ITypeDescriptorContext context, object value, Attribute[] attrs)
{
    if (value is BeepBlockEntityDefinition entityDef
     && !string.IsNullOrWhiteSpace(entityDef.ConnectionName)
     && !string.IsNullOrWhiteSpace(entityDef.EntityName)
     && entityDef.Fields.Count == 0)
    {
        TryPopulateFromIde(entityDef);
    }
    return base.GetProperties(context, value, attrs);
}

private static void TryPopulateFromIde(BeepBlockEntityDefinition entityDef)
{
    try
    {
        // Accessing IDataSource here is acceptable: this is IDE extension code (design-time
        // tool boundary). BeepBlock.cs and BeepForms.cs runtime code must never do this.
        var editor = ExtensionEntrypoint.GetEditor();
        var ds = editor?.GetDataSource(entityDef.ConnectionName);
        if (ds == null) return;
        if (ds.ConnectionStatus != ConnectionState.Open) ds.Openconnection();
        var structure = ds.GetEntityStructure(entityDef.EntityName, false);
        if (structure?.Fields == null) return;
        var snapshot = BeepBlock.CreateEntityDefinition(null, structure);  // internal static
        entityDef.Fields.Clear();
        entityDef.Fields.AddRange(snapshot.Fields);
    }
    catch { /* never crash VS */ }
}
```

### `Blocks/BeepBlock/BeepBlock.Metadata.cs`

Change `CreateEntityDefinition` from `private static` to `internal static`.

**Exit criteria:** Designer shows real column names when `ConnectionName` + `EntityName` are set. If connection unavailable, designer stays empty without exception.

---

# Work Item Summary

## 7A — EntityField Fidelity (Beep.Winform)

| # | Item | File |
|---|---|---|
| 7A-1 | Add 5 fields to `BeepBlockEntityFieldDefinition` | `Blocks/Models/BeepBlockEntityDefinition.cs` |
| 7A-2 | Update `CreateEntityDefinition()` mapping loop | `Blocks/BeepBlock/BeepBlock.Metadata.cs` |
| 7A-3 | Update `ToFieldDefinition()` — `IsVisible`, `IsReadOnly`, `DefaultValue` | `Blocks/Models/BeepBlockEntityDefinition.cs` |
| 7A-4 | Update `Clone()` for 5 new fields | `Blocks/Models/BeepBlockEntityDefinition.cs` |
| 7A-5 | Add `DefaultValue` to `BeepFieldDefinition` + `Clone()` | `Blocks/Models/BeepFieldDefinition.cs` |

## 7B — FormsManager SetupBlockAsync (BeepDM)

| # | Item | File |
|---|---|---|
| 7B-1 | Add `SetupBlockAsync` to `IUnitofWorksManager` | `DataManagementEngineStandard/Editor/Forms/Interfaces/IUnitofWorksManagerInterfaces.cs` |
| 7B-2 | Implement `SetupBlockAsync` in `FormsManager.cs` | `DataManagementEngineStandard/Editor/Forms/FormsManager.cs` |

## 7C — FieldCategory → ControlType (Beep.Winform)

| # | Item | File |
|---|---|---|
| 7C-1 | Add `isLong` param + short-circuit to `ResolveDefaultFieldSettings` | `Blocks/Services/BeepFieldControlTypeRegistry.cs` |
| 7C-2 | Complete category switch | `Blocks/Services/BeepFieldControlTypeRegistry.cs` |
| 7C-3 | Update `ResolveWidth()` | `Blocks/Models/BeepBlockEntityDefinition.cs` |
| 7C-4 | Update call sites to pass `isLong` | `Blocks/Models/BeepBlockEntityDefinition.cs` |

## 7D — BeepForms Bootstrap Trigger (Beep.Winform)

| # | Item | File |
|---|---|---|
| 7D-1 | Create `BootstrapState` enum + `BeepFormsBootstrapEventArgs` | `Forms/Models/BeepFormsBootstrapModels.cs` (new) |
| 7D-2 | Add `BootstrapState` to `BeepFormsViewState` | `Forms/Models/BeepFormsViewState.cs` |
| 7D-3 | Add `InitializeAsync()` + `BootstrapCompleted` event | `Forms/BeepForms/BeepForms.cs` |
| 7D-4 | Update `FormsManager` + `Definition` property setters | `Forms/BeepForms/BeepForms.cs` |
| 7D-5 | Surface `BootstrapState` in `BeepFormsStatusStrip` | `Forms/Surfaces/BeepFormsStatusStrip.cs` |

## 7E — Designer-Time Preview (Beep.Winform)

| # | Item | File |
|---|---|---|
| 7E-1 | `CreateEntityDefinition` → `internal static` | `Blocks/BeepBlock/BeepBlock.Metadata.cs` |
| 7E-2 | Override `GetProperties` + `TryPopulateFromIde` | `Design.Server/Editors/BeepBlockEntityDefinitionTypeConverter.cs` |

---

## Architecture Boundary

| Concern | Owner |
|---|---|
| Open datasource, `GetEntityStructure`, create UoW, `RegisterBlock` | **FormsManager (BeepDM)** |
| Provide `DataBlockInfo` to UI | **FormsManager (BeepDM)** |
| Map `EntityField` → UI snapshot, render controls | **BeepBlock (UI)** |
| Host blocks, route commands, trigger bootstrap | **BeepForms (UI)** — passes strings only |
| `IDataSource` at design-time | **IDE Extension** (Design.Server) — design boundary only |

---

## Decision Log

| Date | Decision |
|---|---|
| 2026-04-13 | Bootstrap gap identified; Phase 7 plan created. |
| 2026-04-13 | `LoadEntityStructure(EntityStructure)` **removed** from `BeepBlock.Metadata.cs` — violated UI-only rule. |
| 2026-04-13 | `IBeepFormsBootstrapper` / `BeepFormsDefaultBootstrapper` **not created** — they called `IDataSource` directly. |
| 2026-04-13 | Phase 7B renamed from "Direct EntityStructure Feed" to "FormsManager `SetupBlockAsync`". FormsManager is the only place datasources are accessed. |
| 2026-04-13 | Architecture boundary codified: FormsManager is sole data authority; BeepBlock/BeepForms are pure UI. |
