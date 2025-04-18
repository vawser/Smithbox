namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Utilities;
    using System.Numerics;

    public unsafe struct TextHighlightSpan
    {
        public StdWString* String;
        public Vector2 Origin;
        public int Start;
        public int End;
        public float Size;
        public uint Color;
        public bool HasColor;
        public bool Control;

        public TextHighlightSpan(StdWString* str, Vector2 origin, ColorRGBA color, int start, int length, bool control = false)
        {
            String = str;
            Origin = origin;
            Start = start;
            End = start + length;
            Color = ImGui.ColorConvertFloat4ToU32(color);
            HasColor = true;
            Control = control;
        }

        public TextHighlightSpan(StdWString* str, Vector2 origin, ColorRGBA color, bool hasColor, int start, int length)
        {
            String = str;
            Origin = origin;
            Start = start;
            End = start + length;
            Color = ImGui.ColorConvertFloat4ToU32(color);
            HasColor = hasColor;
        }

        public TextHighlightSpan(StdWString* str, Vector2 origin, int start, int length)
        {
            String = str;
            Origin = origin;
            Start = start;
            End = start + length;
            HasColor = false;
        }

        public readonly int Length => End - Start;

        public readonly char* Data => String->Data + Start;

        public readonly ReadOnlySpan<char> AsReadOnlySpan()
        {
            return new ReadOnlySpan<char>(String->Data + Start, Length);
        }

        public readonly Span<char> AsSpan()
        {
            return new Span<char>(String->Data + Start, Length);
        }

        public override readonly string ToString()
        {
            return $"[{Start}-{End}] {Color:X}, {new string(Data, 0, Length)}";
        }
    }
}