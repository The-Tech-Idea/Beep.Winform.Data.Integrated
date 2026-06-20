# WinForms Oracle Forms Advanced Parity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Expose every UI-meaningful advanced Forms engine feature through the WinForms thin layer without moving datasource, query, trigger, transaction, or state logic out of `IUnitofWorksManager`.

**Architecture:** `WinFormFormHost` remains the only WinForms type allowed to reference `IUnitofWorksManager`; it implements grouped additions to `IBeepFormsHost` and relays engine events to registered `IBlockView` instances. `WinFormBlockHost`, field presenters, and feature controls depend only on `IBeepFormsHost` plus platform-neutral Forms models. The work is delivered in four independently testable increments: runtime interaction, state/navigation, runtime objects, and multi-form/utilities.

**Tech Stack:** .NET, C#, WinForms, xUnit, Moq, BeepDM Forms engine, Beep WinForms controls.

---

## File Map

### Shared engine contracts

- Modify `../BeepDM/DataManagementModelsStandard/Editor/Forms/Hosts/IBeepFormsHost.cs`: add platform-neutral advanced operations grouped by feature.
- Modify `../BeepDM/DataManagementModelsStandard/Editor/Forms/Hosts/IBlockView.cs`: add host-to-block relay methods and advanced block commands.
- Modify `../BeepDM/DataManagementModelsStandard/Editor/Forms/Hosts/IFieldPresenter.cs`: add QBE criteria state without adding WinForms types.
- Create `../BeepDM/DataManagementModelsStandard/Editor/Forms/Models/FormsHostEvents.cs`: event models for timer, message, and form-message relays.
- Test `../BeepDM/DataManagementModelsStandard.Tests/Editor/Forms/FormsHostAdvancedContractTests.cs`: reflection tests that protect the thin-layer boundary.

### WinForms host

- Modify `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.cs`: advanced event declarations and lifecycle hook points.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.Subscriptions.cs`: attach/detach manager, trigger, timer, message, and form-message subscriptions.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.Triggers.cs`: trigger query/control/fire delegation and block-scoped event routing.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.Query.cs`: QBE filter building, templates, and query history.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.Alerts.cs`: message queue and alert-provider integration.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.Locks.cs`: record locking and lock-on-edit delegation.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.Savepoints.cs`: savepoint operations and rollback refresh.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.History.cs`: navigation history and bookmarks.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.RuntimeObjects.cs`: timers, sequences, record groups, and parameter lists.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.MultiForm.cs`: form factory, globals, parameters, messaging, and call-stack commands.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.Utilities.cs`: state, computed values, freeze, import/export, paging, text I/O, properties, transactions, and status.

### WinForms block and presenters

- Create `Beep.Winform.Data.Integrated.Controls/Forms/BlockHost/WinFormBlockHost.Triggers.cs`: public relay methods that raise existing trigger events.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/BlockHost/WinFormBlockHost.Query.cs`: criteria collection and query-mode behavior.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/BlockHost/WinFormBlockHost.Locking.cs`: first-edit auto-lock and edit rejection.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/BlockHost/WinFormBlockHost.AdvancedOperations.cs`: history, savepoint, bookmark, state, computed, and utility commands.
- Modify `Beep.Winform.Data.Integrated.Controls/Forms/BlockHost/WinFormBlockHost.cs`: call partial helpers while preserving current binding behavior.
- Modify `Beep.Winform.Data.Integrated.Controls/Forms/FieldHost/WinFormFieldPresenterBase.cs`: store query value, operator, and enabled state.

### WinForms feature controls

- Create `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormLovDialog.cs`.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormQueryPanel.cs`.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormLockPanel.cs`.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormSavepointPanel.cs`.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormHistoryDialog.cs`.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormTimerPanel.cs`.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormSequencePanel.cs`.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormRecordGroupPanel.cs`.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormParameterListPanel.cs`.
- Create `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormMultiFormPanel.cs`.

### WinForms tests

- Create `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostTriggerTests.cs`.
- Create `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormBlockHostQueryTests.cs`.
- Create `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormLovDialogTests.cs`.
- Create `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostMessageTests.cs`.
- Create `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostLockSavepointTests.cs`.
- Create `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostHistoryTests.cs`.
- Create `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostRuntimeObjectTests.cs`.
- Create `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostMultiFormTests.cs`.
- Create `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostUtilityTests.cs`.
- Modify `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormsManagerIntegrationTests.cs`: add one real-manager smoke test per increment.

## Increment 1 — Triggers, QBE, LOV, Alerts

### Task 1: Extend the platform-neutral host contracts

**Files:**
- Modify: `../BeepDM/DataManagementModelsStandard/Editor/Forms/Hosts/IBeepFormsHost.cs`
- Modify: `../BeepDM/DataManagementModelsStandard/Editor/Forms/Hosts/IBlockView.cs`
- Modify: `../BeepDM/DataManagementModelsStandard/Editor/Forms/Hosts/IFieldPresenter.cs`
- Create: `../BeepDM/DataManagementModelsStandard/Editor/Forms/Models/FormsHostEvents.cs`
- Create: `../BeepDM/DataManagementModelsStandard.Tests/Editor/Forms/FormsHostAdvancedContractTests.cs`

- [ ] **Step 1: Add failing contract tests**

```csharp
[Fact]
public void Host_contract_exposes_advanced_features_without_manager_types()
{
    var host = typeof(IBeepFormsHost);
    Assert.NotNull(host.GetMethod(nameof(IBeepFormsHost.GetBlockTriggers)));
    Assert.NotNull(host.GetMethod(nameof(IBeepFormsHost.ExecuteQueryByExampleAsync)));
    Assert.NotNull(host.GetMethod(nameof(IBeepFormsHost.LockCurrentRecordAsync)));
    Assert.NotNull(host.GetMethod(nameof(IBeepFormsHost.CreateTimer)));

    Assert.DoesNotContain(
        host.GetMethods().SelectMany(m => m.GetParameters()).Select(p => p.ParameterType),
        type => typeof(IUnitofWorksManager).IsAssignableFrom(type));
}

[Fact]
public void Block_and_field_contracts_expose_relay_and_query_state()
{
    Assert.NotNull(typeof(IBlockView).GetMethod(nameof(IBlockView.RaiseTriggerExecuting)));
    Assert.NotNull(typeof(IFieldPresenter).GetProperty(nameof(IFieldPresenter.QueryValue)));
    Assert.NotNull(typeof(IFieldPresenter).GetProperty(nameof(IFieldPresenter.QueryOperator)));
}
```

- [ ] **Step 2: Run the contract test and verify it fails**

Run:

```powershell
dotnet test ..\BeepDM\DataManagementModelsStandard.Tests\DataManagementModelsStandard.Tests.csproj --filter FullyQualifiedName~FormsHostAdvancedContractTests
```

Expected: compile failure because the advanced members do not exist.

- [ ] **Step 3: Add the complete grouped contract surface**

Add query and trigger members:

```csharp
IReadOnlyList<TriggerDefinition> GetBlockTriggers(string blockName);
TriggerStatisticsInfo GetTriggerStatistics(string blockName);
Task<TriggerResult> FireBlockTriggerAsync(
    TriggerType type, string blockName, TriggerContext? context = null,
    CancellationToken ct = default);
