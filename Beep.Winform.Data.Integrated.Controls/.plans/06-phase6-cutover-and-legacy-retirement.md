# Phase 6: Cutover And Legacy Retirement

## Goal

Move consumers to the new control family and retire `BeepDataBlock` only after the new path is stable.

## Deliverables

- Cutover checklist for consumers
- Legacy deprecation plan
- Removal criteria for `BeepDataBlock`
- Final documentation that points new development to `BeepForms`

## Work Items

1. Define parity gates.
   - Form shell command routing
   - Block rendering modes
   - Validation
   - LOV
   - Query mode
   - Savepoints
   - Messages and alerts
   - Master-detail behavior

2. Mark legacy status.
   - Stop adding new features to `BeepDataBlock`
   - Mark samples and docs as legacy where needed

3. Plan deletion window.
   - Remove only after replacement samples and consumers are proven
   - Remove converters, designers, helpers, and models tied solely to `BeepDataBlock`

4. Final cleanup.
   - Update readmes and examples
   - Update toolbox guidance
   - Remove stale docs that position `BeepDataBlock` as the strategic direction

## Exit Criteria

- New development uses `BeepForms` and `BeepBlock`
- `BeepDataBlock` is no longer the recommended or required control for Oracle Forms style UI
- Legacy removal can happen with low risk

## Risks

- Deleting legacy code before parity is proven
- Leaving docs and samples split between old and new architectures