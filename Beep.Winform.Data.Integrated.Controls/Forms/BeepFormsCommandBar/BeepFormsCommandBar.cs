using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Base;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Helpers;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

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
        private BeepFormsCommandBarButtons _commandButtons = BeepFormsCommandBarButtons.All;
        private FlowDirection _commandFlowDirection = FlowDirection.LeftToRight;

        public BeepFormsCommandBar()
        {
            UseThemeColors = true;
            Padding = new Padding(0);
            Margin = new Padding(0);
            MinimumSize = new Size(0, 36);
            Height = 36;

            _commandPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(8, 4, 8, 4),
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

            SetButtonState(_syncButton, hasHost);
        }

        private static void SetButtonState(BeepButton? button, bool enabled)
        {
            if (button == null)
            {
                return;
            }

            button.Enabled = enabled;
            button.Visible = true;
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

        private void SyncButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost == null)
            {
                return;
            }

            _formsHost.SyncFromManager();
            _formsHost.ShowInfo("Fresh-start host synchronized from FormsManager.");
        }
    }
}