namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor
{
    using System;
    using System.Collections.Generic;

    public class TextDrawData
    {
        public readonly List<TextHighlightSpan> Spans = [];
        public readonly List<TextSpan> FoldSpans = [];
        public readonly List<LineIndexSpan> LineSpanIndices = [];

        public IEnumerable<TextHighlightSpan> GetVisibleSpans(float scroll, float lineHeight, float maxHeight)
        {
            int start = (int)MathF.Floor(scroll / lineHeight);
            int end = (int)MathF.Ceiling(maxHeight / lineHeight) + start;
            end = Math.Min(end, LineSpanIndices.Count);

            for (int i = start; i < end; i++)
            {
                var (startSpanIndex, endSpanIndex) = LineSpanIndices[i];

                for (int j = startSpanIndex; j <= endSpanIndex; j++)
                {
                    yield return Spans[j];
                }
            }
        }
    }
}