using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Environments;
using TheTechIdea.Beep.Services.AppMap;

namespace TheTechIdea.Beep.Winform.Controls
{
    /// <summary>
    /// Manages environment-scoped connections by delegating to the framework's
    /// <see cref="IEnvironmentManagementService"/> for environment discovery and
    /// <see cref="BeepDataConnection"/> for connection CRUD. No double-writes,
    /// no hardcoded environments.
    /// </summary>
    public sealed class EnvironmentConnectionManager
    {
        private readonly BeepDataConnection _dataConnection;
        private readonly TheTechIdea.Beep.Editor.IDMEEditor _editor;
        private readonly object _switchLock = new();

        public event EventHandler<string>? EnvironmentChanged;

        public string CurrentEnvironment
        {
            get => _dataConnection.ActiveProfileName;
            set
            {
                lock (_switchLock)
                {
                    if (string.Equals(_dataConnection.ActiveProfileName, value, StringComparison.OrdinalIgnoreCase))
                        return;

                    _dataConnection.ActiveProfileName = value;
                    _dataConnection.ReloadConnections();
                }
                EnvironmentChanged?.Invoke(this, value);
            }
        }

        public EnvironmentConnectionManager(BeepDataConnection dataConnection)
        {
            _dataConnection = dataConnection ?? throw new ArgumentNullException(nameof(dataConnection));
            _editor = dataConnection.BeepService?.DMEEditor
                ?? throw new InvalidOperationException("BeepDataConnection must have an initialized BeepService with DMEEditor.");
        }

        /// <summary>
        /// Returns environment names from the framework's
        /// <see cref="IEnvironmentManagementService"/> standard tiers,
        /// falling back to BeepService.Environments keys if available.
        /// </summary>
        public IReadOnlyList<string> GetEnvironments()
        {
            var editorEnv = _editor.Environment;
            if (editorEnv != null)
            {
                var tiers = editorEnv.GetStandardTiers();
                if (tiers != null && tiers.Count > 0)
                {
                    return tiers.OrderBy(t => t.Order).Select(t => t.Name).ToList();
                }
            }

            var beepService = _dataConnection.BeepService;
            if (beepService?.Environments != null && beepService.Environments.Count > 0)
            {
                return beepService.Environments.Keys.Select(k => k.ToString()).ToList();
            }

            return new[] { "Default" };
        }

        public string GetCurrentDisplayName()
        {
            var editorEnv = _editor.Environment;
            if (editorEnv != null)
            {
                var tiers = editorEnv.GetStandardTiers();
                var match = tiers?.FirstOrDefault(t =>
                    string.Equals(t.Name, CurrentEnvironment, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    return match.Name;
                }
            }

            return CurrentEnvironment;
        }

        public IReadOnlyList<ConnectionProperties> GetConnectionsForEnvironment(string environment)
        {
            lock (_switchLock)
            {
                var previous = _dataConnection.ActiveProfileName;
                try
                {
                    _dataConnection.ActiveProfileName = environment;
                    return _dataConnection.GetConnectionsSnapshot(includeRepository: true);
                }
                finally
                {
                    _dataConnection.ActiveProfileName = previous;
                }
            }
        }

        public void SwitchTo(string environment)
        {
            CurrentEnvironment = environment;
        }

        public List<string> GetConnectionNames()
        {
            return _dataConnection.DataConnections
                .Where(c => !string.IsNullOrWhiteSpace(c.ConnectionName))
                .Select(c => c.ConnectionName!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public ConnectionProperties? GetConnection(string connectionName)
        {
            return _dataConnection.DataConnections
                .FirstOrDefault(c => string.Equals(c.ConnectionName, connectionName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Adds a connection to the specified environment profile.
        /// All persistence flows through BeepDataConnection (catalog repository or ConfigEditor).
        /// No double-write to ConfigEditor.
        /// </summary>
        public bool AddConnection(string environment, ConnectionProperties connection)
        {
            lock (_switchLock)
            {
                var previous = _dataConnection.ActiveProfileName;
                try
                {
                    _dataConnection.ActiveProfileName = environment;
                    return _dataConnection.AddOrUpdateConnection(connection);
                }
                finally
                {
                    _dataConnection.ActiveProfileName = previous;
                }
            }
        }

        /// <summary>
        /// Removes a connection from the specified environment profile.
        /// All persistence flows through BeepDataConnection (catalog repository or ConfigEditor).
        /// No double-write to ConfigEditor.
        /// </summary>
        public bool RemoveConnection(string environment, string connectionName)
        {
            lock (_switchLock)
            {
                var previous = _dataConnection.ActiveProfileName;
                try
                {
                    _dataConnection.ActiveProfileName = environment;
                    return _dataConnection.RemoveConnection(connectionName);
                }
                finally
                {
                    _dataConnection.ActiveProfileName = previous;
                }
            }
        }

        public int GetConnectionCount() => _dataConnection.DataConnections.Count;
    }
}
