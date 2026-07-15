using Moq;
using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Editor.UOWManager.Models;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;
using Xunit;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Tests.Forms;

public class WinFormFormHostAdvancedTests
{
    [Fact]
    public Task TriggerEvent_RelaysOnlyToMatchingBlock() => StaTest.RunAsync(() =>
    {
        var triggers = new Mock<ITriggerManager>();
        var messages = new Mock<IMessageQueueManager>();
        var manager = CreateManager(triggers, messages);
        manager.Setup(m => m.BlockExists("ORDERS")).Returns(true);
        manager.Setup(m => m.BlockExists("CUSTOMERS")).Returns(true);

        using var host = new WinFormFormHost { FormsManager = manager.Object };
        var orders = CreateBlock("ORDERS");
        var customers = CreateBlock("CUSTOMERS");
        host.RegisterBlock(orders.Object);
        host.RegisterBlock(customers.Object);

        var args = new TriggerExecutingEventArgs
        {
            Trigger = new TriggerDefinition(),
            Context = TriggerContext.ForBlock(
                TriggerType.WhenValidateRecord,
                "ORDERS")
        };
        triggers.Raise(x => x.TriggerExecuting += null, args);

        orders.Verify(x => x.RaiseTriggerExecuting(args), Times.Once);
        customers.Verify(x => x.RaiseTriggerExecuting(args), Times.Never);
    });

    [Fact]
    public Task QueryByExample_UsesEngineQueryBuilderAndRefreshesBlock() =>
        StaTest.RunAsync(async () =>
        {
            var triggers = new Mock<ITriggerManager>();
            var messages = new Mock<IMessageQueueManager>();
            var queryBuilder = new Mock<IQueryBuilderManager>();
            var manager = CreateManager(triggers, messages);
            manager.SetupGet(m => m.QueryBuilder).Returns(queryBuilder.Object);
            manager.Setup(m => m.BlockExists("ORDERS")).Returns(true);
            manager.Setup(m => m.GetDetailBlocks("ORDERS")).Returns([]);
            manager.Setup(m => m.ExecuteQueryAsync(
                    "ORDERS",
                    It.Is<List<AppFilter>>(filters => filters.Count == 1)))
                .ReturnsAsync(true);
            queryBuilder.Setup(x => x.BuildFilters(
                    "ORDERS",
                    It.Is<Dictionary<string, object>>(values =>
                        Equals(values["STATUS"], "OPEN"))))
                .Returns([new AppFilter { FieldName = "STATUS", FilterValue = "OPEN" }]);

            using var host = new WinFormFormHost { FormsManager = manager.Object };
            var block = CreateBlock("ORDERS");
            host.RegisterBlock(block.Object);

            var result = await host.ExecuteQueryByExampleAsync(
                "ORDERS",
                new Dictionary<string, QueryCriterion>
                {
                    ["STATUS"] = new("OPEN", QueryOperator.Equals)
                });

            Assert.True(result);
            queryBuilder.Verify(x => x.SetQueryOperator(
                "ORDERS", "STATUS", QueryOperator.Equals), Times.Once);
            block.Verify(x => x.SyncFromManager(), Times.Once);
        });

    [Fact]
    public Task MessageEvent_RelaysEngineMessage() => StaTest.RunAsync(() =>
    {
        var triggers = new Mock<ITriggerManager>();
        var messages = new Mock<IMessageQueueManager>();
        var manager = CreateManager(triggers, messages);
        using var host = new WinFormFormHost { FormsManager = manager.Object };
        FormsHostMessageEventArgs? observed = null;
        host.MessageRaised += (_, args) => observed = args;

        messages.Raise(
            x => x.OnMessage += null,
            new BlockMessageEventArgs
            {
                Message = new BlockMessage
                {
                    BlockName = "ORDERS",
                    Text = "Record is locked",
                    Level = MessageLevel.Warning
                }
            });

        Assert.NotNull(observed);
        Assert.Equal("ORDERS", observed!.BlockName);
        Assert.Equal("Record is locked", observed.Message);
        Assert.Equal(MessageLevel.Warning, observed.Level);
    });

