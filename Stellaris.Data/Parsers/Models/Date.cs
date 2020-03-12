using System;

namespace Stellaris.Data.Parsers.Models
{
    public sealed class Date : ITypedValue<DateTime>
    {
        public Date(DateTime value)
        {
            this.Value = value;
        }

        public DateTime Value{ get; }
        
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
            return other != null && this.Equals(other.Value);
        }
    }
}