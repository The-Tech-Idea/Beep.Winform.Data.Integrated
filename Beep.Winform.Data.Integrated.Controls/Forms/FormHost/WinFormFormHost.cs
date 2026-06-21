using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost
{
public partial class WinFormFormHost : UserControl, IBeepFormsHost
{
        private readonly Dictionary<string, IBlockView> _blocks =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly int _ownerThreadId = Environment.CurrentManagedThreadId;
        private IUnitofWorksManager? _formsManager;
        private string? _activeBlockName;
        private bool _lifecycleDisposed;

        public string? ActiveBlockName => _activeBlockName;

        public IUnitofWorksManager? FormsManager
        {
            get => _formsManager;
            set => ReplaceFormsManager(value);
        }

    public event EventHandler? ActiveBlockChanged;
    public event EventHandler<FormsNotificationEventArgs>? NotificationRaised;
    public event EventHandler<FormsHostMessageEventArgs>? MessageRaised;
    public event EventHandler<FormsHostMessageEventArgs>? MessageCleared;
    public event EventHandler<FormsHostTimerEventArgs>? TimerFired;
    public event EventHandler<FormsHostFormMessageEventArgs>? FormMessageReceived;
    public IWinFormFormsFactory? FormFactory { get; set; }

        private IUnitofWorksManager RequireManager() =>
            _formsManager ?? throw new InvalidOperationException(
                "A Forms manager must be assigned before accessing block data.");

        private void ReplaceFormsManager(IUnitofWorksManager? manager)
        {
            RunOnUi(() =>
            {
                if (ReferenceEquals(_formsManager, manager))
                {
                    return;
                }

                var previousManager = _formsManager;
                var bindingSnapshot = _blocks
                    .Select(pair => new BlockBindingSnapshot(
                        pair.Key,
                        pair.Value,
                        pair.Value.IsBound))
                    .ToList();
                var bindAttempts = new HashSet<IBlockView>(
                    ReferenceEqualityComparer.Instance);

                try
                {
                    foreach (var snapshot in bindingSnapshot)
                    {
                        if (snapshot.WasBound)
                        {
                            snapshot.Block.Unbind();
                        }
                    }

                    if (previousManager is not null)
                    {
                        DetachManagerEvents(previousManager);
                    }

                    _formsManager = manager;
                    if (manager is null)
                    {
                        return;
                    }

                    AttachManagerEvents(manager);
                    foreach (var snapshot in bindingSnapshot)
                    {
                        if (!manager.BlockExists(snapshot.BlockName))
                        {
                            continue;
                        }

                        bindAttempts.Add(snapshot.Block);
                        snapshot.Block.Bind(this);
                        snapshot.Block.SyncFromManager();
                    }
                }
                catch (Exception originalException)
                {
                    foreach (var block in bindAttempts)
                    {
                        try
                        {
                            block.Unbind();
                        }
                        catch (Exception cleanupException)
                        {
                            AttachCleanupException(
                                originalException,
                                "ManagerBindCleanup",
                                cleanupException);
                        }
                    }

                    if (manager is not null)
                    {
                        DetachManagerEvents(manager);
                    }

                    _formsManager = previousManager;
                    if (previousManager is not null)
                    {
                        AttachManagerEvents(previousManager);
                    }
                    foreach (var snapshot in bindingSnapshot)
                    {
                        if (!snapshot.WasBound)
                        {
                            continue;
                        }

                        try
                        {
                            snapshot.Block.Bind(this);
                            snapshot.Block.SyncFromManager();
                        }
                        catch (Exception restoreException)
                        {
                            AttachCleanupException(
                                originalException,
                                "ManagerBindingRestore",
                                restoreException);
                        }
                    }

                    throw;
                }
            });
        }

        private sealed record BlockBindingSnapshot(
            string BlockName,
            IBlockView Block,
            bool WasBound);

        private static void AttachCleanupException(
            Exception originalException,
            string operation,
            Exception cleanupException)
        {
            var key = operation;
            var suffix = 1;
            while (originalException.Data.Contains(key))
            {
                key = $"{operation}.{suffix++}";
            }

            originalException.Data[key] = cleanupException;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_lifecycleDisposed)
            {
                _lifecycleDisposed = true;

                foreach (var block in _blocks.Values)
                {
                    try
                    {
                        block.Unbind();
                    }
                    catch
                    {
                        // Disposal must continue so every registered view gets
                        // a chance to release its UI event subscriptions.
                    }
                }

                _blocks.Clear();
                _activeBlockName = null;
                if (_formsManager is not null)
                {
                    DetachManagerEvents(_formsManager);
                }
                _formsManager = null;
            }

            base.Dispose(disposing);
        }
    }
}
