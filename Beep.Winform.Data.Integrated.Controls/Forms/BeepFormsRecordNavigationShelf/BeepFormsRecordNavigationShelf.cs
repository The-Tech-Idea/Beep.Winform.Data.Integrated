using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Base;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Helpers;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(BeepFormsRecordNavigationShelf))]
    [Category("Beep Controls")]
    [DisplayName("Beep Forms Record Navigation Shelf")]
    [Description("Standalone record navigation shelf for BeepForms with First/Previous/Next/Last and position indicator.")]
    [Designer("TheTechIdea.Beep.Winform.Controls.Design.Server.Designers.BeepFormsRecordNavigationShelfDesigner, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public sealed class BeepFormsRecordNavigationShelf : BeepFormsShelfBase
    {
        private readonly FlowLayoutPanel _navPanel;
        private BeepButton? _firstButton;
        private BeepButton? _previousButton;
        private BeepLabel? _positionLabel;
        private BeepButton? _nextButton;
        private BeepButton? _lastButton;
        private BeepFormsRecordNavigationShelfButtons _navigationButtons = BeepFormsRecordNavigationShelfButtons.All;
        private FlowDirection _navigationFlowDirection = FlowDirection.LeftToRight;
        private bool _showPositionLabel = true;

        public BeepFormsRecordNavigationShelf()
        {
            UseThemeColors = true;
            Padding = new Padding(0);
            Margin = new Padding(0);
            MinimumSize = new Size(0, BeepLayoutMetrics.ButtonToolbar.ScaleSize(this).Height + 4);
            Height = BeepLayoutMetrics.ButtonToolbar.ScaleSize(this).Height + 4;

            _navPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                AutoScroll = true,
                Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this),
                Margin = new Padding(0),
                FlowDirection = FlowDirection.LeftToRight
            };

            Controls.Add(_navPanel);
            RefreshNavigationStrip();
        }

        [Browsable(true)]
        [Category("Navigation Shelf")]
        [Description("Select which record navigation buttons are shown.")]
        [DefaultValue(BeepFormsRecordNavigationShelfButtons.All)]
        public BeepFormsRecordNavigationShelfButtons NavigationButtons
        {
            get => _navigationButtons;
            set
            {
                if (_navigationButtons == value)
                    return;

                _navigationButtons = value;
                RefreshNavigationStrip();
            }
        }

        [Browsable(true)]
        [Category("Navigation Shelf")]
        [Description("Controls the flow direction used by the navigation button row.")]
        [DefaultValue(FlowDirection.LeftToRight)]
        public FlowDirection NavigationFlowDirection
        {
            get => _navigationFlowDirection;
            set
            {
                if (_navigationFlowDirection == value)
                    return;

                _navigationFlowDirection = value;
                RefreshNavigationStrip();
            }
        }

        [Browsable(true)]
        [Category("Navigation Shelf")]
        [Description("Show a label indicating the current record position (e.g. 'Record 3 of 47').")]
        [DefaultValue(true)]
        public bool ShowPositionLabel
        {
            get => _showPositionLabel;
            set
            {
                if (_showPositionLabel == value)
                    return;

                _showPositionLabel = value;
                OnFormsHostChanged();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (FormsHost == null) return base.ProcessCmdKey(ref msg, keyData);

            switch (keyData)
            {
                case Keys.PageUp:
                    _ = FormsHost.MoveFirstAsync(); return true;
                case Keys.PageDown:
                    _ = FormsHost.MoveLastAsync(); return true;
                case Keys.Up:
                    _ = FormsHost.MovePreviousAsync(); return true;
                case Keys.Down:
                    _ = FormsHost.MoveNextAsync(); return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void RefreshNavigationStrip()
        {
            _navPanel.SuspendLayout();
            _navPanel.Controls.Clear();
            _navPanel.FlowDirection = NavigationFlowDirection;

            AddButton(ref _firstButton, BeepFormsRecordNavigationShelfButtons.First, "|<", FirstButton_Click, 42);
            AddButton(ref _previousButton, BeepFormsRecordNavigationShelfButtons.Previous, "<", PreviousButton_Click, 42);

            if (NavigationButtons.HasFlag(BeepFormsRecordNavigationShelfButtons.PositionLabel))
            {
                _positionLabel = new BeepLabel
                {
                    AutoSize = true,
                    MinimumSize = new Size(80, 0),
                    TextAlign = ContentAlignment.MiddleCenter,
                    UseThemeColors = true,
                    Margin = new Padding(4, 0, 4, 0),
                    Text = "0 records"
                };
                _navPanel.Controls.Add(_positionLabel);
            }
            else
            {
                _positionLabel = null;
            }

            AddButton(ref _nextButton, BeepFormsRecordNavigationShelfButtons.Next, ">", NextButton_Click, 42);
            AddButton(ref _lastButton, BeepFormsRecordNavigationShelfButtons.Last, ">|", LastButton_Click, 42);

            _navPanel.Visible = _navPanel.Controls.Count > 0;
            _navPanel.ResumeLayout(false);

            OnFormsHostChanged();
        }

        private void AddButton(ref BeepButton? field, BeepFormsRecordNavigationShelfButtons flag, string caption, EventHandler clickHandler, int minimumWidth)
        {
            if (!NavigationButtons.HasFlag(flag))
            {
                field = null;
                return;
            }

            field = new BeepButton
            {
                Width = GetButtonWidth(caption, minimumWidth),
                Height = 28,
                Margin = new Padding(0, 0, 2, 0),
                Text = caption,
                Theme = Theme,
                ShowShadow = false,
                UseThemeColors = true
            };
            field.Click += clickHandler;
            _navPanel.Controls.Add(field);
        }

        private int GetButtonWidth(string caption, int minimumWidth)
        {
            int measuredWidth = TextRenderer.MeasureText(caption ?? string.Empty, Font).Width + 20;
            return Math.Max(minimumWidth, measuredWidth);
        }

        protected override void OnFormsHostChanged()
        {
            base.OnFormsHostChanged();
            bool hasHost = FormsHost != null;
            bool hasActiveBlock = hasHost && !string.IsNullOrWhiteSpace(FormsHost!.ActiveBlockName);
            bool hasRecords = hasActiveBlock && !string.IsNullOrWhiteSpace(FormsHost!.ViewState.RecordPositionText)
                && FormsHost.ViewState.RecordPositionText != "0 records";
            int recordCount = TryParseRecordCount();

            string positionText = hasActiveBlock
                ? FormsHost!.ViewState.RecordPositionText
                : string.Empty;
            if (string.IsNullOrWhiteSpace(positionText))
                positionText = "0 records";

            if (_positionLabel != null && ShowPositionLabel)
            {
                _positionLabel.Text = positionText;
                _positionLabel.Visible = true;
            }

            bool canMovePrevious = hasRecords && TryParseCurrentIndex() > 0;
            bool canMoveNext = hasRecords && TryParseCurrentIndex() < recordCount - 1;

            SetButtonState(_firstButton, hasRecords && canMovePrevious, true);
            SetButtonState(_previousButton, hasRecords && canMovePrevious, true);
            SetButtonState(_nextButton, hasRecords && canMoveNext, true);
            SetButtonState(_lastButton, hasRecords && canMoveNext, true);
        }

        private int TryParseRecordCount()
        {
            string? text = FormsHost?.ViewState.RecordPositionText;
            if (string.IsNullOrWhiteSpace(text) || text == "0 records")
                return 0;

            int slashIndex = text.IndexOf('/');
            if (slashIndex >= 0 && slashIndex < text.Length - 1
                && int.TryParse(text.Substring(slashIndex + 1), out int count))
                return count;

            if (text.IndexOf('/') < 0 && int.TryParse(text, out int solo))
                return solo;

            return 0;
        }

        private int TryParseCurrentIndex()
        {
            string? text = FormsHost?.ViewState.RecordPositionText;
            if (string.IsNullOrWhiteSpace(text))
                return -1;

            int slashIndex = text.IndexOf('/');
            if (slashIndex > 0 && int.TryParse(text.Substring(0, slashIndex), out int current))
                return current - 1;

            return -1;
        }

        private static void SetButtonState(BeepButton? button, bool enabled, bool visible)
        {
            if (button == null)
                return;

            button.Enabled = enabled;
            button.Visible = visible;
        }

        private async void FirstButton_Click(object? sender, EventArgs e)
        {
            if (FormsHost == null) return;
            try { await FormsHost.MoveFirstAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsRecordNavigationShelf.First] {ex.Message}"); }
        }

        private async void PreviousButton_Click(object? sender, EventArgs e)
        {
            if (FormsHost == null) return;
            try { await FormsHost.MovePreviousAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsRecordNavigationShelf.Previous] {ex.Message}"); }
        }

        private async void NextButton_Click(object? sender, EventArgs e)
        {
            if (FormsHost == null) return;
            try { await FormsHost.MoveNextAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsRecordNavigationShelf.Next] {ex.Message}"); }
        }

        private async void LastButton_Click(object? sender, EventArgs e)
        {
            if (FormsHost == null) return;
            try { await FormsHost.MoveLastAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepFormsRecordNavigationShelf.Last] {ex.Message}"); }
        }
    }
}
