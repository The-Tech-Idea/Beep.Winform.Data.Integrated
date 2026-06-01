using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks
{
    /// <summary>
    /// Runtime field-to-control binding registry for BeepBlock.
    ///
    /// This is Side A of the Phase 6 "BeepBlock as Visual Studio dataset designer" contract.
    /// Designer.cs calls <see cref="BindControl"/> for every generated field control during
    /// InitializeComponent.  At runtime this registry drives <see cref="RefreshAllFieldControls"/>
    /// so every bound control stays current with the active UoW record without BeepBlock needing
    /// to own the controls itself.
    ///
    /// Architecture rule: all FormsManager / UoW data access goes through _formsHost proxies —
    /// never via FormsManager directly from BeepBlock.
    /// </summary>
    public partial class BeepBlock
    {
        // field name (case-insensitive) → bound control
        private readonly Dictionary<string, Control> _fieldControlMap =
            new(StringComparer.OrdinalIgnoreCase);

        // field name → binding property name used when writing the DataBinding (e.g. "Text", "Value")
        private readonly Dictionary<string, string> _fieldBindingPropertyMap =
            new(StringComparer.OrdinalIgnoreCase);

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Register <paramref name="control"/> as the display/edit surface for <paramref name="fieldName"/>.
        /// Called from generated Designer.cs InitializeComponent code.
        /// If a different control was already registered for this field it is first unbound.
        /// </summary>
        public void BindControl(string fieldName, Control control)
        {
            RegisterBoundControl(fieldName, control, null, hasBindingPropertyOverride: false);
        }

        /// <summary>
        /// Register <paramref name="control"/> and explicitly specify which property carries the value.
        /// Use this overload when the binding property differs from the registry default
        /// (e.g. a custom control that exposes the value through <c>"SelectedItem"</c>).
        /// </summary>
        public void BindControl(string fieldName, Control control, string bindingProperty)
        {
            RegisterBoundControl(fieldName, control, bindingProperty, hasBindingPropertyOverride: true);
        }

        private void RegisterBoundControl(string fieldName, Control control, string? bindingProperty, bool hasBindingPropertyOverride)
        {
            if (string.IsNullOrWhiteSpace(fieldName) || control == null)
            {
                return;
            }

            if (_fieldControlMap.TryGetValue(fieldName, out var existing))
            {
                ClearDataBindingsForField(existing, fieldName);
                _fieldControlMap.Remove(fieldName);
            }

            if (hasBindingPropertyOverride)
            {
                if (string.IsNullOrWhiteSpace(bindingProperty))
                {
                    _fieldBindingPropertyMap.Remove(fieldName);
                }
                else
                {
                    _fieldBindingPropertyMap[fieldName] = bindingProperty;
                }
            }
            else
            {
                _fieldBindingPropertyMap.Remove(fieldName);
            }

            _fieldControlMap[fieldName] = control;

            if (_recordBindingSource?.Current != null)
            {
                RefreshFieldControl(fieldName);
            }
        }

        /// <summary>
        /// Remove the runtime binding for <paramref name="fieldName"/>.
        /// Clears any WinForms DataBindings added by this class on the previously bound control.
        /// Does NOT destroy or dispose the control itself.
        /// </summary>
        public void UnbindControl(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                return;

            if (_fieldControlMap.TryGetValue(fieldName, out var existing))
            {
                ClearDataBindingsForField(existing, fieldName);
                _fieldControlMap.Remove(fieldName);
            }

            _fieldBindingPropertyMap.Remove(fieldName);
        }

        /// <summary>
        /// Remove all registered field-to-control bindings.
        /// Does NOT destroy or dispose any controls.
        /// </summary>
        public void UnbindAllControls()
        {
            foreach (var kvp in _fieldControlMap)
                ClearDataBindingsForField(kvp.Value, kvp.Key);

            _fieldControlMap.Clear();
            _fieldBindingPropertyMap.Clear();
        }

        /// <summary>
        /// Replace the control bound to <paramref name="fieldName"/> with <paramref name="newControl"/>.
        /// Equivalent to calling <see cref="UnbindControl"/> then <see cref="BindControl"/>.
        /// </summary>
        public void RebindControl(string fieldName, Control newControl)
        {
            UnbindControl(fieldName);
            BindControl(fieldName, newControl);
        }

        /// <summary>
        /// Returns the control currently bound to <paramref name="fieldName"/>, or <c>null</c> if none.
        /// </summary>
        public Control? GetBoundControl(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                return null;

            _fieldControlMap.TryGetValue(fieldName, out var ctl);
            return ctl;
        }

        /// <summary>
        /// Returns a snapshot of all current field → control pairings.
        /// </summary>
        public IReadOnlyDictionary<string, Control> GetAllBindings() =>
            new Dictionary<string, Control>(_fieldControlMap, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Push the current record value for <paramref name="fieldName"/> into its bound control.
        /// No-op if no control is bound or there is no current record.
        /// </summary>
        public void RefreshFieldControl(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                return;

            if (!_fieldControlMap.TryGetValue(fieldName, out var control) || control == null)
                return;

            object? current = _recordBindingSource?.Current;
            if (current == null)
                return;

            string bindProp = GetEffectiveBindingProperty(fieldName, control);
            object? value = GetFieldValueFromRecord(current, fieldName);

            SetControlValue(control, bindProp, value);
        }

        /// <summary>
        /// Push the current record values for ALL registered field controls.
        /// Called after a record navigation or after data reload.
        /// </summary>
        public void RefreshAllFieldControls()
        {
            if (_fieldControlMap.Count == 0)
                return;

            object? current = _recordBindingSource?.Current;
            if (current == null)
                return;

            foreach (var kvp in _fieldControlMap)
            {
                if (kvp.Value == null)
                    continue;

                string bindProp = GetEffectiveBindingProperty(kvp.Key, kvp.Value);
                object? value = GetFieldValueFromRecord(current, kvp.Key);
                SetControlValue(kvp.Value, bindProp, value);
            }
        }

        // ── Internal helpers ────────────────────────────────────────────────────────

        /// <summary>
        /// Called from SyncFromManager / RefreshLayout after navigation so registered controls
        /// stay current whenever the block's internal UI refreshes.
        /// </summary>
        private void RefreshGeneratedFieldControls()
        {
            if (_fieldControlMap.Count > 0)
                RefreshAllFieldControls();
        }

        private string GetEffectiveBindingProperty(string fieldName, Control control)
        {
            if (_fieldBindingPropertyMap.TryGetValue(fieldName, out var explicit_))
                return explicit_;

            // Ask the registry for the default binding property for this control type.
            string controlTypeName = control.GetType().Name;
            return Services.BeepFieldControlTypeRegistry.ResolveDefaultBindingProperty(controlTypeName);
        }

        private static object? GetFieldValueFromRecord(object record, string fieldName)
        {
            if (record == null || string.IsNullOrWhiteSpace(fieldName))
                return null;

            // Try reflection — works for both strongly-typed POCO and dynamic/expando records.
            var prop = record.GetType().GetProperty(fieldName,
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.IgnoreCase);

            if (prop != null)
                return prop.GetValue(record);

            // Fallback: IDictionary<string,object> (expando-style records)
            if (record is System.Collections.IDictionary dict &&
                dict.Contains(fieldName))
                return dict[fieldName];

            return null;
        }

        private static void SetControlValue(Control control, string bindingProperty, object? value)
        {
            if (control == null || string.IsNullOrWhiteSpace(bindingProperty))
                return;

            try
            {
                var prop = control.GetType().GetProperty(bindingProperty,
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (prop == null || !prop.CanWrite)
                    return;

                // Coerce DBNull → null
                object? coerced = value is DBNull ? null : value;

                if (coerced == null)
                {
                    // Set to default for value types
                    prop.SetValue(control, prop.PropertyType.IsValueType
                        ? Activator.CreateInstance(prop.PropertyType)
                        : null);
                }
                else
                {
                    // Simple direct assignment where types match; convert if they differ.
                    if (prop.PropertyType.IsAssignableFrom(coerced.GetType()))
                        prop.SetValue(control, coerced);
                    else
                    {
                        try
                        {
                            prop.SetValue(control, Convert.ChangeType(coerced, prop.PropertyType));
                        }
                        catch
                        {
                            prop.SetValue(control, coerced.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[BeepBlock.ControlGeneration] SetControlValue failed for field binding: {ex.Message}");
            }
        }

        private static void ClearDataBindingsForField(Control control, string fieldName)
        {
            if (control == null)
                return;

            // Remove any Binding whose DataMember targets this field.
            for (int i = control.DataBindings.Count - 1; i >= 0; i--)
            {
                var b = control.DataBindings[i];
                if (string.Equals(b.BindingMemberInfo.BindingField, fieldName, StringComparison.OrdinalIgnoreCase))
                    control.DataBindings.RemoveAt(i);
            }
        }

        private void ReconcileDesignerGeneratedBindings(BeepBlockDefinition? definition)
        {
            if (definition?.PresentationMode != BeepBlockPresentationMode.DesignerGenerated)
                return;

            HashSet<string> activeFields = GetDesignerGeneratedFieldNames(definition);

            foreach (string fieldName in _fieldControlMap.Keys.ToArray())
            {
                if (activeFields.Contains(fieldName))
                    continue;

                System.Diagnostics.Debug.WriteLine($"[BeepBlock.ControlGeneration] Removing stale designer-generated binding for '{fieldName}'.");
                UnbindControl(fieldName);
            }

            foreach (string fieldName in _fieldBindingPropertyMap.Keys.ToArray())
            {
                if (!activeFields.Contains(fieldName))
                    _fieldBindingPropertyMap.Remove(fieldName);
            }
        }

        private static HashSet<string> GetDesignerGeneratedFieldNames(BeepBlockDefinition definition)
        {
            var fieldNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (definition.Fields != null)
            {
                foreach (var field in definition.Fields)
                {
                    if (!string.IsNullOrWhiteSpace(field?.FieldName))
                        fieldNames.Add(field.FieldName);
                }
            }

            if (fieldNames.Count > 0)
                return fieldNames;

            if (definition.Entity?.Fields != null)
            {
                foreach (var field in definition.Entity.Fields)
                {
                    if (!string.IsNullOrWhiteSpace(field?.FieldName))
                        fieldNames.Add(field.FieldName);
                }
            }

            return fieldNames;
        }
    }
}
