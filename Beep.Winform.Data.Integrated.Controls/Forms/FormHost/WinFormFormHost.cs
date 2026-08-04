using System.ComponentModel;
﻿using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost
{
[ToolboxItem(true)]
[DisplayName("Beep Form Host")]
[Category("Beep Forms")]
[Description("Hosts a BeepDM form: owns the FormsManager and wires the block views and status surfaces on the form.")]
public partial class WinFormFormHost : UserControl, IBeepFormsHost
{
        private readonly Dictionary<string, IBlockView> _blocks =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly int _ownerThreadId = Environment.CurrentManagedThreadId;
        private IUnitofWorksManager? _formsManager;
        private string? _activeBlockName;
        private bool _lifecycleDisposed;

        public string? ActiveBlockName => _activeBlockName;

        /// <summary>
        /// Blocks declared on this form at design time. Generated designer code
        /// registers into it with
        /// <c>this._formsHost.Definition.Blocks.Add(Ord);</c>, so it is never
        /// null and needs no guard in generated source.
        /// </summary>
        public FormDefinition Definition { get; } = new();

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
                        RenderMenu(null);
                        return;
                    }

                    AttachManagerEvents(manager);

                    // Design-time blocks become live blocks here, BEFORE the
                    // binding pass below — that pass skips any block the manager
                    // does not know, and a designer-declared block is not known
                    // until this runs.
                    //
                    // The logic lives in the engine. This host is a thin
                    // presentation layer and must not carry its own copy; the
                    // WPF host makes the same single call.
                    TheTechIdea.Beep.Editor.Forms.Helpers.DefinitionBlockRegistrar
                        .RegisterAll(manager, Definition);

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

                    // Views dropped on the form before a manager existed were
                    // discovered but could not bind, and a status surface bound
                    // to a manager-less host showed nothing. There is one now:
                    // sweep again and let them read it.
                    AutoWireSurfaces();
                    RefreshStatusSurfaces();

                    // Render the form's menu bar (if any) against the newly-bound
                    // manager. The tree comes from Definition.Menu; every click
                    // dispatches through the engine's InvokeMenuItemAsync.
                    RenderMenu(manager);
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
                RenderMenu(null);
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
