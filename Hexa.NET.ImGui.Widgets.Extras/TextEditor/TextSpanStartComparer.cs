
namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor
{
    public class TextSpanStartComparer : IComparer<TextSpan>
    {
        public static readonly TextSpanStartComparer Instance = new();

        public int Compare(TextSpan x, TextSpan y)
        {
            if (x.Start > y.Start)
            {
                return 1;
            }
            else if (x.Start < y.Start)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}