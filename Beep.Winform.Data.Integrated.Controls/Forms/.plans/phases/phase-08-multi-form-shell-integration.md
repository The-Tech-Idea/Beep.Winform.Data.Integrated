# Phase 8: Multi-Form Shell Integration

**Priority:** Medium | **Est. Tasks:** 9

## Goal

Surface the multi-form capabilities (OpenForm, GoForm, :GLOBAL variables, inter-form messaging) in the `BeepApplication` host. Add form-switching toolbar, global variables viewer, and inter-form message log.

## Dependencies

- Engine: `FormsManager.InterFormComm.cs`, `FormsManager.MultiFormNavigation.cs` (complete)
- UI: `BeepApplication.cs` (exists)

## Engine Methods Used (No New Engine Code)

| UI Action | Engine Method | Status |
|-----------|---------------|--------|
| Open Form (modeless) | `FormsManager.OpenFormAsync(formName, params)` | ✅ |
| Call Form (modal) | `FormsManager.CallFormAsync(target, params, Modal)` | ✅ |
| New Form (replace) | `FormsManager.NewFormAsync(formName)` | ✅ |
| Close Form | `FormsManager.CloseForm` (via host) | ✅ |
| Return to Caller | `FormsManager.ReturnToCallerAsync(returnData)` | ✅ |
| Set Global Variable | `FormsManager.SetGlobalVariable(name, value)` | ✅ |
| Get Global Variable | `FormsManager.GetGlobalVariable(name)` | ✅ |
| Get Active Forms | `IFormRegistry.GetActiveFormNames()` | ✅ |
| Post Message | `FormsManager.PostMessage(targetForm, type, payload)` | ✅ |
| Subscribe to Message | `FormsManager.SubscribeToMessage(type, handler)` | ✅ |
| Broadcast Message | `FormsManager.BroadcastMessage(type, payload)` | ✅ |

## Implementation Seam

### 8.1 — BeepApplication Form Switcher

Add a form-switching toolbar to `BeepApplication`:

```
Current BeepApplication:
├── MDI container with OpenForm/GoForm/CloseForm logic
└── :GLOBAL variable storage (via FormsManager)

Additions:
├── FormSwitcherToolbar (new visual element)
│   ├── [Open Form ▼] dropdown
│   │   ├── Customer Form
│   │   ├── Order Form
│   │   └── ...
│   ├── [Active Form Tabs or List]
│   │   ├── [Customers] [Orders] [Products]
│   │   └── Click to GoForm
│   └── [Close Form] button
├── Global Variables Panel (debug tool)
└── Inter-Form Message Log Panel (debug tool)
```

**Form tabs/list:**
- Shows all open forms (from `_activeForms` dictionary)
- Active form is highlighted
- Dirty forms show an asterisk (*)
- Click a tab to GoForm (bring to front)
- Right-click for context menu: Close, Close All Others, Refresh

**Open Form dialog:**
```
┌──────────────────────────────────┐
│ Open Form                    [X] │
├──────────────────────────────────┤
│ Available Forms:                 │
│ ┌────────────────────────────┐   │
│ │ Customer Management        │   │
│ │ Order Entry                │   │
│ │ Product Catalog            │   │
│ │ Inventory Lookup           │   │
│ └────────────────────────────┘   │
│                                  │
│ Mode: ○ Modeless  ○ Modal        │
│                                  │
│         [Open]  [Cancel]         │
└──────────────────────────────────┘
```

### 8.2 — Global Variables Viewer

Add a debug panel for `:GLOBAL.*` variables:

```
┌──────────────────────────────────────────┐
│ Global Variables                     [X] │
├──────────────────────────────────────────┤
│ Name            │ Value        │ Updated │
├──────────────────┼──────────────┼─────────┤
│ current_user     │ "admin"      │ 10:23   │
│ company_name     │ "Acme Corp"  │ 10:15   │
│ default_warehouse│ "WH-001"     │ 09:45   │
└──────────────────────────────────────────┘
                    [Copy Value] [Refresh]
```

**Implementation:**
- Read all globals from `FormsManager` via `GetGlobalVariable` for known keys or via `BeepApplication._globalVariables` dictionary
- Display in a read-only `DataGridView`
- "Copy Value" copies selected cell to clipboard
- "Refresh" re-reads from engine

### 8.3 — Inter-Form Message Log

Add a debug panel for inter-form messages:

```
┌──────────────────────────────────────────────────────────┐
│ Inter-Form Message Log                               [X] │
├──────────────────────────────────────────────────────────┤
│ Time    │ From      │ To    │ Type        │ Payload      │
├─────────┼───────────┼───────┼─────────────┼──────────────┤
│ 10:23:01│ Customers │ Orders│ RefreshData │ {orderId:42} │
│ 10:22:55│ Products  │ All   │ CacheClear  │ {}           │
│ 10:22:30│ Orders    │ Cust  │ NotifySave  │ {recId:101}  │
└──────────────────────────────────────────────────────────┘
                         [Clear Log] [Auto-scroll ✓]
```

**Implementation:**
- Subscribe to `OnFormMessage` event on the current FormsManager
- Log all messages with timestamp, source, target, type, payload
- Filter by source form or message type
- Auto-scroll toggle
- Clear log button

## Verification

1. Open BeepApplication with multiple forms
2. Verify form tabs show all open forms
3. Click a tab — verify it switches to that form (GoForm)
4. Close a form — verify tab is removed
5. Open a form via "Open Form" dialog — verify it appears as a new tab
6. Open global variables viewer — verify all globals are listed
7. Set a global variable from code — verify it appears in viewer on refresh
8. Post a message from one form to another — verify it appears in message log

## New Files

- `Models/FormSwitcherItem.cs` (simple model for form list items)

## Modified Files

- `BeepApplication.cs` — add form switcher toolbar, tabs/list, Open Form dialog
- `Models/` — add model classes for global variables viewer, message log

## Risks

- **Low:** All engine methods exist. BeepApplication already has the core multi-form logic.
- Form tabs must handle form disposal correctly (remove tab when form closes).
- Global variables viewer is a diagnostic tool — not for production use. Consider making it a "Developer Mode" toggle.
- Message log can grow large — implement circular buffer with configurable capacity.
