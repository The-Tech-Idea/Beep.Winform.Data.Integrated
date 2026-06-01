using System.ComponentModel;
using System.Linq;
using TheTechIdea.Beep.Winform.Controls.Integrated.Blocks;

namespace TheTechIdea.Beep.Winform.Controls.Converters
{
    public class DataBlockEntityConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context?.Instance is BeepBlock block)
            {
                return new StandardValuesCollection(block.GetAvailableEntityNames().ToList());
            }

            return new StandardValuesCollection(Array.Empty<string>());
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string text)
            {
                return text;
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
