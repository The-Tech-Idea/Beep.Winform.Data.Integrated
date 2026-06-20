# WinForms Oracle Forms Host Design

## Objective

Implement the complete WinForms presentation layer for the BeepDM Oracle Forms engine. The implementation replaces the deleted WinForms Forms UI and completes the existing `WinFormFormHost`, `WinFormBlockHost`, and field-presenter skeletons.

The WinForms project is a pure UI adapter. `IUnitofWorksManager` and its Forms subsystems remain the sole owners of datasource access, entity metadata, records, relationships, modes, validation, triggers, LOV definitions, dirty state, and persistence.

## Architectural Boundary

The runtime dependency direction is:

```text
Beep WinForms controls implementing IBeepUIComponent
            |
IFieldPresenter implementations
            |
WinFormBlockHost : UserControl, IBlockView
            |
WinFormFormHost : UserControl, IBeepFormsHost
            |
IUnitofWorksManager
```

There is no WinForms-specific engine, repository, datasource abstraction, business model, built-in layer, bridge, or duplicate contract. All shared contracts remain in `BeepDM/DataManagementModelsStandard/Editor/Forms/Hosts`.

`WinFormFormHost` delegates directly to `IUnitofWorksManager`. `BeepFormsHostBridge` is not used in the WinForms runtime path.

## Existing Contracts

The implementation uses exactly these platform-neutral presentation contracts:

- `IBeepFormsHost`
- `IBlockView`
- `IFieldPresenter`

`IBlockNavigationBar` is an existing supporting view contract and is implemented by the WinForms navigation control. No new WinForms interfaces are introduced.

### Required Shared-Contract Completion

The current `IBeepFormsHost` contract is missing three operations required to keep blocks platform-neutral and prevent direct engine access:

```csharp
int GetCurrentBlockRecordIndex(string blockName);
object? GetFieldValue(string blockName, string fieldName);
bool SetFieldValue(string blockName, string fieldName, object? value);
```

These methods are added to the existing shared interface in BeepDM and implemented directly by `WinFormFormHost` using `IUnitofWorksManager` and engine helpers. They will also be available to future WPF and Blazor hosts.

No new interface is created. The contract is not expanded with datasource, connection, table, or unit-of-work objects.

## WinFormFormHost

`WinFormFormHost` is a designer-friendly `UserControl` implementing `IBeepFormsHost`.

### Responsibilities

- Hold the assigned `IUnitofWorksManager`.
- Maintain a case-insensitive registry of bound `IBlockView` instances.
- Enforce unique, non-empty block names.
- Track and publish the active block.
- Register, bind, unbind, and unregister block controls.
- Delegate all data, state, navigation, query, CRUD, LOV, and validation calls directly to `IUnitofWorksManager`.
- Refresh affected block views after successful engine operations.
- Refresh detail blocks after a master record changes.
- Marshal visual updates to the WinForms UI thread.
- Display informational, warning, and error notifications using Beep WinForms UI.
- Own default LOV dialog presentation while obtaining all LOV definitions and rows from the engine.
- Detach blocks and event subscriptions during disposal or manager replacement.

### Manager Assignment

Assigning `FormsManager`:

1. Unsubscribes from the previous manager and unbinds registered blocks.
2. Stores the new `IUnitofWorksManager`.
3. Rebinds eligible registered blocks.
4. Synchronizes all blocks from the new manager.

Assigning `null` leaves visual controls intact but unbound and disabled for engine operations.

### Block Registration

`RegisterBlock(object blockView)` accepts only an `IBlockView` whose `View` is a WinForms `Control`.

Registration fails explicitly when:

- the object does not implement `IBlockView`;
- the block name is empty;
- another instance already owns the same block name;
- the view is not a WinForms control.

Successful registration binds the block to the host when a manager is available. Registering a block does not create or configure an engine block; engine block definitions must already exist.

### Direct Engine Delegation

Every `IBeepFormsHost` operation validates its arguments and then calls the corresponding `IUnitofWorksManager` API or an existing engine subsystem. The host does not reproduce engine rules.

After a successful mutation or navigation:

- the target block synchronizes its current record, mode, count, and presenters;
- registered detail blocks returned by `GetDetailBlockNames` synchronize after a master navigation or query;
- active-block state remains unchanged unless the operation explicitly activates another block.

## WinFormBlockHost

`WinFormBlockHost` is a designer-friendly `UserControl` implementing `IBlockView`.

### Responsibilities

- Store block identity and designer configuration.
- Bind only through `IBeepFormsHost`.
- Maintain a case-insensitive presenter collection.
- Build an optional metadata-driven field layout.
- Allow manually placed designer controls/presenters to override generated fields.
- Synchronize current engine values into presenters.
- Forward user edits to the engine.
- Delegate navigation, query, insert, delete, save, rollback, clear, and duplicate commands to the form host.
- Represent query mode and normal record mode visually.
- Display field validation errors.
- Coordinate an optional `IBlockNavigationBar`.
- Relay engine trigger and unit-of-work activity through the existing contract events.
- Remove subscriptions and dispose only controls it created.

