using System;
using System.ComponentModel;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls.Base;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Helpers;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms;

public abstract class BeepFormsShelfBase : BaseControl
{
    protected BeepForms? _formsHost;
    private bool _autoBindFormsHost = true;

    [Browsable(true), Category("Behavior")]
    [Description("Optional BeepForms coordinator surfaced by this shelf.")]
    [DefaultValue(null)]
    public BeepForms? FormsHost
    {
        get => _formsHost;
        set
        {
            if (ReferenceEquals(_formsHost, value)) return;
            DetachFormsHost(_formsHost);
            _formsHost = value;
            AttachFormsHost(_formsHost);
            OnFormsHostChanged();
        }
    }

    [Browsable(true), Category("Behavior")]
    [Description("Automatically resolve a nearby BeepForms host when FormsHost is not set explicitly.")]
    [DefaultValue(true)]
    public bool AutoBindFormsHost
    {
        get => _autoBindFormsHost;
        set
        {
            if (_autoBindFormsHost == value) return;
            _autoBindFormsHost = value;
            if (_autoBindFormsHost && _formsHost == null)
                TryBindFormsHostFromHierarchy();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) DetachFormsHost(_formsHost);
        base.Dispose(disposing);
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        TryBindFormsHostFromHierarchy();
    }

    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);
        TryBindFormsHostFromHierarchy();
    }

    private void AttachFormsHost(BeepForms? f)
    {
        if (f == null) return;
        f.ActiveBlockChanged += OnStateChanged;
        f.FormsManagerChanged += OnStateChanged;
        f.ViewStateChanged += OnStateChanged;
        f.Disposed += OnHostDisposed;
    }

    private void DetachFormsHost(BeepForms? f)
    {
        if (f == null) return;
        f.ActiveBlockChanged -= OnStateChanged;
        f.FormsManagerChanged -= OnStateChanged;
        f.ViewStateChanged -= OnStateChanged;
        f.Disposed -= OnHostDisposed;
    }

    private void OnStateChanged(object? s, EventArgs e) => OnFormsHostChanged();

    private void OnHostDisposed(object? s, EventArgs e)
    {
        BeepFormsHostResolver.Invalidate(this);
        FormsHost = null;
        TryBindFormsHostFromHierarchy();
    }

    private void TryBindFormsHostFromHierarchy()
    {
        if (!AutoBindFormsHost || _formsHost != null || Parent == null) return;
        FormsHost = BeepFormsHostResolver.Find(this);
    }

    protected virtual void OnFormsHostChanged() { }
}
