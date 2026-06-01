using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Base;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Helpers;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(BeepFormsStatusStrip))]
    [Category("Beep Controls")]
    [DisplayName("Beep Forms Status Strip")]
    [Description("Standalone status and workflow message strip for a BeepForms host.")]
    [Designer("TheTechIdea.Beep.Winform.Controls.Design.Server.Designers.BeepFormsStatusStripDesigner, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public class BeepFormsStatusStrip : BaseControl
    {
        private readonly TableLayoutPanel _table;
        private readonly BeepLabel _statusLabel;
        private readonly BeepLabel _messageLabel;
        private readonly BeepLabel _coordinationLabel;
        private readonly BeepLabel _workflowLabel;
        private readonly BeepLabel _savepointLabel;
        private readonly BeepLabel _alertLabel;
        private BeepForms? _formsHost;
        private bool _autoBindFormsHost = true;
        private bool _showStatusLine = true;
        private bool _showMessageLine = true;
        private bool _showCoordinationLine = true;
        private bool _showWorkflowLine = true;
        private bool _showSavepointLine = true;
        private bool _showAlertLine = true;
        private int _workflowHistoryVisibleCount = 3;

        public BeepFormsStatusStrip()
        {
            UseThemeColors = true;
            Padding = new Padding(0);
            Margin = new Padding(0);
            MinimumSize = new Size(0, 36);
            Height = 110;

            _table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(10, 6, 10, 6),
                Margin = new Padding(0)
            };
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            _statusLabel = CreateLineLabel();
            _messageLabel = CreateLineLabel();
            _coordinationLabel = CreateLineLabel();
            _workflowLabel = CreateLineLabel();
            _workflowLabel.Multiline = true;
            _workflowLabel.WordWrap = true;
            _workflowLabel.AutoEllipsis = false;
            _workflowLabel.TextAlign = ContentAlignment.TopLeft;
            _savepointLabel = CreateLineLabel();
            _alertLabel = CreateLineLabel();

            _table.Controls.Add(_statusLabel, 0, 0);
            _table.Controls.Add(_messageLabel, 0, 1);
            _table.Controls.Add(_coordinationLabel, 0, 2);
            _table.Controls.Add(_workflowLabel, 0, 3);
            _table.Controls.Add(_savepointLabel, 0, 4);
            _table.Controls.Add(_alertLabel, 0, 5);

            Controls.Add(_table);
            UpdateFromViewState();
        }

        [Browsable(true)]
        [Category("Behavior")]
        [Description("Optional BeepForms coordinator surfaced by this status strip.")]
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
                UpdateFromViewState();
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
        [Category("Display")]
        [Description("Show the primary status line.")]
        [DefaultValue(true)]
        public bool ShowStatusLine
        {
            get => _showStatusLine;
            set
            {
                if (_showStatusLine == value)
                {
                    return;
                }

                _showStatusLine = value;
                UpdateFromViewState();
            }
        }

        [Browsable(true)]
        [Category("Display")]
        [Description("Show the shared current-message line.")]
        [DefaultValue(true)]
        public bool ShowMessageLine
        {
            get => _showMessageLine;
            set
            {
                if (_showMessageLine == value)
                {
                    return;
                }

                _showMessageLine = value;
                UpdateFromViewState();
            }
        }

        [Browsable(true)]
        [Category("Display")]
        [Description("Show the coordination summary line.")]
        [DefaultValue(true)]
        public bool ShowCoordinationLine
        {
            get => _showCoordinationLine;
            set
            {
                if (_showCoordinationLine == value)
                {
                    return;
                }

                _showCoordinationLine = value;
                UpdateFromViewState();
            }
        }

        [Browsable(true)]
        [Category("Display")]
        [Description("Show the workflow summary line.")]
        [DefaultValue(true)]
        public bool ShowWorkflowLine
        {
            get => _showWorkflowLine;
            set
            {
                if (_showWorkflowLine == value)
                {
                    return;
                }

                _showWorkflowLine = value;
                UpdateFromViewState();
            }
        }

        [Browsable(true)]
        [Category("Display")]
        [Description("Maximum number of recent workflow entries to render in the rolling history surface.")]
        [DefaultValue(3)]
        public int WorkflowHistoryVisibleCount
        {
            get => _workflowHistoryVisibleCount;
            set
            {
                int normalizedValue = Math.Max(1, value);
                if (_workflowHistoryVisibleCount == normalizedValue)
                {
                    return;
                }

                _workflowHistoryVisibleCount = normalizedValue;
                UpdateFromViewState();
            }
        }

        [Browsable(true)]
        [Category("Display")]
        [Description("Show the savepoint summary line.")]
        [DefaultValue(true)]
        public bool ShowSavepointLine
        {
            get => _showSavepointLine;
            set
            {
                if (_showSavepointLine == value)
                {
                    return;
                }

                _showSavepointLine = value;
                UpdateFromViewState();
            }
        }

        [Browsable(true)]
        [Category("Display")]
        [Description("Show the alert summary line.")]
        [DefaultValue(true)]
        public bool ShowAlertLine
        {
            get => _showAlertLine;
            set
            {
                if (_showAlertLine == value)
                {
                    return;
                }

                _showAlertLine = value;
                UpdateFromViewState();
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

        private static BeepLabel CreateLineLabel()
        {
            return new BeepLabel
            {
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                UseThemeColors = true,
                Margin = new Padding(0)
            };
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
            UpdateFromViewState();
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

        private void UpdateFromViewState()
        {
            if (_formsHost == null)
            {
                ApplyLine(_statusLabel, "Status: No BeepForms host attached.", BeepFormsMessageSeverity.Info, visible: ShowStatusLine);
                ApplyLine(_messageLabel, string.Empty, BeepFormsMessageSeverity.None, visible: false);
                ApplyLine(_coordinationLabel, string.Empty, BeepFormsMessageSeverity.None, visible: false);
                ApplyLine(_workflowLabel, string.Empty, BeepFormsMessageSeverity.None, visible: false);
                ApplyLine(_savepointLabel, string.Empty, BeepFormsMessageSeverity.None, visible: false);
                ApplyLine(_alertLabel, string.Empty, BeepFormsMessageSeverity.None, visible: false);
                ApplyLayout();
                return;
            }

            BeepFormsViewState viewState = _formsHost.ViewState;
            string workflowText = BuildWorkflowHistoryText(viewState);

            string statusText = BeepFormsDisplayTextResolver.ResolveStatusText(_formsHost);

            // Phase 7D — prepend bootstrap state indicator
            string bootstrapPrefix = viewState.BootstrapState switch
            {
                BootstrapState.Running       => "[Loading…] ",
                BootstrapState.PartialSuccess => "[Partial] ",
                BootstrapState.Failed        => "[Load Failed] ",
                _                            => string.Empty
            };

            var statusSeverity = viewState.BootstrapState == BootstrapState.Failed
                ? BeepFormsMessageSeverity.Error
                : viewState.BootstrapState == BootstrapState.PartialSuccess
                    ? BeepFormsMessageSeverity.Warning
                    : ResolveStatusSeverity(viewState);

            ApplyLine(_statusLabel, $"Status: {bootstrapPrefix}{statusText}", statusSeverity, visible: ShowStatusLine);
            ApplyLine(_messageLabel, string.IsNullOrWhiteSpace(viewState.CurrentMessage) ? string.Empty : $"Message: {viewState.CurrentMessage}", viewState.MessageSeverity, ShowMessageLine && !string.IsNullOrWhiteSpace(viewState.CurrentMessage));
            ApplyLine(_coordinationLabel, string.IsNullOrWhiteSpace(viewState.CoordinationText) ? string.Empty : $"Coordination: {viewState.CoordinationText}", viewState.CoordinationSeverity, ShowCoordinationLine && !string.IsNullOrWhiteSpace(viewState.CoordinationText));
            ApplyLine(_workflowLabel, workflowText, ResolveWorkflowHistorySeverity(viewState), ShowWorkflowLine && !string.IsNullOrWhiteSpace(workflowText));
            ApplyLine(_savepointLabel, string.IsNullOrWhiteSpace(viewState.SavepointText) ? string.Empty : $"Savepoints: {viewState.SavepointText}", viewState.SavepointSeverity, ShowSavepointLine && !string.IsNullOrWhiteSpace(viewState.SavepointText));
            ApplyLine(_alertLabel, string.IsNullOrWhiteSpace(viewState.AlertText) ? string.Empty : $"Alerts: {viewState.AlertText}", viewState.AlertSeverity, ShowAlertLine && !string.IsNullOrWhiteSpace(viewState.AlertText));

            ApplyLayout();
        }

        private void ApplyLine(BeepLabel label, string text, BeepFormsMessageSeverity severity, bool visible)
        {
            label.Text = text;
            label.ForeColor = GetMessageColor(severity);
            label.Visible = visible;
        }

        private void ApplyLayout()
        {
            int visibleLines = 0;
            visibleLines += ApplyRowVisibility(0, _statusLabel.Visible, 1);
            visibleLines += ApplyRowVisibility(1, _messageLabel.Visible, 1);
            visibleLines += ApplyRowVisibility(2, _coordinationLabel.Visible, 1);
            visibleLines += ApplyRowVisibility(3, _workflowLabel.Visible, GetWorkflowRowLineCount());
            visibleLines += ApplyRowVisibility(4, _savepointLabel.Visible, 1);
            visibleLines += ApplyRowVisibility(5, _alertLabel.Visible, 1);

            Height = Math.Max(36, visibleLines * 18 + 16);
        }

        private int ApplyRowVisibility(int rowIndex, bool visible, int lineCount)
        {
            while (_table.RowStyles.Count <= rowIndex)
            {
                _table.RowStyles.Add(new RowStyle(SizeType.Absolute, 0f));
            }

            _table.RowStyles[rowIndex].SizeType = SizeType.Absolute;
            _table.RowStyles[rowIndex].Height = visible ? 18f * Math.Max(1, lineCount) : 0f;
            return visible ? Math.Max(1, lineCount) : 0;
        }

        private int GetWorkflowRowLineCount()
        {
            if (!_workflowLabel.Visible || string.IsNullOrWhiteSpace(_workflowLabel.Text))
            {
                return 0;
            }

            int availableWidth = Math.Max(180, (_table.DisplayRectangle.Width > 0 ? _table.DisplayRectangle.Width : Width) - _table.Padding.Horizontal);
            Size preferredSize = _workflowLabel.GetPreferredSize(new Size(availableWidth, 0));
            return Math.Max(1, (int)Math.Ceiling(preferredSize.Height / 18d));
        }

        private string BuildWorkflowHistoryText(BeepFormsViewState viewState)
        {
            BeepFormsWorkflowEntry[] entries = viewState.WorkflowHistory
                .Take(WorkflowHistoryVisibleCount)
                .ToArray();

            if (entries.Length == 0)
            {
                return string.IsNullOrWhiteSpace(viewState.WorkflowText)
                    ? string.Empty
                    : $"Workflow: {viewState.WorkflowText}";
            }

            if (entries.Length == 1)
            {
                return $"Workflow: {FormatWorkflowEntry(entries[0])}";
            }

            return "Workflow:" + Environment.NewLine + string.Join(Environment.NewLine, entries.Select(entry => $"  {FormatWorkflowEntry(entry)}"));
        }

        private BeepFormsMessageSeverity ResolveWorkflowHistorySeverity(BeepFormsViewState viewState)
        {
            BeepFormsWorkflowEntry[] entries = viewState.WorkflowHistory
                .Take(WorkflowHistoryVisibleCount)
                .ToArray();

            if (entries.Length == 0)
            {
                return viewState.WorkflowSeverity;
            }

            if (entries.Any(entry => entry.Severity == BeepFormsMessageSeverity.Error))
            {
                return BeepFormsMessageSeverity.Error;
            }

            if (entries.Any(entry => entry.Severity == BeepFormsMessageSeverity.Warning))
            {
                return BeepFormsMessageSeverity.Warning;
            }

            if (entries.Any(entry => entry.Severity == BeepFormsMessageSeverity.Success))
            {
                return BeepFormsMessageSeverity.Success;
            }

            if (entries.Any(entry => entry.Severity == BeepFormsMessageSeverity.Info))
            {
                return BeepFormsMessageSeverity.Info;
            }

            return BeepFormsMessageSeverity.None;
        }

        private static string FormatWorkflowEntry(BeepFormsWorkflowEntry entry)
        {
            string severityCode = entry.Severity switch
            {
                BeepFormsMessageSeverity.Error => "ERR",
                BeepFormsMessageSeverity.Warning => "WARN",
                BeepFormsMessageSeverity.Success => "OK",
                BeepFormsMessageSeverity.Info => "INFO",
                _ => "NOTE"
            };

            return $"{entry.Timestamp:HH:mm} {severityCode} {entry.Text}";
        }

        private static BeepFormsMessageSeverity ResolveStatusSeverity(BeepFormsViewState viewState)
        {
            if (viewState.IsDirty)
            {
                return BeepFormsMessageSeverity.Warning;
            }

            if (viewState.IsQueryMode)
            {
                return BeepFormsMessageSeverity.Info;
            }

            return BeepFormsMessageSeverity.Success;
        }

        private static Color GetMessageColor(BeepFormsMessageSeverity severity)
        {
            return severity switch
            {
                BeepFormsMessageSeverity.Success => Color.ForestGreen,
                BeepFormsMessageSeverity.Warning => Color.DarkOrange,
                BeepFormsMessageSeverity.Error => Color.Firebrick,
                _ => Color.Black
            };
        }
    }
}