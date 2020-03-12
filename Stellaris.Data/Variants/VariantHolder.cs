namespace Stellaris.Data.Variants
{
    internal sealed class VariantHolder<T> : IVariantHolder
    {
        public T Item { get; }

        public bool Is<U>() => typeof(U) == typeof(T);

        public object Get() => this.Item;

        public VariantHolder(T item) => this.Item = item;
    }
}
