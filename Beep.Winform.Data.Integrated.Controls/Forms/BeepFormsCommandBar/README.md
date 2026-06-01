# BeepFormsCommandBar

`BeepFormsCommandBar` is the standalone block-selection and sync surface for the fresh-start integrated forms path. It sits above per-block navigation and excludes query-mode and persistence actions.

## Responsibilities

- Bind to a `BeepForms` host through `FormsHost`
- Auto-discover a nearby host when `AutoBindFormsHost` is enabled
- Switch the active hosted block from a block selector popup
- Surface sync without mixing in query or persistence actions
- Expose a design-time smart-tag so button composition and flow direction can be preset like the toolbar
- Stay complementary to `BeepBlockNavigationBar` rather than duplicating per-record navigation

## Notes

- `BeepFormsHeader` owns title/context rendering
- `BeepFormsQueryShelf` owns query-mode entry, execution, and caption variants
- `BeepFormsPersistenceShelf` owns commit and rollback
- `BeepFormsToolbar` owns savepoint and alert actions
- `BeepFormsStatusStrip` owns shared workflow/status rendering
- `BeepBlockNavigationBar` remains the per-block record navigation surface