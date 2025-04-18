using Hexa.NET.ImGui.Widgets.Extensions;
using System.Numerics;
using System.Reflection.Emit;

namespace Hexa.NET.ImGui.Widgets
{
    public static unsafe class ImGuiSplitter
    {
        public static bool VerticalSplitter(ReadOnlySpan<byte> strId, ref float width)
        {
            return VerticalSplitter(strId, ref width, float.MinValue, float.MaxValue, 0, 2, 8);
        }

        public static bool VerticalSplitter(ReadOnlySpan<byte> strId, ref float width, float minWidth, float maxWidth)
        {
            return VerticalSplitter(strId, ref width, minWidth, maxWidth, 0, 2, 8);
        }

        public static bool VerticalSplitter(ReadOnlySpan<byte> strId, ref float width, float minWidth, float maxWidth, float height)
        {
            return VerticalSplitter(strId, ref width, minWidth, maxWidth, height, 2, 8);
        }

        public static bool VerticalSplitter(ReadOnlySpan<byte> strId, ref float width, float minWidth, float maxWidth, float height, float thickness, float tolerance)
        {
            fixed (byte* pStrId0 = strId)
            {
                return VerticalSplitter(pStrId0, ref width, minWidth, maxWidth, height, thickness, tolerance);
            }
        }

        public static bool VerticalSplitter(string strId, ref float width)
        {
            return VerticalSplitter(strId, ref width, float.MinValue, float.MaxValue, 0, 2, 8);
        }

        public static bool VerticalSplitter(string strId, ref float width, float minWidth, float maxWidth)
        {
            return VerticalSplitter(strId, ref width, minWidth, maxWidth, 0, 2, 8);
        }

        public static bool VerticalSplitter(string strId, ref float width, float minWidth, float maxWidth, float height)
        {
            return VerticalSplitter(strId, ref width, minWidth, maxWidth, height, 2, 8);
        }

        public static bool VerticalSplitter(string strId, ref float width, float minWidth, float maxWidth, float height, float thickness, float tolerance)
        {
            int sizeInBytes0 = System.Text.Encoding.UTF8.GetByteCount(strId);
            byte* pStrId0;
            if (sizeInBytes0 + 1 >= 2048)
            {
                pStrId0 = AllocT<byte>(sizeInBytes0 + 1);
            }
            else
            {
                byte* stackLabel = stackalloc byte[sizeInBytes0 + 1];
                pStrId0 = stackLabel;
            }
            System.Text.Encoding.UTF8.GetBytes(strId.AsSpan(), new Span<byte>(pStrId0, sizeInBytes0));
            pStrId0[sizeInBytes0] = 0;

            bool result = VerticalSplitter(pStrId0, ref width, minWidth, maxWidth, height, thickness, tolerance);

            if (sizeInBytes0 + 1 >= 2048)
            {
                Free(pStrId0);
            }

            return result;
        }

        public static bool VerticalSplitter(byte* strId, ref float width)
        {
            return VerticalSplitter(strId, ref width, float.MinValue, float.MaxValue, 0, 2, 8);
        }

        public static bool VerticalSplitter(byte* strId, ref float width, float minWidth, float maxWidth)
        {
            return VerticalSplitter(strId, ref width, minWidth, maxWidth, 0, 2, 8);
        }

        public static bool VerticalSplitter(byte* strId, ref float width, float minWidth, float maxWidth, float height)
        {
            return VerticalSplitter(strId, ref width, minWidth, maxWidth, height, 2, 8);
        }

        public static bool VerticalSplitter(byte* strId, ref float width, float minWidth, float maxWidth, float height, float thickness, float tolerance)
        {
            ImGuiWindow* window = ImGuiP.GetCurrentWindow();
            if (window->SkipItems == 1)
            {
                return false;
            }

            ImGui.SameLine();

            var origin = ImGui.GetCursorScreenPos();
            var avail = ImGui.GetContentRegionAvail();
            if (height <= 0)
            {
                height += avail.Y;
            }

            Vector2 min = origin;
            Vector2 max = origin + new Vector2(thickness, height);
            ImRect bb = new() { Max = max, Min = min };
            ImRect bbTolerance = new() { Max = max + new Vector2(tolerance, 0), Min = min + new Vector2(-tolerance, 0) };

            uint id = ImGui.GetID(strId);

            ImGuiP.ItemSize(bb);
            if (!ImGuiP.ItemAdd(bb, id, &bbTolerance))
            {
                return false;
            }

            var io = ImGui.GetIO();

            var drawList = ImGui.GetWindowDrawList();
            bool hovered;
            bool held;
            ImGuiP.ButtonBehavior(bbTolerance, id, &hovered, &held, ImGuiButtonFlags.None);

            uint color = ImGui.GetColorU32(ImGuiCol.Separator);
            if (hovered)
            {
                color = ImGui.GetColorU32(ImGuiCol.SeparatorHovered);

                if (hovered)
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEw);
                }
            }

