namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor
{
    public struct CursorState
    {
        public int Index;
        public int Line;
        public int Column;

        public CursorState(int index, int line, int column)
        {
            Index = index;
            Line = line;
            Column = column;
        }

        public static readonly CursorState NewLineLF = new(1, 1, 0);
        public static readonly CursorState NewLineCR = new(1, 1, 0);
        public static readonly CursorState NewLineCRLF = new(2, 1, 0);
        public static readonly CursorState Invalid = new(-1, -1, -1);

        public static CursorState FromOffset(int offset)
        {
            return new CursorState(offset, 0, offset);
        }

        public static CursorState FromIndex(int index, TextSource source)
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

            return new(index, line, column);
        }

        public static CursorState FromLineColumn(int line, int column, TextSource source)
        {
            var lineSpan = source.Lines[line];
            var index = lineSpan.Start + column;
            return new(index, line, column);
        }

        public static CursorState operator ++(CursorState state)
        {
            int newIndex = state.Index + 1;
            int newColumn = state.Column + 1;
            return new(newIndex, state.Line, newColumn);
        }

        public static CursorState operator --(CursorState state)
        {
            int newLine = state.Line;
            int newColumn = state.Column - 1;
            if (newColumn < 0)
            {
                newLine--;
                newColumn = 0;
            }
            return new(state.Index - 1, newLine, newColumn);
        }

        public static CursorState operator +(CursorState a, CursorState b)
        {
            return new CursorState(a.Index + b.Index, a.Line + b.Line, a.Column + b.Column);
        }

        public static CursorState operator -(CursorState a, CursorState b)
        {
            return new CursorState(a.Index - b.Index, a.Line - b.Line, a.Column - b.Column);
        }

        public static implicit operator int(CursorState state) => state.Index;
    }
}