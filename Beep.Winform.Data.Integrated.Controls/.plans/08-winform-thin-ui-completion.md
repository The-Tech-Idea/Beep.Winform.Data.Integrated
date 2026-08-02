# Phase 8 — Completing the WinForms thin UI for Oracle Forms parity

**Created 2026-08-01.** The plan of record for this project. It replaced
`todo-tracker.md`, which was **retired the same day** (§8.0) — see §0 for what
was wrong with it, since the same failure is worth not repeating.

---

## 0. Why `todo-tracker.md` was retired

**Resolved 2026-08-01 (§8.0).** `todo-tracker.md` is now a tombstone pointing
here; the text below is why.

It marked a long list of Phase 2–4 items `in-progress` or `done` against classes
that **do not exist in this solution**:

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
`Beep.Winform.Data.Integrated`. The only trace is
`Beep.Winform.Controls.Tests/BeepBlockDesignerTests.cs`.

**The runtime that actually exists is the `WinForm*` family in this project**,
and it is what the IDE generates against and what the integration harness
exercises:

| Component | Files |
|---|---|
| `WinFormFormHost` | `Forms/FormHost/` — implements the whole `IBeepFormsHost` contract |
| `WinFormBlockHost` | `Forms/BlockHost/` — renders a block's fields, maps function keys |
| `WinFormBlockNavigationBar` | `Forms/BlockHost/` |
| Field presenters (8) | `Forms/FieldHost/` — text, numeric, date, boolean, combo, reflective |
| Feature panels (15) | `Forms/FeatureControls/` — LOV dialog, query, audit, locks, savepoints, security, dirty state, cross-block validation, history, multi-form, parameter list, record group, alerts, item properties |

So the tracker was not a record of this codebase. It has been replaced by a
tombstone that says so and points here; nothing in it was rewritten, because
there was no accurate row to keep, and a second status document would only have
drifted from this one. The original text remains in git history.

Add to `WinForm*FieldPresenter`s and the feature panels above:
`WinFormFormStatusBar` (§8.5), over `BeepStatusBar` (§8.7).

This is the same failure the IDE extension spent a month unwinding: status
documents describing a product that was not there. It is worth naming rather
than quietly working around.

---

## 1. The rule this phase is built on

**The WinForms layer is presentation only.** Every behaviour — modes,
navigation, CRUD, query, LOV resolution, validation, master-detail
synchronisation, locking, savepoints, audit — belongs to `FormsManager` in
BeepDM. The UI reads state from it and sends commands to it.

Two consequences, both already demonstrated:

- A capability the WPF host also needs goes in **BeepDM**, not in a host.
  `DefinitionBlockRegistrar` is the reference: the engine holds ~110 lines, and
  each host calls it in one line. An earlier attempt put that implementation in
  `WinFormFormHost` and would have needed a twin in `BeepWpfForms`.
- The UI never touches `IDataSource`. `BlockFactory` resolves a block's row type
  through `ds.GetEntityType(entityName)`; a host reimplementing that — for
  instance by generating and compiling a POCO — is doing the engine's job badly.

---

## 2. What already works (verified, not assumed)

Established by the integration harness
(`Beep.Desktop/…/IDE.Extensions/Integration/`), which boots a real `DMEEditor` +
`FormsManager`, compiles an IDE-generated form, loads it and runs it against a
live `JsonMultiFileDataSource`:

- A block declared at design time becomes a **live engine block**
  (`DefinitionBlockRegistrar`, 2026-08-01)
- Multi-file JSON resolves as independent entities; a master and a detail block
  from two different files both register
- Triggers, LOVs, validation rules and master-detail relations authored in the
  IDE reach the engine
- **Firing a trigger runs the IDE-scaffolded handler**
- Re-assigning the manager neither loses nor duplicates registrations

In the UI layer itself, present and working:

- Oracle Forms function keys on the block host — `F4` duplicate, `F6` create,
  `Shift+F6` delete, `F7` enter-query, `F8` execute-query, `F9` list-of-values,
  `F10` commit

Verified since (2026-08-01/02), each by reading state back out of the engine:

- **The form wires itself** — a block view or status bar dropped on the form
  registers and binds with no `RegisterBlock` / `Bind` call (§8.11)
- **19 trigger types fire** across query, record, block, DML and commit, asserted
  as a set (§8.12)
- **LOVs resolve** — list loads, present value validates, absent value rejected
  (§8.13)
- **CRUD round-trips to the datasource**, including delete, with the committed
  value read back out of the file
- **Query-by-example honours its operators**, not just equality (§8.10)
- **Validation, status line and record position on both platforms** (§8.4, §8.5,
  §8.9)
- **Savepoints restore, undo works, dirty items report, audit records,
  sequences/parameter lists/record groups populate** (§8.14)
- `TabIndex` honoured from item metadata
- Field controls built from the block definition by
  `WinFormFieldPresenterRegistry` — the IDE deliberately emits no controls

---

## 2a. Use Beep controls. Never raw WinForms/WPF controls.

**Rule:** every visual element comes from the Beep control libraries.
`System.Windows.Forms.TextBox`, `DataGridView`, `Label`, `Panel`,
`StatusStrip`, `ToolStrip` and their WPF equivalents do not appear in this
layer. The Beep controls carry the theming, DPI, icon and accessibility work;
a raw control bypasses all of it and looks foreign the moment a theme changes.

- **WinForms:** `Beep.Winform/TheTechIdea.Beep.Winform.Controls/`
- **WPF:** `Beep.WPF/TheTechIdea.Beep.WPF.Controls/`

