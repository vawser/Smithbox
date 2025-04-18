namespace Hexa.NET.ImGui.Widgets.Extensions
{
    using System;
    using System.Collections.Generic;

    public static class ListExtensions
    {
        public static bool Contains(this List<string> list, ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            foreach (var item in list)
            {
                if (item.AsSpan().Equals(value, comparisonType))
                {
                    return true;
                }
            }

            return false;
        }
    }
}