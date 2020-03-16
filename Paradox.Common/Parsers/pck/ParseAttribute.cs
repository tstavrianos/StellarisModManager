using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace Paradox.Common.Parsers.pck
{
    internal class ParseAttributeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return typeof(InstanceDescriptor) == destinationType || base.CanConvertTo(context, destinationType);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (typeof(InstanceDescriptor) != destinationType)
                return base.ConvertTo(context, culture, value, destinationType);
            var attr = (ParseAttribute)value;
            return new InstanceDescriptor(typeof(ParseAttribute).GetConstructor(new[] { typeof(string), typeof(object) }), new[] { attr.Name, attr.Value });
        }
    }
    [TypeConverter(typeof(ParseAttributeConverter))]
    public struct ParseAttribute
    {
        public readonly string Name;
        public readonly object Value;
        public ParseAttribute(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
