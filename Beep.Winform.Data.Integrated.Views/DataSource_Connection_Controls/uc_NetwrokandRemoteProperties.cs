using System;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;

namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    /// <summary>
    /// Tab control for Network and Remote Connection Properties region of IConnectionProperties
    /// Properties: Host, Port, Url
    /// </summary>
    public partial class uc_NetwrokandRemoteProperties : uc_DataConnectionPropertiesBaseControl
    {
        public uc_NetwrokandRemoteProperties()
        {
            InitializeComponent();
        }

        public override void SetupBindings(ConnectionProperties conn)
        {
            base.SetupBindings(conn);
            Text = "Network";
            if (conn == null) return;

            // Clear existing bindings
            Network_HostbeepTextBox.DataBindings.Clear();
            Network_PortbeepTextBox.DataBindings.Clear();
            Network_UrlbeepTextBox.DataBindings.Clear();

            // Bindings for Network and Remote Connection Properties region
            Network_HostbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.Host), true, DataSourceUpdateMode.OnPropertyChanged));
            Network_PortbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.Port), true, DataSourceUpdateMode.OnPropertyChanged));
            Network_UrlbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.Url), true, DataSourceUpdateMode.OnPropertyChanged));
        }
    }
}