Task<TriggerResult> FireKeyTriggerAsync(KeyTriggerType keyType, string blockName);
void EnableTrigger(string triggerId);
void DisableTrigger(string triggerId);
void SuspendTriggers();
void ResumeTriggers();

Task<bool> ExecuteQueryByExampleAsync(
    string blockName, IReadOnlyDictionary<string, QueryCriterion> criteria,
    CancellationToken ct = default);
void SaveQueryTemplate(string blockName, string templateName, IReadOnlyDictionary<string, QueryCriterion> criteria);
QueryTemplateInfo? LoadQueryTemplate(string blockName, string templateName);
IReadOnlyList<QueryTemplateInfo> GetQueryTemplates(string blockName);
bool DeleteQueryTemplate(string blockName, string templateName);
IReadOnlyList<QueryHistoryEntry> GetQueryHistory(string blockName);
void ClearQueryHistory(string blockName);
```

Add runtime/state/utility members in the same interface, grouped exactly as the design:

```csharp
Task<bool> LockCurrentRecordAsync(string blockName, CancellationToken ct = default);
bool UnlockCurrentRecord(string blockName);
void UnlockAllRecords(string blockName);
bool IsCurrentRecordLocked(string blockName);
RecordLockInfo? GetCurrentRecordLockInfo(string blockName);
IReadOnlyList<RecordLockInfo> GetAllLocks(string blockName);
LockMode GetLockMode(string blockName);
void SetLockMode(string blockName, LockMode mode);
bool GetLockOnEdit(string blockName);
void SetLockOnEdit(string blockName, bool value);

string CreateSavepoint(string blockName, string? savepointName = null);
Task<bool> RollbackToSavepointAsync(string blockName, string savepointName, CancellationToken ct = default);
bool ReleaseSavepoint(string blockName, string savepointName);
void ReleaseAllSavepoints(string blockName);
IReadOnlyList<SavepointInfo> GetSavepoints(string blockName);

Task<bool> NavigateBackAsync(string blockName);
Task<bool> NavigateForwardAsync(string blockName);
bool CanNavigateBack(string blockName);
bool CanNavigateForward(string blockName);
IReadOnlyList<NavigationHistoryEntry> GetNavigationHistory(string blockName);
void ClearNavigationHistory(string blockName);
void SetBookmark(string blockName, string bookmarkName);
bool GoToBookmark(string blockName, string bookmarkName);
void RemoveBookmark(string blockName, string bookmarkName);
void ClearBookmarks(string blockName);

TimerDefinition CreateTimer(string timerName, TimeSpan interval, bool repeating = false);
bool DeleteTimer(string timerName);
TimerDefinition? GetTimer(string timerName);
IReadOnlyList<TimerDefinition> GetTimers();
long GetNextSequence(string sequenceName);
long PeekNextSequence(string sequenceName);
void CreateSequence(string sequenceName, long startValue = 1, long incrementBy = 1);
void ResetSequence(string sequenceName, long startValue = 1);

void CreateRecordGroup(string name, string dataSourceName, string entityName, List<AppFilter>? filters = null);
Task<bool> PopulateRecordGroupAsync(string name, CancellationToken ct = default);
RecordGroup? GetRecordGroup(string name);
IReadOnlyList<RecordGroup> GetRecordGroups();
bool RemoveRecordGroup(string name);
void ClearRecordGroups();

ParameterList CreateParameterList(string name);
bool DestroyParameterList(string name);
void SetParameter(string listName, string parameterName, object? value);
object? GetParameter(string listName, string parameterName);
bool RemoveParameter(string listName, string parameterName);
IReadOnlyList<ParameterList> GetParameterLists();
void ClearParameterList(string listName);
```

Add multi-form and utility members:

```csharp
Task<bool> CallFormAsync(string formName, Dictionary<string, object>? parameters = null,
    FormCallMode mode = FormCallMode.Modal, CancellationToken ct = default);
Task<bool> OpenFormModelessAsync(string formName, Dictionary<string, object>? parameters = null);
Task<bool> NewFormAsync(string formName, Dictionary<string, object>? parameters = null);
Task<bool> ReturnToCallerAsync(object? returnData = null);
void SetGlobalVariable(string name, object? value);
object? GetGlobalVariable(string name);
void PostMessage(string targetForm, string messageType, object? payload = null);
void BroadcastMessage(string messageType, object? payload = null);

FormStateSnapshot SaveFormState();
Task<bool> RestoreFormStateAsync(FormStateSnapshot snapshot, CancellationToken ct = default);
IReadOnlyDictionary<string, object> GetComputedValues(string blockName);
void FreezeBlock(string blockName);
void UnfreezeBlock(string blockName);
bool RevertCurrentRecord(string blockName);
Task<bool> RefreshBlockAsync(string blockName, ConflictMode mode = ConflictMode.ServerWins,
    CancellationToken ct = default);
ChangeSummary GetBlockChangeSummary(string blockName);
IReadOnlyList<object> GetDetailedChangeLog(string blockName);
Task ExportBlockToJsonAsync(string blockName, Stream stream, CancellationToken ct = default);
Task ExportBlockToCsvAsync(string blockName, Stream stream, char delimiter = ',',
    CancellationToken ct = default);
Task<int> ImportBlockFromJsonAsync(string blockName, Stream stream, bool clearFirst = true,
    CancellationToken ct = default);
Task<int> ImportBlockFromCsvAsync(string blockName, Stream stream, char delimiter = ',',
    bool clearFirst = true, bool hasHeaderRow = true, CancellationToken ct = default);
Task GoToPageAsync(string blockName, int page, CancellationToken ct = default);
Task PrefetchAdjacentPagesAsync(string blockName, CancellationToken ct = default);
Task<string> ReadTextFileAsync(string path, CancellationToken ct = default);
Task WriteTextFileAsync(string path, string content, CancellationToken ct = default);
void SetClientInfo(string clientInfo);
string GetClientInfo();
void SetApplicationProperty(string name, object? value);
object? GetApplicationProperty(string name);
bool BeginFormTransaction();
bool CommitFormTransaction();
void EndFormTransaction();
BlockStatus GetBlockStatus(string blockName);

