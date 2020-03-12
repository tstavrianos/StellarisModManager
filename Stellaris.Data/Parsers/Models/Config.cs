using System;
using System.Collections;
using System.Collections.Generic;

namespace Stellaris.Data.Parsers.Models
{
    public sealed class Config: IEquatable<Config>, ITyped<IReadOnlyList<Assignment>>, IReadOnlyList<Assignment>
    {
        public Config(IReadOnlyList<Assignment> value)
        {
            this.Value = value;
        }

        public IReadOnlyList<Assignment> Value { get; }
        
        public bool Equals(Config other)
        {
            return other != null && this.Equals(other.Value);
        }
        
        public bool Equals(ITyped<IReadOnlyList<Assignment>> other)
        {
            return other != null && this.Equals(other.Value);
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

        public IEnumerator<Assignment> GetEnumerator()
        {
            return this.Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) this.Value).GetEnumerator();
        }

        public int Count => this.Value.Count;

        public Assignment this[int index] => this.Value[index];
    }
}