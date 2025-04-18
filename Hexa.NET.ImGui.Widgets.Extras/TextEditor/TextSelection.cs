namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor
{
    using Hexa.NET.Utilities;
    using System;

    public unsafe struct TextSelection
    {
        public StdWString* Text;
        public CursorState Start;
        public CursorState End;

        public TextSelection()
        {
            Start = CursorState.Invalid;
            End = CursorState.Invalid;
        }

        public TextSelection(StdWString* text, CursorState start, CursorState end)
        {
            Text = text;
            Start = start;
            End = end;
        }

        public static readonly TextSelection Invalid = new(null, CursorState.Invalid, CursorState.Invalid);

        public char* Data => Text->Data + Math.Min(Start.Index, End.Index);

        public readonly int Length => Math.Abs(End - Start);

        public bool IsValid()
        {
            var start = Start.Index;
            var end = End.Index;
            if (start > end)
            {
                (start, end) = (end, start);
            }
            return start >= 0 && Text != null && end <= Text->Size;
        }

        public readonly CursorState EffectiveStart => Start.Index <= End.Index ? Start : End;

        public readonly CursorState EffectiveEnd => Start.Index <= End.Index ? End : Start;
    }
}