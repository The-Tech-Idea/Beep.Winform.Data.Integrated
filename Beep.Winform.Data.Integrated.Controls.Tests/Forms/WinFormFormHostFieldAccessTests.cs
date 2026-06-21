using Moq;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;
using Xunit;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Tests.Forms;

public class WinFormFormHostFieldAccessTests
{
    [Fact]
    public Task GetCurrentBlockRecordIndex_ReturnsUnitOfWorkCurrentIndex() => StaTest.RunAsync(() =>
    {
        var units = new TestUnits { CurrentIndex = 3 };
        var unitOfWork = new Mock<IUnitofWork>();
        unitOfWork.SetupGet(u => u.Units).Returns(units);
        var manager = new Mock<IUnitofWorksManager>();
        manager.Setup(m => m.GetUnitOfWork("EMP")).Returns(unitOfWork.Object);
        using var host = new WinFormFormHost { FormsManager = manager.Object };

        Assert.Equal(3, host.GetCurrentBlockRecordIndex("EMP"));
    });

    [Fact]
    public Task GetCurrentBlockRecordIndex_MissingUnitOfWorkOrUnits_ReturnsMinusOne() => StaTest.RunAsync(() =>
    {
        var manager = new Mock<IUnitofWorksManager>();
        manager.Setup(m => m.GetUnitOfWork("MISSING")).Returns((IUnitofWork)null!);
        using var host = new WinFormFormHost { FormsManager = manager.Object };

        Assert.Equal(-1, host.GetCurrentBlockRecordIndex("MISSING"));
    });

    [Fact]
    public Task GetFieldValue_CurrentRecord_DelegatesToManager() => StaTest.RunAsync(() =>
    {
        var record = new EmployeeRecord { Name = "Alice" };
        var unitOfWork = new Mock<IUnitofWork>();
        unitOfWork.SetupGet(u => u.CurrentItem).Returns(record);
        var manager = new Mock<IUnitofWorksManager>();
        manager.Setup(m => m.GetUnitOfWork("EMP")).Returns(unitOfWork.Object);
        manager.Setup(m => m.GetFieldValue(record, "Name")).Returns("Alice");
        using var host = new WinFormFormHost { FormsManager = manager.Object };

        Assert.Equal("Alice", host.GetFieldValue("EMP", "Name"));
        manager.Verify(m => m.GetFieldValue(record, "Name"), Times.Once);
    });

    [Fact]
    public Task GetFieldValue_NoCurrentRecord_ReturnsNullWithoutReadingField() => StaTest.RunAsync(() =>
    {
        var unitOfWork = new Mock<IUnitofWork>();
        unitOfWork.SetupGet(u => u.CurrentItem).Returns((object)null!);
        var manager = new Mock<IUnitofWorksManager>();
        manager.Setup(m => m.GetUnitOfWork("EMP")).Returns(unitOfWork.Object);
        using var host = new WinFormFormHost { FormsManager = manager.Object };

        Assert.Null(host.GetFieldValue("EMP", "Name"));
        manager.Verify(
            m => m.GetFieldValue(It.IsAny<object>(), It.IsAny<string>()),
            Times.Never);
    });

    [Fact]
    public Task SetFieldValue_UpdateSucceeds_ReturnsValidationResult() => StaTest.RunAsync(() =>
    {
        var record = new EmployeeRecord();
        var unitOfWork = new Mock<IUnitofWork>();
        unitOfWork.SetupGet(u => u.CurrentItem).Returns(record);
        var manager = new Mock<IUnitofWorksManager>();
        manager.Setup(m => m.GetUnitOfWork("EMP")).Returns(unitOfWork.Object);
        manager.Setup(m => m.SetFieldValue(record, "Name", "Alice")).Returns(true);
        manager.Setup(m => m.ValidateField("EMP", "Name", "Alice")).Returns(true);
        using var host = new WinFormFormHost { FormsManager = manager.Object };

        Assert.True(host.SetFieldValue("EMP", "Name", "Alice"));
        manager.Verify(m => m.SetFieldValue(record, "Name", "Alice"), Times.Once);
        manager.Verify(m => m.ValidateField("EMP", "Name", "Alice"), Times.Once);
    });

    [Fact]
    public Task SetFieldValue_UpdateFails_DoesNotValidate() => StaTest.RunAsync(() =>
    {
        var record = new EmployeeRecord();
        var unitOfWork = new Mock<IUnitofWork>();
        unitOfWork.SetupGet(u => u.CurrentItem).Returns(record);
        var manager = new Mock<IUnitofWorksManager>();
        manager.Setup(m => m.GetUnitOfWork("EMP")).Returns(unitOfWork.Object);
        manager.Setup(m => m.SetFieldValue(record, "Name", "Alice")).Returns(false);
        using var host = new WinFormFormHost { FormsManager = manager.Object };

        Assert.False(host.SetFieldValue("EMP", "Name", "Alice"));
        manager.Verify(
            m => m.ValidateField(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()),
            Times.Never);
    });

    [Fact]
    public Task SetFieldValue_NoCurrentRecord_ReturnsFalseWithoutWriting() => StaTest.RunAsync(() =>
    {
        var unitOfWork = new Mock<IUnitofWork>();
        unitOfWork.SetupGet(u => u.CurrentItem).Returns((object)null!);
        var manager = new Mock<IUnitofWorksManager>();
        manager.Setup(m => m.GetUnitOfWork("EMP")).Returns(unitOfWork.Object);
        using var host = new WinFormFormHost { FormsManager = manager.Object };

        Assert.False(host.SetFieldValue("EMP", "Name", "Alice"));
        manager.Verify(
            m => m.SetFieldValue(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<object?>()),
            Times.Never);
    });

    [Fact]
    public Task DirectFieldAccess_WithoutManager_ReturnsSafeDefaults() => StaTest.RunAsync(() =>
    {
        using var host = new WinFormFormHost();

        Assert.Null(host.GetCurrentBlockItem("EMP"));
        Assert.Equal(-1, host.GetCurrentBlockRecordIndex("EMP"));
        Assert.Null(host.GetFieldValue("EMP", "Name"));
        Assert.False(host.SetFieldValue("EMP", "Name", "Alice"));
    });

    [Fact]
    public Task DirectFieldAccess_MissingBlock_ReturnsSafeDefaults() => StaTest.RunAsync(() =>
    {
        var manager = new Mock<IUnitofWorksManager>();
        manager.Setup(m => m.GetUnitOfWork("MISSING"))
            .Throws(new KeyNotFoundException("missing block"));
        using var host = new WinFormFormHost { FormsManager = manager.Object };

        Assert.Null(host.GetCurrentBlockItem("MISSING"));
        Assert.Equal(-1, host.GetCurrentBlockRecordIndex("MISSING"));
        Assert.Null(host.GetFieldValue("MISSING", "Name"));
        Assert.False(host.SetFieldValue("MISSING", "Name", "Alice"));
    });

    public sealed class TestUnits
    {
        public int CurrentIndex { get; set; }
    }
}
