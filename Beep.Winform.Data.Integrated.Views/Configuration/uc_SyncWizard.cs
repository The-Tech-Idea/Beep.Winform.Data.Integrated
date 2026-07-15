using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.BeepSync;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    [AddinAttribute(Caption = "Sync Wizard", Name = "uc_SyncWizard",
        misc = "Config", menu = "Configuration", addinType = AddinType.Control,
        displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 11, RootNodeName = "Configuration", Order = 11, ID = 11,
        BranchText = "Sync Wizard", BranchType = EnumPointType.Function,
        IconImageName = "sync.svg", BranchClass = "ADDIN",
        BranchDescription = "Configure, validate, and run datasource synchronisation.")]

    public partial class uc_SyncWizard : TemplateUserControl, IAddinVisSchema
    {
        private enum Stage { Scope = 0, Run = 1 }

        public event EventHandler<WizardCompletedEventArgs>? Completed;

        private Stage _stage = Stage.Scope;
        private bool _busy;
        private CancellationTokenSource? _cts;

        public uc_SyncWizard() : this(null) { }
        public uc_SyncWizard(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            Details.AddinName = "Sync Wizard";
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
        public int Order { get; set; } = 11;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ID { get; set; } = 11;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchText { get; set; } = "Sync Wizard";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Level { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BranchID { get; set; } = 11;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconImageName { get; set; } = "sync.svg";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchStatus { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ParentBranchID { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchDescription { get; set; } = "Configure, validate, and run datasource synchronisation.";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchClass { get; set; } = "ADDIN";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string AddinName { get; set; } = "uc_SyncWizard";
        #endregion

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
            _cboSourceConn.SelectedItemChanged += (_, _) => LoadEntities(_cboSourceConn, _cboSourceEntity);
            _cboDestConn.SelectedItemChanged += (_, _) => LoadEntities(_cboDestConn, _cboDestEntity);
            _cboSourceEntity.SelectedItemChanged += (_, _) => UpdateStageUi();
            _cboDestEntity.SelectedItemChanged += (_, _) => UpdateStageUi();
        }

        private void ApplyDpiScaledLayout()
        {
            _rootPanel.Padding = BeepLayoutMetrics.DialogPadding.ScalePadding(this);
            _contentHost.Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this);
            _actionsPanel.Padding = BeepLayoutMetrics.ButtonStripPd.ScalePadding(this);

            int btnH = BeepLayoutMetrics.ButtonStandard.Height.ScaleValue(this);
            _btnCancel.MinimumSize = new System.Drawing.Size(
                BeepLayoutMetrics.ButtonStandard.Width.ScaleValue(this), btnH);
            _btnBack.MinimumSize = new System.Drawing.Size(
                BeepLayoutMetrics.ButtonStandard.Width.ScaleValue(this), btnH);
            _btnNext.MinimumSize = new System.Drawing.Size(
                BeepLayoutMetrics.ButtonLarge.Width.ScaleValue(this),
                BeepLayoutMetrics.ButtonLarge.Height.ScaleValue(this));
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

        private void LoadEntities(BeepComboBox connectionCombo, BeepComboBox entityCombo)
        {
            entityCombo.ListItems.Clear();

            var editor = beepService?.DMEEditor;
            var name = connectionCombo.SelectedItem?.Value as string;
            if (editor == null || string.IsNullOrWhiteSpace(name)) { UpdateStageUi(); return; }

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
            UpdateStageUi();
        }

        private string? SourceConn => _cboSourceConn.SelectedItem?.Value as string;
        private string? SourceEntity => _cboSourceEntity.SelectedItem?.Value as string;
        private string? DestConn => _cboDestConn.SelectedItem?.Value as string;
        private string? DestEntity => _cboDestEntity.SelectedItem?.Value as string;

        private async void BtnNext_Click(object? sender, EventArgs e)
        {
            if (_busy) return;

            try
            {
                if (_stage == Stage.Scope)
                {
                    _stage = Stage.Run;
                    UpdateStageUi();
                    await RunSyncAsync();
                }
                else
                {
                    Completed?.Invoke(this, new WizardCompletedEventArgs
                    {
                        Succeeded = true,
                        Summary = _lblRunStatus.Text
                    });
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Sync wizard error: {ex.Message}");
            }
        }

        private void UpdateStageUi()
        {
            _stepScope.Visible = _stage == Stage.Scope;
            _stepRun.Visible = _stage == Stage.Run;
            (_stage == Stage.Scope ? (Control)_stepScope : _stepRun).BringToFront();

            (_lblSubtitle.Text, _btnNext.Text) = _stage == Stage.Scope
                ? ("Step 1 of 2 — choose source, destination, and options.", "Run Sync")
                : ("Step 2 of 2 — sync progress and result.", "Finish");

            _btnBack.Enabled = !_busy && _stage == Stage.Run;
            _btnNext.Enabled = !_busy && (_stage == Stage.Run
                || (SourceConn != null && SourceEntity != null && DestConn != null && DestEntity != null));
        }

        private DataSyncSchema BuildSchema() => new()
        {
            SourceDataSourceName = SourceConn!,
            SourceEntityName = SourceEntity!,
            DestinationDataSourceName = DestConn!,
            DestinationEntityName = DestEntity!,
            EntityName = SourceEntity!,
            BatchSize = int.TryParse(_txtBatchSize.Text, out var b) && b > 0 ? b : 50,
            CreateDestinationIfNotExists = _chkCreateDestination.Checked,
            AddMissingColumns = _chkAddMissingColumns.Checked,
            RunPreflight = _chkPreflight.Checked
        };

        private async Task RunSyncAsync()
        {
            var editor = beepService?.DMEEditor;
            if (editor == null)
            {
                SetStatus("IDMEEditor is not available; cannot sync.");
                return;
            }

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _lstRunLog.ClearItems();
            _progress.Value = 0;

            using var busy = BeginBusy("Running sync…");
            try
            {
                var manager = new BeepSyncManager(editor);
                var schema = BuildSchema();

                var progress = new Progress<PassedArgs>(args =>
                {
                    if (args.ParameterInt1 > 0)
                        _progress.Value = Math.Clamp(args.ParameterInt1, 0, 100);
                    AppendLog(args.Messege ?? string.Empty);
                    if (!string.IsNullOrWhiteSpace(args.Messege))
                        _lblRunStatus.Text = args.Messege;
                });

                var result = await manager.SyncDataAsync(schema, token, progress).ConfigureAwait(true);
                bool ok = result?.Flag == Errors.Ok;

                _progress.Value = ok ? 100 : _progress.Value;
                _lblRunStatus.Text = ok ? "Sync completed." : $"Sync failed: {result?.Message}";
                AppendLog(_lblRunStatus.Text);
                SetStatus(_lblRunStatus.Text);

                Completed?.Invoke(this, new WizardCompletedEventArgs
                {
                    Succeeded = ok,
                    Summary = _lblRunStatus.Text
                });
            }
            catch (OperationCanceledException)
            {
                _lblRunStatus.Text = "Sync cancelled.";
                AppendLog(_lblRunStatus.Text);
                SetStatus(_lblRunStatus.Text);
            }
            catch (Exception ex)
            {
                _lblRunStatus.Text = $"Sync crashed: {ex.Message}";
                AppendLog(_lblRunStatus.Text);
                SetStatus(_lblRunStatus.Text);
            }
        }

        private void AppendLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            _lstRunLog.AddItem(new SimpleItem { Text = message });
            _lstRunLog.RefreshItems();
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
            private readonly uc_SyncWizard _owner;
            public BusyScope(uc_SyncWizard owner) => _owner = owner;

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
