namespace Stellaris.Data.Parsers.Models
{
    public sealed class String : ITypedValue<string>, ITypedField<string>
    {
        public String(string value)
        {
            this.Value = value;
            if (!string.IsNullOrWhiteSpace(this.Value) && this.Value.Length >= 2 && this.Value[0] == '"' && this.Value[this.Value.Length - 1] == '"')
            {
                this.Value = this.Value.Substring(1, this.Value.Length - 2);
            }
        }

        public string Value{ get; }
        
        private bool Equals(string other)
        {
            return this.Value.Equals(other);
        }

        public bool Equals(IValue other)
        {
            if (other is ITyped<string> a) return this.Equals(a);
            return false;
        }

        public bool Equals(IField other)
        {
            if (other is ITyped<string> a) return this.Equals(a);
            return false;
        }

        public bool Equals(ITyped<string> other)
        {
            return other != null && this.Equals(other.Value);
        }
    }
}