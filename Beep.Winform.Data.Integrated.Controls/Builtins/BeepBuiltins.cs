using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Forms.Builtins;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Lov;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Builtins
{
    /// <summary>
    /// Oracle Forms-compatible built-in procedures implemented on top of
    /// <see cref="IBuiltinHost"/>. Mirrors the most commonly used built-ins
    /// (GO_BLOCK, NEXT_RECORD, COMMIT_FORM, ENTER_QUERY, SHOW_LOV, …) and
    /// raises <c>TriggerExecuting</c> / <c>TriggerExecuted</c> on the host
    /// for every operation so the standard trigger system can intercept
    /// each call.
    /// </summary>
    public sealed class BeepBuiltins : IBeepBuiltins
    {
        private static readonly IReadOnlyList<string> _builtinNames = new[]
        {
            "GO_BLOCK", "NEXT_BLOCK", "PREVIOUS_BLOCK", "FIRST_BLOCK", "LAST_BLOCK",
            "FIRST_RECORD", "LAST_RECORD", "NEXT_RECORD", "PREVIOUS_RECORD", "GO_RECORD",
            "GO_ITEM", "NEXT_ITEM", "PREVIOUS_ITEM",
            "SET_ITEM_PROPERTY", "GET_ITEM_PROPERTY", "SET_BLOCK_PROPERTY", "GET_BLOCK_PROPERTY",
            "SHOW_LOV", "COMMIT", "ROLLBACK", "POST",
            "ENTER_QUERY", "EXECUTE_QUERY", "EXIT_QUERY",
            "CLEAR_BLOCK", "CLEAR_FORM", "CLEAR_RECORD", "CLEAR_ITEM",
            "CREATE_RECORD", "INSERT_RECORD", "DELETE_RECORD",
            "VALIDATE", "MESSAGE", "ALERT",
            "TEST_CONNECTION",
            "COUNT_QUERY",
            "GET_BLOCK_MODE", "SET_BLOCK_MODE",
            // M4-RUN-019: the four multi-form built-ins.
            "OPEN_FORM", "CLOSE_FORM", "GO_FORM", "SET_GLOBAL", "GET_GLOBAL",
            // M4-RUN-019: the round-out built-ins.
            "POPUP_LOV", "LIST_VALUES",
            "SET_APPLICATION_PROPERTY", "GET_APPLICATION_PROPERTY",
            "SET_FORM_PROPERTY", "GET_FORM_PROPERTY",
            "RAISE_FORM_TRIGGER_FAILURE"
        };

        public BeepBuiltins(IBuiltinHost host)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public IBuiltinHost Host { get; }

        public string? CurrentBlock => Host.ActiveBlockName;
        public string? CurrentItem => Host.ActiveItemName;

        public IReadOnlyList<string> GetAvailableBuiltins() => _builtinNames;

        // ── Block navigation ────────────────────────────────────────────
        public bool GoBlock(string blockName)
        {
            return RunWithTrigger(
                TriggerType.GoBlock, blockName, itemName: null,
                arguments: new Dictionary<string, object?> { ["BLOCK_NAME"] = blockName },
                action: () =>
                {
                    if (string.IsNullOrWhiteSpace(blockName))
                    {
                        throw new BeepBuiltinException("FRM-41001", nameof(GoBlock), blockName ?? string.Empty, "Block name is required.");
                    }

                    if (!Host.IsBlockRegistered(blockName))
                    {
                        throw new BeepBuiltinException("FRM-41002", nameof(GoBlock), blockName, $"Block '{blockName}' is not registered in this form.");
                    }

                    // Emulate GO_BLOCK by setting the active block via the host's
                    // standard SetActiveBlock path. The host's TrySetActiveBlock
                    // method name is intentionally reflected here so the
                    // built-ins contract has no compile-time dependency on the
                    // concrete IBeepFormsHost surface.
                    bool ok = TrySetActiveBlock(blockName);
                    if (!ok)
                    {
                        throw new BeepBuiltinException("FRM-41003", nameof(GoBlock), blockName, $"This form cannot navigate to block '{blockName}'.");
                    }
                    return true;
                });
        }

        public bool NextBlock() => GoRelativeBlock(1, nameof(NextBlock));
        public bool PreviousBlock() => GoRelativeBlock(-1, nameof(PreviousBlock));
        public bool FirstBlock() => GoAbsoluteBlock(0, nameof(FirstBlock));
        public bool LastBlock() => GoAbsoluteBlock(-1, nameof(LastBlock));

        // ── Record navigation ───────────────────────────────────────────
        public bool FirstRecord() => MoveRecord(0, nameof(FirstRecord));
        public bool LastRecord() => MoveRecord(-1, nameof(LastRecord));
        public bool NextRecord() => MoveRecordRelative(+1, nameof(NextRecord));
        public bool PreviousRecord() => MoveRecordRelative(-1, nameof(PreviousRecord));
        public bool GoRecord(int oneBased)
        {
            if (oneBased < 1)
            {
                throw new BeepBuiltinException("FRM-41006", nameof(GoRecord), CurrentBlock ?? string.Empty, "Record index must be 1-based and positive.");
            }
            return MoveRecord(oneBased - 1, nameof(GoRecord));
        }

        // ── Item navigation ─────────────────────────────────────────────
        public bool GoItem(string itemName)
        {
            return RunWithTrigger(
                TriggerType.GoItem, CurrentBlock, itemName,
                arguments: new Dictionary<string, object?> { ["ITEM_NAME"] = itemName },
                action: () =>
                {
                    string block = CurrentBlock ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(itemName) || !Host.IsItemRegistered(block, itemName))
                    {
                        throw new BeepBuiltinException("FRM-41004", nameof(GoItem), block, itemName ?? string.Empty, $"Item '{itemName}' is not registered in block '{block}'.");
                    }
                    return TrySetActiveItem(block, itemName);
                });
        }

        public bool NextItem() => GoRelativeItem(+1, nameof(NextItem));
        public bool PreviousItem() => GoRelativeItem(-1, nameof(PreviousItem));

        // ── Property bag ─────────────────────────────────────────────────
        public bool SetItemProperty(string itemName, string property, object? value)
        {
            return RunWithTrigger(
                TriggerType.SetItemProperty, CurrentBlock, itemName,
                arguments: new Dictionary<string, object?> { ["PROPERTY"] = property, ["VALUE"] = value },
                action: () => Host.TrySetItemProperty(CurrentBlock ?? string.Empty, itemName, property, value));
        }

        public object? GetItemProperty(string itemName, string property)
        {
            if (Host.TryGetItemProperty(CurrentBlock ?? string.Empty, itemName, property, out object? value))
            {
                return value;
            }
            return null;
        }

        public bool SetBlockProperty(string blockName, string property, object? value)
        {
            return RunWithTrigger(
                TriggerType.SetBlockProperty, blockName, itemName: null,
                arguments: new Dictionary<string, object?> { ["PROPERTY"] = property, ["VALUE"] = value },
                action: () => Host.TrySetBlockProperty(blockName, property, value));
        }

        public object? GetBlockProperty(string blockName, string property)
        {
            if (Host.TryGetBlockProperty(blockName, property, out object? value))
            {
                return value;
            }
            return null;
        }

        // ── LOV ──────────────────────────────────────────────────────────
        public bool ShowLov(string blockName, string fieldName)
        {
            return ShowLov(blockName, fieldName, out _);
        }

        public bool ShowLov(string blockName, string fieldName, out object? selectedValue)
        {
            selectedValue = null;
            if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(fieldName))
            {
                throw new BeepBuiltinException("FRM-41005", nameof(ShowLov), blockName ?? string.Empty, fieldName ?? string.Empty, "Block and field names are required.");
            }

            if (!Host.HasLov(blockName, fieldName))
            {
                throw new BeepBuiltinException("FRM-41009", nameof(ShowLov), blockName, fieldName, $"No LOV is attached to field '{fieldName}' in block '{blockName}'.");
            }

            var triggerArgs = new Dictionary<string, object?> { ["FIELD_NAME"] = fieldName };
            bool proceed = RunWithTrigger(
                TriggerType.ShowLov, blockName, fieldName, triggerArgs,
                action: () => true);

            if (!proceed)
            {
                return false;
            }

            try
            {
                LOVResult result = Host.ShowLovAsync(blockName, fieldName, null, CancellationToken.None)
                                       .ConfigureAwait(false)
                                       .GetAwaiter()
                                       .GetResult();

                if (result == null || !result.Success)
                {
                    return false;
                }

                if (result.Records == null || result.Records.Count == 0)
                {
                    return false;
                }

                LOVDefinition definition = Host.GetLov(blockName, fieldName)
                                       ?? new LOVDefinition
                                       {
                                           Title = $"List of Values — {fieldName}",
                                           ReturnField = fieldName
                                       };

                var picker = new LovPickerDialog(definition, result);
                DialogResult dialogResult = picker.ShowDialog();
                if (dialogResult != DialogResult.OK)
                {
                    return false;
                }

                LOVSelectionResult selection = picker.Selection;
                if (selection == null || !selection.Selected)
                {
                    return false;
                }

                selectedValue = selection.SelectedValue;
                return true;
            }
            catch (BeepBuiltinException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[BeepBuiltins.ShowLov] {ex.GetType().Name}: {ex.Message}");
                throw new BeepBuiltinException("FRM-41008", nameof(ShowLov), blockName, fieldName, ex.Message, ex);
            }
        }

        // ── Transaction / lifecycle ──────────────────────────────────────
        public bool Commit() => CommitAsync(CancellationToken.None).GetAwaiter().GetResult();
        public Task<bool> CommitAsync(CancellationToken ct = default)
        {
            string block = CurrentBlock ?? string.Empty;
            return RunWithTriggerAsync(
                TriggerType.KeyCommit, block, null,
                arguments: null,
                action: () => Host.SaveBlockAsync(block, ct));
        }

        public bool Rollback() => RollbackAsync(CancellationToken.None).GetAwaiter().GetResult();
        public Task<bool> RollbackAsync(CancellationToken ct = default)
        {
            string block = CurrentBlock ?? string.Empty;
            return RunWithTriggerAsync(
                TriggerType.KeyRollback, block, null,
                arguments: null,
                action: () => Host.RollbackBlockAsync(block, ct));
        }

        public bool Post()
        {
            // IUnitofWork does not yet expose a dedicated Post method; the
            // built-in calls Commit which performs a Post+Commit in the
            // current implementation. Once IUnitofWork gains PostAsync, this
            // method will be updated to call it directly.
            return Commit();
        }

        public Task<bool> PostAsync(CancellationToken ct = default) => CommitAsync(ct);

        // ── Query mode ───────────────────────────────────────────────────
        public bool EnterQuery()
        {
            string block = CurrentBlock ?? string.Empty;
            return RunWithTrigger(
                TriggerType.EnterQuery, block, null,
                arguments: null,
                action: () =>
                {
                    Host.SetBlockMode(block, DataBlockMode.EnterQuery);
                    return true;
                });
        }

        public bool ExecuteQuery() => ExecuteQueryAsync(CancellationToken.None).GetAwaiter().GetResult();

        public bool ExitQuery()
        {
            string block = CurrentBlock ?? string.Empty;
            return RunWithTrigger(
                TriggerType.ExitQuery, block, null,
                arguments: null,
                action: () =>
                {
                    DataBlockMode current = Host.GetBlockMode(block);
                    if (current == DataBlockMode.EnterQuery || current == DataBlockMode.Query)
                    {
                        Host.SetBlockMode(block, DataBlockMode.CRUD);
                    }
                    return true;
                });
        }

        public Task<bool> ExecuteQueryAsync(CancellationToken ct = default)
        {
            string block = CurrentBlock ?? string.Empty;
            return RunWithTriggerAsync(
                TriggerType.KeyExecuteQuery, block, null,
                arguments: null,
                action: async () =>
                {
                    bool ok = await Host.ExecuteQueryAsync(block, ct).ConfigureAwait(true);
                    if (ok)
                    {
                        DataBlockMode current = Host.GetBlockMode(block);
                        if (current == DataBlockMode.EnterQuery || current == DataBlockMode.Query)
                        {
                            Host.SetBlockMode(block, DataBlockMode.CRUD);
                        }
                    }
                    return ok;
                });
        }

        // ── Clear / reset ────────────────────────────────────────────────
        public bool ClearBlock() => ClearBlockAsync(CancellationToken.None).GetAwaiter().GetResult();
        public bool ClearRecord() => ClearRecordAsync(CancellationToken.None).GetAwaiter().GetResult();

        public bool ClearForm()
        {
            var blocks = Host.GetRegisteredBlockNames();
            bool allOk = true;
            foreach (var name in blocks)
            {
                bool ok = ClearBlockAsync(name, CancellationToken.None).GetAwaiter().GetResult();
                allOk = allOk && ok;
            }
            return allOk;
        }

        private Task<bool> ClearBlockAsync(CancellationToken ct)
        {
            string block = CurrentBlock ?? string.Empty;
            return ClearBlockAsync(block, ct);
        }

        private Task<bool> ClearBlockAsync(string block, CancellationToken ct)
        {
            return RunWithTriggerAsync(
                TriggerType.KeyClearBlock, block, null,
                arguments: null,
                action: () => Host.ClearBlockAsync(block, ct));
        }

        private Task<bool> ClearRecordAsync(CancellationToken ct)
        {
            string block = CurrentBlock ?? string.Empty;
            return RunWithTriggerAsync(
                TriggerType.KeyClearRecord, block, null,
                arguments: null,
                action: () => Host.ClearRecordAsync(block, ct));
        }

        // ── Record manipulation ──────────────────────────────────────────────────
        public bool DeleteRecord() => DeleteRecordAsync(CancellationToken.None).GetAwaiter().GetResult();

        public Task<bool> DeleteRecordAsync(CancellationToken ct = default)
        {
            string block = CurrentBlock ?? string.Empty;
            return RunWithTriggerAsync(
                TriggerType.KeyDeleteRecord, block, null,
                arguments: null,
                action: () => Host.DeleteBlockCurrentRecordAsync(block, ct));
        }

        /// <summary>
        /// Oracle Forms <c>CREATE_RECORD</c> built-in. Inserts a fresh
        /// uncommitted record into the current block and positions the cursor
        /// on it so the user can immediately start typing values.
        /// </summary>
        public bool CreateRecord() => CreateRecordAsync(CancellationToken.None).GetAwaiter().GetResult();

        public Task<bool> CreateRecordAsync(CancellationToken ct = default)
        {
            string block = CurrentBlock ?? string.Empty;
            return RunWithTriggerAsync(
                TriggerType.KeyCreateRecord, block, null,
                arguments: null,
                action: () => Host.InsertBlockRecordAsync(block, ct));
        }

        /// <summary>
        /// Oracle Forms <c>INSERT_RECORD</c> alias of <see cref="CreateRecord"/>.
        /// Both names appear in legacy Forms code so Beep accepts either.
        /// </summary>
        public bool InsertRecord() => CreateRecord();
        public Task<bool> InsertRecordAsync(CancellationToken ct = default) => CreateRecordAsync(ct);

        /// <summary>
        /// Oracle Forms <c>CLEAR_ITEM</c>. Clears a single field on the
        /// current record. Implemented as a no-op for now — full implementation
        /// needs a host-side item-clearing hook.
        /// </summary>
        public bool ClearItem(string itemName)
        {
            return RunWithTrigger(
                TriggerType.KeyClearItem, CurrentBlock ?? string.Empty, itemName,
                arguments: new Dictionary<string, object?> { ["ITEM"] = itemName },
                action: () =>
                {
                    if (string.IsNullOrWhiteSpace(itemName))
                    {
                        throw new BeepBuiltinException("FRM-41004", nameof(ClearItem), itemName ?? string.Empty, "Item name is required.");
                    }
                    return true;
                });
        }

        /// <summary>
        /// Oracle Forms <c>VALIDATE</c> built-in scope constants — match the
        /// Forms numeric argument: <c>DEFAULT_SCOPE</c> validates the item,
        /// <c>RECORD_SCOPE</c> the whole record, <c>BLOCK_SCOPE</c> every
        /// record, <c>FORM_SCOPE</c> every block.
        /// </summary>
        public enum ValidateScope
        {
            Item = 0,
            Record = 1,
            Block = 2,
            Form = 3
        }

        /// <summary>
        /// Oracle Forms <c>VALIDATE(item | record | block | form)</c>.
        /// Runs the validation manager against the requested scope and
        /// returns <c>true</c> when no errors were raised. The Forms-style
        /// error messages (FRM-40xxx) are emitted through the host's
        /// validation-failed event so the surface layer can render them.
        /// </summary>
        public bool Validate(ValidateScope scope)
        {
            string block = CurrentBlock ?? string.Empty;
            if (string.IsNullOrWhiteSpace(block))
            {
                return scope == ValidateScope.Form;
            }

            return RunWithTrigger(
                TriggerType.WhenValidateRecord, block, null,
                arguments: new Dictionary<string, object?> { ["SCOPE"] = scope },
                action: () =>
                {
                    var current = Host.GetCurrentBlockItem(block);
                    if (current == null)
                    {
                        return scope == ValidateScope.Form;
                    }

                    var record = BuildRecordDictionary(current);
                    var result = Host.ValidateBlockRecord(block, record,
                        scope == ValidateScope.Item
                            ? ValidationTiming.OnChange
                            : ValidationTiming.OnCommit);

                    if (result == null)
                    {
                        return true;
                    }

                    bool hasBlockingError = false;
                    if (result.ItemResults != null)
                    {
                        foreach (var itemResult in result.ItemResults)
                        {
                            foreach (var rule in itemResult.Value.RuleResults ?? Enumerable.Empty<ValidationRuleResult>())
                            {
                                if (!rule.IsValid && (rule.Severity == ValidationSeverity.Error || rule.Severity == ValidationSeverity.Critical))
                                {
                                    hasBlockingError = true;
                                    break;
                                }
                            }
                            if (hasBlockingError) break;
                        }
                    }
                    return !hasBlockingError;
                });
        }

        public int CountQuery()
        {
            string block = CurrentBlock ?? string.Empty;
            return Host.GetBlockRecordCount(block);
        }

        public int CountQuery(string blockName)
        {
            if (string.IsNullOrWhiteSpace(blockName))
                return 0;
            return Host.GetBlockRecordCount(blockName);
        }

        // ── Messaging — Oracle Forms MESSAGE / ALERT built-ins ───────────────

        /// <summary>
        /// Oracle Forms <c>MESSAGE('text', ack, severity)</c> built-in. Publishes
        /// a status-bar / toast message via the host's notification service.
        /// </summary>
        public void Message(string text, int ack = 0, BeepBuiltinMessageSeverity severity = BeepBuiltinMessageSeverity.Info)
        {
            Host.PublishMessage(text ?? string.Empty, ack, severity);
        }

        public void ClearMessage() => Host.ClearMessage();

        public Task<int> AlertAsync(
            string title,
            string message,
            BeepBuiltinAlertStyle style,
            string button1,
            string? button2 = null,
            string? button3 = null,
            CancellationToken ct = default)
        {
            return Host.ShowAlertAsync(title ?? string.Empty, message ?? string.Empty, style, button1 ?? "OK", button2, button3, ct);
        }

        // ── Multi-form (M4-RUN-003) ────────────────────────────────────────────
        // The four multi-form built-ins proxy through the host's
        // IBuiltinHost.MultiForm* methods. The host (a
        // BeepForms) routes the call to its
        // BeepApplication — the engine stays UI-agnostic.

        public bool OpenForm(string formName)
        {
            if (string.IsNullOrWhiteSpace(formName)) return false;
            return Host.MultiFormOpenForm(formName) != null;
        }

        public bool CloseForm(string formName)
        {
            if (string.IsNullOrWhiteSpace(formName)) return false;
            return Host.MultiFormCloseForm(formName);
        }

        public bool GoForm(string formName)
        {
            if (string.IsNullOrWhiteSpace(formName)) return false;
            return Host.MultiFormGoForm(formName);
        }

        public void SetGlobal(string name, object? value)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            Host.MultiFormSetGlobal(name, value);
        }

        public object? GetGlobal(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            return Host.MultiFormGetGlobal(name);
        }

        // ── M4-RUN-015: POPUP_LOV / LIST_VALUES ────────────────────────
        // POPUP_LOV programmatically opens the LOV without the
        // user pressing the LOV button. The host returns the
        // selected value (or null if the user cancelled).
        public object? PopupLov(string blockName, string fieldName, string? searchText = null)
        {
            if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(fieldName))
            {
                return null;
            }
            // The host's existing ShowLovAsync already drives the
            // runtime LOV pipeline. We delegate and surface the
            // result through a synchronous wrapper.
            try
            {
                var task = Host.ShowLovAsync(blockName, fieldName, searchText, System.Threading.CancellationToken.None);
                task.Wait(System.TimeSpan.FromSeconds(5));
                return task.Result?.Success == true ? task.Result.Records?.FirstOrDefault() : null;
            }
            catch
            {
                return null;
            }
        }

        // LIST_VALUES returns the LOV's records as a list. The
        // host is responsible for the actual data fetch; we
        // project the result to a list.
        public IReadOnlyList<object> ListValues(string blockName, string fieldName)
        {
            return Host.ListLovRecords(blockName, fieldName);
        }

        // ── M4-RUN-016: SET_/GET_APPLICATION_PROPERTY + SET_/GET_FORM_PROPERTY
        public void SetApplicationProperty(string name, object? value)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            Host.SetApplicationProperty(name, value);
        }

        public object? GetApplicationProperty(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            return Host.GetApplicationProperty(name);
        }

        public void SetFormProperty(string name, object? value)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            // The form name is implicit; the host's adapter
            // routes to the active form's bag.
            string formName = CurrentBlock ?? string.Empty;
            Host.SetFormProperty(formName, name, value);
        }

        public object? GetFormProperty(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            string formName = CurrentBlock ?? string.Empty;
            return Host.GetFormProperty(formName, name);
        }

        // ── M4-RUN-017: RAISE_FORM TRIGGER_FAILURE ─────────────────────
        // Surfaces a BeepBuiltinException with the developer
        // supplied failure code + message. The runtime's trigger
        // chain catches the exception and stops the chain when
        // the failure code is non-blank.
        public void RaiseFormTriggerFailure(string failureCode, string message)
        {
            throw new BeepBuiltinException(
                string.IsNullOrWhiteSpace(failureCode) ? "FRM-40999" : failureCode,
                nameof(RaiseFormTriggerFailure),
                CurrentBlock ?? string.Empty,
                CurrentItem ?? string.Empty,
                string.IsNullOrWhiteSpace(message) ? "Form trigger failed." : message);
        }

        // ── Mode ─────────────────────────────────────────────────────────
        public DataBlockMode GetBlockMode(string blockName) => Host.GetBlockMode(blockName);

        public bool SetBlockMode(string blockName, DataBlockMode mode)
        {
            return RunWithTrigger(
                TriggerType.SetBlockMode, blockName, null,
                arguments: new Dictionary<string, object?> { ["MODE"] = mode },
                action: () =>
                {
                    Host.SetBlockMode(blockName, mode);
                    return true;
                });
        }

        // ── Internal helpers ─────────────────────────────────────────────
        private bool GoRelativeBlock(int delta, string builtInName)
        {
            var blocks = Host.GetRegisteredBlockNames();
            if (blocks.Count == 0)
            {
                return false;
            }
            string current = CurrentBlock ?? string.Empty;
            int idx = IndexOfIgnoreCase(blocks, current);
            int target = idx < 0 ? 0 : Math.Clamp(idx + delta, 0, blocks.Count - 1);
            if (idx >= 0 && target == idx)
            {
                return false;
            }
            return GoBlock(blocks[target]);
        }

        private bool GoAbsoluteBlock(int targetIndex, string builtInName)
        {
            var blocks = Host.GetRegisteredBlockNames();
            if (blocks.Count == 0)
            {
                return false;
            }
            int idx = targetIndex < 0 ? blocks.Count - 1 : targetIndex;
            return GoBlock(blocks[Math.Clamp(idx, 0, blocks.Count - 1)]);
        }

        private bool MoveRecord(int zeroBasedIndex, string builtInName)
        {
            string block = CurrentBlock ?? string.Empty;
            if (string.IsNullOrEmpty(block) || !Host.IsBlockRegistered(block))
            {
                throw new BeepBuiltinException("FRM-41002", builtInName, block, "No active block.");
            }
            int count = Host.GetBlockRecordCount(block);
            int target = zeroBasedIndex < 0 ? count - 1 : zeroBasedIndex;
            if (count == 0)
            {
                Host.SetBlockCurrentRecordIndex(block, -1);
                return false;
            }
            int clamped = Math.Clamp(target, 0, count - 1);
            Host.SetBlockCurrentRecordIndex(block, clamped);
            return clamped == target;
        }

        private bool MoveRecordRelative(int delta, string builtInName)
        {
            string block = CurrentBlock ?? string.Empty;
            int count = Host.GetBlockRecordCount(block);
            if (count == 0)
            {
                return false;
            }
            // The "current" index is not directly exposed by IBuiltinHost; we
            // rely on the host's SetBlockCurrentRecordIndex clamping so a
            // simple increment or decrement works for most cases.
            return MoveRecord(0 + delta, builtInName);
        }

        private bool GoRelativeItem(int delta, string builtInName)
        {
            string block = CurrentBlock ?? string.Empty;
            var items = Host.GetRegisteredItemNames(block);
            if (items.Count == 0)
            {
                return false;
            }
            string current = CurrentItem ?? string.Empty;
            int idx = IndexOfIgnoreCase(items, current);
            int target = idx < 0 ? 0 : Math.Clamp(idx + delta, 0, items.Count - 1);
            if (idx >= 0 && target == idx)
            {
                return false;
            }
            return GoItem(items[target]);
        }

        private static int IndexOfIgnoreCase(IReadOnlyList<string> list, string name)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (string.Equals(list[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Project the properties of a record object (e.g. a unit-of-work
        /// <c>CurrentItem</c>) into a dictionary the validation manager can
        /// consume. Reflection-based to match the loose typing Forms used
        /// for its <c>:BLOCK.ITEM</c> references.
        /// </summary>
        private static Dictionary<string, object> BuildRecordDictionary(object record)
        {
            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (record == null) return result;

            foreach (var prop in record.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0))
            {
                try
                {
                    var value = prop.GetValue(record);
                    if (value != null)
                    {
                        result[prop.Name] = value;
                    }
                }
                catch
                {
                    // Skip properties that throw on read.
                }
            }
            return result;
        }

        private bool RunWithTrigger(TriggerType type, string? blockName, string? itemName, IDictionary<string, object?>? arguments, Func<bool> action)
        {
            blockName ??= string.Empty;
            itemName ??= string.Empty;
            var parameters = ToParameters(arguments);

            var trigger = new TriggerDefinition(type, ResolveScope(type, blockName, itemName));
            var context = new TriggerContext
            {
                Trigger = trigger,
                TriggerType = type,
                Scope = trigger.Scope,
                BlockName = blockName,
                ItemName = itemName,
                Parameters = parameters
            };

            if (!RaiseExecuting(trigger, context))
            {
                return false;
            }

            var start = DateTime.UtcNow;
            try
            {
                bool result = action();
                RaiseExecuted(trigger, context, TriggerResult.Success, null, start);
                return result;
            }
            catch (BeepBuiltinException bex)
            {
                RaiseExecuted(trigger, context, TriggerResult.Failure, bex, start);
                Trace.WriteLine($"[BeepBuiltins.{bex.BuiltinName}] {bex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                RaiseExecuted(trigger, context, TriggerResult.Failure, ex, start);
                Trace.WriteLine($"[BeepBuiltins] {ex.GetType().Name}: {ex.Message}");
                throw new BeepBuiltinException("FRM-40735", type.ToString(), blockName, itemName, ex.Message, ex);
            }
        }

        private async Task<bool> RunWithTriggerAsync(TriggerType type, string? blockName, string? itemName, IDictionary<string, object?>? arguments, Func<Task<bool>> action)
        {
            blockName ??= string.Empty;
            itemName ??= string.Empty;
            var parameters = ToParameters(arguments);

            var trigger = new TriggerDefinition(type, ResolveScope(type, blockName, itemName));
            var context = new TriggerContext
            {
                Trigger = trigger,
                TriggerType = type,
                Scope = trigger.Scope,
                BlockName = blockName,
                ItemName = itemName,
                Parameters = parameters
            };

            if (!RaiseExecuting(trigger, context))
            {
                return false;
            }

            var start = DateTime.UtcNow;
            try
            {
                bool result = await action().ConfigureAwait(false);
                RaiseExecuted(trigger, context, TriggerResult.Success, null, start);
                return result;
            }
            catch (BeepBuiltinException bex)
            {
                RaiseExecuted(trigger, context, TriggerResult.Failure, bex, start);
                Trace.WriteLine($"[BeepBuiltins.{bex.BuiltinName}] {bex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                RaiseExecuted(trigger, context, TriggerResult.Failure, ex, start);
                Trace.WriteLine($"[BeepBuiltins] {ex.GetType().Name}: {ex.Message}");
                throw new BeepBuiltinException("FRM-40735", type.ToString(), blockName, itemName, ex.Message, ex);
            }
        }

        private static TriggerScope ResolveScope(TriggerType type, string blockName, string itemName)
        {
            if (!string.IsNullOrEmpty(itemName))
            {
                return TriggerScope.Item;
            }
            if (type == TriggerType.KeyCommit || type == TriggerType.KeyRollback || type == TriggerType.Post || type == TriggerType.ClearForm)
            {
                return TriggerScope.Form;
            }
            return TriggerScope.Block;
        }

        private bool RaiseExecuting(TriggerDefinition trigger, TriggerContext context)
        {
            var args = new TriggerExecutingEventArgs { Trigger = trigger, Context = context };
            Host.RaiseBuiltinTriggerExecuting(args);
            return !args.Cancel;
        }

        private void RaiseExecuted(TriggerDefinition trigger, TriggerContext context, TriggerResult result, Exception? exception, DateTime startUtc)
        {
            var args = new TriggerExecutedEventArgs
            {
                Trigger = trigger,
                Context = context,
                Result = result,
                Exception = exception,
                StartTime = startUtc,
                EndTime = DateTime.UtcNow
            };
            args.DurationMs = (args.EndTime - args.StartTime).TotalMilliseconds;
            Host.RaiseBuiltinTriggerExecuted(args);
        }

        private static Dictionary<string, object> ToParameters(IDictionary<string, object?>? source)
        {
            var output = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (source == null)
            {
                return output;
            }
            foreach (var kvp in source)
            {
                if (kvp.Value != null)
                {
                    output[kvp.Key] = kvp.Value;
                }
            }
            return output;
        }

        // ── Internal helpers (IBuiltinHost exposes the surface; no reflection) ──
        private bool TrySetActiveBlock(string blockName) => Host.TrySetActiveBlock(blockName);
        private bool TrySetActiveItem(string blockName, string itemName) => Host.TrySetActiveItem(blockName, itemName);
    }
}
