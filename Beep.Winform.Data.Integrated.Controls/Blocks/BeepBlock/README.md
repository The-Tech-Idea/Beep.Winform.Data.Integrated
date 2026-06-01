# BeepBlock

`BeepBlock` is the fresh-start block surface for the integrated controls path. Each instance maps to one logical FormsManager block and is hosted by `BeepForms`.

Visible block UI should use Beep controls by default. Stock WinForms controls are fallback-only for layout infrastructure where no Beep equivalent exists.

## Current structure

- `BeepBlock.cs`: root control state, host binding, and manager synchronization
- `BeepBlock.Layout.cs`: caption/header, workflow strip, validation summary, and record/grid host regions
- `BeepBlock.RecordMode.cs`: definition-driven record editor row scaffold plus typed query-row composition
- `BeepBlock.QueryMode.cs`: query-mode state, typed criteria capture, and manager-filter packaging helpers
- `BeepBlock.TriggerProxy.cs`: host-proxied trigger and UoW activity handling for block-local runtime state
- `BeepBlock.Validation.cs`: field highlighting hooks plus severity-aware block-level record validation summary feedback, next-step guidance, semantic tooltips, and field-surface status badges

## Current responsibilities

- Track manager-aligned block view state such as current mode, current record index, and record count
- Keep a typed UI-side entity snapshot on `BeepBlock.Definition.Entity` so the block always carries connection, entity, and field structure metadata without pushing runtime ownership into the control
- Render a record-mode scaffold from `BeepFieldDefinition`
- Persist field-level editor metadata in `Designer.cs`, including the presenter key, explicit control type override, and binding property used for generated record editors
- Persist designer-generated field-control layout intent in `BeepBlockDefinition.Metadata["FieldControlsLayoutMode"]` so design-time tooling and IDE scaffolding can agree on `StackedVertical`, `LabelFieldPairs`, or `GridLayout` generation
- Resolve generated field defaults through `BeepFieldControlTypeRegistry`, combining the built-in control map with optional persisted policy rules from `%LocalAppData%\TheTechIdea\Beep.Winform\field-control-defaults.json`, and expose that policy through the BeepBlock smart tag, setup wizard, and field-property editor so design-time authors can tune defaults without leaving the integrated workflow
- Allow `BeepBlockEntityFieldDefinition` snapshots to preseed `EditorKey`, `ControlType`, and `BindingProperty` so entity-level field definitions can override the generated control and binding contract before `BeepFieldDefinition` rows are emitted
- Rebuild query mode as a real criteria-entry surface with operator/value rows for manager-queryable items instead of only switching view state
- Treat class-object records as the bound record contract in record helpers; block binding reads and writes public properties/fields on record instances returned from datasource `List<object>` results
- Bind record-mode editors to a shared block-level `BindingSource` over `FormsManager.GetUnitOfWork(blockName).Units`
- Keep `BindingSource.Position` and `UnitOfWork.CurrentItem` synchronized so single-record view and grid view share the same current record
- Capture a typed runtime entity snapshot from `FormsManager.GetBlock(blockName).UnitOfWork.EntityStructure` and use that snapshot to scaffold field definitions whenever explicit UI field definitions were not supplied
- Resolve editors through `BeepBlockPresenterRegistry` with default text, numeric, date, checkbox, and combo/LOV presenters
- Honor `BeepFieldDefinition.ControlType` and `BeepFieldDefinition.BindingProperty` when a field needs a specific Beep control or a stock WinForms control with an explicit binding contract
- Bind combo editors through typed `BeepComboBox.SelectedValue` instead of text fallback, so enum and LOV return values stay in their original CLR shape
- Populate combo/LOV editors from runtime sources: explicit `BeepFieldDefinition.Options`, enum properties, and FormsManager LOV registrations all produce typed `SimpleItem` values
- Surface record-mode LOV fields with an explicit picker button plus `F9` keyboard entry so the block can open a forms-native popup instead of forcing users through the inline combo drop-down only
- Apply LOV related-field mappings back onto the current record and refresh sibling editors immediately after a selection in record mode
- Reuse `BeepLovPopup` as the record-mode LOV dialog, pre-seed it from manager-backed LOV loads, debounce popup search into manager LOV reloads, and carry visible LOV column metadata into the popup through projected `SimpleItem` text/subtext fields
- Host `BeepBlockNavigationBar` as the block-level command surface; record navigation and query/commit/rollback forward through `BeepForms`, while local new/delete actions use the current block UoW
- Collect query criteria in the UI and package them as `AppFilter` instances; date and numeric fields now support typed range entry for both `between` and `not between`, rows expose per-field reset affordances, operator-aware no-value states hide unused editors, and packaged filters populate `FilterValue1` for manager execution
- Provide dedicated list-entry editing for `in` and `not in` operators so string/numeric/date fields can capture membership filters without pushing list parsing into the normal single-value editors, and reuse static option or LOV item sets as multi-pick sources when those are available
- Route query-list validation and duplicate-value feedback through `BeepForms` shared notifications when the block is hosted, while still falling back to modal dialogs for standalone usage
- Derive navigator command availability from manager state plus typed `BeepBlockDefinition.Navigation` overrides, while still honoring legacy `BeepBlockDefinition.Metadata` flags such as `navigation.enabled`, `navigation.first.enabled`, `navigation.new.enabled`, and `navigation.save.enabled`
- Configure `BeepGridPro` columns from the same runtime field/entity metadata path used by record mode
- Bind `BeepGridPro` to the same shared block `BindingSource` in grid mode
- Offer typed design-time editor-key suggestions and nearby host block-name suggestions so block registration and field presenter selection do not depend on raw string recall
- Expose typed entity snapshots in design-time editors so block authoring works against first-class entity/field structure instead of loose metadata keys
- Provide a BeepBlock-native design-time workflow through the smart-tag setup wizard, dedicated navigation editor action, and field editor so authors can select connection/entity metadata, choose visible fields, switch record versus grid versus designer-generated presentation, preview generated layout compositions, select generated field-control layout metadata, add or remove persisted field definitions, and tune field labels/order/editor keys/control types/binding properties/default values without relying on the removed legacy data-block designer surface
- Keep the design-time surfaces synchronized: smart-tag block-name edits now round-trip into `Definition.BlockName`, designer-generated layout metadata is only directly editable while `PresentationMode` is `DesignerGenerated`, wizard entity retargeting defaults the new entity's fields selected without resurrecting same-entity removals, explicit empty field selections now persist through runtime and wizard round-trips, and both the dedicated field editor and generic collection editors renormalize `Order` after add/remove/move operations
- Keep extension scaffolding and runtime fallback aligned by generating persisted field definitions through the same shared registry-driven default resolver that the integrated controls use at runtime
- Project manager validation and LOV validation messages into generated editors through the shared `BaseControl` error surface plus row-level label/status accents, semantic tooltips, and explicit severity badges
- Surface record-level validation problems above the block as a headline-plus-detail summary with severity badges and next-step guidance so blocking issues, warnings, and informational notes stay readable even when the current field is out of view
- Surface manager master/detail context in the workflow strip so coordinated blocks stay understandable without duplicating coordination logic in the UI
- Surface trigger counts, last trigger activity, and normalized UoW activity in the workflow strip through the BeepForms host proxy so the block can reflect runtime workflow without direct `FormsManager` or raw UoW coupling
- Suppress record-validation noise while query mode is active so the block can behave as a clean criteria-entry surface
- Prefer Beep editors and Beep shell controls for record-mode visuals

