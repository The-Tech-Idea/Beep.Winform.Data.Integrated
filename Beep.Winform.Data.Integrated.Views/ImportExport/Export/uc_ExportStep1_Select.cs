using System.ComponentModel;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport.Models;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport.Export
{
    public partial class uc_ExportStep1_Select : TemplateUserControl, IWizardStepContent
    {
        private ExportConfiguration? _config;
        private bool _isComplete;

        public uc_ExportStep1_Select(IServiceProvider services) : base(services)
        {
            InitializeComponent();
            cmbSourceDS.SelectedItemChanged += (_, _) => OnSourceDSChanged();
            cmbSourceEntity.SelectedItemChanged += (_, _) => OnSourceEntityChanged();
            cmbFormat.SelectedItemChanged += (_, _) => OnFormatChanged();
            btnBrowse.Click += (_, _) => BrowseFile();
            radioDestMode.SelectionChanged += (_, _) => OnDestModeChanged();
        }

        public bool IsComplete => _isComplete;
        public string NextButtonText => "Next";
        public event EventHandler<StepValidationEventArgs>? ValidationStateChanged;

        public void OnStepEnter(WizardContext context)
        {
            LoadDataSources();
            _config = context.GetValue<ExportConfiguration?>(WizardKeys.ExportConfig, null) ?? new ExportConfiguration();
            RestoreSelections();
            OnDestModeChanged();
        }

        public void OnStepLeave(WizardContext context)
        {
            _config ??= new ExportConfiguration();
            _config.SourceDataSourceName = cmbSourceDS.SelectedItem?.Value?.ToString() ?? string.Empty;
            _config.SourceEntityName = cmbSourceEntity.SelectedItem?.Value?.ToString() ?? string.Empty;
            _config.FilePath = txtFilePath.Text;
            _config.Format = GetSelectedFormat();
            _config.DestDataSourceName = cmbDestDS.SelectedItem?.Value?.ToString() ?? string.Empty;
            _config.DestEntityName = cmbDestEntity.SelectedItem?.Value?.ToString() ?? string.Empty;
            context.SetValue(WizardKeys.ExportConfig, _config);
        }

        public WizardValidationResult Validate()
        {
            if (string.IsNullOrEmpty(_config?.SourceDataSourceName))
                return WizardValidationResult.Error("Please select a source data source.");
            if (string.IsNullOrEmpty(_config.SourceEntityName))
                return WizardValidationResult.Error("Please select a source entity.");
            if (GetDestMode() == ExportDestMode.File && string.IsNullOrEmpty(_config.FilePath))
                return WizardValidationResult.Error("Please specify an output file path.");
            if (GetDestMode() == ExportDestMode.DataSource && string.IsNullOrEmpty(_config.DestDataSourceName))
                return WizardValidationResult.Error("Please select a destination data source.");
            return WizardValidationResult.Success();
        }

        public Task<WizardValidationResult> ValidateAsync() => Task.FromResult(Validate());

        private void LoadDataSources()
        {
            if (Editor?.ConfigEditor?.DataConnections == null) return;
            var dsNames = Editor.ConfigEditor.DataConnections
                .Select(c => c.ConnectionName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .ToList();

            var items = dsNames.Select(n => new SimpleItem { Text = n, Value = n }).ToList();
            cmbSourceDS.ListItems = new BindingList<SimpleItem>(items);
            cmbDestDS.ListItems = new BindingList<SimpleItem>(items);

            cmbFormat.ListItems = new BindingList<SimpleItem>(new List<SimpleItem>
            {
                new() { Text = "CSV", Value = ExportFormat.Csv },
                new() { Text = "JSON", Value = ExportFormat.Json },
                new() { Text = "XML", Value = ExportFormat.Xml },
            });
            cmbFormat.SelectedIndex = 0;

            radioDestMode.AddItems(new List<SimpleItem>
            {
                new() { Text = "Export to File", Value = "File" },
                new() { Text = "Export to DataSource", Value = "DataSource" },
            });
        }

        private void OnSourceDSChanged()
        {
            var dsName = cmbSourceDS.SelectedItem?.Value?.ToString();
            if (string.IsNullOrEmpty(dsName)) return;
            var entities = Editor?.GetDataSource(dsName)?.GetEntitesList();
            if (entities == null) return;
            cmbSourceEntity.ListItems = new BindingList<SimpleItem>(
                entities.Select(e => new SimpleItem { Text = e, Value = e }).ToList());
        }

        private void OnSourceEntityChanged()
        {
            _config ??= new ExportConfiguration();
            _config.SourceDataSourceName = cmbSourceDS.SelectedItem?.Value?.ToString() ?? string.Empty;
            _config.SourceEntityName = cmbSourceEntity.SelectedItem?.Value?.ToString() ?? string.Empty;
            UpdateCompleteness();
        }

        private void OnFormatChanged()
        {
            UpdateFileExtension();
        }

        private void OnDestModeChanged()
        {
            var mode = GetDestMode();
            bool isFile = mode == ExportDestMode.File;

            lblFormat.Visible = isFile;
            cmbFormat.Visible = isFile;
            lblFilePath.Visible = isFile;
            txtFilePath.Visible = isFile;
            btnBrowse.Visible = isFile;

            lblDestDS.Visible = !isFile;
            cmbDestDS.Visible = !isFile;
            lblDestEntity.Visible = !isFile;
            cmbDestEntity.Visible = !isFile;

            UpdateCompleteness();
        }

        private void UpdateFileExtension()
        {
            var path = txtFilePath.Text;
            if (string.IsNullOrEmpty(path)) return;
            var format = GetSelectedFormat();
            var ext = ExportFormatWriter.GetExtension(format);
            var dir = Path.GetDirectoryName(path);
            var name = Path.GetFileNameWithoutExtension(path);
            txtFilePath.Text = Path.Combine(dir ?? string.Empty, name + ext);
        }

        private void BrowseFile()
        {
            var format = GetSelectedFormat();
            using var dlg = new SaveFileDialog { Filter = ExportFormatWriter.GetFileFilter(format) };
            if (dlg.ShowDialog() == DialogResult.OK)
                txtFilePath.Text = dlg.FileName;
        }

        private ExportFormat GetSelectedFormat()
        {
            var item = cmbFormat.SelectedItem?.Value;
            return item is ExportFormat f ? f : ExportFormat.Csv;
        }

        private ExportDestMode GetDestMode()
        {
            var val = radioDestMode.GetValue()?.ToString();
            return val == "DataSource" ? ExportDestMode.DataSource : ExportDestMode.File;
        }

        private void UpdateCompleteness()
        {
            var wasComplete = _isComplete;
            _isComplete = !string.IsNullOrEmpty(_config?.SourceDataSourceName)
                       && !string.IsNullOrEmpty(_config?.SourceEntityName)
                       && (GetDestMode() != ExportDestMode.File || !string.IsNullOrEmpty(_config?.FilePath))
                       && (GetDestMode() != ExportDestMode.DataSource || !string.IsNullOrEmpty(_config?.DestDataSourceName));
            if (wasComplete != _isComplete)
                ValidationStateChanged?.Invoke(this, new StepValidationEventArgs(_isComplete));
        }

        private void RestoreSelections()
        {
            if (_config == null) return;
            if (!string.IsNullOrEmpty(_config.SourceDataSourceName))
            {
                SelectCombo(cmbSourceDS, _config.SourceDataSourceName);
                OnSourceDSChanged();
                SelectCombo(cmbSourceEntity, _config.SourceEntityName);
            }
            txtFilePath.Text = _config.FilePath;
            if (!string.IsNullOrEmpty(_config.DestDataSourceName))
                SelectCombo(cmbDestDS, _config.DestDataSourceName);
        }

        private static void SelectCombo(BeepComboBox combo, string? value)
        {
            if (string.IsNullOrEmpty(value)) return;
            for (int i = 0; i < combo.ListItems?.Count; i++)
            {
                if (combo.ListItems[i]?.Value?.ToString() == value)
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
        }
    }
}