event EventHandler<FormsHostTimerEventArgs>? TimerFired;
event EventHandler<FormsHostMessageEventArgs>? MessageRaised;
event EventHandler<FormsHostMessageEventArgs>? MessageCleared;
event EventHandler<FormsHostFormMessageEventArgs>? FormMessageReceived;
```

Add the shared query model:

```csharp
public sealed record QueryCriterion(
    object? Value,
    QueryOperator Operator = QueryOperator.Equals,
    bool IsEnabled = true);
```

Add block relay methods:

```csharp
void RaiseTriggerExecuting(TriggerExecutingEventArgs args);
void RaiseTriggerExecuted(TriggerExecutedEventArgs args);
void RaiseTriggerRegistered(TriggerRegisteredEventArgs args);
void RaiseTriggerUnregistered(TriggerUnregisteredEventArgs args);
```

Add presenter state:

```csharp
object? QueryValue { get; set; }
QueryOperator QueryOperator { get; set; }
bool IsQueryEnabled { get; set; }
```

- [ ] **Step 4: Run contract and model builds**

Run:

```powershell
dotnet test ..\BeepDM\DataManagementModelsStandard.Tests\DataManagementModelsStandard.Tests.csproj --filter FullyQualifiedName~FormsHostAdvancedContractTests
dotnet build ..\BeepDM\DataManagementModelsStandard\DataManagementModelsStandard.csproj
```

Expected: both commands pass.

- [ ] **Step 5: Commit the contract increment**

```powershell
git -C ..\BeepDM add DataManagementModelsStandard/Editor/Forms DataManagementModelsStandard.Tests/Editor/Forms
git -C ..\BeepDM commit -m "feat(forms): expose advanced host contract"
```

### Task 2: Implement trigger delegation and lifecycle-safe event relay

**Files:**
- Modify: `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.Subscriptions.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.Triggers.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/BlockHost/WinFormBlockHost.Triggers.cs`
- Create: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostTriggerTests.cs`
- Modify: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostLifecycleTests.cs`

- [ ] **Step 1: Write failing relay and unsubscription tests**

```csharp
[Fact]
public void Trigger_event_is_relayed_only_to_matching_block()
{
    using var host = TestHost.CreateWithManager(out var manager);
    var orders = new Mock<IBlockView>();
    orders.SetupGet(x => x.BlockName).Returns("ORDERS");
    var customers = new Mock<IBlockView>();
    customers.SetupGet(x => x.BlockName).Returns("CUSTOMERS");
    host.RegisterBlock(orders.Object);
    host.RegisterBlock(customers.Object);

    manager.Triggers.RegisterBlockTrigger(
        TriggerType.WhenValidateRecord, "ORDERS", _ => TriggerResult.Success);
    manager.Triggers.FireBlockTrigger(TriggerType.WhenValidateRecord, "ORDERS");

    orders.Verify(x => x.RaiseTriggerExecuting(It.IsAny<TriggerExecutingEventArgs>()), Times.Once);
    customers.Verify(x => x.RaiseTriggerExecuting(It.IsAny<TriggerExecutingEventArgs>()), Times.Never);
}

[Fact]
public void Replacing_manager_detaches_old_trigger_events()
{
    using var host = TestHost.CreateWithManager(out var first);
    var second = TestHost.CreateManager();
    host.FormsManager = second;

    first.Triggers.RegisterBlockTrigger(
        TriggerType.WhenNewRecordInstance, "ORDERS", _ => TriggerResult.Success);
    first.Triggers.FireBlockTrigger(TriggerType.WhenNewRecordInstance, "ORDERS");

    Assert.Equal(0, host.RelayedTriggerEventCount);
}
```

- [ ] **Step 2: Run tests and verify failure**

```powershell
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter FullyQualifiedName~WinFormFormHostTriggerTests
```

Expected: compile failure because relay methods and subscriptions are absent.

- [ ] **Step 3: Implement subscription ownership**

Use explicit symmetric methods:

```csharp
private void AttachManagerEvents(IUnitofWorksManager manager)
{
    manager.Triggers.TriggerExecuting += ManagerTriggerExecuting;
    manager.Triggers.TriggerExecuted += ManagerTriggerExecuted;
    manager.Triggers.TriggerRegistered += ManagerTriggerRegistered;
    manager.Triggers.TriggerUnregistered += ManagerTriggerUnregistered;
    manager.Messages.OnMessage += ManagerMessageRaised;
    manager.Messages.OnMessageCleared += ManagerMessageCleared;
    manager.OnFormMessage += ManagerFormMessage;
    manager.Timers.TimerFired += ManagerTimerFired;
}

private void DetachManagerEvents(IUnitofWorksManager manager)
{
    manager.Triggers.TriggerExecuting -= ManagerTriggerExecuting;
    manager.Triggers.TriggerExecuted -= ManagerTriggerExecuted;
    manager.Triggers.TriggerRegistered -= ManagerTriggerRegistered;
    manager.Triggers.TriggerUnregistered -= ManagerTriggerUnregistered;
    manager.Messages.OnMessage -= ManagerMessageRaised;
    manager.Messages.OnMessageCleared -= ManagerMessageCleared;
    manager.OnFormMessage -= ManagerFormMessage;
    manager.Timers.TimerFired -= ManagerTimerFired;
}
```

Add the existing engine providers to `IUnitofWorksManager` as read-only `ITimerManager Timers` and `ISequenceProvider Sequences` properties. `FormsManager` already implements those properties; do not construct providers in WinForms.

Call `DetachManagerEvents(previousManager)` before replacement and `AttachManagerEvents(manager)` after successful assignment. On replacement failure, detach the new manager and restore subscriptions to the previous manager.

- [ ] **Step 4: Implement trigger delegation and block relay**

```csharp
public IReadOnlyList<TriggerDefinition> GetBlockTriggers(string blockName) =>
    RequireManager().Triggers.GetBlockTriggers(blockName);

public TriggerStatisticsInfo GetTriggerStatistics(string blockName) =>
    RequireManager().Triggers.GetTriggerStatistics(blockName);

public Task<TriggerResult> FireBlockTriggerAsync(
    TriggerType type, string blockName, TriggerContext? context = null,
    CancellationToken ct = default) =>
    RequireManager().Triggers.FireBlockTriggerAsync(type, blockName, context, ct);

private void ManagerTriggerExecuting(object? sender, TriggerExecutingEventArgs args) =>
    RunOnUi(() => FindBlockForTrigger(args)?.RaiseTriggerExecuting(args));
```

Implement the other three event handlers with the same block resolution rule: use `args.Context.BlockName`; if blank, relay to the active block only.

In `WinFormBlockHost.Triggers.cs`:

```csharp
public void RaiseTriggerExecuting(TriggerExecutingEventArgs args) =>
    TriggerExecuting?.Invoke(this, args);

public void RaiseTriggerExecuted(TriggerExecutedEventArgs args) =>
    TriggerExecuted?.Invoke(this, args);

