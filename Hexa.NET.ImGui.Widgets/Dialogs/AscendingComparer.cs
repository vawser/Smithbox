namespace Hexa.NET.ImGui.Widgets.Dialogs
{
    public readonly struct AscendingComparer<T, TComparer> : IComparer<T> where TComparer : struct, IComparer<T> where T : struct
    {
        private readonly TComparer comparer = new();

        public AscendingComparer()
        {
        }

        public int Compare(T x, T y)
        {
            return -comparer.Compare(x, y);
        }
    }
}