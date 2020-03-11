using System.Collections;
using System.Collections.Generic;

namespace Stellaris.Data.Parser
{
    public sealed class ArrayEntry : IEntry, IReadOnlyList<IEntry>
    {
        public ArrayEntry(IReadOnlyList<IEntry> values)
        {
            this.Value = values;
        }

        public IReadOnlyList<IEntry> Value { get; }
        public IEnumerator<IEntry> GetEnumerator()
        {
            return this.Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) this.Value).GetEnumerator();
        }

        public int Count => this.Value.Count;

        public IEntry this[int index] => this.Value[index];

        public override string ToString()
        {
            return $"[{string.Join(',', this.Value)}]";
        }
    }
}