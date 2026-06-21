using System.Windows.Forms;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Vis.Modules;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FieldHost;

public abstract class WinFormFieldPresenterBase : IFieldPresenter, IDisposable
{
    private bool _synchronizing;
    private bool _disposed;
    private string _label;
    private string? _validationError;
    private string? _prompt;

    protected WinFormFieldPresenterBase(EntityField field, IBeepUIComponent editor)
    {
        Field = field ?? throw new ArgumentNullException(nameof(field));
        Editor = editor ?? throw new ArgumentNullException(nameof(editor));
        if (editor is not System.Windows.Forms.Control)
            throw new ArgumentException("A WinForms presenter requires a Control.", nameof(editor));

        _label = string.IsNullOrWhiteSpace(field.Caption) ? field.FieldName : field.Caption;
        Editor.FieldID = field.FieldName;
        Editor.ComponentName = field.FieldName;
        Editor.IsRequired = field.IsRequired;
        Editor.IsReadOnly = field.IsReadOnly || field.IsIdentity;
        Editor.IsEditable = !Editor.IsReadOnly;
        Editor.IsVisible = !field.IsHidden;
        Editor.OnValueChanged += EditorOnValueChanged;
    }

    protected EntityField Field { get; }
    protected IBeepUIComponent Editor { get; }
    protected Control Control => (Control)Editor;

    public string FieldName => Field.FieldName;
    public string FieldType => Field.Fieldtype;
    public virtual string Label { get => _label; set => _label = value ?? string.Empty; }
    public object? Value
    {
        get => Editor.GetValue();
        set
        {
            _synchronizing = true;
            try { Editor.SetValue(value!); }
            finally { _synchronizing = false; }
            ValueChanged?.Invoke(this, Editor.GetValue());
        }
    }
    public bool IsReadOnly
    {
        get => Editor.IsReadOnly;
        set { Editor.IsReadOnly = value; Editor.IsEditable = !value; }
    }
    public bool IsRequired { get => Editor.IsRequired; set => Editor.IsRequired = value; }
    public bool IsVisible
    {
        get => Control.Visible;
        set { Editor.IsVisible = value; Control.Visible = value; }
    }
    public bool IsEnabled { get => Control.Enabled; set => Control.Enabled = value; }
    public string? ValidationError
    {
        get => _validationError;
        set { _validationError = value; Editor.ToolTipText = value ?? _prompt ?? string.Empty; }
    }
    public string? Prompt
    {
        get => _prompt;
        set { _prompt = value; if (string.IsNullOrWhiteSpace(_validationError)) Editor.ToolTipText = value ?? string.Empty; }
    }
    public object? QueryValue { get; set; }
    public QueryOperator QueryOperator { get; set; } = QueryOperator.Equals;
    public bool IsQueryEnabled { get; set; } = true;

    public object View => Control;
    public abstract string Key { get; }
    public event EventHandler<object?>? ValueChanged;

    public void SetValue(object? value)
    {
        _synchronizing = true;
        try { Editor.SetValue(value!); }
        finally { _synchronizing = false; }
    }

    public void Clear()
    {
        _synchronizing = true;
        try { Editor.ClearValue(); }
        finally { _synchronizing = false; }
    }

    public bool Validate()
    {
        var valid = Editor.ValidateData(out var message);
        ValidationError = valid ? null : message;
        return valid;
    }

    public abstract bool CanPresent(object fieldDefinition);
    public abstract object CreateEditor(object fieldDefinition);

    public virtual void ApplyMetadata(object editor, object fieldDefinition)
    {
        if (editor is not IBeepUIComponent component || fieldDefinition is not EntityField field)
            throw new ArgumentException("Expected an IBeepUIComponent and EntityField.");
        component.FieldID = field.FieldName;
        component.IsRequired = field.IsRequired;
        component.IsReadOnly = field.IsReadOnly || field.IsIdentity;
        component.IsEditable = !component.IsReadOnly;
        component.IsVisible = !field.IsHidden;
        component.ToolTipText = field.Description ?? string.Empty;
    }

    private void EditorOnValueChanged(object? sender, BeepComponentEventArgs e)
    {
        if (!_synchronizing)
            ValueChanged?.Invoke(this, Editor.GetValue());
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Editor.OnValueChanged -= EditorOnValueChanged;
        Control.Dispose();
        GC.SuppressFinalize(this);
    }
}
