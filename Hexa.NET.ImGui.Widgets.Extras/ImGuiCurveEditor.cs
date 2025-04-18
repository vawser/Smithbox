namespace Hexa.NET.ImGui.Widgets.Extras
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Mathematics;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public static unsafe class ImGuiCurveEditor
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Remap(float v, float a, float b, float c, float d)
        {
            return c + (d - c) * (v - a) / (b - a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 Remap(Vector2 v, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            return new Vector2(Remap(v.X, a.X, b.X, c.X, d.X), Remap(v.Y, a.Y, b.Y, c.Y, d.Y));
        }

        public static bool Curve(string label, Vector2 size, ref Curve curve, Vector2 rangeMin, Vector2 rangeMax, ref int selection)
        {
            return Curve(label, size, ref curve, rangeMin, rangeMax, (int*)Unsafe.AsPointer(ref selection));
        }

        public static bool Curve(string label, Vector2 size, ref Curve curve, Vector2 rangeMin, Vector2 rangeMax, int* selection)
        {
            ImGuiWindow* window = ImGuiP.GetCurrentWindow();

            uint id = ImGuiP.GetID(window, label, (byte*)null);
            if (window->SkipItems != 0)
                return false;

            var cursorPos = ImGui.GetCursorScreenPos();

            ImRect bb = new()
            {
                Min = cursorPos,
                Max = cursorPos + size
            };

            ImGuiP.ItemSize(bb, -1);
            if (!ImGuiP.ItemAdd(bb, id, &bb, 0))
                return false;

            ImGui.PushID(label);

            bool hovered = ImGuiP.ItemHoverable(bb, id, 0);

            ImGuiStylePtr style = ImGui.GetStyle();
            ImGuiIOPtr io = ImGui.GetIO();
            ImGuiP.RenderFrame(bb.Min, bb.Max, ImGui.GetColorU32(ImGuiCol.FrameBg, 1), true, style.FrameRounding);

            Vector2 canvas = new(bb.Max.X - bb.Min.X, bb.Max.Y - bb.Min.Y);

            int hoveredPoint = -1;

            int currentSelection = selection != null ? *selection : -1;

            bool modified = false;

            const float pointRadiusInPixels = 5.0f;

            var mousePos = ImGui.GetMousePos();

            bool draggingPoint = ImGui.IsMouseDragging(0) && currentSelection != -1;

            if (hovered)
            {
                if (draggingPoint)
                {
                    if (selection != null)
                        ImGuiP.SetActiveID(id, window);

                    ImGuiP.SetFocusID(id, window);
                    ImGuiP.FocusWindow(window, ImGuiFocusRequestFlags.None);

                    modified = true;

                    Vector2 pos = (io.MousePos - bb.Min) / (bb.Max - bb.Min);

                    // constrain Y to min/max
                    pos.Y = 1.0f - pos.Y;
                    pos = Remap(pos, Vector2.Zero, Vector2.One, rangeMin, rangeMax);

                    float pointXRangeMin = currentSelection > 0 ? curve.Points[currentSelection - 1].X : rangeMin.X;
                    float pointXRangeMax = currentSelection + 1 < curve.Points.Count ? curve.Points[currentSelection + 1].X : rangeMax.X;

                    pos = Vector2.Clamp(pos, new Vector2(pointXRangeMin, rangeMin.Y), new Vector2(pointXRangeMax, rangeMax.Y));

                    var point = curve.Points[currentSelection];
                    point.Pos = pos;
                    curve.Points[currentSelection] = point;
                }
                else if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    currentSelection = -1;
                }
            }

            if (!ImGui.IsMouseDragging(0) && ImGuiP.GetActiveID() == id && selection != null && *selection != -1 && currentSelection == -1)
            {
                ImGuiP.ClearActiveID();
            }

            uint gridColor1 = ImGui.GetColorU32(ImGuiCol.TextDisabled, 0.15f);
            uint gridColor2 = ImGui.GetColorU32(ImGuiCol.TextDisabled, 0.05f);

            ImDrawList* drawList = ImGui.GetWindowDrawList();

            // bg grid
            {
                int horizontalLinesCount = 16;
                int verticalLinesCount = 16;

                float horizontalStep = canvas.X / horizontalLinesCount;
                float verticalStep = canvas.Y / verticalLinesCount;

                for (int j = 1; j < horizontalLinesCount; j++)
                {
                    float yPos = bb.Min.Y + horizontalStep * j;
                    drawList->AddLine(new Vector2(bb.Min.X, yPos), new Vector2(bb.Max.X, yPos), gridColor1, j % 4 == 0 ? 3 : 1);
                }

                for (int ii = 1; ii < verticalLinesCount; ii++)
                {
                    float xPos = bb.Min.X + verticalStep * ii;
                    drawList->AddLine(new Vector2(xPos, bb.Min.Y), new Vector2(xPos, bb.Max.Y), gridColor2, ii % 4 == 0 ? 3 : 1);
                }
            }

            Mathematics.Curve.CalculateCurve(ref curve);

            var curveColor = ImGui.GetColorU32(ImGuiCol.PlotLines);

            for (int i = 0; i < curve.Samples.Length - 1; i++)
            {
                float x0 = i / (float)(curve.Samples.Length - 1);
                float x1 = (i + 1) / (float)(curve.Samples.Length - 1);
                float sample0 = curve.Samples[i];
                float sample1 = curve.Samples[i + 1];

                sample0 = Remap(sample0, rangeMin.Y, rangeMax.Y, 0, 1);
                sample1 = Remap(sample1, rangeMin.Y, rangeMax.Y, 0, 1);

                Vector2 gp0 = bb.Min + new Vector2(x0 * size.X, (1 - sample0) * size.Y);
                Vector2 gp1 = bb.Min + new Vector2(x1 * size.X, (1 - sample1) * size.Y);

                drawList->AddLine(gp0, gp1, curveColor);
            }

            for (int i = 0; i < curve.Points.Count; i++)
            {
                Vector2 p = curve.Points[i].Pos;
                p = Remap(p, rangeMin, rangeMax, Vector2.Zero, Vector2.One);

                Vector2 pos = bb.Min + new Vector2(p.X * size.X, (1 - p.Y) * size.Y);

                Vector2 hoverPos = mousePos;

                uint color = ImGui.GetColorU32(ImGuiCol.PlotLines);

                float distance = Vector2.Distance(pos, hoverPos);

                if (distance <= pointRadiusInPixels)
                {
                    hoveredPoint = i;

                    color = 0xFF00FFFF;

                    if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && currentSelection == -1)
                    {
                        currentSelection = i;
                    }
                }

                drawList->AddCircleFilled(pos, pointRadiusInPixels, color);
            }

            if (hovered && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                if (currentSelection == -1 && !draggingPoint)
                {
                    Vector2 pos = (io.MousePos - bb.Min) / (bb.Max - bb.Min);

                    // constrain Y to min/max
                    pos.Y = 1.0f - pos.Y;
                    pos = Remap(pos, Vector2.Zero, Vector2.One, rangeMin, rangeMax);

                    int key = 0;
                    while (curve.Points[key].X < pos.X && key != curve.Points.Count)
                    {
                        key++;
                    }

                    if (key == curve.Points.Count)
                    {
                        curve.Points.Add(new(pos, CurvePointType.Smooth));
                    }
                    else
                    {
                        curve.Points.Insert(key, new(pos, CurvePointType.Smooth));
                    }

                    modified = true;
                }
                else if (currentSelection != -1 && !draggingPoint)
                {
                    curve.Points.RemoveAt(currentSelection);
                    modified = true;
                }
            }

            // draw the text at mouse position

            if (hovered && draggingPoint)
            {
                byte* buf = stackalloc byte[128];
                Vector2 pos = (io.MousePos - bb.Min) / (bb.Max - bb.Min);
                pos.Y = 1.0f - pos.Y;

                pos.X = MathUtil.Lerp(rangeMin.X, rangeMax.X, pos.X);
                pos.Y = MathUtil.Lerp(rangeMin.Y, rangeMax.Y, pos.Y);

                ImGuiP.RenderTextClipped(new Vector2(bb.Min.X, bb.Min.Y + style.FramePadding.Y), bb.Max, $"({pos.X:N2},{pos.Y:N2})", (byte*)null, (Vector2*)null, new Vector2(1f, 1f), &bb);
            }

            ImGui.PopID();

            if (selection != null)
            {
                *selection = currentSelection;
            }

            return modified;
        }
    }
}