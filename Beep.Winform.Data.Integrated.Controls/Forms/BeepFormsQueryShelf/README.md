# BeepFormsQueryShelf

`BeepFormsQueryShelf` is the standalone query-mode action surface for the fresh-start integrated forms path. It keeps query entry and execution separate from the general host command bar.

## Responsibilities

- Bind to a `BeepForms` host through `FormsHost`
- Auto-discover a nearby host when `AutoBindFormsHost` is enabled
- Show an in-shelf caption that can render title-only, target-only, or target-plus-mode variants
- Surface query-mode entry and query execution actions through existing `BeepForms` wrappers
- Expose a design-time smart-tag so query button composition, caption visibility, caption mode, and flow direction can be preset

## Notes

- `BeepFormsCommandBar` owns block switching and sync
- `BeepFormsPersistenceShelf` owns commit and rollback
- `BeepFormsToolbar` owns savepoint and alert actions
- `BeepFormsStatusStrip` owns shared workflow/status rendering