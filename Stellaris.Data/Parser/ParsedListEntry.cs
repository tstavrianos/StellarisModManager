using System.Collections.Generic;

namespace Stellaris.Data.Parser
{
    public sealed class ParsedListEntry: IParsedEntry
    {
        public string Name { get; }
        public IList<string> Values { get; set; }
        
        public ParsedListEntry(string name, IList<string> values)
        {
            this.Name = name;
            this.Values = values;
        }

        public override string ToString()
        {
            return $"{this.Name} = {string.Join(',', this.Values)}";
        }
    }
}