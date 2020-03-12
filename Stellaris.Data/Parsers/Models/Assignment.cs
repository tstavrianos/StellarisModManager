using System;

namespace Stellaris.Data.Parsers.Models
{
    public sealed class Assignment: IEquatable<Assignment>
    {
        public Assignment(IField field, Operator @operator, IValue value)
        {
            this.Field = field;
            this.Operator = @operator;
            this.Value = value;
        }

        public IField Field { get; }
        public Operator Operator { get; }
        public IValue Value { get; }

        public bool Equals(Assignment other)
        {
            if (other == null) return false;
            if (this.Field == null) return false;
            if (this.Value == null) return false;

            return this.Field.Equals(other.Field) && this.Operator == other.Operator && this.Value.Equals(other.Value);
        }

    }
}