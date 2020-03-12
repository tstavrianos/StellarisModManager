namespace Stellaris.Data.ParadoxParsers.Types
{
    public sealed class StringField : ITypedField<string>
    {
        public StringField(string value)
        {
            this.Value = value;
        }

        public string Value { get; }

        private bool Equals(string other)
        {
            return this.Value.Equals(other);
        }

        public bool Equals(IField other)
        {
            if (other is ITyped<string> a) return this.Equals(a);
            return false;
        }

        public bool Equals(ITyped<string> other)
        {
            if (other == null) return false;
            return this.Equals(other.Value);
        }

    }
}
