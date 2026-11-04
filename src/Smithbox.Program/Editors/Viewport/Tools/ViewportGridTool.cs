using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Renderer;
using System;
using System.Numerics;

namespace StudioCore.Editors.Viewport;

/// <summary>
/// The tool window section that configures the viewport grids of an editor.
/// </summary>
public class ViewportGridTool
{
    private readonly ViewportGridEditor Editor;

    /// <summary>
    /// The viewport the grids are drawn in, for Move Grid to Selection. May return null.
    /// </summary>
    private readonly Func<VulkanViewport> GetViewport;

    /// <summary>
    /// The grid shown here. Kept in <see cref="ViewportGridConfig" />, as the gizmo snaps to it.
    /// </summary>
    private ViewportGridType CurrentGridType
    {
        get => ViewportGridConfig.GetCurrentGrid(Editor);
        set => ViewportGridConfig.SetCurrentGrid(Editor, value);
    }

    public ViewportGridTool(ViewportGridEditor editor, Func<VulkanViewport> getViewport)
    {
        Editor = editor;
        GetViewport = getViewport;
    }

    public void OnToolWindow()
    {
        if (ToolSections.Header($"{LOC.Get("VIEWPORT_GridConfig_Header")}##viewportGridTool"))
        {
            ImGui.BeginChild("ViewportGridToolSection", ImGuiChildFlags.Borders);

            var grid = ViewportGridConfig.Get(Editor, CurrentGridType);

            // Current Grid
            GUI.SimpleHeader(
                LOC.Get($"VIEWPORT_GridConfig_{CurrentGridType}_Grid_Header"),
                LOC.Get($"VIEWPORT_GridConfig_{CurrentGridType}_Grid_Header_TT"));

            GUI.MultiButtonInput("gridVisibility",
                "toggleGrid",
                LOC.Get("VIEWPORT_GridConfig_Action_Toggle_Grid_Visibility"),
                LOC.Get("VIEWPORT_GridConfig_Action_Toggle_Grid_Visibility_TT"),
                ToggleCurrentGrid);

            // Placement
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("VIEWPORT_GridConfig_Placement_Header"),
                LOC.Get("VIEWPORT_GridConfig_Placement_Header_TT"));

            var isMoving = ViewportGridMovement.IsMoving(Editor, CurrentGridType);

            GUI.MultiButtonInput("gridPlacement",
                "toggleGridMovement",
                LOC.Get(isMoving ? "VIEWPORT_GridConfig_Action_Stop_Move_Grid" : "VIEWPORT_GridConfig_Action_Move_Grid"),
                LOC.Get("VIEWPORT_GridConfig_Action_Move_Grid_TT"),
                ToggleGridMovement,

                "moveGridToSelection",
                LOC.Get("VIEWPORT_GridConfig_Action_Move_Grid_To_Selection"),
                LOC.Get("VIEWPORT_GridConfig_Action_Move_Grid_To_Selection_TT"),
                MoveGridToSelection,

                "alignGridToSelection",
                LOC.Get("VIEWPORT_GridConfig_Action_Align_Grid_To_Selection"),
                LOC.Get("VIEWPORT_GridConfig_Action_Align_Grid_To_Selection_TT"),
                AlignGridToSelection,

                "resetGridPosition",
                LOC.Get("VIEWPORT_GridConfig_Action_Reset_Grid_Position"),
                LOC.Get("VIEWPORT_GridConfig_Action_Reset_Grid_Position_TT"),
                ResetGridPosition,

                "resetGridRotation",
                LOC.Get("VIEWPORT_GridConfig_Action_Reset_Grid_Rotation"),
                LOC.Get("VIEWPORT_GridConfig_Action_Reset_Grid_Rotation_TT"),
                ResetGridRotation);

            if (isMoving && !grid.Display)
            {
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color,
                    LOC.Get("VIEWPORT_GridConfig_Move_Grid_Hidden_Warning"));
            }

            // Read-only, the grid is placed with the gizmo
            ImGui.TextUnformatted($"{LOC.Get("VIEWPORT_GridConfig_Grid_Position_Header")}: " +
                $"{grid.Position.X:0.###}, {grid.Position.Y:0.###}, {grid.Position.Z:0.###}");
            GUI.Tooltip(LOC.Get("VIEWPORT_GridConfig_Grid_Position_Header_TT"));

            ImGui.TextUnformatted($"{LOC.Get("VIEWPORT_GridConfig_Grid_Rotation_Header")}: " +
                $"{grid.Rotation.X:0.##}, {grid.Rotation.Y:0.##}, {grid.Rotation.Z:0.##}");
            GUI.Tooltip(LOC.Get("VIEWPORT_GridConfig_Grid_Rotation_Header_TT"));

