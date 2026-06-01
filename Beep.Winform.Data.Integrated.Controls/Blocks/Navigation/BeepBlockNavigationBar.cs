using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Base;

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
        private readonly Dictionary<string, BeepButton> _buttons = new(StringComparer.OrdinalIgnoreCase);
        private BeepBlock? _block;

        public BeepBlockNavigationBar()
        {
            ShowAllBorders = false;
            ShowShadow = false;
            IsRounded = false;
            Height = 38;
            Padding = new Padding(0);

            _layoutRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = SystemColors.Control,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            _layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            _layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120f));

            _commandsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = false,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0),
                Padding = new Padding(6, 4, 6, 4),
                BackColor = SystemColors.Control
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
            _layoutRoot.Controls.Add(_positionLabel, 1, 0);
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
                }

                SyncFromBlock();
            }
        }

        public override void ApplyTheme()
        {
            base.ApplyTheme();

            Color chromeBackColor = BackColor.A == 0 ? ParentBackColor : BackColor;
            if (chromeBackColor.A == 0)
            {
                chromeBackColor = SystemColors.Control;
            }

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
                foreach (var button in _buttons.Values)
                {
                    button.Enabled = false;
                }

                return;
            }

            _positionLabel.Text = _block.GetNavigatorPositionText();

            foreach (var button in _buttons)
            {
                bool visible = _block.IsNavigatorCommandVisible(button.Key);
                button.Value.Visible = visible;
                button.Value.Enabled = visible && _block.IsNavigatorCommandEnabled(button.Key);
            }
        }

        private async void FirstButton_Click(object? sender, EventArgs e)
        {
            if (_block != null)
            {
                await _block.MoveFirstAsync().ConfigureAwait(true);
            }
        }

        private async void PreviousButton_Click(object? sender, EventArgs e)
        {
            if (_block != null)
            {
                await _block.MovePreviousAsync().ConfigureAwait(true);
            }
        }

        private async void NextButton_Click(object? sender, EventArgs e)
        {
            if (_block != null)
            {
                await _block.MoveNextAsync().ConfigureAwait(true);
            }
        }

        private async void LastButton_Click(object? sender, EventArgs e)
        {
            if (_block != null)
            {
                await _block.MoveLastAsync().ConfigureAwait(true);
            }
        }

        private async void NewButton_Click(object? sender, EventArgs e)
        {
            if (_block != null)
            {
                await _block.NewRecordAsync().ConfigureAwait(true);
            }
        }

        private async void DeleteButton_Click(object? sender, EventArgs e)
        {
            if (_block != null)
            {
                await _block.DeleteCurrentRecordAsync().ConfigureAwait(true);
            }
        }

        private async void QueryButton_Click(object? sender, EventArgs e)
        {
            if (_block != null)
            {
                await _block.EnterQueryAsync().ConfigureAwait(true);
            }
        }

        private async void ExecuteButton_Click(object? sender, EventArgs e)
        {
            if (_block != null)
            {
                await _block.ExecuteQueryAsync().ConfigureAwait(true);
            }
        }

        private async void SaveButton_Click(object? sender, EventArgs e)
        {
            if (_block != null)
            {
                await _block.CommitAsync().ConfigureAwait(true);
            }
        }

        private async void RollbackButton_Click(object? sender, EventArgs e)
        {
            if (_block != null)
            {
                await _block.RollbackAsync().ConfigureAwait(true);
            }
        }
    }
}