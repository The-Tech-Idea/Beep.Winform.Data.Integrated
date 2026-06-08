
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing.Design;
using System.Threading;
using System.Threading.Tasks;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Helpers;
using TheTechIdea.Beep.Services;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Winform.Controls.Converters;

namespace TheTechIdea.Beep.Winform.Controls
{
    [Designer("TheTechIdea.Beep.Winform.Controls.Design.Server.Designers.BeepDataConnectionDesigner, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public class BeepDataConnection : Component, INotifyPropertyChanged
    {
        private IBeepService? _beepService;
        private BeepConnectionRepository? _connectionRepository;
        private EventHandler? _repositoryChangedHandler;
        private bool _ownsBeepService = true;

        public IBeepService? BeepService => _beepService;
        public event EventHandler? ConnectionsChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public BeepDataConnection()
        {
            InitializeBeepService();
            DataConnections = new List<ConnectionProperties>();
            InitializeRepository();
            ReloadConnections();
        }

        [Browsable(true)]
        [Category("Persistence")]
        [Description("Shared repository/container name used by BeepService configuration.")]
        [DefaultValue("BeepPlatformConnections")]
        public string AppRepoName
        {
            get => _appRepoName;
            set
            {
                var normalized = string.IsNullOrWhiteSpace(value) ? "BeepPlatformConnections" : value.Trim();
                if (string.Equals(_appRepoName, normalized, StringComparison.Ordinal))
                {
                    return;
                }

                _appRepoName = normalized;
                OnStorageConfigurationChanged();
                OnPropertyChanged(nameof(AppRepoName));
            }
        }

        [Browsable(true)]
        [Category("Persistence")]
        [Description("Base directory used by BeepService configuration. Leave empty to use application base directory.")]
        [DefaultValue("")]
        public string DirectoryPath
        {
            get => _directoryPath;
            set
            {
                var normalized = value?.Trim() ?? string.Empty;
                if (string.Equals(_directoryPath, normalized, StringComparison.Ordinal))
                {
                    return;
                }

                _directoryPath = normalized;
                OnStorageConfigurationChanged();
                OnPropertyChanged(nameof(DirectoryPath));
            }
        }

        [Browsable(true)]
        [Category("Persistence")]
        [Description("Logical storage scope used for keying shared design-time contexts.")]
        [DefaultValue(ConnectionStorageScope.Project)]
        public ConnectionStorageScope PersistenceScope
        {
            get => _persistenceScope;
            set
            {
                if (_persistenceScope == value)
                {
                    return;
                }

                _persistenceScope = value;
                ApplyRepositorySettings();
                OnPropertyChanged(nameof(PersistenceScope));
            }
        }

        [Browsable(true)]
        [Category("Persistence")]
        [Description("Active profile used to group named connection sets.")]
        [DefaultValue("Default")]
        public string ActiveProfileName
        {
            get => _activeProfileName;
            set
            {
                var normalized = string.IsNullOrWhiteSpace(value) ? "Default" : value.Trim();
                if (string.Equals(_activeProfileName, normalized, StringComparison.Ordinal))
                {
                    return;
                }

                _activeProfileName = normalized;
                ApplyRepositorySettings();
                OnPropertyChanged(nameof(ActiveProfileName));
            }
        }

        [Browsable(true)]
        [Category("Persistence")]
        [Description("When true, loads Project->User->Machine fallback chain for the selected profile.")]
        [DefaultValue(true)]
        public bool UseScopePrecedence
        {
            get => _useScopePrecedence;
            set
            {
                if (_useScopePrecedence == value)
                {
                    return;
                }

                _useScopePrecedence = value;
                ApplyRepositorySettings();
                OnPropertyChanged(nameof(UseScopePrecedence));
            }
        }

        [Browsable(true)]
        [Category("Validation")]
        [Description("When true, editor requires successful test connection before save.")]
        [DefaultValue(true)]
        public bool RequireSuccessfulTestBeforeSave { get; set; } = true;

        [Browsable(true)]
        [Category("Connections")]
        [Editor(typeof(CollectionEditor), typeof(UITypeEditor))]
        public List<ConnectionProperties> DataConnections { get; private set; }

        [Browsable(true)]
        [Category("Current Connection")]
        [TypeConverter(typeof(DataConnectionConverter))]
        public ConnectionProperties? CurrentConnection { get; set; }
        private ConnectionStorageScope _persistenceScope = ConnectionStorageScope.Project;
        private string _appRepoName = "BeepPlatformConnections";
        private string _directoryPath = string.Empty;
        private string _activeProfileName = "Default";
        private bool _useScopePrecedence = true;

        /// <summary>
        /// Reloads connections from shared config when available; otherwise keeps local list.
        /// </summary>
        public IReadOnlyList<ConnectionProperties> ReloadConnections()
        {
            if (!TryLoadFromRepository())
            {
                if (IsInDesignTime())
                {
                    LoadDesignTimeConnections();
                }
            }

            return DataConnections.AsReadOnly();
        }

        /// <summary>
        /// Returns a non-destructive snapshot of known connections.
        /// By default this does not force repository reload to avoid clobbering in-memory edits.
        /// </summary>
        public IReadOnlyList<ConnectionProperties> GetConnectionsSnapshot(bool includeRepository = false)
        {
            if (includeRepository)
            {
                TryLoadFromRepository();
            }

            return new List<ConnectionProperties>(DataConnections).AsReadOnly();
        }

        /// <summary>
        /// Adds or updates a connection and persists it to DataConnections.json when service is available.
        /// </summary>
        public bool AddOrUpdateConnection(ConnectionProperties connection, bool persist = true)
        {
            if (connection == null || string.IsNullOrWhiteSpace(connection.ConnectionName))
            {
                return false;
            }

            bool changed;
            if (_connectionRepository != null)
            {
                ApplyRepositorySettings();
                changed = _connectionRepository.AddOrUpdate(connection, persist);
                if (changed)
                {
                    SetLocalConnections(_connectionRepository.LoadConnections());
                }
            }
            else
            {
                changed = AddOrUpdateLocalConnection(connection);
            }

            if (changed)
            {
                RaiseConnectionsChanged();
            }

            return changed;
        }

        /// <summary>
        /// Removes a connection by name and persists changes when service is available.
        /// </summary>
        public bool RemoveConnection(string connectionName, bool persist = true)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                return false;
            }

            bool removed;
            if (_connectionRepository != null)
            {
                ApplyRepositorySettings();
                removed = _connectionRepository.Remove(connectionName, persist);
                if (removed)
                {
                    SetLocalConnections(_connectionRepository.LoadConnections());
                }
            }
            else
            {
                removed = RemoveLocalConnection(connectionName);
            }

            if (removed)
            {
                RaiseConnectionsChanged();
            }

            return removed;
        }

        /// <summary>
        /// Persists the current local connection list to shared configuration.
        /// </summary>
        public bool SaveConnections()
        {
            if (_connectionRepository == null)
            {
                return false;
            }

            ApplyRepositorySettings();
            var saved = _connectionRepository.Save(new List<ConnectionProperties>(DataConnections));
            if (saved)
            {
                SetLocalConnections(_connectionRepository.LoadConnections());
                RaiseConnectionsChanged();
            }

            return saved;
        }

        /// <summary>
        /// Replaces the component service context with a shared host-managed service.
        /// </summary>
        public void AttachSharedBeepService(IBeepService beepService, bool reloadConnections = true)
        {
            if (_ownsBeepService && _beepService is IDisposable disposableOwnedService)
            {
                disposableOwnedService.Dispose();
            }

            _beepService = beepService ?? throw new ArgumentNullException(nameof(beepService));
            _ownsBeepService = false;
            EnsureDriverCatalogHydrated();
            InitializeRepository();

            if (reloadConnections)
            {
                ReloadConnections();
            }
        }

        /// <summary>
        /// Initializes the BeepService for design-time or runtime use.
        /// </summary>
        private void InitializeBeepService()
        {
            try
            {
                // Designer load should stay lightweight and resilient. The design-server
                // manager attaches a shared service lease when available.
                if (IsInDesignTime())
                {
                    _beepService = null;
                    _ownsBeepService = false;
                    return;
                }

                var service = new BeepService();
                var directory = string.IsNullOrWhiteSpace(DirectoryPath) ? AppContext.BaseDirectory : DirectoryPath;
                var appRepo = string.IsNullOrWhiteSpace(AppRepoName) ? "BeepPlatformConnections" : AppRepoName;
                service.Configure(directory, appRepo, BeepConfigType.DataConnector, false);
                _beepService = service;
                _ownsBeepService = true;
                EnsureDriverCatalogHydrated();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[BeepDataConnection.InitializeBeepService] {ex.GetType().Name}: {ex.Message}");
                _beepService = null;
                _ownsBeepService = false;
            }
        }

        private bool TryLoadFromRepository()
        {
            if (_connectionRepository == null)
            {
                return false;
            }

            try
            {
                ApplyRepositorySettings();
                SetLocalConnections(_connectionRepository.LoadConnections());
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[BeepDataConnection.TryLoadFromRepository] Failed to load connections: {ex.GetType().Name}: {ex.Message}");
                return false;
            }
        }

        private void OnStorageConfigurationChanged()
        {
            ReconfigureOwnedBeepService();
            InitializeRepository();
            TryLoadFromRepository();
        }

        private void ReconfigureOwnedBeepService()
        {
            if (!_ownsBeepService || _beepService is not BeepService service)
            {
                return;
            }

            var directory = string.IsNullOrWhiteSpace(DirectoryPath) ? AppContext.BaseDirectory : DirectoryPath;
            var appRepo = string.IsNullOrWhiteSpace(AppRepoName) ? "BeepPlatformConnections" : AppRepoName;
            service.Configure(directory, appRepo, BeepConfigType.DataConnector, false);
        }

        private void InitializeRepository()
        {
            if (_connectionRepository != null && _repositoryChangedHandler != null)
            {
                _connectionRepository.ConnectionsChanged -= _repositoryChangedHandler;
            }

            if (_beepService == null)
            {
                _connectionRepository = null;
                _repositoryChangedHandler = null;
                return;
            }

            _connectionRepository = new BeepConnectionRepository(_beepService);
            ApplyRepositorySettings();
            _repositoryChangedHandler = (_, _) =>
            {
                SetLocalConnections(_connectionRepository?.LoadConnections());
                RaiseConnectionsChanged();
            };
            _connectionRepository.ConnectionsChanged += _repositoryChangedHandler;
        }

        private void RaiseConnectionsChanged()
        {
            OnPropertyChanged(nameof(DataConnections));
            ConnectionsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void EnsureDriverCatalogHydrated()
        {
            var editor = _beepService?.DMEEditor;
            var configEditor = editor?.ConfigEditor ?? _beepService?.Config_editor;
            if (editor == null || configEditor == null)
            {
                return;
            }

            try
            {
                editor.AddAllConnectionConfigurations();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[BeepDataConnection.EnsureDriverCatalogHydrated] AddAllConnectionConfigurations failed: {ex.GetType().Name}: {ex.Message}");
            }

            try
            {
                configEditor.LoadConnectionDriversConfigValues();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[BeepDataConnection.EnsureDriverCatalogHydrated] LoadConnectionDriversConfigValues failed: {ex.GetType().Name}: {ex.Message}");
            }

            configEditor.DataDriversClasses ??= new List<ConnectionDriversConfig>();
            var changed = false;
            var defaults = ConnectionHelper.GetAllConnectionConfigs();
            foreach (var candidate in defaults)
            {
                if (candidate == null || string.IsNullOrWhiteSpace(candidate.PackageName))
                {
                    continue;
                }

                var exists = configEditor.DataDriversClasses.Any(existing => DriverCatalogItemMatches(existing, candidate));
                if (exists)
                {
                    continue;
                }

                configEditor.DataDriversClasses.Add(candidate);
                changed = true;
            }

            if (!changed)
            {
                return;
            }

            try
            {
                configEditor.SaveConnectionDriversConfigValues();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[BeepDataConnection.EnsureDriverCatalogHydrated] SaveConnectionDriversConfigValues failed: {ex.GetType().Name}: {ex.Message}");
            }
        }

        private static bool DriverCatalogItemMatches(ConnectionDriversConfig existing, ConnectionDriversConfig candidate)
        {
            if (existing == null || candidate == null)
            {
                return false;
            }

            if (!string.Equals(existing.PackageName, candidate.PackageName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return existing.DatasourceType == candidate.DatasourceType;
        }

        /// <summary>
        /// Loads design-time connections from the BeepService configuration.
        /// </summary>
        private void LoadDesignTimeConnections()
        {
            var configEditor = _beepService?.Config_editor;
            if (configEditor == null)
            {
                return;
            }

            var loaded = configEditor.LoadDataConnectionsValues() ?? configEditor.DataConnections;
            SetLocalConnections(loaded);
        }

        private void SetLocalConnections(IEnumerable<ConnectionProperties>? connections)
        {
            DataConnections.Clear();
            if (connections != null)
            {
                DataConnections.AddRange(connections);
            }

            if (CurrentConnection == null ||
                !DataConnections.Any(c =>
                    !string.IsNullOrWhiteSpace(c.GuidID) &&
                    !string.IsNullOrWhiteSpace(CurrentConnection.GuidID) &&
                    string.Equals(c.GuidID, CurrentConnection.GuidID, StringComparison.OrdinalIgnoreCase)))
            {
                CurrentConnection = DataConnections.FirstOrDefault();
            }

            OnPropertyChanged(nameof(DataConnections));
            OnPropertyChanged(nameof(CurrentConnection));
        }

        private bool AddOrUpdateLocalConnection(ConnectionProperties connection)
        {
            if (string.IsNullOrWhiteSpace(connection.GuidID))
            {
                connection.GuidID = Guid.NewGuid().ToString("D");
            }

            var existing = DataConnections.FirstOrDefault(c =>
                (!string.IsNullOrWhiteSpace(c.GuidID) &&
                 string.Equals(c.GuidID, connection.GuidID, StringComparison.OrdinalIgnoreCase)) ||
                string.Equals(c.ConnectionName, connection.ConnectionName, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                DataConnections.Add(connection);
                if (CurrentConnection == null)
                {
                    CurrentConnection = connection;
                }

                return true;
            }

            var index = DataConnections.IndexOf(existing);
            DataConnections[index] = connection;
            CurrentConnection = connection;
            return true;
        }

        private bool RemoveLocalConnection(string connectionName)
        {
            var existing = DataConnections.FirstOrDefault(c =>
                !string.IsNullOrWhiteSpace(c.ConnectionName) &&
                string.Equals(c.ConnectionName, connectionName, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                return false;
            }

            DataConnections.Remove(existing);
            if (CurrentConnection == existing)
            {
                CurrentConnection = DataConnections.FirstOrDefault();
            }

            return true;
        }

        /// <summary>
        /// Determines whether the current context is design-time.
        /// </summary>
        private static bool IsInDesignTime()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }

        /// <summary>
        /// Mirrors the Oracle Forms <c>TEST_CONNECTION</c> built-in.
        /// Opens a short-lived <see cref="IDataSource"/> against the named
        /// connection and reports success / failure with a human-readable
        /// message. Throws are caught and translated to <c>success=false</c>.
        /// </summary>
        public bool TestConnection(string connectionName, out string message)
        {
            message = string.Empty;
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                message = "Connection name is required.";
                return false;
            }

            var connection = DataConnections.FirstOrDefault(c =>
                !string.IsNullOrWhiteSpace(c.ConnectionName) &&
                string.Equals(c.ConnectionName, connectionName, StringComparison.OrdinalIgnoreCase));
            if (connection == null)
            {
                message = $"Connection '{connectionName}' is not registered.";
                return false;
            }

            if (_beepService == null)
            {
                message = "BeepService is not available — cannot test connection.";
                return false;
            }

            try
            {
                var ds = _beepService.DMEEditor?.GetDataSource(connectionName);
                if (ds == null)
                {
                    message = $"No data source registered for connection '{connectionName}'.";
                    return false;
                }

                // Fire the ON-LOGON trigger equivalent. A handler that
                // cancels via ConnectionLifecycleEventArgs.Cancel aborts the
                // open, mirroring the Forms ON-ERROR-after-LOGON path.
                var lifecycle = new ConnectionLifecycleEventArgs(connectionName, DateTime.UtcNow);
                RaiseLogonStarting(connectionName);
                if (lifecycle.Cancel)
                {
                    message = $"Connection '{connectionName}' logon cancelled by handler.";
                    UpdateConnectionState(connectionName, BeepConnectionState.Failed, message);
                    RaiseLogoffRequested(connectionName, "cancelled by handler");
                    return false;
                }

                bool opened;
                using (ds)
                {
                    var rawState = ds.Openconnection();
                    opened = rawState == System.Data.ConnectionState.Open;
                }
                if (opened)
                {
                    message = $"Connection '{connectionName}' opened successfully.";
                    UpdateConnectionState(connectionName, BeepConnectionState.Connected, message);
                    RaiseLogonCompleted(connectionName, message);
                    return true;
                }

                message = $"Connection '{connectionName}' failed to open. Check connection string and credentials.";
                UpdateConnectionState(connectionName, BeepConnectionState.Failed, message);
                RaiseLogoffRequested(connectionName, message);
                return false;
            }
            catch (Exception ex)
            {
                message = $"Connection '{connectionName}' threw {ex.GetType().Name}: {ex.Message}";
                UpdateConnectionState(connectionName, BeepConnectionState.Failed, message);
                RaiseLogoffRequested(connectionName, message);
                return false;
            }
        }

        /// <summary>
        /// Asynchronous variant of <see cref="TestConnection"/>. Safe to call
        /// from UI thread — does not block the message loop.
        /// </summary>
        public async Task<bool> TestConnectionAsync(string connectionName, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => TestConnection(connectionName, out _), cancellationToken)
                .ConfigureAwait(false);
        }

        private readonly Dictionary<string, BeepConnectionState> _connectionStates = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DateTime> _lastConnectedAt = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _connectionStateLock = new();

        /// <summary>
        /// Returns the most recently observed connection state for a named
        /// connection. Defaults to <see cref="BeepConnectionState.Unknown"/> when
        /// no test or open has been performed.
        /// </summary>
        public BeepConnectionState GetConnectionState(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                return BeepConnectionState.Unknown;
            }
            lock (_connectionStateLock)
            {
                return _connectionStates.TryGetValue(connectionName, out var s) ? s : BeepConnectionState.Unknown;
            }
        }

        /// <summary>
        /// Timestamp of the last successful connection open. <c>null</c>
        /// when the connection has never been opened successfully.
        /// </summary>
        public DateTime? GetLastConnectedAt(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                return null;
            }
            lock (_connectionStateLock)
            {
                return _lastConnectedAt.TryGetValue(connectionName, out var t) ? t : (DateTime?)null;
            }
        }

        private void UpdateConnectionState(string connectionName, BeepConnectionState state, string? message)
        {
            DateTime timestamp = DateTime.UtcNow;
            lock (_connectionStateLock)
            {
                _connectionStates[connectionName] = state;
                if (state == BeepConnectionState.Connected)
                {
                    _lastConnectedAt[connectionName] = timestamp;
                }
            }
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(connectionName, state, message, timestamp));
        }

