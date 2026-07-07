using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.GridX;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Default.Views.Template;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Icons;

using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Defaults;
using TheTechIdea.Beep.Editor.Migration;
using TheTechIdea.Beep.Editor.Mapping;
using TheTechIdea.Beep.Logger;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.MVVM.ViewModels;
using TheTechIdea.Beep.Vis.Modules;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    [AddinAttribute(Caption = "Entity Editor", Name = "uc_EntityEditor",
        misc = "Config", menu = "Configuration", addinType = AddinType.Control,
        displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 1, RootNodeName = "Configuration", Order = 1, ID = 1,
        BranchText = "Entity Editor", BranchType = EnumPointType.Function,
        IconImageName = "entityeditor.svg", BranchClass = "ADDIN",
        BranchDescription = "Create / edit entity schema with MigrationManager.")]

    public partial class uc_EntityEditor : TemplateUserControl, IAddinVisSchema
    {
        private enum EntityEditorMode { CreateNew, UpdateExisting }
        private EntityEditorMode _mode = EntityEditorMode.CreateNew;
        private bool _isApplyingSchema;
        private string _lastSummary = "Idle";
        private EntityManagerViewModel? _viewModel;

        public uc_EntityEditor(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            Details.AddinName = "Entity Editor";
            WireButtonEvents();
            ApplyDpiScaledLayout();
        }

        // ── Skill § "Sizing tokens": DPI-scaled overrides applied in
        //    code-behind after InitializeComponent(). The Designer owns all
        //    Size / Location / Dock / Padding values; this method overlays
        //    token-based values that scale with the host display DPI.

        private void ApplyDpiScaledLayout()
        {
            Size = BeepLayoutMetrics.DialogLarge.ScaleSize(this);
            _comboRow.Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this);
        }

        // ── Event wiring (Designer owns the controls; code-behind wires them)

        private void WireButtonEvents()
        {
            _btnEditData.Click += BtnEditData_Click;
            _btnDefaults.Click += BtnDefaults_Click;
            _btnMapEntity.Click += BtnMapEntity_Click;
        }

        #region "IAddinVisSchema"
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string RootNodeName { get; set; } = "Configuration";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CatgoryName { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Order { get; set; } = 1;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ID { get; set; } = 1;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchText { get; set; } = "Entity Editor";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Level { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BranchID { get; set; } = 1;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconImageName { get; set; } = "entityeditor.svg";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchStatus { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ParentBranchID { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchDescription { get; set; } = "Create / edit entity schema with MigrationManager.";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchClass { get; set; } = "ADDIN";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string AddinName { get; set; } = "uc_EntityEditor";
        #endregion

        // ── Configure ───────────────────────────────────────────────────────

        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);
            if (beepService?.DMEEditor == null || appManager == null) return;

            _viewModel ??= new EntityManagerViewModel(beepService.DMEEditor, appManager);
            entityManagerViewModelBindingSource.DataSource = _viewModel;

            DatasourcebeepComboBox.SelectedItemChanged -= DatasourcebeepComboBox_SelectedItemChanged;
            EntitiesbeepComboBox.SelectedItemChanged    -= EntitiesbeepComboBox_SelectedItemChanged;
            ApplybeepButton.Click                      -= ApplybeepButton_Click;
            DatasourcebeepComboBox.SelectedItemChanged += DatasourcebeepComboBox_SelectedItemChanged;
            EntitiesbeepComboBox.SelectedItemChanged    += EntitiesbeepComboBox_SelectedItemChanged;
            ApplybeepButton.Click                      += ApplybeepButton_Click;

            ApplyLayoutDefaults();
            DatasourcebeepComboBox.ListItems = new BindingList<SimpleItem>();
            EntitiesbeepComboBox.ListItems = new BindingList<SimpleItem>();
            DatasourcebeepComboBox.Text = string.Empty;
            EntitiesbeepComboBox.Text = string.Empty;

            if (beepService.Config_editor?.DataConnections != null)
                foreach (var conn in beepService.Config_editor.DataConnections)
                    DatasourcebeepComboBox.ListItems.Add(new SimpleItem
                    {
                        DisplayField = conn.ConnectionName, Text = conn.ConnectionName,
                        Name = conn.ConnectionName, Value = conn.ConnectionName,
                        GuidId = conn.GuidID, ContainerGuidID = conn.GuidID
                    });
            SyncBindings();
        }

        private void ApplyLayoutDefaults()
        {
            DatasourcebeepComboBox.LeadingImagePath = SvgsUI.Database;
            DatasourcebeepComboBox.DropdownIconPath = SvgsUI.ChevronDown;
            EntitiesbeepComboBox.LeadingImagePath   = SvgsUI.Grid;
            EntitiesbeepComboBox.DropdownIconPath   = SvgsUI.ChevronDown;
            RefreshApplyButtonIcon();
            RefreshProgressiveDisclosure(GetEntityNameFromUi());
        }

        private void RefreshApplyButtonIcon() =>
            ApplybeepButton.ImagePath = !ApplybeepButton.Enabled
                ? SvgsUI.AlertTriangle
                : _mode == EntityEditorMode.CreateNew ? SvgsUI.PlusCircle : SvgsUI.Save;

        // ── Datasource selection ───────────────────────────────────────────

        private void DatasourcebeepComboBox_SelectedItemChanged(object? sender, SelectedItemChangedEventArgs e)
        {
            if (e?.SelectedItem == null || _viewModel == null || beepService?.DMEEditor == null) return;
            string dsName = e.SelectedItem.Text ?? string.Empty;
            _viewModel.Datasourcename = dsName;
            _viewModel.SourceConnection = beepService.DMEEditor.GetDataSource(dsName);
            _viewModel.EntityName = string.Empty;
            var ds = _viewModel.SourceConnection;
            if (ds == null) { LogStatus("Datasource not found", Errors.Failed); return; }
            if (ds.ConnectionStatus != ConnectionState.Open)
            {
                ds.Openconnection();
                if (ds.ConnectionStatus != ConnectionState.Open)
                { LogStatus("Could not open datasource", Errors.Failed); return; }
            }
            _viewModel.UpdateFieldTypes();
            ConfigureFieldTypeColumn();
            _viewModel.Structure = null; _viewModel.DBWork = null;
            _viewModel.Fields = null; _viewModel.EntityName = null;
            _lastSummary = $"Datasource: {dsName}";
            SyncBindings(); LoadEntitiesList();
            RefreshProgressiveDisclosure(_viewModel.EntityName);
        }

        // ── Entity selection ───────────────────────────────────────────────

        private void EntitiesbeepComboBox_SelectedItemChanged(object? sender, SelectedItemChangedEventArgs e)
        {
            if (e?.SelectedItem == null || _viewModel == null) return;
            LoadOrCreateEntity(e.SelectedItem.Text);
            RefreshEditorModeState(e.SelectedItem.Text);
            RefreshProgressiveDisclosure(e.SelectedItem.Text);
        }

        private void LoadEntitiesList()
        {
            EntitiesbeepComboBox.ListItems = new BindingList<SimpleItem>();
            if (_viewModel?.SourceConnection == null) return;
            foreach (var n in _viewModel.SourceConnection.GetEntitesList())
                EntitiesbeepComboBox.ListItems.Add(new SimpleItem { DisplayField = n, Text = n, Name = n, Value = n });
            if (!string.IsNullOrWhiteSpace(_viewModel.EntityName)) SelectEntity(_viewModel.EntityName);
        }

        private bool LoadOrCreateEntity(string? entityName)
        {
            if (_viewModel == null || string.IsNullOrWhiteSpace(entityName)) return false;
            _viewModel.LoadOrCreateEntityStructure(entityName.Trim());
            SyncBindings(); return true;
        }

        private string? GetEntityNameFromUi()
        {
            if (EntitiesbeepComboBox.SelectedItem is SimpleItem { Text: { Length: > 0 } t }) return t.Trim();
            if (!string.IsNullOrWhiteSpace(EntitiesbeepComboBox.Text)) return EntitiesbeepComboBox.Text.Trim();
            return _viewModel?.EntityName;
        }

        private void SelectEntity(string entityName)
        {
            var existing = EntitiesbeepComboBox.ListItems?.Cast<SimpleItem>()
                .FirstOrDefault(i => string.Equals(i.Text, entityName, StringComparison.OrdinalIgnoreCase));
            if (existing != null) EntitiesbeepComboBox.SelectedItem = existing;
            EntitiesbeepComboBox.Text = entityName;
        }

        // ── Navigation ─────────────────────────────────────────────────────

        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            base.OnNavigatedTo(parameters);
            if (_viewModel == null || beepService?.DMEEditor == null) return;
            if (parameters.TryGetValue("Datasource", out var dsObj)) {
                _viewModel.Datasourcename = dsObj?.ToString() ?? "";
                _viewModel.SourceConnection = beepService.DMEEditor.GetDataSource(_viewModel.Datasourcename);
                _viewModel.EntityName = ""; _viewModel.UpdateFieldTypes(); ConfigureFieldTypeColumn(); LoadEntitiesList();
            }
            if (parameters.TryGetValue("EntityName", out var entObj)) {
                _viewModel.EntityName = entObj?.ToString() ?? "";
                _viewModel.SourceConnection ??= beepService.DMEEditor.GetDataSource(_viewModel.Datasourcename);
                _viewModel.LoadOrCreateEntityStructure(_viewModel.EntityName);
                _viewModel.IsNew = false; _viewModel.IsChanged = false;
                RefreshEditorModeState(_viewModel.EntityName);
            } else { _viewModel.IsNew = true; _mode = EntityEditorMode.CreateNew; }
            SyncBindings();
        }

        // ── Apply (Create / Update via MigrationManager) ───────────────────

        private void ApplybeepButton_Click(object? sender, EventArgs e)
        {
            if (_viewModel == null || beepService?.DMEEditor == null) return;
            if (_viewModel.SourceConnection == null) { LogStatus("Select a datasource first", Errors.Failed); return; }
            if (_isApplyingSchema) { LogStatus("Schema operation already running.", Errors.Warning); return; }
            string entityName = GetEntityNameFromUi() ?? "";
            if (string.IsNullOrWhiteSpace(entityName)) { LogStatus("Select or type an entity name", Errors.Failed); return; }
            if (_viewModel.Structure == null || !string.Equals(_viewModel.EntityName, entityName, StringComparison.OrdinalIgnoreCase))
                if (!LoadOrCreateEntity(entityName)) return;
            fieldsBindingSource.EndEdit();
            if (BindingContext?[fieldsBindingSource] is CurrencyManager cm) cm.EndCurrentEdit();
            _isApplyingSchema = true;
            try
            {
                var draft = BuildDraftStructure(entityName);
                if (!ValidateDraft(draft)) return;
                RefreshEditorModeState(entityName);
                if (_mode == EntityEditorMode.CreateNew) ExecuteCreate(draft); else ExecuteUpdate(draft);
            }
            finally { _isApplyingSchema = false; SyncBindings(); }
        }

        // ── Integration: Edit Data ──────────────────────────────────────────

        private void BtnEditData_Click(object? sender, EventArgs e)
        {
            if (_viewModel == null || appManager == null) return;
            string entityName = GetEntityNameFromUi() ?? "";
            if (string.IsNullOrWhiteSpace(entityName) || string.IsNullOrWhiteSpace(_viewModel.Datasourcename)) return;
            appManager.ShowPage("uc_DataEdit", new PassedArgs { CurrentEntity = entityName, DatasourceName = _viewModel.Datasourcename, EventType = "CRUDENTITY" });
        }

        // ── Integration: Defaults ───────────────────────────────────────────

        private void BtnDefaults_Click(object? sender, EventArgs e)
        {
            if (_viewModel == null || appManager == null) return;
            string entityName = GetEntityNameFromUi() ?? "";
            if (string.IsNullOrWhiteSpace(entityName) || string.IsNullOrWhiteSpace(_viewModel.Datasourcename)) return;
            appManager.ShowPage("uc_DefaultsEditor", new PassedArgs { DatasourceName = _viewModel.Datasourcename, CurrentEntity = entityName });
        }

        // ── Integration: Map Entity ─────────────────────────────────────────

        private void BtnMapEntity_Click(object? sender, EventArgs e)
        {
            if (_viewModel == null || appManager == null || beepService?.DMEEditor == null) return;
            string entityName = GetEntityNameFromUi() ?? "";
            string dsName = _viewModel.Datasourcename ?? "";
            if (string.IsNullOrWhiteSpace(entityName) || string.IsNullOrWhiteSpace(dsName)) return;
            var result = MappingManager.CreateEntityMap(beepService.DMEEditor, entityName, dsName);
            LogStatus(result.Item1.Flag == Errors.Ok ? $"Entity map for '{entityName}' created." : $"Map: {result.Item1.Message}", result.Item1.Flag);
            if (result.Item1.Flag == Errors.Ok)
                appManager.ShowPage("uc_MappingEditor", new PassedArgs { DatasourceName = dsName, CurrentEntity = entityName });
        }

        // ── Sync / bindings ────────────────────────────────────────────────

        private void SyncBindings()
        {
            if (_viewModel == null) return;
            _viewModel.Fields = _viewModel.DBWork?.Units;
            entityManagerViewModelBindingSource.DataSource = _viewModel;
            entityManagerViewModelBindingSource.ResetBindings(false);
            fieldsBindingSource.ResetBindings(false);
            if (_viewModel.DBWork != null) { EntityFieldsbeepGridPro.DataSource = null; EntityFieldsbeepGridPro.Uow = _viewModel.DBWork; }
            else { EntityFieldsbeepGridPro.Uow = null; EntityFieldsbeepGridPro.DataSource = _viewModel.Fields; }
            ConfigureEditorsFromEntityFieldProperties(); ConfigureFieldTypeColumn();
            RefreshEditorModeState(_viewModel.EntityName); RefreshProgressiveDisclosure(_viewModel.EntityName);
        }

        private void RefreshEditorModeState(string? entityName)
        {
            if (_viewModel?.SourceConnection == null) { _mode = EntityEditorMode.CreateNew; ApplybeepButton.Text = "Create Entity"; ApplybeepButton.Enabled = false; RefreshApplyButtonIcon(); return; }
            if (string.IsNullOrWhiteSpace(entityName?.Trim())) { _mode = EntityEditorMode.CreateNew; ApplybeepButton.Text = "Create Entity"; ApplybeepButton.Enabled = true; RefreshApplyButtonIcon(); return; }
            bool exists = false;
            try { exists = _viewModel.SourceConnection.CheckEntityExist(entityName.Trim()); } catch { }
            _mode = exists ? EntityEditorMode.UpdateExisting : EntityEditorMode.CreateNew;
            if (_mode == EntityEditorMode.CreateNew) { ApplybeepButton.Text = "Create Entity"; ApplybeepButton.Enabled = true; RefreshApplyButtonIcon(); return; }
            var helper = beepService?.DMEEditor?.GetDataSourceHelper(_viewModel.SourceConnection.DatasourceType);
            bool canEvolve = helper?.Capabilities == null || helper.Capabilities.SupportsSchemaEvolution;
            ApplybeepButton.Text = canEvolve ? "Update Schema" : "Update Not Supported"; ApplybeepButton.Enabled = canEvolve;
            RefreshApplyButtonIcon();
            _stateLabel.Text = canEvolve ? $"Mode: Update existing schema | {_lastSummary}" : $"Mode: Update unavailable for '{_viewModel.SourceConnection.DatasourceType}'";
        }

        // ── Draft + validation ─────────────────────────────────────────────

        private EntityStructure BuildDraftStructure(string entityName)
        {
            var s = _viewModel?.Structure != null ? (EntityStructure)_viewModel.Structure.Clone() : new EntityStructure();
            s.EntityName = entityName.Trim(); s.DatasourceEntityName = entityName.Trim();
            s.DatabaseType = _viewModel?.SourceConnection?.DatasourceType ?? s.DatabaseType;
            s.Fields = ExtractDraftFields(); return s;
        }

        private List<EntityField> ExtractDraftFields()
        {
            var f = new List<EntityField>();
            if (_viewModel?.DBWork?.Units is IEnumerable<object> src) f.AddRange(src.OfType<EntityField>().Select(CloneField));
            else if (_viewModel?.Fields is IEnumerable<object> fb) f.AddRange(fb.OfType<EntityField>().Select(CloneField));
            return f;
        }

        private static EntityField CloneField(EntityField f) => new()
        {
            FieldName = f.FieldName, Originalfieldname = f.Originalfieldname, Fieldtype = f.Fieldtype, FieldCategory = f.FieldCategory,
            Size1 = f.Size1, Size = f.Size, Size2 = f.Size2, NumericPrecision = f.NumericPrecision, NumericScale = f.NumericScale,
            AllowDBNull = f.AllowDBNull, IsKey = f.IsKey, IsAutoIncrement = f.IsAutoIncrement, IsUnique = f.IsUnique,
            IsIdentity = f.IsIdentity, IsReadOnly = f.IsReadOnly, IsDisplayField = f.IsDisplayField,
            EntityName = f.EntityName, Description = f.Description, DefaultValue = f.DefaultValue
        };

        private bool ValidateDraft(EntityStructure draft)
        {
            if (string.IsNullOrWhiteSpace(draft.EntityName)) { LogStatus("Entity name required.", Errors.Failed); return false; }
            if (draft.Fields == null || draft.Fields.Count == 0) { LogStatus("At least one field required.", Errors.Failed); return false; }
            var dup = draft.Fields.Where(f => !string.IsNullOrWhiteSpace(f?.FieldName)).GroupBy(f => f.FieldName, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
            if (dup != null) { LogStatus($"Duplicate field '{dup.Key}'.", Errors.Failed); return false; }
            if (_viewModel?.SourceConnection != null)
            {
                var h = beepService?.DMEEditor?.GetDataSourceHelper(_viewModel.SourceConnection.DatasourceType);
                if (h != null) try { var (ok, errs) = h.ValidateEntity(draft); if (!ok) { LogStatus(string.Join("; ", errs ?? new List<string>()), Errors.Failed); return false; } } catch { }
            }
            return true;
        }

        // ── Create / Update ────────────────────────────────────────────────

        private void ExecuteCreate(EntityStructure draft)
        {
            var ds = _viewModel?.SourceConnection;
            if (ds == null) { LogStatus("Datasource not available.", Errors.Failed); return; }
            if (ds.CheckEntityExist(draft.EntityName)) { LogStatus($"'{draft.EntityName}' already exists.", Errors.Failed); return; }
            var started = DateTime.Now;
            var migration = new MigrationManager(beepService!.DMEEditor, ds);
            var result = migration.EnsureEntity(draft);
            if (result.Flag != Errors.Ok) { LogStatus($"Create: {result.Message}", Errors.Failed); return; }
            _lastSummary = $"Created {draft.EntityName} in {(DateTime.Now - started).TotalMilliseconds:0} ms";
            LogStatus(_lastSummary, Errors.Ok);
            LoadEntitiesList(); SelectEntity(draft.EntityName); LoadOrCreateEntity(draft.EntityName); RefreshEditorModeState(draft.EntityName);
        }

        private void ExecuteUpdate(EntityStructure desired)
        {
            var ds = _viewModel?.SourceConnection;
            if (ds == null) { LogStatus("Datasource not available.", Errors.Failed); return; }
            if (!ds.CheckEntityExist(desired.EntityName)) { LogStatus($"'{desired.EntityName}' does not exist.", Errors.Failed); return; }
            var helper = beepService?.DMEEditor?.GetDataSourceHelper(ds.DatasourceType);
            if (helper == null) { LogStatus($"No helper for '{ds.DatasourceType}'.", Errors.Failed); return; }
            if (helper.Capabilities is { SupportsSchemaEvolution: false }) { LogStatus($"Schema evolution unsupported.", Errors.Failed); return; }
            var cur = ds.GetEntityStructure(desired.EntityName, true);
            if (cur?.Fields == null) { LogStatus("Cannot resolve current structure.", Errors.Failed); return; }
            var steps = BuildSchemaSteps(helper, desired.EntityName, cur.Fields, desired.Fields ?? new List<EntityField>());
            if (steps.Count == 0) { LogStatus("No schema changes.", Errors.Information); return; }
            if (MessageBox.Show(BuildPreviewMessage(steps), "Apply Schema Update", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            { LogStatus("Update cancelled.", Errors.Warning); return; }
            var started = DateTime.Now;
            foreach (var step in steps)
            {
                var migRes = ds.ExecuteSql(step.Sql);
                if (migRes == null || migRes.Flag == Errors.Failed) { LogStatus($"Update failed at '{step.Description}': {migRes?.Message}", Errors.Failed); return; }
            }
            _lastSummary = $"Updated {desired.EntityName} ({steps.Count} steps) in {(DateTime.Now - started).TotalMilliseconds:0} ms";
            LogStatus(_lastSummary, Errors.Ok);
            LoadEntitiesList(); SelectEntity(desired.EntityName); LoadOrCreateEntity(desired.EntityName); RefreshEditorModeState(desired.EntityName);
        }

        private static List<SchemaStep> BuildSchemaSteps(IDataSourceHelper h, string ent, List<EntityField> cur, List<EntityField> des)
        {
            var steps = new List<SchemaStep>();
            var cd = cur.Where(f => !string.IsNullOrWhiteSpace(f?.FieldName)).ToDictionary(f => f.FieldName, StringComparer.OrdinalIgnoreCase);
            var dd = des.Where(f => !string.IsNullOrWhiteSpace(f?.FieldName)).ToDictionary(f => f.FieldName, StringComparer.OrdinalIgnoreCase);
            foreach (var kv in dd) if (!cd.ContainsKey(kv.Key)) { var (s, ok, _) = h.GenerateAddColumnSql(ent, kv.Value); if (ok && !string.IsNullOrWhiteSpace(s)) steps.Add(new SchemaStep { Description = $"Add {kv.Key}", Sql = s }); }
            foreach (var kv in dd) { if (!cd.TryGetValue(kv.Key, out var old) || FieldsEqual(old, kv.Value)) continue; var (s, ok, _) = h.GenerateAlterColumnSql(ent, kv.Key, kv.Value); if (ok && !string.IsNullOrWhiteSpace(s)) steps.Add(new SchemaStep { Description = $"Alter {kv.Key}", Sql = s }); }
            foreach (var kv in cd) { if (dd.ContainsKey(kv.Key)) continue; var (s, ok, _) = h.GenerateDropColumnSql(ent, kv.Key); if (ok && !string.IsNullOrWhiteSpace(s)) steps.Add(new SchemaStep { Description = $"Drop {kv.Key}", Sql = s }); }
            return steps;
        }

        private static bool FieldsEqual(EntityField a, EntityField b) =>
            a != null && b != null && string.Equals(a.Fieldtype, b.Fieldtype, StringComparison.OrdinalIgnoreCase) && a.Size1 == b.Size1;

        private static string BuildPreviewMessage(List<SchemaStep> steps)
        {
            var sb = new StringBuilder(); sb.AppendLine($"Apply {steps.Count} change(s)?"); sb.AppendLine();
            foreach (var s in steps.Take(8)) sb.AppendLine($"- {s.Description}");
            if (steps.Count > 8) sb.AppendLine($"… +{steps.Count - 8} more");
            return sb.ToString();
        }

        private sealed class SchemaStep { public string Description { get; set; } = ""; public string Sql { get; set; } = ""; }

        // ── Logging + disclosure ───────────────────────────────────────────

        private void LogStatus(string msg, Errors flag)
        {
            _lastSummary = msg; beepService?.DMEEditor?.AddLogMessage("EntityEditor", _lastSummary, DateTime.Now, 0, _viewModel?.EntityName, flag);
            _stateLabel.Text = $"Status: {_lastSummary}";
        }

        private void RefreshProgressiveDisclosure(string? entityName)
        {
            bool hasDs = _viewModel?.SourceConnection != null, hasEnt = !string.IsNullOrWhiteSpace(entityName), isExisting = hasEnt && _mode == EntityEditorMode.UpdateExisting;
            EntitiesbeepComboBox.Enabled = hasDs;
            EntityFieldsbeepGridPro.Enabled = hasDs && hasEnt;
            ApplybeepButton.Enabled = hasDs && ApplybeepButton.Enabled;
            _btnEditData.Visible = isExisting;
            _btnDefaults.Visible = isExisting;
            _btnMapEntity.Visible = isExisting;
            _stateLabel.Text = !hasDs ? "Select datasource." : !hasEnt ? "Select or type entity name." : _mode == EntityEditorMode.CreateNew ? $"Review fields, then create '{entityName}'." : $"Review diff, then update '{entityName}'.";
        }

        private void ConfigureEditorsFromEntityFieldProperties()
        {
            if (EntityFieldsbeepGridPro?.Columns == null) return;
            foreach (var col in EntityFieldsbeepGridPro.Columns)
            {
                if (col == null || string.IsNullOrWhiteSpace(col.ColumnName) || string.Equals(col.ColumnName, "Fieldtype", StringComparison.OrdinalIgnoreCase)) continue;
                var pt = ResolveEntityFieldPropertyType(col); if (pt == null) continue;
                var t = Nullable.GetUnderlyingType(pt) ?? pt;
                if (t == typeof(bool)) col.CellEditor = BeepColumnType.CheckBoxBool;
                else if (t == typeof(char)) col.CellEditor = BeepColumnType.CheckBoxChar;
            }
        }

        private static Type? ResolveEntityFieldPropertyType(BeepColumnConfig col)
        {
            if (!string.IsNullOrWhiteSpace(col.PropertyTypeName)) { var t = Type.GetType(col.PropertyTypeName, throwOnError: false); if (t != null) return t; }
            return typeof(EntityField).GetProperty(col.ColumnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?.PropertyType;
        }

        private void ConfigureFieldTypeColumn()
        {
            if (EntityFieldsbeepGridPro?.Columns == null || _viewModel == null) return;
            var tc = EntityFieldsbeepGridPro.Columns.FirstOrDefault(c => string.Equals(c.ColumnName, "Fieldtype", StringComparison.OrdinalIgnoreCase));
            if (tc == null) return;
            tc.CellEditor = BeepColumnType.ComboBox;
            tc.Items = (_viewModel.DatatypeMappings ?? Enumerable.Empty<DatatypeMapping>()).Where(m => !string.IsNullOrWhiteSpace(m.NetDataType))
                .GroupBy(m => m.NetDataType, StringComparer.OrdinalIgnoreCase).Select(g => g.OrderByDescending(m => m.Fav).First())
                .OrderByDescending(m => m.Fav).ThenBy(m => m.NetDataType)
                .Select(m => new SimpleItem { DisplayField = m.NetDataType, Text = m.NetDataType, Name = m.NetDataType, Value = m.NetDataType }).ToList();
        }
    }
}
