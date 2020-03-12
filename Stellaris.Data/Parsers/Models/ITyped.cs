using System;

namespace Stellaris.Data.Parsers.Models
{
    public interface ITyped<T> : IEquatable<ITyped<T>>
    {
        T Value { get; }
    }
}