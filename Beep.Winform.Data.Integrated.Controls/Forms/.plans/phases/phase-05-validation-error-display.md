# Phase 5: Validation Error Display

**Priority:** High | **Est. Tasks:** 8

## Goal

Show per-field validation errors visually (red border, error icon, tooltip) instead of only in the status strip. Add an error summary and "Jump to First Error" navigation. Show cross-block validation errors before commit.

## Dependencies

- Engine: `ValidationManager`, `IValidationManager`, `ValidationResult` (complete)
- UI: `BeepFormsStatusStrip` (exists), `BeepBlock` (exists)

## Engine Methods Used (No New Engine Code)

| UI Action | Engine Method | Status |
|-----------|---------------|--------|
| Get Field Errors | `FormsManager.GetBlock(blockName)?.ErrorLog` | ✅ |
| Validate Item | `FormsManager.ValidateItem(block, item)` | ✅ |
| Validate Block | `FormsManager.ValidateBlock(blockName)` | ✅ |
| Cross-Block Validate | `FormsManager.CrossBlockValidation.Validate(...)` | ✅ |
| Get Validation Result | `ValidationResult` with severity, message, field name | ✅ |

## Implementation Seam

### 5.1 — Per-Field Error Indicators on BeepBlock

Add error state rendering to `BeepBlock` fields:

**Error state properties on each field:**
```csharp
class FieldErrorState
{
    public bool HasError { get; set; }
    public string? ErrorMessage { get; set; }
    public ValidationSeverity Severity { get; set; } // Error, Warning, Info
}
```

**Visual indicators:**
| Severity | Border Color | Icon | Behavior |
|----------|-------------|------|----------|
| Error | Red (#E74C3C) | ⚠ or red exclamation | Blocking — must fix before save |
| Warning | Orange (#F39C12) | ⚠ or yellow triangle | Non-blocking — can proceed |
| Info | Blue (#3498DB) | ℹ or blue circle | Informational only |

**Rendering approach:**
- When `HasError == true`, draw a 2px colored border around the input control
- Show a small icon (16×16) next to the field label or inside the control
- Set tooltip to the error message
- On mouse hover, show full error message in a tooltip
- Clear error indicators when user modifies the field (re-validate on next change)

**Error source:**
```csharp
// In BeepBlock, after each field validation:
private void UpdateFieldErrorState(string fieldName)
{
    var errors = _formsHost?.GetBlockErrors(BlockName); // new method on IBeepFormsHost
    var fieldError = errors?.FirstOrDefault(e => e.FieldName == fieldName);
    
    if (fieldError != null)
    {
        SetFieldError(fieldName, fieldError.Message, MapSeverity(fieldError.Severity));
    }
    else
    {
        ClearFieldError(fieldName);
    }
}
```

### 5.2 — Error Summary in StatusStrip

Add error count badge to `BeepFormsStatusStrip`:

```
StatusStrip layout (enhanced):
┌──────────────────────────────────────────────────────────────────┐
│ Mode: Normal │ Block: CUSTOMERS │ Rec 3/47 │ ⚠ 2 errors │ Dirty │
└──────────────────────────────────────────────────────────────────┘
```

**Error count label:**
- Shows "⚠ N errors" when there are errors
- Shows "⚠ N warnings" when there are only warnings
- Shows nothing (hidden) when there are no validation issues
- Click to open error summary dialog
- Color: Red for errors, Orange for warnings

**Error summary dialog:**
```
┌──────────────────────────────────────────┐
│ Validation Issues                    [X] │
├──────────────────────────────────────────┤
│ ⚠ Error: Customer Name is required       │
│   Block: CUSTOMERS | Field: CustomerName │
│                                          │
│ ⚠ Warning: Discount exceeds 50%          │
│   Block: ORDERS | Field: Discount        │
│                                          │
│ [Jump to Issue] [Ignore Warnings] [Close]│
└──────────────────────────────────────────┘
```

**"Jump to First Error" button:**
- In the error summary dialog: "Jump to Issue" button
- Also a button in StatusStrip or CommandBar
- Navigates to the block+item of the first error
- Sets focus to the field with the error
- Shows the field-level error indicator

### 5.3 — Cross-Block Validation Before Commit

Before commit, check cross-block validation rules and show results:

```csharp
// In BeepForms, before CommitFormAsync:
if (_formsManager?.Configuration?.Validation?.ValidateBeforeCommit == true)
{
    var crossBlockErrors = _formsManager.CrossBlockValidation.Validate(...);
    if (crossBlockErrors.Any(e => e.IsError))
    {
        // Show dialog with errors
        var result = ShowValidationErrorsDialog(crossBlockErrors);
        if (result == ValidationDialogResult.Cancel)
        {
            return; // Abort commit
        }
        // If "Proceed Anyway", continue with commit
    }
}
```

## Verification

1. Create a validation rule (e.g., CustomerName required)
2. Clear the CustomerName field — verify red border appears on the field
3. Hover over field — verify tooltip shows "Customer Name is required"
4. Verify error count badge appears in StatusStrip ("⚠ 1 error")
5. Click error count — verify error summary dialog opens
6. Click "Jump to Issue" — verify focus moves to the CustomerName field
7. Fix the field — verify error indicators clear
8. Create a cross-block validation rule — verify it blocks commit with a dialog

## New Files

- `Models/FieldErrorState.cs` (new model class, or add to existing models)

## Modified Files

- `BeepBlock/` — add per-field error rendering, error state tracking
- `BeepFormsStatusStrip/BeepFormsStatusStrip.cs` — add error count badge
- `BeepForms/BeepForms.Commands.cs` — add pre-commit cross-block validation check
- `Contracts/IBeepFormsHost.cs` — add `GetBlockErrors(blockName)` method if missing
- `Services/BeepFormsManagerAdapter.cs` — add error state sync

## Risks

- **Low:** Validation is engine-owned. UI only renders engine-provided error data.
- BeepBlock field rendering must be modified to support error state borders — ensure this doesn't break existing field layouts.
- Error state must clear when user edits the field — need to hook into field change events.
- Cross-block validation blocking commit may surprise users — the dialog must clearly explain the options.
