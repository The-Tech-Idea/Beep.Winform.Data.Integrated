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

        public uc_DefaultsEditor() : this(null) { }
        public uc_DefaultsEditor(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            Details.AddinName = "Defaults Editor";
            WireEvents();
            ApplyDpiScaledLayout();
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

        private void ApplyDpiScaledLayout()
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
                foreach (var d in loaded) _defaults.Add(d);
                SetStatus($"Loaded {_defaults.Count} default(s) for '{connectionName}'.");
            }
            catch (Exception ex)
            {
                SetStatus($"Failed to load defaults: {ex.Message}");
            }
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

            var failures = new List<string>();
            int checkedCount = 0;

            foreach (var d in _defaults)
            {
                if (!d.IsValid())
                    failures.Add($"{d.PropertyName}: incomplete definition.");

                if (string.IsNullOrWhiteSpace(d.Rule)) continue;
                checkedCount++;

                var result = DefaultsManager.ValidateRule(editor, d.Rule);
                if (result?.Flag != Errors.Ok)
                    failures.Add($"{d.PropertyName}: {result?.Message ?? "rule is invalid."}");
            }

            if (failures.Count == 0)
            {
                SetStatus(checkedCount == 0
                    ? $"{_defaults.Count} default(s) are well-formed; no rules to evaluate."
                    : $"All {checkedCount} rule(s) are valid.");
                return;
            }

            SetStatus($"{failures.Count} problem(s) found across {_defaults.Count} default(s).");
            BeepDialogManager.Instance.ShowWarning(
                "Validation Problems",
                string.Join(Environment.NewLine, failures));
        }

        /// <summary>Evaluates the selected row's rule and shows the value it resolves to.</summary>
        private void BtnTest_Click(object? sender, EventArgs e)
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

            try
            {
                var (result, value) = DefaultsManager.TestRule(editor, selected.Rule);
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
            catch (Exception ex)
            {
                SetStatus($"Rule test threw: {ex.Message}");
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var editor = beepService?.DMEEditor;
            if (editor == null || string.IsNullOrWhiteSpace(_currentConnection))
            {
                SetStatus("Select a connection before saving.");
                return;
            }

            try
            {
                var result = editor.Savedefaults(_defaults.ToList(), _currentConnection);
                bool ok = result?.Flag == Errors.Ok;

                SetStatus(ok
                    ? $"Saved {_defaults.Count} default(s) for '{_currentConnection}'."
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
        }

        private DefaultValue? SelectedDefault()
        {
            int index = _gridDefaults.CurrentRowIndex;
            return index >= 0 && index < _defaults.Count ? _defaults[index] : null;
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
