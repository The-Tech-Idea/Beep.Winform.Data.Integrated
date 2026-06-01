using System;
using System.Collections.Generic;
using TheTechIdea.Beep.NuGet;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    #region Persistence Models

    public sealed class NuggetInstallRequest
    {
        public string PackageId { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public List<string> Sources { get; set; } = new();
        public bool UseSingleSharedContext { get; set; } = true;
        public bool LoadAfterInstall { get; set; } = true;
        public bool UseProcessHost { get; set; }
        public string AppInstallPath { get; set; } = string.Empty;
    }

    public sealed class NuggetInstallResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int LoadedAssembliesCount { get; set; }
        public DateTime CompletedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public sealed class NuggetInstalledState
    {
        public string PackageId { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string InstallPath { get; set; } = string.Empty;
        public bool IsLoaded { get; set; }
        public bool IsEnabledAtStartup { get; set; }
        public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
    }

    public sealed class NuggetsPersistedState
    {
        public string LastSourceUrl { get; set; } = string.Empty;
        public string LastSearchTerm { get; set; } = string.Empty;
        public bool IncludePrerelease { get; set; }
        public bool LoadAfterInstall { get; set; } = true;
        public bool UseSingleSharedContext { get; set; } = true;
        public bool UseProcessHost { get; set; }
        public string LastInstallPath { get; set; } = string.Empty;
        public int LastActiveTabIndex { get; set; }
        public List<NuGetSourceConfig> Sources { get; set; } = new();
        public List<NuggetInstalledState> InstalledStates { get; set; } = new();
    }

    #endregion

    /// <summary>
    /// UI-specific view model for search results displayed in BeepGridPro.
    /// </summary>
    public sealed class NuggetSearchViewModel
    {
        public string PackageId { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Authors { get; set; } = string.Empty;
        public long TotalDownloads { get; set; }
        public string IconUrl { get; set; } = string.Empty;
        public string ProjectUrl { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
    }

    /// <summary>
    /// UI-specific view model for installed nuggets displayed in BeepGridPro.
    /// </summary>
    public sealed class NuggetInstalledViewModel
    {
        public string PackageId { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Startup { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string InstallPath { get; set; } = string.Empty;
        public bool IsLoaded { get; set; }
        public bool IsEnabledAtStartup { get; set; }
        public DateTime LastUpdatedUtc { get; set; }
    }

    /// <summary>
    /// Log entry for NuGet operations shown in the Activity tab.
    /// </summary>
    public sealed class NuggetOperationLog
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public NuggetLogSeverity Severity { get; set; } = NuggetLogSeverity.Info;
        public string Message { get; set; } = string.Empty;
        public string? PackageId { get; set; }
        public string? Version { get; set; }
    }

    public enum NuggetLogSeverity
    {
        Info,
        Success,
        Warning,
        Error
    }

    /// <summary>
    /// Shared context keys used across the install wizard steps.
    /// </summary>
    public static class NuggetWizardKeys
    {
        public const string Service           = "Service";
        public const string PackageId         = "PackageId";
        public const string IncludePrerelease = "IncludePrerelease";
        public const string SelectedVersion   = "SelectedVersion";
        public const string SelectedSourceUrl = "SelectedSourceUrl";
        public const string LoadAfterInstall  = "LoadAfterInstall";
        public const string SharedContext     = "SharedContext";
        public const string UseProcessHost    = "UseProcessHost";
        public const string InstallPath       = "InstallPath";
        public const string InstallResult     = "InstallResult";
    }

}
