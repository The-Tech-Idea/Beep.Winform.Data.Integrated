using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Forms.Builtins;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Builtins
{
    /// <summary>
    /// Adapter that exposes the WinForms <see cref="IBeepFormsHost"/> as the
    /// portable <see cref="IBuiltinHost"/> consumed by <see cref="BeepBuiltins"/>.
    /// The adapter is intentionally a thin pass-through — every method delegates
    /// to the host so the built-ins always see the live state of the form.
    /// </summary>
    public sealed class BeepBuiltinsHostAdapter : IBuiltinHost
    {
        private readonly IBeepFormsHost _host;

        public BeepBuiltinsHostAdapter(IBeepFormsHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public string? ActiveBlockName => _host.ActiveBlockName;

        public string? ActiveItemName => _host.ActiveItemName;

        public bool TrySetActiveBlock(string blockName) => _host.TrySetActiveBlock(blockName);

        public bool TrySetActiveItem(string blockName, string itemName)
            => _host.TrySetActiveItem(blockName, itemName);

        public void RaiseBuiltinTriggerExecuting(TriggerExecutingEventArgs args)
        {
            // BeepForms owns the TriggerExecuting event; cast to the concrete
            // type so we can invoke it. Hosts that don't derive from BeepForms
            // (for example test doubles) simply skip the fan-out.
            if (_host is BeepForms beepForms)
            {
                beepForms.RaiseBuiltinTriggerExecuting(args);
            }
        }

        public void RaiseBuiltinTriggerExecuted(TriggerExecutedEventArgs args)
        {
            if (_host is BeepForms beepForms)
            {
                beepForms.RaiseBuiltinTriggerExecuted(args);
            }
        }

        public bool IsBlockRegistered(string blockName) => _host.IsBlockRegistered(blockName);

        public bool IsItemRegistered(string blockName, string itemName)
            => _host.IsItemRegistered(blockName, itemName);

        public IReadOnlyList<string> GetRegisteredBlockNames() => _host.GetRegisteredBlockNames();

        public IReadOnlyList<string> GetRegisteredItemNames(string blockName)
            => _host.GetRegisteredItemNames(blockName);

        public DataBlockInfo? GetBlockInfo(string blockName) => _host.GetBlockInfo(blockName);

        public IUnitofWork? GetBlockUnitOfWork(string blockName) => _host.GetBlockUnitOfWork(blockName);

        public object? GetCurrentBlockItem(string blockName) => _host.GetCurrentBlockItem(blockName);

        public bool IsBlockQueryAllowed(string blockName) => _host.IsBlockQueryAllowed(blockName);

        public void SetBlockCurrentRecordIndex(string blockName, int index)
            => _host.SetBlockCurrentRecordIndex(blockName, index);

        public int GetBlockRecordCount(string blockName) => _host.GetBlockRecordCount(blockName);

        public DataBlockMode GetBlockMode(string blockName) => _host.GetBlockMode(blockName);

        public void SetBlockMode(string blockName, DataBlockMode mode) => _host.SetBlockMode(blockName, mode);

        public bool HasLov(string blockName, string fieldName) => _host.HasLov(blockName, fieldName);

        public LOVDefinition? GetLov(string blockName, string fieldName) => _host.GetLov(blockName, fieldName);

        public Task<LOVResult> ShowLovAsync(string blockName, string fieldName, string? searchText, CancellationToken ct)
            => _host.ShowLovAsync(blockName, fieldName, searchText, ct);

        public bool TryGetItemProperty(string blockName, string itemName, string property, out object? value)
            => _host.TryGetItemProperty(blockName, itemName, property, out value);

        public bool TrySetItemProperty(string blockName, string itemName, string property, object? value)
            => _host.TrySetItemProperty(blockName, itemName, property, value);

        public bool TryGetBlockProperty(string blockName, string property, out object? value)
            => _host.TryGetBlockProperty(blockName, property, out value);

        public bool TrySetBlockProperty(string blockName, string property, object? value)
            => _host.TrySetBlockProperty(blockName, property, value);

        public Task<bool> SaveBlockAsync(string blockName, CancellationToken ct) => _host.SaveBlockAsync(blockName);
        public Task<bool> RollbackBlockAsync(string blockName, CancellationToken ct) => _host.RollbackBlockAsync(blockName);
        public Task<bool> InsertBlockRecordAsync(string blockName, CancellationToken ct) => _host.InsertBlockRecordAsync(blockName);
        public Task<bool> DeleteBlockCurrentRecordAsync(string blockName, CancellationToken ct) => _host.DeleteBlockCurrentRecordAsync(blockName);
        public Task<bool> ExecuteQueryAsync(string blockName, CancellationToken ct) => _host.ExecuteQueryAsync(blockName, ct);
        public Task<bool> ClearBlockAsync(string blockName, CancellationToken ct) => _host.ClearBlockAsync(blockName, ct);
        public Task<bool> ClearRecordAsync(string blockName, CancellationToken ct) => _host.ClearRecordAsync(blockName, ct);

        public RecordValidationResult? ValidateBlockRecord(string blockName, IDictionary<string, object> record, ValidationTiming timing)
            => _host.ValidateBlockRecord(blockName, record, timing);

        // ── Messaging (Oracle Forms MESSAGE / ALERT) ──────────────────────

        public void PublishMessage(string message, int messageLevel, BeepBuiltinMessageSeverity severity)
        {
            if (_host is BeepForms beepForms)
            {
                beepForms.PublishBuiltinMessage(message, messageLevel, severity);
                return;
            }

            // Fallback: write to debug so tests / non-WinForms hosts still get
            // visibility into the message stream.
            System.Diagnostics.Debug.WriteLine($"[MESSAGE{severity}] {message}");
        }

        public void ClearMessage()
        {
            if (_host is BeepForms beepForms)
            {
                beepForms.ClearBuiltinMessage();
            }
        }

        public Task<int> ShowAlertAsync(string title, string message, BeepBuiltinAlertStyle style, string button1, string? button2, string? button3, CancellationToken ct)
        {
            if (_host is BeepForms beepForms)
            {
                return beepForms.ShowBuiltinAlertAsync(title, message, style, button1, button2, button3, ct);
            }
            return Task.FromResult(1);
        }

        // ── Multi-form (M4-RUN-003) ────────────────────────────────────────────
        // The four multi-form built-ins are routed through the
        // host's Application property. The host is expected to be
        // a BeepForms; the adapter is no-op when the host is a
        // test double. The actual orchestration (opening / closing
        // forms, managing :GLOBAL variables) is the
        // BeepApplication's responsibility.

        public object? MultiFormOpenForm(string formName)
        {
            if (string.IsNullOrWhiteSpace(formName)) return null;
            var app = (_host as BeepForms)?.Application;
            if (app == null) return null;
            var form = app.OpenForm(formName);
            return form?.Name;
        }

        public bool MultiFormCloseForm(string formName)
        {
            if (string.IsNullOrWhiteSpace(formName)) return false;
            var app = (_host as BeepForms)?.Application;
            if (app == null) return false;
            return app.CloseForm(formName);
        }

        public bool MultiFormGoForm(string formName)
        {
            if (string.IsNullOrWhiteSpace(formName)) return false;
            var app = (_host as BeepForms)?.Application;
            if (app == null) return false;
            return app.GoForm(formName);
        }

        public void MultiFormSetGlobal(string name, object? value)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            var app = (_host as BeepForms)?.Application;
            app?.SetGlobal(name, value);
        }

        public object? MultiFormGetGlobal(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var app = (_host as BeepForms)?.Application;
            return app?.GetGlobal(name);
        }

        // ── Application / Form property bag (M4-RUN-016) ────────────
        public void SetApplicationProperty(string name, object? value)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            var app = (_host as BeepForms)?.Application;
            app?.SetGlobal(name, value);
        }

        public object? GetApplicationProperty(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var app = (_host as BeepForms)?.Application;
            return app?.GetGlobal(name);
        }

        public void SetFormProperty(string formName, string name, object? value)
        {
            // Per-form property bag is not yet implemented on
            // BeepApplication. The application-level property bag
            // is the practical M4 surface; per-form routing
            // follows in M4 polish.
            if (string.IsNullOrWhiteSpace(name)) return;
            var app = (_host as BeepForms)?.Application;
            app?.SetGlobal($"{formName}.{name}", value);
        }

        public object? GetFormProperty(string formName, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var app = (_host as BeepForms)?.Application;
            return app?.GetGlobal($"{formName}.{name}");
        }

        // ── LIST_VALUES (M4-RUN-015) ─────────────────────────────────
        public System.Collections.Generic.IReadOnlyList<object> ListLovRecords(
            string blockName, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(fieldName))
            {
                return System.Array.Empty<object>();
            }
            if (!_host.IsBlockRegistered(blockName))
            {
                return System.Array.Empty<object>();
            }
            // The host's GetLov is the typed accessor; the
            // engine surface has HasLov / GetLov through
            // IBuiltinHost. We project the LOV's records through
            // a synchronous show — the production runtime uses
            // the async path; the playground calls this one.
            try
            {
                var task = System.Threading.Tasks.Task.Run(async () =>
                {
                    var lov = _host.GetLov(blockName, fieldName);
                    if (lov == null) return System.Array.Empty<object>();
                    var result = await _host.ShowLovAsync(blockName, fieldName, null, System.Threading.CancellationToken.None);
                    return (System.Collections.Generic.IReadOnlyList<object>)(result?.Records ?? new System.Collections.Generic.List<object>());
                });
                if (task.Wait(System.TimeSpan.FromSeconds(5)))
                {
                    return task.Result;
                }
            }
            catch
            {
                // Best effort.
            }
            return System.Array.Empty<object>();
        }
    }
}
