# Phase 3: BeepBlock And Field Rendering

## Goal

Build the new block-level UI surface as a dedicated control that represents one logical block inside `FormsManager`.

## Deliverables

- `BeepBlock` core control
- Record layout mode and `BeepGridPro`-backed grid layout mode
- Field presenter registry for editor selection
- Block-level navigation and current-record display
- Field and shell visuals built from Beep controls by default

## Work Items

1. Create `BeepBlock` core responsibilities.
   - Bind to a manager block name
   - Render one block only
   - Expose block view state without owning persistence rules

2. Create block definition and field definition models.
   - Visible fields
   - Editor hints
   - Layout metadata
   - Display labels and grouping
   - Allow runtime fallback from `FormsManager` block metadata when explicit definitions are missing
   - Prefer `UnitOfWork.EntityStructure` over duplicated external metadata when synthesizing runtime field definitions

3. Implement presenter strategy.
   - Text presenter
   - Numeric presenter
   - Date presenter
   - LOV-capable presenter
   - Checkbox and option presenter
   - Register default presenters once per `BeepBlock` registry so metadata-driven editor selection works without extra setup
   - Prefer `BeepTextBox`, `BeepComboBox`, `BeepCheckBox`, and other Beep editors where equivalents exist

4. Implement layout modes.
   - Record controls mode for Oracle Forms style editing
   - `BeepGridPro` grid mode for quick browse/edit flows
   - Prefer Beep shell controls for captions, group surfaces, and status visuals

5. Replace navigation assumptions.
   - New `BeepBlockNavigationBar` should bind to `BeepBlock`, not to `BeepDataBlock`
   - Command availability should come from manager state and block configuration

## Exit Criteria

- A block can render fields for one manager block
- A block can swap between record and grid modes
- Field editors are selected from a registry rather than hard-coded legacy branches

## Risks

- Rebuilding `BeepDataBlock` under a different name instead of designing a cleaner surface
- Coupling field layout to the data source rather than to block/view definitions