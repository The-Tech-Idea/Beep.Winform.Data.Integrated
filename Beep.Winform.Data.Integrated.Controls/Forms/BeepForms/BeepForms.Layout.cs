using System;
using System.Windows.Forms;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    public partial class BeepForms
    {
        private Panel? _blocksHostPanel;

        public Control? BlocksHostControl => _blocksHostPanel;

        private void InitializeLayout()
        {
            SuspendLayout();

            _blocksHostPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(8),
                Margin = new Padding(0)
            };

            Controls.Add(_blocksHostPanel);

            ResumeLayout(false);
        }

        private void ApplyShellStateToUi()
        {
            ViewStateChanged?.Invoke(this, EventArgs.Empty);

            if (IsHandleCreated)
            {
                Invalidate();
            }
        }
    }
}