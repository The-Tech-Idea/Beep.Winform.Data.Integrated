using Moq;
using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;
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

    [Fact]
    public Task SecurityPanel_DelegatesPoliciesAndViolationQueries() => StaTest.RunAsync(() =>
    {
        var host = new Mock<IBeepFormsHost>();
        var context = new SecurityContext { UserName = "reader" };
        var policy = new FieldSecurity
        {
            BlockName = "ORDERS",
            FieldName = "TOTAL",
            Editable = false
        };
        var violations = new[]
        {
            new SecurityViolationEventArgs
            {
                BlockName = "ORDERS",
                FieldName = "TOTAL",
                Permission = SecurityPermission.Update
            }
        };
        host.Setup(x => x.GetSecurityViolations()).Returns(violations);
        using var panel = new WinFormSecurityPanel(host.Object, "ORDERS");

        panel.ApplyContext(context);
        panel.SetFieldPolicy("TOTAL", policy);

        host.Verify(x => x.SetSecurityContext(context), Times.Once);
        host.Verify(x => x.SetFieldSecurity("ORDERS", "TOTAL", policy), Times.Once);
        Assert.Same(violations, panel.GetViolations());
    });

    [Fact]
    public Task RemainingFeaturePanels_DependOnlyOnFormsHost() => StaTest.RunAsync(async () =>
    {
        var host = new Mock<IBeepFormsHost>();
        host.Setup(x => x.UndoBlock("ORDERS")).Returns(true);
        host.Setup(x => x.ValidateCrossBlock()).Returns(["invalid"]);
        host.Setup(x => x.GetDirtyBlocks()).Returns(["ORDERS"]);
        host.Setup(x => x.SaveDirtyBlocksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        host.Setup(x => x.GetItemProperty("ORDERS", "TOTAL", "FORMAT_MASK"))
            .Returns("#,##0.00");

        using var undo = new WinFormUndoRedoPanel(host.Object, "ORDERS");
        using var validation = new WinFormCrossBlockValidationPanel(host.Object);
        using var dirty = new WinFormDirtyStatePanel(host.Object);
        using var items = new WinFormItemPropertyPanel(host.Object, "ORDERS");
        using var audit = new WinFormAuditPanel(host.Object, "ORDERS");

        Assert.True(undo.Undo());
        Assert.Single(validation.ValidateRules());
        Assert.Single(dirty.GetDirtyBlocks());
        Assert.True(await dirty.SaveAsync());
        Assert.Equal("#,##0.00", items.GetProperty("TOTAL", "FORMAT_MASK"));
        Assert.Empty(audit.GetEntries());
    });
}
