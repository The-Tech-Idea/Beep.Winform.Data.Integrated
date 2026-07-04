using System.Collections;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;
using Xunit;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Tests.Forms;

public class WinFormFormHostEngineTests
{
    [Fact]
    public Task Navigation_DelegatesEveryDirectionAndRefreshesMasterAndDetail() =>
        StaTest.RunAsync(() =>
        {
            var manager = CreateStrictManager();
            manager.Setup(m => m.BlockExists("EMP")).Returns(true);
            manager.Setup(m => m.BlockExists("DETAIL")).Returns(true);
            manager.Setup(m => m.FirstRecordAsync("EMP")).ReturnsAsync(true);
            manager.Setup(m => m.PreviousRecordAsync("EMP")).ReturnsAsync(true);
            manager.Setup(m => m.NextRecordAsync("EMP")).ReturnsAsync(true);
            manager.Setup(m => m.LastRecordAsync("EMP")).ReturnsAsync(true);
            manager.Setup(m => m.NavigateToRecordAsync("EMP", 4))
                .ReturnsAsync(true);
            manager.Setup(m => m.GetDetailBlocks("EMP")).Returns(["DETAIL"]);
            manager.Setup(m => m.GetDetailBlocks("DETAIL")).Returns([]);

            using var host = new WinFormFormHost { FormsManager = manager.Object };
            var master = CreateBlock("EMP");
            var detail = CreateBlock("DETAIL");
            host.RegisterBlock(master.Object);
            host.RegisterBlock(detail.Object);

            Assert.True(host.MoveFirstAsync("EMP"));
            Assert.True(host.MovePreviousAsync("EMP"));
            Assert.True(host.MoveNextAsync("EMP"));
            Assert.True(host.MoveLastAsync("EMP"));
            Assert.True(host.MoveToRecordAsync("EMP", 4));

            manager.Verify(m => m.FirstRecordAsync("EMP"), Times.Once);
            manager.Verify(m => m.PreviousRecordAsync("EMP"), Times.Once);
            manager.Verify(m => m.NextRecordAsync("EMP"), Times.Once);
            manager.Verify(m => m.LastRecordAsync("EMP"), Times.Once);
            manager.Verify(
                m => m.NavigateToRecordAsync("EMP", 4),
                Times.Once);
            master.Verify(m => m.SyncFromManager(), Times.Exactly(5));
            detail.Verify(m => m.SyncFromManager(), Times.Exactly(5));
        });

