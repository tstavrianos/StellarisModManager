namespace Stellaris.Data.ParadoxParsers.Types
{
    using System;

    public sealed class DateValue : ITypedValue<DateTime>
    {
        public DateValue(DateTime value)
        {
            this.Value = value;
        }

        public DateTime Value { get; }

        private bool Equals(DateTime other)
        {
            return this.Value.Equals(other);
        }

        public bool Equals(IValue other)
        {
            if (other is ITyped<DateTime> a) return this.Equals(a);
            return false;
        }

        public bool Equals(ITyped<DateTime> other)
        {
            if (other == null) return false;
            return this.Equals(other.Value);
        }
    }
}