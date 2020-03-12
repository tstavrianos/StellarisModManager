namespace Stellaris.Data.Variants
{
    internal interface IVariantHolder
    {
        bool Is<T>();

        object Get();
    }
}