    [Fact]
    public Task CrudAndQuery_DelegateAndRefreshOnSuccessOnly() => StaTest.RunAsync(() =>
    {
        var manager = CreateStrictManager();
        manager.Setup(m => m.BlockExists("EMP")).Returns(true);
        manager.Setup(m => m.InsertRecordAsync("EMP", null)).ReturnsAsync(true);
        manager.Setup(m => m.DeleteCurrentRecordAsync("EMP")).ReturnsAsync(true);
        manager.Setup(m => m.DuplicateCurrentRecordAsync("EMP", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        manager.Setup(m => m.ExecuteQueryAsync("EMP", null)).ReturnsAsync(false);
        manager.Setup(m => m.GetDetailBlocks("EMP")).Returns([]);

        using var host = new WinFormFormHost { FormsManager = manager.Object };
        var block = CreateBlock("EMP");
        host.RegisterBlock(block.Object);

        Assert.True(host.InsertBlockRecordAsync("EMP"));
        Assert.True(host.DeleteBlockCurrentRecordAsync("EMP"));
        Assert.True(host.DuplicateCurrentRecordAsync("EMP"));
        Assert.False(host.ExecuteQueryAsync("EMP"));

        manager.Verify(m => m.InsertRecordAsync("EMP", null), Times.Once);
        manager.Verify(m => m.DeleteCurrentRecordAsync("EMP"), Times.Once);
        manager.Verify(
            m => m.DuplicateCurrentRecordAsync("EMP", It.IsAny<CancellationToken>()),
            Times.Once);
        manager.Verify(m => m.ExecuteQueryAsync("EMP", null), Times.Once);
        block.Verify(m => m.SyncFromManager(), Times.Exactly(3));
    });

    [Fact]
    public Task SaveRollbackClearAndNew_UseUnitOfWorkAndManager() => StaTest.RunAsync(() =>
    {
        var manager = CreateStrictManager();
        manager.Setup(m => m.BlockExists("EMP")).Returns(true);
        manager.Setup(m => m.ClearBlockAsync("EMP")).Returns(Task.CompletedTask);
        manager.Setup(m => m.GetDetailBlocks("EMP")).Returns([]);
        var unit = new Mock<IUnitofWork>(MockBehavior.Strict);
        unit.Setup(m => m.Commit()).ReturnsAsync(new ErrorsInfo { Flag = Errors.Ok });
        unit.Setup(m => m.Rollback()).ReturnsAsync(new ErrorsInfo { Flag = Errors.Ok });
        unit.Setup(m => m.New());
        manager.Setup(m => m.GetUnitOfWork("EMP")).Returns(unit.Object);

        using var host = new WinFormFormHost { FormsManager = manager.Object };
        var block = CreateBlock("EMP");
        host.RegisterBlock(block.Object);

        Assert.True(host.SaveBlockAsync("EMP"));
        Assert.True(host.RollbackBlockAsync("EMP"));
        Assert.True(host.ClearBlockAsync("EMP"));
        Assert.True(host.ClearRecordAsync("EMP"));

        unit.Verify(m => m.Commit(), Times.Once);
        unit.Verify(m => m.Rollback(), Times.Once);
        unit.Verify(m => m.New(), Times.Once);
        manager.Verify(m => m.ClearBlockAsync("EMP"), Times.Once);
        block.Verify(m => m.SyncFromManager(), Times.Exactly(4));
    });

    [Fact]
    public Task QueryMode_DelegatesEnterAndExitAndRefreshes() => StaTest.RunAsync(() =>
    {
        var manager = CreateStrictManager();
        manager.Setup(m => m.BlockExists("EMP")).Returns(true);
        manager.Setup(m => m.EnterQueryAsync("EMP")).ReturnsAsync(true);
        manager.Setup(m => m.ExitingQueryModeAsync("EMP"));
        manager.Setup(m => m.GetDetailBlocks("EMP")).Returns([]);
        using var host = new WinFormFormHost { FormsManager = manager.Object };
        var block = CreateBlock("EMP");
        host.RegisterBlock(block.Object);

        Assert.True(host.EnterQueryModeAsync("EMP"));
        Assert.True(host.ExitQueryModeAsync("EMP"));

        manager.Verify(m => m.EnterQueryAsync("EMP"), Times.Once);
        manager.Verify(m => m.ExitingQueryModeAsync("EMP"), Times.Once);
        block.Verify(m => m.SyncFromManager(), Times.Exactly(2));
    });

    [Fact]
    public Task StateMetadataAndData_DelegateToBlockUnitAndItemProperties() =>
        StaTest.RunAsync(() =>
        {
            var fields = new List<EntityField> { new() { FieldName = "Name" } };
            var info = new DataBlockInfo
            {
                BlockName = "EMP",
                EntityStructure = new EntityStructure { Fields = fields },
                Mode = DataBlockMode.CRUD,
                QueryAllowed = false
            };
            var units = new ArrayList { new EmployeeRecord(), new EmployeeRecord() };
            var unit = new Mock<IUnitofWork>(MockBehavior.Strict);
            unit.SetupGet(m => m.Units).Returns(units);
            unit.SetupGet(m => m.IsDirty).Returns(true);
            var itemProperties = new Mock<IItemPropertyManager>(MockBehavior.Strict);
            itemProperties.Setup(m => m.IsItemQueryAllowed("EMP", "Name")).Returns(false);
            var manager = CreateStrictManager();
            manager.Setup(m => m.GetBlock("EMP")).Returns(info);
            manager.Setup(m => m.GetUnitOfWork("EMP")).Returns(unit.Object);
            manager.Setup(m => m.GetBlockCount("EMP", null)).Returns(2);
            manager.SetupGet(m => m.ItemProperties).Returns(itemProperties.Object);
            manager.Setup(m => m.GetDetailBlocks("EMP")).Returns(["DETAIL"]);
            using var host = new WinFormFormHost { FormsManager = manager.Object };

            Assert.Same(info, host.GetBlockInfo("EMP"));
            Assert.Same(fields, host.GetBlockFields("EMP"));
            Assert.Same(units, host.GetBlockData("EMP"));
            Assert.Equal(2, host.GetBlockRecordCount("EMP"));
            Assert.Equal(DataBlockMode.CRUD, host.GetBlockMode("EMP"));
            Assert.False(host.IsBlockQueryAllowed("EMP"));
            Assert.False(host.IsFieldQueryAllowed("EMP", "Name"));
            Assert.True(host.IsBlockDirty("EMP"));
            Assert.Equal(["DETAIL"], host.GetDetailBlockNames("EMP"));
        });

    [Fact]
    public Task GetBlockRecordCount_DelegatesNormalizedNameToManager() => StaTest.RunAsync(() =>
    {
        var manager = CreateStrictManager();
        manager.Setup(m => m.GetBlockCount("EMP", null)).Returns(7);
        using var host = new WinFormFormHost { FormsManager = manager.Object };

        Assert.Equal(7, host.GetBlockRecordCount(" EMP "));

        manager.Verify(m => m.GetBlockCount("EMP", null), Times.Once);
    });

    [Fact]
    public Task ExecuteQuery_SuccessRefreshesTargetAndRegisteredDetails() => StaTest.RunAsync(() =>
    {
        var manager = CreateStrictManager();
        manager.Setup(m => m.BlockExists("EMP")).Returns(true);
        manager.Setup(m => m.BlockExists("DETAIL")).Returns(true);
        manager.Setup(m => m.ExecuteQueryAsync("EMP", null)).ReturnsAsync(true);
        manager.Setup(m => m.GetDetailBlocks("EMP")).Returns(["DETAIL"]);
        var target = CreateBlock("EMP");
        var detail = CreateBlock("DETAIL");
        using var host = new WinFormFormHost { FormsManager = manager.Object };
        host.RegisterBlock(target.Object);
        host.RegisterBlock(detail.Object);

        Assert.True(host.ExecuteQueryAsync("EMP"));

        target.Verify(m => m.SyncFromManager(), Times.Once);
        detail.Verify(m => m.SyncFromManager(), Times.Once);
    });

    [Fact]
    public Task Save_SuccessRefreshesTargetAndRegisteredDetails() => StaTest.RunAsync(() =>
    {
        var manager = CreateStrictManager();
        manager.Setup(m => m.BlockExists("EMP")).Returns(true);
        manager.Setup(m => m.BlockExists("DETAIL")).Returns(true);
        manager.Setup(m => m.GetDetailBlocks("EMP")).Returns(["DETAIL"]);
        var unit = new Mock<IUnitofWork>(MockBehavior.Strict);
        unit.Setup(m => m.Commit()).ReturnsAsync(new ErrorsInfo { Flag = Errors.Ok });
        manager.Setup(m => m.GetUnitOfWork("EMP")).Returns(unit.Object);
        var target = CreateBlock("EMP");
        var detail = CreateBlock("DETAIL");
        using var host = new WinFormFormHost { FormsManager = manager.Object };
        host.RegisterBlock(target.Object);
        host.RegisterBlock(detail.Object);

        Assert.True(host.SaveBlockAsync("EMP"));

        unit.Verify(m => m.Commit(), Times.Once);
        target.Verify(m => m.SyncFromManager(), Times.Once);
        detail.Verify(m => m.SyncFromManager(), Times.Once);
    });

    [Fact]
    public Task LovAndValidation_DelegateToEngineManagers() => StaTest.RunAsync(() =>
    {
        var lov = new LOVDefinition();
        var loaded = LOVResult.Ok([new EmployeeRecord()]);
        var shown = LOVResult.Ok([new EmployeeRecord()]);
        var related = new Dictionary<string, object> { ["Name"] = "Alice" };
        var record = new Dictionary<string, object> { ["Name"] = "Alice" };
        var validationResult = new RecordValidationResult { BlockName = "EMP" };
        var lovManager = new Mock<ILOVManager>(MockBehavior.Strict);
        lovManager.Setup(m => m.HasLOV("EMP", "Name")).Returns(true);
        lovManager.Setup(m => m.GetLOV("EMP", "Name")).Returns(lov);
        lovManager.Setup(m => m.LoadLOVDataAsync("EMP", "Name", "A")).ReturnsAsync(loaded);
        lovManager.Setup(m => m.GetRelatedFieldValues(lov, record)).Returns(related);
        var validation = new Mock<IValidationManager>(MockBehavior.Strict);
        validation.Setup(m => m.ValidateRecord("EMP", record, ValidationTiming.Manual))
            .Returns(validationResult);
        var manager = CreateStrictManager();
        manager.SetupGet(m => m.LOV).Returns(lovManager.Object);
        manager.SetupGet(m => m.Validation).Returns(validation.Object);
        manager.Setup(m => m.ShowLOVAsync("EMP", "Name", "A", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shown);
        using var host = new WinFormFormHost { FormsManager = manager.Object };

        Assert.True(host.HasLov("EMP", "Name"));
        Assert.Same(lov, host.GetLov("EMP", "Name"));
        Assert.Same(loaded, host.LoadLovDataAsync("EMP", "Name", "A"));
        Assert.Same(shown, host.ShowLovAsync("EMP", "Name", "A"));
        Assert.Same(related, host.GetLovRelatedFieldValues(lov, record));
        Assert.Same(
            validationResult,
            host.ValidateBlockRecord("EMP", record, ValidationTiming.Manual));
    });

    [Fact]
    public Task OperationsWithoutManager_ReturnFalseAndQueriesReturnSafeDefaults() =>
        StaTest.RunAsync(() =>
        {
            using var host = new WinFormFormHost();

            Assert.False(host.MoveNextAsync("EMP"));
            Assert.False(host.SaveBlockAsync("EMP"));
            Assert.False(host.ExecuteQueryAsync("EMP"));
            Assert.Null(host.GetBlockInfo("EMP"));
            Assert.Null(host.GetBlockFields("EMP"));
            Assert.Null(host.GetBlockData("EMP"));
            Assert.Equal(0, host.GetBlockRecordCount("EMP"));
            Assert.Equal(DataBlockMode.Query, host.GetBlockMode("EMP"));
            Assert.False(host.IsBlockDirty("EMP"));
            Assert.Empty(host.GetDetailBlockNames("EMP"));
            Assert.False(host.HasLov("EMP", "Name"));
            Assert.Null(host.GetLov("EMP", "Name"));
            Assert.False(host.LoadLovDataAsync("EMP", "Name").Success);
            Assert.Null(host.ValidateBlockRecord(
                "EMP",
                new Dictionary<string, object>(),
                ValidationTiming.Manual));
        });

    [Fact]
    public Task EngineException_ReturnsFalseDoesNotRefreshAndNotificationsDoNotThrow() =>
        StaTest.RunAsync(() =>
        {
            var manager = CreateStrictManager();
            manager.Setup(m => m.BlockExists("EMP")).Returns(true);
            manager.Setup(m => m.NextRecordAsync("EMP"))
                .ThrowsAsync(new InvalidOperationException("navigation failed"));
            using var host = new WinFormFormHost { FormsManager = manager.Object };
            var block = CreateBlock("EMP");
            host.RegisterBlock(block.Object);

            Assert.False(host.MoveNextAsync("EMP"));
            block.Verify(m => m.SyncFromManager(), Times.Never);
            host.ShowInfo("info");
            host.ShowWarning("warning");
            host.ShowError("error");
        });

    [Fact]
    public Task Operation_NormalizesBlockNameBeforeEngineDelegation() => StaTest.RunAsync(() =>
    {
        var manager = CreateStrictManager();
        manager.Setup(m => m.NextRecordAsync("EMP")).ReturnsAsync(true);
        manager.Setup(m => m.GetDetailBlocks("EMP")).Returns([]);
        using var host = new WinFormFormHost { FormsManager = manager.Object };

        Assert.True(host.MoveNextAsync(" EMP "));

        manager.Verify(m => m.NextRecordAsync("EMP"), Times.Once);
    });

    private static Mock<IUnitofWorksManager> CreateStrictManager() =>
        new(MockBehavior.Strict);

    private static Mock<IBlockView> CreateBlock(string blockName)
    {
        var block = new Mock<IBlockView>(MockBehavior.Strict);
        block.SetupGet(m => m.BlockName).Returns(blockName);
        block.SetupGet(m => m.View).Returns(new Panel());
        block.SetupGet(m => m.IsBound).Returns(false);
        block.Setup(m => m.Bind(It.IsAny<IBeepFormsHost>()));
        block.Setup(m => m.Unbind());
        block.Setup(m => m.SyncFromManager());
        return block;
    }
}
