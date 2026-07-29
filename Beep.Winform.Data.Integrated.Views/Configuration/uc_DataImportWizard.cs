using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Editor.Importing;
using TheTechIdea.Beep.Editor.Importing.Factories;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls.Layouts.Helpers;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport.Import;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport.Models;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    [AddinAttribute(Caption = "Data Import Wizard", Name = "uc_DataImportWizard",
        misc = "Config", menu = "Configuration", addinType = AddinType.Control,
        displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 15, RootNodeName = "Configuration", Order = 15, ID = 15,
        BranchText = "Data Import Wizard", BranchType = EnumPointType.Function,
        IconImageName = "import.svg", BranchClass = "ADDIN",
        BranchDescription = "Profile, transform, and run data imports.")]

    /// <summary>
    /// Data import addin. It owns no UI of its own: the five import steps are the shipped
    /// <c>Views.ImportExport.Import</c> controls, and the shell around them — stepper, Back/Next/
    /// Cancel, progress, validation display, step lifecycle — is the shipped
    /// <c>Controls.Wizards</c> framework. All import work happens inside those steps through
    /// <see cref="DataImportManager"/>; this class assembles them and hosts the result.
    /// </summary>
    /// <remarks>
    /// There is no designer file and no control of its own by design. The addin is
    /// <see cref="DisplayType.InControl"/>, so the framework's wizard form is embedded
    /// (<c>TopLevel = false</c>, borderless, docked) rather than shown modally the way
    /// <c>uc_ImportExportLauncher</c> shows the same five steps — the wizard behaves identically
    /// either way, this one just lives inside the addin surface instead of over it.
    /// </remarks>
    public class uc_DataImportWizard : TemplateUserControl, IAddinVisSchema
    {
        /// <remarks>
        /// Resolves to the view-local <c>WizardCompletedEventArgs</c> (same namespace), not the
        /// same-named type in <c>Controls.Wizards</c> — the enclosing namespace wins over usings.
        /// </remarks>
        public event EventHandler<WizardCompletedEventArgs>? Completed;

        /// <summary>Needed to construct the step controls; each one resolves its own services from it.</summary>
        private readonly IServiceProvider? _services;

        private WizardInstance? _wizard;
        private Form? _host;

        /// <summary>
        /// Designer/parameterless ctor. Deliberately does not chain to the IServiceProvider overload
        /// with null — that would resolve services off a null provider and throw. Without services
        /// the steps cannot be built, so the addin reports that instead.
        /// </summary>
        public uc_DataImportWizard()
        {
            _services = null;
            Size = new System.Drawing.Size(940, 640);
        }

        public uc_DataImportWizard(IServiceProvider services) : base(services)
        {
            _services = services;
            Size = new System.Drawing.Size(940, 640);
        }

        #region "IAddinVisSchema"
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string RootNodeName { get; set; } = "Configuration";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CatgoryName { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Order { get; set; } = 15;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ID { get; set; } = 15;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchText { get; set; } = "Data Import Wizard";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Level { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BranchID { get; set; } = 15;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconImageName { get; set; } = "import.svg";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchStatus { get; set; } = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ParentBranchID { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchDescription { get; set; } = "Profile, transform, and run data imports.";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BranchClass { get; set; } = "ADDIN";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string AddinName { get; set; } = "uc_DataImportWizard";
        #endregion

        /// <summary>
        /// The wizard is the view — it starts as soon as the addin is shown. OnLoad rather than a
        /// constructor so the VS designer never instantiates the step controls or the wizard form.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!DesignMode && _wizard == null)
                StartWizard();
        }

        /// <summary>
        /// Builds the five shipped import steps, declares them to the wizard framework, and embeds
        /// the framework's host form into this control.
        /// </summary>
        private void StartWizard()
        {
            Details.AddinName = "Data Import Wizard";

            if (_services == null)
            {
                ShowUnavailable("The import wizard needs application services, which are not available on this instance.");
                return;
            }

            var config = new WizardConfig
            {
                Title = "Data Import Wizard",
                Description = "Import data from source to destination",
                // Skill § 1: use BeepLayoutMetrics tokens for dialog sizes; ScaleSize() DPI-scales it.
                Size = BeepLayoutMetrics.DialogLarge.ScaleSize(this),
                Style = WizardStyle.HorizontalStepper,
            };

            config.Steps.Add(new WizardStep { Key = "configure", Title = "Configure", Description = "Source & destination", Content = new uc_ImportStep1_Configure(_services) });
            config.Steps.Add(new WizardStep { Key = "columns", Title = "Columns", Description = "Select columns", Content = new uc_ImportStep2_Columns(_services) });
            config.Steps.Add(new WizardStep { Key = "mapping", Title = "Mapping", Description = "Map fields", Content = new uc_ImportStep3_Mapping(_services) });
            config.Steps.Add(new WizardStep { Key = "options", Title = "Options", Description = "Batch size, quality rules", Content = new uc_ImportStep4_Options(_services) });
            config.Steps.Add(new WizardStep { Key = "run", Title = "Run", Description = "Execute import", Content = new uc_ImportStep5_Run(_services) });

            config.OnComplete = async ctx =>
            {
                await RecordRunAsync(ctx).ConfigureAwait(true);
                Completed?.Invoke(this, new WizardCompletedEventArgs
                {
                    Succeeded = ctx.GetValue(WizardKeys.LastRunSucceeded, false),
                    Summary = DescribeRun(ctx)
                });
            };

            config.OnCancel = ctx => Completed?.Invoke(this, new WizardCompletedEventArgs
            {
                Cancelled = true,
                Summary = DescribeRun(ctx)
            });

            _wizard = WizardManager.CreateWizard(config);

            // The modal path marks the first step current from ShowDialogAsync; embedding has to do
            // it here so the stepper renders step one as active.
            config.Steps[0].State = TheTechIdea.Beep.Winform.Controls.Wizards.StepState.Current;

            var host = WizardFormFactory.CreateForm(config.Style, _wizard);
            _wizard.BindFormHost(host);

            _host = (Form)host;
            _host.TopLevel = false;
            _host.FormBorderStyle = FormBorderStyle.None;
            _host.Dock = DockStyle.Fill;

            Controls.Clear();
            Controls.Add(_host);

            host.UpdateUI();     // parents step one's content before anything fires
            _host.Show();

            // Complete and Cancel both close the form, which disposes it and the steps inside it.
            // Restart so the addin always shows a live wizard rather than an empty panel.
            _host.FormClosed += (_, _) =>
            {
                if (IsDisposed || Disposing) return;
                _wizard = null;
                _host = null;
                BeginInvoke(new Action(() =>
                {
                    if (!IsDisposed && !Disposing) StartWizard();
                }));
            };
        }

        /// <summary>
        /// Files the completed run with the shared history store, so the run shows up in the
        /// Export view's history feed. This view is the only import entry point, so it is the only
        /// writer of <see cref="WizardKeys.ImportHistoryContext"/>.
        /// </summary>
        /// <remarks>
        /// The store is per-context — <c>GetRunsAsync</c> only ever returns one key's records and
        /// contexts cannot be enumerated — which is why runs go under one fixed key rather than
        /// "{datasource}.{entity}". Nothing else consumes these records: DataImportManager never
        /// reads <c>DataImportConfiguration.RunHistoryStore</c>.
        /// </remarks>
        private async Task RecordRunAsync(WizardContext context)
        {
            var config = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null);
            var summary = context.GetValue<ImportRunSummary?>(WizardKeys.RunSummary, null);
            if (config == null || summary == null || Editor == null) return;

            try
            {
                var store = LocalStoreFactory.CreateHistoryStore(Editor);
                if (store == null) return;

                await store.SaveRunAsync(new ImportRunRecord
                {
                    ContextKey = WizardKeys.ImportHistoryContext,
                    StartedAt = DateTime.UtcNow - summary.Duration,
                    FinishedAt = DateTime.UtcNow,
                    FinalState = summary.FailedRows == 0 ? ImportState.Completed : ImportState.Faulted,
                    SyncMode = config.SyncMode,
                    RecordsRead = summary.TotalRows,
                    RecordsWritten = summary.TotalRows - summary.FailedRows,
                    RecordsBlocked = summary.FailedRows,
                    Summary = $"{config.SourceDataSourceName}.{config.SourceEntityName} → " +
                              $"{config.DestDataSourceName}.{config.DestEntityName} — " +
                              $"Added:{summary.AddedRows} Updated:{summary.UpdatedRows} Failed:{summary.FailedRows}",
                }).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                // History is a side record, never a reason to fail a completed import.
                System.Diagnostics.Debug.WriteLine($"Import run history not saved: {ex.Message}");
            }
        }

        /// <summary>
        /// Reads the run outcome the steps put in the wizard context: step 5 files an
        /// <see cref="ImportRunSummary"/> under <see cref="WizardKeys.RunSummary"/>, and step 1 the
        /// <see cref="DataImportConfiguration"/> under <see cref="WizardKeys.ImportConfig"/>.
        /// </summary>
        private static string DescribeRun(WizardContext context)
        {
            var config = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null);
            var summary = context.GetValue<ImportRunSummary?>(WizardKeys.RunSummary, null);
            if (summary == null)
                return config == null ? string.Empty : $"{config.SourceEntityName} → {config.DestEntityName}: not run.";

            string scope = config == null
                ? string.Empty
                : $"{config.SourceDataSourceName}.{config.SourceEntityName} → " +
                  $"{config.DestDataSourceName}.{config.DestEntityName} — ";

            return $"{scope}Added:{summary.AddedRows} Updated:{summary.UpdatedRows} " +
                   $"Failed:{summary.FailedRows} of {summary.TotalRows} row(s).";
        }

        /// <summary>
        /// Fallback surface for the parameterless/designer instance, which has no services and so
        /// cannot construct the steps.
        /// </summary>
        private void ShowUnavailable(string message)
        {
            Controls.Clear();
            Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = message,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Padding = new Padding(24)
            });
        }
    }
}
