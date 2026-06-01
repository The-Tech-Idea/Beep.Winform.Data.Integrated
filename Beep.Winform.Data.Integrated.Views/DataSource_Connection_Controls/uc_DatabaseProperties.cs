using System;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Winform.Controls.Common;
using TheTechIdea.Beep.Winform.Controls.Models;

namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    /// <summary>
    /// Tab control for Database Properties region of IConnectionProperties
    /// Properties: DatabaseType, Database, Databases, SchemaName, OracleSIDorService
    /// </summary>
    public partial class uc_DatabaseProperties : uc_DataConnectionPropertiesBaseControl
    {
        public event EventHandler? DatabaseTypeChanged;

        public uc_DatabaseProperties()
        {
            InitializeComponent();
        }

        public override void SetupBindings(ConnectionProperties conn)
        {
            base.SetupBindings(conn);
            if (conn == null) return;

            // Clear existing bindings
            Database_DatabaseTypebeepComboBox.DataBindings.Clear();
            Database_DatabasebeepTextBox.DataBindings.Clear();
            Database_SchemaNamebeepTextBox.DataBindings.Clear();
            Database_OracleSIDorServicebeepTextBox.DataBindings.Clear();

            // Setup DatabaseType ComboBox
            Database_DatabaseTypebeepComboBox.ListItems = Enum.GetValues(typeof(DataSourceType))
                .Cast<DataSourceType>()
                .Select(c => new SimpleItem { Text = c.ToString(), Value = c })
                .ToBindingList();
            Database_DatabaseTypebeepComboBox.DataBindings.Add(new Binding("SelectedValue", conn, nameof(conn.DatabaseType), true, DataSourceUpdateMode.OnPropertyChanged));
            Database_DatabaseTypebeepComboBox.SelectedItemChanged -= Database_DatabaseTypebeepComboBox_SelectedItemChanged;
            Database_DatabaseTypebeepComboBox.SelectedItemChanged += Database_DatabaseTypebeepComboBox_SelectedItemChanged;
            Database_DatabaseTypebeepComboBox.SetValue(conn.DatabaseType);

            // Bindings for Database Properties region
            Database_DatabasebeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.Database), true, DataSourceUpdateMode.OnPropertyChanged));
            Database_SchemaNamebeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.SchemaName), true, DataSourceUpdateMode.OnPropertyChanged));
            Database_OracleSIDorServicebeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.OracleSIDorService), true, DataSourceUpdateMode.OnPropertyChanged));
        }

        private void Database_DatabaseTypebeepComboBox_SelectedItemChanged(object sender, SelectedItemChangedEventArgs e)
        {
            if (ConnectionProperties == null || e?.SelectedItem is not SimpleItem selectedItem)
            {
                return;
            }

            if (selectedItem.Value is DataSourceType sourceType)
            {
                ConnectionProperties.DatabaseType = sourceType;
            }
            else if (selectedItem.Value != null && Enum.TryParse(selectedItem.Value.ToString(), true, out DataSourceType parsedType))
            {
                ConnectionProperties.DatabaseType = parsedType;
            }

            DatabaseTypeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
