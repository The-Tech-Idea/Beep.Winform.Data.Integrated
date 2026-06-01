using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Controls.Wizards;
using TheTechIdea.Beep.Winform.Default.Views.Template;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    /// <summary>
    /// Export wizard step 1: choose source DS/entity plus the export destination
    /// (either a file with format/delimiter/encoding options, or another data source).
    /// </summary>
    public partial class uc_Export_SelectDSandFile : TemplateUserControl, IWizardStepContent
    {
        private bool _isInitializing;

        public uc_Export_SelectDSandFile(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            WireEvents();
            PopulateStaticCombos();
        }

        // ── IWizardStepContent ────────────────────────────────────────────────

        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;

        public bool IsComplete => ValidateStep().IsValid;

        public string NextButtonText => string.Empty;

        public void OnStepEnter(WizardContext context)
        {
            _isInitializing = true;
            try
            {
                LoadDataSources();

                var config = context.GetValue<ExportConfiguration?>(ExportWizardKeys.ExportConfig, null)
                             ?? new ExportConfiguration();
                RestoreFromConfig(config);
            }
            finally { _isInitializing = false; }

            RaiseValidationState();
        }

        public void OnStepLeave(WizardContext context)
        {
            var config = context.GetValue<ExportConfiguration?>(ExportWizardKeys.ExportConfig, null)
                         ?? new ExportConfiguration();
            ApplyToConfig(config);
            context.SetValue(ExportWizardKeys.ExportConfig, config);
        }

        WizardValidationResult IWizardStepContent.Validate() => ValidateStep();

        public Task<WizardValidationResult> ValidateAsync() => Task.FromResult(ValidateStep());

        // ── Private helpers ────────────────────────────────────────────────────

        private void WireEvents()
        {
            cmbSourceDS.SelectedItemChanged    += (_, _) => OnSourceDsChanged();
            cmbDestMode.SelectedItemChanged    += (_, _) => OnDestModeChanged();
            cmbDestDS.SelectedItemChanged      += (_, _) => OnDestDsChanged();
            cmbFormat.SelectedItemChanged      += (_, _) => OnFormatChanged();
            txtFilePath.TextChanged            += (_, _) => { if (!_isInitializing) RaiseValidationState(); };
            cmbSourceEntity.SelectedItemChanged += (_, _) => { if (!_isInitializing) RaiseValidationState(); };
            cmbDestEntity.SelectedItemChanged  += (_, _) => { if (!_isInitializing) RaiseValidationState(); };
            btnBrowse.Click                    += BtnBrowse_Click;
        }

        private void PopulateStaticCombos()
        {
            // Destination mode
            cmbDestMode.ListItems = new BindingList<SimpleItem>
            {
                new SimpleItem { Text = "File",        Item = ExportDestMode.File },
                new SimpleItem { Text = "Data Source", Item = ExportDestMode.DataSource }
            };
            cmbDestMode.SelectItemByText("File");

            // Format
            cmbFormat.ListItems = new BindingList<SimpleItem>
            {
                new SimpleItem { Text = "CSV",  Item = ExportFormat.Csv  },
                new SimpleItem { Text = "JSON", Item = ExportFormat.Json },
                new SimpleItem { Text = "XML",  Item = ExportFormat.Xml  }
            };
            cmbFormat.SelectItemByText("CSV");

            // Encoding
            cmbEncoding.ListItems = new BindingList<SimpleItem>
            {
                new SimpleItem { Text = "UTF-8",       Item = "UTF-8"  },
                new SimpleItem { Text = "UTF-16",      Item = "UTF-16" },
                new SimpleItem { Text = "ASCII",       Item = "ASCII"  },
                new SimpleItem { Text = "ISO-8859-1",  Item = "ISO-8859-1" }
            };
            cmbEncoding.SelectItemByText("UTF-8");

            txtDelimiter.Text = ",";
            chkIncludeHeaders.CurrentValue = true;

            ApplyDestModeVisibility(ExportDestMode.File);
        }

        private void LoadDataSources()
        {
            var names = (Editor?.ConfigEditor?.DataConnections ?? new List<ConnectionProperties>())
                .Select(p => p.ConnectionName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n)
                .Select(n => new SimpleItem { Text = n, Item = n })
                .ToList();

            var list = new BindingList<SimpleItem>(names);
            cmbSourceDS.ListItems = list;
            cmbDestDS.ListItems   = new BindingList<SimpleItem>(names.ToList());
        }

        private void LoadEntities(BeepComboBox combo, string dataSourceName, string restore = "")
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
                var sel = !string.IsNullOrWhiteSpace(restore) ? restore : combo.SelectedItem?.Text ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(sel)) combo.SelectItemByText(sel);
            }
            catch (Exception ex)
            {
                Editor?.AddLogMessage("ExportWizard", $"Error loading entities for '{dataSourceName}': {ex.Message}",
                    DateTime.Now, 0, null, Errors.Failed);
            }
        }

        private void RestoreFromConfig(ExportConfiguration config)
        {
            if (!string.IsNullOrWhiteSpace(config.SourceDataSourceName))
                cmbSourceDS.SelectItemByText(config.SourceDataSourceName);
            LoadEntities(cmbSourceEntity, cmbSourceDS.SelectedItem?.Text ?? string.Empty, config.SourceEntityName);

            if (!string.IsNullOrWhiteSpace(config.FilePath))
            {
                txtFilePath.Text = config.FilePath;
                cmbDestMode.SelectItemByText("File");
            }
            else if (!string.IsNullOrWhiteSpace(config.DestDataSourceName))
            {
                cmbDestMode.SelectItemByText("Data Source");
                cmbDestDS.SelectItemByText(config.DestDataSourceName);
                LoadEntities(cmbDestEntity, config.DestDataSourceName, config.DestEntityName);
            }

            cmbFormat.SelectItemByText(config.Format.ToString());
            txtDelimiter.Text = string.IsNullOrWhiteSpace(config.CsvDelimiter) ? "," : config.CsvDelimiter;
            chkIncludeHeaders.CurrentValue = config.IncludeHeaders;
            cmbEncoding.SelectItemByText(string.IsNullOrWhiteSpace(config.Encoding) ? "UTF-8" : config.Encoding);

            ApplyDestModeVisibility(GetSelectedDestMode());
        }

        private void ApplyToConfig(ExportConfiguration config)
        {
            config.SourceDataSourceName = cmbSourceDS.SelectedItem?.Text ?? string.Empty;
            config.SourceEntityName     = cmbSourceEntity.SelectedItem?.Text ?? string.Empty;

            var mode = GetSelectedDestMode();
            if (mode == ExportDestMode.File)
            {
                config.FilePath          = txtFilePath.Text.Trim();
                config.DestDataSourceName = string.Empty;
                config.DestEntityName     = string.Empty;
            }
            else
            {
                config.FilePath          = string.Empty;
                config.DestDataSourceName = cmbDestDS.SelectedItem?.Text ?? string.Empty;
                config.DestEntityName     = cmbDestEntity.SelectedItem?.Text ?? string.Empty;
            }

            if (cmbFormat.SelectedItem?.Item is ExportFormat fmt)
                config.Format = fmt;

            config.CsvDelimiter    = txtDelimiter.Text.Length == 0 ? "," : txtDelimiter.Text;
            config.IncludeHeaders  = chkIncludeHeaders.CurrentValue;
            config.Encoding        = cmbEncoding.SelectedItem?.Text ?? "UTF-8";
        }

        private WizardValidationResult ValidateStep()
        {
            if (string.IsNullOrWhiteSpace(cmbSourceDS.SelectedItem?.Text))
                return WizardValidationResult.Error("Select a source data source.");
            if (string.IsNullOrWhiteSpace(cmbSourceEntity.SelectedItem?.Text))
                return WizardValidationResult.Error("Select a source entity.");

            var mode = GetSelectedDestMode();
            if (mode == ExportDestMode.File)
            {
                if (string.IsNullOrWhiteSpace(txtFilePath.Text))
                    return WizardValidationResult.Error("Specify an output file path.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(cmbDestDS.SelectedItem?.Text))
                    return WizardValidationResult.Error("Select a destination data source.");
                if (string.IsNullOrWhiteSpace(cmbDestEntity.SelectedItem?.Text))
                    return WizardValidationResult.Error("Select a destination entity.");
            }

            return WizardValidationResult.Success();
        }

        private void RaiseValidationState()
        {
            var result = ValidateStep();
            ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(result.IsValid, result.ErrorMessage));
        }

        private ExportDestMode GetSelectedDestMode()
            => cmbDestMode.SelectedItem?.Item is ExportDestMode m ? m : ExportDestMode.File;

        private void ApplyDestModeVisibility(ExportDestMode mode)
        {
            pnlFileOptions.Visible = mode == ExportDestMode.File;
            pnlDsOptions.Visible   = mode == ExportDestMode.DataSource;
        }

        // ── Event handlers ─────────────────────────────────────────────────────

        private void OnSourceDsChanged()
        {
            if (_isInitializing) return;
            LoadEntities(cmbSourceEntity, cmbSourceDS.SelectedItem?.Text ?? string.Empty);
            RaiseValidationState();
        }

        private void OnDestModeChanged()
        {
            ApplyDestModeVisibility(GetSelectedDestMode());
            if (!_isInitializing) RaiseValidationState();
        }

        private void OnDestDsChanged()
        {
            if (_isInitializing) return;
            LoadEntities(cmbDestEntity, cmbDestDS.SelectedItem?.Text ?? string.Empty);
            RaiseValidationState();
        }

        private void OnFormatChanged()
        {
            if (_isInitializing) return;
            var isCsv = cmbFormat.SelectedItem?.Item is ExportFormat f && f == ExportFormat.Csv;
            lblDelimiter.Visible  = isCsv;
            txtDelimiter.Visible  = isCsv;
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            var fmt = cmbFormat.SelectedItem?.Item is ExportFormat f ? f : ExportFormat.Csv;
            var ext = fmt switch
            {
                ExportFormat.Json => "json",
                ExportFormat.Xml  => "xml",
                _                 => "csv"
            };

            using var dlg = new SaveFileDialog
            {
                Title  = "Choose export file",
                Filter = $"{ext.ToUpperInvariant()} files (*.{ext})|*.{ext}|All files (*.*)|*.*",
                DefaultExt = ext
            };

            if (!string.IsNullOrWhiteSpace(txtFilePath.Text))
            {
                dlg.InitialDirectory = Path.GetDirectoryName(txtFilePath.Text) ?? string.Empty;
                dlg.FileName = Path.GetFileName(txtFilePath.Text);
            }

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = dlg.FileName;
                RaiseValidationState();
            }
        }
    }

    /// <summary>Destination kind for an export operation.</summary>
    public enum ExportDestMode { File, DataSource }
}
