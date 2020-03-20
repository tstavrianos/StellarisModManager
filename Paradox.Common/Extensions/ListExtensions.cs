using System.Collections.Generic;

namespace Paradox.Common.Extensions
{
    public static class ListExtensions
    {
        public static void Swap<T>(this IList<T> list, int a, int b)
        {
            var tmp = list[a];
            list[a] = list[b];
            list[b] = tmp;
        }
    }
}