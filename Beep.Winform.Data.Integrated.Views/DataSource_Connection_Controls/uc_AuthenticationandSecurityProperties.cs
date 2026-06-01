using System;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;

namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    /// <summary>
    /// Tab control for Authentication and Security region of IConnectionProperties
    /// Properties: UserID, Password, ApiKey, KeyToken, CertificatePath, IntegratedSecurity, 
    /// PersistSecurityInfo, TrustedConnection, EncryptConnection, MultiSubnetFailover,
    /// TrustServerCertificate, AllowPublicKeyRetrieval, UseSSL, RequireSSL, etc.
    /// </summary>
    public partial class uc_AuthenticationandSecurityProperties : uc_DataConnectionPropertiesBaseControl
    {
        public uc_AuthenticationandSecurityProperties()
        {
            InitializeComponent();
        }

        public override void SetupBindings(ConnectionProperties conn)
        {
            base.SetupBindings(conn);
            Text = "Authentication";
            if (conn == null) return;

            // Clear existing bindings - Credentials
            Auth_UserIDbeepTextBox.DataBindings.Clear();
            Auth_PasswordbeepTextBox.DataBindings.Clear();
            Auth_ApiKeybeepTextBox.DataBindings.Clear();
            Auth_KeyTokenbeepTextBox.DataBindings.Clear();
            Auth_CertificatePathbeepTextBox.DataBindings.Clear();

            // Clear existing bindings - Security Flags
            Auth_IntegratedSecuritybeepCheckBox.DataBindings.Clear();
            Auth_PersistSecurityInfobeepCheckBox.DataBindings.Clear();
            Auth_TrustedConnectionbeepCheckBox.DataBindings.Clear();
            Auth_EncryptConnectionbeepCheckBox.DataBindings.Clear();
            Auth_MultiSubnetFailoverbeepCheckBox.DataBindings.Clear();
            Auth_TrustServerCertificatebeepCheckBox.DataBindings.Clear();
            Auth_AllowPublicKeyRetrievalbeepCheckBox.DataBindings.Clear();
            Auth_UseSSLbeepCheckBox.DataBindings.Clear();
            Auth_RequireSSLbeepCheckBox.DataBindings.Clear();
            Auth_BypassServerCertValidationbeepCheckBox.DataBindings.Clear();
            Auth_UseWindowsAuthbeepCheckBox.DataBindings.Clear();
            Auth_UseOAuthbeepCheckBox.DataBindings.Clear();
            Auth_UseApiKeybeepCheckBox.DataBindings.Clear();
            Auth_UseCertificatebeepCheckBox.DataBindings.Clear();
            Auth_UseUserAndPasswordbeepCheckBox.DataBindings.Clear();
            Auth_SavePasswordbeepCheckBox.DataBindings.Clear();
            Auth_ReadOnlybeepCheckBox.DataBindings.Clear();

            // Bindings for Credentials
            Auth_UserIDbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.UserID), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_PasswordbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.Password), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_ApiKeybeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.ApiKey), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_KeyTokenbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.KeyToken), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_CertificatePathbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.CertificatePath), true, DataSourceUpdateMode.OnPropertyChanged));

            // Bindings for Security Flags
            Auth_IntegratedSecuritybeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.IntegratedSecurity), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_PersistSecurityInfobeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.PersistSecurityInfo), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_TrustedConnectionbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.TrustedConnection), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_EncryptConnectionbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.EncryptConnection), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_MultiSubnetFailoverbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.MultiSubnetFailover), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_TrustServerCertificatebeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.TrustServerCertificate), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_AllowPublicKeyRetrievalbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.AllowPublicKeyRetrieval), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_UseSSLbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.UseSSL), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_RequireSSLbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.RequireSSL), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_BypassServerCertValidationbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.BypassServerCertificateValidation), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_UseWindowsAuthbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.UseWindowsAuthentication), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_UseOAuthbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.UseOAuth), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_UseApiKeybeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.UseApiKey), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_UseCertificatebeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.UseCertificate), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_UseUserAndPasswordbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.UseUserAndPassword), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_SavePasswordbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.SavePassword), true, DataSourceUpdateMode.OnPropertyChanged));
            Auth_ReadOnlybeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.ReadOnly), true, DataSourceUpdateMode.OnPropertyChanged));

            // Set password mode
            Auth_PasswordbeepTextBox.UseSystemPasswordChar = true;
        }
    }
}
