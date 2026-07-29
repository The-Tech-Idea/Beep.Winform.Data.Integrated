using TheTechIdea.Beep.Winform.Default.Views.ImportExport.Models;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    /// <summary>
    /// Translates between the wizard's <see cref="ImportPurpose"/> and the engine's
    /// <see cref="SyncMode"/>.
    /// </summary>
    /// <remarks>
    /// Template persistence is not here: <c>IDMEEditor.ConfigEditor</c> already owns
    /// <c>SaveImportConfiguration</c>, <c>LoadImportConfiguration</c>,
    /// <c>GetSavedImportConfigNames</c> and <c>DeleteImportConfiguration</c>, so the steps call
    /// those directly rather than through a pass-through wrapper.
    /// </remarks>
    public static class ImportTemplateManager
    {
        public static ImportPurpose GetPurpose(DataImportConfiguration config)
        {
            return config.SyncMode switch
            {
                SyncMode.Upsert => ImportPurpose.AddOrUpdate,
                _ => ImportPurpose.AddOnly,
            };
        }

        public static void ApplyPurpose(DataImportConfiguration config, ImportPurpose purpose)
        {
            config.SyncMode = purpose switch
            {
                ImportPurpose.AddOrUpdate => SyncMode.Upsert,
                ImportPurpose.ReplaceAll => SyncMode.FullRefresh,
                _ => SyncMode.FullRefresh,
            };
        }
    }
}
