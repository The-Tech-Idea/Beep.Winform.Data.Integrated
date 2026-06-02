using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Lov
{
    /// <summary>
    /// Modal WinForms picker for Oracle Forms-style LOV selection.
    /// Loads <see cref="LOVResult.Records"/> into a searchable
    /// <see cref="DataGridView"/>; the user picks one row and presses OK
    /// (or double-clicks). Returns a populated <see cref="LOVSelectionResult"/>
    /// or <see cref="LOVSelectionResult.Cancelled"/>.
    /// </summary>
    public class LovPickerDialog : Form
    {
        private readonly LOVDefinition _definition;
        private readonly IList<object> _records;
        private readonly List<Dictionary<string, object?>> _rows;

        private TextBox _searchBox = null!;
        private DataGridView _grid = null!;
        private Button _okButton = null!;
        private Button _cancelButton = null!;
        private Label _statusLabel = null!;

        public LOVSelectionResult Selection { get; private set; } = LOVSelectionResult.Cancelled();

        public LovPickerDialog(LOVDefinition definition, LOVResult result)
        {
            _definition = definition ?? new LOVDefinition { Title = "List of Values" };
            _records = result?.Records ?? new List<object>();
            _rows = _records.Select(MaterializeRecord).ToList();

            InitializeComponent();
            ApplyDefinition();
            ApplyFilter(string.Empty);
        }

        // ── Layout ────────────────────────────────────────────────────────
        private void InitializeComponent()
        {
            SuspendLayout();

            Text = string.IsNullOrWhiteSpace(_definition.Title) ? "List of Values" : _definition.Title;
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = true;
            ShowInTaskbar = false;
            ClientSize = new Size(
                Math.Max(400, _definition.Width),
                Math.Max(280, _definition.Height));
            Font = new System.Drawing.Font("Segoe UI", 9F);

            var topRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 36,
                ColumnCount = 2,
                Padding = new Padding(8, 8, 8, 0)
            };
            topRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            topRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            var searchLabel = new Label
            {
                Text = "Search:",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 6, 8, 0)
            };
            _searchBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 3, 0, 3)
            };
            _searchBox.TextChanged += (_, __) => ApplyFilter(_searchBox.Text);

            topRow.Controls.Add(searchLabel, 0, 0);
            topRow.Controls.Add(_searchBox, 1, 0);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = _definition.ShowRowNumbers,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = _definition.AllowMultiSelect,
                AutoSizeColumnsMode = _definition.AutoSizeColumns
                    ? DataGridViewAutoSizeColumnsMode.Fill
                    : DataGridViewAutoSizeColumnsMode.None,
                BackgroundColor = SystemColors.Window
            };
            _grid.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex < 0) return;
                AcceptSelection();
            };

            var bottomRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8, 6, 8, 6)
            };
            _okButton = new Button { Text = "OK", Width = 80, Margin = new Padding(4, 4, 0, 4) };
            _okButton.Click += (_, __) => AcceptSelection();
            _cancelButton = new Button
            {
                Text = "Cancel",
                Width = 80,
                Margin = new Padding(4, 4, 0, 4),
                DialogResult = DialogResult.Cancel
            };

            _statusLabel = new Label
            {
                Text = string.Empty,
                AutoSize = true,
                Margin = new Padding(0, 12, 12, 0)
            };

            bottomRow.Controls.Add(_okButton);
            bottomRow.Controls.Add(_cancelButton);
            bottomRow.Controls.Add(_statusLabel);

            Controls.Add(_grid);
            Controls.Add(bottomRow);
            Controls.Add(topRow);

            AcceptButton = _okButton;
            CancelButton = _cancelButton;

            ResumeLayout(false);
        }

        // ── Definition → grid columns ────────────────────────────────────
        private void ApplyDefinition()
        {
            _grid.Columns.Clear();
            _grid.DataSource = null;

            var dt = new System.Data.DataTable();
            var fields = ResolveVisibleFields();

            foreach (var f in fields)
            {
                dt.Columns.Add(f, typeof(object));
            }

            // Hidden "Record" column carries the source object so we can
            // round-trip the original record into LOVSelectionResult.
            dt.Columns.Add("__Record", typeof(object));

            foreach (var row in _rows)
            {
                var values = new object?[fields.Count + 1];
                for (int i = 0; i < fields.Count; i++)
                {
                    row.TryGetValue(fields[i], out var v);
                    values[i] = v ?? string.Empty;
                }
                values[fields.Count] = _records[_rows.IndexOf(row)];
                dt.Rows.Add(values);
            }

            _grid.DataSource = dt;

            // Hide the carrier column.
            if (_grid.Columns["__Record"] is { } carrier)
            {
                carrier.Visible = false;
            }

            // Apply column display names + widths from LOVDefinition.Columns.
            foreach (var col in _definition.Columns ?? new List<LOVColumn>())
            {
                if (_grid.Columns.Contains(col.FieldName))
                {
                    var gridCol = _grid.Columns[col.FieldName];
                    if (!string.IsNullOrWhiteSpace(col.DisplayName))
                    {
                        gridCol.HeaderText = col.DisplayName;
                    }
                    if (col.Width > 0 && _grid.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.None)
                    {
                        gridCol.Width = col.Width;
                    }
                }
            }
        }

        private List<string> ResolveVisibleFields()
        {
            var explicitCols = (_definition.Columns ?? new List<LOVColumn>())
                .Where(c => c.Visible)
                .OrderBy(c => c.SortOrder)
                .Select(c => c.FieldName)
                .ToList();

            if (explicitCols.Count > 0)
            {
                return explicitCols;
            }

            // Fall back: any field present in the first record.
            if (_rows.Count > 0)
            {
                return _rows[0].Keys
                    .Where(k => k != "__Record")
                    .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            return new List<string>();
        }

        // ── Filtering ────────────────────────────────────────────────────
        private void ApplyFilter(string? text)
        {
            if (_grid.DataSource is not System.Data.DataTable dt)
            {
                return;
            }

            dt.DefaultView.RowFilter = BuildRowFilter(text);

            int visible = dt.DefaultView.Count;
            int total = _rows.Count;
            _statusLabel.Text = total == visible
                ? $"{total} record(s)"
                : $"{visible} of {total} record(s)";

            if (visible > 0)
            {
                _grid.ClearSelection();
                _grid.Rows[0].Selected = true;
                _grid.CurrentCell = _grid.Rows[0].Cells[0];
            }
        }

        private string BuildRowFilter(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var searchable = (_definition.Columns ?? new List<LOVColumn>())
                .Where(c => c.Searchable && c.Visible)
                .Select(c => c.FieldName)
                .ToList();

            if (searchable.Count == 0)
            {
                searchable = ResolveVisibleFields();
            }

            if (searchable.Count == 0)
            {
                return string.Empty;
            }

            string needle = text.Trim().Replace("'", "''");
            return string.Join(" OR ", searchable.Select(f =>
                $"Convert([{f}], 'System.String') LIKE '%{needle}%'"));
        }

        // ── Selection ────────────────────────────────────────────────────
        private void AcceptSelection()
        {
            if (_grid.SelectedRows.Count == 0)
            {
                _statusLabel.Text = "Select a row first.";
                DialogResult = DialogResult.None;
                return;
            }

            var row = _grid.SelectedRows[0];
            var record = row.Cells["__Record"].Value;
            var related = new Dictionary<string, object?>();

            foreach (var f in ResolveVisibleFields())
            {
                if (row.Cells[f]?.Value is { } v)
                {
                    related[f] = v;
                }
            }

            object? returnValue = ResolveReturnValue(record);
            object? displayValue = ResolveDisplayValue(record);

            Selection = LOVSelectionResult.Select(
                returnValue ?? record,
                record,
                related.ToDictionary(kv => kv.Key, kv => kv.Value!));

            if (displayValue != null && displayValue != returnValue)
            {
                Selection.DisplayValue = displayValue;
            }

            DialogResult = DialogResult.OK;
        }

        private object? ResolveReturnValue(object? record)
        {
            if (record == null || string.IsNullOrWhiteSpace(_definition.ReturnField))
            {
                return record;
            }
            return ReadField(record, _definition.ReturnField);
        }

        private object? ResolveDisplayValue(object? record)
        {
            if (record == null || string.IsNullOrWhiteSpace(_definition.DisplayField))
            {
                return record;
            }
            return ReadField(record, _definition.DisplayField);
        }

        // ── Record materialization ───────────────────────────────────────
        private static Dictionary<string, object?> MaterializeRecord(object record)
        {
            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            if (record == null)
            {
                return dict;
            }

            if (record is IDictionary idict)
            {
                foreach (DictionaryEntry entry in idict)
                {
                    dict[entry.Key?.ToString() ?? string.Empty] = entry.Value;
                }
                return dict;
            }

            foreach (var prop in record.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead || prop.GetIndexParameters().Length > 0)
                {
                    continue;
                }
                object? value;
                try
                {
                    value = prop.GetValue(record);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[LovPickerDialog.MaterializeRecord] {prop.Name}: {ex.GetType().Name}: {ex.Message}");
                    value = null;
                }
                dict[prop.Name] = value;
            }

            foreach (var field in record.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    dict[field.Name] = field.GetValue(record);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[LovPickerDialog.MaterializeRecord] {field.Name}: {ex.GetType().Name}: {ex.Message}");
                }
            }

            return dict;
        }

        private static object? ReadField(object record, string fieldName)
        {
            if (record is IDictionary idict)
            {
                foreach (DictionaryEntry entry in idict)
                {
                    if (string.Equals(entry.Key?.ToString(), fieldName, StringComparison.OrdinalIgnoreCase))
                    {
                        return entry.Value;
                    }
                }
                return null;
            }

            var prop = record.GetType().GetProperty(fieldName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null && prop.CanRead)
            {
                return prop.GetValue(record);
            }

            var field = record.GetType().GetField(fieldName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return field?.GetValue(record);
        }
    }
}
