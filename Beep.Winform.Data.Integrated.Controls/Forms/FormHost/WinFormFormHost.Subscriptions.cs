using TheTechIdea.Beep.Editor.Forms.Hosts;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;

namespace TheTechIdea.Beep.Winform.Data.Integrated.Forms.FormHost;

public partial class WinFormFormHost
{
    private ITriggerManager? _subscribedTriggers;
    private IMessageQueueManager? _subscribedMessages;
    private ITimerManager? _subscribedTimers;
    private IUnitofWorksManager? _subscribedManager;

    private void AttachManagerEvents(IUnitofWorksManager manager)
    {
        _subscribedManager = manager;
        manager.OnFormMessage += ManagerFormMessage;
        try
        {
            _subscribedTriggers = manager.Triggers;
        }
        catch
        {
            _subscribedTriggers = null;
        }

        if (_subscribedTriggers is not null)
        {
            _subscribedTriggers.TriggerExecuting += ManagerTriggerExecuting;
            _subscribedTriggers.TriggerExecuted += ManagerTriggerExecuted;
            _subscribedTriggers.TriggerRegistered += ManagerTriggerRegistered;
            _subscribedTriggers.TriggerUnregistered += ManagerTriggerUnregistered;
        }

        try
        {
            _subscribedMessages = manager.Messages;
        }
        catch
        {
            _subscribedMessages = null;
        }

        if (_subscribedMessages is not null)
        {
            _subscribedMessages.OnMessage += ManagerMessageRaised;
            _subscribedMessages.OnMessageCleared += ManagerMessageCleared;
        }

        try
        {
            _subscribedTimers = manager.Timers;
        }
        catch
        {
            _subscribedTimers = null;
        }

        if (_subscribedTimers is not null)
        {
            _subscribedTimers.TimerFired += ManagerTimerFired;
        }
    }

    private void DetachManagerEvents(IUnitofWorksManager manager)
    {
        if (_subscribedManager is not null)
        {
            _subscribedManager.OnFormMessage -= ManagerFormMessage;
            _subscribedManager = null;
        }
        if (_subscribedTriggers is not null)
        {
            _subscribedTriggers.TriggerExecuting -= ManagerTriggerExecuting;
            _subscribedTriggers.TriggerExecuted -= ManagerTriggerExecuted;
            _subscribedTriggers.TriggerRegistered -= ManagerTriggerRegistered;
            _subscribedTriggers.TriggerUnregistered -= ManagerTriggerUnregistered;
            _subscribedTriggers = null;
        }

        if (_subscribedMessages is not null)
        {
            _subscribedMessages.OnMessage -= ManagerMessageRaised;
            _subscribedMessages.OnMessageCleared -= ManagerMessageCleared;
            _subscribedMessages = null;
        }

        if (_subscribedTimers is not null)
        {
            _subscribedTimers.TimerFired -= ManagerTimerFired;
            _subscribedTimers = null;
        }
    }

    private void ManagerTriggerExecuting(object? sender, TriggerExecutingEventArgs args) =>
        RunOnUi(() => FindTriggerBlock(args.Context?.BlockName)?.RaiseTriggerExecuting(args));

    private void ManagerTriggerExecuted(object? sender, TriggerExecutedEventArgs args) =>
        RunOnUi(() => FindTriggerBlock(args.Context?.BlockName)?.RaiseTriggerExecuted(args));

    private void ManagerTriggerRegistered(object? sender, TriggerRegisteredEventArgs args) =>
        RunOnUi(() => FindTriggerBlock(args.BlockName)?.RaiseTriggerRegistered(args));

    private void ManagerTriggerUnregistered(object? sender, TriggerUnregisteredEventArgs args) =>
        RunOnUi(() => FindTriggerBlock(args.BlockName)?.RaiseTriggerUnregistered(args));

    private IBlockView? FindTriggerBlock(string? blockName)
    {
        var name = string.IsNullOrWhiteSpace(blockName)
            ? _activeBlockName
            : blockName.Trim();
        return name is not null && _blocks.TryGetValue(name, out var block)
            ? block
            : null;
    }

    private void ManagerMessageRaised(object? sender, BlockMessageEventArgs args) =>
        RelayMessage(args, MessageRaised);

    private void ManagerMessageCleared(object? sender, BlockMessageEventArgs args) =>
        RelayMessage(args, MessageCleared);

    private void ManagerTimerFired(object? sender, TimerFiredEventArgs args) =>
        RunOnUi(() => TimerFired?.Invoke(
            this,
            new FormsHostTimerEventArgs(
                args.TimerName,
                args.FireCount,
                args.FiredAt)));

    private void ManagerFormMessage(object? sender, FormMessageEventArgs args)
    {
        if (args.Message is null)
        {
            return;
        }

        RunOnUi(() => FormMessageReceived?.Invoke(
            this,
            new FormsHostFormMessageEventArgs(args.Message)));
    }

    private void RelayMessage(
        BlockMessageEventArgs args,
        EventHandler<FormsHostMessageEventArgs>? handler)
    {
        var message = args.Message;
        if (message is null)
        {
            return;
        }

        RunOnUi(() => handler?.Invoke(
            this,
            new FormsHostMessageEventArgs(
                message.BlockName ?? string.Empty,
                message.Text ?? string.Empty,
                message.Level)));
    }
}
