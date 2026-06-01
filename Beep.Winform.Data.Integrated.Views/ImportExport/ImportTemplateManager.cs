using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TheTechIdea.Beep.Editor.Importing;
using TheTechIdea.Beep.Editor.Importing.Interfaces;
using TheTechIdea.Beep.Workflow;
using TheTechIdea.Beep.Workflow.Mapping;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    /// <summary>
    /// Serializable DTO for a saved mapping template — wraps the field-level mapping rows
    /// plus the import purpose/options so the user can reload an entire workflow preset.
    /// </summary>
    public sealed class ImportTemplateDto
    {
        public string         Name             { get; set; } = string.Empty;
        public ImportPurpose  Purpose          { get; set; } = ImportPurpose.AddOnly;
        public string         MatchByField     { get; set; } = string.Empty;
        public bool           UpdateEmptyFields{ get; set; }
        public int            BatchSize        { get; set; } = 500;
        public List<string>   SelectedColumns  { get; set; } = new();
        // Each element is a simple "SrcField|SrcType|DstField|DstType" token
        public List<string>   MappingRows      { get; set; } = new();
        public DateTime       SavedAt          { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Lightweight helper — no DI required.
    /// Stores templates as JSON files under  &lt;AppData&gt;/BeepImport/ImportTemplates/
    /// </summary>
    public static class ImportTemplateManager
    {
        private static readonly JsonSerializerOptions _json =
            new JsonSerializerOptions { WriteIndented = true };

        // ── Storage root ───────────────────────────────────────────────────────
        private static string TemplateDir
        {
            get
            {
                // Prefer %AppData%/BeepImport; fall back gracefully.
                var root = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "BeepImport", "ImportTemplates");
                if (!Directory.Exists(root))
                    Directory.CreateDirectory(root);
                return root;
            }
        }

        private static string FilePath(string name)
            => Path.Combine(TemplateDir, SanitiseName(name) + ".json");

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Returns the names of all saved templates, ordered by save-date desc.</summary>
        public static IReadOnlyList<string> ListAll()
        {
            try
            {
                return Directory.EnumerateFiles(TemplateDir, "*.json")
                    .Select(f =>
                    {
                        try
                        {
                            var dto = Deserialise(f);
                            return (Name: dto?.Name ?? Path.GetFileNameWithoutExtension(f),
                                    SavedAt: dto?.SavedAt ?? File.GetLastWriteTimeUtc(f));
                        }
                        catch { return (Name: Path.GetFileNameWithoutExtension(f), SavedAt: File.GetLastWriteTimeUtc(f)); }
                    })
                    .OrderByDescending(t => t.SavedAt)
                    .Select(t => t.Name)
                    .ToList();
            }
            catch { return Array.Empty<string>(); }
        }

        /// <summary>Persists a template DTO under the given name (overwrites existing).</summary>
        public static void Save(ImportTemplateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            dto.SavedAt = DateTime.UtcNow;
            File.WriteAllText(FilePath(dto.Name), JsonSerializer.Serialize(dto, _json));
        }

        /// <summary>Creates and saves a template from the current mapping rows + context values.</summary>
        public static void Save(string name,
                                 List<ImportFieldMapRow> rows,
                                 ImportPurpose purpose     = ImportPurpose.AddOnly,
                                 string matchByField       = "",
                                 bool   updateEmptyFields  = false,
                                 int    batchSize          = 500,
                                 List<string>? selectedCols = null)
        {
            var dto = new ImportTemplateDto
            {
                Name             = name,
                Purpose          = purpose,
                MatchByField     = matchByField,
                UpdateEmptyFields= updateEmptyFields,
                BatchSize        = batchSize,
                SelectedColumns  = selectedCols ?? new List<string>(),
                MappingRows      = rows
                    .Where(r => r.Selected && !string.IsNullOrWhiteSpace(r.DestinationField))
                    .Select(r => $"{r.SourceField}|{r.SourceType}|{r.DestinationField}|{r.DestinationType}|{r.Transform}")
                    .ToList()
            };
            Save(dto);
        }

        /// <summary>Loads a template by name; returns null if not found.</summary>
        public static ImportTemplateDto? Load(string name)
        {
            var path = FilePath(name);
            if (!File.Exists(path)) return null;
            return Deserialise(path);
        }

        /// <summary>Applies a loaded template to a list of map rows (matches by source field name).</summary>
        public static void ApplyToRows(List<ImportFieldMapRow> rows, ImportTemplateDto dto)
        {
            // Build lookup from template serialised rows
            var lookup = new Dictionary<string, (string SrcType, string DstField, string DstType, string Transform)>(
                StringComparer.OrdinalIgnoreCase);
            foreach (var token in dto.MappingRows)
            {
                var parts = token.Split('|');
                if (parts.Length >= 4)
                    lookup[parts[0]] = (parts[1], parts[2], parts[3], parts.Length > 4 ? parts[4] : string.Empty);
            }

            foreach (var row in rows)
            {
                if (lookup.TryGetValue(row.SourceField, out var m))
                {
                    row.Selected         = true;
                    row.DestinationField = m.DstField;
                    row.DestinationType  = m.DstType;
                    row.Transform        = m.Transform;
                }
                else
                {
                    row.Selected = false;
                }
            }
        }

        /// <summary>Deletes a template by name — silently ignores missing files.</summary>
        public static void Delete(string name)
        {
            var path = FilePath(name);
            if (File.Exists(path)) File.Delete(path);
        }

        /// <summary>
        /// Applies a loaded template DTO directly to a <see cref="DataImportConfiguration"/>.
        /// Populates <c>Mapping</c>, <c>BatchSize</c>, and other scalar options.
        /// </summary>
        public static void ApplyToConfig(ImportTemplateDto dto, DataImportConfiguration config)
        {
            if (dto == null || config == null) return;

            // Scalar options
            config.BatchSize = dto.BatchSize > 0 ? dto.BatchSize : config.BatchSize;

            // Build field-mapping structure
            var fieldMappings = new List<Mapping_rep_fields>();
            foreach (var token in dto.MappingRows)
            {
                var parts = token.Split('|');
                if (parts.Length < 4) continue;
                fieldMappings.Add(new Mapping_rep_fields
                {
                    FromEntityName = config.SourceEntityName,
                    FromFieldName  = parts[0],
                    FromFieldType  = parts[1],
                    ToEntityName   = config.DestEntityName,
                    ToFieldName    = parts[2],
                    ToFieldType    = parts[3],
                    Rules          = parts.Length > 4 ? parts[4] : string.Empty
                });
            }

            if (fieldMappings.Count > 0)
            {
                var dtl = new EntityDataMap_DTL
                {
                    EntityName       = config.DestEntityName,
                    EntityDataSource = config.DestDataSourceName,
                    FieldMapping     = fieldMappings
                };
                config.Mapping = config.Mapping ?? new EntityDataMap();
                config.Mapping.MappingName    = dto.Name;
                config.Mapping.EntityName     = config.DestEntityName;
                config.Mapping.EntityDataSource = config.DestDataSourceName;
                config.Mapping.MappedEntities = new List<EntityDataMap_DTL> { dtl };
            }

            // Selected columns
            if (dto.SelectedColumns?.Count > 0)
                config.SelectedFields = dto.SelectedColumns;
        }

        // ── Helpers ────────────────────────────────────────────────────────────
        private static string SanitiseName(string name)
            => string.Join("_", name.Split(Path.GetInvalidFileNameChars()));

        private static ImportTemplateDto? Deserialise(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ImportTemplateDto>(json, _json);
        }
    }
}
