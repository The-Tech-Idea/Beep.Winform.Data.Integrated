# Import/Export Wizard — Enhancement Plan

**Date:** 2026-03-03  
**Based on:** Oracle MySQL Workbench Table Data Wizard, Pega Data Import Wizard, current codebase state  
**Scope:** UX re-architecture + new functionality for all 5 files in the ImportExport folder

---

## 1. Reference Analysis

### Oracle MySQL Workbench (Table Data Export/Import Wizard)
Key lessons:
- **Source step** shows a live row count and column selector — user can choose *which* columns to export/import, not just all-or-nothing
- **Configuration step** exposes encoding, delimiter, quoting, null handling, date format — these are transport-format settings
- **Results step** shows a real-time progress table per-row-batch with a final summary (rows processed, errors, duration)
- Export and Import are **separate directed flows** — not the same UI with a direction toggle
- The destination table step offers **"Create new table"** or **"Use existing table"** as a first-class radio choice

### Pega Data Import Wizard
Key lessons:
- **Purpose** (Add-only vs Add-or-update) is selected on step 1 — it drives all downstream logic
- **Match-by field** for upsert: which key field to use to detect existing records
- **Update type** for empty fields: overwrite vs skip-blank — prevents accidental data erasure
- **Template save/load**: mapping configuration can be saved as a named template and re-applied next time
- **Validate-before-run** with a sample row count (test with 5 rows before full load)
- **Validate-and-review** step: shows processed / added / failed counts *before* committing

---

## 2. Current State (What We Have)

| File | Role | Issues |
|------|------|--------|
| `uc_ImportExportWizardLauncher.cs` | Pre-config panel + launch | No template load/save, no direction-specific labelling in wizard titles |
| `uc_Import_SelectDSandEntity.cs` | Step 1 — pick DS + entity | No purpose selector (add-only vs upsert), no match-by field, no row-count preview |
| `uc_Import_MapFields.cs` | Step 2 — field mapping | No template save/restore, no source sample preview, missing type mismatch warnings |
| `uc_Import_Run.cs` | Step 3 — run | No dry-run/sample mode, no per-row error table, no final summary card |
| `WizardKeys` (in SelectDSandEntity.cs) | Shared context keys | Missing keys for new features (purpose, matchByField, template name, etc.) |

---

## 3. New 5-Step Wizard Architecture

Replace the current 3-step flow with a **5-step flow** that mirrors both reference products:

```
Step 1: Configure
  Direction (Import / Export)
  Purpose (Add Only / Add or Update / Replace All)
  Source DS + Entity  →  live row count badge
  Destination DS + Entity  →  "Create if not exists" toggle
  Match-by field (shown only when Purpose = Add or Update)

Step 2: Column Selection
  Checkable list of source columns with data type badges
  "Select all / none / auto-match" toolbar
  Row preview: first 5 rows of source data as a mini read-only grid

Step 3: Field Mapping
  Source column → destination column (with type badges)
  Type mismatch warnings (yellow) and incompatible type errors (red)
  Transform expression per field (optional — simple expressions: trim, upper, lower, date format)
  Load / Save mapping template (named, stored in config)

Step 4: Options & Pre-flight
  Batch size slider (100 / 500 / 1000 / 5000 / all)
  Run validation rules toggle
  Skip blank / overwrite on update
  Run migration preflight check (schema compatibility)
  DRY RUN button → runs on first N rows only, shows result summary without committing

Step 5: Review, Run & Summary
  Config summary card (read-only: all choices from steps 1-4)
  Real-time progress: rows processed / added / updated / failed — live table
  Pause / Resume / Cancel
  Post-run summary card with duration, throughput (rows/sec), error count
  Export error rows to CSV button
  "Save as template" shortcut if not done on step 3
```

---

## 4. Detailed Changes Per File

### 4.1 `WizardKeys` (stays in `uc_Import_SelectDSandEntity.cs`)

Add the following constants:

```csharp
public const string Purpose           = "Purpose";           // ImportPurpose enum
public const string MatchByField      = "MatchByField";      // string
public const string UpdateEmptyFields = "UpdateEmptyFields"; // bool
public const string SelectedColumns   = "SelectedColumns";   // List<string>
public const string TemplateName      = "TemplateName";      // string
public const string BatchSize         = "BatchSize";         // int
public const string DryRunRowCount    = "DryRunRowCount";    // int (0 = disabled)
public const string RunValidation     = "RunValidation";     // bool
public const string RunSummary        = "RunSummary";        // ImportRunSummary
```