The existing field presenters already follow this — `WinFormTextBoxFieldPresenter`
→ `BeepTextBox`, `WinFormComboFieldPresenter` → `BeepComboBox`,
`WinFormDateFieldPresenter` → `BeepDatePicker`, `WinFormNumericFieldPresenter`
→ `BeepNumericUpDown`, `WinFormBooleanFieldPresenter` → `BeepCheckBoxBool`.
Everything below extends that habit; nothing here needs a new control written.

### The mapping

| Forms need | WinForms control | WPF control |
|---|---|---|
| Multi-record / tabular block | **`BeepGridPro`** (`GridX/`) | **`BeepDataGrid`** |
| Single-record block surface | `BeepDataRecord` (`DataControls/`) | `BeepDataRecord` |
| Navigation bar (first/prev/next/last, insert, delete, save, rollback, query) | **`BeepDataNavigator`** (`DataControls/`) | **`BeepDataNavigator`** |
| Text item | `BeepTextBox` | `BeepTextBox` |
| Numeric item | `BeepNumericUpDown` | `BeepNumericUpDown` |
| Date item | `BeepDatePicker` / `BeepDateDropDown` | `BeepDatePicker` / `BeepDateDropDown` |
| Boolean item | `BeepCheckBoxBool` | `BeepCheckBox` |
| List item | `BeepComboBox` / `BeepListBox` | `BeepComboBox` / `BeepListBox` |
| Password item | *(none — use `BeepTextBox`)* | `BeepPasswordBox` |
| List of values (F9) | `BeepLovPopup`, `BeepListofValuesBox` (`Lovs/`) | `BeepLovPopup`, `BeepListofValuesBox` |
| Item validation marker | **`BeepValidationBadge`** | **`BeepValidationBadge`** |
| Query-by-example criteria | *field presenters — no filter control (see §8.8)* | *field presenters — no filter control (see §8.8)* |
| Status / message line | **`BeepStatusBar`** (`StatusBars/`, added §8.7) | **`BeepStatusBar`** |
| Alerts, prompts, pickers | `BeepDialogManager`, `BeepMessageDialog`, `BeepInputDialog`, `BeepListDialog` | `BeepDialogManager`, `BeepDialog` |
| Toast / notification | `BeepNotification`, `BeepNotificationManager` | `BeepNotification`, `BeepNotificationManager` |
| Labels, buttons, panels | `BeepLabel`, `BeepButton`, `BeepPanel` | `BeepLabel`, `BeepButton`, `BeepPanel` |
| Master/detail splitting | `BeepMultiSplitter`, `BeepLayoutControl` | `BeepMultiSplitter`, `BeepSplitContainer` |
| Waiting / progress | `BeepWait`, `BeepProgressBar` | `BeepWait`, `BeepProgressBar` |

### Two parity gaps found while scanning

1. ~~**WinForms has no `BeepStatusBar`; WPF does.**~~ — **closed by §8.7
   (2026-08-01).** Added to the WinForms control library, where it belongs, not
   composed in this project and not a `System.Windows.Forms.StatusStrip`.
2. ~~**WPF has no `Filtering/` equivalent** of `BeepFilter` / `BeepFilterRow`.~~
   — **resolved by §8.8 (2026-08-01):** query-by-example uses field presenters,
   not a filter control, on either platform, so §8.3 never depended on this. WPF
   filtering exists inside `BeepDataGrid`; a standalone control would be new
   product work, and is deferred.

### How to use them

**`BeepGridPro` is hosted, not reimplemented.** It already has
`DataSource` (`BeepGridPro.Properties.cs`), a `Columns`
(`BeepGridColumnConfigCollection`) surface, and a full editor set —
`BeepGridTextEditor`, `BeepGridNumericEditor`, `BeepGridComboBoxEditor`,
`BeepGridDateDropDownEditor`, `BeepGridCheckBoxEditor`, `BeepGridMaskedEditor`.
The block host's job is to translate, in both directions:

- block items → `BeepColumnConfig` entries, choosing the editor from the same
  `FieldTypeMapper` category the single-record presenters already use, so a
  field looks and behaves the same in either mode
- `IBeepFormsHost.GetBlockData(blockName)` → `DataSource`
- grid row selection → `MoveToRecordAsync`, and
  `GetCurrentBlockRecordIndex` → `BeepGridCurrentRow`

**`BeepDataNavigator` is wired, not rebuilt.** It already raises
`NewRecordCreated`, `SaveCalled`, `DeleteCalled`, `EditCalled`,
`RollbackCalled`, `QueryCalled`, `FilterCalled`. Those map one-to-one onto host
methods — `InsertBlockRecordAsync`, `SaveBlockAsync`,
`DeleteBlockCurrentRecordAsync`, `RollbackBlockAsync`, `EnterQueryModeAsync`.
`WinFormBlockNavigationBar` should be checked against it before any new button
is drawn.

**Consistency rule:** the same logical item must resolve to the same Beep
control in single-record and multi-record mode, and to the matching control on
WPF. The mapping table above is the single place that decides it — extend the
table rather than making the choice inline in a presenter.

---

## 3. The gaps, in the order they block a real application

### 8.1 — Multi-record (tabular) block mode — ✅ DONE (2026-08-01)

Proven end to end by the integration harness against a live engine and a
multi-file JSON datasource: `PresentationMode="Grid"` switches the block host to
host `BeepGridPro`, columns are built from the block's fields with editors from
`WinFormFieldPresenterRegistry.ResolveColumnType`, the grid binds to
`GetBlockData`, rows load, and selecting a row moves the **engine's** record
pointer.

`WinFormBlockHost.GridMode.cs` hosts the control and translates in both
directions. It contains no grid drawing and no `DataGridView`.

**Five defects surfaced on the way, none of them in the grid code:**