        /// <summary>
        /// Raised whenever <see cref="TestConnection"/> (or the connection
        /// lifecycle code) reports a new state. Subscribers can update the UI
        /// to show the green / red indicator Oracle Forms shows next to a
        /// connection name.
        /// </summary>
        public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

        // ── Oracle Forms ON-LOGON / ON-LOGOFF triggers ──────────────────

        /// <summary>
        /// Raised immediately before the data source for a named connection
        /// is opened. Hosts / developers can subscribe to fire the
        /// <c>ON-LOGON</c> trigger equivalent (typically used to initialize
        /// per-connection state in Oracle Forms apps). Returning
        /// <c>false</c> from a handler by setting
        /// <see cref="ConnectionLifecycleEventArgs.Cancel"/> aborts the
        /// open.
        /// </summary>
        public event EventHandler<ConnectionLifecycleEventArgs>? LogonStarting;

        /// <summary>
        /// Raised after the data source has been opened successfully.
        /// Mirrors the <c>POST-LOGON</c> trigger Oracle Forms fires once
        /// the user is connected.
        /// </summary>
        public event EventHandler<ConnectionLifecycleEventArgs>? LogonCompleted;

        /// <summary>
        /// Raised when the data source for a named connection is closed
        /// (or fails to open). Mirrors the <c>ON-LOGOFF</c> trigger.
        /// </summary>
        public event EventHandler<ConnectionLifecycleEventArgs>? LogoffRequested;

