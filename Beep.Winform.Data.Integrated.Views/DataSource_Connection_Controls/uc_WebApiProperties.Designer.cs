namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    partial class uc_WebApiProperties
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
            base.InitializeComponent();
            this.WebApi_propertiesPanel = new TheTechIdea.Beep.Winform.Controls.BeepPanel();
            this.WebApi_HttpMethodbeepComboBox = new TheTechIdea.Beep.Winform.Controls.BeepComboBox();
            this.WebApi_TimeoutMsbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.WebApi_MaxRetriesbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.WebApi_RetryIntervalMsbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.WebApi_IgnoreSSLErrorsbeepCheckBox = new TheTechIdea.Beep.Winform.Controls.CheckBoxes.BeepCheckBoxBool();
            this.WebApi_ValidateServerCertbeepCheckBox = new TheTechIdea.Beep.Winform.Controls.CheckBoxes.BeepCheckBoxBool();
            this.WebApi_RequiresAuthbeepCheckBox = new TheTechIdea.Beep.Winform.Controls.CheckBoxes.BeepCheckBoxBool();
            this.WebApi_RequiresTokenRefreshbeepCheckBox = new TheTechIdea.Beep.Winform.Controls.CheckBoxes.BeepCheckBoxBool();
            this.WebApi_propertiesPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // WebApi_propertiesPanel
            // 
            this.WebApi_propertiesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WebApi_propertiesPanel.Name = "WebApi_propertiesPanel";
            this.WebApi_propertiesPanel.TabIndex = 0;
            this.WebApi_propertiesPanel.IsChild = true;
            this.WebApi_propertiesPanel.Controls.Add(this.WebApi_HttpMethodbeepComboBox);
            this.WebApi_propertiesPanel.Controls.Add(this.WebApi_TimeoutMsbeepTextBox);
            this.WebApi_propertiesPanel.Controls.Add(this.WebApi_MaxRetriesbeepTextBox);
            this.WebApi_propertiesPanel.Controls.Add(this.WebApi_RetryIntervalMsbeepTextBox);
            this.WebApi_propertiesPanel.Controls.Add(this.WebApi_IgnoreSSLErrorsbeepCheckBox);
            this.WebApi_propertiesPanel.Controls.Add(this.WebApi_ValidateServerCertbeepCheckBox);
            this.WebApi_propertiesPanel.Controls.Add(this.WebApi_RequiresAuthbeepCheckBox);
            this.WebApi_propertiesPanel.Controls.Add(this.WebApi_RequiresTokenRefreshbeepCheckBox);
            // 
            // WebApi_HttpMethodbeepComboBox
            // 
            this.WebApi_HttpMethodbeepComboBox.Name = "WebApi_HttpMethodbeepComboBox";
            this.WebApi_HttpMethodbeepComboBox.LabelText = "HTTP Method";
            this.WebApi_HttpMethodbeepComboBox.LabelTextOn = true;
            this.WebApi_HttpMethodbeepComboBox.Location = new System.Drawing.Point(20, 20);
            this.WebApi_HttpMethodbeepComboBox.Size = new System.Drawing.Size(150, 50);
            this.WebApi_HttpMethodbeepComboBox.IsChild = true;
            // 
            // WebApi_TimeoutMsbeepTextBox
            // 
            this.WebApi_TimeoutMsbeepTextBox.Name = "WebApi_TimeoutMsbeepTextBox";
            this.WebApi_TimeoutMsbeepTextBox.LabelText = "Timeout (ms)";
            this.WebApi_TimeoutMsbeepTextBox.LabelTextOn = true;
            this.WebApi_TimeoutMsbeepTextBox.Location = new System.Drawing.Point(190, 20);
            this.WebApi_TimeoutMsbeepTextBox.Size = new System.Drawing.Size(120, 50);
            this.WebApi_TimeoutMsbeepTextBox.IsChild = true;
            // 
            // WebApi_MaxRetriesbeepTextBox
            // 
            this.WebApi_MaxRetriesbeepTextBox.Name = "WebApi_MaxRetriesbeepTextBox";
            this.WebApi_MaxRetriesbeepTextBox.LabelText = "Max Retries";
            this.WebApi_MaxRetriesbeepTextBox.LabelTextOn = true;
            this.WebApi_MaxRetriesbeepTextBox.Location = new System.Drawing.Point(330, 20);
            this.WebApi_MaxRetriesbeepTextBox.Size = new System.Drawing.Size(80, 50);
            this.WebApi_MaxRetriesbeepTextBox.IsChild = true;
            // 
            // WebApi_RetryIntervalMsbeepTextBox
            // 
            this.WebApi_RetryIntervalMsbeepTextBox.Name = "WebApi_RetryIntervalMsbeepTextBox";
            this.WebApi_RetryIntervalMsbeepTextBox.LabelText = "Retry Interval (ms)";
            this.WebApi_RetryIntervalMsbeepTextBox.LabelTextOn = true;
            this.WebApi_RetryIntervalMsbeepTextBox.Location = new System.Drawing.Point(420, 20);
            this.WebApi_RetryIntervalMsbeepTextBox.Size = new System.Drawing.Size(100, 50);
            this.WebApi_RetryIntervalMsbeepTextBox.IsChild = true;
            // 
            // WebApi_IgnoreSSLErrorsbeepCheckBox
            // 
            this.WebApi_IgnoreSSLErrorsbeepCheckBox.Name = "WebApi_IgnoreSSLErrorsbeepCheckBox";
            this.WebApi_IgnoreSSLErrorsbeepCheckBox.Text = "Ignore SSL Errors";
            this.WebApi_IgnoreSSLErrorsbeepCheckBox.Location = new System.Drawing.Point(20, 90);
            this.WebApi_IgnoreSSLErrorsbeepCheckBox.Size = new System.Drawing.Size(150, 30);
            this.WebApi_IgnoreSSLErrorsbeepCheckBox.IsChild = true;
            // 
            // WebApi_ValidateServerCertbeepCheckBox
            // 
            this.WebApi_ValidateServerCertbeepCheckBox.Name = "WebApi_ValidateServerCertbeepCheckBox";
            this.WebApi_ValidateServerCertbeepCheckBox.Text = "Validate Server Cert";
            this.WebApi_ValidateServerCertbeepCheckBox.Location = new System.Drawing.Point(180, 90);
            this.WebApi_ValidateServerCertbeepCheckBox.Size = new System.Drawing.Size(160, 30);
            this.WebApi_ValidateServerCertbeepCheckBox.IsChild = true;
            // 
            // WebApi_RequiresAuthbeepCheckBox
            // 
            this.WebApi_RequiresAuthbeepCheckBox.Name = "WebApi_RequiresAuthbeepCheckBox";
            this.WebApi_RequiresAuthbeepCheckBox.Text = "Requires Auth";
            this.WebApi_RequiresAuthbeepCheckBox.Location = new System.Drawing.Point(350, 90);
            this.WebApi_RequiresAuthbeepCheckBox.Size = new System.Drawing.Size(150, 30);
            this.WebApi_RequiresAuthbeepCheckBox.IsChild = true;
            // 
            // WebApi_RequiresTokenRefreshbeepCheckBox
            // 
            this.WebApi_RequiresTokenRefreshbeepCheckBox.Name = "WebApi_RequiresTokenRefreshbeepCheckBox";
            this.WebApi_RequiresTokenRefreshbeepCheckBox.Text = "Token Refresh";
            this.WebApi_RequiresTokenRefreshbeepCheckBox.Location = new System.Drawing.Point(20, 130);
            this.WebApi_RequiresTokenRefreshbeepCheckBox.Size = new System.Drawing.Size(150, 30);
            this.WebApi_RequiresTokenRefreshbeepCheckBox.IsChild = true;
            // 
            // uc_WebApiProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "uc_WebApiProperties";
            this.Size = new System.Drawing.Size(550, 220);
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            // Add the panel to this UserControl
            this.Controls.Add(this.WebApi_propertiesPanel);
            this.Text = "Web API";
            this.WebApi_propertiesPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private TheTechIdea.Beep.Winform.Controls.BeepPanel WebApi_propertiesPanel;
        private TheTechIdea.Beep.Winform.Controls.BeepComboBox WebApi_HttpMethodbeepComboBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox WebApi_TimeoutMsbeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox WebApi_MaxRetriesbeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox WebApi_RetryIntervalMsbeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.CheckBoxes.BeepCheckBoxBool WebApi_IgnoreSSLErrorsbeepCheckBox;
        private TheTechIdea.Beep.Winform.Controls.CheckBoxes.BeepCheckBoxBool WebApi_ValidateServerCertbeepCheckBox;
        private TheTechIdea.Beep.Winform.Controls.CheckBoxes.BeepCheckBoxBool WebApi_RequiresAuthbeepCheckBox;
        private TheTechIdea.Beep.Winform.Controls.CheckBoxes.BeepCheckBoxBool WebApi_RequiresTokenRefreshbeepCheckBox;
    }
}
