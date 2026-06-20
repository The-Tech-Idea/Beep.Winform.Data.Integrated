# WinForms Oracle Forms Advanced Parity Design

## Objective

Extend the completed WinForms Forms host from core record editing into advanced Oracle Forms parity while preserving the platform boundary:

```text
WinForms feature controls
  -> IBeepFormsHost
  -> IUnitofWorksManager
```

WinForms renders state, collects user input, forwards commands, and displays engine results. It does not implement triggers, locking, savepoints, query parsing, timers, sequences, relationships, persistence, or inter-form coordination.

## Shared Contract Strategy

The existing `IBeepFormsHost` remains the single platform-neutral facade used by block controls. It gains grouped methods for features that directly affect a hosted form or block.

The contract must expose models and primitive values, not manager interfaces. `WinFormBlockHost` must not receive `IUnitofWorksManager`, `ITriggerManager`, `ILockManager`, `ISavepointManager`, or datasource objects.

Feature-specific methods are grouped into partial implementation files on `WinFormFormHost`; no new WinForms interfaces are created.

## Increment 1: Triggers, QBE, LOV, Alerts

### Trigger Relay

`WinFormFormHost` subscribes to `FormsManager.Triggers` events and routes block-scoped events to matching registered `IBlockView` instances.

`IBlockView` already defines:

- `TriggerExecuting`
- `TriggerExecuted`
- `TriggerRegistered`
- `TriggerUnregistered`
- `UnitOfWorkActivity`

`WinFormBlockHost` raises those events without executing trigger business logic.

The host also exposes:

- query trigger definitions by scope;
- fire named form and key triggers;
- enable, disable, suspend, and resume triggers;
- trigger statistics.

### Query by Example

Query mode stores criteria separately from current record values. Each field presenter supplies:

- query value;
- `QueryOperator`;
- query enabled state.

`WinFormBlockHost` collects criteria into a dictionary. `WinFormFormHost` delegates filter construction to `FormsManager.QueryBuilder.BuildFilters` and query execution to `FormsManager.ExecuteQueryAsync`.

The WinForms layer provides:

- operator selector per field;
- execute, cancel, and clear criteria commands;
- query-template save/load/delete UI;
- query-history browser.

Failed queries preserve criteria. Successful queries clear criteria and synchronize the block.

### LOV Picker

The current engine LOV load path remains authoritative. A Beep dialog displays:

- search input;
- engine-defined columns;
- engine-returned rows;
- single or multiple selection according to `LOVDefinition`.

Selection applies values returned by `GetLovRelatedFieldValues`. Cancellation performs no mutation.

### Alerts and Messages

Engine `SetMessage`, message queue events, and `ShowAlertAsync` are rendered through Beep notification/dialog controls.

The host maps engine severity using `MessageClassifier`. It does not create another severity model.

## Increment 2: Locks, Savepoints, History, Bookmarks

### Locking

The host exposes current lock state and delegates:

- lock/unlock current record;
- unlock all;
- lock mode;
- lock-on-edit;
- all lock records.

`WinFormBlockHost` shows lock status and optionally auto-locks on the first edit by calling the host. Lock failure cancels the edit and restores the engine value.

### Savepoints

A Beep savepoint panel supports:

- create named savepoint;
- list savepoints;
- rollback;
- release one;
- release all.

All snapshot and rollback semantics remain in `FormsManager.Savepoints`.

### Navigation and Query History

The navigation bar adds back/forward commands using engine navigation history. Separate history dialogs display navigation and query entries and allow clearing them.

### Bookmarks

The block command surface supports named bookmark creation, navigation, removal, and clearing. Bookmark storage remains in the engine.

## Increment 3: Timers, Sequences, Record Groups, Parameters

### Timers

The host delegates timer creation, deletion, pause/resume, and inspection to `ITimerManager`. Timer expiration is relayed to WinForms and then through block/form events.

WinForms does not own timer scheduling.

### Sequences

A sequence panel delegates sequence creation, next/current value, reset, and removal to the engine sequence provider.

Sequences used for record defaults remain engine-controlled; the UI panel is an administrative/runtime view.

