# WinForms Oracle Forms Host Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the complete thin WinForms presentation layer for the BeepDM Oracle Forms engine using `WinFormFormHost`, `WinFormBlockHost`, Beep `IBeepUIComponent` field presenters, and the existing shared Forms contracts.

**Architecture:** `WinFormFormHost` is the only WinForms type that accesses `IUnitofWorksManager` and directly implements `IBeepFormsHost`. `WinFormBlockHost` communicates only through `IBeepFormsHost`; presenters wrap Beep controls and contain no datasource or engine access. Shared contracts receive only the minimal current-index and field-value methods needed to preserve that boundary.

**Tech Stack:** C# 12, .NET 10 Windows, WinForms, Beep WinForms controls, BeepDM Forms engine, xUnit, Moq.

---

## File Map

### Shared BeepDM contract

- Modify `../BeepDM/DataManagementModelsStandard/Editor/Forms/Hosts/IBeepFormsHost.cs`: add current-index and field get/set operations.

### WinForms production files

- Replace `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.cs`: core state and contract surface.
- Create `Forms/FormHost/WinFormFormHost.Blocks.cs`: block registry and active block.
- Create `Forms/FormHost/WinFormFormHost.Engine.cs`: direct `IUnitofWorksManager` delegation.
- Create `Forms/FormHost/WinFormFormHost.Lov.cs`: LOV loading and selection.
- Create `Forms/FormHost/WinFormFormHost.Notifications.cs`: Beep notification surface.
- Create `Forms/FormHost/WinFormFormHost.Threading.cs`: UI-thread marshaling.
- Replace `Forms/BlockHost/WinFormBlockHost.cs`: core block state.
- Create `Forms/BlockHost/WinFormBlockHost.Binding.cs`: bind/unbind and synchronization.
- Create `Forms/BlockHost/WinFormBlockHost.Fields.cs`: presenter discovery/generation and edits.
- Create `Forms/BlockHost/WinFormBlockHost.Operations.cs`: navigation and CRUD delegation.
- Create `Forms/BlockHost/WinFormBlockHost.QueryMode.cs`: query-mode UI state.
- Create `Forms/BlockHost/WinFormBlockHost.Validation.cs`: field and record validation display.
- Create `Forms/BlockHost/WinFormBlockNavigationBar.cs`: `IBlockNavigationBar` Beep control.
- Delete/rename `Forms/FieldHost/WinformTextBoxFiellPresenter.cs`.
- Create `Forms/FieldHost/WinFormFieldPresenterBase.cs`.
- Create six concrete presenter files and `WinFormFieldPresenterRegistry.cs`.

### Tests

- Create `Beep.Winform.Data.Integrated.Controls.Tests/TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj`.
- Create `Forms/StaTest.cs`: STA-thread test runner.
- Create `Forms/TestRecords.cs`: test record fixtures.
- Create focused test classes per task.

---

### Task 1: Complete the platform-neutral host contract

**Files:**
- Modify: `../BeepDM/DataManagementModelsStandard/Editor/Forms/Hosts/IBeepFormsHost.cs`
- Modify: `../BeepDM/DataManagementModelsStandard/Editor/Forms/Interfaces/IUnitofWorksManager.cs`
- Test: `../BeepDM/DataManagementEngineStandard/Editor/Forms.Tests/FormsHostContractTests.cs`

- [ ] **Step 1: Write the failing contract-shape test**

```csharp
using TheTechIdea.Beep.Editor.Forms.Hosts;
using Xunit;

namespace TheTechIdea.Beep.Editor.UOWManager.Tests;

public class FormsHostContractTests
{
    [Theory]
    [InlineData("GetCurrentBlockRecordIndex", typeof(int), typeof(string))]
    [InlineData("GetFieldValue", typeof(object), typeof(string), typeof(string))]
    [InlineData("SetFieldValue", typeof(bool), typeof(string), typeof(string), typeof(object))]
    public void IBeepFormsHost_ContainsThinBlockAccessMethods(
        string name, Type returnType, params Type[] parameters)
    {
        var method = typeof(IBeepFormsHost).GetMethod(name, parameters);
        Assert.NotNull(method);
        Assert.Equal(returnType, method!.ReturnType);
    }

    [Fact]
    public void IUnitofWorksManager_ExposesRecordFieldRead()
    {
        var method = typeof(IUnitofWorksManager).GetMethod(
            "GetFieldValue", [typeof(object), typeof(string)]);
        Assert.NotNull(method);
        Assert.Equal(typeof(object), method!.ReturnType);
    }
}
```

