using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Renderer;

/// <summary>
///     The panel along the bottom of a viewport that displays the progress of each category of resources during a load.
/// </summary>
public class ResourceLoadWindow
{
    private const float MinCategoryWidth = 280.0f;
    private const int MaxColumns = 3;

    /// <summary>
    ///     The panel may not take more than this fraction of the viewport
    /// </summary>
    private const float MaxHeightFraction = 0.5f;

    /// <summary>
    ///     The height the panel needs, which the viewport has to give up. Zero if there is nothing to display.
    /// </summary>
    public float GetHeight(Vector2 viewportSize)
    {
        var count = ResourceLoadTracker.Statuses.Count;

        if (count == 0)
            return 0;

        var columns = GetColumnCount(viewportSize.X);
        var rows = (count + columns - 1) / columns;

        var height = ImGui.GetStyle().WindowPadding.Y * 2
            + ImGui.GetTextLineHeightWithSpacing()
            + ImGui.GetFrameHeightWithSpacing() * rows;

        return MathF.Min(height, viewportSize.Y * MaxHeightFraction);
    }

    /// <summary>
    ///     Displays the panel, as part of the viewport's window so that it stays attached to the viewport.
    /// </summary>
    public void Display(Vector2 viewportPos, Vector2 viewportSize, float height)
    {
        IReadOnlyList<ResourceLoadStatus> statuses = ResourceLoadTracker.Statuses;

        if (statuses.Count == 0)
            return;

        ImGui.SetCursorScreenPos(viewportPos + new Vector2(0, viewportSize.Y - height));

        // Inputs are not used here, and taking focus from the viewport would stop it responding during the load
        var flags = ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoScrollbar;

        if (ImGui.BeginChild("##resourceLoadPanel", new Vector2(viewportSize.X, height),
            ImGuiChildFlags.Borders | ImGuiChildFlags.AlwaysUseWindowPadding, flags))
        {
            DisplayHeader();
            DisplayCategories(statuses, viewportSize.X);
        }

        ImGui.EndChild();
    }

    private void DisplayHeader()
    {
        ImGui.Text(GetTitle());

        var file = ResourceLoadTracker.CurrentFile;

        if (!string.IsNullOrEmpty(file))
        {
            ImGui.SameLine();
            ImGui.TextDisabled(file);
        }
    }

    private void DisplayCategories(IReadOnlyList<ResourceLoadStatus> statuses, float viewportWidth)
    {
        var columns = GetColumnCount(viewportWidth);
        var spacing = ImGui.GetStyle().ItemSpacing.X * 2;
        var cellWidth = (ImGui.GetContentRegionAvail().X - spacing * (columns - 1)) / columns;
        var labelWidth = MathF.Min(GetLabelWidth(statuses), cellWidth * 0.5f);
        var startX = ImGui.GetCursorPosX();

        for (var i = 0; i < statuses.Count; i++)
        {
            ResourceLoadStatus status = statuses[i];
            var column = i % columns;
            var x = startX + column * (cellWidth + spacing);

            if (column > 0)
            {
                ImGui.SameLine(x);
            }

            ImGui.AlignTextToFramePadding();

            if (status.State is ResourceLoadState.Waiting)
            {
                ImGui.TextDisabled(status.Name);
            }
            else
            {
                ImGui.Text(status.Name);
            }

            ImGui.SameLine(x + labelWidth);
            ImGui.ProgressBar(GetFraction(status), new Vector2(cellWidth - labelWidth, 0), GetProgressText(status));
        }
    }

    private float GetLabelWidth(IReadOnlyList<ResourceLoadStatus> statuses)
    {
        var width = 0.0f;

        foreach (ResourceLoadStatus status in statuses)
        {
            width = MathF.Max(width, ImGui.CalcTextSize(status.Name).X);
        }

        return width + ImGui.GetStyle().ItemSpacing.X;
    }

    private int GetColumnCount(float width)
    {
        return Math.Clamp((int)(width / (MinCategoryWidth * DPI.UIScale())), 1, MaxColumns);
    }

    private static float GetFraction(ResourceLoadStatus status)
    {
        return status.State switch
        {
            ResourceLoadState.Done => 1.0f,
            ResourceLoadState.Loading when status.Total > 0 => status.Loaded / (float)status.Total,
            _ => 0.0f
        };
    }

    private static string GetProgressText(ResourceLoadStatus status)
    {
        return status.State switch
        {
            ResourceLoadState.Waiting => LOC.Get("REND_Load_Panel_Waiting"),
            ResourceLoadState.Done => LOC.Get("REND_Load_Panel_Done"),
            _ => status.IsEstimate ? $"{status.Loaded} / ~{status.Total}" : $"{status.Loaded} / {status.Total}"
        };
    }

    private static string GetTitle()
    {
        var name = ResourceLoadTracker.GetSessionName(out var others);

        if (name == null)
            return LOC.Get("REND_Load_Panel_Title_Resources");

        return others > 0
            ? LOC.Get("REND_Load_Panel_Title_Multiple", name, others)
            : LOC.Get("REND_Load_Panel_Title", name);
    }
}
