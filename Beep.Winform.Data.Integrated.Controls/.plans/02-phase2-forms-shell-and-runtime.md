# Phase 2: BeepForms Shell And Runtime Bridge

## Goal

Build the form-level shell that represents one Oracle Forms style screen backed by one `FormsManager` instance.

## Deliverables

- `BeepForms` control skeleton
- Runtime bridge from `BeepForms` to `FormsManager`
- Form-level status line, message area, toolbar region, and block container region
- Active block tracking and form command routing
- Visible shell surfaces built with Beep controls by default

## Work Items

1. Create `BeepForms` core control.
   - Derive from `BaseControl`
   - Expose `FormsManager`, `FormName`, `ActiveBlockName`, and lifecycle hooks
   - Prefer `BeepPanel`, `BeepLabel`, and other Beep shell controls over stock WinForms widgets

2. Create the runtime adapter layer.
   - Register blocks by manager block name, not by local child naming heuristics
   - Prefer `FormsManager.RegisterBlock(blockName, unitOfWork, dataSourceName, ...)` when the UOW already carries `EntityStructure`
   - Subscribe to manager events for status, record changes, messages, and block change notifications

3. Define layout regions.
   - Command bar
   - Form header
   - Message/status strip
   - Block host panel
   - Optional navigation and side panels
   - Use stock layout-only controls only where no Beep equivalent exists

4. Build the form command router.
   - Enter query
   - Execute query
   - Commit form
   - Rollback form
   - Navigate block
   - Show alerts/messages

5. Add state synchronization.
   - Current block
   - Dirty state
   - Query mode state
   - Form-level command availability

## Exit Criteria

- `BeepForms` can host block views and connect to `FormsManager`
- Form-level commands route through the manager cleanly
- The shell can display manager-driven status and messages without block-local hacks

## Risks

- Letting `BeepForms` infer business behavior instead of observing `FormsManager`
- Mixing block-level rendering details into the form shell