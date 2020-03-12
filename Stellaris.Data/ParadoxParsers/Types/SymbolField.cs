namespace Stellaris.Data.ParadoxParsers.Types
{
    public sealed class SymbolField : ITypedField<ISymbol>
    {
        public SymbolField(ISymbol value)
        {
            this.Value = value;
        }

        public ISymbol Value { get; }

        private bool Equals(ISymbol other)
        {
            return this.Value.Equals(other);
        }

        public bool Equals(IField other)
        {
            if (other is ITyped<ISymbol> a) return this.Equals(a);
            return false;
        }

        public bool Equals(ITyped<ISymbol> other)
        {
            if (other == null) return false;
            return this.Equals(other.Value);
        }
    }
}