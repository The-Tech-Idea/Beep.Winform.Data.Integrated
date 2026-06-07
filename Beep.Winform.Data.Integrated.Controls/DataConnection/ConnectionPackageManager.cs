using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TheTechIdea.Beep.ConfigUtil;

namespace TheTechIdea.Beep.Winform.Controls
{
    public sealed class ConnectionPackageResult
    {
        public bool Success { get; set; }
        public string? FilePath { get; set; }
        public string? Message { get; set; }
        public int ConnectionCount { get; set; }

        public static ConnectionPackageResult Ok(string path, int count)
            => new() { Success = true, FilePath = path, ConnectionCount = count, Message = $"Exported {count} connection(s)." };

        public static ConnectionPackageResult Fail(string message)
            => new() { Success = false, Message = message };
    }

    public sealed class ConnectionPackageManager
    {
        private readonly BeepDataConnection _dataConnection;
        private readonly TheTechIdea.Beep.Editor.IDMEEditor? _editor;

        public ConnectionPackageManager(BeepDataConnection dataConnection)
        {
            _dataConnection = dataConnection ?? throw new ArgumentNullException(nameof(dataConnection));
            _editor = dataConnection.BeepService?.DMEEditor;
        }

        public ConnectionPackageResult Import(string filePath, ConnectionConflictPolicy conflictPolicy = ConnectionConflictPolicy.Skip, bool importWhenEmptyOnly = false)
        {
            if (!File.Exists(filePath))
                return ConnectionPackageResult.Fail($"Package file not found: {filePath}");

            if (_dataConnection.ImportEmbeddedDefaults(filePath, conflictPolicy, importWhenEmptyOnly, out var message))
            {
                _dataConnection.ReloadConnections();

                // Sync imported connections to ConfigEditor for global availability
                var configEditor = _editor?.ConfigEditor;
                if (configEditor != null)
                {
                    foreach (var conn in _dataConnection.DataConnections)
                    {
                        var existing = configEditor.DataConnections?
                            .FirstOrDefault(c => string.Equals(c.GuidID, conn.GuidID, StringComparison.OrdinalIgnoreCase));
                        if (existing == null)
                            configEditor.AddDataConnection(conn);
                    }
                    configEditor.SaveDataconnectionsValues();
                }

                var connections = _dataConnection.DataConnections;
                return ConnectionPackageResult.Ok(filePath, connections.Count);
            }
            return ConnectionPackageResult.Fail(message);
        }

        public ConnectionPackageResult Export(string filePath, bool includeEncryptedSecretsOnly = true)
        {
            if (_dataConnection.ExportEmbeddedDefaults(filePath, includeEncryptedSecretsOnly, out var message))
            {
                var connections = _dataConnection.DataConnections;
                return ConnectionPackageResult.Ok(filePath, connections.Count);
            }
            return ConnectionPackageResult.Fail(message);
        }

        public ConnectionPackageResult PromoteToShared(ConnectionConflictPolicy conflictPolicy = ConnectionConflictPolicy.MergeByGuid)
        {
            if (_dataConnection.PromoteConnections(ConnectionStoreKind.ProjectLocal, ConnectionStoreKind.Shared, conflictPolicy, out var message))
            {
                var connections = _dataConnection.DataConnections;
                return ConnectionPackageResult.Ok("shared", connections.Count);
            }
            return ConnectionPackageResult.Fail(message);
        }

        public ConnectionPackageResult DemoteToProject(ConnectionConflictPolicy conflictPolicy = ConnectionConflictPolicy.MergeByGuid)
        {
            if (_dataConnection.DemoteSharedToProject(conflictPolicy, out var message))
            {
                var connections = _dataConnection.DataConnections;
                return ConnectionPackageResult.Ok("project", connections.Count);
            }
            return ConnectionPackageResult.Fail(message);
        }

        public PackagePreviewResult Preview(string filePath)
        {
            if (!File.Exists(filePath))
                return PackagePreviewResult.Fail("File not found.");

            try
            {
                var json = File.ReadAllText(filePath);
                var package = JsonSerializer.Deserialize<ConnectionCatalogPackage>(json);
                if (package?.Records == null || package.Records.Count == 0)
                    return PackagePreviewResult.Fail("Package contains no connections.");

                var preview = new PackagePreviewResult
                {
                    Success = true,
                    TotalConnections = package.Records.Count,
                    ProfileName = package.ProfileName ?? "Default",
                    ConnectionNames = package.Records
                        .Where(r => r.Connection != null)
                        .Select(r => r.Connection!.ConnectionName ?? "Unknown")
                        .ToList(),
                    ExistingConflicts = _dataConnection.DataConnections
                        .Select(c => c.ConnectionName)
                        .Intersect(package.Records
                            .Where(r => r.Connection != null)
                            .Select(r => r.Connection!.ConnectionName!), StringComparer.OrdinalIgnoreCase)
                        .ToList()
                };
                return preview;
            }
            catch (Exception ex)
            {
                return PackagePreviewResult.Fail($"Failed to read package: {ex.Message}");
            }
        }
    }

    public sealed class PackagePreviewResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int TotalConnections { get; set; }
        public string? ProfileName { get; set; }
        public List<string> ConnectionNames { get; set; } = new();
        public List<string> ExistingConflicts { get; set; } = new();

        public static PackagePreviewResult Fail(string message) => new() { Success = false, Message = message };
    }
}