Add new enum (same file):

```csharp
public enum ImportPurpose { AddOnly, AddOrUpdate, ReplaceAll }
```

Add new summary DTO (same file):

```csharp
public sealed class ImportRunSummary
{
    public int  TotalRows      { get; set; }
    public int  AddedRows      { get; set; }
    public int  UpdatedRows    { get; set; }
    public int  SkippedRows    { get; set; }
    public int  FailedRows     { get; set; }
    public TimeSpan Duration   { get; set; }
    public double RowsPerSecond => Duration.TotalSeconds > 0 ? TotalRows / Duration.TotalSeconds : 0;
    public List<ImportRowError> Errors { get; set; } = new();
}

public sealed class ImportRowError
{
    public int    RowIndex    { get; set; }
    public string Field       { get; set; } = string.Empty;
    public string Value       { get; set; } = string.Empty;
    public string ErrorMessage{ get; set; } = string.Empty;
}
```

---

### 4.2 `uc_Import_SelectDSandEntity.cs` → Becomes **Step 1: Configure**

**New controls (Designer.cs):**
- `cmbPurpose` — BeepComboBox: Add Only / Add or Update / Replace All
- `lblPurpose` — BeepLabel
- `lblMatchBy` — BeepLabel (visible only when purpose = AddOrUpdate)
- `cmbMatchBy` — BeepComboBox (visible only when purpose = AddOrUpdate, populated from source entity fields)
- `chkUpdateEmpty` — BeepCheckBoxBool "Overwrite empty fields on update" (visible only when AddOrUpdate)
- `lblRowCount` — BeepLabel showing live row count: e.g. "~12,450 rows"
- `btnRefreshCount` — BeepButton (small, icon-only) to re-query row count

**Behaviour changes:**
- After source entity is selected: async background query for approximate row count → display in `lblRowCount`
- `cmbMatchBy` populates from source entity fields after source entity loads
- Toggle visibility of match-by controls based on purpose selection
- `OnStepLeave` persists `Purpose`, `MatchByField`, `UpdateEmptyFields` into `DataImportConfiguration`

**`DataImportConfiguration` fields to set:**
```csharp
config.SyncMode              = purpose == AddOrUpdate ? SyncMode.Upsert : SyncMode.Insert;
config.WatermarkColumn       = matchByField;  // reuse as upsert key
```

---

### 4.3 **New Step 2: Column Selection** (`uc_Import_ColumnSelection.cs` — new file)

**Purpose:** Let user see source data and pick which columns to bring across. Inspired directly by Oracle Workbench's Source step.

**Controls (Designer.cs):**
- `beepDataGrid1` — read-only preview grid (first 5 rows of source) — left panel, ~55% width
- `chkListColumns` — BeepSimpleGrid with checkbox column — right panel, ~40% width
  - Columns: `[✓] Column Name | Type | Sample Value`
- `btnSelectAll`, `btnSelectNone` — BeepButton toolbar
- `lblPreviewStatus` — BeepLabel "Showing first 5 rows of [EntityName]"
- `btnRefreshPreview` — BeepButton

**Behaviour:**
- `OnStepEnter`: connect to source DS, load first 5 rows async (use `GetView(entityName, filter: null, from: 0, to: 5)`), populate both the preview grid and the column checklist
- `OnStepLeave`: persist `SelectedColumns` list into wizard context; update `config.Mapping` to only include selected columns (pre-population of mapping for Step 3)

**`IsComplete`:** at least one column checked.

---

### 4.4 `uc_Import_MapFields.cs` → Becomes **Step 3: Field Mapping**

**New controls (Designer.cs):**
- `cmbTemplateLoad` — BeepComboBox "Load template…" (populated from saved templates in config)
- `btnTemplateSave` — BeepButton "Save as template…"
- `btnTemplateDelete` — BeepButton (icon only)
- Grid column additions (in the existing `beepDataGrid1`):
  - **Type mismatch** indicator column (icon cell: green/yellow/red)
  - **Transform** column: optional expression (e.g. `TRIM`, `UPPER`, `LOWER`, `DATE:{format}`)
- `lblMappingStatus` — BeepLabel showing "N of M source fields mapped | X warnings | Y errors"

