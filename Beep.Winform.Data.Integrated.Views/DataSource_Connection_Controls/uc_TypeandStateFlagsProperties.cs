using System;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;

namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    /// <summary>
    /// Tab control for Type and State Flags region of IConnectionProperties
    /// Properties: IsLocal, IsRemote, IsWebApi, IsFile, IsDatabase, IsComposite, IsCloud, IsFavourite, IsInMemory
    /// </summary>
    public partial class uc_TypeandStateFlagsProperties : uc_DataConnectionPropertiesBaseControl
    {
        public uc_TypeandStateFlagsProperties()
        {
            InitializeComponent();
        }

        public override void SetupBindings(ConnectionProperties conn)
        {
            base.SetupBindings(conn);
            if (conn == null) return;

            // Clear existing bindings
            Flags_IsLocalbeepCheckBox.DataBindings.Clear();
            Flags_IsRemotebeepCheckBox.DataBindings.Clear();
            Flags_IsWebApibeepCheckBox.DataBindings.Clear();
            Flags_IsFilebeepCheckBox.DataBindings.Clear();
            Flags_IsDatabasebeepCheckBox.DataBindings.Clear();
            Flags_IsCompositebeepCheckBox.DataBindings.Clear();
            Flags_IsCloudbeepCheckBox.DataBindings.Clear();
            Flags_IsFavouritebeepCheckBox.DataBindings.Clear();
            Flags_IsInMemorybeepCheckBox.DataBindings.Clear();

            // Bindings for Type and State Flags region
            Flags_IsLocalbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.IsLocal), true, DataSourceUpdateMode.OnPropertyChanged));
            Flags_IsRemotebeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.IsRemote), true, DataSourceUpdateMode.OnPropertyChanged));
            Flags_IsWebApibeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.IsWebApi), true, DataSourceUpdateMode.OnPropertyChanged));
            Flags_IsFilebeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.IsFile), true, DataSourceUpdateMode.OnPropertyChanged));
            Flags_IsDatabasebeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.IsDatabase), true, DataSourceUpdateMode.OnPropertyChanged));
            Flags_IsCompositebeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.IsComposite), true, DataSourceUpdateMode.OnPropertyChanged));
            Flags_IsCloudbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.IsCloud), true, DataSourceUpdateMode.OnPropertyChanged));
            Flags_IsFavouritebeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.IsFavourite), true, DataSourceUpdateMode.OnPropertyChanged));
            Flags_IsInMemorybeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.IsInMemory), true, DataSourceUpdateMode.OnPropertyChanged));
        }
    }
}
