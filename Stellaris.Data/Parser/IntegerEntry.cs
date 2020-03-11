namespace Stellaris.Data.Parser
{
    public sealed class IntegerEntry : IEntry
    {
        public IntegerEntry(long value)
        {
            this.Value = value;
        }

        public long Value { get; }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}