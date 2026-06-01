using System;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;

namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    /// <summary>
    /// Tab control for General Properties region of IConnectionProperties
    /// Properties: ID, GuidID, ConnectionName, ConnectionString, Category, Favourite, IsDefault, Drawn
    /// </summary>
    public partial class uc_GeneralProperties : uc_DataConnectionPropertiesBaseControl
    {
        public uc_GeneralProperties()
        {
            InitializeComponent();
        }

        public override void SetupBindings(ConnectionProperties conn)
        {
            base.SetupBindings(conn);
            if (conn == null) return;

            // Clear existing bindings
            General_IDbeepTextBox.DataBindings.Clear();
            General_GuidIDbeepTextBox.DataBindings.Clear();
            General_ConnectionNamebeepTextBox.DataBindings.Clear();
            General_ConnectionStringbeepTextBox.DataBindings.Clear();
            General_CategorybeepComboBox.DataBindings.Clear();
            General_FavouritebeepCheckBox.DataBindings.Clear();
            General_IsDefaultbeepCheckBox.DataBindings.Clear();
            General_DrawnbeepCheckBox.DataBindings.Clear();

            // Bindings for General Properties region
            General_IDbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.ID), true, DataSourceUpdateMode.OnPropertyChanged));
            General_GuidIDbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.GuidID), true, DataSourceUpdateMode.OnPropertyChanged));
            General_ConnectionNamebeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.ConnectionName), true, DataSourceUpdateMode.OnPropertyChanged));
            General_ConnectionStringbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.ConnectionString), true, DataSourceUpdateMode.OnPropertyChanged));
            General_CategorybeepComboBox.DataBindings.Add(new Binding("SelectedValue", conn, nameof(conn.Category), true, DataSourceUpdateMode.OnPropertyChanged));
            General_FavouritebeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.Favourite), true, DataSourceUpdateMode.OnPropertyChanged));
            General_IsDefaultbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.IsDefault), true, DataSourceUpdateMode.OnPropertyChanged));
            General_DrawnbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.Drawn), true, DataSourceUpdateMode.OnPropertyChanged));

            // Setup Category ComboBox
            General_CategorybeepComboBox.ListItems = Enum.GetValues(typeof(DatasourceCategory))
                .Cast<DatasourceCategory>()
                .Select(c => new Winform.Controls.Models.SimpleItem { Text = c.ToString(), Value = c })
                .ToBindingList();
            General_CategorybeepComboBox.SetValue(conn.Category);

            // Read-only fields
            General_IDbeepTextBox.ReadOnly = true;
            General_GuidIDbeepTextBox.ReadOnly = true;
        }
    }
}
