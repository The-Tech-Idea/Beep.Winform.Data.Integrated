using System;
using System.IO;
using System.Text.Json;
using TheTechIdea.Beep.Editor;

namespace TheTechIdea.Beep.Winform.Default.Views.NuggetsManage
{
    /// <summary>
    /// Persists NuggetsManage UI state (sources, installed packages, last search, etc.).
    /// </summary>
    public sealed class NuggetsManageStateStore
    {
        private readonly IDMEEditor _editor;
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            WriteIndented = true
        };

        public NuggetsManageStateStore(IDMEEditor editor)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
        }

        public NuggetsPersistedState Load()
        {
            var statePath = GetStatePath();
            if (!File.Exists(statePath))
            {
                return new NuggetsPersistedState();
            }

            try
            {
                var json = File.ReadAllText(statePath);
                return JsonSerializer.Deserialize<NuggetsPersistedState>(json, _serializerOptions)
                    ?? new NuggetsPersistedState();
            }
            catch
            {
                return new NuggetsPersistedState();
            }
        }

        public void Save(NuggetsPersistedState state)
        {
            var statePath = GetStatePath();
            var dir = Path.GetDirectoryName(statePath);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(state ?? new NuggetsPersistedState(), _serializerOptions);
            File.WriteAllText(statePath, json);
        }

        private string GetStatePath()
        {
            var configPath = _editor.ConfigEditor?.ConfigPath;
            if (string.IsNullOrWhiteSpace(configPath))
            {
                configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
            }

            return Path.Combine(configPath, "NuggetsManageState.json");
        }
    }
}
