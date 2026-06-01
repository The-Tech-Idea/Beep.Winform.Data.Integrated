using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Windows.Forms;
using TheTechIdea.Beep.Vis.Modules;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks;

namespace TheTechIdea.Beep.Winform.Controls.Converters
{
    public class DataBlockConverter : TypeConverter
    {
        private Dictionary<string, BeepBlock> _blockMap = new();
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string) || sourceType == typeof(BeepBlock))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context?.Container == null || context.Instance is not BeepBlock currentBlock)
                return new StandardValuesCollection(Array.Empty<string>());

            if (context.Container is not IDesignerHost designerHost)
                return new StandardValuesCollection(Array.Empty<string>());

            var allBlocks = designerHost.Container.Components
                .OfType<BeepBlock>()
                .Where(block => block != currentBlock && !IsChildBlock(currentBlock, block))
                .ToList();

            // Map block names to objects for conversion
            _blockMap = allBlocks
                .Where(block => block.Site != null)
                .ToDictionary(block => block.Name, block => block);

            // Return the names of the blocks
            return new StandardValuesCollection(_blockMap.Keys.ToList());
        }
        public override object ConvertFrom(
     ITypeDescriptorContext context,
     CultureInfo culture,
     object value)
        {
            // If the incoming value is a block, just return it.
            if (value is BeepBlock blockValue)
                return blockValue;

            // If it's a string, try to look it up in _blockMap.
            if (value is string blockName)
            {
                if (_blockMap != null && _blockMap.TryGetValue(blockName, out var block))
                {
                    return block;
                }
                else
                {
                    // Decide how you want to handle an unknown block name:
                    // Option A: return null (if your property can be null)
                    // return null;

                    // Option B: throw a descriptive exception
                    throw new ArgumentException($"Block named '{blockName}' was not found.");
                }
            }
            // Otherwise, fall back to the base behavior
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is BeepBlock block)
            {
                return block.Name ?? block.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
        private static bool IsChildBlock(BeepBlock parentBlock, BeepBlock block)
        {
            Control? current = block.Parent;
            while (current != null)
            {
                if (ReferenceEquals(current, parentBlock))
                {
                    return true;
                }

                current = current.Parent;
            }

            return false;
        }
    }
}
