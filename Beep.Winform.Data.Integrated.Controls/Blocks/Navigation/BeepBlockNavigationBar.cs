using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Base;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Controls.ToolTips;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks
{
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(BeepBlockNavigationBar))]
    [Category("Beep Controls")]
    [DisplayName("Beep Block Navigation Bar")]
    [Description("Navigation and command bar bound to one BeepBlock.")]
    public sealed class BeepBlockNavigationBar : BaseControl
    {
        private readonly TableLayoutPanel _layoutRoot;
        private readonly FlowLayoutPanel _commandsPanel;
        private readonly BeepLabel _positionLabel;
        private readonly BeepLabel _recordStatusLabel;
        private readonly Dictionary<string, BeepButton> _buttons = new(StringComparer.OrdinalIgnoreCase);
        private BeepBlock? _block;

        public BeepBlockNavigationBar()
        {
            ShowAllBorders = false;
            ShowShadow = false;
            IsRounded = false;
            Height = BeepLayoutMetrics.ButtonToolbar.ScaleSize(this).Height + 6;
            Padding = new Padding(0);
            UseThemeColors = true;

            _layoutRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            _layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            _layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32f));
            _layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120f));

            _commandsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = false,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0),
                Padding = BeepLayoutMetrics.ContainerPadding.ScalePadding(this)
            };

            _recordStatusLabel = new BeepLabel
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                UseThemeColors = false,
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold),
                Text = string.Empty,
                BackColor = Color.Transparent,
                ToolTipText = "Record status (Oracle Forms :SYSTEM.RECORD_STATUS)",
                TooltipType = ToolTipType.Info,
                Margin = new Padding(0)
            };

            _positionLabel = new BeepLabel
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                UseThemeColors = true,
                Margin = new Padding(0, 0, 8, 0),
                Text = "0 / 0"
            };

            _layoutRoot.Controls.Add(_commandsPanel, 0, 0);
            _layoutRoot.Controls.Add(_recordStatusLabel, 1, 0);
            _layoutRoot.Controls.Add(_positionLabel, 2, 0);
            Controls.Add(_layoutRoot);

            CreateButtons();
            SyncFromBlock();
        }

        protected override bool IsContainerControl => true;

        [Browsable(false)]
        public BeepBlock? Block
        {
            get => _block;
            set
            {
                if (ReferenceEquals(_block, value))
                {
                    return;
                }

                if (_block != null)
                {
                    _block.ViewStateChanged -= Block_ViewStateChanged;
                }

                _block = value;

                if (_block != null)
                {
                    Theme = _block.Theme;
                    _block.ViewStateChanged += Block_ViewStateChanged;
                    ApplyTheme();
                }

                SyncFromBlock();
            }
        }

        public override void ApplyTheme()
        {
            base.ApplyTheme();
            Color chromeBackColor = BackColor.A == 0 ? ParentBackColor : BackColor;
            if (chromeBackColor.A == 0) chromeBackColor = BackColor;

            _layoutRoot.BackColor = chromeBackColor;
            _commandsPanel.BackColor = chromeBackColor;

            _positionLabel.Theme = Theme;
            foreach (var button in _buttons.Values)
            {
                button.Theme = Theme;
            }

            Invalidate();
        }

        private void CreateButtons()
        {
            AddButton("first", "First record", "TheTechIdea.Beep.Winform.Controls.GFX.SVG.angle-double-small-left.svg", FirstButton_Click);
            AddButton("previous", "Previous record", "TheTechIdea.Beep.Winform.Controls.GFX.SVG.angle-small-left.svg", PreviousButton_Click);
            AddButton("next", "Next record", "TheTechIdea.Beep.Winform.Controls.GFX.SVG.angle-small-right.svg", NextButton_Click);
            AddButton("last", "Last record", "TheTechIdea.Beep.Winform.Controls.GFX.SVG.angle-double-small-right.svg", LastButton_Click);
            AddButton("new", "New record", "TheTechIdea.Beep.Winform.Controls.GFX.SVG.add.svg", NewButton_Click);
            AddButton("delete", "Delete record", "TheTechIdea.Beep.Winform.Controls.GFX.SVG.minus.svg", DeleteButton_Click);
            AddButton("query", "Enter query mode", "TheTechIdea.Beep.Winform.Controls.GFX.SVG.search.svg", QueryButton_Click);
            AddButton("execute", "Execute query", "TheTechIdea.Beep.Winform.Controls.GFX.SVG.filter.svg", ExecuteButton_Click);
            AddButton("save", "Commit changes", "TheTechIdea.Beep.Winform.Controls.GFX.SVG.floppy-disk.svg", SaveButton_Click);
            AddButton("rollback", "Rollback changes", "TheTechIdea.Beep.Winform.Controls.GFX.SVG.back-button.svg", RollbackButton_Click);
        }

        private void AddButton(string key, string toolTip, string imagePath, EventHandler clickHandler)
        {
            var button = new BeepButton
            {
                Size = new Size(24, 24),
                MaxImageSize = new Size(18, 18),
                ImagePath = imagePath,
                ImageAlign = ContentAlignment.MiddleCenter,
                Text = string.Empty,
                ToolTipText = toolTip,
                ShowAllBorders = false,
                ShowShadow = false,
                IsFrameless = true,
                IsChild = true,
                Theme = Theme,
                Margin = new Padding(0, 0, 6, 0),
                Padding = new Padding(0)
            };
            button.Click += clickHandler;

            _buttons[key] = button;
            _commandsPanel.Controls.Add(button);
        }

        private void Block_ViewStateChanged(object? sender, EventArgs e)
        {
            SyncFromBlock();
        }

        private void SyncFromBlock()
        {
            if (_block == null)
            {
                _positionLabel.Text = "0 / 0";
                ResetRecordStatus();
                foreach (var button in _buttons.Values)
                {
                    button.Enabled = false;
                }

                return;
            }

            _positionLabel.Text = _block.GetNavigatorPositionText();
            ApplyRecordStatus(_block.ViewState.RecordStatus);

            foreach (var button in _buttons)
            {
                try
                {
                    bool visible = _block.IsNavigatorCommandVisible(button.Key);
                    button.Value.Visible = visible;
                    button.Value.Enabled = visible && _block.IsNavigatorCommandEnabled(button.Key);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[BeepBlockNavigationBar.SyncFromBlock] {button.Key}: {ex.GetType().Name} - {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Render the Oracle Forms <c>:SYSTEM.RECORD_STATUS</c> as a small
        /// colored chip between the command bar and the position label.
        /// Mirrors the well-known Forms convention where an asterisk signals
        /// "unsaved changes" and a yellow background signals query criteria.
        /// </summary>
        private void ApplyRecordStatus(BeepRecordStatus status)
        {
            bool showIndicator = _block?.Definition?.ShowRecordStatusIndicator ?? true;
            if (!showIndicator || _block == null)
            {
                ResetRecordStatus();
                return;
            }

            switch (status)
            {
                case BeepRecordStatus.New:
                case BeepRecordStatus.Insert:
                case BeepRecordStatus.Changed:
                    _recordStatusLabel.Text = "*";
                    _recordStatusLabel.ForeColor = Color.FromArgb(196, 18, 18);
                    _recordStatusLabel.BackColor = Color.FromArgb(255, 235, 235);
                    _recordStatusLabel.ToolTipText = $"Record status: {status.ToFormsString()} (unsaved changes)";
                    break;
                case BeepRecordStatus.QueryCriteria:
                    _recordStatusLabel.Text = "Q";
                    _recordStatusLabel.ForeColor = Color.FromArgb(102, 85, 0);
                    _recordStatusLabel.BackColor = Color.FromArgb(255, 248, 196);
                    _recordStatusLabel.ToolTipText = "Record status: QUERY (Enter-Query criteria)";
                    break;
                case BeepRecordStatus.Query:
                default:
                    ResetRecordStatus();
                    break;
            }
        }

        private void ResetRecordStatus()
        {
            _recordStatusLabel.Text = string.Empty;
            _recordStatusLabel.ForeColor = ForeColor;
            _recordStatusLabel.BackColor = Color.Transparent;
            _recordStatusLabel.ToolTipText = "Record status: QUERY (no changes)";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _block != null)
            {
                _block.ViewStateChanged -= Block_ViewStateChanged;
                _block = null;
            }
            base.Dispose(disposing);
        }

        private async void FirstButton_Click(object? sender, EventArgs e)
        {
            if (_block == null) return;
            try { await _block.MoveFirstAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepBlockNavigationBar.First] {ex.Message}"); }
        }

        private async void PreviousButton_Click(object? sender, EventArgs e)
        {
            if (_block == null) return;
            try { await _block.MovePreviousAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepBlockNavigationBar.Previous] {ex.Message}"); }
        }

        private async void NextButton_Click(object? sender, EventArgs e)
        {
            if (_block == null) return;
            try { await _block.MoveNextAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepBlockNavigationBar.Next] {ex.Message}"); }
        }

        private async void LastButton_Click(object? sender, EventArgs e)
        {
            if (_block == null) return;
            try { await _block.MoveLastAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepBlockNavigationBar.Last] {ex.Message}"); }
        }

        private async void NewButton_Click(object? sender, EventArgs e)
        {
            if (_block == null) return;
            try { await _block.NewRecordAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepBlockNavigationBar.New] {ex.Message}"); }
        }

        private async void DeleteButton_Click(object? sender, EventArgs e)
        {
            if (_block == null) return;
            try { await _block.DeleteCurrentRecordAsync(CancellationToken.None).ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepBlockNavigationBar.Delete] {ex.Message}"); }
        }

        private async void QueryButton_Click(object? sender, EventArgs e)
        {
            if (_block == null) return;
            try { await _block.EnterQueryAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepBlockNavigationBar.Query] {ex.Message}"); }
        }

        private async void ExecuteButton_Click(object? sender, EventArgs e)
        {
            if (_block == null) return;
            try { await _block.ExecuteQueryAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepBlockNavigationBar.Execute] {ex.Message}"); }
        }

        private async void SaveButton_Click(object? sender, EventArgs e)
        {
            if (_block == null) return;
            try { await _block.CommitAsync(CancellationToken.None).ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepBlockNavigationBar.Save] {ex.Message}"); }
        }

        private async void RollbackButton_Click(object? sender, EventArgs e)
        {
            if (_block == null) return;
            try { await _block.RollbackAsync().ConfigureAwait(true); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepBlockNavigationBar.Rollback] {ex.Message}"); }
        }
    }
}