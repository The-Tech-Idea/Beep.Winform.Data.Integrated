# Phase 11: Designer Experience Polish

**Priority:** Low | **Est. Tasks:** 11

## Goal

Improve the Visual Studio designer experience for form developers using BeepForms and BeepBlock controls. Add auto-generation verbs, entity pickers, validation commands, and smart-tag enhancements.

## Dependencies

- UI: Designer classes in `TheTechIdea.Beep.Winform.Controls.Design.Server` (exist)
- UI: `BeepForms`, `BeepBlock`, all shelf controls (exist)

## Implementation Seam

### 11.1 — BeepForms Designer Enhancements

#### "Auto-Generate Shelves" Smart-Tag Action

When the developer drops a `BeepForms` on a form, they can click "Auto-Generate Shelves" to create all 6 shelf controls in the parent container with auto-bind:

```csharp
// In BeepFormsHostDesigner or BeepFormsActionList:
private void AutoGenerateShelves()
{
    IDesignerHost host = (IDesignerHost)GetService(typeof(IDesignerHost));
    if (host == null || Component is not BeepForms forms) return;

    // Get the parent container
    Control? parent = forms.Parent;
    if (parent == null) return;

    // Create shelves in order
    CreateShelf<BeepFormsHeader>(host, parent, "beepFormsHeader1", 
        shelf => { /* configure */ });
    CreateShelf<BeepFormsCommandBar>(host, parent, "beepFormsCommandBar1",
        shelf => { /* configure */ });
    CreateShelf<BeepFormsQueryShelf>(host, parent, "beepFormsQueryShelf1",
        shelf => { shelf.QueryButtons = BeepFormsQueryShelfButtons.All; });
    CreateShelf<BeepFormsPersistenceShelf>(host, parent, "beepFormsPersistenceShelf1",
        shelf => { shelf.PersistenceButtons = BeepFormsPersistenceShelfButtons.All; });
    CreateShelf<BeepFormsToolbar>(host, parent, "beepFormsToolbar1",
        shelf => { /* configure */ });
    CreateShelf<BeepFormsStatusStrip>(host, parent, "beepFormsStatusStrip1",
        shelf => { shelf.VisibleLines = BeepFormsStatusStripLines.All; });

    // Auto-bind each shelf to the BeepForms host
    // (AutoBindFormsHost defaults to true, so shelves find the host automatically)
}

private void CreateShelf<T>(IDesignerHost host, Control parent, string name, 
    Action<T> configure) where T : Control
{
    T shelf = (T)host.CreateComponent(typeof(T), name);
    configure(shelf);
    shelf.Dock = DockStyle.Top;
    parent.Controls.Add(shelf);
    // The designer will serialize this to the form's designer.cs
}
```

#### "Generate from Entity" Smart-Tag Action
- Opens a dialog that lists available entities from the connected data source
- Selecting an entity auto-fills the `BeepForms.Definition` with a block definition for that entity
- Includes option to generate related detail blocks for foreign key relationships

#### Designer Verbs
- "Validate Definition" — checks all block definitions for missing fields, missing entities, circular master-detail relationships
- "Export Definition..." — exports the form definition as JSON
- "Import Definition..." — imports a form definition from JSON
- "Clear Definition" — removes all block definitions from the form

#### Smart-Tag Preview
The smart-tag shows a live preview of the form's state:
```
┌─────────────────────────────────────┐
│ BeepForms Tasks                     │
│ ─────────────────────────────────── │
│ Form Name: CustomerManagement       │
│ Blocks: 3 (CUSTOMERS, ORDERS, ...)  │
│ Connection: MainDB (Connected)      │
│ Definition: Configured ✓            │
│ Shelves: 6 auto-generated           │
└─────────────────────────────────────┘
```

### 11.2 — BeepBlock Designer Enhancements

#### "Configure LOV..." Smart-Tag Action
- Opens a dialog to configure LOV definitions for the block's fields
- Lists all fields, shows which have LOVs, lets user add/edit/remove LOV definitions
- Integrates with the existing `BeepBlockFieldEditorForm`

#### "Set as Master Block" / "Set as Detail Block" Smart-Tag Actions
- "Set as Master Block" — marks the block as a master and lets user select detail blocks and key fields
- "Set as Detail Block of..." — shows a dropdown of other blocks on the same form, picks master, sets FK field

#### Smart-Tag Preview
```
┌─────────────────────────────────┐
│ BeepBlock Tasks                 │
│ ─────────────────────────────── │
│ Block Name: CUSTOMERS           │
│ Entity: Customer (SQLite)       │
│ Mode: Normal · 47 records       │
│ Master: None                    │
│ Details: ORDERS (1)             │
│ LOVs: CustomerType, Region      │
└─────────────────────────────────┘
```

### 11.3 — Designer Verification

#### Verify All Designer Registrations
- [ ] `BeepFormsCommandBarDesigner` registered and working in toolbox
- [ ] `BeepFormsQueryShelfDesigner` registered and working in toolbox
- [ ] `BeepFormsPersistenceShelfDesigner` registered and working in toolbox
- [ ] `BeepFormsToolbarDesigner` registered and working in toolbox
- [ ] `BeepFormsHeaderDesigner` registered and working in toolbox
- [ ] `BeepFormsStatusStripDesigner` registered and working in toolbox
- [ ] `BeepFormsHostDesigner` registered and working in toolbox
- [ ] `BeepBlockDesigner` registered and working in toolbox

#### Verify Auto-Bind Discovery
- [ ] Drop `BeepForms` on a form
- [ ] Drop `BeepFormsCommandBar` below it (as sibling or child)
- [ ] Verify the CommandBar's `FormsHost` auto-binds to the `BeepForms` without manual configuration
- [ ] Repeat for all 6 shelf controls

#### Verify Serialization
- [ ] Auto-generate shelves via smart-tag
- [ ] Save and close the form
- [ ] Reopen the form — verify all shelves and their properties persist correctly
- [ ] Verify `AutoBindFormsHost = true` persists
- [ ] Verify `FormsHost` reference persists (designer serializer should handle this)

## Verification

1. Drop BeepForms on a form, click "Auto-Generate Shelves" — verify 6 shelves appear
2. Save and reopen — verify all shelves persist
3. Delete BeepForms — verify all auto-generated shelves are cleaned up
4. Drop BeepBlock, click "Configure LOV..." — verify LOV editor opens
5. Set a BeepBlock as master, another as detail — verify relationship is configured
6. Click "Validate Definition" — verify validation report shows any issues

## New Files

- `Designers/BeepFormsSetupWizardForm.cs` (may already exist — verify)
- `Designers/BeepBlockLOVEditorForm.cs` (may already exist — verify)

## Modified Files

- `Designers/BeepFormsHostDesigner.cs` or `BeepFormsActionList.cs` — add smart-tag actions
- `Designers/BeepBlockDesigner.cs` or `BeepBlockActionList.cs` — add smart-tag actions
- `Designers/DesignRegistration.cs` — verify all registrations

## Risks

- **Low:** Designer-only work. No runtime impact.
- Must use `IDesignerHost.CreateComponent` for auto-generated controls (see AGENTS.md design pattern).
- Smart-tag preview state reading must handle design-time where FormsManager is null (show "Design mode" placeholders).
- Auto-bind must work in the designer — verify `BeepFormsHostResolver.Find` works correctly with design-time control trees.
