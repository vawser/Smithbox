
namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor
{
    public class TextHighlightSpanStartComparer : IComparer<TextHighlightSpan>
    {
        public static readonly TextHighlightSpanStartComparer Instance = new();

        public int Compare(TextHighlightSpan x, TextHighlightSpan y)
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