### Binding

`Bind(IBeepFormsHost formsHost)`:

1. Validates the host and block name.
2. Verifies that the engine block is registered.
3. Stores the host.
4. Discovers manually supplied presenters.
5. Reads field metadata from `GetBlockFields`.
6. Creates missing presenters when automatic generation is enabled.
7. Connects presenter and navigation events.
8. Synchronizes mode, current record, record count, and field values.

`Unbind()` detaches events, clears transient binding state, and leaves designer-owned controls in place.

### Field Generation

Automatic generation uses engine `EntityField` metadata and engine-side `FieldTypeMapper`. WinForms does not maintain an independent field-type policy.

The generated layout:

- orders fields using engine metadata or entity ordinal position;
- skips hidden fields;
- uses engine captions/descriptions for labels and prompts;
- applies required, read-only, queryable, maximum-length, and format metadata;
- assigns block and field identifiers to the underlying `IBeepUIComponent`;
- uses Beep controls rather than standard WinForms editor controls.

If a manually registered presenter has the same field name, it takes precedence and no generated control is created.

### Synchronization

`SyncFromManager()` reads:

- block mode;
- current record;
- current record index;
- record count;
- query permissions;
- item properties;
- dirty state.

Record values are read and written using the engine `RecordPropertyAccessor`. The block does not use raw reflection, `DataTable`, `DataView`, or `DataRowView`.

A synchronization guard prevents engine-to-control updates from being interpreted as user edits.

### User Edits

When a presenter raises `ValueChanged`:

1. Ignore the event while synchronization is active.
2. Resolve the current record and field through the host.
3. Convert and assign the value through `IBeepFormsHost.SetFieldValue`.
4. Execute engine field validation and trigger flow.
5. Set or clear the presenter's validation error.
6. Resynchronize dependent or computed fields when required.

The presenter never accesses a datasource, entity collection, unit of work, or `IUnitofWorksManager`.

### Query Mode

Entering query mode delegates to the host and then:

- clears presenter values without changing datasource records;
- enables only queryable fields;
- disables mutation commands;
- marks the navigation bar as query mode.

Executing a query delegates criteria interpretation and execution to the engine. On success, the block exits query mode and synchronizes returned records. On failure, criteria remain visible.

Exiting query mode delegates to the host and restores current engine record values.

## Field Presenters

All field presenters directly implement `IFieldPresenter` and wrap a Beep control implementing `IBeepUIComponent`.

### Shared Presenter Base

An abstract WinForms presenter base centralizes:

- `EntityField` metadata;
- `IBeepUIComponent` ownership;
- conversion between control and engine values;
- synchronization guards;
- visibility, enabled, read-only, required, prompt, and validation state;
- subscription to `OnValueChanged`, `OnValidate`, and disposal;
- safe access to the underlying WinForms `Control`.

The base is a class, not a new interface.

### Required Presenter Implementations

- Text: `BeepTextBox`
- Numeric: the appropriate Beep numeric editor, with a text-based numeric fallback
- Date/time: `BeepDatePicker` or the applicable Beep date/time editor
- Boolean: `BeepCheckBox`
- Combo/LOV: `BeepComboBox`, with LOV data loaded through the host
- Reflective fallback: wraps another `IBeepUIComponent` selected by engine template metadata

The initial text skeleton is corrected to `WinFormTextBoxFieldPresenter`; compatibility with its current misspelled filename is handled by renaming the file without introducing a duplicate type.

### Presenter Registry

A concrete registry class, not an interface, contains presenter factories ordered from specific to fallback. It uses:

1. engine template identifier when supplied;
2. engine `FieldTypeMapper` category;
3. fallback presenter.

Consumers may add or replace factory registrations through concrete configuration methods without changing shared contracts.

## Navigation Bar

A designer-friendly WinForms control implements `IBlockNavigationBar`.

It uses Beep buttons and a Beep-compatible record-position editor to expose:

- first;
- previous;
- next;
- last;
- direct record index.

The block subscribes to navigation events and delegates them through `IBeepFormsHost`. The navigation bar contains no record collection and no engine reference.

## LOV Behavior

The engine owns LOV definitions, filtering, rows, mappings, and validation.

The WinForms host:

1. requests the `LOVDefinition` and `LOVResult`;
2. renders the result in a Beep WinForms dialog/list or grid;
3. returns the selected engine row;
4. asks the engine for related field mappings;
5. applies mapped values through the block's normal engine field-update path;
6. synchronizes affected presenters.

Cancellation is not an error and leaves record values unchanged.

## Master-Detail Behavior

The engine owns relationships and detail filtering.

After successful master navigation, query, insert, delete, save, rollback, or current-record change, `WinFormFormHost` obtains detail block names from the engine and calls `SyncFromManager()` on registered detail views. The WinForms layer does not calculate relationship filters or copy foreign keys independently.

## Validation and Error Display

Field-level validation errors are shown on the presenter using the Beep control's validation and tooltip facilities.

