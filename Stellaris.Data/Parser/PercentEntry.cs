namespace Stellaris.Data.Parser
{
    public sealed class PercentEntry : IEntry
    {
        public PercentEntry(int value)
        {
            this.Value = value;
        }

        public int Value { get; }
        
        public override string ToString()
        {
            return this.Value.ToString() + '%';
        }
    }
}