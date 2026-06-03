using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Default.Views.Template;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Controls.Wizards.Forms;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    /// <summary>
    /// NuGet Manager — a single control that IS the Beep wizard.
    /// All NuGet work is delegated to the editor's <see cref="TheTechIdea.Beep.Tools.IAssemblyHandler"/>
    /// (the modern <c>SharedContextAssemblyHandler</c>).
    /// </summary>
    [AddinAttribute(Caption = "Nugget Manager", Name = "uc_NuggetsManage",
        misc = "Config", menu = "Configuration", addinType = AddinType.Control,
        displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 4, RootNodeName = "Configuration", Order = 4, ID = 4,
        BranchText = "Nugget Manager", BranchType = EnumPointType.Function,
        IconImageName = "drivers.svg", BranchClass = "ADDIN",
        BranchDescription = "NuGet package search, install, and management")]
    public partial class uc_NuggetsManage : TemplateUserControl, IAddinVisSchema
    {
        private TheTechIdea.Beep.Tools.IAssemblyHandler? _handler;
        private HorizontalStepperWizardForm? _wizardForm;
        private WizardInstance? _wizardInstance;
        private WizardContext? _wizardContext;
        private NuggetActivityLog? _log;
        private bool _built;

        // Serialises BuildWizard so we never end up with two forms attached to
        // the control if the host races OnNavigatedTo / EnsureBuilt / HandleCreated.
        private readonly object _buildLock = new object();
        private bool _hasWarnedNoHandler;
        private bool _hasWarnedNoEditor;

        public event EventHandler<NuggetInstallCompletedEventArgs>? PackageInstallCompleted;

        public uc_NuggetsManage(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            Details.AddinName = "Nugget Manager";

            // Fallback: if the host never calls OnNavigatedTo or EnsureBuilt,
            // build the wizard as soon as the control is realised. The handler
            // unsubscribes itself once it has built (or decided not to), so we
            // don't react to handle recreations (theme change, DPI change, etc.).
            HandleCreated += OnHandleCreatedFallback;
        }

        public uc_NuggetsManage() : this(new SimpleServiceProvider())
        {
        }

        private void OnHandleCreatedFallback(object? sender, EventArgs e)
        {
            try
            {
                if (_built)
                {
                    // Already built by OnNavigatedTo or an earlier pass — unhook
                    // ourselves so we don't fire on every handle recreation.
                    HandleCreated -= OnHandleCreatedFallback;
                    return;
                }
                _handler = Editor?.assemblyHandler;
                if (_handler == null) return;
                BuildWizard();
                SeedContext();
                if (_built)
                {
                    HandleCreated -= OnHandleCreatedFallback;
                }
            }
            catch
            {
                // Defensive: never let the handle-created callback crash the host.
            }
        }

        #region IAddinVisSchema
        public string RootNodeName { get; set; } = "Configuration";
        public string CatgoryName { get; set; } = string.Empty;
        public int Order { get; set; } = 4;
        public int ID { get; set; } = 4;
        public string BranchText { get; set; } = "Nugget Manager";
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        public int BranchID { get; set; } = 4;
        public string IconImageName { get; set; } = "drivers.svg";
        public string BranchStatus { get; set; } = string.Empty;
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; } = "NuGet package search, install, and management";
        public string BranchClass { get; set; } = "ADDIN";
        public string AddinName { get; set; } = "uc_NuggetsManage";
        #endregion

        #region Public read-only state
        /// <summary>True once the wizard form has been created and added to the control.</summary>
        public bool IsWizardBuilt => _built && _wizardForm != null && !_wizardForm.IsDisposed;

        /// <summary>
        /// True when both the editor and its <c>IAssemblyHandler</c> are available
        /// — i.e. a call to <see cref="EnsureBuilt"/> or <see cref="OnNavigatedTo"/>
        /// would actually build the wizard right now.
        /// </summary>
        public bool CanBuild => Editor != null && Editor.assemblyHandler != null && !IsDisposed;

        /// <summary>Key of the current wizard step, or null if the wizard has not been built yet.</summary>
        public string? CurrentStepKey => _wizardInstance?.CurrentStep?.Key;

        /// <summary>Zero-based index of the current wizard step, or -1 if the wizard has not been built yet.</summary>
        public int CurrentStepIndex => _wizardInstance?.CurrentStepIndex ?? -1;

        /// <summary>Number of wizard steps, or 0 if the wizard has not been built yet.</summary>
        public int StepCount => _wizardInstance?.Config?.Steps?.Count ?? 0;
        #endregion

        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            try
            {
                base.OnNavigatedTo(parameters ?? new Dictionary<string, object>());

                // Refresh in case the editor swapped its handler since the last visit.
                _handler = Editor?.assemblyHandler;
                if (Editor == null)
                {
                    WarnOnce(ref _hasWarnedNoEditor,
                        "Nugget Manager: editor is not available; cannot initialize wizard.");
                    return;
                }
                if (_handler == null)
                {
                    WarnOnce(ref _hasWarnedNoHandler,
                        "Nugget Manager: editor.assemblyHandler is null; cannot initialize wizard.");
                    return;
                }

                // Handler is now present — clear the "missing" warning flags so we
                // warn again only if it goes missing in a later session.
                _hasWarnedNoHandler = false;
                _hasWarnedNoEditor = false;

                BuildWizard();

                // Re-seed the context with the freshest handler / log so the steps
                // always see the current IAssemblyHandler even if the editor was
                // re-initialised while the user was away.
                SeedContext();

                // Re-show the embedded form in case we are returning from Suspend().
                if (_wizardForm != null && !_wizardForm.IsDisposed)
                {
                    _wizardForm.Visible = true;
                    _wizardForm.BringToFront();
                }
            }
            catch (Exception ex)
            {
                LogToEditor($"OnNavigatedTo failed: {ex.Message}", Errors.Failed);
            }
        }

        /// <summary>
        /// Force-build the wizard if it has not been built yet. Call this from a host
        /// that embeds the control directly without going through
        /// <see cref="OnNavigatedTo"/>. Safe to call multiple times.
        /// </summary>
        public void EnsureBuilt()
        {
            if (_built) return;
            _handler = Editor?.assemblyHandler;
            if (_handler == null) return;
            BuildWizard();
            SeedContext();
        }

        public override void Suspend()
        {
            base.Suspend();
            if (_wizardForm != null && !_wizardForm.IsDisposed)
                _wizardForm.Visible = false;
        }

        public override void Resume()
        {
            base.Resume();
            if (_wizardForm != null && !_wizardForm.IsDisposed)
            {
                _wizardForm.Visible = true;
                _wizardForm.BringToFront();
            }
        }

        public override void ApplyTheme()
        {
            base.ApplyTheme();
            if (_wizardForm == null || _wizardForm.IsDisposed) return;

            // The wizard chrome listens to its own Theme setter, which triggers its ApplyTheme.
            _wizardForm.Theme = Theme;

            // The wizard chrome does not theme the step content controls, so we walk the
            // form's control tree and apply the theme to every IBeepUIComponent we find
            // (BeepGridPro, BeepTextBox, BeepComboBox, BeepButton, BeepLabel, etc.).
            PropagateThemeToStepControls(_wizardForm);
        }

        /// <summary>
        /// Disposes the wizard and rebuilds it from scratch so the user can run a fresh
        /// search → install → manage flow. Safe to call before the first <see cref="OnNavigatedTo"/>.
        /// </summary>
        public void ResetWizard()
        {
            DisposeWizard();
            _built = false;
            if (_handler != null)
            {
                BuildWizard();
                SeedContext();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try { HandleCreated -= OnHandleCreatedFallback; } catch { /* already gone */ }
                DisposeWizard();
                _built = false;
            }
            base.Dispose(disposing);
        }

        // ---- internals ----

        private void BuildWizard()
        {
            // Serialize so a race between OnNavigatedTo and EnsureBuilt (or
            // HandleCreated) cannot attach two wizard forms to the control.
            lock (_buildLock)
            {
                if (_built && (_wizardForm == null || _wizardForm.IsDisposed))
                {
                    // Previous instance died without DisposeWizard — clean it up
                    // so we don't leak the disposed form reference.
                    DisposeWizard();
                }
                if (_built) return;
                if (_handler == null) return;
                if (IsDisposed || Disposing) return;

                _log = new NuggetActivityLog();
                _log.Info("Wizard started.");

                var config = new WizardConfig
                {
                    Key = "NuggetManager",
                    Title = "NuGet Manager",
                    Description = "Search, install and manage NuGet packages.",
                    Style = WizardStyle.HorizontalStepper,
                    ShowProgressBar = true,
                    ShowStepList = true,
                    AllowBack = true,
                    AllowCancel = false,
                    Size = new System.Drawing.Size(900, 600),
                    NextButtonText = "Next",
                    BackButtonText = "Back",
                    FinishButtonText = "Finish",
                    Steps =
                    {
                        new WizardStep { Key = "search",    Title = "1. Search",    Description = "Select package",     Content = new NuggetWizard_SearchStep()    },
                        new WizardStep { Key = "options",   Title = "2. Options",   Description = "Version",            Content = new NuggetWizard_OptionsStep()   },
                        new WizardStep { Key = "run",       Title = "3. Install",   Description = "Review & install",   Content = new NuggetWizard_RunStep()       },
                        new WizardStep { Key = "installed", Title = "4. Installed", Description = "Manage",             Content = new NuggetWizard_InstalledStep() }
                    },
                    OnComplete = ctx => OnWizardComplete(ctx)
                };

                _wizardInstance = new WizardInstance(config);
                _wizardInstance.Completed += OnWizardInstanceCompleted;
                _wizardContext = _wizardInstance.Context;
                ClearStaleContextValues();
                SeedContext();

                try
                {
                    _wizardForm = new HorizontalStepperWizardForm(_wizardInstance)
                    {
                        TopLevel = false,
                        FormBorderStyle = FormBorderStyle.None,
                        Dock = DockStyle.Fill
                    };
                }
                catch
                {
                    // Form constructor failed (bad instance, theme crash, etc.) —
                    // unwind the partial state so a retry has a clean slate.
                    _wizardForm = null;
                    try { _wizardInstance.Completed -= OnWizardInstanceCompleted; } catch { }
                    _wizardInstance = null;
                    _wizardContext = null;
                    _log = null;
                    throw;
                }

                // Guard against the form throwing during construction.
                if (IsDisposed || Disposing)
                {
                    try { _wizardForm.Dispose(); } catch { }
                    _wizardForm = null;
                    try { _wizardInstance.Completed -= OnWizardInstanceCompleted; } catch { }
                    _wizardInstance = null;
                    _wizardContext = null;
                    _log = null;
                    return;
                }

                try
                {
                    Controls.Add(_wizardForm);
                    _wizardForm.Show();
                }
                catch
                {
                    // Controls.Add or Show() failed (parent disposed between
                    // the IsDisposed check and here, theme crash, etc.) — clean
                    // up so a retry doesn't see a half-attached form.
                    try { _wizardForm.Dispose(); } catch { }
                    _wizardForm = null;
                    try { _wizardInstance.Completed -= OnWizardInstanceCompleted; } catch { }
                    _wizardInstance = null;
                    _wizardContext = null;
                    _log = null;
                    throw;
                }

                // Apply current theme to the wizard + step content.
                _wizardForm.Theme = Theme;
                PropagateThemeToStepControls(_wizardForm);

                _built = true;
            }
        }

        private void DisposeWizard()
        {
            if (_wizardInstance != null)
            {
                try { _wizardInstance.Completed -= OnWizardInstanceCompleted; } catch { /* gone */ }
                _wizardInstance = null;
            }

            if (_wizardForm != null)
            {
                if (!_wizardForm.IsDisposed)
                {
                    if (Controls.Contains(_wizardForm)) Controls.Remove(_wizardForm);
                    try { _wizardForm.Close(); }   catch { /* form already closing */ }
                    try { _wizardForm.Dispose(); } catch { /* already disposed */ }
                }
                _wizardForm = null;
            }

            _wizardContext = null;
            _log = null;
        }

        private void SeedContext()
        {
            if (_wizardContext == null || _handler == null) return;
            _wizardContext.SetValue(NuggetWizardKeys.Handler, _handler);
            if (_log != null) _wizardContext.SetValue(NuggetWizardKeys.Log, _log);
        }

        /// <summary>
        /// Wipe any user-input values left over from a previous wizard run so the
        /// new wizard starts on a clean slate. Handler and Log are re-seeded by
        /// <see cref="SeedContext"/> right after.
        /// </summary>
        private void ClearStaleContextValues()
        {
            if (_wizardContext == null) return;
            _wizardContext.SetValue(NuggetWizardKeys.PackageId,     string.Empty);
            _wizardContext.SetValue(NuggetWizardKeys.Version,       string.Empty);
            _wizardContext.SetValue(NuggetWizardKeys.SourceUrl,     string.Empty);
            _wizardContext.SetValue(NuggetWizardKeys.IncludePre,    false);
            _wizardContext.SetValue(NuggetWizardKeys.InstallPath,   string.Empty);
            _wizardContext.SetValue(NuggetWizardKeys.LoadAfter,     true);
            _wizardContext.SetValue(NuggetWizardKeys.SharedCtx,     true);
            _wizardContext.SetValue(NuggetWizardKeys.ProcessHost,   false);
            _wizardContext.SetValue(NuggetWizardKeys.InstallResult, null);
        }

        private void OnWizardInstanceCompleted(object? sender, WizardCompletedEventArgs e)
        {
            // Mirror of OnWizardComplete so the host gets notified even if a custom
            // WizardConfig.OnComplete is wired by a subclass. The actual event
            // PackageInstallCompleted is fired from OnWizardComplete which is
            // registered on WizardConfig.OnComplete — we don't fire it twice here.
            var stepKey   = (sender as WizardInstance)?.CurrentStep?.Key ?? "?";
            var stepIndex = (sender as WizardInstance)?.CurrentStepIndex
                            ?? e.Context?.CurrentStepIndex
                            ?? -1;
            _log?.Info($"Wizard finished at step '{stepKey}' (#{stepIndex}).");
        }

        private void OnWizardComplete(WizardContext ctx)
        {
            if (ctx == null) return;

            var packageId = ctx.GetValue(NuggetWizardKeys.PackageId, string.Empty);
            var version   = ctx.GetValue(NuggetWizardKeys.Version,   string.Empty);
            var rawResult = ctx.GetValue<object?>(NuggetWizardKeys.InstallResult, null);

            // Discard a stale InstallResult left over from a previous wizard
            // run that was for a different package/version. Otherwise we'd
            // report the old install as the result of the current run.
            var result = StripStaleInstallResult(rawResult, packageId, version);

            bool success = false;
            string message;

            if (result is TheTechIdea.Beep.NuGet.PackageInstallResult pir)
            {
                success = pir.Success;
                message = pir.Success
                    ? $"{pir.PackageId} {pir.Version} installed in {pir.DurationMs} ms."
                    : pir.Error ?? "Install failed.";
            }
            else if (result is TheTechIdea.Beep.NuGet.NuggetInfo ni)
            {
                success = ni != null && ni.LoadedAssemblies != null && ni.LoadedAssemblies.Count > 0;
                message = success
                    ? $"{ni.Id} {ni.Version} loaded ({ni.LoadedAssemblies.Count} assembly(ies))."
                    : "No assemblies were loaded.";
            }
            else if (rawResult != null && !ReferenceEquals(rawResult, result))
            {
                // We had an InstallResult but it was stale and got stripped.
                message = string.IsNullOrEmpty(packageId)
                    ? "Wizard completed without selecting a package."
                    : $"Wizard completed without installing {packageId} (previous result was for a different package).";
            }
            else
            {
                // User reached the final step without running the install.
                message = string.IsNullOrEmpty(packageId)
                    ? "Wizard completed without selecting a package."
                    : $"Wizard completed without installing {packageId}.";
            }

            PackageInstallCompleted?.Invoke(this,
                new NuggetInstallCompletedEventArgs(packageId, version, success, message));
        }

        /// <summary>
        /// Returns <paramref name="rawResult"/> if it matches the current
        /// package/version the user is reviewing, otherwise <c>null</c>. This
        /// prevents a previously-installed package's result from being reported
        /// as the result of the current wizard run when the user changed their
        /// selection and skipped the install step.
        /// </summary>
        private static object? StripStaleInstallResult(object? rawResult, string currentId, string currentVer)
        {
            if (rawResult == null) return null;
            bool matches = rawResult switch
            {
                TheTechIdea.Beep.NuGet.PackageInstallResult pir =>
                    string.Equals(pir.PackageId, currentId,  StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(pir.Version,   currentVer, StringComparison.OrdinalIgnoreCase),
                TheTechIdea.Beep.NuGet.NuggetInfo ni =>
                    string.Equals(ni.Id,      currentId,  StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(ni.Version, currentVer, StringComparison.OrdinalIgnoreCase),
                _ => false
            };
            return matches ? rawResult : null;
        }

        private void PropagateThemeToStepControls(Control parent)
        {
            if (parent == null) return;
            foreach (Control child in parent.Controls)
            {
                if (child is IBeepUIComponent ui)
                {
                    try
                    {
                        ui.Theme = Theme;
                        ui.ApplyThemeToChilds = true;
                        ui.ApplyTheme();
                    }
                    catch
                    {
                        // Some controls throw on theme change before they are realized;
                        // ignore and let the next ApplyTheme pass handle them.
                    }
                }
                if (child.HasChildren) PropagateThemeToStepControls(child);
            }
        }

        private void WarnOnce(ref bool flag, string message)
        {
            if (flag) return;
            flag = true;
            LogToEditor(message, Errors.Warning);
        }

        private void LogToEditor(string message, Errors severity = Errors.Failed)
        {
            try
            {
                Editor?.AddLogMessage("Nugget Manager", message, DateTime.Now, -1, null, severity);
            }
            catch
            {
                // Fall back silently if the editor is in an unusable state.
            }
        }
    }

    internal sealed class SimpleServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
