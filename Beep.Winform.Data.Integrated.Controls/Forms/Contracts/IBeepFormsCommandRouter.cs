using System.Collections.Generic;
using System.Threading.Tasks;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor.UOWManager.Interfaces;
using TheTechIdea.Beep.Report;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts
{
    public interface IBeepFormsCommandRouter
    {
        IUnitofWorksManager? FormsManager { get; set; }

        Task<bool> SwitchToBlockAsync(string blockName);
        Task<bool> EnterQueryAsync(string blockName);
        Task<bool> ExecuteQueryAsync(string blockName, List<AppFilter>? filters = null);
        Task<IErrorsInfo> CommitFormAsync();
        Task<IErrorsInfo> RollbackFormAsync();
        Task<bool> FirstRecordAsync(string blockName);
        Task<bool> PreviousRecordAsync(string blockName);
        Task<bool> NextRecordAsync(string blockName);
        Task<bool> LastRecordAsync(string blockName);
    }
}