using System;
using System.Globalization;
using System.Text;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Services
{
    /// <summary>
    /// Translates Oracle Forms <c>FORMAT_MASK</c> strings (e.g.
    /// <c>$999,999.00</c>, <c>DD-MON-YYYY</c>, <c>999G999G999D99</c>) into
    /// a representation the .NET editing controls can consume.
    /// <para>
    /// Beep only translates a subset — the masks Forms developers use in
    /// 95% of forms. Anything that does not map cleanly returns the
    /// input unchanged so the runtime falls back to the field's plain
    /// text behaviour.
    /// </para>
    /// </summary>
    public static class BeepFormatMaskTranslator
    {
        /// <summary>
        /// Convert a Forms format mask into a dot-net-friendly mask that
        /// <see cref="System.Windows.Forms.MaskedTextBox.Mask"/> accepts.
        /// Returns <c>string.Empty</c> when the mask cannot be translated
        /// (so callers can short-circuit instead of assigning garbage).
        /// </summary>
        public static string ToDotNetMask(string? formsMask)
        {
            if (string.IsNullOrWhiteSpace(formsMask))
            {
                return string.Empty;
            }

            string mask = formsMask.Trim();

            // Date masks
            if (mask.Equals("DD-MON-YYYY", StringComparison.OrdinalIgnoreCase) ||
                mask.Equals("DD-MON-YY", StringComparison.OrdinalIgnoreCase))
            {
                return "00-LLL-0000";
            }
            if (mask.StartsWith("DD", StringComparison.OrdinalIgnoreCase) &&
                mask.Contains("MM", StringComparison.OrdinalIgnoreCase) &&
                mask.Contains("YY", StringComparison.OrdinalIgnoreCase))
            {
                return "00/00/00";
            }
            if (mask.StartsWith("HH", StringComparison.OrdinalIgnoreCase) &&
                mask.Contains("MI", StringComparison.OrdinalIgnoreCase))
            {
                return "00:00";
            }

            // Currency / number masks
            if (mask.StartsWith("$", StringComparison.Ordinal) && mask.IndexOfAny(new[] { '9', '0' }) > 0)
            {
                int nineCount = CountChar(mask, '9') + CountChar(mask, '0');
                if (nineCount > 0 && mask.Contains("."))
                {
                    int decimalPos = mask.IndexOf('.');
                    int decimals = mask.Length - decimalPos - 1;
                    int wholePart = nineCount - decimals;
                    wholePart = Math.Max(0, wholePart);
                    return new string('0', wholePart) + "." + new string('0', decimals);
                }
            }

            if (mask.Replace("9", "").Replace("0", "").Replace("G", "").Replace("D", "")
                .Replace(",", "").Replace(".", "").Trim() == string.Empty)
            {
                // Pure digit / thousands / decimal mask — translate character by character.
                StringBuilder sb = new(mask.Length);
                foreach (char c in mask)
                {
                    switch (c)
                    {
                        case '9':
                        case '0':
                            sb.Append('0');
                            break;
                        case 'G':
                        case ',':
                            // thousand-grouping separator: skip in .NET (it uses
                            // culture-aware group separator that the .NET mask
                            // can't reliably mirror without per-locale setup).
                            break;
                        case 'D':
                            // decimal separator → use culture's decimal point
                            sb.Append(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                            break;
                        case '.':
                            sb.Append(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                            break;
                        default:
                            sb.Append(c);
                            break;
                    }
                }
                return sb.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Format a value for display using a Forms-style format mask. Used
        /// by the field presenters when the user is not actively editing the
        /// cell, so they see "$1,234.50" instead of "1234.5".
        /// </summary>
        public static string FormatValue(object? value, string? formsMask, IFormatProvider? provider = null)
        {
            if (value == null || string.IsNullOrWhiteSpace(formsMask))
            {
                return value?.ToString() ?? string.Empty;
            }

            provider ??= CultureInfo.CurrentCulture;
            string upper = formsMask.Trim().ToUpperInvariant();

            // Date masks
            if (value is DateTime dt)
            {
                if (upper == "DD-MON-YYYY") return dt.ToString("dd-MMM-yyyy", provider);
                if (upper == "DD-MON-YY") return dt.ToString("dd-MMM-yy", provider);
                if (upper.StartsWith("DD") && upper.Contains("MM") && upper.Contains("YY"))
                {
                    return dt.ToString("dd/MM/yy", provider);
                }
            }

            // Numeric masks — start with $ and contain at least one 9 or 0
            if (upper.StartsWith("$") && upper.IndexOfAny(new[] { '9', '0' }) > 0)
            {
                if (value is IFormattable formattable)
                {
                    int dotIndex = upper.IndexOf('.');
                    int decimals = dotIndex >= 0 ? upper.Length - dotIndex - 1 : 0;
                    decimals = Math.Clamp(decimals, 0, 8);
                    string dotNetFormat = decimals > 0
                        ? "C" + decimals.ToString(provider)
                        : "C";
                    return formattable.ToString(dotNetFormat, provider);
                }
            }

            return value.ToString() ?? string.Empty;
        }

        private static int CountChar(string s, char c)
        {
            int n = 0;
            foreach (char ch in s)
            {
                if (ch == c) n++;
            }
            return n;
        }
    }
}
