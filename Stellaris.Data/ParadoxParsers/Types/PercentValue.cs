namespace Stellaris.Data.ParadoxParsers.Types
{
    public sealed class PercentValue : ITypedValue<int>
    {
        public PercentValue(int value)
        {
            this.Value = value;
        }

        public int Value { get; }

        private bool Equals(int other)
        {
            return this.Value.Equals(other);
        }

        public bool Equals(IValue other)
        {
            if (other is ITyped<int> a) return this.Equals(a);
            return false;
        }

        public bool Equals(ITyped<int> other)
        {
            if (other == null) return false;
            return this.Equals(other.Value);
        }
    }
}