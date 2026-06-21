using Moq;
using System.Reflection;
using System.Windows.Forms;
using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;
using Xunit;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Tests.Forms;

public class WinFormFormHostLifecycleTests
{
    [Fact]
    public Task ReplacingManager_UnbindsOldBlocksThenBindsAndSyncsEligibleBlocks() =>
        StaTest.RunAsync(() =>
        {
            var oldManager = new Mock<IUnitofWorksManager>();
            oldManager.Setup(m => m.BlockExists("EMP")).Returns(true);
            var newManager = new Mock<IUnitofWorksManager>();
            newManager.Setup(m => m.BlockExists("EMP")).Returns(true);
            var block = CreateBlock("EMP", isBound: true);
            using var host = new WinFormFormHost { FormsManager = oldManager.Object };
            host.RegisterBlock(block.Object);
            block.Invocations.Clear();

            host.FormsManager = newManager.Object;

            block.Verify(b => b.Unbind(), Times.Once);
            block.Verify(b => b.Bind(host), Times.Once);
            block.Verify(b => b.SyncFromManager(), Times.Once);
            Assert.Same(newManager.Object, host.FormsManager);
            var lifecycleInvocations = block.Invocations
                .Where(invocation => !invocation.Method.IsSpecialName)
                .ToList();
            Assert.Collection(
                lifecycleInvocations,
                invocation => Assert.Equal(nameof(IBlockView.Unbind), invocation.Method.Name),
                invocation => Assert.Equal(nameof(IBlockView.Bind), invocation.Method.Name),
                invocation => Assert.Equal(nameof(IBlockView.SyncFromManager), invocation.Method.Name));
        });

    [Fact]
    public Task AssigningNullManager_UnbindsBoundBlocks() => StaTest.RunAsync(() =>
    {
        var manager = new Mock<IUnitofWorksManager>();
        manager.Setup(m => m.BlockExists("EMP")).Returns(true);
        var block = CreateBlock("EMP", isBound: true);
        using var host = new WinFormFormHost { FormsManager = manager.Object };
        host.RegisterBlock(block.Object);
        block.Invocations.Clear();

        host.FormsManager = null;

        block.Verify(b => b.Unbind(), Times.Once);
        block.Verify(b => b.Bind(It.IsAny<IBeepFormsHost>()), Times.Never);
        block.Verify(b => b.SyncFromManager(), Times.Never);
        Assert.Null(host.FormsManager);
        Assert.True(host.IsBlockRegistered("EMP"));
    });

    [Fact]
    public Task Dispose_UnbindsRegisteredBlocksOnlyOnce() => StaTest.RunAsync(() =>
    {
        var block = CreateBlock("EMP", isBound: true);
        var host = new WinFormFormHost();
        host.RegisterBlock(block.Object);

        host.Dispose();
        host.Dispose();

        block.Verify(b => b.Unbind(), Times.Once);
    });

    [Fact]
    public Task RegisterBlock_BlockExistsThrows_LeavesBlockUnregistered() => StaTest.RunAsync(() =>
    {
        var manager = new Mock<IUnitofWorksManager>();
        manager.Setup(m => m.BlockExists("EMP"))
            .Throws(new InvalidOperationException("engine failure"));
        var block = CreateBlock("EMP");
        using var host = new WinFormFormHost { FormsManager = manager.Object };

        Assert.Throws<InvalidOperationException>(() => host.RegisterBlock(block.Object));

        Assert.False(host.IsBlockRegistered("EMP"));
        Assert.Null(host.ActiveBlockName);
        block.Verify(b => b.Bind(It.IsAny<IBeepFormsHost>()), Times.Never);
    });

    [Fact]
    public Task RegisterBlock_BindThrows_LeavesBlockUnregistered() => StaTest.RunAsync(() =>
    {
        var manager = new Mock<IUnitofWorksManager>();
        manager.Setup(m => m.BlockExists("EMP")).Returns(true);
        var block = CreateBlock("EMP");
        block.Setup(b => b.Bind(It.IsAny<IBeepFormsHost>()))
            .Throws(new InvalidOperationException("bind failure"));
        using var host = new WinFormFormHost { FormsManager = manager.Object };

        Assert.Throws<InvalidOperationException>(() => host.RegisterBlock(block.Object));

        Assert.False(host.IsBlockRegistered("EMP"));
        Assert.Null(host.ActiveBlockName);
    });

