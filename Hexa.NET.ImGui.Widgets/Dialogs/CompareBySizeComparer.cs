namespace Hexa.NET.ImGui.Widgets.Dialogs
{
    public readonly struct CompareBySizeComparer<T> : IComparer<T> where T : struct, IFileSystemItem
    {
        public int Compare(T a, T b)
        {
            int cmp = BaseComparer.CompareByBase(a, b);
            if (cmp != 0)
            {
                return cmp;
            }
            cmp = a.Size.CompareTo(b.Size);
            if (cmp != 0)
            {
                return cmp;
            }
            return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
        }
    }
}