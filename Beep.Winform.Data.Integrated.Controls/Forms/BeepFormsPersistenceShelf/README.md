# BeepFormsPersistenceShelf

`BeepFormsPersistenceShelf` is the standalone commit/rollback surface for the fresh-start integrated forms path. It keeps persistence actions out of the general host command bar.

## Responsibilities

- Bind to a `BeepForms` host through `FormsHost`
- Auto-discover a nearby host when `AutoBindFormsHost` is enabled
- Surface commit and rollback actions through existing `BeepForms` wrappers
- Enable persistence actions only when the shared host state reports pending changes
- Expose a design-time smart-tag so button composition and flow direction can be preset

## Notes

- `BeepFormsCommandBar` owns block switching and sync
- `BeepFormsQueryShelf` owns query-mode entry, execution, and caption variants
- `BeepFormsToolbar` owns savepoint and alert actions
- `BeepFormsStatusStrip` owns shared workflow/status rendering