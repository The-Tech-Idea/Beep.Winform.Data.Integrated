using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Base;
using TheTechIdea.Beep.Winform.Controls.FontManagement;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Helpers;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(BeepFormsQueryShelf))]
    [Category("Beep Controls")]
    [DisplayName("Beep Forms Query Shelf")]
    [Description("Standalone query-mode action shelf for BeepForms.")]
    [Designer("TheTechIdea.Beep.Winform.Controls.Design.Server.Designers.BeepFormsQueryShelfDesigner, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public sealed class BeepFormsQueryShelf : BeepFormsShelfBase
    {
        private readonly TableLayoutPanel _layoutRoot;
        private readonly BeepLabel _captionLabel;
        private readonly FlowLayoutPanel _commandPanel;
        private BeepButton? _enterQueryButton;
        private BeepButton? _executeQueryButton;
        private BeepButton? _exitQueryButton;
        private BeepFormsQueryShelfButtons _queryButtons = BeepFormsQueryShelfButtons.All;
        private FlowDirection _queryFlowDirection = FlowDirection.LeftToRight;
        private bool _showQueryContextCaption = true;
        private BeepFormsQueryShelfCaptionMode _queryCaptionMode = BeepFormsQueryShelfCaptionMode.TargetPlusMode;
        private bool _highlightQueryMode = true;

        public BeepFormsQueryShelf()
        {
            UseThemeColors = true;
            Padding = new Padding(0);
            Margin = new Padding(0);
            MinimumSize = new Size(0, BeepLayoutMetrics.ButtonToolbar.ScaleSize(this).Height + 8);
            Height = BeepLayoutMetrics.ButtonToolbar.ScaleSize(this).Height + 8;

            _layoutRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this),
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
                OnFormsHostChanged();
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
                    return;

                _queryCaptionMode = value;
                OnFormsHostChanged();
            }
        }

        [Browsable(true)]
        [Category("Query Shelf")]
        [Description("Highlight the query shelf with a distinct background when the active block is in query mode.")]
        [DefaultValue(true)]
        public bool HighlightQueryMode
        {
            get => _highlightQueryMode;
            set
            {
                if (_highlightQueryMode == value)
                    return;

                _highlightQueryMode = value;
                OnFormsHostChanged();
            }
        }

        private void RefreshQueryShelf()
        {
            _commandPanel.SuspendLayout();
            _commandPanel.Controls.Clear();
            _commandPanel.FlowDirection = QueryFlowDirection;

            AddButton(ref _enterQueryButton, BeepFormsQueryShelfButtons.EnterQuery, "Enter Query", EnterQueryButton_Click, 108);
            AddButton(ref _executeQueryButton, BeepFormsQueryShelfButtons.ExecuteQuery, "Execute Query", ExecuteQueryButton_Click, 118);
            AddButton(ref _exitQueryButton, BeepFormsQueryShelfButtons.ExitQuery, "Cancel Query", ExitQueryButton_Click, 112);

            _commandPanel.Visible = _commandPanel.Controls.Count > 0;
            _commandPanel.ResumeLayout(false);

            OnFormsHostChanged();
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

        protected override void OnFormsHostChanged()
        {
            base.OnFormsHostChanged();
            bool hasHost = FormsHost != null;
            bool hasActiveBlock = hasHost && !string.IsNullOrWhiteSpace(FormsHost!.ActiveBlockName);
            bool isQueryMode = hasActiveBlock && FormsHost!.ViewState.IsQueryMode;
            bool canQuery = hasActiveBlock && IsBlockQueryAllowed();

            string captionText = BeepFormsDisplayTextResolver.ResolveQueryTargetCaption(FormsHost, QueryCaptionMode);
            if (isQueryMode)
                captionText = string.Concat(captionText, "  —  Enter criteria and execute");

            _captionLabel.Text = captionText;
            _captionLabel.Visible = ShowQueryContextCaption;
            _captionLabel.ForeColor = ResolveCaptionColor();

            _layoutRoot.RowStyles[0].Height = _captionLabel.Visible ? 16f : 0f;
            Height = _captionLabel.Visible ? 54 : 36;

            if (_highlightQueryMode && isQueryMode)
            {
                BackColor = Color.FromArgb(220, 235, 252);
            }
            else
            {
                BackColor = Color.Transparent;
            }

            SetButtonState(_enterQueryButton, hasActiveBlock && !isQueryMode, QueryButtons.HasFlag(BeepFormsQueryShelfButtons.EnterQuery));
            SetButtonState(_executeQueryButton, canQuery, QueryButtons.HasFlag(BeepFormsQueryShelfButtons.ExecuteQuery));
            SetButtonState(_exitQueryButton, isQueryMode, QueryButtons.HasFlag(BeepFormsQueryShelfButtons.ExitQuery));
        }

        private bool IsBlockQueryAllowed()
        {
            if (FormsHost == null || string.IsNullOrWhiteSpace(FormsHost.ActiveBlockName)) return false;
            return FormsHost.TryGetBlockProperty(FormsHost.ActiveBlockName, "QueryAllowed", out object? val) && val is true;
        }

        private Color ResolveCaptionColor()
        {
            if (FormsHost == null)
            {
                return Color.DimGray;
            }

            if (string.IsNullOrWhiteSpace(FormsHost.ActiveBlockName))
            {
                return Color.DarkOrange;
            }

            if (FormsHost.ViewState.IsQueryMode)
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
            if (FormsHost == null) return;
            try { await FormsHost.EnterQueryAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsQueryShelf.EnterQuery] {ex.Message}"); }
        }

        private async void ExecuteQueryButton_Click(object? sender, EventArgs e)
        {
            if (FormsHost == null) return;
            try { await FormsHost.ExecuteQueryAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsQueryShelf.ExecuteQuery] {ex.Message}"); }
        }

        private async void ExitQueryButton_Click(object? sender, EventArgs e)
        {
            if (FormsHost == null) return;
            try { await FormsHost.ExitQueryAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsQueryShelf.ExitQuery] {ex.Message}"); }
        }
    }
}