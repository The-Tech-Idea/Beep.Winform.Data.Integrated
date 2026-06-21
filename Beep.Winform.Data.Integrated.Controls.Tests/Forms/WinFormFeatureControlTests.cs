using Moq;
using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;
using Xunit;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Tests.Forms;

public class WinFormFeatureControlTests
{
    [Fact]
    public Task LockPanel_DelegatesCurrentRecordCommands() => StaTest.RunAsync(async () =>
    {
        var host = new Mock<IBeepFormsHost>();
        host.Setup(x => x.LockCurrentRecordAsync(
                "ORDERS", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        host.Setup(x => x.UnlockCurrentRecord("ORDERS")).Returns(true);
        using var panel = new WinFormLockPanel(host.Object, "ORDERS");

        Assert.True(await panel.LockAsync());
        Assert.True(panel.Unlock());
    });

    [Fact]
    public Task SavepointPanel_DelegatesNamedSavepointCommands() => StaTest.RunAsync(async () =>
    {
        var host = new Mock<IBeepFormsHost>();
        host.Setup(x => x.CreateSavepoint("ORDERS", "review")).Returns("review");
        host.Setup(x => x.RollbackToSavepointAsync(
                "ORDERS", "review", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        using var panel = new WinFormSavepointPanel(host.Object, "ORDERS");

        Assert.Equal("review", panel.Create("review"));
        Assert.True(await panel.RollbackAsync("review"));
    });

    [Fact]
    public Task RuntimePanels_DelegateWithoutManagerDependency() => StaTest.RunAsync(() =>
    {
        var host = new Mock<IBeepFormsHost>();
        host.Setup(x => x.GetNextSequence("ORDER_SEQ")).Returns(5);
        using var sequencePanel = new WinFormSequencePanel(host.Object);

        Assert.Equal(5, sequencePanel.NextValue("ORDER_SEQ"));
        Assert.DoesNotContain(
            typeof(IBeepFormsHost).Assembly.GetReferencedAssemblies(),
            assembly => assembly.Name?.Contains("WinForms", StringComparison.OrdinalIgnoreCase) == true);
    });
}