## Current gaps

- `BeepFieldDefinition.Options` now has a dedicated design-time collection editor, navigator flags have both a typed `BeepBlockDefinition.Navigation` editor surface and a dedicated smart-tag action, metadata uses a focused key/value editor, `EditorKey`/`BlockName` expose typed standard-value suggestions, and the smart tag now includes a BeepBlock-native setup wizard with live layout preview plus field-property editor; remaining tooling work is mostly deeper host-aware multi-block authoring beyond the current single-block workflow
- The smart tag now also exposes direct caption / manager-block / presentation editing plus designer-generated layout presets, and the wizard carries `DesignerGenerated` plus `FieldControlsLayoutMode` metadata through the same design-time flow; remaining tooling work is mostly deeper host-aware multi-block authoring beyond the current single-block workflow
- Global field-control defaults can now be overridden through the persisted policy file, the dedicated BeepBlock policy editor surface, and per-entity field snapshots that preseed control and binding overrides; remaining tooling work is mostly any future scope-aware sharing beyond the current global policy plus per-field overrides
- Query mode now supports per-field reset, typed `between`/`not between` filters for numeric/date fields, `is null`/`is not null` no-value operators, and dedicated `in`/`not in` list-entry widgets that can switch between free-text entry and known-value multi-pick for static combo/LOV sources; remaining work is any deeper specialized widgets beyond the current range/list/no-value path
- LOV support now covers the record-mode popup path for manager-registered LOV fields, including debounced manager-backed reloads from the popup search box; remaining tooling work is mostly richer designer authoring and any future typed editors beyond navigator settings