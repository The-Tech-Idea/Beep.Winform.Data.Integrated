
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
        private IConnectionCatalogRepository? _connectionRepository;
        private EventHandler? _repositoryChangedHandler;
        private bool _ownsBeepService = false;

        public IBeepService? BeepService => _beepService;
        public event EventHandler? ConnectionsChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Default constructor for WinForms designer and runtime use.
        /// At design-time, the service stays null until the design-server
        /// manager attaches a shared lease via <see cref="AttachSharedBeepService"/>.
        /// At runtime, a self-owned BeepService is created; hosts should call
        /// <see cref="AttachSharedBeepService"/> to inject a shared instance.
        /// </summary>
        public BeepDataConnection()
        {
            DataConnections = new List<ConnectionProperties>();
            InitializeBeepService();
            InitializeRepository();
            ReloadConnections();
        }

        public BeepDataConnection(IBeepService beepService) : this()
        {
            AttachSharedBeepService(beepService);
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
        /// Reloads connections from the catalog repository when available;
        /// falls back to ConfigEditor.DataConnections when no repository is set.
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
        /// Adds or updates a connection and persists it through the catalog repository
        /// or ConfigEditor when available.
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
                changed = AddOrUpdateViaConfigEditor(connection, persist);
            }

            if (changed)
            {
                RaiseConnectionsChanged();
            }

            return changed;
        }

        /// <summary>
        /// Removes a connection by name and persists changes through the catalog repository
        /// or ConfigEditor when available.
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
                removed = RemoveViaConfigEditor(connectionName, persist);
            }

            if (removed)
            {
                RaiseConnectionsChanged();
            }

            return removed;
        }

        /// <summary>
        /// Persists the current connection list through the catalog repository.
        /// </summary>
        public bool SaveConnections()
        {
            if (_connectionRepository == null)
            {
                return SaveViaConfigEditor();
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
        /// Initializes the BeepService for runtime use. At design-time, the service
        /// remains null until the design-server manager attaches a shared lease.
        /// </summary>
        private void InitializeBeepService()
        {
            if (IsInDesignTime())
            {
                _beepService = null;
                _ownsBeepService = false;
                return;
            }

            try
            {
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

            // Use the shared catalog repository from ConfigEditor, or create a local one.
            var editor = _beepService.DMEEditor;
            var configEditor = editor?.ConfigEditor ?? _beepService.Config_editor;
            _connectionRepository = configEditor?.ConnectionCatalogRepository
                ?? new BeepConnectionRepository(_beepService);
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

        /// <summary>
        /// Ensures the driver catalog is hydrated via the framework's
        /// EnvironmentService.AddAllConnectionConfigurations. This delegates
        /// driver-catalog management to the engine layer.
        /// </summary>
        private void EnsureDriverCatalogHydrated()
        {
            var editor = _beepService?.DMEEditor;
            if (editor == null)
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
        }

        /// <summary>
        /// Loads design-time connections from ConfigEditor.DataConnections.
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

        /// <summary>
        /// Fallback CRUD via ConfigEditor when no catalog repository is available.
        /// Uses the framework's ConfigEditor.AddDataConnection / RemoveDataConnection.
        /// </summary>
        private bool AddOrUpdateViaConfigEditor(ConnectionProperties connection, bool persist)
        {
            var configEditor = _beepService?.Config_editor;
            if (configEditor == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(connection.GuidID))
            {
                connection.GuidID = Guid.NewGuid().ToString("D");
            }

            var existing = configEditor.DataConnections?.FirstOrDefault(c =>
                (!string.IsNullOrWhiteSpace(c.GuidID) &&
                 string.Equals(c.GuidID, connection.GuidID, StringComparison.OrdinalIgnoreCase)) ||
                string.Equals(c.ConnectionName, connection.ConnectionName, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                configEditor.UpdateDataConnection(connection, existing.GuidID);
            }
            else
            {
                configEditor.AddDataConnection(connection);
            }

            if (persist)
            {
                configEditor.SaveDataconnectionsValues();
            }

            SetLocalConnections(configEditor.DataConnections);
            return true;
        }

        /// <summary>
        /// Fallback removal via ConfigEditor when no catalog repository is available.
        /// </summary>
        private bool RemoveViaConfigEditor(string connectionName, bool persist)
        {
            var configEditor = _beepService?.Config_editor;
            if (configEditor == null)
            {
                return false;
            }

            var existing = configEditor.DataConnections?.FirstOrDefault(c =>
                !string.IsNullOrWhiteSpace(c.ConnectionName) &&
                string.Equals(c.ConnectionName, connectionName, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                return false;
            }

            configEditor.RemoveDataConnection(connectionName);
            if (persist)
            {
                configEditor.SaveDataconnectionsValues();
            }

            SetLocalConnections(configEditor.DataConnections);
            return true;
        }

        /// <summary>
        /// Fallback save via ConfigEditor when no catalog repository is available.
        /// </summary>
        private bool SaveViaConfigEditor()
        {
            var configEditor = _beepService?.Config_editor;
            if (configEditor == null)
            {
                return false;
            }

            configEditor.SaveDataconnectionsValues();
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
        /// Opens a short-lived <see cref="IDataSource"/> against the named
        /// connection and reports success / failure with a human-readable
        /// message.
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
                LogonStarting?.Invoke(this, lifecycle);
                if (lifecycle.Cancel)
                {
                    message = $"Connection '{connectionName}' logon cancelled by handler.";
                    UpdateConnectionState(connectionName, BeepConnectionLifecycle.Failed, message);
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
                    UpdateConnectionState(connectionName, BeepConnectionLifecycle.Connected, message);
                    RaiseLogonCompleted(connectionName, message);
                    return true;
                }

                message = $"Connection '{connectionName}' failed to open. Check connection string and credentials.";
                UpdateConnectionState(connectionName, BeepConnectionLifecycle.Failed, message);
                RaiseLogoffRequested(connectionName, message);
                return false;
            }
            catch (Exception ex)
            {
                message = $"Connection '{connectionName}' threw {ex.GetType().Name}: {ex.Message}";
                UpdateConnectionState(connectionName, BeepConnectionLifecycle.Failed, message);
                RaiseLogoffRequested(connectionName, message);
                return false;
            }
        }

        /// <summary>
        /// Asynchronous variant of <see cref="TestConnection"/>.
        /// </summary>
        public async Task<bool> TestConnectionAsync(string connectionName, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => TestConnection(connectionName, out _), cancellationToken)
                .ConfigureAwait(false);
        }

        private readonly Dictionary<string, BeepConnectionLifecycle> _connectionStates = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DateTime> _lastConnectedAt = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _connectionStateLock = new();

        /// <summary>
        /// Returns the most recently observed connection state for a named
        /// connection. Defaults to <see cref="BeepConnectionLifecycle.Unknown"/> when
        /// no test or open has been performed.
        /// </summary>
        public BeepConnectionLifecycle GetConnectionState(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                return BeepConnectionLifecycle.Unknown;
            }
            lock (_connectionStateLock)
            {
                return _connectionStates.TryGetValue(connectionName, out var s) ? s : BeepConnectionLifecycle.Unknown;
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

        private void UpdateConnectionState(string connectionName, BeepConnectionLifecycle state, string? message)
        {
            DateTime timestamp = DateTime.UtcNow;
            lock (_connectionStateLock)
            {
                _connectionStates[connectionName] = state;
                if (state == BeepConnectionLifecycle.Connected)
                {
                    _lastConnectedAt[connectionName] = timestamp;
                }
            }
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(connectionName, state, message, timestamp));
        }

        /// <summary>
        /// Raised whenever <see cref="TestConnection"/> reports a new state.
        /// </summary>
        public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

        // ── Oracle Forms ON-LOGON / ON-LOGOFF triggers ──────────────────

        /// <summary>
        /// Raised immediately before the data source for a named connection
        /// is opened. Set <see cref="ConnectionLifecycleEventArgs.Cancel"/> to abort.
        /// </summary>
        public event EventHandler<ConnectionLifecycleEventArgs>? LogonStarting;

        /// <summary>
        /// Raised after the data source has been opened successfully.
        /// </summary>
        public event EventHandler<ConnectionLifecycleEventArgs>? LogonCompleted;

        /// <summary>
        /// Raised when the data source for a named connection is closed
        /// (or fails to open).
        /// </summary>
        public event EventHandler<ConnectionLifecycleEventArgs>? LogoffRequested;

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
