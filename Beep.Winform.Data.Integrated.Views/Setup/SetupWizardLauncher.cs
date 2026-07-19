using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.SetUp;
using TheTechIdea.Beep.SetUp.Adapters;
using TheTechIdea.Beep.SetUp.Steps;
using TheTechIdea.Beep.Winform.Controls.Wizards;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    /// <summary>
    /// Coordinates first-run detection, setup wizard, and import/export.
    /// All wizards open as modal popups via WizardManager — never embedded as tabs.
    /// </summary>
    public class SetupWizardLauncher
    {
        private readonly IServiceProvider? _services;
        private readonly IDMEEditor? _editor;
        private readonly IWin32Window _owner;
        private IFirstRunDetector? _detector;

        public SetupWizardLauncher(IServiceProvider? services, IDMEEditor? editor, IWin32Window owner)
        {
            _services = services;
            _editor = editor;
            _owner = owner;
        }

        /// <summary>
        /// Detects first run, collects configuration, then runs setup through the framework.
        /// Returns true only when setup actually completed successfully.
        /// </summary>
        /// <remarks>
        /// Orchestration is <c>BeepBootstrapper</c>'s — the framework's first-run coordinator —
        /// rather than hand-rolled here. That matters for correctness, not just tidiness: the
        /// bootstrapper runs through <c>ISetupWizardAdapter.RunAsync</c> (so the run never blocks
        /// the UI thread) and calls <c>MarkSetupCompleteAsync</c> <b>only after a successful
        /// report</b>. The previous code marked setup complete unconditionally — including when the
        /// user declined and when the run failed — so a failed first run was never offered again.
        ///
        /// This method still owns the WinForms-specific part the framework can't: prompting, and
        /// showing the modal step UI that populates the typed options before the pipeline runs.
        /// </remarks>
        public async Task<bool> TryShowFirstRunAsync()
        {
            if (_editor == null) return false;

            _detector ??= _services?.GetService(typeof(IFirstRunDetector)) as IFirstRunDetector
                          ?? new FileBasedFirstRunDetector(_editor);

            try
            {
                if (!await _detector.IsFirstRunAsync().ConfigureAwait(true))
                    return false;
            }
            catch (Exception ex)
            {
                _editor.AddLogMessage("SetupWizardLauncher",
                    $"First-run detection failed: {ex.Message}", DateTime.Now, 0, null, Errors.Warning);
                return false;
            }

            // Ask user directly — no intermediate welcome page
            var answer = MessageBox.Show(
                _owner,
                "Welcome! This appears to be your first run.\n\nWould you like to run the setup wizard to configure your data platform?",
                "First Run Setup",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            // Declining is not the same as having set up. Leave the marker alone so the offer
            // returns next launch rather than silently never appearing again.
            if (answer != DialogResult.Yes) return false;

            var shell = new uc_SetupWizard(_services);
            var config = shell.BuildWizardConfig();
            if (config == null) return false;

            config.Key = "platform-setup-winform";
            config.Title = "Platform Setup";

            // Collect configuration into the typed options. Cancelling here means no setup ran.
            if (WizardManager.ShowWizard(config, _owner) != DialogResult.OK)
                return false;

            // uc_SetupWizard is itself the ISetupWizardFactory, so the bootstrapper runs the wizard
            // the user just configured rather than DefaultSetupWizardFactory's empty-options one.
            var bootstrapper = new BeepBootstrapper(
                _detector, shell, () => _editor, shell.Adapter);

            var result = await bootstrapper.BootstrapAsync().ConfigureAwait(true);

            if (!result.Succeeded)
                MessageBox.Show(_owner,
                    $"Setup did not complete: {result.FailureMessage ?? "unknown error"}.\n\n" +
                    "You will be offered setup again next time the application starts.",
                    "Setup Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            return result.Succeeded;
        }

        /// <summary>
        /// Phase 9 (A3): runs the per-startup <b>version-gate upgrade pass</b> on an app that was already
        /// set up. Compares the declared schema version and the entity model against the version recorded
        /// <em>in the target database</em> and applies pending migrations. Call this on every startup (after
        /// first-run setup); it is a no-op when setup was never completed or the DB is already current.
        /// </summary>
        /// <returns>
        /// The bootstrap result (with <c>MigratedFrom</c>/<c>MigratedTo</c>), or null when nothing ran
        /// (no editor, or setup not yet completed). Failures surface a warning; the app still continues.
        /// </returns>
        public async Task<BootstrapResult?> TryRunStartupUpgradeAsync(
            string datasourceName,
            IReadOnlyList<Type> entityTypes,
            string? declaredVersion = null,
            bool migrateOnStartup = true,
            string environment = "Production",
            CancellationToken cancellationToken = default)
        {
            if (_editor == null || string.IsNullOrWhiteSpace(datasourceName)) return null;

            _detector ??= _services?.GetService(typeof(IFirstRunDetector)) as IFirstRunDetector
                          ?? new FileBasedFirstRunDetector(_editor);

            // Only meaningful once first-run setup has completed.
            try
            {
                if (await _detector.IsFirstRunAsync().ConfigureAwait(true)) return null;
            }
            catch (Exception ex)
            {
                _editor.AddLogMessage("SetupWizardLauncher",
                    $"Startup upgrade check skipped (first-run detection failed): {ex.Message}",
                    DateTime.Now, 0, null, Errors.Warning);
                return null;
            }

            var factory = new DefaultSetupWizardFactory();
            var options = new SetupOptions
            {
                Environment = environment,
                MigrateOnStartup = migrateOnStartup,
                DeclaredSchemaVersion = string.IsNullOrWhiteSpace(declaredVersion) ? null : declaredVersion
            };
            var gate = new VersionGateStepOptions
            {
                DatasourceName = datasourceName,
                EntityTypes = entityTypes,
                DeclaredVersion = declaredVersion
            };

            var bootstrapper = new BeepBootstrapper(
                _detector, factory, () => _editor, new DesktopSetupWizardAdapter(),
                logger: null,
                upgradeWizardFactory: e => factory.CreateUpgrade(e, options, gate));

            var result = await bootstrapper.BootstrapAsync(cancellationToken).ConfigureAwait(true);

            if (result.Succeeded && !string.IsNullOrEmpty(result.MigratedTo) &&
                !string.Equals(result.MigratedFrom, result.MigratedTo, StringComparison.Ordinal))
            {
                _editor.AddLogMessage("SetupWizardLauncher",
                    $"Database '{datasourceName}' upgraded {result.MigratedFrom} → {result.MigratedTo}.",
                    DateTime.Now, 0, null, Errors.Ok);
            }
            else if (!result.Succeeded)
            {
                _editor.AddLogMessage("SetupWizardLauncher",
                    $"Startup migration check failed: {result.FailureMessage ?? "unknown error"}.",
                    DateTime.Now, 0, null, Errors.Warning);
            }

            return result;
        }

        /// <summary>
        /// Shows the setup wizard on demand (menu/tree action), outside the first-run flow.
        /// </summary>
        public void ShowSetupWizard()
        {
            var shell = new uc_SetupWizard(_services);
            var config = shell.BuildWizardConfig();
            if (config == null) return;

            config.Key = "platform-setup-winform";
            config.Title = "Platform Setup";
            if (WizardManager.ShowWizard(config, _owner) != DialogResult.OK) return;

            // Fire-and-forget is deliberate: this entry point is a void menu action, and the adapter
            // already turns any failure into a report rather than an escaping exception. Progress
            // and completion surface through the shell's own events.
            _ = RunConfiguredSetupAsync(shell);
        }

        private async Task RunConfiguredSetupAsync(uc_SetupWizard shell)
        {
            try
            {
                var (wizard, context) = shell.CreateDefault(_editor!);
                var report = await shell.Adapter.RunAsync(wizard, context).ConfigureAwait(true);

                if (report?.Succeeded != true)
                    MessageBox.Show(_owner,
                        "Setup did not complete successfully. See the setup report for details.",
                        "Setup Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                _editor?.AddLogMessage("SetupWizardLauncher",
                    $"Setup run failed: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
            }
        }

        /// <summary>
        /// Shows the import/export hub as a modal popup.
        /// </summary>
        /// <remarks>
        /// The hub owns both the Import and Export wizards (each launched through
        /// <c>WizardManager</c>) plus run history, so it is what opens rather than one wizard —
        /// the routed action id contains both "import" and "export" and cannot disambiguate.
        /// </remarks>
        public void ShowImportExport()
            => ImportExport.uc_ImportExportLauncher.ShowAsDialog(_services, _owner);
    }
}
