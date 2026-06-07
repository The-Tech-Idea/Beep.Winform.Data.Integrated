using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.ConfigUtil;

namespace TheTechIdea.Beep.Winform.Controls
{
    public sealed class EnvironmentConnectionManager
    {
        private readonly BeepDataConnection _dataConnection;
        private readonly TheTechIdea.Beep.Editor.IDMEEditor _editor;
        private readonly object _switchLock = new();
        private readonly Dictionary<string, ConnectionEnvironmentProfile> _profiles = new(StringComparer.OrdinalIgnoreCase);

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

        public IReadOnlyList<string> GetEnvironments() =>
            new[] { "Development", "Staging", "Production" };

        public string GetCurrentDisplayName() => CurrentEnvironment switch
        {
            "Development" => "🛠 Development",
            "Staging" => "🧪 Staging",
            "Production" => "🚀 Production",
            _ => $"📁 {CurrentEnvironment}"
        };

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

        public bool AddConnection(string environment, ConnectionProperties connection)
        {
            lock (_switchLock)
            {
                var previous = _dataConnection.ActiveProfileName;
                try
                {
                    _dataConnection.ActiveProfileName = environment;
                    _dataConnection.AddOrUpdateConnection(connection);
                    _dataConnection.SaveConnections();

                    // Also sync through ConfigEditor for global availability
                    var configEditor = _editor.ConfigEditor;
                    if (configEditor != null)
                    {
                        if (string.IsNullOrWhiteSpace(connection.GuidID))
                            connection.GuidID = Guid.NewGuid().ToString("D");
                        configEditor.AddDataConnection(connection);
                        configEditor.SaveDataconnectionsValues();
                    }

                    return true;
                }
                finally
                {
                    _dataConnection.ActiveProfileName = previous;
                }
            }
        }

        public bool RemoveConnection(string environment, string connectionName)
        {
            lock (_switchLock)
            {
                var previous = _dataConnection.ActiveProfileName;
                try
                {
                    _dataConnection.ActiveProfileName = environment;
                    _dataConnection.RemoveConnection(connectionName);
                    _dataConnection.SaveConnections();

                    // Also remove from ConfigEditor for global consistency
                    var configEditor = _editor.ConfigEditor;
                    if (configEditor != null)
                    {
                        var existing = configEditor.DataConnections?
                            .FirstOrDefault(c => string.Equals(c.ConnectionName, connectionName, StringComparison.OrdinalIgnoreCase));
                        if (existing != null)
                        {
                            configEditor.DataConnections!.Remove(existing);
                            configEditor.SaveDataconnectionsValues();
                        }
                    }

                    return true;
                }
                finally
                {
                    _dataConnection.ActiveProfileName = previous;
                }
            }
        }

        public int GetConnectionCount() => _dataConnection.DataConnections.Count;
    }

    public sealed class ConnectionEnvironmentProfile
    {
        public string Environment { get; set; } = "Development";
        public string DisplayName { get; set; } = string.Empty;
        public List<ConnectionProperties> Connections { get; set; } = new();
    }
}
