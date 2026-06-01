using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.Importing;
using TheTechIdea.Beep.Editor.Importing.Interfaces;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    /// <summary>
    /// Configuration for data export operations.
    /// </summary>
    public sealed class ExportConfiguration
    {
        public string SourceDataSourceName { get; set; } = string.Empty;
        public string SourceEntityName { get; set; } = string.Empty;
        public string DestDataSourceName { get; set; } = string.Empty;
        public string DestEntityName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public ExportFormat Format { get; set; } = ExportFormat.Csv;
        public List<string> SelectedFields { get; set; } = new();
        public string CsvDelimiter { get; set; } = ",";
        public bool IncludeHeaders { get; set; } = true;
        public string Encoding { get; set; } = "UTF-8";
        public int BatchSize { get; set; } = 1000;
        public List<AppFilter> Filters { get; set; } = new();
    }

    public enum ExportFormat { Csv, Json, Xml, Excel }

    /// <summary>
    /// Keys used in the export wizard context.
    /// </summary>
    public static class ExportWizardKeys
    {
        public const string ExportConfig = "ExportConfig";
        public const string SelectedColumns = "SelectedColumns";
        public const string LastRunSucceeded = "LastRunSucceeded";
        public const string RunSummary = "RunSummary";
    }

    /// <summary>
    /// Summary of an export run.
    /// </summary>
    public sealed class ExportRunSummary
    {
        public int TotalRows { get; set; }
        public int ExportedRows { get; set; }
        public int FailedRows { get; set; }
        public TimeSpan Duration { get; set; }
        public double RowsPerSecond => Duration.TotalSeconds > 0 ? TotalRows / Duration.TotalSeconds : 0;
        public string FilePath { get; set; } = string.Empty;
    }

    [AddinAttribute(Caption = "Export Wizard", Name = "uc_ExportWizard",
        misc = "Config", menu = "Configuration", addinType = AddinType.Control,
        displayType = DisplayType.InControl, ObjectType = "Beep")]
    [AddinVisSchema(BranchID = 6, RootNodeName = "Configuration", Order = 6, ID = 6,
        BranchText = "Export Wizard", BranchType = EnumPointType.Function,
        IconImageName = "drivers.svg", BranchClass = "ADDIN",
        BranchDescription = "Export data to files or other data sources")]
    public partial class uc_ExportWizard : TemplateUserControl, IAddinVisSchema
    {
        private readonly IServiceProvider _services;
        private ExportConfiguration _config = new();

        #region IAddinVisSchema
        public string RootNodeName { get; set; } = "Configuration";
        public string CatgoryName { get; set; } = string.Empty;
        public int Order { get; set; } = 6;
        public int ID { get; set; } = 6;
        public string BranchText { get; set; } = "Export Wizard";
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.Function;
        public int BranchID { get; set; } = 6;
        public string IconImageName { get; set; } = "drivers.svg";
        public string BranchStatus { get; set; } = string.Empty;
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; } = "Export data to files or other data sources";
        public string BranchClass { get; set; } = "ADDIN";
        public string AddinName { get; set; } = "uc_ExportWizard";
        #endregion

        public uc_ExportWizard(IServiceProvider services) : base(services)
        {
            _services = services;
            Details.AddinName = "Export Wizard";
            InitializeComponent();
        }

        public override void OnNavigatedTo(Dictionary<string, object> parameters)
        {
            base.OnNavigatedTo(parameters);
            LoadDataSources();
            AppendLog("Ready. Configure source and destination then click Launch Export.");
        }

        public override void Configure(Dictionary<string, object> settings)
        {
            base.Configure(settings);
            WireEvents();
            LoadDataSources();
        }

        private void WireEvents()
        {
            cmbSourceDS.SelectedItemChanged += (_, _) => LoadEntities(cmbSourceDS.SelectedItem?.Text, cmbSourceEntity);
            btnLaunch.Click += (_, _) => LaunchExportWizard();
            btnClearLog.Click += (_, _) => txtLog.Clear();
            btnBrowseFile.Click += (_, _) => BrowseFile_Click();
        }

        private void LoadDataSources()
        {
            try
            {
                var names = (Editor?.ConfigEditor?.DataConnections ?? new List<ConnectionProperties>())
                    .Select(p => p.ConnectionName)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(n => n)
                    .Select(n => new SimpleItem { Text = n, Item = n })
                    .ToList();

                cmbSourceDS.ListItems = new BindingList<SimpleItem>(names);

                // Populate format combo
                var formats = new BindingList<SimpleItem>
                {
                    new SimpleItem { Text = "Csv", Item = ExportFormat.Csv },
                    new SimpleItem { Text = "Json", Item = ExportFormat.Json },
                    new SimpleItem { Text = "Xml", Item = ExportFormat.Xml }
                };
                cmbFormat.ListItems = formats;
                cmbFormat.SelectItemByText("Csv");
            }
            catch (Exception ex)
            {
                Editor?.AddLogMessage("ExportWizard", $"Error loading data sources: {ex.Message}",
                    DateTime.Now, 0, null, Errors.Failed);
            }
        }

        private void LoadEntities(string dataSourceName, BeepComboBox combo)
        {
            if (string.IsNullOrWhiteSpace(dataSourceName) || combo == null) return;
            try
            {
                var ds = Editor?.GetDataSource(dataSourceName);
                if (ds == null) return;
                if (ds.ConnectionStatus != System.Data.ConnectionState.Open) ds.Openconnection();
                var list = ds.GetEntitesList()?.ToList() ?? new List<string>();
                if (list.Count == 0 && ds.EntitiesNames != null) list = ds.EntitiesNames.ToList();

                var items = new BindingList<SimpleItem>(
                    list.Where(n => !string.IsNullOrWhiteSpace(n))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(n => n)
                        .Select(n => new SimpleItem { Text = n, Item = n })
                        .ToList());

                combo.ListItems = items;
            }
            catch (Exception ex)
            {
                Editor?.AddLogMessage("ExportWizard", $"Error loading entities: {ex.Message}",
                    DateTime.Now, 0, null, Errors.Failed);
            }
        }

        private void BrowseFile_Click()
        {
            using var dlg = new SaveFileDialog
            {
                Title = "Export To File",
                Filter = "CSV files (*.csv)|*.csv|JSON files (*.json)|*.json|XML files (*.xml)|*.xml|Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                FileName = $"export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = dlg.FileName;
                // Auto-detect format from extension
                var ext = System.IO.Path.GetExtension(dlg.FileName).ToLowerInvariant();
                _config.Format = ext switch
                {
                    ".json" => ExportFormat.Json,
                    ".xml" => ExportFormat.Xml,
                    ".xlsx" => ExportFormat.Excel,
                    _ => ExportFormat.Csv
                };
                cmbFormat.SelectItemByText(_config.Format.ToString());
            }
        }

        private void LaunchExportWizard()
        {
            _config = BuildConfig();

            // Use the new 3-step flow: Configure → Select Columns → Run
            var selectStep = new uc_Export_SelectDSandFile(_services);
            var colStep    = new uc_Export_ColumnSelection(_services, _config);
            var runStep    = new uc_Export_Run(_services, _config);

            var wizardConfig = new WizardConfig
            {
                Key              = $"ExportWizard_{Guid.NewGuid():N}",
                Title            = "Export Wizard",
                Description      = "Choose source, select columns, then export data.",
                Style            = WizardStyle.HorizontalStepper,
                ShowProgressBar  = true,
                ShowStepList     = true,
                AllowBack        = true,
                AllowCancel      = true,
                ShowInlineErrors = true,
                Steps = new List<WizardStep>
                {
                    new WizardStep { Key = "select",  Title = "Configure",       Description = "Choose source entity and export destination.", Content = selectStep },
                    new WizardStep { Key = "columns", Title = "Select Columns",  Description = "Choose which columns to export.",               Content = colStep   },
                    new WizardStep { Key = "run",     Title = "Review & Export", Description = "Review summary and execute the export.",        Content = runStep   }
                }
            };

            wizardConfig.OnComplete = ctx =>
            {
                var summary = ctx.GetValue<ExportRunSummary?>(ExportWizardKeys.RunSummary, null);
                if (summary != null)
                    AppendLog($"Export complete: {summary.ExportedRows:N0} rows exported to {summary.FilePath}");
            };
            wizardConfig.OnCancel = _ => AppendLog("Export wizard cancelled.");

            var wizardInstance = WizardManager.CreateWizard(wizardConfig);
            wizardInstance.Context.SetValue(ExportWizardKeys.ExportConfig, _config);

            var owner  = FindForm();
            var result = owner == null ? wizardInstance.ShowDialog() : wizardInstance.ShowDialog(owner);
            AppendLog($"Export wizard closed: {result}");
        }

        private ExportConfiguration BuildConfig()
        {
            return new ExportConfiguration
            {
                SourceDataSourceName = cmbSourceDS.SelectedItem?.Text ?? string.Empty,
                SourceEntityName = cmbSourceEntity.SelectedItem?.Text ?? string.Empty,
                FilePath = txtFilePath.Text ?? string.Empty,
                Format = Enum.TryParse<ExportFormat>(cmbFormat.SelectedItem?.Text, out var fmt) ? fmt : ExportFormat.Csv,
                CsvDelimiter = txtDelimiter.Text ?? ",",
                IncludeHeaders = chkHeaders.CurrentValue,
                BatchSize = (int)(numBatchSize?.Value ?? 1000)
            };
        }

        private void AppendLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            if (txtLog.InvokeRequired) { txtLog.BeginInvoke(new Action<string>(AppendLog), message); return; }
            txtLog.AppendText($"{DateTime.Now:HH:mm:ss}  {message}{Environment.NewLine}");
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }
    }
}