- [ ] **Step 2: Run the test and verify it fails**

Run:

```powershell
dotnet test .\DataManagementEngineStandard\Editor\Forms.Tests\FormsManager.Tests.csproj --filter FormsHostContractTests
```

Workdir: `../BeepDM`

Expected: FAIL because the host methods and engine-interface field reader are absent.

- [ ] **Step 3: Add the minimal methods to `IBeepFormsHost`**

```csharp
int GetCurrentBlockRecordIndex(string blockName);
object? GetFieldValue(string blockName, string fieldName);
bool SetFieldValue(string blockName, string fieldName, object? value);
```

Place the index method in the State section and field methods in the Data section. Do not add engine, datasource, table, or unit-of-work objects to the contract.

Add the existing `FormsManager` field-reader capability to `IUnitofWorksManager` beside `SetFieldValue`:

```csharp
object? GetFieldValue(object record, string FieldName);
```

- [ ] **Step 4: Run the focused test**

Run the command from Step 2.

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add DataManagementModelsStandard/Editor/Forms/Hosts/IBeepFormsHost.cs DataManagementModelsStandard/Editor/Forms/Interfaces/IUnitofWorksManager.cs DataManagementEngineStandard/Editor/Forms.Tests/FormsHostContractTests.cs
git commit -m "feat(forms): complete host field access contract"
```

Workdir: `../BeepDM`

---

### Task 2: Add the WinForms Forms test project and STA harness

**Files:**
- Create: `Beep.Winform.Data.Integrated.Controls.Tests/TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj`
- Create: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/StaTest.cs`
- Create: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/TestRecords.cs`
- Test: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/StaTestTests.cs`

- [ ] **Step 1: Create the test project**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>12.0</LangVersion>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Moq" Version="4.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Beep.Winform.Data.Integrated.Controls\TheTechIdea.Beep.Winform.Data.Integrated.csproj" />
    <ProjectReference Include="..\..\BeepDM\DataManagementEngineStandard\DataManagementEngine.csproj" />
    <ProjectReference Include="..\..\BeepDM\DataManagementModelsStandard\DataManagementModels.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Write the failing STA test**

```csharp
using Xunit;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Tests.Forms;

public class StaTestTests
{
    [Fact]
    public async Task RunAsync_ExecutesOnStaThread()
    {
        var state = await StaTest.RunAsync(
            () => Thread.CurrentThread.GetApartmentState());

        Assert.Equal(ApartmentState.STA, state);
    }
}
```

- [ ] **Step 3: Run the test and verify it fails**

```powershell
dotnet test .\Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter StaTestTests
```

Expected: FAIL because `StaTest` does not exist.

- [ ] **Step 4: Implement the STA harness and record fixture**

```csharp
namespace TheTechIdea.Beep.Winform.Data.Integrated.Tests.Forms;

internal static class StaTest
{
    public static Task<T> RunAsync<T>(Func<T> action)
    {
        var completion = new TaskCompletionSource<T>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try { completion.SetResult(action()); }
            catch (Exception ex) { completion.SetException(ex); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return completion.Task;
    }

    public static Task RunAsync(Action action) =>
        RunAsync(() => { action(); return true; });
}

internal sealed class EmployeeRecord
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime HireDate { get; set; }
    public bool Active { get; set; }
    public int DepartmentId { get; set; }
}
```

- [ ] **Step 5: Run the test and commit**

Expected: PASS.

```powershell
git add Beep.Winform.Data.Integrated.Controls.Tests
git commit -m "test(forms): add WinForms STA test harness"
```

---

### Task 3: Implement `WinFormFormHost` core registry and direct field access

**Files:**
- Replace: `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.cs`
- Create: `Forms/FormHost/WinFormFormHost.Blocks.cs`
- Create: `Forms/FormHost/WinFormFormHost.Threading.cs`
- Test: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostRegistryTests.cs`
- Test: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostFieldAccessTests.cs`

- [ ] **Step 1: Write registration and active-block tests**

Test these exact behaviors:

