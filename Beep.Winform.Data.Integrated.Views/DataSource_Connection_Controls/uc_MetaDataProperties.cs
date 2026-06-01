using System;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;

namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    public partial class uc_MetaDataProperties : uc_DataConnectionPropertiesBaseControl
    {
        public uc_MetaDataProperties()
        {
            InitializeComponent();
        }

        public override void SetupBindings(ConnectionProperties conn)
        {
            base.SetupBindings(conn);
            Text = "Metadata";
            if (conn == null)
            {
                return;
            }

            Meta_SchemaNamebeepTextBox.DataBindings.Clear();
            Meta_SchemaNamebeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.SchemaName), true, DataSourceUpdateMode.OnPropertyChanged));

            Meta_MetadataCatalogbeepTextBox.DataBindings.Clear();
            Meta_MetadataCatalogbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.Database), true, DataSourceUpdateMode.OnPropertyChanged));

            Meta_EntityFilterbeepTextBox.DataBindings.Clear();
            Meta_EntityFilterbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.CompositeLayerName), true, DataSourceUpdateMode.OnPropertyChanged));

            Meta_RefreshSecondsbeepTextBox.DataBindings.Clear();
            Meta_RefreshSecondsbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.Timeout), true, DataSourceUpdateMode.OnPropertyChanged));

            Meta_IncludeViewsbeepCheckBox.DataBindings.Clear();
            Meta_IncludeSystemObjectsbeepCheckBox.DataBindings.Clear();
            Meta_IncludeViewsbeepCheckBox.Enabled = false;
            Meta_IncludeSystemObjectsbeepCheckBox.Enabled = false;
        }
    }
}
