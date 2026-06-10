using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.SetUp;
using TheTechIdea.Beep.SetUp.Steps;

namespace TheTechIdea.Beep.Winform.Default.Views.Setup
{
    /// <summary>
    /// Helper that constructs a properly-configured <see cref="ISetupWizard"/> using
    /// <see cref="ISetupWizardFactory.Create"/> (not <c>CreateDefault</c>) with
    /// pre-populated step options.
    ///
    /// <para>
    /// This is a WinForms-side mirror of the Blazor <c>BootstrapWizardBuilder</c>.
    /// Both call <c>factory.Create(editor, options, configure)</c> with a builder
    /// that supplies non-empty step options, which the steps' <c>Validate</c>
    /// methods require to pass.
    /// </para>
    /// </summary>
    public static class BootstrapWizardBuilder
    {
        public static (ISetupWizard wizard, SetupContext context) BuildForFirstRun(
            ISetupWizardFactory factory,
            IDMEEditor editor)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (editor == null) throw new ArgumentNullException(nameof(editor));

            return factory.Create(editor, new SetupOptions(), builder =>
            {
                builder
                    .WithId("beep-web-bootstrap")
                    .AddStep(BuildDriverProvisionStep(editor))
                    .AddStep(BuildConnectionConfigStep(editor))
                    .AddStep(new SchemaSetupStep(new SchemaSetupStepOptions()))
                    .AddStep(new DefaultsSetupStep());
            });
        }

        private static DriverProvisionStep BuildDriverProvisionStep(IDMEEditor editor)
        {
            ConnectionDriversConfig driver = null;
            try
            {
                driver = editor?.ConfigEditor?.DataDriversClasses?
                    .FirstOrDefault(d => d.AutoLoad && !d.NuggetMissing)
                    ?? editor?.ConfigEditor?.DataDriversClasses?.FirstOrDefault();
            }
            catch { }

            return new DriverProvisionStep(new DriverProvisionStepOptions
            {
                PackageName = driver?.PackageName ?? string.Empty
            });
        }

        private static ConnectionConfigStep BuildConnectionConfigStep(IDMEEditor editor)
        {
            ConnectionProperties conn = null;
            try
            {
                conn = editor?.ConfigEditor?.DataConnections?.FirstOrDefault();
            }
            catch { }

            if (conn == null)
            {
                conn = new ConnectionProperties
                {
                    ConnectionName = "MainDB",
                    DatabaseType = DataSourceType.SqlLite,
                    ConnectionString = $"Data Source={Path.Combine(AppContext.BaseDirectory, "beep-bootstrap.db")}",
                    Category = DatasourceCategory.RDBMS
                };
            }

            return new ConnectionConfigStep(new ConnectionConfigStepOptions
            {
                ConnectionProperties = conn
            });
        }
    }
}