```csharp
[Fact]
public Task RegisterBlock_ValidBlock_BindsAndActivates() => StaTest.RunAsync(() =>
{
    var manager = new Mock<IUnitofWorksManager>();
    manager.Setup(m => m.BlockExists("EMP")).Returns(true);
    var host = new WinFormFormHost { FormsManager = manager.Object };
    var block = new WinFormBlockHost { BlockName = "EMP" };

    Assert.True(host.RegisterBlock(block));
    Assert.True(host.IsBlockRegistered("EMP"));
    Assert.Equal("EMP", host.ActiveBlockName);
    Assert.Same(host, block.FormsHost);
});

[Fact]
public Task RegisterBlock_DuplicateName_Throws() => StaTest.RunAsync(() =>
{
    var host = new WinFormFormHost();
    host.RegisterBlock(new WinFormBlockHost { BlockName = "EMP" });

    Assert.Throws<InvalidOperationException>(() =>
        host.RegisterBlock(new WinFormBlockHost { BlockName = "emp" }));
});
```

- [ ] **Step 2: Run tests and verify they fail**

```powershell
dotnet test .\Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter "WinFormFormHostRegistryTests|WinFormFormHostFieldAccessTests"
```

Expected: FAIL due to `NotImplementedException`.

- [ ] **Step 3: Implement the host core**

Use this state shape:

```csharp
public partial class WinFormFormHost : UserControl, IBeepFormsHost
{
    private readonly Dictionary<string, IBlockView> _blocks =
        new(StringComparer.OrdinalIgnoreCase);
    private IUnitofWorksManager? _formsManager;
    private string? _activeBlockName;

    public string? ActiveBlockName => _activeBlockName;
    public IUnitofWorksManager? FormsManager
    {
        get => _formsManager;
        set => ReplaceFormsManager(value);
    }

    public event EventHandler? ActiveBlockChanged;
}
```

Registration must require `IBlockView.View is Control`, enforce unique names, call `Bind(this)` when the engine block exists, and call `Unbind()` on unregister.

- [ ] **Step 4: Implement current index and field access directly over the manager**

```csharp
public int GetCurrentBlockRecordIndex(string blockName)
{
    var uow = RequireManager().GetUnitOfWork(blockName);
    return uow?.Units?.CurrentIndex ?? -1;
}

public object? GetFieldValue(string blockName, string fieldName)
{
    var record = GetCurrentBlockItem(blockName);
    return record is null ? null : RequireManager().GetFieldValue(record, fieldName);
}

public bool SetFieldValue(string blockName, string fieldName, object? value)
{
    var record = GetCurrentBlockItem(blockName);
    if (record is null) return false;
    if (!RequireManager().SetFieldValue(record, fieldName, value)) return false;
    return RequireManager().ValidateField(blockName, fieldName, value);
}
```

`GetFieldValue(object, string)` was added to `IUnitofWorksManager` in Task 1 and is already implemented by `FormsManager.FormsSimulation.cs`. Do not use reflection in WinForms.

- [ ] **Step 5: Run focused tests and commit**

Expected: PASS.

```powershell
git add Beep.Winform.Data.Integrated.Controls/Forms/FormHost Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHost* ../BeepDM/DataManagementModelsStandard/Editor/Forms/Interfaces/IUnitofWorksManager.cs ../BeepDM/DataManagementEngineStandard/Editor/Forms/FormsManager.FormsSimulation.cs
git commit -m "feat(forms): implement WinForms form host core"
```

Run `git add` and commit separately in each repository if the workspaces are separate Git repositories.

---

### Task 4: Implement complete engine delegation and refresh policy

