using System.Reflection;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Services
{
    public sealed class BeepFormsManagerAdapter
    {
        public IUnitofWorksManager? FormsManager { get; private set; }

        public void Attach(IUnitofWorksManager? formsManager)
        {
            FormsManager = formsManager;
        }

        public void Sync(BeepFormsViewState viewState)
        {
            viewState.IsDirty = FormsManager?.IsDirty ?? false;
            viewState.StatusText = FormsManager?.Status ?? string.Empty;
            viewState.ActiveBlockName = FormsManager?.CurrentBlockName;

            SyncRecordPosition(viewState);

            if (TryGetCurrentMessage(viewState.ActiveBlockName, out string message, out BeepFormsMessageSeverity severity))
            {
                viewState.CurrentMessage = message;
                viewState.MessageSeverity = severity;
            }
        }

        private void SyncRecordPosition(BeepFormsViewState viewState)
        {
            viewState.RecordPositionText = string.Empty;

            string? blockName = viewState.ActiveBlockName;
            if (FormsManager == null || string.IsNullOrWhiteSpace(blockName))
                return;

            if (!FormsManager.BlockExists(blockName))
                return;

            try
            {
                var blockInfo = FormsManager.GetBlock(blockName);
                if (blockInfo?.UnitOfWork == null)
                    return;

                object? currentItem = blockInfo.UnitOfWork.CurrentItem;
                if (currentItem == null)
                {
                    viewState.RecordPositionText = "0 records";
                    return;
                }

                try
                {
                    dynamic uow = blockInfo.UnitOfWork;
                    int count = uow.Count;
                    int currentIndex = uow.CurrentIndex;

                    if (count > 0 && currentIndex >= 0)
                        viewState.RecordPositionText = $"{currentIndex + 1}/{count}";
                    else if (count > 0)
                        viewState.RecordPositionText = $"*/{count}";
                    else
                        viewState.RecordPositionText = "0 records";
                }
                catch
                {
                    viewState.RecordPositionText = "1/?";
                }
            }
            catch
            {
                viewState.RecordPositionText = string.Empty;
            }
        }

        public void SyncBlock(IBeepBlockView blockView)
        {
            blockView.SyncFromManager();
        }

        public bool TryGetCurrentMessage(string? activeBlockName, out string message, out BeepFormsMessageSeverity severity)
        {
            message = string.Empty;
            severity = BeepFormsMessageSeverity.None;

            if (FormsManager == null)
            {
                return false;
            }

            string blockName = !string.IsNullOrWhiteSpace(activeBlockName)
                ? activeBlockName
                : FormsManager.CurrentBlockName ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(blockName))
            {
                string blockMessage = FormsManager.Messages.GetCurrentMessage(blockName);
                if (!string.IsNullOrWhiteSpace(blockMessage))
                {
                    message = $"[{blockName}] {blockMessage}";
                    severity = MapMessageLevel(FormsManager.Messages.GetCurrentMessageLevel(blockName));
                    return true;
                }
            }

            object? statusMessage = FormsManager.GetType()
                .GetProperty("CurrentMessage", BindingFlags.Instance | BindingFlags.Public)
                ?.GetValue(FormsManager);

            if (statusMessage == null)
            {
                return false;
            }

            string? statusText = statusMessage.GetType()
                .GetProperty("Text", BindingFlags.Instance | BindingFlags.Public)
                ?.GetValue(statusMessage) as string;

            if (string.IsNullOrWhiteSpace(statusText))
            {
                return false;
            }

            if (statusMessage.GetType()
                    .GetProperty("Level", BindingFlags.Instance | BindingFlags.Public)
                    ?.GetValue(statusMessage) is MessageLevel level)
            {
                severity = MapMessageLevel(level);
            }
            else
            {
                severity = BeepFormsMessageSeverity.Info;
            }

            message = statusText;
            return true;
        }

        private static BeepFormsMessageSeverity MapMessageLevel(MessageLevel level)
        {
            return level switch
            {
                MessageLevel.Success => BeepFormsMessageSeverity.Success,
                MessageLevel.Warning => BeepFormsMessageSeverity.Warning,
                MessageLevel.Error => BeepFormsMessageSeverity.Error,
                _ => BeepFormsMessageSeverity.Info
            };
        }
    }
}