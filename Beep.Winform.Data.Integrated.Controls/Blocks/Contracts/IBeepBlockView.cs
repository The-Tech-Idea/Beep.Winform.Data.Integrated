using System.Collections.Generic;
using System.Windows.Forms;
using TheTechIdea.Beep.Editor.Forms.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Contracts;
using TheTechIdea.Beep.Winform.Controls.Integrated.Forms.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Contracts
{
    public interface IBeepBlockView
    {
        string BlockName { get; set; }
        string ManagerBlockName { get; }
        bool IsBound { get; }
        IBeepFormsHost? FormsHost { get; }
        BeepBlockDefinition? Definition { get; set; }
        BeepBlockViewState ViewState { get; }

        event EventHandler<TriggerExecutingEventArgs>? TriggerExecuting;
        event EventHandler<TriggerExecutedEventArgs>? TriggerExecuted;
        event EventHandler<TriggerRegisteredEventArgs>? TriggerRegistered;
        event EventHandler<TriggerUnregisteredEventArgs>? TriggerUnregistered;
        event EventHandler<BeepFormsUnitOfWorkEventArgs>? UnitOfWorkActivity;

        void Bind(IBeepFormsHost formsHost);
        void Unbind();
        void ApplyDefinition(BeepBlockDefinition definition);
        void SyncFromManager();

        // Phase 6 — designer-generated field controls
        void BindControl(string fieldName, Control control);
        void BindControl(string fieldName, Control control, string bindingProperty);
        void UnbindControl(string fieldName);
        void UnbindAllControls();
        void RebindControl(string fieldName, Control newControl);
        Control? GetBoundControl(string fieldName);
        IReadOnlyDictionary<string, Control> GetAllBindings();
        void RefreshFieldControl(string fieldName);
        void RefreshAllFieldControls();
    }
}