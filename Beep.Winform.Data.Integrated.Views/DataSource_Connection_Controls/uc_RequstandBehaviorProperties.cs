using System;
using System.Windows.Forms;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;

namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    public partial class uc_RequstandBehaviorProperties : uc_DataConnectionPropertiesBaseControl
    {
        public uc_RequstandBehaviorProperties()
        {
            InitializeComponent();
        }

        public override void SetupBindings(ConnectionProperties conn)
        {
            base.SetupBindings(conn);
            Text = "Request and Behavior";
            if (conn == null)
            {
                return;
            }

            Req_TimeoutbeepTextBox.DataBindings.Clear();
            Req_MaxRetriesbeepTextBox.DataBindings.Clear();
            Req_RetryIntervalbeepTextBox.DataBindings.Clear();
            Req_ConnectionTimeoutbeepTextBox.DataBindings.Clear();
            Req_CommandTimeoutbeepTextBox.DataBindings.Clear();
            Req_MinPoolSizebeepTextBox.DataBindings.Clear();
            Req_MaxPoolSizebeepTextBox.DataBindings.Clear();
            Req_PoolingbeepCheckBox.DataBindings.Clear();
            Req_KeepAlivebeepCheckBox.DataBindings.Clear();

            Req_TimeoutbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.TimeoutMs), true, DataSourceUpdateMode.OnPropertyChanged));
            Req_MaxRetriesbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.MaxRetries), true, DataSourceUpdateMode.OnPropertyChanged));
            Req_RetryIntervalbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.RetryIntervalMs), true, DataSourceUpdateMode.OnPropertyChanged));
            Req_ConnectionTimeoutbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.Timeout), true, DataSourceUpdateMode.OnPropertyChanged));
            Req_CommandTimeoutbeepTextBox.DataBindings.Add(new Binding("Text", conn, nameof(conn.TimeoutMs), true, DataSourceUpdateMode.OnPropertyChanged));

            Req_MinPoolSizebeepTextBox.ReadOnly = true;
            Req_MaxPoolSizebeepTextBox.ReadOnly = true;
            Req_PoolingbeepCheckBox.Enabled = false;
            Req_KeepAlivebeepCheckBox.Enabled = false;
            Req_MinPoolSizebeepTextBox.Text = "Use Driver tab Parameters";
            Req_MaxPoolSizebeepTextBox.Text = "Use Driver tab Parameters";
        }
    }
}
