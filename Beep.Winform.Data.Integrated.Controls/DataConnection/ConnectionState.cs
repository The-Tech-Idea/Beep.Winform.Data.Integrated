using System;

namespace TheTechIdea.Beep.Winform.Controls
{
    /// <summary>
    /// Coarse lifecycle state of a single Beep connection. Used by
    /// <see cref="BeepDataConnection"/> to surface a status indicator the
    /// same way Oracle Forms shows a green / red dot next to each
    /// connection in the Login screen.
    /// <para>
    /// Renamed to <c>BeepConnectionState</c> to avoid clashing with
    /// <see cref="System.Data.ConnectionState"/>.
    /// </para>
    /// </summary>
    public enum BeepConnectionState
    {
        /// <summary>No test or open has been performed on this connection yet.</summary>
        Unknown = 0,
        /// <summary>The last test / open succeeded.</summary>
        Connected = 1,
        /// <summary>The last test / open failed; check the message for details.</summary>
        Failed = 2,
        /// <summary>The connection is currently being tested.</summary>
        Testing = 3,
        /// <summary>The connection has been deliberately closed.</summary>
        Closed = 4
    }

    /// <summary>
    /// Payload for the <see cref="BeepDataConnection.ConnectionStateChanged"/>
    /// event.
    /// </summary>
    public sealed class ConnectionStateChangedEventArgs : EventArgs
    {
        public ConnectionStateChangedEventArgs(string connectionName, BeepConnectionState state, string? message, DateTime timestampUtc)
        {
            ConnectionName = connectionName ?? string.Empty;
            State = state;
            Message = message;
            TimestampUtc = timestampUtc;
        }

        public string ConnectionName { get; }
        public BeepConnectionState State { get; }
        public string? Message { get; }
        public DateTime TimestampUtc { get; }
    }

    /// <summary>
    /// Payload for the <see cref="BeepDataConnection.LogonStarting"/>,
    /// <see cref="BeepDataConnection.LogonCompleted"/> and
    /// <see cref="BeepDataConnection.LogoffRequested"/> events. Mirrors
    /// the Oracle Forms <c>ON-LOGON</c> / <c>POST-LOGON</c> /
    /// <c>ON-LOGOFF</c> trigger events.
    /// </summary>
    public sealed class ConnectionLifecycleEventArgs : EventArgs
    {
        public ConnectionLifecycleEventArgs(string connectionName, DateTime timestampUtc, string? message = null)
        {
            ConnectionName = connectionName ?? string.Empty;
            TimestampUtc = timestampUtc;
            Message = message;
        }

        public string ConnectionName { get; }
        public DateTime TimestampUtc { get; }
        public string? Message { get; }

        /// <summary>
        /// Set by handlers of <see cref="BeepDataConnection.LogonStarting"/>
        /// to abort the data-source open. Mirrors the Forms
        /// <c>LOGON_SCREEN</c> abort path.
        /// </summary>
        public bool Cancel { get; set; }
    }
}
