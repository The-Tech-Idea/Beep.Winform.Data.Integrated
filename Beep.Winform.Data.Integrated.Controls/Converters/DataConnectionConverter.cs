using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.ComponentModel.TypeConverter;
using TheTechIdea.Beep.ConfigUtil;

namespace TheTechIdea.Beep.Winform.Controls.Converters
{
    /// <summary>
    /// A custom type converter to provide a dropdown of available connections.
    /// </summary>
    public class DataConnectionConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context?.Instance is BeepDataConnection dataConnection)
            {
                var connections = GetAvailableConnections(dataConnection);
                return new StandardValuesCollection(connections);
            }
            return new StandardValuesCollection(new List<ConnectionProperties>());
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string stringValue && context?.Instance is BeepDataConnection dataConnection)
            {
                return GetAvailableConnections(dataConnection)
                    .FirstOrDefault(c => string.Equals(c.ConnectionName, stringValue, StringComparison.OrdinalIgnoreCase));
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is ConnectionProperties connection)
            {
                return connection.ConnectionName;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        private static List<ConnectionProperties> GetAvailableConnections(BeepDataConnection dataConnection)
        {
            var serviceConnections = dataConnection.BeepService?.Config_editor?.DataConnections;
            if (serviceConnections != null && serviceConnections.Any())
            {
                return serviceConnections;
            }

            return dataConnection.DataConnections ?? new List<ConnectionProperties>();
        }
    }
}
