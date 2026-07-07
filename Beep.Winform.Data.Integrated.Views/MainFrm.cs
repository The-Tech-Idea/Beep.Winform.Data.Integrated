
using System;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.SetUp;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Integrated.NuggetsManage;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport;
using TheTechIdea.Beep.Winform.Default.Views.Setup;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views
{
    [AddinAttribute(Caption = "Home", Name = "MainForm", misc = "Main", menu = "Main", addinType = AddinType.Page, displayType = DisplayType.Popup, ObjectType = "Beep")]

    public partial class MainFrm : TemplateForm
    {
        private uc_SetupWizard? _setupWizard;
        private uc_ImportExportLauncher? _importExportWizard;
        private uc_AppBootstrap? _appBootstrap;
        private IFirstRunDetector? _firstRunDetector;
        private System.Threading.CancellationTokenSource? _bootstrapCts;


        IServiceProvider _serviceprovider;
        public IDMEEditor Editor { get; }

        public MainFrm()
        {
            InitializeComponent();
            Theme = BeepThemesManager.CurrentThemeName;
            FormStyle = BeepThemesManager.CurrentStyle;
            ApplyTheme();
        }
        public MainFrm(IServiceProvider services) : base(services)
        {

            InitializeComponent();
            _serviceprovider = services;

            appManager.Container = beepDisplayContainer1;
            appManager.Container.ContainerType = ContainerTypeEnum.TabbedPanel;

            beepAppTree1.init(beepService,appManager);
            beepAppTree1.CreateRootTree();
            beepAppTree1.CollapseAll();
            beepAppTree1.NodeSelected += BeepAppTree1_NodeSelected;
            FormStyle = BeepThemesManager.CurrentStyle;

            beepMenuAppBar1.beepServices = beepService;
            beepMenuAppBar1.CreateMenuItems();
            beepMenuAppBar1.SelectedItemChanged += BeepMenuAppBar1_SelectedItemChanged;

            InitializeWorkspaceControls();

            if (beepService?.DMEEditor != null)
            {
                NuggetsStartupBootstrapper.TryRestore(beepService.DMEEditor);
            }

        }

        private void MainFrm_Load(object sender, EventArgs e)
        {
            _ = TryShowBootstrapAsync();
        }

        private async Task TryShowBootstrapAsync()
        {
            if (beepService?.DMEEditor == null) return;
            if (IsDisposed || Disposing) return;

            _bootstrapCts ??= new System.Threading.CancellationTokenSource();
            _firstRunDetector ??= _serviceprovider != null
                ? Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions
                    .GetService<IFirstRunDetector>(_serviceprovider)
                ?? new FileBasedFirstRunDetector(beepService.DMEEditor)
                : new FileBasedFirstRunDetector(beepService.DMEEditor);

            try
            {
                bool isFirstRun = await _firstRunDetector.IsFirstRunAsync();
                if (IsDisposed || Disposing) return;

                if (!isFirstRun)
                {
                    return;
                }

                if (InvokeRequired)
                {
                    BeginInvoke(ShowBootstrapControl);
                }
                else
                {
                    ShowBootstrapControl();
                }
            }
            catch (Exception ex)
            {
                beepService.DMEEditor.AddLogMessage("MainFrm",
                    $"Bootstrap init failed: {ex.Message}",
                    DateTime.Now, -1, null, TheTechIdea.Beep.ConfigUtil.Errors.Failed);
            }
        }

        private void ShowBootstrapControl()
        {
            if (IsDisposed || Disposing) return;
            if (_appBootstrap != null) return;

            // Skill § 9.1 (parent rule) + § 9.6 (runtime popup allowed):
            //   The bootstrap is a *user-requested first-run popup*, not a layout surface, so
            //   wrapping it in a modal Form is the right pattern. The host Form is the dialog;
            //   uc_AppBootstrap is its single child.
            //
            // Why a modal popup instead of Controls.Add(_appBootstrap):
            //   - The main form is correctly disabled until the user finishes / skips / escapes.
            //   - Pressing Esc closes the dialog (CancelButton = the invisible Esc-handler below).
            //   - The user can close via the X button (DialogResult.Cancel) or complete the wizard
            //     (BootstrapCompleted → DialogResult.OK) — both are explicit user choices.
            var host = new Form
            {
                Text = "First Run Setup",
                // Skill § 1: dialog size flows from BeepLayoutMetrics tokens; DPI-aware.
                Size = BeepLayoutMetrics.DialogLarge.ScaleSize(this),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
                KeyPreview = true,   // so Esc reaches the Form before child controls
            };

            _appBootstrap = new uc_AppBootstrap(_serviceprovider)
            {
                Dock = DockStyle.Fill
            };
            _appBootstrap.BootstrapCompleted += AppBootstrap_BootstrapCompleted;
            host.Controls.Add(_appBootstrap);

            // Esc binding: a Cancel-only button doesn't have to be visible — assigning it to
            // CancelButton is what wires the Esc key to close the dialog with DialogResult.Cancel.
            var escHandler = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Visible = false,     // hidden — only the Esc keybinding matters
                TabStop = false
            };
            host.CancelButton = escHandler;
            host.AcceptButton = escHandler; // also bind Enter → Cancel (defensive)

            // Skill § 9.1: blocking modal call. Pass owner so dialog stays on top of MainFrm.
            DialogResult result = host.ShowDialog(this);

            // Cleanup after the dialog closes (Esc, X, or BootstrapCompleted).
            // The host Dispose cascade also disposes _appBootstrap; the explicit Dispose here
            // runs first so event handlers can still unhook cleanly.
            if (_appBootstrap != null)
            {
                _appBootstrap.BootstrapCompleted -= AppBootstrap_BootstrapCompleted;
                _appBootstrap.Dispose();
                _appBootstrap = null;
            }
            host.Dispose();

            // Only initialize the workspace if the user completed (or skipped) the bootstrap
            // (DialogResult.OK). Esc / X button / window-close leave it untouched.
            if (result == DialogResult.OK)
            {
                InitializeWorkspaceControls();
            }
        }

        private void AppBootstrap_BootstrapCompleted(object? sender, BootstrapCompletedEventArgs e)
        {
            // Skill § 9.6: the user has explicitly completed the bootstrap. Close the host
            // dialog with OK so ShowDialog returns and the workspace initializes.
            if (_appBootstrap == null) return;

            var host = _appBootstrap.Parent as Form;
            if (host == null || host.IsDisposed) return;

            if (host.InvokeRequired) host.BeginInvoke(() => { host.DialogResult = DialogResult.OK; host.Close(); });
            else { host.DialogResult = DialogResult.OK; host.Close(); }
        }

        private void InitializeWorkspaceControls()
        {
            if (beepDisplayContainer1 == null)
                return;

            _setupWizard ??= new uc_SetupWizard(_serviceprovider);
            _setupWizard.SetupCompleted -= SetupWizard_SetupCompleted;
            _setupWizard.SetupCompleted += SetupWizard_SetupCompleted;

            if (!beepDisplayContainer1.IsControlExit(_setupWizard))
            {
                beepDisplayContainer1.AddControl(_setupWizard.BranchText, _setupWizard, ContainerTypeEnum.TabbedPanel);
            }

            beepDisplayContainer1.ShowControl(_setupWizard.BranchText, _setupWizard);
        }

        private void SetupWizard_SetupCompleted(object? sender, uc_SetupWizard.SetupCompletedEventArgs e)
        {
            if (beepDisplayContainer1 == null)
                return;

            _importExportWizard ??= new uc_ImportExportLauncher(_serviceprovider);

            if (!beepDisplayContainer1.IsControlExit(_importExportWizard))
            {
                beepDisplayContainer1.AddControl(_importExportWizard.BranchText, _importExportWizard, ContainerTypeEnum.TabbedPanel);
            }

            beepDisplayContainer1.ShowControl(_importExportWizard.BranchText, _importExportWizard);
        }

        private void BeepAppTree1_NodeSelected(object? sender, BeepMouseEventArgs e)
        {
            var item = e?.Data as SimpleItem;
            if (item == null)
                return;

            var branchText = item.Text ?? item.BranchName ?? string.Empty;
            if (branchText.Contains("Setup", StringComparison.OrdinalIgnoreCase))
            {
                ShowHostedControl(_setupWizard ??= new uc_SetupWizard(_serviceprovider), _setupWizard.BranchText);
                return;
            }

            if (branchText.Contains("Import", StringComparison.OrdinalIgnoreCase) ||
                branchText.Contains("Export", StringComparison.OrdinalIgnoreCase))
            {
                ShowHostedControl(_importExportWizard ??= new uc_ImportExportLauncher(_serviceprovider), _importExportWizard.BranchText);
            }
        }

        private void ShowHostedControl(Control control, string title)
        {
            if (beepDisplayContainer1 == null || control == null)
                return;

            if (!beepDisplayContainer1.IsControlExit(control as IDM_Addin))
            {
                beepDisplayContainer1.AddControl(title, control as IDM_Addin, ContainerTypeEnum.TabbedPanel);
            }

            beepDisplayContainer1.ShowControl(title, control as IDM_Addin);
        }

        private void beepMenuAppBar1_Click(object sender, EventArgs e)
        {

        }

        private void BeepMenuAppBar1_SelectedItemChanged(object? sender, SelectedItemChangedEventArgs e)
        {
            var selectedItem = e?.SelectedItem;
            if (selectedItem == null)
                return;

            var itemText = selectedItem.Text ?? selectedItem.BranchName ?? selectedItem.MenuName ?? string.Empty;
            if (itemText.Contains("Setup", StringComparison.OrdinalIgnoreCase))
            {
                ShowHostedControl(_setupWizard ??= new uc_SetupWizard(_serviceprovider), _setupWizard.BranchText);
                return;
            }

            if (itemText.Contains("Import", StringComparison.OrdinalIgnoreCase) ||
                itemText.Contains("Export", StringComparison.OrdinalIgnoreCase))
            {
                ShowHostedControl(_importExportWizard ??= new uc_ImportExportLauncher(_serviceprovider), _importExportWizard.BranchText);
            }
        }
    }
}