    [Fact]
    public Task RegisterBlock_BindPartiallyBindsThenThrows_AttemptsUnbindAndRollsBack() =>
        StaTest.RunAsync(() =>
        {
            var manager = new Mock<IUnitofWorksManager>();
            manager.Setup(m => m.BlockExists("EMP")).Returns(true);
            var binding = CreateStatefulBlock("EMP");
            binding.Block.Setup(b => b.Bind(It.IsAny<IBeepFormsHost>()))
                .Callback(() =>
                {
                    binding.SetBound(true);
                    throw new InvalidOperationException("partial bind failure");
                });
            using var host = new WinFormFormHost { FormsManager = manager.Object };

            var error = Assert.Throws<InvalidOperationException>(
                () => host.RegisterBlock(binding.Block.Object));

            Assert.Equal("partial bind failure", error.Message);
            Assert.False(host.IsBlockRegistered("EMP"));
            Assert.Null(host.ActiveBlockName);
            Assert.False(binding.IsBound());
            binding.Block.Verify(b => b.Unbind(), Times.Once);
        });

    [Fact]
    public Task RegisterBlock_BindAndCleanupUnbindThrow_PreservesBindFailureAndRollsBackState() =>
        StaTest.RunAsync(() =>
        {
            var manager = new Mock<IUnitofWorksManager>();
            manager.Setup(m => m.BlockExists("EMP")).Returns(true);
            var block = CreateBlock("EMP");
            block.Setup(b => b.Bind(It.IsAny<IBeepFormsHost>()))
                .Throws(new InvalidOperationException("bind failure"));
            block.Setup(b => b.Unbind())
                .Throws(new ApplicationException("cleanup failure"));
            using var host = new WinFormFormHost { FormsManager = manager.Object };

            var error = Assert.Throws<InvalidOperationException>(
                () => host.RegisterBlock(block.Object));

            Assert.Equal("bind failure", error.Message);
            Assert.False(host.IsBlockRegistered("EMP"));
            Assert.Null(host.ActiveBlockName);
            block.Verify(b => b.Unbind(), Times.Once);
            Assert.Contains(
                error.Data.Values.Cast<object>(),
                value => value is ApplicationException cleanup &&
                         cleanup.Message == "cleanup failure");
        });

    [Fact]
    public Task RegisterBlock_ActiveBlockChangedThrows_PropagatesAndRestoresPreCallState() =>
        StaTest.RunAsync(() =>
        {
            var manager = new Mock<IUnitofWorksManager>();
            manager.Setup(m => m.BlockExists("EMP")).Returns(true);
            var binding = CreateStatefulBlock("EMP");
            using var host = new WinFormFormHost { FormsManager = manager.Object };
            host.ActiveBlockChanged += (_, _) =>
                throw new InvalidOperationException("event failure");

            var error = Assert.Throws<InvalidOperationException>(
                () => host.RegisterBlock(binding.Block.Object));

            Assert.Equal("event failure", error.Message);
            Assert.False(host.IsBlockRegistered("EMP"));
            Assert.Null(host.ActiveBlockName);
            Assert.False(binding.IsBound());
            binding.Block.Verify(b => b.Bind(host), Times.Once);
            binding.Block.Verify(b => b.Unbind(), Times.Once);
        });

