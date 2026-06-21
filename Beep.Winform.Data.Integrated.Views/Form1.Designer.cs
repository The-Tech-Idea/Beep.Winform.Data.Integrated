namespace TheTechIdea.Beep.Winform.Default.Views
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            beepButton1 = new BeepButton();
            beepDatePicker1 = new BeepDatePicker();
            beepComboBox1 = new BeepComboBox();
            beepPanel1 = new BeepPanel();
            SuspendLayout();
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(beepPanel1);
            Controls.Add(beepComboBox1);
            Controls.Add(beepButton1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private BeepButton beepButton1;
        private BeepDatePicker beepDatePicker1;
        private BeepComboBox beepComboBox1;
        private BeepPanel beepPanel1;
    }
}