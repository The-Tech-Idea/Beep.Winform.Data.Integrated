namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    partial class uc_RequstandBehaviorProperties
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.Req_propertiesPanel = new TheTechIdea.Beep.Winform.Controls.BeepPanel();
            this.Req_TimeoutbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.Req_MaxRetriesbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.Req_RetryIntervalbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.Req_ConnectionTimeoutbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.Req_CommandTimeoutbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.Req_MinPoolSizebeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.Req_MaxPoolSizebeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.Req_PoolingbeepCheckBox = new TheTechIdea.Beep.Winform.Controls.CheckBoxes.BeepCheckBoxBool();
            this.Req_KeepAlivebeepCheckBox = new TheTechIdea.Beep.Winform.Controls.CheckBoxes.BeepCheckBoxBool();
            this.Req_propertiesPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // Req_propertiesPanel
            // 
            this.Req_propertiesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Req_propertiesPanel.Name = "Req_propertiesPanel";
            this.Req_propertiesPanel.TabIndex = 0;
            this.Req_propertiesPanel.IsChild = true;
            this.Req_propertiesPanel.AutoScroll = true;
            this.Req_propertiesPanel.Controls.Add(this.Req_TimeoutbeepTextBox);
            this.Req_propertiesPanel.Controls.Add(this.Req_MaxRetriesbeepTextBox);
            this.Req_propertiesPanel.Controls.Add(this.Req_RetryIntervalbeepTextBox);
            this.Req_propertiesPanel.Controls.Add(this.Req_ConnectionTimeoutbeepTextBox);
            this.Req_propertiesPanel.Controls.Add(this.Req_CommandTimeoutbeepTextBox);
            this.Req_propertiesPanel.Controls.Add(this.Req_MinPoolSizebeepTextBox);
            this.Req_propertiesPanel.Controls.Add(this.Req_MaxPoolSizebeepTextBox);
            this.Req_propertiesPanel.Controls.Add(this.Req_PoolingbeepCheckBox);
            this.Req_propertiesPanel.Controls.Add(this.Req_KeepAlivebeepCheckBox);
            // 
            // Req_TimeoutbeepTextBox
            // 
            this.Req_TimeoutbeepTextBox.Name = "Req_TimeoutbeepTextBox";
            this.Req_TimeoutbeepTextBox.LabelText = "Timeout (ms)";
            this.Req_TimeoutbeepTextBox.LabelTextOn = true;
            this.Req_TimeoutbeepTextBox.Location = new System.Drawing.Point(20, 20);
            this.Req_TimeoutbeepTextBox.Size = new System.Drawing.Size(480, 50);
            this.Req_TimeoutbeepTextBox.IsChild = true;
            // 
            // Req_MaxRetriesbeepTextBox
            // 
            this.Req_MaxRetriesbeepTextBox.Name = "Req_MaxRetriesbeepTextBox";
            this.Req_MaxRetriesbeepTextBox.LabelText = "Max Retries";
            this.Req_MaxRetriesbeepTextBox.LabelTextOn = true;
            this.Req_MaxRetriesbeepTextBox.Location = new System.Drawing.Point(20, 80);
            this.Req_MaxRetriesbeepTextBox.Size = new System.Drawing.Size(480, 50);
            this.Req_MaxRetriesbeepTextBox.IsChild = true;
            // 
            // Req_RetryIntervalbeepTextBox
            // 
            this.Req_RetryIntervalbeepTextBox.Name = "Req_RetryIntervalbeepTextBox";
            this.Req_RetryIntervalbeepTextBox.LabelText = "Retry Interval (ms)";
            this.Req_RetryIntervalbeepTextBox.LabelTextOn = true;
            this.Req_RetryIntervalbeepTextBox.Location = new System.Drawing.Point(20, 140);
            this.Req_RetryIntervalbeepTextBox.Size = new System.Drawing.Size(480, 50);
            this.Req_RetryIntervalbeepTextBox.IsChild = true;
            // 
            // Req_ConnectionTimeoutbeepTextBox
            // 
            this.Req_ConnectionTimeoutbeepTextBox.Name = "Req_ConnectionTimeoutbeepTextBox";
            this.Req_ConnectionTimeoutbeepTextBox.LabelText = "Connection Timeout (sec)";
            this.Req_ConnectionTimeoutbeepTextBox.LabelTextOn = true;
            this.Req_ConnectionTimeoutbeepTextBox.Location = new System.Drawing.Point(20, 200);
            this.Req_ConnectionTimeoutbeepTextBox.Size = new System.Drawing.Size(480, 50);
            this.Req_ConnectionTimeoutbeepTextBox.IsChild = true;
            // 
            // Req_CommandTimeoutbeepTextBox
            // 
            this.Req_CommandTimeoutbeepTextBox.Name = "Req_CommandTimeoutbeepTextBox";
            this.Req_CommandTimeoutbeepTextBox.LabelText = "Command Timeout (sec)";
            this.Req_CommandTimeoutbeepTextBox.LabelTextOn = true;
            this.Req_CommandTimeoutbeepTextBox.Location = new System.Drawing.Point(20, 260);
            this.Req_CommandTimeoutbeepTextBox.Size = new System.Drawing.Size(480, 50);
            this.Req_CommandTimeoutbeepTextBox.IsChild = true;
            // 
            // Req_MinPoolSizebeepTextBox
            // 
            this.Req_MinPoolSizebeepTextBox.Name = "Req_MinPoolSizebeepTextBox";
            this.Req_MinPoolSizebeepTextBox.LabelText = "Min Pool Size";
            this.Req_MinPoolSizebeepTextBox.LabelTextOn = true;
            this.Req_MinPoolSizebeepTextBox.Location = new System.Drawing.Point(20, 320);
            this.Req_MinPoolSizebeepTextBox.Size = new System.Drawing.Size(480, 50);
            this.Req_MinPoolSizebeepTextBox.IsChild = true;
            // 
            // Req_MaxPoolSizebeepTextBox
            // 
            this.Req_MaxPoolSizebeepTextBox.Name = "Req_MaxPoolSizebeepTextBox";
            this.Req_MaxPoolSizebeepTextBox.LabelText = "Max Pool Size";
            this.Req_MaxPoolSizebeepTextBox.LabelTextOn = true;
            this.Req_MaxPoolSizebeepTextBox.Location = new System.Drawing.Point(20, 380);
            this.Req_MaxPoolSizebeepTextBox.Size = new System.Drawing.Size(480, 50);
            this.Req_MaxPoolSizebeepTextBox.IsChild = true;
            // 
            // Req_PoolingbeepCheckBox
            // 
            this.Req_PoolingbeepCheckBox.Name = "Req_PoolingbeepCheckBox";
            this.Req_PoolingbeepCheckBox.Text = "Pooling";
            this.Req_PoolingbeepCheckBox.LabelText = "Pooling";
            this.Req_PoolingbeepCheckBox.LabelTextOn = true;
            this.Req_PoolingbeepCheckBox.Location = new System.Drawing.Point(20, 440);
            this.Req_PoolingbeepCheckBox.Size = new System.Drawing.Size(200, 40);
            this.Req_PoolingbeepCheckBox.IsChild = true;
            // 
            // Req_KeepAlivebeepCheckBox
            // 
            this.Req_KeepAlivebeepCheckBox.Name = "Req_KeepAlivebeepCheckBox";
            this.Req_KeepAlivebeepCheckBox.Text = "Keepalive";
            this.Req_KeepAlivebeepCheckBox.LabelText = "Keepalive";
            this.Req_KeepAlivebeepCheckBox.LabelTextOn = true;
            this.Req_KeepAlivebeepCheckBox.Location = new System.Drawing.Point(20, 490);
            this.Req_KeepAlivebeepCheckBox.Size = new System.Drawing.Size(200, 40);
            this.Req_KeepAlivebeepCheckBox.IsChild = true;
            // 
            // uc_RequstandBehaviorProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "uc_RequstandBehaviorProperties";
            this.Size = new System.Drawing.Size(550, 600);
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Controls.Add(this.Req_propertiesPanel);
            this.Text = "Request and Behavior";
            this.Req_propertiesPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private TheTechIdea.Beep.Winform.Controls.BeepPanel Req_propertiesPanel;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox Req_TimeoutbeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox Req_MaxRetriesbeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox Req_RetryIntervalbeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox Req_ConnectionTimeoutbeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox Req_CommandTimeoutbeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox Req_MinPoolSizebeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox Req_MaxPoolSizebeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.CheckBoxes.BeepCheckBoxBool Req_PoolingbeepCheckBox;
        private TheTechIdea.Beep.Winform.Controls.CheckBoxes.BeepCheckBoxBool Req_KeepAlivebeepCheckBox;
    }
}
