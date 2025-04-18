namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor
{
    using Hexa.NET.Utilities;
    using System;

    public unsafe struct SearchResult
    {
        public StdWString* String;
        public int Start;
        public int End;
        public int Line;
        public int Column;

        public SearchResult(StdWString* @string, int start, int len, int line, int column)
        {
            String = @string;
            Start = start;
            End = start + len;
            Line = line;
            Column = column;
        }

        public static SearchResult FromIndex(StdWString* @string, int index, int len, TextSource source)
        {
            int line = 0;
            int column = 0;
            for (; line < source.LineCount; line++)
            {
                var lineSpan = source.Lines[line];
                if (lineSpan.Start <= index && lineSpan.End >= index)
                {
                    column = index - lineSpan.Start;
                    break;
                }
            }

            return new(@string, index, len, line, column);
        }

        public readonly int Length => End - Start;

        public readonly char* Data => String->Data + Start;

        public readonly char* DataEnd => String->Data + End;

        public readonly ReadOnlySpan<char> AsReadOnlySpan()
        {
            return new ReadOnlySpan<char>(String->Data + Start, Length);
        }

        public readonly Span<char> AsSpan()
        {
            return new Span<char>(String->Data + Start, Length);
        }
    }
}