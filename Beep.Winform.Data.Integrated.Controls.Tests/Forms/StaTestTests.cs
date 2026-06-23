using Xunit;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Tests.Forms;

public class StaTestTests
{
    [Fact]
    public async Task RunAsync_ExecutesOnStaThread()
    {
        var state = await StaTest.RunAsync(
            () => Thread.CurrentThread.GetApartmentState());

        Assert.Equal(ApartmentState.STA, state);
    }

    [Fact]
    public async Task RunAsync_ActionOverloadExecutesAction()
    {
        var executed = false;

        await StaTest.RunAsync((Action)(() => executed = true));

        Assert.True(executed);
    }

    [Fact]
    public async Task RunAsync_PropagatesDelegateException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => StaTest.RunAsync<int>(
                () => throw new InvalidOperationException("STA failure")));

        Assert.Equal("STA failure", exception.Message);
    }

    [Fact]
    public async Task RunAsync_UsesBackgroundThread()
    {
        var isBackground = await StaTest.RunAsync(
            () => Thread.CurrentThread.IsBackground);

        Assert.True(isBackground);
    }

    [Fact]
    public async Task RunAsync_AwaitsAsynchronousDelegate()
    {
        var completed = false;

        await StaTest.RunAsync(async () =>
        {
            await Task.Delay(50);
            completed = true;
        });

        Assert.True(completed);
    }

    [Fact]
    public async Task RunAsync_TimesOutBlockedDelegate()
    {
        await Assert.ThrowsAsync<TimeoutException>(
            () => StaTest.RunAsync(
                () => Thread.Sleep(TimeSpan.FromSeconds(1)),
                TimeSpan.FromMilliseconds(50)));
    }
}
