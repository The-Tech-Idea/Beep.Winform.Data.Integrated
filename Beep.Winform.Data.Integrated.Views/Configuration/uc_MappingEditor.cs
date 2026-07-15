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

        public uc_MappingEditor() : this(null) { }
        public uc_MappingEditor(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            Details.AddinName = "Mapping Editor";
            WireEvents();
            ApplyDpiScaledLayout();
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
            _cboSourceConn.SelectedItemChanged += (_, _) => LoadEntities(_cboSourceConn, _cboSourceEntity);
            _cboDestConn.SelectedItemChanged += (_, _) => LoadEntities(_cboDestConn, _cboDestEntity);
            _cboSourceEntity.SelectedItemChanged += (_, _) => UpdateButtons();
            _cboDestEntity.SelectedItemChanged += (_, _) => UpdateButtons();
        }

        private void ApplyDpiScaledLayout()
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

        /// <summary>Fills an entity picker from the datasource's own entity list.</summary>
        private void LoadEntities(BeepComboBox connectionCombo, BeepComboBox entityCombo)
        {
            entityCombo.ListItems.Clear();

            var editor = beepService?.DMEEditor;
            var name = connectionCombo.SelectedItem?.Value as string;
            if (editor == null || string.IsNullOrWhiteSpace(name)) { UpdateButtons(); return; }

            try
            {
                var entities = editor.GetDataSource(name)?.GetEntitesList();
                if (entities != null)
                {
                    foreach (var entity in entities.Where(e => !string.IsNullOrWhiteSpace(e)).OrderBy(e => e))
                        entityCombo.ListItems.Add(new SimpleItem { Text = entity, Value = entity });
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Could not list entities for '{name}': {ex.Message}");
            }
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            bool scoped = SourceConn != null && SourceEntity != null && DestConn != null && DestEntity != null;
            _btnCreate.Enabled = scoped;
            _btnLoad.Enabled = DestConn != null && DestEntity != null;
            _btnValidate.Enabled = _map != null;
            _btnSave.Enabled = _map != null;
        }

        private string? SourceConn => _cboSourceConn.SelectedItem?.Value as string;
        private string? SourceEntity => _cboSourceEntity.SelectedItem?.Value as string;
        private string? DestConn => _cboDestConn.SelectedItem?.Value as string;
        private string? DestEntity => _cboDestEntity.SelectedItem?.Value as string;

        /// <summary>
        /// Builds the mapping through the engine, which loads or initializes the destination
        /// map and attaches the source entity as a mapped detail.
        /// </summary>
        private void BtnCreate_Click(object? sender, EventArgs e)
        {
            var editor = beepService?.DMEEditor;
            if (editor == null || SourceConn == null || SourceEntity == null
                || DestConn == null || DestEntity == null) return;

            try
            {
                var (status, map) = MappingManager.CreateEntityMap(
                    editor, SourceEntity, SourceConn, DestEntity, DestConn);

                if (status?.Flag != Errors.Ok || map == null)
                {
                    SetStatus($"Create map failed: {status?.Message}");
                    BeepDialogManager.Instance.ShowError("Create Map Failed", status?.Message ?? "Unknown error.");
                    return;
                }

                _map = map;
                BindDetail(map);
                SetStatus($"Mapped '{SourceEntity}' → '{DestEntity}': {_fields.Count} field mapping(s).");
            }
            catch (Exception ex)
            {
                SetStatus($"Create map threw: {ex.Message}");
            }
            UpdateButtons();
        }

        private void BtnLoad_Click(object? sender, EventArgs e)
        {
            var config = beepService?.DMEEditor?.ConfigEditor;
            if (config == null || DestConn == null || DestEntity == null) return;

            try
            {
                var map = config.LoadMappingValues(DestEntity, DestConn);
                if (map == null)
                {
                    SetStatus($"No saved mapping for '{DestEntity}' on '{DestConn}'.");
                    return;
                }

                _map = map;
                BindDetail(map);
                SetStatus($"Loaded mapping '{map.MappingName}' with {_fields.Count} field mapping(s).");
            }
            catch (Exception ex)
            {
                SetStatus($"Load failed: {ex.Message}");
            }
            UpdateButtons();
        }

        /// <summary>
        /// Field mappings live on the mapped-entity detail, so pick the detail matching the
        /// chosen source (falling back to the first) and bind its FieldMapping to the grid.
        /// </summary>
        private void BindDetail(EntityDataMap map)
        {
            _detail = map.MappedEntities?.FirstOrDefault(d =>
                          string.Equals(d.EntityName, SourceEntity, StringComparison.OrdinalIgnoreCase))
                      ?? map.MappedEntities?.FirstOrDefault();

            _fields.Clear();
            if (_detail?.FieldMapping == null) return;

            foreach (var f in _detail.FieldMapping) _fields.Add(f);
        }

        private async void BtnValidate_Click(object? sender, EventArgs e)
        {
            var editor = beepService?.DMEEditor;
            if (editor == null || _map == null) return;

            try
            {
                SyncFieldsBack();

                var map = _map;
                var report = await Task.Run(() =>
                    MappingManager.ValidateMappingWithScore(editor, map)).ConfigureAwait(true);

                var summary = $"Score {report.Score}/100 ({report.Band}); " +
                              $"production threshold {report.ProductionThreshold}: " +
                              $"{(report.MeetsProductionThreshold ? "met" : "not met")}; " +
                              $"{report.Issues.Count} issue(s).";
                SetStatus(summary);

                if (report.Issues.Count > 0)
                {
                    var lines = report.Issues.Select(i => i.ToString()).ToList();
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
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var config = beepService?.DMEEditor?.ConfigEditor;
            if (config == null || _map == null || DestConn == null || DestEntity == null) return;

            try
            {
                SyncFieldsBack();
                config.SaveMappingValues(DestEntity, DestConn, _map);
                SetStatus($"Saved mapping for '{DestEntity}' on '{DestConn}'.");
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

        public sealed class WizardCompletedEventArgs : EventArgs
        {
            public bool Succeeded { get; init; }
            public bool Cancelled { get; init; }
            public string Summary { get; init; } = string.Empty;
        }
    }
}