**Behaviour changes:**
- After population, compare source type vs destination type for each mapped row:
  - Green: compatible types
  - Yellow: lossy (e.g. string → int)
  - Red: incompatible (e.g. blob → bool)
- Type mismatch tooltip on hover
- Template save: open `BeepDialogBox` asking for template name → serialize `List<EntityDataMap>` to named entry in `Editor.ConfigEditor` (or a local JSON file under `Config/ImportTemplates/`)
- Template load: populate grid from saved template, matching by field name
- `lblMappingStatus` updates live as rows are checked/changed
- `IsComplete` blocks on red errors (incompatible mappings) — warns on yellow

---

### 4.5 **New Step 4: Options & Pre-flight** (`uc_Import_Options.cs` — new file)

**Controls (Designer.cs):**
- `sldBatchSize` — `BeepNumericUpDown` (or slider) with preset buttons: 100 / 500 / 1000 / 5000
- `chkRunValidation` — BeepCheckBoxBool "Run validation rules"
- `chkSkipBlanks` — BeepCheckBoxBool "Skip empty values on update" (only shown for AddOrUpdate)
- `chkPreflight` — BeepCheckBoxBool "Run schema compatibility check"
- `chkCreateDest` — BeepCheckBoxBool "Create destination if not exists" (mirrors launcher)
- `chkAddMissing` — BeepCheckBoxBool "Add missing columns"
- `chkSyncDraft` — BeepCheckBoxBool "Save sync profile draft"
- `btnDryRun` — BeepButton "Dry Run (first 10 rows)" 
- `lblDryRunResult` — BeepLabel (hidden until dry run completes)
- `preflightStatusLabel` — BeepLabel showing preflight result

**Behaviour:**
- `btnDryRun` sets `config.DryRunRowCount = 10`, calls `DataImportManager.RunImportAsync` with a special cancellation after 10 rows, shows result in `lblDryRunResult` (e.g. "✓ 10 rows processed — no errors")
- `chkPreflight` click fires `RunMigrationPreflightAsync` inline when checked ON, shows result immediately in `preflightStatusLabel`
- All checkbox values write directly to `DataImportConfiguration` on step leave

---

### 4.6 `uc_Import_Run.cs` → Becomes **Step 5: Review, Run & Summary**

**New controls (Designer.cs):**
- **Summary card panel** (top, collapsible):
  - Read-only labels: Source, Destination, Purpose, Columns mapped, Batch size, Options
  - `btnEditStep` links per row → navigate back to that step
- Replace flat `beepLogBox` with a **split layout**:
  - Left: `beepLogBox` (RichTextBox) — log messages
  - Right: `errorGrid` (DataGridView) — error rows (row index, field, value, message)
- `lblSummaryCard` — post-run summary: Added N | Updated N | Skipped N | Failed N | Duration | Rows/sec
- `btnExportErrors` — BeepButton "Export errors to CSV" (enabled only when errors > 0)
- `statusProgressBar` enhancement — show percentage text inside bar
- `lblThroughput` — live "N rows/sec" label during run

**Behaviour changes:**
- On step enter: render the summary card from `DataImportConfiguration` + wizard context
- During run: populate `errorGrid` in real-time from `DataImportManager` error events
- After run: populate `ImportRunSummary`, store in `WizardKeys.RunSummary`
- `btnExportErrors` saves `errorGrid` rows to a timestamped CSV file via `SaveFileDialog`
- Summary card collapsed by default, expand on click (using BeepPanel or a collapsible panel)

---

### 4.7 `uc_ImportExportWizardLauncher.cs`

**Additions:**
- `cmbRecentTemplates` — BeepComboBox "Recent templates" (shows last 5 used template names; selecting one pre-fills source/dest/mapping in wizard context)
- `btnQuickImport` — BeepButton "Quick Import (skip mapping)" uses saved template directly from launcher, bypasses wizard
- History panel: last 5 runs as a BeepSimpleGrid showing: timestamp, source→dest, rows, status (success/failed)
- `btnViewLastSummary` — reopens the last `ImportRunSummary` in a popup
- Wire `wizardConfig.InitialContext` to pass all 5 step defaults including `Purpose`, `MatchByField`, template name

---

## 5. Designer.cs Pattern — Controls Summary

