using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.Forms.Builtins;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Builtins;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Logon;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Services;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(BeepForms))]
    [Category("Beep Controls")]
    [DisplayName("Beep Forms")]
    [Description("Non-visual BeepForms coordinator that hosts block views over a FormsManager instance.")]
    [Designer("TheTechIdea.Beep.Winform.Controls.Design.Server.Designers.BeepFormsHostDesigner, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public partial class BeepForms : Panel, IBeepFormsHost
    {
        private readonly List<IBeepBlockView> _blocks = new();
        private readonly BeepViewState _viewState = new();
        private readonly BeepFormsManagerAdapter _managerAdapter = new();
        private IUnitofWorksManager? _formsManager;
        private BeepFormsDefinition? _definition;
        private bool _autoCreateBlocksFromDefinition = true;
        private string _formName = string.Empty;
        private BeepBuiltinsHostAdapter? _builtinsAdapter;
        private IBeepBuiltins? _builtins;
        private BeepDataConnection? _dataConnection;

        public BeepForms()
        {
            InitializeComponent();
            InitializeLayout();
            InitializeDragDrop();
            NotificationService = new BeepFormsMessageService();
        }

        [Browsable(true)]
        [Category("Data")]
        [Description("Logical form name used for FormsManager coordination.")]
        public string FormName
        {
            get => _formName;
            set
            {
                _formName = value?.Trim() ?? string.Empty;
                ApplyShellStateToUi();
            }
        }

        [Browsable(false)]
        public string? ActiveBlockName => _viewState.ActiveBlockName;

        /// <summary>
        /// Phase 5C — Optional <see cref="BeepDataConnection"/> used by <see cref="LogonAsync"/>
        /// to enumerate connection options in the WinForms logon dialog.
        /// </summary>
        [Browsable(true)]
        [Category("Data")]
        [Description("Optional BeepDataConnection component used by LogonAsync to enumerate connection options.")]
        [DefaultValue(null)]
        [TypeConverter(typeof(TheTechIdea.Beep.Winform.Controls.Converters.BeepDataConnectionComponentConverter))]
        public BeepDataConnection? DataConnection
        {
            get => _dataConnection;
            set => _dataConnection = value;
        }

        [Browsable(false)]
        public string? ActiveItemName => _viewState.ActiveItemName;

        [Browsable(false)]
        public IBeepBuiltins? Builtins
        {
            get
            {
                if (_builtins == null)
                {
                    _builtinsAdapter ??= new BeepBuiltinsHostAdapter(this);
                    _builtins = new BeepBuiltins(_builtinsAdapter);
                }
                return _builtins;
            }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [Description("Automatically create hosted BeepBlock controls from the assigned definition.")]
        [DefaultValue(true)]
        public bool AutoCreateBlocksFromDefinition
        {
            get => _autoCreateBlocksFromDefinition;
            set => _autoCreateBlocksFromDefinition = value;
        }

        [Browsable(false)]
        public IUnitofWorksManager? FormsManager
        {
            get => _formsManager;
            set
            {
                if (ReferenceEquals(_formsManager, value))
                {
                    return;
                }

                DetachFromFormsManager(_formsManager);
                _formsManager = value;
                _managerAdapter.Attach(_formsManager);
                AttachToFormsManager(_formsManager);
                if (_formsManager != null && _definition?.Blocks?.Count > 0)
                    _ = InitializeAsync();
                else
                    SyncFromManager();
                FormsManagerChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Browsable(true)]
        [Category("Data")]
        [Description("Definition-driven form configuration used to materialize BeepBlock surfaces.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepFormsDefinitionEditor, TheTechIdea.Beep.Winform.Controls.Design.Server", typeof(UITypeEditor))]
        [DefaultValue(null)]
        public BeepFormsDefinition? Definition
        {
            get => _definition;
            set
            {
                _definition = value;
                if (!string.IsNullOrWhiteSpace(value?.FormName))
                {
                    _formName = value.FormName;
                }

                ApplyShellStateToUi();
                RebuildBlocksFromDefinition();
                if (_formsManager != null && _definition?.Blocks?.Count > 0)
                    _ = InitializeAsync();
            }
        }

        [Browsable(false)]
        public BeepViewState ViewState => _viewState;

        [Browsable(false)]
        public IReadOnlyList<IBeepBlockView> Blocks => _blocks;

        public event EventHandler? ActiveBlockChanged;
        public event EventHandler? FormsManagerChanged;
        public event EventHandler? ViewStateChanged;

        // Phase 7D — raised when all blocks have been bootstrapped (or failed)
        public event EventHandler<BeepBootstrapEventArgs>? BootstrapCompleted;

        public bool RegisterBlock(IBeepBlockView blockView)
        {
            if (blockView == null || string.IsNullOrWhiteSpace(blockView.BlockName))
            {
                return false;
            }

            if (_blocks.Any(x => string.Equals(x.BlockName, blockView.BlockName, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            _blocks.Add(blockView);
            blockView.Bind(this);

            if (blockView is Control control && _blocksHostPanel != null && !_blocksHostPanel.Controls.Contains(control))
            {
                control.Dock = DockStyle.Top;
                control.Margin = new Padding(0, 0, 0, 8);
                if (control.Height < 180)
                {
                    control.Height = 180;
                }

                _blocksHostPanel.Controls.Add(control);
                _blocksHostPanel.Controls.SetChildIndex(control, 0);
            }

            _managerAdapter.SyncBlock(blockView);
            RefreshUnitOfWorkEventSubscriptions();
            return true;
        }

        public bool UnregisterBlock(string blockName)
        {
            var blockView = _blocks.FirstOrDefault(x => string.Equals(x.BlockName, blockName, StringComparison.OrdinalIgnoreCase));
            if (blockView == null)
            {
                return false;
            }

            blockView.Unbind();
            _blocks.Remove(blockView);

            if (blockView is Control control && _blocksHostPanel != null && _blocksHostPanel.Controls.Contains(control))
            {
                _blocksHostPanel.Controls.Remove(control);
            }

            if (string.Equals(_viewState.ActiveBlockName, blockName, StringComparison.OrdinalIgnoreCase))
            {
                _viewState.ActiveBlockName = null;
                ActiveBlockChanged?.Invoke(this, EventArgs.Empty);
            }

            _definitionBlockNames.Remove(blockName);
            RefreshUnitOfWorkEventSubscriptions();

            return true;
        }

        public bool TrySetActiveBlock(string blockName)
        {
            if (!_blocks.Any(x => string.Equals(x.BlockName, blockName, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            if (string.Equals(_viewState.ActiveBlockName, blockName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            _viewState.ActiveBlockName = blockName;
            if (_formsManager != null)
            {
                _formsManager.CurrentBlockName = blockName;
            }

            UpdateMasterDetailShellContext(blockName);
            ActiveBlockChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public bool TrySetActiveItem(string blockName, string itemName)
        {
            if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(itemName)) return false;
            if (!TrySetActiveBlock(blockName)) return false;
            if (!IsItemRegistered(blockName, itemName)) return false;
            if (!string.Equals(_viewState.ActiveItemName, itemName, StringComparison.OrdinalIgnoreCase))
            {
                _viewState.ActiveItemName = itemName;
            }
            return true;
        }

        /// <summary>
        /// Raises the host's <see cref="TriggerExecuting"/> event from inside
        /// the declaring class so external callers (for example
        /// <see cref="Builtins.BeepBuiltinsHostAdapter"/>) can dispatch a
        /// synthetic built-in trigger through the same fan-out path used by
        /// the FormsManager-originated triggers.
        /// </summary>
        public void RaiseBuiltinTriggerExecuting(TriggerExecutingEventArgs args)
        {
            TriggerExecuting?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the host's <see cref="TriggerExecuted"/> event. See
        /// <see cref="RaiseBuiltinTriggerExecuting"/> for context.
        /// </summary>
        public void RaiseBuiltinTriggerExecuted(TriggerExecutedEventArgs args)
        {
            TriggerExecuted?.Invoke(this, args);
        }

        public void SyncFromManager()
        {
            _managerAdapter.Sync(_viewState);
            RefreshUnitOfWorkEventSubscriptions();

            foreach (var block in _blocks)
            {
                _managerAdapter.SyncBlock(block);
            }

            _viewState.ConnectionName = _dataConnection?.CurrentConnection?.ConnectionName ?? string.Empty;
            SyncErrorCount();
            SyncAggregateText();

            UpdateMasterDetailShellContext();
            ApplyShellStateToUi();
        }

        private void SyncErrorCount()
        {
            _viewState.ErrorCount = 0;
            if (_formsManager == null) return;

            try
            {
                int total = 0;
                foreach (var block in _blocks)
                {
                    if (!string.IsNullOrWhiteSpace(block.BlockName))
                        total += _formsManager.ErrorLog.GetErrorCount(block.BlockName);
                }
                _viewState.ErrorCount = total;
            }
            catch
            {
                _viewState.ErrorCount = 0;
            }
        }

        private void SyncAggregateText()
        {
            _viewState.AggregateText = string.Empty;
            if (_formsManager == null || string.IsNullOrWhiteSpace(_viewState.ActiveBlockName)) return;

            try
            {
                string blockName = _viewState.ActiveBlockName;
                if (!_formsManager.BlockExists(blockName)) return;

                int count = _formsManager.GetBlockCount(blockName);
                if (count == 0) { _viewState.AggregateText = "0 rec"; return; }

                var entity = _formsManager.GetBlock(blockName)?.EntityStructure;
                var fields = entity?.Fields;
                if (fields == null || fields.Count == 0) { _viewState.AggregateText = $"{count} rec"; return; }

                EntityField? firstNumeric = null;
                foreach (var f in fields)
                {
                    if (string.IsNullOrWhiteSpace(f.FieldName)) continue;
                    bool isNumeric = !string.IsNullOrWhiteSpace(f.Fieldtype)
                        && (f.Fieldtype.IndexOf("int", StringComparison.OrdinalIgnoreCase) >= 0
                         || f.Fieldtype.IndexOf("decimal", StringComparison.OrdinalIgnoreCase) >= 0
                         || f.Fieldtype.IndexOf("double", StringComparison.OrdinalIgnoreCase) >= 0
                         || f.Fieldtype.IndexOf("float", StringComparison.OrdinalIgnoreCase) >= 0
                         || f.Fieldtype.IndexOf("numeric", StringComparison.OrdinalIgnoreCase) >= 0
                         || f.Fieldtype.IndexOf("money", StringComparison.OrdinalIgnoreCase) >= 0);
                    if (!isNumeric) continue;

                    bool isLikelyAmount = f.FieldName.IndexOf("Amount", StringComparison.OrdinalIgnoreCase) >= 0
                        || f.FieldName.IndexOf("Total", StringComparison.OrdinalIgnoreCase) >= 0
                        || f.FieldName.IndexOf("Price", StringComparison.OrdinalIgnoreCase) >= 0
                        || f.FieldName.IndexOf("Sum", StringComparison.OrdinalIgnoreCase) >= 0
                        || f.FieldName.IndexOf("Value", StringComparison.OrdinalIgnoreCase) >= 0
                        || f.FieldName.IndexOf("Cost", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (isLikelyAmount)
                    {
                        firstNumeric = f;
                        break;
                    }
                    firstNumeric ??= f;
                }

                if (firstNumeric == null) { _viewState.AggregateText = $"{count} rec"; return; }

                string fieldName = firstNumeric.FieldName;
                decimal sum = _formsManager.GetBlockSum(blockName, fieldName);
                decimal avg = _formsManager.GetBlockAverage(blockName, fieldName);
                _viewState.AggregateText = $"{count} rec · Σ {sum:N0}";
            }
            catch
            {
                _viewState.AggregateText = string.Empty;
            }
        }

        /// <summary>
        /// Phase 7D — Asynchronously bootstraps all blocks defined in <see cref="Definition"/>
        /// by delegating to <see cref="IUnitofWorksManager.SetupBlockAsync"/> for each block.
        /// Updates <see cref="BeepViewState.BootstrapState"/> and raises
        /// <see cref="BootstrapCompleted"/> when done.
        /// </summary>
        public async Task<bool> InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (DesignMode || _formsManager == null
                || _definition?.Blocks == null || _definition.Blocks.Count == 0)
                return false;

            _viewState.BootstrapState = BootstrapState.Running;
            bool allOk = true;

            foreach (var blockDef in _definition.Blocks)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string blockName  = string.IsNullOrWhiteSpace(blockDef.ManagerBlockName)
                    ? blockDef.BlockName
                    : blockDef.ManagerBlockName;
                string connName   = blockDef.Entity?.ConnectionName ?? string.Empty;
                string entityName = blockDef.Entity?.EntityName     ?? string.Empty;

                if (string.IsNullOrWhiteSpace(connName) || string.IsNullOrWhiteSpace(entityName))
                    continue;

                bool ok = await _formsManager.SetupBlockAsync(
                    blockName, connName, entityName,
                    blockDef.Entity?.IsMasterBlock ?? false,
                    cancellationToken).ConfigureAwait(false);

                if (!ok) allOk = false;
            }

            _viewState.BootstrapState = allOk ? BootstrapState.Succeeded : BootstrapState.PartialSuccess;
            SyncFromManager();
            BootstrapCompleted?.Invoke(this, new BeepBootstrapEventArgs(
                allOk ? BootstrapState.Succeeded : BootstrapState.PartialSuccess));
            return allOk;
        }

        /// <summary>
        /// Phase 5C — Shows a WinForms logon dialog wrapped around the configured
        /// <see cref="BeepDataConnection"/>, and on success raises the
        /// When-New-Form-Instance trigger so forms can bootstrap their initial state.
        /// </summary>
        public async Task<BeepLogonContext> LogonAsync(BeepLogonRequest request)
        {
            if (request == null)
            {
                request = new BeepLogonRequest();
            }

            IBeepLogonDialog dialog = new BeepLogonDialog(_dataConnection);
            BeepLogonContext ctx = await dialog.PromptAsync(request).ConfigureAwait(true);

            if (ctx.IsSuccess)
            {
                var trigger = new TriggerDefinition(
                    TriggerType.WhenNewFormInstance,
                    TriggerScope.Form);
                var context = new TriggerContext
                {
                    Trigger = trigger,
                    TriggerType = TriggerType.WhenNewFormInstance,
                    Scope = TriggerScope.Form,
                    BlockName = string.Empty,
                    ItemName = string.Empty,
                    Parameters = new Dictionary<string, object?>
                    {
                        ["UserName"] = ctx.Request.UserName,
                        ["ConnectionName"] = ctx.Request.ConnectionName
                    }
                };

                var executing = new TriggerExecutingEventArgs
                {
                    Trigger = trigger,
                    Context = context
                };
                RaiseBuiltinTriggerExecuting(executing);

                if (!executing.Cancel)
                {
                    var start = DateTime.UtcNow;
                    var executed = new TriggerExecutedEventArgs
                    {
                        Trigger = trigger,
                        Context = context,
                        Result = TriggerResult.Success,
                        StartTime = start,
                        EndTime = DateTime.UtcNow
                    };
                    RaiseBuiltinTriggerExecuted(executed);
                }
            }

            return ctx;
        }

        private void InitializeComponent()
        {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw,
                true);

            BackColor = Color.Transparent;
        }
    }
}