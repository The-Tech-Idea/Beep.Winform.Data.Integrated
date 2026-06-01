using System;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;

namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    /// <summary>
    /// Tab control for Web API Authentication region of IConnectionProperties
    /// Properties: ClientId, ClientSecret, AuthType, AuthUrl, TokenUrl, Scope, GrantType,
    /// ApiKeyHeader, RedirectUri, AuthCode, Proxy settings
    /// </summary>
    public partial class uc_webapiAuthenticationProperties : uc_DataConnectionPropertiesBaseControl
    {
        public uc_webapiAuthenticationProperties()
        {
            InitializeComponent();
        }

        public override void SetupBindings(ConnectionProperties conn)
        {
            base.SetupBindings(conn);
            Text = "OAuth/API Auth";
            if (conn == null) return;

            // Clear existing bindings
            OAuth_ClientIdbeepTextBox.DataBindings.Clear();
            OAuth_ClientSecretbeepTextBox.DataBindings.Clear();
            OAuth_AuthTypebeepComboBox.DataBindings.Clear();
            OAuth_AuthUrlbeepTextBox.DataBindings.Clear();
            OAuth_TokenUrlbeepTextBox.DataBindings.Clear();
            OAuth_ScopebeepTextBox.DataBindings.Clear();
            OAuth_GrantTypebeepTextBox.DataBindings.Clear();
            OAuth_ApiKeyHeaderbeepTextBox.DataBindings.Clear();
            OAuth_RedirectUribeepTextBox.DataBindings.Clear();
            OAuth_AuthCodebeepTextBox.DataBindings.Clear();
            // Proxy settings
            OAuth_UseProxybeepCheckBox.DataBindings.Clear();
            OAuth_ProxyUrlbeepTextBox.DataBindings.Clear();
            OAuth_ProxyPortbeepTextBox.DataBindings.Clear();
            OAuth_ProxyUserbeepTextBox.DataBindings.Clear();
            OAuth_ProxyPasswordbeepTextBox.DataBindings.Clear();

            // Setup AuthType ComboBox
            OAuth_AuthTypebeepComboBox.ListItems = Enum.GetValues(typeof(AuthTypeEnum))
                .Cast<AuthTypeEnum>()
                .Select(a => new Winform.Controls.Models.SimpleItem { Text = a.ToString(), Value = a })
                .ToBindingList();
            OAuth_AuthTypebeepComboBox.DataBindings.Add(new Binding("SelectedValue", conn, nameof(conn.AuthType), true, DataSourceUpdateMode.OnPropertyChanged));
            OAuth_AuthTypebeepComboBox.SetValue(conn.AuthType);

            // Bindings for OAuth properties
            OAuth_ClientIdbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.ClientId), true, DataSourceUpdateMode.OnPropertyChanged));
            OAuth_ClientSecretbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.ClientSecret), true, DataSourceUpdateMode.OnPropertyChanged));
            OAuth_AuthUrlbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.AuthUrl), true, DataSourceUpdateMode.OnPropertyChanged));
            OAuth_TokenUrlbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.TokenUrl), true, DataSourceUpdateMode.OnPropertyChanged));
            OAuth_ScopebeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.Scope), true, DataSourceUpdateMode.OnPropertyChanged));
            OAuth_GrantTypebeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.GrantType), true, DataSourceUpdateMode.OnPropertyChanged));
            OAuth_ApiKeyHeaderbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.ApiKeyHeader), true, DataSourceUpdateMode.OnPropertyChanged));
            OAuth_RedirectUribeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.RedirectUri), true, DataSourceUpdateMode.OnPropertyChanged));
            OAuth_AuthCodebeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.AuthCode), true, DataSourceUpdateMode.OnPropertyChanged));

            // Proxy settings
            OAuth_UseProxybeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.UseProxy), true, DataSourceUpdateMode.OnPropertyChanged));
            OAuth_ProxyUrlbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.ProxyUrl), true, DataSourceUpdateMode.OnPropertyChanged));
            OAuth_ProxyPortbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.ProxyPort), true, DataSourceUpdateMode.OnPropertyChanged));
            OAuth_ProxyUserbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.ProxyUser), true, DataSourceUpdateMode.OnPropertyChanged));
            OAuth_ProxyPasswordbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.ProxyPassword), true, DataSourceUpdateMode.OnPropertyChanged));

            // Set password mode for secrets
            OAuth_ClientSecretbeepTextBox.UseSystemPasswordChar = true;
            OAuth_ProxyPasswordbeepTextBox.UseSystemPasswordChar = true;
        }
    }
}
