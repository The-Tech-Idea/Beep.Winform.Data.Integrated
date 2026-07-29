using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;
using TheTechIdea.Beep.Winform.Controls.ProgressBars;
using TheTechIdea.Beep.Winform.Controls.Wizards.Forms;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    partial class uc_SyncWizard
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
                _manager?.Dispose();
            }
            base.Dispose(disposing);
        }

        // The designer owns the three step pages and a status line only. Wizard chrome — stepper,
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
            _lblBatchSize = new BeepLabel();
            _txtBatchSize = new BeepTextBox();
            _chkCreateDestination = new BeepCheckBoxBool();

            _pagePreflight = new WizardPage();
            _lblPreflightSummary = new BeepLabel();
            _lstFindings = new BeepListBox();

            _pageRun = new WizardPage();
            _progress = new BeepProgressBar();
            _lblRunStatus = new BeepLabel();
            _lstRunLog = new BeepListBox();

            _hostPanel.SuspendLayout();
            _pageScope.SuspendLayout();
            _scopeTable.SuspendLayout();
            _pagePreflight.SuspendLayout();
            _pageRun.SuspendLayout();
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
            _pageScope.Description = "Choose source, destination, and options.";
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
            // Six labelled rows (0-5), then a percent-sized filler row that absorbs the slack.
            // Written out one row at a time: the designer's code parser only understands
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

            _lblBatchSize.Dock = System.Windows.Forms.DockStyle.Fill;
            _lblBatchSize.IsFrameless = true;
            _lblBatchSize.Name = "_lblBatchSize";
            _lblBatchSize.Text = "Batch size";
            _lblBatchSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblBatchSize.UseThemeColors = true;
            _txtBatchSize.Dock = System.Windows.Forms.DockStyle.Fill;
            _txtBatchSize.Name = "_txtBatchSize";
            _txtBatchSize.Text = "50";
            _txtBatchSize.UseThemeColors = true;

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
            _scopeTable.Controls.Add(_lblBatchSize, 0, 4);
            _scopeTable.Controls.Add(_txtBatchSize, 1, 4);
            _scopeTable.Controls.Add(_chkCreateDestination, 1, 5);
            _pageScope.Controls.Add(_scopeTable);

            // ── step 2: preflight ──
            _pagePreflight.Description = "Preflight findings — nothing has moved yet.";
            _pagePreflight.Dock = System.Windows.Forms.DockStyle.Fill;
            _pagePreflight.Name = "_pagePreflight";
            _pagePreflight.NextButtonText = "Run Sync";
            _pagePreflight.Title = "Preflight";
            _pagePreflight.Visible = false;

            _lstFindings.Dock = System.Windows.Forms.DockStyle.Fill;
            _lstFindings.Name = "_lstFindings";
            _lstFindings.ShowSearch = false;
            _lstFindings.UseThemeColors = true;

            _lblPreflightSummary.AutoEllipsis = true;
            _lblPreflightSummary.Dock = System.Windows.Forms.DockStyle.Top;
            _lblPreflightSummary.Height = 28;
            _lblPreflightSummary.IsFrameless = true;
            _lblPreflightSummary.Name = "_lblPreflightSummary";
            _lblPreflightSummary.Text = "Not evaluated.";
            _lblPreflightSummary.UseThemeColors = true;

            _pagePreflight.Controls.Add(_lstFindings);
            _pagePreflight.Controls.Add(_lblPreflightSummary);

            // ── step 3: run ──
            _pageRun.Description = "Sync progress and result.";
            _pageRun.Dock = System.Windows.Forms.DockStyle.Fill;
            _pageRun.Name = "_pageRun";
            _pageRun.Title = "Run";
            _pageRun.Visible = false;

            _lstRunLog.Dock = System.Windows.Forms.DockStyle.Fill;
            _lstRunLog.Name = "_lstRunLog";
            _lstRunLog.ShowSearch = false;
            _lstRunLog.UseThemeColors = true;

            _lblRunStatus.AutoEllipsis = true;
            _lblRunStatus.Dock = System.Windows.Forms.DockStyle.Top;
            _lblRunStatus.Height = 28;
            _lblRunStatus.IsFrameless = true;
            _lblRunStatus.Name = "_lblRunStatus";
            _lblRunStatus.Text = "Not started.";
            _lblRunStatus.UseThemeColors = true;

            _progress.Dock = System.Windows.Forms.DockStyle.Top;
            _progress.Height = 24;
            _progress.Maximum = 100;
            _progress.Minimum = 0;
            _progress.Name = "_progress";
            _progress.UseThemeColors = true;

            _pageRun.Controls.Add(_lstRunLog);
            _pageRun.Controls.Add(_lblRunStatus);
            _pageRun.Controls.Add(_progress);

            // Parented here so they are owned (and disposed) even if the wizard never starts; the
            // wizard form re-parents whichever page is current into its own content panel.
            _hostPanel.Controls.Add(_pageRun);
            _hostPanel.Controls.Add(_pagePreflight);
            _hostPanel.Controls.Add(_pageScope);

            // ── uc_SyncWizard ──
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(_hostPanel);
            Controls.Add(_lblStatus);
            Name = "uc_SyncWizard";
            Size = new System.Drawing.Size(840, 560);

            _pageRun.ResumeLayout(false);
            _pagePreflight.ResumeLayout(false);
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
        private BeepLabel _lblBatchSize;
        private BeepTextBox _txtBatchSize;
        private BeepCheckBoxBool _chkCreateDestination;

        private WizardPage _pagePreflight;
        private BeepLabel _lblPreflightSummary;
        private BeepListBox _lstFindings;

        private WizardPage _pageRun;
        private BeepProgressBar _progress;
        private BeepLabel _lblRunStatus;
        private BeepListBox _lstRunLog;
    }
}
