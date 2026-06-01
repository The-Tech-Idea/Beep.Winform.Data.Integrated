using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Editor.Importing;
using TheTechIdea.Beep.Editor.Importing.Interfaces;
using TheTechIdea.Beep.Winform.Controls.Wizards;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    /// <summary>
    /// Validates that data persisted through the Import/Export wizard steps
    /// is consistent and complete.  Called from step <c>OnStepEnter</c>
    /// handlers to guard against context corruption when users navigate
    /// backwards and forwards.
    /// </summary>
    public static class ImportExportWizardValidation
    {
        /// <summary>
        /// Performs a full end-to-end validation of the wizard context.
        /// Returns <c>true</c> when every expected key is present and
        /// non-empty; otherwise returns <c>false</c> and writes
        /// diagnostics to <paramref name="diagnostics"/>.
        /// </summary>
        public static bool ValidateContextIntegrity(
            WizardContext context,
            out List<string> diagnostics)
        {
            diagnostics = new List<string>();
            bool ok = true;

            // ── Step 1: SelectDSandEntity ──
            var config = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null);
            if (config == null)
            {
                diagnostics.Add("[Step-1] ImportConfig is missing from context.");
                ok = false;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(config.SourceDataSourceName))
                    diagnostics.Add("[Step-1] SourceDataSourceName is empty.");
                if (string.IsNullOrWhiteSpace(config.SourceEntityName))
                    diagnostics.Add("[Step-1] SourceEntityName is empty.");
                if (string.IsNullOrWhiteSpace(config.DestDataSourceName))
                    diagnostics.Add("[Step-1] DestDataSourceName is empty.");
                if (string.IsNullOrWhiteSpace(config.DestEntityName))
                    diagnostics.Add("[Step-1] DestEntityName is empty.");
            }

            var purpose = context.GetValue<ImportPurpose?>(WizardKeys.Purpose, null);
            if (purpose == null)
                diagnostics.Add("[Step-1] Purpose key is missing.");

            // ── Step 2: ColumnSelection ──
            var selectedCols = context.GetValue<List<string>?>(WizardKeys.SelectedColumns, null);
            if (selectedCols == null)
                diagnostics.Add("[Step-2] SelectedColumns key is missing (will default to all).");

            // ── Step 3: MapFields ──
            var mapping = config?.Mapping;
            if (mapping == null)
            {
                diagnostics.Add("[Step-3] Field Mapping is missing from ImportConfig.");
                ok = false;
            }
            else
            {
                int mappedEntities = mapping.MappedEntities?.Count ?? 0;
                int mappedFields = mapping.MappedEntities?
                    .SelectMany(d => d.FieldMapping ?? new List<TheTechIdea.Beep.Workflow.Mapping_rep_fields>())
                    .Count() ?? 0;

                if (mappedEntities == 0)
                    diagnostics.Add("[Step-3] Mapping has zero destination entities.");
                if (mappedFields == 0)
                    diagnostics.Add("[Step-3] Mapping has zero mapped fields.");
            }

            // ── Step 4: Options ──
            var batchSize = context.GetValue<int?>(WizardKeys.BatchSize, null);
            if (batchSize == null)
                diagnostics.Add("[Step-4] BatchSize key is missing (will default to 50).");

            var runValidation = context.GetValue<bool?>(WizardKeys.RunValidation, null);
            if (runValidation == null)
                diagnostics.Add("[Step-4] RunValidation key is missing (will default to true).");

            // ── Cross-step consistency ──
            if (config != null && selectedCols != null && selectedCols.Count > 0)
            {
                // Ensure every selected column appears at least once in the mapping
                var mappedSrcFields = mapping?.MappedEntities?
                    .SelectMany(d => d.FieldMapping ?? new List<TheTechIdea.Beep.Workflow.Mapping_rep_fields>())
                    .Select(f => f.FromFieldName)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase)
                    ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var orphaned = selectedCols
                    .Where(c => !mappedSrcFields.Contains(c))
                    .ToList();

                if (orphaned.Count > 0)
                    diagnostics.Add($"[Cross-step] Selected columns not present in mapping: {string.Join(", ", orphaned)}.");
            }

            return ok && diagnostics.Count == 0;
        }

        /// <summary>
        /// Validates specifically that the mapping created in Step 3
        /// round-trips correctly into Steps 4 and 5.
        /// </summary>
        public static bool ValidateMappingRoundtrip(
            WizardContext context,
            out List<string> diagnostics)
        {
            diagnostics = new List<string>();
            var config = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null);
            if (config?.Mapping == null)
            {
                diagnostics.Add("Mapping is absent from wizard context.");
                return false;
            }

            var mapping = config.Mapping;
            bool ok = true;

            if (string.IsNullOrWhiteSpace(mapping.MappingName))
                diagnostics.Add("MappingName is empty — usually computed as 'Src_to_Dst'.");

            if (string.IsNullOrWhiteSpace(mapping.EntityName))
                diagnostics.Add("Mapping.EntityName (source) is empty.");

            if (string.IsNullOrWhiteSpace(mapping.EntityDataSource))
                diagnostics.Add("Mapping.EntityDataSource (source) is empty.");

            if (mapping.MappedEntities == null || mapping.MappedEntities.Count == 0)
            {
                diagnostics.Add("MappedEntities collection is empty.");
                ok = false;
            }
            else
            {
                foreach (var dtl in mapping.MappedEntities)
                {
                    if (string.IsNullOrWhiteSpace(dtl.EntityName))
                        diagnostics.Add($"MappedEntity destination name is empty.");
                    if (string.IsNullOrWhiteSpace(dtl.EntityDataSource))
                        diagnostics.Add($"MappedEntity destination data source is empty.");
                    if (dtl.FieldMapping == null || dtl.FieldMapping.Count == 0)
                        diagnostics.Add($"MappedEntity '{dtl.EntityName}' has no FieldMapping entries.");
                }
            }

            // Verify config scalar values align with mapping source/dest
            if (!string.Equals(config.SourceEntityName, mapping.EntityName, StringComparison.OrdinalIgnoreCase))
                diagnostics.Add($"Config SourceEntityName ('{config.SourceEntityName}') != Mapping.EntityName ('{mapping.EntityName}').");

            if (!string.Equals(config.SourceDataSourceName, mapping.EntityDataSource, StringComparison.OrdinalIgnoreCase))
                diagnostics.Add($"Config SourceDataSourceName ('{config.SourceDataSourceName}') != Mapping.EntityDataSource ('{mapping.EntityDataSource}').");

            return ok && diagnostics.Count == 0;
        }

        /// <summary>
        /// Validates that the <see cref="DataImportConfiguration.CreateDestinationIfNotExists"/>
        /// flag and related entity auto-create settings are well-formed and
        /// consistent with the wizard context.
        /// </summary>
        public static bool ValidateAutoCreateSettings(
            WizardContext context,
            out List<string> diagnostics)
        {
            diagnostics = new List<string>();
            var config = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null);
            if (config == null)
            {
                diagnostics.Add("ImportConfig missing — cannot validate auto-create.");
                return false;
            }

            bool ok = true;

            if (string.IsNullOrWhiteSpace(config.DestDataSourceName))
            {
                diagnostics.Add("Destination data source is not set — auto-create cannot function.");
                ok = false;
            }

            if (string.IsNullOrWhiteSpace(config.DestEntityName))
            {
                diagnostics.Add("Destination entity name is not set — auto-create cannot function.");
                ok = false;
            }

            // The checkbox from Step 1 is mapped to config.CreateDestinationIfNotExists
            if (!config.CreateDestinationIfNotExists)
                diagnostics.Add("CreateDestinationIfNotExists is FALSE — destination must already exist.");

            // AddMissingColumns is also relevant
            if (!config.AddMissingColumns)
                diagnostics.Add("AddMissingColumns is FALSE — schema drift will not be handled.");

            return ok && diagnostics.Count == 0;
        }
    }
}
