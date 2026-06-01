using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls.GridX;
using TheTechIdea.Beep.Winform.Controls.Models;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    public partial class uc_NuggetsManage
    {
        private DataTable? _dtLogs;

        private void InitializeActivityData()
        {
            _cmbLogFilter.ListItems = new BindingList<SimpleItem>
            {
                new SimpleItem { Text = "All", Item = "All" },
                new SimpleItem { Text = "Info", Item = "Info" },
                new SimpleItem { Text = "Success", Item = "Success" },
                new SimpleItem { Text = "Warning", Item = "Warning" },
                new SimpleItem { Text = "Error", Item = "Error" }
            };

            _dtLogs = new DataTable("ActivityLog");
            _dtLogs.Columns.Add("Time", typeof(string));
            _dtLogs.Columns.Add("Severity", typeof(string));
            _dtLogs.Columns.Add("Message", typeof(string));
            _dtLogs.Columns.Add("Package", typeof(string));

            _gridLogs.Columns.Add(new BeepColumnConfig { ColumnName = "Time", ColumnCaption = "Time", Width = 80 });
            _gridLogs.Columns.Add(new BeepColumnConfig { ColumnName = "Severity", ColumnCaption = "Severity", Width = 80 });
            _gridLogs.Columns.Add(new BeepColumnConfig { ColumnName = "Message", ColumnCaption = "Message", Width = 400 });
            _gridLogs.Columns.Add(new BeepColumnConfig { ColumnName = "Package", ColumnCaption = "Package", Width = 150 });
            _gridLogs.DataSource = _dtLogs;
        }

        // ---- Event handlers wired in Designer ----

        private void CmbLogFilter_SelectedItemChanged(object? sender, EventArgs e) => RefreshLogs();
        private void BtnLogClear_Click(object? sender, EventArgs e) { GetService().ClearLogs(); RefreshLogs(); }
        private void BtnLogCopy_Click(object? sender, EventArgs e) => CopyLogsToClipboard();
        private void BtnLogExport_Click(object? sender, EventArgs e) => ExportLogs();
        private void TabActivity_Enter(object? sender, EventArgs e) => RefreshLogs();

        // ---- Logic ----

        private void RefreshLogs()
        {
            if (_dtLogs == null) return;
            _dtLogs.Rows.Clear();
            try
            {
                var filter = _cmbLogFilter?.SelectedItem?.Item?.ToString() ?? "All";
                var logs = GetService().Logs;
                if (filter != "All")
                    logs = logs.Where(l => l.Severity.ToString() == filter).ToList();

                foreach (var log in logs)
                    _dtLogs.Rows.Add(log.Timestamp.ToString("HH:mm:ss"), log.Severity.ToString(), log.Message, log.PackageId ?? string.Empty);

                _lblLogStatus.Text = $"{logs.Count} entries";
            }
            catch (Exception ex) { _lblLogStatus.Text = $"Refresh failed: {ex.Message}"; }
        }

        private void CopyLogsToClipboard()
        {
            try
            {
                var text = string.Join(Environment.NewLine, GetService().Logs.Select(l => $"[{l.Timestamp:HH:mm:ss}] {l.Severity}: {l.Message}"));
                Clipboard.SetText(text);
                _lblLogStatus.Text = "Copied to clipboard.";
            }
            catch (Exception ex) { _lblLogStatus.Text = $"Copy failed: {ex.Message}"; }
        }

        private void ExportLogs()
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "Log files (*.log)|*.log|JSON files (*.json)|*.json|All files (*.*)|*.*",
                FileName = $"NuggetsActivity_{DateTime.Now:yyyyMMdd_HHmmss}.log"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                var text = string.Join(Environment.NewLine, GetService().Logs.Select(l => $"[{l.Timestamp:yyyy-MM-dd HH:mm:ss}] {l.Severity}: {l.Message}"));
                System.IO.File.WriteAllText(dialog.FileName, text);
                _lblLogStatus.Text = $"Exported to {dialog.FileName}";
            }
            catch (Exception ex) { _lblLogStatus.Text = $"Export failed: {ex.Message}"; }
        }
    }
}

