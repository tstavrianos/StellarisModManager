using System.Collections;
using System.Collections.Generic;

namespace Stellaris.Data.Parsers.Models
{
    public sealed class Array : ITypedValue<IReadOnlyList<IValue>>, IReadOnlyList<IValue>
    {
        public Array(IReadOnlyList<IValue> value)
        {
            this.Value = value;
        }

        public IReadOnlyList<IValue> Value{ get; }
        
        public bool Equals(IValue other)
        {
            if (other is ITyped<IReadOnlyList<IValue>> a) return this.Equals(a);
            return false;
        }

        public bool Equals(ITyped<IReadOnlyList<IValue>> other)
        {
            return other != null && this.Equals(other.Value);
        }

        private bool Equals(IReadOnlyList<IValue> other)
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

        public IEnumerator<IValue> GetEnumerator()
        {
            return this.Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) this.Value).GetEnumerator();
        }

        public int Count => this.Value.Count;

        public IValue this[int index] => this.Value[index];
    }
}