namespace Stellaris.Data.ParadoxParsers.Types
{
    public sealed class SymbolValue : ITypedValue<ISymbol>
    {
        public SymbolValue(ISymbol value)
        {
            this.Value = value;
        }

        public ISymbol Value { get; }

        private bool Equals(ISymbol other)
        {
            return this.Value.Equals(other);
        }

        public bool Equals(IValue other)
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