**Files:**
- Create: `Forms/FormHost/WinFormFormHost.Engine.cs`
- Test: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostEngineTests.cs`

- [ ] **Step 1: Write delegation tests**

Cover each method group with representative strict Moq verification:

```csharp
[Fact]
public async Task MoveNextAsync_DelegatesAndRefreshesMasterAndDetails()
{
    await StaTest.RunAsync(() =>
    {
        var manager = new Mock<IUnitofWorksManager>();
        manager.Setup(m => m.BlockExists("EMP")).Returns(true);
        manager.Setup(m => m.BlockExists("DETAIL")).Returns(true);
        manager.Setup(m => m.NextRecordAsync("EMP")).ReturnsAsync(true);
        manager.Setup(m => m.GetDetailBlocks("EMP"))
            .Returns(["DETAIL"]);
        manager.Setup(m => m.GetBlock(It.IsAny<string>()))
            .Returns((DataBlockInfo?)null);
        manager.Setup(m => m.GetUnitOfWork(It.IsAny<string>()))
            .Returns((IUnitofWork?)null);

        using var host = new WinFormFormHost { FormsManager = manager.Object };
        using var master = new WinFormBlockHost { BlockName = "EMP" };
        using var detail = new WinFormBlockHost { BlockName = "DETAIL" };
        host.RegisterBlock(master);
        host.RegisterBlock(detail);

        Assert.True(host.MoveNextAsync("EMP").GetAwaiter().GetResult());
        manager.Verify(m => m.NextRecordAsync("EMP"), Times.Once);
    });
}
```

Add focused tests for save, rollback, insert, delete, execute query, clear block, clear record, duplicate, enter query, exit query, block state, metadata, data, and validation.

- [ ] **Step 2: Run and verify failure**

Use the test command from Task 3 with `--filter WinFormFormHostEngineTests`.

- [ ] **Step 3: Implement direct delegation**

Map methods as follows:

| Host operation | Engine operation |
|---|---|
| Move first/previous/next/last | `FirstRecordAsync`, `PreviousRecordAsync`, `NextRecordAsync`, `LastRecordAsync` |
| Move to index | `Locking.SetCurrentRecordIndex` |
| Insert/delete/duplicate | matching manager methods |
| Execute query | `ExecuteQueryAsync` |
| Enter/exit query | `EnterQueryAsync` / `ExitingQueryModeAsync` |
| Save block | `GetUnitOfWork(block).Commit()` |
| Rollback block | `GetUnitOfWork(block).Rollback()` |
| Clear block | `ClearBlockAsync` |
| Clear record | current unit of work `New()` |
| State and metadata | `GetBlock`, `GetUnitOfWork`, `ItemProperties`, `LOV`, `Validation` |

Each successful operation calls:

```csharp
private void RefreshBlockAndDetails(string blockName)
{
    if (_blocks.TryGetValue(blockName, out var block))
        RunOnUi(block.SyncFromManager);

    foreach (var detailName in GetDetailBlockNames(blockName))
        if (_blocks.TryGetValue(detailName, out var detail))
            RunOnUi(detail.SyncFromManager);
}
```

- [ ] **Step 4: Run tests and commit**

```powershell
git add Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.Engine.cs Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostEngineTests.cs
git commit -m "feat(forms): delegate WinForms host operations to FormsManager"
```

---

### Task 5: Implement presenter base, registry, and six Beep presenters

**Files:**
- Delete: `Forms/FieldHost/WinformTextBoxFiellPresenter.cs`
- Create: `Forms/FieldHost/WinFormFieldPresenterBase.cs`
- Create: `Forms/FieldHost/WinFormTextBoxFieldPresenter.cs`
- Create: `Forms/FieldHost/WinFormNumericFieldPresenter.cs`
- Create: `Forms/FieldHost/WinFormDateFieldPresenter.cs`
- Create: `Forms/FieldHost/WinFormBooleanFieldPresenter.cs`
- Create: `Forms/FieldHost/WinFormComboFieldPresenter.cs`
- Create: `Forms/FieldHost/WinFormReflectiveFieldPresenter.cs`
- Create: `Forms/FieldHost/WinFormFieldPresenterRegistry.cs`
- Test: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/FieldPresenterTests.cs`
- Test: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/FieldPresenterRegistryTests.cs`

- [ ] **Step 1: Write presenter behavior tests**

```csharp
[Fact]
public Task TextPresenter_SetValue_DoesNotRaiseValueChanged() => StaTest.RunAsync(() =>
{
    var presenter = new WinFormTextBoxFieldPresenter(
        new EntityField { FieldName = "Name", Fieldtype = "string" });
    var raised = 0;
    presenter.ValueChanged += (_, _) => raised++;

    presenter.SetValue("Alice");

    Assert.Equal("Alice", presenter.Value);
    Assert.Equal(0, raised);
});

