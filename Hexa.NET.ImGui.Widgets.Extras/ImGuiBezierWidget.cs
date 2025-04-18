// https://github.com/GavinRay97/reaimgui-extra-personal/
// No Licence provided. :(
//
// Modified and ported by Juna Meinhold :3.

namespace Hexa.NET.ImGui.Widgets.Extras
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Mathematics;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides utility methods for rendering and interacting with Bezier curves in ImGui.
    /// </summary>
    public static unsafe class ImGuiBezierWidget
    {
        private const int SMOOTHNESS = 64; // curve smoothness: the higher number of segments, the smoother curve

        private static readonly BezierTable BezierTable = new(SMOOTHNESS);

        private static readonly (string, Vector4)[] presets = [
            ("Linear", new Vector4(0.000f, 0.000f, 1.000f, 1.000f)),
            ("In Sine", new Vector4(0.470f, 0.000f, 0.745f, 0.715f)),
            ("In Quad", new Vector4(0.550f, 0.085f, 0.680f, 0.530f)),
            ("In Cubic", new Vector4(0.550f, 0.055f, 0.675f, 0.190f)),
            ("In Quart", new Vector4(0.895f, 0.030f, 0.685f, 0.220f)),
            ("In Quint", new Vector4(0.755f, 0.050f, 0.855f, 0.060f)),
            ("In Expo", new Vector4(0.950f, 0.050f, 0.795f, 0.035f)),
            ("In Circ", new Vector4(0.600f, 0.040f, 0.980f, 0.335f)),
            ("In Back", new Vector4(0.600f, -0.28f, 0.735f, 0.045f)),

            ("Out Sine", new Vector4(0.390f, 0.575f, 0.565f, 1.000f)),
            ("Out Quad", new Vector4(0.250f, 0.460f, 0.450f, 0.940f)),
            ("Out Cubic", new Vector4(0.215f, 0.610f, 0.355f, 1.000f)),
            ("Out Quart", new Vector4(0.165f, 0.840f, 0.440f, 1.000f)),
            ("Out Quint", new Vector4(0.230f, 1.000f, 0.320f, 1.000f)),
            ("Out Expo", new Vector4(0.190f, 1.000f, 0.220f, 1.000f)),
            ("Out Circ", new Vector4(0.075f, 0.820f, 0.165f, 1.000f)),
            ("Out Back", new Vector4(0.175f, 0.885f, 0.320f, 1.275f)),

            ("InOut Sine", new Vector4(0.445f, 0.050f, 0.550f, 0.950f)),
            ("InOut Quad", new Vector4(0.455f, 0.030f, 0.515f, 0.955f)),
            ("InOut Cubic", new Vector4(0.645f, 0.045f, 0.355f, 1.000f)),
            ("InOut Quart", new Vector4(0.770f, 0.000f, 0.175f, 1.000f)),
            ("InOut Quint", new Vector4(0.860f, 0.000f, 0.070f, 1.000f)),
            ("InOut Expo", new Vector4(1.000f, 0.000f, 0.000f, 1.000f)),
            ("InOut Circ", new Vector4(0.785f, 0.135f, 0.150f, 0.860f)),
            ("InOut Back", new Vector4(0.680f, -0.55f, 0.265f, 1.550f))];

        /// <summary>
        /// Creates an ImGui Bezier widget for interactive curve editing.
        /// </summary>
        /// <param name="label">The label for the widget.</param>
        /// <param name="P">The reference to the BezierCurve that represents control points.</param>
        /// <param name="size">The size of the widget.</param>
        /// <param name="curveWidth">The width of the main curved line.</param>
        /// <param name="lineWidth">The width of handlers' small lines.</param>
        /// <param name="grabRadius">The radius of the handlers' circle.</param>
        /// <param name="grabBorder">The border width of handlers' circle.</param>
        /// <param name="areaConstrained">Flag indicating if grabbers should be constrained to the grid area.</param>
        /// <returns>True if the Bezier curve is changed, false otherwise.</returns>
        public static bool Bezier(string label, ref BezierCurve P, float size = 128, float curveWidth = 4, float lineWidth = 1, float grabRadius = 8, float grabBorder = 2, bool areaConstrained = true)
        {
            // visuals
            ImGuiStylePtr Style = ImGui.GetStyle();
            // ImGuiIOPtr IO = ImGui.GetIO();
            ImDrawList* DrawList = ImGui.GetWindowDrawList();
            ImGuiWindow* Window = ImGuiP.GetCurrentWindow();
            if (Window->SkipItems != 0)
                return false;

            // header and spacing
            bool changed = ImGui.SliderFloat4(label, (float*)Unsafe.AsPointer(ref P), 0, 1, "%.3f");
            bool hovered = ImGui.IsItemActive() || ImGui.IsItemHovered(); // IsItemDragged() ?
            ImGui.Dummy(new(0, 3));

            // prepare canvas
            float avail = ImGui.GetContentRegionAvail().X;
            float dim = size > 0 ? size : avail;
            Vector2 Canvas = new(dim, dim);

            var cursorPos = ImGui.GetCursorPos();

            ImRect bb = new()
            {
                Min = Window->DC.CursorPos + cursorPos,
                Max = Window->DC.CursorPos + cursorPos + Canvas
            };

            ImGuiP.ItemSize(bb, -1);
            if (!ImGuiP.ItemAdd(bb, 0, null, ImGuiItemFlags.None))
                return changed;

            uint id = ImGuiP.GetID(Window, label, (byte*)null);
            hovered |= ImGuiP.ItemHoverable(new ImRect() { Min = bb.Min - new Vector2(grabRadius), Max = bb.Min + new Vector2(avail, dim) + new Vector2(grabRadius) }, id, ImGuiItemFlags.None);

            ImGuiP.RenderFrame(bb.Min, bb.Max, ImGui.GetColorU32(ImGuiCol.FrameBg, 1), true, Style.FrameRounding);

            // background grid
            for (int i = 0; i <= Canvas.X; i += (int)(Canvas.X / 4))
            {
                DrawList->AddLine(
                    new Vector2(bb.Min.X + i, bb.Min.Y),
                    new Vector2(bb.Min.X + i, bb.Max.Y),
                 ImGui.GetColorU32(ImGuiCol.TextDisabled));
            }
            for (int i = 0; i <= Canvas.Y; i += (int)(Canvas.Y / 4))
            {
                DrawList->AddLine(
                    new Vector2(bb.Min.X, bb.Min.Y + i),
                    new Vector2(bb.Max.X, bb.Min.Y + i),
                ImGui.GetColorU32(ImGuiCol.TextDisabled));
            }

            // eval curve
            Vector2* Q = stackalloc Vector2[] { new Vector2(0, 0), P[0], P[1], new Vector2(1, 1) };
            Vector2* results = stackalloc Vector2[SMOOTHNESS + 1];
            BezierTable.Compute(Q, results);

            // control points: 2 lines and 2 circles
            {
                // handle grabbers
                Vector2 mouse = ImGui.GetIO().MousePos;
                Vector2* pos = stackalloc Vector2[2];
                float* distance = stackalloc float[2];

                for (int i = 0; i < 2; ++i)
                {
                    pos[i] = new Vector2(P[i].X, 1 - P[i].Y) * (bb.Max - bb.Min) + bb.Min;
                    distance[i] = (pos[i].X - mouse.X) * (pos[i].X - mouse.X) + (pos[i].Y - mouse.Y) * (pos[i].Y - mouse.Y);
                }

                int selected = distance[0] < distance[1] ? 0 : 1;
                if (distance[selected] < 4 * grabRadius * 4 * grabRadius)
                {
                    ImGui.SetTooltip($"({P[selected].X}, {P[selected].Y})");

                    if (/*hovered &&*/ ImGui.IsMouseClicked(0) || ImGui.IsMouseDragging(0))
                    {
                        float canvasScale = 1.0f / ImGui.GetFontSize();
                        float px = P[selected].X += ImGui.GetIO().MouseDelta.X / Canvas.X;
                        float py = P[selected].Y -= ImGui.GetIO().MouseDelta.Y / Canvas.Y;

                        if (areaConstrained)
                        {
                            P[selected].X = px < 0 ? 0 : px > 1 ? 1 : px;
                            P[selected].Y = py < 0 ? 0 : py > 1 ? 1 : py;
                        }

                        changed = true;
                    }
                }
            }

            // if (hovered || changed) DrawList->PushClipRectFullScreen();

            // draw curve
            {
                Vector4 color = Style.Colors[(int)ImGuiCol.PlotLines];
                for (int i = 0; i < SMOOTHNESS; ++i)
                {
                    Vector2 p = new(results[i + 0].X, 1 - results[i + 0].Y);
                    Vector2 q = new(results[i + 1].X, 1 - results[i + 1].Y);
                    Vector2 r = new(p.X * (bb.Max.X - bb.Min.X) + bb.Min.X, p.Y * (bb.Max.Y - bb.Min.Y) + bb.Min.Y);
                    Vector2 s = new(q.X * (bb.Max.X - bb.Min.X) + bb.Min.X, q.Y * (bb.Max.Y - bb.Min.Y) + bb.Min.Y);
                    DrawList->AddLine(r, s, ImGui.ColorConvertFloat4ToU32(color), curveWidth);
                }
            }

            {
                // draw lines and grabbers
                float luma = ImGui.IsItemActive() || ImGui.IsItemHovered() ? 0.5f : 1.0f;
                Vector4 white = ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
                Vector4 pink = new(1.00f, 0.00f, 0.75f, luma), cyan = new(0.00f, 0.75f, 1.00f, luma);
                Vector2 p1 = new Vector2(P[0].X, 1 - P[0].Y) * (bb.Max - bb.Min) + bb.Min;
                Vector2 p2 = new Vector2(P[1].X, 1 - P[1].Y) * (bb.Max - bb.Min) + bb.Min;
                DrawList->AddLine(new Vector2(bb.Min.X, bb.Max.Y), p1, ImGui.ColorConvertFloat4ToU32(white), lineWidth);
                DrawList->AddLine(new Vector2(bb.Max.X, bb.Min.Y), p2, ImGui.ColorConvertFloat4ToU32(white), lineWidth);
                DrawList->AddCircleFilled(p1, grabRadius, ImGui.ColorConvertFloat4ToU32(white));
                DrawList->AddCircleFilled(p1, grabRadius - grabBorder, ImGui.ColorConvertFloat4ToU32(pink));
                DrawList->AddCircleFilled(p2, grabRadius, ImGui.ColorConvertFloat4ToU32(white));
                DrawList->AddCircleFilled(p2, grabRadius - grabBorder, ImGui.ColorConvertFloat4ToU32(cyan));
            }

            // if (hovered || changed) DrawList->PopClipRect();

            return changed;
        }
    }
}