| File | New Designer Controls |
|------|-----------------------|
| `uc_Import_SelectDSandEntity.Designer.cs` | `cmbPurpose`, `lblPurpose`, `lblMatchBy`, `cmbMatchBy`, `chkUpdateEmpty`, `lblRowCount`, `btnRefreshCount` |
| `uc_Import_ColumnSelection.Designer.cs` *(new)* | `beepPreviewGrid`, `colSelectionGrid`, `btnSelectAll`, `btnSelectNone`, `lblPreviewStatus`, `btnRefreshPreview`, splitter panel |
| `uc_Import_MapFields.Designer.cs` | `cmbTemplateLoad`, `btnTemplateSave`, `btnTemplateDelete`, `lblMappingStatus` + 2 new grid columns |
| `uc_Import_Options.Designer.cs` *(new)* | `nudBatchSize`, `chkRunValidation`, `chkSkipBlanks`, `chkPreflight`, `chkCreateDest`, `chkAddMissing`, `chkSyncDraft`, `btnDryRun`, `lblDryRunResult`, `preflightStatusLabel` |
| `uc_Import_Run.Designer.cs` | Summary card panel, `errorGrid`, `btnExportErrors`, `lblSummaryCard`, `lblThroughput` |
| `uc_ImportExportWizardLauncher.Designer.cs` | `cmbRecentTemplates`, `btnQuickImport`, `historyGrid`, `btnViewLastSummary` |

---

## 6. `DataImportConfiguration` Field Mapping

| New Wizard Feature | Config Field |
|--------------------|-------------|
| Purpose → Add Only | `config.SyncMode = SyncMode.Insert` |
| Purpose → Add or Update | `config.SyncMode = SyncMode.Upsert` |
| Purpose → Replace All | `config.SyncMode = SyncMode.Replace` |
| Match-by field | `config.WatermarkColumn` (repurposed as upsert key) |
| Selected columns only | `config.Mapping` list (filtered to selected) |
| Batch size | `config.BatchSize` |
| Create dest if not exists | `config.CreateDestinationIfNotExists` |
| Add missing columns | `config.AddMissingColumns` |
| Sync draft | `config.CreateSyncProfileDraft` |
| Preflight | `config.RunMigrationPreflight` |

---

## 7. Template Storage

Templates are named `List<EntityDataMap>` + metadata serialized to JSON.

```
Config/
  ImportTemplates/
    {TemplateName}.json   ← { sourceName, destName, purpose, mapping[] }
```

Manager: simple `ImportTemplateManager` static helper class in this folder:
- `Save(name, config)` → write JSON
- `Load(name)` → read JSON, return `DataImportConfiguration` partial
- `ListAll()` → return all template names
- `Delete(name)` → remove file

---

## 8. Implementation Phases

### Phase 1 — Core model additions (no UI change)
1. Add new `WizardKeys` constants
2. Add `ImportPurpose` enum, `ImportRunSummary`, `ImportRowError` DTOs
3. Add `ImportTemplateManager` static helper

### Phase 2 — Enhance existing steps
4. `uc_Import_SelectDSandEntity`: add Purpose, MatchBy, row count
5. `uc_Import_MapFields`: add template save/load, type mismatch indicators, mapping status label
6. `uc_Import_Run`: add summary card, error grid, export errors, throughput label

### Phase 3 — New steps
7. `uc_Import_ColumnSelection` (new Step 2)
8. `uc_Import_Options` (new Step 4)
9. Update `uc_ImportExportWizardLauncher` to wire all 5 steps

### Phase 4 — Launcher extras
10. Recent templates combo, Quick Import, history grid, btnViewLastSummary

---

## 9. UX Principles Applied

| Principle | Implementation |
|-----------|---------------|
| **Progressive disclosure** | Advanced options (transforms, sync draft) hidden in collapsed panels / Step 4 |
| **Prevent errors early** | Type mismatch warnings on mapping, preflight check, dry-run before full load |
| **Reversibility** | Every step navigable Back; swap button on launcher; cancel at any point |
| **Visibility of system status** | Live row count on Step 1, live throughput + progress on Step 5 |
| **Efficiency for experts** | Templates save & load, Quick Import bypasses wizard entirely |
| **Recovery from errors** | Error grid on Step 5, export errors to CSV, partial-success not treated as full failure |
| **Match-to-mental-model** | Purpose (Add / Upsert / Replace) names match what users say — not technical enum names |
