using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;

namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    public partial class uc_CredentialsandConnectionStringProperties : uc_DataConnectionPropertiesBaseControl
    {
        public uc_CredentialsandConnectionStringProperties()
        {
            InitializeComponent();
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
        }
    }
}
