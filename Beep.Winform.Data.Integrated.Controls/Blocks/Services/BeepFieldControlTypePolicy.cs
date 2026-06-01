using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TheTechIdea.Beep.Utilities;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Services
{
    public sealed class BeepFieldControlTypePolicy
    {
        public int Version { get; set; } = 1;

        public List<BeepFieldControlTypePolicyRule> Rules { get; set; } = new();
    }

    public sealed class BeepFieldControlTypePolicyRule
    {
        public string Name { get; set; } = string.Empty;

        public DbFieldCategory? Category { get; set; }

        public string DataTypePattern { get; set; } = string.Empty;

        public bool? IsCheck { get; set; }

        public string EditorKey { get; set; } = string.Empty;

        public string ControlType { get; set; } = string.Empty;

        public string BindingProperty { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;

        public bool Matches(DbFieldCategory category, string? dataType, bool isCheck, string editorKey)
        {
            if (!IsEnabled)
            {
                return false;
            }

            if (Category.HasValue && Category.Value != category)
            {
                return false;
            }

            if (IsCheck.HasValue && IsCheck.Value != isCheck)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(EditorKey) &&
                !string.Equals(EditorKey.Trim(), editorKey, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(DataTypePattern))
            {
                return true;
            }

            string candidateType = (dataType ?? string.Empty).Trim();
            if (candidateType.Length == 0)
            {
                return false;
            }

            string pattern = DataTypePattern.Trim();
            if (pattern.IndexOfAny(['*', '?']) >= 0)
            {
                string regexPattern = "^" + Regex.Escape(pattern)
                    .Replace("\\*", ".*")
                    .Replace("\\?", ".") + "$";
                return Regex.IsMatch(candidateType, regexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }

            return string.Equals(candidateType, pattern, StringComparison.OrdinalIgnoreCase)
                || candidateType.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public bool MatchesEditorKeyOnly(string editorKey)
        {
            if (!IsEnabled || string.IsNullOrWhiteSpace(EditorKey))
            {
                return false;
            }

            if (Category.HasValue || IsCheck.HasValue || !string.IsNullOrWhiteSpace(DataTypePattern))
            {
                return false;
            }

            return string.Equals(EditorKey.Trim(), editorKey, StringComparison.OrdinalIgnoreCase);
        }

        public int GetSpecificityScore()
        {
            int score = 0;

            if (Category.HasValue)
            {
                score += 8;
            }

            if (IsCheck.HasValue)
            {
                score += 4;
            }

            if (!string.IsNullOrWhiteSpace(EditorKey))
            {
                score += 2;
            }

            if (!string.IsNullOrWhiteSpace(DataTypePattern))
            {
                score += 1 + DataTypePattern.Trim().Length;
            }

            return score;
        }

        public BeepFieldControlTypePolicyRule Clone()
        {
            return new BeepFieldControlTypePolicyRule
            {
                Name = Name,
                Category = Category,
                DataTypePattern = DataTypePattern,
                IsCheck = IsCheck,
                EditorKey = EditorKey,
                ControlType = ControlType,
                BindingProperty = BindingProperty,
                IsEnabled = IsEnabled
            };
        }
    }

    public readonly record struct BeepFieldControlTypeDefaultResolution(
        string EditorKey,
        string ControlType,
        string BindingProperty,
        BeepFieldControlTypePolicyRule? Rule = null);
}