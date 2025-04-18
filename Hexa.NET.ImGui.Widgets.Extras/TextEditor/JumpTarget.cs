using Hexa.NET.ImGui;

namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor
{
    public struct JumpTarget
    {
        public int Index;
        public int Length;
        public int Line;
        public int Column;
        public ImGuiScrollFlags Flags;

        public JumpTarget(int index, int line, int column, ImGuiScrollFlags flags)
        {
            Index = index;
            Line = line;
            Column = column;
            Flags = flags;
        }

        public JumpTarget(int index, int length, int line, int column, ImGuiScrollFlags flags)
        {
            Index = index;
            Length = length;
            Line = line;
            Column = column;

            Flags = flags;
        }
    }
}