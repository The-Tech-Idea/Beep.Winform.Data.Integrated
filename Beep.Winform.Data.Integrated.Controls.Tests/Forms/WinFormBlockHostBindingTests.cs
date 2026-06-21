using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Data.Integrated.Forms.BlockHost;
using Xunit;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Tests.Forms;

public class WinFormBlockHostBindingTests
{
    [Fact]
    public Task Bind_GeneratesVisiblePresentersAndSynchronizesWithoutWriteback() => StaTest.RunAsync(() =>
    {
        var manager = new Mock<IUnitofWorksManager>();
        var host = new Mock<IBeepFormsHost>();
        host.SetupGet(h => h.FormsManager).Returns(manager.Object);
        host.Setup(h => h.IsBlockRegistered("EMP")).Returns(true);
        host.Setup(h => h.GetBlockFields("EMP")).Returns([
            new EntityField { FieldName = "Name", Fieldtype = "string" },
            new EntityField { FieldName = "Secret", Fieldtype = "string", IsHidden = true }
        ]);
        host.Setup(h => h.GetFieldValue("EMP", "Name")).Returns("Alice");
        host.Setup(h => h.GetBlockMode("EMP")).Returns(DataBlockMode.Query);
        host.Setup(h => h.GetBlockRecordCount("EMP")).Returns(1);
        host.Setup(h => h.GetCurrentBlockRecordIndex("EMP")).Returns(0);

        using var block = new WinFormBlockHost { BlockName = "EMP" };
        block.Bind(host.Object);

        Assert.Single(block.FieldPresenters);
        Assert.Equal("Alice", block.FindFieldPresenter("Name")!.Value);
        host.Verify(h => h.SetFieldValue(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()), Times.Never);
    });

    [Fact]
    public Task UserEdit_WritesThroughHostOnce() => StaTest.RunAsync(() =>
    {
        var host = BuildHost();
        using var block = new WinFormBlockHost { BlockName = "EMP" };
        block.Bind(host.Object);

        var presenter = block.FindFieldPresenter("Name")!;
        presenter.Value = "Bob";

        host.Verify(h => h.SetFieldValue("EMP", "Name", "Bob"), Times.Once);
    });

    [Fact]
    public Task EnterQuery_UserEditStoresCriterionWithoutWritingRecord() => StaTest.RunAsync(() =>
    {
        var host = BuildHost();
        host.Setup(h => h.EnterQueryModeAsync("EMP")).ReturnsAsync(true);
        using var block = new WinFormBlockHost { BlockName = "EMP" };
        block.Bind(host.Object);

        block.EnterQueryMode();
        block.FindFieldPresenter("Name")!.Value = "Alice";

        Assert.Equal("Alice", block.FindFieldPresenter("Name")!.QueryValue);
        host.Verify(h => h.SetFieldValue(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()), Times.Never);
    });

    [Fact]
    public Task ExecuteQuery_SuccessUsesCriteriaAndClearsThem() => StaTest.RunAsync(async () =>
    {
        var host = BuildHost();
        host.Setup(h => h.EnterQueryModeAsync("EMP")).ReturnsAsync(true);
        host.Setup(h => h.ExecuteQueryByExampleAsync(
                "EMP",
                It.Is<IReadOnlyDictionary<string, QueryCriterion>>(criteria =>
                    Equals(criteria["Name"].Value, "Alice")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        using var block = new WinFormBlockHost { BlockName = "EMP" };
        block.Bind(host.Object);
        block.EnterQueryMode();
        block.FindFieldPresenter("Name")!.Value = "Alice";

        Assert.True(await block.ExecuteQueryAsync());

        Assert.Null(block.FindFieldPresenter("Name")!.QueryValue);
    });

    [Fact]
    public Task LockOnEditFailure_RestoresEngineValueWithoutWriting() => StaTest.RunAsync(async () =>
    {
        var host = BuildHost();
        host.Setup(h => h.GetLockOnEdit("EMP")).Returns(true);
        host.Setup(h => h.IsCurrentRecordLocked("EMP")).Returns(false);
        host.Setup(h => h.LockCurrentRecordAsync("EMP", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        host.Setup(h => h.GetFieldValue("EMP", "Name")).Returns("Alice");
        using var block = new WinFormBlockHost { BlockName = "EMP" };
        block.Bind(host.Object);

        block.FindFieldPresenter("Name")!.Value = "Bob";
        await Task.Delay(1);

        Assert.Equal("Alice", block.FindFieldPresenter("Name")!.Value);
        host.Verify(h => h.SetFieldValue("EMP", "Name", "Bob"), Times.Never);
    });

    private static Mock<IBeepFormsHost> BuildHost()
    {
        var host = new Mock<IBeepFormsHost>();
        host.Setup(h => h.IsBlockRegistered("EMP")).Returns(true);
        host.Setup(h => h.GetBlockFields("EMP")).Returns([
            new EntityField { FieldName = "Name", Fieldtype = "string" }
        ]);
        host.Setup(h => h.GetBlockMode("EMP")).Returns(DataBlockMode.Query);
        host.Setup(h => h.GetCurrentBlockRecordIndex("EMP")).Returns(0);
        host.Setup(h => h.SetFieldValue("EMP", "Name", It.IsAny<object?>())).Returns(true);
        return host;
    }
}
