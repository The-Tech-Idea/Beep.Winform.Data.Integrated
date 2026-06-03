using System;
using System.Collections.Generic;
using TheTechIdea.Beep.DataBase;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    /// <summary>
    /// One recorded sub-step of a <see cref="DatasourceSetupResult"/>.
    /// Use these to display a per-step progress / audit trail.
    /// </summary>
    public sealed class DatasourceSetupStep
    {
        public DatasourceSetupStep(string name, bool success, string message, TimeSpan duration)
        {
            Name = name ?? string.Empty;
            Success = success;
            Message = message ?? string.Empty;
            Duration = duration;
        }

        public string Name { get; }
        public bool Success { get; }
        public string Message { get; }
        public TimeSpan Duration { get; }

        public override string ToString()
            => $"[{(Success ? "OK" : "FAIL")}] {Name} ({Duration.TotalMilliseconds:0} ms) - {Message}";
    }

    /// <summary>
    /// Outcome of a <see cref="DatasourceSetupHandler"/> run.
    /// Aggregates per-step status with the overall result and a reference
    /// to the now-open datasource (when setup reached the open stage).
    /// </summary>
    public sealed class DatasourceSetupResult
    {
        private DatasourceSetupResult(
            bool success,
            string message,
            IDataSource? dataSource,
            bool driverProvisioned,
            bool connectionPersisted,
            bool connectionOpened,
            TimeSpan duration,
            IReadOnlyList<DatasourceSetupStep> steps,
            Exception? exception)
        {
            Success = success;
            Message = message ?? string.Empty;
            DataSource = dataSource;
            DriverProvisioned = driverProvisioned;
            ConnectionPersisted = connectionPersisted;
            ConnectionOpened = connectionOpened;
            Duration = duration;
            Steps = steps ?? Array.Empty<DatasourceSetupStep>();
            Exception = exception;
        }

        public bool Success { get; }
        public string Message { get; }
        public IDataSource? DataSource { get; }
        public bool DriverProvisioned { get; }
        public bool ConnectionPersisted { get; }
        public bool ConnectionOpened { get; }
        public TimeSpan Duration { get; }
        public IReadOnlyList<DatasourceSetupStep> Steps { get; }
        public Exception? Exception { get; }

        public static DatasourceSetupResult Ok(
            string message,
            IDataSource? dataSource,
            bool driverProvisioned,
            bool connectionPersisted,
            bool connectionOpened,
            TimeSpan duration,
            IReadOnlyList<DatasourceSetupStep> steps) =>
            new DatasourceSetupResult(true, message, dataSource,
                driverProvisioned, connectionPersisted, connectionOpened,
                duration, steps, null);

        public static DatasourceSetupResult Fail(
            string message,
            IReadOnlyList<DatasourceSetupStep> steps,
            TimeSpan duration,
            Exception? exception = null) =>
            new DatasourceSetupResult(false, message, null,
                driverProvisioned: false,
                connectionPersisted: false,
                connectionOpened: false,
                duration, steps, exception);
    }
}
