using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls.GridX;
using TheTechIdea.Beep.Winform.Controls.GridX.Export;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks
{
    public partial class BeepBlock
    {
        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading == value)
                    return;
                _isLoading = value;
                RefreshPresentation();
                Invalidate();
            }
        }

        public event EventHandler<OperationCompletedEventArgs>? OperationCompleted;

        public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName))
                return false;

            IsLoading = true;
            try
            {
                var result = await _formsHost.SaveBlockAsync(ManagerBlockName).ConfigureAwait(true);
                SyncFromManager();
                OperationCompleted?.Invoke(this, new OperationCompletedEventArgs("Commit", result));
                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine($"[BeepBlock.CommitAsync] {ManagerBlockName}: {ex.GetType().Name} - {ex.Message}");
                OperationCompleted?.Invoke(this, new OperationCompletedEventArgs("Commit", false, ex.Message));
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public bool ExportToCsv(string filePath)
        {
            var grid = GetGridView();
            if (grid == null)
                return false;
            grid.ExportToCsv(filePath);
            return true;
        }

        public bool ExportToJson(string filePath)
        {
            var grid = GetGridView();
            if (grid == null)
                return false;
            grid.ExportToJson(filePath);
            return true;
        }

        public bool ExportToHtml(string filePath)
        {
            var grid = GetGridView();
            if (grid == null)
                return false;
            grid.ExportToHtml(filePath);
            return true;
        }

        public string ExportToCsvString()
        {
            var grid = GetGridView();
            return grid?.ExportToString(GridExportFormat.Csv) ?? string.Empty;
        }

        public string ExportToJsonString()
        {
            var grid = GetGridView();
            return grid?.ExportToString(GridExportFormat.Json) ?? string.Empty;
        }

        public void CopyToClipboard(bool includeHeaders = true)
        {
            var grid = GetGridView();
            if (grid == null)
            {
                try { System.Windows.Forms.Clipboard.SetText(ExportToCsvString()); }
                catch { }
                return;
            }
            grid.CopyToClipboard(includeHeaders);
        }

        public void PasteFromClipboard()
        {
            var grid = GetGridView();
            grid?.PasteFromClipboard();
        }

        private BeepGridPro? GetGridView()
        {
            return _gridView as BeepGridPro;
        }
    }

    public sealed class OperationCompletedEventArgs : EventArgs
    {
        public OperationCompletedEventArgs(string operation, bool success, string? message = null)
        {
            Operation = operation;
            Success = success;
            Message = message ?? (success ? "Completed" : "Failed");
        }

        public string Operation { get; }
        public bool Success { get; }
        public string Message { get; }
    }
}
