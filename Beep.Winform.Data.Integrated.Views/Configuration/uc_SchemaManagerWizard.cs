using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor.Schema;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    [AddinAttribute(Caption = "Schema Manager", Name = "uc_SchemaManagerWizard",
        misc = "Config", menu = "Configuration", addinType = AddinType.Control,
        displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 10, RootNodeName = "Configuration", Order = 10, ID = 10,
        BranchText = "Schema Manager", BranchType = EnumPointType.Function,
        IconImageName = "schema.svg", BranchClass = "ADDIN",
        BranchDescription = "Plan, dry-run, and apply schema migrations.")]

    public partial class uc_SchemaManagerWizard : TemplateUserControl, IAddinVisSchema
    {
        /// <summary>Scope selection, then preflight/draft results.</summary>
        private enum Stage { Scope = 0, Results = 1 }

        public event EventHandler<WizardCompletedEventArgs>? Completed;

        private Stage _stage = Stage.Scope;
        private bool _busy;
        private ISchemaManager? _schema;
        private SchemaPreflightResult? _preflight;
        private CancellationTokenSource? _cts;

        public uc_SchemaManagerWizard() : this(null) { }

        public uc_SchemaManagerWizard(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            Details.AddinName = "Schema Manager";
            WireEvents();
            ApplyDpiScaledLayout();
            PopulateConnections();
            UpdateStageUi();
        }

        #region "IAddinVisSchema"
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string RootNodeName { get; set; } = "Configuration";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CatgoryName { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Order { get; set; } = 10;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ID { get; set; } = 10;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchText { get; set; } = "Schema Manager";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Level { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BranchID { get; set; } = 10;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconImageName { get; set; } = "schema.svg";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchStatus { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ParentBranchID { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchDescription { get; set; } = "Plan, dry-run, and apply schema migrations.";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchClass { get; set; } = "ADDIN";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string AddinName { get; set; } = "uc_SchemaManagerWizard";
        #endregion

        /// <summary>Result of the last preflight, or null before Run Preflight.</summary>
        public SchemaPreflightResult? Preflight => _preflight;

        private void WireEvents()
        {
            _btnCancel.Click += (_, _) =>
            {
                _cts?.Cancel();
                Completed?.Invoke(this, new WizardCompletedEventArgs { Cancelled = true });
            };
            _btnBack.Click += (_, _) =>
            {
                if (_busy || _stage == Stage.Scope) return;
                _stage = Stage.Scope;
                UpdateStageUi();
            };
            _btnNext.Click += BtnNext_Click;
            _cboSourceConn.SelectedItemChanged += (_, _) =>
                LoadEntities(_cboSourceConn, _cboSourceEntity);
            _cboDestConn.SelectedItemChanged += (_, _) =>
                LoadEntities(_cboDestConn, _cboDestEntity);
        }

        private void ApplyDpiScaledLayout()
        {
            _rootPanel.Padding = BeepLayoutMetrics.DialogPadding.ScalePadding(this);
            _contentHost.Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this);
            _actionsPanel.Padding = BeepLayoutMetrics.ButtonStripPd.ScalePadding(this);

            int btnH = BeepLayoutMetrics.ButtonStandard.Height.ScaleValue(this);
            int btnLargeH = BeepLayoutMetrics.ButtonLarge.Height.ScaleValue(this);
            _btnCancel.MinimumSize = new System.Drawing.Size(
                BeepLayoutMetrics.ButtonStandard.Width.ScaleValue(this), btnH);
            _btnBack.MinimumSize = new System.Drawing.Size(
                BeepLayoutMetrics.ButtonStandard.Width.ScaleValue(this), btnH);
            _btnNext.MinimumSize = new System.Drawing.Size(
                BeepLayoutMetrics.ButtonLarge.Width.ScaleValue(this), btnLargeH);
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
                _cboSourceConn.ListItems.Add(new SimpleItem
                {
                    Text = $"{conn.ConnectionName} ({conn.DatabaseType})",
                    Value = conn.ConnectionName
                });
                _cboDestConn.ListItems.Add(new SimpleItem
                {
                    Text = $"{conn.ConnectionName} ({conn.DatabaseType})",
                    Value = conn.ConnectionName
                });
            }
        }

        /// <summary>
        /// Fills an entity picker from the datasource's own entity list, so the user picks a
        /// real entity rather than typing a name that may not exist.
        /// </summary>
        private void LoadEntities(BeepComboBox connectionCombo, BeepComboBox entityCombo)
        {
            entityCombo.ListItems.Clear();

            var editor = beepService?.DMEEditor;
            var name = connectionCombo.SelectedItem?.Value as string;
            if (editor == null || string.IsNullOrWhiteSpace(name)) return;

            try
            {
                var ds = editor.GetDataSource(name);
                var entities = ds?.GetEntitesList();
                if (entities == null) return;

                foreach (var entity in entities.Where(e => !string.IsNullOrWhiteSpace(e)).OrderBy(e => e))
                    entityCombo.ListItems.Add(new SimpleItem { Text = entity, Value = entity });
            }
            catch (Exception ex)
            {
                SetStatus($"Could not list entities for '{name}': {ex.Message}");
            }
        }

        private async void BtnNext_Click(object? sender, EventArgs e)
        {
            if (_busy) return;

            try
            {
                if (_stage == Stage.Scope)
                {
                    if (await RunPreflightAsync())
                    {
                        _stage = Stage.Results;
                        UpdateStageUi();
                    }
                }
                else
                {
                    await BuildDraftAsync();
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Schema manager error: {ex.Message}");
            }
        }

        private void UpdateStageUi()
        {
            _stepScope.Visible = _stage == Stage.Scope;
            _stepResults.Visible = _stage == Stage.Results;
            (_stage == Stage.Scope ? (Control)_stepScope : _stepResults).BringToFront();

            (_lblSubtitle.Text, _btnNext.Text) = _stage == Stage.Scope
                ? ("Step 1 of 2 — choose source and destination entities.", "Run Preflight")
                : ("Step 2 of 2 — preflight results; build a sync draft when ready.", "Build Sync Draft");

            _btnBack.Enabled = !_busy && _stage != Stage.Scope;
            _btnNext.Enabled = !_busy && CanAdvance();
        }

        private bool CanAdvance() => _stage switch
        {
            Stage.Scope => _cboSourceConn.SelectedItem != null
                           && _cboSourceEntity.SelectedItem != null
                           && _cboDestConn.SelectedItem != null
                           && _cboDestEntity.SelectedItem != null,
            // A draft is only meaningful once both sides resolved.
            _ => _preflight != null
                 && _preflight.SourceResolved
                 && _preflight.DestinationResolved,
        };

        private SchemaRequest BuildRequest() => new()
        {
            SourceDataSourceName = (string)_cboSourceConn.SelectedItem!.Value!,
            SourceEntityName = (string)_cboSourceEntity.SelectedItem!.Value!,
            DestinationDataSourceName = (string)_cboDestConn.SelectedItem!.Value!,
            DestinationEntityName = (string)_cboDestEntity.SelectedItem!.Value!,
            AddMissingColumns = _chkAddMissingColumns.Checked,
            CreateDestinationIfNotExists = _chkCreateDestination.Checked
        };

        private async Task<bool> RunPreflightAsync()
        {
            var editor = beepService?.DMEEditor;
            if (editor == null)
            {
                SetStatus("IDMEEditor is not available; cannot run preflight.");
                return false;
            }

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _schema ??= new SchemaManager(editor);
            var request = BuildRequest();
            var log = new List<string>();

            using var busy = BeginBusy("Running schema preflight…");
            try
            {
                _preflight = await _schema.RunPreflightAsync(request, log.Add, token).ConfigureAwait(true);
                BindResults(_preflight, log);

                bool ok = _preflight.Status?.Flag == Errors.Ok;
                _lblResultsSummary.Text =
                    $"Source: {(_preflight.SourceConnected ? "connected" : "not connected")}, " +
                    $"destination: {(_preflight.DestinationConnected ? "connected" : "not connected")}, " +
                    $"destination {(_preflight.DestinationExisted ? "exists" : "missing")}, " +
                    $"{_preflight.MissingDestinationFields.Count} missing field(s).";

                SetStatus(ok ? "Preflight completed." : $"Preflight reported: {_preflight.Status?.Message}");
                return true;
            }
            catch (OperationCanceledException)
            {
                SetStatus("Preflight cancelled.");
                return false;
            }
            catch (Exception ex)
            {
                SetStatus($"Preflight failed: {ex.Message}");
                return false;
            }
        }

        private void BindResults(SchemaPreflightResult result, IReadOnlyList<string> log)
        {
            _lstResults.ClearItems();
            var items = new List<SimpleItem>
            {
                new() { Text = $"[status] {result.Status?.Message}" },
                new() { Text = $"[source] resolved={result.SourceResolved}, connected={result.SourceConnected}, structure={result.SourceStructureLoaded}" },
                new() { Text = $"[destination] resolved={result.DestinationResolved}, connected={result.DestinationConnected}, structure={result.DestinationStructureLoaded}, existed={result.DestinationExisted}" }
            };

            foreach (var field in result.MissingDestinationFields)
                items.Add(new SimpleItem { Text = $"[missing field] {field}" });

            foreach (var line in log)
                items.Add(new SimpleItem { Text = $"[log] {line}" });

            _lstResults.AddItems(items);
            _lstResults.RefreshItems();
        }

        private async Task BuildDraftAsync()
        {
            var editor = beepService?.DMEEditor;
            if (editor == null || _schema == null) return;

            using var busy = BeginBusy("Building sync draft…");
            try
            {
                var draft = await _schema.BuildSyncDraftAsync(BuildRequest()).ConfigureAwait(true);
                bool ok = draft.Status?.Flag == Errors.Ok && draft.Draft != null;

                _lstResults.AddItem(new SimpleItem
                {
                    Text = ok
                        ? $"[draft] Built sync draft '{draft.Draft!.Id}'."
                        : $"[draft] Failed: {draft.Status?.Message}"
                });
                _lstResults.RefreshItems();

                SetStatus(ok ? "Sync draft built." : $"Draft failed: {draft.Status?.Message}");
                Completed?.Invoke(this, new WizardCompletedEventArgs
                {
                    Succeeded = ok,
                    Summary = _lblResultsSummary.Text
                });
            }
            catch (Exception ex)
            {
                SetStatus($"Draft failed: {ex.Message}");
            }
        }

        private void SetStatus(string message) => _lblStatus.Text = message;

        private IDisposable BeginBusy(string message)
        {
            _busy = true;
            SetStatus(message);
            _btnNext.Enabled = false;
            _btnBack.Enabled = false;
            Cursor = Cursors.WaitCursor;
            return new BusyScope(this);
        }

        private sealed class BusyScope : IDisposable
        {
            private readonly uc_SchemaManagerWizard _owner;
            public BusyScope(uc_SchemaManagerWizard owner) => _owner = owner;

            public void Dispose()
            {
                _owner._busy = false;
                _owner.Cursor = Cursors.Default;
                _owner.UpdateStageUi();
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            _cts?.Cancel();
            base.OnHandleDestroyed(e);
        }

        public sealed class WizardCompletedEventArgs : EventArgs
        {
            public bool Succeeded { get; init; }
            public bool Cancelled { get; init; }
            public string Summary { get; init; } = string.Empty;
        }
    }
}
