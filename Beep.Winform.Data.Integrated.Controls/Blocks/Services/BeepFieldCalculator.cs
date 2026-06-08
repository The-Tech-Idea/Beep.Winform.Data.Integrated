using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Services
{
    /// <summary>
    /// M3-RUN-016: tiny infix evaluator for the runtime's
    /// formula calculation. The Oracle Forms <c>Calculation =
    /// Formula</c> use case is simple enough that a hand-rolled
    /// evaluator is more honest than pulling in
    /// <c>System.Data.DataTable.Compute</c> (which has been
    /// deprecated for years). The evaluator supports:
    /// <list type="bullet">
    /// <item>Numeric literals (<c>42</c>, <c>3.14</c>).</item>
    /// <item>Field references by <c>FieldName</c> resolved against
    /// the supplied <c>record</c> dictionary (case-insensitive).</item>
    /// <item>Operators <c>+</c> <c>-</c> <c>*</c> <c>/</c> with
    /// standard precedence.</item>
    /// <item>Parentheses.</item>
    /// </list>
    /// </summary>
    public static class BeepFieldCalculator
    {
        /// <summary>
        /// Evaluate <paramref name="formula"/> against the
        /// supplied <paramref name="record"/>. Field references
        /// in the formula are looked up in the dictionary. The
        /// function returns <c>null</c> on any parse or
        /// evaluation error (the caller surfaces a friendly
        /// message through the field's status lane).
        /// </summary>
        public static object? EvaluateFormula(
            BeepFieldDefinition field,
            IDictionary<string, object?>? record,
            IEnumerable<IDictionary<string, object?>>? allRecords = null)
        {
            if (field == null || string.IsNullOrWhiteSpace(field.CalculationFormula)) return null;
            if (record == null) return null;
            try
            {
                var parser = new InfixParser(field.CalculationFormula, record);
                return parser.ParseAndEvaluate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BeepFieldCalculator] formula error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Aggregate <paramref name="field"/>'s column across
        /// <paramref name="allRecords"/> using the chosen
        /// <see cref="BeepSummaryOperation"/>. The function
        /// returns <c>null</c> on any aggregation error.
        /// </summary>
        public static object? EvaluateSummary(
            BeepFieldDefinition field,
            IEnumerable<IDictionary<string, object?>>? allRecords)
        {
            if (field == null || allRecords == null) return null;
            if (field.Summary == BeepSummaryOperation.None) return null;

            try
            {
                var values = new List<double>();
                foreach (var record in allRecords)
                {
                    if (record == null) continue;
                    if (!record.TryGetValue(field.FieldName, out var raw)) continue;
                    if (raw == null) continue;
                    if (double.TryParse(raw.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    {
                        values.Add(d);
                    }
                }

                if (values.Count == 0) return 0d;

                return field.Summary switch
                {
                    BeepSummaryOperation.Sum => values.Sum(),
                    BeepSummaryOperation.Average => values.Average(),
                    BeepSummaryOperation.Minimum => values.Min(),
                    BeepSummaryOperation.Maximum => values.Max(),
                    BeepSummaryOperation.Count => (double)values.Count,
                    _ => null
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BeepFieldCalculator] summary error: {ex.Message}");
                return null;
            }
        }

        // ── M3-RUN-016: tiny infix parser ───────────────────────────
        // Grammar:
        //   expression := term ( ('+'|'-') term )*
        //   term       := factor ( ('*'|'/') factor )*
        //   factor     := number | field | '(' expression ')'
        // Field references are resolved against the supplied
        // record dictionary.
        private sealed class InfixParser
        {
            private readonly string _expression;
            private readonly IDictionary<string, object?> _record;
            private int _pos;

            public InfixParser(string expression, IDictionary<string, object?> record)
            {
                _expression = expression ?? string.Empty;
                _record = record ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            }

            public double ParseAndEvaluate()
            {
                _pos = 0;
                double result = ParseExpression();
                SkipWhitespace();
                if (_pos < _expression.Length)
                {
                    throw new FormatException($"Unexpected character '{_expression[_pos]}' at position {_pos}.");
                }
                return result;
            }

            private double ParseExpression()
            {
                double left = ParseTerm();
                while (true)
                {
                    SkipWhitespace();
                    if (_pos >= _expression.Length) return left;
                    char op = _expression[_pos];
                    if (op != '+' && op != '-') return left;
                    _pos++;
                    double right = ParseTerm();
                    left = op == '+' ? left + right : left - right;
                }
            }

            private double ParseTerm()
            {
                double left = ParseFactor();
                while (true)
                {
                    SkipWhitespace();
                    if (_pos >= _expression.Length) return left;
                    char op = _expression[_pos];
                    if (op != '*' && op != '/') return left;
                    _pos++;
                    double right = ParseFactor();
                    left = op == '*' ? left * right : (right == 0 ? 0 : left / right);
                }
            }

            private double ParseFactor()
            {
                SkipWhitespace();
                if (_pos >= _expression.Length)
                {
                    throw new FormatException("Unexpected end of expression.");
                }

                if (_expression[_pos] == '(')
                {
                    _pos++;
                    double inner = ParseExpression();
                    SkipWhitespace();
                    if (_pos >= _expression.Length || _expression[_pos] != ')')
                    {
                        throw new FormatException("Missing closing parenthesis.");
                    }
                    _pos++;
                    return inner;
                }

                if (char.IsDigit(_expression[_pos]) || _expression[_pos] == '.')
                {
                    return ParseNumber();
                }

                // Field reference: read until the next operator
                // or closing paren.
                int start = _pos;
                while (_pos < _expression.Length &&
                       !"+-*/()".Contains(_expression[_pos]))
                {
                    _pos++;
                }
                string name = _expression.Substring(start, _pos - start).Trim();
                if (string.IsNullOrEmpty(name))
                {
                    throw new FormatException("Empty field reference.");
                }
                if (_record.TryGetValue(name, out var raw) && raw != null
                    && double.TryParse(raw.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                {
                    return d;
                }
                return 0d;
            }

            private double ParseNumber()
            {
                int start = _pos;
                while (_pos < _expression.Length &&
                       (char.IsDigit(_expression[_pos]) || _expression[_pos] == '.'))
                {
                    _pos++;
                }
                string text = _expression.Substring(start, _pos - start);
                if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                {
                    return d;
                }
                throw new FormatException($"Invalid number '{text}'.");
            }

            private void SkipWhitespace()
            {
                while (_pos < _expression.Length && char.IsWhiteSpace(_expression[_pos]))
                {
                    _pos++;
                }
            }
        }
    }
}
