namespace Stellaris.Data.ParadoxParsers.Types
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class MapValue : ITypedValue<IReadOnlyList<Assignment>>, IReadOnlyDictionary<IField, IEnumerable<IValue>>
    {
        public MapValue(IReadOnlyList<Assignment> value)
        {
            this.Value = value;
        }

        public IReadOnlyList<Assignment> Value { get; }

        public int Count => this.Value.Count;


        public bool ContainsKey(IField key)
        {
            return this.Value.Any(x => x.Field.Equals(key));
        }

        public bool TryGetValue(IField key, out IEnumerable<IValue> value)
        {
            var i = this.Value.Where(x => x.Field.Equals(key)).Select(x => x.Value);
            if (i.Any())
            {
                value = i;
                return true;
            }
            value = default;
            return false;
        }

        public IEnumerable<IValue> this[IField key] => this.Value.Where(x => x.Field.Equals(key)).Select(x => x.Value);

        public IEnumerable<IField> Keys => this.Value.Select(x => x.Field).Distinct();

        public IEnumerable<IEnumerable<IValue>> Values => this.Value.GroupBy(x => x.Field).Select(x => x.Select(y => y.Value));

        public IEnumerator<KeyValuePair<IField, IEnumerable<IValue>>> GetEnumerator()
        {
            foreach (var value in this.Value.GroupBy(x => x.Field))
            {
                yield return new KeyValuePair<IField, IEnumerable<IValue>>(value.Key, value.Select(x => x.Value));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool Equals(IValue other)
        {
            if (other is ITyped<IReadOnlyList<Assignment>> a) return this.Equals(a);
            return false;
        }

        public bool Equals(ITyped<IReadOnlyList<Assignment>> other)
        {
            if (other == null) return false;
            return this.Equals(other.Value);
        }

        private bool Equals(IReadOnlyList<Assignment> other)
        {
            if (other == null || this.Value == null) return false;
            if (other.Count != this.Value.Count) return false;
            for (var i = 0; i < this.Value.Count; i++)
            {
                if (this.Value[i] == null) return false;
                if (this.Value[i].Equals(other[i])) return false;
            }

            return true;
        }
    }
}