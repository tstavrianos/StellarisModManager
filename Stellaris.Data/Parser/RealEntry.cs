using System.Globalization;

namespace Stellaris.Data.Parser
{
    public sealed class RealEntry : IEntry
    {
        public RealEntry(double value)
        {
            this.Value = value;
        }

        public double Value { get; }
        
        public override string ToString()
        {
            return this.Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}