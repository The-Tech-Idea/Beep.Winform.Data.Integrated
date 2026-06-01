using System;
using System.Collections.Generic;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models
{
    public static class BeepBlockFieldDefinitionStateHelper
    {
        public const string ExplicitEmptyFieldsMetadataKey = "ExplicitEmptyFields";

        public static bool HasExplicitFieldDefinitions(BeepBlockDefinition? definition)
        {
            if (definition?.Fields != null && definition.Fields.Count > 0)
            {
                return true;
            }

            return IsExplicitlyEmpty(definition);
        }

        public static bool IsExplicitlyEmpty(BeepBlockDefinition? definition)
        {
            return definition?.Metadata != null
                && definition.Metadata.TryGetValue(ExplicitEmptyFieldsMetadataKey, out string? rawValue)
                && bool.TryParse(rawValue, out bool explicitEmpty)
                && explicitEmpty;
        }

        public static void UpdateExplicitFieldState(BeepBlockDefinition? definition, bool treatEmptyAsExplicit = false)
        {
            if (definition == null)
            {
                return;
            }

            if (definition.Fields != null && definition.Fields.Count > 0)
            {
                ClearExplicitEmpty(definition);
                return;
            }

            if (treatEmptyAsExplicit || IsExplicitlyEmpty(definition))
            {
                SetExplicitEmpty(definition, true);
                return;
            }

            ClearExplicitEmpty(definition);
        }

        public static void SetExplicitEmpty(BeepBlockDefinition? definition, bool isExplicitEmpty)
        {
            if (definition == null)
            {
                return;
            }

            definition.Metadata ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (isExplicitEmpty)
            {
                definition.Metadata[ExplicitEmptyFieldsMetadataKey] = bool.TrueString;
                return;
            }

            ClearExplicitEmpty(definition);
        }

        private static void ClearExplicitEmpty(BeepBlockDefinition definition)
        {
            if (definition.Metadata == null)
            {
                return;
            }

            definition.Metadata.Remove(ExplicitEmptyFieldsMetadataKey);
        }
    }
}