using System.Numerics;
using System.Text;

namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor
{
    public static partial class ImGuiWChar
    {
        public const int StackallocLimit = 2048;

        public static unsafe Vector2 CalcTextSize(char* start, char* end)
        {
            int length = (int)(end - start);

            var byteCount = Encoding.UTF8.GetByteCount(new Span<char>(start, length));

            byte* ptr;
            if (byteCount + 1 > StackallocLimit)
            {
                ptr = AllocT<byte>(byteCount + 1);
            }
            else
            {
                byte* pPtr = stackalloc byte[byteCount + 1];
                ptr = pPtr;
            }
            Encoding.UTF8.GetBytes(new Span<char>(start, length), new Span<byte>(ptr, byteCount));
            ptr[byteCount] = 0;

            Vector2 result = ImGui.CalcTextSize(ptr);

            if (byteCount + 1 > StackallocLimit)
            {
                Free(ptr);
            }

            return result;
        }

        public static unsafe void AddText(ImDrawList* drawList, Vector2 pos, uint col, char* textBegin, char* textEnd)
        {
            int length = (int)(textEnd - textBegin);

            var byteCount = Encoding.UTF8.GetByteCount(new Span<char>(textBegin, length));

            byte* ptr;
            if (byteCount + 1 > StackallocLimit)
            {
                ptr = AllocT<byte>(byteCount + 1);
            }
            else
            {
                byte* pPtr = stackalloc byte[byteCount + 1];
                ptr = pPtr;
            }
            Encoding.UTF8.GetBytes(new Span<char>(textBegin, length), new Span<byte>(ptr, byteCount));
            ptr[byteCount] = 0;

            ImGui.AddText(drawList, pos, col, ptr);

            if (byteCount + 1 > StackallocLimit)
            {
                Free(ptr);
            }
        }
    }
}