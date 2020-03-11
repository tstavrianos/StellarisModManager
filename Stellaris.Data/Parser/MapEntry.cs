using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Stellaris.Data.Parser
{
    public sealed class MapEntry : IEntry, IReadOnlyDictionary<string, IList<IEntry>>
    {
        public MapEntry(IReadOnlyDictionary<string, IList<IEntry>> values)
        {
            this.Value = values;
        }

        public IReadOnlyDictionary<string, IList<IEntry>> Value { get; }
        public IEnumerator<KeyValuePair<string, IList<IEntry>>> GetEnumerator()
        {
            return this.Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) this.Value).GetEnumerator();
        }

        public int Count => this.Value.Count;

        public bool ContainsKey(string key)
        {
            return this.Value.ContainsKey(key);
        }

        public bool TryGetValue(string key, out IList<IEntry> value)
        {
            return this.Value.TryGetValue(key, out value);
        }

        public IList<IEntry> this[string key] => this.Value[key];

        public IEnumerable<string> Keys => this.Value.Keys;

        public IEnumerable<IList<IEntry>> Values => this.Value.Values;
        
        public override string ToString()
        {
            var values = this.Value.Select(x => $"{x.Key} => [{string.Join(',', x.Value)}]");
            return "{" + $"{string.Join(',', values)}" + "}";
        }
    }
}