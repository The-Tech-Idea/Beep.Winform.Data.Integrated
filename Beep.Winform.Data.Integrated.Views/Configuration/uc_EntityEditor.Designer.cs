using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.GridX;

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

        // Layout is TableLayoutPanel throughout — one root table (toolbar / identity / tabs / status)
        // with nested tables for the button strips, so columns line up and spacing is uniform.
        // Every RowStyle/ColumnStyle is written out one call at a time: a loop here makes the VS
        // designer refuse to parse InitializeComponent.
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            _rootTable = new System.Windows.Forms.TableLayoutPanel();
            _toolbarTable = new System.Windows.Forms.TableLayoutPanel();
            DatasourcebeepComboBox = new BeepComboBox();
            EntitiesbeepComboBox = new BeepComboBox();
            _btnNew = new BeepButton();
            ApplybeepButton = new BeepButton();
            _btnRename = new BeepButton();
            _btnTruncate = new BeepButton();
            _btnDrop = new BeepButton();
            _btnRefresh = new BeepButton();
            _btnPlan = new BeepButton();
            _btnEditData = new BeepButton();
            _btnDefaults = new BeepButton();
            _btnMapEntity = new BeepButton();

            _identityTable = new System.Windows.Forms.TableLayoutPanel();
            _lblEntityName = new BeepLabel();
            _txtEntityName = new BeepTextBox();
            _lblMode = new BeepLabel();

            _tabs = new System.Windows.Forms.TabControl();
            _tabColumns = new System.Windows.Forms.TabPage();
            _columnsTable = new System.Windows.Forms.TableLayoutPanel();
            EntityFieldsbeepGridPro = new BeepGridPro();
            _columnButtonsTable = new System.Windows.Forms.TableLayoutPanel();
            _btnAddColumn = new BeepButton();
            _btnDeleteColumn = new BeepButton();
            _btnMoveUp = new BeepButton();
            _btnMoveDown = new BeepButton();

            _tabKeys = new System.Windows.Forms.TabPage();
            _keysTable = new System.Windows.Forms.TableLayoutPanel();
            _lstKeys = new BeepListBox();
            _indexButtonsTable = new System.Windows.Forms.TableLayoutPanel();
            _btnCreateIndex = new BeepButton();
            _btnDropIndex = new BeepButton();
            _fkTable = new System.Windows.Forms.TableLayoutPanel();
            _lblFkReferences = new BeepLabel();
            _cboFkEntity = new BeepComboBox();
            _cboFkColumn = new BeepComboBox();
            _cboFkOnDelete = new BeepComboBox();
            _btnAddFk = new BeepButton();
            _btnDropFk = new BeepButton();

            _tabPlan = new System.Windows.Forms.TabPage();
            _planTable = new System.Windows.Forms.TableLayoutPanel();
            _txtPlan = new System.Windows.Forms.TextBox();
            _planButtonsTable = new System.Windows.Forms.TableLayoutPanel();
            _btnCopyPlan = new BeepButton();

            _stateLabel = new BeepLabel();
            fieldsBindingSource = new System.Windows.Forms.BindingSource(components);
            entityManagerViewModelBindingSource = new System.Windows.Forms.BindingSource(components);

            _rootTable.SuspendLayout();
            _toolbarTable.SuspendLayout();
            _identityTable.SuspendLayout();
            _tabs.SuspendLayout();
            _tabColumns.SuspendLayout();
            _columnsTable.SuspendLayout();
            _columnButtonsTable.SuspendLayout();
            _tabKeys.SuspendLayout();
            _keysTable.SuspendLayout();
            _indexButtonsTable.SuspendLayout();
            _fkTable.SuspendLayout();
            _tabPlan.SuspendLayout();
            _planTable.SuspendLayout();
            _planButtonsTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)EntityFieldsbeepGridPro).BeginInit();
            ((System.ComponentModel.ISupportInitialize)fieldsBindingSource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)entityManagerViewModelBindingSource).BeginInit();
            SuspendLayout();

            // ── root: toolbar / identity / tabs / status ──────────────────────
            _rootTable.ColumnCount = 1;
            _rootTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _rootTable.Dock = System.Windows.Forms.DockStyle.Fill;
            _rootTable.Name = "_rootTable";
            _rootTable.Padding = new System.Windows.Forms.Padding(8);
            _rootTable.RowCount = 4;
            _rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            _rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            _rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));

            // ── toolbar row ───────────────────────────────────────────────────
            _toolbarTable.ColumnCount = 13;
            _toolbarTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 230F));
            _toolbarTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 230F));
            _toolbarTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            _toolbarTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            _toolbarTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            _toolbarTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            _toolbarTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            _toolbarTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            _toolbarTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            _toolbarTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            _toolbarTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            _toolbarTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            _toolbarTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _toolbarTable.Dock = System.Windows.Forms.DockStyle.Fill;
            _toolbarTable.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            _toolbarTable.Name = "_toolbarTable";
            _toolbarTable.RowCount = 1;
            _toolbarTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            DatasourcebeepComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            DatasourcebeepComboBox.LabelText = "Datasource";
            DatasourcebeepComboBox.LabelTextOn = true;
            DatasourcebeepComboBox.Margin = new System.Windows.Forms.Padding(0, 2, 6, 2);
            DatasourcebeepComboBox.Name = "DatasourcebeepComboBox";
            DatasourcebeepComboBox.PlaceholderText = "Select datasource";
            DatasourcebeepComboBox.ShowSearchInDropdown = true;
            DatasourcebeepComboBox.UseThemeColors = true;

            EntitiesbeepComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            EntitiesbeepComboBox.LabelText = "Open entity";
            EntitiesbeepComboBox.LabelTextOn = true;
            EntitiesbeepComboBox.Margin = new System.Windows.Forms.Padding(0, 2, 12, 2);
            EntitiesbeepComboBox.Name = "EntitiesbeepComboBox";
            EntitiesbeepComboBox.PlaceholderText = "Open an existing entity";
            EntitiesbeepComboBox.ShowSearchInDropdown = true;
            EntitiesbeepComboBox.UseThemeColors = true;

            _btnNew.Dock = System.Windows.Forms.DockStyle.Fill;
            _btnNew.Margin = new System.Windows.Forms.Padding(0, 4, 4, 4);
            _btnNew.Name = "_btnNew";
            _btnNew.Text = "New";
            _btnNew.ToolTipText = "Start a new entity definition.";
            _btnNew.UseThemeColors = true;

            ApplybeepButton.Dock = System.Windows.Forms.DockStyle.Fill;
            ApplybeepButton.Margin = new System.Windows.Forms.Padding(0, 4, 4, 4);
            ApplybeepButton.Name = "ApplybeepButton";
            ApplybeepButton.Text = "Create Entity";
            ApplybeepButton.ToolTipText = "Create or update the entity schema.";
            ApplybeepButton.UseThemeColors = true;

            // Rename / Truncate / Drop are only shown when the datasource's ISchemaMigrationProvider
            // declares support for them — a CSV or Web API provider hides what it cannot do.
            _btnRename.Dock = System.Windows.Forms.DockStyle.Fill;
            _btnRename.Margin = new System.Windows.Forms.Padding(0, 4, 4, 4);
            _btnRename.Name = "_btnRename";
            _btnRename.Text = "Rename";
            _btnRename.ToolTipText = "Rename this entity through the datasource's migration provider.";
            _btnRename.UseThemeColors = true;
            _btnRename.Visible = false;

            _btnTruncate.Dock = System.Windows.Forms.DockStyle.Fill;
            _btnTruncate.Margin = new System.Windows.Forms.Padding(0, 4, 4, 4);
            _btnTruncate.Name = "_btnTruncate";
            _btnTruncate.Text = "Truncate";
            _btnTruncate.ToolTipText = "Delete every row, keeping the entity definition.";
            _btnTruncate.UseThemeColors = true;
            _btnTruncate.Visible = false;

            _btnDrop.Dock = System.Windows.Forms.DockStyle.Fill;
            _btnDrop.Margin = new System.Windows.Forms.Padding(0, 4, 4, 4);
            _btnDrop.Name = "_btnDrop";
            _btnDrop.Text = "Drop";
            _btnDrop.ToolTipText = "Drop this entity from the datasource.";
            _btnDrop.UseThemeColors = true;
            _btnDrop.Visible = false;

            _btnRefresh.Dock = System.Windows.Forms.DockStyle.Fill;
            _btnRefresh.Margin = new System.Windows.Forms.Padding(0, 4, 4, 4);
            _btnRefresh.Name = "_btnRefresh";
            _btnRefresh.Text = "Refresh";
            _btnRefresh.ToolTipText = "Re-read the structure from the datasource, discarding edits.";
            _btnRefresh.UseThemeColors = true;

            _btnPlan.Dock = System.Windows.Forms.DockStyle.Fill;
            _btnPlan.Margin = new System.Windows.Forms.Padding(0, 4, 12, 4);
            _btnPlan.Name = "_btnPlan";
            _btnPlan.Text = "Plan";
            _btnPlan.ToolTipText = "Show the schema operations the migration provider will run.";
            _btnPlan.UseThemeColors = true;

            _btnEditData.Dock = System.Windows.Forms.DockStyle.Fill;
            _btnEditData.Margin = new System.Windows.Forms.Padding(0, 4, 4, 4);
            _btnEditData.Name = "_btnEditData";
            _btnEditData.Text = "Edit Data";
            _btnEditData.ToolTipText = "Open the Data Edit grid to CRUD entity rows.";
            _btnEditData.UseThemeColors = true;
            _btnEditData.Visible = false;

            _btnDefaults.Dock = System.Windows.Forms.DockStyle.Fill;
            _btnDefaults.Margin = new System.Windows.Forms.Padding(0, 4, 4, 4);
            _btnDefaults.Name = "_btnDefaults";
            _btnDefaults.Text = "Defaults";
            _btnDefaults.ToolTipText = "Open the Defaults editor for this entity's fields.";
            _btnDefaults.UseThemeColors = true;
            _btnDefaults.Visible = false;

            _btnMapEntity.Dock = System.Windows.Forms.DockStyle.Fill;
            _btnMapEntity.Margin = new System.Windows.Forms.Padding(0, 4, 4, 4);
            _btnMapEntity.Name = "_btnMapEntity";
            _btnMapEntity.Text = "Map Entity";
            _btnMapEntity.ToolTipText = "Create a field mapping for this entity.";
            _btnMapEntity.UseThemeColors = true;
            _btnMapEntity.Visible = false;

            _toolbarTable.Controls.Add(DatasourcebeepComboBox, 0, 0);
            _toolbarTable.Controls.Add(EntitiesbeepComboBox, 1, 0);
            _toolbarTable.Controls.Add(_btnNew, 2, 0);
            _toolbarTable.Controls.Add(ApplybeepButton, 3, 0);
            _toolbarTable.Controls.Add(_btnRename, 4, 0);
            _toolbarTable.Controls.Add(_btnTruncate, 5, 0);
            _toolbarTable.Controls.Add(_btnDrop, 6, 0);
            _toolbarTable.Controls.Add(_btnRefresh, 7, 0);
            _toolbarTable.Controls.Add(_btnPlan, 8, 0);
            _toolbarTable.Controls.Add(_btnEditData, 9, 0);
            _toolbarTable.Controls.Add(_btnDefaults, 10, 0);
            _toolbarTable.Controls.Add(_btnMapEntity, 11, 0);

            // ── identity row: the entity name is edited here, not in the combo ─
            _identityTable.ColumnCount = 3;
            _identityTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            _identityTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _identityTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 260F));
            _identityTable.Dock = System.Windows.Forms.DockStyle.Fill;
            _identityTable.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            _identityTable.Name = "_identityTable";
            _identityTable.RowCount = 1;
            _identityTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            _lblEntityName.Dock = System.Windows.Forms.DockStyle.Fill;
            _lblEntityName.IsFrameless = true;
            _lblEntityName.Name = "_lblEntityName";
            _lblEntityName.Text = "Entity name";
            _lblEntityName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblEntityName.UseThemeColors = true;

            _txtEntityName.Dock = System.Windows.Forms.DockStyle.Fill;
            _txtEntityName.Margin = new System.Windows.Forms.Padding(0, 4, 12, 4);
            _txtEntityName.Name = "_txtEntityName";
            _txtEntityName.PlaceholderText = "Type a name to create a new entity";
            _txtEntityName.UseThemeColors = true;

            _lblMode.AutoEllipsis = true;
            _lblMode.Dock = System.Windows.Forms.DockStyle.Fill;
            _lblMode.IsFrameless = true;
            _lblMode.Name = "_lblMode";
            _lblMode.Text = "";
            _lblMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            _lblMode.UseThemeColors = true;

            _identityTable.Controls.Add(_lblEntityName, 0, 0);
            _identityTable.Controls.Add(_txtEntityName, 1, 0);
            _identityTable.Controls.Add(_lblMode, 2, 0);

            // ── tabs: Columns / Keys & Indexes / DDL ──────────────────────────
            _tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            _tabs.Name = "_tabs";

            // Columns tab: grid over its own button strip.
            _tabColumns.Name = "_tabColumns";
            _tabColumns.Text = "Columns";

            _columnsTable.ColumnCount = 1;
            _columnsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _columnsTable.Dock = System.Windows.Forms.DockStyle.Fill;
            _columnsTable.Name = "_columnsTable";
            _columnsTable.RowCount = 2;
            _columnsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _columnsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));

            EntityFieldsbeepGridPro.Dock = System.Windows.Forms.DockStyle.Fill;
            EntityFieldsbeepGridPro.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            EntityFieldsbeepGridPro.Name = "EntityFieldsbeepGridPro";
            EntityFieldsbeepGridPro.ReadOnly = false;
            EntityFieldsbeepGridPro.ShowCheckBox = true;

            _columnButtonsTable.ColumnCount = 5;
            _columnButtonsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            _columnButtonsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            _columnButtonsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            _columnButtonsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            _columnButtonsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _columnButtonsTable.Dock = System.Windows.Forms.DockStyle.Fill;
            _columnButtonsTable.Name = "_columnButtonsTable";
            _columnButtonsTable.RowCount = 1;
            _columnButtonsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            _btnAddColumn.Dock = System.Windows.Forms.DockStyle.Fill;
            _btnAddColumn.Margin = new System.Windows.Forms.Padding(0, 2, 6, 2);
            _btnAddColumn.Name = "_btnAddColumn";
            _btnAddColumn.Text = "Add Column";
            _btnAddColumn.UseThemeColors = true;

            _btnDeleteColumn.Dock = System.Windows.Forms.DockStyle.Fill;
            _btnDeleteColumn.Margin = new System.Windows.Forms.Padding(0, 2, 12, 2);
            _btnDeleteColumn.Name = "_btnDeleteColumn";
            _btnDeleteColumn.Text = "Delete Column";
            _btnDeleteColumn.UseThemeColors = true;

            _btnMoveUp.Dock = System.Windows.Forms.DockStyle.Fill;
            _btnMoveUp.Margin = new System.Windows.Forms.Padding(0, 2, 6, 2);
            _btnMoveUp.Name = "_btnMoveUp";
            _btnMoveUp.Text = "Move Up";
            _btnMoveUp.UseThemeColors = true;

            _btnMoveDown.Dock = System.Windows.Forms.DockStyle.Fill;
            _btnMoveDown.Margin = new System.Windows.Forms.Padding(0, 2, 6, 2);
            _btnMoveDown.Name = "_btnMoveDown";
            _btnMoveDown.Text = "Move Down";
            _btnMoveDown.UseThemeColors = true;

            _columnButtonsTable.Controls.Add(_btnAddColumn, 0, 0);
            _columnButtonsTable.Controls.Add(_btnDeleteColumn, 1, 0);
            _columnButtonsTable.Controls.Add(_btnMoveUp, 2, 0);
            _columnButtonsTable.Controls.Add(_btnMoveDown, 3, 0);

            _columnsTable.Controls.Add(EntityFieldsbeepGridPro, 0, 0);
            _columnsTable.Controls.Add(_columnButtonsTable, 0, 1);
            _tabColumns.Controls.Add(_columnsTable);

            // Keys & indexes tab: current keys/indexes/foreign keys, an index button strip, and an
            // inline foreign-key editor. Every action routes through the datasource's own migration
            // provider, so the strips are hidden when that provider does not support the operation.
            _tabKeys.Name = "_tabKeys";
            _tabKeys.Text = "Keys && Indexes";

            _keysTable.ColumnCount = 1;
            _keysTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _keysTable.Dock = System.Windows.Forms.DockStyle.Fill;
            _keysTable.Name = "_keysTable";
            _keysTable.RowCount = 3;
            _keysTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _keysTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            _keysTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));

            _lstKeys.Dock = System.Windows.Forms.DockStyle.Fill;
            _lstKeys.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            _lstKeys.Name = "_lstKeys";
            _lstKeys.ShowSearch = false;
            _lstKeys.UseThemeColors = true;

            _indexButtonsTable.ColumnCount = 3;
            _indexButtonsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            _indexButtonsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            _indexButtonsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _indexButtonsTable.Dock = System.Windows.Forms.DockStyle.Fill;
            _indexButtonsTable.Name = "_indexButtonsTable";
            _indexButtonsTable.RowCount = 1;
            _indexButtonsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            _btnCreateIndex.Dock = System.Windows.Forms.DockStyle.Fill;
            _btnCreateIndex.Margin = new System.Windows.Forms.Padding(0, 2, 6, 2);
            _btnCreateIndex.Name = "_btnCreateIndex";
            _btnCreateIndex.Text = "Create Index";
            _btnCreateIndex.ToolTipText = "Index the column(s) selected on the Columns tab.";
            _btnCreateIndex.UseThemeColors = true;

            _btnDropIndex.Dock = System.Windows.Forms.DockStyle.Fill;
            _btnDropIndex.Margin = new System.Windows.Forms.Padding(0, 2, 6, 2);
            _btnDropIndex.Name = "_btnDropIndex";
            _btnDropIndex.Text = "Drop Index";
            _btnDropIndex.ToolTipText = "Drop the index selected in the list above.";
            _btnDropIndex.UseThemeColors = true;

            _indexButtonsTable.Controls.Add(_btnCreateIndex, 0, 0);
            _indexButtonsTable.Controls.Add(_btnDropIndex, 1, 0);

            _fkTable.ColumnCount = 7;
            _fkTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            _fkTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            _fkTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            _fkTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            _fkTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            _fkTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            _fkTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _fkTable.Dock = System.Windows.Forms.DockStyle.Fill;
            _fkTable.Name = "_fkTable";
            _fkTable.RowCount = 1;
            _fkTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            _lblFkReferences.Dock = System.Windows.Forms.DockStyle.Fill;
            _lblFkReferences.IsFrameless = true;
            _lblFkReferences.Name = "_lblFkReferences";
            _lblFkReferences.Text = "Foreign key →";
            _lblFkReferences.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblFkReferences.UseThemeColors = true;

            _cboFkEntity.Dock = System.Windows.Forms.DockStyle.Fill;
            _cboFkEntity.Margin = new System.Windows.Forms.Padding(0, 4, 6, 4);
            _cboFkEntity.Name = "_cboFkEntity";
            _cboFkEntity.PlaceholderText = "Referenced entity";
            _cboFkEntity.ShowSearchInDropdown = true;
            _cboFkEntity.UseThemeColors = true;

            _cboFkColumn.Dock = System.Windows.Forms.DockStyle.Fill;
            _cboFkColumn.Margin = new System.Windows.Forms.Padding(0, 4, 6, 4);
            _cboFkColumn.Name = "_cboFkColumn";
            _cboFkColumn.PlaceholderText = "Referenced column";
            _cboFkColumn.ShowSearchInDropdown = true;
            _cboFkColumn.UseThemeColors = true;

            _cboFkOnDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            _cboFkOnDelete.Margin = new System.Windows.Forms.Padding(0, 4, 12, 4);
            _cboFkOnDelete.Name = "_cboFkOnDelete";
            _cboFkOnDelete.PlaceholderText = "On delete";
            _cboFkOnDelete.UseThemeColors = true;

            _btnAddFk.Dock = System.Windows.Forms.DockStyle.Fill;
            _btnAddFk.Margin = new System.Windows.Forms.Padding(0, 2, 6, 2);
            _btnAddFk.Name = "_btnAddFk";
            _btnAddFk.Text = "Add Foreign Key";
            _btnAddFk.ToolTipText = "Reference the entity above from the column(s) selected on the Columns tab.";
            _btnAddFk.UseThemeColors = true;

            _btnDropFk.Dock = System.Windows.Forms.DockStyle.Fill;
            _btnDropFk.Margin = new System.Windows.Forms.Padding(0, 2, 6, 2);
            _btnDropFk.Name = "_btnDropFk";
            _btnDropFk.Text = "Drop Foreign Key";
            _btnDropFk.ToolTipText = "Drop the foreign key selected in the list above.";
            _btnDropFk.UseThemeColors = true;

            _fkTable.Controls.Add(_lblFkReferences, 0, 0);
            _fkTable.Controls.Add(_cboFkEntity, 1, 0);
            _fkTable.Controls.Add(_cboFkColumn, 2, 0);
            _fkTable.Controls.Add(_cboFkOnDelete, 3, 0);
            _fkTable.Controls.Add(_btnAddFk, 4, 0);
            _fkTable.Controls.Add(_btnDropFk, 5, 0);

            _keysTable.Controls.Add(_lstKeys, 0, 0);
            _keysTable.Controls.Add(_indexButtonsTable, 0, 1);
            _keysTable.Controls.Add(_fkTable, 0, 2);
            _tabKeys.Controls.Add(_keysTable);

            // Plan tab: what the datasource's own migration provider will do, and the DDL evidence it
            // recorded after the last apply. This view never composes DDL — each IDataSource has its
            // own ISchemaMigrationProvider and that provider owns its dialect.
            _tabPlan.Name = "_tabPlan";
            _tabPlan.Text = "Plan";

            _planTable.ColumnCount = 1;
            _planTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _planTable.Dock = System.Windows.Forms.DockStyle.Fill;
            _planTable.Name = "_planTable";
            _planTable.RowCount = 2;
            _planTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _planTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));

            _txtPlan.Dock = System.Windows.Forms.DockStyle.Fill;
            _txtPlan.Font = new System.Drawing.Font("Consolas", 9.75F);
            _txtPlan.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            _txtPlan.Multiline = true;
            _txtPlan.Name = "_txtPlan";
            _txtPlan.ReadOnly = true;
            _txtPlan.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            _txtPlan.WordWrap = false;

            _planButtonsTable.ColumnCount = 2;
            _planButtonsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            _planButtonsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _planButtonsTable.Dock = System.Windows.Forms.DockStyle.Fill;
            _planButtonsTable.Name = "_planButtonsTable";
            _planButtonsTable.RowCount = 1;
            _planButtonsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            _btnCopyPlan.Dock = System.Windows.Forms.DockStyle.Fill;
            _btnCopyPlan.Margin = new System.Windows.Forms.Padding(0, 2, 6, 2);
            _btnCopyPlan.Name = "_btnCopyPlan";
            _btnCopyPlan.Text = "Copy";
            _btnCopyPlan.UseThemeColors = true;
            _planButtonsTable.Controls.Add(_btnCopyPlan, 0, 0);

            _planTable.Controls.Add(_txtPlan, 0, 0);
            _planTable.Controls.Add(_planButtonsTable, 0, 1);
            _tabPlan.Controls.Add(_planTable);

            _tabs.TabPages.Add(_tabColumns);
            _tabs.TabPages.Add(_tabKeys);
            _tabs.TabPages.Add(_tabPlan);
            _tabs.SelectedIndex = 0;

            // ── status row ────────────────────────────────────────────────────
            _stateLabel.AutoEllipsis = true;
            _stateLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            _stateLabel.IsFrameless = true;
            _stateLabel.Name = "_stateLabel";
            _stateLabel.Text = "Select datasource to begin.";
            _stateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _stateLabel.UseThemeColors = true;

            _rootTable.Controls.Add(_toolbarTable, 0, 0);
            _rootTable.Controls.Add(_identityTable, 0, 1);
            _rootTable.Controls.Add(_tabs, 0, 2);
            _rootTable.Controls.Add(_stateLabel, 0, 3);

            // ── uc_EntityEditor ───────────────────────────────────────────────
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(_rootTable);
            Name = "uc_EntityEditor";
            Size = new System.Drawing.Size(1040, 640);

            ((System.ComponentModel.ISupportInitialize)entityManagerViewModelBindingSource).EndInit();
            ((System.ComponentModel.ISupportInitialize)fieldsBindingSource).EndInit();
            ((System.ComponentModel.ISupportInitialize)EntityFieldsbeepGridPro).EndInit();
            _planButtonsTable.ResumeLayout(false);
            _planTable.ResumeLayout(false);
            _planTable.PerformLayout();
            _tabPlan.ResumeLayout(false);
            _fkTable.ResumeLayout(false);
            _indexButtonsTable.ResumeLayout(false);
            _keysTable.ResumeLayout(false);
            _tabKeys.ResumeLayout(false);
            _columnButtonsTable.ResumeLayout(false);
            _columnsTable.ResumeLayout(false);
            _tabColumns.ResumeLayout(false);
            _tabs.ResumeLayout(false);
            _identityTable.ResumeLayout(false);
            _toolbarTable.ResumeLayout(false);
            _rootTable.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel _rootTable;

        private System.Windows.Forms.TableLayoutPanel _toolbarTable;
        private BeepComboBox DatasourcebeepComboBox;
        private BeepComboBox EntitiesbeepComboBox;
        private BeepButton _btnNew;
        private BeepButton ApplybeepButton;
        private BeepButton _btnRename;
        private BeepButton _btnTruncate;
        private BeepButton _btnDrop;
        private BeepButton _btnRefresh;
        private BeepButton _btnPlan;
        private BeepButton _btnEditData;
        private BeepButton _btnDefaults;
        private BeepButton _btnMapEntity;

        private System.Windows.Forms.TableLayoutPanel _identityTable;
        private BeepLabel _lblEntityName;
        private BeepTextBox _txtEntityName;
        private BeepLabel _lblMode;

        private System.Windows.Forms.TabControl _tabs;
        private System.Windows.Forms.TabPage _tabColumns;
        private System.Windows.Forms.TableLayoutPanel _columnsTable;
        private BeepGridPro EntityFieldsbeepGridPro;
        private System.Windows.Forms.TableLayoutPanel _columnButtonsTable;
        private BeepButton _btnAddColumn;
        private BeepButton _btnDeleteColumn;
        private BeepButton _btnMoveUp;
        private BeepButton _btnMoveDown;

        private System.Windows.Forms.TabPage _tabKeys;
        private System.Windows.Forms.TableLayoutPanel _keysTable;
        private BeepListBox _lstKeys;
        private System.Windows.Forms.TableLayoutPanel _indexButtonsTable;
        private BeepButton _btnCreateIndex;
        private BeepButton _btnDropIndex;
        private System.Windows.Forms.TableLayoutPanel _fkTable;
        private BeepLabel _lblFkReferences;
        private BeepComboBox _cboFkEntity;
        private BeepComboBox _cboFkColumn;
        private BeepComboBox _cboFkOnDelete;
        private BeepButton _btnAddFk;
        private BeepButton _btnDropFk;

        private System.Windows.Forms.TabPage _tabPlan;
        private System.Windows.Forms.TableLayoutPanel _planTable;
        private System.Windows.Forms.TextBox _txtPlan;
        private System.Windows.Forms.TableLayoutPanel _planButtonsTable;
        private BeepButton _btnCopyPlan;

        private BeepLabel _stateLabel;
        private System.Windows.Forms.BindingSource fieldsBindingSource;
        private System.Windows.Forms.BindingSource entityManagerViewModelBindingSource;
    }
}
