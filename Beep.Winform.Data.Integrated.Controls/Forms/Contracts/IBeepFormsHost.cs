using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Forms.Builtins;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Logon;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts
{
    /// <summary>
    /// Host interface exposed to <see cref="BeepBlock"/> controls.
    /// Hierarchy: BeepBlock → IBeepFormsHost (BeepForms) → IUnitofWorksManager (FormsManager) → IDataSource.
    /// BeepBlock must ONLY call methods on IBeepFormsHost — never dereference FormsManager directly.
    /// </summary>
    public interface IBeepFormsHost
    {
        // ── Identity / state ──────────────────────────────────────────────────────────
        string FormName { get; set; }
        string? ActiveBlockName { get; }
        string? ActiveItemName { get; }

        /// <summary>
        /// M4-RUN-005: the multi-form application that owns
        /// this form. The host sets the property when it is
        /// opened through <see cref="BeepApplication.OpenForm"/>;
        /// test doubles return <c>null</c>. The built-ins
        /// proxy multi-form calls (OpenForm, CloseForm, GoForm,
        /// SetGlobal, GetGlobal) through this property.
        /// </summary>
        BeepApplication? Application { get; set; }

        /// <summary>
        /// The underlying FormsManager.  Exposed for external wiring (designer, bootstrapper).
        /// BeepBlock must NOT call this property; use the proxy methods below instead.
        /// </summary>
        IUnitofWorksManager? FormsManager { get; set; }

        BeepFormsDefinition? Definition { get; set; }
        BeepViewState ViewState { get; }
        IReadOnlyList<IBeepBlockView> Blocks { get; }

        // ── Built-ins ─────────────────────────────────────────────────────────────────
        /// <summary>
        /// Oracle Forms-compatible built-in procedures (GO_BLOCK, NEXT_RECORD, COMMIT, …).
        /// Always non-null after <see cref="InitializeAsync"/>; null during the design-time
        /// load window.
        /// </summary>
        IBeepBuiltins? Builtins { get; }

        // ── Events ────────────────────────────────────────────────────────────────────
        event EventHandler? ActiveBlockChanged;
        event EventHandler? FormsManagerChanged;
        event EventHandler? ViewStateChanged;

        // Phase 7D — raised by InitializeAsync when all blocks are bootstrapped
        event EventHandler<BeepBootstrapEventArgs>? BootstrapCompleted;

        // Manager-owned trigger and UoW activity proxies.
        event EventHandler<TriggerExecutingEventArgs>? TriggerExecuting;
        event EventHandler<TriggerExecutedEventArgs>? TriggerExecuted;
        event EventHandler<TriggerRegisteredEventArgs>? TriggerRegistered;
        event EventHandler<TriggerUnregisteredEventArgs>? TriggerUnregistered;
        event EventHandler<TriggerChainCompletedEventArgs>? TriggerChainCompleted;
        event EventHandler<BeepUnitOfWorkEventArgs>? BlockUnitOfWorkActivity;

        // ── Registration / routing ────────────────────────────────────────────────────
        bool RegisterBlock(IBeepBlockView blockView);
        bool UnregisterBlock(string blockName);
        bool TrySetActiveBlock(string blockName);
        bool TrySetActiveItem(string blockName, string itemName);
        void SyncFromManager();

        /// <summary>
        /// Phase 7D — Async bootstrap that calls <see cref="IUnitofWorksManager.SetupBlockAsync"/>
        /// for every block in <see cref="Definition"/>. Updates <see cref="BeepViewState.BootstrapState"/>
        /// and raises <see cref="BootstrapCompleted"/> when finished.
        /// Should be called automatically when both FormsManager and Definition are set.
        /// </summary>
        Task<bool> InitializeAsync(CancellationToken cancellationToken = default);

        // ── Block / UoW query proxies (all FormsManager calls BeepBlock needs) ────────

        /// <summary>Returns true when the block is registered in FormsManager.</summary>
        bool IsBlockRegistered(string blockName);

        /// <summary>Returns true when the item/field is registered in the block.</summary>
        bool IsItemRegistered(string blockName, string itemName);

        /// <summary>Returns the names of all registered blocks in registration order.</summary>
        IReadOnlyList<string> GetRegisteredBlockNames();

        /// <summary>Returns the names of all registered items/fields in the block.</summary>
        IReadOnlyList<string> GetRegisteredItemNames(string blockName);

        /// <summary>Returns the DataBlockInfo for the block, or null if unregistered.</summary>
        DataBlockInfo? GetBlockInfo(string blockName);

        /// <summary>Returns the live IUnitofWork for the block.</summary>
        IUnitofWork? GetBlockUnitOfWork(string blockName);

        /// <summary>Returns the current record item for the block.</summary>
        object? GetCurrentBlockItem(string blockName);

        /// <summary>Returns the detail block names for a master block.</summary>
        IEnumerable<string> GetDetailBlockNames(string blockName);

        /// <summary>Moves the current-record cursor in FormsManager.</summary>
        void SetBlockCurrentRecordIndex(string blockName, int index);

        /// <summary>Returns the current record count for the block (0 when empty).</summary>
        int GetBlockRecordCount(string blockName);

        /// <summary>Returns the current <see cref="DataBlockMode"/> for the block.</summary>
        DataBlockMode GetBlockMode(string blockName);

        /// <summary>Updates the <see cref="DataBlockMode"/> for the block.</summary>
        void SetBlockMode(string blockName, DataBlockMode mode);

        /// <summary>Returns whether query mode is allowed for the block.</summary>
        bool IsBlockQueryAllowed(string blockName);

        /// <summary>Returns aggregate trigger statistics for the block.</summary>
        TriggerStatisticsInfo? GetTriggerStatistics(string blockName);

        /// <summary>Returns form-scope triggers available to the block.</summary>
        IReadOnlyList<TriggerDefinition> GetFormLevelTriggers(string blockName);

        /// <summary>Returns block-scope triggers available to the block.</summary>
        IReadOnlyList<TriggerDefinition> GetBlockLevelTriggers(string blockName);

        /// <summary>Returns record-scope triggers available to the block.</summary>
        IReadOnlyList<TriggerDefinition> GetRecordLevelTriggers(string blockName);

        /// <summary>Returns item-scope triggers available to the block.</summary>
        IReadOnlyList<TriggerDefinition> GetItemLevelTriggers(string blockName);

        /// <summary>Returns connection names available from DMEEditor's config.</summary>
        IEnumerable<string> GetAvailableConnectionNames();

        // ── LOV proxies ───────────────────────────────────────────────────────────────
        bool HasLov(string blockName, string fieldName);
        LOVDefinition? GetLov(string blockName, string fieldName);
        Task<LOVResult> LoadLovDataAsync(string blockName, string fieldName, string? searchText = null);
        Task<LOVValidationResult> ValidateLovValueAsync(string blockName, string fieldName, object? value);
        Dictionary<string, object>? GetLovRelatedFieldValues(LOVDefinition lov, object? selectedItem);

        /// <summary>Loads and returns LOV data for a field (ShowLOV path in FormsManager).</summary>
        Task<LOVResult> ShowLovAsync(string blockName, string fieldName, string? searchText = null, CancellationToken ct = default);

        // ── Validation proxy ──────────────────────────────────────────────────────────
        RecordValidationResult? ValidateBlockRecord(string blockName, IDictionary<string, object> record, ValidationTiming timing);

        // ── Item-properties proxy ─────────────────────────────────────────────────────
        bool IsFieldQueryAllowed(string blockName, string fieldName);

        /// <summary>Returns the value of a named item property, or false if not found.</summary>
        bool TryGetItemProperty(string blockName, string itemName, string property, out object? value);

        /// <summary>Sets the value of a named item property. Returns true on success.</summary>
        bool TrySetItemProperty(string blockName, string itemName, string property, object? value);

        /// <summary>Returns the value of a named block property, or false if not found.</summary>
        bool TryGetBlockProperty(string blockName, string property, out object? value);

        /// <summary>Sets the value of a named block property. Returns true on success.</summary>
        bool TrySetBlockProperty(string blockName, string property, object? value);

        // ── Mutation proxies (used by BeepBlock when host is not a concrete BeepForms) ─
        Task<bool> SaveBlockAsync(string blockName, CancellationToken ct = default);
        Task<bool> RollbackBlockAsync(string blockName, CancellationToken ct = default);
        Task<bool> InsertBlockRecordAsync(string blockName, CancellationToken ct = default);
        Task<bool> DeleteBlockCurrentRecordAsync(string blockName, CancellationToken ct = default);
        Task<bool> ExecuteQueryAsync(string blockName, CancellationToken ct = default);
        Task<bool> ClearBlockAsync(string blockName, CancellationToken ct = default);
        Task<bool> ClearRecordAsync(string blockName, CancellationToken ct = default);

        // ── Logon flow ───────────────────────────────────────────────────────────
        /// <summary>Shows the configured logon dialog and, on success, raises the When-New-Form-Instance trigger.</summary>
        Task<BeepLogonContext> LogonAsync(BeepLogonRequest request);

        // ── Post / Alert ──────────────────────────────────────────────────────────
        /// <summary>Post (validate and send to DB without committing). Oracle Forms POST equivalent.</summary>
        Task<bool> PostBlockAsync(string blockName, CancellationToken ct = default);
        /// <summary>Show a modal alert dialog. Returns 1-based button index.</summary>
        Task<int> ShowAlertAsync(string title, string message, BeepBuiltinAlertStyle style,
            string button1Text, string? button2Text = null, string? button3Text = null, CancellationToken ct = default);
    }
}