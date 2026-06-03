using System;
using System.Drawing;
using System.Windows.Forms;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks
{
    public partial class BeepBlock
    {
        private ContextMenuStrip? _fieldContextMenu;

        private void EnsureFieldContextMenu()
        {
            if (_fieldContextMenu != null)
                return;

            _fieldContextMenu = new ContextMenuStrip();
            _fieldContextMenu.Items.Add("Copy Value", null, (_, _) => CopyCurrentFieldValue());
            _fieldContextMenu.Items.Add("Paste Value", null, (_, _) => PasteCurrentFieldValue());
            _fieldContextMenu.Items.Add("-");
            _fieldContextMenu.Items.Add("Clear Field", null, (_, _) => ClearCurrentFieldValue());
            _fieldContextMenu.Items.Add("-");
            _fieldContextMenu.Items.Add("Copy All as CSV", null, (_, _) => CopyAllFieldsAsCsv());
        }

        private string? _contextMenuFieldName;

        internal void AttachFieldContextMenu(Control editor, string fieldName)
        {
            EnsureFieldContextMenu();
            editor.MouseDown += (_, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    _contextMenuFieldName = fieldName;
                    _fieldContextMenu?.Show(editor, e.Location);
                }
            };
        }

        private void CopyCurrentFieldValue()
        {
            if (string.IsNullOrWhiteSpace(_contextMenuFieldName))
                return;

            var value = GetFieldValue(_contextMenuFieldName);
            try
            {
                Clipboard.SetText(value ?? string.Empty);
            }
            catch { }
        }

        private void PasteCurrentFieldValue()
        {
            if (string.IsNullOrWhiteSpace(_contextMenuFieldName))
                return;

            try
            {
                if (!Clipboard.ContainsText())
                    return;

                var text = Clipboard.GetText();
                SetFieldValue(_contextMenuFieldName, text);
                ApplyCurrentRecordToEditors();
                MarkAsDirty();
            }
            catch { }
        }

        private void ClearCurrentFieldValue()
        {
            if (string.IsNullOrWhiteSpace(_contextMenuFieldName))
                return;

            SetFieldValue(_contextMenuFieldName, null);
            ApplyCurrentRecordToEditors();
            MarkAsDirty();
        }

        private void CopyAllFieldsAsCsv()
        {
            var csv = ExportToCsvString();
            if (string.IsNullOrWhiteSpace(csv))
                return;

            try
            {
                Clipboard.SetText(csv);
            }
            catch { }
        }

        private string? GetFieldValue(string fieldName)
        {
            if (_recordBindingSource?.Current == null)
                return null;

            var current = _recordBindingSource.Current;
            try
            {
                var value = current.GetType().GetProperty(fieldName)?.GetValue(current);
                return value?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private void SetFieldValue(string fieldName, object? value)
        {
            var current = GetCurrentRecord();
            if (current == null)
                return;

            try
            {
                var prop = current.GetType().GetProperty(fieldName);
                if (prop?.CanWrite == true)
                {
                    prop.SetValue(current, ConvertFieldValue(prop.PropertyType, value));
                }
            }
            catch { }
        }

        private void MarkAsDirty()
        {
            if (_formsHost == null || string.IsNullOrWhiteSpace(ManagerBlockName))
                return;

            _viewState.IsDirty = true;
            UpdateSummaryText();
            NotifyViewStateChanged();
        }

        private static object? ConvertFieldValue(Type targetType, object? value)
        {
            if (value == null || value == DBNull.Value)
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

            if (targetType.IsAssignableFrom(value.GetType()))
                return value;

            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                return value;
            }
        }
    }
}
