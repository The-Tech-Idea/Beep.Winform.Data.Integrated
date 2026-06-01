using TheTechIdea.Beep.Winform.Controls;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    partial class uc_Export_SelectDSandFile
    {
        private System.ComponentModel.IContainer components = null;

        // ── Source section ────────────────────────────────────────────────────
        private BeepLabel lblSourceDS;
        private BeepComboBox cmbSourceDS;
        private BeepLabel lblSourceEntity;
        private BeepComboBox cmbSourceEntity;

        // ── Destination section ───────────────────────────────────────────────
        private BeepLabel lblDestMode;
        private BeepComboBox cmbDestMode;

        // File-export controls
        private BeepLabel lblFilePath;
        private BeepTextBox txtFilePath;
        private BeepButton btnBrowse;
        private BeepLabel lblFormat;
        private BeepComboBox cmbFormat;

        // DS-export controls
        private BeepLabel lblDestDS;
        private BeepComboBox cmbDestDS;
        private BeepLabel lblDestEntity;
        private BeepComboBox cmbDestEntity;

        // ── Options ────────────────────────────────────────────────────────────
        private BeepLabel lblDelimiter;
        private BeepTextBox txtDelimiter;
        private BeepCheckBoxBool chkIncludeHeaders;
        private BeepLabel lblEncoding;
        private BeepComboBox cmbEncoding;

        // ── Layout ─────────────────────────────────────────────────────────────
        private System.Windows.Forms.TableLayoutPanel tlpRoot;
        private System.Windows.Forms.Panel pnlFileOptions;
        private System.Windows.Forms.Panel pnlDsOptions;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            tlpRoot = new System.Windows.Forms.TableLayoutPanel();
            lblSourceDS = new BeepLabel();
            cmbSourceDS = new BeepComboBox();
            lblSourceEntity = new BeepLabel();
            cmbSourceEntity = new BeepComboBox();
            lblDestMode = new BeepLabel();
            cmbDestMode = new BeepComboBox();
            pnlFileOptions = new System.Windows.Forms.Panel();
            lblFilePath = new BeepLabel();
            txtFilePath = new BeepTextBox();
            btnBrowse = new BeepButton();
            lblFormat = new BeepLabel();
            cmbFormat = new BeepComboBox();
            lblDelimiter = new BeepLabel();
            txtDelimiter = new BeepTextBox();
            chkIncludeHeaders = new BeepCheckBoxBool();
            lblEncoding = new BeepLabel();
            cmbEncoding = new BeepComboBox();
            pnlDsOptions = new System.Windows.Forms.Panel();
            lblDestDS = new BeepLabel();
            cmbDestDS = new BeepComboBox();
            lblDestEntity = new BeepLabel();
            cmbDestEntity = new BeepComboBox();

            tlpRoot.SuspendLayout();
            pnlFileOptions.SuspendLayout();
            pnlDsOptions.SuspendLayout();
            SuspendLayout();

            // ── tlpRoot ──────────────────────────────────────────────────────
            tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpRoot.ColumnCount = 2;
            tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpRoot.RowCount = 9;
            tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpRoot.Padding = new System.Windows.Forms.Padding(8);

            // Row 0 – source DS label + combo
            tlpRoot.Controls.Add(lblSourceDS, 0, 0);
            tlpRoot.Controls.Add(cmbSourceDS, 1, 0);

            // Row 1 – source entity
            tlpRoot.Controls.Add(lblSourceEntity, 0, 1);
            tlpRoot.Controls.Add(cmbSourceEntity, 1, 1);

            // Row 2 – destination mode
            tlpRoot.Controls.Add(lblDestMode, 0, 2);
            tlpRoot.Controls.Add(cmbDestMode, 1, 2);

            // Row 3 – file options panel (spans 2 cols)
            tlpRoot.SetColumnSpan(pnlFileOptions, 2);
            tlpRoot.Controls.Add(pnlFileOptions, 0, 3);

            // Row 4 – ds options panel (spans 2 cols)
            tlpRoot.SetColumnSpan(pnlDsOptions, 2);
            tlpRoot.Controls.Add(pnlDsOptions, 0, 4);

            // ── Labels / combos ───────────────────────────────────────────────
            lblSourceDS.Text = "Source:";
            lblSourceDS.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblSourceDS.AutoSize = true;
            lblSourceDS.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);

            cmbSourceDS.Dock = System.Windows.Forms.DockStyle.Fill;
            cmbSourceDS.Margin = new System.Windows.Forms.Padding(3);

            lblSourceEntity.Text = "Entity:";
            lblSourceEntity.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblSourceEntity.AutoSize = true;
            lblSourceEntity.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);

            cmbSourceEntity.Dock = System.Windows.Forms.DockStyle.Fill;
            cmbSourceEntity.Margin = new System.Windows.Forms.Padding(3);

            lblDestMode.Text = "Export to:";
            lblDestMode.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblDestMode.AutoSize = true;
            lblDestMode.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);

            cmbDestMode.Dock = System.Windows.Forms.DockStyle.Fill;
            cmbDestMode.Margin = new System.Windows.Forms.Padding(3);

            // ── pnlFileOptions ────────────────────────────────────────────────
            pnlFileOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlFileOptions.AutoSize = true;
            pnlFileOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            pnlFileOptions.Padding = new System.Windows.Forms.Padding(4);

            var tlpFile = new System.Windows.Forms.TableLayoutPanel();
            tlpFile.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpFile.ColumnCount = 3;
            tlpFile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            tlpFile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpFile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            tlpFile.RowCount = 4;
            tlpFile.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tlpFile.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tlpFile.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tlpFile.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));

            // file path row
            lblFilePath.Text = "Output file:";
            lblFilePath.AutoSize = true;
            lblFilePath.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblFilePath.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);

            txtFilePath.Dock = System.Windows.Forms.DockStyle.Fill;
            txtFilePath.Margin = new System.Windows.Forms.Padding(3);

            btnBrowse.Text = "…";
            btnBrowse.Size = new System.Drawing.Size(32, 28);
            btnBrowse.Margin = new System.Windows.Forms.Padding(3);

            tlpFile.Controls.Add(lblFilePath, 0, 0);
            tlpFile.Controls.Add(txtFilePath, 1, 0);
            tlpFile.Controls.Add(btnBrowse, 2, 0);

            // format row
            lblFormat.Text = "Format:";
            lblFormat.AutoSize = true;
            lblFormat.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblFormat.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);

            cmbFormat.Dock = System.Windows.Forms.DockStyle.Fill;
            cmbFormat.Margin = new System.Windows.Forms.Padding(3);

            tlpFile.SetColumnSpan(cmbFormat, 2);
            tlpFile.Controls.Add(lblFormat, 0, 1);
            tlpFile.Controls.Add(cmbFormat, 1, 1);

            // delimiter row
            lblDelimiter.Text = "Delimiter:";
            lblDelimiter.AutoSize = true;
            lblDelimiter.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblDelimiter.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);

            txtDelimiter.Dock = System.Windows.Forms.DockStyle.Fill;
            txtDelimiter.Margin = new System.Windows.Forms.Padding(3);

            tlpFile.SetColumnSpan(txtDelimiter, 2);
            tlpFile.Controls.Add(lblDelimiter, 0, 2);
            tlpFile.Controls.Add(txtDelimiter, 1, 2);

            // encoding + include-headers row
            lblEncoding.Text = "Encoding:";
            lblEncoding.AutoSize = true;
            lblEncoding.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblEncoding.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);

            cmbEncoding.Dock = System.Windows.Forms.DockStyle.Fill;
            cmbEncoding.Margin = new System.Windows.Forms.Padding(3);

            tlpFile.SetColumnSpan(cmbEncoding, 2);
            tlpFile.Controls.Add(lblEncoding, 0, 3);
            tlpFile.Controls.Add(cmbEncoding, 1, 3);

            chkIncludeHeaders.Text = "Include column headers";
            chkIncludeHeaders.AutoSize = true;
            chkIncludeHeaders.CurrentValue = true;
            chkIncludeHeaders.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);

            tlpFile.RowCount = 5;
            tlpFile.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tlpFile.SetColumnSpan(chkIncludeHeaders, 3);
            tlpFile.Controls.Add(chkIncludeHeaders, 0, 4);

            pnlFileOptions.Controls.Add(tlpFile);

            // ── pnlDsOptions ──────────────────────────────────────────────────
            pnlDsOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlDsOptions.AutoSize = true;
            pnlDsOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            pnlDsOptions.Padding = new System.Windows.Forms.Padding(4);

            var tlpDs = new System.Windows.Forms.TableLayoutPanel();
            tlpDs.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpDs.ColumnCount = 2;
            tlpDs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            tlpDs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpDs.RowCount = 2;
            tlpDs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tlpDs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));

            lblDestDS.Text = "Destination DS:";
            lblDestDS.AutoSize = true;
            lblDestDS.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblDestDS.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);

            cmbDestDS.Dock = System.Windows.Forms.DockStyle.Fill;
            cmbDestDS.Margin = new System.Windows.Forms.Padding(3);

            lblDestEntity.Text = "Destination entity:";
            lblDestEntity.AutoSize = true;
            lblDestEntity.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblDestEntity.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);

            cmbDestEntity.Dock = System.Windows.Forms.DockStyle.Fill;
            cmbDestEntity.Margin = new System.Windows.Forms.Padding(3);

            tlpDs.Controls.Add(lblDestDS, 0, 0);
            tlpDs.Controls.Add(cmbDestDS, 1, 0);
            tlpDs.Controls.Add(lblDestEntity, 0, 1);
            tlpDs.Controls.Add(cmbDestEntity, 1, 1);

            pnlDsOptions.Controls.Add(tlpDs);

            // ── UserControl ───────────────────────────────────────────────────
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tlpRoot);
            Name = "uc_Export_SelectDSandFile";
            Size = new System.Drawing.Size(640, 400);

            tlpRoot.ResumeLayout(false);
            pnlFileOptions.ResumeLayout(false);
            pnlDsOptions.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
