using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Helpers;
using TheTechIdea.Beep.SetUp.Steps;
using TheTechIdea.Beep.Tools;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    /// <summary>
    /// Reusable handler that takes a datasource from "not present" (or
    /// "broken") to "ready to use" in a single call.
    ///
    /// Orchestrates, in order:
    ///   1. <b>Resolve driver</b> — either the override in
    ///      <see cref="DatasourceSetupOptions.DriverPackageName"/> or
    ///      <see cref="ConnectionHelper.GetBestMatchingDriver"/>.
    ///   2. <b>Provision driver</b> — if the resolved driver is missing,
    ///      download it from NuGet through
    ///      <see cref="IAssemblyHandler.LoadNuggetFromNuGetAsync"/>
    ///      (or load from local cache via
    ///      <see cref="IAssemblyHandler.LoadDriverFromLocalPackage"/>).
    ///   3. <b>Persist connection</b> — add or update the connection in
    ///      <see cref="IConfigEditor.DataConnections"/> and save the config.
    ///   4. <b>Open datasource</b> — when
    ///      <see cref="DatasourceSetupOptions.OpenConnection"/> is true.
    ///
    /// The handler is the single entry-point that any UI (WinForms, Blazor,
    /// console) can call to "set up this datasource". The WinForms
    /// <c>uc_SetupDriverStep</c> and the framework
    /// <see cref="DriverProvisionStep"/>+<see cref="ConnectionConfigStep"/>
    /// pipeline share the same building blocks.
    /// </summary>
    public sealed class DatasourceSetupHandler
    {
        private readonly IDMEEditor _editor;

        public DatasourceSetupHandler(IDMEEditor editor)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
        }

        /// <summary>
        /// Run the full setup pipeline for the configured datasource.
        /// Always returns a <see cref="DatasourceSetupResult"/> — failures
        /// are surfaced via the result, not by throwing.
        /// </summary>
        public async Task<DatasourceSetupResult> SetupAsync(
            DatasourceSetupOptions options,
            IProgress<(int Percent, string Message)>? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.ConnectionProperties == null)
                throw new ArgumentException("ConnectionProperties must be set.", nameof(options));
            if (string.IsNullOrWhiteSpace(options.ConnectionProperties.ConnectionName))
                throw new ArgumentException("ConnectionProperties.ConnectionName must be set.", nameof(options));

            var sw = Stopwatch.StartNew();
            var steps = new List<DatasourceSetupStep>();

            try
            {
                Report(progress, 2, "Preparing datasource setup…");

                // ── 0. Idempotency: already open? ────────────────────────────
                if (options.SkipWhenAlreadyOpen)
                {
                    var existing = TryGetOpenDataSource(options.ConnectionProperties.ConnectionName);
                    if (existing != null)
                    {
                        var skipMsg = $"Datasource '{options.ConnectionProperties.ConnectionName}' is already open.";
                        steps.Add(new DatasourceSetupStep("Skip-already-open", true, skipMsg, TimeSpan.Zero));
                        return DatasourceSetupResult.Ok(
                            skipMsg, existing,
                            driverProvisioned: false, connectionPersisted: false, connectionOpened: true,
                            sw.Elapsed, steps);
                    }
                }

                // ── 1. Resolve driver ─────────────────────────────────────────
                var resolveStart = Stopwatch.StartNew();
                ConnectionDriversConfig? driver = null;
                string resolveMsg;
                if (!string.IsNullOrWhiteSpace(options.DriverPackageName))
                {
                    driver = _editor.ConfigEditor?.DataDriversClasses?
                        .FirstOrDefault(d => string.Equals(
                            d.PackageName, options.DriverPackageName,
                            StringComparison.OrdinalIgnoreCase));
                    resolveMsg = driver != null
                        ? $"Driver '{options.DriverPackageName}' found in DataDriversClasses."
                        : $"Driver '{options.DriverPackageName}' not registered. Will be downloaded by package name.";
                }
                else
                {
                    driver = ConnectionHelper.GetBestMatchingDriver(
                        options.ConnectionProperties, _editor.ConfigEditor);
                    resolveMsg = driver != null
                        ? $"Resolved driver '{driver.PackageName}' for DatabaseType={options.ConnectionProperties.DatabaseType}."
                        : $"No driver registered for DatabaseType={options.ConnectionProperties.DatabaseType}.";
                }
                steps.Add(new DatasourceSetupStep("Resolve driver", true, resolveMsg, resolveStart.Elapsed));
                Report(progress, 15, resolveMsg);

                // ── 2. Provision driver (if missing) ──────────────────────────
                bool driverProvisioned = false;
                if (driver == null || driver.IsMissing)
                {
                    if (!options.ProvisionDriverIfMissing)
                    {
                        var errMsg = driver == null
                            ? "No matching driver is registered and ProvisionDriverIfMissing is false."
                            : $"Driver '{driver.PackageName}' is marked missing and ProvisionDriverIfMissing is false.";
                        steps.Add(new DatasourceSetupStep("Provision driver", false, errMsg, TimeSpan.Zero));
                        return DatasourceSetupResult.Fail(errMsg, steps, sw.Elapsed);
                    }

                    var provisionStart = Stopwatch.StartNew();
                    var provision = await ProvisionDriverAsync(
                        driver, options, progress, cancellationToken).ConfigureAwait(false);

                    steps.Add(new DatasourceSetupStep(
                        "Provision driver", provision.success, provision.message, provisionStart.Elapsed));

                    if (!provision.success)
                        return DatasourceSetupResult.Fail(provision.message, steps, sw.Elapsed, provision.exception);

                    driverProvisioned = true;

                    // After provisioning, re-resolve the driver config (it should now be registered).
                    driver = _editor.ConfigEditor?.DataDriversClasses?
                        .FirstOrDefault(d => string.Equals(
                            d.PackageName, options.DriverPackageName ?? provision.packageName,
                            StringComparison.OrdinalIgnoreCase))
                        ?? driver;
                }
                else
                {
                    steps.Add(new DatasourceSetupStep("Provision driver", true,
                        $"Driver '{driver.PackageName}' already loaded in-process.", TimeSpan.Zero));
                }

                // ── 3. Persist connection ────────────────────────────────────
                var persistStart = Stopwatch.StartNew();
                bool persisted;
                string persistMsg;
                try
                {
                    // Link connection to driver so ConnectionConfigStep can find it.
                    if (driver != null)
                    {
                        options.ConnectionProperties.DriverName = driver.PackageName;
                        if (!string.IsNullOrWhiteSpace(driver.version))
                            options.ConnectionProperties.DriverVersion = driver.version;
                    }

                    var storedConn = _editor.ConfigEditor?.DataConnections?
                        .FirstOrDefault(c => string.Equals(
                            c.ConnectionName, options.ConnectionProperties.ConnectionName,
                            StringComparison.OrdinalIgnoreCase));

                    persisted = storedConn != null
                        ? _editor.ConfigEditor?.UpdateDataConnection(options.ConnectionProperties, storedConn.GuidID) == true
                        : _editor.ConfigEditor?.AddDataConnection(options.ConnectionProperties) == true;

                    if (persisted)
                        _editor.ConfigEditor?.SaveDataconnectionsValues();

                    persistMsg = persisted
                        ? $"Connection '{options.ConnectionProperties.ConnectionName}' persisted."
                        : "Failed to persist connection (config editor returned false).";
                }
                catch (Exception ex)
                {
                    steps.Add(new DatasourceSetupStep("Persist connection", false, ex.Message, persistStart.Elapsed));
                    return DatasourceSetupResult.Fail($"Persist connection failed: {ex.Message}", steps, sw.Elapsed, ex);
                }

                steps.Add(new DatasourceSetupStep("Persist connection", persisted, persistMsg, persistStart.Elapsed));
                if (!persisted)
                    return DatasourceSetupResult.Fail(persistMsg, steps, sw.Elapsed);

                Report(progress, 70, persistMsg);

                // ── 4. Open datasource ───────────────────────────────────────
                if (!options.OpenConnection)
                {
                    steps.Add(new DatasourceSetupStep("Open datasource", true, "Skipped (OpenConnection=false).", TimeSpan.Zero));
                    return DatasourceSetupResult.Ok(
                        $"Datasource '{options.ConnectionProperties.ConnectionName}' setup (not opened).",
                        null, driverProvisioned, persisted, false, sw.Elapsed, steps);
                }

                var openStart = Stopwatch.StartNew();
                ConnectionState state;
                IDataSource? opened = null;
                string openMsg;
                try
                {
                    state = _editor.OpenDataSource(options.ConnectionProperties.ConnectionName);
                    opened = _editor.GetDataSource(options.ConnectionProperties.ConnectionName);
                }
                catch (Exception ex)
                {
                    steps.Add(new DatasourceSetupStep("Open datasource", false, ex.Message, openStart.Elapsed));
                    return DatasourceSetupResult.Fail($"Open datasource failed: {ex.Message}", steps, sw.Elapsed, ex);
                }

                bool openedOk = state == ConnectionState.Open && opened != null;
                openMsg = openedOk
                    ? $"Datasource '{options.ConnectionProperties.ConnectionName}' opened ({state})."
                    : $"Open returned {state} (no datasource handle).";

                steps.Add(new DatasourceSetupStep("Open datasource", openedOk, openMsg, openStart.Elapsed));
                Report(progress, 100, openMsg);

                if (!openedOk)
                    return DatasourceSetupResult.Fail(openMsg, steps, sw.Elapsed);

                var finalMsg = $"Datasource '{options.ConnectionProperties.ConnectionName}' ready in {sw.Elapsed.TotalSeconds:0.00}s.";
                return DatasourceSetupResult.Ok(
                    finalMsg, opened,
                    driverProvisioned, persisted, connectionOpened: true,
                    sw.Elapsed, steps);
            }
            catch (OperationCanceledException)
            {
                var msg = "Datasource setup was cancelled.";
                steps.Add(new DatasourceSetupStep("Cancellation", false, msg, sw.Elapsed));
                return DatasourceSetupResult.Fail(msg, steps, sw.Elapsed);
            }
            catch (Exception ex)
            {
                steps.Add(new DatasourceSetupStep("Unhandled error", false, ex.Message, sw.Elapsed));
                return DatasourceSetupResult.Fail($"Unhandled error: {ex.Message}", steps, sw.Elapsed, ex);
            }
        }

        // ── internals ─────────────────────────────────────────────────────

        private IDataSource? TryGetOpenDataSource(string connectionName)
        {
            try
            {
                var ds = _editor.GetDataSource(connectionName);
                return (ds != null && ds.ConnectionStatus == ConnectionState.Open) ? ds : null;
            }
            catch
            {
                return null;
            }
        }

        private async Task<(bool success, string message, string packageName, Exception? exception)>
            ProvisionDriverAsync(
                ConnectionDriversConfig? driver,
                DatasourceSetupOptions options,
                IProgress<(int Percent, string Message)>? progress,
                CancellationToken cancellationToken)
        {
            var handler = _editor.assemblyHandler;
            if (handler == null)
                return (false, "IDMEEditor.assemblyHandler is null. Cannot provision driver.", string.Empty, null);

            // Use the override, the resolved driver's name, or fail.
            var packageName = options.DriverPackageName
                              ?? driver?.PackageName;
            if (string.IsNullOrWhiteSpace(packageName))
                return (false, "No driver package name could be resolved.", string.Empty, null);

            // State 2 — package is cached on disk; try local first.
            if (driver != null && !driver.NuggetMissing)
            {
                Report(progress, 30, $"Loading driver from local package: {packageName}");
                try
                {
                    bool loaded = handler.LoadDriverFromLocalPackage(driver, out _);
                    if (loaded && !driver.IsMissing)
                    {
                        Report(progress, 80, "Driver loaded from local cache.");
                        try { _editor.ConfigEditor?.SaveConnectionDriversConfigValues(); } catch { /* non-fatal */ }
                        return (true, $"Driver '{packageName}' loaded from local cache.", packageName, null);
                    }
                }
                catch
                {
                    // fall through to download
                }
                driver.NuggetMissing = true;
            }

            // State 3 — download from NuGet.
            Report(progress, 40, $"Downloading NuGet package: {packageName}");

            IList<string>? sources = options.NuGetSources?.Any() == true
                ? options.NuGetSources.Where(s => !string.IsNullOrWhiteSpace(s)).ToList()
                : null;

            if (driver?.NuggetSource != null && !string.IsNullOrWhiteSpace(driver.NuggetSource))
            {
                sources ??= new List<string>();
                ((List<string>)sources).Insert(0, driver.NuggetSource);
            }

            try
            {
                var assemblies = await handler.LoadNuggetFromNuGetAsync(
                    packageName: packageName,
                    version: options.DriverVersion
                             ?? (string.IsNullOrWhiteSpace(driver?.NuggetVersion) ? null : driver!.NuggetVersion),
                    sources: sources,
                    useSingleSharedContext: true,
                    appInstallPath: options.AppInstallPath)
                    .ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                if (assemblies == null || assemblies.Count == 0)
                    return (false, $"NuGet download returned no assemblies for '{packageName}'.", packageName, null);

                Report(progress, 90, "Persisting driver configuration…");
                try { _editor.ConfigEditor?.SaveConnectionDriversConfigValues(); } catch { /* non-fatal */ }

                return (true, $"Driver '{packageName}' downloaded and loaded from NuGet.", packageName, null);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return (false, $"Failed to download driver '{packageName}': {ex.Message}",
                    packageName, ex);
            }
        }

        private static void Report(IProgress<(int Percent, string Message)>? progress, int pct, string message)
            => progress?.Report((pct, message));
    }
}
