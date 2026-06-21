# WinForms Oracle Forms Engine Coverage

The removed legacy implementation has been replaced by a thin WinForms layer
over the BeepDM Forms engine.

| Feature | Host/API | WinForms surface | Status |
|---|---|---|---|
| Block lifecycle and active block | `WinFormFormHost` | `WinFormBlockHost` | Implemented |
| Metadata fields and editing | `IBeepFormsHost` field access | Beep field presenters | Implemented |
| Navigation and CRUD | host navigation/CRUD methods | block and navigation bar | Implemented |
| Master-detail refresh | engine relationships | automatic host synchronization | Implemented |
| Triggers and key triggers | trigger host methods/events | block trigger relay | Implemented |
| Query by example | engine `QueryBuilder` | `WinFormQueryPanel` | Implemented |
| Query templates/history | engine query APIs | query panel/history dialog | Implemented |
| LOV | engine LOV manager | `WinFormLovDialog` | Implemented |
| Messages and alerts | engine message/alert APIs | host events and `WinFormAlertProvider` | Implemented |
| Record locking | engine lock manager | `WinFormLockPanel` and lock-on-edit | Implemented |
| Savepoints | engine savepoint manager | `WinFormSavepointPanel` | Implemented |
| Navigation history/bookmarks | engine history/bookmarks | `WinFormHistoryDialog` and host commands | Implemented |
| Timers | engine timer manager | `WinFormTimerPanel` and timer relay | Implemented |
| Sequences | engine sequence provider | `WinFormSequencePanel` | Implemented |
| Record groups | engine registry | `WinFormRecordGroupPanel` | Implemented |
| Parameter lists | engine parameter manager | `WinFormParameterListPanel` | Implemented |
| Multi-form calls/messages/globals | engine registry/message bus | `WinFormMultiFormPanel` and factory | Implemented |
| Form state | engine snapshot APIs | host commands | Implemented |
| Computed values/freeze/batch | engine block APIs | host commands | Implemented |
| Revert/refresh/change log | engine data operations | host commands | Implemented |
| Aggregates | engine loaded/source aggregates | host commands | Implemented |
| Import/export | engine stream APIs | host commands | Implemented |
| Virtual paging | engine paging APIs | host commands | Implemented |
| TEXT_IO | engine file APIs | host commands | Implemented |
| Client/application properties | engine property APIs | host commands | Implemented |
| Transactions/post/status | engine form APIs | host commands | Implemented |
| Runtime security and field masking | engine security/item-property APIs | `WinFormSecurityPanel` and presenter synchronization | Implemented |
| Audit trail and field history | engine audit manager | `WinFormAuditPanel` | Implemented |
| Undo/redo and change summaries | engine data operations | `WinFormUndoRedoPanel` | Implemented |
| Cross-block validation | engine validation rules | `WinFormCrossBlockValidationPanel` | Implemented |
| Item properties, values, errors, and tab order | engine item-property manager | `WinFormItemPropertyPanel` and presenter synchronization | Implemented |
| Dirty-state save/rollback workflows | engine dirty-state manager | `WinFormDirtyStatePanel` | Implemented |

## Boundary verification

- Only files under `Forms/FormHost` reference `IUnitofWorksManager`.
- `WinFormBlockHost` and feature controls depend on `IBeepFormsHost`.
- Field presenters wrap controls implementing `IBeepUIComponent`.
- No WinForms class loads tables or calls `IDataSource`.
- Query building, triggers, validation, locks, savepoints, timers, sequences,
  relationships, persistence, and transactions remain in the engine.

Application code still supplies the mapping from a logical engine form name to
an actual WinForms window through `IWinFormFormsFactory`. This is platform
construction, not Forms business logic.