public void RaiseTriggerRegistered(TriggerRegisteredEventArgs args) =>
    TriggerRegistered?.Invoke(this, args);

public void RaiseTriggerUnregistered(TriggerUnregisteredEventArgs args) =>
    TriggerUnregistered?.Invoke(this, args);
```

- [ ] **Step 5: Run trigger and lifecycle tests**

```powershell
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter "FullyQualifiedName~WinFormFormHostTriggerTests|FullyQualifiedName~WinFormFormHostLifecycleTests"
```

Expected: all selected tests pass.

- [ ] **Step 6: Commit trigger relay**

```powershell
git add Beep.Winform.Data.Integrated.Controls/Forms Beep.Winform.Data.Integrated.Controls.Tests/Forms
git commit -m "feat(forms): relay engine triggers to WinForms blocks"
```

### Task 3: Implement QBE criteria, templates, and query history

**Files:**
- Modify: `Beep.Winform.Data.Integrated.Controls/Forms/FieldHost/WinFormFieldPresenterBase.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.Query.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/BlockHost/WinFormBlockHost.Query.cs`
- Modify: `Beep.Winform.Data.Integrated.Controls/Forms/BlockHost/WinFormBlockHost.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormQueryPanel.cs`
- Create: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormBlockHostQueryTests.cs`

- [ ] **Step 1: Write failing QBE behavior tests**

```csharp
[Fact]
public async Task Successful_qbe_uses_host_and_clears_criteria()
{
    var host = new Mock<IBeepFormsHost>();
    host.Setup(x => x.IsBlockRegistered("ORDERS")).Returns(true);
    host.Setup(x => x.ExecuteQueryByExampleAsync(
            "ORDERS",
            It.Is<IReadOnlyDictionary<string, QueryCriterion>>(c =>
                Equals(c["STATUS"].Value, "OPEN") &&
                c["STATUS"].Operator == QueryOperator.Equals),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(true);

    using var block = TestBlock.Create(host.Object, "ORDERS", "STATUS");
    block.EnterQueryMode();
    block.FindFieldPresenter("STATUS")!.QueryValue = "OPEN";

    Assert.True(await block.ExecuteQueryAsync());
    Assert.Null(block.FindFieldPresenter("STATUS")!.QueryValue);
}

[Fact]
public async Task Failed_qbe_preserves_criteria()
{
    var host = TestHost.MockQueryResult(false);
    using var block = TestBlock.Create(host.Object, "ORDERS", "STATUS");
    block.EnterQueryMode();
    block.FindFieldPresenter("STATUS")!.QueryValue = "OPEN";

    Assert.False(await block.ExecuteQueryAsync());
    Assert.Equal("OPEN", block.FindFieldPresenter("STATUS")!.QueryValue);
}
```

- [ ] **Step 2: Run test and verify failure**

```powershell
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter FullyQualifiedName~WinFormBlockHostQueryTests
```

Expected: compile failure because presenter query state and QBE execution are absent.

- [ ] **Step 3: Implement presenter and block query state**

```csharp
public object? QueryValue { get; set; }
public QueryOperator QueryOperator { get; set; } = QueryOperator.Equals;
public bool IsQueryEnabled { get; set; } = true;
```

Collect criteria without writing them into the current record:

```csharp
public IReadOnlyDictionary<string, QueryCriterion> GetQueryCriteria() =>
    _presenters
        .Where(p => p.IsQueryEnabled && p.QueryValue is not null)
        .ToDictionary(
            p => p.FieldName,
            p => new QueryCriterion(p.QueryValue, p.QueryOperator, true),
            StringComparer.OrdinalIgnoreCase);

private void ClearQueryCriteria()
{
    foreach (var presenter in _presenters)
        presenter.QueryValue = null;
}
```

During query mode, `PresenterOnValueChanged` updates `QueryValue` and returns without calling `SetFieldValue`.

- [ ] **Step 4: Implement host-side engine query building**

```csharp
public async Task<bool> ExecuteQueryByExampleAsync(
    string blockName,
    IReadOnlyDictionary<string, QueryCriterion> criteria,
    CancellationToken ct = default)
{
    var manager = RequireManager();
    var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    manager.QueryBuilder.ClearQueryOperators(blockName);

    foreach (var pair in criteria.Where(x => x.Value.IsEnabled && x.Value.Value is not null))
    {
        manager.QueryBuilder.SetQueryOperator(blockName, pair.Key, pair.Value.Operator);
        values[pair.Key] = pair.Value.Value!;
    }

    var filters = manager.QueryBuilder.BuildFilters(blockName, values);
    ct.ThrowIfCancellationRequested();
    var result = await manager.ExecuteQueryAsync(blockName, filters);
    if (result)
        SyncBlockAndDetails(blockName);
    return result;
}
```

Template methods convert `QueryCriterion` to filters through `QueryBuilder.BuildFilters`; load returns the engine `QueryTemplateInfo` unchanged.

- [ ] **Step 5: Add the thin query panel**

`WinFormQueryPanel` receives `IBeepFormsHost` and `IBlockView` through constructor properties, renders one operator selector for each query-enabled presenter, and wires execute/cancel/clear/template commands. It must not reference `IUnitofWorksManager`, `IDataSource`, or `FormsManager`.

- [ ] **Step 6: Run QBE and existing binding tests**

```powershell
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter "FullyQualifiedName~WinFormBlockHostQueryTests|FullyQualifiedName~WinFormBlockHostBindingTests"
```

Expected: all selected tests pass.

- [ ] **Step 7: Commit QBE**

```powershell
git add Beep.Winform.Data.Integrated.Controls/Forms Beep.Winform.Data.Integrated.Controls.Tests/Forms
git commit -m "feat(forms): add engine-backed WinForms QBE"
```

### Task 4: Implement LOV dialog, alerts, and message queue rendering

**Files:**
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.Alerts.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormLovDialog.cs`
- Create: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormLovDialogTests.cs`
- Create: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostMessageTests.cs`

- [ ] **Step 1: Write failing LOV cancellation and message relay tests**

```csharp
[Fact]
public async Task Cancelled_lov_does_not_mutate_block()
{
    var host = new Mock<IBeepFormsHost>(MockBehavior.Strict);
    using var dialog = new WinFormLovDialog(host.Object, "ORDERS", "CUSTOMER_ID");
    dialog.CancelSelection();

    Assert.False(await dialog.ApplySelectionAsync());
    host.Verify(x => x.SetFieldValue(
        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()), Times.Never);
}

