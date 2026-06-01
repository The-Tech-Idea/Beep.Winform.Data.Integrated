using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Models;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Blocks.Contracts
{
    public interface IBeepFieldPresenter
    {
        string Key { get; }

        bool CanPresent(BeepFieldDefinition fieldDefinition);
        Control CreateEditor(BeepFieldDefinition fieldDefinition);
        void ApplyMetadata(Control editor, BeepFieldDefinition fieldDefinition);
    }
}