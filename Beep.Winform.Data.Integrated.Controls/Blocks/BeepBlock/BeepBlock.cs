using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Controls.Base;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Services;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Services.Presenters;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks
{
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(BeepBlock))]
    [Category("Beep Controls")]
    [DisplayName("Beep Block")]
    [Description("Block-level UI surface bound to one logical FormsManager block.")]
    [Designer("TheTechIdea.Beep.Winform.Controls.Design.Server.Designers.BeepBlockDesigner, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public partial class BeepBlock : BaseControl, IBeepBlockView
    {
        private readonly BeepBlockViewState _viewState = new();
        private BeepBlockPresenterRegistry _presenterRegistry = new();
        private string _blockName = string.Empty;
        private BeepBlockDefinition? _definition;
        private IBeepFormsHost? _formsHost;

        public BeepBlock()
        {
            InitializeComponent();
            InitializeLayout();
            InitializeRecordBinding();
            PresenterRegistry = new BeepBlockPresenterRegistry();
            PresenterRegistry.RegisterDefaults();
        }

        [Browsable(true)]
        [Category("Data")]
        [Description("Logical UI block name.")]
        [TypeConverter("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepFormsBlockNameTypeConverter, TheTechIdea.Beep.Winform.Controls.Design.Server")]
        public string BlockName
        {
            get => _blockName;
            set => _blockName = value?.Trim() ?? string.Empty;
        }

        [Browsable(false)]
        public string ManagerBlockName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_definition?.ManagerBlockName))
                {
                    return _definition.ManagerBlockName;
                }

                return _blockName;
            }
        }

        [Browsable(false)]
        public bool IsBound => _formsHost != null;

        [Browsable(false)]
        public IBeepFormsHost? FormsHost => _formsHost;

        [Browsable(true)]
        [Category("Data")]
        [Description("Definition-driven block configuration used for presentation, fields, and editor options.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepBlockDefinitionEditor, TheTechIdea.Beep.Winform.Controls.Design.Server", typeof(UITypeEditor))]
        [DefaultValue(null)]
        public BeepBlockDefinition? Definition
        {
            get => _definition;
            set
            {
                bool hadExplicitFieldDefinitions = BeepBlockFieldDefinitionStateHelper.HasExplicitFieldDefinitions(_definition);
                BeepBlockFieldDefinitionStateHelper.UpdateExplicitFieldState(value, hadExplicitFieldDefinitions);
                _definition = value;
                if (!string.IsNullOrWhiteSpace(value?.BlockName))
                {
                    _blockName = value.BlockName;
                }

                _viewState.ManagerBlockName = ManagerBlockName;
                RefreshRuntimeDefinition(TryGetManagerBlockInfo());
                ReconcileDesignerGeneratedBindings(EffectiveDefinition);
                RefreshPresentation();
                NotifyViewStateChanged();
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Data")]
        [Description("Typed UI-side entity metadata for this block, including connection, entity, and field structure.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("TheTechIdea.Beep.Winform.Controls.Design.Server.Editors.BeepBlockEntityDefinitionEditor, TheTechIdea.Beep.Winform.Controls.Design.Server", typeof(UITypeEditor))]
        public BeepBlockEntityDefinition Entity
        {
            get
            {
                EnsureDefinition();
                return _definition!.Entity;
            }
            set
            {
                BeepBlockDefinition definition = (_definition ?? CreateDefinitionShell()).Clone();
                definition.Entity = value?.Clone() ?? new BeepBlockEntityDefinition();
                Definition = definition;
            }
        }

        [Browsable(false)]
        public BeepBlockEntityDefinition ResolvedEntity
            => (_runtimeDefinition?.Entity ?? _definition?.Entity ?? new BeepBlockEntityDefinition()).Clone();

        [Browsable(false)]
        public BeepBlockViewState ViewState => _viewState;

        [Browsable(false)]
        public BeepBlockPresenterRegistry PresenterRegistry
        {
            get => _presenterRegistry;
            set => _presenterRegistry = value ?? new BeepBlockPresenterRegistry();
        }

        [Browsable(false)]
        public event EventHandler? ViewStateChanged;

        public void Bind(IBeepFormsHost formsHost)
        {
            DetachFromFormsHost(_formsHost);
            _formsHost = formsHost ?? throw new ArgumentNullException(nameof(formsHost));
            AttachToFormsHost(_formsHost);
            _viewState.IsBound = true;
            _viewState.ManagerBlockName = ManagerBlockName;
            SyncFromManager();
            RefreshPresentation();
        }

        public void Unbind()
        {
            DetachFromFormsHost(_formsHost);
            _formsHost = null;
            ResetRecordBinding();
            SyncValidationSubscriptions(null);
            ResetValidationState();
            ResetTriggerState();
            _viewState.IsBound = false;
            _viewState.IsDirty = false;
            _viewState.IsQueryMode = false;
            _viewState.CurrentRecordIndex = -1;
            _viewState.RecordCount = 0;
            SyncGridFromManager(null);
            RefreshRuntimeDefinition(null);
            ReconcileDesignerGeneratedBindings(EffectiveDefinition);
            RefreshPresentation();
            NotifyViewStateChanged();
        }

        public void ApplyDefinition(BeepBlockDefinition definition)
        {
            Definition = definition;
        }

        public void SyncFromManager()
        {
            if (_formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName) || !_formsHost.IsBlockRegistered(ManagerBlockName))
            {
                SyncValidationSubscriptions(null);
                ResetRecordBinding();
                ResetValidationState();
                ResetTriggerState();
                _viewState.IsDirty = false;
                _viewState.IsQueryMode = false;
                _viewState.CurrentRecordIndex = -1;
                _viewState.RecordCount = 0;
                SyncGridFromManager(null);
                RefreshRuntimeDefinition(null);
                ReconcileDesignerGeneratedBindings(EffectiveDefinition);
                RefreshPresentation();
                NotifyViewStateChanged();
                return;
            }

            var blockInfo = _formsHost.GetBlockInfo(ManagerBlockName);
            var unitOfWork = _formsHost.GetBlockUnitOfWork(ManagerBlockName);

            SyncValidationSubscriptions(_formsHost?.FormsManager);
            RefreshRuntimeDefinition(blockInfo);
            ReconcileDesignerGeneratedBindings(EffectiveDefinition);
            SyncRecordBinding(unitOfWork);

            _viewState.IsDirty = unitOfWork?.IsDirty ?? false;
            _viewState.Mode = blockInfo?.Mode ?? DataBlockMode.Query;
            _viewState.IsQueryMode = _viewState.Mode == DataBlockMode.Query;

            SyncGridFromManager(_formsHost?.FormsManager);
            UpdateRecordViewState(unitOfWork);
            RefreshTriggerState();
            RefreshPresentation();
            NotifyViewStateChanged();
        }

        public BeepBlockEntityDefinition? CreateEntitySnapshotFromManager()
        {
            var blockInfo = TryGetManagerBlockInfo();
            var entity = ResolveEntityDefinition(blockInfo, _definition?.Entity);
            if (string.IsNullOrWhiteSpace(entity.EntityName) && entity.Fields.Count == 0)
            {
                return null;
            }

            return entity;
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);

            if (_formsHost != null && !string.IsNullOrWhiteSpace(BlockName))
            {
                _formsHost.TrySetActiveBlock(BlockName);
            }
        }

        private void InitializeComponent()
        {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            AutoScroll = true;
            BackColor = Color.Transparent;
        }

        private void EnsureDefinition()
        {
            _definition ??= CreateDefinitionShell();
        }

        private BeepBlockDefinition CreateDefinitionShell()
        {
            string resolvedBlockName = string.IsNullOrWhiteSpace(_blockName) ? Name : _blockName;
            return new BeepBlockDefinition
            {
                BlockName = resolvedBlockName,
                Caption = resolvedBlockName,
                Entity = new BeepBlockEntityDefinition()
            };
        }

        private void NotifyViewStateChanged()
        {
            ViewStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}