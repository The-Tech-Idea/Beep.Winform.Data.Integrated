using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Winform.Default.Views.ImportExport.Models;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    public static class ImportTemplateManager
    {
        public static void Save(IDMEEditor editor, string name, DataImportConfiguration config)
        {
            if (editor?.ConfigEditor == null || string.IsNullOrWhiteSpace(name))
                return;
            editor.ConfigEditor.SaveImportConfiguration(name, config);
        }

        public static DataImportConfiguration? Load(IDMEEditor editor, string name)
        {
            if (editor?.ConfigEditor == null || string.IsNullOrWhiteSpace(name))
                return null;
            try { return editor.ConfigEditor.LoadImportConfiguration(name); }
            catch { return null; }
        }

        public static List<string> ListAll(IDMEEditor editor)
        {
            if (editor?.ConfigEditor == null)
                return new List<string>();
            try { return editor.ConfigEditor.GetSavedImportConfigNames(); }
            catch { return new List<string>(); }
        }

        public static void Delete(IDMEEditor editor, string name)
        {
            if (editor?.ConfigEditor == null || string.IsNullOrWhiteSpace(name))
                return;
            editor.ConfigEditor.DeleteImportConfiguration(name);
        }

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
