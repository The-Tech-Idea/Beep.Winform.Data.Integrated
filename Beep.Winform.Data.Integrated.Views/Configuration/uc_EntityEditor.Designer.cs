namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    partial class uc_EntityEditor
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) { components.Dispose(); }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            _comboRow = new System.Windows.Forms.FlowLayoutPanel();
            DatasourcebeepComboBox = new TheTechIdea.Beep.Winform.Controls.BeepComboBox();
            EntitiesbeepComboBox    = new TheTechIdea.Beep.Winform.Controls.BeepComboBox();
            ApplybeepButton        = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            _btnEditData           = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            _btnDefaults           = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            _btnMapEntity          = new TheTechIdea.Beep.Winform.Controls.BeepButton();
            EntityFieldsbeepGridPro = new TheTechIdea.Beep.Winform.Controls.GridX.BeepGridPro();
            _titleLabel            = new TheTechIdea.Beep.Winform.Controls.BeepLabel();
            _stateLabel            = new TheTechIdea.Beep.Winform.Controls.BeepLabel();
            fieldsBindingSource    = new System.Windows.Forms.BindingSource(components);
            entityManagerViewModelBindingSource = new System.Windows.Forms.BindingSource(components);
            _comboRow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)EntityFieldsbeepGridPro).BeginInit();
            ((System.ComponentModel.ISupportInitialize)fieldsBindingSource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)entityManagerViewModelBindingSource).BeginInit();
            SuspendLayout();

            // ── _comboRow — toolbar strip below title ──────────────────────
            _comboRow.Dock = System.Windows.Forms.DockStyle.Top;
            _comboRow.AutoSize = true;
            _comboRow.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            _comboRow.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            _comboRow.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);

            // ── DatasourcebeepComboBox ──────────────────────────────────────
            DatasourcebeepComboBox.Size = new System.Drawing.Size(260, 34);
            DatasourcebeepComboBox.PlaceholderText = "Select datasource";
            DatasourcebeepComboBox.LabelText = "Datasource";
            DatasourcebeepComboBox.LabelTextOn = true;
            DatasourcebeepComboBox.ShowSearchInDropdown = true;

            // ── EntitiesbeepComboBox ────────────────────────────────────────
            EntitiesbeepComboBox.Size = new System.Drawing.Size(260, 34);
            EntitiesbeepComboBox.PlaceholderText = "Select or type entity name";
            EntitiesbeepComboBox.LabelText = "Entity";
            EntitiesbeepComboBox.LabelTextOn = true;
            EntitiesbeepComboBox.ShowSearchInDropdown = true;

            // ── ApplybeepButton — primary CTA ───────────────────────────────
            ApplybeepButton.Text = "Create Entity";
            ApplybeepButton.Size = new System.Drawing.Size(130, 36);
            ApplybeepButton.ToolTipText = "Create or update the entity schema.";

            // ── _btnEditData ────────────────────────────────────────────────
            _btnEditData.Text = "Edit Data";
            _btnEditData.Size = new System.Drawing.Size(100, 32);
            _btnEditData.ToolTipText = "Open the Data Edit grid to CRUD entity rows.";
            _btnEditData.Visible = false;

            // ── _btnDefaults ────────────────────────────────────────────────
            _btnDefaults.Text = "Defaults";
            _btnDefaults.Size = new System.Drawing.Size(100, 32);
            _btnDefaults.ToolTipText = "Open the Defaults editor for this entity's fields.";
            _btnDefaults.Visible = false;

            // ── _btnMapEntity ───────────────────────────────────────────────
            _btnMapEntity.Text = "Map Entity";
            _btnMapEntity.Size = new System.Drawing.Size(110, 32);
            _btnMapEntity.ToolTipText = "Create a field mapping for this entity.";
            _btnMapEntity.Visible = false;

            // ── _titleLabel — page title ────────────────────────────────────
            _titleLabel.Dock = System.Windows.Forms.DockStyle.Top;
            _titleLabel.AutoSize = true;
            _titleLabel.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            _titleLabel.Padding = new System.Windows.Forms.Padding(12, 10, 12, 4);
            _titleLabel.Text = "Entity Editor";

            // ── _stateLabel — status bar ────────────────────────────────────
            _stateLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            _stateLabel.AutoSize = true;
            _stateLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            _stateLabel.Padding = new System.Windows.Forms.Padding(12, 4, 12, 4);
            _stateLabel.Text = "Select datasource to begin.";

            // ── EntityFieldsbeepGridPro — field structure grid ──────────────
            EntityFieldsbeepGridPro.Dock = System.Windows.Forms.DockStyle.Fill;
            EntityFieldsbeepGridPro.ReadOnly = false;

            // ── Populate _comboRow ──────────────────────────────────────────
            _comboRow.Controls.Add(DatasourcebeepComboBox);
            _comboRow.Controls.Add(EntitiesbeepComboBox);
            _comboRow.Controls.Add(ApplybeepButton);
            _comboRow.Controls.Add(_btnEditData);
            _comboRow.Controls.Add(_btnDefaults);
            _comboRow.Controls.Add(_btnMapEntity);

            // ── uc_EntityEditor ─────────────────────────────────────────────
            Controls.Add(_titleLabel);
            Controls.Add(_comboRow);
            Controls.Add(EntityFieldsbeepGridPro);
            Controls.Add(_stateLabel);
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Name = "uc_EntityEditor";
            Size = new System.Drawing.Size(840, 560);
            _comboRow.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)EntityFieldsbeepGridPro).EndInit();
            ((System.ComponentModel.ISupportInitialize)fieldsBindingSource).EndInit();
            ((System.ComponentModel.ISupportInitialize)entityManagerViewModelBindingSource).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel _comboRow;
        private TheTechIdea.Beep.Winform.Controls.BeepComboBox DatasourcebeepComboBox;
        private TheTechIdea.Beep.Winform.Controls.BeepComboBox EntitiesbeepComboBox;
        private TheTechIdea.Beep.Winform.Controls.BeepButton ApplybeepButton;
        private TheTechIdea.Beep.Winform.Controls.BeepButton _btnEditData;
        private TheTechIdea.Beep.Winform.Controls.BeepButton _btnDefaults;
        private TheTechIdea.Beep.Winform.Controls.BeepButton _btnMapEntity;
        private TheTechIdea.Beep.Winform.Controls.GridX.BeepGridPro EntityFieldsbeepGridPro;
        private TheTechIdea.Beep.Winform.Controls.BeepLabel _titleLabel;
        private TheTechIdea.Beep.Winform.Controls.BeepLabel _stateLabel;
        private System.Windows.Forms.BindingSource fieldsBindingSource;
        private System.Windows.Forms.BindingSource entityManagerViewModelBindingSource;
    }
}