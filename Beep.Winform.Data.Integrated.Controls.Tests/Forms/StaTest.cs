namespace TheTechIdea.Beep.Winform.Data.Integrated.Tests.Forms;

internal static class StaTest
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    public static Task<T> RunAsync<T>(Func<T> action) =>
        RunAsync(action, DefaultTimeout);

    /// <summary>
    /// Executes a delegate on a background STA thread and limits how long the
    /// returned task waits. A timeout cannot forcibly abort the delegate's
    /// thread; the background thread may continue until the delegate returns.
    /// </summary>
    public static Task<T> RunAsync<T>(Func<T> action, TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(action);

        var completion = new TaskCompletionSource<T>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try
            {
                completion.SetResult(action());
            }
            catch (Exception ex)
            {
                completion.SetException(ex);
            }
        })
        {
            IsBackground = true
        };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        return completion.Task.WaitAsync(timeout);
    }

    public static Task RunAsync(Action action) =>
        RunAsync(action, DefaultTimeout);

    public static Task RunAsync(Action action, TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(action);

        return RunAsync(() =>
        {
            action();
            return true;
        }, timeout);
    }
}
