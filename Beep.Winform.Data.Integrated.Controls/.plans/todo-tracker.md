# BeepForms todo tracker — RETIRED (2026-08-01)

**This file no longer tracks anything. Do not plan from it and do not add rows
to it.**

## Why it was retired

It tracked a design that was never built. Every Phase 2–4 row carried a status —
`in-progress` or `done` — against classes that do not exist anywhere in this
solution:

| Named in the tracker | Exists? |
|---|---|
| `BeepForms` | ❌ |
| `BeepBlock` | ❌ |
| `BeepFormsHeader` | ❌ |
| `BeepFormsCommandBar` | ❌ |
| `BeepFormsQueryShelf` | ❌ |
| `BeepFormsPersistenceShelf` | ❌ |
| `BeepFormsToolbar` | ❌ |
| `BeepFormsStatusStrip` | ❌ |
| `BeepLovPopup` | ✅ (in `Beep.Winform.Controls/Lovs/`) |

Searched by class declaration across `Beep.Winform` and
`Beep.Winform.Data.Integrated`; the only trace was a designer test file. Rows
such as "Create `BeepFormsStatusStrip` visual surface — *in-progress*: shared
status/message lines now live in a separate BaseControl-derived strip" described
a file that has never existed.

Nothing here was worth rewriting: there was no accurate row to keep. Rewriting it
as a second status document would also have recreated the original problem — two
places claiming what is done, drifting apart. The full text is in git history if
it is ever needed.

## What actually exists

The runtime is the `WinForm*` family in this project — `WinFormFormHost`,
`WinFormBlockHost`, `WinFormBlockNavigationBar`, eight `WinForm*FieldPresenter`s,
fifteen `Forms/FeatureControls/` panels, and `WinFormFormStatusBar` — mirrored on
WPF by `BeepWpfForms` / `BeepWpfBlock`. That is what the IDE generates against
and what the harnesses exercise.

## Where status lives now

- **`08-winform-thin-ui-completion.md`** — the plan of record, written against
  the code that is actually here.
- **The harnesses** — the only real evidence of status, because in this codebase
  a green build and a `true` return prove very little:
  - `Beep.Desktop/…/IDE.Extensions/` — `dotnet run --project Integration`,
    `dotnet run --project Verification`,
    `python tools/command-reachability.py .`,
    `dotnet run --project SmokeTests/BeepDataManager.TriggerSmokeTests.csproj`
  - `Beep.WPF/TheTechIdea.Beep.Wpf.Data.Integrated.Integration/` —
    `dotnet run --project BeepWpfFormsIntegration.csproj`

A claim that something works belongs in a harness check, not in a status column.
