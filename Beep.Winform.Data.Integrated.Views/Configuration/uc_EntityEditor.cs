using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Default.Views.Template;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Icons;

using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Logger;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.MVVM.ViewModels;
using TheTechIdea.Beep.Vis.Modules;


namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    [AddinAttribute(Caption = "Entity Editor", Name = "uc_EntityEditor", misc = "Config", menu = "Configuration", addinType = AddinType.Control, displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 1, RootNodeName = "Configuration", Order = 1, ID = 1, BranchText = "Entity Editor", BranchType = EnumPointType.Function, IconImageName = "entityeditor.svg", BranchClass = "ADDIN", BranchDescription = "Local DB Connections Setup Screen")]

    public partial class uc_EntityEditor : TemplateUserControl, IAddinVisSchema
    {
        private enum EntityEditorMode
        {
            CreateNew,
            UpdateExisting
        }

        private EntityEditorMode _mode = EntityEditorMode.CreateNew;
        private bool _isApplyingSchema;
        private string _lastSummary = "Idle";

        public uc_EntityEditor(IServiceProvider services): base(services)
        {
            InitializeComponent();
          
            Details.AddinName = "Entity Editor";
            Resize -= Uc_EntityEditor_Resize;
            Resize += Uc_EntityEditor_Resize;

        }
        #region "IAddinVisSchema"
        public string RootNodeName { get; set; } = "Configuration";
        public string CatgoryName { get; set; }
        public int Order { get; set; } = 1;
        public int ID { get; set; } = 1;
        public string BranchText { get; set; } = "Entity Editor";
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        public int BranchID { get; set; } = 1;
        public string IconImageName { get; set; } = "entityeditor.svg";
        public string BranchStatus { get; set; }
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; } = "Entity Editor Screen";
        public string BranchClass { get; set; } = "ADDIN";
        public string AddinName { get; set; }
        #endregion "IAddinVisSchema"
        EntityManagerViewModel viewModel;

        List<SimpleItem> Drivers = new List<SimpleItem>();
    
        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);
            if (beepService == null || appManager == null)
            {
                return;
            }

            viewModel ??= new EntityManagerViewModel(beepService.DMEEditor, appManager);
            entityManagerViewModelBindingSource.DataSource = viewModel;

            DatasourcebeepComboBox.SelectedItemChanged -= DatasourcebeepComboBox_SelectedItemChanged;
            EntitiesbeepComboBox.SelectedItemChanged -= EntitiesbeepComboBox_SelectedItemChanged;
            ApplybeepButton.Click -= ApplybeepButton_Click;
            DatasourcebeepComboBox.SelectedItemChanged += DatasourcebeepComboBox_SelectedItemChanged;
            EntitiesbeepComboBox.SelectedItemChanged += EntitiesbeepComboBox_SelectedItemChanged;
            ApplybeepButton.Click += ApplybeepButton_Click;

            ApplyLayoutDefaults();
            DatasourcebeepComboBox.ListItems = new BindingList<SimpleItem>();
            EntitiesbeepComboBox.ListItems = new BindingList<SimpleItem>();
            DatasourcebeepComboBox.Text = string.Empty;
            EntitiesbeepComboBox.Text = string.Empty;
            Drivers.Clear();

            foreach (var item in beepService.Config_editor.DataConnections)
            {
                SimpleItem conn = new SimpleItem();
                conn.DisplayField = item.ConnectionName;
                conn.Text = item.ConnectionName;
                conn.Name = item.ConnectionName;
                conn.Value = item.ConnectionName;
                conn.GuidId = item.GuidID;
                conn.ParentItem = null;
                conn.ContainerGuidID = item.GuidID;
                DatasourcebeepComboBox.ListItems.Add(conn);
            }
            List<SimpleItem> versions = new List<SimpleItem>();
            foreach (var item in beepService.Config_editor.DataDriversClasses.Select(o=>o.PackageName))
            {
                SimpleItem driveritem = new SimpleItem();
                driveritem.DisplayField = item;
                driveritem.Text = item;
                driveritem.Name = item;
                driveritem.Value = item;
                foreach (var DriversClasse in beepService.Config_editor.DataDriversClasses.Where(x => x.PackageName == item))
                {
                    SimpleItem itemversion = new SimpleItem();
                    itemversion.DisplayField = DriversClasse.version;
                    itemversion.Value = DriversClasse.version;
                    itemversion.Text = DriversClasse.version;
                    itemversion.Name = DriversClasse.version;
                    itemversion.ParentItem = driveritem;
                    itemversion.ParentValue = item;
                    versions.Add(itemversion);
                }
                 Drivers.Add(driveritem);
            }
            SyncBindings();


            // idx = 0;
            //foreach (var item in viewModel.PackageVersions)
            //{
            //    SimpleItem driveritem = new SimpleItem();
            //    driveritem.IsDisplayField = item;
            //    driveritem.Value = idx++;
            //    driveritem.Text = item;
            //    driveritem.Name = item;
            //    driverversion.Items.Add(driveritem);
            //}
        }

        private void ApplyLayoutDefaults()
        {
            DatasourcebeepComboBox.PlaceholderText = "Select datasource";
            DatasourcebeepComboBox.LabelText = "Datasource";
            DatasourcebeepComboBox.LabelTextOn = true;
            DatasourcebeepComboBox.ShowSearchInDropdown = true;
            DatasourcebeepComboBox.ToolTipText = "Choose the datasource where the entity will be created or updated.";

            EntitiesbeepComboBox.PlaceholderText = "Select or type entity name";
            EntitiesbeepComboBox.LabelText = "Entity";
            EntitiesbeepComboBox.LabelTextOn = true;
            EntitiesbeepComboBox.ShowSearchInDropdown = true;
            EntitiesbeepComboBox.ToolTipText = "Pick an existing entity to update schema or type a new name to create.";

            ApplybeepButton.Text = "Create Entity";
            ApplybeepButton.ToolTipText = "Applies create or update operation based on current mode.";
            ConfigureContextIcons();

            ApplyResponsiveLayout();
            RefreshProgressiveDisclosure(GetEntityNameFromUi());
        }

        private void ConfigureContextIcons()
        {
            DatasourcebeepComboBox.LeadingImagePath = SvgsUI.Database;
            DatasourcebeepComboBox.DropdownIconPath = SvgsUI.ChevronDown;
            EntitiesbeepComboBox.LeadingImagePath = SvgsUI.Grid;
            EntitiesbeepComboBox.DropdownIconPath = SvgsUI.ChevronDown;
            RefreshApplyButtonIcon();
        }

        private void RefreshApplyButtonIcon()
        {
            if (!ApplybeepButton.Enabled)
            {
                ApplybeepButton.ImagePath = SvgsUI.AlertTriangle;
                return;
            }

            ApplybeepButton.ImagePath = _mode == EntityEditorMode.CreateNew
                ? SvgsUI.PlusCircle
                : SvgsUI.Save;
        }

        private void DatasourcebeepComboBox_SelectedItemChanged(object? sender, SelectedItemChangedEventArgs e)
        {
            // set entiies from selected datasource
            if (e.SelectedItem != null)
            {
                string datasource = e.SelectedItem.Text;
                viewModel.Datasourcename = datasource;
                viewModel.SourceConnection = beepService.DMEEditor.GetDataSource(datasource);
                viewModel.EntityName = string.Empty;
                if (viewModel.SourceConnection != null)
                {
                    viewModel.UpdateFieldTypes();
                    ConfigureFieldTypeColumn();
                }
                if(viewModel.SourceConnection != null)
                {
                    if(viewModel.SourceConnection.ConnectionStatus != ConnectionState.Open)
                    {
                        viewModel.SourceConnection.Openconnection();
                        if(viewModel.SourceConnection.ConnectionStatus != ConnectionState.Open)
                        {
                            beepService.DMEEditor.AddLogMessage("Beep", "Datasource not Found", DateTime.Now, 0, null, Errors.Failed);
                            return;
                        }
                    }
                }else
                {
                    beepService.DMEEditor.AddLogMessage("Beep", "Datasource not Found", DateTime.Now, 0, null, Errors.Failed);
                    return;
                }

                viewModel.Structure = null;
                viewModel.DBWork = null;
                viewModel.Fields = null;
                viewModel.EntityName = null;
                _lastSummary = $"Datasource: {datasource}";
                SyncBindings();
                LoadEntitiesList();
                RefreshProgressiveDisclosure(viewModel.EntityName);

            }
        }

        private void EntitiesbeepComboBox_SelectedItemChanged(object? sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null || viewModel == null)
            {
                return;
            }

            LoadOrCreateEntity(e.SelectedItem.Text);
            RefreshEditorModeState(e.SelectedItem.Text);
            RefreshProgressiveDisclosure(e.SelectedItem.Text);
        }

        private void BeepButton1_Click(object? sender, EventArgs e)
        {
            if (viewModel == null)
            {
                return;
            }

            viewModel.IsChanged = true;
            viewModel.SaveEntity();
            LoadEntitiesList();
        }

        private void BeepButton2_Click(object? sender, EventArgs e)
        {
            if (viewModel == null)
            {
                return;
            }

            viewModel.DeleteEntity();
            LoadEntitiesList();
        }

        private void LoadEntitiesList()
        {
            EntitiesbeepComboBox.ListItems = new BindingList<SimpleItem>();
            if (viewModel?.SourceConnection == null)
            {
                return;
            }

            foreach (var item in viewModel.SourceConnection.GetEntitesList())
            {
                SimpleItem entityitem = new SimpleItem();
                entityitem.DisplayField = item;
                entityitem.Text = item;
                entityitem.Name = item;
                entityitem.Value = item;
                EntitiesbeepComboBox.ListItems.Add(entityitem);
            }

            if (!string.IsNullOrWhiteSpace(viewModel.EntityName))
            {
                SelectEntity(viewModel.EntityName);
            }
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            base.OnNavigatedTo(parameters);
            if (viewModel == null || beepService == null)
            {
                return;
            }
            if (parameters.TryGetValue("Datasource", out var datasourceObj))
            {
                string datasource = datasourceObj?.ToString() ?? string.Empty;
                viewModel.Datasourcename = datasource;
                viewModel.SourceConnection = beepService.DMEEditor.GetDataSource(datasource);
                viewModel.EntityName = string.Empty;
                if (viewModel.SourceConnection != null)
                {
                    viewModel.UpdateFieldTypes();
                    ConfigureFieldTypeColumn();
                }
                LoadEntitiesList();
            }
            if (parameters.TryGetValue("EntityName", out var entityNameObj))
            {
                string entityname = entityNameObj?.ToString() ?? string.Empty;
                viewModel.EntityName = entityname;
                if(viewModel.SourceConnection == null)
                {
                    viewModel.SourceConnection = beepService.DMEEditor.GetDataSource(viewModel.Datasourcename);
                }
                viewModel.LoadOrCreateEntityStructure(entityname);
                viewModel.IsNew = false;
                viewModel.IsChanged = false;
                RefreshEditorModeState(entityname);
                
            }
            else
            {
                viewModel.IsNew = true;
                _mode = EntityEditorMode.CreateNew;
            }
            //HeaderbeepPanel.TitleText = viewModel.EntityName?? "Entity Editor";
          //  EntityFieldsbeepGridPro.TitleText = "Field Structure";
            SyncBindings();
        }

        private void ApplybeepButton_Click(object? sender, EventArgs e)
        {
            if (viewModel == null || beepService == null)
            {
                return;
            }

            if (viewModel.SourceConnection == null)
            {
                beepService.DMEEditor.AddLogMessage("Beep", "Select a datasource first", DateTime.Now, 0, null, Errors.Failed);
                return;
            }

            if (_isApplyingSchema)
            {
                LogStatus("Schema operation is already running.", Errors.Warning);
                return;
            }

            string entityName = GetEntityNameFromUi();
            if (string.IsNullOrWhiteSpace(entityName))
            {
                beepService.DMEEditor.AddLogMessage("Beep", "Select or type an entity name", DateTime.Now, 0, null, Errors.Failed);
                return;
            }

            if (viewModel.Structure == null || !string.Equals(viewModel.EntityName, entityName, StringComparison.OrdinalIgnoreCase))
            {
                if (!LoadOrCreateEntity(entityName))
                {
                    return;
                }
            }

            fieldsBindingSource.EndEdit();
            if (BindingContext[fieldsBindingSource] is CurrencyManager cm)
            {
                cm.EndCurrentEdit();
            }

            _isApplyingSchema = true;
            try
            {
                var draft = BuildDraftStructure(entityName);
                if (!ValidateDraft(draft))
                {
                    return;
                }

                RefreshEditorModeState(entityName);
                if (_mode == EntityEditorMode.CreateNew)
                {
                    ExecuteCreate(draft);
                }
                else
                {
                    ExecuteUpdate(draft);
                }
            }
            finally
            {
                _isApplyingSchema = false;
                SyncBindings();
            }
        }

        private bool LoadOrCreateEntity(string? entityName)
        {
            if (viewModel == null || string.IsNullOrWhiteSpace(entityName))
            {
                return false;
            }

            viewModel.LoadOrCreateEntityStructure(entityName.Trim());
            SyncBindings();
            return true;
        }

        private string? GetEntityNameFromUi()
        {
            if (EntitiesbeepComboBox.SelectedItem is SimpleItem selected && !string.IsNullOrWhiteSpace(selected.Text))
            {
                return selected.Text.Trim();
            }

            if (!string.IsNullOrWhiteSpace(EntitiesbeepComboBox.Text))
            {
                return EntitiesbeepComboBox.Text.Trim();
            }

            return viewModel?.EntityName;
        }

        private void SelectEntity(string entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName) || EntitiesbeepComboBox.ListItems == null)
            {
                return;
            }

            var existing = EntitiesbeepComboBox.ListItems.FirstOrDefault(i =>
                string.Equals(i.Text, entityName, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                EntitiesbeepComboBox.SelectedItem = existing;
            }

            EntitiesbeepComboBox.Text = entityName;
        }

        private void SyncBindings()
        {
            if (viewModel == null)
            {
                return;
            }

            viewModel.Fields = viewModel.DBWork?.Units;
            entityManagerViewModelBindingSource.DataSource = viewModel;
            entityManagerViewModelBindingSource.ResetBindings(false);
            fieldsBindingSource.ResetBindings(false);

            // BeepGridPro binding modes are mutually exclusive:
            // if Uow is set, it becomes the authoritative source and DataSource is ignored.
            if (viewModel.DBWork != null)
            {
                EntityFieldsbeepGridPro.DataSource = null;
                EntityFieldsbeepGridPro.Uow = viewModel.DBWork;
            }
            else
            {
                EntityFieldsbeepGridPro.Uow = null;
                EntityFieldsbeepGridPro.DataSource = viewModel.Fields;
            }
            ConfigureEditorsFromEntityFieldProperties();
            ConfigureFieldTypeColumn();
            RefreshEditorModeState(viewModel.EntityName);
            RefreshProgressiveDisclosure(viewModel.EntityName);
        }

        private void RefreshEditorModeState(string? entityName)
        {
            var canApply = viewModel?.SourceConnection != null;
            if (!canApply)
            {
                _mode = EntityEditorMode.CreateNew;
                ApplybeepButton.Text = "Create Entity";
                ApplybeepButton.Enabled = false;
                RefreshApplyButtonIcon();
                return;
            }

            var normalizedName = entityName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                _mode = EntityEditorMode.CreateNew;
                ApplybeepButton.Text = "Create Entity";
                ApplybeepButton.Enabled = true;
                RefreshApplyButtonIcon();
                return;
            }

            var exists = false;
            try
            {
                exists = viewModel!.SourceConnection.CheckEntityExist(normalizedName);
            }
            catch
            {
                exists = false;
            }

            _mode = exists ? EntityEditorMode.UpdateExisting : EntityEditorMode.CreateNew;
            if (_mode == EntityEditorMode.CreateNew)
            {
                ApplybeepButton.Text = "Create Entity";
                ApplybeepButton.Enabled = true;
                RefreshApplyButtonIcon();
                return;
            }

            var helper = beepService?.DMEEditor?.GetDataSourceHelper(viewModel!.SourceConnection.DatasourceType);
            var canEvolve = helper?.Capabilities == null || helper.Capabilities.SupportsSchemaEvolution;
            ApplybeepButton.Text = canEvolve ? "Update Schema" : "Update Not Supported";
            ApplybeepButton.Enabled = canEvolve;
            RefreshApplyButtonIcon();

            _stateLabel.Text = canEvolve
                ? $"Mode: Update existing schema | {_lastSummary}"
                : $"Mode: Update unavailable for datasource type '{viewModel.SourceConnection.DatasourceType}'";
        }

        private EntityStructure BuildDraftStructure(string entityName)
        {
            var structure = viewModel?.Structure != null ? (EntityStructure)viewModel.Structure.Clone() : new EntityStructure();
            structure.EntityName = entityName.Trim();
            structure.DatasourceEntityName = entityName.Trim();
            structure.DatabaseType = viewModel?.SourceConnection?.DatasourceType ?? structure.DatabaseType;
            structure.Fields = ExtractDraftFields();
            return structure;
        }

        private List<EntityField> ExtractDraftFields()
        {
            var fields = new List<EntityField>();
            var source = viewModel?.DBWork?.Units as IEnumerable<object>;
            if (source != null)
            {
                fields.AddRange(source.OfType<EntityField>().Select(CloneField));
            }
            else if (viewModel?.Fields is IEnumerable<object> fallback)
            {
                fields.AddRange(fallback.OfType<EntityField>().Select(CloneField));
            }

            return fields;
        }

        private static EntityField CloneField(EntityField input)
        {
            return new EntityField
            {
                FieldName = input.FieldName,
                Originalfieldname = input.Originalfieldname,
                Fieldtype = input.Fieldtype,
                FieldCategory = input.FieldCategory,
                Size1 = input.Size1,
                Size = input.Size,
                Size2 = input.Size2,
                NumericPrecision = input.NumericPrecision,
                NumericScale = input.NumericScale,
                AllowDBNull = input.AllowDBNull,
                IsKey = input.IsKey,
                IsAutoIncrement = input.IsAutoIncrement,
                IsUnique = input.IsUnique,
                IsIdentity = input.IsIdentity,
                ValueRetrievedFromParent = input.ValueRetrievedFromParent,
                IsReadOnly = input.IsReadOnly,
                IsDisplayField = input.IsDisplayField,
                EntityName = input.EntityName,
                Description = input.Description,
                DefaultValue = input.DefaultValue
            };
        }

        private bool ValidateDraft(EntityStructure draft)
        {
            if (draft == null || string.IsNullOrWhiteSpace(draft.EntityName))
            {
                LogStatus("Validation failed: entity name is required.", Errors.Failed);
                return false;
            }

            if (draft.Fields == null || draft.Fields.Count == 0)
            {
                LogStatus("Validation failed: at least one field is required.", Errors.Failed);
                return false;
            }

            var duplicate = draft.Fields
                .Where(f => !string.IsNullOrWhiteSpace(f?.FieldName))
                .GroupBy(f => f.FieldName, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);
            if (duplicate != null)
            {
                LogStatus($"Validation failed: duplicate field name '{duplicate.Key}'.", Errors.Failed);
                return false;
            }

            var helper = beepService?.DMEEditor?.GetDataSourceHelper(viewModel!.SourceConnection.DatasourceType);
            if (helper != null)
            {
                try
                {
                    var (isValid, errors) = helper.ValidateEntity(draft);
                    if (!isValid)
                    {
                        var message = errors != null && errors.Count > 0
                            ? string.Join("; ", errors)
                            : "Unknown validation error.";
                        LogStatus($"Validation failed: {message}", Errors.Failed);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    LogStatus($"Validation warning: helper validation skipped ({ex.Message}).", Errors.Warning);
                }
            }

            return true;
        }

        private void ExecuteCreate(EntityStructure draft)
        {
            var source = viewModel?.SourceConnection;
            if (source == null)
            {
                LogStatus("Create failed: datasource is not available.", Errors.Failed);
                return;
            }

            if (source.CheckEntityExist(draft.EntityName))
            {
                LogStatus($"Create blocked: entity '{draft.EntityName}' already exists.", Errors.Failed);
                return;
            }

            var started = DateTime.Now;
            var created = source.CreateEntityAs(draft);
            var elapsed = DateTime.Now - started;
            if (!created)
            {
                var details = source.ErrorObject?.Message ?? "Unknown datasource error.";
                LogStatus($"Create failed: {details}", Errors.Failed);
                return;
            }

            _lastSummary = $"Created {draft.EntityName} in {elapsed.TotalMilliseconds:0} ms";
            LogStatus(_lastSummary, Errors.Ok);
            LoadEntitiesList();
            SelectEntity(draft.EntityName);
            LoadOrCreateEntity(draft.EntityName);
            RefreshEditorModeState(draft.EntityName);
        }

        private sealed class SchemaStep
        {
            public string Description { get; set; } = string.Empty;
            public string Sql { get; set; } = string.Empty;
        }

        private void ExecuteUpdate(EntityStructure desired)
        {
            var source = viewModel?.SourceConnection;
            if (source == null)
            {
                LogStatus("Update failed: datasource is not available.", Errors.Failed);
                return;
            }

            if (!source.CheckEntityExist(desired.EntityName))
            {
                LogStatus($"Update blocked: entity '{desired.EntityName}' does not exist.", Errors.Failed);
                return;
            }

            var helper = beepService?.DMEEditor?.GetDataSourceHelper(source.DatasourceType);
            if (helper == null)
            {
                LogStatus($"Update not supported: no helper registered for '{source.DatasourceType}'.", Errors.Failed);
                return;
            }

            if (helper.Capabilities != null && !helper.Capabilities.SupportsSchemaEvolution)
            {
                LogStatus($"Update not supported: datasource '{source.DatasourceType}' does not support schema evolution.", Errors.Failed);
                return;
            }

            var current = source.GetEntityStructure(desired.EntityName, true);
            if (current?.Fields == null)
            {
                LogStatus("Update failed: unable to resolve current entity structure.", Errors.Failed);
                return;
            }

            var steps = BuildSchemaSteps(helper, desired.EntityName, current.Fields, desired.Fields ?? new List<EntityField>());
            if (steps.Count == 0)
            {
                LogStatus("No schema changes detected.", Errors.Information);
                return;
            }

            var preview = BuildPreviewMessage(steps);
            var confirm = MessageBox.Show(preview, "Apply Schema Update", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
            {
                LogStatus("Update cancelled by user.", Errors.Warning);
                return;
            }

            var started = DateTime.Now;
            foreach (var step in steps)
            {
                var result = source.ExecuteSql(step.Sql);
                if (result == null || result.Flag == Errors.Failed)
                {
                    var msg = result?.Message ?? "Unknown execution error.";
                    LogStatus($"Update failed at step '{step.Description}': {msg}", Errors.Failed);
                    return;
                }
            }

            var elapsed = DateTime.Now - started;
            _lastSummary = $"Updated {desired.EntityName} ({steps.Count} steps) in {elapsed.TotalMilliseconds:0} ms";
            LogStatus(_lastSummary, Errors.Ok);
            LoadEntitiesList();
            SelectEntity(desired.EntityName);
            LoadOrCreateEntity(desired.EntityName);
            RefreshEditorModeState(desired.EntityName);
        }

        private static List<SchemaStep> BuildSchemaSteps(IDataSourceHelper helper, string entityName, List<EntityField> currentFields, List<EntityField> desiredFields)
        {
            var steps = new List<SchemaStep>();
            var current = currentFields
                .Where(f => !string.IsNullOrWhiteSpace(f?.FieldName))
                .ToDictionary(f => f.FieldName, StringComparer.OrdinalIgnoreCase);
            var desired = desiredFields
                .Where(f => !string.IsNullOrWhiteSpace(f?.FieldName))
                .ToDictionary(f => f.FieldName, StringComparer.OrdinalIgnoreCase);

            foreach (var kv in desired)
            {
                if (!current.ContainsKey(kv.Key))
                {
                    var (sql, success, _) = helper.GenerateAddColumnSql(entityName, kv.Value);
                    if (success && !string.IsNullOrWhiteSpace(sql))
                    {
                        steps.Add(new SchemaStep { Description = $"Add column {kv.Key}", Sql = sql });
                    }
                }
            }

            foreach (var kv in desired)
            {
                if (!current.TryGetValue(kv.Key, out var oldField))
                {
                    continue;
                }

                if (FieldsEqual(oldField, kv.Value))
                {
                    continue;
                }

                var (sql, success, _) = helper.GenerateAlterColumnSql(entityName, kv.Key, kv.Value);
                if (success && !string.IsNullOrWhiteSpace(sql))
                {
                    steps.Add(new SchemaStep { Description = $"Alter column {kv.Key}", Sql = sql });
                }
            }

            foreach (var kv in current)
            {
                if (desired.ContainsKey(kv.Key))
                {
                    continue;
                }

                var (sql, success, _) = helper.GenerateDropColumnSql(entityName, kv.Key);
                if (success && !string.IsNullOrWhiteSpace(sql))
                {
                    steps.Add(new SchemaStep { Description = $"Drop column {kv.Key}", Sql = sql });
                }
            }

            return steps;
        }

        private static bool FieldsEqual(EntityField current, EntityField desired)
        {
            if (current == null || desired == null)
            {
                return false;
            }

            return string.Equals(current.Fieldtype, desired.Fieldtype, StringComparison.OrdinalIgnoreCase)
                   && current.Size1 == desired.Size1
                   && current.NumericPrecision == desired.NumericPrecision
                   && current.NumericScale == desired.NumericScale
                   && current.AllowDBNull == desired.AllowDBNull;
        }

        private static string BuildPreviewMessage(List<SchemaStep> steps)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Apply {steps.Count} schema changes?");
            sb.AppendLine();
            foreach (var step in steps.Take(8))
            {
                sb.AppendLine($"- {step.Description}");
            }

            if (steps.Count > 8)
            {
                sb.AppendLine($"- ... and {steps.Count - 8} more");
            }

            return sb.ToString();
        }

        private void LogStatus(string message, Errors flag)
        {
            _lastSummary = message ?? string.Empty;
            beepService?.DMEEditor?.AddLogMessage("EntityEditor", _lastSummary, DateTime.Now, 0, viewModel?.EntityName, flag);
            _stateLabel.Text = $"Status: {_lastSummary}";
        }

        private void Uc_EntityEditor_Resize(object? sender, EventArgs e)
        {
            ApplyResponsiveLayout();
        }

        private void ApplyResponsiveLayout()
        {
            if (IsDisposed)
            {
                return;
            }

            const int outerMargin = 16;
            const int sectionGap = 12;
            const int topY = 44;
            const int controlHeight = 44;
            const int applyWidth = 170;
            const int minComboWidth = 180;
            const int gridTop = 132;
            const int gridBottomGap = 16;

            _titleLabel.Location = new System.Drawing.Point(outerMargin, 12);
            _titleLabel.Size = new System.Drawing.Size(Math.Max(220, Width / 3), 24);

            var contentWidth = Math.Max(0, Width - (outerMargin * 2));
            var totalGap = sectionGap * 2;
            var availableForCombo = Math.Max((minComboWidth * 2), contentWidth - applyWidth - totalGap);
            var comboWidth = Math.Max(minComboWidth, availableForCombo / 2);

            DatasourcebeepComboBox.Location = new System.Drawing.Point(outerMargin, topY);
            DatasourcebeepComboBox.Size = new System.Drawing.Size(comboWidth, controlHeight);

            EntitiesbeepComboBox.Location = new System.Drawing.Point(DatasourcebeepComboBox.Right + sectionGap, topY);
            EntitiesbeepComboBox.Size = new System.Drawing.Size(comboWidth, controlHeight);

            ApplybeepButton.Location = new System.Drawing.Point(Math.Max(outerMargin, Width - outerMargin - applyWidth), topY + 4);
            ApplybeepButton.Size = new System.Drawing.Size(applyWidth, 36);
            ApplybeepButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            _stateLabel.Location = new System.Drawing.Point(outerMargin, 96);
            _stateLabel.Size = new System.Drawing.Size(Math.Max(120, Width - (outerMargin * 2)), 24);

            EntityFieldsbeepGridPro.Location = new System.Drawing.Point(outerMargin, gridTop);
            EntityFieldsbeepGridPro.Size = new System.Drawing.Size(
                Math.Max(240, Width - (outerMargin * 2)),
                Math.Max(180, Height - gridTop - gridBottomGap));
            EntityFieldsbeepGridPro.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        }

        private void RefreshProgressiveDisclosure(string? entityName)
        {
            var hasDatasource = viewModel?.SourceConnection != null;
            var hasEntityName = !string.IsNullOrWhiteSpace(entityName);

            EntitiesbeepComboBox.Enabled = hasDatasource;
            EntityFieldsbeepGridPro.Enabled = hasDatasource && hasEntityName;
            ApplybeepButton.Enabled = ApplybeepButton.Enabled && hasDatasource;

            if (!hasDatasource)
            {
                _stateLabel.Text = "Step 1 of 3: Select datasource.";
                return;
            }

            if (!hasEntityName)
            {
                _stateLabel.Text = "Step 2 of 3: Select existing entity or type a new entity name.";
                return;
            }

            _stateLabel.Text = _mode == EntityEditorMode.CreateNew
                ? $"Step 3 of 3: Review fields and create new entity '{entityName}'."
                : $"Step 3 of 3: Review diff and update schema for '{entityName}'.";
        }

        private void ConfigureEditorsFromEntityFieldProperties()
        {
            if (EntityFieldsbeepGridPro?.Columns == null)
            {
                return;
            }

            foreach (var column in EntityFieldsbeepGridPro.Columns)
            {
                if (column == null || string.IsNullOrWhiteSpace(column.ColumnName))
                {
                    continue;
                }

                if (string.Equals(column.ColumnName, "Fieldtype", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var propertyType = ResolveEntityFieldPropertyType(column);
                if (propertyType == null)
                {
                    continue;
                }

                var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                if (type == typeof(bool))
                {
                    column.CellEditor = BeepColumnType.CheckBoxBool;
                }
                else if (type == typeof(char))
                {
                    column.CellEditor = BeepColumnType.CheckBoxChar;
                }
            }
        }

        private static Type? ResolveEntityFieldPropertyType(BeepColumnConfig column)
        {
            if (!string.IsNullOrWhiteSpace(column.PropertyTypeName))
            {
                var resolved = Type.GetType(column.PropertyTypeName, throwOnError: false);
                if (resolved != null)
                {
                    return resolved;
                }
            }

            var prop = typeof(EntityField).GetProperty(
                column.ColumnName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            return prop?.PropertyType;
        }

        private void ConfigureFieldTypeColumn()
        {
            if (EntityFieldsbeepGridPro?.Columns == null || viewModel == null)
            {
                return;
            }

            var fieldTypeColumn = EntityFieldsbeepGridPro.Columns.FirstOrDefault(c =>
                string.Equals(c.ColumnName, "Fieldtype", StringComparison.OrdinalIgnoreCase));

            if (fieldTypeColumn == null)
            {
                return;
            }

            var typeItems = (viewModel.DatatypeMappings ?? Enumerable.Empty<DatatypeMapping>())
                .Where(m => !string.IsNullOrWhiteSpace(m.NetDataType))
                .GroupBy(m => m.NetDataType, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(m => m.Fav).First())
                .OrderByDescending(m => m.Fav)
                .ThenBy(m => m.NetDataType)
                .Select(m => new SimpleItem
                {
                    DisplayField = m.NetDataType,
                    Text = m.NetDataType,
                    Name = m.NetDataType,
                    Value = m.NetDataType
                })
                .ToList();

            fieldTypeColumn.CellEditor = BeepColumnType.ComboBox;
            fieldTypeColumn.Items = typeItems;
            fieldTypeColumn.EnumSourceType = null;
            fieldTypeColumn.QueryToGetValues = null;
        }
       
    }
}
