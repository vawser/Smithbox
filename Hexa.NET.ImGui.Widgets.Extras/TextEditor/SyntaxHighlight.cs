namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor
{
    using Hexa.NET.Utilities;
    using System.Numerics;
    using System.Text.RegularExpressions;

    public class SyntaxHighlight
    {
        private readonly List<Regex> regexes = [];

        public SyntaxHighlight(string name, string pattern)
        {
            Name = name;
            Pattern = pattern;
        }

        public string Name { get; set; }

        public string Pattern { get; set; }

        public List<SyntaxHighlightDefinition> Definitions { get; } = [];

        public void Initialize()
        {
            regexes.Clear();
            for (int i = 0; i < Definitions.Count; i++)
            {
                var definition = Definitions[i];
                Regex regex = new(definition.Pattern, RegexOptions.Compiled);

                regexes.Add(regex);
            }
        }

        private readonly List<TextHighlightSpan> matchSpans = [];
        private readonly List<TextSpan> foldSpans = [];
        private readonly List<LineIndexSpan> lineSpanIndices = [];

        public unsafe void Analyze(StdWString* text, List<TextSpan> lines, List<TextHighlightSpan> spans, List<LineIndexSpan> lineSpanIndices, float lineHeight)
        {
            matchSpans.Clear();
            foldSpans.Clear();
            lineSpanIndices.Clear();
            Text = text;

            var span = text->AsSpan();

            FindMatches(text, span);

            const bool controls = false;
            if (controls)
            {
                for (int i = 0; i < text->Size; i++)
                {
                    var c = (*text)[i];
                    if (char.IsControl(c))
                    {
                        EmitSpan(i, 1, new(1, 0, 0, 1), true);
                    }
                }
            }

            RemoveOverlapping();
            FillGaps(text, spans);
            SplitFolds(spans);
            ComputeOrigin(lines, lineHeight, spans);
            ComputeLineIndices(lines, spans, lineSpanIndices);
        }

        private static unsafe void ComputeLineIndices(List<TextSpan> lines, List<TextHighlightSpan> spans, List<LineIndexSpan> lineSpanIndices)
        {
            int spanIndex = 0;
            for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
            {
                while (spanIndex < spans.Count && spans[spanIndex].End < lines[lineIndex].Start)
                    spanIndex++;

                int startSpanIndex = spanIndex;

                while (spanIndex < spans.Count && spans[spanIndex].Start <= lines[lineIndex].End)
                    spanIndex++;

                int endSpanIndex = spanIndex - 1;

                lineSpanIndices.Add(new(startSpanIndex, endSpanIndex));
            }
        }

        public unsafe void Analyze(TextSource source, TextDrawData drawData, float lineHeight)
        {
            Analyze(source.Text, source.Lines, drawData.Spans, drawData.LineSpanIndices, lineHeight);
        }

        private int InFolding(int idx, int len)
        {
            int end = idx + len;
            for (int i = 0; i < foldSpans.Count; i++)
            {
                var span = foldSpans[i];
                if (span.Start <= end && span.End >= idx)
                {
                    return i;
                }
            }
            return -1;
        }

        private unsafe void SplitFolds(List<TextHighlightSpan> spans)
        {
            foldSpans.Sort(TextSpanStartComparer.Instance);

            int lastFoldIndex = 0;
            for (int i = spans.Count - 1; i >= 0; i--)
            {
                var span = spans[i];
                bool foundOverlap = false;
                for (int j = lastFoldIndex; j < foldSpans.Count; j++)
                {
                    var fold = foldSpans[j];
                    if (span.Start < fold.End && span.End > fold.Start)
                    {
                        // Split the span at the overlapping points
                        if (span.Start < fold.Start && span.End > fold.End)
                        {
                            // Case 3: The span starts before the fold and ends after the fold
                            spans.Insert(i, new TextHighlightSpan(span.String, default, span.Color, span.HasColor, span.Start, fold.Start - span.Start));
                            spans.Insert(i + 1, new TextHighlightSpan(span.String, default, span.Color, span.HasColor, fold.Start, fold.End - fold.Start));
                            spans.Insert(i + 2, new TextHighlightSpan(span.String, default, span.Color, span.HasColor, fold.End, span.End - fold.End));
                            spans.RemoveAt(i + 3);
                        }
                        else if (span.Start < fold.Start)
                        {
                            // Case 1: The span starts before the fold and ends within the fold
                            spans.Insert(i, new TextHighlightSpan(span.String, default, span.Color, span.HasColor, span.Start, fold.Start - span.Start));
                            spans.RemoveAt(i + 1);
                        }
                        else if (span.End > fold.End)
                        {
                            // Case 2: The span starts within the fold and ends after the fold
                            spans.Insert(i, new TextHighlightSpan(span.String, default, span.Color, span.HasColor, fold.End, span.End - fold.End));
                            spans.RemoveAt(i + 1);
                        }
                        else
                        {
                            // Case 4: The span is completely inside the fold
                            // Do nothing
                        }
                        lastFoldIndex = j;
                        foundOverlap = true;
                    }
                    else if (foundOverlap)
                    {
                        break;
                    }
                }
            }
        }

        private static unsafe void ComputeOrigin(List<TextSpan> lines, float lineHeight, List<TextHighlightSpan> spans)
        {
            for (int i = 0; i < spans.Count; i++)
            {
                var span = spans[i];
                span.Origin = ComputeOrigin(lines, lineHeight, span.Start);
                spans[i] = span;
            }
        }

        private unsafe void FillGaps(StdWString* text, List<TextHighlightSpan> spans)
        {
            spans.Clear();
            int currentPos = 0;
            for (int i = 0; i < matchSpans.Count; i++)
            {
                var match = matchSpans[i];
                if (match.Start != currentPos)
                {
                    SplitLines(text, currentPos, match.Start - currentPos, spans);
                }
                spans.Add(match);
                currentPos = match.End;
            }

            if (currentPos < text->Size)
            {
                SplitLines(text, currentPos, text->Size - currentPos, spans);
            }

            spans.Sort(TextHighlightSpanStartComparer.Instance);
        }

        protected unsafe StdWString* Text { get; private set; }

        protected virtual unsafe void FindMatches(StdWString* text, Span<char> span)
        {
            for (int i = 0; i < regexes.Count; i++)
            {
                var def = Definitions[i];
                var col = def.Color;
                var regex = regexes[i];

                foreach (var match in regex.EnumerateMatches(span))
                {
                    var idx = match.Index;
                    var len = match.Length;
                    EmitSpan(idx, len, col);
                }
            }
        }

        protected virtual unsafe void FindFoldings(StdWString* text, Span<char> span)
        {
        }

        protected unsafe void EmitSpan(int idx, int len, ColorRGBA color, bool control = false)
        {
            TextHighlightSpan textSpan = new(Text, default, color, idx, len, control);
            matchSpans.Add(textSpan);
        }

        protected unsafe void EmitFolding(int idx, int len)
        {
            TextSpan textSpan = new(Text, idx, len);
            foldSpans.Add(textSpan);
        }

        private unsafe void RemoveOverlapping()
        {
            matchSpans.Sort(TextHighlightSpanStartComparer.Instance);

            // remove overlapping.
            int lastMatch = -1;
            int lastIndex = -1;
            int lastLength = 0;

            for (int i = 0; i < matchSpans.Count; i++)
            {
                var currentSpan = matchSpans[i];
                if (currentSpan.Start >= lastIndex + lastLength)
                {
                    lastMatch = i;
                    lastIndex = currentSpan.Start;
                    lastLength = currentSpan.Length;
                }
                else
                {
                    if (currentSpan.Start == lastIndex && currentSpan.Length == lastLength)
                    {
                        matchSpans.RemoveAt(i);
                        i--;
                    }

                    Split(lastMatch, i);
                }
            }
        }

        private unsafe void Split(int lastMatch, int i)
        {
            var span1 = matchSpans[lastMatch];
            var span2 = matchSpans[i];

            int overlapStart = Math.Max(span1.Start, span2.Start);
            int overlapEnd = Math.Min(span1.End, span2.End);

            // Handle the overlap by creating or adjusting spans
            if (span2.Start < span1.Start)
            {
                // Part before the overlapping section
                matchSpans[i] = new TextHighlightSpan(span2.String, default, span2.Start, span1.Start - span2.Start);
                matchSpans.Insert(i + 1, new TextHighlightSpan(span2.String, default, span2.Color, overlapStart, overlapEnd - overlapStart));
            }
            else
            {
                // Overlapping section
                if (overlapStart < overlapEnd)
                {
                    matchSpans.Insert(i, new TextHighlightSpan(span2.String, default, span2.Color, overlapStart, overlapEnd - overlapStart));
                    i++; // Move to the newly inserted span
                }

                // Part after the overlapping section
                if (span2.End > overlapEnd)
                {
                    matchSpans[i] = new TextHighlightSpan(span2.String, default, overlapEnd, span2.End - overlapEnd);
                }
                else
                {
                    matchSpans.RemoveAt(i);
                }
            }
        }

        private unsafe void SplitLines(StdWString* text, int currentPos, int length, List<TextHighlightSpan> outSpans)
        {
            char* pText = text->Data;
            int lineStart = 0;

            for (int index = 0; index < length; index++)
            {
                var c = pText[currentPos + index];
                if (c == '\r' || c == '\n')
                {
                    if (c == '\r' && index + 1 < length && pText[currentPos + index + 1] == '\n')
                    {
                        index++;
                    }

                    TextHighlightSpan textSpan = new(text, default, currentPos + lineStart, index - lineStart);
                    outSpans.Add(textSpan);
                    lineStart = index + 1;
                }
            }

            if (lineStart < length)
            {
                TextHighlightSpan textSpan = new(text, default, currentPos + lineStart, length - lineStart);
                if (textSpan.Length > 0)
                {
                    outSpans.Add(textSpan);
                }
            }
        }

        private static unsafe Vector2 ComputeOrigin(List<TextSpan> lines, float lineHeight, int idx)
        {
            var lineIndex = FindLineIndexOfCharacter(lines, idx);
            var line = lines[lineIndex];
            var characterIndex = FindCharacterIndexInLine(line, idx);

            int controls = 0;
            for (int i = 0; i < characterIndex; i++)
            {
                var c = line.Data[i];
                if (char.IsControl(c) && c != '\t')
                {
                    controls++;
                }
            }

            float width = 0;
            if (characterIndex >= 0)
            {
                width = ImGuiWChar.CalcTextSize(line.Data, line.Data + characterIndex).X;
            }
            width += controls * 10;

            var origin = new Vector2(width, lineIndex * lineHeight);
            return origin;
        }

        public static int FindLineIndexOfCharacter(List<TextSpan> lines, int idx)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line.Start <= idx && line.End >= idx)
                {
                    return i;
                }
            }
            return lines.Count - 1;
        }

        public static int FindCharacterIndexInLine(TextSpan line, int idx)
        {
            return idx - line.Start;
        }
    }
}