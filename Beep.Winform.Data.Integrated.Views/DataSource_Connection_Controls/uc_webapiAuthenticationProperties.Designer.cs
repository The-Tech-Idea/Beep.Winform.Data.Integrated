namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    partial class uc_webapiAuthenticationProperties
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
            this.OAuth_propertiesPanel = new TheTechIdea.Beep.Winform.Controls.BeepPanel();
            this.OAuth_ClientIdbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.OAuth_ClientSecretbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.OAuth_AuthTypebeepComboBox = new TheTechIdea.Beep.Winform.Controls.BeepComboBox();
            this.OAuth_AuthUrlbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.OAuth_TokenUrlbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.OAuth_ScopebeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.OAuth_GrantTypebeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.OAuth_ApiKeyHeaderbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.OAuth_RedirectUribeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.OAuth_AuthCodebeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.OAuth_UseProxybeepCheckBox = new TheTechIdea.Beep.Winform.Controls.CheckBoxes.BeepCheckBoxBool();
            this.OAuth_ProxyUrlbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.OAuth_ProxyPortbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.OAuth_ProxyUserbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.OAuth_ProxyPasswordbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.OAuth_propertiesPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // OAuth_propertiesPanel
            // 
            this.OAuth_propertiesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OAuth_propertiesPanel.Name = "OAuth_propertiesPanel";
            this.OAuth_propertiesPanel.TabIndex = 0;
            this.OAuth_propertiesPanel.IsChild = true;
            this.OAuth_propertiesPanel.AutoScroll = true;
            this.OAuth_propertiesPanel.Controls.Add(this.OAuth_AuthTypebeepComboBox);
            this.OAuth_propertiesPanel.Controls.Add(this.OAuth_ClientIdbeepTextBox);
            this.OAuth_propertiesPanel.Controls.Add(this.OAuth_ClientSecretbeepTextBox);
            this.OAuth_propertiesPanel.Controls.Add(this.OAuth_AuthUrlbeepTextBox);
            this.OAuth_propertiesPanel.Controls.Add(this.OAuth_TokenUrlbeepTextBox);
            this.OAuth_propertiesPanel.Controls.Add(this.OAuth_ScopebeepTextBox);
            this.OAuth_propertiesPanel.Controls.Add(this.OAuth_GrantTypebeepTextBox);
            this.OAuth_propertiesPanel.Controls.Add(this.OAuth_ApiKeyHeaderbeepTextBox);
            this.OAuth_propertiesPanel.Controls.Add(this.OAuth_RedirectUribeepTextBox);
            this.OAuth_propertiesPanel.Controls.Add(this.OAuth_AuthCodebeepTextBox);
            this.OAuth_propertiesPanel.Controls.Add(this.OAuth_UseProxybeepCheckBox);
            this.OAuth_propertiesPanel.Controls.Add(this.OAuth_ProxyUrlbeepTextBox);
            this.OAuth_propertiesPanel.Controls.Add(this.OAuth_ProxyPortbeepTextBox);
            this.OAuth_propertiesPanel.Controls.Add(this.OAuth_ProxyUserbeepTextBox);
            this.OAuth_propertiesPanel.Controls.Add(this.OAuth_ProxyPasswordbeepTextBox);
            // 
            // OAuth_AuthTypebeepComboBox - Row 1
            // 
            this.OAuth_AuthTypebeepComboBox.Name = "OAuth_AuthTypebeepComboBox";
            this.OAuth_AuthTypebeepComboBox.LabelText = "Auth Type";
            this.OAuth_AuthTypebeepComboBox.LabelTextOn = true;
            this.OAuth_AuthTypebeepComboBox.Location = new System.Drawing.Point(20, 20);
            this.OAuth_AuthTypebeepComboBox.Size = new System.Drawing.Size(200, 50);
            this.OAuth_AuthTypebeepComboBox.IsChild = true;
            // 
            // OAuth_ClientIdbeepTextBox
            // 
            this.OAuth_ClientIdbeepTextBox.Name = "OAuth_ClientIdbeepTextBox";
            this.OAuth_ClientIdbeepTextBox.LabelText = "Client ID";
            this.OAuth_ClientIdbeepTextBox.LabelTextOn = true;
            this.OAuth_ClientIdbeepTextBox.Location = new System.Drawing.Point(240, 20);
            this.OAuth_ClientIdbeepTextBox.Size = new System.Drawing.Size(270, 50);
            this.OAuth_ClientIdbeepTextBox.IsChild = true;
            // 
            // OAuth_ClientSecretbeepTextBox - Row 2
            // 
            this.OAuth_ClientSecretbeepTextBox.Name = "OAuth_ClientSecretbeepTextBox";
            this.OAuth_ClientSecretbeepTextBox.LabelText = "Client Secret";
            this.OAuth_ClientSecretbeepTextBox.LabelTextOn = true;
            this.OAuth_ClientSecretbeepTextBox.Location = new System.Drawing.Point(20, 90);
            this.OAuth_ClientSecretbeepTextBox.Size = new System.Drawing.Size(490, 50);
            this.OAuth_ClientSecretbeepTextBox.UseSystemPasswordChar = true;
            this.OAuth_ClientSecretbeepTextBox.IsChild = true;
            // 
            // OAuth_AuthUrlbeepTextBox - Row 3
            // 
            this.OAuth_AuthUrlbeepTextBox.Name = "OAuth_AuthUrlbeepTextBox";
            this.OAuth_AuthUrlbeepTextBox.LabelText = "Auth URL";
            this.OAuth_AuthUrlbeepTextBox.LabelTextOn = true;
            this.OAuth_AuthUrlbeepTextBox.Location = new System.Drawing.Point(20, 160);
            this.OAuth_AuthUrlbeepTextBox.Size = new System.Drawing.Size(490, 50);
            this.OAuth_AuthUrlbeepTextBox.IsChild = true;
            // 
            // OAuth_TokenUrlbeepTextBox - Row 4
            // 
            this.OAuth_TokenUrlbeepTextBox.Name = "OAuth_TokenUrlbeepTextBox";
            this.OAuth_TokenUrlbeepTextBox.LabelText = "Token URL";
            this.OAuth_TokenUrlbeepTextBox.LabelTextOn = true;
            this.OAuth_TokenUrlbeepTextBox.Location = new System.Drawing.Point(20, 230);
            this.OAuth_TokenUrlbeepTextBox.Size = new System.Drawing.Size(490, 50);
            this.OAuth_TokenUrlbeepTextBox.IsChild = true;
            // 
            // OAuth_ScopebeepTextBox - Row 5
            // 
            this.OAuth_ScopebeepTextBox.Name = "OAuth_ScopebeepTextBox";
            this.OAuth_ScopebeepTextBox.LabelText = "Scope";
            this.OAuth_ScopebeepTextBox.LabelTextOn = true;
            this.OAuth_ScopebeepTextBox.Location = new System.Drawing.Point(20, 300);
            this.OAuth_ScopebeepTextBox.Size = new System.Drawing.Size(300, 50);
            this.OAuth_ScopebeepTextBox.IsChild = true;
            // 
            // OAuth_GrantTypebeepTextBox
            // 
            this.OAuth_GrantTypebeepTextBox.Name = "OAuth_GrantTypebeepTextBox";
            this.OAuth_GrantTypebeepTextBox.LabelText = "Grant Type";
            this.OAuth_GrantTypebeepTextBox.LabelTextOn = true;
            this.OAuth_GrantTypebeepTextBox.Location = new System.Drawing.Point(340, 300);
            this.OAuth_GrantTypebeepTextBox.Size = new System.Drawing.Size(170, 50);
            this.OAuth_GrantTypebeepTextBox.IsChild = true;
            // 
            // OAuth_ApiKeyHeaderbeepTextBox - Row 6
            // 
            this.OAuth_ApiKeyHeaderbeepTextBox.Name = "OAuth_ApiKeyHeaderbeepTextBox";
            this.OAuth_ApiKeyHeaderbeepTextBox.LabelText = "API Key Header";
            this.OAuth_ApiKeyHeaderbeepTextBox.LabelTextOn = true;
            this.OAuth_ApiKeyHeaderbeepTextBox.Location = new System.Drawing.Point(20, 370);
            this.OAuth_ApiKeyHeaderbeepTextBox.Size = new System.Drawing.Size(230, 50);
            this.OAuth_ApiKeyHeaderbeepTextBox.IsChild = true;
            // 
            // OAuth_RedirectUribeepTextBox
            // 
            this.OAuth_RedirectUribeepTextBox.Name = "OAuth_RedirectUribeepTextBox";
            this.OAuth_RedirectUribeepTextBox.LabelText = "Redirect URI";
            this.OAuth_RedirectUribeepTextBox.LabelTextOn = true;
            this.OAuth_RedirectUribeepTextBox.Location = new System.Drawing.Point(270, 370);
            this.OAuth_RedirectUribeepTextBox.Size = new System.Drawing.Size(240, 50);
            this.OAuth_RedirectUribeepTextBox.IsChild = true;
            // 
            // OAuth_AuthCodebeepTextBox - Row 7
            // 
            this.OAuth_AuthCodebeepTextBox.Name = "OAuth_AuthCodebeepTextBox";
            this.OAuth_AuthCodebeepTextBox.LabelText = "Auth Code";
            this.OAuth_AuthCodebeepTextBox.LabelTextOn = true;
            this.OAuth_AuthCodebeepTextBox.Location = new System.Drawing.Point(20, 440);
            this.OAuth_AuthCodebeepTextBox.Size = new System.Drawing.Size(490, 50);
            this.OAuth_AuthCodebeepTextBox.IsChild = true;
            // 
            // OAuth_UseProxybeepCheckBox - Row 8 (Proxy Section)
            // 
            this.OAuth_UseProxybeepCheckBox.Name = "OAuth_UseProxybeepCheckBox";
            this.OAuth_UseProxybeepCheckBox.Text = "Use Proxy";
            this.OAuth_UseProxybeepCheckBox.Location = new System.Drawing.Point(20, 510);
            this.OAuth_UseProxybeepCheckBox.Size = new System.Drawing.Size(150, 30);
            this.OAuth_UseProxybeepCheckBox.IsChild = true;
            // 
            // OAuth_ProxyUrlbeepTextBox - Row 9
            // 
            this.OAuth_ProxyUrlbeepTextBox.Name = "OAuth_ProxyUrlbeepTextBox";
            this.OAuth_ProxyUrlbeepTextBox.LabelText = "Proxy URL";
            this.OAuth_ProxyUrlbeepTextBox.LabelTextOn = true;
            this.OAuth_ProxyUrlbeepTextBox.Location = new System.Drawing.Point(20, 550);
            this.OAuth_ProxyUrlbeepTextBox.Size = new System.Drawing.Size(350, 50);
            this.OAuth_ProxyUrlbeepTextBox.IsChild = true;
            // 
            // OAuth_ProxyPortbeepTextBox
            // 
            this.OAuth_ProxyPortbeepTextBox.Name = "OAuth_ProxyPortbeepTextBox";
            this.OAuth_ProxyPortbeepTextBox.LabelText = "Port";
            this.OAuth_ProxyPortbeepTextBox.LabelTextOn = true;
            this.OAuth_ProxyPortbeepTextBox.Location = new System.Drawing.Point(390, 550);
            this.OAuth_ProxyPortbeepTextBox.Size = new System.Drawing.Size(120, 50);
            this.OAuth_ProxyPortbeepTextBox.IsChild = true;
            // 
            // OAuth_ProxyUserbeepTextBox - Row 10
            // 
            this.OAuth_ProxyUserbeepTextBox.Name = "OAuth_ProxyUserbeepTextBox";
            this.OAuth_ProxyUserbeepTextBox.LabelText = "Proxy User";
            this.OAuth_ProxyUserbeepTextBox.LabelTextOn = true;
            this.OAuth_ProxyUserbeepTextBox.Location = new System.Drawing.Point(20, 620);
            this.OAuth_ProxyUserbeepTextBox.Size = new System.Drawing.Size(230, 50);
            this.OAuth_ProxyUserbeepTextBox.IsChild = true;
            // 
            // OAuth_ProxyPasswordbeepTextBox
            // 
            this.OAuth_ProxyPasswordbeepTextBox.Name = "OAuth_ProxyPasswordbeepTextBox";
            this.OAuth_ProxyPasswordbeepTextBox.LabelText = "Proxy Password";
            this.OAuth_ProxyPasswordbeepTextBox.LabelTextOn = true;
            this.OAuth_ProxyPasswordbeepTextBox.Location = new System.Drawing.Point(270, 620);
            this.OAuth_ProxyPasswordbeepTextBox.Size = new System.Drawing.Size(240, 50);
            this.OAuth_ProxyPasswordbeepTextBox.UseSystemPasswordChar = true;
            this.OAuth_ProxyPasswordbeepTextBox.IsChild = true;
            // 
            // uc_webapiAuthenticationProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "uc_webapiAuthenticationProperties";
            this.Size = new System.Drawing.Size(550, 720);
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            // Add the panel to this UserControl
            this.Controls.Add(this.OAuth_propertiesPanel);
            this.Text = "OAuth/API Auth";
            this.OAuth_propertiesPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private TheTechIdea.Beep.Winform.Controls.BeepPanel OAuth_propertiesPanel;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox OAuth_ClientIdbeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox OAuth_ClientSecretbeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepComboBox OAuth_AuthTypebeepComboBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox OAuth_AuthUrlbeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox OAuth_TokenUrlbeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox OAuth_ScopebeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox OAuth_GrantTypebeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox OAuth_ApiKeyHeaderbeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox OAuth_RedirectUribeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox OAuth_AuthCodebeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.CheckBoxes.BeepCheckBoxBool OAuth_UseProxybeepCheckBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox OAuth_ProxyUrlbeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox OAuth_ProxyPortbeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox OAuth_ProxyUserbeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox OAuth_ProxyPasswordbeepTextBox;
    }
}