[Fact]
public void Engine_message_is_marshaled_and_raised_by_host()
{
    using var host = TestHost.CreateWithManager(out var manager);
    FormsHostMessageEventArgs? observed = null;
    host.MessageRaised += (_, args) => observed = args;

    manager.Messages.ShowWarningMessage("ORDERS", "Record is locked");

    Assert.Equal("ORDERS", observed!.BlockName);
    Assert.Equal(MessageLevel.Warning, observed.Level);
}
```

- [ ] **Step 2: Run tests and verify failure**

```powershell
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter "FullyQualifiedName~WinFormLovDialogTests|FullyQualifiedName~WinFormFormHostMessageTests"
```

Expected: compile failure because the controls and relay events do not exist.

- [ ] **Step 3: Implement message and alert delegation**

```csharp
private void ManagerMessageRaised(object? sender, BlockMessageEventArgs args) =>
    RunOnUi(() => MessageRaised?.Invoke(
        this,
        new FormsHostMessageEventArgs(args.BlockName, args.Message, args.Level)));

public Task<AlertResult> ShowEngineAlertAsync(
    string title, string message, AlertStyle style,
    string button1Text = "OK", string? button2Text = null,
    string? button3Text = null, CancellationToken ct = default) =>
    RequireManager().ShowAlertAsync(
        title, message, style, button1Text, button2Text, button3Text, ct);
```

Keep `ShowInfo`, `ShowWarning`, and `ShowError` as notification adapters; engine messages flow through `MessageRaised`.

- [ ] **Step 4: Implement the LOV dialog**

The dialog:

1. Calls `LoadLovDataAsync` after debounce or explicit search.
2. Uses columns supplied by `LOVDefinition`.
3. Stores the selected engine row as `SelectedRecord`.
4. Calls `GetLovRelatedFieldValues` only when accepted.
5. Applies each returned field through `SetFieldValue`.
6. Returns `false` without mutation on cancellation or no selection.

Core apply method:

```csharp
public Task<bool> ApplySelectionAsync(CancellationToken ct = default)
{
    if (!_accepted || SelectedRecord is null)
        return Task.FromResult(false);

    var lov = _host.GetLov(_blockName, _fieldName);
    var values = lov is null
        ? null
        : _host.GetLovRelatedFieldValues(lov, SelectedRecord);
    if (values is null || values.Count == 0)
        return Task.FromResult(false);

    return Task.FromResult(values.All(pair =>
        _host.SetFieldValue(_blockName, pair.Key, pair.Value)));
}
```

- [ ] **Step 5: Run Increment 1 tests**

```powershell
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter "FullyQualifiedName~WinFormFormHostTriggerTests|FullyQualifiedName~WinFormBlockHostQueryTests|FullyQualifiedName~WinFormLovDialogTests|FullyQualifiedName~WinFormFormHostMessageTests"
```

Expected: all Increment 1 tests pass.

- [ ] **Step 6: Commit Increment 1**

```powershell
git add Beep.Winform.Data.Integrated.Controls/Forms Beep.Winform.Data.Integrated.Controls.Tests/Forms
git commit -m "feat(forms): complete WinForms runtime interaction features"
```

## Increment 2 — Locks, Savepoints, History, Bookmarks

### Task 5: Implement locks and savepoints

**Files:**
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.Locks.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.Savepoints.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/BlockHost/WinFormBlockHost.Locking.cs`
- Modify: `Beep.Winform.Data.Integrated.Controls/Forms/BlockHost/WinFormBlockHost.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormLockPanel.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormSavepointPanel.cs`
- Create: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostLockSavepointTests.cs`

- [ ] **Step 1: Write failing lock rejection and rollback refresh tests**

```csharp
[Fact]
public async Task Lock_failure_rejects_first_edit_and_restores_presenter()
{
    var host = TestHost.MockAutoLock("ORDERS", result: false, engineValue: "OLD");
    using var block = TestBlock.Create(host.Object, "ORDERS", "STATUS");
    block.FindFieldPresenter("STATUS")!.SetValue("NEW");

    await block.WaitForPendingEditAsync();

    Assert.Equal("OLD", block.FindFieldPresenter("STATUS")!.Value);
    host.Verify(x => x.SetFieldValue("ORDERS", "STATUS", "NEW"), Times.Never);
}

[Fact]
public async Task Savepoint_rollback_synchronizes_master_and_details()
{
    using var host = TestHost.CreateWithManager(out var manager);
    manager.Savepoints.CreateSavepoint("ORDERS", "before-edit");

    Assert.True(await host.RollbackToSavepointAsync("ORDERS", "before-edit"));
    Assert.Equal(1, host.GetSyncCount("ORDERS"));
    Assert.Equal(1, host.GetSyncCount("ORDER_LINES"));
}
```

- [ ] **Step 2: Run tests and verify failure**

```powershell
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter FullyQualifiedName~WinFormFormHostLockSavepointTests
```

Expected: compile failure because lock/savepoint host methods are absent.

- [ ] **Step 3: Implement direct lock and savepoint delegation**

```csharp
public Task<bool> LockCurrentRecordAsync(string blockName, CancellationToken ct = default) =>
    RequireManager().Locking.LockCurrentRecordAsync(blockName, ct);