    [Fact]
    public Task ReplaceManager_BlockExistsThrows_RestoresOldManagerAndBindingThenAllowsRetry() =>
        StaTest.RunAsync(() =>
        {
            var oldManager = new Mock<IUnitofWorksManager>();
            oldManager.Setup(m => m.BlockExists("EMP")).Returns(true);
            var newManager = new Mock<IUnitofWorksManager>();
            newManager.SetupSequence(m => m.BlockExists("EMP"))
                .Throws(new InvalidOperationException("exists failure"))
                .Returns(true);
            var binding = CreateStatefulBlock("EMP");
            using var host = new WinFormFormHost { FormsManager = oldManager.Object };
            host.RegisterBlock(binding.Block.Object);
            binding.Block.Invocations.Clear();

            var error = Assert.Throws<InvalidOperationException>(
                () => host.FormsManager = newManager.Object);

            Assert.Equal("exists failure", error.Message);
            Assert.Same(oldManager.Object, host.FormsManager);
            Assert.True(binding.IsBound());

            host.FormsManager = newManager.Object;

            Assert.Same(newManager.Object, host.FormsManager);
            Assert.True(binding.IsBound());
        });

    [Fact]
    public Task ReplaceManager_BindThrows_RestoresOldManagerAndBindingThenAllowsRetry() =>
        StaTest.RunAsync(() =>
        {
            var oldManager = new Mock<IUnitofWorksManager>();
            oldManager.Setup(m => m.BlockExists("EMP")).Returns(true);
            var newManager = new Mock<IUnitofWorksManager>();
            newManager.Setup(m => m.BlockExists("EMP")).Returns(true);
            var binding = CreateStatefulBlock("EMP");
            using var host = new WinFormFormHost { FormsManager = oldManager.Object };
            host.RegisterBlock(binding.Block.Object);
            var failNewBind = true;
            binding.Block.Setup(b => b.Bind(host)).Callback(() =>
            {
                if (ReferenceEquals(host.FormsManager, newManager.Object) && failNewBind)
                {
                    failNewBind = false;
                    throw new InvalidOperationException("bind failure");
                }

                binding.SetBound(true);
            });

            var error = Assert.Throws<InvalidOperationException>(
                () => host.FormsManager = newManager.Object);

            Assert.Equal("bind failure", error.Message);
            Assert.Same(oldManager.Object, host.FormsManager);
            Assert.True(binding.IsBound());

            host.FormsManager = newManager.Object;

            Assert.Same(newManager.Object, host.FormsManager);
            Assert.True(binding.IsBound());
        });

    [Fact]
    public Task ReplaceManager_PartialBindReportsUnbound_StillCleansAttemptBeforeRestoringOldManager() =>
        StaTest.RunAsync(() =>
        {
            var oldManager = new Mock<IUnitofWorksManager>();
            oldManager.Setup(m => m.BlockExists("EMP")).Returns(true);
            var newManager = new Mock<IUnitofWorksManager>();
            newManager.Setup(m => m.BlockExists("EMP")).Returns(true);
            var binding = CreateStatefulBlock("EMP");
            using var host = new WinFormFormHost { FormsManager = oldManager.Object };
            host.RegisterBlock(binding.Block.Object);
            var partialBindingState = false;
            var failNewBind = true;
            binding.Block.Setup(b => b.Bind(host)).Callback(() =>
            {
                if (ReferenceEquals(host.FormsManager, newManager.Object) && failNewBind)
                {
                    failNewBind = false;
                    partialBindingState = true;
                    binding.SetBound(false);
                    throw new InvalidOperationException("partial bind failure");
                }

                binding.SetBound(true);
            });
            binding.Block.Setup(b => b.Unbind()).Callback(() =>
            {
                partialBindingState = false;
                binding.SetBound(false);
            });

            var error = Assert.Throws<InvalidOperationException>(
                () => host.FormsManager = newManager.Object);

            Assert.Equal("partial bind failure", error.Message);
            Assert.False(partialBindingState);
            Assert.Same(oldManager.Object, host.FormsManager);
            Assert.True(binding.IsBound());

            host.FormsManager = newManager.Object;

            Assert.Same(newManager.Object, host.FormsManager);
            Assert.True(binding.IsBound());
        });