[Theory]
[InlineData("int", typeof(WinFormNumericFieldPresenter))]
[InlineData("date", typeof(WinFormDateFieldPresenter))]
[InlineData("bool", typeof(WinFormBooleanFieldPresenter))]
[InlineData("string", typeof(WinFormTextBoxFieldPresenter))]
public Task Registry_UsesEngineFieldTypeMapping(string fieldType, Type expected) =>
    StaTest.RunAsync(() =>
    {
        var registry = new WinFormFieldPresenterRegistry();
        var presenter = registry.Create(
            new EntityField { FieldName = "F", Fieldtype = fieldType });
        Assert.IsType(expected, presenter);
    });
```

- [ ] **Step 2: Run and verify failure**

Filter: `FieldPresenterTests|FieldPresenterRegistryTests`.

- [ ] **Step 3: Implement the shared base**

The base must:

- store `EntityField`;
- require an editor implementing both `Control` and `IBeepUIComponent`;
- use `SetValue`, `GetValue`, `ClearValue`, and `ValidateData`;
- translate `IFieldPresenter` state to Beep component state;
- suppress `ValueChanged` during `SetValue`;
- subscribe/unsubscribe `OnValueChanged`;
- expose `View` as the underlying `Control`.

Core event logic:

```csharp
private void EditorOnValueChanged(object? sender, BeepComponentEventArgs e)
{
    if (_synchronizing) return;
    ValueChanged?.Invoke(this, Editor.GetValue());
}

public void SetValue(object? value)
{
    _synchronizing = true;
    try { Editor.SetValue(value!); }
    finally { _synchronizing = false; }
}
```

- [ ] **Step 4: Implement concrete controls**

Use:

- `BeepTextBox`
- `BeepNumericUpDown`
- `BeepDatePicker`
- `BeepCheckBoxBool`
- `BeepComboBox`
- a configured `IBeepUIComponent` instance for reflective fallback.

Apply `FieldName`, `Fieldtype`, caption/description, size, required, read-only, identity, hidden, numeric precision/scale, and maximum length where the control supports them.

- [ ] **Step 5: Implement the registry**

```csharp
public IFieldPresenter Create(EntityField field)
{
    return FieldTypeMapper.GetCanonicalFieldType(field) switch
    {
        "Numeric" => new WinFormNumericFieldPresenter(field),
        "Date" => new WinFormDateFieldPresenter(field),
        "Boolean" or "Checkbox" => new WinFormBooleanFieldPresenter(field),
        "ReadOnly" => new WinFormTextBoxFieldPresenter(field) { IsReadOnly = true },
        _ => new WinFormTextBoxFieldPresenter(field)
    };
}
```

LOV replacement occurs when the block detects `FormsHost.HasLov`.

- [ ] **Step 6: Run tests and commit**

```powershell
git add Beep.Winform.Data.Integrated.Controls/Forms/FieldHost Beep.Winform.Data.Integrated.Controls.Tests/Forms/FieldPresenter*
git commit -m "feat(forms): add Beep field presenters"
```

---

### Task 6: Implement block binding, metadata generation, and edit synchronization

**Files:**
- Replace: `Forms/BlockHost/WinFormBlockHost.cs`
- Create: `Forms/BlockHost/WinFormBlockHost.Binding.cs`
- Create: `Forms/BlockHost/WinFormBlockHost.Fields.cs`
- Test: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormBlockHostBindingTests.cs`

- [ ] **Step 1: Write binding tests**

Cover:

- invalid/empty block name fails;
- unknown engine block fails;
- metadata creates one presenter per visible field;
- hidden fields are skipped;
- manual presenter wins by case-insensitive field name;
- `SyncFromManager` fills values and does not call `SetFieldValue`;
- user edit calls `SetFieldValue` exactly once;
- `Unbind` removes events and generated controls.

Representative test:

```csharp
[Fact]
public Task SyncFromManager_UpdatesPresenterWithoutWritingBack() => StaTest.RunAsync(() =>
{
    var host = new Mock<IBeepFormsHost>();
    host.SetupGet(h => h.FormsManager).Returns(new Mock<IUnitofWorksManager>().Object);
    host.Setup(h => h.IsBlockRegistered("EMP")).Returns(true);
    host.Setup(h => h.GetBlockFields("EMP")).Returns(
        [new EntityField { FieldName = "Name", Fieldtype = "string" }]);
    host.Setup(h => h.GetFieldValue("EMP", "Name")).Returns("Alice");
    host.Setup(h => h.GetBlockMode("EMP")).Returns(DataBlockMode.Query);
    host.Setup(h => h.GetBlockRecordCount("EMP")).Returns(1);
    host.Setup(h => h.GetCurrentBlockRecordIndex("EMP")).Returns(0);

    using var block = new WinFormBlockHost { BlockName = "EMP" };
    block.Bind(host.Object);

    Assert.Equal("Alice", block.FindFieldPresenter("Name")!.Value);
    host.Verify(h => h.SetFieldValue(
        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
});
```

