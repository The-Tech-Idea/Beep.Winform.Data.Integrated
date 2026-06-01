using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows.Forms;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;
using TheTechIdea.Beep.Winform.Controls.Dates;
using TheTechIdea.Beep.Winform.Controls.Numerics;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Services
{
    public static class BeepFieldControlTypeRegistry
    {
        private static readonly Dictionary<string, Type> KnownControlTypes = BuildKnownControlTypes();
        private static readonly object PolicySyncRoot = new();
        private static readonly JsonSerializerOptions PolicyJsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private static BeepFieldControlTypePolicy? _policy;

        public static string DefaultPolicyFilePath
        {
            get
            {
                string baseDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "TheTechIdea",
                    "Beep.Winform");
                return Path.Combine(baseDirectory, "field-control-defaults.json");
            }
        }

        public static string ResolveDefaultEditorKey(DbFieldCategory category, string? dataType = null, bool isCheck = false, bool isLong = false)
        {
            return ResolveDefaultFieldSettings(category, dataType, isCheck, isLong).EditorKey;
        }

        public static BeepFieldControlTypeDefaultResolution ResolveDefaultFieldSettings(DbFieldCategory category, string? dataType = null, bool isCheck = false, bool isLong = false)
        {
            string builtInEditorKey = ResolveBuiltInDefaultEditorKey(category, dataType, isCheck, isLong);
            var policyRule = ResolvePolicyRule(category, dataType, isCheck, builtInEditorKey);
            string editorKey = string.IsNullOrWhiteSpace(policyRule?.EditorKey)
                ? builtInEditorKey
                : policyRule!.EditorKey.Trim();

            string controlType = string.IsNullOrWhiteSpace(policyRule?.ControlType)
                ? ResolveDefaultControlType(editorKey)
                : policyRule!.ControlType.Trim();

            string bindingProperty = string.IsNullOrWhiteSpace(policyRule?.BindingProperty)
                ? ResolveBuiltInDefaultBindingProperty(controlType, editorKey)
                : policyRule!.BindingProperty.Trim();

            return new BeepFieldControlTypeDefaultResolution(editorKey, controlType, bindingProperty, policyRule?.Clone());
        }

        private static string ResolveBuiltInDefaultEditorKey(DbFieldCategory category, string? dataType = null, bool isCheck = false, bool isLong = false)
        {
            string typeName = dataType ?? string.Empty;

            if (isCheck ||
                category == DbFieldCategory.Boolean ||
                typeName.IndexOf("bool", StringComparison.OrdinalIgnoreCase) >= 0 ||
                typeName.IndexOf("bit", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "checkbox";
            }

            if (category == DbFieldCategory.Enum)
            {
                return "combo";
            }

            if (category == DbFieldCategory.Binary ||
                typeName.IndexOf("byte[]", StringComparison.OrdinalIgnoreCase) >= 0 ||
                typeName.IndexOf("binary", StringComparison.OrdinalIgnoreCase) >= 0 ||
                typeName.IndexOf("image", StringComparison.OrdinalIgnoreCase) >= 0 ||
                typeName.IndexOf("blob", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "blob";
            }

            // IsLong (TEXT/CLOB) short-circuit — must come before generic date/numeric checks
            if (isLong ||
                category == DbFieldCategory.Text ||
                typeName.IndexOf("clob", StringComparison.OrdinalIgnoreCase) >= 0 ||
                typeName.IndexOf("ntext", StringComparison.OrdinalIgnoreCase) >= 0 ||
                typeName.IndexOf("text", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "memo";
            }

            if (category == DbFieldCategory.DateTime ||
                typeName.IndexOf("datetime", StringComparison.OrdinalIgnoreCase) >= 0 ||
                typeName.IndexOf("timestamp", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "datetime";
            }

            if (category == DbFieldCategory.Date ||
                typeName.IndexOf("date", StringComparison.OrdinalIgnoreCase) >= 0 ||
                typeName.IndexOf("time", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "date";
            }

            if (category == DbFieldCategory.Numeric ||
                category == DbFieldCategory.Integer ||
                category == DbFieldCategory.Decimal ||
                category == DbFieldCategory.Double ||
                category == DbFieldCategory.Float ||
                category == DbFieldCategory.Currency ||
                typeName.IndexOf("int", StringComparison.OrdinalIgnoreCase) >= 0 ||
                typeName.IndexOf("decimal", StringComparison.OrdinalIgnoreCase) >= 0 ||
                typeName.IndexOf("double", StringComparison.OrdinalIgnoreCase) >= 0 ||
                typeName.IndexOf("float", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "numeric";
            }

            return "text";
        }

        public static string ResolveDefaultControlType(string? editorKey)
        {
            string normalizedEditorKey = (editorKey ?? string.Empty).Trim();
            var policyRule = ResolveEditorOnlyPolicyRule(normalizedEditorKey);
            if (!string.IsNullOrWhiteSpace(policyRule?.ControlType))
            {
                return policyRule!.ControlType.Trim();
            }

            return ResolveBuiltInDefaultControlType(normalizedEditorKey);
        }

        private static string ResolveBuiltInDefaultControlType(string? editorKey)
        {
            return (editorKey ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "checkbox" => nameof(BeepCheckBoxBool),
                "date" => nameof(BeepDatePicker),
                "datetime" => nameof(BeepDateTimePicker),
                "numeric" => nameof(BeepNumericUpDown),
                "memo" => nameof(BeepTextBox),         // multiline mode; presenter sets Multiline = true
                "blob" => nameof(BeepTextBox),         // placeholder until BeepBlobViewer is available
                "combo" or "lov" or "option" => nameof(BeepComboBox),
                _ => nameof(BeepTextBox)
            };
        }

        public static string ResolveDefaultControlType(DbFieldCategory category, string? dataType = null, bool isCheck = false, bool isLong = false)
        {
            return ResolveDefaultFieldSettings(category, dataType, isCheck, isLong).ControlType;
        }

        public static string ResolveDefaultBindingProperty(string? controlType, string? editorKey = null)
        {
            var editorOnlyRule = ResolveEditorOnlyPolicyRule((editorKey ?? string.Empty).Trim());
            string normalizedControlType = SimplifyControlTypeName(controlType);
            if (string.IsNullOrWhiteSpace(normalizedControlType))
            {
                normalizedControlType = !string.IsNullOrWhiteSpace(editorOnlyRule?.ControlType)
                    ? SimplifyControlTypeName(editorOnlyRule!.ControlType)
                    : SimplifyControlTypeName(ResolveDefaultControlType(editorKey));
            }

            if (!string.IsNullOrWhiteSpace(editorOnlyRule?.BindingProperty) &&
                (string.IsNullOrWhiteSpace(editorOnlyRule.ControlType) ||
                 string.Equals(SimplifyControlTypeName(editorOnlyRule.ControlType), normalizedControlType, StringComparison.OrdinalIgnoreCase)))
            {
                return editorOnlyRule.BindingProperty.Trim();
            }

            return ResolveBuiltInDefaultBindingProperty(normalizedControlType, editorKey);
        }

        public static string ResolveDefaultBindingProperty(DbFieldCategory category, string? dataType = null, bool isCheck = false)
        {
            return ResolveDefaultFieldSettings(category, dataType, isCheck).BindingProperty;
        }

        private static string ResolveBuiltInDefaultBindingProperty(string? controlType, string? editorKey = null)
        {
            string normalizedControlType = SimplifyControlTypeName(controlType);
            if (string.IsNullOrWhiteSpace(normalizedControlType))
            {
                normalizedControlType = SimplifyControlTypeName(ResolveBuiltInDefaultControlType(editorKey));
            }

            return normalizedControlType switch
            {
                nameof(BeepCheckBoxBool) => nameof(BeepCheckBoxBool.CurrentValue),
                nameof(CheckBox) => nameof(CheckBox.Checked),
                nameof(BeepDatePicker) => nameof(BeepDatePicker.SelectedDateTime),
                nameof(BeepDateTimePicker) => nameof(BeepDateTimePicker.SelectedDateTime),
                nameof(DateTimePicker) => nameof(DateTimePicker.Value),
                nameof(BeepNumericUpDown) => nameof(BeepNumericUpDown.Value),
                nameof(NumericUpDown) => nameof(NumericUpDown.Value),
                nameof(BeepComboBox) => nameof(BeepComboBox.SelectedValue),
                nameof(ComboBox) => nameof(ComboBox.SelectedValue),
                nameof(MaskedTextBox) => nameof(MaskedTextBox.Text),
                _ => nameof(Control.Text)
            };
        }

        public static IReadOnlyList<string> GetKnownControlTypes()
        {
            var knownControlTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string key in KnownControlTypes.Keys
                .Where(key => !string.IsNullOrWhiteSpace(key) && !key.Contains('.'))
                .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                knownControlTypes.Add(key);
            }

            foreach (var policyRule in GetPolicyRules())
            {
                if (!string.IsNullOrWhiteSpace(policyRule.ControlType))
                {
                    knownControlTypes.Add(SimplifyControlTypeName(policyRule.ControlType));
                }
            }

            return knownControlTypes
                .OrderBy(key => key, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static IReadOnlyList<string> GetKnownBindingProperties(string? controlType = null)
        {
            var properties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                nameof(Control.Text),
                nameof(ComboBox.Text),
                nameof(ComboBox.SelectedValue),
                nameof(CheckBox.Checked),
                nameof(DateTimePicker.Value),
                nameof(NumericUpDown.Value),
                nameof(BeepCheckBoxBool.CurrentValue),
                nameof(BeepDatePicker.SelectedDateTime),
                nameof(BeepComboBox.SelectedValue)
            };

            foreach (var policyRule in GetPolicyRules())
            {
                if (!string.IsNullOrWhiteSpace(policyRule.BindingProperty))
                {
                    properties.Add(policyRule.BindingProperty.Trim());
                }
            }

            var resolvedType = ResolveControlType(controlType);
            if (resolvedType != null)
            {
                foreach (var property in resolvedType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(property => property.CanRead && property.CanWrite && property.GetIndexParameters().Length == 0))
                {
                    properties.Add(property.Name);
                }
            }

            return properties
                .OrderBy(property => property, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static IReadOnlyList<BeepFieldControlTypePolicyRule> GetPolicyRules()
        {
            lock (PolicySyncRoot)
            {
                return EnsurePolicyLoaded().Rules
                    .Select(rule => rule.Clone())
                    .ToList();
            }
        }

        public static bool SavePolicyRules(IEnumerable<BeepFieldControlTypePolicyRule>? rules, out string message)
        {
            try
            {
                var policy = new BeepFieldControlTypePolicy();
                if (rules != null)
                {
                    policy.Rules.AddRange(rules.Where(rule => rule != null).Select(rule => rule.Clone()));
                }

                string? directory = Path.GetDirectoryName(DefaultPolicyFilePath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(DefaultPolicyFilePath, JsonSerializer.Serialize(policy, PolicyJsonOptions));
                lock (PolicySyncRoot)
                {
                    _policy = policy;
                }

                message = $"Saved {policy.Rules.Count} field-control policy rule(s) to '{DefaultPolicyFilePath}'.";
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public static bool ReloadPolicy(out string message)
        {
            try
            {
                lock (PolicySyncRoot)
                {
                    _policy = LoadPolicyFromDisk();
                }

                message = File.Exists(DefaultPolicyFilePath)
                    ? $"Reloaded field-control policy from '{DefaultPolicyFilePath}'."
                    : "Field-control policy reset to built-in defaults because no persisted policy file was found.";
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public static bool ResetPolicyRules(out string message)
        {
            try
            {
                if (File.Exists(DefaultPolicyFilePath))
                {
                    File.Delete(DefaultPolicyFilePath);
                }

                lock (PolicySyncRoot)
                {
                    _policy = new BeepFieldControlTypePolicy();
                }

                message = "Field-control policy reverted to built-in defaults.";
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public static Type? ResolveControlType(string? controlTypeName)
        {
            if (string.IsNullOrWhiteSpace(controlTypeName))
            {
                return null;
            }

            if (KnownControlTypes.TryGetValue(controlTypeName.Trim(), out var knownType))
            {
                return knownType;
            }

            var resolvedType = Type.GetType(controlTypeName.Trim(), throwOnError: false, ignoreCase: true);
            if (resolvedType != null && typeof(Control).IsAssignableFrom(resolvedType))
            {
                return resolvedType;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    resolvedType = assembly.GetTypes().FirstOrDefault(type =>
                        typeof(Control).IsAssignableFrom(type) &&
                        (string.Equals(type.FullName, controlTypeName, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(type.Name, controlTypeName, StringComparison.OrdinalIgnoreCase)));
                }
                catch (ReflectionTypeLoadException ex)
                {
                    resolvedType = ex.Types.FirstOrDefault(type =>
                        type != null &&
                        typeof(Control).IsAssignableFrom(type) &&
                        (string.Equals(type.FullName, controlTypeName, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(type.Name, controlTypeName, StringComparison.OrdinalIgnoreCase)));
                }

                if (resolvedType != null)
                {
                    return resolvedType;
                }
            }

            return null;
        }

        public static string SimplifyControlTypeName(string? controlTypeName)
        {
            if (string.IsNullOrWhiteSpace(controlTypeName))
            {
                return string.Empty;
            }

            if (KnownControlTypes.TryGetValue(controlTypeName.Trim(), out var knownType))
            {
                return knownType.Name;
            }

            string normalized = controlTypeName.Trim();
            int lastSeparator = normalized.LastIndexOf('.');
            return lastSeparator >= 0 && lastSeparator < normalized.Length - 1
                ? normalized.Substring(lastSeparator + 1)
                : normalized;
        }

        private static Dictionary<string, Type> BuildKnownControlTypes()
        {
            var known = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

            Register(known, typeof(BeepTextBox), nameof(BeepTextBox));
            Register(known, typeof(BeepComboBox), nameof(BeepComboBox));
            Register(known, typeof(BeepDatePicker), nameof(BeepDatePicker));
            Register(known, typeof(BeepDateTimePicker), nameof(BeepDateTimePicker));
            Register(known, typeof(BeepCheckBoxBool), nameof(BeepCheckBoxBool));
            Register(known, typeof(BeepNumericUpDown), nameof(BeepNumericUpDown));
            Register(known, typeof(TextBox), nameof(TextBox));
            Register(known, typeof(MaskedTextBox), nameof(MaskedTextBox));
            Register(known, typeof(ComboBox), nameof(ComboBox));
            Register(known, typeof(CheckBox), nameof(CheckBox));
            Register(known, typeof(DateTimePicker), nameof(DateTimePicker));
            Register(known, typeof(NumericUpDown), nameof(NumericUpDown));

            return known;
        }

        private static void Register(IDictionary<string, Type> registry, Type controlType, string alias)
        {
            registry[alias] = controlType;
            registry[controlType.Name] = controlType;

            if (!string.IsNullOrWhiteSpace(controlType.FullName))
            {
                registry[controlType.FullName] = controlType;
            }

            if (!string.IsNullOrWhiteSpace(controlType.AssemblyQualifiedName))
            {
                registry[controlType.AssemblyQualifiedName] = controlType;
            }
        }

        private static BeepFieldControlTypePolicy EnsurePolicyLoaded()
        {
            lock (PolicySyncRoot)
            {
                _policy ??= LoadPolicyFromDisk();
                return _policy;
            }
        }

        private static BeepFieldControlTypePolicy LoadPolicyFromDisk()
        {
            if (!File.Exists(DefaultPolicyFilePath))
            {
                return new BeepFieldControlTypePolicy();
            }

            var policy = JsonSerializer.Deserialize<BeepFieldControlTypePolicy>(File.ReadAllText(DefaultPolicyFilePath), PolicyJsonOptions);
            return policy ?? new BeepFieldControlTypePolicy();
        }

        private static BeepFieldControlTypePolicyRule? ResolvePolicyRule(DbFieldCategory category, string? dataType, bool isCheck, string editorKey)
        {
            return EnsurePolicyLoaded().Rules
                .Where(rule => rule != null && rule.Matches(category, dataType, isCheck, editorKey))
                .OrderByDescending(rule => rule.GetSpecificityScore())
                .ThenBy(rule => rule.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
        }

        private static BeepFieldControlTypePolicyRule? ResolveEditorOnlyPolicyRule(string editorKey)
        {
            if (string.IsNullOrWhiteSpace(editorKey))
            {
                return null;
            }

            return EnsurePolicyLoaded().Rules
                .Where(rule => rule != null && rule.MatchesEditorKeyOnly(editorKey))
                .OrderByDescending(rule => rule.GetSpecificityScore())
                .ThenBy(rule => rule.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
        }
    }
}