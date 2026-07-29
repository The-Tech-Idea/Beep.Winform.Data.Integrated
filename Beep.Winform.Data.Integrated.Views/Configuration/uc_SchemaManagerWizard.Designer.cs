using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;
using TheTechIdea.Beep.Winform.Controls.VerticalTables;
using TheTechIdea.Beep.Winform.Controls.Wizards.Forms;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    partial class uc_SchemaManagerWizard
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) { components.Dispose(); }
            base.Dispose(disposing);
        }

        // The designer owns the two step pages and a status line only. Wizard chrome — stepper,
        // Back/Next/Cancel, progress, validation display — comes from the Wizards framework form
        // that StartWizard embeds into _hostPanel.
        private void InitializeComponent()
        {
            _hostPanel = new System.Windows.Forms.Panel();
            _lblStatus = new BeepLabel();

            _pageScope = new WizardPage();
            _scopeTable = new System.Windows.Forms.TableLayoutPanel();
            _lblSourceConn = new BeepLabel();
            _cboSourceConn = new BeepComboBox();
            _lblSourceEntity = new BeepLabel();
            _cboSourceEntity = new BeepComboBox();
            _lblDestConn = new BeepLabel();
            _cboDestConn = new BeepComboBox();
            _lblDestEntity = new BeepLabel();
            _cboDestEntity = new BeepComboBox();
            _chkAddMissingColumns = new BeepCheckBoxBool();
            _chkCreateDestination = new BeepCheckBoxBool();

            _pageResults = new WizardPage();
            _lblResultsSummary = new BeepLabel();
            _lblFingerprint = new BeepLabel();
            _tblCompare = new BeepVerticalTable();
            _lstResults = new BeepListBox();

            _hostPanel.SuspendLayout();
            _pageScope.SuspendLayout();
            _scopeTable.SuspendLayout();
            _pageResults.SuspendLayout();
            SuspendLayout();

            // ── host for the embedded wizard form ──
            _hostPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _hostPanel.Name = "_hostPanel";

            _lblStatus.AutoEllipsis = true;
            _lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            _lblStatus.Height = 28;
            _lblStatus.IsFrameless = true;
            _lblStatus.Name = "_lblStatus";
            _lblStatus.Padding = new System.Windows.Forms.Padding(12, 4, 12, 4);
            _lblStatus.Text = "";
            _lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblStatus.UseThemeColors = true;

            // ── step 1: scope ──
            _pageScope.Description = "Choose source and destination entities.";
            _pageScope.Dock = System.Windows.Forms.DockStyle.Fill;
            _pageScope.Name = "_pageScope";
            _pageScope.NextButtonText = "Run Preflight";
            _pageScope.Title = "Scope";

            _scopeTable.ColumnCount = 2;
            _scopeTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            _scopeTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _scopeTable.Dock = System.Windows.Forms.DockStyle.Fill;
            _scopeTable.Name = "_scopeTable";
            _scopeTable.RowCount = 7;
            // Six rows written out one at a time: the designer's code parser only understands
            // assignments, object creation and method calls, so a loop here breaks the design view.
            _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            _lblSourceConn.Dock = System.Windows.Forms.DockStyle.Fill;
            _lblSourceConn.IsFrameless = true;
            _lblSourceConn.Name = "_lblSourceConn";
            _lblSourceConn.Text = "Source connection";
            _lblSourceConn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblSourceConn.UseThemeColors = true;
            _cboSourceConn.Dock = System.Windows.Forms.DockStyle.Fill;
            _cboSourceConn.Name = "_cboSourceConn";
            _cboSourceConn.UseThemeColors = true;

            _lblSourceEntity.Dock = System.Windows.Forms.DockStyle.Fill;
            _lblSourceEntity.IsFrameless = true;
            _lblSourceEntity.Name = "_lblSourceEntity";
            _lblSourceEntity.Text = "Source entity";
            _lblSourceEntity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblSourceEntity.UseThemeColors = true;
            _cboSourceEntity.Dock = System.Windows.Forms.DockStyle.Fill;
            _cboSourceEntity.Name = "_cboSourceEntity";
            _cboSourceEntity.UseThemeColors = true;

            _lblDestConn.Dock = System.Windows.Forms.DockStyle.Fill;
            _lblDestConn.IsFrameless = true;
            _lblDestConn.Name = "_lblDestConn";
            _lblDestConn.Text = "Destination connection";
            _lblDestConn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblDestConn.UseThemeColors = true;
            _cboDestConn.Dock = System.Windows.Forms.DockStyle.Fill;
            _cboDestConn.Name = "_cboDestConn";
            _cboDestConn.UseThemeColors = true;

            _lblDestEntity.Dock = System.Windows.Forms.DockStyle.Fill;
            _lblDestEntity.IsFrameless = true;
            _lblDestEntity.Name = "_lblDestEntity";
            _lblDestEntity.Text = "Destination entity";
            _lblDestEntity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblDestEntity.UseThemeColors = true;
            _cboDestEntity.Dock = System.Windows.Forms.DockStyle.Fill;
            _cboDestEntity.Name = "_cboDestEntity";
            _cboDestEntity.UseThemeColors = true;

            _chkAddMissingColumns.Checked = true;
            _chkAddMissingColumns.Dock = System.Windows.Forms.DockStyle.Fill;
            _chkAddMissingColumns.Name = "_chkAddMissingColumns";
            _chkAddMissingColumns.Text = "Add missing destination columns";
            _chkAddMissingColumns.UseThemeColors = true;

            _chkCreateDestination.Checked = true;
            _chkCreateDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            _chkCreateDestination.Name = "_chkCreateDestination";
            _chkCreateDestination.Text = "Create destination entity if it does not exist";
            _chkCreateDestination.UseThemeColors = true;

            _scopeTable.Controls.Add(_lblSourceConn, 0, 0);
            _scopeTable.Controls.Add(_cboSourceConn, 1, 0);
            _scopeTable.Controls.Add(_lblSourceEntity, 0, 1);
            _scopeTable.Controls.Add(_cboSourceEntity, 1, 1);
            _scopeTable.Controls.Add(_lblDestConn, 0, 2);
            _scopeTable.Controls.Add(_cboDestConn, 1, 2);
            _scopeTable.Controls.Add(_lblDestEntity, 0, 3);
            _scopeTable.Controls.Add(_cboDestEntity, 1, 3);
            _scopeTable.Controls.Add(_chkAddMissingColumns, 1, 4);
            _scopeTable.Controls.Add(_chkCreateDestination, 1, 5);
            _pageScope.Controls.Add(_scopeTable);

            // ── step 2: results ──
            _pageResults.Description = "Preflight results — build a sync draft when ready.";
            _pageResults.Dock = System.Windows.Forms.DockStyle.Fill;
            _pageResults.Name = "_pageResults";
            _pageResults.Title = "Results";
            _pageResults.Visible = false;

            // Field-level comparison table (source vs destination, via SchemaComparator).
            // A vertical comparison table reads far better than a flat grid for schema diffs:
            // one row per field, one column per side plus a verdict column.
            _tblCompare.Dock = System.Windows.Forms.DockStyle.Fill;
            _tblCompare.Name = "_tblCompare";
            _tblCompare.RowGroupingEnabled = true;
            _tblCompare.SelectionMode = TheTechIdea.Beep.Winform.Controls.VerticalTables.VerticalTableSelectionMode.Single;

            // Preflight/draft log — the flat status/log lines live at the bottom so the
            // structured field comparison gets the main real estate.
            _lstResults.Dock = System.Windows.Forms.DockStyle.Bottom;
            _lstResults.Height = 150;
            _lstResults.Name = "_lstResults";
            _lstResults.ShowSearch = false;
            _lstResults.UseThemeColors = true;

            _lblFingerprint.AutoEllipsis = true;
            _lblFingerprint.Dock = System.Windows.Forms.DockStyle.Top;
            _lblFingerprint.Height = 24;
            _lblFingerprint.IsFrameless = true;
            _lblFingerprint.Name = "_lblFingerprint";
            _lblFingerprint.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            _lblFingerprint.Text = "";
            _lblFingerprint.UseThemeColors = true;

            _lblResultsSummary.AutoEllipsis = true;
            _lblResultsSummary.Dock = System.Windows.Forms.DockStyle.Top;
            _lblResultsSummary.Height = 44;
            _lblResultsSummary.IsFrameless = true;
            _lblResultsSummary.Name = "_lblResultsSummary";
            _lblResultsSummary.Text = "Preflight has not run yet.";
            _lblResultsSummary.UseThemeColors = true;

            // Fill added first so the docked edges (summary/fingerprint on top, log on
            // bottom) claim their bands and the table takes the remainder.
            _pageResults.Controls.Add(_tblCompare);
            _pageResults.Controls.Add(_lstResults);
            _pageResults.Controls.Add(_lblFingerprint);
            _pageResults.Controls.Add(_lblResultsSummary);

            // Parented here so they are owned (and disposed) even if the wizard never starts; the
            // wizard form re-parents whichever page is current into its own content panel.
            _hostPanel.Controls.Add(_pageResults);
            _hostPanel.Controls.Add(_pageScope);

            // ── uc_SchemaManagerWizard ──
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(_hostPanel);
            Controls.Add(_lblStatus);
            Name = "uc_SchemaManagerWizard";
            Size = new System.Drawing.Size(840, 560);

            _pageResults.ResumeLayout(false);
            _scopeTable.ResumeLayout(false);
            _pageScope.ResumeLayout(false);
            _hostPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private System.Windows.Forms.Panel _hostPanel;
        private BeepLabel _lblStatus;

        private WizardPage _pageScope;
        private System.Windows.Forms.TableLayoutPanel _scopeTable;
        private BeepLabel _lblSourceConn;
        private BeepComboBox _cboSourceConn;
        private BeepLabel _lblSourceEntity;
        private BeepComboBox _cboSourceEntity;
        private BeepLabel _lblDestConn;
        private BeepComboBox _cboDestConn;
        private BeepLabel _lblDestEntity;
        private BeepComboBox _cboDestEntity;
        private BeepCheckBoxBool _chkAddMissingColumns;
        private BeepCheckBoxBool _chkCreateDestination;

        private WizardPage _pageResults;
        private BeepLabel _lblResultsSummary;
        private BeepLabel _lblFingerprint;
        private BeepVerticalTable _tblCompare;
        private BeepListBox _lstResults;
    }
}
