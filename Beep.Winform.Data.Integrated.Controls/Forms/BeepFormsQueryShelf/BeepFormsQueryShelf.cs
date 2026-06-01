using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Base;
using TheTechIdea.Beep.Winform.Controls.FontManagement;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Helpers;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(BeepFormsQueryShelf))]
    [Category("Beep Controls")]
    [DisplayName("Beep Forms Query Shelf")]
    [Description("Standalone query-mode action shelf for BeepForms.")]
    [Designer("TheTechIdea.Beep.Winform.Controls.Design.Server.Designers.BeepFormsQueryShelfDesigner, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public sealed class BeepFormsQueryShelf : BaseControl
    {
        private readonly TableLayoutPanel _layoutRoot;
        private readonly BeepLabel _captionLabel;
        private readonly FlowLayoutPanel _commandPanel;
        private BeepForms? _formsHost;
        private bool _autoBindFormsHost = true;
        private BeepButton? _enterQueryButton;
        private BeepButton? _executeQueryButton;
        private BeepFormsQueryShelfButtons _queryButtons = BeepFormsQueryShelfButtons.All;
        private FlowDirection _queryFlowDirection = FlowDirection.LeftToRight;
        private bool _showQueryContextCaption = true;
        private BeepFormsQueryShelfCaptionMode _queryCaptionMode = BeepFormsQueryShelfCaptionMode.TargetPlusMode;

        public BeepFormsQueryShelf()
        {
            UseThemeColors = true;
            Padding = new Padding(0);
            Margin = new Padding(0);
            MinimumSize = new Size(0, 54);
            Height = 54;

            _layoutRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(8, 4, 8, 4),
                Margin = new Padding(0)
            };
            _layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            _layoutRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 16f));
            _layoutRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));

            _captionLabel = new BeepLabel
            {
                Dock = DockStyle.Fill,
                Font = BeepFontManager.StatusBarFont,
                TextAlign = ContentAlignment.MiddleLeft,
                UseThemeColors = true,
                Margin = new Padding(0)
            };

            _commandPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0),
                Margin = new Padding(0),
                FlowDirection = FlowDirection.LeftToRight
            };

            _layoutRoot.Controls.Add(_captionLabel, 0, 0);
            _layoutRoot.Controls.Add(_commandPanel, 0, 1);
            Controls.Add(_layoutRoot);
            RefreshQueryShelf();
        }

        [Browsable(true)]
        [Category("Behavior")]
        [Description("Optional BeepForms coordinator surfaced by this query shelf.")]
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
                UpdateQueryShelfState();
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
        [Category("Query Shelf")]
        [Description("Select which query-mode buttons are shown.")]
        [DefaultValue(BeepFormsQueryShelfButtons.All)]
        public BeepFormsQueryShelfButtons QueryButtons
        {
            get => _queryButtons;
            set
            {
                if (_queryButtons == value)
                {
                    return;
                }

                _queryButtons = value;
                RefreshQueryShelf();
            }
        }

        [Browsable(true)]
        [Category("Query Shelf")]
        [Description("Controls the flow direction used by the query shelf button row.")]
        [DefaultValue(FlowDirection.LeftToRight)]
        public FlowDirection QueryFlowDirection
        {
            get => _queryFlowDirection;
            set
            {
                if (_queryFlowDirection == value)
                {
                    return;
                }

                _queryFlowDirection = value;
                RefreshQueryShelf();
            }
        }

        [Browsable(true)]
        [Category("Query Shelf")]
        [Description("Show a caption that indicates which block the query shelf currently targets.")]
        [DefaultValue(true)]
        public bool ShowQueryContextCaption
        {
            get => _showQueryContextCaption;
            set
            {
                if (_showQueryContextCaption == value)
                {
                    return;
                }

                _showQueryContextCaption = value;
                UpdateQueryShelfState();
            }
        }

        [Browsable(true)]
        [Category("Query Shelf")]
        [Description("Controls how much host context the query shelf caption displays when visible.")]
        [DefaultValue(BeepFormsQueryShelfCaptionMode.TargetPlusMode)]
        public BeepFormsQueryShelfCaptionMode QueryCaptionMode
        {
            get => _queryCaptionMode;
            set
            {
                if (_queryCaptionMode == value)
                {
                    return;
                }

                _queryCaptionMode = value;
                UpdateQueryShelfState();
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
            UpdateQueryShelfState();
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

        private void RefreshQueryShelf()
        {
            _commandPanel.SuspendLayout();
            _commandPanel.Controls.Clear();
            _commandPanel.FlowDirection = QueryFlowDirection;

            AddButton(ref _enterQueryButton, BeepFormsQueryShelfButtons.EnterQuery, "Enter Query", EnterQueryButton_Click, 108);
            AddButton(ref _executeQueryButton, BeepFormsQueryShelfButtons.ExecuteQuery, "Execute Query", ExecuteQueryButton_Click, 118);

            _commandPanel.Visible = _commandPanel.Controls.Count > 0;
            _commandPanel.ResumeLayout(false);

            UpdateQueryShelfState();
        }

        private void AddButton(ref BeepButton? field, BeepFormsQueryShelfButtons flag, string caption, EventHandler clickHandler, int minimumWidth)
        {
            if (!QueryButtons.HasFlag(flag))
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

        private void UpdateQueryShelfState()
        {
            bool hasHost = _formsHost != null;
            bool hasActiveBlock = hasHost && !string.IsNullOrWhiteSpace(_formsHost!.ActiveBlockName);
            bool isQueryMode = hasActiveBlock && _formsHost!.ViewState.IsQueryMode;

            string captionText = BeepFormsDisplayTextResolver.ResolveQueryTargetCaption(_formsHost, QueryCaptionMode);
            _captionLabel.Text = captionText;
            _captionLabel.Visible = ShowQueryContextCaption;
            _captionLabel.ForeColor = ResolveCaptionColor();

            _layoutRoot.RowStyles[0].Height = _captionLabel.Visible ? 16f : 0f;
            Height = _captionLabel.Visible ? 54 : 36;

            SetButtonState(_enterQueryButton, hasActiveBlock, QueryButtons.HasFlag(BeepFormsQueryShelfButtons.EnterQuery));
            SetButtonState(_executeQueryButton, isQueryMode, QueryButtons.HasFlag(BeepFormsQueryShelfButtons.ExecuteQuery));
        }

        private Color ResolveCaptionColor()
        {
            if (_formsHost == null)
            {
                return Color.DimGray;
            }

            if (string.IsNullOrWhiteSpace(_formsHost.ActiveBlockName))
            {
                return Color.DarkOrange;
            }

            if (_formsHost.ViewState.IsQueryMode)
            {
                return Color.SteelBlue;
            }

            return Color.ForestGreen;
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

        private async void EnterQueryButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost != null)
            {
                await _formsHost.EnterQueryAsync().ConfigureAwait(true);
            }
        }

        private async void ExecuteQueryButton_Click(object? sender, EventArgs e)
        {
            if (_formsHost != null)
            {
                await _formsHost.ExecuteQueryAsync().ConfigureAwait(true);
            }
        }
    }
}