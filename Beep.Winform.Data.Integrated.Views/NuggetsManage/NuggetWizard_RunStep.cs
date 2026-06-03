using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.NuGet;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.ProgressBars;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Controls.Wizards.Forms;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    /// <summary>
    /// Step 3: Review and install.
    /// All work goes through <see cref="IAssemblyHandler"/>'s
    /// <c>LoadNuggetFromNuGetAsync</c> which downloads, extracts and loads
    /// the package in a single call.
    /// </summary>
    public class NuggetWizard_RunStep : WizardPage
    {
        private BeepLabel _lblSummary;
        private BeepProgressBar _progress;
        private BeepLabel _lblStatus;
        private BeepButton _btnInstall;

        // Set to true while an install is in flight. Lets the cancellation
        // and post-install UI updates skip safely if the control is disposed
        // mid-flight (ObjectDisposedException race).
        private bool _installing;

        public NuggetWizard_RunStep()
        {
            Title = "Review & Install";
            Description = "Confirm the settings and install the package.";
            NextButtonText = "Install";
            BuildLayout();
        }

        private IAssemblyHandler? Handler
            => Context?.GetValue<IAssemblyHandler>(NuggetWizardKeys.Handler, null!);
        private NuggetActivityLog? Log
            => Context?.GetValue<NuggetActivityLog>(NuggetWizardKeys.Log, null!);

        private void BuildLayout()
        {
            Dock = DockStyle.Fill;
            Padding = new Padding(12);

            _lblSummary = new BeepLabel
            {
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Consolas", 10F),
                Text = string.Empty,
                Padding = new Padding(8),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = System.Drawing.ContentAlignment.TopLeft
            };

            _progress = new BeepProgressBar { Dock = DockStyle.Bottom, Height = 6, Visible = false };

            _lblStatus = new BeepLabel
            {
                Dock = DockStyle.Bottom,
                Height = 22,
                Text = "Click Install to proceed.",
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0)
            };

            _btnInstall = new BeepButton { Text = "Install", Dock = DockStyle.Bottom, Height = 36 };
            _btnInstall.Click += async (s, e) => await RunInstallAsync();

            // Z-order: Fill on top, then bottom controls
            Controls.Add(_lblSummary);
            Controls.Add(_btnInstall);
            Controls.Add(_progress);
            Controls.Add(_lblStatus);
        }

        public override void OnStepEnter(WizardContext context)
        {
            base.OnStepEnter(context);
            IsComplete = false;
            _btnInstall.Enabled = true;
            _lblStatus.Text = "Click Install to proceed.";

            ClearStaleInstallResult(context);

            var src = context.GetValue(NuggetWizardKeys.SourceUrl, string.Empty);
            _lblSummary.Text =
                $"Package:    {context.GetValue(NuggetWizardKeys.PackageId, string.Empty)}\n" +
                $"Version:    {context.GetValue(NuggetWizardKeys.Version, string.Empty)}\n" +
                $"Source:     {(string.IsNullOrWhiteSpace(src) ? "(default)" : src)}\n" +
                $"Load now:   {(context.GetValue(NuggetWizardKeys.LoadAfter,    true) ? "Yes" : "No")}\n" +
                $"Shared ctx: {(context.GetValue(NuggetWizardKeys.SharedCtx,    true) ? "Yes" : "No")}\n" +
                $"Process:    {(context.GetValue(NuggetWizardKeys.ProcessHost, false) ? "Yes" : "No")}\n" +
                $"Path:       {context.GetValue(NuggetWizardKeys.InstallPath, string.Empty)}";
        }

        /// <summary>
        /// Wipes <c>InstallResult</c> from the context if it does not match the
        /// package/version the user is currently reviewing. The result of a
        /// previous wizard run must not leak into a new flow.
        /// </summary>
        private static void ClearStaleInstallResult(WizardContext context)
        {
            if (context == null) return;
            var currentId  = context.GetValue(NuggetWizardKeys.PackageId, string.Empty);
            var currentVer = context.GetValue(NuggetWizardKeys.Version,   string.Empty);
            var result     = context.GetValue<object?>(NuggetWizardKeys.InstallResult, null);
            if (result == null) return;

            bool stale = result switch
            {
                NuggetInfo ni =>
                    !string.Equals(ni.Id,      currentId,  StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(ni.Version, currentVer, StringComparison.OrdinalIgnoreCase),
                PackageInstallResult pir =>
                    !string.Equals(pir.PackageId, currentId,  StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(pir.Version,   currentVer, StringComparison.OrdinalIgnoreCase),
                _ => true
            };
            if (stale) context.SetValue(NuggetWizardKeys.InstallResult, null);
        }

        public override async Task<WizardValidationResult> ValidateAsync()
        {
            if (IsComplete) return WizardValidationResult.Success();
            return await RunInstallAsync();
        }

        private async Task<WizardValidationResult> RunInstallAsync()
        {
            var ctx = Context;
            var h = Handler;
            if (h == null || ctx == null) return WizardValidationResult.Error("AssemblyHandler not available.");

            var request = new NuggetInstallRequest
            {
                PackageId        = ctx.GetValue(NuggetWizardKeys.PackageId,    string.Empty),
                Version          = ctx.GetValue(NuggetWizardKeys.Version,      string.Empty),
                SourceUrl        = ctx.GetValue(NuggetWizardKeys.SourceUrl,    string.Empty),
                InstallPath      = ctx.GetValue(NuggetWizardKeys.InstallPath,  string.Empty),
                LoadAfterInstall = ctx.GetValue(NuggetWizardKeys.LoadAfter,    true),
                UseSharedContext = ctx.GetValue(NuggetWizardKeys.SharedCtx,    true),
                UseProcessHost   = ctx.GetValue(NuggetWizardKeys.ProcessHost,  false)
            };

            _installing = true;
            SafeSetUi(() => { _progress.Visible = true; _btnInstall.Enabled = false; },
                      $"Installing {request.PackageId} {request.Version}…");
            Log?.Info($"Starting install of {request.PackageId} {request.Version}.", request.PackageId);

            try
            {
                IEnumerable<string>? sources = string.IsNullOrWhiteSpace(request.SourceUrl)
                    ? null
                    : new[] { request.SourceUrl };

                List<Assembly> assemblies = await h.LoadNuggetFromNuGetAsync(
                    request.PackageId,
                    string.IsNullOrWhiteSpace(request.Version) ? null : request.Version,
                    sources,
                    request.UseSharedContext,
                    string.IsNullOrWhiteSpace(request.InstallPath) ? null : request.InstallPath,
                    request.UseProcessHost);

                // Control may have been disposed while we were awaiting.
                if (IsDisposed || !_installing) return WizardValidationResult.Error("Install aborted: control was disposed.");

                var info = new NuggetInfo
                {
                    Id              = request.PackageId,
                    Name            = request.PackageId,
                    Version         = request.Version,
                    LoadedAssemblies= assemblies,
                    LoadedAt        = DateTime.Now,
                    IsSharedContext = request.UseSharedContext,
                    IsActive        = true
                };
                ctx.SetValue(NuggetWizardKeys.InstallResult, info);

                bool ok = assemblies != null && assemblies.Count > 0;
                IsComplete = ok;
                SafeSetUi(() =>
                {
                    _lblStatus.Text = ok
                        ? $"Installed. {assemblies.Count} assembly(ies) loaded."
                        : "Install completed but no assemblies were returned.";
                });

                if (ok) Log?.Success($"Installed {request.PackageId} {request.Version}; {assemblies.Count} assembly(ies) loaded.", request.PackageId);
                else    Log?.Warn($"Install of {request.PackageId} {request.Version} returned no assemblies.", request.PackageId);

                return ok
                    ? WizardValidationResult.Success()
                    : WizardValidationResult.Error("Install completed but no assemblies were returned.");
            }
            catch (Exception ex)
            {
                if (IsDisposed) return WizardValidationResult.Error($"Install failed: {ex.Message}");
                SafeSetUi(() =>
                {
                    _lblStatus.Text = $"Install failed: {ex.Message}";
                    IsComplete = false;
                    _btnInstall.Enabled = true;
                });
                Log?.Error($"Install of {request.PackageId} failed: {ex.Message}", request.PackageId);
                return WizardValidationResult.Error(ex.Message);
            }
            finally
            {
                _installing = false;
                SafeSetUi(() => _progress.Visible = false);
            }
        }

        /// <summary>
        /// Apply a UI mutation only if the control is still alive; swallow
        /// <see cref="ObjectDisposedException"/> silently. Used for all
        /// post-<c>await</c> UI updates where the control may have been
        /// disposed (or its parent page removed) while an async install
        /// was in flight.
        /// </summary>
        private void SafeSetUi(Action mutate, string statusFallback = null!)
        {
            if (IsDisposed || Disposing) return;
            try
            {
                mutate();
            }
            catch (ObjectDisposedException) { /* control tree went away mid-flight */ }
            catch (InvalidOperationException)
            {
                // Re-asserting handle after disposal can throw this.
            }
        }

        protected override void Dispose(bool disposing)
        {
            _installing = false;
            base.Dispose(disposing);
        }
    }
}