### Record Groups

A record-group browser/editor supports:

- create from datasource/entity/filter metadata;
- populate;
- inspect rows;
- remove;
- clear all.

Datasource access remains exclusively inside `FormsManager`.

### Parameter Lists

A parameter-list editor supports create, add/update/remove parameter, inspect lists, clear, and destroy. Values are passed as objects and rendered through type-aware Beep editors.

## Increment 4: Multi-Form, State, Computed and Utility Features

### Multi-Form

The host delegates:

- call modal form;
- open modeless form;
- replace with new form;
- return to caller;
- post and broadcast messages;
- globals and form parameters.

Actual Form creation is provided by a WinForms form factory registered on `WinFormFormHost`. The engine remains responsible for call stack and message semantics.

### Form State

WinForms exposes save/restore commands over engine `FormStateSnapshot`. File persistence, when requested, serializes the engine snapshot; the UI does not reconstruct state independently.

### Computed Columns and Freeze

Computed values are read from the engine and displayed as read-only presenters. Computation delegates are registered by application code, not authored inside generic WinForms controls.

Freeze and batch-update commands delegate directly to engine methods and temporarily suspend visual refresh until unfreeze.

### Utility Surfaces

Additional thin commands cover:

- revert record;
- refresh block and conflict mode;
- change summaries and detailed change log;
- source aggregates;
- import/export;
- virtual paging;
- TEXT_IO file dialogs;
- client and application properties;
- form transactions;
- block status.

## Component Organization

```text
Forms/
  FormHost/
    WinFormFormHost.Triggers.cs
    WinFormFormHost.Query.cs
    WinFormFormHost.Alerts.cs
    WinFormFormHost.Locks.cs
    WinFormFormHost.Savepoints.cs
    WinFormFormHost.History.cs
    WinFormFormHost.RuntimeObjects.cs
    WinFormFormHost.MultiForm.cs
    WinFormFormHost.Utilities.cs
  BlockHost/
    WinFormBlockHost.Triggers.cs
    WinFormBlockHost.Query.cs
    WinFormBlockHost.Locking.cs
    WinFormBlockHost.AdvancedOperations.cs
  FeatureControls/
    WinFormLovDialog.cs
    WinFormQueryPanel.cs
    WinFormLockPanel.cs
    WinFormSavepointPanel.cs
    WinFormHistoryDialog.cs
    WinFormTimerPanel.cs
    WinFormSequencePanel.cs
    WinFormRecordGroupPanel.cs
    WinFormParameterListPanel.cs
    WinFormMultiFormPanel.cs
```

Feature controls depend only on `IBeepFormsHost` and engine model types.

## Error and Threading Policy

- Engine cancellation is neutral and does not display an error.
- Engine failures preserve current visual state and show an engine-classified message.
- Configuration errors fail explicitly.
- All engine event callbacks marshal to the owning WinForms control.
- Long-running engine calls remain asynchronous.
- Repeated commands are gated while an operation is active.
- Event subscriptions are detached during manager replacement and disposal.

## Testing

Each increment adds:

- host delegation tests;
- event relay tests;
- feature-control tests using mocked `IBeepFormsHost`;
- real `FormsManager` integration smoke tests;
- lifecycle tests for manager replacement and disposal.

Critical scenarios include:

- QBE operators produce engine-built filters and failed queries retain criteria;
- LOV cancellation is non-mutating;
- trigger events reach only the matching block;
- lock failure rejects edits;
- savepoint rollback refreshes master and details;
- timers do not run in the UI layer;
- record groups never call a datasource from WinForms;
- multi-form calls preserve engine call-stack behavior;
- state restore synchronizes every registered block.

## Acceptance Criteria

Advanced parity is complete when:

- every engine feature listed in this design has a WinForms command or display surface where a UI is meaningful;
- all operations delegate through `IBeepFormsHost`;
- only `WinFormFormHost` references `IUnitofWorksManager`;
- no WinForms datasource or business logic is introduced;
- engine trigger/message/timer events are relayed and unsubscribed correctly;
- all tests and production builds pass.
