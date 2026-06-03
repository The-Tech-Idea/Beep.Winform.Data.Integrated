using System;
using System.Collections.Generic;
using TheTechIdea.Beep.ConfigUtil;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    /// <summary>
    /// Options for <see cref="DatasourceSetupHandler"/>.
    /// Carries everything the handler needs to take a datasource from
    /// "not present" (or "broken") to "ready to use" — including, when
    /// needed, downloading the driver NuGet package.
    /// </summary>
    public sealed class DatasourceSetupOptions
    {
        /// <summary>
        /// Connection to configure / persist / open. Required.
        /// At minimum <see cref="ConnectionProperties.ConnectionName"/> and
        /// <see cref="ConnectionProperties.DatabaseType"/> must be set.
        /// </summary>
        public ConnectionProperties ConnectionProperties { get; set; } = new();

        /// <summary>
        /// Optional override for the driver package name. When null, the
        /// best-matching driver is resolved from
        /// <see cref="TheTechIdea.Beep.Helpers.ConnectionHelper.GetBestMatchingDriver"/>.
        /// </summary>
        public string? DriverPackageName { get; set; }

        /// <summary>
        /// Optional explicit NuGet package version for the driver.
        /// </summary>
        public string? DriverVersion { get; set; }

        /// <summary>
        /// Additional NuGet sources to try when downloading the driver package.
        /// </summary>
        public IList<string> NuGetSources { get; set; } = new List<string>();

        /// <summary>
        /// Override install path for the driver NuGet package.
        /// When null, the assemblyHandler default path is used.
        /// </summary>
        public string? AppInstallPath { get; set; }

        /// <summary>
        /// Base directory for file-path normalisation of file-based datasources.
        /// When null, <see cref="AppContext.BaseDirectory"/> is used.
        /// </summary>
        public string? BaseDirectory { get; set; }

        /// <summary>
        /// If true, the datasource is opened at the end of setup and any
        /// open error is surfaced as a step failure. Default: true.
        /// </summary>
        public bool OpenConnection { get; set; } = true;

        /// <summary>
        /// If true, skip the connection string structure validation.
        /// Default: false.
        /// </summary>
        public bool SkipConnectionStringValidation { get; set; }

        /// <summary>
        /// If true (default), download + load the driver NuGet package when
        /// no driver is registered in <c>ConfigEditor.DataDriversClasses</c>
        /// for the target connection.
        /// </summary>
        public bool ProvisionDriverIfMissing { get; set; } = true;

        /// <summary>
        /// If true, treat the handler as a no-op when the connection is
        /// already registered and the datasource is already open.
        /// </summary>
        public bool SkipWhenAlreadyOpen { get; set; } = true;
    }
}
