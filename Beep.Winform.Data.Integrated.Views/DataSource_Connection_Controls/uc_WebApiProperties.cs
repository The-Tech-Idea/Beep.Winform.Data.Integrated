using System;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;

namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    /// <summary>
    /// Tab control for Web API Properties region of IConnectionProperties
    /// Properties: HttpMethod, TimeoutMs, MaxRetries, RetryIntervalMs, IgnoreSSLErrors, 
    /// ValidateServerCertificate, RequiresAuthentication, RequiresTokenRefresh
    /// </summary>
    public partial class uc_WebApiProperties : uc_DataConnectionPropertiesBaseControl
    {
        public uc_WebApiProperties()
        {
            InitializeComponent();
        }

        public override void SetupBindings(ConnectionProperties conn)
        {
            base.SetupBindings(conn);
            Text = "Web API";
            if (conn == null) return;

            // Clear existing bindings
            WebApi_HttpMethodbeepComboBox.DataBindings.Clear();
            WebApi_TimeoutMsbeepTextBox.DataBindings.Clear();
            WebApi_MaxRetriesbeepTextBox.DataBindings.Clear();
            WebApi_RetryIntervalMsbeepTextBox.DataBindings.Clear();
            WebApi_IgnoreSSLErrorsbeepCheckBox.DataBindings.Clear();
            WebApi_ValidateServerCertbeepCheckBox.DataBindings.Clear();
            WebApi_RequiresAuthbeepCheckBox.DataBindings.Clear();
            WebApi_RequiresTokenRefreshbeepCheckBox.DataBindings.Clear();

            // Setup HttpMethod ComboBox
            var httpMethods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS" };
            WebApi_HttpMethodbeepComboBox.ListItems = httpMethods
                .Select(m => new Winform.Controls.Models.SimpleItem { Text = m, Value = m })
                .ToBindingList();
            WebApi_HttpMethodbeepComboBox.DataBindings.Add(new Binding("SelectedValue", conn, nameof(conn.HttpMethod), true, DataSourceUpdateMode.OnPropertyChanged));
            WebApi_HttpMethodbeepComboBox.SetValue(conn.HttpMethod ?? "GET");

            // Bindings for Web API Properties
            WebApi_TimeoutMsbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.TimeoutMs), true, DataSourceUpdateMode.OnPropertyChanged));
            WebApi_MaxRetriesbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.MaxRetries), true, DataSourceUpdateMode.OnPropertyChanged));
            WebApi_RetryIntervalMsbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.RetryIntervalMs), true, DataSourceUpdateMode.OnPropertyChanged));
            WebApi_IgnoreSSLErrorsbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.IgnoreSSLErrors), true, DataSourceUpdateMode.OnPropertyChanged));
            WebApi_ValidateServerCertbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.ValidateServerCertificate), true, DataSourceUpdateMode.OnPropertyChanged));
            WebApi_RequiresAuthbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.RequiresAuthentication), true, DataSourceUpdateMode.OnPropertyChanged));
            WebApi_RequiresTokenRefreshbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.RequiresTokenRefresh), true, DataSourceUpdateMode.OnPropertyChanged));
        }
    }
}
