using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.NuGet;
using TheTechIdea.Beep.NuGetManagement;
using TheTechIdea.Beep.Tools;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    /// <summary>
    /// UI service for NuggetsManage that wraps NuGetPackageManager and assemblyHandler.
    /// Adds logging, progress reporting, and UI-specific state persistence.
    /// </summary>
    public sealed class NuggetsManageService : IDisposable
    {
        private const string DefaultNugetName = "nuget.org";
        private const string DefaultNugetUrl = "https://api.nuget.org/v3/index.json";
        private const int MaxLogEntries = 1000;

        private readonly IDMEEditor _editor;
        private readonly NuggetsManageStateStore _stateStore;
        private readonly List<NuggetOperationLog> _logs = new();
        private readonly HttpClient _httpClient;
        private bool _disposed;

        public NuggetsManageService(IDMEEditor editor)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _stateStore = new NuggetsManageStateStore(editor);
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        }

        #region Logging

        public IReadOnlyList<NuggetOperationLog> Logs => _logs.AsReadOnly();

        public void Log(NuggetLogSeverity severity, string message, string? packageId = null, string? version = null)
        {
            var entry = new NuggetOperationLog
            {
                Severity = severity,
                Message = message,
                PackageId = packageId,
                Version = version
            };
            _logs.Add(entry);

            // Prevent memory leak by limiting log size
            if (_logs.Count > MaxLogEntries)
            {
                _logs.RemoveRange(0, _logs.Count - MaxLogEntries);
            }
        }

        public void ClearLogs() => _logs.Clear();

        #endregion

        #region State Persistence

        public NuggetsPersistedState LoadState()
        {
            var state = _stateStore.Load();
            if (state.Sources.Count == 0)
            {
                state.Sources.Add(new NuGetSourceConfig { Name = DefaultNugetName, Url = DefaultNugetUrl, IsEnabled = true });
            }
            return state;
        }

        public void SaveState(NuggetsPersistedState state)
        {
            _stateStore.Save(state ?? new NuggetsPersistedState());
        }

        #endregion

        #region Sources

        public List<NuGetSourceConfig> GetAllSources()
        {
            var persisted = LoadState();
            var runtime = _editor.assemblyHandler.GetNuGetSources() ?? new List<NuGetSourceConfig>();

            foreach (var source in persisted.Sources)
            {
                if (runtime.Any(r => string.Equals(r.Name, source.Name, StringComparison.OrdinalIgnoreCase)))
                    continue;
                runtime.Add(source);
            }

            if (!runtime.Any(s => string.Equals(s.Name, DefaultNugetName, StringComparison.OrdinalIgnoreCase)))
            {
                runtime.Insert(0, new NuGetSourceConfig { Name = DefaultNugetName, Url = DefaultNugetUrl, IsEnabled = true });
            }

            return runtime
                .Where(s => !string.IsNullOrWhiteSpace(s.Url))
                .GroupBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();
        }

        public void AddSource(string name, string url, bool isEnabled = true)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url))
                return;

            _editor.assemblyHandler.AddNuGetSource(name.Trim(), url.Trim(), isEnabled);
            var state = LoadState();
            var existing = state.Sources.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
                state.Sources.Add(new NuGetSourceConfig { Name = name.Trim(), Url = url.Trim(), IsEnabled = isEnabled });
            else
            {
                existing.Url = url.Trim();
                existing.IsEnabled = isEnabled;
            }
            SaveState(state);
            Log(NuggetLogSeverity.Info, $"Source '{name}' added.");
        }

        public void RemoveSource(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            _editor.assemblyHandler.RemoveNuGetSource(name.Trim());
            var state = LoadState();
            state.Sources.RemoveAll(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
            if (state.Sources.Count == 0)
                state.Sources.Add(new NuGetSourceConfig { Name = DefaultNugetName, Url = DefaultNugetUrl, IsEnabled = true });
            SaveState(state);
            Log(NuggetLogSeverity.Info, $"Source '{name}' removed.");
        }

        public async Task<bool> TestSourceAsync(NuGetSourceConfig source)
        {
            if (source == null || string.IsNullOrWhiteSpace(source.Url))
                return false;

            try
            {
                if (source.IsLocal || Directory.Exists(source.Url))
                {
                    var exists = Directory.Exists(source.Url);
                    Log(exists ? NuggetLogSeverity.Success : NuggetLogSeverity.Error, $"Local source '{source.Name}': {(exists ? "exists" : "not found")}");
                    return exists;
                }

                var response = await _httpClient.GetAsync(source.Url);
                var healthy = response.IsSuccessStatusCode;
                Log(healthy ? NuggetLogSeverity.Success : NuggetLogSeverity.Error,
                    $"Source '{source.Name}' test: {(healthy ? "healthy" : $"failed ({(int)response.StatusCode})")}");
                return healthy;
            }
            catch (Exception ex)
            {
                Log(NuggetLogSeverity.Error, $"Source '{source.Name}' test failed: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Search & Versions

        public async Task<List<NuGetSearchResult>> SearchAsync(string term, bool includePrerelease, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new List<NuGetSearchResult>();

            Log(NuggetLogSeverity.Info, $"Searching for '{term}'...");
            try
            {
                var results = await _editor.assemblyHandler.SearchNuGetPackagesAsync(term.Trim(), 0, 30, includePrerelease, token);
                Log(NuggetLogSeverity.Success, $"Found {results.Count} packages for '{term}'.");
                return results;
            }
            catch (Exception ex)
            {
                Log(NuggetLogSeverity.Error, $"Search failed: {ex.Message}");
                throw;
            }
        }

        public async Task<List<string>> GetVersionsAsync(string packageId, bool includePrerelease, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                return new List<string>();

            try
            {
                return await _editor.assemblyHandler.GetNuGetPackageVersionsAsync(packageId.Trim(), includePrerelease, token);
            }
            catch (Exception ex)
            {
                Log(NuggetLogSeverity.Error, $"Version lookup failed for '{packageId}': {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Install / Load / Unload / Remove

        public async Task<NuggetInstallResult> InstallAsync(NuggetInstallRequest request, IProgress<string>? progress = null, CancellationToken token = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.PackageId))
            {
                Log(NuggetLogSeverity.Error, "Install failed: Package id is required.");
                return new NuggetInstallResult { Success = false, Message = "Package id is required." };
            }

            progress?.Report($"Installing {request.PackageId}...");
            Log(NuggetLogSeverity.Info, $"Installing '{request.PackageId}' v{request.Version}...", request.PackageId, request.Version);

            try
            {
                var sources = request.Sources?.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase);
                var loaded = await _editor.assemblyHandler.LoadNuggetFromNuGetAsync(
                    request.PackageId,
                    string.IsNullOrWhiteSpace(request.Version) ? null : request.Version,
                    sources,
                    request.UseSingleSharedContext,
                    string.IsNullOrWhiteSpace(request.AppInstallPath) ? null : request.AppInstallPath,
                    request.UseProcessHost).ConfigureAwait(false);

                var loadedCount = loaded?.Count ?? 0;
                var success = loadedCount > 0 || request.LoadAfterInstall == false;

                UpsertInstalledState(new NuggetInstalledState
                {
                    PackageId = request.PackageId,
                    Version = request.Version ?? string.Empty,
                    Source = request.Sources?.FirstOrDefault() ?? string.Empty,
                    InstallPath = request.AppInstallPath ?? string.Empty,
                    IsLoaded = loadedCount > 0,
                    IsEnabledAtStartup = request.LoadAfterInstall,
                    LastUpdatedUtc = DateTime.UtcNow
                });

                var message = success
                    ? $"Package '{request.PackageId}' installed ({loadedCount} assemblies)."
                    : $"Package '{request.PackageId}' did not load any assemblies.";

                Log(success ? NuggetLogSeverity.Success : NuggetLogSeverity.Warning, message, request.PackageId, request.Version);
                progress?.Report(message);

                return new NuggetInstallResult
                {
                    Success = success,
                    Message = message,
                    LoadedAssembliesCount = loadedCount,
                    CompletedAtUtc = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                Log(NuggetLogSeverity.Error, $"Install failed: {ex.Message}", request.PackageId, request.Version);
                progress?.Report($"Install failed: {ex.Message}");
                return new NuggetInstallResult
                {
                    Success = false,
                    Message = $"Install failed: {ex.Message}",
                    CompletedAtUtc = DateTime.UtcNow
                };
            }
        }

        public bool LoadNugget(string pathOrPackageId)
        {
            if (string.IsNullOrWhiteSpace(pathOrPackageId))
                return false;

            var loaded = _editor.assemblyHandler.LoadNugget(pathOrPackageId);
            Log(loaded ? NuggetLogSeverity.Success : NuggetLogSeverity.Error, $"Load '{pathOrPackageId}': {(loaded ? "success" : "failed")}", pathOrPackageId);
            if (loaded)
                MarkLoaded(pathOrPackageId, true);
            return loaded;
        }

        public bool UnloadNugget(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                return false;

            var unloaded = _editor.assemblyHandler.UnloadNugget(packageId);
            Log(unloaded ? NuggetLogSeverity.Success : NuggetLogSeverity.Error, $"Unload '{packageId}': {(unloaded ? "success" : "failed")}", packageId);
            if (unloaded)
                MarkLoaded(packageId, false);
            return unloaded;
        }

        public async Task<bool> RemoveAsync(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                return false;

            Log(NuggetLogSeverity.Info, $"Removing '{packageId}'...", packageId);

            // Get install path BEFORE removing from state
            var state = LoadState();
            var installed = state.InstalledStates.FirstOrDefault(s => 
                string.Equals(s.PackageId, packageId, StringComparison.OrdinalIgnoreCase));
            var installPath = installed?.InstallPath;

            // Unload first
            _editor.assemblyHandler.UnloadNugget(packageId);

            // Remove from state
            state.InstalledStates.RemoveAll(s => string.Equals(s.PackageId, packageId, StringComparison.OrdinalIgnoreCase));
            SaveState(state);

            // Try to delete files if we know the path
            if (!string.IsNullOrWhiteSpace(installPath) && Directory.Exists(installPath))
            {
                try
                {
                    Directory.Delete(installPath, true);
                }
                catch (Exception ex)
                {
                    Log(NuggetLogSeverity.Warning, $"Could not delete install path: {ex.Message}", packageId);
                }
            }

            Log(NuggetLogSeverity.Success, $"Removed '{packageId}'.", packageId);
            return true;
        }

        public async Task<NuggetInstallResult> UpdateAsync(string packageId, string? version = null, CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(packageId))
            {
                Log(NuggetLogSeverity.Error, "Update failed: Package id is required.");
                return new NuggetInstallResult { Success = false, Message = "Package id is required." };
            }

            Log(NuggetLogSeverity.Info, $"Updating '{packageId}' to {(version ?? "latest")}...", packageId);

            try
            {
                var result = await _editor.assemblyHandler.UpdateNuGetPackageAsync(packageId, version);
                Log(result.Success ? NuggetLogSeverity.Success : NuggetLogSeverity.Error,
                    $"Update '{packageId}': {(result.Success ? $"updated to {result.NewVersion}" : $"failed - {result.Error}")}", packageId);

                if (result.Success && result.WasUpdated)
                {
                    UpsertInstalledState(new NuggetInstalledState
                    {
                        PackageId = packageId,
                        Version = result.NewVersion ?? version ?? string.Empty,
                        LastUpdatedUtc = DateTime.UtcNow,
                        IsLoaded = true
                    });
                }

                return new NuggetInstallResult
                {
                    Success = result.Success,
                    Message = result.Error ?? $"Updated to {result.NewVersion}",
                    CompletedAtUtc = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                Log(NuggetLogSeverity.Error, $"Update failed: {ex.Message}", packageId);
                return new NuggetInstallResult { Success = false, Message = $"Update failed: {ex.Message}" };
            }
        }

        #endregion

        #region Installed State Management

        public List<NuggetInstalledState> GetInstalledStates()
        {
            var state = LoadState();
            return state.InstalledStates.OrderByDescending(s => s.LastUpdatedUtc).ToList();
        }

        public void UpsertInstalledState(NuggetInstalledState installedState)
        {
            if (installedState == null || string.IsNullOrWhiteSpace(installedState.PackageId))
                return;

            var state = LoadState();
            var existing = state.InstalledStates.FirstOrDefault(s =>
                string.Equals(s.PackageId, installedState.PackageId, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
                state.InstalledStates.Add(installedState);
            else
            {
                existing.Version = installedState.Version;
                existing.Source = installedState.Source;
                existing.InstallPath = installedState.InstallPath;
                existing.IsLoaded = installedState.IsLoaded;
                existing.IsEnabledAtStartup = installedState.IsEnabledAtStartup;
                existing.LastUpdatedUtc = installedState.LastUpdatedUtc;
            }

            SaveState(state);
        }

        public void SetStartupEnabled(string packageId, bool enabled)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                return;

            var state = LoadState();
            var existing = state.InstalledStates.FirstOrDefault(s =>
                string.Equals(s.PackageId, packageId, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.IsEnabledAtStartup = enabled;
                existing.LastUpdatedUtc = DateTime.UtcNow;
                SaveState(state);
                Log(NuggetLogSeverity.Info, $"Startup {(enabled ? "enabled" : "disabled")} for '{packageId}'.", packageId);
            }
        }

        public bool IsAssemblyLoaded(string packageOrAssemblyName)
        {
            if (string.IsNullOrWhiteSpace(packageOrAssemblyName))
                return false;

            var candidate = packageOrAssemblyName.Trim();
            return _editor.assemblyHandler.LoadedAssemblies.Any(asm =>
                string.Equals(asm.GetName().Name, candidate, StringComparison.OrdinalIgnoreCase));
        }

        private void MarkLoaded(string packageId, bool loaded)
        {
            var state = LoadState();
            var installed = state.InstalledStates.FirstOrDefault(s =>
                string.Equals(s.PackageId, packageId, StringComparison.OrdinalIgnoreCase));

            if (installed == null)
            {
                installed = new NuggetInstalledState { PackageId = packageId };
                state.InstalledStates.Add(installed);
            }

            installed.IsLoaded = loaded;
            installed.LastUpdatedUtc = DateTime.UtcNow;
            SaveState(state);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _httpClient.Dispose();
            }
        }

        #endregion
    }
}
