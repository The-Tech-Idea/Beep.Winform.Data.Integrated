using TheTechIdea.Beep.Winform.Default.Views.ImportExport.Models;

namespace TheTechIdea.Beep.Winform.Default.Views.ImportExport
{
    public static class ImportExportWizardValidation
    {
        public static bool ValidateContextIntegrity(WizardContext context, out List<string> diagnostics)
        {
            diagnostics = new List<string>();
            bool ok = true;

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

            var selectedCols = context.GetValue<List<string>?>(WizardKeys.SelectedColumns, null);
            if (selectedCols == null)
                diagnostics.Add("[Step-2] SelectedColumns key is missing (will default to all).");

            var mapping = config?.Mapping;
            if (mapping == null)
            {
                diagnostics.Add("[Step-3] Field Mapping is missing from ImportConfig.");
                ok = false;
            }
            else
            {
                int mappedFields = mapping.MappedEntities?
                    .SelectMany(d => d.FieldMapping ?? new List<Mapping_rep_fields>())
                    .Count() ?? 0;

                if (mappedFields == 0)
                    diagnostics.Add("[Step-3] Mapping has zero mapped fields.");
            }

            var batchSize = context.GetValue<int?>(WizardKeys.BatchSize, null);
            if (batchSize == null)
                diagnostics.Add("[Step-4] BatchSize key is missing (will default to 50).");

            return ok && diagnostics.Count == 0;
        }

        public static bool ValidateMappingRoundtrip(WizardContext context, out List<string> diagnostics)
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
                diagnostics.Add("MappingName is empty.");

            if (mapping.MappedEntities == null || mapping.MappedEntities.Count == 0)
            {
                diagnostics.Add("MappedEntities collection is empty.");
                ok = false;
            }
            else
            {
                foreach (var dtl in mapping.MappedEntities)
                {
                    if (dtl.FieldMapping == null || dtl.FieldMapping.Count == 0)
                        diagnostics.Add($"MappedEntity '{dtl.EntityName}' has no FieldMapping entries.");
                }
            }

            if (!string.Equals(config.SourceEntityName, mapping.EntityName, StringComparison.OrdinalIgnoreCase))
                diagnostics.Add($"Config SourceEntityName != Mapping.EntityName.");

            return ok && diagnostics.Count == 0;
        }

        public static bool ValidateAutoCreateSettings(WizardContext context, out List<string> diagnostics)
        {
            diagnostics = new List<string>();
            var config = context.GetValue<DataImportConfiguration?>(WizardKeys.ImportConfig, null);
            if (config == null)
            {
                diagnostics.Add("ImportConfig missing.");
                return false;
            }

            bool ok = true;
            if (string.IsNullOrWhiteSpace(config.DestDataSourceName)) { diagnostics.Add("Destination data source is not set."); ok = false; }
            if (string.IsNullOrWhiteSpace(config.DestEntityName)) { diagnostics.Add("Destination entity name is not set."); ok = false; }
            return ok;
        }
    }
}