        private void RaiseLogonStarting(string connectionName)
        {
            var args = new ConnectionLifecycleEventArgs(connectionName, DateTime.UtcNow);
            try { LogonStarting?.Invoke(this, args); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepDataConnection.LogonStarting] {ex.Message}"); }
        }

        private void RaiseLogonCompleted(string connectionName, string? message)
        {
            try { LogonCompleted?.Invoke(this, new ConnectionLifecycleEventArgs(connectionName, DateTime.UtcNow, message)); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepDataConnection.LogonCompleted] {ex.Message}"); }
        }

        private void RaiseLogoffRequested(string connectionName, string? reason)
        {
            try { LogoffRequested?.Invoke(this, new ConnectionLifecycleEventArgs(connectionName, DateTime.UtcNow, reason)); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[BeepDataConnection.LogoffRequested] {ex.Message}"); }
        }

        public bool PromoteConnections(ConnectionStoreKind source, ConnectionStoreKind target, ConnectionConflictPolicy conflictPolicy, out string message)
        {
            message = string.Empty;
            if (_connectionRepository == null)
            {
                message = "Repository is not available.";
                return false;
            }

            if (source == target)
            {
                message = "Source and target stores must differ.";
                return false;
            }

            var sourceScope = MapStoreToScope(source);
            var targetScope = MapStoreToScope(target);
            _connectionRepository.ActiveScope = sourceScope;
            _connectionRepository.ActiveProfileName = ActiveProfileName;
            var promoted = _connectionRepository.Promote(targetScope, conflictPolicy, out message);
            _connectionRepository.ActiveScope = PersistenceScope;
            ApplyRepositorySettings();
            if (promoted)
            {
                ReloadConnections();
            }

            return promoted;
        }

        public bool ExportEmbeddedDefaults(string packagePath, bool includeEncryptedSecretsOnly, out string message)
        {
            message = string.Empty;
            if (_connectionRepository == null)
            {
                message = "Repository is not available.";
                return false;
            }

            ApplyRepositorySettings();
            return _connectionRepository.ExportPackage(packagePath, includeEncryptedSecretsOnly, out message);
        }

        public bool ImportEmbeddedDefaults(string packagePath, ConnectionConflictPolicy conflictPolicy, bool importWhenEmptyOnly, out string message)
        {
            message = string.Empty;
            if (_connectionRepository == null)
            {
                message = "Repository is not available.";
                return false;
            }

            ApplyRepositorySettings();
            var imported = _connectionRepository.ImportPackage(packagePath, conflictPolicy, importWhenEmptyOnly, out message);
            if (imported)
            {
                ReloadConnections();
            }

            return imported;
        }

        public bool PromoteCurrentToShared(ConnectionConflictPolicy conflictPolicy, out string message)
        {
            return PromoteConnections(ConnectionStoreKind.ProjectLocal, ConnectionStoreKind.Shared, conflictPolicy, out message);
        }

        public bool DemoteSharedToProject(ConnectionConflictPolicy conflictPolicy, out string message)
        {
            return PromoteConnections(ConnectionStoreKind.Shared, ConnectionStoreKind.ProjectLocal, conflictPolicy, out message);
        }

        private void ApplyRepositorySettings()
        {
            if (_connectionRepository == null)
            {
                return;
            }

            _connectionRepository.ActiveScope = PersistenceScope;
            _connectionRepository.ActiveProfileName = ActiveProfileName;
            _connectionRepository.UseScopePrecedence = UseScopePrecedence;
        }

        private static ConnectionStorageScope MapStoreToScope(ConnectionStoreKind storeKind)
        {
            return storeKind switch
            {
                ConnectionStoreKind.Shared => ConnectionStorageScope.User,
                _ => ConnectionStorageScope.Project
            };
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_connectionRepository != null && _repositoryChangedHandler != null)
                {
                    _connectionRepository.ConnectionsChanged -= _repositoryChangedHandler;
                }

                if (_ownsBeepService && _beepService is IDisposable disposableService)
                {
                    disposableService.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