| # | Defect | Where |
|---|---|---|
| a | `FieldTypeMapper` matched only bare type names (`int`), so `System.Int32` and every other qualified name fell through to `Text` — numeric/date/boolean fields rendered as text boxes in **both** modes, and the IDE assigned text editor keys | BeepDM |
| b | `DesignerBlockGenerator` never emitted `PresentationMode`, silently forcing every generated block to single-record | IDE |
| c | Six datasources each answered `GetEntityType` differently and none returned an `Entity`-derived type (`DMTypeBuilder` → `object`; `Dictionary<string,object>` with "could implement later"), so `UnitofWork<T>`'s `T : Entity` constraint could not be met and blocks held no records | BeepDM — now one `EntityTypeFactory` |
| d | `UnitOfWorkWrapper` did not forward `IAggregatable`, and `GetBlockCount` is `(uow as IAggregatable)?.Count() ?? 0` — so **every** wrapper-backed block reported zero records regardless of content | BeepDM |
| e | `FormsManager.LogError` accepted an exception and logged only the caption, making every failure in the manager undiagnosable | BeepDM |

(e) is the one that mattered most: it was fixed first, and it immediately named
the cause of the next failure. A logger that drops the exception costs more than
the bugs it hides.

`CSVDataSource.GetEntityType` also ignored its `EntityName` argument and always
returned the *first* entity's shape. Fixed by the same consolidation.

**Two harness lessons, not product defects:**

- A `WinFormFormHost` belongs to the thread that constructed it. The harness now
  runs UI work on a dedicated STA thread with a real message loop
  (`RunOnUiThread` / `Pump`), and detaches the main-thread host first — a manager
  notifies *every* attached host, so two hosts on two threads cross-thread.
