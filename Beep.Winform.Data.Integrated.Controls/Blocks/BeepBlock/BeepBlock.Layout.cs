using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.GridX;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks
{
    public partial class BeepBlock
    {
        private BeepPanel? _headerPanel;
        private BeepLabel? _captionLabel;
        private BeepLabel? _summaryLabel;
        private BeepPanel? _workflowPanel;
        private BeepLabel? _workflowLabel;
        private BeepButton? _clearQueryButton;
        private BeepButton? _executeQueryButton;
        private BeepPanel? _validationSummaryPanel;
        private Panel? _validationSummaryHeaderPanel;
        private BeepLabel? _validationSummaryHeadlineLabel;
        private BeepLabel? _validationSummarySeverityLabel;
        private BeepLabel? _validationSummaryDetailsLabel;
        private BeepLabel? _validationSummaryHintLabel;
        private BeepBlockNavigationBar? _navigationBar;
        private FlowLayoutPanel? _recordHostPanel;
        private BeepPanel? _gridHostPanel;
        private BeepGridPro? _gridView;
        private BindingSource? _recordBindingSource;
        private readonly Dictionary<string, Control> _fieldEditors = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, BeepPanel> _fieldRows = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, BeepLabel> _fieldLabels = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, BeepLabel> _fieldStateLabels = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, BeepComboBox> _queryOperatorEditors = new(StringComparer.OrdinalIgnoreCase);

        private void InitializeLayout()
        {
            SuspendLayout();

            _headerPanel = new BeepPanel
            {
                Dock = DockStyle.Top,
                Height = 34,
                Padding = new Padding(8, 6, 8, 4),
                ShowTitle = false,
                ShowTitleLine = false,
                UseThemeColors = true
            };

            _captionLabel = new BeepLabel
            {
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                UseThemeColors = true
            };

            _summaryLabel = new BeepLabel
            {
                Dock = DockStyle.Right,
                Width = 220,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleRight,
                UseThemeColors = true
            };

            _headerPanel.Controls.Add(_captionLabel);
            _headerPanel.Controls.Add(_summaryLabel);

            _workflowPanel = new BeepPanel
            {
                Dock = DockStyle.Top,
                Height = 56,
                Padding = new Padding(8, 4, 8, 4),
                ShowTitle = false,
                ShowTitleLine = false,
                UseThemeColors = true,
                Visible = false
            };

            _workflowLabel = new BeepLabel
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                WordWrap = true,
                TextAlign = ContentAlignment.MiddleLeft,
                UseThemeColors = true
            };

            _executeQueryButton = new BeepButton
            {
                Dock = DockStyle.Right,
                Width = 112,
                Text = "Execute",
                Theme = Theme,
                ShowShadow = false,
                Margin = new Padding(8, 0, 0, 0)
            };
            _executeQueryButton.Click += ExecuteQueryButton_Click;

            _clearQueryButton = new BeepButton
            {
                Dock = DockStyle.Right,
                Width = 96,
                Text = "Clear",
                Theme = Theme,
                ShowShadow = false,
                Margin = new Padding(8, 0, 0, 0)
            };
            _clearQueryButton.Click += ClearQueryButton_Click;

            _workflowPanel.Controls.Add(_workflowLabel);
            _workflowPanel.Controls.Add(_executeQueryButton);
            _workflowPanel.Controls.Add(_clearQueryButton);

            _validationSummaryPanel = new BeepPanel
            {
                Dock = DockStyle.Top,
                Height = 108,
                Padding = new Padding(8, 4, 8, 6),
                ShowTitle = false,
                ShowTitleLine = false,
                UseThemeColors = true,
                Visible = false
            };

            _validationSummaryHeaderPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 24,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            _validationSummaryHeadlineLabel = new BeepLabel
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                UseThemeColors = true
            };

            _validationSummarySeverityLabel = new BeepLabel
            {
                Dock = DockStyle.Right,
                Width = 104,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(6, 0, 6, 0),
                UseThemeColors = false,
                Visible = false
            };

            _validationSummaryDetailsLabel = new BeepLabel
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                WordWrap = true,
                TextAlign = ContentAlignment.MiddleLeft,
                UseThemeColors = true
            };

            _validationSummaryHintLabel = new BeepLabel
            {
                Dock = DockStyle.Bottom,
                Height = 20,
                TextAlign = ContentAlignment.MiddleLeft,
                UseThemeColors = true,
                Visible = false
            };

            _validationSummaryHeaderPanel.Controls.Add(_validationSummaryHeadlineLabel);
            _validationSummaryHeaderPanel.Controls.Add(_validationSummarySeverityLabel);

            _validationSummaryPanel.Controls.Add(_validationSummaryDetailsLabel);
            _validationSummaryPanel.Controls.Add(_validationSummaryHintLabel);
            _validationSummaryPanel.Controls.Add(_validationSummaryHeaderPanel);

            _navigationBar = new BeepBlockNavigationBar
            {
                Dock = DockStyle.Bottom,
                Height = 38,
                Block = this,
                Theme = Theme
            };

            _recordHostPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(8)
            };

            _gridHostPanel = new BeepPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                Visible = false,
                ShowTitle = false,
                ShowTitleLine = false,
                UseThemeColors = true
            };

            _gridView = new BeepGridPro
            {
                Dock = DockStyle.Fill,
                UseThemeColors = true
            };

            _gridHostPanel.Controls.Add(_gridView);

            Controls.Add(_gridHostPanel);
            Controls.Add(_recordHostPanel);
            Controls.Add(_navigationBar);
            Controls.Add(_validationSummaryPanel);
            Controls.Add(_workflowPanel);
            Controls.Add(_headerPanel);

            ResumeLayout(false);
        }

        private void RefreshPresentation()
        {
            if (_captionLabel != null)
            {
                _captionLabel.Text = string.IsNullOrWhiteSpace(EffectiveDefinition?.Caption)
                    ? (string.IsNullOrWhiteSpace(BlockName) ? Name : BlockName)
                    : EffectiveDefinition.Caption;
            }

            if (_summaryLabel != null)
            {
                UpdateSummaryText();
            }

            RebuildRecordLayoutFromDefinition();
            ApplyCurrentRecordToEditors();
            ApplyPresentationMode();
            ShowFieldErrors(_fieldValidationStates);
            UpdateWorkflowSurface();
            UpdateValidationSummarySurface();
        }

        private void ApplyPresentationMode()
        {
            var mode = EffectiveDefinition?.PresentationMode ?? BeepBlockPresentationMode.Record;

            // DesignerGenerated: the host panels are not used; the parent form owns the controls.
            if (mode == BeepBlockPresentationMode.DesignerGenerated)
            {
                if (_recordHostPanel != null) _recordHostPanel.Visible = false;
                if (_gridHostPanel != null)   _gridHostPanel.Visible   = false;
                // Push current record values into the designer-generated controls.
                RefreshGeneratedFieldControls();
                return;
            }

            bool showGrid = !ViewState.IsQueryMode && mode == BeepBlockPresentationMode.Grid;

            if (_recordHostPanel != null)
            {
                _recordHostPanel.Visible = !showGrid;
            }

            if (_gridHostPanel != null)
            {
                _gridHostPanel.Visible = showGrid;
            }
        }

        private void UpdateSummaryText()
        {
            if (_summaryLabel == null)
            {
                return;
            }

            int currentRecord = ViewState.CurrentRecordIndex >= 0 ? ViewState.CurrentRecordIndex + 1 : 0;
            _summaryLabel.Text = $"{currentRecord} / {ViewState.RecordCount}";
        }
    }
}