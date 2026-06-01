# Phase 1: Architecture And Boundaries

## Goal

Define the new control family, namespace layout, contracts, and separation of responsibilities before any implementation starts.

## Deliverables

- Canonical naming for `BeepForms`, `BeepBlock`, and supporting services
- Namespace and folder layout for the new implementation
- Fresh contracts for form shell, block shell, and field descriptors
- Explicit rule that `BeepDataBlock` is legacy and not reused as a base type
- Explicit rule that visible UI defaults to Beep controls rather than stock WinForms controls

## Work Items

1. Define the top-level roles.
   - `BeepForms`: form shell, toolbar/status/message host, active block coordination
   - `BeepBlock`: one data block UI surface
   - `BeepBlockFieldPresenter`: pluggable editor/display adapter per field type

2. Define contract boundaries.
   - `IBeepFormsHost`
   - `IBeepBlockView`
   - `IBeepBlockBindingAdapter`
   - `IBeepFormsCommandRouter`
   - `IBeepFormsNotificationService`

3. Define model boundaries.
   - `BeepFormsDefinition`
   - `BeepBlockDefinition`
   - `BeepFieldDefinition`
   - `BeepFormsViewState`
   - `BeepBlockViewState`

4. Define dependency direction.
   - UI depends on `FormsManager`
   - UI models do not own persistence logic
   - UI helpers do not duplicate manager rules
   - Visible controls prefer `BeepPanel`, `BeepLabel`, `BeepTextBox`, and other Beep equivalents by default
   - Stock WinForms controls are only acceptable for internal layout helpers when no Beep equivalent exists

5. Define naming and deprecation policy.
   - No new type names containing `BeepDataBlock`
   - No new code added to `DataBlocks` for the fresh-start line
   - Legacy code remains untouched until cutover phase

## Exit Criteria

- The new folder and namespace map is approved
- The team can name every new class without referencing legacy names
- There is a clear answer for what belongs in UI, adapter, model, and runtime bridge layers

## Risks

- Reusing old names will pull legacy assumptions into the new design
- Letting the UI own logic already handled by `FormsManager` will recreate duplication

## Notes

This phase should produce design decisions, not implementation shortcuts.