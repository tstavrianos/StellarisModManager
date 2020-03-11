namespace Stellaris.Data.Parser
{
    public sealed class StringEntry: IEntry
    {
        public StringEntry(string value)
        {
            this.Value = value;
        }

        public string Value { get; }
        
        public override string ToString()
        {
            return this.Value;
        }
    }
}