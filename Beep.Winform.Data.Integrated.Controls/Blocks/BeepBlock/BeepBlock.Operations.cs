using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Forms.Builtins;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Controls.GridX;
using TheTechIdea.Beep.Winform.Controls.GridX.Export;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms;
using TheTechIdea.Beep.Editor.Forms.Models;

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
                catch (Exception) { }
                return;
            }
            grid.CopyToClipboard(includeHeaders);
        }

        public void PasteFromClipboard()
        {
            var grid = GetGridView();
            grid?.PasteFromClipboard();
        }

        /// <summary>
        /// Convenience wrapper around the host's <c>InsertBlockRecordAsync</c>
        /// that also enforces the <see cref="BeepBlockDefinition.InsertAllowed"/>
        /// property. Mirrors the Oracle Forms <c>CREATE_RECORD</c> built-in.
        /// </summary>
        public async Task<bool> CreateRecordAsync(CancellationToken cancellationToken = default)
        {
            if (!IsInsertAllowed())
            {
                OperationCompleted?.Invoke(this, new OperationCompletedEventArgs("CreateRecord", false, "INSERT_ALLOWED is false on this block."));
                return false;
            }

            if (_formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName))
                return false;

            IsLoading = true;
            try
            {
                var result = await _formsHost.InsertBlockRecordAsync(ManagerBlockName).ConfigureAwait(true);
                SyncFromManager();
                OperationCompleted?.Invoke(this, new OperationCompletedEventArgs("CreateRecord", result));
                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine($"[BeepBlock.CreateRecordAsync] {ManagerBlockName}: {ex.GetType().Name} - {ex.Message}");
                OperationCompleted?.Invoke(this, new OperationCompletedEventArgs("CreateRecord", false, ex.Message));
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Alias of <see cref="CreateRecordAsync"/> — both <c>CREATE_RECORD</c>
        /// and <c>INSERT_RECORD</c> exist in Oracle Forms code so Beep accepts
        /// either.
        /// </summary>
        public Task<bool> InsertRecordAsync(CancellationToken cancellationToken = default)
            => CreateRecordAsync(cancellationToken);

        /// <summary>
        /// Convenience wrapper around the host's
        /// <c>DeleteBlockCurrentRecordAsync</c> that enforces
        /// <see cref="BeepBlockDefinition.DeleteAllowed"/>.
        /// </summary>
        public async Task<bool> DeleteCurrentRecordAsync(CancellationToken cancellationToken = default)
        {
            if (!IsDeleteAllowed())
            {
                OperationCompleted?.Invoke(this, new OperationCompletedEventArgs("DeleteRecord", false, "DELETE_ALLOWED is false on this block."));
                return false;
            }

            if (_formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName))
                return false;

            IsLoading = true;
            try
            {
                var result = await _formsHost.DeleteBlockCurrentRecordAsync(ManagerBlockName).ConfigureAwait(true);
                SyncFromManager();
                OperationCompleted?.Invoke(this, new OperationCompletedEventArgs("DeleteRecord", result));
                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine($"[BeepBlock.DeleteCurrentRecordAsync] {ManagerBlockName}: {ex.GetType().Name} - {ex.Message}");
                OperationCompleted?.Invoke(this, new OperationCompletedEventArgs("DeleteRecord", false, ex.Message));
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Run a Forms-style <c>VALIDATE</c> pass on the current record. The
        /// host's validation manager is invoked with the
        /// <see cref="ValidationTiming.OnCommit"/> timing. Returns
        /// <c>true</c> when no blocking error was raised.
        /// </summary>
        public bool ValidateRecord()
        {
            if (_formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName))
                return false;

            var current = _formsHost.GetCurrentBlockItem(ManagerBlockName);
            if (current == null) return true;

            var record = BuildValidationRecord(current);
            if (record == null) return true;

            var result = _formsHost.ValidateBlockRecord(ManagerBlockName, record, ValidationTiming.OnCommit);
            if (result == null) return true;

            if (result.ItemResults == null) return true;

            bool hasBlockingError = false;
            foreach (var itemResult in result.ItemResults)
            {
                foreach (var rule in itemResult.Value.RuleResults ?? System.Linq.Enumerable.Empty<ValidationRuleResult>())
                {
                    if (!rule.IsValid && (rule.Severity == ValidationSeverity.Error || rule.Severity == ValidationSeverity.Critical))
                    {
                        hasBlockingError = true;
                        break;
                    }
                }
                if (hasBlockingError) break;
            }
            return !hasBlockingError;
        }

        /// <summary>
        /// Clear the current block back to its initial state. Mirrors the
        /// Oracle Forms <c>CLEAR_BLOCK</c> built-in.
        /// </summary>
        public async Task<bool> ClearBlockAsync(CancellationToken cancellationToken = default)
        {
            if (_formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName))
                return false;

            IsLoading = true;
            try
            {
                var result = await _formsHost.ClearBlockAsync(ManagerBlockName, cancellationToken).ConfigureAwait(true);
                SyncFromManager();
                OperationCompleted?.Invoke(this, new OperationCompletedEventArgs("ClearBlock", result));
                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine($"[BeepBlock.ClearBlockAsync] {ManagerBlockName}: {ex.GetType().Name} - {ex.Message}");
                OperationCompleted?.Invoke(this, new OperationCompletedEventArgs("ClearBlock", false, ex.Message));
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Reset the block view state to defaults without affecting the
        /// underlying unit-of-work. Useful for re-entering a freshly-loaded
        /// block in a consistent state.
        /// </summary>
        public void ResetViewState()
        {
            _viewState.IsDirty = false;
            _viewState.IsQueryMode = false;
            _viewState.CurrentRecordIndex = -1;
            _viewState.RecordCount = 0;
            _viewState.RecordStatus = BeepRecordStatus.Query;
            _viewState.CursorBlock = BlockName;
            _viewState.CursorItem = string.Empty;
            _viewState.CursorRecord = 0;
            NotifyViewStateChanged();
        }

        /// <summary>
        /// Display a status-bar message routed through the host's notification
        /// service. Mirrors the Oracle Forms <c>MESSAGE</c> built-in at the
        /// block level so block code does not have to reach into the form.
        /// </summary>
        public void Message(string text, BeepMessageSeverity severity = BeepMessageSeverity.Info)
        {
            if (_formsHost is BeepForms beepForms)
            {
                beepForms.PublishBuiltinMessage(text ?? string.Empty, 0, MapToBuiltinSeverity(severity));
            }
        }

        private static BeepBuiltinMessageSeverity MapToBuiltinSeverity(BeepMessageSeverity severity)
        {
            return severity switch
            {
                BeepMessageSeverity.Warning => BeepBuiltinMessageSeverity.Warning,
                BeepMessageSeverity.Error => BeepBuiltinMessageSeverity.Error,
                _ => BeepBuiltinMessageSeverity.Info
            };
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
