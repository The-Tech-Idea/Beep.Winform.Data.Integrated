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

Tab, Shift+Tab, Enter, and Shift+Enter navigation is routed through the Forms
lifecycle rather than native control traversal. `WinFormBlockHost` validates the current item and
executes WHEN-VALIDATE-ITEM, POST-TEXT-ITEM, KEY-NEXT/PREV-ITEM,
PRE-TEXT-ITEM, and WHEN-NEW-ITEM-INSTANCE before changing focus. Failed
validation or triggers retain focus on the current presenter.

Oracle Forms keyboard commands are trigger-first: F4 duplicates a record, F6
creates a record, Shift+F6 deletes, F7 enters query mode, F8 executes query,
F9 opens the current item's LOV, F10 commits, Ctrl+R rolls back, Ctrl+Delete
clears the record, and Ctrl+Up/Down navigates records. The corresponding key
trigger must succeed before the host operation runs.

Advanced Oracle Forms surfaces are also available:

- `WinFormQueryPanel` for query-by-example and templates
- `WinFormLovDialog` for engine-loaded LOV selection
- `WinFormAlertProvider` for `SHOW_ALERT`
- `WinFormLockPanel` and `WinFormSavepointPanel`
- `WinFormHistoryDialog` for navigation and query history
- `WinFormTimerPanel` and `WinFormSequencePanel`
- `WinFormRecordGroupPanel` and `WinFormParameterListPanel`
- `WinFormMultiFormPanel` plus `IWinFormFormsFactory`
- `WinFormSecurityPanel` for security contexts, block/field policies, and violation inspection
- `WinFormAuditPanel` for audit logs, field history, export, and maintenance
- `WinFormUndoRedoPanel` and `WinFormDirtyStatePanel`
- `WinFormCrossBlockValidationPanel`
- `WinFormItemPropertyPanel` for Oracle Forms item properties, values, errors, and tab order

LOV return mappings translate the engine `__RETURN_VALUE__` sentinel back to
the invoking field. Related fields are populated only when the LOV definition
enables `AutoPopulateRelatedFields`. LOV combo presenters retain both the
engine return value and the user-facing display text after dialog selection.

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
