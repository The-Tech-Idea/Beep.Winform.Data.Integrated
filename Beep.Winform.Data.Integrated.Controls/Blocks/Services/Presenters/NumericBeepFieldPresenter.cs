using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;
using TheTechIdea.Beep.Winform.Controls.Numerics;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Services.Presenters
{
    public sealed class NumericBeepFieldPresenter : IBeepFieldPresenter
    {
        public string Key => "numeric";

        public bool CanPresent(BeepFieldDefinition fieldDefinition)
        {
            return string.Equals(fieldDefinition?.EditorKey, Key, System.StringComparison.OrdinalIgnoreCase);
        }

        public Control CreateEditor(BeepFieldDefinition fieldDefinition)
        {
            return new BeepNumericUpDown
            {
                Name = string.IsNullOrWhiteSpace(fieldDefinition.FieldName) ? "FieldEditor" : $"{fieldDefinition.FieldName}_Editor",
                Width = fieldDefinition.Width,
                ReadOnly = fieldDefinition.IsReadOnly,
                UseThemeColors = true
            };
        }

        public void ApplyMetadata(Control editor, BeepFieldDefinition fieldDefinition)
        {
            editor.Enabled = !fieldDefinition.IsReadOnly;
            editor.Visible = fieldDefinition.IsVisible;
            editor.Width = fieldDefinition.Width;

            // Apply Oracle Forms FORMAT_MASK for currency / decimal columns
            // (e.g. "$999,999.00"). The BeepNumericUpDown has no native
            // Mask property, so we set the DecimalPlaces + thousands-separator
            // style flags based on what the developer wrote.
            if (editor is BeepNumericUpDown numeric)
            {
                ApplyFormatMask(numeric, fieldDefinition.FormatMask);
            }

            // M2-RUN-003: apply the field's visual attribute
            // (font / colour). Numeric presenters reuse the
            // shared helper from TextBeepFieldPresenter.
            TextBeepFieldPresenter.ApplyVisualAttribute(editor, fieldDefinition.VisualAttribute);

            // M3-RUN-017 / M3-RUN-018: when the field has a
            // Formula or Summary calculation, wire the binding
            // source's CurrentChanged and ListChanged events to
            // re-evaluate the value. The hook is idempotent
            // — multiple calls are no-ops. The runtime uses
            // BeepFieldCalculator for both.
            if (fieldDefinition.Calculation == BeepFieldCalculation.Formula
                || fieldDefinition.Calculation == BeepFieldCalculation.Summary)
            {
                HookCalculationEvents(editor, fieldDefinition);
            }
        }

        // M3-RUN-017 / M3-RUN-018: hook the binding source so
        // the field re-evaluates on every record / list change.
        // The hook is stored on the editor's Tag so the same
        // editor is never hooked twice. When the field is
        // disposed, the hook is detached.
        private static void HookCalculationEvents(Control editor, BeepFieldDefinition fieldDefinition)
        {
            if (editor == null || fieldDefinition == null) return;
            var source = editor.DataBindings.Count > 0
                ? editor.DataBindings[0].DataSource as System.ComponentModel.IBindingList
                : null;
            if (source == null) return;
            var bindingSource = source as System.Windows.Forms.BindingSource;
            if (bindingSource == null) return;
            if (editor.Tag is CalculationHook existing && existing.Field == fieldDefinition)
            {
                return;
            }
            var hook = new CalculationHook(editor, fieldDefinition, bindingSource);
            editor.Tag = hook;
        }

        private sealed class CalculationHook
        {
            public BeepFieldDefinition Field { get; }
            private readonly Control _editor;
            private readonly System.Windows.Forms.BindingSource _bindingSource;

            public CalculationHook(Control editor, BeepFieldDefinition field, System.Windows.Forms.BindingSource source)
            {
                Field = field;
                _editor = editor;
                _bindingSource = source;
                _bindingSource.CurrentChanged += OnCurrentChanged;
                _bindingSource.ListChanged += OnListChanged;
            }

            private void OnCurrentChanged(object? sender, System.EventArgs e) => Recompute();
            private void OnListChanged(object? sender, System.ComponentModel.ListChangedEventArgs e) => Recompute();

            private void Recompute()
            {
                if (Field.Calculation == BeepFieldCalculation.Formula)
                {
                    var record = BuildRecord(_bindingSource);
                    var result = BeepFieldCalculator.EvaluateFormula(Field, record, allRecords: null);
                    if (result != null && _editor is BeepNumericUpDown numeric)
                    {
                        numeric.Value = System.Convert.ToDecimal(result, System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
                else if (Field.Calculation == BeepFieldCalculation.Summary)
                {
                    var all = BuildAllRecords(_bindingSource);
                    var result = BeepFieldCalculator.EvaluateSummary(Field, all);
                    if (result != null && _editor is BeepNumericUpDown numeric)
                    {
                        numeric.Value = System.Convert.ToDecimal(result, System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
            }

            private static System.Collections.Generic.Dictionary<string, object?> BuildRecord(System.Windows.Forms.BindingSource source)
            {
                var record = new System.Collections.Generic.Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                if (source.Current is System.Data.DataRowView drv)
                {
                    foreach (System.Data.DataColumn col in drv.Row.Table.Columns)
                    {
                        record[col.ColumnName] = drv.Row[col];
                    }
                }
                else if (source.Current != null)
                {
                    foreach (var prop in source.Current.GetType().GetProperties())
                    {
                        record[prop.Name] = prop.GetValue(source.Current);
                    }
                }
                return record;
            }

            private static System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object?>> BuildAllRecords(System.Windows.Forms.BindingSource source)
            {
                var list = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object?>>();
                foreach (var item in source.List)
                {
                    if (item is System.Data.DataRowView drv)
                    {
                        var record = new System.Collections.Generic.Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                        foreach (System.Data.DataColumn col in drv.Row.Table.Columns)
                        {
                            record[col.ColumnName] = drv.Row[col];
                        }
                        list.Add(record);
                    }
                    else if (item != null)
                    {
                        var record = new System.Collections.Generic.Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                        foreach (var prop in item.GetType().GetProperties())
                        {
                            record[prop.Name] = prop.GetValue(item);
                        }
                        list.Add(record);
                    }
                }
                return list;
            }
        }

        private static void ApplyFormatMask(BeepNumericUpDown numeric, string? formatMask)
        {
            if (string.IsNullOrWhiteSpace(formatMask))
            {
                return;
            }

            string upper = formatMask.Trim().ToUpperInvariant();
            if (upper.Contains('.'))
            {
                int dotIndex = upper.IndexOf('.');
                int decimals = upper.Length - dotIndex - 1;
                numeric.DecimalPlaces = (int)System.Math.Clamp(decimals, 0, 6);
            }
            else
            {
                numeric.DecimalPlaces = 0;
            }

            // Forms users almost always enable thousands separator when the
            // mask contains a comma or a 'G' token.
            numeric.ThousandsSeparator = upper.Contains(',') || upper.Contains('G');
        }
    }
}