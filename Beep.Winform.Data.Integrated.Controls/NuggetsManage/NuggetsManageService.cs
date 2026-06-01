using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.NuggetsManage
{
    internal sealed class NuggetsManageService
    {
        private readonly IDMEEditor _editor;
        private readonly NuggetStateStore _stateStore;

        public NuggetsManageService(IDMEEditor editor)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _stateStore = new NuggetStateStore(editor);
        }

        public List<NuggetItemState> LoadPersistedStates()
        {
            return _stateStore.Load();
        }

        public void SaveStates(IEnumerable<NuggetItemState> states)
        {
            _stateStore.Save(states);
        }

        public List<NuggetItemState> ScanNuggets(IEnumerable<string> additionalRoots = null)
        {
            var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            AddIfDirectory(roots, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConnectionDrivers"));
            AddIfDirectory(roots, Path.Combine(_editor.ConfigEditor?.ExePath ?? AppDomain.CurrentDomain.BaseDirectory, "ConnectionDrivers"));

            if (additionalRoots != null)
            {
                foreach (var root in additionalRoots)
                {
                    AddIfDirectory(roots, root);
                }
            }

            var states = new List<NuggetItemState>();
            foreach (var root in roots)
            {
                states.AddRange(EnumerateNuggets(root));
            }

            return states
                .GroupBy(state => state.SourcePath, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(state => state.NuggetName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public NuggetOperationResult LoadNugget(NuggetItemState nugget)
        {
            if (!TryValidateNuggetPath(nugget, out var validationError))
            {
                return NuggetOperationResult.Fail(validationError, NuggetOperationSeverity.Warn);
            }

            try
            {
                var loaded = _editor.assemblyHandler.LoadNugget(nugget.SourcePath);
                nugget.IsLoaded = loaded;
                nugget.IsMissing = false;
                nugget.LastUpdatedUtc = DateTime.UtcNow;
                if (!loaded)
                {
                    return NuggetOperationResult.Fail($"Failed to load nugget '{nugget.NuggetName}'.");
                }

                return NuggetOperationResult.Ok($"Loaded nugget '{nugget.NuggetName}'.");
            }
            catch (Exception ex)
            {
                return NuggetOperationResult.Fail($"Load failed for '{nugget.NuggetName}': {ex.Message}");
            }
        }

        public NuggetOperationResult UnloadNugget(NuggetItemState nugget)
        {
            if (nugget == null || string.IsNullOrWhiteSpace(nugget.NuggetName))
            {
                return NuggetOperationResult.Fail("Select a nugget first.", NuggetOperationSeverity.Warn);
            }

            try
            {
                var unloaded = _editor.assemblyHandler.UnloadNugget(nugget.NuggetName);
                nugget.IsLoaded = !unloaded;
                nugget.LastUpdatedUtc = DateTime.UtcNow;
                if (!unloaded)
                {
                    return NuggetOperationResult.Fail($"Unload failed for '{nugget.NuggetName}'.", NuggetOperationSeverity.Warn);
                }

                return NuggetOperationResult.Ok($"Unloaded nugget '{nugget.NuggetName}'.");
            }
            catch (Exception ex)
            {
                return NuggetOperationResult.Fail($"Unload failed for '{nugget.NuggetName}': {ex.Message}");
            }
        }

        public NuggetOperationResult InstallFromPath(string sourcePath, List<NuggetItemState> existingStates)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                return NuggetOperationResult.Fail("Path is required.", NuggetOperationSeverity.Warn);
            }

            if (!File.Exists(sourcePath) && !Directory.Exists(sourcePath))
            {
                return NuggetOperationResult.Fail("Install path does not exist.", NuggetOperationSeverity.Warn);
            }

            try
            {
                var installRoot = GetInstallDirectory();
                Directory.CreateDirectory(installRoot);

                if (File.Exists(sourcePath))
                {
                    var copiedPath = CopyFileToInstallRoot(sourcePath, installRoot);
                    UpsertState(existingStates, BuildState(copiedPath, "Installed"));
                }
                else
                {
                    foreach (var file in Directory.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories)
                                 .Where(IsNuggetFile))
                    {
                        var copiedPath = CopyFileToInstallRoot(file, installRoot);
                        UpsertState(existingStates, BuildState(copiedPath, "Installed"));
                    }
                }

                return NuggetOperationResult.Ok("Nugget package files installed to app nugget directory.");
            }
            catch (Exception ex)
            {
                return NuggetOperationResult.Fail($"Install failed: {ex.Message}");
            }
        }

        public NuggetOperationResult RestoreEnabledNuggets(List<NuggetItemState> states)
        {
            if (states == null || states.Count == 0)
            {
                return NuggetOperationResult.Ok("No persisted nuggets to restore.");
            }

            var restored = 0;
            foreach (var state in states.Where(state => state.IsEnabled))
            {
                if (!TryValidateNuggetPath(state, out _))
                {
                    state.IsMissing = true;
                    continue;
                }

                try
                {
                    var ok = _editor.assemblyHandler.LoadNugget(state.SourcePath);
                    state.IsLoaded = ok;
                    state.IsMissing = !ok;
                    state.LastUpdatedUtc = DateTime.UtcNow;
                    if (ok)
                    {
                        restored++;
                    }
                }
                catch
                {
                    state.IsLoaded = false;
                    state.IsMissing = true;
                }
            }

            return NuggetOperationResult.Ok($"Restored {restored} enabled nuggets.");
        }

        private static void AddIfDirectory(HashSet<string> roots, string directoryPath)
        {
            if (!string.IsNullOrWhiteSpace(directoryPath) && Directory.Exists(directoryPath))
            {
                roots.Add(directoryPath);
            }
        }

        private static IEnumerable<NuggetItemState> EnumerateNuggets(string rootPath)
        {
            return Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)
                .Where(IsNuggetFile)
                .Select(filePath => BuildState(filePath, rootPath));
        }

        private static bool IsNuggetFile(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            return extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".nupkg", StringComparison.OrdinalIgnoreCase);
        }

        private static NuggetItemState BuildState(string sourcePath, string source)
        {
            var name = Path.GetFileNameWithoutExtension(sourcePath);
            var version = ExtractVersionFromFileName(Path.GetFileName(sourcePath));
            return new NuggetItemState
            {
                NuggetName = name,
                SourcePath = sourcePath,
                NuggetVersion = version,
                NuggetSource = source,
                IsLoaded = false,
                IsEnabled = false,
                IsMissing = false,
                LastUpdatedUtc = DateTime.UtcNow
            };
        }

        private static string ExtractVersionFromFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return string.Empty;
            }

            var tokens = fileName.Split('.');
            return tokens.Length > 2 ? tokens[^2] : string.Empty;
        }

        private static void UpsertState(ICollection<NuggetItemState> states, NuggetItemState newState)
        {
            var existing = states.FirstOrDefault(state =>
                state.SourcePath.Equals(newState.SourcePath, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                states.Add(newState);
                return;
            }

            existing.NuggetName = newState.NuggetName;
            existing.NuggetVersion = newState.NuggetVersion;
            existing.NuggetSource = newState.NuggetSource;
            existing.IsMissing = false;
            existing.LastUpdatedUtc = DateTime.UtcNow;
        }

        private string GetInstallDirectory()
        {
            var basePath = _editor.ConfigEditor?.ExePath;
            if (string.IsNullOrWhiteSpace(basePath))
            {
                basePath = AppDomain.CurrentDomain.BaseDirectory;
            }

            return Path.Combine(basePath, "ConnectionDrivers", "InstalledNuggets");
        }

        private static string CopyFileToInstallRoot(string sourceFilePath, string installRoot)
        {
            var targetPath = Path.Combine(installRoot, Path.GetFileName(sourceFilePath));
            File.Copy(sourceFilePath, targetPath, overwrite: true);
            return targetPath;
        }

        private static bool TryValidateNuggetPath(NuggetItemState nugget, out string validationError)
        {
            validationError = string.Empty;
            if (nugget == null || string.IsNullOrWhiteSpace(nugget.SourcePath))
            {
                validationError = "Nugget path is empty.";
                return false;
            }

            if (!File.Exists(nugget.SourcePath) && !Directory.Exists(nugget.SourcePath))
            {
                nugget.IsMissing = true;
                validationError = $"Nugget path not found: {nugget.SourcePath}";
                return false;
            }

            return true;
        }
    }
}
