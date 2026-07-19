using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;
using TheTechIdea.Beep.Winform.Controls.ProgressBars;

namespace TheTechIdea.Beep.Winform.Default.Views.Configuration
{
    partial class uc_DataImportWizard
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();

                // Cancel first, so a run still in flight is asked to stop before its manager is torn
                // out from under it. Then dispose and null: base.Dispose destroys the handle, which
                // raises OnHandleDestroyed, which cancels _cts — nulling stops that override from
                // throwing ObjectDisposedException back out of Dispose.
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;

                // DataImportManager is IDisposable and is held across the Preflight and Run stages
                // rather than per-call, so the control owns its lifetime.
                _manager?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _rootPanel = new BeepPanel();
            _headerPanel = new BeepPanel();
            _lblTitle = new BeepLabel();
            _lblSubtitle = new BeepLabel();
            _contentHost = new BeepPanel();

            _stepScope = new BeepPanel();
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
            _chkApplyDefaults = new BeepCheckBoxBool();

            _stepPreflight = new BeepPanel();
            _lblPreflightSummary = new BeepLabel();
            _lstFindings = new BeepListBox();

            _stepRun = new BeepPanel();
            _progress = new BeepProgressBar();
            _lblRunStatus = new BeepLabel();
            _lstRunLog = new BeepListBox();

            _actionsPanel = new BeepPanel();
            _actionsFlow = new System.Windows.Forms.FlowLayoutPanel();
            _btnNext = new BeepButton();
            _btnPause = new BeepButton();
            _btnBack = new BeepButton();
            _btnCancel = new BeepButton();
            _lblStatus = new BeepLabel();

            _rootPanel.SuspendLayout();
            _headerPanel.SuspendLayout();
            _contentHost.SuspendLayout();
            _stepScope.SuspendLayout();
            _scopeTable.SuspendLayout();
            _stepPreflight.SuspendLayout();
            _stepRun.SuspendLayout();
            _actionsPanel.SuspendLayout();
            _actionsFlow.SuspendLayout();
            SuspendLayout();

            // ── root ──
            _rootPanel.ControlStyle = TheTechIdea.Beep.Winform.Controls.Common.BeepControlStyle.Material3;
            _rootPanel.IsFrameless = true;
            _rootPanel.ShowTitle = false;
            _rootPanel.ShowTitleLine = false;
            _rootPanel.UseThemeColors = true;
            _rootPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _rootPanel.Padding = new System.Windows.Forms.Padding(12);

            // ── header ──
            _headerPanel.IsFrameless = true;
            _headerPanel.ShowTitle = false;
            _headerPanel.ShowTitleLine = false;
            _headerPanel.UseThemeColors = true;
            _headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            _headerPanel.Height = 76;

            _lblSubtitle.UseThemeColors = true;
            _lblSubtitle.IsFrameless = true;
            _lblSubtitle.AutoEllipsis = true;
            _lblSubtitle.Dock = System.Windows.Forms.DockStyle.Top;
            _lblSubtitle.Height = 24;
            _lblSubtitle.Padding = new System.Windows.Forms.Padding(16, 2, 16, 4);
            _lblSubtitle.Text = "Choose source and destination, then run the import.";

            _lblTitle.UseThemeColors = true;
            _lblTitle.IsFrameless = true;
            _lblTitle.AutoEllipsis = true;
            _lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            _lblTitle.Height = 40;
            _lblTitle.Padding = new System.Windows.Forms.Padding(16, 12, 16, 0);
            _lblTitle.Text = "Data Import Wizard";

            _headerPanel.Controls.Add(_lblSubtitle);
            _headerPanel.Controls.Add(_lblTitle);

            // ── content host ──
            _contentHost.IsFrameless = true;
            _contentHost.ShowTitle = false;
            _contentHost.ShowTitleLine = false;
            _contentHost.UseThemeColors = true;
            _contentHost.Dock = System.Windows.Forms.DockStyle.Fill;
            _contentHost.Padding = new System.Windows.Forms.Padding(8);

            // ── step 1: scope ──
            _stepScope.IsFrameless = true;
            _stepScope.ShowTitle = false;
            _stepScope.ShowTitleLine = false;
            _stepScope.UseThemeColors = true;
            _stepScope.Dock = System.Windows.Forms.DockStyle.Fill;

