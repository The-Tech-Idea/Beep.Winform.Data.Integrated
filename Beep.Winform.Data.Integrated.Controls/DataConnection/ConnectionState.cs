using System;
using TheTechIdea.Beep.ConfigUtil;

namespace TheTechIdea.Beep.Winform.Controls
{
    /// <summary>
    /// Payload for the <see cref="BeepDataConnection.ConnectionStateChanged"/>
    /// event.
    /// </summary>
    public sealed class ConnectionStateChangedEventArgs : EventArgs
    {
        public ConnectionStateChangedEventArgs(string connectionName, BeepConnectionLifecycle state, string? message, DateTime timestampUtc)
        {
            ConnectionName = connectionName ?? string.Empty;
            State = state;
            Message = message;
            TimestampUtc = timestampUtc;
        }

        public string ConnectionName { get; }
        public BeepConnectionLifecycle State { get; }
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
