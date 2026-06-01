# BeepForms Fresh Start Overview

## Decision

This plan treats `BeepDataBlock` as legacy and does not use it as the baseline for the new implementation.

The new UI surface will be built around:

- `BeepForms`: the top-level Oracle Forms style UI host for one `FormsManager` instance.
- `BeepBlock`: the UI representation of one registered block inside `FormsManager`.

## Why This Reset Is Better

- `BeepDataBlock` mixes UI layout, block state, trigger logic, validation, query helpers, navigation, and coordinator behavior in one legacy surface.
- The name does not reflect the real runtime source of truth, which is now `FormsManager`.
- Several current helpers and models are coupled to the old control name and should not define the new architecture.
- A clean separation between form shell and block surface will match Oracle Forms concepts more accurately.

## Core Principles

1. `FormsManager` is the runtime authority.
2. `BeepForms` owns form-level UI, orchestration, and active block context.
3. `BeepBlock` owns block-level rendering and interaction only.
4. Visible UI should use Beep controls by default whenever an equivalent exists.
5. Standard WinForms controls are fallback-only for layout helpers or technical gaps where no Beep control exists yet.
6. Shared models must be named around forms and blocks, not around `BeepDataBlock`.
7. Old `BeepDataBlock` code is ignored during greenfield implementation.
8. Deletion or retirement of legacy code happens only after replacement parity is proven.

## Scope Boundaries

### In Scope

- New control family for Oracle Forms style UI on top of `FormsManager`
- New models, presenters, services, and designers for `BeepForms` and `BeepBlock`
- New samples demonstrating master-detail, query mode, validation, LOV, messages, and navigation
- Event and command flow that mirrors `FormsManager` capabilities directly

### Out of Scope

- Refactoring `BeepDataBlock`
- Renaming existing `BeepDataBlock` files into the new design
- Carrying over helper and model classes without redesign
- Deleting legacy code before the new surface is stable

## Proposed Target Structure

```text
TheTechIdea.Beep.Winform.Controls.Integrated/
  Forms/
    BeepForms/
      BeepForms.cs
      BeepForms.Layout.cs
      BeepForms.Commands.cs
      BeepForms.Messages.cs
      BeepForms.Navigation.cs
    Blocks/
      BeepBlock.cs
      BeepBlock.Layout.cs
      BeepBlock.Binding.cs
      BeepBlock.Validation.cs
      BeepBlock.QueryMode.cs
    Models/
      BeepFormsDefinition.cs
      BeepBlockDefinition.cs
      BeepFieldDefinition.cs
      BeepFormsViewState.cs
      BeepBlockViewState.cs
    Services/
      BeepFormsManagerAdapter.cs
      BeepBlockBindingAdapter.cs
      BeepFormsCommandRouter.cs
      BeepFormsMessageService.cs
      BeepFormsValidationBridge.cs
    Designers/
    Dialogs/
    Examples/
```

## Existing Helpers and Models Classification

The current `DataBlocks/Helpers` and `DataBlocks/Models` folders are not a clean UI-only layer.

- UI-specific examples: `BeepDataBlockItem`, design-time templates, notifier implementations
- Runtime-coupled examples: `DataBlockCoordinationState`, `IBeepDataBlock`, `TriggerContext`, trigger models, validation rule models
- Mixed utility examples: property helpers, trigger helpers, unit of work helpers

For the fresh start, none of these should be copied forward unchanged. At most, they can be used as reference material while new contracts are designed.

## Success Criteria

The fresh-start effort is successful when:

- A `BeepForms` control can host multiple blocks backed by `FormsManager`
- A `BeepBlock` can render one logical block in record and grid style modes
- Query, commit, rollback, navigation, validation, LOV, messages, and savepoints are driven through `FormsManager`
- Legacy `BeepDataBlock` is no longer needed for new screens

## Phase Documents

- `01-phase1-architecture-and-boundaries.md`
- `02-phase2-forms-shell-and-runtime.md`
- `03-phase3-beepblock-and-field-rendering.md`
- `04-phase4-oracle-forms-workflows.md`
- `05-phase5-design-time-and-samples.md`
- `06-phase6-cutover-and-legacy-retirement.md`
- `todo-tracker.md`