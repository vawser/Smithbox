using Hexa.NET.ImGui;
using StudioCore.Application;

namespace StudioCore.Editors.Viewport;

/// <summary>
/// The tool window section that configures the viewport grids of an editor.
/// </summary>
public class ViewportGridTool
{
    private readonly ViewportGridEditor Editor;

    private ViewportGridType CurrentGridType = ViewportGridType.Primary;

    public ViewportGridTool(ViewportGridEditor editor)
    {
        Editor = editor;
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

            // Position
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("VIEWPORT_GridConfig_Grid_Position_Header"),
                LOC.Get("VIEWPORT_GridConfig_Grid_Position_Header_TT"));

            // X
            ImGui.InputFloat($"{LOC.Get("VIEWPORT_GridConfig_Position_X")}##gridPosX", ref grid.PositionX);
            GUI.Tooltip(LOC.Get("VIEWPORT_GridConfig_Position_X_TT"));

            // Y
            ImGui.InputFloat($"{LOC.Get("VIEWPORT_GridConfig_Position_Y")}##gridPosY", ref grid.PositionY);
            GUI.Tooltip(LOC.Get("VIEWPORT_GridConfig_Position_Y_TT"));

            // Z
            ImGui.InputFloat($"{LOC.Get("VIEWPORT_GridConfig_Position_Z")}##gridPosZ", ref grid.PositionZ);
            GUI.Tooltip(LOC.Get("VIEWPORT_GridConfig_Position_Z_TT"));

            // Rotation
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("VIEWPORT_GridConfig_Grid_Rotation_Header"),
                LOC.Get("VIEWPORT_GridConfig_Grid_Rotation_Header_TT"));

            // X
            ImGui.InputFloat($"{LOC.Get("VIEWPORT_GridConfig_Rotation_X")}##gridRotX", ref grid.RotationX);
            GUI.Tooltip(LOC.Get("VIEWPORT_GridConfig_Rotation_X_TT"));

            // Y
            ImGui.InputFloat($"{LOC.Get("VIEWPORT_GridConfig_Rotation_Y")}##gridRotY", ref grid.RotationY);
            GUI.Tooltip(LOC.Get("VIEWPORT_GridConfig_Rotation_Y_TT"));

            // Z
            ImGui.InputFloat($"{LOC.Get("VIEWPORT_GridConfig_Rotation_Z")}##gridRotZ", ref grid.RotationZ);
            GUI.Tooltip(LOC.Get("VIEWPORT_GridConfig_Rotation_Z_TT"));

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
        CurrentGridType = ViewportGridType.Primary;
    }
    public void ViewSecondaryGrid()
    {
        CurrentGridType = ViewportGridType.Secondary;
    }
    public void ViewTertiaryGrid()
    {
        CurrentGridType = ViewportGridType.Tertiary;
    }

    public void ToggleCurrentGrid()
    {
        var grid = ViewportGridConfig.Get(Editor, CurrentGridType);

        grid.Display = !grid.Display;
    }
}
