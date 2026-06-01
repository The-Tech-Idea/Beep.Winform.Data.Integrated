using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Services;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(BeepForms))]
    [Category("Beep Controls")]
    [DisplayName("Beep Forms")]
    [Description("Non-visual BeepForms coordinator that hosts block views over a FormsManager instance.")]
    [Designer("TheTechIdea.Beep.Winform.Controls.Design.Server.Designers.BeepFormsHostDesigner, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public partial class BeepForms : Control, IBeepFormsHost
    {
        private readonly List<IBeepBlockView> _blocks = new();
        private readonly BeepFormsViewState _viewState = new();
        private readonly BeepFormsManagerAdapter _managerAdapter = new();
        private IUnitofWorksManager? _formsManager;
        private BeepFormsDefinition? _definition;
        private bool _autoCreateBlocksFromDefinition = true;
        private string _formName = string.Empty;

        public BeepForms()
        {
            InitializeComponent();
            InitializeLayout();
            CommandRouter = new BeepFormsCommandRouter();
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
                _commandRouter.FormsManager = _formsManager;
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
        public BeepFormsViewState ViewState => _viewState;

        [Browsable(false)]
        public IReadOnlyList<IBeepBlockView> Blocks => _blocks;

        public event EventHandler? ActiveBlockChanged;
        public event EventHandler? FormsManagerChanged;
        public event EventHandler? ViewStateChanged;

        // Phase 7D — raised when all blocks have been bootstrapped (or failed)
        public event EventHandler<BeepFormsBootstrapEventArgs>? BootstrapCompleted;

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

        public void SyncFromManager()
        {
            _managerAdapter.Sync(_viewState);
            RefreshUnitOfWorkEventSubscriptions();

            foreach (var block in _blocks)
            {
                _managerAdapter.SyncBlock(block);
            }

            UpdateMasterDetailShellContext();
            ApplyShellStateToUi();
        }

        /// <summary>
        /// Phase 7D — Asynchronously bootstraps all blocks defined in <see cref="Definition"/>
        /// by delegating to <see cref="IUnitofWorksManager.SetupBlockAsync"/> for each block.
        /// Updates <see cref="BeepFormsViewState.BootstrapState"/> and raises
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
            BootstrapCompleted?.Invoke(this, new BeepFormsBootstrapEventArgs(
                allOk ? BootstrapState.Succeeded : BootstrapState.PartialSuccess));
            return allOk;
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