namespace Stellaris.Data.Parsers.Models
{
    public sealed class Percent : ITypedValue<int>
    {
        public Percent(int value)
        {
            this.Value = value;
        }

        public int Value{ get; }
        
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
            return other != null && this.Equals(other.Value);
        }
    }
}