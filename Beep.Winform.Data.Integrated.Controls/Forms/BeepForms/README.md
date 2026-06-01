# BeepForms

`BeepForms` is the fresh-start non-visual form coordinator for the integrated controls path. It hosts one or more `IBeepBlockView` instances, keeps view state synchronized from `FormsManager`, and treats `IUnitofWorksManager` as the runtime authority.

`BeepFormsHeader` is now the separate visual title/context surface, `BeepFormsCommandBar` is the separate block-selection/sync surface, `BeepFormsQueryShelf` is the separate query-mode surface, `BeepFormsPersistenceShelf` is the separate commit/rollback surface, `BeepFormsToolbar` is the separate savepoint/alert action surface, and `BeepFormsStatusStrip` is the shared-state surface for status/message/workflow history lines. `BeepForms` itself should not own shell chrome.

Visible shell surfaces should use Beep controls by default. Standard WinForms controls are only fallback helpers for layout cases where no Beep equivalent exists.

## Current structure

- `BeepForms.cs`: root control state and block registration
- `BeepForms.Layout.cs`: non-visual block host layout only
- `BeepForms.Commands.cs`: form and block command routing wrappers
- `BeepForms.Navigation.cs`: record navigation wrappers
- `BeepForms.Messages.cs`: message publishing and severity mapping helpers for shared form view state
- `BeepForms.Events.cs`: manager event subscriptions for field changes, block messages, form messages, and error/warning propagation
- `BeepForms.TriggerProxy.cs`: manager-trigger and block-UoW event proxying for hosted blocks
- `BeepForms.MasterDetail.cs`: manager-driven master/detail context and coordinated detail refresh messaging helpers
- `BeepForms.WorkflowShell.cs`: savepoint and alert wrappers that delegate to `FormsManager`/provider services and publish workflow state for separate visual surfaces
- `../BeepFormsHeader/`: extracted title/context header control over shared host metadata
- `../BeepFormsCommandBar/`: extracted form-level command surface for block switching and sync
- `../BeepFormsQueryShelf/`: extracted query-mode command surface for entering and executing query mode, including selectable caption variants
- `../BeepFormsPersistenceShelf/`: extracted commit/rollback surface over shared dirty-state awareness
- `../BeepFormsToolbar/`: extracted savepoint/alert toolbar control and designer-time composition surface
- `../BeepFormsStatusStrip/`: extracted status/message strip that renders shared `BeepFormsViewState` with designer-configurable line presets and a compact rolling workflow history row

## Current responsibilities

- Maintain shared form view state for active block, status text, dirty state, current message, and active coordination/workflow/savepoint/alert context
- Host block controls and keep them synchronized from `FormsManager`
- Materialize definition-owned `BeepBlock` controls from `BeepFormsDefinition`
- Route form-level commands through `IBeepFormsCommandRouter`
- Reflect manager block messages, form status-area messages, and error log events in shared view state instead of owning built-in shell chrome
- Proxy manager trigger statistics, trigger lifecycle events, and normalized block-level UoW activity through `IBeepFormsHost` so hosted blocks can observe runtime workflow without dereferencing `FormsManager`
- Consume the current `IUnitofWorksManager` surface directly for form messages, relationships, LOV actions, field updates, and alert workflows so the host stays aligned with the local `FormsManager` implementation
- Reflect manager-owned master/detail context, trigger-chain completion, rollback outcomes, savepoint outcomes, and alert outcomes in shared state so separate surfaces can render them without moving workflow ownership out of `FormsManager`
- Keep a bounded workflow history in shared form view state so shell surfaces can show recent trigger-chain and rollback activity without promoting `BeepForms` into a full visual shell
- Preserve cancellation/error intent when commands fail so trigger cancellations stay warnings while true failures remain errors
- Fall back to a shared integrated dialog surface for savepoint prompts, pickers, list dialogs, and workflow alerts when the manager does not provide its own alert UI
- Stay focused on block coordination and manager bridging instead of top-level shell chrome
- Leave title/context rendering to `BeepFormsHeader`, block-selection/sync rendering to `BeepFormsCommandBar`, query-mode rendering to `BeepFormsQueryShelf`, commit/rollback rendering to `BeepFormsPersistenceShelf`, savepoint/alert rendering to `BeepFormsToolbar`, and workflow state rendering to `BeepFormsStatusStrip`
- Expose the root `Definition` through a designer-friendly modal editor and a small smart-tag on `BeepForms` itself so definition-first setup no longer depends on raw object graph editing only
- Feed nearby `BeepBlock` instances typed design-time block-name suggestions from the host definition so manual block registration does not rely on free-text matching alone

## Current gaps

- `BeepFormsToolbar` currently covers savepoints and alert presets only; future workflow shelves should follow the same extracted-surface pattern instead of moving shell chrome back into `BeepForms`
- Typed design-time support now exists for `BeepForms.Definition`, `BeepBlock.Definition`, `BeepBlockDefinition.Navigation`, block-name selection against nearby host definitions, field editor-key selection, and key/value metadata dictionaries
- Future workflow shelves should reuse the same shared integrated dialog helper instead of introducing new ad hoc modal forms