    [Fact]
    public Task ReplaceManager_SyncThrows_RestoresOldManagerAndBindingThenAllowsRetry() =>
        StaTest.RunAsync(() =>
        {
            var oldManager = new Mock<IUnitofWorksManager>();
            oldManager.Setup(m => m.BlockExists("EMP")).Returns(true);
            var newManager = new Mock<IUnitofWorksManager>();
            newManager.Setup(m => m.BlockExists("EMP")).Returns(true);
            var binding = CreateStatefulBlock("EMP");
            using var host = new WinFormFormHost { FormsManager = oldManager.Object };
            host.RegisterBlock(binding.Block.Object);
            var failNewSync = true;
            binding.Block.Setup(b => b.SyncFromManager()).Callback(() =>
            {
                if (ReferenceEquals(host.FormsManager, newManager.Object) && failNewSync)
                {
                    failNewSync = false;
                    throw new InvalidOperationException("sync failure");
                }
            });

            var error = Assert.Throws<InvalidOperationException>(
                () => host.FormsManager = newManager.Object);

            Assert.Equal("sync failure", error.Message);
            Assert.Same(oldManager.Object, host.FormsManager);
            Assert.True(binding.IsBound());

            host.FormsManager = newManager.Object;

            Assert.Same(newManager.Object, host.FormsManager);
            Assert.True(binding.IsBound());
        });

    [Fact]
    public Task UnregisterBlock_UnbindThrows_LeavesRegistryAndActiveBlockUnchanged() =>
        StaTest.RunAsync(() =>
        {
            var block = CreateBlock("EMP", isBound: true);
            using var host = new WinFormFormHost();
            host.RegisterBlock(block.Object);
            block.Setup(b => b.Unbind())
                .Throws(new InvalidOperationException("unbind failure"));

            Assert.Throws<InvalidOperationException>(() => host.UnregisterBlock("EMP"));

            Assert.True(host.IsBlockRegistered("EMP"));
            Assert.Equal("EMP", host.ActiveBlockName);
        });

    [Fact]
    public Task UnregisterBlock_ActiveBlockChangedThrows_RestoresRegistryBindingAndActiveBlock() =>
        StaTest.RunAsync(() =>
        {
            var manager = new Mock<IUnitofWorksManager>();
            manager.Setup(m => m.BlockExists("EMP")).Returns(true);
            var binding = CreateStatefulBlock("EMP");
            using var host = new WinFormFormHost { FormsManager = manager.Object };
            host.RegisterBlock(binding.Block.Object);
            host.ActiveBlockChanged += (_, _) =>
                throw new InvalidOperationException("event failure");

            var error = Assert.Throws<InvalidOperationException>(
                () => host.UnregisterBlock("EMP"));

            Assert.Equal("event failure", error.Message);
            Assert.True(host.IsBlockRegistered("EMP"));
            Assert.Equal("EMP", host.ActiveBlockName);
            Assert.True(binding.IsBound());
        });

    [Fact]
    public Task TrySetActiveBlock_ActiveBlockChangedThrows_RestoresPreviousActiveBlock() =>
        StaTest.RunAsync(() =>
        {
            using var host = new WinFormFormHost();
            host.RegisterBlock(CreateBlock("EMP").Object);
            host.RegisterBlock(CreateBlock("DEPT").Object);
            host.ActiveBlockChanged += (_, _) =>
                throw new InvalidOperationException("event failure");

            var error = Assert.Throws<InvalidOperationException>(
                () => host.TrySetActiveBlock("DEPT"));

            Assert.Equal("event failure", error.Message);
            Assert.Equal("EMP", host.ActiveBlockName);
            Assert.True(host.IsBlockRegistered("EMP"));
            Assert.True(host.IsBlockRegistered("DEPT"));
        });

    [Fact]
    public Task RunOnUi_BeforeHandleCreation_ExecutesWithoutThrowing() => StaTest.RunAsync(() =>
    {
        using var host = new WinFormFormHost();
        var invoked = false;

        InvokeRunOnUi(host, () => invoked = true);

        Assert.True(invoked);
        Assert.False(host.IsHandleCreated);
    });

