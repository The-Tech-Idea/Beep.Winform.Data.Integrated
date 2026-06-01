using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms;
using TheTechIdea.Beep.Winform.Controls.ListBoxs;
using TheTechIdea.Beep.Winform.Controls.Models;
using TheTechIdea.Beep.Winform.Controls.Numerics;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks
{
    public partial class BeepBlock
    {
        private readonly Dictionary<string, QueryCriterionState> _queryCriteria = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, QueryEditorGroup> _queryEditorGroups = new(StringComparer.OrdinalIgnoreCase);
        private bool _isApplyingQueryCriteria;

        private sealed class QueryCriterionState
        {
            public string Operator { get; set; } = string.Empty;
            public object? Value { get; set; }
            public object? SecondaryValue { get; set; }
            public bool HasValue { get; set; }
            public bool HasSecondaryValue { get; set; }
        }

        private sealed class QueryEditorGroup
        {
            public TableLayoutPanel Host { get; init; } = null!;
            public Control PrimaryEditor { get; init; } = null!;
            public Control? SecondaryEditor { get; init; }
            public BeepLabel? RangeSeparator { get; init; }
            public BeepLabel? NoValueHintLabel { get; init; }
            public QueryListEditor? ListEditor { get; init; }
        }

        private sealed class QueryListSelectionOptions
        {
            public QueryListSelectionOptions(BindingList<SimpleItem> items, bool allowManualEntry, string sourceCaption)
            {
                Items = items ?? new BindingList<SimpleItem>();
                AllowManualEntry = allowManualEntry;
                SourceCaption = string.IsNullOrWhiteSpace(sourceCaption) ? "Known Values" : sourceCaption;
            }

            public BindingList<SimpleItem> Items { get; }
            public bool AllowManualEntry { get; }
            public string SourceCaption { get; }
        }

        private sealed class QueryListEditor : TableLayoutPanel
        {
            private readonly List<string> _entries = new();
            private readonly BeepLabel _summaryLabel;
            private readonly BeepButton _editButton;

            public QueryListEditor()
            {
                Dock = DockStyle.Fill;
                ColumnCount = 2;
                RowCount = 1;
                Margin = new Padding(0);
                Padding = new Padding(0);
                ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86f));

                _summaryLabel = new BeepLabel
                {
                    Dock = DockStyle.Fill,
                    AutoEllipsis = true,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                    UseThemeColors = true,
                    Text = "No list values configured."
                };

                _editButton = new BeepButton
                {
                    Dock = DockStyle.Fill,
                    Text = "Edit...",
                    ShowShadow = false,
                    UseThemeColors = true,
                    Margin = new Padding(8, 0, 0, 0)
                };
                _editButton.Click += (_, _) => EditRequested?.Invoke(this, EventArgs.Empty);

                Controls.Add(_summaryLabel, 0, 0);
                Controls.Add(_editButton, 1, 0);
            }

            public event EventHandler? EditRequested;
            public event EventHandler? ValuesChanged;

            public IReadOnlyList<string> Entries => _entries;

            public void SetEntries(IEnumerable<string>? entries)
            {
                _entries.Clear();
                if (entries != null)
                {
                    _entries.AddRange(entries.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()));
                }

                UpdateSummaryText();
                ValuesChanged?.Invoke(this, EventArgs.Empty);
            }

            private void UpdateSummaryText()
            {
                if (_entries.Count == 0)
                {
                    _summaryLabel.Text = "No list values configured.";
                    return;
                }

                string preview = string.Join(", ", _entries.Take(3));
                if (_entries.Count > 3)
                {
                    preview += $" (+{_entries.Count - 3} more)";
                }

                _summaryLabel.Text = _entries.Count == 1
                    ? $"1 value: {preview}"
                    : $"{_entries.Count} values: {preview}";
            }
        }

        public void EnterQueryMode()
        {
            _viewState.IsQueryMode = true;
            _viewState.Mode = DataBlockMode.Query;
            RefreshPresentation();
            NotifyViewStateChanged();
        }

        public void ExitQueryMode()
        {
            _viewState.IsQueryMode = false;
            if (_viewState.Mode == DataBlockMode.Query)
            {
                _viewState.Mode = DataBlockMode.CRUD;
            }

            RefreshPresentation();
            NotifyViewStateChanged();
        }

        public void ClearQueryCriteria()
        {
            _queryCriteria.Clear();
            ApplyQueryCriteriaToEditors();
            UpdateWorkflowSurface();
        }

        public void ClearQueryCriterion(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                return;
            }

            _queryCriteria.Remove(fieldName);
            _isApplyingQueryCriteria = true;
            try
            {
                if (_queryOperatorEditors.TryGetValue(fieldName, out var operatorEditor))
                {
                    operatorEditor.SelectedValue = GetDefaultQueryOperator(ResolveFieldDataType(fieldName));
                }

                if (_fieldEditors.TryGetValue(fieldName, out var primaryEditor))
                {
                    ApplyQueryValueToEditor(primaryEditor, null, false);
                }

                if (_queryEditorGroups.TryGetValue(fieldName, out var editorGroup) && editorGroup.SecondaryEditor != null)
                {
                    ApplyQueryValueToEditor(editorGroup.SecondaryEditor, null, false);
                }
            }
            finally
            {
                _isApplyingQueryCriteria = false;
            }

            UpdateQueryEditorPresentation(fieldName);
            UpdateWorkflowSurface();
        }

        private async void ExecuteQueryButton_Click(object? sender, EventArgs e)
        {
            await ExecuteQueryAsync().ConfigureAwait(true);
        }

        private void ClearQueryButton_Click(object? sender, EventArgs e)
        {
            ClearQueryCriteria();
        }

        private QueryCriterionState GetOrCreateQueryCriterion(string fieldName)
        {
            if (!_queryCriteria.TryGetValue(fieldName, out var criterion))
            {
                criterion = new QueryCriterionState
                {
                    Operator = GetDefaultQueryOperator(ResolveFieldDataType(fieldName))
                };
                _queryCriteria[fieldName] = criterion;
            }

            return criterion;
        }

        private IEnumerable<Models.BeepFieldDefinition> GetRenderableFields(Models.BeepBlockDefinition? definition)
        {
            if (definition?.Fields == null)
            {
                return Enumerable.Empty<Models.BeepFieldDefinition>();
            }

            var fields = definition.Fields
                .Where(field => field != null && field.IsVisible)
                .OrderBy(field => field.Order);

            return ViewState.IsQueryMode
                ? fields.Where(IsFieldQueryable)
                : fields;
        }

        private bool IsFieldQueryable(Models.BeepFieldDefinition field)
        {
            if (field == null || string.IsNullOrWhiteSpace(field.FieldName))
            {
                return false;
            }

            if (_formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName) || !_formsHost.IsBlockRegistered(ManagerBlockName))
            {
                return true;
            }

            var blockInfo = _formsHost.GetBlockInfo(ManagerBlockName);
            if (blockInfo != null && !blockInfo.QueryAllowed)
            {
                return false;
            }

            try
            {
                return _formsHost.IsFieldQueryAllowed(ManagerBlockName, field.FieldName);
            }
            catch
            {
                var fieldMetadata = blockInfo?.FieldMetadata?
                    .FirstOrDefault(item => string.Equals(item.FieldName, field.FieldName, StringComparison.OrdinalIgnoreCase));

                return fieldMetadata?.IsQueryable ?? true;
            }
        }

        private void CaptureQueryCriteriaFromEditors()
        {
            if (!ViewState.IsQueryMode)
            {
                return;
            }

            foreach (var editorEntry in _fieldEditors)
            {
                if (!_queryOperatorEditors.TryGetValue(editorEntry.Key, out var operatorEditor))
                {
                    continue;
                }

                string fieldName = editorEntry.Key;
                QueryCriterionState criterion = GetOrCreateQueryCriterion(fieldName);
                criterion.Operator = ResolveQueryOperatorValue(fieldName, operatorEditor);

                if (IsNoValueOperator(criterion.Operator))
                {
                    criterion.HasValue = false;
                    criterion.Value = null;
                    criterion.HasSecondaryValue = false;
                    criterion.SecondaryValue = null;
                    continue;
                }

                if (_queryEditorGroups.TryGetValue(fieldName, out var listEditorGroup) &&
                    IsListOperator(criterion.Operator) &&
                    listEditorGroup.ListEditor != null)
                {
                    object? listValue = ReadQueryEditorValue(listEditorGroup.ListEditor, fieldName, isSecondary: false);
                    criterion.HasValue = DetermineQueryEditorHasValue(listEditorGroup.ListEditor, fieldName, listValue, isSecondary: false);
                    criterion.Value = criterion.HasValue ? listValue : null;
                    criterion.HasSecondaryValue = false;
                    criterion.SecondaryValue = null;
                    continue;
                }

                object? value = ReadQueryEditorValue(editorEntry.Value, fieldName, isSecondary: false);
                criterion.HasValue = DetermineQueryEditorHasValue(editorEntry.Value, fieldName, value, isSecondary: false);
                criterion.Value = criterion.HasValue ? value : null;

                if (_queryEditorGroups.TryGetValue(fieldName, out var editorGroup) && editorGroup.SecondaryEditor != null)
                {
                    object? secondaryValue = ReadQueryEditorValue(editorGroup.SecondaryEditor, fieldName, isSecondary: true);
                    criterion.HasSecondaryValue = DetermineQueryEditorHasValue(editorGroup.SecondaryEditor, fieldName, secondaryValue, isSecondary: true);
                    criterion.SecondaryValue = criterion.HasSecondaryValue ? secondaryValue : null;
                }
                else
                {
                    criterion.HasSecondaryValue = false;
                    criterion.SecondaryValue = null;
                }
            }
        }

        private void ApplyQueryCriteriaToEditors()
        {
            _isApplyingQueryCriteria = true;
            try
            {
                foreach (var editorEntry in _fieldEditors)
                {
                    string fieldName = editorEntry.Key;
                    _queryCriteria.TryGetValue(fieldName, out var criterion);
                    if (_queryOperatorEditors.TryGetValue(fieldName, out var operatorEditor))
                    {
                        operatorEditor.SelectedValue = ResolveStoredQueryOperator(fieldName);
                    }

                    UpdateQueryEditorPresentation(fieldName);
                    ApplyQueryValueToEditor(editorEntry.Value, criterion?.Value, criterion?.HasValue == true);

                    if (_queryEditorGroups.TryGetValue(fieldName, out var editorGroup) && editorGroup.SecondaryEditor != null)
                    {
                        ApplyQueryValueToEditor(editorGroup.SecondaryEditor, criterion?.SecondaryValue, criterion?.HasSecondaryValue == true);
                    }
                }
            }
            finally
            {
                _isApplyingQueryCriteria = false;
            }
        }

        private string ResolveStoredQueryOperator(string fieldName)
        {
            if (_queryCriteria.TryGetValue(fieldName, out var criterion) && !string.IsNullOrWhiteSpace(criterion.Operator))
            {
                return criterion.Operator;
            }

            return GetDefaultQueryOperator(ResolveFieldDataType(fieldName));
        }

        private string ResolveQueryOperatorValue(string fieldName, BeepComboBox operatorEditor)
        {
            string? selectedOperator = operatorEditor.SelectedValue as string;
            if (string.IsNullOrWhiteSpace(selectedOperator))
            {
                selectedOperator = operatorEditor.SelectedItem?.Value?.ToString();
            }

            if (string.IsNullOrWhiteSpace(selectedOperator))
            {
                selectedOperator = operatorEditor.SelectedItem?.Text;
            }

            return string.IsNullOrWhiteSpace(selectedOperator)
                ? GetDefaultQueryOperator(ResolveFieldDataType(fieldName))
                : selectedOperator;
        }

        private bool TryBuildQueryFiltersFromEditors(out List<AppFilter> filters, out string validationMessage)
        {
            CaptureQueryCriteriaFromEditors();

            filters = new List<AppFilter>();
            validationMessage = string.Empty;

            var definition = EffectiveDefinition;
            if (definition == null)
            {
                return true;
            }

            foreach (var field in GetRenderableFields(definition))
            {
                if (!_queryCriteria.TryGetValue(field.FieldName, out var criterion))
                {
                    continue;
                }

                bool isNoValueOperator = IsNoValueOperator(criterion.Operator);
                bool isListOperator = IsListOperator(criterion.Operator);
                if (!isNoValueOperator && (!criterion.HasValue || IsQueryValueEmpty(criterion.Value)))
                {
                    continue;
                }

                object? convertedValue = isNoValueOperator || isListOperator
                    ? null
                    : ConvertEditorValueForDataSource(field.FieldName, criterion.Value);
                Type? fieldType = ResolveFieldDataType(field.FieldName);
                bool isRangeOperator = IsRangeOperator(criterion.Operator);
                object? convertedSecondaryValue = null;

                if (isListOperator)
                {
                    List<string> rawValues = NormalizeQueryListValues(criterion.Value);
                    if (rawValues.Count == 0)
                    {
                        continue;
                    }

                    string packagedValues = string.Join(",", rawValues
                        .Select(item => ConvertEditorValueForDataSource(field.FieldName, item))
                        .Select(ConvertQueryValueToFilterText)
                        .Where(item => !string.IsNullOrWhiteSpace(item)));

                    if (string.IsNullOrWhiteSpace(packagedValues))
                    {
                        continue;
                    }

                    filters.Add(new AppFilter
                    {
                        FieldName = field.FieldName,
                        Operator = string.IsNullOrWhiteSpace(criterion.Operator)
                            ? GetDefaultQueryOperator(fieldType)
                            : criterion.Operator,
                        FilterValue = packagedValues,
                        FilterValue1 = string.Empty,
                        valueType = (Nullable.GetUnderlyingType(fieldType ?? typeof(string)) ?? fieldType ?? typeof(string)).Name,
                        FieldType = fieldType ?? typeof(string)
                    });
                    continue;
                }

                if (isRangeOperator)
                {
                    if (!criterion.HasSecondaryValue || IsQueryValueEmpty(criterion.SecondaryValue))
                    {
                        validationMessage = $"Field '{ResolveQueryFieldLabel(field)}' needs both start and end values for a {criterion.Operator} query.";
                        filters.Clear();
                        return false;
                    }

                    convertedSecondaryValue = ConvertEditorValueForDataSource(field.FieldName, criterion.SecondaryValue);
                }

                filters.Add(new AppFilter
                {
                    FieldName = field.FieldName,
                    Operator = string.IsNullOrWhiteSpace(criterion.Operator)
                        ? GetDefaultQueryOperator(fieldType)
                        : criterion.Operator,
                    FilterValue = isNoValueOperator ? string.Empty : ConvertQueryValueToFilterText(convertedValue),
                    FilterValue1 = isRangeOperator ? ConvertQueryValueToFilterText(convertedSecondaryValue) : string.Empty,
                    valueType = (Nullable.GetUnderlyingType(fieldType ?? typeof(string)) ?? fieldType ?? typeof(string)).Name,
                    FieldType = fieldType ?? typeof(string)
                });
            }

            return true;
        }

        private static string ConvertQueryValueToFilterText(object? value)
        {
            if (value == null || value == DBNull.Value)
            {
                return string.Empty;
            }

            return value switch
            {
                DateTime dateTime => dateTime.ToString("o", CultureInfo.InvariantCulture),
                bool booleanValue => booleanValue ? bool.TrueString : bool.FalseString,
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString() ?? string.Empty
            };
        }

        private static bool IsQueryValueEmpty(object? value)
        {
            return value == null || value == DBNull.Value ||
                   (value is string stringValue && string.IsNullOrWhiteSpace(stringValue));
        }

        private QueryEditorGroup CreateQueryEditorGroup(Models.BeepFieldDefinition fieldDefinition)
        {
            Type? fieldType = ResolveFieldDataType(fieldDefinition.FieldName);
            Control primaryEditor = CreateQueryValueEditor(fieldDefinition, fieldType);
            Control? secondaryEditor = SupportsBetweenOperator(fieldType)
                ? CreateRangeEditor(fieldDefinition, fieldType)
                : null;
            QueryListEditor? listEditor = SupportsListOperator(fieldType)
                ? CreateQueryListEditor(fieldDefinition, fieldType)
                : null;

            var host = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            host.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            host.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, secondaryEditor == null ? 0f : 28f));
            host.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0f));

            primaryEditor.Dock = DockStyle.Fill;
            host.Controls.Add(primaryEditor, 0, 0);
            host.SetColumnSpan(primaryEditor, 3);

            BeepLabel? rangeSeparator = null;
            BeepLabel? noValueHintLabel = null;
            if (secondaryEditor != null)
            {
                rangeSeparator = new BeepLabel
                {
                    Dock = DockStyle.Fill,
                    Text = "to",
                    TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                    UseThemeColors = true,
                    Visible = false
                };

                secondaryEditor.Dock = DockStyle.Fill;
                secondaryEditor.Visible = false;

                host.Controls.Add(rangeSeparator, 1, 0);
                host.Controls.Add(secondaryEditor, 2, 0);
            }

            noValueHintLabel = new BeepLabel
            {
                Dock = DockStyle.Fill,
                Text = "No value required for this operator.",
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                UseThemeColors = true,
                Visible = false
            };
            host.Controls.Add(noValueHintLabel, 0, 0);
            host.SetColumnSpan(noValueHintLabel, 3);

            if (listEditor != null)
            {
                listEditor.Visible = false;
                host.Controls.Add(listEditor, 0, 0);
                host.SetColumnSpan(listEditor, 3);
            }

            AttachQueryValueTracking(fieldDefinition.FieldName, primaryEditor, isSecondary: false);
            if (secondaryEditor != null)
            {
                AttachQueryValueTracking(fieldDefinition.FieldName, secondaryEditor, isSecondary: true);
            }

            return new QueryEditorGroup
            {
                Host = host,
                PrimaryEditor = primaryEditor,
                SecondaryEditor = secondaryEditor,
                RangeSeparator = rangeSeparator,
                NoValueHintLabel = noValueHintLabel,
                ListEditor = listEditor
            };
        }

        private QueryListEditor CreateQueryListEditor(Models.BeepFieldDefinition fieldDefinition, Type? fieldType)
        {
            var editor = new QueryListEditor();
            editor.EditRequested += (_, _) => EditQueryListValues(fieldDefinition, fieldType, editor);
            return editor;
        }

        private Control CreateQueryValueEditor(Models.BeepFieldDefinition fieldDefinition, Type? fieldType)
        {
            if (ShouldUseQueryComboEditor(fieldDefinition, fieldType))
            {
                var comboBox = new BeepComboBox
                {
                    Dock = DockStyle.Fill,
                    Theme = Theme,
                    AllowFreeText = false,
                    IsEditable = false,
                    DataSourceProperty = fieldDefinition.FieldName
                };

                ConfigureQueryComboEditor(comboBox, fieldDefinition, fieldType);
                return comboBox;
            }

            if (IsDateField(fieldType))
            {
                var datePicker = new BeepDatePicker
                {
                    Dock = DockStyle.Fill,
                    UseThemeColors = true
                };
                datePicker.SelectedDate = string.Empty;
                return datePicker;
            }

            if (IsNumericField(fieldType))
            {
                return new BeepNumericUpDown
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = false,
                    UseThemeColors = true,
                    MinimumValue = decimal.MinValue,
                    MaximumValue = decimal.MaxValue,
                    DecimalPlaces = ResolveNumericDecimalPlaces(fieldType),
                    Value = 0m
                };
            }

            return new BeepTextBox
            {
                Dock = DockStyle.Fill,
                Theme = Theme,
                Text = string.Empty
            };
        }

        private Control CreateRangeEditor(Models.BeepFieldDefinition fieldDefinition, Type? fieldType)
        {
            if (IsDateField(fieldType))
            {
                var datePicker = new BeepDatePicker
                {
                    Dock = DockStyle.Fill,
                    UseThemeColors = true
                };
                datePicker.SelectedDate = string.Empty;
                return datePicker;
            }

            return new BeepNumericUpDown
            {
                Dock = DockStyle.Fill,
                ReadOnly = false,
                UseThemeColors = true,
                MinimumValue = decimal.MinValue,
                MaximumValue = decimal.MaxValue,
                DecimalPlaces = ResolveNumericDecimalPlaces(fieldType),
                Value = 0m
            };
        }

        private BeepComboBox CreateQueryOperatorEditor(Models.BeepFieldDefinition fieldDefinition)
        {
            Type? fieldType = ResolveFieldDataType(fieldDefinition.FieldName);
            var comboBox = new BeepComboBox
            {
                Dock = DockStyle.Fill,
                Width = 118,
                Theme = Theme,
                AllowFreeText = false,
                IsEditable = false,
                ListItems = BuildQueryOperatorItems(fieldType)
            };
            comboBox.SelectedValue = ResolveStoredQueryOperator(fieldDefinition.FieldName);
            comboBox.SelectedItemChanged += (_, _) => HandleQueryOperatorChanged(fieldDefinition.FieldName, comboBox);
            return comboBox;
        }

        private bool ShouldUseQueryComboEditor(Models.BeepFieldDefinition fieldDefinition, Type? fieldType)
        {
            if (fieldDefinition.Options != null && fieldDefinition.Options.Count > 0)
            {
                return true;
            }

            Type? effectiveType = Nullable.GetUnderlyingType(fieldType ?? typeof(object)) ?? fieldType;
            if (effectiveType == typeof(bool) || effectiveType?.IsEnum == true)
            {
                return true;
            }

            return _formsHost != null &&
                   !string.IsNullOrWhiteSpace(ManagerBlockName) &&
                   !string.IsNullOrWhiteSpace(fieldDefinition.FieldName) &&
                   _formsHost.IsBlockRegistered(ManagerBlockName) &&
                   _formsHost.HasLov(ManagerBlockName, fieldDefinition.FieldName);
        }

        private void ConfigureQueryComboEditor(BeepComboBox comboBox, Models.BeepFieldDefinition fieldDefinition, Type? fieldType)
        {
            if (fieldDefinition.Options != null && fieldDefinition.Options.Count > 0)
            {
                comboBox.ListItems = BuildOptionItems(fieldDefinition.Options);
                return;
            }

            if (_formsHost != null && !string.IsNullOrWhiteSpace(ManagerBlockName) && _formsHost.IsBlockRegistered(ManagerBlockName) &&
                _formsHost.HasLov(ManagerBlockName, fieldDefinition.FieldName))
            {
                LoadQueryLovItemsAsync(comboBox, fieldDefinition.FieldName);
                return;
            }

            Type? effectiveType = Nullable.GetUnderlyingType(fieldType ?? typeof(object)) ?? fieldType;
            if (effectiveType == typeof(bool))
            {
                comboBox.ListItems = BuildBooleanItems();
                return;
            }

            if (effectiveType?.IsEnum == true)
            {
                comboBox.ListItems = BuildEnumItems(effectiveType);
            }
        }

        private async void LoadQueryLovItemsAsync(BeepComboBox comboBox, string fieldName)
        {
            var hostSnapshot = _formsHost;
            string blockName = ManagerBlockName;
            if (comboBox.IsDisposed || hostSnapshot == null || string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(fieldName))
            {
                return;
            }

            try
            {
                var lov = hostSnapshot.GetLov(blockName, fieldName);
                if (lov == null)
                {
                    return;
                }

                var result = await hostSnapshot.LoadLovDataAsync(blockName, fieldName).ConfigureAwait(true);
                if (!result.Success || comboBox.IsDisposed || !ReferenceEquals(_formsHost, hostSnapshot) ||
                    !string.Equals(blockName, ManagerBlockName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                comboBox.ListItems = BuildLovItems(lov, result.Records);
            }
            catch
            {
            }
        }

        private static BindingList<SimpleItem> BuildBooleanItems()
        {
            return new BindingList<SimpleItem>
            {
                new() { Text = bool.TrueString, Name = bool.TrueString, Value = true, Item = true },
                new() { Text = bool.FalseString, Name = bool.FalseString, Value = false, Item = false }
            };
        }

        private static BindingList<SimpleItem> BuildQueryOperatorItems(Type? fieldType)
        {
            Type? effectiveType = Nullable.GetUnderlyingType(fieldType ?? typeof(object)) ?? fieldType;
            bool isBooleanOrEnum = effectiveType == typeof(bool) || effectiveType?.IsEnum == true;
            bool isComparable = effectiveType == typeof(DateTime) ||
                                effectiveType == typeof(byte) ||
                                effectiveType == typeof(short) ||
                                effectiveType == typeof(int) ||
                                effectiveType == typeof(long) ||
                                effectiveType == typeof(float) ||
                                effectiveType == typeof(double) ||
                                effectiveType == typeof(decimal);

            var items = new BindingList<SimpleItem>
            {
                new() { Text = "Equals", Name = "equals", Value = "=", Item = "=" },
                new() { Text = "Not Equals", Name = "not_equals", Value = "!=", Item = "!=" }
            };

            if (effectiveType == typeof(string) || effectiveType == null)
            {
                items.Clear();
                items.Add(new SimpleItem { Text = "Contains", Name = "contains", Value = "contains", Item = "contains" });
                items.Add(new SimpleItem { Text = "Starts With", Name = "starts_with", Value = "startswith", Item = "startswith" });
                items.Add(new SimpleItem { Text = "Ends With", Name = "ends_with", Value = "endswith", Item = "endswith" });
                items.Add(new SimpleItem { Text = "Equals", Name = "equals", Value = "=", Item = "=" });
                items.Add(new SimpleItem { Text = "Not Equals", Name = "not_equals", Value = "!=", Item = "!=" });
                items.Add(new SimpleItem { Text = "In List", Name = "in_list", Value = "in", Item = "in" });
                items.Add(new SimpleItem { Text = "Not In List", Name = "not_in_list", Value = "not in", Item = "not in" });
                items.Add(new SimpleItem { Text = "Is Blank", Name = "is_blank", Value = "is null", Item = "is null" });
                items.Add(new SimpleItem { Text = "Has Value", Name = "has_value", Value = "is not null", Item = "is not null" });
                return items;
            }

            if (!isBooleanOrEnum && isComparable)
            {
                bool isDate = effectiveType == typeof(DateTime);
                items.Add(new SimpleItem { Text = isDate ? "After" : "Greater Than", Name = "greater_than", Value = ">", Item = ">" });
                items.Add(new SimpleItem { Text = isDate ? "On Or After" : "Greater Or Equal", Name = "greater_or_equal", Value = ">=", Item = ">=" });
                items.Add(new SimpleItem { Text = isDate ? "Before" : "Less Than", Name = "less_than", Value = "<", Item = "<" });
                items.Add(new SimpleItem { Text = isDate ? "On Or Before" : "Less Or Equal", Name = "less_or_equal", Value = "<=", Item = "<=" });
                items.Add(new SimpleItem { Text = "In List", Name = "in_list", Value = "in", Item = "in" });
                items.Add(new SimpleItem { Text = "Not In List", Name = "not_in_list", Value = "not in", Item = "not in" });
                if (SupportsBetweenOperator(fieldType))
                {
                    items.Add(new SimpleItem { Text = "Between", Name = "between", Value = "between", Item = "between" });
                    items.Add(new SimpleItem { Text = isDate ? "Outside Range" : "Not Between", Name = "not_between", Value = "not between", Item = "not between" });
                }
            }

            items.Add(new SimpleItem { Text = "Is Blank", Name = "is_blank", Value = "is null", Item = "is null" });
            items.Add(new SimpleItem { Text = "Has Value", Name = "has_value", Value = "is not null", Item = "is not null" });

            return items;
        }

        private static string GetDefaultQueryOperator(Type? fieldType)
        {
            Type? effectiveType = Nullable.GetUnderlyingType(fieldType ?? typeof(object)) ?? fieldType;
            return effectiveType == typeof(string) || effectiveType == null
                ? "contains"
                : "=";
        }

        private object? ReadQueryEditorValue(Control editor, string fieldName, bool isSecondary)
        {
            return editor switch
            {
                BeepTextBox beepTextBox => string.IsNullOrWhiteSpace(beepTextBox.Text) ? null : beepTextBox.Text.Trim(),
                BeepComboBox comboBox => comboBox.SelectedValue,
                BeepDatePicker beepDatePicker => HasStoredQueryValue(fieldName, isSecondary) && beepDatePicker.SelectedDateTime != DateTime.MinValue
                    ? beepDatePicker.SelectedDateTime
                    : null,
                BeepNumericUpDown beepNumericUpDown => HasStoredQueryValue(fieldName, isSecondary)
                    ? beepNumericUpDown.Value
                    : null,
                QueryListEditor queryListEditor => queryListEditor.Entries.ToList(),
                _ => ConvertEditorValueForDataSource(fieldName, editor.Text)
            };
        }

        private void AttachQueryValueTracking(string fieldName, Control editor, bool isSecondary)
        {
            switch (editor)
            {
                case BeepTextBox beepTextBox:
                    beepTextBox.TextChanged += (_, _) => UpdateQueryCriterionHasValue(fieldName, isSecondary, !string.IsNullOrWhiteSpace(beepTextBox.Text));
                    break;
                case BeepComboBox comboBox:
                    comboBox.SelectedItemChanged += (_, _) => UpdateQueryCriterionHasValue(fieldName, isSecondary, comboBox.SelectedValue != null);
                    break;
                case BeepDatePicker beepDatePicker:
                    beepDatePicker.SelectedDateTimeChanged += (_, _) => UpdateQueryCriterionHasValue(fieldName, isSecondary, beepDatePicker.SelectedDateTime != DateTime.MinValue);
                    break;
                case BeepNumericUpDown beepNumericUpDown:
                    beepNumericUpDown.ValueChanged += (_, _) => UpdateQueryCriterionHasValue(fieldName, isSecondary, true);
                    break;
                case QueryListEditor queryListEditor:
                    queryListEditor.ValuesChanged += (_, _) => UpdateQueryCriterionHasValue(fieldName, isSecondary, queryListEditor.Entries.Count > 0);
                    break;
            }
        }

        private void UpdateQueryCriterionHasValue(string fieldName, bool isSecondary, bool hasValue)
        {
            if (_isApplyingQueryCriteria)
            {
                return;
            }

            QueryCriterionState criterion = GetOrCreateQueryCriterion(fieldName);
            if (isSecondary)
            {
                criterion.HasSecondaryValue = hasValue;
                if (!hasValue)
                {
                    criterion.SecondaryValue = null;
                }

                return;
            }

            criterion.HasValue = hasValue;
            if (!hasValue)
            {
                criterion.Value = null;
            }
        }

        private bool DetermineQueryEditorHasValue(Control editor, string fieldName, object? value, bool isSecondary)
        {
            if (editor is QueryListEditor queryListEditor)
            {
                return queryListEditor.Entries.Count > 0;
            }

            if (editor is BeepDatePicker || editor is BeepNumericUpDown)
            {
                return HasStoredQueryValue(fieldName, isSecondary);
            }

            return !IsQueryValueEmpty(value);
        }

        private bool HasStoredQueryValue(string fieldName, bool isSecondary)
        {
            if (!_queryCriteria.TryGetValue(fieldName, out var criterion))
            {
                return false;
            }

            return isSecondary ? criterion.HasSecondaryValue : criterion.HasValue;
        }

        private void ApplyQueryValueToEditor(Control? editor, object? value, bool hasValue)
        {
            if (editor == null)
            {
                return;
            }

            if (editor is QueryListEditor queryListEditor)
            {
                queryListEditor.SetEntries(hasValue ? NormalizeQueryListValues(value) : null);
                return;
            }

            if (!hasValue)
            {
                switch (editor)
                {
                    case BeepDatePicker beepDatePicker:
                        beepDatePicker.SelectedDate = string.Empty;
                        return;
                    case BeepNumericUpDown beepNumericUpDown:
                        beepNumericUpDown.Value = 0m;
                        return;
                    default:
                        ApplyValueToEditor(editor, null);
                        return;
                }
            }

            ApplyValueToEditor(editor, value);
        }

        private void HandleQueryOperatorChanged(string fieldName, BeepComboBox operatorEditor)
        {
            if (_isApplyingQueryCriteria)
            {
                return;
            }

            QueryCriterionState criterion = GetOrCreateQueryCriterion(fieldName);
            criterion.Operator = ResolveQueryOperatorValue(fieldName, operatorEditor);
            UpdateQueryEditorPresentation(fieldName);
        }

        private void UpdateQueryEditorPresentation(string fieldName)
        {
            if (!_queryEditorGroups.TryGetValue(fieldName, out var group))
            {
                return;
            }

            bool showNoValueHint = _queryCriteria.TryGetValue(fieldName, out var criterion) && IsNoValueOperator(criterion.Operator);
            bool showListEditor = !showNoValueHint && criterion != null && IsListOperator(criterion.Operator) && group.ListEditor != null;
            bool showRange = !showNoValueHint && _queryCriteria.TryGetValue(fieldName, out criterion) && IsRangeOperator(criterion.Operator);

            group.Host.SuspendLayout();
            if (showNoValueHint)
            {
                group.Host.ColumnStyles[0].SizeType = SizeType.Percent;
                group.Host.ColumnStyles[0].Width = 100f;
                group.Host.ColumnStyles[1].SizeType = SizeType.Absolute;
                group.Host.ColumnStyles[1].Width = 0f;
                group.Host.ColumnStyles[2].SizeType = SizeType.Absolute;
                group.Host.ColumnStyles[2].Width = 0f;

                group.PrimaryEditor.Visible = false;
                if (group.SecondaryEditor != null)
                {
                    group.SecondaryEditor.Visible = false;
                }
                if (group.ListEditor != null)
                {
                    group.ListEditor.Visible = false;
                }

                if (group.RangeSeparator != null)
                {
                    group.RangeSeparator.Visible = false;
                }

                if (group.NoValueHintLabel != null)
                {
                    group.NoValueHintLabel.Text = BuildNoValueOperatorHintText(criterion?.Operator);
                    group.NoValueHintLabel.Visible = true;
                }

                if (_queryCriteria.TryGetValue(fieldName, out var noValueState))
                {
                    noValueState.Value = null;
                    noValueState.HasValue = false;
                    noValueState.SecondaryValue = null;
                    noValueState.HasSecondaryValue = false;
                }
            }
            else if (showListEditor && group.ListEditor != null)
            {
                group.Host.ColumnStyles[0].SizeType = SizeType.Percent;
                group.Host.ColumnStyles[0].Width = 100f;
                group.Host.ColumnStyles[1].SizeType = SizeType.Absolute;
                group.Host.ColumnStyles[1].Width = 0f;
                group.Host.ColumnStyles[2].SizeType = SizeType.Absolute;
                group.Host.ColumnStyles[2].Width = 0f;

                group.PrimaryEditor.Visible = false;
                if (group.SecondaryEditor != null)
                {
                    group.SecondaryEditor.Visible = false;
                }
                if (group.RangeSeparator != null)
                {
                    group.RangeSeparator.Visible = false;
                }
                if (group.NoValueHintLabel != null)
                {
                    group.NoValueHintLabel.Visible = false;
                }

                List<string> listValues = NormalizeQueryListValues(criterion?.Value);
                bool previousApplyState = _isApplyingQueryCriteria;
                _isApplyingQueryCriteria = true;
                try
                {
                    group.ListEditor.SetEntries(listValues);
                }
                finally
                {
                    _isApplyingQueryCriteria = previousApplyState;
                }

                group.ListEditor.Visible = true;

                if (_queryCriteria.TryGetValue(fieldName, out var listState))
                {
                    listState.Value = listValues.Count > 0 ? listValues : null;
                    listState.HasValue = listValues.Count > 0;
                    listState.SecondaryValue = null;
                    listState.HasSecondaryValue = false;
                }
            }
            else if (showRange && group.SecondaryEditor != null)
            {
                group.Host.ColumnStyles[0].SizeType = SizeType.Percent;
                group.Host.ColumnStyles[0].Width = 48f;
                group.Host.ColumnStyles[1].SizeType = SizeType.Absolute;
                group.Host.ColumnStyles[1].Width = 28f;
                group.Host.ColumnStyles[2].SizeType = SizeType.Percent;
                group.Host.ColumnStyles[2].Width = 52f;
                group.PrimaryEditor.Visible = true;
                if (group.ListEditor != null)
                {
                    group.ListEditor.Visible = false;
                }
                if (group.RangeSeparator != null)
                {
                    group.RangeSeparator.Text = GetRangeSeparatorText(criterion?.Operator);
                    group.RangeSeparator.Visible = true;
                }

                group.SecondaryEditor.Visible = true;
                if (group.NoValueHintLabel != null)
                {
                    group.NoValueHintLabel.Visible = false;
                }

                if (_queryCriteria.TryGetValue(fieldName, out var rangeState) && rangeState.Value is IEnumerable && rangeState.Value is not string)
                {
                    rangeState.Value = null;
                    rangeState.HasValue = false;
                    bool previousApplyState = _isApplyingQueryCriteria;
                    _isApplyingQueryCriteria = true;
                    try
                    {
                        ApplyQueryValueToEditor(group.PrimaryEditor, null, false);
                    }
                    finally
                    {
                        _isApplyingQueryCriteria = previousApplyState;
                    }
                }
            }
            else
            {
                group.Host.ColumnStyles[0].SizeType = SizeType.Percent;
                group.Host.ColumnStyles[0].Width = 100f;
                group.Host.ColumnStyles[1].SizeType = SizeType.Absolute;
                group.Host.ColumnStyles[1].Width = 0f;
                group.Host.ColumnStyles[2].SizeType = SizeType.Absolute;
                group.Host.ColumnStyles[2].Width = 0f;
                group.PrimaryEditor.Visible = true;
                if (group.RangeSeparator != null)
                {
                    group.RangeSeparator.Visible = false;
                }

                if (group.SecondaryEditor != null)
                {
                    group.SecondaryEditor.Visible = false;
                }
                if (group.ListEditor != null)
                {
                    group.ListEditor.Visible = false;
                }

                if (_queryCriteria.TryGetValue(fieldName, out var state))
                {
                    state.HasSecondaryValue = false;
                    if (!IsNoValueOperator(state.Operator))
                    {
                        state.SecondaryValue = null;
                    }
                }

                if (group.SecondaryEditor != null)
                {
                    bool previousApplyState = _isApplyingQueryCriteria;
                    _isApplyingQueryCriteria = true;
                    try
                    {
                        ApplyQueryValueToEditor(group.SecondaryEditor, null, false);
                    }
                    finally
                    {
                        _isApplyingQueryCriteria = previousApplyState;
                    }
                }

                if (group.NoValueHintLabel != null)
                {
                    group.NoValueHintLabel.Visible = false;
                }

                if (_queryCriteria.TryGetValue(fieldName, out var standardState) && standardState.Value is IEnumerable && standardState.Value is not string)
                {
                    standardState.Value = null;
                    standardState.HasValue = false;
                    bool previousApplyState = _isApplyingQueryCriteria;
                    _isApplyingQueryCriteria = true;
                    try
                    {
                        ApplyQueryValueToEditor(group.PrimaryEditor, null, false);
                    }
                    finally
                    {
                        _isApplyingQueryCriteria = previousApplyState;
                    }
                }
            }

            group.Host.ResumeLayout();
        }

        private async void EditQueryListValues(Models.BeepFieldDefinition fieldDefinition, Type? fieldType, QueryListEditor editor)
        {
            QueryListSelectionOptions? selectionOptions = await ResolveQueryListSelectionOptionsAsync(fieldDefinition, fieldType).ConfigureAwait(true);
            List<string>? updatedValues = ShowQueryListEntryDialog(
                ResolveQueryFieldLabel(fieldDefinition),
                fieldType,
                editor.Entries,
                selectionOptions);
            if (updatedValues == null)
            {
                return;
            }

            editor.SetEntries(updatedValues);
            QueryCriterionState criterion = GetOrCreateQueryCriterion(fieldDefinition.FieldName);
            criterion.Value = updatedValues.Count > 0 ? updatedValues : null;
            criterion.HasValue = updatedValues.Count > 0;
            UpdateWorkflowSurface();
        }

        private async Task<QueryListSelectionOptions?> ResolveQueryListSelectionOptionsAsync(Models.BeepFieldDefinition fieldDefinition, Type? fieldType)
        {
            if (fieldDefinition == null)
            {
                return null;
            }

            if (fieldDefinition.Options != null && fieldDefinition.Options.Count > 0)
            {
                return new QueryListSelectionOptions(
                    CloneSimpleItems(BuildOptionItems(fieldDefinition.Options)),
                    allowManualEntry: false,
                    sourceCaption: "Known Options");
            }

            if (_fieldEditors.TryGetValue(fieldDefinition.FieldName, out var existingEditor) && existingEditor is BeepComboBox existingCombo && existingCombo.ListItems?.Count > 0)
            {
                return new QueryListSelectionOptions(
                    CloneSimpleItems(existingCombo.ListItems),
                    allowManualEntry: existingCombo.AllowFreeText,
                    sourceCaption: "Known Values");
            }

            if (_formsHost != null && !string.IsNullOrWhiteSpace(ManagerBlockName) && !string.IsNullOrWhiteSpace(fieldDefinition.FieldName) &&
                _formsHost.IsBlockRegistered(ManagerBlockName) && _formsHost.HasLov(ManagerBlockName, fieldDefinition.FieldName))
            {
                var lov = _formsHost.GetLov(ManagerBlockName, fieldDefinition.FieldName);
                if (lov != null)
                {
                    try
                    {
                        var result = await _formsHost.LoadLovDataAsync(ManagerBlockName, fieldDefinition.FieldName).ConfigureAwait(true);
                        if (result.Success)
                        {
                            return new QueryListSelectionOptions(
                                CloneSimpleItems(BuildLovItems(lov, result.Records)),
                                allowManualEntry: lov.ValidationType != LOVValidationType.ListOnly,
                                sourceCaption: "LOV Values");
                        }
                    }
                    catch
                    {
                    }
                }
            }

            Type? effectiveType = Nullable.GetUnderlyingType(fieldType ?? typeof(object)) ?? fieldType;
            if (effectiveType?.IsEnum == true)
            {
                return new QueryListSelectionOptions(
                    CloneSimpleItems(BuildEnumItems(effectiveType)),
                    allowManualEntry: false,
                    sourceCaption: "Enum Values");
            }

            return null;
        }

        private List<string>? ShowQueryListEntryDialog(string fieldLabel, Type? fieldType, IReadOnlyList<string> currentValues, QueryListSelectionOptions? selectionOptions)
        {
            using Form dialog = new()
            {
                Text = $"Configure List Values - {fieldLabel}",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false,
                ClientSize = new System.Drawing.Size(560, 400),
                ShowInTaskbar = false
            };

            BindingList<SimpleItem> availableItems = selectionOptions?.Items ?? new BindingList<SimpleItem>();
            BeepListBox? knownValuesList = null;
            var customValues = new BindingList<SimpleItem>();
            bool allowManualEntry = selectionOptions == null || selectionOptions.AllowManualEntry;

            BeepLabel descriptionLabel = new()
            {
                Dock = DockStyle.Top,
                Height = 46,
                Multiline = true,
                WordWrap = true,
                Text = BuildQueryListDialogDescription(fieldLabel, fieldType, selectionOptions, allowManualEntry),
                UseThemeColors = true,
                Margin = new Padding(12, 12, 12, 8)
            };

            TableLayoutPanel contentLayout = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(12, 0, 12, 8)
            };
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            void AddContentRow(Control control, int height, SizeType sizeType)
            {
                int rowIndex = contentLayout.RowCount - 1;
                contentLayout.RowStyles.Add(new RowStyle(sizeType, height));
                contentLayout.Controls.Add(control, 0, rowIndex);
                contentLayout.RowCount++;
            }

            if (selectionOptions != null && availableItems.Count > 0)
            {
                knownValuesList = new BeepListBox
                {
                    Dock = DockStyle.Fill,
                    UseThemeColors = true,
                    ListItems = CloneSimpleItems(availableItems),
                    ShowCheckBox = true,
                    MultiSelect = true,
                    SelectionMode = SelectionModeEnum.MultiSimple,
                    ListBoxType = ListBoxType.MultiSelectionTeal,
                    Margin = new Padding(0, 0, 0, 8)
                };

                AddContentRow(new BeepLabel
                {
                    Dock = DockStyle.Fill,
                    Height = 18,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                    UseThemeColors = true,
                    Text = selectionOptions.SourceCaption
                }, 22, SizeType.Absolute);
                AddContentRow(knownValuesList, 52, SizeType.Percent);
            }

            HashSet<string> matchedKnownValues = new(StringComparer.OrdinalIgnoreCase);
            foreach (string currentValue in currentValues)
            {
                if (knownValuesList != null && TrySelectKnownQueryValue(knownValuesList, currentValue, matchedKnownValues))
                {
                    continue;
                }

                customValues.Add(new SimpleItem { Text = currentValue, Name = currentValue, Value = currentValue, Item = currentValue });
            }

            BeepListBox customValuesList = new()
            {
                Dock = DockStyle.Fill,
                UseThemeColors = true,
                ListItems = customValues,
                Margin = new Padding(0, 0, 0, 8)
            };

            bool showCustomSection = selectionOptions == null || allowManualEntry || customValues.Count > 0;

            TableLayoutPanel entryPanel = new()
            {
                Dock = DockStyle.Top,
                ColumnCount = 3,
                RowCount = 1,
                Height = 36,
                Margin = new Padding(0, 0, 0, 8),
                Padding = new Padding(0)
            };
            entryPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            entryPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88f));
            entryPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88f));

            BeepTextBox inputBox = new()
            {
                Dock = DockStyle.Fill,
                Theme = Theme,
                Text = string.Empty,
                ReadOnly = !allowManualEntry
            };

            BeepButton addButton = new()
            {
                Dock = DockStyle.Fill,
                Text = "Add",
                Theme = Theme,
                ShowShadow = false,
                Enabled = false,
                Margin = new Padding(8, 0, 0, 0)
            };

            BeepButton removeButton = new()
            {
                Dock = DockStyle.Fill,
                Text = "Remove",
                Theme = Theme,
                ShowShadow = false,
                Enabled = false,
                Margin = new Padding(8, 0, 0, 0)
            };

            entryPanel.Controls.Add(inputBox, 0, 0);
            entryPanel.Controls.Add(addButton, 1, 0);
            entryPanel.Controls.Add(removeButton, 2, 0);

            if (showCustomSection)
            {
                AddContentRow(new BeepLabel
                {
                    Dock = DockStyle.Fill,
                    Height = 18,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                    UseThemeColors = true,
                    Text = allowManualEntry ? "Custom Values" : "Existing Unmatched Values"
                }, 22, SizeType.Absolute);
                AddContentRow(entryPanel, 36, SizeType.Absolute);
                AddContentRow(customValuesList, 48, SizeType.Percent);
            }

            FlowLayoutPanel buttonsPanel = new()
            {
                Dock = DockStyle.Bottom,
                Height = 42,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(12, 6, 12, 6)
            };

            BeepButton okButton = new()
            {
                Width = 96,
                Text = "OK",
                Theme = Theme,
                ShowShadow = false
            };
            BeepButton cancelButton = new()
            {
                Width = 96,
                Text = "Cancel",
                Theme = Theme,
                ShowShadow = false
            };

            okButton.Click += (_, _) => dialog.DialogResult = DialogResult.OK;
            cancelButton.Click += (_, _) => dialog.DialogResult = DialogResult.Cancel;
            buttonsPanel.Controls.Add(okButton);
            buttonsPanel.Controls.Add(cancelButton);

            void SyncEntrySelection(SimpleItem? selectedItem)
            {
                inputBox.Text = selectedItem?.Value?.ToString() ?? string.Empty;
                removeButton.Enabled = selectedItem != null;
                addButton.Text = selectedItem == null ? "Add" : "Update";
                addButton.Enabled = allowManualEntry && !string.IsNullOrWhiteSpace(inputBox.Text);
            }

            void ShowQueryListDialogMessage(string message, string caption, MessageBoxIcon icon)
            {
                if (_formsHost is BeepForms forms)
                {
                    if (!string.IsNullOrWhiteSpace(BlockName))
                    {
                        forms.TrySetActiveBlock(BlockName);
                    }

                    if (icon == MessageBoxIcon.Warning)
                    {
                        forms.ShowWarning(message);
                    }
                    else if (icon == MessageBoxIcon.Error)
                    {
                        forms.ShowError(message);
                    }
                    else
                    {
                        forms.ShowInfo(message);
                    }

                    return;
                }

                MessageBox.Show(dialog, message, caption, MessageBoxButtons.OK, icon);
            }

            customValuesList.SelectedItemChanged += (_, args) => SyncEntrySelection(args.SelectedItem);
            inputBox.TextChanged += (_, _) => addButton.Enabled = allowManualEntry && !string.IsNullOrWhiteSpace(inputBox.Text);
            removeButton.Click += (_, _) =>
            {
                if (customValuesList.SelectedItem == null)
                {
                    return;
                }

                customValues.Remove(customValuesList.SelectedItem);
                customValuesList.SelectedItem = null;
                SyncEntrySelection(null);
            };
            addButton.Click += (_, _) =>
            {
                if (!allowManualEntry)
                {
                    return;
                }

                if (!TryNormalizeQueryListEntry(fieldType, inputBox.Text, out string normalizedValue, out string validationMessage))
                {
                    ShowQueryListDialogMessage(validationMessage, "Invalid List Entry", MessageBoxIcon.Warning);
                    return;
                }

                if (knownValuesList != null && knownValuesList.ListItems.Any(item => string.Equals(GetQueryListItemValue(item), normalizedValue, StringComparison.OrdinalIgnoreCase)))
                {
                    ShowQueryListDialogMessage("That value already exists in the known values list. Select it there instead of adding a duplicate custom value.", "Duplicate Value", MessageBoxIcon.Information);
                    return;
                }

                SimpleItem? selectedItem = customValuesList.SelectedItem;
                if (selectedItem != null)
                {
                    selectedItem.Text = normalizedValue;
                    selectedItem.Name = normalizedValue;
                    selectedItem.Value = normalizedValue;
                    selectedItem.Item = normalizedValue;
                    customValuesList.Invalidate();
                }
                else
                {
                    if (customValues.Any(item => string.Equals(item.Value?.ToString(), normalizedValue, StringComparison.OrdinalIgnoreCase)))
                    {
                        ShowQueryListDialogMessage("That value is already in the list.", "Duplicate Value", MessageBoxIcon.Information);
                        return;
                    }

                    customValues.Add(new SimpleItem { Text = normalizedValue, Name = normalizedValue, Value = normalizedValue, Item = normalizedValue });
                }

                inputBox.Text = string.Empty;
                customValuesList.SelectedItem = null;
                SyncEntrySelection(null);
            };

            if (!allowManualEntry)
            {
                inputBox.Text = "Manual entry is disabled for this field.";
                addButton.Enabled = false;
                inputBox.Enabled = false;
            }

            dialog.Controls.Add(contentLayout);
            dialog.Controls.Add(buttonsPanel);
            dialog.Controls.Add(descriptionLabel);

            DialogResult result = dialog.ShowDialog(FindForm());
            if (result != DialogResult.OK)
            {
                return null;
            }

            var collectedValues = new List<string>();
            var seenValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (knownValuesList != null)
            {
                foreach (SimpleItem selectedItem in knownValuesList.SelectedItems)
                {
                    string rawValue = GetQueryListItemValue(selectedItem);
                    if (!string.IsNullOrWhiteSpace(rawValue) && seenValues.Add(rawValue))
                    {
                        collectedValues.Add(rawValue);
                    }
                }
            }

            foreach (string customValue in customValues
                .Select(item => item.Value?.ToString() ?? item.Text)
                .Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                if (seenValues.Add(customValue))
                {
                    collectedValues.Add(customValue);
                }
            }

            return collectedValues;
        }

        private static string BuildQueryListDialogDescription(string fieldLabel, Type? fieldType, QueryListSelectionOptions? selectionOptions, bool allowManualEntry)
        {
            if (selectionOptions == null)
            {
                return $"Add one value per entry for '{fieldLabel}'. {BuildQueryListEntryHint(fieldType)}";
            }

            string sourceText = $"Select one or more values from {selectionOptions.SourceCaption.ToLowerInvariant()} for '{fieldLabel}'.";
            if (!allowManualEntry)
            {
                return sourceText;
            }

            return sourceText + " You can also add custom values that are not in the known list.";
        }

        private static bool TrySelectKnownQueryValue(BeepListBox knownValuesList, string currentValue, ISet<string> matchedValues)
        {
            foreach (SimpleItem item in knownValuesList.ListItems)
            {
                string rawItemValue = GetQueryListItemValue(item);
                if (!string.Equals(rawItemValue, currentValue, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                knownValuesList.SetItemCheckbox(item, true);
                matchedValues.Add(currentValue);
                return true;
            }

            return false;
        }

        private static BindingList<SimpleItem> CloneSimpleItems(IEnumerable<SimpleItem> items)
        {
            var clone = new BindingList<SimpleItem>();
            if (items == null)
            {
                return clone;
            }

            foreach (SimpleItem item in items)
            {
                if (item == null)
                {
                    continue;
                }

                clone.Add(new SimpleItem
                {
                    Text = item.Text,
                    Name = item.Name,
                    Description = item.Description,
                    SubText = item.SubText,
                    Value = item.Value,
                    Item = item.Item,
                    ImagePath = item.ImagePath,
                    IsCheckable = item.IsCheckable
                });
            }

            return clone;
        }

        private static string GetQueryListItemValue(SimpleItem item)
        {
            object? value = item?.Value ?? item?.Item ?? item?.Text;
            return ConvertQueryValueToFilterText(value);
        }

        private static bool TryNormalizeQueryListEntry(Type? fieldType, string rawValue, out string normalizedValue, out string validationMessage)
        {
            normalizedValue = string.Empty;
            validationMessage = string.Empty;

            string candidate = rawValue?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(candidate))
            {
                validationMessage = "Enter a value before adding it to the list.";
                return false;
            }

            if (candidate.IndexOfAny(new[] { ',', ';', '|' }) >= 0)
            {
                validationMessage = "List entries cannot contain commas, semicolons, or pipe characters because those are used as filter separators.";
                return false;
            }

            Type? effectiveType = Nullable.GetUnderlyingType(fieldType ?? typeof(object)) ?? fieldType;
            if (effectiveType == null || effectiveType == typeof(string))
            {
                normalizedValue = candidate;
                return true;
            }

            if (effectiveType == typeof(DateTime))
            {
                if (DateTime.TryParse(candidate, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime currentCultureDate) ||
                    DateTime.TryParse(candidate, CultureInfo.InvariantCulture, DateTimeStyles.None, out currentCultureDate))
                {
                    normalizedValue = currentCultureDate.ToString("o", CultureInfo.InvariantCulture);
                    return true;
                }

                validationMessage = "Enter a valid date/time value for this list entry.";
                return false;
            }

            if (IsNumericField(fieldType))
            {
                if (decimal.TryParse(candidate, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal currentCultureNumber) ||
                    decimal.TryParse(candidate, NumberStyles.Any, CultureInfo.InvariantCulture, out currentCultureNumber))
                {
                    normalizedValue = currentCultureNumber.ToString(CultureInfo.InvariantCulture);
                    return true;
                }

                validationMessage = "Enter a valid numeric value for this list entry.";
                return false;
            }

            normalizedValue = candidate;
            return true;
        }

        private static string BuildQueryListEntryHint(Type? fieldType)
        {
            Type? effectiveType = Nullable.GetUnderlyingType(fieldType ?? typeof(object)) ?? fieldType;
            if (effectiveType == typeof(DateTime))
            {
                return "Dates will be normalized when they are added.";
            }

            if (IsNumericField(fieldType))
            {
                return "Numbers will be normalized when they are added.";
            }

            return "Entries are matched as individual values.";
        }

        private static List<string> NormalizeQueryListValues(object? value)
        {
            if (value == null || value == DBNull.Value)
            {
                return new List<string>();
            }

            if (value is string singleValue)
            {
                return singleValue
                    .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(item => item.Trim())
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .ToList();
            }

            if (value is IEnumerable<string> stringValues)
            {
                return stringValues
                    .Select(item => item?.Trim())
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .Cast<string>()
                    .ToList();
            }

            if (value is IEnumerable enumerable)
            {
                var normalized = new List<string>();
                foreach (object? item in enumerable)
                {
                    string? text = item?.ToString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        normalized.Add(text);
                    }
                }

                return normalized;
            }

            return new List<string> { value.ToString() ?? string.Empty };
        }

        private static bool IsNoValueOperator(string? operatorValue)
        {
            return string.Equals(operatorValue, "is null", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(operatorValue, "is not null", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(operatorValue, "isnull", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(operatorValue, "isnotnull", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsListOperator(string? operatorValue)
        {
            return string.Equals(operatorValue, "in", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(operatorValue, "not in", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(operatorValue, "notin", StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildNoValueOperatorHintText(string? operatorValue)
        {
            return string.Equals(operatorValue, "is not null", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(operatorValue, "isnotnull", StringComparison.OrdinalIgnoreCase)
                ? "No value entry is required. The query will keep rows where this field has a value."
                : "No value entry is required. The query will keep rows where this field is blank or null.";
        }

        private static bool SupportsBetweenOperator(Type? fieldType)
        {
            return IsDateField(fieldType) || IsNumericField(fieldType);
        }

        private static bool SupportsListOperator(Type? fieldType)
        {
            Type? effectiveType = Nullable.GetUnderlyingType(fieldType ?? typeof(object)) ?? fieldType;
            if (effectiveType == typeof(bool) || effectiveType?.IsEnum == true)
            {
                return false;
            }

            return effectiveType == null || effectiveType == typeof(string) || IsDateField(fieldType) || IsNumericField(fieldType);
        }

        private static bool IsRangeOperator(string? operatorValue)
        {
            return string.Equals(operatorValue, "between", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(operatorValue, "not between", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(operatorValue, "notbetween", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetRangeSeparatorText(string? operatorValue)
        {
            return string.Equals(operatorValue, "not between", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(operatorValue, "notbetween", StringComparison.OrdinalIgnoreCase)
                ? "outside"
                : "to";
        }

        private static bool IsDateField(Type? fieldType)
        {
            Type? effectiveType = Nullable.GetUnderlyingType(fieldType ?? typeof(object)) ?? fieldType;
            return effectiveType == typeof(DateTime);
        }

        private static bool IsNumericField(Type? fieldType)
        {
            Type? effectiveType = Nullable.GetUnderlyingType(fieldType ?? typeof(object)) ?? fieldType;
            return effectiveType == typeof(byte) ||
                   effectiveType == typeof(short) ||
                   effectiveType == typeof(int) ||
                   effectiveType == typeof(long) ||
                   effectiveType == typeof(float) ||
                   effectiveType == typeof(double) ||
                   effectiveType == typeof(decimal);
        }

        private static int ResolveNumericDecimalPlaces(Type? fieldType)
        {
            Type? effectiveType = Nullable.GetUnderlyingType(fieldType ?? typeof(object)) ?? fieldType;
            return effectiveType == typeof(float) || effectiveType == typeof(double) || effectiveType == typeof(decimal)
                ? 4
                : 0;
        }

        private static string ResolveQueryFieldLabel(Models.BeepFieldDefinition fieldDefinition)
        {
            return string.IsNullOrWhiteSpace(fieldDefinition.Label)
                ? fieldDefinition.FieldName
                : fieldDefinition.Label;
        }

        private void UpdateWorkflowSurface()
        {
            if (_workflowPanel == null || _workflowLabel == null || _clearQueryButton == null || _executeQueryButton == null)
            {
                return;
            }

            string relationText = BuildMasterDetailContextText();
            string runtimeText = BuildRuntimeActivityText();
            var workflowLines = new List<string>();

            if (!string.IsNullOrWhiteSpace(relationText))
            {
                workflowLines.Add(relationText);
            }

            if (ViewState.IsQueryMode)
            {
                workflowLines.Add("Enter criteria below. Execute and clear stay in the UI; FormsManager performs the query and mode transition.");
            }

            if (!string.IsNullOrWhiteSpace(runtimeText))
            {
                workflowLines.Add(runtimeText);
            }

            bool showWorkflowPanel = workflowLines.Count > 0;
            if (!showWorkflowPanel)
            {
                _workflowPanel.Visible = false;
                _workflowLabel.Text = string.Empty;
                return;
            }

            _workflowPanel.Visible = true;
            _workflowPanel.Height = Math.Max(ViewState.IsQueryMode ? 60 : 42, 24 + workflowLines.Count * 18);

            _clearQueryButton.Visible = ViewState.IsQueryMode;
            _executeQueryButton.Visible = ViewState.IsQueryMode;
            _clearQueryButton.Enabled = ViewState.IsQueryMode;
            _executeQueryButton.Enabled = ViewState.IsQueryMode && IsManagerQueryAllowed();

            _workflowLabel.Text = string.Join(Environment.NewLine, workflowLines);
        }

        private string BuildRuntimeActivityText()
        {
            var parts = new List<string>();

            if (ViewState.TriggerCount > 0)
            {
                parts.Add($"Triggers: {ViewState.TriggerCount} total (F{ViewState.FormTriggerCount}/B{ViewState.BlockTriggerCount}/R{ViewState.RecordTriggerCount}/I{ViewState.ItemTriggerCount}).");
            }

            if (!string.IsNullOrWhiteSpace(ViewState.LastTriggerText))
            {
                parts.Add($"Last trigger: {ViewState.LastTriggerText}.");
            }

            if (!string.IsNullOrWhiteSpace(ViewState.LastUnitOfWorkActivityText))
            {
                parts.Add($"Last activity: {ViewState.LastUnitOfWorkActivityText}.");
            }

            return parts.Count == 0 ? string.Empty : string.Join(" ", parts);
        }

        private string BuildMasterDetailContextText()
        {
            if (_formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName) || !_formsHost.IsBlockRegistered(ManagerBlockName))
            {
                return string.Empty;
            }

            var blockInfo = _formsHost.GetBlockInfo(ManagerBlockName);
            string masterBlockName = blockInfo?.MasterBlockName;
            var detailBlocks = (_formsHost.GetDetailBlockNames(ManagerBlockName) ?? Enumerable.Empty<string>()).ToList();

            if (!string.IsNullOrWhiteSpace(masterBlockName))
            {
                if (!string.IsNullOrWhiteSpace(blockInfo?.MasterKeyField) && !string.IsNullOrWhiteSpace(blockInfo.ForeignKeyField))
                {
                    return $"Detail block of '{masterBlockName}' on {blockInfo.MasterKeyField} -> {blockInfo.ForeignKeyField}.";
                }

                return $"Detail block of '{masterBlockName}'.";
            }

            if (detailBlocks.Count > 0)
            {
                return $"Master block for: {string.Join(", ", detailBlocks)}.";
            }

            return string.Empty;
        }

        private bool IsManagerQueryAllowed()
        {
            if (_formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName) || !_formsHost.IsBlockRegistered(ManagerBlockName))
            {
                return false;
            }

            return _formsHost.GetBlockInfo(ManagerBlockName)?.QueryAllowed ?? false;
        }
    }
}