            _scopeTable.Dock = System.Windows.Forms.DockStyle.Fill;
            _scopeTable.ColumnCount = 2;
            _scopeTable.RowCount = 8;
            _scopeTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            _scopeTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            // Seven labelled rows (0-6), then a percent-sized filler row that absorbs the slack.
            for (int i = 0; i < 7; i++)
                _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            _scopeTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            _lblSourceConn.UseThemeColors = true;
            _lblSourceConn.IsFrameless = true;
            _lblSourceConn.Text = "Source connection";
            _lblSourceConn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblSourceConn.Dock = System.Windows.Forms.DockStyle.Fill;
            _cboSourceConn.UseThemeColors = true;
            _cboSourceConn.Dock = System.Windows.Forms.DockStyle.Fill;

            _lblSourceEntity.UseThemeColors = true;
            _lblSourceEntity.IsFrameless = true;
            _lblSourceEntity.Text = "Source entity";
            _lblSourceEntity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblSourceEntity.Dock = System.Windows.Forms.DockStyle.Fill;
            _cboSourceEntity.UseThemeColors = true;
            _cboSourceEntity.Dock = System.Windows.Forms.DockStyle.Fill;

            _lblDestConn.UseThemeColors = true;
            _lblDestConn.IsFrameless = true;
            _lblDestConn.Text = "Destination connection";
            _lblDestConn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblDestConn.Dock = System.Windows.Forms.DockStyle.Fill;
            _cboDestConn.UseThemeColors = true;
            _cboDestConn.Dock = System.Windows.Forms.DockStyle.Fill;

            _lblDestEntity.UseThemeColors = true;
            _lblDestEntity.IsFrameless = true;
            _lblDestEntity.Text = "Destination entity";
            _lblDestEntity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblDestEntity.Dock = System.Windows.Forms.DockStyle.Fill;
            _cboDestEntity.UseThemeColors = true;
            _cboDestEntity.Dock = System.Windows.Forms.DockStyle.Fill;

            _lblBatchSize.UseThemeColors = true;
            _lblBatchSize.IsFrameless = true;
            _lblBatchSize.Text = "Batch size";
            _lblBatchSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblBatchSize.Dock = System.Windows.Forms.DockStyle.Fill;
            _txtBatchSize.UseThemeColors = true;
            _txtBatchSize.Text = "50";
            _txtBatchSize.Dock = System.Windows.Forms.DockStyle.Fill;

            _chkCreateDestination.UseThemeColors = true;
            _chkCreateDestination.Text = "Create destination entity if it does not exist";
            _chkCreateDestination.Checked = true;
            _chkCreateDestination.Dock = System.Windows.Forms.DockStyle.Fill;

            _chkApplyDefaults.UseThemeColors = true;
            _chkApplyDefaults.Text = "Apply defaults before write";
            _chkApplyDefaults.Checked = true;
            _chkApplyDefaults.Dock = System.Windows.Forms.DockStyle.Fill;

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
            _scopeTable.Controls.Add(_chkApplyDefaults, 1, 6);
            _stepScope.Controls.Add(_scopeTable);

            // ── step 2: preflight ──
            _stepPreflight.IsFrameless = true;
            _stepPreflight.ShowTitle = false;
            _stepPreflight.ShowTitleLine = false;
            _stepPreflight.UseThemeColors = true;
            _stepPreflight.Dock = System.Windows.Forms.DockStyle.Fill;
            _stepPreflight.Visible = false;

            _lstFindings.UseThemeColors = true;
            _lstFindings.ShowSearch = false;
            _lstFindings.Dock = System.Windows.Forms.DockStyle.Fill;

            _lblPreflightSummary.UseThemeColors = true;
            _lblPreflightSummary.IsFrameless = true;
            _lblPreflightSummary.AutoEllipsis = true;
            _lblPreflightSummary.Dock = System.Windows.Forms.DockStyle.Top;
            _lblPreflightSummary.Height = 28;
            _lblPreflightSummary.Text = "Not evaluated.";

            _stepPreflight.Controls.Add(_lstFindings);
            _stepPreflight.Controls.Add(_lblPreflightSummary);

            // ── step 3: run ──
            _stepRun.IsFrameless = true;
            _stepRun.ShowTitle = false;
            _stepRun.ShowTitleLine = false;
            _stepRun.UseThemeColors = true;
            _stepRun.Dock = System.Windows.Forms.DockStyle.Fill;
            _stepRun.Visible = false;

            _lstRunLog.UseThemeColors = true;
            _lstRunLog.ShowSearch = false;
            _lstRunLog.Dock = System.Windows.Forms.DockStyle.Fill;