            if (held)
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEw);
                color = ImGui.GetColorU32(ImGuiCol.SeparatorActive);
                width += io.MouseDelta.X;
                if (width < minWidth) width = minWidth;
                if (width > maxWidth) width = maxWidth;
            }

            drawList.AddRectFilled(min, max, color);

            ImGui.SameLine(); // Do it automatically for the user, reduces boilerplate.

            return held;
        }

        public static bool HorizontalSplitter(ReadOnlySpan<byte> strId, ref float height)
        {
            return HorizontalSplitter(strId, ref height, float.MinValue, float.MaxValue, 0, 2, 8);
        }

        public static bool HorizontalSplitter(ReadOnlySpan<byte> strId, ref float height, float minHeight, float maxHeight)
        {
            return HorizontalSplitter(strId, ref height, minHeight, maxHeight, 0, 2, 8);
        }

        public static bool HorizontalSplitter(ReadOnlySpan<byte> strId, ref float height, float minHeight, float maxHeight, float width)
        {
            return HorizontalSplitter(strId, ref height, minHeight, maxHeight, width, 2, 8);
        }

        public static bool HorizontalSplitter(ReadOnlySpan<byte> strId, ref float height, float minHeight, float maxHeight, float width, float thickness, float tolerance)
        {
            fixed (byte* pStrId = strId)
            {
                return HorizontalSplitter(pStrId, ref height, minHeight, maxHeight, width, thickness, tolerance);
            }
        }

        public static bool HorizontalSplitter(string strId, ref float height)
        {
            return HorizontalSplitter(strId, ref height, float.MinValue, float.MaxValue, 0, 2, 8);
        }

        public static bool HorizontalSplitter(string strId, ref float height, float minHeight, float maxHeight)
        {
            return HorizontalSplitter(strId, ref height, minHeight, maxHeight, 0, 2, 8);
        }

        public static bool HorizontalSplitter(string strId, ref float height, float minHeight, float maxHeight, float width)
        {
            return HorizontalSplitter(strId, ref height, minHeight, maxHeight, width, 2, 8);
        }

        public static bool HorizontalSplitter(string strId, ref float height, float minHeight, float maxHeight, float width, float thickness, float tolerance)
        {
            int sizeInBytes0 = System.Text.Encoding.UTF8.GetByteCount(strId);
            byte* pStrId0;
            if (sizeInBytes0 + 1 >= 2048)
            {
                pStrId0 = AllocT<byte>(sizeInBytes0 + 1);
            }
            else
            {
                byte* stackLabel = stackalloc byte[sizeInBytes0 + 1];
                pStrId0 = stackLabel;
            }
            System.Text.Encoding.UTF8.GetBytes(strId.AsSpan(), new Span<byte>(pStrId0, sizeInBytes0));
            pStrId0[sizeInBytes0] = 0;

            bool result = HorizontalSplitter(pStrId0, ref height, minHeight, maxHeight, width, thickness, tolerance);

            if (sizeInBytes0 + 1 >= 2048)
            {
                Free(pStrId0);
            }

            return result;
        }

        public static bool HorizontalSplitter(byte* strId, ref float height)
        {
            return HorizontalSplitter(strId, ref height, float.MinValue, float.MaxValue, 0, 2, 8);
        }

        public static bool HorizontalSplitter(byte* strId, ref float height, float minHeight, float maxHeight)
        {
            return HorizontalSplitter(strId, ref height, minHeight, maxHeight, 0, 2, 8);
        }

        public static bool HorizontalSplitter(byte* strId, ref float height, float minHeight, float maxHeight, float width)
        {
            return HorizontalSplitter(strId, ref height, minHeight, maxHeight, width, 2, 8);
        }

        public static bool HorizontalSplitter(byte* strId, ref float height, float minHeight, float maxHeight, float width, float thickness, float tolerance)
        {
            ImGuiWindow* window = ImGuiP.GetCurrentWindow();
            if (window->SkipItems == 1)
            {
                return false;
            }

            var origin = ImGui.GetCursorScreenPos();
            var avail = ImGui.GetContentRegionAvail();
            if (width <= 0)
            {
                width += avail.X;
            }

            Vector2 min = origin;
            Vector2 max = origin + new Vector2(width, thickness);
            ImRect bb = new() { Max = max, Min = min };
            ImRect bbTolerance = new() { Max = max + new Vector2(0, tolerance), Min = min + new Vector2(0, -tolerance) };

            uint id = ImGui.GetID(strId);

            ImGuiP.ItemSize(bb, 0);
            if (!ImGuiP.ItemAdd(bb, id, &bbTolerance, ImGuiItemFlags.None))
            {
                return false;
            }

            var io = ImGui.GetIO();
            var drawList = ImGui.GetWindowDrawList();
            bool hovered;
            bool held;
            ImGuiP.ButtonBehavior(bbTolerance, id, &hovered, &held, ImGuiButtonFlags.None);

            uint color = ImGui.GetColorU32(ImGuiCol.Separator);
            if (hovered)
            {
                color = ImGui.GetColorU32(ImGuiCol.SeparatorHovered);
                if (hovered)
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeNs);
                }
            }

            if (held)
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeNs);
                color = ImGui.GetColorU32(ImGuiCol.SeparatorActive);
                height -= io.MouseDelta.Y;
                if (height < minHeight) height = minHeight;
                if (height > maxHeight) height = maxHeight;
            }

            drawList.AddRectFilled(min, max, color);

            return held;
        }
    }
}