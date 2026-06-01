# BeepFormsToolbar

`BeepFormsToolbar` is the visual savepoint/alert surface for the fresh-start integrated forms path. It is separate from `BeepForms` by design.

## Architecture note

- `BeepForms` is the non-visual coordinator/host for `BeepBlock` instances and `FormsManager` state.
- `BeepFormsHeader` is the companion title/context surface.
- `BeepFormsCommandBar` is the companion block-selection and sync surface.
- `BeepFormsPersistenceShelf` is the companion commit/rollback surface.
- `BeepFormsToolbar` is the visual toolbar that surfaces savepoint and alert actions.
- `BeepFormsStatusStrip` is the companion read-only strip that renders shared workflow/status state.
- Workflow execution still belongs to `FormsManager`; the toolbar only calls `BeepForms` wrappers.

## Current structure

- `BeepFormsToolbar.cs`: BaseControl host, host binding, serialized toolbar composition, and popup button layout
- `BeepFormsToolbar.Actions.cs`: savepoint/alert handlers that delegate prompt, picker, and savepoint-list UI through the shared integrated dialog helper

## Current responsibilities

- Bind to a `BeepForms` host through `FormsHost`
- Auto-discover a nearby host when `AutoBindFormsHost` is enabled
- Expose savepoint and alert popup actions with designer-configurable composition
- Keep savepoint and alert execution in `FormsManager` through `BeepForms` wrappers
- Use Beep controls for toolbar buttons and the shared integrated dialog surface used for prompts, pickers, lists, and alerts

## Current gaps

- The toolbar intentionally covers savepoints and alert presets only; broader host commands now belong in `BeepFormsCommandBar`, `BeepFormsQueryShelf`, and `BeepFormsPersistenceShelf`
- Workflow feedback is still published into `BeepForms` view state so the separate header and strip surfaces can keep rendering in sync without moving workflow ownership out of `FormsManager`
- Future toolbar-driven workflow prompts should reuse the same shared dialog helper instead of constructing new modal forms inline