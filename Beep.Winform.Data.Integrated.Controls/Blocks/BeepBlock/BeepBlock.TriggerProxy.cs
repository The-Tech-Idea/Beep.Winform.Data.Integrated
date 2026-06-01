using System;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks
{
    public partial class BeepBlock
    {
        public event EventHandler<TriggerExecutingEventArgs>? TriggerExecuting;
        public event EventHandler<TriggerExecutedEventArgs>? TriggerExecuted;
        public event EventHandler<TriggerRegisteredEventArgs>? TriggerRegistered;
        public event EventHandler<TriggerUnregisteredEventArgs>? TriggerUnregistered;
        public event EventHandler<BeepFormsUnitOfWorkEventArgs>? UnitOfWorkActivity;

        private void AttachToFormsHost(IBeepFormsHost formsHost)
        {
            formsHost.TriggerExecuting += HandleHostTriggerExecuting;
            formsHost.TriggerExecuted += HandleHostTriggerExecuted;
            formsHost.TriggerRegistered += HandleHostTriggerRegistered;
            formsHost.TriggerUnregistered += HandleHostTriggerUnregistered;
            formsHost.BlockUnitOfWorkActivity += HandleHostUnitOfWorkActivity;
        }

        private void DetachFromFormsHost(IBeepFormsHost? formsHost)
        {
            if (formsHost == null)
            {
                return;
            }

            formsHost.TriggerExecuting -= HandleHostTriggerExecuting;
            formsHost.TriggerExecuted -= HandleHostTriggerExecuted;
            formsHost.TriggerRegistered -= HandleHostTriggerRegistered;
            formsHost.TriggerUnregistered -= HandleHostTriggerUnregistered;
            formsHost.BlockUnitOfWorkActivity -= HandleHostUnitOfWorkActivity;
        }

        private void RefreshTriggerState()
        {
            if (_formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName) || !_formsHost.IsBlockRegistered(ManagerBlockName))
            {
                ResetTriggerState();
                return;
            }

            TriggerStatisticsInfo? statistics = _formsHost.GetTriggerStatistics(ManagerBlockName);
            _viewState.TriggerCount = statistics?.TotalTriggers ?? 0;
            _viewState.FormTriggerCount = _formsHost.GetFormLevelTriggers(ManagerBlockName).Count;
            _viewState.BlockTriggerCount = _formsHost.GetBlockLevelTriggers(ManagerBlockName).Count;
            _viewState.RecordTriggerCount = _formsHost.GetRecordLevelTriggers(ManagerBlockName).Count;
            _viewState.ItemTriggerCount = _formsHost.GetItemLevelTriggers(ManagerBlockName).Count;
        }

        private void ResetTriggerState()
        {
            _viewState.TriggerCount = 0;
            _viewState.FormTriggerCount = 0;
            _viewState.BlockTriggerCount = 0;
            _viewState.RecordTriggerCount = 0;
            _viewState.ItemTriggerCount = 0;
            _viewState.LastTriggerText = string.Empty;
            _viewState.LastUnitOfWorkActivityText = string.Empty;
        }

        private void HandleHostTriggerExecuting(object? sender, TriggerExecutingEventArgs e)
        {
            if (!MatchesTriggerDefinition(e.Trigger))
            {
                return;
            }

            RunOnBlockUiThread(() =>
            {
                _viewState.LastTriggerText = BuildTriggerText(e.Trigger, "executing");
                UpdateWorkflowSurface();
                TriggerExecuting?.Invoke(this, e);
                NotifyViewStateChanged();
                Invalidate();
            });
        }

        private void HandleHostTriggerExecuted(object? sender, TriggerExecutedEventArgs e)
        {
            if (!MatchesTriggerDefinition(e.Trigger))
            {
                return;
            }

            RunOnBlockUiThread(() =>
            {
                _viewState.LastTriggerText = BuildTriggerText(e.Trigger, e.Result.ToString());
                UpdateWorkflowSurface();
                TriggerExecuted?.Invoke(this, e);
                NotifyViewStateChanged();
                Invalidate();
            });
        }

        private void HandleHostTriggerRegistered(object? sender, TriggerRegisteredEventArgs e)
        {
            if (!MatchesTriggerRegistration(e.BlockName))
            {
                return;
            }

            RunOnBlockUiThread(() =>
            {
                RefreshTriggerState();
                _viewState.LastTriggerText = string.IsNullOrWhiteSpace(e.Trigger?.TriggerName)
                    ? "Trigger registered"
                    : $"Registered trigger: {e.Trigger.TriggerName}";
                UpdateWorkflowSurface();
                TriggerRegistered?.Invoke(this, e);
                NotifyViewStateChanged();
                Invalidate();
            });
        }

        private void HandleHostTriggerUnregistered(object? sender, TriggerUnregisteredEventArgs e)
        {
            if (!MatchesTriggerRegistration(e.BlockName))
            {
                return;
            }

            RunOnBlockUiThread(() =>
            {
                RefreshTriggerState();
                _viewState.LastTriggerText = $"Unregistered {e.RemovedCount} trigger(s)";
                UpdateWorkflowSurface();
                TriggerUnregistered?.Invoke(this, e);
                NotifyViewStateChanged();
                Invalidate();
            });
        }

        private void HandleHostUnitOfWorkActivity(object? sender, BeepFormsUnitOfWorkEventArgs e)
        {
            if (!MatchesManagerBlock(e.BlockName))
            {
                return;
            }

            RunOnBlockUiThread(() =>
            {
                _viewState.LastUnitOfWorkActivityText = e.ActivityText;
                if (ShouldSyncFromUnitOfWorkActivity(e.EventKind))
                {
                    SyncFromManager();
                }
                else
                {
                    UpdateWorkflowSurface();
                    NotifyViewStateChanged();
                    Invalidate();
                }

                UnitOfWorkActivity?.Invoke(this, e);
            });
        }

        private bool MatchesTriggerDefinition(TriggerDefinition? trigger)
        {
            if (trigger == null)
            {
                return false;
            }

            if (MatchesManagerBlock(trigger.BlockName))
            {
                return true;
            }

            return string.IsNullOrWhiteSpace(trigger.BlockName)
                && _formsHost != null
                && string.Equals(_formsHost.ActiveBlockName, BlockName, StringComparison.OrdinalIgnoreCase);
        }

        private bool MatchesTriggerRegistration(string? blockName)
        {
            if (MatchesManagerBlock(blockName))
            {
                return true;
            }

            return string.IsNullOrWhiteSpace(blockName)
                && _formsHost != null
                && string.Equals(_formsHost.ActiveBlockName, BlockName, StringComparison.OrdinalIgnoreCase);
        }

        private bool MatchesManagerBlock(string? blockName)
        {
            return !string.IsNullOrWhiteSpace(blockName)
                && string.Equals(blockName, ManagerBlockName, StringComparison.OrdinalIgnoreCase);
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

        private static string BuildTriggerText(TriggerDefinition trigger, string state)
        {
            string triggerName = !string.IsNullOrWhiteSpace(trigger.TriggerName)
                ? trigger.TriggerName
                : trigger.TriggerType.ToString();

            return $"{triggerName} ({state})";
        }

        private void RunOnBlockUiThread(Action action)
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                if (IsHandleCreated)
                {
                    BeginInvoke(action);
                }
                else
                {
                    action();
                }

                return;
            }

            action();
        }
    }
}