- `BeepGridPro.SelectCell` is highlight-only **by design** ("Do not raise
  RowSelectionChanged on highlight-only changes"). User selection flows through
  `GridInputHelper` → `OnRowSelectionChanged`. An earlier draft of this plan
  called that a control defect; it is not. The harness drives the same entry
  point the input path uses.

---

### 8.1 (original) — Multi-record (tabular) block mode — **P0**

`WinFormBlockHost` renders one record at a time. An Oracle Forms block is
routinely **multi-record**: a scrolling tabular region showing N rows with the
current record highlighted. Most master-detail screens are a single-record
master over a multi-record detail, so without this the most common Forms layout
cannot be built.

- `BlockDefinition.PresentationMode` already exists and is already carried
  through the IDE's block editors — it is the natural switch
- The engine already exposes what a grid needs: `GetBlockData`,
  `GetBlockRecordCount`, `GetCurrentBlockRecordIndex`, `MoveToRecordAsync`
- **Host `BeepGridPro`** (WinForms) / **`BeepDataGrid`** (WPF) — see §2a. No grid
  is written, and no `DataGridView` appears anywhere

**Acceptance:** a detail block declared multi-record shows all rows in
`BeepGridPro`, the current row tracks `GetCurrentBlockRecordIndex`, selecting a
row moves the engine's record pointer, and each column uses the same Beep editor
its field uses in single-record mode.

### 8.2 — Master-detail synchronisation visible in the UI — ✅ DONE (2026-08-01)

Proven by the integration harness: with a master over `orders` and a tabular
detail over `orderlines`, moving the master re-queries the detail and the grid
rebinds. Order 1 has two lines, order 2 has one, so the row set must change —
and does.

The engine machinery was already there (`SynchronizeDetailBlocksAsync`, called
from the navigation path). What stopped it working was one line in
`UnitofWork.Get(List<AppFilter>)`:

```csharp
public ObservableBindingList<T> Units => IsFilterOn ? _filteredunits : _units;
```

`Get(filters)` set `IsFilterOn = true` for any non-null filter, then loaded the
datasource's rows into **`_units`** — while `Units` now read
**`_filteredunits`**, which only the in-memory filter branch ever populates. So
a filtered query fetched the correct rows, stored them, and `Units` returned
**null**. `Count` is `Units?.Count ?? 0`, so every caller saw zero records with
no error logged anywhere: **master-detail emptied every detail block on every
master move.**

`IsFilterOn` means "an in-memory filter is masking Units". On the datasource
path the filter was applied at the source and the returned rows *are* the set,
so it now stays false; the in-memory branch keeps it.

**Two further defects on the same path**, both found while chasing this and both
real in their own right — `RecordPropertyAccessor` and
`UnitofWorkDataHelper.ConvertToEntity` each reflected for *properties* against
records that are `IDictionary<string, object>` (what file and cache datasources
return). The first made `FormsManager` read a null master key and clear the
detail; the second turned fetched rows into blank entities. Both would have bitten
the moment the `Units` bug was fixed.

**Method note.** Five hypotheses were tried before the cause was found, three of
them wrong. What ended it was a discriminating test rather than more reasoning:
query the datasource directly with the same filter the sync builds (2 rows —
correct), then log the one hop between fetch and storage (`Units` null). Two log
lines collapsed a search space that days of inference had not. When a fix does
not move the symptom, instrument the boundary instead of guessing at the next
layer.

---

### 8.3 — Query mode round-trip in the UI — ✅ DONE (2026-08-01)

Proven end to end by the integration harness: `F7` on a record-mode block over
`orders`, type `Qty = 50` into the field by name, `F8` — the block comes back
with 1 of its 3 rows, and leaving query mode returns it to data entry. Five
checks, all driven through `IBeepFormsHost`; the host contributes no query logic
of its own.

**The engine had two ENTER_QUERY implementations, and hosts called the wrong
one.** `FormsManager.EnterQueryModeAsync` (ModeTransitions) is the real one —
unsaved-change validation, related-block validation, block clear, block-enter
event. `FormsManager.EnterQueryAsync` (BasicDataOps) was a second, four-line
version that assigned `Mode` and returned true. `WinFormFormHost` calls
`EnterQueryAsync`, so **in practice ENTER_QUERY never validated, never cleared
the block, and never raised its event** — Oracle Forms clears the block on
ENTER_QUERY, and this silently did not. `EnterQueryAsync` now delegates; there is
one implementation.

**The mode it set was also wrong.** Both wrote `DataBlockMode.Query`, but the
enum documents `EnterQuery` as "the user is typing example criteria" and `Query`
as "results are loaded and editable". Nothing in the engine ever *wrote*
`EnterQuery` — three sites read `Query || EnterQuery` and so masked it. Two
consequences: `CreateBlockInfo` registers a block with `Mode = Query`, so
`EnterQueryModeAsync`'s own "already in query mode" guard matched on the very
first call and returned success without doing anything; and
`WinFormBlockHost.SyncFromManager` derives `_queryMode` from
`Mode == EnterQuery`, which could never be true, so the block dropped out of
query mode on the next sync. Fixed at both the guard and the assignment.

**Method note.** The mode fix alone did not move the symptom, and the reflex was
to suspect a stale DLL — the third such suspicion this session. It was not:
`GetBlockInfo` reads the live object, which ruled out staleness in one step and
pointed at the assignment never running. Grepping every writer of `Mode` — rather
than re-reading the method already believed to be at fault — showed the second
implementation immediately. When an edit to the obvious code path changes
nothing, check that the caller reaches *that* path before doubting the build.

---

### 8.4 — Validation surfaced at the item — ✅ DONE (2026-08-01)

Proven by the integration harness against the rule the IDE emitted (`Qty_Range`,
`0..100`, "Quantity must be 0-100."). Setting Qty to 500 blocks the commit, marks
the field with a **`BeepValidationBadge`** carrying the rule's own message,
correcting it to 50 clears the marker, and the corrected record commits — and the
committed value is then read back **out of `orders.json`**, not inferred from a
`true` return.

UI side is thin: `WinFormFieldPresenterBase.ValidationError` shows or clears the
badge (a badge attaches to its target's *parent*, so it re-applies on
`ParentChanged` for fields marked before layout), and the block host asks the
engine for the message instead of composing "X is invalid" itself. No validation
logic was added to the host.

**Five engine defects sat behind this, each silent.** In order of discovery:

1. **`ValidationManager.GetApplicableRules` filtered rules by exact timing.**
   `ValidationRule.Timing` defaults to `OnBlur` and the IDE emits no timing, so
   every authored rule was evaluated on blur and *nowhere else* — commit included.
   A record whose field the user never visited committed unvalidated. `OnCommit`
   now matches every rule, like `Manual`: a timing says when a rule fires *early*,
   not when it may be skipped.

2. **`FormsManager` raised `RuntimeBinderException` on every field change.** Its
   ItemChanged handler called `unitOfWork.Units.IndexOf(e.Item)`. `Units` is
   `dynamic`, and a dynamic call binds its *statically typed* arguments by their
   compile-time type — `e.Item` is declared `Entity`, the list is
   `ObservableBindingList<orders>`, so the binder looked for `IndexOf(Entity)`,
   found only `IndexOf(orders)`, and threw. The throw escaped through the property
   setter, so `SetFieldValue` reported failure and **no edit ever reached a
   record**. Now uses non-generic `IList.IndexOf(object)`.

3. **Authored primary keys never reached the entity structure.** A schemaless
   source returns an empty `PrimaryKeys` — nothing in a `.json` file says which
   column is the key — and `DefinitionBlockRegistrar` passed the structure through
   untouched, discarding `BlockFieldDefinition.IsPrimaryKey`. `UnitofWork` logged
   "Entity dont have a primary key" and refused every update. Reads were
   unaffected, which is why it stayed invisible: forms queried and navigated
   normally and dropped edits.

4. **`UnitofWork.Commit` required transaction support to save anything.** It
   called `BeginTransaction` unconditionally and aborted if the result was not
   `Ok`. Every non-transactional source — JSON, CSV, file, cache — answers
   "Transactions not supported", so the commit failed *before writing*. It now
   asks `DataSourceCapabilityMatrix` whether the source has transactions rather
   than inferring capability from a failure; a source that does support them and
   fails to begin one is still a hard error.

5. **`JsonMultiFileDataSource` never wrote, then wrote to the wrong row.**
   `SaveAllDirtyEntities()` had no caller anywhere — the only flush was in
   `Dispose`, so a successful commit left the change in memory. Made
   write-through. That exposed the worse half: `UpdateEntity` looked the row's id
   up with `item["id"]`, an ordinal case-sensitive lookup that never matches a
   file written with `"Id"`, and then **fell back to matching on the first non-id
   field**. Updating `{Id:1, Qty:50}` found no id match, located a different row
   that happened to have `Qty` 50, and copied every property onto it — including
   `Id`. Two rows ended up sharing an id and the intended row was never touched.
   Lookups are now case-insensitive, and an update carrying an identifier fails
   rather than falling back to a data-value match.

**Method note.** The `true`-returning commit is why the acceptance check reads the
file instead of the return value. Four of these five reported success while doing
nothing; only #2 produced an exception, and that one was swallowed and throttled
to one log line per minute, so it looked like a single anomaly rather than every
edit failing. Two habits did the work: assert the *effect*, not the API result,
and when a log says something threw, print the inner exception before theorising —
`TargetInvocationException`'s own message names neither the error nor its origin.

---

### 8.7 — `BeepStatusBar` for WinForms — ✅ DONE (2026-08-01)

Added to the **control library** (`Beep.Winform/…Controls/StatusBars/`), not
composed locally and not a `System.Windows.Forms.StatusStrip`. Every theme
already defined `StatusBarBackColor` / `ForeColor` / `BorderColor` — the contract
was there and only the control was missing, so it consumes those directly.

A message on the left, right-aligned keyed segments on the right; segments are
addressed by key (`SetSegment("position", …)`) so a caller can refresh one
without knowing what else is on the bar. Severity reuses
`Badges.ValidationState` rather than adding a fourth near-identical severity enum
to that assembly. It takes no dependency on the Forms contracts — the controls
library consumes `DataManagementModels` as a **NuGet package**, so referencing
working-tree types there would be a version trap.

### 8.5 — Record status and the status line — ✅ DONE (2026-08-01)

`WinFormFormStatusBar : BeepStatusBar` binds to `IBeepFormsHost` and shows
`Record n of m`, the block mode, and a `Changed` indicator, plus the form's
message line. Thin: every value is read from the engine
(`GetBlockStatus`, `MessageRaised` / `MessageCleared`); the class decides nothing.

Verified end to end — position tracks navigation, an engine `SetMessage` reaches
the line with its severity, and `ClearMessage` empties it.

**Three engine defects, the same shape as §8.3's.**

1. **The documented UI event had no publisher on the form path.**
   `IDataOperations.OnMessage` is commented "UI layers subscribe to
   OnMessage/OnMessageCleared to display messages", and only
   `MessageQueueManager` raises it — but `FormsManager.SetMessage`, the
   form-level MESSAGE built-in every host calls, just stored the text in a field
   and set `Status`. Two implementations of "set the message"; hosts reached the
   one that told nobody, so a status line driven off the documented event stayed
   blank however many messages the form set. It now also publishes.

2. **`SetMessage` enqueues behind an undismissed message.** Correct for a message
   the user must acknowledge, wrong for a status line — Oracle Forms' MESSAGE
   overwrites it. Added `IMessageQueueManager.ReplaceMessage`, which the
   form-level built-in uses.

3. **Every operation's status notification queued forever.** `FormsManager`
   calls `ShowInfoMessage` / `ShowSuccessMessage` / … after each query, save,
   navigation and delete, and those routed to `SetMessage`. Nothing in a running
   form dismisses a status message, so the *first* operation's message became
   current and all ~25 later ones queued behind it: an unbounded queue, a line
   frozen on the first thing that ever happened, and — since `ClearMessage`
   advances the queue — a clear that replayed a message from minutes earlier.
   The four `Show*Message` methods now replace; `SetMessage` keeps queue
   semantics for callers that want them. A check pins the queue at zero.

**Method note.** The clue was the failure text: after `ClearMessage` the line read
*"Query executed successfully for block 'Ord'"* — not blank, and not the message
just set. A stale value rather than an empty one meant something was actively
writing, which ruled out "the event never fires" and pointed straight at the
queue. Printing `GetQueuedMessageCount` (3) turned a guess into a fact in one run.

---

### 8.6 — WPF runtime parity — ✅ DONE (2026-08-01)

New harness: `Beep.WPF/TheTechIdea.Beep.Wpf.Data.Integrated.Integration/`
(`dotnet run --project BeepWpfFormsIntegration.csproj`). It boots a real
`DMEEditor` + `FormsManager`, stands up the same `JsonMultiFileDataSource`, and
runs the WinForms invariants against `BeepWpfForms`: design-time blocks become
live engine blocks, the master loads 3 rows, moving the master re-queries the
detail (2 lines then 1), a `BeepWpfBlock` binds and builds field presenters, and
`GetBlockStatus` reports through the host. Eleven checks.

**It passed on the first run, and that is the point.** The WPF host had never
been instantiated by anything — it only ever had to compile — yet every engine
defect fixed for WinForms in §8.4 and §8.5 (authored primary keys, the
transaction-capability check, the dynamic `IndexOf`, JSON write-through, the
message queue) was already fixed for WPF, because all of them were fixed in
BeepDM. That is the "one place" rule paying for itself: the WPF host contributes
one line of block registration and inherits the rest.

**§8.9 extended it to feature parity** — as first written this section proved
runtime *bring-up*, not that WPF had §8.4 and §8.5. It did not. See below.

**Separate project, not a section in the WinForms harness.** The WPF stack is
`net9.0-windows` and project-references BeepDM, so it binds the net9.0 build of
`DataManagementEngine`; the WinForms harness is `net10.0-windows`. Loading both
into one process would put two builds of the same assembly identity side by
side — a `MissingMethodException` waiting to happen a long way from its cause.

### 8.8 — WPF `BeepFilter` equivalent — ✅ CONFIRMED, none needed (2026-08-01)

**Query-by-example uses no filter control on either platform.**
`WinFormBlockHost.ExecuteQueryAsync` builds criteria from the field presenters'
`QueryValue` / `QueryOperator` into `QueryCriterion` and hands them to the
engine; `BeepFilter`, `BeepFilterRow` and `BeepAdvancedFilterDialog` are not
referenced anywhere in the WinForms thin UI. So §8.3 never depended on them and
the gap does not block parity.

WPF does have filtering, scoped to the grid — `GridFilterRow`,
`BeepDataGrid.FilterBuilder`, `BeepDataGrid.FilterPopup`,
`GridAdvancedFilterHelper`. What it lacks is a *standalone* filter control
matching WinForms' `Filtering/` family. Building one is new product work rather
than parity, so it is recorded here and deferred, not invented.

---

### 8.9 — WPF feature parity for validation and status — ✅ DONE (2026-08-01)

§8.6 proved the WPF host *runs*. It did not give WPF §8.4 or §8.5, and saying
"WPF runtime parity — done" invited the reading that it had. AGENTS.md §8 wants a
counterpart or an explicit deferral; this is the counterpart.

**Validation marker.** `WpfFieldPresenterBase` already pushed `ValidationError`
and `HasError` onto the editor through `IBeepValidatable` — and **no editor
template in the WPF library binds `HasError`**, so a rejected field carried the
error state and rendered nothing at all. Only `TextWpfFieldPresenter` did
anything (inline `ErrorText`). The base now drives a `BeepValidationBadge` — the
same control the WinForms side uses, from the WPF library — exposed as
`ValidationMarker` for the host to place, with `HasValidationMarker` to assert.

Exposed rather than self-attached because WPF has no badge manager: an adorner
needs an `AdornerDecorator` ancestor, which a block built outside a window does
not have, and that would have made the marker untestable headless.
`BeepWpfBlock` already lays fields out as label | editor, so it gained a third
`Auto` column for the marker. The presenter owns state, the host owns placement.

**Status line.** `BeepWpfStatusBar` existed as a shell control with a message and
form/block labels, and nothing bound it to a host — so the engine's MESSAGE
built-in had no WPF surface. `WpfFormStatusBar` adds the binding and the record
segments on top of it rather than building a second status bar.

The WPF harness grew from 11 checks to 19: an authored rule blocks the commit,
the field carries the rule's message, the badge shows and clears, the position
segment tracks navigation, and a message reaches the line with its severity and
clears.

### 8.10 — Query-by-example ignored its operators — ✅ FIXED (2026-08-01)

`JsonMultiFileDataSource.MatchesFilters` compared every filter with string
equality and **never read `AppFilter.Operator`**, so `Qty > 100` silently behaved
as `Qty = 100`. §8.3's round-trip did not catch it because the check used
equality, which is the one operator the bug happens to implement correctly.

Operators now honoured: `=`, `<>`/`!=`, `>`, `>=`, `<`, `<=`, `like`/`not like`
(with `%` and `_`), `in`/`not in`, `between` (inclusive), `is null`/`is not
null`. Comparison is numeric when both sides parse as numbers, then date, then
ordinal case-insensitive — so `9 < 100` orders as a number, not as text. **An
operator this source does not implement now rejects the row** rather than
falling through to equality; a silent wrong answer is worse than an empty one.

The new check discriminates: orders are 5, 50 and 500, so `Qty > 100` must return
exactly the 500. Under string equality it returned none.

---

### 8.11 — The form wires itself — ✅ DONE (2026-08-01)

Nothing discovered anything: every block view had to be passed to
`RegisterBlock` and every status bar to `Bind`, by hand, in the right order,
after the manager was assigned — and the harness had to do it too. If a harness
has to hand-wire a form, so does every user of the product.

`WinFormFormHost` now walks the form it lives on and adopts what it finds.
Discovery is idempotent and re-runs on `ControlAdded`, so design-time drops,
runtime additions and controls created before the manager exists all wire the
same way. Adoption and binding are separate steps: discovery runs as soon as a
control appears, usually before a manager exists, and a status surface binding
then would immediately read block status — which the host throws on by design.

The three controls also carry `[ToolboxItem]` / `[DisplayName]` / `[Category]`
for the first time, so they can be dragged from the Toolbox at all, and
`BlockName` is marked for designer serialisation so it round-trips through
`.Designer.cs`.

**Proof:** a `Form` with a host, a block view and a status bar; one line of
manager assignment; no `RegisterBlock`, no `Bind`, no `SyncFromManager`.

### 8.12 — The trigger matrix: 1 verified → 19 — ✅ DONE (2026-08-02)

One trigger type was ever verified to fire (`WhenValidateItem`), out of the ~50
the engine has fire points for. *Registered* is not *fires*: a registration can
be accepted, stored, returned by `GetBlockTriggers`, and never invoked. The
harness now arms a handler per type across query, record, block, DML and commit,
drives the ordinary operations, and asserts all 19.

Four engine defects, all of the same shape — a trigger that could be registered
and could never run:

1. **`PreRecord` / `PostRecord` / `WhenNewRecordInstance` had no fire point.**
   Present in the enum and in `TriggerLibrary`'s catalogue; never invoked.
   `WHEN-NEW-RECORD-INSTANCE` is the trigger most Oracle Forms code uses to react
   to the cursor moving. Now fired around navigation.
2. **`PreBlock` / `PostBlock` had no fire point** — `WhenNewBlockInstance` fired
   on a block switch and its companions did not.
3. **`PreCommit` / `PostCommit` could never fire.** `TriggerManager` registers a
   form trigger under `FormName ?? "DEFAULT"` and looks one up under
   `formName ?? "DEFAULT"`, but `CommitFormAsync` passed
   `_currentFormName ?? "FORM"` — searching a bucket nothing is registered in.
   Two defaults that disagreed, so any form whose name the engine was never told
   could not fire a commit trigger.
4. **`WhenValidateRecord` had no fire point.** Now fires from
   `ValidateRecordForOperation`, the gate INSERT/UPDATE/DELETE pass through.

**And a broken delete, found through a trigger.** `PreDelete` fired and
`PostDelete` did not, because `PostDelete` is only reached on a *successful*
delete. `JsonMultiFileDataSource.DeleteEntity` matched `JObject`,
`Dictionary<string,object>` and an `int` index — and `UnitofWork.DeleteAsync`
passes the entity itself, so a POCO fell through every branch and it reported
"No matching record found to delete", blaming the data. It also took the *first*
property as the identifier and compared case-sensitively — the same two faults
the update path had. The missing trigger was the symptom; the broken delete was
the cause.

**Method note.** The matrix first appeared to deadlock the engine, and it did
not. Creating WinForms controls installs a `WindowsFormsSynchronizationContext`
on that thread which survives the UI section with no message loop; the engine
awaits without `ConfigureAwait(false)`, so continuations were posted to a dead
context. A stack dump — main thread blocked on a task, every pool thread idle,
no Beep frame anywhere — said "orphaned continuation", not "lock contention".
Running the matrix on a pool thread fixed it. **The underlying hazard is real
for any WinForms app calling an engine method with `.Result` on the UI thread;
the durable fix is `ConfigureAwait(false)` through the engine's async paths, and
it has not been made.**

### 8.13 — The LOV resolves — ✅ DONE (2026-08-02)

The harness proved the engine *held* the LOV the IDE registered and stopped
there. Now it loads the list from the LOV's own datasource, validates a present
value, rejects an absent one — an LOV that accepts anything restricts nothing —
and resolves a value back to its record. Four checks, all green on the first
run; this one closed a gap rather than finding a defect.

---

### 8.14 — The feature panels' capabilities — ✅ DONE (2026-08-02)

The eighteen `Forms/FeatureControls/` panels are pure pass-throughs to
`IBeepFormsHost` — `WinFormSavepointPanel.Create` is one line calling
`Host.CreateSavepoint`. So testing a panel means testing the capability behind
it, and **none of them had any coverage.** The sweep covers savepoints, locking,
undo, dirty state, sequences, parameter lists, record groups, item properties,
cross-block validation and audit.

It runs on its **own `FormsManager`**. These operations lock records, enable undo
and leave edits behind; on the shared manager they broke the trigger matrix and
the LOV section for reasons unrelated to what those test.

**Six defects, all "reports success, does nothing":**

1. **Undo was entirely inert.** `UnitOfWorkWrapper` carried
   `CanUndo`/`CanRedo`/`Undo`/`Redo` and never declared `IUndoable`, and
   `FormsManager` reaches undo through `GetUnitOfWork(name) as IUndoable`. Blocks
   are registered with the wrapper, so the cast was always null and
   `SetBlockUndoEnabled` / `UndoBlock` / `CanUndoBlock` silently no-opped. This is
   the **same defect as the `IAggregatable` gap fixed earlier on the same class** —
   an optional-capability interface is silent when unimplemented, which is what
   makes dropping one so expensive.
2. **`UnitofWork.Rollback()` required transaction support.** It called
   `DataSource.EndTransaction` unconditionally and failed if the result was not
   `Ok` — the **identical defect fixed in `Commit()`, in its sibling method,
   missed at the time.** Rolling back was impossible on JSON/CSV/file/cache, and
   savepoint restore died with it.
3. **The host bypassed the engine's savepoint wrappers.** It called
   `RequireManager().Savepoints` — the *store*, whose two-argument
   `CreateSavepoint` captures no values and whose `RollbackToSavepointAsync` only
   prunes later savepoints. So it created savepoints that snapshotted nothing and
   rolled back without restoring, reporting success both times.
   `CreateBlockSavepoint` and the data-aware `RollbackToSavepointAsync` existed on
   `FormsManager` and were on **no interface**, so the host could not reach them;
   both are now on `IUnitofWorksManager`.
4. **The item-property store was never populated.**
   `RegisterItemsFromEntityStructure` had no caller anywhere, so no block had
   items: `GetDirtyItems` always empty, `MarkItemDirty` finding nothing to mark,
   `WinFormDirtyStatePanel` permanently blank. Seeded at block registration.
5. **Record-group population was unimplemented** — honestly declared, returning
   false with a reason: "pending a dedicated `IDataSource.CreateUnitOfWork` API".
   The premise no longer held: a record group is a queried row set, and
   `GetEntity` returns rows.
6. **Audit never reached the store on a block save.** `AuditManager` accumulates
   pending changes and flushes only in `CommitFormAsync`, but the UI saves through
   the block path — Save button, F10, `WinFormBlockHost.SaveAsync`. Every audit
   entry from normal editing stayed pending forever.

**Method note — the false-alarm rate.** Four failures in this section were the
harness's own doing, not the engine: auditing is off by default (reasonable);
`LockCurrentRecordAsync` short-circuits to `true` when the lock mode is `None`
(coherent — locking is off, nothing blocks the edit); undo is carried by the
block's *collection*, so it must be enabled **after** the query that loads it;
and a deliberately always-failing cross-block rule, left registered, failed every
later commit. Shared-manager ordering produced more false alarms here than real
defects. If this section grows, give each check its own manager rather than
cleanup calls.

### 8.15 — The transactional commit branch — ✅ EXERCISED (2026-08-02)

`UnitofWork.Commit` and `Rollback` both branch on
`DataSourceCapabilityMatrix.Supports(..., SupportsTransactions)`. Everything ran
against JSON, which reports false, so **the `true` side had never executed** —
and two defects were fixed in it blind (Commit's unconditional
`BeginTransaction`, then Rollback's, the second surfacing only because savepoint
restore depended on it).

`DatasourceType` is settable, so a source can be made to *claim* transactions it
does not implement. That drives the engine down the transactional path and pins
the contract: when a source claims support and then fails to begin a
transaction, the commit aborts and **writes nothing**.

**This proves the engine's branch, not a driver's.** No RDBMS datasource exists
in this engine assembly — the SQL drivers are separate plugins — so running
against a real database remains open and needs a target chosen.

---

### 8.2 (original) — Master-detail synchronisation visible in the UI — **P0**

The engine synchronises detail blocks on master navigation. Nothing verifies the
UI reflects it: moving the master must re-query and repaint the detail.

**Acceptance:** an integration test drives `MoveNextAsync` on the master and
asserts the detail block's row set changed to match the new master key.

### 8.3 (original) — Query mode round-trip in the UI — **P1**

`WinFormQueryPanel` exists and the engine has
`EnterQueryModeAsync` / `ExecuteQueryByExampleAsync` / `ExitQueryModeAsync`.
Unverified: that entering query mode makes fields accept criteria instead of
data, that `F8` packages them, and that results land back in the block.

**Acceptance:** `F7` → type a criterion → `F8` → the block shows only matching
rows, driven end to end through the manager.

### 8.4 (original) — Validation surfaced at the item — **P1**

Use **`BeepValidationBadge`** (present in both libraries) for the field marker —
do not hand-draw an error adornment.

`ValidateItem` and `RecordValidationResult` exist, as does
`WinFormCrossBlockValidationPanel`. Unverified: a `When-Validate-Item` failure
visibly marks the offending field and blocks navigation off it.

**Acceptance:** a rule the IDE authored rejects a value, the field shows the
error, and the record cannot be committed until it is fixed.

### 8.5 (original) — Record status and the status line — **P2**

Oracle Forms shows record status (new / changed / query) and a message line.
`GetBlockStatus`, `SetMessage` and `GetDirtyItems` exist on the host; there is
no evidence of a surface rendering them.

WPF has **`BeepStatusBar`**. WinForms does not — see the parity gap in §2a. Add
it to the WinForms control library rather than composing something local here,
and never fall back to `System.Windows.Forms.StatusStrip`.

### 8.6 (original) — WPF parity — **P2**

`BeepWpfForms` carries the same one-line registrar call and compiles, but **no
WPF form is instantiated at runtime by any harness**. Per `AGENTS.md`, a
WinForms-only capability needs a WPF counterpart or an explicit deferral. A WPF
mirror of the integration harness would close it.

---

## 4. Sequencing

| # | Item | Priority | Depends on |
|---|---|---|---|
| 8.0 | Retire or rewrite `todo-tracker.md` | ✅ retired | — |
| 8.1 | Multi-record block mode | ✅ done | — |
| 8.2 | Master-detail sync in the UI | ✅ done | 8.1 |
| 8.3 | Query-mode round-trip | ✅ done | — |
| 8.4 | Item-level validation display | ✅ done | — |
| 8.5 | Record status + message line | ✅ done | 8.7 |
| 8.6 | WPF runtime parity | ✅ done | — |
| 8.7 | `BeepStatusBar` for WinForms (control library) | ✅ done | — |
| 8.8 | Confirm or add WPF `BeepFilter` equivalent (control library) | ✅ confirmed — not needed | 8.3 |
| 8.9 | WPF feature parity for validation + status | ✅ done | 8.4, 8.5, 8.6 |
| 8.10 | Query-by-example operators honoured | ✅ done | 8.3 |
| 8.11 | The form wires itself (no hand-registration) | ✅ done | — |
| 8.12 | Trigger matrix: 19 types verified | ✅ done | — |
| 8.13 | LOV resolves at runtime | ✅ done | — |
| 8.14 | Feature-panel capabilities | ✅ done | — |
| 8.15 | Transactional commit branch exercised | ✅ done | — |

8.0 first, and it is not busywork: the next person to plan from that tracker will
build against classes that do not exist.

---

## 5. How each item is proved

Extend the existing integration harness rather than adding another. It already
boots the engine, compiles an IDE-generated form and runs it on a live
multi-file JSON datasource; each item above is a few assertions on top of that.

The bar is the one this codebase has had to learn twice: **reading state back
out of the engine, not out of the UI's own fields.** A UI test that asserts what
the UI just set proves nothing — that is precisely how the registration path
stayed green for months while emitting code that named methods existing nowhere.

---

## 6. What is still open (2026-08-02)

Everything in §4 is done. This is what stands between the current state and
"Oracle Forms ready", stated plainly so it is not mistaken for finished.

| Open | Why it matters |
|---|---|
| **No real database.** No RDBMS datasource exists in this engine assembly; the SQL drivers are separate plugins. | `UnitofWork.Commit`'s transactional path is exercised only by a source that *claims* support (§8.15) — that proves the engine's branch, not a driver's. Two defects were already fixed in that code blind. Needs a target chosen. |
| **`ConfigureAwait(false)` is absent from the engine's async paths.** | Any WinForms caller using `.Result` or `.Wait()` on the UI thread deadlocks: the continuation is posted to a context that is not pumping. The harness hit this and works around it with `Task.Run`; the hosts avoid it only by awaiting properly. |
| **The extension is never driven inside Visual Studio.** | `command-reachability.py` is static analysis. Nothing exercises the Remote UI dialogs in the experimental hive. |
| **`UpdateCurrentRecordAsync` is not on `IUnitofWorksManager`.** | A host cannot reach the explicit update path at all — only `SetFieldValue` plus commit. The harness casts to `FormsManager` to test it. |
| **Panels still uncovered:** multi-form (`CallFormAsync`), timers, history, security. | Same pass-through shape as §8.14, so the same approach applies. |

### The pattern worth carrying forward

Nearly every defect found across §8.1–§8.15 reported success while doing
nothing: a null-yielding `as` cast, a `?? 0`, an interface a wrapper forgot to
declare, a method with no caller, a snapshot captured and never restored, a
buffer flushed on a path the UI never takes. **A green build and a `true` return
prove nothing here.** Assert the observable effect — read the row back out of
the file, count the triggers that actually fired, check the marker is visible.
That is what turned "it compiles" into evidence.
