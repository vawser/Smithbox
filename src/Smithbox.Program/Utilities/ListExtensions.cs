using System.Collections.Generic;

namespace StudioCore.Utilities;

public static class ListExtensions
{
    public static bool IndexExists<T>(this List<T> list, int index)
    {
        return index >= 0 && index < list.Count;
    }
}