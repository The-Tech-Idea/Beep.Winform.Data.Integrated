using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Services;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;
using TheTechIdea.Beep.Winform.Controls.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.NuggetsManage
{
    [AddinAttribute(Caption = "Nuggets Manager", Name = "uc_NuggetsManage", misc = "Config", menu = "Configuration", addinType = AddinType.Control, displayType = DisplayType.InControl, ObjectType = "Beep")]
    public partial class uc_NuggetsManage : UserControl
    {
        private readonly IServiceProvider? _services;
        private readonly IBeepService? _beepService;
        private readonly IDMEEditor? _editor;

        private readonly List<NuggetItemState> _states = new();
        private readonly BindingList<SimpleItem> _listItems = new();
        private NuggetsManageService _service;
        private bool _isUpdatingSelectionUi;

        public uc_NuggetsManage() : base()
        {
            InitializeComponent();
        }

        public uc_NuggetsManage(IServiceProvider services) : this()
        {
            _services = services;
            _beepService = services.GetService(typeof(IBeepService)) as IBeepService;
            _editor = _beepService?.DMEEditor;

            _nuggetList.ListItems = _listItems;

            _enableAtStartupCheckBox.StateChanged += EnableAtStartupCheckBox_StateChanged;
            _browseScanPathButton.Click += BrowseScanPathButton_Click;
            _scanButton.Click += ScanButton_Click;
            _installButton.Click += InstallButton_Click;
            _loadButton.Click += LoadButton_Click;
            _unloadButton.Click += UnloadButton_Click;
            _refreshButton.Click += RefreshButton_Click;
            _copyLogsButton.Click += CopyLogsButton_Click;
            _clearLogsButton.Click += ClearLogsButton_Click;
            _nuggetList.SelectedItemChanged += NuggetList_SelectedItemChanged;
        }

        public void InitializeManager()
        {
            InitializeService();
            RestorePersistedStates();
            RefreshNuggetList();
        }

        private void InitializeService()
        {
            if (_service != null || _editor == null)
            {
                return;
            }

            _service = new NuggetsManageService(_editor);
        }

        private void RestorePersistedStates()
        {
            if (_service == null)
            {
                return;
            }

            _states.Clear();
            _states.AddRange(_service.LoadPersistedStates());
            AppendLog(NuggetOperationSeverity.Info, $"Loaded {_states.Count} persisted nugget state entries.");
            BindStatesToList();
        }

        private void ScanButton_Click(object sender, EventArgs e)
        {
            if (_service == null)
            {
                return;
            }

            var roots = new List<string>();
            if (!string.IsNullOrWhiteSpace(_scanPathTextBox.Text))
            {
                roots.Add(_scanPathTextBox.Text.Trim());
            }

            var scanned = _service.ScanNuggets(roots);
            foreach (var scannedItem in scanned)
            {
                MergeScannedState(scannedItem);
            }

            _service.SaveStates(_states);
            BindStatesToList();
            AppendLog(NuggetOperationSeverity.Info, $"Scan completed. Found {scanned.Count} nuggets.");
            SetStatus("Scan completed.");
        }

        private void InstallButton_Click(object sender, EventArgs e)
        {
            if (_service == null)
            {
                return;
            }

            using var openDialog = new OpenFileDialog
            {
                Title = "Select nugget package or assembly",
                Filter = "Nugget Files (*.nupkg;*.dll)|*.nupkg;*.dll|All Files (*.*)|*.*",
                Multiselect = false
            };

            if (openDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var result = _service.InstallFromPath(openDialog.FileName, _states);
            AppendLog(result.Severity, result.Message);
            SetStatus(result.Message);
            _service.SaveStates(_states);
            BindStatesToList();
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            if (_service == null)
            {
                return;
            }

            var selected = GetSelectedState();
            if (selected == null)
            {
                AppendLog(NuggetOperationSeverity.Warn, "Select a nugget to load.");
                return;
            }

            var result = _service.LoadNugget(selected);
            if (result.Success)
            {
                selected.IsEnabled = true;
            }

            PersistAndRefresh(result);
        }

        private void UnloadButton_Click(object sender, EventArgs e)
        {
            if (_service == null)
            {
                return;
            }

            var selected = GetSelectedState();
            if (selected == null)
            {
                AppendLog(NuggetOperationSeverity.Warn, "Select a nugget to unload.");
                return;
            }

            var result = _service.UnloadNugget(selected);
            if (result.Success)
            {
                selected.IsEnabled = false;
            }

            PersistAndRefresh(result);
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            RefreshNuggetList();
        }

        private void BrowseScanPathButton_Click(object sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog
            {
                Description = "Select nugget scan root"
            };

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                _scanPathTextBox.Text = folderDialog.SelectedPath;
            }
        }

        private void NuggetList_SelectedItemChanged(object sender, SelectedItemChangedEventArgs e)
        {
            var state = e?.SelectedItem?.Item as NuggetItemState;
            _isUpdatingSelectionUi = true;
            _enableAtStartupCheckBox.CurrentValue = state?.IsEnabled ?? false;
            _isUpdatingSelectionUi = false;
            UpdateDetails(state);
        }

        private void EnableAtStartupCheckBox_StateChanged(object sender, EventArgs e)
        {
            if (_isUpdatingSelectionUi || _service == null)
            {
                return;
            }

            var selected = GetSelectedState();
            if (selected == null)
            {
                return;
            }

            selected.IsEnabled = _enableAtStartupCheckBox.CurrentValue;
            selected.LastUpdatedUtc = DateTime.UtcNow;
            _service.SaveStates(_states);
            BindStatesToList();
            UpdateDetails(selected);
            var statusText = selected.IsEnabled ? "enabled" : "disabled";
            AppendLog(NuggetOperationSeverity.Info, $"Startup auto-load {statusText} for '{selected.NuggetName}'.");
            SetStatus($"Startup auto-load {statusText}.");
        }

        private void CopyLogsButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_operationLogTextBox.Text))
            {
                Clipboard.SetText(_operationLogTextBox.Text);
                SetStatus("Logs copied.");
            }
        }

        private void ClearLogsButton_Click(object sender, EventArgs e)
        {
            _operationLogTextBox.Text = string.Empty;
            SetStatus("Logs cleared.");
        }

        private void RefreshNuggetList()
        {
            if (_service == null)
            {
                return;
            }

            foreach (var state in _states)
            {
                state.IsMissing = !File.Exists(state.SourcePath) && !Directory.Exists(state.SourcePath);
                if (state.IsMissing)
                {
                    state.IsLoaded = false;
                }
            }

            _service.SaveStates(_states);
            BindStatesToList();
            SetStatus("Nugget list refreshed.");
        }

        private void MergeScannedState(NuggetItemState scannedState)
        {
            var existing = _states.FirstOrDefault(state =>
                state.SourcePath.Equals(scannedState.SourcePath, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                _states.Add(scannedState);
                return;
            }

            existing.NuggetName = scannedState.NuggetName;
            existing.NuggetVersion = scannedState.NuggetVersion;
            existing.NuggetSource = scannedState.NuggetSource;
            existing.IsMissing = false;
            existing.LastUpdatedUtc = DateTime.UtcNow;
        }

        private void BindStatesToList()
        {
            _listItems.Clear();
            foreach (var state in _states.OrderBy(state => state.NuggetName, StringComparer.OrdinalIgnoreCase))
            {
                _listItems.Add(new SimpleItem
                {
                    Text = state.ToDisplayText(),
                    Item = state
                });
            }

            if (_listItems.Count > 0 && _nuggetList.SelectedItem == null)
            {
                _nuggetList.SelectedItem = _listItems[0];
            }
        }

        private NuggetItemState GetSelectedState()
        {
            return _nuggetList.SelectedItem?.Item as NuggetItemState;
        }

        private void UpdateDetails(NuggetItemState state)
        {
            if (state == null)
            {
                _detailsTextBox.Text = "Select a nugget item to view details.";
                return;
            }

            _detailsTextBox.Text =
                $"Name: {state.NuggetName}{Environment.NewLine}" +
                $"Version: {state.NuggetVersion}{Environment.NewLine}" +
                $"Source: {state.NuggetSource}{Environment.NewLine}" +
                $"Path: {state.SourcePath}{Environment.NewLine}" +
                $"Loaded: {state.IsLoaded}{Environment.NewLine}" +
                $"EnabledAtStartup: {state.IsEnabled}{Environment.NewLine}" +
                $"Missing: {state.IsMissing}{Environment.NewLine}" +
                $"LastUpdatedUtc: {state.LastUpdatedUtc:O}";
        }

        private void PersistAndRefresh(NuggetOperationResult result)
        {
            if (_service != null)
            {
                _service.SaveStates(_states);
            }

            BindStatesToList();
            AppendLog(result.Severity, result.Message);
            SetStatus(result.Message);
            UpdateDetails(GetSelectedState());
        }

        private void AppendLog(NuggetOperationSeverity severity, string message)
        {
            var line = $"{DateTime.Now:HH:mm:ss} [{severity.ToString().ToUpperInvariant()}] {message}";
            _operationLogTextBox.Text = string.IsNullOrWhiteSpace(_operationLogTextBox.Text)
                ? line
                : $"{_operationLogTextBox.Text}{Environment.NewLine}{line}";

            if (_editor != null)
            {
                var flag = severity == NuggetOperationSeverity.Error ? Errors.Failed : Errors.Ok;
                _editor.AddLogMessage("NuggetsManage", message, DateTime.Now, 0, null, flag);
            }
        }

        private void SetStatus(string message)
        {
            _statusLabel.Text = message;
        }
    }
}