public async Task<bool> RollbackToSavepointAsync(
    string blockName, string savepointName, CancellationToken ct = default)
{
    var result = await RequireManager().Savepoints
        .RollbackToSavepointAsync(blockName, savepointName, ct);
    if (result)
        SyncBlockAndDetails(blockName);
    return result;
}
```

Implement every lock/savepoint contract method as a one-line delegation except rollback, which refreshes the block tree.

- [ ] **Step 4: Gate the first edit through engine locking**

Convert `PresenterOnValueChanged` to an async helper. When lock-on-edit is enabled and the current record is not locked, call `LockCurrentRecordAsync`. On failure, set the presenter back to `GetFieldValue`, set a validation error, and do not call `SetFieldValue`.

- [ ] **Step 5: Implement thin lock and savepoint panels**

Panels render engine-returned `RecordLockInfo` and `SavepointInfo`; button handlers call only `IBeepFormsHost`. Disable buttons while async operations run.

- [ ] **Step 6: Run tests and commit**

```powershell
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter FullyQualifiedName~WinFormFormHostLockSavepointTests
git add Beep.Winform.Data.Integrated.Controls/Forms Beep.Winform.Data.Integrated.Controls.Tests/Forms
git commit -m "feat(forms): add record locks and savepoints"
```

### Task 6: Implement navigation history and bookmarks

**Files:**
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.History.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/BlockHost/WinFormBlockHost.AdvancedOperations.cs`
- Modify: `Beep.Winform.Data.Integrated.Controls/Forms/BlockHost/WinFormBlockNavigationBar.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormHistoryDialog.cs`
- Create: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostHistoryTests.cs`

- [ ] **Step 1: Write failing history delegation test**

```csharp
[Fact]
public async Task Navigation_back_delegates_and_synchronizes_block()
{
    using var host = TestHost.CreateWithManager(out var manager);
    await manager.FirstRecordAsync("ORDERS");
    await manager.NextRecordAsync("ORDERS");

    Assert.True(await host.NavigateBackAsync("ORDERS"));
    Assert.Equal(manager.GetBlock("ORDERS").CurrentRecordIndex,
        host.GetCurrentBlockRecordIndex("ORDERS"));
}
```

- [ ] **Step 2: Run the test and verify failure**

```powershell
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter FullyQualifiedName~WinFormFormHostHistoryTests
```

Expected: compile failure because history methods are absent.

- [ ] **Step 3: Implement history/bookmark delegation and navigation-bar commands**

Every navigation operation synchronizes the block after a successful engine result:

```csharp
public async Task<bool> NavigateBackAsync(string blockName)
{
    var result = await RequireManager().NavigateBackAsync(blockName);
    if (result) SyncBlockAndDetails(blockName);
    return result;
}
```

Add `BackClicked` and `ForwardClicked` events to the navigation bar and enable them from `CanNavigateBack` and `CanNavigateForward`. The history dialog displays engine `NavigationHistoryEntry` and `QueryHistoryEntry` objects and exposes clear commands.

- [ ] **Step 4: Run Increment 2 tests and commit**

```powershell
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter "FullyQualifiedName~WinFormFormHostLockSavepointTests|FullyQualifiedName~WinFormFormHostHistoryTests"
git add Beep.Winform.Data.Integrated.Controls/Forms Beep.Winform.Data.Integrated.Controls.Tests/Forms
git commit -m "feat(forms): add history and bookmarks"
```

## Increment 3 — Timers, Sequences, Record Groups, Parameters

### Task 7: Implement timers and sequences

**Files:**
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.RuntimeObjects.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormTimerPanel.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormSequencePanel.cs`
- Create: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostRuntimeObjectTests.cs`

- [ ] **Step 1: Write failing timer ownership test**

```csharp
[Fact]
public void Timer_is_created_by_engine_provider_and_relayed_by_host()
{
    var timers = new Mock<ITimerManager>();
    timers.Setup(x => x.CreateTimer("refresh", TimeSpan.FromMilliseconds(10), false))
        .Returns(new TimerDefinition
        {
            TimerName = "refresh",
            Interval = TimeSpan.FromMilliseconds(10)
        });
    using var host = TestHost.CreateWithProviders(timers.Object, out _);
    FormsHostTimerEventArgs? observed = null;
    host.TimerFired += (_, args) => observed = args;

    var timer = host.CreateTimer("refresh", TimeSpan.FromMilliseconds(10));
    timers.Raise(x => x.TimerFired += null,
        new TimerFiredEventArgs
        {
            TimerName = "refresh",
            FireCount = 1,
            FiredAt = DateTime.UtcNow
        });

    Assert.Equal("refresh", timer.TimerName);
    Assert.Equal("refresh", observed!.TimerName);
}
```

- [ ] **Step 2: Run test and verify failure**

```powershell
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter FullyQualifiedName~WinFormFormHostRuntimeObjectTests
```

Expected: compile failure because runtime object host methods are absent.

- [ ] **Step 3: Implement provider delegation and timer relay**

```csharp
public TimerDefinition CreateTimer(string timerName, TimeSpan interval, bool repeating = false) =>
    RequireManager().Timers.CreateTimer(timerName, interval, repeating);

public long GetNextSequence(string sequenceName) =>
    RequireManager().Sequences.GetNextSequence(sequenceName);

private void ManagerTimerFired(object? sender, TimerFiredEventArgs args) =>
    RunOnUi(() => TimerFired?.Invoke(
        this,
        new FormsHostTimerEventArgs(args.TimerName, args.FireCount, args.FiredAt)));
```

The timer panel never creates `System.Windows.Forms.Timer` or `System.Threading.Timer`.

- [ ] **Step 4: Run tests and commit**

```powershell
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter FullyQualifiedName~WinFormFormHostRuntimeObjectTests
git add Beep.Winform.Data.Integrated.Controls/Forms Beep.Winform.Data.Integrated.Controls.Tests/Forms
git commit -m "feat(forms): expose engine timers and sequences"
```

### Task 8: Implement record groups and parameter lists

**Files:**
- Modify: `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.RuntimeObjects.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormRecordGroupPanel.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormParameterListPanel.cs`
- Modify: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostRuntimeObjectTests.cs`

- [ ] **Step 1: Add failing datasource-boundary and parameter tests**

```csharp
[Fact]
public async Task Record_group_population_delegates_to_manager()
{
    using var host = TestHost.CreateWithManager(out var manager);
    host.CreateRecordGroup("OPEN_ORDERS", "Main", "Orders", []);

    Assert.True(await host.PopulateRecordGroupAsync("OPEN_ORDERS"));
    Assert.Same(manager.GetRecordGroup("OPEN_ORDERS"),
        host.GetRecordGroup("OPEN_ORDERS"));
}

[Fact]
public void Parameter_values_round_trip_as_objects()
{
    using var host = TestHost.CreateWithManager(out _);
    host.CreateParameterList("CALL_PARAMS");
    host.SetParameter("CALL_PARAMS", "CUSTOMER_ID", 42);
    Assert.Equal(42, host.GetParameter("CALL_PARAMS", "CUSTOMER_ID"));
}
```

- [ ] **Step 2: Run tests and verify failure**

Run the Task 7 test command. Expected: compile failure for missing record-group and parameter methods.

- [ ] **Step 3: Implement direct manager delegation**

```csharp
public void CreateRecordGroup(
    string name, string dataSourceName, string entityName, List<AppFilter>? filters = null) =>
    RequireManager().CreateRecordGroup(name, dataSourceName, entityName, filters);

public void SetParameter(string listName, string parameterName, object? value) =>
    RequireManager().AddParameter(listName, parameterName, value!);
```

Panels render returned models and do not reference datasource types beyond `AppFilter`.

- [ ] **Step 4: Run Increment 3 tests and commit**

```powershell
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter FullyQualifiedName~WinFormFormHostRuntimeObjectTests
git add Beep.Winform.Data.Integrated.Controls/Forms Beep.Winform.Data.Integrated.Controls.Tests/Forms
git commit -m "feat(forms): add record groups and parameter lists"
```

## Increment 4 — Multi-Form, State, Computed Values, Utilities

### Task 9: Implement multi-form calls, messaging, globals, and form factory

**Files:**
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.MultiForm.cs`
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FeatureControls/WinFormMultiFormPanel.cs`
- Create: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostMultiFormTests.cs`

- [ ] **Step 1: Write failing call-stack and form-message tests**

```csharp
[Fact]
public async Task Modal_form_call_uses_engine_then_registered_factory()
{
    using var host = TestHost.CreateWithManager(out _);
    var factory = new Mock<IWinFormFormsFactory>();
    host.FormFactory = factory.Object;

    Assert.True(await host.CallFormAsync(
        "ORDER_DETAILS",
        new Dictionary<string, object> { ["ORDER_ID"] = 10 }));

    factory.Verify(x => x.ShowModalAsync(
        "ORDER_DETAILS",
        It.IsAny<IReadOnlyDictionary<string, object>>(),
        It.IsAny<CancellationToken>()), Times.Once);
}

