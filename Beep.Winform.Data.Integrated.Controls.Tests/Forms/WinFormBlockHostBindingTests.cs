using Moq;
using System.Reflection;
using System.Windows.Forms;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Winform.Data.Integrated.Forms.BlockHost;
using TheTechIdea.Beep.Winform.Data.Integrated.Forms.FieldHost;
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

    [Fact]
    public Task SyncFromManager_AppliesEffectiveItemSecurityAndMaskedValue() => StaTest.RunAsync(() =>
    {
        var host = BuildHost();
        host.Setup(h => h.GetFieldValue("EMP", "Name")).Returns("123456789");
        host.Setup(h => h.GetFieldSecurity("EMP", "Name")).Returns(new FieldSecurity
        {
            BlockName = "EMP",
            FieldName = "Name",
            Masked = true,
            MaskPattern = "***-**-####"
        });
        host.Setup(h => h.GetMaskedFieldValue("EMP", "Name", "123456789"))
            .Returns("***-**-6789");
        host.Setup(h => h.GetItemInfo("EMP", "Name")).Returns(new ItemInfo
        {
            BlockName = "EMP",
            ItemName = "Name",
            Enabled = false,
            Visible = true,
            Required = true
        });

        using var block = new WinFormBlockHost { BlockName = "EMP" };
        block.Bind(host.Object);

        var presenter = block.FindFieldPresenter("Name")!;
        Assert.Equal("***-**-6789", presenter.Value);
        Assert.False(presenter.IsEnabled);
        Assert.True(presenter.IsVisible);
        Assert.True(presenter.IsRequired);
    });

    [Fact]
    public Task SyncFromManager_AppliesFullItemPresentationState() => StaTest.RunAsync(() =>
    {
        var host = BuildHost();
        host.Setup(h => h.GetItemInfo("EMP", "Name")).Returns(new ItemInfo
        {
            BlockName = "EMP",
            ItemName = "Name",
            Enabled = true,
            Visible = true,
            Required = true,
            UpdateAllowed = false,
            PromptText = "Employee name",
            HintText = "Read-only employee name"
        });

        using var block = new WinFormBlockHost { BlockName = "EMP" };
        block.Bind(host.Object);

        var presenter = block.FindFieldPresenter("Name")!;
        Assert.Equal("Employee name", presenter.Label);
        Assert.Equal("Read-only employee name", presenter.Prompt);
        Assert.True(presenter.IsReadOnly);
    });

    [Fact]
    public Task MoveToNextItem_FiresItemExitAndEntryTriggersInOrder() =>
        StaTest.RunAsync(async () =>
        {
            var calls = new List<string>();
            var host = new Mock<IBeepFormsHost>();
            host.Setup(instance => instance.IsBlockRegistered("EMP"))
                .Returns(true);
            host.Setup(instance => instance.GetBlockMode("EMP"))
                .Returns(DataBlockMode.Query);
            host.Setup(instance => instance.GetNextItem("EMP", "NAME"))
                .Returns("SALARY");
            host.Setup(instance => instance.FireBlockTriggerAsync(
                    It.IsAny<TriggerType>(),
                    "EMP",
                    It.IsAny<TriggerContext?>(),
                    It.IsAny<CancellationToken>()))
                .Callback<TriggerType, string, TriggerContext?, CancellationToken>(
                    (type, _, context, _) =>
                        calls.Add($"{type}:{context?.ItemName}"))
                .ReturnsAsync(TriggerResult.Success);
            host.Setup(instance => instance.FireKeyTriggerAsync(
                    KeyTriggerType.NextItem,
                    "EMP"))
                .Callback(() => calls.Add("KeyNextItem"))
                .ReturnsAsync(TriggerResult.Success);
            using var block = new WinFormBlockHost
            {
                BlockName = "EMP",
                AutoGenerateFields = false,
            };
            block.AddFieldPresenter(new WinFormTextBoxFieldPresenter(
                new EntityField { FieldName = "NAME", Fieldtype = "string" }));
            block.AddFieldPresenter(new WinFormNumericFieldPresenter(
                new EntityField { FieldName = "SALARY", Fieldtype = "decimal" }));
            block.Bind(host.Object);

            var moved = await block.MoveToNextItemAsync("NAME");

            Assert.True(moved);
            Assert.Equal("SALARY", block.ActiveFieldName);
            Assert.Equal(
                [
                    "WhenValidateItem:NAME",
                    "PostTextItem:NAME",
                    "KeyNextItem",
                    "PreTextItem:SALARY",
                    "WhenNewItemInstance:SALARY",
                ],
                calls);
        });

    [Fact]
    public Task MoveToNextItem_InvalidCurrentItemKeepsFocus() =>
        StaTest.RunAsync(async () =>
        {
            var invalid = new ItemValidationResult
            {
                BlockName = "EMP",
                ItemName = "NAME",
                Value = "",
                RuleResults =
                [
                    ValidationRuleResult.Failure(
                        "Required",
                        "NAME",
                        "Name is required."),
                ],
            };
            var host = new Mock<IBeepFormsHost>();
            host.Setup(instance => instance.IsBlockRegistered("EMP"))
                .Returns(true);
            host.Setup(instance => instance.GetBlockMode("EMP"))
                .Returns(DataBlockMode.Query);
            host.Setup(instance => instance.ValidateItem(
                    "EMP",
                    "NAME",
                    "",
                    ValidationTiming.OnBlur))
                .Returns(invalid);
            using var block = new WinFormBlockHost
            {
                BlockName = "EMP",
                AutoGenerateFields = false,
            };
            var presenter = new WinFormTextBoxFieldPresenter(
                new EntityField { FieldName = "NAME", Fieldtype = "string" });
            presenter.SetValue("");
            block.AddFieldPresenter(presenter);
            block.Bind(host.Object);

            var moved = await block.MoveToNextItemAsync("NAME");

            Assert.False(moved);
            Assert.Equal("NAME", block.ActiveFieldName);
            Assert.Equal("Name is required.", presenter.ValidationError);
            host.Verify(instance => instance.FireKeyTriggerAsync(
                It.IsAny<KeyTriggerType>(),
                It.IsAny<string>()), Times.Never);
        });

    [Fact]
    public Task MoveToPreviousItem_StopsWhenKeyTriggerFails() =>
        StaTest.RunAsync(async () =>
        {
            var host = new Mock<IBeepFormsHost>();
            host.Setup(instance => instance.IsBlockRegistered("EMP"))
                .Returns(true);
            host.Setup(instance => instance.GetBlockMode("EMP"))
                .Returns(DataBlockMode.Query);
            host.Setup(instance => instance.FireBlockTriggerAsync(
                    It.IsAny<TriggerType>(),
                    "EMP",
                    It.IsAny<TriggerContext?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(TriggerResult.Success);
            host.Setup(instance => instance.FireKeyTriggerAsync(
                    KeyTriggerType.PreviousItem,
                    "EMP"))
                .ReturnsAsync(TriggerResult.FormTriggerFailure);
            using var block = new WinFormBlockHost
            {
                BlockName = "EMP",
                AutoGenerateFields = false,
            };
            block.AddFieldPresenter(new WinFormTextBoxFieldPresenter(
                new EntityField { FieldName = "NAME", Fieldtype = "string" }));
            block.Bind(host.Object);

            var moved = await block.MoveToPreviousItemAsync("NAME");

            Assert.False(moved);
            host.Verify(instance => instance.GetPreviousItem(
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        });

    [Fact]
    public Task TabKey_UsesFormsItemNavigationPipeline() =>
        StaTest.RunAsync(async () =>
        {
            var navigationFired =
                new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var host = new Mock<IBeepFormsHost>();
            host.Setup(instance => instance.IsBlockRegistered("EMP"))
                .Returns(true);
            host.Setup(instance => instance.GetBlockMode("EMP"))
                .Returns(DataBlockMode.Query);
            host.Setup(instance => instance.GetNextItem("EMP", "NAME"))
                .Returns("SALARY");
            host.Setup(instance => instance.FireBlockTriggerAsync(
                    It.IsAny<TriggerType>(),
                    "EMP",
                    It.IsAny<TriggerContext?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(TriggerResult.Success);
            host.Setup(instance => instance.FireKeyTriggerAsync(
                    KeyTriggerType.NextItem,
                    "EMP"))
                .Callback(() => navigationFired.TrySetResult())
                .ReturnsAsync(TriggerResult.Success);
            using var block = new WinFormBlockHost
            {
                BlockName = "EMP",
                AutoGenerateFields = false,
            };
            block.AddFieldPresenter(new WinFormTextBoxFieldPresenter(
                new EntityField { FieldName = "NAME", Fieldtype = "string" }));
            block.AddFieldPresenter(new WinFormNumericFieldPresenter(
                new EntityField { FieldName = "SALARY", Fieldtype = "decimal" }));
            block.Bind(host.Object);
            var editor = Assert.IsAssignableFrom<Control>(
                block.FindFieldPresenter("NAME")!.View);

            typeof(Control)
                .GetMethod(
                    "OnKeyDown",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(editor, [new KeyEventArgs(Keys.Tab)]);
            await navigationFired.Task.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.Equal("SALARY", block.ActiveFieldName);
        });

    [Fact]
    public Task EnterKey_UsesFormsItemNavigationPipeline() =>
        StaTest.RunAsync(async () =>
        {
            var navigationFired =
                new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var host = new Mock<IBeepFormsHost>();
            host.Setup(instance => instance.IsBlockRegistered("EMP"))
                .Returns(true);
            host.Setup(instance => instance.GetBlockMode("EMP"))
                .Returns(DataBlockMode.Query);
            host.Setup(instance => instance.GetNextItem("EMP", "NAME"))
                .Returns("SALARY");
            host.Setup(instance => instance.FireBlockTriggerAsync(
                    It.IsAny<TriggerType>(),
                    "EMP",
                    It.IsAny<TriggerContext?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(TriggerResult.Success);
            host.Setup(instance => instance.FireKeyTriggerAsync(
                    KeyTriggerType.NextItem,
                    "EMP"))
                .Callback(() => navigationFired.TrySetResult())
                .ReturnsAsync(TriggerResult.Success);
            using var block = new WinFormBlockHost
            {
                BlockName = "EMP",
                AutoGenerateFields = false,
            };
            block.AddFieldPresenter(new WinFormTextBoxFieldPresenter(
                new EntityField { FieldName = "NAME", Fieldtype = "string" }));
            block.AddFieldPresenter(new WinFormNumericFieldPresenter(
                new EntityField { FieldName = "SALARY", Fieldtype = "decimal" }));
            block.Bind(host.Object);
            var editor = Assert.IsAssignableFrom<Control>(
                block.FindFieldPresenter("NAME")!.View);

            typeof(Control)
                .GetMethod(
                    "OnKeyDown",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(editor, [new KeyEventArgs(Keys.Enter)]);
            await navigationFired.Task.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.Equal("SALARY", block.ActiveFieldName);
        });

    [Fact]
    public Task ValidateCurrentRecord_FocusesFirstInvalidItem() =>
        StaTest.RunAsync(() =>
        {
            var invalid = new ItemValidationResult
            {
                BlockName = "EMP",
                ItemName = "SALARY",
                RuleResults =
                [
                    ValidationRuleResult.Failure(
                        "Positive",
                        "SALARY",
                        "Salary must be positive."),
                ],
            };
            var host = new Mock<IBeepFormsHost>();
            host.Setup(instance => instance.IsBlockRegistered("EMP"))
                .Returns(true);
            host.Setup(instance => instance.GetBlockMode("EMP"))
                .Returns(DataBlockMode.Query);
            host.Setup(instance => instance.ValidateBlockRecord(
                    "EMP",
                    It.IsAny<IDictionary<string, object>>(),
                    ValidationTiming.Manual))
                .Returns(new RecordValidationResult
                {
                    BlockName = "EMP",
                    ItemResults =
                    {
                        ["SALARY"] = invalid,
                    },
                });
            using var block = new WinFormBlockHost
            {
                BlockName = "EMP",
                AutoGenerateFields = false,
            };
            block.AddFieldPresenter(new WinFormTextBoxFieldPresenter(
                new EntityField { FieldName = "NAME", Fieldtype = "string" }));
            block.AddFieldPresenter(new WinFormNumericFieldPresenter(
                new EntityField
                {
                    FieldName = "SALARY",
                    Fieldtype = "decimal",
                }));
            block.Bind(host.Object);

            var valid = block.ValidateCurrentRecord();

            Assert.False(valid);
            Assert.Equal("SALARY", block.ActiveFieldName);
        });

    [Fact]
    public Task ApplyLovSelection_MapsReturnValueToInvokingField() =>
        StaTest.RunAsync(async () =>
        {
            var selected = new { Id = 7, Name = "Alice" };
            var definition = LOVDefinition.Create(
                "EMP_LOV",
                "Oracle",
                "EMPLOYEES",
                "Name",
                "Id");
            var host = new Mock<IBeepFormsHost>();
            host.Setup(instance => instance.IsBlockRegistered("ORDERS"))
                .Returns(true);
            host.Setup(instance => instance.GetBlockMode("ORDERS"))
                .Returns(DataBlockMode.Query);
            host.Setup(instance => instance.GetLov(
                    "ORDERS",
                    "EMPLOYEE_ID"))
                .Returns(definition);
            host.Setup(instance => instance.GetLovRelatedFieldValues(
                    definition,
                    selected))
                .Returns(new Dictionary<string, object>
                {
                    ["__RETURN_VALUE__"] = 7,
                });
            host.Setup(instance => instance.SetFieldValue(
                    "ORDERS",
                    "EMPLOYEE_ID",
                    7))
                .Returns(true);
            using var block = new WinFormBlockHost
            {
                BlockName = "ORDERS",
                AutoGenerateFields = false,
            };
            block.Bind(host.Object);

            var applied = await block.ApplyLovSelectionAsync(
                "EMPLOYEE_ID",
                selected);

            Assert.True(applied);
            host.Verify(instance => instance.SetFieldValue(
                "ORDERS",
                "__RETURN_VALUE__",
                It.IsAny<object?>()), Times.Never);
        });

    [Fact]
    public Task ApplyLovSelection_DoesNotPopulateRelatedFieldsWhenDisabled() =>
        StaTest.RunAsync(async () =>
        {
            var selected = new { Id = 7, Name = "Alice" };
            var definition = LOVDefinition.Create(
                "EMP_LOV",
                "Oracle",
                "EMPLOYEES",
                "Name",
                "Id");
            definition.AutoPopulateRelatedFields = false;
            var host = new Mock<IBeepFormsHost>();
            host.Setup(instance => instance.IsBlockRegistered("ORDERS"))
                .Returns(true);
            host.Setup(instance => instance.GetBlockMode("ORDERS"))
                .Returns(DataBlockMode.Query);
            host.Setup(instance => instance.GetLov(
                    "ORDERS",
                    "EMPLOYEE_ID"))
                .Returns(definition);
            host.Setup(instance => instance.GetLovRelatedFieldValues(
                    definition,
                    selected))
                .Returns(new Dictionary<string, object>
                {
                    ["__RETURN_VALUE__"] = 7,
                    ["EMPLOYEE_NAME"] = "Alice",
                });
            host.Setup(instance => instance.SetFieldValue(
                    "ORDERS",
                    "EMPLOYEE_ID",
                    7))
                .Returns(true);
            using var block = new WinFormBlockHost
            {
                BlockName = "ORDERS",
                AutoGenerateFields = false,
            };
            block.Bind(host.Object);

            Assert.True(await block.ApplyLovSelectionAsync(
                "EMPLOYEE_ID",
                selected));
            host.Verify(instance => instance.SetFieldValue(
                "ORDERS",
                "EMPLOYEE_NAME",
                It.IsAny<object?>()), Times.Never);
        });

    [Fact]
    public Task ExecuteFormsCommand_CommitFiresTriggerBeforeSave() =>
        StaTest.RunAsync(async () =>
        {
            var calls = new List<string>();
            var host = new Mock<IBeepFormsHost>();
            host.Setup(instance => instance.IsBlockRegistered("EMP"))
                .Returns(true);
            host.Setup(instance => instance.GetBlockMode("EMP"))
                .Returns(DataBlockMode.Query);
            host.Setup(instance => instance.FireKeyTriggerAsync(
                    KeyTriggerType.Commit,
                    "EMP"))
                .Callback(() => calls.Add("CommitTrigger"))
                .ReturnsAsync(TriggerResult.Success);
            host.Setup(instance => instance.ValidateBlockRecord(
                    "EMP",
                    It.IsAny<IDictionary<string, object>>(),
                    ValidationTiming.OnCommit))
                .Returns((RecordValidationResult?)null);
            host.Setup(instance => instance.SaveBlockAsync(
                    "EMP",
                    It.IsAny<CancellationToken>()))
                .Callback(() => calls.Add("Save"))
                .ReturnsAsync(true);
            using var block = new WinFormBlockHost
            {
                BlockName = "EMP",
                AutoGenerateFields = false,
            };
            block.Bind(host.Object);

            var executed = await block.ExecuteFormsCommandAsync(
                KeyTriggerType.Commit);

            Assert.True(executed);
            Assert.Equal(["CommitTrigger", "Save"], calls);
        });

    [Fact]
    public Task ExecuteFormsCommand_TriggerFailureBlocksOperation() =>
        StaTest.RunAsync(async () =>
        {
            var host = new Mock<IBeepFormsHost>();
            host.Setup(instance => instance.IsBlockRegistered("EMP"))
                .Returns(true);
            host.Setup(instance => instance.GetBlockMode("EMP"))
                .Returns(DataBlockMode.Query);
            host.Setup(instance => instance.FireKeyTriggerAsync(
                    KeyTriggerType.CreateRecord,
                    "EMP"))
                .ReturnsAsync(TriggerResult.FormTriggerFailure);
            using var block = new WinFormBlockHost
            {
                BlockName = "EMP",
                AutoGenerateFields = false,
            };
            block.Bind(host.Object);

            var executed = await block.ExecuteFormsCommandAsync(
                KeyTriggerType.CreateRecord);

            Assert.False(executed);
            host.Verify(instance => instance.InsertBlockRecordAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()), Times.Never);
        });

    [Fact]
    public Task HandleFormsShortcut_F10ExecutesCommitCommand() =>
        StaTest.RunAsync(async () =>
        {
            var host = new Mock<IBeepFormsHost>();
            host.Setup(instance => instance.IsBlockRegistered("EMP"))
                .Returns(true);
            host.Setup(instance => instance.GetBlockMode("EMP"))
                .Returns(DataBlockMode.Query);
            host.Setup(instance => instance.FireKeyTriggerAsync(
                    KeyTriggerType.Commit,
                    "EMP"))
                .ReturnsAsync(TriggerResult.Success);
            host.Setup(instance => instance.ValidateBlockRecord(
                    "EMP",
                    It.IsAny<IDictionary<string, object>>(),
                    ValidationTiming.OnCommit))
                .Returns((RecordValidationResult?)null);
            host.Setup(instance => instance.SaveBlockAsync(
                    "EMP",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            using var block = new WinFormBlockHost
            {
                BlockName = "EMP",
                AutoGenerateFields = false,
            };
            block.Bind(host.Object);

            var handled = await block.HandleFormsShortcutAsync(Keys.F10);

            Assert.True(handled);
            host.Verify(instance => instance.SaveBlockAsync(
                "EMP",
                It.IsAny<CancellationToken>()), Times.Once);
        });

    [Fact]
    public Task HandleFormsShortcut_F9ShowsAndAppliesLov() =>
        StaTest.RunAsync(async () =>
        {
            var selected = new { Id = 10, Name = "Acme" };
            var lov = LOVDefinition.CreateLookup(
                "CUSTOMERS",
                "MAIN",
                "CUSTOMERS",
                "Id",
                "Name");
            var host = new Mock<IBeepFormsHost>();
            host.Setup(instance => instance.IsBlockRegistered("ORDERS"))
                .Returns(true);
            host.Setup(instance => instance.GetBlockMode("ORDERS"))
                .Returns(DataBlockMode.Query);
            host.Setup(instance => instance.FireKeyTriggerAsync(
                    KeyTriggerType.ListValues,
                    "ORDERS"))
                .ReturnsAsync(TriggerResult.Success);
            host.Setup(instance => instance.HasLov(
                    "ORDERS",
                    "CUSTOMER_ID"))
                .Returns(true);
            host.Setup(instance => instance.GetLov(
                    "ORDERS",
                    "CUSTOMER_ID"))
                .Returns(lov);
            host.Setup(instance => instance.LoadLovDataAsync(
                    "ORDERS",
                    "CUSTOMER_ID",
                    null))
                .ReturnsAsync(LOVResult.Ok([selected]));
            host.Setup(instance => instance.GetLovRelatedFieldValues(
                    lov,
                    selected))
                .Returns(new Dictionary<string, object>
                {
                    ["CUSTOMER_ID"] = 10,
                    ["CUSTOMER_NAME"] = "Acme",
                });
            host.Setup(instance => instance.SetFieldValue(
                    "ORDERS",
                    It.IsAny<string>(),
                    It.IsAny<object?>()))
                .Returns(true);
            using var block = new WinFormBlockHost
            {
                BlockName = "ORDERS",
                AutoGenerateFields = false,
                LovDialogPresenter = dialog =>
                {
                    dialog.AcceptSelection(selected);
                    return DialogResult.OK;
                },
            };
            block.AddFieldPresenter(new WinFormComboFieldPresenter(
                new EntityField
                {
                    FieldName = "CUSTOMER_ID",
                    Fieldtype = "string",
                }));
            block.Bind(host.Object);
            block.FocusField("CUSTOMER_ID");

            var handled = await block.HandleFormsShortcutAsync(Keys.F9);

            Assert.True(handled);
            host.Verify(instance => instance.SetFieldValue(
                "ORDERS",
                "CUSTOMER_ID",
                10), Times.Once);
            host.Verify(instance => instance.SetFieldValue(
                "ORDERS",
                "CUSTOMER_NAME",
                "Acme"), Times.Once);
            var combo = Assert.IsType<WinFormComboFieldPresenter>(
                block.FindFieldPresenter("CUSTOMER_ID"));
            var editor = Assert.IsType<
                TheTechIdea.Beep.Winform.Controls.BeepComboBox>(
                combo.View);
            Assert.Equal(10, combo.Value);
            Assert.Equal("Acme", editor.SelectedText);
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