            _lblRunStatus.UseThemeColors = true;
            _lblRunStatus.IsFrameless = true;
            _lblRunStatus.AutoEllipsis = true;
            _lblRunStatus.Dock = System.Windows.Forms.DockStyle.Top;
            _lblRunStatus.Height = 28;
            _lblRunStatus.Text = "Not started.";

            _progress.UseThemeColors = true;
            _progress.Dock = System.Windows.Forms.DockStyle.Top;
            _progress.Height = 24;
            _progress.Minimum = 0;
            _progress.Maximum = 100;

            _stepRun.Controls.Add(_lstRunLog);
            _stepRun.Controls.Add(_lblRunStatus);
            _stepRun.Controls.Add(_progress);

            _contentHost.Controls.Add(_stepRun);
            _contentHost.Controls.Add(_stepPreflight);
            _contentHost.Controls.Add(_stepScope);

            // ── actions ──
            _actionsPanel.IsFrameless = true;
            _actionsPanel.ShowTitle = false;
            _actionsPanel.ShowTitleLine = false;
            _actionsPanel.UseThemeColors = true;
            _actionsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            _actionsPanel.Height = 52;
            _actionsPanel.Padding = new System.Windows.Forms.Padding(10);

            _actionsFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            _actionsFlow.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            _actionsFlow.WrapContents = false;

            // Flow is RightToLeft: the first child is rightmost.
            _btnNext.UseThemeColors = true;
            _btnNext.Text = "Run Import";
            _btnNext.MinimumSize = new System.Drawing.Size(130, 36);

            // Pause/Resume is real: DataImportManager.PauseImport resets the _pauseEvent that
            // RunImportAsync's batch loop waits on. Hidden until a run is actually in flight.
            _btnPause.UseThemeColors = true;
            _btnPause.Text = "Pause";
            _btnPause.MinimumSize = new System.Drawing.Size(100, 32);
            _btnPause.Visible = false;

            _btnBack.UseThemeColors = true;
            _btnBack.Text = "Back";
            _btnBack.MinimumSize = new System.Drawing.Size(100, 32);
            _btnBack.Enabled = false;

            _btnCancel.UseThemeColors = true;
            _btnCancel.Text = "Cancel";
            _btnCancel.MinimumSize = new System.Drawing.Size(100, 32);

            _lblStatus.UseThemeColors = true;
            _lblStatus.IsFrameless = true;
            _lblStatus.AutoEllipsis = true;
            _lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            _lblStatus.Dock = System.Windows.Forms.DockStyle.Left;
            _lblStatus.Width = 320;
            _lblStatus.Text = string.Empty;

            _actionsFlow.Controls.Add(_btnNext);
            _actionsFlow.Controls.Add(_btnPause);
            _actionsFlow.Controls.Add(_btnBack);
            _actionsFlow.Controls.Add(_btnCancel);
            _actionsPanel.Controls.Add(_actionsFlow);
            _actionsPanel.Controls.Add(_lblStatus);

            _rootPanel.Controls.Add(_contentHost);
            _rootPanel.Controls.Add(_headerPanel);
            _rootPanel.Controls.Add(_actionsPanel);

            // ── uc_DataImportWizard ──
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(_rootPanel);
            Name = "uc_DataImportWizard";
            Size = new System.Drawing.Size(840, 560);

            _actionsFlow.ResumeLayout(false);
            _actionsPanel.ResumeLayout(false);
            _stepRun.ResumeLayout(false);
            _stepPreflight.ResumeLayout(false);
            _scopeTable.ResumeLayout(false);
            _stepScope.ResumeLayout(false);
            _contentHost.ResumeLayout(false);
            _headerPanel.ResumeLayout(false);
            _rootPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private BeepPanel _rootPanel;
        private BeepPanel _headerPanel;
        private BeepLabel _lblTitle;
        private BeepLabel _lblSubtitle;
        private BeepPanel _contentHost;

        private BeepPanel _stepScope;
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
        private BeepCheckBoxBool _chkApplyDefaults;

        private BeepPanel _stepPreflight;
        private BeepLabel _lblPreflightSummary;
        private BeepListBox _lstFindings;

        private BeepPanel _stepRun;
        private BeepProgressBar _progress;
        private BeepLabel _lblRunStatus;
        private BeepListBox _lstRunLog;

        private BeepPanel _actionsPanel;
        private System.Windows.Forms.FlowLayoutPanel _actionsFlow;
        private BeepButton _btnNext;
        private BeepButton _btnPause;
        private BeepButton _btnBack;
        private BeepButton _btnCancel;
        private BeepLabel _lblStatus;
    }
}
