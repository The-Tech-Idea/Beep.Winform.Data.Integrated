using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls;
using TheTechIdea.Beep.Winform.Controls.Base;
using TheTechIdea.Beep.Winform.Controls.CheckBoxes;
using TheTechIdea.Beep.Winform.Controls.Numerics;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks
{
    public partial class BeepBlock
    {
        // Read-only mirror of FormsManager.GetUnitOfWork(ManagerBlockName).
        // One BeepBlock → one entity → one UnitOfWork, always owned and provided by FormsManager.
        // This field is ONLY assigned in SyncRecordBinding (called from SyncFromManager).
        // All mutations (commit, rollback, new, delete, position) must route through FormsManager.
        private IUnitofWork? _boundUnitOfWork;
        private bool _isSyncingRecordBinding;
        private bool _isApplyingLovRelatedFieldMappings;

        private void InitializeRecordBinding()
        {
            _recordBindingSource = new BindingSource();
            _recordBindingSource.CurrentChanged += RecordBindingSource_CurrentChanged;
            _recordBindingSource.PositionChanged += RecordBindingSource_PositionChanged;
            _recordBindingSource.ListChanged += RecordBindingSource_ListChanged;
        }

        private void SyncRecordBinding(IUnitofWork? unitOfWork)
        {
            _boundUnitOfWork = unitOfWork;

            if (_recordBindingSource == null)
            {
                return;
            }

            _isSyncingRecordBinding = true;
            try
            {
                object? units = unitOfWork?.Units;
                if (!ReferenceEquals(_recordBindingSource.DataSource, units))
                {
                    _recordBindingSource.DataSource = units;
                }

                SyncBindingSourcePositionFromUnitOfWork();
                UpdateRecordViewState(unitOfWork);
                RefreshValidationState();
            }
            finally
            {
                _isSyncingRecordBinding = false;
            }
        }

        private void ResetRecordBinding()
        {
            _boundUnitOfWork = null;

            if (_recordBindingSource == null)
            {
                return;
            }

            _isSyncingRecordBinding = true;
            try
            {
                _recordBindingSource.DataSource = null;
                UpdateRecordViewState(null);
                ResetValidationState();
            }
            finally
            {
                _isSyncingRecordBinding = false;
            }
        }

        private void SyncBindingSourcePositionFromUnitOfWork()
        {
            if (_recordBindingSource == null || _boundUnitOfWork == null || _boundUnitOfWork.Units is not IList units)
            {
                return;
            }

            object? currentItem = _boundUnitOfWork.CurrentItem;
            if (currentItem == null)
            {
                return;
            }

            int targetIndex = IndexOfItem(units, currentItem);
            if (targetIndex >= 0 && targetIndex < _recordBindingSource.Count && _recordBindingSource.Position != targetIndex)
            {
                _recordBindingSource.Position = targetIndex;
            }
        }

        private static int IndexOfItem(IList items, object currentItem)
        {
            for (int index = 0; index < items.Count; index++)
            {
                object? candidate = items[index];
                if (ReferenceEquals(candidate, currentItem) || Equals(candidate, currentItem))
                {
                    return index;
                }
            }

            return -1;
        }

        private void RecordBindingSource_CurrentChanged(object? sender, EventArgs e)
        {
            SyncUnitOfWorkFromBindingSource();
        }

        private void RecordBindingSource_PositionChanged(object? sender, EventArgs e)
        {
            SyncUnitOfWorkFromBindingSource();
        }

        private void RecordBindingSource_ListChanged(object? sender, ListChangedEventArgs e)
        {
            if (_isSyncingRecordBinding)
            {
                return;
            }

            UpdateRecordViewState(_boundUnitOfWork);

            if (e.ListChangedType == ListChangedType.ItemChanged)
            {
                RefreshValidationState();
            }
        }

        private void SyncUnitOfWorkFromBindingSource()
        {
            UpdateRecordViewState(_boundUnitOfWork);

            if (_isSyncingRecordBinding || _recordBindingSource == null || _boundUnitOfWork == null)
            {
                return;
            }

            int position = _recordBindingSource.Position;
            if (position < 0)
            {
                return;
            }

            _isSyncingRecordBinding = true;
            try
            {
                // Route cursor movement through BeepForms host — never access FormsManager from BeepBlock.
                _formsHost?.SetBlockCurrentRecordIndex(ManagerBlockName, position);
                _viewState.IsDirty = _boundUnitOfWork?.IsDirty ?? false;
                RefreshValidationState();
            }
            finally
            {
                _isSyncingRecordBinding = false;
            }
        }

        private void UpdateRecordViewState(IUnitofWork? unitOfWork)
        {
            int count = _recordBindingSource?.Count ?? 0;
            _viewState.IsDirty = unitOfWork?.IsDirty ?? false;
            _viewState.RecordCount = count > 0 ? count : (unitOfWork?.TotalItemCount ?? 0);
            _viewState.CurrentRecordIndex = _recordBindingSource?.Position >= 0 ? _recordBindingSource.Position : -1;
            UpdateSummaryText();
            NotifyViewStateChanged();
        }

        private void BindEditorToCurrentRecord(Control editor, Models.BeepFieldDefinition fieldDefinition)
        {
            if (_recordBindingSource == null || string.IsNullOrWhiteSpace(fieldDefinition.FieldName))
            {
                return;
            }

            string bindingProperty = string.IsNullOrWhiteSpace(fieldDefinition.BindingProperty)
                ? Services.BeepFieldControlTypeRegistry.ResolveDefaultBindingProperty(fieldDefinition.ControlType, fieldDefinition.EditorKey)
                : fieldDefinition.BindingProperty;
            if (string.IsNullOrWhiteSpace(bindingProperty) || editor.GetType().GetProperty(bindingProperty, BindingFlags.Public | BindingFlags.Instance) == null)
            {
                bindingProperty = ResolveEditorBindingProperty(editor);
            }

            var existingBinding = editor.DataBindings[bindingProperty];
            if (existingBinding != null)
            {
                editor.DataBindings.Remove(existingBinding);
            }

            var binding = new Binding(bindingProperty, _recordBindingSource, fieldDefinition.FieldName, true, DataSourceUpdateMode.OnPropertyChanged)
            {
                FormattingEnabled = true
            };

            binding.Format += (_, args) => args.Value = ConvertDataSourceValueForEditor(editor, args.Value);
            binding.Parse += (_, args) => args.Value = ConvertEditorValueForDataSource(fieldDefinition.FieldName, args.Value);
            editor.DataBindings.Add(binding);

            if (editor is IBeepUIComponent beepComponent)
            {
                beepComponent.BoundProperty = bindingProperty;
                beepComponent.DataSourceProperty = fieldDefinition.FieldName;
            }

            if (editor is BeepComboBox comboBox)
            {
                ConfigureComboEditor(comboBox, fieldDefinition);
            }
        }

        private void ApplyCurrentRecordToEditors()
        {
            if (ViewState.IsQueryMode)
            {
                ApplyQueryCriteriaToEditors();
                return;
            }

            if (_recordBindingSource?.DataSource != null)
            {
                _recordBindingSource.ResetBindings(false);
                UpdateRecordViewState(_boundUnitOfWork);
                return;
            }

            var definition = EffectiveDefinition;
            if (definition?.Fields == null || definition.Fields.Count == 0)
            {
                return;
            }

            object? currentRecord = GetCurrentRecord();

            foreach (var field in definition.Fields)
            {
                if (!_fieldEditors.TryGetValue(field.FieldName, out var editor))
                {
                    continue;
                }

                object? value = currentRecord == null ? null : GetRecordValue(currentRecord, field.FieldName);
                ApplyValueToEditor(editor, value);
            }
        }

        private object? GetCurrentRecord()
        {
            if (_recordBindingSource?.Current != null)
            {
                return _recordBindingSource.Current;
            }

            if (_formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName))
            {
                return null;
            }

            return _formsHost.GetCurrentBlockItem(ManagerBlockName);
        }

        private static object? GetRecordValue(object record, string fieldName)
        {
            var recordType = record.GetType();
            var property = recordType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));

            if (property != null)
            {
                return property.GetValue(record);
            }

            var field = recordType
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));

            return field?.GetValue(record);
        }

        private bool SetRecordValue(object record, string fieldName, object? value)
        {
            if (record == null || string.IsNullOrWhiteSpace(fieldName))
            {
                return false;
            }

            var manager = _formsHost?.FormsManager; // field-value setter: FormsManager owns coercion
            if (manager?.SetFieldValue(record, fieldName, value) == true)
            {
                return true;
            }

            var recordType = record.GetType();
            var property = recordType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));

            if (property?.CanWrite == true)
            {
                property.SetValue(record, ConvertValueForAssignment(property.PropertyType, value));
                return true;
            }

            var field = recordType
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));

            if (field != null && !field.IsInitOnly)
            {
                field.SetValue(record, ConvertValueForAssignment(field.FieldType, value));
                return true;
            }

            return false;
        }

        private static string ResolveEditorBindingProperty(Control editor)
        {
            return editor switch
            {
                BeepCheckBoxBool => nameof(BeepCheckBoxBool.CurrentValue),
                BeepDatePicker => nameof(BeepDatePicker.SelectedDateTime),
                BeepNumericUpDown => nameof(BeepNumericUpDown.Value),
                BeepComboBox => nameof(BeepComboBox.SelectedValue),
                BeepTextBox => nameof(BeepTextBox.Text),
                TextBoxBase => nameof(TextBoxBase.Text),
                ComboBox => nameof(ComboBox.Text),
                CheckBox => nameof(CheckBox.Checked),
                _ => ResolveEditorBindingPropertyByReflection(editor)
            };
        }

        private static string ResolveEditorBindingPropertyByReflection(Control editor)
        {
            foreach (string propertyName in new[] { "CurrentValue", "SelectedDateTime", "SelectedValue", "Value", "Checked", nameof(Control.Text) })
            {
                if (editor.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance) != null)
                {
                    return propertyName;
                }
            }

            return nameof(Control.Text);
        }

        private object? ConvertDataSourceValueForEditor(Control editor, object? value)
        {
            string textValue = value == null || value == DBNull.Value ? string.Empty : value.ToString() ?? string.Empty;

            if (editor is BeepCheckBoxBool)
            {
                return TryConvertToBoolean(value, textValue);
            }

            if (editor is BeepNumericUpDown)
            {
                return TryConvertToDecimal(value, out var numericValue) ? numericValue : 0m;
            }

            if (editor is BeepDatePicker)
            {
                return TryConvertToDateTime(value, textValue, out var dateTimeValue) ? dateTimeValue : DateTime.MinValue;
            }

            if (editor is BeepComboBox)
            {
                return value == DBNull.Value ? null : value;
            }

            if (editor is BeepTextBox || editor is TextBoxBase || editor is ComboBox)
            {
                return textValue;
            }

            return value;
        }

        private object? ConvertEditorValueForDataSource(string fieldName, object? value)
        {
            Type? targetType = ResolveFieldDataType(fieldName);
            if (targetType == null)
            {
                return value;
            }

            if (value == null || value == DBNull.Value)
            {
                if (!targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null)
                {
                    return null;
                }

                return Activator.CreateInstance(targetType);
            }

            try
            {
                return ConvertToTargetType(targetType, value);
            }
            catch
            {
                return value;
            }
        }

        private Type? ResolveFieldDataType(string fieldName)
        {
            object? currentRecord = GetCurrentRecord();

            if (currentRecord != null)
            {
                var property = currentRecord.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                if (property != null)
                {
                    return property.PropertyType;
                }

                var field = currentRecord.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(x => string.Equals(x.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                if (field != null)
                {
                    return field.FieldType;
                }
            }

            if (_formsHost != null && !string.IsNullOrWhiteSpace(ManagerBlockName) && _formsHost.IsBlockRegistered(ManagerBlockName))
            {
                var entityField = ResolveEntityStructure(_formsHost.GetBlockInfo(ManagerBlockName))?.Fields?
                    .FirstOrDefault(x => string.Equals(x.FieldName, fieldName, StringComparison.OrdinalIgnoreCase));

                if (entityField != null)
                {
                    return ResolveClrType(entityField);
                }
            }

            return typeof(string);
        }

        private static object? ConvertToTargetType(Type targetType, object value)
        {
            Type effectiveType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            string textValue = value.ToString() ?? string.Empty;

            if (effectiveType == typeof(string))
            {
                return textValue;
            }

            if (effectiveType == typeof(bool))
            {
                return TryConvertToBoolean(value, textValue);
            }

            if (effectiveType == typeof(DateTime))
            {
                return TryConvertToDateTime(value, textValue, out var dateValue) ? dateValue : DateTime.MinValue;
            }

            if (effectiveType.IsEnum)
            {
                return Enum.TryParse(effectiveType, textValue, true, out var enumValue)
                    ? enumValue
                    : Activator.CreateInstance(effectiveType);
            }

            if (TryConvertToDecimal(value, out var decimalValue))
            {
                return effectiveType switch
                {
                    var x when x == typeof(decimal) => decimalValue,
                    var x when x == typeof(byte) => (byte)decimalValue,
                    var x when x == typeof(short) => (short)decimalValue,
                    var x when x == typeof(int) => (int)decimalValue,
                    var x when x == typeof(long) => (long)decimalValue,
                    var x when x == typeof(float) => (float)decimalValue,
                    var x when x == typeof(double) => (double)decimalValue,
                    _ => Convert.ChangeType(decimalValue, effectiveType)
                };
            }

            return Convert.ChangeType(value, effectiveType);
        }

        private static object? ConvertValueForAssignment(Type targetType, object? value)
        {
            if (value == null || value == DBNull.Value)
            {
                if (!targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null)
                {
                    return null;
                }

                return Activator.CreateInstance(targetType);
            }

            return ConvertToTargetType(targetType, value);
        }

        private static void ApplyValueToEditor(Control editor, object? value)
        {
            string textValue = value == null || value == DBNull.Value ? string.Empty : value.ToString() ?? string.Empty;

            if (editor is BeepTextBox beepTextBox)
            {
                beepTextBox.Text = textValue;
                return;
            }

            if (editor is BeepComboBox beepComboBox)
            {
                beepComboBox.SelectedValue = value == DBNull.Value ? null : value;

                return;
            }

            if (editor is BeepCheckBoxBool beepCheckBox)
            {
                beepCheckBox.CurrentValue = TryConvertToBoolean(value, textValue);
                return;
            }

            if (editor is BeepDatePicker beepDatePicker)
            {
                if (value is DateTime dateTime)
                {
                    beepDatePicker.SelectedDateTime = dateTime;
                    return;
                }

                if (DateTime.TryParse(textValue, out var parsedDate))
                {
                    beepDatePicker.SelectedDateTime = parsedDate;
                    return;
                }

                beepDatePicker.SelectedDate = textValue;
                return;
            }

            if (editor is BeepNumericUpDown beepNumericUpDown)
            {
                if (TryConvertToDecimal(value, out var numericValue))
                {
                    beepNumericUpDown.Value = numericValue;
                }

                return;
            }

            if (editor is TextBoxBase textBox)
            {
                textBox.Text = textValue;
                return;
            }

            if (editor is ComboBox comboBox)
            {
                comboBox.Text = textValue;
                return;
            }

            if (editor is CheckBox checkBox)
            {
                if (value is bool booleanValue)
                {
                    checkBox.Checked = booleanValue;
                    return;
                }

                if (bool.TryParse(textValue, out bool parsedValue))
                {
                    checkBox.Checked = parsedValue;
                    return;
                }
            }

            editor.Text = textValue;
        }

        private void ConfigureComboEditor(BeepComboBox comboBox, Models.BeepFieldDefinition fieldDefinition)
        {
            if (comboBox == null || fieldDefinition == null)
            {
                return;
            }

            if (fieldDefinition.Options != null && fieldDefinition.Options.Count > 0)
            {
                comboBox.AllowFreeText = false;
                comboBox.IsEditable = false;
                comboBox.ListItems = BuildOptionItems(fieldDefinition.Options);
                return;
            }

            if (_formsHost != null && !string.IsNullOrWhiteSpace(ManagerBlockName) && !string.IsNullOrWhiteSpace(fieldDefinition.FieldName)
                && _formsHost.HasLov(ManagerBlockName, fieldDefinition.FieldName))
            {
                AttachComboValidationHandler(comboBox);

                var lov = _formsHost.GetLov(ManagerBlockName, fieldDefinition.FieldName);
                if (lov != null)
                {
                    comboBox.AllowFreeText = lov.ValidationType != LOVValidationType.ListOnly;
                    comboBox.IsEditable = comboBox.AllowFreeText;
                }

                LoadLovItemsAsync(comboBox, fieldDefinition.FieldName);
                return;
            }

            if (string.IsNullOrWhiteSpace(fieldDefinition.FieldName))
            {
                return;
            }

            Type? fieldType = ResolveFieldDataType(fieldDefinition.FieldName);
            Type? enumType = Nullable.GetUnderlyingType(fieldType ?? typeof(object)) ?? fieldType;
            if (enumType?.IsEnum == true)
            {
                comboBox.AllowFreeText = false;
                comboBox.IsEditable = false;
                comboBox.ListItems = BuildEnumItems(enumType);
            }
        }

        private static BindingList<SimpleItem> BuildOptionItems(IEnumerable<Models.BeepFieldOptionDefinition> options)
        {
            var items = new BindingList<SimpleItem>();
            if (options == null)
            {
                return items;
            }

            foreach (var option in options)
            {
                if (option == null)
                {
                    continue;
                }

                string text = string.IsNullOrWhiteSpace(option.Text)
                    ? Convert.ToString(option.Value) ?? option.Name ?? string.Empty
                    : option.Text;

                if (string.IsNullOrWhiteSpace(text) && option.Value == null)
                {
                    continue;
                }

                items.Add(new SimpleItem
                {
                    Text = text,
                    Name = string.IsNullOrWhiteSpace(option.Name) ? text : option.Name,
                    Description = string.IsNullOrWhiteSpace(option.Description) ? text : option.Description,
                    Value = option.Value,
                    Item = option.Value ?? text
                });
            }

            return items;
        }

        private async void LoadLovItemsAsync(BeepComboBox comboBox, string fieldName)
        {
            string blockName = ManagerBlockName;
            if (comboBox.IsDisposed || string.IsNullOrWhiteSpace(blockName) || string.IsNullOrWhiteSpace(fieldName) || _formsHost == null)
            {
                return;
            }

            var hostSnapshot = _formsHost;
            try
            {
                var lov = hostSnapshot.GetLov(blockName, fieldName);
                if (lov == null)
                {
                    return;
                }

                var result = await hostSnapshot.LoadLovDataAsync(blockName, fieldName).ConfigureAwait(true);
                if (!result.Success || comboBox.IsDisposed || !ReferenceEquals(_formsHost, hostSnapshot)
                    || !string.Equals(blockName, ManagerBlockName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                comboBox.ListItems = BuildLovItems(lov, result.Records);
            }
            catch
            {
            }
        }

        private static BindingList<SimpleItem> BuildEnumItems(Type enumType)
        {
            var items = new BindingList<SimpleItem>();
            foreach (var enumValue in Enum.GetValues(enumType))
            {
                string text = enumValue?.ToString() ?? string.Empty;
                items.Add(new SimpleItem
                {
                    Text = text,
                    Name = text,
                    Value = enumValue,
                    Item = enumValue
                });
            }

            return items;
        }

        private static BindingList<SimpleItem> BuildLovItems(LOVDefinition lov, IEnumerable<object> records)
        {
            var items = new BindingList<SimpleItem>();
            if (lov == null || records == null)
            {
                return items;
            }

            string displayField = string.IsNullOrWhiteSpace(lov.DisplayField) ? lov.ReturnField ?? string.Empty : lov.DisplayField;
            string returnField = string.IsNullOrWhiteSpace(lov.ReturnField) ? displayField : lov.ReturnField;
            var supplementalFields = ResolveLovSupplementalFields(lov, displayField, returnField);

            foreach (var record in records)
            {
                if (record == null)
                {
                    continue;
                }

                object? returnValue = string.IsNullOrWhiteSpace(returnField) ? record : GetRecordValue(record, returnField);
                string text = string.IsNullOrWhiteSpace(displayField)
                    ? Convert.ToString(returnValue) ?? string.Empty
                    : Convert.ToString(GetRecordValue(record, displayField)) ?? string.Empty;

                if (string.IsNullOrWhiteSpace(text) && returnValue == null)
                {
                    continue;
                }

                var item = new SimpleItem
                {
                    Text = text,
                    Name = Convert.ToString(returnValue) ?? text,
                    Description = text,
                    DisplayField = displayField,
                    ValueField = returnField,
                    Value = returnValue,
                    Item = record
                };

                if (supplementalFields.Count > 0)
                {
                    item.SubText = Convert.ToString(GetRecordValue(record, supplementalFields[0])) ?? string.Empty;
                }

                if (supplementalFields.Count > 1)
                {
                    item.SubText2 = Convert.ToString(GetRecordValue(record, supplementalFields[1])) ?? string.Empty;
                }

                if (supplementalFields.Count > 2)
                {
                    item.SubText3 = Convert.ToString(GetRecordValue(record, supplementalFields[2])) ?? string.Empty;
                }

                items.Add(item);
            }

            return items;
        }

        private void ApplyLovRelatedFieldMappings(string fieldName, SimpleItem? selectedItem)
        {
            if (_isApplyingLovRelatedFieldMappings || selectedItem?.Item == null || string.IsNullOrWhiteSpace(ManagerBlockName) || _formsHost == null)
            {
                return;
            }

            object? currentRecord = GetCurrentRecord();
            if (currentRecord == null)
            {
                return;
            }

            var lov = _formsHost.GetLov(ManagerBlockName, fieldName);
            if (lov == null)
            {
                return;
            }

            var relatedValues = _formsHost.GetLovRelatedFieldValues(lov, selectedItem.Item);
            if (relatedValues == null || relatedValues.Count == 0)
            {
                return;
            }

            bool appliedAny = false;
            _isApplyingLovRelatedFieldMappings = true;
            try
            {
                foreach (var kv in relatedValues)
                {
                    string targetField = string.Equals(kv.Key, "__RETURN_VALUE__", StringComparison.Ordinal)
                        ? fieldName
                        : kv.Key;

                    if (string.IsNullOrWhiteSpace(targetField))
                    {
                        continue;
                    }

                    appliedAny |= SetRecordValue(currentRecord, targetField, kv.Value);
                }

                if (!appliedAny)
                {
                    return;
                }

                if (_recordBindingSource?.DataSource != null)
                {
                    _recordBindingSource.ResetBindings(false);
                    UpdateRecordViewState(_boundUnitOfWork);
                }
                else
                {
                    ApplyCurrentRecordToEditors();
                }

                RefreshValidationState();
            }
            finally
            {
                _isApplyingLovRelatedFieldMappings = false;
            }
        }

        private void AttachComboValidationHandler(BeepComboBox comboBox)
        {
            comboBox.SelectedItemChanged -= ComboBox_SelectedItemChanged;
            comboBox.SelectedItemChanged += ComboBox_SelectedItemChanged;
        }

        private async void ComboBox_SelectedItemChanged(object? sender, SelectedItemChangedEventArgs e)
        {
            if (_isApplyingLovRelatedFieldMappings || sender is not BeepComboBox comboBox || _formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName))
            {
                return;
            }

            string fieldName = comboBox.DataSourceProperty;
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                return;
            }

            if (!_formsHost.HasLov(ManagerBlockName, fieldName))
            {
                ClearFieldError(fieldName);
                return;
            }

            var hostSnapshot = _formsHost;
            try
            {
                ApplyLovRelatedFieldMappings(fieldName, e.SelectedItem);

                var result = await hostSnapshot.ValidateLovValueAsync(ManagerBlockName, fieldName, comboBox.SelectedValue).ConfigureAwait(true);
                if (comboBox.IsDisposed || !ReferenceEquals(_formsHost, hostSnapshot))
                {
                    return;
                }

                if (result?.IsValid != false)
                {
                    ClearFieldError(fieldName);
                }
                else
                {
                    SetFieldError(fieldName, result.ErrorMessage);
                }
            }
            catch
            {
            }
        }

        private static bool TryConvertToDecimal(object? value, out decimal result)
        {
            result = 0m;

            if (value == null || value == DBNull.Value)
            {
                return false;
            }

            switch (value)
            {
                case decimal decimalValue:
                    result = decimalValue;
                    return true;
                case byte byteValue:
                    result = byteValue;
                    return true;
                case short shortValue:
                    result = shortValue;
                    return true;
                case int intValue:
                    result = intValue;
                    return true;
                case long longValue:
                    result = longValue;
                    return true;
                case float floatValue:
                    result = (decimal)floatValue;
                    return true;
                case double doubleValue:
                    result = (decimal)doubleValue;
                    return true;
                case string stringValue:
                    return decimal.TryParse(stringValue, out result);
                default:
                    return decimal.TryParse(value.ToString(), out result);
            }
        }

        private static bool TryConvertToBoolean(object? value, string textValue)
        {
            if (value is bool booleanValue)
            {
                return booleanValue;
            }

            if (value is byte byteValue)
            {
                return byteValue != 0;
            }

            if (value is short shortValue)
            {
                return shortValue != 0;
            }

            if (value is int intValue)
            {
                return intValue != 0;
            }

            if (bool.TryParse(textValue, out bool parsedBool))
            {
                return parsedBool;
            }

            if (string.Equals(textValue, "Y", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(textValue, "YES", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(textValue, "1", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static bool TryConvertToDateTime(object? value, string textValue, out DateTime result)
        {
            if (value is DateTime dateTimeValue)
            {
                result = dateTimeValue;
                return true;
            }

            return DateTime.TryParse(textValue, out result);
        }

        private static Type ResolveClrType(EntityField field)
        {
            if (!string.IsNullOrWhiteSpace(field.Fieldtype))
            {
                Type? reflectedType = Type.GetType(field.Fieldtype, false, true);
                if (reflectedType != null)
                {
                    return reflectedType;
                }

                switch (field.Fieldtype.Trim().ToLowerInvariant())
                {
                    case "string":
                    case "nvarchar":
                    case "varchar":
                    case "char":
                    case "text":
                        return typeof(string);
                    case "bool":
                    case "boolean":
                    case "bit":
                        return typeof(bool);
                    case "byte":
                    case "tinyint":
                        return typeof(byte);
                    case "short":
                    case "int16":
                    case "smallint":
                        return typeof(short);
                    case "int":
                    case "int32":
                    case "integer":
                        return typeof(int);
                    case "long":
                    case "int64":
                    case "bigint":
                        return typeof(long);
                    case "float":
                    case "single":
                        return typeof(float);
                    case "double":
                        return typeof(double);
                    case "decimal":
                    case "numeric":
                    case "money":
                        return typeof(decimal);
                    case "datetime":
                    case "datetime2":
                    case "date":
                    case "time":
                        return typeof(DateTime);
                }
            }

            return field.FieldCategory switch
            {
                DbFieldCategory.Boolean => typeof(bool),
                DbFieldCategory.Date => typeof(DateTime),
                DbFieldCategory.DateTime => typeof(DateTime),
                DbFieldCategory.Integer => typeof(int),
                DbFieldCategory.Numeric => typeof(decimal),
                DbFieldCategory.Decimal => typeof(decimal),
                DbFieldCategory.Double => typeof(double),
                DbFieldCategory.Float => typeof(float),
                DbFieldCategory.Currency => typeof(decimal),
                _ => typeof(string)
            };
        }
    }
}