            // Color
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("VIEWPORT_GridConfig_Color_Header"),
                LOC.Get("VIEWPORT_GridConfig_Color_Header_TT"));

            ImGui.ColorEdit3("##Color", ref grid.Color);
            GUI.Tooltip(LOC.Get("VIEWPORT_GridConfig_Color_Input_TT"));

            // Square Size
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("VIEWPORT_GridConfig_Square_Size_Header"),
                LOC.Get("VIEWPORT_GridConfig_Square_Size_Header_TT"));

            ImGui.InputFloat("##SquareSize", ref grid.SectionSize);
            GUI.Tooltip(LOC.Get("VIEWPORT_GridConfig_Square_Size_Input_TT"));

            // Grid Size
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("VIEWPORT_GridConfig_Size"),
                LOC.Get("VIEWPORT_GridConfig_Size_TT"));

            ImGui.InputInt("##GridSize", ref grid.Size);
            GUI.Tooltip(LOC.Get("VIEWPORT_GridConfig_Size_Input_TT"));

            // Shared by every grid
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("VIEWPORT_GridConfig_Shared_Header"),
                LOC.Get("VIEWPORT_GridConfig_Shared_Header_TT"));

            if (ImGui.InputInt($"{LOC.Get("VIEWPORT_GridConfig_Major_Line_Interval")}##gridMajorInterval",
                ref CFG.Current.Viewport_Grid_Major_Line_Interval))
            {
                CFG.Current.Viewport_Grid_Major_Line_Interval = Math.Max(CFG.Current.Viewport_Grid_Major_Line_Interval, 0);
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_GridConfig_Major_Line_Interval_TT"));

            ImGui.Checkbox($"{LOC.Get("VIEWPORT_GridConfig_Display_Axis_Lines")}##gridAxisLines",
                ref CFG.Current.Viewport_Grid_Display_Axis_Lines);
            GUI.Tooltip(LOC.Get("VIEWPORT_GridConfig_Display_Axis_Lines_TT"));

            ImGui.SliderFloat($"{LOC.Get("VIEWPORT_GridConfig_Opacity")}##gridOpacity",
                ref CFG.Current.Viewport_Grid_Opacity, 0.05f, 1.0f, "%.2f", ImGuiSliderFlags.AlwaysClamp);
            GUI.Tooltip(LOC.Get("VIEWPORT_GridConfig_Opacity_TT"));

            if (ImGui.InputFloat($"{LOC.Get("VIEWPORT_GridConfig_Fade_Distance")}##gridFadeDistance",
                ref CFG.Current.Viewport_Grid_Fade_Distance))
            {
                CFG.Current.Viewport_Grid_Fade_Distance = Math.Max(CFG.Current.Viewport_Grid_Fade_Distance, 0f);
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_GridConfig_Fade_Distance_TT"));

            // Actions
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("VIEWPORT_GridConfig_Actions_Header"),
                LOC.Get("VIEWPORT_GridConfig_Actions_Header_TT"));

            GUI.MultiButtonInput("gridActions",
                "setGridToPrimary",
                LOC.Get("VIEWPORT_GridConfig_View_Primary_Grid_Action"),
                LOC.Get("VIEWPORT_GridConfig_View_Primary_Grid_Action_TT"),
                ViewPrimaryGrid,

                "setGridToSecondary",
                LOC.Get("VIEWPORT_GridConfig_View_Secondary_Grid_Action"),
                LOC.Get("VIEWPORT_GridConfig_View_Secondary_Grid_Action_TT"),
                ViewSecondaryGrid,

                "setGridToTertiary",
                LOC.Get("VIEWPORT_GridConfig_View_Tertiary_Grid_Action"),
                LOC.Get("VIEWPORT_GridConfig_View_Tertiary_Grid_Action_TT"),
                ViewTertiaryGrid);

            ImGui.EndChild();
        }
    }

    public void ViewPrimaryGrid()
    {
        SetCurrentGrid(ViewportGridType.Primary);
    }
    public void ViewSecondaryGrid()
    {
        SetCurrentGrid(ViewportGridType.Secondary);
    }
    public void ViewTertiaryGrid()
    {
        SetCurrentGrid(ViewportGridType.Tertiary);
    }

    private void SetCurrentGrid(ViewportGridType type)
    {
        // Moving is tied to the grid shown here, so switching grid ends it
        if (CurrentGridType != type)
        {
            ViewportGridMovement.Stop(Editor);
        }

        CurrentGridType = type;
    }

    public void ToggleCurrentGrid()
    {
        var grid = ViewportGridConfig.Get(Editor, CurrentGridType);

        grid.Display = !grid.Display;

        if (!grid.Display && ViewportGridMovement.IsMoving(Editor, CurrentGridType))
        {
            ViewportGridMovement.Stop(Editor);
        }
    }

    public void ToggleGridMovement()
    {
        if (ViewportGridMovement.IsMoving(Editor, CurrentGridType))
        {
            ViewportGridMovement.Stop(Editor);
        }
        else
        {
            // The gizmo is only shown on a visible grid
            var grid = ViewportGridConfig.Get(Editor, CurrentGridType);
            grid.Display = true;
            ViewportGridMovement.Start(Editor, CurrentGridType);
        }

        GetViewport()?.DelayPicking();
    }

    public void MoveGridToSelection()
    {
        if (!TryGetSelectionTransform(out var transform))
        {
            return;
        }

        ViewportGridMovement.SetGridPosition(ViewportGridConfig.Get(Editor, CurrentGridType), transform.Position);
    }

    public void AlignGridToSelection()
    {
        if (!TryGetSelectionTransform(out var transform))
        {
            return;
        }

        ViewportGridMovement.SetGridRotation(ViewportGridConfig.Get(Editor, CurrentGridType), transform.Rotation);
    }

    public void ResetGridPosition()
    {
        ViewportGridMovement.SetGridPosition(ViewportGridConfig.Get(Editor, CurrentGridType), Vector3.Zero);
    }

    public void ResetGridRotation()
    {
        ViewportGridMovement.SetGridRotation(ViewportGridConfig.Get(Editor, CurrentGridType), Quaternion.Identity);
    }

    private bool TryGetSelectionTransform(out Transform transform)
    {
        transform = Transform.Default;

        var selection = GetViewport()?.Gizmos?.SelectionTarget;
        if (selection == null || !selection.HasTarget)
        {
            Smithbox.LogError<ViewportGridTool>("No object selected.");
            return false;
        }

        transform = selection.GetSelectionTransform(false);
        return true;
    }
}
