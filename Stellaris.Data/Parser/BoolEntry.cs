using System.Reflection.Metadata.Ecma335;

namespace Stellaris.Data.Parser
{
    public sealed class BoolEntry : IEntry
    {
        public BoolEntry(bool value)
        {
            this.Value = value;
        }

        public bool Value { get; }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}