    [Fact]
    public Task RunOnUi_AfterDisposal_DoesNotExecuteOrThrow() => StaTest.RunAsync(() =>
    {
        var host = new WinFormFormHost();
        host.Dispose();
        var invoked = false;

        InvokeRunOnUi(host, () => invoked = true);

        Assert.False(invoked);
    });

    [Fact]
    public Task RegistryVisualLifecycle_FromWorkerThread_RunsOnControlThread() =>
        StaTest.RunAsync(() =>
        {
            var manager = new Mock<IUnitofWorksManager>();
            manager.Setup(m => m.BlockExists("EMP")).Returns(true);
            using var host = new WinFormFormHost { FormsManager = manager.Object };
            host.CreateControl();
            var uiThreadId = Environment.CurrentManagedThreadId;
            var bindThreadId = -1;
            var unbindThreadId = -1;
            var eventThreadIds = new List<int>();
            var block = CreateBlock("EMP", isBound: true);
            block.Setup(b => b.Bind(host))
                .Callback(() => bindThreadId = Environment.CurrentManagedThreadId);
            block.Setup(b => b.Unbind())
                .Callback(() => unbindThreadId = Environment.CurrentManagedThreadId);
            host.ActiveBlockChanged += (_, _) =>
                eventThreadIds.Add(Environment.CurrentManagedThreadId);

            var register = Task.Run(() => host.RegisterBlock(block.Object));
            PumpUntil(register);
            var unregister = Task.Run(() => host.UnregisterBlock("EMP"));
            PumpUntil(unregister);

            Assert.Equal(uiThreadId, bindThreadId);
            Assert.Equal(uiThreadId, unbindThreadId);
            Assert.Equal([uiThreadId, uiThreadId], eventThreadIds);
        });

    [Fact]
    public Task RunOnUi_ActionThrowsObjectDisposedExceptionFromWorker_PropagatesActionException() =>
        StaTest.RunAsync(() =>
        {
            using var host = new WinFormFormHost();
            host.CreateControl();
            host.RegisterBlock(CreateBlock("EMP").Object);
            host.RegisterBlock(CreateBlock("DEPT").Object);
            host.ActiveBlockChanged += (_, _) =>
                throw new ObjectDisposedException("action-body");

            var operation = Task.Run(() => host.TrySetActiveBlock("DEPT"));
            var error = Assert.Throws<ObjectDisposedException>(() => PumpUntil(operation));

            Assert.Equal("action-body", error.ObjectName);
            Assert.Equal("EMP", host.ActiveBlockName);
        });

    private static Mock<IBlockView> CreateBlock(string name, bool isBound = false)
    {
        var block = new Mock<IBlockView>();
        block.SetupGet(b => b.BlockName).Returns(name);
        block.SetupGet(b => b.View).Returns(new Panel());
        block.SetupGet(b => b.IsBound).Returns(isBound);
        return block;
    }

    private static StatefulBlock CreateStatefulBlock(string name)
    {
        var isBound = false;
        var block = new Mock<IBlockView>();
        block.SetupGet(b => b.BlockName).Returns(name);
        block.SetupGet(b => b.View).Returns(new Panel());
        block.SetupGet(b => b.IsBound).Returns(() => isBound);
        block.Setup(b => b.Bind(It.IsAny<IBeepFormsHost>()))
            .Callback(() => isBound = true);
        block.Setup(b => b.Unbind())
            .Callback(() => isBound = false);
        return new StatefulBlock(
            block,
            () => isBound,
            value => isBound = value);
    }

    private sealed record StatefulBlock(
        Mock<IBlockView> Block,
        Func<bool> IsBound,
        Action<bool> SetBound);

    private static void InvokeRunOnUi(WinFormFormHost host, Action action)
    {
        var method = typeof(WinFormFormHost).GetMethod(
            "RunOnUi",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        method!.Invoke(host, [action]);
    }

    private static void PumpUntil(Task task)
    {
        var timeout = DateTime.UtcNow.AddSeconds(5);
        while (!task.IsCompleted && DateTime.UtcNow < timeout)
        {
            Application.DoEvents();
            Thread.Sleep(1);
        }

        task.GetAwaiter().GetResult();
    }
}
