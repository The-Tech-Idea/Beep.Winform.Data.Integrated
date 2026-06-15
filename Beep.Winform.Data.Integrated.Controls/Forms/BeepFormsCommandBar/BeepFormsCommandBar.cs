using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Base;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Helpers;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(BeepFormsCommandBar))]
    [Category("Beep Controls")]
    [DisplayName("Beep Forms Command Bar")]
    [Description("Standalone form-level command surface for BeepForms block switching and sync.")]
    [Designer("TheTechIdea.Beep.Winform.Controls.Design.Server.Designers.BeepFormsCommandBarDesigner, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public sealed class BeepFormsCommandBar : BaseControl
    {
        private readonly FlowLayoutPanel _commandPanel;
        private BeepForms? _formsHost;
        private bool _autoBindFormsHost = true;
        private BeepButton? _blockSelectorButton;
        private BeepButton? _syncButton;
        private BeepButton? _previousBlockButton;
        private BeepButton? _nextBlockButton;
        private BeepButton? _firstBlockButton;
        private BeepButton? _lastBlockButton;
        private BeepButton? _insertButton;
        private BeepButton? _deleteButton;
        private BeepButton? _duplicateButton;
        private BeepButton? _clearRecordButton;
        private BeepButton? _clearBlockButton;
        private BeepButton? _showLovButton;
        private BeepButton? _refreshButton;
        private BeepButton? _undoButton;
        private BeepButton? _redoButton;
        private BeepButton? _exportButton;
        private BeepButton? _importButton;
        private BeepButton? _jumpToErrorButton;
        private BeepButton? _gridViewToggleButton;
        private BeepFormsCommandBarButtons _commandButtons = BeepFormsCommandBarButtons.All;
        private FlowDirection _commandFlowDirection = FlowDirection.LeftToRight;

        public BeepFormsCommandBar()
        {
            UseThemeColors = true;
            Padding = new Padding(0);
            Margin = new Padding(0);
            MinimumSize = new Size(0, BeepLayoutMetrics.ButtonToolbar.ScaleSize(this).Height + 4);
            Height = BeepLayoutMetrics.ButtonToolbar.ScaleSize(this).Height + 4;

            _commandPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                AutoScroll = true,
                Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this),
                Margin = new Padding(0),
                FlowDirection = FlowDirection.LeftToRight
            };

            Controls.Add(_commandPanel);
            RefreshCommandStrip();
        }

        [Browsable(true)]
        [Category("Behavior")]
        [Description("Optional BeepForms coordinator surfaced by this command bar.")]
        [DefaultValue(null)]
        public BeepForms? FormsHost
        {
            get => _formsHost;
            set
            {
                if (ReferenceEquals(_formsHost, value))
                {
                    return;
                }

                DetachFormsHost(_formsHost);
                _formsHost = value;
                AttachFormsHost(_formsHost);
                UpdateCommandStripState();
            }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [Description("Automatically resolve a nearby BeepForms host when FormsHost is not set explicitly.")]
        [DefaultValue(true)]
        public bool AutoBindFormsHost
        {
            get => _autoBindFormsHost;
            set
            {
                if (_autoBindFormsHost == value)
                {
                    return;
                }

                _autoBindFormsHost = value;
                if (_autoBindFormsHost && _formsHost == null)
                {
                    TryBindFormsHostFromHierarchy();
                }
            }
        }

        [Browsable(true)]
        [Category("Command Bar")]
        [Description("Select which top-level form command buttons are shown.")]
        [DefaultValue(BeepFormsCommandBarButtons.All)]
        public BeepFormsCommandBarButtons CommandButtons
        {
            get => _commandButtons;
            set
            {
                if (_commandButtons == value)
                {
                    return;
                }

                _commandButtons = value;
                RefreshCommandStrip();
            }
        }

        [Browsable(true)]
        [Category("Command Bar")]
        [Description("Controls the flow direction used by the form command button row.")]
        [DefaultValue(FlowDirection.LeftToRight)]
        public FlowDirection CommandFlowDirection
        {
            get => _commandFlowDirection;
            set
            {
                if (_commandFlowDirection == value)
                {
                    return;
                }

                _commandFlowDirection = value;
                RefreshCommandStrip();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DetachFormsHost(_formsHost);
            }

            base.Dispose(disposing);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            TryBindFormsHostFromHierarchy();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            TryBindFormsHostFromHierarchy();
        }

        private void AttachFormsHost(BeepForms? formsHost)
        {
            if (formsHost == null)
            {
                return;
            }

            formsHost.ActiveBlockChanged += FormsHost_StateChanged;
            formsHost.FormsManagerChanged += FormsHost_StateChanged;
            formsHost.ViewStateChanged += FormsHost_StateChanged;
            formsHost.Disposed += FormsHost_Disposed;
        }

        private void DetachFormsHost(BeepForms? formsHost)
        {
            if (formsHost == null)
            {
                return;
            }

            formsHost.ActiveBlockChanged -= FormsHost_StateChanged;
            formsHost.FormsManagerChanged -= FormsHost_StateChanged;
            formsHost.ViewStateChanged -= FormsHost_StateChanged;
            formsHost.Disposed -= FormsHost_Disposed;
        }

        private void FormsHost_StateChanged(object? sender, EventArgs e)
        {
            UpdateCommandStripState();
        }

        private void FormsHost_Disposed(object? sender, EventArgs e)
        {
            FormsHost = null;
            TryBindFormsHostFromHierarchy();
        }

        private void TryBindFormsHostFromHierarchy()
        {
            if (!AutoBindFormsHost || _formsHost != null || Parent == null)
            {
                return;
            }

            BeepForms? resolvedHost = BeepFormsHostResolver.Find(this);
            if (resolvedHost != null)
            {
                FormsHost = resolvedHost;
            }
        }

        private void RefreshCommandStrip()
        {
            _commandPanel.SuspendLayout();
            _commandPanel.Controls.Clear();
            _commandPanel.FlowDirection = CommandFlowDirection;

            AddBlockSelectorButton();
            AddButton(ref _firstBlockButton, BeepFormsCommandBarButtons.FirstBlock, "|◀", FirstBlockButton_Click, 42);
            AddButton(ref _previousBlockButton, BeepFormsCommandBarButtons.PreviousBlock, "◀", PreviousBlockButton_Click, 42);
            AddButton(ref _nextBlockButton, BeepFormsCommandBarButtons.NextBlock, "▶", NextBlockButton_Click, 42);
            AddButton(ref _lastBlockButton, BeepFormsCommandBarButtons.LastBlock, "▶|", LastBlockButton_Click, 42);
            AddButton(ref _insertButton, BeepFormsCommandBarButtons.InsertRecord, "+", InsertButton_Click, 42);
            AddButton(ref _deleteButton, BeepFormsCommandBarButtons.DeleteRecord, "−", DeleteButton_Click, 42);
            AddButton(ref _duplicateButton, BeepFormsCommandBarButtons.DuplicateRecord, "Dup", DuplicateButton_Click, 52);
            AddButton(ref _clearRecordButton, BeepFormsCommandBarButtons.ClearRecord, "Clr Rec", ClearRecordButton_Click, 76);
            AddButton(ref _clearBlockButton, BeepFormsCommandBarButtons.ClearBlock, "Clr Blk", ClearBlockButton_Click, 76);
            AddButton(ref _showLovButton, BeepFormsCommandBarButtons.ShowLOV, "LOV", ShowLovButton_Click, 54);
            AddButton(ref _refreshButton, BeepFormsCommandBarButtons.RefreshBlock, "⟳", RefreshButton_Click, 42);
            AddButton(ref _undoButton, BeepFormsCommandBarButtons.Undo, "↩", UndoButton_Click, 42);
            AddButton(ref _redoButton, BeepFormsCommandBarButtons.Redo, "↪", RedoButton_Click, 42);
            AddButton(ref _exportButton, BeepFormsCommandBarButtons.ExportJson, "⤓", ExportButton_Click, 42);
            AddButton(ref _importButton, BeepFormsCommandBarButtons.ImportJson, "⤒", ImportButton_Click, 42);
            AddButton(ref _jumpToErrorButton, BeepFormsCommandBarButtons.JumpToFirstError, "⟶Err", JumpToErrorButton_Click, 62);
            AddButton(ref _gridViewToggleButton, BeepFormsCommandBarButtons.ToggleGridView, "⊞", GridViewToggleButton_Click, 42);
            AddButton(ref _syncButton, BeepFormsCommandBarButtons.Sync, "Sync", SyncButton_Click, 88);

            _commandPanel.Visible = _commandPanel.Controls.Count > 0;
            _commandPanel.ResumeLayout(false);

            UpdateCommandStripState();
        }

        private void AddBlockSelectorButton()
        {
            if (!CommandButtons.HasFlag(BeepFormsCommandBarButtons.BlockSelector))
            {
                _blockSelectorButton = null;
                return;
            }

            _blockSelectorButton = new BeepButton
            {
                Width = 150,
                Height = 28,
                Margin = new Padding(0, 0, 8, 0),
                Text = "Select Block",
                Theme = Theme,
                ShowShadow = false,
                PopupMode = true,
                UseThemeColors = true
            };
            _blockSelectorButton.SelectedItemChanged += BlockSelectorButton_SelectedItemChanged;
            _commandPanel.Controls.Add(_blockSelectorButton);
        }

        private void AddButton(ref BeepButton? field, BeepFormsCommandBarButtons flag, string caption, EventHandler clickHandler, int minimumWidth)
        {
            if (!CommandButtons.HasFlag(flag))
            {
                field = null;
                return;
            }

            field = new BeepButton
            {
                Width = GetButtonWidth(caption, minimumWidth),
                Height = 28,
                Margin = new Padding(0, 0, 8, 0),
                Text = caption,
                Theme = Theme,
                ShowShadow = false,
                UseThemeColors = true
            };
            field.Click += clickHandler;
            _commandPanel.Controls.Add(field);
        }

        private int GetButtonWidth(string caption, int minimumWidth)
        {
            int measuredWidth = TextRenderer.MeasureText(caption ?? string.Empty, Font).Width + 28;
            return Math.Max(minimumWidth, measuredWidth);
        }

        private void UpdateCommandStripState()
        {
            bool hasHost = _formsHost != null;
            bool hasActiveBlock = hasHost && !string.IsNullOrWhiteSpace(_formsHost!.ActiveBlockName);

            if (_blockSelectorButton != null)
            {
                var blockItems = BuildBlockSelectorItems();
                _blockSelectorButton.ListItems = blockItems;
                _blockSelectorButton.Enabled = hasHost && blockItems.Count > 0;
                _blockSelectorButton.Text = hasActiveBlock ? $"Block: {_formsHost!.ActiveBlockName}" : "Select Block";
                _blockSelectorButton.Visible = CommandButtons.HasFlag(BeepFormsCommandBarButtons.BlockSelector);
            }

            bool blockNavHasMultiple = hasHost && _formsHost!.Blocks.Count > 1;
            bool blockInsertAllowed = hasActiveBlock && IsBlockInsertAllowed();
            bool blockDeleteAllowed = hasActiveBlock && IsBlockDeleteAllowed();
            bool hasLovForActiveItem = hasActiveBlock && HasLovForActiveItem();
            bool hasErrors = hasHost && _formsHost!.ViewState.ErrorCount > 0;

            SetButtonState(_firstBlockButton, blockNavHasMultiple, true);
            SetButtonState(_previousBlockButton, blockNavHasMultiple, true);
            SetButtonState(_nextBlockButton, blockNavHasMultiple, true);
            SetButtonState(_lastBlockButton, blockNavHasMultiple, true);
            SetButtonState(_insertButton, blockInsertAllowed, true);
            SetButtonState(_deleteButton, blockDeleteAllowed, true);
            SetButtonState(_duplicateButton, hasActiveBlock, true);
            SetButtonState(_clearRecordButton, hasActiveBlock, true);
            SetButtonState(_clearBlockButton, hasActiveBlock, true);
            SetButtonState(_showLovButton, hasLovForActiveItem, true);
            if (_showLovButton != null)
            {
                string? lovTitle = GetLovTitle();
                _showLovButton.ToolTipText = !string.IsNullOrWhiteSpace(lovTitle) ? $"Show LOV: {lovTitle}" : "Show LOV";
            }
            SetButtonState(_refreshButton, hasActiveBlock, true);
            SetButtonState(_undoButton, hasActiveBlock && CanUndo(), true);
            SetButtonState(_redoButton, hasActiveBlock && CanRedo(), true);
            SetButtonState(_exportButton, hasActiveBlock, true);
            SetButtonState(_importButton, hasActiveBlock, true);
            SetButtonState(_jumpToErrorButton, hasErrors, true);
            SetButtonState(_gridViewToggleButton, hasActiveBlock, true);
            if (_gridViewToggleButton != null)
                _gridViewToggleButton.Text = IsGridViewActive() ? "⊟" : "⊞";
            SetButtonState(_syncButton, hasHost, true);
        }

        private bool IsBlockInsertAllowed()
        {
            if (_formsHost == null || string.IsNullOrWhiteSpace(_formsHost.ActiveBlockName)) return false;
            return _formsHost.TryGetBlockProperty(_formsHost.ActiveBlockName, "InsertAllowed", out object? val) && val is true;
        }

        private bool IsBlockDeleteAllowed()
        {
            if (_formsHost == null || string.IsNullOrWhiteSpace(_formsHost.ActiveBlockName)) return false;
            return _formsHost.TryGetBlockProperty(_formsHost.ActiveBlockName, "DeleteAllowed", out object? val) && val is true;
        }

        private bool HasLovForActiveItem()
        {
            if (_formsHost == null || string.IsNullOrWhiteSpace(_formsHost.ActiveBlockName)
                || string.IsNullOrWhiteSpace(_formsHost.ActiveItemName))
                return false;
            return _formsHost.HasLov(_formsHost.ActiveBlockName, _formsHost.ActiveItemName);
        }

        private string? GetLovTitle()
        {
            if (_formsHost == null || string.IsNullOrWhiteSpace(_formsHost.ActiveBlockName)
                || string.IsNullOrWhiteSpace(_formsHost.ActiveItemName))
                return null;
            return _formsHost.GetLov(_formsHost.ActiveBlockName, _formsHost.ActiveItemName)?.Title;
        }

        private bool CanUndo()
        {
            if (_formsHost?.FormsManager == null || string.IsNullOrWhiteSpace(_formsHost.ActiveBlockName))
                return false;
            try { return _formsHost.FormsManager.CanUndoBlock(_formsHost.ActiveBlockName); }
            catch { return false; }
        }

        private bool CanRedo()
        {
            if (_formsHost?.FormsManager == null || string.IsNullOrWhiteSpace(_formsHost.ActiveBlockName))
                return false;
            try { return _formsHost.FormsManager.CanRedoBlock(_formsHost.ActiveBlockName); }
            catch { return false; }
        }

        private static void SetButtonState(BeepButton? button, bool enabled, bool visible)
        {
            if (button == null) return;
            button.Enabled = enabled;
            button.Visible = visible;
        }

        private BindingList<SimpleItem> BuildBlockSelectorItems()
        {
            var items = new BindingList<SimpleItem>();
            if (_formsHost == null)
            {
                return items;
            }

            foreach (var block in _formsHost.Blocks)
            {
                if (string.IsNullOrWhiteSpace(block.BlockName))
                {
                    continue;
                }

                items.Add(new SimpleItem
                {
                    Text = block.BlockName,
                    Name = block.BlockName,
                    Value = block.BlockName,
                    Item = block.BlockName
                });
            }

            return items;
        }

        private async void BlockSelectorButton_SelectedItemChanged(object? sender, SelectedItemChangedEventArgs e)
        {
            if (_formsHost == null)
            {
                return;
            }

            try
            {
                string? blockName = e.SelectedItem?.Item?.ToString() ?? e.SelectedItem?.Value?.ToString() ?? e.SelectedItem?.Text;
                if (string.IsNullOrWhiteSpace(blockName))
                {
                    return;
                }

                bool switched = await _formsHost.SwitchToBlockAsync(blockName).ConfigureAwait(true);
                if (!switched)
                {
                    UpdateCommandStripState();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BeepFormsCommandBar.BlockSelectorButton] {ex.Message}");
            }
        }

        private void SyncButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null)
            {
                return;
            }

            _formsHost.SyncFromManager();
            _formsHost.ShowInfo("Fresh-start host synchronized from FormsManager.");
        }

        private void PreviousBlockButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null) return;
            try
            {
                _formsHost.Builtins?.PreviousBlock();
                _formsHost.SyncFromManager();
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsCommandBar.PreviousBlock] {ex.Message}"); }
        }

        private void NextBlockButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null) return;
            try
            {
                _formsHost.Builtins?.NextBlock();
                _formsHost.SyncFromManager();
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsCommandBar.NextBlock] {ex.Message}"); }
        }

        private async void InsertButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null) return;
            try { await _formsHost.InsertRecordAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsCommandBar.Insert] {ex.Message}"); }
        }

        private async void DeleteButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null) return;
            try { await _formsHost.DeleteCurrentRecordAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsCommandBar.Delete] {ex.Message}"); }
        }

        private async void DuplicateButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null) return;
            try { await _formsHost.DuplicateCurrentRecordAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsCommandBar.Duplicate] {ex.Message}"); }
        }

        private async void ClearRecordButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null) return;
            try { await _formsHost.ClearRecordAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsCommandBar.ClearRecord] {ex.Message}"); }
        }

        private async void ClearBlockButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null) return;
            try { await _formsHost.ClearBlockAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsCommandBar.ClearBlock] {ex.Message}"); }
        }

        private async void ShowLovButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null) return;
            try
            {
                string? blockName = _formsHost.ActiveBlockName;
                string? itemName = _formsHost.ActiveItemName;
                if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(itemName)) return;
                await _formsHost.ShowLovAsync(blockName, itemName).ConfigureAwait(true);
                _formsHost.SyncFromManager();
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsCommandBar.ShowLOV] {ex.Message}"); }
        }

        private void FirstBlockButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null) return;
            try { _formsHost.Builtins?.FirstBlock(); _formsHost.SyncFromManager(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsCommandBar.FirstBlock] {ex.Message}"); }
        }

        private void LastBlockButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null) return;
            try { _formsHost.Builtins?.LastBlock(); _formsHost.SyncFromManager(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsCommandBar.LastBlock] {ex.Message}"); }
        }

        private void RefreshButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null) return;
            try
            {
                _formsHost.SyncFromManager();
                _formsHost.ShowInfo("Block refreshed from FormsManager.");
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsCommandBar.Refresh] {ex.Message}"); }
        }

        private void UndoButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null || string.IsNullOrWhiteSpace(_formsHost.ActiveBlockName)) return;
            try
            {
                _formsHost.FormsManager?.UndoBlock(_formsHost.ActiveBlockName);
                _formsHost.SyncFromManager();
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsCommandBar.Undo] {ex.Message}"); }
        }

        private void RedoButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null || string.IsNullOrWhiteSpace(_formsHost.ActiveBlockName)) return;
            try
            {
                _formsHost.FormsManager?.RedoBlock(_formsHost.ActiveBlockName);
                _formsHost.SyncFromManager();
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsCommandBar.Redo] {ex.Message}"); }
        }

        private async void ExportButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null || string.IsNullOrWhiteSpace(_formsHost.ActiveBlockName)) return;
            try
            {
                string blockName = _formsHost.ActiveBlockName;
                var connection = _formsHost.DataConnection;
                string defaultName = connection?.CurrentConnection?.ConnectionName ?? "export";
                string fileName = $"{defaultName}_{blockName}.json";

                using var dialog = new SaveFileDialog
                {
                    FileName = fileName,
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    Title = $"Export {blockName} as JSON"
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    await ExportBlockToFileAsync(blockName, dialog.FileName);
                    _formsHost.ShowSuccess($"Exported '{blockName}' to {dialog.FileName}.");
                }
            }
            catch (Exception ex)
            {
                _formsHost?.ShowError($"Export failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[BeepFormsCommandBar.Export] {ex.Message}");
            }
        }

        private async void ImportButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null || string.IsNullOrWhiteSpace(_formsHost.ActiveBlockName)) return;
            try
            {
                string blockName = _formsHost.ActiveBlockName;

                using var dialog = new OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    Title = $"Import into {blockName}"
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    bool confirmed = await _formsHost.ConfirmAsync("Import Data",
                        $"Import data from {Path.GetFileName(dialog.FileName)} into '{blockName}'? " +
                        "This will add records to the existing block data.").ConfigureAwait(true);
                    if (!confirmed) return;

                    bool isJson = dialog.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
                    await ImportBlockFromFileAsync(blockName, dialog.FileName, isJson);
                    _formsHost.SyncFromManager();
                    _formsHost.ShowSuccess($"Imported data into '{blockName}' from {Path.GetFileName(dialog.FileName)}.");
                }
            }
            catch (Exception ex)
            {
                _formsHost?.ShowError($"Import failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[BeepFormsCommandBar.Import] {ex.Message}");
            }
        }

        private async Task ImportBlockFromFileAsync(string blockName, string filePath, bool isJson)
        {
            if (_formsHost?.FormsManager == null) return;
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            if (isJson)
                await _formsHost.FormsManager.ImportBlockFromJsonAsync(blockName, stream).ConfigureAwait(true);
            else
                await _formsHost.FormsManager.ImportBlockFromCsvAsync(blockName, stream).ConfigureAwait(true);
        }

        private async Task ExportBlockToFileAsync(string blockName, string filePath)
        {
            if (_formsHost?.FormsManager == null) return;
            using var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            await _formsHost.FormsManager.ExportBlockToJsonAsync(blockName, stream).ConfigureAwait(true);
        }

        private void JumpToErrorButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null || _formsHost.ViewState.ErrorCount <= 0) return;
            try
            {
                _formsHost.ShowErrorSummary();
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsCommandBar.JumpToError] {ex.Message}"); }
        }

        private void GridViewToggleButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null || string.IsNullOrWhiteSpace(_formsHost.ActiveBlockName)) return;
            try
            {
                var block = _formsHost.Blocks.FirstOrDefault(b =>
                    string.Equals(b.BlockName, _formsHost.ActiveBlockName, StringComparison.OrdinalIgnoreCase));
                if (block == null) return;

                bool isGrid = block is IBeepBlockView view && view.Definition?.PresentationMode == BeepBlockPresentationMode.Grid;
                if (block.Definition == null)
                {
                    block.Definition = new BeepBlockDefinition { PresentationMode = BeepBlockPresentationMode.Grid };
                }
                else if (isGrid)
                    block.Definition.PresentationMode = BeepBlockPresentationMode.Record;
                else
                    block.Definition.PresentationMode = BeepBlockPresentationMode.Grid;

                _formsHost.SyncFromManager();
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsCommandBar.GridToggle] {ex.Message}"); }
        }

        private bool IsGridViewActive()
        {
            if (_formsHost == null || string.IsNullOrWhiteSpace(_formsHost.ActiveBlockName)) return false;
            var block = _formsHost.Blocks.FirstOrDefault(b =>
                string.Equals(b.BlockName, _formsHost.ActiveBlockName, StringComparison.OrdinalIgnoreCase));
            return block is IBeepBlockView view && view.Definition?.PresentationMode == BeepBlockPresentationMode.Grid;
        }
    }
}