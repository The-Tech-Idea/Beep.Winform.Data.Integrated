using System.Windows.Forms;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls.Badges;
using TheTechIdea.Beep.Winform.Controls.Badges.Builtin;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FieldHost;

public abstract class WinFormFieldPresenterBase : IFieldPresenter, IDisposable
{
    private bool _synchronizing;
    private bool _disposed;
    private string _label;
    private string? _validationError;
    private string? _prompt;
    private BeepValidationBadge? _validationBadge;

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
        Control.ParentChanged += ControlOnParentChanged;
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
        set
        {
            _validationError = value;
            Editor.ToolTipText = value ?? _prompt ?? string.Empty;
            UpdateValidationBadge();
        }
    }

    /// <summary>
    /// True while this field is carrying a visible error marker. Exposed so a host
    /// (and the integration harness) can assert the marker without reaching into
    /// the badge manager.
    /// </summary>
    public bool HasValidationMarker => _validationBadge is not null;

    /// <summary>
    /// Shows or clears the field's error marker, using the control library's
    /// <see cref="BeepValidationBadge"/> rather than a hand-drawn adornment.
    /// </summary>
    /// <remarks>
    /// A badge attaches to the target's <em>parent</em>, so it cannot be applied
    /// before the editor is laid out. When the error is set first — which is the
    /// normal order, since the block host validates as it builds — the badge is
    /// applied when the parent arrives.
    /// </remarks>
    private void UpdateValidationBadge()
    {
        if (_disposed) return;

        if (string.IsNullOrWhiteSpace(_validationError))
        {
            if (_validationBadge is not null)
            {
                Control.ClearBadge();
                _validationBadge = null;
            }
            return;
        }

        if (Control.Parent is null) return;

        _validationBadge = new BeepValidationBadge(ValidationState.Error)
            .SetMessage(_validationError!);
        Control.ShowBadge(_validationBadge);
    }

    // The border colour last applied by a visual attribute, so the effect can be
    // asserted without reaching into the control's theme state. null = no VA.
    private System.Drawing.Color? _visualAttributeBorderColor;

    /// <summary>The border colour a visual attribute set on this field, or null.</summary>
    public System.Drawing.Color? VisualAttributeBorderColor => _visualAttributeBorderColor;

    /// <summary>
    /// Applies a resolved <see cref="VisualAttribute"/> to this field's control:
    /// border through the Beep component surface (<c>IBeepUIComponent.BorderColor</c>),
    /// foreground / background / font through the control. Passing null clears the
    /// attribute back to the control's theme default.
    /// </summary>
    public void ApplyVisualAttribute(VisualAttribute visualAttribute)
    {
        if (_disposed) return;

        if (visualAttribute is null)
        {
            _visualAttributeBorderColor = null;
            return;
        }

        if (TryParseHex(visualAttribute.BorderColorHex, out var border))
        {
            Editor.BorderColor = border;
            _visualAttributeBorderColor = border;
        }
        if (TryParseHex(visualAttribute.ForeColorHex, out var fore))
            Control.ForeColor = fore;
        if (TryParseHex(visualAttribute.BackColorHex, out var back))
            Control.BackColor = back;

        if (!string.IsNullOrWhiteSpace(visualAttribute.FontFamily) || visualAttribute.FontSize.HasValue)
        {
            var family = string.IsNullOrWhiteSpace(visualAttribute.FontFamily)
                ? Control.Font.FontFamily.Name : visualAttribute.FontFamily;
            var size = visualAttribute.FontSize ?? Control.Font.Size;
            var style = (System.Drawing.FontStyle)(visualAttribute.FontStyleMask ?? (int)Control.Font.Style);
            try { Control.Font = new System.Drawing.Font(family, size, style); }
            catch (Exception) { /* an unknown family or bad size falls back to the current font */ }
        }

        Control.Invalidate();
    }

    private static bool TryParseHex(string hex, out System.Drawing.Color color)
    {
        color = default;
        if (string.IsNullOrWhiteSpace(hex)) return false;
        try { color = System.Drawing.ColorTranslator.FromHtml(hex.Trim()); return true; }
        catch (Exception) { return false; }
    }

    private void ControlOnParentChanged(object? sender, EventArgs e) => UpdateValidationBadge();
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
        Control.ParentChanged -= ControlOnParentChanged;
        if (_validationBadge is not null)
        {
            Control.ClearBadge();
            _validationBadge = null;
        }
        Control.Dispose();
        GC.SuppressFinalize(this);
    }
}
