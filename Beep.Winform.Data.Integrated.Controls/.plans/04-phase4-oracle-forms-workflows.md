# Phase 4: Oracle Forms Workflows

## Goal

Implement the user workflows that make the new UI feel like Oracle Forms while staying aligned with `FormsManager` semantics.

## Deliverables

- Query mode UX
- Validation bridge
- LOV dialogs and dependent field population
- Savepoints, messages, alerts, and record locking integration
- Master-detail synchronization behavior exposed in the UI

## Current Progress

- Query mode now has a real `BeepBlock` criteria-entry surface: only manager-queryable items render in query mode, each row exposes operator + value entry, rows include per-field reset affordances, numeric/date fields support typed `between` and `not between` editors, and dedicated list-entry widgets now cover `in` and `not in` with option-aware multi-pick for static combo/LOV sources before execution is forwarded to `FormsManager`.
- Validation now has both inline field errors and a block-level current-record summary surface so off-screen issues remain visible, with severity-aware styling for warnings vs blocking errors.
- Master/detail and workflow work now uses a split surface: `BeepForms` keeps manager-owned form state and coordination while `BeepFormsHeader` renders title/context, `BeepFormsCommandBar` renders block selection plus sync, `BeepFormsQueryShelf` renders query-mode actions with caption variants, `BeepFormsPersistenceShelf` renders commit/rollback actions, `BeepFormsToolbar` renders savepoint/alert popup actions, and `BeepFormsStatusStrip` renders the shared status/message/workflow lines, so the coordinator itself stays non-visual.
- LOV support now has a real record-mode popup path: manager-registered LOV fields in `BeepBlock` expose an explicit picker button and `F9` shortcut, the popup reuses `BeepLovPopup` over manager-backed loads, popup search now debounces into manager-backed LOV reloads, and visible LOV column metadata is carried through projected `SimpleItem` fields.
- Trigger-aware UX now keeps the shared status/message strip aligned with manager-owned current messages, downgrades trigger/user cancellations to warnings instead of generic errors, and routes remaining query-list validation/duplicate feedback through `BeepForms` notifications instead of ad hoc modal dialogs when a host is available.

## Work Items

1. Query mode.
   - Enter query mode at form and block level
   - Render query operators and criteria entry cleanly
   - Execute and clear criteria through manager-backed services
   - Current status: end-to-end block query criteria flow is in place, including per-field reset, typed numeric/date `between` and `not between` editors, dedicated `in`/`not in` list-entry widgets with option-aware multi-pick for static combo/LOV fields, and no-value operator presentation; future work is any additional specialized widgets beyond the current range/list/no-value path

2. Validation.
   - Show field validation state using manager and block definitions
   - Support record-level validation summary
   - Surface warnings, infos, and blocking errors consistently
   - Current status: summary surface and severity styling are in place; next pass is richer warning/info presentation and stronger visual affordances than color alone

3. LOV support.
   - Create new LOV model and service names around forms/blocks, not `BeepDataBlock`
   - Support dialog selection, filtering, and related field fill-in
   - Current status: record-mode LOV fields now open a dedicated popup picker backed by `BeepLovPopup`, use manager-loaded LOV data, debounce popup search into manager-backed LOV reloads, keep combo binding/validation on typed return values, and carry up to three extra LOV columns into the popup; future work is richer designer authoring only

4. Trigger-aware UX.
   - Support pre/post operation feedback
   - Show cancellation reasons and trigger errors in a predictable way
   - Current status: active-block message syncing now prefers manager-owned current messages in `BeepForms` view state, command wrappers no longer overwrite manager errors with blanket warnings, navigation/query/delete cancellations surface as warnings, and form commit/rollback preserve explicit cancel reasons when triggers/events provide them

5. Savepoints, messages, and alerts.
   - Bind to `FormsManager` savepoints and message services
   - Provide separate visual surfaces over `BeepForms` shared view state instead of embedding shell chrome in the coordinator
   - Current status: `BeepForms` exposes manager-backed query/commit/rollback/navigation wrappers plus savepoint create/list/release/rollback and alert/confirm wrappers, `BeepFormsHeader` now owns title/context rendering, `BeepFormsCommandBar` now owns block switching plus sync, `BeepFormsQueryShelf` now owns query-mode entry/execution plus selectable in-shelf caption variants, `BeepFormsPersistenceShelf` now owns commit/rollback, `BeepFormsToolbar` now owns the savepoint/alert popup actions plus picker/name dialogs, `BeepFormsStatusStrip` now owns the shared status/message/workflow lines, and the design-server smart tags now shape header context, command-bar composition/flow, query-shelf composition/caption presets/flow, persistence-shelf composition/flow, toolbar composition, and status-strip line presets on the standalone controls

6. Master-detail.
   - Let `BeepForms` host coordinated blocks
   - Reflect detail refreshes from manager state instead of duplicating filter logic in the UI
   - Current status: `BeepBlock` exposes manager-owned relationship context locally and `BeepForms` now keeps coordinated refresh/status state without reintroducing relationship logic into the UI; separate visual surfaces can render that state without turning the coordinator back into a shell

## Exit Criteria

- A user can perform realistic Oracle Forms workflows without touching legacy controls
- Validation, query mode, and LOV flows are visually and behaviorally coherent
- Master-detail state stays manager-driven

## Risks

- Putting trigger logic back into the UI layer
- Reintroducing duplicated query and synchronization helpers outside the manager bridge