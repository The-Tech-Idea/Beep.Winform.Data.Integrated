# Phase 5: Design-Time Tooling And Samples

## Goal

Make the new control family usable in the designer and prove it with fresh examples that do not depend on `BeepDataBlock`.

## Deliverables

- Design-time support for `BeepForms` and `BeepBlock`
- Editors for block registration, field inclusion, and layout selection
- Sample screens showing single-block and multi-block forms
- Migration notes for future consumers of the new controls

## Work Items

1. Design-time metadata.
   - Toolbox support
   - Category and property organization
   - Safe designer-time initialization

2. Designer editors.
   - Form block picker
   - Field selection editor
   - Layout/profile editor
   - Command bar options

3. Sample coverage.
   - [x] Single-block maintenance form
   - [x] Master-detail form
   - [x] Query-heavy screen
   - [x] LOV-driven form
   - [x] Validation-heavy workflow

4. Documentation.
   - How to host `FormsManager`
   - How to define blocks
   - How to attach view definitions
   - What not to do with legacy code

## Exit Criteria

- A developer can place `BeepForms` in the designer and configure it without manual wiring everywhere
- Samples demonstrate the new path without using `BeepDataBlock`

## Risks

- Letting design-time concerns reshape runtime architecture too early
- Carrying legacy editors forward instead of writing new focused tooling