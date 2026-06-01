# BeepFormsHeader

`BeepFormsHeader` is the standalone title/context surface for the fresh-start integrated forms path. It keeps form title and active-context rendering out of `BeepForms` so the coordinator stays non-visual.

## Responsibilities

- Bind to a `BeepForms` host through `FormsHost`
- Auto-discover a nearby host when `AutoBindFormsHost` is enabled
- Render the form title from `BeepFormsDefinition.Title` or `FormName`
- Render active block and mode summary context without duplicating workflow execution
- Expose a small smart-tag surface for common header context presets at design time

## Notes

- `BeepForms` remains the non-visual coordinator/host
- `BeepFormsCommandBar` owns block switching and sync
- `BeepFormsQueryShelf` owns query-mode entry, execution, and caption variants
- `BeepFormsPersistenceShelf` owns commit and rollback
- `BeepFormsToolbar` remains the savepoint/alert action surface
- `BeepFormsStatusStrip` remains the shared workflow/status reader