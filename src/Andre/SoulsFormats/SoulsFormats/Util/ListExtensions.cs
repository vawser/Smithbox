using System.Collections.Generic;

namespace SoulsFormats
{
    internal static class ListExtensions
    {
        public static T EchoAdd<T>(this List<T> list, T item)
        {
            list.Add(item);
            return item;
        }
    }
}
