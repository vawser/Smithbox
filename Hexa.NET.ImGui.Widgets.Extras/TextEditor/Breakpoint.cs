namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor
{
    using System;

    public class BreakpointComparer : IComparer<Breakpoint>
    {
        public static readonly BreakpointComparer Instance = new();

        public int Compare(Breakpoint x, Breakpoint y)
        {
            return x.Line.CompareTo(y.Line);
        }
    }

    public struct Breakpoint : IEquatable<Breakpoint>
    {
        public int Line;
        public bool Enabled;

        public Breakpoint(int line, bool enabled)
        {
            Line = line;
            Enabled = enabled;
        }

        public static readonly Breakpoint Invalid = new(-1, false);

        public override readonly bool Equals(object? obj)
        {
            return obj is Breakpoint breakpoint && Equals(breakpoint);
        }

        public readonly bool Equals(Breakpoint other)
        {
            return Line == other.Line;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Line);
        }

        public static bool operator ==(Breakpoint left, Breakpoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Breakpoint left, Breakpoint right)
        {
            return !(left == right);
        }
    }
}