Record-level validation:

- is requested from the engine;
- maps returned field errors to matching presenters;
- focuses the first invalid visible field;
- displays non-field errors through the host notification surface.

WinForms does not duplicate required, range, pattern, lookup, uniqueness, or cross-block validation rules.

## Notifications and Error Policy

Expected engine-operation failures:

- return `false` or the engine failure result;
- display an appropriate host notification;
- retain the current visual state unless the engine has already changed state;
- resynchronize when engine state may have changed.

Cancellation is handled without an error notification.

Programmer and configuration errors fail explicitly with argument or invalid-operation exceptions. Examples include duplicate block names, a non-WinForms view, missing block identity, and unsupported manually supplied controls.

Severity classification uses the engine `MessageClassifier`. The WinForms layer does not define a parallel severity model.

## Threading

Engine calls may complete on worker threads. All access to WinForms controls is marshalled through the owning control using `InvokeRequired` and `BeginInvoke`/`Invoke`.

The implementation:

- never blocks the UI thread on incomplete tasks;
- disables or gates repeated commands while an operation is running;
- honors cancellation tokens exposed by contracts;
- checks disposal before posting UI work.

## Lifecycle and Ownership

- Designer-created controls are owned by their parent form and are never disposed by presenter removal.
- Auto-generated controls and presenters are owned and disposed by `WinFormBlockHost`.
- Host manager replacement detaches previous subscriptions.
- Block unregistration calls `Unbind`.
- Disposal is idempotent.
- Event handlers use named methods where unsubscription is required.

## File Organization

The implementation remains under:

```text
Forms/
  FormHost/
    WinFormFormHost.cs
    WinFormFormHost.Blocks.cs
    WinFormFormHost.Engine.cs
    WinFormFormHost.Lov.cs
    WinFormFormHost.Notifications.cs
    WinFormFormHost.Threading.cs
  BlockHost/
    WinFormBlockHost.cs
    WinFormBlockHost.Binding.cs
    WinFormBlockHost.Fields.cs
    WinFormBlockHost.Operations.cs
    WinFormBlockHost.QueryMode.cs
    WinFormBlockHost.Validation.cs
    WinFormBlockNavigationBar.cs
  FieldHost/
    WinFormFieldPresenterBase.cs
    WinFormTextBoxFieldPresenter.cs
    WinFormNumericFieldPresenter.cs
    WinFormDateFieldPresenter.cs
    WinFormBooleanFieldPresenter.cs
    WinFormComboFieldPresenter.cs
    WinFormReflectiveFieldPresenter.cs
    WinFormFieldPresenterRegistry.cs
```

Partial classes keep host and block responsibilities small without adding interfaces.

## Testing Strategy

A dedicated WinForms Forms test project will test behavior against fake or mocked shared contracts and engine collaborators without connecting to a real datasource.

Required tests:

- form host accepts valid blocks and rejects invalid or duplicate registrations;
- active-block changes raise exactly one event;
- assigning and replacing `FormsManager` binds and unbinds correctly;
- all host operations call the intended engine operation;
- block binding creates presenters from engine metadata;
- manually supplied presenters override generated presenters;
- engine-to-presenter synchronization does not write values back;
- user edits write once through engine field APIs and execute validation;
- query mode changes field editability and preserves failed criteria;
- navigation refreshes current record and detail blocks;
- CRUD, save, rollback, clear, and duplicate operations refresh state correctly;
- LOV selection applies engine-provided field mappings;
- validation errors appear on matching presenters;
- UI work is marshalled to the control thread;
- unbind and disposal remove handlers and dispose only owned controls.

An integration smoke test will instantiate a real `FormsManager`, register an in-memory block, host it in `WinFormFormHost`, generate fields, navigate records, edit a value, validate, save, and rollback.

## Delivery Phases

Implementation is delivered in dependency order:

1. Correct skeleton types and implement direct form-host delegation.
2. Implement block registration, binding, synchronization, and lifecycle.
3. Implement presenter base, registry, and six Beep presenters.
4. Implement navigation and core CRUD/query operations.
5. Implement validation and LOV presentation.
6. Implement master-detail refresh, event relays, threading, and disposal.
7. Add unit and integration tests, then run full project verification.

Each phase must compile and include focused tests before proceeding.

## Acceptance Criteria

The implementation is complete when:

- no `NotImplementedException` remains in the Forms WinForms layer;
- all UI contracts are implemented directly by the WinForms types;
- no new WinForms interfaces or datasource/business layers exist;
- no WinForms class except `WinFormFormHost` accesses `IUnitofWorksManager`;
- blocks obtain all data and operations through `IBeepFormsHost`;
- presenters wrap `IBeepUIComponent` controls and contain no engine or datasource access;
- metadata-driven and designer-supplied fields both work;
- navigation, query mode, CRUD, LOV, validation, save, rollback, and master-detail synchronization work through the Forms engine;
- tests pass and the WinForms integration project builds on `net10.0-windows`.
