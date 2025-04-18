namespace Hexa.NET.ImGui.Widgets
{
    using Hexa.NET.ImGui;
    using System.Drawing;
    using System.Numerics;

    public static class ImGuiProgressBar
    {
        public static unsafe void ProgressBar(float value, Vector2 size, uint backgroundColor, uint foregroundColor)
        {
            ImGuiWindow* window = ImGuiP.GetCurrentWindow();
            if (window->SkipItems == 1)
            {
                return;
            }

            ImDrawList* drawList = ImGui.GetWindowDrawList();
            ImGuiContextPtr g = ImGui.GetCurrentContext();
            ImGuiStylePtr style = ImGui.GetStyle();

            var avail = ImGui.GetContentRegionAvail();

            if (size.X <= 0)
            {
                size.X += avail.X;
            }
            if (size.Y == 0)
            {
                size.Y = 10;
            }

            var pos = ImGui.GetCursorScreenPos();

            pos += style.FramePadding;
            size += style.FramePadding * 2;

            ImRect bb = new(pos, pos + size);
            ImGuiP.ItemSize(bb, style.FramePadding.Y);

            value = Clamp(value, 0f, 1f);

            // Render
            Vector2 progressBBMax = new(pos.X + size.X * value, bb.Max.Y);

            drawList->AddRectFilled(bb.Min, bb.Max, backgroundColor);
            drawList->AddRectFilled(bb.Min, progressBBMax, foregroundColor);

            uint col_b = 0x6c000000; // ABGR

            float time = (float)g.Time;
            float speed = 5f * (100 / size.X); // 500px/s
            float gradient_pos = time * speed % 1;

            float gradient_width = 40; // Adjust gradient width here
            float gradient_start_x = bb.Min.X + gradient_pos * size.X;

            Vector2 gradient_p1 = new(gradient_start_x - gradient_width, bb.Min.Y);
            Vector2 gradient_p2 = new(gradient_start_x, bb.Max.Y);
            Vector2 gradient_p3 = new(gradient_start_x, bb.Min.Y);
            Vector2 gradient_p4 = new(gradient_start_x + gradient_width, bb.Max.Y);

            ImGui.PushClipRect(bb.Min, progressBBMax, true);

            drawList->AddRectFilledMultiColor(gradient_p1, gradient_p2, 0x0, col_b, col_b, 0x0);
            drawList->AddRectFilledMultiColor(gradient_p3, gradient_p4, col_b, 0x0, 0x0, col_b);

            ImGui.PopClipRect();
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}