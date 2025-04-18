namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor
{
    using System.Collections.Generic;

    public readonly struct CaseInsensitiveComparer : IEqualityComparer<char>
    {
        public static readonly CaseInsensitiveComparer Default = new();

        public readonly bool Equals(char x, char y)
        {
            return char.ToLowerInvariant(x) == char.ToLowerInvariant(y);
        }

        public readonly int GetHashCode(char obj)
        {
            return char.ToLowerInvariant(obj).GetHashCode();
        }
    }
}