- [ ] **Step 2: Run and verify failure**

Filter: `WinFormBlockHostBindingTests`.

- [ ] **Step 3: Implement block core state**

Use:

```csharp
private readonly List<IFieldPresenter> _fieldPresenters = [];
private readonly HashSet<IFieldPresenter> _ownedPresenters = [];
private IBeepFormsHost? _formsHost;
private bool _isBound;
private bool _isSynchronizing;
private int _currentRecordIndex = -1;
```

`View` returns `this`. `ManagerBlockName` returns normalized `BlockName`. `FormsHost` returns `_formsHost`.

- [ ] **Step 4: Implement presenter generation and layout**

Use a `TableLayoutPanel` or Beep container held by the block only as WinForms layout infrastructure. Generate missing presenters from ordered visible `EntityField` values. Add each underlying `Control` and a `BeepLabel`; track generated presenters in `_ownedPresenters`.

- [ ] **Step 5: Implement synchronization and edits**

```csharp
public void SyncFromManager()
{
    if (!_isBound || _formsHost is null) return;
    _isSynchronizing = true;
    try
    {
        _currentRecordIndex = _formsHost.GetCurrentBlockRecordIndex(BlockName);
        foreach (var presenter in _fieldPresenters)
            presenter.SetValue(_formsHost.GetFieldValue(BlockName, presenter.FieldName));
        ApplyMode(_formsHost.GetBlockMode(BlockName));
        RefreshNavigationBar();
    }
    finally { _isSynchronizing = false; }
}
```

Presenter edits call `_formsHost.SetFieldValue(BlockName, presenter.FieldName, value)`. On failure, set `ValidationError`; on success, clear it and synchronize engine-dependent fields.

- [ ] **Step 6: Run tests and commit**

```powershell
git add Beep.Winform.Data.Integrated.Controls/Forms/BlockHost Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormBlockHostBindingTests.cs
git commit -m "feat(forms): bind WinForms blocks to engine state"
```

---

### Task 7: Implement navigation, CRUD, and query mode

**Files:**
- Create: `Forms/BlockHost/WinFormBlockNavigationBar.cs`
- Create: `Forms/BlockHost/WinFormBlockHost.Operations.cs`
- Create: `Forms/BlockHost/WinFormBlockHost.QueryMode.cs`
- Test: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormBlockOperationsTests.cs`
- Test: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormBlockNavigationBarTests.cs`

- [ ] **Step 1: Write operation tests**

Verify each block method delegates to the corresponding host method with its `BlockName`, then synchronizes on success. Verify query mode disables non-queryable presenters and restores them on exit.

```csharp
[Fact]
public Task NavigateNextAsync_DelegatesToHost() => StaTest.RunAsync(() =>
{
    var host = BuildBoundHost("EMP");
    host.Setup(h => h.MoveNextAsync("EMP")).ReturnsAsync(true);
    using var block = BindBlock(host.Object, "EMP");

    Assert.True(block.NavigateNextAsync().GetAwaiter().GetResult());
    host.Verify(h => h.MoveNextAsync("EMP"), Times.Once);
});
```

- [ ] **Step 2: Run and verify failure**

Filter: `WinFormBlockOperationsTests|WinFormBlockNavigationBarTests`.

- [ ] **Step 3: Implement the navigation bar**

Build a `UserControl` implementing `IBlockNavigationBar` with Beep first/previous/next/last buttons and a numeric record position editor. Button clicks raise contract events only. `Refresh()` displays one-based position while the contract property remains zero-based.

- [ ] **Step 4: Implement block operations**

Each operation uses:

```csharp
private async Task<bool> ExecuteAndSyncAsync(Func<IBeepFormsHost, Task<bool>> operation)
{
    if (_formsHost is null) return false;
    var result = await operation(_formsHost).ConfigureAwait(true);
    if (result) SyncFromManager();
    return result;
}
```

Do not use `.Result` or `.Wait()` in production.

