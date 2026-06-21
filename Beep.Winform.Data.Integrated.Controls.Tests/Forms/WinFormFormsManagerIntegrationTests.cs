using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.UOWManager;
using TheTechIdea.Beep.Winform.Data.Integrated.Forms.BlockHost;
using TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;
using Xunit;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Tests.Forms;

public class WinFormFormsManagerIntegrationTests
{
    [Fact]
    public Task RealFormsManager_GeneratesAndEditsBlockFields() => StaTest.RunAsync(() =>
    {
        var record = new EmployeeRecord { Id = 1, Name = "Alice" };
        var editor = new Mock<IDMEEditor>();
        var uow = new Mock<IUnitofWork>();
        uow.SetupGet(x => x.CurrentItem).Returns(record);
        uow.SetupGet(x => x.TotalItemCount).Returns(1);

        var entity = new EntityStructure("EMPLOYEES")
        {
            Fields =
            [
                new EntityField { FieldName = "Id", Fieldtype = "int", IsReadOnly = true },
                new EntityField { FieldName = "Name", Fieldtype = "string" }
            ]
        };

        using var manager = new FormsManager(editor.Object);
        manager.RegisterBlock("EMP", uow.Object, entity, "OracleConnection");
        using var host = new WinFormFormHost { FormsManager = manager };
        using var block = new WinFormBlockHost { BlockName = "EMP" };

        Assert.True(host.RegisterBlock(block));
        Assert.Equal(2, block.FieldPresenters.Count);
        Assert.Equal("Alice", block.FindFieldPresenter("Name")!.Value);

        block.FindFieldPresenter("Name")!.Value = "Bob";

        Assert.Equal("Bob", record.Name);
    });
}
