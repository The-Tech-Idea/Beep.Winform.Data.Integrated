using System;
using System.Collections.Generic;
using System.ComponentModel;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Tools;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    /// <summary>
    /// Wizard context keys — the only "service" the wizard needs beyond the
    /// editor's <see cref="IAssemblyHandler"/> is a tiny in-memory activity log,
    /// kept here because the handler does not surface one to UI.
    /// </summary>
    internal static class NuggetWizardKeys
    {
        public const string Handler    = "Handler";
        public const string Log        = "ActivityLog";
        public const string PackageId  = "PackageId";
        public const string Version    = "SelectedVersion";
        public const string SourceUrl  = "SelectedSourceUrl";
        public const string IncludePre = "IncludePrerelease";
        public const string LoadAfter  = "LoadAfterInstall";
        public const string SharedCtx  = "SharedContext";
        public const string ProcessHost= "UseProcessHost";
        public const string InstallPath= "InstallPath";
        public const string InstallResult = "InstallResult";
    }

    /// <summary>
    /// Bundles the install options captured by the wizard's Options step.
    /// </summary>
    internal sealed class NuggetInstallRequest
    {
        public string PackageId   { get; set; } = string.Empty;
        public string Version     { get; set; } = string.Empty;
        public string SourceUrl   { get; set; } = string.Empty;
        public string InstallPath { get; set; } = string.Empty;
        public bool   LoadAfterInstall   { get; set; } = true;
        public bool   UseSharedContext   { get; set; } = true;
        public bool   UseProcessHost     { get; set; } = false;
    }

    /// <summary>
    /// One row in the in-memory activity log surfaced by the Installed step.
    /// The editor already keeps a <see cref="ComponentModel.BindingList{T}"/> of
    /// <see cref="ILogAndError"/> for the global log; this is the wizard-local
    /// tail of that stream.
    /// </summary>
    internal sealed class NuggetLogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string   Severity  { get; set; } = "Info";
        public string   Package   { get; set; } = string.Empty;
        public string   Message   { get; set; } = string.Empty;
    }

    /// <summary>
    /// Backing store for the wizard's activity log — a simple
    /// observable list that the Installed step renders.
    /// </summary>
    internal sealed class NuggetActivityLog
    {
        public BindingList<NuggetLogEntry> Entries { get; } = new BindingList<NuggetLogEntry>();

        public void Info(string message, string package = "")
            => Add("Info", message, package);

        public void Success(string message, string package = "")
            => Add("Success", message, package);

        public void Warn(string message, string package = "")
            => Add("Warning", message, package);

        public void Error(string message, string package = "")
            => Add("Error", message, package);

        public void Add(string severity, string message, string package = "")
        {
            if (Entries.Count > 500) Entries.RemoveAt(0); // cap the log
            Entries.Add(new NuggetLogEntry { Severity = severity, Message = message, Package = package });
        }

        public void Clear() => Entries.Clear();
    }

    /// <summary>
    /// Raised by the host control when a wizard install completes.
    /// </summary>
    public sealed class NuggetInstallCompletedEventArgs : EventArgs
    {
        public NuggetInstallCompletedEventArgs(string packageId, string version, bool success, string message)
        {
            PackageId = packageId;
            Version = version;
            Success = success;
            Message = message;
        }
        public string PackageId { get; }
        public string Version { get; }
        public bool Success { get; }
        public string Message { get; }
    }
}