- [ ] **Step 5: Implement query mode**

`EnterQueryMode()` and `ExitQueryMode()` call the asynchronous host methods through internal async command handlers. Public synchronous contract methods update visual state only after host success. Maintain criteria in presenters when query execution fails.

- [ ] **Step 6: Run tests and commit**

```powershell
git add Beep.Winform.Data.Integrated.Controls/Forms/BlockHost Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormBlockOperationsTests.cs Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormBlockNavigationBarTests.cs
git commit -m "feat(forms): add block navigation CRUD and query mode"
```

---

### Task 8: Implement LOV, validation, notifications, and master-detail refresh

**Files:**
- Create: `Forms/FormHost/WinFormFormHost.Lov.cs`
- Create: `Forms/FormHost/WinFormFormHost.Notifications.cs`
- Create: `Forms/BlockHost/WinFormBlockHost.Validation.cs`
- Modify: `Forms/FieldHost/WinFormComboFieldPresenter.cs`
- Test: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormLovTests.cs`
- Test: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormValidationTests.cs`
- Test: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormMasterDetailTests.cs`

- [ ] **Step 1: Write LOV and validation tests**

Verify:

- LOV data is loaded only through manager `LOV`/`ShowLOVAsync`;
- cancellation does not modify fields;
- selection applies every engine-provided related mapping via `SetFieldValue`;
- record validation maps field errors to presenters;
- first invalid visible presenter receives focus;
- non-field errors call `ShowError`;
- master operations synchronize registered detail blocks.

- [ ] **Step 2: Run and verify failure**

Filter: `WinFormLovTests|WinFormValidationTests|WinFormMasterDetailTests`.

- [ ] **Step 3: Implement LOV rendering**

Use a small Beep WinForms dialog containing `BeepComboBox` or the existing Beep grid/list control. The dialog accepts `LOVDefinition` and `LOVResult`; it has no engine reference. `WinFormFormHost.ShowLovAsync` loads data from `IUnitofWorksManager`, displays the dialog on the UI thread, and returns the engine result/selection.

- [ ] **Step 4: Apply LOV mappings**

For the selected row:

```csharp
var mapped = GetLovRelatedFieldValues(lov, selectedItem);
if (mapped is not null)
    foreach (var pair in mapped)
        SetFieldValue(blockName, pair.Key, pair.Value);
RefreshBlockAndDetails(blockName);
```

- [ ] **Step 5: Implement validation display**

Build the current-record dictionary from the engine host values for registered presenters, call `ValidateBlockRecord`, set `ValidationError` by field name, and call `ShowError` for general errors. Do not reimplement validation rules.

- [ ] **Step 6: Implement notifications**

Use Beep notification/dialog controls where available. If a notification manager requires a parent form, fall back to `BeepDialogBox`; do not use a standard `MessageBox`.

- [ ] **Step 7: Run tests and commit**

```powershell
git add Beep.Winform.Data.Integrated.Controls/Forms/FormHost Beep.Winform.Data.Integrated.Controls/Forms/BlockHost/WinFormBlockHost.Validation.cs Beep.Winform.Data.Integrated.Controls/Forms/FieldHost/WinFormComboFieldPresenter.cs Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormLovTests.cs Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormValidationTests.cs Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormMasterDetailTests.cs
git commit -m "feat(forms): add LOV validation and detail synchronization"
```

---

### Task 9: Implement threading, lifecycle, and event relays

**Files:**
- Modify: `Forms/FormHost/WinFormFormHost.Threading.cs`
- Modify: `Forms/FormHost/WinFormFormHost.cs`
- Modify: `Forms/BlockHost/WinFormBlockHost.Binding.cs`
- Test: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormLifecycleTests.cs`
- Test: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormThreadingTests.cs`

- [ ] **Step 1: Write lifecycle tests**

Verify manager replacement unbinds/rebinds, unregister removes handlers, generated presenters are disposed, designer presenters survive removal, and repeated disposal does not throw.

- [ ] **Step 2: Write UI-thread test**

Create the host on the STA thread, call a refresh from a worker thread, pump the WinForms message loop until completion, and assert the control update ran on the creating thread.

- [ ] **Step 3: Run and verify failure**

Filter: `WinFormLifecycleTests|WinFormThreadingTests`.

- [ ] **Step 4: Implement safe UI marshaling**

```csharp
private void RunOnUi(Action action)
{
    if (IsDisposed || Disposing) return;
    if (InvokeRequired)
    {
        BeginInvoke((MethodInvoker)(() =>
        {
            if (!IsDisposed && !Disposing) action();
        }));
        return;
    }
    action();
}
```

- [ ] **Step 5: Implement ownership and unsubscription**

Use named event handlers. Dispose only presenters in `_ownedPresenters`; always detach presenter, navigation, host, and manager events before clearing collections.

- [ ] **Step 6: Run tests and commit**

```powershell
git add Beep.Winform.Data.Integrated.Controls/Forms Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormLifecycleTests.cs Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormThreadingTests.cs
git commit -m "fix(forms): harden WinForms lifecycle and threading"
```

---

### Task 10: Add a real FormsManager integration smoke test

**Files:**
- Create: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormsManagerIntegrationTests.cs`

