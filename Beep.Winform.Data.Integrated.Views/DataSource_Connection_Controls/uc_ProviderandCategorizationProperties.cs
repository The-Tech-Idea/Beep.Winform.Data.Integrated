using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Controls.Models;

namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    public partial class uc_ProviderandCategorizationProperties : uc_DataConnectionPropertiesBaseControl
    {
        public uc_ProviderandCategorizationProperties()
        {
            InitializeComponent();
        }

        public override void SetupBindings(ConnectionProperties conn)
        {
            base.SetupBindings(conn);
            Text = "Providers";
            if (conn == null) return;

            Provider_CategorybeepComboBox.DataBindings.Clear();
            Provider_DatabaseTypebeepComboBox.DataBindings.Clear();
            Provider_DriverNamebeepTextBox.DataBindings.Clear();
            Provider_DriverVersionbeepTextBox.DataBindings.Clear();

            // Load Categories from enum (static)
            Provider_CategorybeepComboBox.ListItems = Enum.GetValues(typeof(DatasourceCategory))
                .Cast<DatasourceCategory>()
                .Select(e => new SimpleItem { Text = e.ToString(), Value = e })
                .ToBindingList();

            // Load DatabaseTypes filtered by selected Category (dynamic — matches WPF)
            LoadDatabaseTypesForCategory(conn.Category);

            // Wire events for dynamic filtering chain (Category → DatabaseType → ClassHandler)
            Provider_CategorybeepComboBox.SelectedIndexChanged -= OnCategoryChanged;
            Provider_CategorybeepComboBox.SelectedIndexChanged += OnCategoryChanged;
            Provider_DatabaseTypebeepComboBox.SelectedIndexChanged -= OnDatabaseTypeChanged;
            Provider_DatabaseTypebeepComboBox.SelectedIndexChanged += OnDatabaseTypeChanged;

            // Bindings
            Provider_CategorybeepComboBox.DataBindings.Add(new Binding("SelectedValue", conn, nameof(conn.Category), true, DataSourceUpdateMode.OnPropertyChanged));
            Provider_DatabaseTypebeepComboBox.DataBindings.Add(new Binding("SelectedValue", conn, nameof(conn.DatabaseType), true, DataSourceUpdateMode.OnPropertyChanged));
            Provider_DriverNamebeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.DriverName), true, DataSourceUpdateMode.OnPropertyChanged));
            Provider_DriverVersionbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.DriverVersion), true, DataSourceUpdateMode.OnPropertyChanged));

            // Set selected values
            Provider_CategorybeepComboBox.SetValue(conn.Category);
            Provider_DatabaseTypebeepComboBox.SetValue(conn.DatabaseType);
        }

        /// <summary>Dynamic filtering: when Category changes, reload DatabaseTypes from driver catalog.</summary>
        private void OnCategoryChanged(object? sender, EventArgs e)
        {
            if (ConnectionProperties == null) return;
            var selectedCategory = (DatasourceCategory)(Provider_CategorybeepComboBox.SelectedValue
                ?? DatasourceCategory.RDBMS);
            LoadDatabaseTypesForCategory(selectedCategory);
        }

        /// <summary>Dynamic filtering: when DatabaseType changes, auto-fill DriverName/Version from best-match driver.</summary>
        private void OnDatabaseTypeChanged(object? sender, EventArgs e)
        {
            if (ConnectionProperties == null) return;
            var selectedType = (DataSourceType)(Provider_DatabaseTypebeepComboBox.SelectedValue
                ?? DataSourceType.NONE);
            var bestMatch = DriverCatalog
                .Where(d => d.DatasourceType == selectedType && !string.IsNullOrWhiteSpace(d.classHandler))
                .OrderBy(d => d.PackageName)
                .FirstOrDefault();

            if (bestMatch != null)
            {
                if (string.IsNullOrWhiteSpace(ConnectionProperties.DriverName))
                    ConnectionProperties.DriverName = bestMatch.classHandler;
                if (string.IsNullOrWhiteSpace(ConnectionProperties.DriverVersion))
                    ConnectionProperties.DriverVersion = bestMatch.NuggetVersion ?? bestMatch.version;
            }
        }

        /// <summary>Load DatabaseTypes filtered by the selected Category from the driver catalog.</summary>
        private void LoadDatabaseTypesForCategory(DatasourceCategory category)
        {
            var availableTypes = DriverCatalog
                .Where(d => d.DatasourceCategory == category)
                .Select(d => d.DatasourceType)
                .Distinct()
                .OrderBy(t => t.ToString())
                .ToList();

            // Fallback: if catalog is empty, show all enum values
            if (availableTypes.Count == 0)
                availableTypes = Enum.GetValues(typeof(DataSourceType)).Cast<DataSourceType>().ToList();

            Provider_DatabaseTypebeepComboBox.ListItems = availableTypes
                .Select(e => new SimpleItem { Text = e.ToString(), Value = e })
                .ToBindingList();
        }
    }
}