    [Fact]
    public Task AlertAndMessageCommands_DelegateToEngine() => StaTest.RunAsync(async () =>
    {
        var triggers = new Mock<ITriggerManager>();
        var messages = new Mock<IMessageQueueManager>();
        var manager = CreateManager(triggers, messages);
        manager.Setup(m => m.ShowAlertAsync(
                "Delete",
                "Delete record?",
                AlertStyle.Question,
                "Yes",
                "No",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(AlertResult.Button1);
        using var host = new WinFormFormHost { FormsManager = manager.Object };

        host.SetMessage("Saved", MessageLevel.Success);
        host.ClearMessage();
        var result = await host.ShowAlertAsync(
            "Delete", "Delete record?", AlertStyle.Question, "Yes", "No");

        manager.Verify(m => m.SetMessage("Saved", MessageLevel.Success), Times.Once);
        manager.Verify(m => m.ClearMessage(), Times.Once);
        Assert.Equal(AlertResult.Button1, result);
    });

    [Fact]
    public Task LocksAndSavepoints_DelegateAndRollbackRefreshesBlock() =>
        StaTest.RunAsync(async () =>
        {
            var triggers = new Mock<ITriggerManager>();
            var messages = new Mock<IMessageQueueManager>();
            var locking = new Mock<ILockManager>();
            var savepoints = new Mock<ISavepointManager>();
            var manager = CreateManager(triggers, messages);
            manager.SetupGet(m => m.Locking).Returns(locking.Object);
            manager.SetupGet(m => m.Savepoints).Returns(savepoints.Object);
            manager.Setup(m => m.BlockExists("ORDERS")).Returns(true);
            manager.Setup(m => m.GetDetailBlocks("ORDERS")).Returns([]);
            locking.Setup(x => x.LockCurrentRecordAsync(
                    "ORDERS", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            savepoints.Setup(x => x.CreateSavepoint("ORDERS", "before-edit"))
                .Returns("before-edit");
            savepoints.Setup(x => x.RollbackToSavepointAsync(
                    "ORDERS", "before-edit", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            using var host = new WinFormFormHost { FormsManager = manager.Object };
            var block = CreateBlock("ORDERS");
            host.RegisterBlock(block.Object);

            Assert.True(await host.LockCurrentRecordAsync("ORDERS"));
            Assert.Equal("before-edit", host.CreateSavepoint("ORDERS", "before-edit"));
            Assert.True(await host.RollbackToSavepointAsync("ORDERS", "before-edit"));

            block.Verify(x => x.SyncFromManager(), Times.Once);
        });

    [Fact]
    public Task HistoryAndBookmarks_DelegateAndNavigationRefreshesBlock() =>
        StaTest.RunAsync(async () =>
        {
            var triggers = new Mock<ITriggerManager>();
            var messages = new Mock<IMessageQueueManager>();
            var manager = CreateManager(triggers, messages);
            manager.Setup(m => m.BlockExists("ORDERS")).Returns(true);
            manager.Setup(m => m.GetDetailBlocks("ORDERS")).Returns([]);
            manager.Setup(m => m.NavigateBackAsync("ORDERS")).ReturnsAsync(true);
            manager.Setup(m => m.CanNavigateBack("ORDERS")).Returns(true);
            using var host = new WinFormFormHost { FormsManager = manager.Object };
            var block = CreateBlock("ORDERS");
            host.RegisterBlock(block.Object);

            host.SetBookmark("ORDERS", "review");
            Assert.True(host.CanNavigateBack("ORDERS"));
            Assert.True(await host.NavigateBackAsync("ORDERS"));
            host.RemoveBookmark("ORDERS", "review");

            manager.Verify(m => m.SetBlockBookmark("ORDERS", "review"), Times.Once);
            manager.Verify(m => m.RemoveBlockBookmark("ORDERS", "review"), Times.Once);
            block.Verify(x => x.SyncFromManager(), Times.Once);
        });

    [Fact]
    public Task TimersAndSequences_UseEngineProvidersAndRelayTimerEvent() =>
        StaTest.RunAsync(() =>
        {
            var triggers = new Mock<ITriggerManager>();
            var messages = new Mock<IMessageQueueManager>();
            var timers = new Mock<ITimerManager>();
            var sequences = new Mock<ISequenceProvider>();
            var manager = CreateManager(triggers, messages);
            manager.SetupGet(m => m.Timers).Returns(timers.Object);
            manager.SetupGet(m => m.Sequences).Returns(sequences.Object);
            timers.Setup(x => x.CreateTimer(
                    "refresh", TimeSpan.FromSeconds(10), true))
                .Returns(new TimerDefinition
                {
                    TimerName = "refresh",
                    Interval = TimeSpan.FromSeconds(10),
                    Repeating = true
                });
            sequences.Setup(x => x.GetNextSequence("ORDER_SEQ")).Returns(42);
            using var host = new WinFormFormHost { FormsManager = manager.Object };
            FormsHostTimerEventArgs? observed = null;
            host.TimerFired += (_, args) => observed = args;

            var timer = host.CreateTimer(
                "refresh", TimeSpan.FromSeconds(10), repeating: true);
            var next = host.GetNextSequence("ORDER_SEQ");
            timers.Raise(x => x.TimerFired += null, new TimerFiredEventArgs
            {
                TimerName = "refresh",
                FireCount = 1,
                FiredAt = DateTime.UtcNow
            });

            Assert.Equal("refresh", timer.TimerName);
            Assert.Equal(42, next);
            Assert.Equal("refresh", observed!.TimerName);
        });

    [Fact]
    public Task RecordGroupsAndParameters_DelegateToManager() =>
        StaTest.RunAsync(async () =>
        {
            var triggers = new Mock<ITriggerManager>();
            var messages = new Mock<IMessageQueueManager>();
            var manager = CreateManager(triggers, messages);
            var group = new RecordGroup("OPEN_ORDERS", "MAIN", "ORDERS");
            var parameters = new ParameterList("CALL_PARAMS");
            manager.Setup(m => m.PopulateRecordGroupAsync(
                    "OPEN_ORDERS", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            manager.Setup(m => m.GetRecordGroup("OPEN_ORDERS")).Returns(group);
            manager.Setup(m => m.CreateParameterList("CALL_PARAMS")).Returns(parameters);
            manager.Setup(m => m.GetParameter("CALL_PARAMS", "ORDER_ID")).Returns(10);
            using var host = new WinFormFormHost { FormsManager = manager.Object };

            host.CreateRecordGroup("OPEN_ORDERS", "MAIN", "ORDERS");
            Assert.True(await host.PopulateRecordGroupAsync("OPEN_ORDERS"));
            Assert.Same(group, host.GetRecordGroup("OPEN_ORDERS"));
            Assert.Same(parameters, host.CreateParameterList("CALL_PARAMS"));
            host.SetParameter("CALL_PARAMS", "ORDER_ID", 10);
            Assert.Equal(10, host.GetParameter("CALL_PARAMS", "ORDER_ID"));

            manager.Verify(m => m.CreateRecordGroup(
                "OPEN_ORDERS", "MAIN", "ORDERS", null), Times.Once);
            manager.Verify(m => m.AddParameter(
                "CALL_PARAMS", "ORDER_ID", 10), Times.Once);
        });

    [Fact]
    public Task ModelessFormCall_UsesEngineAndWinFormsFactory() =>
        StaTest.RunAsync(async () =>
        {
            var triggers = new Mock<ITriggerManager>();
            var messages = new Mock<IMessageQueueManager>();
            var manager = CreateManager(triggers, messages);
            var factory = new Mock<IWinFormFormsFactory>();
            var parameters = new Dictionary<string, object> { ["ORDER_ID"] = 10 };
            manager.Setup(m => m.OpenFormModelessAsync("ORDER_DETAILS", parameters))
                .ReturnsAsync(true);
            factory.Setup(x => x.ShowModelessAsync(
                    "ORDER_DETAILS",
                    parameters,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            using var host = new WinFormFormHost
            {
                FormsManager = manager.Object,
                FormFactory = factory.Object
            };

            Assert.True(await host.OpenFormModelessAsync(
                "ORDER_DETAILS", parameters));

            factory.Verify(x => x.ShowModelessAsync(
                "ORDER_DETAILS", parameters, It.IsAny<CancellationToken>()), Times.Once);
        });

    [Fact]
    public Task FormMessages_AreRelayedWithoutInterpretation() => StaTest.RunAsync(() =>
    {
        var triggers = new Mock<ITriggerManager>();
        var messages = new Mock<IMessageQueueManager>();
        var manager = CreateManager(triggers, messages);
        using var host = new WinFormFormHost { FormsManager = manager.Object };
        FormsHostFormMessageEventArgs? observed = null;
        host.FormMessageReceived += (_, args) => observed = args;
        var message = new FormMessage
        {
            SenderForm = "ORDERS",
            TargetForm = "ORDER_DETAILS",
            MessageType = "REFRESH",
            Payload = 10
        };

        manager.Raise(x => x.OnFormMessage += null,
            new FormMessageEventArgs { Message = message });

        Assert.Equal("REFRESH", observed!.MessageType);
        Assert.Equal(10, observed.Payload);
        Assert.Same(message, observed.Message);
    });

    [Fact]
    public Task StateRestoreAndUtilities_DelegateAndSynchronizeRegisteredBlocks() =>
        StaTest.RunAsync(async () =>
        {
            var triggers = new Mock<ITriggerManager>();
            var messages = new Mock<IMessageQueueManager>();
            var manager = CreateManager(triggers, messages);
            var snapshot = new FormStateSnapshot { FormName = "ORDERS_FORM" };
            manager.Setup(m => m.SaveFormState()).Returns(snapshot);
            manager.Setup(m => m.RestoreFormStateAsync(
                    snapshot, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            manager.Setup(m => m.GetAllBlockComputedValues("ORDERS"))
                .Returns(new Dictionary<string, object> { ["TOTAL"] = 125m });
            manager.Setup(m => m.BlockExists("ORDERS")).Returns(true);
            using var host = new WinFormFormHost { FormsManager = manager.Object };
            var block = CreateBlock("ORDERS");
            host.RegisterBlock(block.Object);

            Assert.Same(snapshot, host.SaveFormState());
            Assert.True(await host.RestoreFormStateAsync(snapshot));
            Assert.Equal(125m, host.GetComputedValues("ORDERS")["TOTAL"]);
            host.FreezeBlock("ORDERS");
            host.UnfreezeBlock("ORDERS");

            manager.Verify(m => m.FreezeBlock("ORDERS"), Times.Once);
            manager.Verify(m => m.UnfreezeBlock("ORDERS"), Times.Once);
            block.Verify(m => m.SyncFromManager(), Times.Exactly(2));
        });

    [Fact]
    public Task SecurityContextAndPolicies_DelegateAndRefreshRegisteredBlocks() =>
        StaTest.RunAsync(() =>
        {
            var triggers = new Mock<ITriggerManager>();
            var messages = new Mock<IMessageQueueManager>();
            var manager = CreateManager(triggers, messages);
            var context = new SecurityContext { UserName = "reader" };
            var policy = new FieldSecurity
            {
                BlockName = "ORDERS",
                FieldName = "TOTAL",
                Editable = false,
                Masked = true
            };
            manager.Setup(m => m.BlockExists("ORDERS")).Returns(true);
            using var host = new WinFormFormHost { FormsManager = manager.Object };
            var block = CreateBlock("ORDERS");
            host.RegisterBlock(block.Object);

            host.SetSecurityContext(context);
            host.SetFieldSecurity("ORDERS", "TOTAL", policy);

            manager.Verify(m => m.SetSecurityContext(context), Times.Once);
            manager.Verify(m => m.SetFieldSecurity("ORDERS", "TOTAL", policy), Times.Once);
            block.Verify(m => m.SyncFromManager(), Times.Exactly(2));
        });

    [Fact]
    public Task UndoRedoAndDirtyState_DelegateAndRefreshBlocks() =>
        StaTest.RunAsync(async () =>
        {
            var manager = CreateManager(new Mock<ITriggerManager>(), new Mock<IMessageQueueManager>());
            manager.Setup(m => m.BlockExists("ORDERS")).Returns(true);
            manager.Setup(m => m.GetDetailBlocks("ORDERS")).Returns([]);
            manager.Setup(m => m.UndoBlock("ORDERS")).Returns(true);
            manager.Setup(m => m.RedoBlock("ORDERS")).Returns(true);
            manager.Setup(m => m.SaveDirtyBlocksAsync()).ReturnsAsync(true);
            using var host = new WinFormFormHost { FormsManager = manager.Object };
            var block = CreateBlock("ORDERS");
            host.RegisterBlock(block.Object);

            Assert.True(host.UndoBlock("ORDERS"));
            Assert.True(host.RedoBlock("ORDERS"));
            Assert.True(await host.SaveDirtyBlocksAsync());

            block.Verify(m => m.SyncFromManager(), Times.Exactly(3));
        });

    [Fact]
    public Task AuditValidationAndItemProperties_DelegateToEngine() =>
        StaTest.RunAsync(() =>
        {
            var manager = CreateManager(new Mock<ITriggerManager>(), new Mock<IMessageQueueManager>());
            var items = new Mock<IItemPropertyManager>();
            var failures = new[] { "Order total is invalid." };
            manager.SetupGet(m => m.ItemProperties).Returns(items.Object);
            manager.Setup(m => m.ValidateCrossBlock()).Returns(failures);
            items.Setup(x => x.GetItemProperty("ORDERS", "TOTAL", "FORMAT_MASK"))
                .Returns("#,##0.00");
            using var host = new WinFormFormHost { FormsManager = manager.Object };

            host.SetAuditUser("tester");
            host.SetItemProperty("ORDERS", "TOTAL", "FORMAT_MASK", "#,##0.00");

            manager.Verify(m => m.SetAuditUser("tester"), Times.Once);
            items.Verify(x => x.SetItemProperty(
                "ORDERS", "TOTAL", "FORMAT_MASK", "#,##0.00"), Times.Once);
            Assert.Equal("#,##0.00",
                host.GetItemProperty("ORDERS", "TOTAL", "FORMAT_MASK"));
            Assert.Same(failures, host.ValidateCrossBlock());
        });

    private static Mock<IUnitofWorksManager> CreateManager(
        Mock<ITriggerManager> triggers,
        Mock<IMessageQueueManager> messages)
    {
        var manager = new Mock<IUnitofWorksManager>();
        manager.SetupGet(m => m.Triggers).Returns(triggers.Object);
        manager.SetupGet(m => m.Messages).Returns(messages.Object);
        return manager;
    }

    private static Mock<IBlockView> CreateBlock(string name)
    {
        var block = new Mock<IBlockView>();
        block.SetupGet(x => x.BlockName).Returns(name);
        block.SetupGet(x => x.View).Returns(new Panel());
        block.SetupGet(x => x.IsBound).Returns(false);
        return block;
    }
}
