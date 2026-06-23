using Moq;
using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Data.Integrated.Forms.FeatureControls;
using Xunit;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Tests.Forms;

public class WinFormLovDialogTests
{
    [Fact]
    public Task CancelledSelection_DoesNotMutateBlock() => StaTest.RunAsync(async () =>
    {
        var host = new Mock<IBeepFormsHost>(MockBehavior.Strict);
        host.Setup(x => x.GetLov("ORDERS", "CUSTOMER_ID"))
            .Returns((LOVDefinition?)null);
        using var dialog = new WinFormLovDialog(host.Object, "ORDERS", "CUSTOMER_ID");

        dialog.CancelSelection();

        Assert.False(await dialog.ApplySelectionAsync());
        host.Verify(x => x.SetFieldValue(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()), Times.Never);
    });

    [Fact]
    public Task AcceptedSelection_AppliesEngineMappedFields() => StaTest.RunAsync(async () =>
    {
        var selected = new { Id = 10, Name = "Acme" };
        var lov = LOVDefinition.CreateLookup(
            "CUSTOMERS", "MAIN", "CUSTOMERS", "Id", "Name");
        var host = new Mock<IBeepFormsHost>();
        host.Setup(x => x.GetLov("ORDERS", "CUSTOMER_ID")).Returns(lov);
        host.Setup(x => x.GetLovRelatedFieldValues(lov, selected))
            .Returns(new Dictionary<string, object>
            {
                ["CUSTOMER_ID"] = 10,
                ["CUSTOMER_NAME"] = "Acme"
            });
        host.Setup(x => x.SetFieldValue(
            "ORDERS", It.IsAny<string>(), It.IsAny<object?>())).Returns(true);
        using var dialog = new WinFormLovDialog(host.Object, "ORDERS", "CUSTOMER_ID");

        dialog.AcceptSelection(selected);

        Assert.True(await dialog.ApplySelectionAsync());
        host.Verify(x => x.SetFieldValue("ORDERS", "CUSTOMER_ID", 10), Times.Once);
        host.Verify(x => x.SetFieldValue("ORDERS", "CUSTOMER_NAME", "Acme"), Times.Once);
    });

    [Fact]
    public Task AcceptedSelection_MapsReturnSentinelToInvokingField() =>
        StaTest.RunAsync(async () =>
        {
            var selected = new { Id = 10, Name = "Acme" };
            var lov = LOVDefinition.CreateLookup(
                "CUSTOMERS", "MAIN", "CUSTOMERS", "Id", "Name");
            var host = new Mock<IBeepFormsHost>();
            host.Setup(x => x.GetLov("ORDERS", "CUSTOMER_ID")).Returns(lov);
            host.Setup(x => x.GetLovRelatedFieldValues(lov, selected))
                .Returns(new Dictionary<string, object>
                {
                    ["__RETURN_VALUE__"] = 10,
                });
            host.Setup(x => x.SetFieldValue(
                    "ORDERS", "CUSTOMER_ID", 10)).Returns(true);
            using var dialog = new WinFormLovDialog(
                host.Object,
                "ORDERS",
                "CUSTOMER_ID");
            dialog.AcceptSelection(selected);

            Assert.True(await dialog.ApplySelectionAsync());
            host.Verify(x => x.SetFieldValue(
                "ORDERS",
                "__RETURN_VALUE__",
                It.IsAny<object?>()), Times.Never);
        });

    [Fact]
    public Task AcceptedSelection_DoesNotPopulateRelatedFieldsWhenDisabled() =>
        StaTest.RunAsync(async () =>
        {
            var selected = new { Id = 10, Name = "Acme" };
            var lov = LOVDefinition.CreateLookup(
                "CUSTOMERS", "MAIN", "CUSTOMERS", "Id", "Name");
            lov.AutoPopulateRelatedFields = false;
            var host = new Mock<IBeepFormsHost>();
            host.Setup(x => x.GetLov("ORDERS", "CUSTOMER_ID")).Returns(lov);
            host.Setup(x => x.GetLovRelatedFieldValues(lov, selected))
                .Returns(new Dictionary<string, object>
                {
                    ["__RETURN_VALUE__"] = 10,
                    ["CUSTOMER_NAME"] = "Acme",
                });
            host.Setup(x => x.SetFieldValue(
                    "ORDERS", "CUSTOMER_ID", 10)).Returns(true);
            using var dialog = new WinFormLovDialog(
                host.Object,
                "ORDERS",
                "CUSTOMER_ID");
            dialog.AcceptSelection(selected);

            Assert.True(await dialog.ApplySelectionAsync());
            host.Verify(x => x.SetFieldValue(
                "ORDERS",
                "CUSTOMER_NAME",
                It.IsAny<object?>()), Times.Never);
        });
}
