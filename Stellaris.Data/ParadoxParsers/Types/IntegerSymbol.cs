namespace Stellaris.Data.ParadoxParsers.Types
{
    public sealed class IntegerSymbol : ITypedSymbol<long>
    {
        public IntegerSymbol(long value)
        {
            this.Value = value;
        }

        public long Value { get; }

        private bool Equals(long other)
        {
            return this.Value.Equals(other);
        }

        public bool Equals(ISymbol other)
        {
            if (other is ITyped<long> a) return this.Equals(a);
            return false;
        }

        public bool Equals(ITyped<long> other)
        {
            if (other == null) return false;
            return this.Equals(other.Value);
        }
    }
}