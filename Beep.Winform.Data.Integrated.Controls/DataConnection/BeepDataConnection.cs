
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Helpers;
using TheTechIdea.Beep.Services;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Winform.Controls.Converters;

namespace TheTechIdea.Beep.Winform.Controls
{
    public enum ConnectionStorageScope
    {
        Project,
        User,
        Machine
    }

    [Designer("TheTechIdea.Beep.Winform.Controls.Design.Server.Designers.BeepDataConnectionDesigner, TheTechIdea.Beep.Winform.Controls.Design.Server")]
    public class BeepDataConnection : Component
    {
        private IBeepService? _beepService;
        private BeepConnectionRepository? _connectionRepository;
        private EventHandler? _repositoryChangedHandler;
        private bool _ownsBeepService = true;

        public IBeepService? BeepService => _beepService;
        public event EventHandler? ConnectionsChanged;

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
                _persistenceScope = value;
                ApplyRepositorySettings();
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
                _activeProfileName = string.IsNullOrWhiteSpace(value) ? "Default" : value.Trim();
                ApplyRepositorySettings();
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
                _useScopePrecedence = value;
                ApplyRepositorySettings();
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
        public List<ConnectionProperties> DataConnections { get; set; }

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
                ConnectionsChanged?.Invoke(this, EventArgs.Empty);
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
                ConnectionsChanged?.Invoke(this, EventArgs.Empty);
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
                ConnectionsChanged?.Invoke(this, EventArgs.Empty);
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
            catch
            {
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

            ApplyRepositorySettings();
            SetLocalConnections(_connectionRepository.LoadConnections());
            return true;
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
                ConnectionsChanged?.Invoke(this, EventArgs.Empty);
            };
            _connectionRepository.ConnectionsChanged += _repositoryChangedHandler;
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
            catch
            {
                // Keep design/runtime initialization resilient.
            }

            try
            {
                configEditor.LoadConnectionDriversConfigValues();
            }
            catch
            {
                // Keep design/runtime initialization resilient.
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
            catch
            {
                // Persistence is best-effort; skip on design hosts that do not allow writes.
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
