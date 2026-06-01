using System;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.NuggetsManage
{
    public static class NuggetsStartupBootstrapper
    {
        private static bool _restored;
        private static readonly object SyncRoot = new();

        public static void TryRestore(IDMEEditor editor)
        {
            if (editor == null)
            {
                return;
            }

            lock (SyncRoot)
            {
                if (_restored)
                {
                    return;
                }

                try
                {
                    var service = new NuggetsManageService(editor);
                    var states = service.LoadPersistedStates();
                    var result = service.RestoreEnabledNuggets(states);
                    service.SaveStates(states);
                    editor.AddLogMessage("NuggetsManage", result.Message, DateTime.Now, 0, null, Errors.Ok);
                    _restored = true;
                }
                catch (Exception ex)
                {
                    editor.AddLogMessage("NuggetsManage", $"Startup nugget restore failed: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                }
            }
        }
    }
}
