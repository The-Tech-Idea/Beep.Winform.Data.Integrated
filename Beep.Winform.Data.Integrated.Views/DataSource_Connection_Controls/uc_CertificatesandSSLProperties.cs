using System;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;

namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    public partial class uc_CertificatesandSSLProperties : uc_DataConnectionPropertiesBaseControl
    {
        public uc_CertificatesandSSLProperties()
        {
            InitializeComponent();
        }

        public override void SetupBindings(ConnectionProperties conn)
        {
            base.SetupBindings(conn);
            Text = "Certificate and SSL";
            if (conn == null)
            {
                return;
            }

            Cert_CertificatePathbeepTextBox.DataBindings.Clear();
            Cert_CertificatePathbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.CertificatePath), true, DataSourceUpdateMode.OnPropertyChanged));

            Cert_UseSslbeepCheckBox.DataBindings.Clear();
            Cert_UseSslbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.UseSSL), true, DataSourceUpdateMode.OnPropertyChanged));

            Cert_RequireSslbeepCheckBox.DataBindings.Clear();
            Cert_RequireSslbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.RequireSSL), true, DataSourceUpdateMode.OnPropertyChanged));

            Cert_TrustServerCertificatebeepCheckBox.DataBindings.Clear();
            Cert_TrustServerCertificatebeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.TrustServerCertificate), true, DataSourceUpdateMode.OnPropertyChanged));

            Cert_BypassServerValidationbeepCheckBox.DataBindings.Clear();
            Cert_BypassServerValidationbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.BypassServerCertificateValidation), true, DataSourceUpdateMode.OnPropertyChanged));

            Cert_SslModebeepTextBox.DataBindings.Clear();
            Cert_SslModebeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.SSLMode), true, DataSourceUpdateMode.OnPropertyChanged));

            Cert_SslCertificatebeepTextBox.DataBindings.Clear();
            Cert_SslCertificatebeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.ClientCertificatePath), true, DataSourceUpdateMode.OnPropertyChanged));

            Cert_SslKeybeepTextBox.DataBindings.Clear();
            Cert_SslKeybeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.ClientCertificatePassword), true, DataSourceUpdateMode.OnPropertyChanged));

            Cert_SslRootCertificatebeepTextBox.DataBindings.Clear();
            Cert_SslRootCertificatebeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.ClientCertificateThumbprint), true, DataSourceUpdateMode.OnPropertyChanged));

            Cert_SslCrlbeepTextBox.DataBindings.Clear();
            Cert_SslCrlbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.ClientCertificateSubjectName), true, DataSourceUpdateMode.OnPropertyChanged));

            Cert_ValidateCertificateChainbeepCheckBox.DataBindings.Clear();
            Cert_ValidateCertificateChainbeepCheckBox.DataBindings.Add(new Binding("CurrentValue", conn, nameof(conn.ValidateServerCertificate), true, DataSourceUpdateMode.OnPropertyChanged));
        }
    }
}
