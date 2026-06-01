namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    partial class uc_CredentialsandConnectionStringProperties
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
            this.Credentials_propertiesPanel = new TheTechIdea.Beep.Winform.Controls.BeepPanel();
            this.Credentials_ParametersbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.Credentials_ConnectionStringbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.Credentials_PasswordbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.Credentials_UserIDbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.SuspendLayout();
            this.Credentials_propertiesPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ConnectionPropertytabPage
            // 
            this.Controls.Add(this.Credentials_propertiesPanel);
            // 
            // Credentials_propertiesPanel
            // 
            this.Credentials_propertiesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Credentials_propertiesPanel.Name = "Credentials_propertiesPanel";
            this.Credentials_propertiesPanel.TabIndex = 0;
            this.Credentials_propertiesPanel.Controls.Add(this.Credentials_ParametersbeepTextBox);
            this.Credentials_propertiesPanel.Controls.Add(this.Credentials_ConnectionStringbeepTextBox);
            this.Credentials_propertiesPanel.Controls.Add(this.Credentials_PasswordbeepTextBox);
            this.Credentials_propertiesPanel.Controls.Add(this.Credentials_UserIDbeepTextBox);
            // 
            // Credentials_UserIDbeepTextBox
            // 
            this.Credentials_UserIDbeepTextBox.Name = "Credentials_UserIDbeepTextBox";
            this.Credentials_UserIDbeepTextBox.PlaceholderText = "User ID";
            this.Credentials_UserIDbeepTextBox.Location = new System.Drawing.Point(24, 24);
            this.Credentials_UserIDbeepTextBox.Size = new System.Drawing.Size(240, 40);
            // 
            // Credentials_PasswordbeepTextBox
            // 
            this.Credentials_PasswordbeepTextBox.Name = "Credentials_PasswordbeepTextBox";
            this.Credentials_PasswordbeepTextBox.PlaceholderText = "Password";
            this.Credentials_PasswordbeepTextBox.UseSystemPasswordChar = true;
            this.Credentials_PasswordbeepTextBox.Location = new System.Drawing.Point(280, 24);
            this.Credentials_PasswordbeepTextBox.Size = new System.Drawing.Size(240, 40);
            // 
            // Credentials_ConnectionStringbeepTextBox
            // 
            this.Credentials_ConnectionStringbeepTextBox.Name = "Credentials_ConnectionStringbeepTextBox";
            this.Credentials_ConnectionStringbeepTextBox.PlaceholderText = "Connection String";
            this.Credentials_ConnectionStringbeepTextBox.Multiline = true;
            this.Credentials_ConnectionStringbeepTextBox.Location = new System.Drawing.Point(24, 80);
            this.Credentials_ConnectionStringbeepTextBox.Size = new System.Drawing.Size(496, 120);
            // 
            // Credentials_ParametersbeepTextBox
            // 
            this.Credentials_ParametersbeepTextBox.Name = "Credentials_ParametersbeepTextBox";
            this.Credentials_ParametersbeepTextBox.PlaceholderText = "Parameters (serialized)";
            this.Credentials_ParametersbeepTextBox.Multiline = true;
            this.Credentials_ParametersbeepTextBox.Location = new System.Drawing.Point(24, 210);
            this.Credentials_ParametersbeepTextBox.Size = new System.Drawing.Size(496, 120);
            // 
            // uc_CredentialsandConnectionStringProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "uc_CredentialsandConnectionStringProperties";
            this.Size = new System.Drawing.Size(547, 669);
            this.ResumeLayout(false);
            this.Credentials_propertiesPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private TheTechIdea.Beep.Winform.Controls.BeepPanel Credentials_propertiesPanel;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox Credentials_UserIDbeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox Credentials_PasswordbeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox Credentials_ConnectionStringbeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox Credentials_ParametersbeepTextBox;
    }
}