[Fact]
public void Form_message_is_relayed_without_interpretation()
{
    using var host = TestHost.CreateWithManager(out var manager);
    FormsHostFormMessageEventArgs? observed = null;
    host.FormMessageReceived += (_, args) => observed = args;

    manager.PostMessage(manager.CurrentFormName, "REFRESH", 10);

    Assert.Equal("REFRESH", observed!.MessageType);
    Assert.Equal(10, observed.Payload);
}
```

- [ ] **Step 2: Run tests and verify failure**

```powershell
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter FullyQualifiedName~WinFormFormHostMultiFormTests
```

Expected: compile failure because the form factory and host methods are absent.

- [ ] **Step 3: Add a WinForms-only factory contract**

```csharp
public interface IWinFormFormsFactory
{
    Task<bool> ShowModalAsync(
        string formName,
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken ct);
    Task<bool> ShowModelessAsync(
        string formName,
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken ct);
    Task<bool> ReplaceCurrentAsync(
        string formName,
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken ct);
    Task<bool> ReturnToCallerAsync(object? returnData, CancellationToken ct);
}
```

The factory belongs in `FormHost` because it constructs WinForms windows; it is not added to engine contracts.

- [ ] **Step 4: Implement engine-first multi-form commands**

Call the manager first so the Forms engine owns call-stack and parameter semantics. Invoke the factory only after engine success. If the factory fails, return `false` and raise an error notification; do not reconstruct the engine stack in WinForms.

- [ ] **Step 5: Run tests and commit**

```powershell
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter FullyQualifiedName~WinFormFormHostMultiFormTests
git add Beep.Winform.Data.Integrated.Controls/Forms Beep.Winform.Data.Integrated.Controls.Tests/Forms
git commit -m "feat(forms): add multi-form WinForms integration"
```

### Task 10: Implement form state, computed values, freeze, and utilities

**Files:**
- Create: `Beep.Winform.Data.Integrated.Controls/Forms/FormHost/WinFormFormHost.Utilities.cs`
- Modify: `Beep.Winform.Data.Integrated.Controls/Forms/BlockHost/WinFormBlockHost.AdvancedOperations.cs`
- Create: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormHostUtilityTests.cs`

- [ ] **Step 1: Write failing state synchronization and utility delegation tests**

```csharp
[Fact]
public async Task Restoring_form_state_synchronizes_every_registered_block()
{
    using var host = TestHost.CreateWithManager(out _);
    var snapshot = host.SaveFormState();

    Assert.True(await host.RestoreFormStateAsync(snapshot));
    Assert.All(host.RegisteredBlocks, block =>
        Assert.True(host.GetSyncCount(block.BlockName) > 0));
}

[Fact]
public void Computed_values_are_returned_without_ui_computation()
{
    using var host = TestHost.CreateWithManager(out var manager);
    manager.RegisterBlockComputed("ORDERS", "TOTAL", _ => 125m);
    Assert.Equal(125m, host.GetComputedValues("ORDERS")["TOTAL"]);
}
```

- [ ] **Step 2: Run tests and verify failure**

```powershell
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter FullyQualifiedName~WinFormFormHostUtilityTests
```

Expected: compile failure because utility methods are absent.

- [ ] **Step 3: Implement utility delegation**

Use one-line manager delegation for pure reads/writes. Synchronize registered blocks after:

- successful `RestoreFormStateAsync`;
- successful refresh;
- successful import;
- page navigation;
- revert current record;
- unfreeze after batched changes.

Representative implementation:

```csharp
public async Task<bool> RestoreFormStateAsync(
    FormStateSnapshot snapshot, CancellationToken ct = default)
{
    var result = await RequireManager().RestoreFormStateAsync(snapshot, ct);
    if (result)
        foreach (var block in RegisteredBlockViews)
            block.SyncFromManager();
    return result;
}

public IReadOnlyDictionary<string, object> GetComputedValues(string blockName) =>
    RequireManager().GetAllBlockComputedValues(blockName);
```

File dialog selection remains in WinForms; file content operations delegate through host text-I/O and import/export methods.

- [ ] **Step 4: Run utility tests and commit**

```powershell
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter FullyQualifiedName~WinFormFormHostUtilityTests
git add Beep.Winform.Data.Integrated.Controls/Forms Beep.Winform.Data.Integrated.Controls.Tests/Forms
git commit -m "feat(forms): expose form state and utility operations"
```

### Task 11: Add real FormsManager smoke coverage for every increment

**Files:**
- Modify: `Beep.Winform.Data.Integrated.Controls.Tests/Forms/WinFormFormsManagerIntegrationTests.cs`

- [ ] **Step 1: Add four integration tests**

```csharp
[Fact]
public Task Real_manager_executes_qbe_through_host() => StaTest.RunAsync(async () =>
{
    using var fixture = AdvancedFormsFixture.Create();
    var criteria = new Dictionary<string, QueryCriterion>
    {
        ["Name"] = new("Alice", QueryOperator.Equals)
    };

    Assert.True(await fixture.Host.ExecuteQueryByExampleAsync("EMP", criteria));
    Assert.Equal(1, fixture.Manager.QueryBuilder
        .BuildFilters("EMP", new Dictionary<string, object> { ["Name"] = "Alice" })
        .Count);
});

[Fact]
public Task Real_manager_rolls_back_named_savepoint_through_host() => StaTest.RunAsync(async () =>
{
    using var fixture = AdvancedFormsFixture.Create();
    fixture.Host.CreateSavepoint("EMP", "before-edit");
    fixture.Record.Name = "Bob";

    Assert.True(await fixture.Host.RollbackToSavepointAsync("EMP", "before-edit"));
    Assert.Equal("Alice", fixture.Record.Name);
});

[Fact]
public Task Real_manager_runtime_objects_round_trip_through_host() => StaTest.RunAsync(() =>
{
    using var fixture = AdvancedFormsFixture.Create();
    fixture.Host.CreateSequence("EMP_SEQ", 10, 5);
    fixture.Host.CreateParameterList("CALL_PARAMS");
    fixture.Host.SetParameter("CALL_PARAMS", "EMP_ID", 1);

    Assert.Equal(10, fixture.Host.GetNextSequence("EMP_SEQ"));
    Assert.Equal(1, fixture.Host.GetParameter("CALL_PARAMS", "EMP_ID"));
});

[Fact]
public Task Real_manager_restores_form_state_through_host() => StaTest.RunAsync(async () =>
{
    using var fixture = AdvancedFormsFixture.Create();
    var snapshot = fixture.Host.SaveFormState();
    fixture.Record.Name = "Changed";

    Assert.True(await fixture.Host.RestoreFormStateAsync(snapshot));
    Assert.Equal("Alice", fixture.Block.FindFieldPresenter("Name")!.Value);
});
```

