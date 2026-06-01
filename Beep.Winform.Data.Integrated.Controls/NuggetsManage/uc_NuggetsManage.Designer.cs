namespace TheTechIdea.Beep.Winform.Controls.Integrated.NuggetsManage
{
    partial class uc_NuggetsManage
    {
        private System.ComponentModel.IContainer components = null;

        private TheTechIdea.Beep.Winform.Controls.BeepLabel _statusLabel;
        private TheTechIdea.Beep.Winform.Controls.CheckBoxes.BeepCheckBoxBool _enableAtStartupCheckBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox _scanPathTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepButton _browseScanPathButton;
        private TheTechIdea.Beep.Winform.Controls.BeepButton _scanButton;
        private System.Windows.Forms.Panel scanBar;
        private TheTechIdea.Beep.Winform.Controls.BeepButton _installButton;
        private TheTechIdea.Beep.Winform.Controls.BeepButton _loadButton;
        private TheTechIdea.Beep.Winform.Controls.BeepButton _unloadButton;
        private TheTechIdea.Beep.Winform.Controls.BeepButton _refreshButton;
        private TheTechIdea.Beep.Winform.Controls.BeepButton _copyLogsButton;
        private TheTechIdea.Beep.Winform.Controls.BeepButton _clearLogsButton;
        private System.Windows.Forms.Panel commandBar;
        private TheTechIdea.Beep.Winform.Controls.BeepListBox _nuggetList;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox _detailsTextBox;
        private TheTechIdea.Beep.Winform.Controls.BeepTextBox _operationLogTextBox;
        private System.Windows.Forms.SplitContainer rightSplit;
        private System.Windows.Forms.SplitContainer mainSplit;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._statusLabel = new TheTechIdea.Beep.Winform.Controls.BeepLabel();
            this._enableAtStartupCheckBox = new TheTechIdea.Beep.Winform.Controls.CheckBoxes.BeepCheckBoxBool();
            this._scanPathTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this._browseScanPathButton = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            this._scanButton = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            this.scanBar = new System.Windows.Forms.Panel();
            this._installButton = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            this._loadButton = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            this._unloadButton = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            this._refreshButton = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            this._copyLogsButton = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            this._clearLogsButton = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            this.commandBar = new System.Windows.Forms.Panel();
            this._nuggetList = new TheTechIdea.Beep.Winform.Controls.BeepListBox();
            this._detailsTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this._operationLogTextBox = new TheTechIdea.Beep.Winform.Controls.BeepTextBox();
            this.rightSplit = new System.Windows.Forms.SplitContainer();
            this.mainSplit = new System.Windows.Forms.SplitContainer();
            this.scanBar.SuspendLayout();
            this.commandBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rightSplit)).BeginInit();
            this.rightSplit.Panel1.SuspendLayout();
            this.rightSplit.Panel2.SuspendLayout();
            this.rightSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainSplit)).BeginInit();
            this.mainSplit.Panel1.SuspendLayout();
            this.mainSplit.Panel2.SuspendLayout();
            this.mainSplit.SuspendLayout();
            this.SuspendLayout();
            // 
            // _statusLabel
            // 
            this._statusLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this._statusLabel.Height = 28;
            this._statusLabel.Text = "Nuggets manager ready.";
            this._statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _enableAtStartupCheckBox
            // 
            this._enableAtStartupCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
            this._enableAtStartupCheckBox.Height = 28;
            this._enableAtStartupCheckBox.Text = "Enable at startup";
            this._enableAtStartupCheckBox.CurrentValue = false;
            // 
            // _scanPathTextBox
            // 
            this._scanPathTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._scanPathTextBox.PlaceholderText = "Optional scan root path...";
            // 
            // _browseScanPathButton
            // 
            this._browseScanPathButton.Dock = System.Windows.Forms.DockStyle.Right;
            this._browseScanPathButton.Width = 36;
            this._browseScanPathButton.Text = "...";
            // 
            // _scanButton
            // 
            this._scanButton.Dock = System.Windows.Forms.DockStyle.Right;
            this._scanButton.Width = 90;
            this._scanButton.Text = "Scan";
            // 
            // scanBar
            // 
            this.scanBar.Controls.Add(this._scanPathTextBox);
            this.scanBar.Controls.Add(this._scanButton);
            this.scanBar.Controls.Add(this._browseScanPathButton);
            this.scanBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.scanBar.Height = 36;
            this.scanBar.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            // 
            // _installButton
            // 
            this._installButton.Text = "Install";
            this._installButton.Dock = System.Windows.Forms.DockStyle.Left;
            this._installButton.Width = 100;
            // 
            // _loadButton
            // 
            this._loadButton.Text = "Load";
            this._loadButton.Dock = System.Windows.Forms.DockStyle.Left;
            this._loadButton.Width = 100;
            // 
            // _unloadButton
            // 
            this._unloadButton.Text = "Unload";
            this._unloadButton.Dock = System.Windows.Forms.DockStyle.Left;
            this._unloadButton.Width = 100;
            // 
            // _refreshButton
            // 
            this._refreshButton.Text = "Refresh";
            this._refreshButton.Dock = System.Windows.Forms.DockStyle.Left;
            this._refreshButton.Width = 100;
            // 
            // _copyLogsButton
            // 
            this._copyLogsButton.Text = "Copy Logs";
            this._copyLogsButton.Dock = System.Windows.Forms.DockStyle.Right;
            this._copyLogsButton.Width = 100;
            // 
            // _clearLogsButton
            // 
            this._clearLogsButton.Text = "Clear Logs";
            this._clearLogsButton.Dock = System.Windows.Forms.DockStyle.Right;
            this._clearLogsButton.Width = 100;
            // 
            // commandBar
            // 
            this.commandBar.Controls.Add(this._clearLogsButton);
            this.commandBar.Controls.Add(this._copyLogsButton);
            this.commandBar.Controls.Add(this._refreshButton);
            this.commandBar.Controls.Add(this._unloadButton);
            this.commandBar.Controls.Add(this._loadButton);
            this.commandBar.Controls.Add(this._installButton);
            this.commandBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.commandBar.Height = 36;
            this.commandBar.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            // 
            // _nuggetList
            // 
            this._nuggetList.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // _detailsTextBox
            // 
            this._detailsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._detailsTextBox.Multiline = true;
            this._detailsTextBox.ReadOnly = true;
            this._detailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            // 
            // _operationLogTextBox
            // 
            this._operationLogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._operationLogTextBox.Multiline = true;
            this._operationLogTextBox.ReadOnly = true;
            this._operationLogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            // 
            // rightSplit
            // 
            this.rightSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.rightSplit.SplitterDistance = 170;
            this.rightSplit.Panel1.Controls.Add(this._enableAtStartupCheckBox);
            this.rightSplit.Panel1.Controls.Add(this._detailsTextBox);
            this.rightSplit.Panel2.Controls.Add(this._operationLogTextBox);
            // 
            // mainSplit
            // 
            this.mainSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplit.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.mainSplit.SplitterDistance = 320;
            this.mainSplit.Panel1.Controls.Add(this._nuggetList);
            this.mainSplit.Panel2.Controls.Add(this.rightSplit);
            // 
            // uc_NuggetsManage
            // 
            this.Controls.Add(this.mainSplit);
            this.Controls.Add(this.commandBar);
            this.Controls.Add(this.scanBar);
            this.Controls.Add(this._statusLabel);
            this.Name = "uc_NuggetsManage";
            this.Size = new System.Drawing.Size(800, 600);
            this.scanBar.ResumeLayout(false);
            this.commandBar.ResumeLayout(false);
            this.rightSplit.Panel1.ResumeLayout(false);
            this.rightSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.rightSplit)).EndInit();
            this.rightSplit.ResumeLayout(false);
            this.mainSplit.Panel1.ResumeLayout(false);
            this.mainSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplit)).EndInit();
            this.mainSplit.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
