namespace Stellaris.Data.ParadoxParsers.Types
{
    using System;

    public interface ITyped<T> : IEquatable<ITyped<T>>
    {
        T Value { get; }
    }
}
