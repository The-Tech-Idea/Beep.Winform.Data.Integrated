using System;
using System.Collections.Generic;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models
{
    public sealed class BeepFormsWorkflowEntry
    {
        public DateTime Timestamp { get; init; } = DateTime.Now;
        public string Text { get; init; } = string.Empty;
        public BeepFormsMessageSeverity Severity { get; init; }
    }

    public sealed class BeepFormsViewState
    {
        private readonly List<BeepFormsWorkflowEntry> _workflowHistory = new();

        public bool IsDirty { get; set; }
        public bool IsQueryMode { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string CoordinationText { get; set; } = string.Empty;
        public string WorkflowText { get; set; } = string.Empty;
        public string SavepointText { get; set; } = string.Empty;
        public string AlertText { get; set; } = string.Empty;
        public string CurrentMessage { get; set; } = string.Empty;
        public BeepFormsMessageSeverity CoordinationSeverity { get; set; }
        public BeepFormsMessageSeverity WorkflowSeverity { get; set; }
        public BeepFormsMessageSeverity SavepointSeverity { get; set; }
        public BeepFormsMessageSeverity AlertSeverity { get; set; }
        public BeepFormsMessageSeverity MessageSeverity { get; set; }
        public string? ActiveBlockName { get; set; }
        public IReadOnlyList<BeepFormsWorkflowEntry> WorkflowHistory => _workflowHistory;
        internal List<BeepFormsWorkflowEntry> WorkflowHistoryItems => _workflowHistory;

        // Phase 7D — bootstrap progress surfaced to status strip
        public BootstrapState BootstrapState { get; set; } = BootstrapState.Idle;
    }
}