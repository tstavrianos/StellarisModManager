namespace Stellaris.Data.Parser
{
    public sealed class ParsedStringEntry: IParsedEntry
    {
        public string Name { get; }
        public string Value { get; }

        public ParsedStringEntry(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public override string ToString()
        {
            return $"{this.Name} = {this.Value}";
        }
    }
}