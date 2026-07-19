using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor.Defaults;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.DialogsManagers;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    [AddinAttribute(Caption = "Defaults Editor", Name = "uc_DefaultsEditor",
        misc = "Config", menu = "Configuration", addinType = AddinType.Control,
        displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 13, RootNodeName = "Configuration", Order = 13, ID = 13,
        BranchText = "Defaults Editor", BranchType = EnumPointType.Function,
        IconImageName = "defaults.svg", BranchClass = "ADDIN",
        BranchDescription = "Edit field-level default-value rules with dry-run preview.")]

    public partial class uc_DefaultsEditor : TemplateUserControl, IAddinVisSchema
    {
        public event EventHandler<WizardCompletedEventArgs>? Completed;

        private readonly BindingList<DefaultValue> _defaults = new();
        private string? _currentConnection;

        /// <summary>
        /// Serialises the engine operations so a second click cannot start a concurrent run while
        /// one is awaiting.
        /// </summary>
        private bool _busy;

        /// <summary>
        /// Cancels an in-flight rule test. A rule is resolved by a plugin pipeline whose vocabulary
        /// includes WEBSERVICE:, API:, HTTP: and LOOKUP: — so testing one can make a network or
        /// database call, and the engine offers neither an async variant nor a timeout.
        /// (DefaultValue.RuleOptions documents a `timeout=` setting, but nothing reads it.)
        /// </summary>
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Designer/parameterless ctor. Must not chain to the IServiceProvider overload with null —
        /// that resolves services off a null provider and throws.
        /// </summary>
        public uc_DefaultsEditor() => InitializeControl();

        public uc_DefaultsEditor(IServiceProvider services) : base(services) => InitializeControl();

        private void InitializeControl()
        {
            InitializeComponent();
            Details.AddinName = "Defaults Editor";
            WireEvents();
            PopulateConnections();
            _gridDefaults.DataSource = _defaults;
        }

        #region "IAddinVisSchema"
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string RootNodeName { get; set; } = "Configuration";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CatgoryName { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Order { get; set; } = 13;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ID { get; set; } = 13;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchText { get; set; } = "Defaults Editor";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Level { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BranchID { get; set; } = 13;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconImageName { get; set; } = "defaults.svg";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchStatus { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ParentBranchID { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchDescription { get; set; } = "Edit field-level default-value rules with dry-run preview.";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchClass { get; set; } = "ADDIN";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string AddinName { get; set; } = "uc_DefaultsEditor";
        #endregion

        /// <summary>The defaults currently loaded in the grid for the selected connection.</summary>
        public IReadOnlyList<DefaultValue> Defaults => _defaults;

        private void WireEvents()
        {
            _btnReload.Click += (_, _) => LoadDefaults();
            _btnAdd.Click += BtnAdd_Click;
            _btnRemove.Click += BtnRemove_Click;
            _btnValidate.Click += BtnValidate_Click;
            _btnTest.Click += BtnTest_Click;
            _btnSave.Click += BtnSave_Click;
            _cboConnection.SelectedItemChanged += (_, _) => LoadDefaults();
        }

        protected override void ApplyDpiScaledLayout()
        {
            _rootPanel.Padding = BeepLayoutMetrics.DialogPadding.ScalePadding(this);
            _contentHost.Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this);
            _toolbarPanel.Padding = BeepLayoutMetrics.ButtonStripPd.ScalePadding(this);
            _actionsPanel.Padding = BeepLayoutMetrics.ButtonStripPd.ScalePadding(this);

            int btnH = BeepLayoutMetrics.ButtonStandard.Height.ScaleValue(this);
            int btnLargeH = BeepLayoutMetrics.ButtonLarge.Height.ScaleValue(this);
            _btnSave.MinimumSize = new System.Drawing.Size(
                BeepLayoutMetrics.ButtonLarge.Width.ScaleValue(this), btnLargeH);
            foreach (var b in new[] { _btnReload, _btnValidate, _btnTest })
                b.MinimumSize = new System.Drawing.Size(
                    BeepLayoutMetrics.ButtonStandard.Width.ScaleValue(this), btnH);
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
                _cboConnection.ListItems.Add(new SimpleItem
                {
                    Text = $"{conn.ConnectionName} ({conn.DatabaseType})",
                    Value = conn.ConnectionName
                });
            }
        }

        /// <summary>
        /// Reads the persisted defaults for the selected connection through
        /// IDMEEditor.Getdefaults (backed by ConnectionProperties.DatasourceDefaults).
        /// </summary>
        private void LoadDefaults()
        {
            var editor = beepService?.DMEEditor;
            var connectionName = _cboConnection.SelectedItem?.Value as string;
            if (editor == null || string.IsNullOrWhiteSpace(connectionName))
                return;

            _currentConnection = connectionName;
            try
            {
                var loaded = editor.Getdefaults(connectionName) ?? new List<DefaultValue>();
                _defaults.Clear();

                // Cloned, not shared. Getdefaults hands back the live
                // ConnectionProperties.DatasourceDefaults list, and binding those very objects to the
                // grid meant every keystroke mutated the saved configuration in place — before Save
                // was ever clicked, and with no way to back out. Editing copies makes Save the point
                // at which anything changes, and Reload a genuine discard.
                foreach (var d in loaded) _defaults.Add(CloneForEditing(d));

                SetStatus($"Loaded {_defaults.Count} default(s) for '{connectionName}'.");
            }
            catch (Exception ex)
            {
                SetStatus($"Failed to load defaults: {ex.Message}");
            }
        }

        /// <summary>
        /// Copies a persisted default into an editable buffer object.
        /// </summary>
        /// <remarks>
        /// Uses the model's own <c>Clone()</c> and then puts back the three fields it deliberately
        /// changes — GuidID, CreatedDate and ModifiedDate. Clone mints a fresh identity and stamps
        /// both dates with "now" because it is built for "duplicate this record", whereas this is
        /// "edit that same record". Without the restore, opening the editor and saving would
        /// re-identify every default and stamp every row — including the ones nobody touched — with
        /// the save time, destroying the real last-modified history.
        /// </remarks>
        private static DefaultValue CloneForEditing(DefaultValue? source)
        {
            if (source == null) return new DefaultValue();
            var copy = source.Clone();
            copy.GuidID = source.GuidID;
            copy.CreatedDate = source.CreatedDate;
            copy.ModifiedDate = source.ModifiedDate;
            return copy;
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentConnection))
            {
                SetStatus("Select a connection before adding a default.");
                return;
            }

            var name = BeepDialogManager.Instance.InputText(
                "Add Default", "Field name this default applies to:");
            if (string.IsNullOrWhiteSpace(name)) return;

            _defaults.Add(new DefaultValue
            {
                PropertyName = name.Trim(),
                propertyType = DefaultValueType.Static,
                IsEnabled = true
            });
            SetStatus($"Added default for '{name.Trim()}'. Not saved yet.");
        }

        private void BtnRemove_Click(object? sender, EventArgs e)
        {
            var selected = SelectedDefault();
            if (selected == null)
            {
                SetStatus("Select a row to remove.");
                return;
            }

            if (!BeepDialogManager.Instance.Confirm(
                    "Remove Default",
                    $"Remove the default for '{selected.PropertyName}'?"))
                return;

            _defaults.Remove(selected);
            SetStatus($"Removed '{selected.PropertyName}'. Not saved yet.");
        }

        /// <summary>
        /// Validates every rule-based default through the engine and reports the failures.
        /// DefaultValue.IsValid() is the model's own self-check, so validity is reported
        /// rather than stamped onto the row.
        /// </summary>
        private void BtnValidate_Click(object? sender, EventArgs e)
        {
            var editor = beepService?.DMEEditor;
            if (editor == null) return;

            // Errors and warnings are kept apart. ValidateRule returns Errors.Warning for a rule it
            // does not recognise but cannot fault — and this used to test `Flag != Errors.Ok`, so
            // every warning was reported as a problem. TestRule only rejects Errors.Failed, so the
            // same rule made Validate say "invalid" and Test say "fine, here's the value". The two
            // buttons now agree on what a failure is.
            var failures = new List<string>();
            var warnings = new List<string>();
            int checkedCount = 0;

            foreach (var d in _defaults)
            {
                if (!d.IsValid())
                    failures.Add($"{d.PropertyName}: incomplete definition.");

                if (string.IsNullOrWhiteSpace(d.Rule)) continue;
                checkedCount++;

                var result = DefaultsManager.ValidateRule(editor, d.Rule);
                if (result == null) continue;

                if (result.Flag == Errors.Failed)
                    failures.Add($"{d.PropertyName}: {result.Message ?? "rule is invalid."}");
                else if (result.Flag != Errors.Ok)
                    warnings.Add($"{d.PropertyName}: {result.Message ?? "rule was not recognised."}");
            }

            if (failures.Count == 0 && warnings.Count == 0)
            {
                SetStatus(checkedCount == 0
                    ? $"{_defaults.Count} default(s) are well-formed; no rules to evaluate."
                    : $"All {checkedCount} rule(s) are valid.");
                return;
            }

            SetStatus($"{failures.Count} error(s) and {warnings.Count} warning(s) across {_defaults.Count} default(s).");

            var report = new List<string>();
            if (failures.Count > 0)
                report.Add($"Errors — these will not resolve:{Environment.NewLine}" +
                           string.Join(Environment.NewLine, failures));
            if (warnings.Count > 0)
                report.Add($"Warnings — unrecognised, but the engine will still try to resolve them:{Environment.NewLine}" +
                           string.Join(Environment.NewLine, warnings));

            BeepDialogManager.Instance.ShowWarning(
                "Validation Problems",
                string.Join(Environment.NewLine + Environment.NewLine, report));
        }

        /// <summary>Evaluates the selected row's rule and shows the value it resolves to.</summary>
        /// <remarks>
        /// Off the UI thread, because resolving a rule is not necessarily cheap: the resolver
        /// pipeline is plugin-driven and its vocabulary includes WEBSERVICE:, API:, HTTP: and
        /// LOOKUP:, so a single test can issue an HTTP or database call. TestRule is synchronous
        /// with no async variant and no timeout, so it ran to completion on the UI thread — a slow
        /// endpoint froze the editor with no way out. The token cannot interrupt the call itself
        /// (nothing in the pipeline observes one); it stops the result being applied.
        /// </remarks>
        private void BtnTest_Click(object? sender, EventArgs e) => _ = TestAsync();

        private async Task TestAsync()
        {
            var editor = beepService?.DMEEditor;
            var selected = SelectedDefault();
            if (editor == null) return;
            if (selected == null)
            {
                SetStatus("Select a row to test.");
                return;
            }
            if (string.IsNullOrWhiteSpace(selected.Rule))
            {
                SetStatus($"'{selected.PropertyName}' has no rule to test.");
                return;
            }
            if (_busy) { SetStatus("An operation is already running."); return; }

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _busy = true;
            SetBusyUi(true, $"Testing '{selected.PropertyName}'…");
            try
            {
                string rule = selected.Rule;
                var (result, value) = await Task.Run(() => DefaultsManager.TestRule(editor, rule), token)
                    .ConfigureAwait(true);

                if (token.IsCancellationRequested || IsDisposed) return;

                if (result?.Flag == Errors.Ok)
                {
                    BeepDialogManager.Instance.ShowInfo(
                        "Rule Result",
                        $"{selected.PropertyName}{Environment.NewLine}{Environment.NewLine}" +
                        $"Rule: {selected.Rule}{Environment.NewLine}Resolves to: {value ?? "(null)"}");
                    SetStatus($"'{selected.PropertyName}' resolved to: {value ?? "(null)"}");
                }
                else
                {
                    BeepDialogManager.Instance.ShowError(
                        "Rule Failed",
                        $"{selected.PropertyName}{Environment.NewLine}{Environment.NewLine}" +
                        $"Rule: {selected.Rule}{Environment.NewLine}{Environment.NewLine}{result?.Message}");
                    SetStatus($"'{selected.PropertyName}' failed: {result?.Message}");
                }
            }
            catch (OperationCanceledException)
            {
                SetStatus("Rule test cancelled.");
            }
            catch (Exception ex)
            {
                SetStatus($"Rule test threw: {ex.Message}");
            }
            finally
            {
                _busy = false;
                SetBusyUi(false, null);
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e) => _ = SaveAsync();

        /// <summary>Persists the edited defaults for the current connection.</summary>
        /// <remarks>
        /// Off the UI thread: Savedefaults writes through to
        /// <c>ConfigEditor.SaveDataconnectionsValues()</c>, which serialises the WHOLE
        /// DataConnections.json — every connection, not just this one — so it is real disk I/O
        /// rather than an in-memory update, despite Getdefaults being purely in-memory.
        /// </remarks>
        private async Task SaveAsync()
        {
            var editor = beepService?.DMEEditor;
            if (editor == null || string.IsNullOrWhiteSpace(_currentConnection))
            {
                SetStatus("Select a connection before saving.");
                return;
            }
            if (_busy) { SetStatus("An operation is already running."); return; }

            _busy = true;
            SetBusyUi(true, "Saving defaults…");
            try
            {
                // Snapshotted before the hop so grid edits during the write cannot alter what is
                // being serialised.
                var payload = _defaults.Select(CloneForEditing).ToList();
                string connection = _currentConnection;

                var result = await Task.Run(() => editor.Savedefaults(payload, connection)).ConfigureAwait(true);
                if (IsDisposed) return;
                bool ok = result?.Flag == Errors.Ok;

                // Reports the snapshot, not the live fields: `connection`/`payload` are what was
                // actually written, whereas _currentConnection and _defaults can have moved on.
                SetStatus(ok
                    ? $"Saved {payload.Count} default(s) for '{connection}'."
                    : $"Save failed: {result?.Message}");

                if (!ok)
                    BeepDialogManager.Instance.ShowError("Save Failed", result?.Message ?? "Unknown error.");

                Completed?.Invoke(this, new WizardCompletedEventArgs
                {
                    Succeeded = ok,
                    Summary = _lblStatus.Text
                });
            }
            catch (Exception ex)
            {
                SetStatus($"Save threw: {ex.Message}");
                BeepDialogManager.Instance.ShowError("Save Failed", ex.Message);
            }
            finally
            {
                _busy = false;
                SetBusyUi(false, null);
            }
        }

        private DefaultValue? SelectedDefault()
        {
            int index = _gridDefaults.CurrentRowIndex;
            return index >= 0 && index < _defaults.Count ? _defaults[index] : null;
        }

        /// <summary>
        /// Locks the action buttons for the duration of an engine call. The buttons stay live across
        /// an await otherwise, and the grid must not be edited while a save is serialising its rows.
        /// </summary>
        private void SetBusyUi(bool busy, string? message)
        {
            foreach (var b in new[] { _btnSave, _btnReload, _btnValidate, _btnTest, _btnAdd, _btnRemove })
                b.Enabled = !busy;
            _gridDefaults.Enabled = !busy;
            // The connection picker too: its SelectedItemChanged reloads _defaults and reassigns
            // _currentConnection, so switching connection mid-save swapped the editor's state out
            // from under the in-flight write.
            _cboConnection.Enabled = !busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
            if (message != null) SetStatus(message);
        }

        private void SetStatus(string message) => _lblStatus.Text = message;

        /// <summary>Asks an in-flight rule test to stop being applied; it cannot interrupt the call.</summary>
        protected override void OnHandleDestroyed(EventArgs e)
        {
            _cts?.Cancel();
            base.OnHandleDestroyed(e);
        }
    }
}
