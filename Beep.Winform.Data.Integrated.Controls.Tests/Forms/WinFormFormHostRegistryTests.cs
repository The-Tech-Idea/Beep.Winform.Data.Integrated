using Moq;
using System.Windows.Forms;
using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;
using Xunit;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Tests.Forms;

public class WinFormFormHostRegistryTests
{
    [Fact]
    public Task Host_IsAWinFormsControlAndFormsHost() => StaTest.RunAsync(() =>
    {
        using var host = new WinFormFormHost();

        Assert.IsAssignableFrom<UserControl>(host);
        Assert.IsAssignableFrom<IBeepFormsHost>(host);
    });

    [Fact]
    public Task RegisterBlock_EngineBlockExists_BindsAndActivates() => StaTest.RunAsync(() =>
    {
        var manager = new Mock<IUnitofWorksManager>();
        manager.Setup(m => m.BlockExists("EMP")).Returns(true);
        var block = CreateBlock("EMP");
        using var host = new WinFormFormHost { FormsManager = manager.Object };
        var activeBlockChanges = 0;
        host.ActiveBlockChanged += (_, _) => activeBlockChanges++;

        var result = host.RegisterBlock(block.Object);

        Assert.True(result);
        Assert.True(host.IsBlockRegistered("emp"));
        Assert.Equal("EMP", host.ActiveBlockName);
        Assert.Equal(1, activeBlockChanges);
        block.Verify(b => b.Bind(host), Times.Once);
    });

    [Fact]
    public Task RegisterBlock_WithoutManager_RegistersWithoutBinding() => StaTest.RunAsync(() =>
    {
        var block = CreateBlock("EMP");
        using var host = new WinFormFormHost();

        Assert.True(host.RegisterBlock(block.Object));

        Assert.True(host.IsBlockRegistered("EMP"));
        Assert.Equal("EMP", host.ActiveBlockName);
        block.Verify(b => b.Bind(It.IsAny<IBeepFormsHost>()), Times.Never);
    });

    [Fact]
    public Task RegisterBlock_ManagerDoesNotContainBlock_RegistersWithoutBinding() => StaTest.RunAsync(() =>
    {
        var manager = new Mock<IUnitofWorksManager>();
        manager.Setup(m => m.BlockExists("EMP")).Returns(false);
        var block = CreateBlock("EMP");
        using var host = new WinFormFormHost { FormsManager = manager.Object };

        Assert.True(host.RegisterBlock(block.Object));

        Assert.True(host.IsBlockRegistered("EMP"));
        block.Verify(b => b.Bind(It.IsAny<IBeepFormsHost>()), Times.Never);
    });

    [Fact]
    public Task RegisterBlock_DuplicateNameIgnoringCase_Throws() => StaTest.RunAsync(() =>
    {
        using var host = new WinFormFormHost();
        host.RegisterBlock(CreateBlock("EMP").Object);

        Assert.Throws<InvalidOperationException>(
            () => host.RegisterBlock(CreateBlock("emp").Object));
    });

    [Fact]
    public Task RegisterBlock_EmptyName_Throws() => StaTest.RunAsync(() =>
    {
        using var host = new WinFormFormHost();

        Assert.Throws<ArgumentException>(
            () => host.RegisterBlock(CreateBlock(" ").Object));
    });

    [Fact]
    public Task RegisterBlock_ViewIsNotControl_Throws() => StaTest.RunAsync(() =>
    {
        var block = CreateBlock("EMP", new object());
        using var host = new WinFormFormHost();

        Assert.Throws<ArgumentException>(() => host.RegisterBlock(block.Object));
    });

    [Fact]
    public Task RegisterBlock_ObjectIsNotBlockView_Throws() => StaTest.RunAsync(() =>
    {
        using var host = new WinFormFormHost();

        Assert.Throws<ArgumentException>(() => host.RegisterBlock(new object()));
    });

    [Fact]
    public Task UnregisterBlock_RegisteredBlock_UnbindsAndClearsActiveBlock() => StaTest.RunAsync(() =>
    {
        var block = CreateBlock("EMP");
        using var host = new WinFormFormHost();
        host.RegisterBlock(block.Object);
        var activeBlockChanges = 0;
        host.ActiveBlockChanged += (_, _) => activeBlockChanges++;

        var result = host.UnregisterBlock("emp");

        Assert.True(result);
        Assert.False(host.IsBlockRegistered("EMP"));
        Assert.Null(host.ActiveBlockName);
        Assert.Equal(1, activeBlockChanges);
        block.Verify(b => b.Unbind(), Times.Once);
    });

    [Fact]
    public Task TrySetActiveBlock_UsesCaseInsensitiveRegistryAndRaisesOnlyOnChange() => StaTest.RunAsync(() =>
    {
        using var host = new WinFormFormHost();
        host.RegisterBlock(CreateBlock("EMP").Object);
        host.RegisterBlock(CreateBlock("DEPT").Object);
        var activeBlockChanges = 0;
        host.ActiveBlockChanged += (_, _) => activeBlockChanges++;

        Assert.True(host.TrySetActiveBlock("dept"));
        Assert.Equal("DEPT", host.ActiveBlockName);
        Assert.Equal(1, activeBlockChanges);

        Assert.True(host.TrySetActiveBlock("DEPT"));
        Assert.Equal(1, activeBlockChanges);

        Assert.False(host.TrySetActiveBlock("UNKNOWN"));
        Assert.Equal("DEPT", host.ActiveBlockName);
    });

    private static Mock<IBlockView> CreateBlock(string name, object? view = null)
    {
        var block = new Mock<IBlockView>();
        block.SetupGet(b => b.BlockName).Returns(name);
        block.SetupGet(b => b.View).Returns(view ?? new Panel());
        return block;
    }
}
