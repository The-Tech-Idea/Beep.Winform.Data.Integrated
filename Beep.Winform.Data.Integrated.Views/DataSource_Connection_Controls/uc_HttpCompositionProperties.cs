using System;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;

namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    public partial class uc_HttpCompositionProperties : uc_DataConnectionPropertiesBaseControl
    {
        public uc_HttpCompositionProperties()
        {
            InitializeComponent();
        }

        public override void SetupBindings(ConnectionProperties conn)
        {
            base.SetupBindings(conn);
            Text = "Http Composition";
            if (conn == null)
            {
                return;
            }

            Http_BasePathbeepTextBox.DataBindings.Clear();
            Http_BasePathbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.Url), true, DataSourceUpdateMode.OnPropertyChanged));

            Http_AcceptHeaderbeepTextBox.DataBindings.Clear();
            Http_AcceptHeaderbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.ApiKeyHeader), true, DataSourceUpdateMode.OnPropertyChanged));

            Http_ContentTypebeepTextBox.DataBindings.Clear();
            Http_ContentTypebeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.AuthenticationType), true, DataSourceUpdateMode.OnPropertyChanged));

            Http_DefaultHeadersbeepTextBox.DataBindings.Clear();
            Http_DefaultQueryParamsbeepTextBox.DataBindings.Clear();
            Http_UserAgentbeepTextBox.DataBindings.Clear();
            Http_UseCompressionbeepCheckBox.DataBindings.Clear();
            Http_FollowRedirectsbeepCheckBox.DataBindings.Clear();

            Http_DefaultHeadersbeepTextBox.ReadOnly = true;
            Http_DefaultQueryParamsbeepTextBox.ReadOnly = true;
            Http_UserAgentbeepTextBox.ReadOnly = true;
            Http_UseCompressionbeepCheckBox.Enabled = false;
            Http_FollowRedirectsbeepCheckBox.Enabled = false;
            Http_DefaultHeadersbeepTextBox.Text = "Use Driver tab Parameters";
            Http_DefaultQueryParamsbeepTextBox.Text = "Use Driver tab Parameters";
            Http_UserAgentbeepTextBox.Text = "Use Driver tab Parameters";
        }
    }
}
