using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace StellarisModManager.Core.Parsers.pck
{
    internal class CharDfaEntryConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return typeof(InstanceDescriptor) == destinationType || base.CanConvertTo(context, destinationType);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (typeof(InstanceDescriptor) != destinationType)
                return base.ConvertTo(context, culture, value, destinationType);
            var dte = (CharDfaEntry)value;
            return new InstanceDescriptor(typeof(CharDfaEntry).GetConstructor(new [] { typeof(int), typeof(CharDfaTransitionEntry[]) }), new object[] { dte.AcceptSymbolId, dte.Transitions });
        }
    }
    [TypeConverter(typeof(CharDfaEntryConverter))]
    public struct CharDfaEntry
    {
        public CharDfaEntry(int acceptSymbolId, CharDfaTransitionEntry[] transitions)
        {
            this.AcceptSymbolId = acceptSymbolId;
            this.Transitions = transitions;
        }
        public int AcceptSymbolId;
        public CharDfaTransitionEntry[] Transitions;
    }

    internal class CharDfaTransitionEntryConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return typeof(InstanceDescriptor) == destinationType || base.CanConvertTo(context, destinationType);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (typeof(InstanceDescriptor) != destinationType)
                return base.ConvertTo(context, culture, value, destinationType);
            var dte = (CharDfaTransitionEntry)value;
            return new InstanceDescriptor(typeof(CharDfaTransitionEntry).GetConstructor(new[] { typeof(char[]), typeof(int) }), new object[] { dte.PackedRanges, dte.Destination });
        }
    }
    [TypeConverter(typeof(CharDfaTransitionEntryConverter))]
    public struct CharDfaTransitionEntry
    {
        public CharDfaTransitionEntry(char[] transitions, int destination)
        {
            this.PackedRanges = transitions;
            this.Destination = destination;
        }
        public char[] PackedRanges;
        public int Destination;
    }
}