Add this fixture below the tests so all four tests use the same real `FormsManager` setup:

```csharp
private sealed class AdvancedFormsFixture : IDisposable
{
    private AdvancedFormsFixture(
        FormsManager manager,
        WinFormFormHost host,
        WinFormBlockHost block,
        EmployeeRecord record)
    {
        Manager = manager;
        Host = host;
        Block = block;
        Record = record;
    }

    public FormsManager Manager { get; }
    public WinFormFormHost Host { get; }
    public WinFormBlockHost Block { get; }
    public EmployeeRecord Record { get; }

    public static AdvancedFormsFixture Create()
    {
        var record = new EmployeeRecord { Id = 1, Name = "Alice" };
        var editor = new Mock<IDMEEditor>();
        var uow = new Mock<IUnitofWork>();
        uow.SetupGet(x => x.CurrentItem).Returns(record);
        uow.SetupGet(x => x.TotalItemCount).Returns(1);
        uow.SetupProperty(x => x.Units, new List<EmployeeRecord> { record });

        var entity = new EntityStructure("EMPLOYEES")
        {
            Fields =
            [
                new EntityField { FieldName = "Id", Fieldtype = "int", IsReadOnly = true },
                new EntityField { FieldName = "Name", Fieldtype = "string" }
            ]
        };

        var manager = new FormsManager(editor.Object);
        manager.RegisterBlock("EMP", uow.Object, entity, "OracleConnection");
        var host = new WinFormFormHost { FormsManager = manager };
        var block = new WinFormBlockHost { BlockName = "EMP" };
        Assert.True(host.RegisterBlock(block));
        return new AdvancedFormsFixture(manager, host, block, record);
    }

    public void Dispose()
    {
        Block.Dispose();
        Host.Dispose();
        Manager.Dispose();
    }
}
```

- [ ] **Step 2: Run integration tests and verify failures before completing fixtures**

```powershell
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter FullyQualifiedName~WinFormFormsManagerIntegrationTests
```

Expected: the new tests fail at the first unsupported or incorrectly wired operation.

- [ ] **Step 3: Complete only missing integration wiring**

Fix host delegation, synchronization, or fixture setup revealed by the tests. Do not add data operations to controls.

- [ ] **Step 4: Run integration tests and commit**

```powershell
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj --filter FullyQualifiedName~WinFormFormsManagerIntegrationTests
git add Beep.Winform.Data.Integrated.Controls.Tests/Forms Beep.Winform.Data.Integrated.Controls/Forms
git commit -m "test(forms): cover advanced host integration"
```

### Task 12: Verify architecture, document coverage, and run the full suite

**Files:**
- Modify: `Beep.Winform.Data.Integrated.Controls/Forms/README.md`
- Replace: `Beep.Winform.Data.Integrated.Controls/Forms/ENGINE-GAP-ANALYSIS.md`

- [ ] **Step 1: Add an architecture guard test**

Add to `FormsHostAdvancedContractTests.cs`:

```csharp
[Fact]
public void Only_form_host_files_reference_manager_contract()
{
    var projectRoot = TestPaths.WinFormsControlsProject;
    var offenders = Directory.GetFiles(
            Path.Combine(projectRoot, "Forms"), "*.cs", SearchOption.AllDirectories)
        .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}FormHost{Path.DirectorySeparatorChar}"))
        .Where(path => File.ReadAllText(path).Contains("IUnitofWorksManager"))
        .ToArray();

    Assert.Empty(offenders);
}
```

- [ ] **Step 2: Replace the stale gap analysis**

Document each engine feature with:

- host method;
- WinForms surface;
- test class;
- status `Implemented`;
- explicit note that datasource access remains in FormsManager.

Do not retain claims from the deleted implementation.

- [ ] **Step 3: Run formatting and all tests**

```powershell
dotnet format Beep.Winform.Data.Integrated.Controls\TheTechIdea.Beep.Winform.Data.Integrated.csproj --verify-no-changes
dotnet test Beep.Winform.Data.Integrated.Controls.Tests\TheTechIdea.Beep.Winform.Data.Integrated.Tests.csproj
dotnet test ..\BeepDM\DataManagementModelsStandard.Tests\DataManagementModelsStandard.Tests.csproj
dotnet build Beep.Winform.Data.Integrated.Controls\TheTechIdea.Beep.Winform.Data.Integrated.csproj
dotnet build ..\BeepDM\DataManagementEngineStandard\DataManagementEngine.csproj
```

Expected:

- all WinForms tests pass;
- all model contract tests pass;
- both production projects build;
- only the already-known transitive SQLite advisory remains unless dependency remediation is separately authorized.

- [ ] **Step 4: Verify no forbidden dependencies escaped the form host**

```powershell
rg -n "IUnitofWorksManager|FormsManager|IDataSource" Beep.Winform.Data.Integrated.Controls\Forms -g "*.cs"
```

Expected:

- `IUnitofWorksManager` and direct `FormsManager` access appear only under `Forms/FormHost`;
- `IDataSource` does not appear in BlockHost, FieldHost, or FeatureControls;
- feature controls reference `IBeepFormsHost`.

- [ ] **Step 5: Commit documentation and verification guards**

```powershell
git add Beep.Winform.Data.Integrated.Controls/Forms/README.md Beep.Winform.Data.Integrated.Controls/Forms/ENGINE-GAP-ANALYSIS.md Beep.Winform.Data.Integrated.Controls.Tests
git commit -m "docs(forms): document advanced Oracle Forms parity"
```

## Completion Checklist

- [ ] Trigger events route only to matching registered blocks and detach on replacement/disposal.
- [ ] QBE criteria never mutate the current record and failed queries preserve criteria.
- [ ] LOV cancellation performs no mutation.
- [ ] Alerts and queued messages render engine severity without a duplicate severity model.
- [ ] Lock failure rejects edits and restores engine values.
- [ ] Savepoint rollback refreshes master and detail blocks.
- [ ] Navigation history and bookmarks are engine-owned.
- [ ] Timers run only in the engine provider.
- [ ] Sequences, record groups, and parameter lists delegate to the engine.
- [ ] Multi-form calls preserve engine stack and messaging semantics.
- [ ] State restore synchronizes every registered block.
- [ ] Computed values are displayed but never calculated by generic WinForms controls.
- [ ] Import/export, paging, text I/O, properties, transactions, and status delegate through the host.
- [ ] Only `WinFormFormHost` references `IUnitofWorksManager`.
- [ ] Full tests and production builds pass.
