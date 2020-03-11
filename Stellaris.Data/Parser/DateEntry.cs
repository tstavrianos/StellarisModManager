using System;
using System.Globalization;

namespace Stellaris.Data.Parser
{
    public sealed class DateEntry : IEntry
    {
        public DateEntry(DateTime value)
        {
            this.Value = value;
        }

        public DateTime Value { get; }

        public override string ToString()
        {
            return this.Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}