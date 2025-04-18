namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor
{
    public readonly struct LineIndexSpan : IEquatable<LineIndexSpan>
    {
        public readonly int Start;
        public readonly int End;

        public LineIndexSpan(int start, int end)
        {
            Start = start;
            End = end;
        }

        public readonly int Length => End - Start;

        public readonly void Deconstruct(out int startSpanIndex, out int endSpanIndex)
        {
            startSpanIndex = Start;
            endSpanIndex = End;
        }

        public override bool Equals(object? obj)
        {
            return obj is LineIndexSpan span && Equals(span);
        }

        public bool Equals(LineIndexSpan other)
        {
            return Start == other.Start &&
                   End == other.End;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Start, End);
        }

        public static bool operator ==(LineIndexSpan left, LineIndexSpan right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LineIndexSpan left, LineIndexSpan right)
        {
            return !(left == right);
        }
    }
}