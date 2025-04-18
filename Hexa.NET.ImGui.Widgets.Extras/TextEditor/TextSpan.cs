namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor
{
    using Hexa.NET.Utilities;
    using System.Diagnostics;

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public unsafe struct TextSpan : IEquatable<TextSpan>
    {
        public StdWString* String;
        public int Start;
        public int End;
        public float Size;

        public TextSpan(StdWString* str, int start, int length)
        {
            String = str;
            Start = start;
            End = start + length;
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

        public override readonly bool Equals(object? obj)
        {
            return obj is TextSpan span && Equals(span);
        }

        public readonly bool Equals(TextSpan other)
        {
            return Start == other.Start &&
                   End == other.End;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Start, End);
        }

        public static bool operator ==(TextSpan left, TextSpan right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextSpan left, TextSpan right)
        {
            return !(left == right);
        }

        private readonly string GetDebuggerDisplay()
        {
            return $"{Start} .. {End} {new string(Data, 0, Length)}";
        }
    }
}