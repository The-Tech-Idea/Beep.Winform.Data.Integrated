using System;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;

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

            // Fill enums
            Provider_CategorybeepComboBox.ListItems = Enum.GetValues(typeof(DatasourceCategory))
                .Cast<DatasourceCategory>()
                .Select(e => new TheTechIdea.Beep.Winform.Controls.Models.SimpleItem
                {
                    Text = e.ToString(),
                    Value = e
                }).ToBindingList();

            Provider_DatabaseTypebeepComboBox.ListItems = Enum.GetValues(typeof(DataSourceType))
                .Cast<DataSourceType>()
                .Select(e => new TheTechIdea.Beep.Winform.Controls.Models.SimpleItem
                {
                    Text = e.ToString(),
                    Value = e
                }).ToBindingList();

            // Bindings
            Provider_CategorybeepComboBox.DataBindings.Add(new Binding("SelectedValue", conn, nameof(conn.Category), true, DataSourceUpdateMode.OnPropertyChanged));
            Provider_DatabaseTypebeepComboBox.DataBindings.Add(new Binding("SelectedValue", conn, nameof(conn.DatabaseType), true, DataSourceUpdateMode.OnPropertyChanged));
            Provider_DriverNamebeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.DriverName), true, DataSourceUpdateMode.OnPropertyChanged));
            Provider_DriverVersionbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.DriverVersion), true, DataSourceUpdateMode.OnPropertyChanged));

            // Set selected values
            Provider_CategorybeepComboBox.SetValue(conn.Category);
            Provider_DatabaseTypebeepComboBox.SetValue(conn.DatabaseType);
        }
    }
}
