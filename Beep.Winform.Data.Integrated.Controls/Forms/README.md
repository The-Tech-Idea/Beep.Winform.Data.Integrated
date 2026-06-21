# WinForms Oracle Forms Host

This folder implements the WinForms presentation layer for the BeepDM Forms engine.

```csharp
var formsManager = new FormsManager(editor);
await formsManager.SetupBlockAsync(
    "EMP",
    "OracleConnection",
    "EMPLOYEES");

var host = new WinFormFormHost
{
    Dock = DockStyle.Fill,
    FormsManager = formsManager
};

var employees = new WinFormBlockHost
{
    BlockName = "EMP",
    Dock = DockStyle.Fill,
    AutoGenerateFields = true
};

host.Controls.Add(employees);
host.RegisterBlock(employees);
```

The engine block must exist before the UI block binds. Create it through `FormsManager.SetupBlockAsync` or `FormsManager.RegisterBlock`.

Architecture:

```text
IBeepUIComponent controls
  -> IFieldPresenter
  -> WinFormBlockHost / IBlockView
  -> WinFormFormHost / IBeepFormsHost
  -> IUnitofWorksManager
```

Only `WinFormFormHost` accesses `IUnitofWorksManager`. Blocks use `IBeepFormsHost`; presenters only wrap Beep controls.

Advanced Oracle Forms surfaces are also available:

- `WinFormQueryPanel` for query-by-example and templates
- `WinFormLovDialog` for engine-loaded LOV selection
- `WinFormAlertProvider` for `SHOW_ALERT`
- `WinFormLockPanel` and `WinFormSavepointPanel`
- `WinFormHistoryDialog` for navigation and query history
- `WinFormTimerPanel` and `WinFormSequencePanel`
- `WinFormRecordGroupPanel` and `WinFormParameterListPanel`
- `WinFormMultiFormPanel` plus `IWinFormFormsFactory`

The host also delegates form state, computed values, freeze/batch updates,
revert/refresh, change logs, aggregates, import/export, virtual paging,
TEXT_IO, client/application properties, form transactions, posting, and block
status.

For alerts, construct the engine with the WinForms provider:

```csharp
var alertProvider = new WinFormAlertProvider(() => FindForm());
var formsManager = new FormsManager(editor, alertProvider: alertProvider);
```

For multi-form operations, assign an application-specific factory that maps
logical engine form names to WinForms windows:

```csharp
host.FormFactory = formsFactory;
```
