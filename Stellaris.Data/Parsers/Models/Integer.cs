namespace Stellaris.Data.Parsers.Models
{
    public sealed class Integer : ITypedValue<long>, ITypedField<long>
    {
        public Integer(long value)
        {
            this.Value = value;
        }

        public long Value { get; }
        
        private bool Equals(long other)
        {
            return this.Value.Equals(other);
        }

        public bool Equals(IValue other)
        {
            if (other is ITyped<long> a) return this.Equals(a);
            return false;
        }

        public bool Equals(IField other)
        {
            if (other is ITyped<long> a) return this.Equals(a);
            return false;
        }

        public bool Equals(ITyped<long> other)
        {
            return other != null && this.Equals(other.Value);
        }
    }
}