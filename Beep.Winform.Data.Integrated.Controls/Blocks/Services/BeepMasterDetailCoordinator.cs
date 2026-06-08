using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Services
{
    /// <summary>
    /// Coordinates a master block and any number of detail blocks, the way
    /// Oracle Forms coordinates them via <c>Post-Query</c>, <c>Post-Change</c>
    /// and <c>Pre-Query</c> triggers.
    /// <para>
    /// The coordinator owns a small, opinionated policy:
    /// </para>
    /// <list type="bullet">
    /// <item>When the master's current record changes, the coordinator pushes
    /// the master's key into every detail block whose definition declares a
    /// matching <see cref="BeepBlockEntityDefinition.ForeignKeyField"/> /
    /// <see cref="BeepBlockEntityDefinition.MasterKeyField"/> pair.</item>
    /// <item>If the detail block is in Normal mode and currently has no rows,
    /// the coordinator fires a Pre-Query-equivalent and asks the detail
    /// block to re-run its query.</item>
    /// <item>If the detail block is dirty, the coordinator leaves it alone
    /// (matches Forms' "do not requery while a detail has unsaved changes"
    /// rule — the developer is expected to commit or roll back first).</item>
    /// </list>
    /// </summary>
    public sealed class BeepMasterDetailCoordinator
    {
        private readonly object _lock = new();
        private readonly List<BeepBlock> _registeredBlocks = new();
        private BeepBlock? _masterBlock;
        private int _lastMasterRecordIndex = int.MinValue;

        /// <summary>Last master primary key value pushed to detail blocks.</summary>
        public object? LastMasterKey { get; private set; }

        /// <summary>When true, the coordinator is in dry-run mode — actions are logged not performed.</summary>
        public bool TraceEnabled { get; set; }

        /// <summary>
        /// Raised before a detail block is requeried as a result of master
        /// record change. Hosts / developers can subscribe to fire an
        /// Oracle Forms <c>Pre-Query</c> trigger equivalent that customizes
        /// the detail's WHERE clause before the query is reissued.
        /// <para>
        /// Handlers may cancel the requery by setting
        /// <see cref="MasterDetailCoordinatingEventArgs.Cancel"/> to
        /// <c>true</c>.
        /// </para>
        /// </summary>
        public event EventHandler<MasterDetailCoordinatingEventArgs>? DetailPreQuery;

        /// <summary>
        /// Raised after the coordinator has pushed a master key into a
        /// detail block. Use to log, audit, or post-process.
        /// </summary>
        public event EventHandler<MasterDetailCoordinatedEventArgs>? DetailCoordinated;

        /// <summary>Number of detail blocks currently registered as descendants of the master.</summary>
        public int DetailBlockCount
        {
            get { lock (_lock) { return _registeredBlocks.Count(b => !ReferenceEquals(b, _masterBlock)); } }
        }

        /// <summary>
        /// Register a block. The first block registered becomes the master
        /// unless <paramref name="asMaster"/> is set explicitly to false.
        /// </summary>
        public void Register(BeepBlock block, bool asMaster = false)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));

            lock (_lock)
            {
                if (_registeredBlocks.Contains(block))
                {
                    return;
                }

                _registeredBlocks.Add(block);

                if (asMaster || _masterBlock == null)
                {
                    _masterBlock = block;
                }

                block.MasterRecordChanged += OnMasterRecordChanged;
            }
        }

        /// <summary>
        /// Unregister a block. The next master registration will pick a new
        /// master if needed.
        /// </summary>
        public void Unregister(BeepBlock block)
        {
            if (block == null) return;

            lock (_lock)
            {
                _registeredBlocks.Remove(block);
                block.MasterRecordChanged -= OnMasterRecordChanged;

                if (ReferenceEquals(_masterBlock, block))
                {
                    _masterBlock = _registeredBlocks.FirstOrDefault();
                    _lastMasterRecordIndex = int.MinValue;
                }
            }
        }

        /// <summary>Clear all registrations.</summary>
        public void Reset()
        {
            lock (_lock)
            {
                foreach (var block in _registeredBlocks)
                {
                    block.MasterRecordChanged -= OnMasterRecordChanged;
                }
                _registeredBlocks.Clear();
                _masterBlock = null;
                _lastMasterRecordIndex = int.MinValue;
                LastMasterKey = null;
            }
        }

        /// <summary>
        /// Manually trigger a coordination cycle. Useful from a
        /// <c>Post-Change</c> trigger on the master's key field.
        /// </summary>
        public int Coordinate()
        {
            BeepBlock? master;
            List<BeepBlock> details;
            int masterIndex;
            int detailsAffected = 0;

            lock (_lock)
            {
                master = _masterBlock;
                if (master == null) return 0;
                masterIndex = master.ViewState.CurrentRecordIndex;
                details = _registeredBlocks.Where(b => !ReferenceEquals(b, master)).ToList();
            }

            object? newKey = ResolveMasterKey(master);

            foreach (var detail in details)
            {
                if (detail == null) continue;
                if (detail.ViewState.IsDirty)
                {
                    if (TraceEnabled)
                    {
                        System.Diagnostics.Debug.WriteLine($"[MasterDetail] '{detail.BlockName}' is dirty; leaving untouched.");
                    }
                    continue;
                }

                // Fire the Pre-Query hook so subscribers (typically the
                // developer) can run their Forms-equivalent
                // Pre-Query trigger before the FK copy / requery happens.
                var preArgs = new MasterDetailCoordinatingEventArgs(detail, master, newKey, _lastMasterRecordIndex, masterIndex);
                try
                {
                    DetailPreQuery?.Invoke(this, preArgs);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MasterDetail] DetailPreQuery handler threw: {ex.GetType().Name}: {ex.Message}");
                }

                if (preArgs.Cancel)
                {
                    if (TraceEnabled)
                    {
                        System.Diagnostics.Debug.WriteLine($"[MasterDetail] '{detail.BlockName}' requery cancelled by handler.");
                    }
                    continue;
                }

                bool applied = false;
                try
                {
                    applied = ApplyMasterKeyToDetail(detail, preArgs.MasterKey);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MasterDetail] ApplyMasterKeyToDetail failed for '{detail.BlockName}': {ex.GetType().Name}: {ex.Message}");
                }

                if (applied)
                {
                    detailsAffected++;
                    try
                    {
                        DetailCoordinated?.Invoke(this,
                            new MasterDetailCoordinatedEventArgs(detail, master, preArgs.MasterKey));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[MasterDetail] DetailCoordinated handler threw: {ex.GetType().Name}: {ex.Message}");
                    }
                }
            }

            LastMasterKey = newKey;
            _lastMasterRecordIndex = masterIndex;
            return detailsAffected;
        }

        private void OnMasterRecordChanged(object? sender, EventArgs e)
        {
            if (!ReferenceEquals(sender, _masterBlock)) return;
            Coordinate();
        }

        private static object? ResolveMasterKey(BeepBlock master)
        {
            if (master == null) return null;

            string keyField = master.Definition?.Entity?.MasterKeyField ?? string.Empty;
            if (string.IsNullOrWhiteSpace(keyField))
            {
                return null;
            }

            return master.TryGetCurrentFieldValue(keyField, out var value) ? value : null;
        }

        private static bool ApplyMasterKeyToDetail(BeepBlock detail, object? newKey)
        {
            string fkField = detail.Definition?.Entity?.ForeignKeyField ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fkField))
            {
                return false;
            }

            return detail.TrySetCurrentFieldValue(fkField, newKey);
        }
    }

    /// <summary>
    /// Payload for <see cref="BeepMasterDetailCoordinator.DetailPreQuery"/>.
    /// Handlers can override <see cref="MasterKey"/> to redirect the foreign
    /// key, or set <see cref="Cancel"/> to abort the requery entirely (e.g.
    /// because the new master row is itself uncommitted).
    /// </summary>
    public sealed class MasterDetailCoordinatingEventArgs : EventArgs
    {
        public MasterDetailCoordinatingEventArgs(BeepBlock detail, BeepBlock? master, object? masterKey, int previousMasterRecordIndex, int newMasterRecordIndex)
        {
            Detail = detail ?? throw new ArgumentNullException(nameof(detail));
            Master = master;
            MasterKey = masterKey;
            PreviousMasterRecordIndex = previousMasterRecordIndex;
            NewMasterRecordIndex = newMasterRecordIndex;
        }

        public BeepBlock Detail { get; }
        public BeepBlock? Master { get; }
        public object? MasterKey { get; set; }
        public int PreviousMasterRecordIndex { get; }
        public int NewMasterRecordIndex { get; }
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Payload for <see cref="BeepMasterDetailCoordinator.DetailCoordinated"/>.
    /// Fires after the foreign key has been pushed to the detail block.
    /// </summary>
    public sealed class MasterDetailCoordinatedEventArgs : EventArgs
    {
        public MasterDetailCoordinatedEventArgs(BeepBlock detail, BeepBlock? master, object? masterKey)
        {
            Detail = detail ?? throw new ArgumentNullException(nameof(detail));
            Master = master;
            MasterKey = masterKey;
        }

        public BeepBlock Detail { get; }
        public BeepBlock? Master { get; }
        public object? MasterKey { get; }
    }
}