- [ ] **Step 1: Write the integration test**

Create a real `FormsManager` with a mocked `IDMEEditor`, register an `IUnitofWork` backed by an observable list of `EmployeeRecord`, then:

1. assign manager to `WinFormFormHost`;
2. register `WinFormBlockHost`;
3. assert generated presenters;
4. navigate;
5. edit `Name`;
6. validate;
7. invoke save and rollback using configured unit-of-work results.

The test must assert that no WinForms type other than the form host receives or stores `IUnitofWorksManager`.

- [ ] **Step 2: Run and verify failure**

```powershell
dotnet test .\Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter WinFormFormsManagerIntegrationTests
```

- [ ] **Step 3: Make only the minimal corrections exposed by the smoke test**

Corrections must preserve the architecture. Do not add datasource access, new presentation interfaces, a bridge, or raw reflection.

- [ ] **Step 4: Run and commit**

Expected: PASS.

```powershell
git add Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormsManagerIntegrationTests.cs Beep.Winform.Data.Integrated.Controls/Forms
git commit -m "test(forms): verify WinForms host with real FormsManager"
```

---

### Task 11: Full verification and documentation cleanup

**Files:**
- Modify: `Beep.Winform.Data.Integrated.Controls/Forms/ENGINE-GAP-ANALYSIS.md`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/README.md`

- [ ] **Step 1: Verify no implementation stubs remain**

```powershell
rg -n "NotImplementedException" .\Beep.Winform.Data.Integrated.Controls\Forms
```

Expected: no production-code matches.

- [ ] **Step 2: Verify architectural boundaries**

```powershell
rg -n "IUnitofWorksManager" .\Beep.Winform.Data.Integrated.Controls\Forms
```

Expected: matches only in `WinFormFormHost` partials.

```powershell
rg -n "IDataSource|DataTable|DataView|DataRowView|BeepFormsHostBridge" .\Beep.Winform.Data.Integrated.Controls\Forms
```

Expected: no production-code matches.

- [ ] **Step 3: Run all relevant tests**

```powershell
dotnet test .\Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj
dotnet test ..\BeepDM\DataManagementEngineStandard\Editor\Forms.Tests\FormsManager.Tests.csproj
```

Expected: all tests pass.

- [ ] **Step 4: Build production projects**

```powershell
dotnet build .\Beep.Winform.Data.Integrated.Controls\TheTechIdea.Beep.Winform.Data.Integrated.csproj
dotnet build ..\BeepDM\DataManagementEngineStandard\DataManagementEngine.csproj
```

Expected: both builds succeed with zero new warnings from Forms files.

- [ ] **Step 5: Replace the obsolete gap document and add usage documentation**

The README must include:

```csharp
var host = new WinFormFormHost
{
    Dock = DockStyle.Fill,
    FormsManager = formsManager
};

var employees = new WinFormBlockHost
{
    BlockName = "EMP",
    Dock = DockStyle.Fill
};

host.Controls.Add(employees);
host.RegisterBlock(employees);
```

Document engine-first setup: the engine block must be created through `FormsManager.SetupBlockAsync` or `RegisterBlock` before binding the UI block.

- [ ] **Step 6: Commit final documentation**

```powershell
git add Beep.Winform.Data.Integrated.Controls/Forms/README.md Beep.Winform.Data.Integrated.Controls/Forms/ENGINE-GAP-ANALYSIS.md
git commit -m "docs(forms): document WinForms Oracle Forms host"
```
