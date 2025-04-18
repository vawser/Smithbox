//
// Copyright(c) 2019 Sandy Carter
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// Modified and ported by Juna Meinhold :3.

namespace Hexa.NET.ImGui.Widgets
{
    using Hexa.NET.ImGui;
    using System.Numerics;

    /// <summary>
    /// A utility class for rendering flame graph-style visualizations using ImGui.
    /// </summary>
    public static unsafe class ImGuiWidgetFlameGraph
    {
        /// <summary>
        /// A delegate that provides values for the flame graph rendering.
        /// </summary>
        /// <param name="start">Pointer to the start value.</param>
        /// <param name="end">Pointer to the end value.</param>
        /// <param name="level">Pointer to the level value.</param>
        /// <param name="caption">Pointer to the caption.</param>
        /// <param name="data">The data source.</param>
        /// <param name="idx">The index of the data.</param>
        public delegate void ValuesGetter(float* start, float* end, byte* level, byte** caption, void* data, int idx);

        /// <summary>
        /// Renders a flame graph using ImGui.
        /// </summary>
        /// <param name="label">The label for the flame graph.</param>
        /// <param name="valuesGetter">A delegate to retrieve values for rendering.</param>
        /// <param name="data">The data source.</param>
        /// <param name="valuesCount">The count of values to render.</param>
        /// <param name="selected">The index of the selected value (optional).</param>
        /// <param name="flip">Whether to flip the graph (optional).</param>
        /// <param name="valuesOffset">The offset for values (optional).</param>
        /// <param name="overlayText">Text overlay for the graph (optional).</param>
        /// <param name="scaleMin">Minimum scale for the graph (optional).</param>
        /// <param name="scaleMax">Maximum scale for the graph (optional).</param>
        /// <param name="graphSize">Size of the graph (optional).</param>
        public static void PlotFlame(string label, ValuesGetter valuesGetter, void* data, int valuesCount, ref int selected, bool flip = false, int valuesOffset = 0, string? overlayText = null, float scaleMin = float.MaxValue, float scaleMax = float.MaxValue, Vector2 graphSize = default)
        {
            ImGuiWindow* window = ImGuiP.GetCurrentWindow();
            if (window->SkipItems == 1)
                return;

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            ImGuiContextPtr g = ImGui.GetCurrentContext();
            ImGuiStylePtr style = &((ImGuiContext*)g)->Style;

            if (selected != -1)
            {
                valuesGetter(&scaleMin, &scaleMax, null, null, data, selected);
            }

            // Find the maximum depth
            byte maxDepth = 0;
            for (int i = valuesOffset; i < valuesCount; ++i)
            {
                byte depth;
                valuesGetter(null, null, &depth, null, data, i);
                maxDepth = Math.Max(maxDepth, depth);
            }

            float blockHeight = ImGui.GetTextLineHeight() + style.FramePadding.Y * 2;
            Vector2 labelSize = ImGui.CalcTextSize(label, (byte*)null, true);
            if (graphSize.X == 0.0f)
                graphSize.X = ImGui.GetContentRegionAvail().X - (labelSize.X + style.FramePadding.X * 3);
            if (graphSize.Y == 0.0f)
                graphSize.Y = labelSize.Y + style.FramePadding.Y * 3 + blockHeight * (maxDepth + 1);

            ImRect frameBB = new() { Min = window->DC.CursorPos, Max = window->DC.CursorPos + graphSize };
            ImRect innerBB = new() { Min = frameBB.Min + style.FramePadding, Max = frameBB.Max - style.FramePadding };
            ImRect totalBB = new() { Min = frameBB.Min, Max = frameBB.Max + new Vector2(labelSize.X > 0.0f ? style.ItemInnerSpacing.X + labelSize.X : 0.0f, 0) };
            ImGuiP.ItemSize(totalBB, style.FramePadding.Y);
            if (!ImGuiP.ItemAdd(totalBB, 0, &frameBB, ImGuiItemFlags.None))
                return;

            // Determine scale from values if not specified
            if (scaleMin == float.MaxValue || scaleMax == float.MaxValue)
            {
                float vMin = float.MaxValue;
                float vMax = -float.MaxValue;
                for (int i = valuesOffset; i < valuesCount; i++)
                {
                    float v_start, v_end;
                    valuesGetter(&v_start, &v_end, null, null, data, i);
                    if (!float.IsNaN(v_start)) // Check non-NaN values
                        vMin = Math.Min(vMin, v_start);
                    if (!float.IsNaN(v_end)) // Check non-NaN values
                        vMax = Math.Max(vMax, v_end);
                }
                if (scaleMin == float.MaxValue)
                    scaleMin = vMin;
                if (scaleMax == float.MaxValue)
                    scaleMax = vMax;
            }

            ImGuiP.RenderFrame(frameBB.Min, frameBB.Max, ImGui.GetColorU32(ImGuiCol.FrameBg), true, style.FrameRounding);

            bool any_hovered = false;
            if (valuesCount - valuesOffset >= 1)
            {
                uint col_base = ImGui.GetColorU32(ImGuiCol.PlotHistogram) & 0x77FFFFFF;
                uint col_hovered = ImGui.GetColorU32(ImGuiCol.PlotHistogramHovered) & 0x77FFFFFF;
                uint col_selected = ImGui.GetColorU32(ImGuiCol.TextSelectedBg) & 0x77FFFFFF;
                uint col_outline_base = ImGui.GetColorU32(ImGuiCol.PlotHistogram) & 0x7FFFFFFF;
                uint col_outline_hovered = ImGui.GetColorU32(ImGuiCol.PlotHistogramHovered) & 0x7FFFFFFF;

                for (int i = valuesOffset; i < valuesCount; ++i)
                {
                    float stageStart = 0, stageEnd = 1;
                    byte depth = 0;
                    byte* caption = null;

                    valuesGetter(&stageStart, &stageEnd, &depth, &caption, data, i);

                    var duration = scaleMax - scaleMin;
                    if (duration == 0)
                    {
                        return;
                    }

                    var start = stageStart - scaleMin;
                    var end = stageEnd - scaleMin;

                    if (start < 0 || stageStart > scaleMax)
                    {
                        continue;
                    }

                    var startX = (float)(start / (double)duration);
                    var endX = (float)(end / (double)duration);

                    float width = innerBB.Max.X - innerBB.Min.X;
                    float height;
                    if (flip)
                    {
                        height = blockHeight * (maxDepth - depth + 1) - style.FramePadding.Y;
                    }
                    else
                    {
                        height = blockHeight * (depth + 1) - style.FramePadding.Y;
                    }

                    var pos0 = innerBB.Min + new Vector2(startX * width, height);
                    var pos1 = innerBB.Min + new Vector2(endX * width, height + blockHeight);

                    bool v_hovered = false;
                    if (ImGui.IsMouseHoveringRect(pos0, pos1))
                    {
                        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                        {
                            if (selected == i)
                                selected = -1;
                            else
                                selected = i;
                        }
#pragma warning disable CA2014 // note it's not needed to move you out of the loop the if above only allows one item
                        var args = stackalloc byte[sizeof(float*) * sizeof(byte*)];
#pragma warning restore CA2014
                        *(byte**)args = caption;
                        *(float*)((byte**)args + 1) = stageEnd - stageStart;
                        ImGui.SetTooltipV($"%s: {stageEnd - stageStart}ms", (nuint)args);

                        v_hovered = true;
                        any_hovered = v_hovered;
                    }

                    drawList.AddRectFilled(pos0, pos1, i == selected ? col_selected : v_hovered ? col_hovered : col_base);
                    drawList.AddRect(pos0, pos1, i == selected ? col_selected : v_hovered ? col_outline_hovered : col_outline_base);
                    var textSize = ImGui.CalcTextSize(caption);
                    var boxSize = pos1 - pos0;
                    var textOffset = new Vector2(0.0f, 0.0f);
                    if (textSize.X < boxSize.X)
                    {
                        textOffset = new Vector2(0.1f, 0.1f) * (boxSize - textSize);
                        ImGuiP.RenderText(pos0 + textOffset, caption, (byte*)null, true);
                    }
                }

                // Text overlay
                if (overlayText != null)
                    ImGuiP.RenderTextClipped(new Vector2(frameBB.Min.X, frameBB.Min.Y + style.FramePadding.Y), frameBB.Max, overlayText, (byte*)null, (Vector2*)null, new Vector2(0.5f, 0.0f), null);

                if (labelSize.X > 0.0f)
                    ImGuiP.RenderText(new Vector2(frameBB.Max.X + style.ItemInnerSpacing.X, innerBB.Min.Y), label, (byte*)null, true);
            }

            if (!any_hovered && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"Total: {scaleMax - scaleMin} ms");
            }
        }
    }
}