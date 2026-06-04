using TheTechIdea.Beep.Winform.Controls.Shapes;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            beepPanel1 = new BeepPanel();
            beepAdvancedButton1 = new TheTechIdea.Beep.Winform.Controls.Buttons.BeepAdvancedButton.BeepAdvancedButton();
            beepComboBox1 = new BeepComboBox();
            beepDatePicker1 = new BeepDatePicker();
            beepButton1 = new BeepButton();
            beepLabel1 = new BeepLabel();
            beepTextBox1 = new BeepTextBox();
            beepChevronButton1 = new TheTechIdea.Beep.Winform.Controls.Buttons.BeepChevronButton();
            beepCircularButton1 = new TheTechIdea.Beep.Winform.Controls.Buttons.BeepCircularButton();
            beepShape1 = new BeepShape();
            bottomBar1 = new TheTechIdea.Beep.Winform.Controls.BottomNavBars.BottomBar();
            beepCard1 = new BeepCard();
            beepCalendar1 = new TheTechIdea.Beep.Winform.Controls.Calendar.BeepCalendar();
            beepBlock1 = new TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.BeepBlock();
            beepDataConnection1 = new BeepDataConnection();
            SuspendLayout();
            // 
            // beepDataConnection1
            // 
            beepDataConnection1.CurrentConnection = null;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(915, 720);
            Controls.Add(beepBlock1);
            Controls.Add(beepCalendar1);
            Controls.Add(beepCard1);
            Controls.Add(bottomBar1);
            Controls.Add(beepCircularButton1);
            Controls.Add(beepChevronButton1);
            Controls.Add(beepTextBox1);
            Controls.Add(beepLabel1);
            Controls.Add(beepButton1);
            Controls.Add(beepPanel1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion

        private Controls.BeepPanel beepPanel1;
        private Controls.BeepComboBox beepComboBox1;
        private Controls.BeepButton beepButton1;
        private Controls.BeepTextBox beepTextBox1;
        private Controls.BeepLabel beepLabel1;
        private Controls.BeepDatePicker beepDatePicker1;
        private Controls.Buttons.BeepChevronButton beepChevronButton1;
        private Controls.Buttons.BeepCircularButton beepCircularButton1;
        private Controls.BottomNavBars.BottomBar bottomBar1;
        private BeepShape beepShape1;
        private Controls.Buttons.BeepAdvancedButton.BeepAdvancedButton beepAdvancedButton1;
        private BeepCard beepCard1;
        private Controls.Calendar.BeepCalendar beepCalendar1;
        private Controls.Integrated.Blocks.BeepBlock beepBlock1;
        private BeepDataConnection beepDataConnection1;
    }
}