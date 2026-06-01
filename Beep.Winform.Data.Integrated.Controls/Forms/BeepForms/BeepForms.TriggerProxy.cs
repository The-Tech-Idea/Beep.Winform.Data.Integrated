using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    public partial class BeepForms
    {
        private readonly Dictionary<string, UnitOfWorkSubscription> _unitOfWorkSubscriptions = new(StringComparer.OrdinalIgnoreCase);

        public event EventHandler<TriggerExecutingEventArgs>? TriggerExecuting;
        public event EventHandler<TriggerExecutedEventArgs>? TriggerExecuted;
        public event EventHandler<TriggerRegisteredEventArgs>? TriggerRegistered;
        public event EventHandler<TriggerUnregisteredEventArgs>? TriggerUnregistered;
        public event EventHandler<TriggerChainCompletedEventArgs>? TriggerChainCompleted;
        public event EventHandler<BeepFormsUnitOfWorkEventArgs>? BlockUnitOfWorkActivity;

        private sealed class UnitOfWorkSubscription
        {
            public IUnitofWork UnitOfWork { get; init; } = default!;
            public EventHandler? CurrentChanged { get; init; }
            public EventHandler<ItemChangedEventArgs<Entity>>? ItemChanged { get; init; }
            public EventHandler<UnitofWorkParams>? PreCreate { get; init; }
            public EventHandler<UnitofWorkParams>? PostCreate { get; init; }
            public EventHandler<UnitofWorkParams>? PreQuery { get; init; }
            public EventHandler<UnitofWorkParams>? PostQuery { get; init; }
            public EventHandler<UnitofWorkParams>? PreInsert { get; init; }
            public EventHandler<UnitofWorkParams>? PostInsert { get; init; }
            public EventHandler<UnitofWorkParams>? PreUpdate { get; init; }
            public EventHandler<UnitofWorkParams>? PostUpdate { get; init; }
            public EventHandler<UnitofWorkParams>? PostEdit { get; init; }
            public EventHandler<UnitofWorkParams>? PreDelete { get; init; }
            public EventHandler<UnitofWorkParams>? PostDelete { get; init; }
            public EventHandler<UnitofWorkParams>? PreCommit { get; init; }
            public EventHandler<UnitofWorkParams>? PostCommit { get; init; }
        }

        private void AttachTriggerProxy(IUnitofWorksManager formsManager)
        {
            formsManager.Triggers.TriggerExecuting += HandleManagerTriggerExecuting;
            formsManager.Triggers.TriggerExecuted += HandleManagerTriggerExecuted;
            formsManager.Triggers.TriggerRegistered += HandleManagerTriggerRegistered;
            formsManager.Triggers.TriggerUnregistered += HandleManagerTriggerUnregistered;
            formsManager.Triggers.TriggerChainCompleted += HandleManagerTriggerChainCompleted;
            RefreshUnitOfWorkEventSubscriptions();
        }

        private void DetachTriggerProxy(IUnitofWorksManager formsManager)
        {
            ClearUnitOfWorkEventSubscriptions();
            formsManager.Triggers.TriggerExecuting -= HandleManagerTriggerExecuting;
            formsManager.Triggers.TriggerExecuted -= HandleManagerTriggerExecuted;
            formsManager.Triggers.TriggerRegistered -= HandleManagerTriggerRegistered;
            formsManager.Triggers.TriggerUnregistered -= HandleManagerTriggerUnregistered;
            formsManager.Triggers.TriggerChainCompleted -= HandleManagerTriggerChainCompleted;
        }

        private void RefreshUnitOfWorkEventSubscriptions()
        {
            if (_formsManager == null)
            {
                ClearUnitOfWorkEventSubscriptions();
                return;
            }

            var desiredBlockNames = new HashSet<string>(
                _blocks.Select(block => block.ManagerBlockName)
                    .Where(blockName => !string.IsNullOrWhiteSpace(blockName)),
                StringComparer.OrdinalIgnoreCase);

            foreach (var existingBlockName in _unitOfWorkSubscriptions.Keys.ToArray())
            {
                if (!desiredBlockNames.Contains(existingBlockName) || !_formsManager.BlockExists(existingBlockName))
                {
                    RemoveUnitOfWorkSubscription(existingBlockName);
                }
            }

            foreach (var blockName in desiredBlockNames)
            {
                if (!_formsManager.BlockExists(blockName))
                {
                    RemoveUnitOfWorkSubscription(blockName);
                    continue;
                }

                var unitOfWork = _formsManager.GetUnitOfWork(blockName);
                if (unitOfWork == null)
                {
                    RemoveUnitOfWorkSubscription(blockName);
                    continue;
                }

                if (_unitOfWorkSubscriptions.TryGetValue(blockName, out var existing) && ReferenceEquals(existing.UnitOfWork, unitOfWork))
                {
                    continue;
                }

                RemoveUnitOfWorkSubscription(blockName);
                _unitOfWorkSubscriptions[blockName] = CreateUnitOfWorkSubscription(blockName, unitOfWork);
            }
        }

        private void ClearUnitOfWorkEventSubscriptions()
        {
            foreach (var blockName in _unitOfWorkSubscriptions.Keys.ToArray())
            {
                RemoveUnitOfWorkSubscription(blockName);
            }
        }

        private UnitOfWorkSubscription CreateUnitOfWorkSubscription(string blockName, IUnitofWork unitOfWork)
        {
            var subscription = new UnitOfWorkSubscription
            {
                UnitOfWork = unitOfWork,
                CurrentChanged = (_, _) => RaiseBlockUnitOfWorkActivity(new BeepFormsUnitOfWorkEventArgs
                {
                    BlockName = blockName,
                    EventKind = BeepFormsUnitOfWorkEventKind.CurrentChanged,
                    UnitOfWork = unitOfWork,
                    CurrentItem = unitOfWork.CurrentItem
                }),
                ItemChanged = (_, e) => RaiseBlockUnitOfWorkActivity(new BeepFormsUnitOfWorkEventArgs
                {
                    BlockName = blockName,
                    EventKind = BeepFormsUnitOfWorkEventKind.ItemChanged,
                    UnitOfWork = unitOfWork,
                    Item = e.Item,
                    PropertyName = e.PropertyName,
                    CurrentItem = unitOfWork.CurrentItem
                }),
                PreCreate = (_, e) => RaiseBlockUnitOfWorkActivity(CreateUnitOfWorkEventArgs(blockName, unitOfWork, e, BeepFormsUnitOfWorkEventKind.PreCreate)),
                PostCreate = (_, e) => RaiseBlockUnitOfWorkActivity(CreateUnitOfWorkEventArgs(blockName, unitOfWork, e, BeepFormsUnitOfWorkEventKind.PostCreate)),
                PreQuery = (_, e) => RaiseBlockUnitOfWorkActivity(CreateUnitOfWorkEventArgs(blockName, unitOfWork, e, BeepFormsUnitOfWorkEventKind.PreQuery)),
                PostQuery = (_, e) => RaiseBlockUnitOfWorkActivity(CreateUnitOfWorkEventArgs(blockName, unitOfWork, e, BeepFormsUnitOfWorkEventKind.PostQuery)),
                PreInsert = (_, e) => RaiseBlockUnitOfWorkActivity(CreateUnitOfWorkEventArgs(blockName, unitOfWork, e, BeepFormsUnitOfWorkEventKind.PreInsert)),
                PostInsert = (_, e) => RaiseBlockUnitOfWorkActivity(CreateUnitOfWorkEventArgs(blockName, unitOfWork, e, BeepFormsUnitOfWorkEventKind.PostInsert)),
                PreUpdate = (_, e) => RaiseBlockUnitOfWorkActivity(CreateUnitOfWorkEventArgs(blockName, unitOfWork, e, BeepFormsUnitOfWorkEventKind.PreUpdate)),
                PostUpdate = (_, e) => RaiseBlockUnitOfWorkActivity(CreateUnitOfWorkEventArgs(blockName, unitOfWork, e, BeepFormsUnitOfWorkEventKind.PostUpdate)),
                PostEdit = (_, e) => RaiseBlockUnitOfWorkActivity(CreateUnitOfWorkEventArgs(blockName, unitOfWork, e, BeepFormsUnitOfWorkEventKind.PostEdit)),
                PreDelete = (_, e) => RaiseBlockUnitOfWorkActivity(CreateUnitOfWorkEventArgs(blockName, unitOfWork, e, BeepFormsUnitOfWorkEventKind.PreDelete)),
                PostDelete = (_, e) => RaiseBlockUnitOfWorkActivity(CreateUnitOfWorkEventArgs(blockName, unitOfWork, e, BeepFormsUnitOfWorkEventKind.PostDelete)),
                PreCommit = (_, e) => RaiseBlockUnitOfWorkActivity(CreateUnitOfWorkEventArgs(blockName, unitOfWork, e, BeepFormsUnitOfWorkEventKind.PreCommit)),
                PostCommit = (_, e) => RaiseBlockUnitOfWorkActivity(CreateUnitOfWorkEventArgs(blockName, unitOfWork, e, BeepFormsUnitOfWorkEventKind.PostCommit))
            };

            unitOfWork.CurrentChanged += subscription.CurrentChanged;
            unitOfWork.ItemChanged += subscription.ItemChanged;
            unitOfWork.PreCreate += subscription.PreCreate;
            unitOfWork.PostCreate += subscription.PostCreate;
            unitOfWork.PreQuery += subscription.PreQuery;
            unitOfWork.PostQuery += subscription.PostQuery;
            unitOfWork.PreInsert += subscription.PreInsert;
            unitOfWork.PostInsert += subscription.PostInsert;
            unitOfWork.PreUpdate += subscription.PreUpdate;
            unitOfWork.PostUpdate += subscription.PostUpdate;
            unitOfWork.PostEdit += subscription.PostEdit;
            unitOfWork.PreDelete += subscription.PreDelete;
            unitOfWork.PostDelete += subscription.PostDelete;
            unitOfWork.PreCommit += subscription.PreCommit;
            unitOfWork.PostCommit += subscription.PostCommit;

            return subscription;
        }

        private void RemoveUnitOfWorkSubscription(string blockName)
        {
            if (!_unitOfWorkSubscriptions.TryGetValue(blockName, out var subscription))
            {
                return;
            }

            subscription.UnitOfWork.CurrentChanged -= subscription.CurrentChanged;
            subscription.UnitOfWork.ItemChanged -= subscription.ItemChanged;
            subscription.UnitOfWork.PreCreate -= subscription.PreCreate;
            subscription.UnitOfWork.PostCreate -= subscription.PostCreate;
            subscription.UnitOfWork.PreQuery -= subscription.PreQuery;
            subscription.UnitOfWork.PostQuery -= subscription.PostQuery;
            subscription.UnitOfWork.PreInsert -= subscription.PreInsert;
            subscription.UnitOfWork.PostInsert -= subscription.PostInsert;
            subscription.UnitOfWork.PreUpdate -= subscription.PreUpdate;
            subscription.UnitOfWork.PostUpdate -= subscription.PostUpdate;
            subscription.UnitOfWork.PostEdit -= subscription.PostEdit;
            subscription.UnitOfWork.PreDelete -= subscription.PreDelete;
            subscription.UnitOfWork.PostDelete -= subscription.PostDelete;
            subscription.UnitOfWork.PreCommit -= subscription.PreCommit;
            subscription.UnitOfWork.PostCommit -= subscription.PostCommit;

            _unitOfWorkSubscriptions.Remove(blockName);
        }

        private BeepFormsUnitOfWorkEventArgs CreateUnitOfWorkEventArgs(
            string blockName,
            IUnitofWork unitOfWork,
            UnitofWorkParams? parameters,
            BeepFormsUnitOfWorkEventKind fallbackKind)
        {
            return new BeepFormsUnitOfWorkEventArgs
            {
                BlockName = blockName,
                EventKind = ResolveUnitOfWorkEventKind(parameters, fallbackKind),
                UnitOfWork = unitOfWork,
                Parameters = parameters,
                Item = parameters?.Record,
                PropertyName = parameters?.PropertyName,
                CurrentItem = unitOfWork.CurrentItem
            };
        }

        private BeepFormsUnitOfWorkEventKind ResolveUnitOfWorkEventKind(UnitofWorkParams? parameters, BeepFormsUnitOfWorkEventKind fallbackKind)
        {
            if (parameters == null)
            {
                return fallbackKind;
            }

            return parameters.EventAction switch
            {
                EventAction.PreCreate => BeepFormsUnitOfWorkEventKind.PreCreate,
                EventAction.PostCreate => BeepFormsUnitOfWorkEventKind.PostCreate,
                EventAction.PreQuery => BeepFormsUnitOfWorkEventKind.PreQuery,
                EventAction.PostQuery => BeepFormsUnitOfWorkEventKind.PostQuery,
                EventAction.PreInsert => BeepFormsUnitOfWorkEventKind.PreInsert,
                EventAction.PostInsert => BeepFormsUnitOfWorkEventKind.PostInsert,
                EventAction.PreUpdate => BeepFormsUnitOfWorkEventKind.PreUpdate,
                EventAction.PostUpdate => BeepFormsUnitOfWorkEventKind.PostUpdate,
                EventAction.PostEdit => BeepFormsUnitOfWorkEventKind.PostEdit,
                EventAction.PreDelete => BeepFormsUnitOfWorkEventKind.PreDelete,
                EventAction.PostDelete => BeepFormsUnitOfWorkEventKind.PostDelete,
                EventAction.PreCommit => BeepFormsUnitOfWorkEventKind.PreCommit,
                EventAction.PostCommit => BeepFormsUnitOfWorkEventKind.PostCommit,
                _ => fallbackKind
            };
        }

        private void RaiseBlockUnitOfWorkActivity(BeepFormsUnitOfWorkEventArgs eventArgs)
        {
            RunOnUiThread(() =>
            {
                if (ShouldSyncFromUnitOfWorkActivity(eventArgs.EventKind) && !string.IsNullOrWhiteSpace(eventArgs.BlockName))
                {
                    SyncBlockView(eventArgs.BlockName);
                    UpdateMasterDetailShellContext(eventArgs.BlockName);
                    _managerAdapter.Sync(_viewState);
                    ApplyShellStateToUi();
                }

                BlockUnitOfWorkActivity?.Invoke(this, eventArgs);
            });
        }

        private static bool ShouldSyncFromUnitOfWorkActivity(BeepFormsUnitOfWorkEventKind eventKind)
        {
            return eventKind is
                BeepFormsUnitOfWorkEventKind.CurrentChanged or
                BeepFormsUnitOfWorkEventKind.ItemChanged or
                BeepFormsUnitOfWorkEventKind.PostCreate or
                BeepFormsUnitOfWorkEventKind.PostQuery or
                BeepFormsUnitOfWorkEventKind.PostInsert or
                BeepFormsUnitOfWorkEventKind.PostUpdate or
                BeepFormsUnitOfWorkEventKind.PostEdit or
                BeepFormsUnitOfWorkEventKind.PostDelete or
                BeepFormsUnitOfWorkEventKind.PostCommit;
        }

        private void HandleManagerTriggerExecuting(object? sender, TriggerExecutingEventArgs e)
            => RunOnUiThread(() => TriggerExecuting?.Invoke(this, e));

        private void HandleManagerTriggerExecuted(object? sender, TriggerExecutedEventArgs e)
            => RunOnUiThread(() => TriggerExecuted?.Invoke(this, e));

        private void HandleManagerTriggerRegistered(object? sender, TriggerRegisteredEventArgs e)
            => RunOnUiThread(() => TriggerRegistered?.Invoke(this, e));

        private void HandleManagerTriggerUnregistered(object? sender, TriggerUnregisteredEventArgs e)
            => RunOnUiThread(() => TriggerUnregistered?.Invoke(this, e));

        private void HandleManagerTriggerChainCompleted(object? sender, TriggerChainCompletedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                PublishWorkflowState(BuildTriggerChainSummary(e), ResolveTriggerChainSeverity(e));
                TriggerChainCompleted?.Invoke(this, e);
            });
        }

        private string BuildTriggerChainSummary(TriggerChainCompletedEventArgs e)
        {
            string contextBlockName = !string.IsNullOrWhiteSpace(_viewState.ActiveBlockName)
                ? _viewState.ActiveBlockName!
                : _formsManager?.CurrentBlockName ?? string.Empty;

            string contextSuffix = string.IsNullOrWhiteSpace(contextBlockName)
                ? string.Empty
                : $" for '{contextBlockName}'";

            if (e.TriggerCount <= 0)
            {
                return $"Trigger chain {e.TriggerType}{contextSuffix} had no registered handlers.";
            }

            string detail = $"{e.SuccessCount} ok, {e.FailureCount} failed, {e.SkippedCount} skipped";

            if (e.FailureCount > 0)
            {
                return $"Trigger chain {e.TriggerType}{contextSuffix} finished with failures: {detail} in {e.TotalDurationMs:F0} ms.";
            }

            if (e.WasCancelled)
            {
                string cancelMessage = string.IsNullOrWhiteSpace(e.CancelMessage)
                    ? "chain canceled"
                    : $"canceled: {e.CancelMessage}";
                return $"Trigger chain {e.TriggerType}{contextSuffix} was canceled after {detail}; {cancelMessage}.";
            }

            if (e.SkippedCount > 0)
            {
                return $"Trigger chain {e.TriggerType}{contextSuffix} completed with conditional skips: {e.SuccessCount} ran, {e.SkippedCount} skipped in {e.TotalDurationMs:F0} ms.";
            }

            return $"Trigger chain {e.TriggerType}{contextSuffix} completed successfully: {e.SuccessCount}/{e.TriggerCount} ran in {e.TotalDurationMs:F0} ms.";
        }

        private static BeepFormsMessageSeverity ResolveTriggerChainSeverity(TriggerChainCompletedEventArgs e)
        {
            if (e.FailureCount > 0)
            {
                return BeepFormsMessageSeverity.Error;
            }

            if (e.WasCancelled || e.SkippedCount > 0)
            {
                return e.WasCancelled ? BeepFormsMessageSeverity.Warning : BeepFormsMessageSeverity.Info;
            }

            return e.SuccessCount > 0
                ? BeepFormsMessageSeverity.Success
                : BeepFormsMessageSeverity.Info;
        }
    }
}