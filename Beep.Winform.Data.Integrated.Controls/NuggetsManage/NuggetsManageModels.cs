using System;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.NuggetsManage
{
    internal enum NuggetOperationSeverity
    {
        Info,
        Warn,
        Error
    }

    internal sealed class NuggetOperationLogEntry
    {
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public NuggetOperationSeverity Severity { get; set; } = NuggetOperationSeverity.Info;
        public string Message { get; set; } = string.Empty;
    }

    internal sealed class NuggetItemState
    {
        public string NuggetName { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        public string NuggetVersion { get; set; } = string.Empty;
        public string NuggetSource { get; set; } = string.Empty;
        public bool IsLoaded { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsMissing { get; set; }
        public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;

        public string ToDisplayText()
        {
            var state = IsMissing ? "Missing" : (IsLoaded ? "Loaded" : "NotLoaded");
            var version = string.IsNullOrWhiteSpace(NuggetVersion) ? "-" : NuggetVersion;
            var startup = IsEnabled ? "Startup:On" : "Startup:Off";
            return $"{NuggetName} | {state} | {startup} | v{version}";
        }
    }

    internal sealed class NuggetOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public NuggetOperationSeverity Severity { get; set; } = NuggetOperationSeverity.Info;

        public static NuggetOperationResult Ok(string message)
        {
            return new NuggetOperationResult
            {
                Success = true,
                Message = message,
                Severity = NuggetOperationSeverity.Info
            };
        }

        public static NuggetOperationResult Fail(string message, NuggetOperationSeverity severity = NuggetOperationSeverity.Error)
        {
            return new NuggetOperationResult
            {
                Success = false,
                Message = message,
                Severity = severity
            };
        }
    }
}
