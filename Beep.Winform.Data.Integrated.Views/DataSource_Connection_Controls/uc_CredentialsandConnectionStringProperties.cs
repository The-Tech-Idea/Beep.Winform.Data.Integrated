using System;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;

namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    public partial class uc_CredentialsandConnectionStringProperties : uc_DataConnectionPropertiesBaseControl
    {
        private bool _showPassword;

        public uc_CredentialsandConnectionStringProperties()
        {
            InitializeComponent();
        }

        public bool ShowPassword
        {
            get => _showPassword;
            set
            {
                _showPassword = value;
                if (Credentials_PasswordbeepTextBox != null)
                    Credentials_PasswordbeepTextBox.UseSystemPasswordChar = !value;
            }
        }

        public override void SetupBindings(ConnectionProperties conn)
        {
            base.SetupBindings(conn);
            Text = "Credentials";
            if (conn == null) return;

            Credentials_UserIDbeepTextBox.DataBindings.Clear();
            Credentials_PasswordbeepTextBox.DataBindings.Clear();
            Credentials_ConnectionStringbeepTextBox.DataBindings.Clear();
            Credentials_ParametersbeepTextBox.DataBindings.Clear();

            Credentials_UserIDbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.UserID), true, DataSourceUpdateMode.OnPropertyChanged));
            Credentials_PasswordbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.Password), true, DataSourceUpdateMode.OnPropertyChanged));
            Credentials_ConnectionStringbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.ConnectionString), true, DataSourceUpdateMode.OnPropertyChanged));
            Credentials_ParametersbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.Parameters), true, DataSourceUpdateMode.OnPropertyChanged));
            Credentials_PasswordbeepTextBox.UseSystemPasswordChar = true;
        }

        /// <summary>Build connection string from current properties (matches WPF BuildConnectionString).</summary>
        public string BuildConnectionString(ConnectionProperties conn)
        {
            return conn.DatabaseType switch
            {
                DataSourceType.SqlServer => $"Server={conn.Host}{(conn.Port != 1433 && conn.Port > 0 ? $",{conn.Port}" : "")};Database={conn.Database};{(conn.IntegratedSecurity ? "Integrated Security=SSPI" : conn.UseWindowsAuthentication ? "Trusted_Connection=True" : $"User Id={conn.UserID};Password={conn.Password}")}",
                DataSourceType.Mysql => $"Server={conn.Host};Port={conn.Port};Database={conn.Database};Uid={conn.UserID};Pwd={conn.Password};",
                DataSourceType.Postgre => $"Host={conn.Host};Port={conn.Port};Database={conn.Database};Username={conn.UserID};Password={conn.Password};",
                DataSourceType.Oracle => $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={conn.Host})(PORT={conn.Port}))(CONNECT_DATA=(SERVICE_NAME={conn.Database})));User Id={conn.UserID};Password={conn.Password};",
                DataSourceType.SqlLite => $"Data Source={conn.Database};",
                DataSourceType.MongoDB => $"mongodb://{(string.IsNullOrEmpty(conn.UserID) ? "" : $"{conn.UserID}:{conn.Password}@")}{conn.Host}:{conn.Port}/{conn.Database}",
                _ => $"Server={conn.Host};Database={conn.Database};User Id={conn.UserID};Password={conn.Password};",
            };
        }
    }
}
