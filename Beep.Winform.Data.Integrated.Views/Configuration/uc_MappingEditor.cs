using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor.Mapping;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.DialogsManagers;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Default.Views.Template;
using TheTechIdea.Beep.Workflow.Mapping;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    [AddinAttribute(Caption = "Mapping Editor", Name = "uc_MappingEditor",
        misc = "Config", menu = "Configuration", addinType = AddinType.Control,
        displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 14, RootNodeName = "Configuration", Order = 14, ID = 14,
        BranchText = "Mapping Editor", BranchType = EnumPointType.Function,
        IconImageName = "mapping.svg", BranchClass = "ADDIN",
        BranchDescription = "Auto-match and transform entity field mappings.")]

    public partial class uc_MappingEditor : TemplateUserControl, IAddinVisSchema
    {
        public event EventHandler<WizardCompletedEventArgs>? Completed;

        private readonly BindingList<Mapping_rep_fields> _fields = new();
        private EntityDataMap? _map;
        private EntityDataMap_DTL? _detail;

        /// <summary>
        /// Serialises the engine operations. Validate was an async void handler with no gate, so
        /// repeated clicks stacked concurrent ValidateMappingWithScore runs over the same map.
        /// </summary>
        private bool _busy;

        /// <summary>
        /// Bumped whenever a connection selection changes, so an entity list that resolves after the
        /// user has moved on cannot repopulate the combo for the wrong connection.
        /// </summary>
        private int _entityLoadGeneration;

        /// <summary>
        /// Designer/parameterless ctor. Must not chain to the IServiceProvider overload with null —
        /// that resolves services off a null provider and throws.
        /// </summary>
        public uc_MappingEditor() => InitializeControl();

        public uc_MappingEditor(IServiceProvider services) : base(services) => InitializeControl();

        private void InitializeControl()
        {
            InitializeComponent();
            Details.AddinName = "Mapping Editor";
            WireEvents();
            PopulateConnections();
            _gridFields.DataSource = _fields;
            UpdateButtons();
        }

        #region "IAddinVisSchema"
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string RootNodeName { get; set; } = "Configuration";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CatgoryName { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Order { get; set; } = 14;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ID { get; set; } = 14;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchText { get; set; } = "Mapping Editor";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Level { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BranchID { get; set; } = 14;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconImageName { get; set; } = "mapping.svg";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchStatus { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ParentBranchID { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchDescription { get; set; } = "Auto-match and transform entity field mappings.";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchClass { get; set; } = "ADDIN";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string AddinName { get; set; } = "uc_MappingEditor";
        #endregion

        /// <summary>The mapping currently being edited, or null before Create/Load.</summary>
        public EntityDataMap? Map => _map;

        private void WireEvents()
        {
            _btnCreate.Click += BtnCreate_Click;
            _btnLoad.Click += BtnLoad_Click;
            _btnValidate.Click += BtnValidate_Click;
            _btnSave.Click += BtnSave_Click;
            // Fire-and-forget is safe only because LoadEntities catches its own failures.
            _cboSourceConn.SelectedItemChanged += (_, _) => _ = LoadEntities(_cboSourceConn, _cboSourceEntity);
            _cboDestConn.SelectedItemChanged += (_, _) => _ = LoadEntities(_cboDestConn, _cboDestEntity);
            _cboSourceEntity.SelectedItemChanged += (_, _) => UpdateButtons();
            _cboDestEntity.SelectedItemChanged += (_, _) => UpdateButtons();
        }

        protected override void ApplyDpiScaledLayout()
        {
            _rootPanel.Padding = BeepLayoutMetrics.DialogPadding.ScalePadding(this);
            _contentHost.Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this);
            _selectorPanel.Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this);
            _toolbarPanel.Padding = BeepLayoutMetrics.ButtonStripPd.ScalePadding(this);
            _actionsPanel.Padding = BeepLayoutMetrics.ButtonStripPd.ScalePadding(this);

            int btnH = BeepLayoutMetrics.ButtonStandard.Height.ScaleValue(this);
            _btnSave.MinimumSize = new System.Drawing.Size(
                BeepLayoutMetrics.ButtonLarge.Width.ScaleValue(this),
                BeepLayoutMetrics.ButtonLarge.Height.ScaleValue(this));
            foreach (var b in new[] { _btnCreate, _btnLoad, _btnValidate })
                b.MinimumSize = new System.Drawing.Size(
                    BeepLayoutMetrics.ButtonToolbar.Width.ScaleValue(this), btnH);
        }

        private void PopulateConnections()
        {
            var connections = beepService?.DMEEditor?.ConfigEditor?.DataConnections;
            if (connections == null || connections.Count == 0)
            {
                SetStatus("No connections are configured.");
                return;
            }

            foreach (var conn in connections.Where(c => !string.IsNullOrWhiteSpace(c?.ConnectionName)))
            {
                var text = $"{conn.ConnectionName} ({conn.DatabaseType})";
                _cboSourceConn.ListItems.Add(new SimpleItem { Text = text, Value = conn.ConnectionName });
                _cboDestConn.ListItems.Add(new SimpleItem { Text = text, Value = conn.ConnectionName });
            }
        }

        /// <summary>Fills an entity picker from the datasource's own entity list, off the UI thread.</summary>
        private async Task LoadEntities(BeepComboBox connectionCombo, BeepComboBox entityCombo)
        {
            entityCombo.ListItems.Clear();

            var editor = beepService?.DMEEditor;
            var name = connectionCombo.SelectedItem?.Value as string;
            if (editor == null || string.IsNullOrWhiteSpace(name)) { UpdateButtons(); return; }

            int generation = ++_entityLoadGeneration;
            try
            {
                // GetDataSource resolves a driver and GetEntitesList is a metadata round-trip; both
                // blocked the UI thread here.
                var entities = await Task.Run(() => editor.GetDataSource(name)?.GetEntitesList())
                    .ConfigureAwait(true);

                if (generation != _entityLoadGeneration || IsDisposed) return;

                if (entities != null)
                {
                    foreach (var entity in entities.Where(e => !string.IsNullOrWhiteSpace(e)).OrderBy(e => e))
                        entityCombo.ListItems.Add(new SimpleItem { Text = entity, Value = entity });
                }
            }
            catch (Exception ex)
            {
                if (generation != _entityLoadGeneration || IsDisposed) return;
                SetStatus($"Could not list entities for '{name}': {ex.Message}");
            }
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            // Everything is gated on !_busy as well as its own precondition: the engine calls are
            // now awaited, so without this the buttons stay live across the await and a second click
            // would only be caught by the _busy check after already looking clickable.
            bool scoped = SourceConn != null && SourceEntity != null && DestConn != null && DestEntity != null;
            _btnCreate.Enabled = scoped && !_busy;
            _btnLoad.Enabled = DestConn != null && DestEntity != null && !_busy;
            _btnValidate.Enabled = _map != null && !_busy;
            _btnSave.Enabled = _map != null && !_busy;
        }

        private string? SourceConn => _cboSourceConn.SelectedItem?.Value as string;
        private string? SourceEntity => _cboSourceEntity.SelectedItem?.Value as string;
        private string? DestConn => _cboDestConn.SelectedItem?.Value as string;
        private string? DestEntity => _cboDestEntity.SelectedItem?.Value as string;

        /// <summary>
        /// Builds the mapping through the engine, which loads or initializes the destination
        /// map and attaches the source entity as a mapped detail.
        /// </summary>
        private void BtnCreate_Click(object? sender, EventArgs e) => _ = CreateAsync();

        private async Task CreateAsync()
        {
            var editor = beepService?.DMEEditor;
            if (editor == null || SourceConn == null || SourceEntity == null
                || DestConn == null || DestEntity == null) return;
            if (_busy) { SetStatus("An operation is already running."); return; }

            string srcEntity = SourceEntity, srcConn = SourceConn, dstEntity = DestEntity, dstConn = DestConn;
            _busy = true;
            UpdateButtons();
            try
            {
                // CreateEntityMap reads both entity structures from their datasources.
                var (status, map) = await Task.Run(() => MappingManager.CreateEntityMap(
                    editor, srcEntity, srcConn, dstEntity, dstConn)).ConfigureAwait(true);

                if (IsDisposed) return;

                if (status?.Flag != Errors.Ok || map == null)
                {
                    SetStatus($"Create map failed: {status?.Message}");
                    BeepDialogManager.Instance.ShowError("Create Map Failed", status?.Message ?? "Unknown error.");
                    return;
                }

                _map = map;
                BindDetail(map, srcEntity);
                SetStatus($"Mapped '{srcEntity}' → '{dstEntity}': {_fields.Count} field mapping(s).");
            }
            catch (Exception ex)
            {
                SetStatus($"Create map threw: {ex.Message}");
            }
            finally
            {
                _busy = false;
                UpdateButtons();
            }
        }

        private void BtnLoad_Click(object? sender, EventArgs e) => _ = LoadAsync();

        private async Task LoadAsync()
        {
            var config = beepService?.DMEEditor?.ConfigEditor;
            if (config == null || DestConn == null || DestEntity == null) return;
            if (_busy) { SetStatus("An operation is already running."); return; }

            string dstEntity = DestEntity, dstConn = DestConn;
            _busy = true;
            UpdateButtons();
            try
            {
                var map = await Task.Run(() => config.LoadMappingValues(dstEntity, dstConn)).ConfigureAwait(true);
                if (IsDisposed) return;

                if (map == null)
                {
                    SetStatus($"No saved mapping for '{dstEntity}' on '{dstConn}'.");
                    return;
                }

                _map = map;
                BindDetail(map, SourceEntity);
                SetStatus($"Loaded mapping '{map.MappingName}' with {_fields.Count} field mapping(s).");
            }
            catch (Exception ex)
            {
                SetStatus($"Load failed: {ex.Message}");
            }
            finally
            {
                _busy = false;
                UpdateButtons();
            }
        }

        /// <summary>
        /// Field mappings live on the mapped-entity detail, so pick the detail matching
        /// <paramref name="sourceEntity"/> (falling back to the first) and bind its FieldMapping.
        /// </summary>
        /// <remarks>
        /// Takes the entity name rather than reading the SourceEntity combo. The combos stay live
        /// across the Create/Load awaits, so reading the property here could match the detail
        /// against a source the user picked *after* the map was built — silently binding, and later
        /// saving, a different detail's field mappings.
        /// </remarks>
        private void BindDetail(EntityDataMap map, string? sourceEntity)
        {
            _detail = map.MappedEntities?.FirstOrDefault(d =>
                          string.Equals(d.EntityName, sourceEntity, StringComparison.OrdinalIgnoreCase))
                      ?? map.MappedEntities?.FirstOrDefault();

            _fields.Clear();
            if (_detail?.FieldMapping == null) return;

            foreach (var f in _detail.FieldMapping) _fields.Add(f);
        }

        private void BtnValidate_Click(object? sender, EventArgs e) => _ = ValidateAsync();

        private async Task ValidateAsync()
        {
            var editor = beepService?.DMEEditor;
            if (editor == null || _map == null) return;
            if (_busy) { SetStatus("An operation is already running."); return; }

            _busy = true;
            UpdateButtons();
            try
            {
                SyncFieldsBack();

                var map = _map;
                var report = await Task.Run(() =>
                    MappingManager.ValidateMappingWithScore(editor, map)).ConfigureAwait(true);

                // ValidateMappingWithScore reads entity structures, so this can outlive the view.
                // Without the guard a modal quality dialog pops for a control that is already gone.
                if (IsDisposed) return;

                var summary = $"Score {report.Score}/100 ({report.Band}); " +
                              $"production threshold {report.ProductionThreshold}: " +
                              $"{(report.MeetsProductionThreshold ? "met" : "not met")}; " +
                              $"{report.Issues.Count} issue(s).";
                SetStatus(summary);

                if (report.Issues.Count > 0)
                {
                    // Projected field by field. MappingQualityIssue has no ToString override, so
                    // i.ToString() printed the type name once per issue — the engine computed a
                    // Code, Severity, Category, Message, Recommendation and FieldName for each and
                    // every one of them was discarded.
                    var lines = report.Issues
                        .OrderByDescending(i => i.Severity)
                        .Select(i =>
                            $"[{i.Severity}] {i.Code}" +
                            (string.IsNullOrWhiteSpace(i.FieldName) ? "" : $" ({i.FieldName})") +
                            $" — {i.Message}" +
                            (string.IsNullOrWhiteSpace(i.Recommendation) ? "" : $" → {i.Recommendation}"))
                        .ToList();

                    BeepDialogManager.Instance.ShowWarning("Mapping Quality",
                        summary + Environment.NewLine + Environment.NewLine +
                        string.Join(Environment.NewLine, lines));
                }
                else
                {
                    BeepDialogManager.Instance.ShowInfo("Mapping Quality", summary);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Validation failed: {ex.Message}");
            }
            finally
            {
                _busy = false;
                UpdateButtons();
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e) => _ = SaveAsync();

        /// <summary>
        /// Persists the mapping through <c>MappingManager.SaveEntityMap</c>.
        /// </summary>
        /// <remarks>
        /// Not <c>ConfigEditor.SaveMappingValues</c>, which this used to call directly. That writes
        /// the JSON and nothing else, so an edited mapping left the compiled-plan and
        /// destination-setter caches holding the pre-edit plan, and skipped the governance/version
        /// hooks entirely. SaveEntityMap exists for exactly this — its own summary is "Persists a
        /// mapping through MappingManager so cache invalidation and governance hooks are applied" —
        /// and it calls SaveMappingValues internally, so nothing is lost by going through it.
        /// </remarks>
        private async Task SaveAsync()
        {
            var editor = beepService?.DMEEditor;
            if (editor == null || _map == null || DestConn == null || DestEntity == null) return;
            if (_busy) { SetStatus("An operation is already running."); return; }

            _busy = true;
            try
            {
                SyncFieldsBack();
                var map = _map;
                string destEntity = DestEntity, destConn = DestConn;

                // Writes DataConnections/mapping JSON and touches the caches — off the UI thread.
                await Task.Run(() => MappingManager.SaveEntityMap(editor, destEntity, destConn, map))
                    .ConfigureAwait(true);

                if (IsDisposed) return;
                SetStatus($"Saved mapping for '{destEntity}' on '{destConn}'.");
                Completed?.Invoke(this, new WizardCompletedEventArgs
                {
                    Succeeded = true,
                    Summary = _lblStatus.Text
                });
            }
            catch (Exception ex)
            {
                SetStatus($"Save failed: {ex.Message}");
                BeepDialogManager.Instance.ShowError("Save Failed", ex.Message);
            }
            finally
            {
                _busy = false;
                UpdateButtons();
            }
        }

        /// <summary>
        /// Pushes grid edits back onto the mapped-entity detail the map will persist.
        /// Field mappings hang off the detail, not off EntityDataMap itself.
        /// </summary>
        private void SyncFieldsBack()
        {
            if (_map == null || _detail?.FieldMapping == null) return;

            _detail.FieldMapping.Clear();
            _detail.FieldMapping.AddRange(_fields);
        }

        private void SetStatus(string message) => _lblStatus.Text = message;

    }
}
