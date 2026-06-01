using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TheTechIdea.Beep.Editor;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.NuggetsManage
{
    internal sealed class NuggetStateStore
    {
        private readonly IDMEEditor _editor;
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public NuggetStateStore(IDMEEditor editor)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
        }

        public List<NuggetItemState> Load()
        {
            var path = GetStateFilePath();
            if (!File.Exists(path))
            {
                return new List<NuggetItemState>();
            }

            try
            {
                var json = File.ReadAllText(path);
                var items = JsonSerializer.Deserialize<List<NuggetItemState>>(json, _serializerOptions);
                return items ?? new List<NuggetItemState>();
            }
            catch
            {
                return new List<NuggetItemState>();
            }
        }

        public void Save(IEnumerable<NuggetItemState> states)
        {
            var path = GetStateFilePath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var normalized = states
                .Where(state => state != null)
                .GroupBy(state => state.SourcePath, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            var json = JsonSerializer.Serialize(normalized, _serializerOptions);
            File.WriteAllText(path, json);
        }

        private string GetStateFilePath()
        {
            var configPath = _editor.ConfigEditor?.ConfigPath;
            if (string.IsNullOrWhiteSpace(configPath))
            {
                configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
            }

            return Path.Combine(configPath, "NuggetsState.json");
        }
    }
}
