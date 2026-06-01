using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.GridX;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    partial class uc_DataEdit
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            toolbarPanel = new System.Windows.Forms.Panel();
            statusPanel = new System.Windows.Forms.Panel();
            btnNew = new BeepButton();
            btnEdit = new BeepButton();
            btnDelete = new BeepButton();
            btnSave = new BeepButton();
            btnCancel = new BeepButton();
            btnRefresh = new BeepButton();
            btnUndo = new BeepButton();
            btnRedo = new BeepButton();
            btnMap = new BeepButton();
            btnImport = new BeepButton();
            lblEntityName = new BeepLabel();
            lblState = new BeepLabel();
            beepGridPro1 = new BeepGridPro();
            toolbarPanel.SuspendLayout();
            statusPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)beepGridPro1).BeginInit();
            SuspendLayout();
            // 
            // toolbarPanel
            // 
            toolbarPanel.Controls.Add(btnImport);
            toolbarPanel.Controls.Add(btnMap);
            toolbarPanel.Controls.Add(btnRedo);
            toolbarPanel.Controls.Add(btnUndo);
            toolbarPanel.Controls.Add(btnRefresh);
            toolbarPanel.Controls.Add(btnCancel);
            toolbarPanel.Controls.Add(btnSave);
            toolbarPanel.Controls.Add(btnDelete);
            toolbarPanel.Controls.Add(btnEdit);
            toolbarPanel.Controls.Add(btnNew);
            toolbarPanel.Dock = DockStyle.Top;
            toolbarPanel.Location = new Point(0, 0);
            toolbarPanel.Name = "toolbarPanel";
            toolbarPanel.Size = new Size(918, 40);
            toolbarPanel.TabIndex = 0;
            // 
            // statusPanel
            // 
            statusPanel.Controls.Add(lblState);
            statusPanel.Controls.Add(lblEntityName);
            statusPanel.Dock = DockStyle.Top;
            statusPanel.Location = new Point(0, 40);
            statusPanel.Name = "statusPanel";
            statusPanel.Size = new Size(918, 36);
            statusPanel.TabIndex = 1;
            // 
            // btnNew
            // 
            btnNew.Location = new Point(6, 7);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(62, 27);
            btnNew.TabIndex = 0;
            btnNew.Text = "New";
            btnNew.Theme = "DefaultType";
            // 
            // btnEdit
            // 
            btnEdit.Location = new Point(72, 7);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(62, 27);
            btnEdit.TabIndex = 1;
            btnEdit.Text = "Edit";
            btnEdit.Theme = "DefaultType";
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(138, 7);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(62, 27);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "Delete";
            btnDelete.Theme = "DefaultType";
            // 
            // btnSave
            // 
            btnSave.Location = new Point(204, 7);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(62, 27);
            btnSave.TabIndex = 3;
            btnSave.Text = "Save";
            btnSave.Theme = "DefaultType";
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(270, 7);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(62, 27);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancel";
            btnCancel.Theme = "DefaultType";
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(336, 7);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(68, 27);
            btnRefresh.TabIndex = 5;
            btnRefresh.Text = "Refresh";
            btnRefresh.Theme = "DefaultType";
            // 
            // btnUndo
            // 
            btnUndo.Location = new Point(408, 7);
            btnUndo.Name = "btnUndo";
            btnUndo.Size = new Size(62, 27);
            btnUndo.TabIndex = 6;
            btnUndo.Text = "Undo";
            btnUndo.Theme = "DefaultType";
            // 
            // btnRedo
            // 
            btnRedo.Location = new Point(474, 7);
            btnRedo.Name = "btnRedo";
            btnRedo.Size = new Size(62, 27);
            btnRedo.TabIndex = 7;
            btnRedo.Text = "Redo";
            btnRedo.Theme = "DefaultType";
            // 
            // btnMap
            // 
            btnMap.Location = new Point(540, 7);
            btnMap.Name = "btnMap";
            btnMap.Size = new Size(78, 27);
            btnMap.TabIndex = 8;
            btnMap.Text = "Mapping";
            btnMap.Theme = "DefaultType";
            // 
            // btnImport
            // 
            btnImport.Location = new Point(622, 7);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(70, 27);
            btnImport.TabIndex = 9;
            btnImport.Text = "Import";
            btnImport.Theme = "DefaultType";
            // 
            // lblEntityName
            // 
            lblEntityName.AutoSize = false;
            lblEntityName.Location = new Point(6, 6);
            lblEntityName.Name = "lblEntityName";
            lblEntityName.Size = new Size(280, 23);
            lblEntityName.TabIndex = 0;
            lblEntityName.Text = "Entity: (none)";
            lblEntityName.TextAlign = ContentAlignment.MiddleLeft;
            lblEntityName.Theme = "DefaultType";
            // 
            // lblState
            // 
            lblState.AutoSize = false;
            lblState.Location = new Point(292, 6);
            lblState.Name = "lblState";
            lblState.Size = new Size(620, 23);
            lblState.TabIndex = 1;
            lblState.Text = "State: Clean | Mode: Browse | Ready";
            lblState.TextAlign = ContentAlignment.MiddleLeft;
            lblState.Theme = "DefaultType";
            // 
            // beepGridPro1
            // 
            beepGridPro1.Dock = DockStyle.Fill;
            beepGridPro1.Location = new Point(0, 76);
            beepGridPro1.Name = "beepGridPro1";
            beepGridPro1.Size = new Size(918, 631);
            beepGridPro1.TabIndex = 2;
            beepGridPro1.Theme = "DefaultType";
            // 
            // uc_DataEdit
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(beepGridPro1);
            Controls.Add(statusPanel);
            Controls.Add(toolbarPanel);
            Name = "uc_DataEdit";
            Size = new Size(918, 707);
            toolbarPanel.ResumeLayout(false);
            statusPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)beepGridPro1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel toolbarPanel;
        private System.Windows.Forms.Panel statusPanel;
        private BeepButton btnNew;
        private BeepButton btnEdit;
        private BeepButton btnDelete;
        private BeepButton btnSave;
        private BeepButton btnCancel;
        private BeepButton btnRefresh;
        private BeepButton btnUndo;
        private BeepButton btnRedo;
        private BeepButton btnMap;
        private BeepButton btnImport;
        private BeepLabel lblEntityName;
        private BeepLabel lblState;
        private BeepGridPro beepGridPro1;
    }
}
