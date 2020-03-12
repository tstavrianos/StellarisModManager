namespace Stellaris.Data.ParadoxParsers.Types
{
    public sealed class RealValue : ITypedValue<double>
    {
        public RealValue(double value)
        {
            this.Value = value;
        }

        public double Value { get; }

        private bool Equals(double other)
        {
            return this.Value.Equals(other);
        }

        public bool Equals(IValue other)
        {
            if (other is ITyped<double> a) return this.Equals(a);
            return false;
        }

        public bool Equals(ITyped<double> other)
        {
            if (other == null) return false;
            return this.Equals(other.Value);
        }
    }
}