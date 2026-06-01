using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks;

namespace TheTechIdea.Beep.Winform.Controls.Integrated.Forms
{
    public partial class BeepForms
    {
        private readonly HashSet<string> _definitionBlockNames = new(StringComparer.OrdinalIgnoreCase);

        public void RebuildBlocksFromDefinition()
        {
            if (!AutoCreateBlocksFromDefinition)
            {
                return;
            }

            foreach (string blockName in _definitionBlockNames.ToList())
            {
                UnregisterBlock(blockName);
            }

            _definitionBlockNames.Clear();

            if (Definition?.Blocks == null || Definition.Blocks.Count == 0)
            {
                SyncFromManager();
                return;
            }

            foreach (var blockDefinition in Definition.Blocks)
            {
                if (string.IsNullOrWhiteSpace(blockDefinition?.BlockName))
                {
                    continue;
                }

                var blockControl = new BeepBlock
                {
                    Name = $"BeepBlock_{blockDefinition.BlockName}",
                    BlockName = blockDefinition.BlockName,
                    Definition = blockDefinition,
                    Dock = DockStyle.Top
                };

                if (RegisterBlock(blockControl))
                {
                    _definitionBlockNames.Add(blockDefinition.BlockName);
                }
            }

            SyncFromManager();
        }
    }
}