using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Base;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Helpers;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(BeepFormsPersistenceShelf))]
    [Category("Beep Controls")]
    [DisplayName("Beep Forms Persistence Shelf")]
    [Description("Standalone persistence action shelf for BeepForms commit and rollback commands.")]
    [Designer("TheTechIdea.Beep.Winform.Controls.Design.Server.Designers.BeepFormsPersistenceShelfDesigner, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public sealed class BeepFormsPersistenceShelf : BaseControl
    {
        private readonly FlowLayoutPanel _commandPanel;
        private BeepForms? _formsHost;
        private bool _autoBindFormsHost = true;
        private BeepButton? _commitButton;
        private BeepButton? _rollbackButton;
        private BeepButton? _batchCommitButton;
        private BeepFormsPersistenceShelfButtons _persistenceButtons = BeepFormsPersistenceShelfButtons.All;
        private FlowDirection _persistenceFlowDirection = FlowDirection.LeftToRight;

        public BeepFormsPersistenceShelf()
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
            RefreshPersistenceShelf();
        }

        [Browsable(true)]
        [Category("Behavior")]
        [Description("Optional BeepForms coordinator surfaced by this persistence shelf.")]
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
                UpdatePersistenceShelfState();
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
        [Category("Persistence Shelf")]
        [Description("Select which persistence buttons are shown.")]
        [DefaultValue(BeepFormsPersistenceShelfButtons.All)]
        public BeepFormsPersistenceShelfButtons PersistenceButtons
        {
            get => _persistenceButtons;
            set
            {
                if (_persistenceButtons == value)
                {
                    return;
                }

                _persistenceButtons = value;
                RefreshPersistenceShelf();
            }
        }

        [Browsable(true)]
        [Category("Persistence Shelf")]
        [Description("Controls the flow direction used by the persistence shelf button row.")]
        [DefaultValue(FlowDirection.LeftToRight)]
        public FlowDirection PersistenceFlowDirection
        {
            get => _persistenceFlowDirection;
            set
            {
                if (_persistenceFlowDirection == value)
                {
                    return;
                }

                _persistenceFlowDirection = value;
                RefreshPersistenceShelf();
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
            UpdatePersistenceShelfState();
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

        private void RefreshPersistenceShelf()
        {
            _commandPanel.SuspendLayout();
            _commandPanel.Controls.Clear();
            _commandPanel.FlowDirection = PersistenceFlowDirection;

            AddButton(ref _commitButton, BeepFormsPersistenceShelfButtons.Commit, "Commit", CommitButton_Click, 90);
            AddButton(ref _rollbackButton, BeepFormsPersistenceShelfButtons.Rollback, "Rollback", RollbackButton_Click, 96);
            AddButton(ref _batchCommitButton, BeepFormsPersistenceShelfButtons.BatchCommit, "Batch", BatchCommitButton_Click, 72);

            _commandPanel.Visible = _commandPanel.Controls.Count > 0;
            _commandPanel.ResumeLayout(false);

            UpdatePersistenceShelfState();
        }

        private void AddButton(ref BeepButton? field, BeepFormsPersistenceShelfButtons flag, string caption, EventHandler clickHandler, int minimumWidth)
        {
            if (!PersistenceButtons.HasFlag(flag))
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

        private void UpdatePersistenceShelfState()
        {
            bool isDirty = _formsHost != null && _formsHost.ViewState.IsDirty;

            SetButtonState(_commitButton, isDirty, PersistenceButtons.HasFlag(BeepFormsPersistenceShelfButtons.Commit));
            SetButtonState(_rollbackButton, isDirty, PersistenceButtons.HasFlag(BeepFormsPersistenceShelfButtons.Rollback));
            SetButtonState(_batchCommitButton, isDirty, PersistenceButtons.HasFlag(BeepFormsPersistenceShelfButtons.BatchCommit));
        }

        private static void SetButtonState(BeepButton? button, bool enabled, bool visible)
        {
            if (button == null)
            {
                return;
            }

            button.Enabled = enabled;
            button.Visible = visible;
        }

        private async void CommitButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null) return;
            try { await _formsHost.CommitFormAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsPersistenceShelf.Commit] {ex.Message}"); }
        }

        private async void RollbackButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null) return;
            try { await _formsHost.RollbackFormAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsPersistenceShelf.Rollback] {ex.Message}"); }
        }

        private async void BatchCommitButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost?.FormsManager == null) return;
            try
            {
                bool confirmed = await _formsHost.ConfirmAsync("Batch Commit",
                    "Commit all blocks in a single transaction?").ConfigureAwait(true);
                if (!confirmed) return;

                _formsHost.ShowInfo("Running batch commit…");
                var result = await _formsHost.FormsManager.CommitFormBatchAsync().ConfigureAwait(true);
                _formsHost.SyncFromManager();
                _formsHost.ShowSuccess("Batch commit completed.");
            }
            catch (Exception ex)
            {
                _formsHost?.ShowError($"Batch commit failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[BeepFormsPersistenceShelf.BatchCommit] {ex.Message}");
            }
        }
    }
}