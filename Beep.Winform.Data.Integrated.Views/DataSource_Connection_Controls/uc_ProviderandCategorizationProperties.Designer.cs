using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.ComboBoxes;

namespace TheTechIdea.Beep.Winform.Default.Views.DataSource_Connection_Controls
{
    partial class uc_ProviderandCategorizationProperties
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
            this.Provider_propertiesPanel = new TheTechIdea.Beep.Winform.Controls.BeepPanel();
            this.Provider_DriverVersionbeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.Provider_DriverNamebeepTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.Provider_DatabaseTypebeepComboBox = new TheTechIdea.Beep.Winform.Controls.BeepComboBox();
            this.Provider_CategorybeepComboBox = new TheTechIdea.Beep.Winform.Controls.BeepComboBox();
            this.SuspendLayout();
            this.Provider_propertiesPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ConnectionPropertytabPage
            // 
            this.Controls.Add(this.Provider_propertiesPanel);
            // 
            // Provider_propertiesPanel
            // 
            this.Provider_propertiesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Provider_propertiesPanel.Name = "Provider_propertiesPanel";
            this.Provider_propertiesPanel.TabIndex = 0;
            this.Provider_propertiesPanel.Controls.Add(this.Provider_DriverVersionbeepTextBox);
            this.Provider_propertiesPanel.Controls.Add(this.Provider_DriverNamebeepTextBox);
            this.Provider_propertiesPanel.Controls.Add(this.Provider_DatabaseTypebeepComboBox);
            this.Provider_propertiesPanel.Controls.Add(this.Provider_CategorybeepComboBox);
            // 
            // Provider_CategorybeepComboBox
            // 
            this.Provider_CategorybeepComboBox.Name = "Provider_CategorybeepComboBox";
            this.Provider_CategorybeepComboBox.PlaceholderText = "Datasource Category";
            this.Provider_CategorybeepComboBox.Location = new System.Drawing.Point(24, 24);
            this.Provider_CategorybeepComboBox.Size = new System.Drawing.Size(220, 48);
            // 
            // Provider_DatabaseTypebeepComboBox
            // 
            this.Provider_DatabaseTypebeepComboBox.Name = "Provider_DatabaseTypebeepComboBox";
            this.Provider_DatabaseTypebeepComboBox.PlaceholderText = "Data Source Type";
            this.Provider_DatabaseTypebeepComboBox.Location = new System.Drawing.Point(260, 24);
            this.Provider_DatabaseTypebeepComboBox.Size = new System.Drawing.Size(260, 48);
            // 
            // Provider_DriverNamebeepTextBox
            // 
            this.Provider_DriverNamebeepTextBox.Name = "Provider_DriverNamebeepTextBox";
            this.Provider_DriverNamebeepTextBox.PlaceholderText = "Driver Name";
            this.Provider_DriverNamebeepTextBox.Location = new System.Drawing.Point(24, 90);
            this.Provider_DriverNamebeepTextBox.Size = new System.Drawing.Size(496, 40);
            // 
            // Provider_DriverVersionbeepTextBox
            // 
            this.Provider_DriverVersionbeepTextBox.Name = "Provider_DriverVersionbeepTextBox";
            this.Provider_DriverVersionbeepTextBox.PlaceholderText = "Driver Version";
            this.Provider_DriverVersionbeepTextBox.Location = new System.Drawing.Point(24, 140);
            this.Provider_DriverVersionbeepTextBox.Size = new System.Drawing.Size(220, 40);
            // 
            // uc_ProviderandCategorizationProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "uc_ProviderandCategorizationProperties";
            this.Size = new System.Drawing.Size(547, 669);
            this.ResumeLayout(false);
            this.Provider_propertiesPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private TheTechIdea.Beep.Winform.Controls.BeepPanel Provider_propertiesPanel;
        private BeepComboBox Provider_CategorybeepComboBox;
        private BeepComboBox Provider_DatabaseTypebeepComboBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox Provider_DriverNamebeepTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox Provider_DriverVersionbeepTextBox;
    }
}
