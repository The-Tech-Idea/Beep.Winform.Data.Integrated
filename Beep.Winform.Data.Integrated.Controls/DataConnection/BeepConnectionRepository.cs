using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Services;

namespace TheTechIdea.Beep.Winform.Controls
{
    /// <summary>
    /// Centralized connection persistence helper backed by IBeepService.Config_editor.
    /// Keeps connection CRUD and save/reload behavior consistent across forms.
    /// </summary>
    public sealed class BeepConnectionRepository
    {
        private readonly IBeepService _beepService;
        private readonly IConnectionStorageProvider _storageProvider;
        private readonly object _syncRoot = new();

        public event EventHandler? ConnectionsChanged;
        public ConnectionStorageScope ActiveScope { get; set; } = ConnectionStorageScope.Project;
        public string ActiveProfileName { get; set; } = "Default";
        public bool UseScopePrecedence { get; set; } = true;

        public BeepConnectionRepository(IBeepService beepService, IConnectionStorageProvider? storageProvider = null)
        {
            _beepService = beepService ?? throw new ArgumentNullException(nameof(beepService));
            _storageProvider = storageProvider ?? new JsonConnectionStorageProvider(_beepService);
        }

        public IReadOnlyList<ConnectionProperties> LoadConnections()
        {
            lock (_syncRoot)
            {
                return _storageProvider.LoadConnections(ActiveScope, ActiveProfileName, UseScopePrecedence);
            }
        }

        public bool AddOrUpdate(ConnectionProperties connection, bool persist = true)
        {
            if (connection == null || string.IsNullOrWhiteSpace(connection.ConnectionName))
            {
                return false;
            }

            lock (_syncRoot)
            {
                EnsureConnectionDefaults(connection);
                var changed = _storageProvider.AddOrUpdate(ActiveScope, ActiveProfileName, connection, persist);

                if (!changed)
                {
                    return false;
                }

                ConnectionsChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
        }

        public bool Remove(string connectionName, bool persist = true)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                return false;
            }

            lock (_syncRoot)
            {
                var removed = _storageProvider.Remove(ActiveScope, ActiveProfileName, connectionName, persist);
                if (!removed)
                {
                    return false;
                }

                ConnectionsChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
        }

        public bool Save(List<ConnectionProperties> connections)
        {
            lock (_syncRoot)
            {
                var saved = _storageProvider.SaveConnections(ActiveScope, ActiveProfileName, connections ?? new List<ConnectionProperties>());
                if (!saved)
                {
                    return false;
                }

                ConnectionsChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
        }

        public bool Promote(ConnectionStorageScope targetScope, ConnectionConflictPolicy conflictPolicy, out string message)
        {
            lock (_syncRoot)
            {
                var ok = _storageProvider.Promote(ActiveScope, targetScope, ActiveProfileName, conflictPolicy, out message);
                if (ok)
                {
                    ConnectionsChanged?.Invoke(this, EventArgs.Empty);
                }

                return ok;
            }
        }

        public bool ExportPackage(string packagePath, bool includeEncryptedSecretsOnly, out string message)
        {
            lock (_syncRoot)
            {
                return _storageProvider.ExportPackage(ActiveScope, ActiveProfileName, packagePath, includeEncryptedSecretsOnly, out message);
            }
        }

        public bool ImportPackage(
            string packagePath,
            ConnectionConflictPolicy conflictPolicy,
            bool importWhenEmptyOnly,
            out string message)
        {
            lock (_syncRoot)
            {
                var ok = _storageProvider.ImportPackage(ActiveScope, ActiveProfileName, packagePath, conflictPolicy, importWhenEmptyOnly, out message);
                if (ok)
                {
                    ConnectionsChanged?.Invoke(this, EventArgs.Empty);
                }

                return ok;
            }
        }

        private static void EnsureConnectionDefaults(ConnectionProperties connection)
        {
            if (string.IsNullOrWhiteSpace(connection.GuidID))
            {
                connection.GuidID = Guid.NewGuid().ToString("D");
            }
        }
    }
}
