using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Base;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Helpers;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;
using TheTechIdea.Beep.Winform.Controls.ListBoxs;
using TheTechIdea.Beep.Winform.Controls.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(BeepFormsToolbar))]
    [Category("Beep Controls")]
    [DisplayName("Beep Forms Toolbar")]
    [Description("Standalone toolbar surface for BeepForms savepoint and alert actions.")]
    [Designer("TheTechIdea.Beep.Winform.Controls.Design.Server.Designers.BeepFormsToolbarDesigner, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public partial class BeepFormsToolbar : BaseControl
    {
        private readonly FlowLayoutPanel _commandPanel;
        private BeepForms? _formsHost;
        private bool _autoBindFormsHost = true;
        private BeepButton? _savepointActionsButton;
        private BeepButton? _alertPresetsButton;
        private BeepFormsShellToolbarButtons _shellToolbarButtons = BeepFormsShellToolbarButtons.All;
        private BeepFormsSavepointToolbarActions _savepointToolbarActions = BeepFormsSavepointToolbarActions.All;
        private BeepFormsAlertToolbarPresets _alertToolbarPresets = BeepFormsAlertToolbarPresets.All;
        private BeepFormsShellToolbarOrder _shellToolbarOrder = BeepFormsShellToolbarOrder.SavepointsFirst;
        private FlowDirection _shellToolbarFlowDirection = FlowDirection.LeftToRight;
        private string _savepointToolbarCaption = "Savepoints";
        private string _alertToolbarCaption = "Alerts";

        public BeepFormsToolbar()
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
                Margin = new Padding(0)
            };

            Controls.Add(_commandPanel);
            RefreshToolbarConfiguration();
        }

        [Browsable(true)]
        [Category("Behavior")]
        [Description("Optional BeepForms coordinator surfaced by this toolbar.")]
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
        [Category("Toolbar")]
        [Description("Select which top-level toolbar buttons are shown on BeepFormsToolbar.")]
        [DefaultValue(BeepFormsShellToolbarButtons.All)]
        public BeepFormsShellToolbarButtons ShellToolbarButtons
        {
            get => _shellToolbarButtons;
            set
            {
                if (_shellToolbarButtons == value)
                {
                    return;
                }

                _shellToolbarButtons = value;
                RefreshToolbarConfiguration();
            }
        }

        [Browsable(true)]
        [Category("Toolbar")]
        [Description("Select which savepoint actions are available from the toolbar popup.")]
        [DefaultValue(BeepFormsSavepointToolbarActions.All)]
        public BeepFormsSavepointToolbarActions SavepointToolbarActions
        {
            get => _savepointToolbarActions;
            set
            {
                if (_savepointToolbarActions == value)
                {
                    return;
                }

                _savepointToolbarActions = value;
                RefreshToolbarConfiguration();
            }
        }

        [Browsable(true)]
        [Category("Toolbar")]
        [Description("Select which alert presets are available from the toolbar popup.")]
        [DefaultValue(BeepFormsAlertToolbarPresets.All)]
        public BeepFormsAlertToolbarPresets AlertToolbarPresets
        {
            get => _alertToolbarPresets;
            set
            {
                if (_alertToolbarPresets == value)
                {
                    return;
                }

                _alertToolbarPresets = value;
                RefreshToolbarConfiguration();
            }
        }

        [Browsable(true)]
        [Category("Toolbar")]
        [Description("Select which top-level toolbar button appears first.")]
        [DefaultValue(BeepFormsShellToolbarOrder.SavepointsFirst)]
        public BeepFormsShellToolbarOrder ShellToolbarOrder
        {
            get => _shellToolbarOrder;
            set
            {
                if (_shellToolbarOrder == value)
                {
                    return;
                }

                _shellToolbarOrder = value;
                RefreshToolbarConfiguration();
            }
        }

        [Browsable(true)]
        [Category("Toolbar")]
        [Description("Controls the flow direction used by the toolbar button row.")]
        [DefaultValue(FlowDirection.LeftToRight)]
        public FlowDirection ShellToolbarFlowDirection
        {
            get => _shellToolbarFlowDirection;
            set
            {
                if (_shellToolbarFlowDirection == value)
                {
                    return;
                }

                _shellToolbarFlowDirection = value;
                RefreshToolbarConfiguration();
            }
        }

        [Browsable(true)]
        [Category("Toolbar")]
        [Description("Text shown on the savepoint toolbar button.")]
        [DefaultValue("Savepoints")]
        public string SavepointToolbarCaption
        {
            get => _savepointToolbarCaption;
            set
            {
                string normalized = NormalizeToolbarCaption(value, "Savepoints");
                if (string.Equals(_savepointToolbarCaption, normalized, StringComparison.Ordinal))
                {
                    return;
                }

                _savepointToolbarCaption = normalized;
                RefreshToolbarConfiguration();
            }
        }

        [Browsable(true)]
        [Category("Toolbar")]
        [Description("Text shown on the alert toolbar button.")]
        [DefaultValue("Alerts")]
        public string AlertToolbarCaption
        {
            get => _alertToolbarCaption;
            set
            {
                string normalized = NormalizeToolbarCaption(value, "Alerts");
                if (string.Equals(_alertToolbarCaption, normalized, StringComparison.Ordinal))
                {
                    return;
                }

                _alertToolbarCaption = normalized;
                RefreshToolbarConfiguration();
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

        private static string NormalizeToolbarCaption(string? value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value)
                ? fallback
                : value.Trim();
        }

        private void RefreshToolbarConfiguration()
        {
            InitializeCommandStrip();
        }

        private void InitializeCommandStrip()
        {
            _commandPanel.SuspendLayout();
            _commandPanel.Controls.Clear();
            _commandPanel.FlowDirection = ShellToolbarFlowDirection;

            if (ShellToolbarOrder == BeepFormsShellToolbarOrder.SavepointsFirst)
            {
                AddSavepointToolbarButton();
                AddAlertToolbarButton();
            }
            else
            {
                AddAlertToolbarButton();
                AddSavepointToolbarButton();
            }

            _commandPanel.Visible = _commandPanel.Controls.Count > 0;
            _commandPanel.ResumeLayout(false);

            UpdateCommandStripState();
        }

        private void AddSavepointToolbarButton()
        {
            if (!ShellToolbarButtons.HasFlag(BeepFormsShellToolbarButtons.Savepoints))
            {
                _savepointActionsButton = null;
                return;
            }

            BindingList<SimpleItem> savepointItems = BuildSavepointToolbarItems();
            if (savepointItems.Count == 0)
            {
                _savepointActionsButton = null;
                return;
            }

            string caption = string.IsNullOrWhiteSpace(SavepointToolbarCaption) ? "Savepoints" : SavepointToolbarCaption;
            _savepointActionsButton = CreateToolbarPopupButton(caption, GetToolbarPopupButtonWidth(caption, 128), savepointItems);
            _savepointActionsButton.SelectedItemChanged += SavepointActionsButton_SelectedItemChanged;
            _commandPanel.Controls.Add(_savepointActionsButton);
        }

        private void AddAlertToolbarButton()
        {
            if (!ShellToolbarButtons.HasFlag(BeepFormsShellToolbarButtons.Alerts))
            {
                _alertPresetsButton = null;
                return;
            }

            BindingList<SimpleItem> alertItems = BuildAlertPresetItems();
            if (alertItems.Count == 0)
            {
                _alertPresetsButton = null;
                return;
            }

            string caption = string.IsNullOrWhiteSpace(AlertToolbarCaption) ? "Alerts" : AlertToolbarCaption;
            _alertPresetsButton = CreateToolbarPopupButton(caption, GetToolbarPopupButtonWidth(caption, 112), alertItems);
            _alertPresetsButton.SelectedItemChanged += AlertPresetsButton_SelectedItemChanged;
            _commandPanel.Controls.Add(_alertPresetsButton);
        }

        private BeepButton CreateToolbarPopupButton(string text, int width, BindingList<SimpleItem> items)
        {
            return new BeepButton
            {
                Width = width,
                Height = 28,
                Margin = new Padding(0, 0, 8, 0),
                Text = text,
                Theme = Theme,
                ShowShadow = false,
                PopupMode = true,
                ListItems = items,
                UseThemeColors = true
            };
        }

        private int GetToolbarPopupButtonWidth(string text, int minimumWidth)
        {
            int measuredWidth = TextRenderer.MeasureText(text ?? string.Empty, Font).Width + 44;
            return Math.Max(minimumWidth, measuredWidth);
        }

        private BindingList<SimpleItem> BuildSavepointToolbarItems()
        {
            var items = new BindingList<SimpleItem>();

            if (SavepointToolbarActions.HasFlag(BeepFormsSavepointToolbarActions.Capture))
            {
                items.Add(new SimpleItem { Text = "Capture Savepoint", Name = "capture", Value = "capture", Item = "capture" });
            }

            if (SavepointToolbarActions.HasFlag(BeepFormsSavepointToolbarActions.List))
            {
                items.Add(new SimpleItem { Text = "List Savepoints", Name = "list", Value = "list", Item = "list" });
            }

            if (SavepointToolbarActions.HasFlag(BeepFormsSavepointToolbarActions.Rollback))
            {
                items.Add(new SimpleItem { Text = "Rollback...", Name = "rollback", Value = "rollback", Item = "rollback" });
            }

            if (SavepointToolbarActions.HasFlag(BeepFormsSavepointToolbarActions.Release))
            {
                items.Add(new SimpleItem { Text = "Release...", Name = "release", Value = "release", Item = "release" });
            }

            if (SavepointToolbarActions.HasFlag(BeepFormsSavepointToolbarActions.ReleaseAll))
            {
                items.Add(new SimpleItem { Text = "Release All", Name = "release_all", Value = "release_all", Item = "release_all" });
            }

            return items;
        }

        private BindingList<SimpleItem> BuildAlertPresetItems()
        {
            var items = new BindingList<SimpleItem>();

            if (AlertToolbarPresets.HasFlag(BeepFormsAlertToolbarPresets.Info))
            {
                items.Add(new SimpleItem { Text = "Info Status", Name = "info", Value = "info", Item = "info" });
            }

            if (AlertToolbarPresets.HasFlag(BeepFormsAlertToolbarPresets.Caution))
            {
                items.Add(new SimpleItem { Text = "Caution Review", Name = "caution", Value = "caution", Item = "caution" });
            }

            if (AlertToolbarPresets.HasFlag(BeepFormsAlertToolbarPresets.Stop))
            {
                items.Add(new SimpleItem { Text = "Stop Attention", Name = "stop", Value = "stop", Item = "stop" });
            }

            if (AlertToolbarPresets.HasFlag(BeepFormsAlertToolbarPresets.Question))
            {
                items.Add(new SimpleItem { Text = "Confirm Continue", Name = "question", Value = "question", Item = "question" });
            }

            return items;
        }

        private void UpdateCommandStripState()
        {
            string blockName = ResolveWorkflowTargetBlockName();
            bool hasBlockContext = _formsHost?.FormsManager != null &&
                                   !string.IsNullOrWhiteSpace(blockName) &&
                                   _formsHost.FormsManager.BlockExists(blockName);

            if (_savepointActionsButton != null)
            {
                _savepointActionsButton.Enabled = hasBlockContext;
            }

            if (_alertPresetsButton != null)
            {
                _alertPresetsButton.Enabled = _formsHost?.FormsManager?.AlertProvider != null;
            }
        }

        private string ResolveWorkflowTargetBlockName(string? blockName = null)
        {
            return _formsHost?.ResolveToolbarWorkflowBlockName(blockName) ?? string.Empty;
        }
    }
}