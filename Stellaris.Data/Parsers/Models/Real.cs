namespace Stellaris.Data.Parsers.Models
{
    public sealed class Real : ITypedValue<double>
    {
        public Real(double value)
        {
            this.Value = value;
        }

        public double Value{ get; }
        
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
            return other != null && this.Equals(other.Value);
        }
    }
}