using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace TheTechIdea.Beep.Winform.Controls.Converters
{
    /// <summary>
    /// A <see cref="TypeConverter"/> that returns the live <see cref="BeepDataConnection"/>
    /// components on the host form. Used by the BeepForms.DataConnection property so the
    /// property grid shows a typed picker instead of an untyped object reference.
    /// </summary>
    public sealed class BeepDataConnectionComponentConverter : ReferenceConverter
    {
        public BeepDataConnectionComponentConverter()
            : base(typeof(BeepDataConnection))
        {
        }

        protected override bool IsValueAllowed(ITypeDescriptorContext context, object value)
        {
            return value is BeepDataConnection;
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is BeepDataConnection connection)
            {
                string name = connection.Site?.Name ?? connection.GetHashCode().ToString("X8");
                return $"{name} ({connection.GetType().Name})";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
