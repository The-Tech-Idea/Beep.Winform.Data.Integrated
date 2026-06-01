using System.Collections.Generic;
using System.Threading.Tasks;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Services
{
    public sealed class BeepFormsCommandRouter : IBeepFormsCommandRouter
    {
        public IUnitofWorksManager? FormsManager { get; set; }

        public Task<bool> SwitchToBlockAsync(string blockName)
        {
            if (FormsManager == null || string.IsNullOrWhiteSpace(blockName))
            {
                return Task.FromResult(false);
            }

            return FormsManager.SwitchToBlockAsync(blockName);
        }

        public Task<bool> EnterQueryAsync(string blockName)
        {
            if (FormsManager == null || string.IsNullOrWhiteSpace(blockName))
            {
                return Task.FromResult(false);
            }

            return FormsManager.EnterQueryAsync(blockName);
        }

        public Task<bool> ExecuteQueryAsync(string blockName, List<AppFilter>? filters = null)
        {
            if (FormsManager == null || string.IsNullOrWhiteSpace(blockName))
            {
                return Task.FromResult(false);
            }

            return FormsManager.ExecuteQueryAsync(blockName, filters);
        }

        public async Task<IErrorsInfo> CommitFormAsync()
        {
            if (FormsManager == null)
            {
                return new ErrorsInfo { Flag = Errors.Failed, Message = "FormsManager is not assigned." };
            }

            return await FormsManager.CommitFormAsync().ConfigureAwait(false);
        }

        public async Task<IErrorsInfo> RollbackFormAsync()
        {
            if (FormsManager == null)
            {
                return new ErrorsInfo { Flag = Errors.Failed, Message = "FormsManager is not assigned." };
            }

            return await FormsManager.RollbackFormAsync().ConfigureAwait(false);
        }

        public Task<bool> FirstRecordAsync(string blockName)
        {
            if (FormsManager == null || string.IsNullOrWhiteSpace(blockName))
            {
                return Task.FromResult(false);
            }

            return FormsManager.FirstRecordAsync(blockName);
        }

        public Task<bool> PreviousRecordAsync(string blockName)
        {
            if (FormsManager == null || string.IsNullOrWhiteSpace(blockName))
            {
                return Task.FromResult(false);
            }

            return FormsManager.PreviousRecordAsync(blockName);
        }

        public Task<bool> NextRecordAsync(string blockName)
        {
            if (FormsManager == null || string.IsNullOrWhiteSpace(blockName))
            {
                return Task.FromResult(false);
            }

            return FormsManager.NextRecordAsync(blockName);
        }

        public Task<bool> LastRecordAsync(string blockName)
        {
            if (FormsManager == null || string.IsNullOrWhiteSpace(blockName))
            {
                return Task.FromResult(false);
            }

            return FormsManager.LastRecordAsync(blockName);
        }
    }
}