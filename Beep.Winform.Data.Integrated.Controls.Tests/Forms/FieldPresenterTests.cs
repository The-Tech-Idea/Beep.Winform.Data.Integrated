using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Winform.Data.Integrated.Forms.FieldHost;
using Xunit;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Tests.Forms;

public class FieldPresenterTests
{
    [Fact]
    public Task TextPresenter_SetValue_DoesNotRaiseValueChanged() => StaTest.RunAsync(() =>
    {
        using var presenter = new WinFormTextBoxFieldPresenter(
            new EntityField { FieldName = "Name", Fieldtype = "string" });
        var raised = 0;
        presenter.ValueChanged += (_, _) => raised++;

        presenter.SetValue("Alice");

        Assert.Equal("Alice", presenter.Value);
        Assert.Equal(0, raised);
    });

    [Theory]
    [InlineData("int", typeof(WinFormNumericFieldPresenter))]
    [InlineData("date", typeof(WinFormDateFieldPresenter))]
    [InlineData("bool", typeof(WinFormBooleanFieldPresenter))]
    [InlineData("string", typeof(WinFormTextBoxFieldPresenter))]
    public async Task Registry_MapsEngineFieldTypes(string fieldType, Type expected)
    {
        await StaTest.RunAsync(() =>
        {
            var registry = new WinFormFieldPresenterRegistry();
            using var presenter = registry.Create(
                new EntityField { FieldName = "F", Fieldtype = fieldType });
            Assert.IsType(expected, presenter);
        });
    }
}
