using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.Common;

/// <summary>
/// A grid of 3D model previews. The number of columns follows the width of the window.
/// </summary>
public class ModelPreviewBrowser
{
    public const int MinPreviewSize = 64;
    public const int MaxPreviewSize = 512;

    /// <summary>
    /// The number of entries to have ready in each direction beyond what is on screen.
    /// </summary>
    public const int NearbyPreviewCount = 50;

    private ProjectEntry Project;

    /// <summary>
    /// The ID of the selected entry
    /// </summary>
    public string SelectedID = "";

    public ModelPreviewBrowser(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>
    /// Displays the controls for the previews, which belong above the filter of the entries.
    /// </summary>
    public void DisplayOptions()
    {
        ImGui.SetNextItemWidth(100f * DPI.UIScale());
        ImGui.SliderInt($"{LOC.Get("EDITOR_ModelPreview_Size")}##modelPreviewSize",
            ref CFG.Current.Interface_Model_Preview_Size, MinPreviewSize, MaxPreviewSize);
        GUI.Tooltip(LOC.Get("EDITOR_ModelPreview_Size_TT"));

        // Typed values aren't limited by the slider
        CFG.Current.Interface_Model_Preview_Size =
            Math.Clamp(CFG.Current.Interface_Model_Preview_Size, MinPreviewSize, MaxPreviewSize);

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("EDITOR_ModelPreview_Refresh")}##modelPreviewRefresh"))
        {
            ModelPreviewCache.Get(Project).Clear();
        }
        GUI.Tooltip(LOC.Get("EDITOR_ModelPreview_Refresh_TT"));
    }

    /// <summary>
    /// Displays the grid, filling the rest of the window.
    /// </summary>
    /// <param name="entries">The entries that pass the filter.</param>
    /// <param name="onActivate">Called when an entry is double-clicked.</param>
    public void Display(string id, IReadOnlyList<ModelPreviewEntry> entries, Action<ModelPreviewEntry> onActivate)
    {
        ImGui.BeginChild($"##modelPreviewGrid_{id}", new Vector2(0, 0), ImGuiChildFlags.Borders);

        if (ModelPreviewCache.IsSupported)
        {
            DisplayGrid(entries, onActivate);
        }
        else
        {
            GUI.WrappedText(LOC.Get("EDITOR_ModelPreview_Unsupported"));
        }

        ImGui.EndChild();
    }

    private void DisplayGrid(IReadOnlyList<ModelPreviewEntry> entries, Action<ModelPreviewEntry> onActivate)
    {
        if (entries.Count == 0)
            return;

        var cache = ModelPreviewCache.Get(Project);

        var size = (float)CFG.Current.Interface_Model_Preview_Size;
        var spacing = ImGui.GetStyle().ItemSpacing;
        var cellSize = new Vector2(size, size + ImGui.GetTextLineHeightWithSpacing());

        var columns = Math.Max(1, (int)((ImGui.GetContentRegionAvail().X + spacing.X) / (size + spacing.X)));
        var rows = (entries.Count + columns - 1) / columns;

        var firstDisplayed = int.MaxValue;
        var lastDisplayed = -1;

        var clipper = new ImGuiListClipper();
        clipper.Begin(rows, cellSize.Y + spacing.Y);

        while (clipper.Step())
        {
            for (int row = clipper.DisplayStart; row < clipper.DisplayEnd; row++)
            {
                var first = row * columns;
                var last = Math.Min(first + columns, entries.Count);

                firstDisplayed = Math.Min(firstDisplayed, first);
                lastDisplayed = Math.Max(lastDisplayed, last - 1);

                for (int i = first; i < last; i++)
                {
                    if (i > first)
                    {
                        ImGui.SameLine();
                    }

                    DisplayCell(cache, entries[i], i, size, cellSize, onActivate);
                }
            }
        }

        clipper.End();

        RequestNearbyPreviews(cache, entries, firstDisplayed, lastDisplayed);
    }

    /// <summary>
    /// Asks for the entries either side of what is on screen, nearest first, so they are ready when scrolled to.
    /// This comes after the cells so that what is on screen goes first. Being asked for
    /// also keeps these from being evicted, so scrolling back and forth doesn't redo them.
    /// </summary>
    private void RequestNearbyPreviews(ModelPreviewCache cache, IReadOnlyList<ModelPreviewEntry> entries, int first, int last)
    {
        if (last < 0)
            return;

        for (int offset = 1; offset <= NearbyPreviewCount; offset++)
        {
            if (last + offset < entries.Count)
            {
                cache.GetPreview(entries[last + offset], out _);
            }

            if (first - offset >= 0)
            {
                cache.GetPreview(entries[first - offset], out _);
            }
        }
    }

    private void DisplayCell(ModelPreviewCache cache, ModelPreviewEntry entry, int index, float size, Vector2 cellSize,
        Action<ModelPreviewEntry> onActivate)
    {
        var start = ImGui.GetCursorScreenPos();

        if (ImGui.Selectable($"##modelPreviewCell{index}", entry.ID == SelectedID, ImGuiSelectableFlags.AllowDoubleClick, cellSize))
        {
            SelectedID = entry.ID;

            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                onActivate(entry);
            }
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(string.IsNullOrEmpty(entry.Alias) ? entry.ID : $"{entry.ID}\n{entry.Alias}");
        }

        var state = cache.GetPreview(entry, out var textureHandle);

        // Everything is drawn over the selectable with the draw list, as the selectable is the whole
        // of the cell's layout: moving the cursor to place these would trip ImGui's boundary check
        var drawList = ImGui.GetWindowDrawList();

        if (state is ModelPreviewState.Ready)
        {
            drawList.AddImage(new ImTextureID(textureHandle), start, start + new Vector2(size, size));
        }
        else
        {
            DisplayPlaceholder(state, start, size);
        }

        // The entry name is cut off at the edge of the cell
        drawList.PushClipRect(start, start + cellSize, true);
        drawList.AddText(start + new Vector2(0, size), ImGui.GetColorU32(ImGuiCol.Text), entry.ID);
        drawList.PopClipRect();
    }

    private void DisplayPlaceholder(ModelPreviewState state, Vector2 start, float size)
    {
        var text = state is ModelPreviewState.Pending
            ? LOC.Get("EDITOR_ModelPreview_Pending")
            : LOC.Get("EDITOR_ModelPreview_Unavailable");

        var drawList = ImGui.GetWindowDrawList();

        drawList.AddRect(start, start + new Vector2(size, size), ImGui.GetColorU32(ImGuiCol.Border));
        drawList.AddText(start + (new Vector2(size, size) - ImGui.CalcTextSize(text)) * 0.5f,
            ImGui.GetColorU32(ImGuiCol.TextDisabled), text);
    }
}
