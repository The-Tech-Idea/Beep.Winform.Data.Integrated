using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheTechIdea.Beep.Container.Services;
using TheTechIdea.Beep.Services;

namespace TheTechIdea.Beep.Winform.Controls.ITrees.BeepTreeView
{
    public partial class BeepTreeNodeDragandDropHandler
    {
        private IBeepService service;
        private BeepAppTree beepTreeControl;

        public BeepTreeNodeDragandDropHandler(IBeepService service, BeepAppTree beepTreeControl)
        {
            this.service = service;
            this.beepTreeControl = beepTreeControl;
        }
    }
}
