using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Utilities;

public static class ListExtensions
{
    public static bool IndexExists<T>(this List<T> list, int index)
    {
        return index >= 0 && index < list.Count;
    }
}