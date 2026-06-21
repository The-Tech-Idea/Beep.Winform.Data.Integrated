using System.Runtime.ExceptionServices;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public partial class WinFormFormHost
{
    private void RunOnUi(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (_lifecycleDisposed || IsDisposed || Disposing)
        {
            return;
        }

        if (Environment.CurrentManagedThreadId == _ownerThreadId)
        {
            action();
            return;
        }

        if (!IsHandleCreated)
        {
            throw new InvalidOperationException(
                "The WinForms host handle must be created on its owning thread before cross-thread access.");
        }

        ExceptionDispatchInfo? actionFailure = null;
        try
        {
            Invoke((MethodInvoker)(() =>
            {
                if (!_lifecycleDisposed && !IsDisposed && !Disposing)
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        actionFailure = ExceptionDispatchInfo.Capture(ex);
                    }
                }
            }));
        }
        catch (InvalidOperationException) when (
            _lifecycleDisposed || IsDisposed || Disposing || !IsHandleCreated)
        {
            // The control was disposed, or its handle was destroyed, while
            // the invocation was being marshalled.
        }
        catch (ObjectDisposedException)
        {
            // Disposal raced with the synchronous UI invocation.
            return;
        }

        actionFailure?.Throw();
    }
}
