using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        /// <summary>
        /// Cancels the in-flight schema apply. The engine exposes no async DDL — every
        /// MigrationManager and IDataSource call here is synchronous and uncancellable once it has
        /// started — so this stops the run <em>between</em> operations rather than interrupting one.
        /// That still matters: an update is a sequence of add/alter/drop round-trips, and cancelling
        /// after the adds avoids the destructive tail.
        /// </summary>
        private CancellationTokenSource? _applyCts;

        /// <summary>
        /// Cached CheckEntityExist result and the entity it describes. Create-vs-Update mode is
        /// re-derived on every SyncBindings, and CheckEntityExist is a blocking round-trip to the
        /// datasource — probing it on each binding refresh put I/O on the UI thread for what is
        /// almost always the same answer. Invalidated whenever the datasource or entity changes,
        /// and after an apply.
        /// </summary>
        private string? _probedEntity;
        private bool _probedExists;

        /// <summary>
        /// Bumped whenever the datasource or entity selection changes. Every async load captures it
        /// and abandons its result if superseded. Without this, a slow load for datasource A can
        /// resume after the user has already switched to B and rebind SourceConnection to A while
        /// the combo still reads B — which would point the next Apply, including DropColumn, at the
        /// wrong database.
        /// </summary>
        private int _selectionGeneration;

        private int NextSelectionGeneration() => ++_selectionGeneration;
        private bool IsCurrentSelection(int generation) => generation == _selectionGeneration;

        /// <summary>
        /// Designer/parameterless ctor. Must not chain to the IServiceProvider overload with null —
        /// that resolves services off a null provider and throws. Configure() supplies the services
        /// at runtime, so everything below is null-safe without them.
        /// </summary>
        public uc_EntityEditor()
        {
            InitializeComponent();
            Details.AddinName = "Entity Editor";
            WireButtonEvents();
        }

        public uc_EntityEditor(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            Details.AddinName = "Entity Editor";
            WireButtonEvents();
            // No ApplyDpiScaledLayout() here: the base drives it from OnHandleCreated, where the
            // scale factor is real. Calling it from the ctor scaled nothing (handle not yet created).
        }

        // ── Skill § "Sizing tokens": DPI-scaled overrides applied in
        //    code-behind after InitializeComponent(). The Designer owns all
        //    Size / Location / Dock / Padding values; this method overlays
        //    token-based values that scale with the host display DPI.

        protected override void ApplyDpiScaledLayout()
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

        // Fire-and-forget: the handler catches its own failures, and awaiting is not an option on a
        // SelectedItemChanged signature that returns void.
        private void DatasourcebeepComboBox_SelectedItemChanged(object? sender, SelectedItemChangedEventArgs e) =>
            _ = DatasourceChangedAsync(e);

        private async Task DatasourceChangedAsync(SelectedItemChangedEventArgs? e)
        {
            if (e?.SelectedItem == null || _viewModel == null || beepService?.DMEEditor == null) return;
            if (_isApplyingSchema) return;

            string dsName = e.SelectedItem.Text ?? string.Empty;
            int generation = NextSelectionGeneration();
            _viewModel.Datasourcename = dsName;
            _viewModel.EntityName = string.Empty;
            InvalidateEntityProbe();

            try
            {
                // GetDataSource resolves a driver and may activate a plugin; Openconnection is a
                // network round-trip. Both blocked the UI thread here.
                var ds = await Task.Run(() => beepService.DMEEditor.GetDataSource(dsName)).ConfigureAwait(true);
                if (!IsCurrentSelection(generation)) return;

                _viewModel.SourceConnection = ds;
                if (ds == null) { LogStatus("Datasource not found", Errors.Failed); return; }

                if (ds.ConnectionStatus != ConnectionState.Open)
                {
                    LogStatus($"Opening '{dsName}'…", Errors.Information);
                    await Task.Run(() => ds.Openconnection()).ConfigureAwait(true);
                    if (!IsCurrentSelection(generation)) return;
                    if (ds.ConnectionStatus != ConnectionState.Open)
                    { LogStatus("Could not open datasource", Errors.Failed); return; }
                }

                _viewModel.UpdateFieldTypes();
                ConfigureFieldTypeColumn();
                _viewModel.Structure = null; _viewModel.DBWork = null;
                _viewModel.Fields = null; _viewModel.EntityName = null;
                _lastSummary = $"Datasource: {dsName}";
                SyncBindings();
                await LoadEntitiesListAsync(generation).ConfigureAwait(true);
                if (!IsCurrentSelection(generation)) return;
                RefreshProgressiveDisclosure(_viewModel.EntityName);
            }
            catch (Exception ex)
            {
                if (!IsCurrentSelection(generation)) return;
                LogStatus($"Datasource '{dsName}' failed: {ex.Message}", Errors.Failed);
            }
        }

        // ── Entity selection ───────────────────────────────────────────────

        private void EntitiesbeepComboBox_SelectedItemChanged(object? sender, SelectedItemChangedEventArgs e) =>
            _ = EntityChangedAsync(e);

        private async Task EntityChangedAsync(SelectedItemChangedEventArgs? e)
        {
            if (e?.SelectedItem == null || _viewModel == null) return;
            if (_isApplyingSchema) return;

            var name = e.SelectedItem.Text;
            int generation = NextSelectionGeneration();

            try
            {
                LoadOrCreateEntity(name);
                // Resolve Create-vs-Update off-thread before deriving the mode, so the button label
                // and the Apply path agree about whether this entity already exists.
                if (!string.IsNullOrWhiteSpace(name))
                    await ProbeEntityExistsAsync(name.Trim(), CancellationToken.None).ConfigureAwait(true);
                if (!IsCurrentSelection(generation)) return;
                RefreshEditorModeState(name);
                RefreshProgressiveDisclosure(name);
            }
            catch (Exception ex)
            {
                if (!IsCurrentSelection(generation)) return;
                LogStatus($"Could not load '{name}': {ex.Message}", Errors.Failed);
            }
        }

        /// <summary>
        /// Lists entities on the current connection. GetEntitesList is a blocking metadata
        /// round-trip, so it runs off the UI thread. Results are dropped if the selection moved on.
        /// </summary>
        private async Task LoadEntitiesListAsync(int? generation = null)
        {
            int gen = generation ?? _selectionGeneration;
            EntitiesbeepComboBox.ListItems = new BindingList<SimpleItem>();
            var ds = _viewModel?.SourceConnection;
            if (ds == null) return;

            IEnumerable<string>? names = null;
            try
            {
                names = await Task.Run(() => ds.GetEntitesList()).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                if (!IsCurrentSelection(gen)) return;
                LogStatus($"Could not list entities: {ex.Message}", Errors.Failed);
                return;
            }

            if (!IsCurrentSelection(gen)) return;

            foreach (var n in names ?? Enumerable.Empty<string>())
                EntitiesbeepComboBox.ListItems.Add(new SimpleItem { DisplayField = n, Text = n, Name = n, Value = n });
            if (!string.IsNullOrWhiteSpace(_viewModel?.EntityName)) SelectEntity(_viewModel.EntityName);
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
            _ = NavigatedToAsync(parameters);
        }

        /// <summary>
        /// Applies navigation parameters. Async because resolving the datasource, listing entities
        /// and probing whether the entity exists are all blocking round-trips; the base override
        /// returns void and carries no completion contract, so the work continues after it returns.
        /// </summary>
        private async Task NavigatedToAsync(Dictionary<string, object> parameters)
        {
            if (_viewModel == null || beepService?.DMEEditor == null) return;

            // Freezing the combos is not enough: navigation does not go through them. The addin tree
            // stays live during an apply, and ShowPage("uc_EntityEditor", …) on a cached InControl
            // instance re-enters here — repointing SourceConnection at a different datasource while
            // the DDL awaits are still in flight, so the operations would land on the wrong database.
            if (_isApplyingSchema)
            {
                LogStatus("Schema operation in progress — navigation ignored.", Errors.Warning);
                return;
            }

            int generation = NextSelectionGeneration();
            try
            {
                if (parameters.TryGetValue("Datasource", out var dsObj))
                {
                    _viewModel.Datasourcename = dsObj?.ToString() ?? "";
                    var ds = await Task.Run(() =>
                        beepService.DMEEditor.GetDataSource(_viewModel.Datasourcename)).ConfigureAwait(true);
                    if (!IsCurrentSelection(generation)) return;
                    _viewModel.SourceConnection = ds;
                    _viewModel.EntityName = ""; _viewModel.UpdateFieldTypes(); ConfigureFieldTypeColumn();
                    InvalidateEntityProbe();
                    await LoadEntitiesListAsync(generation).ConfigureAwait(true);
                    if (!IsCurrentSelection(generation)) return;
                }
                if (parameters.TryGetValue("EntityName", out var entObj))
                {
                    _viewModel.EntityName = entObj?.ToString() ?? "";
                    if (_viewModel.SourceConnection == null)
                    {
                        var ds2 = await Task.Run(() =>
                            beepService.DMEEditor.GetDataSource(_viewModel.Datasourcename)).ConfigureAwait(true);
                        if (!IsCurrentSelection(generation)) return;
                        _viewModel.SourceConnection = ds2;
                    }
                    _viewModel.LoadOrCreateEntityStructure(_viewModel.EntityName);
                    _viewModel.IsNew = false; _viewModel.IsChanged = false;

                    // Must precede RefreshEditorModeState: mode is read from the probe cache, so
                    // navigating to an existing entity without probing would show "Create Entity".
                    if (!string.IsNullOrWhiteSpace(_viewModel.EntityName))
                        await ProbeEntityExistsAsync(_viewModel.EntityName.Trim(), CancellationToken.None).ConfigureAwait(true);
                    if (!IsCurrentSelection(generation)) return;
                    RefreshEditorModeState(_viewModel.EntityName);
                }
                else { _viewModel.IsNew = true; _mode = EntityEditorMode.CreateNew; }
                SyncBindings();
            }
            catch (Exception ex)
            {
                if (!IsCurrentSelection(generation)) return;
                LogStatus($"Navigation failed: {ex.Message}", Errors.Failed);
            }
        }

        // ── Apply (Create / Update via MigrationManager) ───────────────────

        private void ApplybeepButton_Click(object? sender, EventArgs e) => _ = ApplyAsync();

        private async Task ApplyAsync()
        {
            if (_viewModel == null || beepService?.DMEEditor == null) return;
            if (_viewModel.SourceConnection == null) { LogStatus("Select a datasource first", Errors.Failed); return; }
            if (_isApplyingSchema) { LogStatus("Schema operation already running.", Errors.Warning); return; }

            string entityName = "";

            // Everything from the guard onward is inside the try: this method is fire-and-forget, so
            // an exception escaping it would be observed by nobody, and one thrown after the guard
            // went up but before the finally would wedge _isApplyingSchema true for the control's
            // lifetime — permanently disabling Apply.
            _isApplyingSchema = true;
            try
            {
                entityName = GetEntityNameFromUi() ?? "";
                if (string.IsNullOrWhiteSpace(entityName)) { LogStatus("Select or type an entity name", Errors.Failed); return; }
                if (_viewModel.Structure == null || !string.Equals(_viewModel.EntityName, entityName, StringComparison.OrdinalIgnoreCase))
                    if (!LoadOrCreateEntity(entityName)) return;
                fieldsBindingSource.EndEdit();
                if (BindingContext?[fieldsBindingSource] is CurrencyManager cm) cm.EndCurrentEdit();

                _applyCts?.Cancel();
                _applyCts?.Dispose();
                _applyCts = new CancellationTokenSource();
                var token = _applyCts.Token;

                Cursor = Cursors.WaitCursor;
                // Locks the selection for the duration: the combos stay live during the awaits
                // otherwise, and a datasource switch mid-apply would move the target out from under
                // the DDL.
                RefreshProgressiveDisclosure(entityName);

                var draft = BuildDraftStructure(entityName);
                if (!ValidateDraft(draft)) return;

                // Re-probe rather than trusting the cache: this decides create-vs-update, and acting
                // on a stale answer would either recreate a live entity or try to alter one that is
                // not there.
                //
                // Dispatch on the probe's RETURN value, not on _mode or the _probedExists field.
                // The cache is a UI convenience that a superseded probe deliberately declines to
                // publish, so the field can be unset while this answer is good; the return value is
                // the only authoritative one.
                InvalidateEntityProbe();
                bool? entityExists = await ProbeEntityExistsAsync(entityName, token).ConfigureAwait(true);
                RefreshEditorModeState(entityName);

                // Refuse to guess. A null here means CheckEntityExist faulted, and both branches are
                // unsafe on an unknown: create would mutate a live entity, update would diff against
                // one that may not be there. The mode label may still read "Create Entity" — that
                // display default is fine; acting on it is not.
                if (entityExists == null)
                {
                    LogStatus($"Could not determine whether '{entityName}' exists — nothing applied. " +
                              "Check the datasource and retry.", Errors.Failed);
                    return;
                }

                if (!entityExists.Value) await ExecuteCreateAsync(draft, token).ConfigureAwait(true);
                else await ExecuteUpdateAsync(draft, token).ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                LogStatus("Schema operation cancelled — changes already applied remain in place.", Errors.Warning);
            }
            catch (Exception ex)
            {
                // ExecuteUpdate had no exception boundary at all: MigrationManager.AlterColumn and
                // DropColumn do not try/catch, so a provider fault surfaced as an unhandled crash.
                LogStatus($"Schema operation failed: {ex.Message}", Errors.Failed);
                beepService?.DMEEditor?.AddLogMessage("EntityEditor",
                    $"Apply '{entityName}' threw: {ex}", DateTime.Now, 0, entityName, Errors.Failed);
            }
            finally
            {
                // Deliberately does NOT invalidate the probe. ExecuteCreateAsync/ExecuteUpdateAsync
                // each end by re-probing the entity they just touched, and dropping that here would
                // leave RefreshEditorModeState with an empty cache — showing "Create Entity" for the
                // entity that was just successfully created or updated.
                _isApplyingSchema = false;
                Cursor = Cursors.Default;
                SyncBindings();
            }
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

        /// <summary>
        /// Drops the cached CheckEntityExist answer, forcing the next probe to re-ask. Resets the
        /// flag as well as the key: leaving a stale <c>_probedExists</c> behind an unset
        /// <c>_probedEntity</c> would let any raw read of the flag see the previous entity's answer.
        /// </summary>
        private void InvalidateEntityProbe()
        {
            _probedEntity = null;
            _probedExists = false;
        }

        /// <summary>
        /// Resolves whether <paramref name="entityName"/> exists, off the UI thread, and caches it.
        /// </summary>
        /// <returns>
        /// true/false when the datasource answered; <c>null</c> when it could not be determined.
        /// </returns>
        /// <remarks>
        /// The null case is the whole point of the nullable return. A swallowed CheckEntityExist
        /// fault must never read as "confirmed absent": Apply dispatches create-vs-update on this
        /// value, and a faulted-false would send a live entity down the create path, where
        /// EnsureEntity(addMissingColumns: true) would quietly add columns to it and report it as
        /// created. "Unknown" and "not there" are different answers and only one of them is safe to
        /// act on.
        /// </remarks>
        private async Task<bool?> ProbeEntityExistsAsync(string entityName, CancellationToken token)
        {
            if (string.Equals(_probedEntity, entityName, StringComparison.OrdinalIgnoreCase))
                return _probedExists;

            var ds = _viewModel?.SourceConnection;
            if (ds == null) return null;

            int generation = _selectionGeneration;
            bool? exists;
            try
            {
                exists = await Task.Run(() => ds.CheckEntityExist(entityName), token).ConfigureAwait(true);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                // Report the fault rather than hiding it, and return "unknown" — not false.
                beepService?.DMEEditor?.AddLogMessage("EntityEditor",
                    $"CheckEntityExist('{entityName}') threw: {ex.Message}",
                    DateTime.Now, 0, entityName, Errors.Warning);
                exists = null;
            }

            // A newer selection landed while this probe was in flight — publishing now would cache
            // entity A's answer under the belief that it describes B, and the mode is read from it.
            if (!IsCurrentSelection(generation)) return exists;

            if (exists.HasValue) { _probedEntity = entityName; _probedExists = exists.Value; }
            else InvalidateEntityProbe();   // never cache an unknown as an answer
            return exists;
        }

        /// <summary>
        /// Derives Create-vs-Update mode from the cached probe. Synchronous by design: it runs on
        /// every SyncBindings, so it must not do I/O. When the cache does not cover
        /// <paramref name="entityName"/> the editor stays in Create mode until a probe resolves it —
        /// the callers that change the entity or datasource kick that probe off.
        /// </summary>
        private void RefreshEditorModeState(string? entityName)
        {
            if (_viewModel?.SourceConnection == null) { _mode = EntityEditorMode.CreateNew; ApplybeepButton.Text = "Create Entity"; ApplybeepButton.Enabled = false; RefreshApplyButtonIcon(); return; }
            if (string.IsNullOrWhiteSpace(entityName?.Trim())) { _mode = EntityEditorMode.CreateNew; ApplybeepButton.Text = "Create Entity"; ApplybeepButton.Enabled = true; RefreshApplyButtonIcon(); return; }

            bool exists = string.Equals(_probedEntity, entityName.Trim(), StringComparison.OrdinalIgnoreCase) && _probedExists;
            _mode = exists ? EntityEditorMode.UpdateExisting : EntityEditorMode.CreateNew;
            // Never re-enable Apply while a schema operation is in flight. This runs mid-apply (the
            // mode is re-derived after the pre-apply probe), and _isApplyingSchema would otherwise
            // be the only thing between a second click and a concurrent DDL run.
            if (_mode == EntityEditorMode.CreateNew) { ApplybeepButton.Text = "Create Entity"; ApplybeepButton.Enabled = !_isApplyingSchema; RefreshApplyButtonIcon(); return; }
            var helper = beepService?.DMEEditor?.GetDataSourceHelper(_viewModel.SourceConnection.DatasourceType);
            bool canEvolve = helper?.Capabilities == null || helper.Capabilities.SupportsSchemaEvolution;
            ApplybeepButton.Text = canEvolve ? "Update Schema" : "Update Not Supported"; ApplybeepButton.Enabled = canEvolve && !_isApplyingSchema;
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
                if (h != null)
                {
                    try { var (ok, errs) = h.ValidateEntity(draft); if (!ok) { LogStatus(string.Join("; ", errs ?? new List<string>()), Errors.Failed); return false; } }
                    catch (Exception ex)
                    {
                        // Helper validation is best-effort — a helper that throws must not block the
                        // draft — but the fault is reported rather than swallowed.
                        beepService?.DMEEditor?.AddLogMessage("EntityEditor",
                            $"Helper validation for '{_viewModel.SourceConnection.DatasourceType}' threw: {ex.Message}",
                            DateTime.Now, 0, draft?.EntityName, Errors.Warning);
                    }
                }
            }
            return true;
        }

        // ── Create / Update ────────────────────────────────────────────────

        /// <summary>
        /// Creates the entity through <c>MigrationManager.EnsureEntity</c>, which routes to the
        /// datasource's <c>ISchemaMigrationProvider</c>. No SQL is built here.
        /// </summary>
        private async Task ExecuteCreateAsync(EntityStructure draft, CancellationToken token)
        {
            var ds = _viewModel?.SourceConnection;
            if (ds == null) { LogStatus("Datasource not available.", Errors.Failed); return; }

            // No existence check here: ApplyAsync probed immediately before dispatching and only
            // routes here when the entity does not exist. Re-reading the cache would reintroduce the
            // stale-answer risk this path was restructured to remove.
            var started = DateTime.Now;
            LogStatus($"Creating '{draft.EntityName}'…", Errors.Information);

            // EnsureEntity is synchronous and does a CheckEntityExist plus a CreateEntityAs
            // round-trip; the engine exposes no async variant, so the offload happens here.
            var migration = new MigrationManager(beepService!.DMEEditor, ds);
            IErrorsInfo? result;
            try
            {
                result = await Task.Run(() => migration.EnsureEntity(draft), token).ConfigureAwait(true);
            }
            finally
            {
                // Logged even on failure: EnsureEntity can partially apply (it adds columns one
                // round-trip at a time), so the evidence is the only record of what landed.
                LogDdlEvidence(migration, draft.EntityName);
            }

            if (result == null || result.Flag != Errors.Ok)
            { LogStatus($"Create: {result?.Message}", Errors.Failed); return; }

            _lastSummary = $"Created {draft.EntityName} in {(DateTime.Now - started).TotalMilliseconds:0} ms";
            LogStatus(_lastSummary, Errors.Ok);

            InvalidateEntityProbe();
            await LoadEntitiesListAsync().ConfigureAwait(true);
            SelectEntity(draft.EntityName); LoadOrCreateEntity(draft.EntityName);
            await ProbeEntityExistsAsync(draft.EntityName, token).ConfigureAwait(true);
            RefreshEditorModeState(draft.EntityName);
        }

        /// <summary>
        /// Applies the field diff through <c>MigrationManager</c>, matching <see cref="ExecuteCreateAsync"/>.
        /// </summary>
        /// <remarks>
        /// This used to generate its own add/alter/drop SQL from <c>IDataSourceHelper</c> and feed it
        /// straight to <c>ds.ExecuteSql</c>. MigrationManager routes alter/drop through the
        /// per-datasource <c>ISchemaMigrationProvider</c> rather than raw DDL, and records DDL
        /// evidence — so going through it keeps this path datasource-agnostic and auditable instead
        /// of quietly diverging from every other schema-change surface in the app.
        /// </remarks>
        private async Task ExecuteUpdateAsync(EntityStructure desired, CancellationToken token)
        {
            var ds = _viewModel?.SourceConnection;
            if (ds == null) { LogStatus("Datasource not available.", Errors.Failed); return; }
            // Existence was established by ApplyAsync's probe immediately before dispatch; see
            // ExecuteCreateAsync for why the cache field is not consulted here.
            var helper = beepService?.DMEEditor?.GetDataSourceHelper(ds.DatasourceType);
            if (helper == null) { LogStatus($"No helper for '{ds.DatasourceType}'.", Errors.Failed); return; }
            if (helper.Capabilities is { SupportsSchemaEvolution: false }) { LogStatus($"Schema evolution unsupported.", Errors.Failed); return; }

            // refresh: true forces a live metadata round-trip — no cache — so it must not run on the
            // UI thread.
            LogStatus($"Reading current structure of '{desired.EntityName}'…", Errors.Information);
            var cur = await Task.Run(() => ds.GetEntityStructure(desired.EntityName, true), token).ConfigureAwait(true);
            if (cur?.Fields == null) { LogStatus("Cannot resolve current structure.", Errors.Failed); return; }

            var ops = BuildSchemaOps(cur.Fields, desired.Fields ?? new List<EntityField>());
            if (ops.Count == 0) { LogStatus("No schema changes.", Errors.Information); return; }
            if (MessageBox.Show(BuildPreviewMessage(ops), "Apply Schema Update", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            { LogStatus("Update cancelled.", Errors.Warning); return; }

            var started = DateTime.Now;
            var migration = new MigrationManager(beepService!.DMEEditor, ds);

            // There is no transaction around these operations, so a failure or cancellation partway
            // leaves the entity half-migrated. The evidence is therefore logged on EVERY exit path,
            // not just success — it is the only record of which operations actually ran, and it
            // matters most precisely when the sequence did not finish.
            try
            {
                // Adds first (one EnsureEntity covers every missing column), then alters, then
                // drops — destructive last, so a failure earlier leaves the most data intact. The
                // token is checked between operations: the engine's DDL calls are synchronous and
                // cannot be interrupted once started, so cancelling stops the sequence at the next
                // boundary rather than rolling anything back.
                if (ops.Any(o => o.Kind == SchemaOpKind.Add))
                {
                    token.ThrowIfCancellationRequested();
                    LogStatus("Adding column(s)…", Errors.Information);
                    var addRes = await Task.Run(() =>
                        migration.EnsureEntity(desired, createIfMissing: false, addMissingColumns: true), token).ConfigureAwait(true);
                    if (addRes == null || addRes.Flag != Errors.Ok)
                    { LogStatus($"Update failed adding column(s): {addRes?.Message}", Errors.Failed); return; }
                }

                foreach (var op in ops.Where(o => o.Kind == SchemaOpKind.Alter))
                {
                    token.ThrowIfCancellationRequested();
                    LogStatus($"Altering '{op.FieldName}'…", Errors.Information);
                    var res = await Task.Run(() =>
                        migration.AlterColumn(desired.EntityName, op.FieldName, op.Field!), token).ConfigureAwait(true);
                    if (res == null || res.Flag != Errors.Ok)
                    { LogStatus($"Update stopped at '{op.Description}': {res?.Message}. Earlier changes are already applied.", Errors.Failed); return; }
                }

                foreach (var op in ops.Where(o => o.Kind == SchemaOpKind.Drop))
                {
                    token.ThrowIfCancellationRequested();
                    LogStatus($"Dropping '{op.FieldName}'…", Errors.Information);
                    var res = await Task.Run(() =>
                        migration.DropColumn(desired.EntityName, op.FieldName), token).ConfigureAwait(true);
                    if (res == null || res.Flag != Errors.Ok)
                    { LogStatus($"Update stopped at '{op.Description}': {res?.Message}. Earlier changes are already applied.", Errors.Failed); return; }
                }

                _lastSummary = $"Updated {desired.EntityName} ({ops.Count} change(s)) in {(DateTime.Now - started).TotalMilliseconds:0} ms";
                LogStatus(_lastSummary, Errors.Ok);
            }
            finally
            {
                LogDdlEvidence(migration, desired.EntityName);
            }

            InvalidateEntityProbe();
            await LoadEntitiesListAsync().ConfigureAwait(true);
            SelectEntity(desired.EntityName); LoadOrCreateEntity(desired.EntityName);
            await ProbeEntityExistsAsync(desired.EntityName, token).ConfigureAwait(true);
            RefreshEditorModeState(desired.EntityName);
        }

        /// <summary>
        /// Writes the DDL evidence MigrationManager accumulated for a run, from the provider that
        /// actually executed the operations.
        /// </summary>
        /// <remarks>
        /// <c>SqlHash</c> is a SHA prefix, not the statement — the engine deliberately does not
        /// retain the SQL — so this identifies an operation without pretending to show it. The
        /// evidence list is per-MigrationManager-instance and is never cleared, so this must only be
        /// called with an instance created for the run being reported.
        /// </remarks>
        private void LogDdlEvidence(MigrationManager migration, string entityName)
        {
            foreach (var ev in migration.GetDdlEvidence())
                beepService?.DMEEditor?.AddLogMessage("EntityEditor",
                    $"[ddl/{ev.Outcome}] {ev.OperationName} {ev.EntityName}" +
                    (string.IsNullOrWhiteSpace(ev.ColumnName) ? "" : $".{ev.ColumnName}") +
                    $" via {ev.HelperSource}" +
                    (string.IsNullOrWhiteSpace(ev.SqlHash) ? " (no SQL produced)" : $" sql#{ev.SqlHash}") +
                    (string.IsNullOrWhiteSpace(ev.ReasonCode) ? "" : $" [{ev.ReasonCode}]") +
                    (string.IsNullOrWhiteSpace(ev.Message) ? "" : $" — {ev.Message}"),
                    DateTime.Now, 0, entityName,
                    ev.Outcome == DdlOperationOutcome.Executed ? Errors.Ok : Errors.Warning);
        }

        private enum SchemaOpKind { Add, Alter, Drop }

        /// <summary>
        /// Diffs current vs desired fields into datasource-agnostic operations. No SQL is generated
        /// here — MigrationManager owns the DDL for the connected datasource.
        /// </summary>
        private static List<SchemaOp> BuildSchemaOps(List<EntityField> cur, List<EntityField> des)
        {
            var ops = new List<SchemaOp>();
            var cd = cur.Where(f => !string.IsNullOrWhiteSpace(f?.FieldName)).ToDictionary(f => f.FieldName, StringComparer.OrdinalIgnoreCase);
            var dd = des.Where(f => !string.IsNullOrWhiteSpace(f?.FieldName)).ToDictionary(f => f.FieldName, StringComparer.OrdinalIgnoreCase);

            foreach (var kv in dd)
                if (!cd.ContainsKey(kv.Key))
                    ops.Add(new SchemaOp { Kind = SchemaOpKind.Add, FieldName = kv.Key, Field = kv.Value, Description = $"Add {kv.Key}" });

            foreach (var kv in dd)
            {
                if (!cd.TryGetValue(kv.Key, out var old) || FieldsEqual(old, kv.Value)) continue;
                ops.Add(new SchemaOp { Kind = SchemaOpKind.Alter, FieldName = kv.Key, Field = kv.Value, Description = $"Alter {kv.Key}" });
            }

            foreach (var kv in cd)
                if (!dd.ContainsKey(kv.Key))
                    ops.Add(new SchemaOp { Kind = SchemaOpKind.Drop, FieldName = kv.Key, Field = kv.Value, Description = $"Drop {kv.Key} (data loss)" });

            return ops;
        }

        private static bool FieldsEqual(EntityField a, EntityField b) =>
            a != null && b != null && string.Equals(a.Fieldtype, b.Fieldtype, StringComparison.OrdinalIgnoreCase) && a.Size1 == b.Size1;

        private static string BuildPreviewMessage(List<SchemaOp> ops)
        {
            var sb = new StringBuilder(); sb.AppendLine($"Apply {ops.Count} change(s)?"); sb.AppendLine();
            foreach (var s in ops.Take(8)) sb.AppendLine($"- {s.Description}");
            if (ops.Count > 8) sb.AppendLine($"… +{ops.Count - 8} more");
            return sb.ToString();
        }

        private sealed class SchemaOp
        {
            public SchemaOpKind Kind { get; set; }
            public string FieldName { get; set; } = "";
            public EntityField? Field { get; set; }
            public string Description { get; set; } = "";
        }

        // ── Logging + disclosure ───────────────────────────────────────────

        /// <summary>
        /// Asks an in-flight apply to stop at its next operation boundary. It cannot interrupt a
        /// DDL statement already issued — the engine has no async or cancellable variant — so a
        /// half-applied update stays half-applied; the log records what ran.
        /// </summary>
        protected override void OnHandleDestroyed(EventArgs e)
        {
            _applyCts?.Cancel();
            base.OnHandleDestroyed(e);
        }

        private void LogStatus(string msg, Errors flag)
        {
            _lastSummary = msg; beepService?.DMEEditor?.AddLogMessage("EntityEditor", _lastSummary, DateTime.Now, 0, _viewModel?.EntityName, flag);
            _stateLabel.Text = $"Status: {_lastSummary}";
        }

        private void RefreshProgressiveDisclosure(string? entityName)
        {
            bool hasDs = _viewModel?.SourceConnection != null, hasEnt = !string.IsNullOrWhiteSpace(entityName), isExisting = hasEnt && _mode == EntityEditorMode.UpdateExisting;
            // The selection combos are frozen while a schema operation runs. They are otherwise
            // fully live across the DDL awaits, and switching datasource mid-apply would repoint
            // SourceConnection at a different database while the operations are still executing.
            DatasourcebeepComboBox.Enabled = !_isApplyingSchema;
            EntitiesbeepComboBox.Enabled = hasDs && !_isApplyingSchema;
            EntityFieldsbeepGridPro.Enabled = hasDs && hasEnt && !_isApplyingSchema;
            ApplybeepButton.Enabled = hasDs && ApplybeepButton.Enabled && !_isApplyingSchema;
            _btnEditData.Visible = isExisting;
            _btnDefaults.Visible = isExisting;
            _btnMapEntity.Visible = isExisting;
            // Carries _lastSummary through, mirroring RefreshEditorModeState. This runs at the end of
            // every SyncBindings — including the one in ApplyAsync's finally — so writing a bare
            // prompt here discarded the outcome of the operation that just ran ("Created X…",
            // "No schema changes.", "Could not determine whether X exists…") before it could be read.
            string prompt = !hasDs ? "Select datasource."
                : !hasEnt ? "Select or type entity name."
                : _mode == EntityEditorMode.CreateNew ? $"Review fields, then create '{entityName}'."
                : $"Review diff, then update '{entityName}'.";
            _stateLabel.Text = string.IsNullOrWhiteSpace(_lastSummary) || _lastSummary == "Idle"
                ? prompt
                : $"{prompt} | {_lastSummary}";
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
