using Hexa.NET.ImGui;
using StudioCore.Application;

namespace StudioCore.Editors.MapEditor;

public class MapGridTool
{
    public MapEditorView View;
    public ProjectEntry Project;

    private TargetMapGridType CurrentGridType = TargetMapGridType.Primary;

    public MapGridTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        ImGui.BeginChild("MapGridToolSection", ImGuiChildFlags.Borders);

        // Primary Configuration
        if (CurrentGridType is TargetMapGridType.Primary)
        {
            UIHelper.SimpleHeader("Current Grid (Primary)", "");

            UIHelper.MultiButtonInput("primaryActions",
                "toggleGrid", "Toggle Grid Visibility", "", TogglePrimaryGrid);

            // Position
            UIHelper.SimpleHeader("Grid Position", "The position configuration for the grid.");

            ImGui.InputFloat("X##gridPosX", ref CFG.Current.MapEditor_PrimaryGrid_Position_X);
            UIHelper.Tooltip("The position of the grid on the X-axis.");

            ImGui.InputFloat("Y##gridPosY", ref CFG.Current.MapEditor_PrimaryGrid_Position_Y);
            UIHelper.Tooltip("The position of the grid on the Y-axis.");

            ImGui.InputFloat("Z##gridPosZ", ref CFG.Current.MapEditor_PrimaryGrid_Position_Z);
            UIHelper.Tooltip("The position of the grid on the Z-axis.");

            // Rotation
            UIHelper.SimpleHeader("Grid Rotation", "The rotation configuration for the grid.");

            ImGui.InputFloat("X##gridRotX", ref CFG.Current.MapEditor_PrimaryGrid_Rotation_X);
            UIHelper.Tooltip("The rotation of the grid on the X-axis.");

            ImGui.InputFloat("Y##gridRotY", ref CFG.Current.MapEditor_PrimaryGrid_Rotation_Y);
            UIHelper.Tooltip("The rotation of the grid on the Y-axis.");

            ImGui.InputFloat("Z##gridRotZ", ref CFG.Current.MapEditor_PrimaryGrid_Rotation_Z);
            UIHelper.Tooltip("The rotation of the grid on the Z-axis.");

            // Color
            UIHelper.SimpleHeader("Grid Color", "The color configuration for the grid.");

            ImGui.ColorEdit3("##Color", ref CFG.Current.MapEditor_PrimaryGrid_Color);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                CFG.Current.MapEditor_RegeneratePrimaryGrid = true;
            }
            UIHelper.Tooltip("The color of the grid.");

            // Square Size
            UIHelper.SimpleHeader("Square Size", "The size configuration for the grid.");

            ImGui.InputFloat("##SquareSize", ref CFG.Current.MapEditor_PrimaryGrid_SectionSize);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                CFG.Current.MapEditor_RegeneratePrimaryGrid = true;
            }
            UIHelper.Tooltip("The size of an individual grid square.");

            // Grid Size
            UIHelper.SimpleHeader("Grid Size", "The size configuration for the grid.");

            ImGui.InputInt("##GridSize", ref CFG.Current.MapEditor_PrimaryGrid_Size);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                CFG.Current.MapEditor_RegeneratePrimaryGrid = true;
            }
            UIHelper.Tooltip("The number of grid squares that make up the grid.");
        }

        // Secondary Configuration
        if (CurrentGridType is TargetMapGridType.Secondary)
        {
            UIHelper.SimpleHeader("Current Grid (Secondary)", "");

            UIHelper.MultiButtonInput("secondaryActions",
                "toggleGrid", "Toggle Grid Visibility", "", ToggleSecondaryGrid);

            // Position
            UIHelper.SimpleHeader("Grid Position", "The position configuration for the grid.");

            ImGui.InputFloat("X##gridPosX", ref CFG.Current.MapEditor_SecondaryGrid_Position_X);
            UIHelper.Tooltip("The position of the grid on the X-axis.");

            ImGui.InputFloat("Y##gridPosY", ref CFG.Current.MapEditor_SecondaryGrid_Position_Y);
            UIHelper.Tooltip("The position of the grid on the Y-axis.");

            ImGui.InputFloat("Z##gridPosZ", ref CFG.Current.MapEditor_SecondaryGrid_Position_Z);
            UIHelper.Tooltip("The position of the grid on the Z-axis.");

            // Rotation
            UIHelper.SimpleHeader("Grid Rotation", "The rotation configuration for the grid.");

            ImGui.InputFloat("X##gridRotX", ref CFG.Current.MapEditor_SecondaryGrid_Rotation_X);
            UIHelper.Tooltip("The rotation of the grid on the X-axis.");

            ImGui.InputFloat("Y##gridRotY", ref CFG.Current.MapEditor_SecondaryGrid_Rotation_Y);
            UIHelper.Tooltip("The rotation of the grid on the Y-axis.");

            ImGui.InputFloat("Z##gridRotZ", ref CFG.Current.MapEditor_SecondaryGrid_Rotation_Z);
            UIHelper.Tooltip("The rotation of the grid on the Z-axis.");

            // Color
            UIHelper.SimpleHeader("Grid Color", "The color configuration for the grid.");

            ImGui.ColorEdit3("##Color", ref CFG.Current.MapEditor_SecondaryGrid_Color);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                CFG.Current.MapEditor_RegenerateSecondaryGrid = true;
            }
            UIHelper.Tooltip("The color of the grid.");

            // Square Size
            UIHelper.SimpleHeader("Square Size", "The size configuration for the grid.");

            ImGui.InputFloat("##SquareSize", ref CFG.Current.MapEditor_SecondaryGrid_SectionSize);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                CFG.Current.MapEditor_RegenerateSecondaryGrid = true;
            }
            UIHelper.Tooltip("The size of an individual grid square.");

            // Grid Size
            UIHelper.SimpleHeader("Grid Size", "The size configuration for the grid.");

            ImGui.InputInt("##GridSize", ref CFG.Current.MapEditor_SecondaryGrid_Size);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                CFG.Current.MapEditor_RegenerateSecondaryGrid = true;
            }
            UIHelper.Tooltip("The number of grid squares that make up the grid.");
        }

        // Tertiary Configuration
        if (CurrentGridType is TargetMapGridType.Tertiary)
        {
            UIHelper.SimpleHeader("Current Grid (Tertiary)", "");

            UIHelper.MultiButtonInput("tertiaryActions",
                "toggleGrid", "Toggle Grid Visibility", "", ToggleTertiaryGrid);

            // Position
            UIHelper.SimpleHeader("Grid Position", "The position configuration for the grid.");

            ImGui.InputFloat("X##gridPosX", ref CFG.Current.MapEditor_TertiaryGrid_Position_X);
            UIHelper.Tooltip("The position of the grid on the X-axis.");

            ImGui.InputFloat("Y##gridPosY", ref CFG.Current.MapEditor_TertiaryGrid_Position_Y);
            UIHelper.Tooltip("The position of the grid on the Y-axis.");

            ImGui.InputFloat("Z##gridPosZ", ref CFG.Current.MapEditor_TertiaryGrid_Position_Z);
            UIHelper.Tooltip("The position of the grid on the Z-axis.");

            // Rotation
            UIHelper.SimpleHeader("Grid Rotation", "The rotation configuration for the grid.");

            ImGui.InputFloat("X##gridRotX", ref CFG.Current.MapEditor_TertiaryGrid_Rotation_X);
            UIHelper.Tooltip("The rotation of the grid on the X-axis.");

            ImGui.InputFloat("Y##gridRotY", ref CFG.Current.MapEditor_TertiaryGrid_Rotation_Y);
            UIHelper.Tooltip("The rotation of the grid on the Y-axis.");

            ImGui.InputFloat("Z##gridRotZ", ref CFG.Current.MapEditor_TertiaryGrid_Rotation_Z);
            UIHelper.Tooltip("The rotation of the grid on the Z-axis.");

            // Color
            UIHelper.SimpleHeader("Grid Color", "The color configuration for the grid.");

            ImGui.ColorEdit3("##Color", ref CFG.Current.MapEditor_TertiaryGrid_Color);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                CFG.Current.MapEditor_RegenerateTertiaryGrid = true;
            }
            UIHelper.Tooltip("The color of the grid.");

            // Square Size
            UIHelper.SimpleHeader("Square Size", "The size configuration for the grid.");

            ImGui.InputFloat("##SquareSize", ref CFG.Current.MapEditor_TertiaryGrid_SectionSize);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                CFG.Current.MapEditor_RegenerateTertiaryGrid = true;
            }
            UIHelper.Tooltip("The size of an individual grid square.");

            // Grid Size
            UIHelper.SimpleHeader("Grid Size", "The size configuration for the grid.");

            ImGui.InputInt("##GridSize", ref CFG.Current.MapEditor_TertiaryGrid_Size);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                CFG.Current.MapEditor_RegenerateTertiaryGrid = true;
            }
            UIHelper.Tooltip("The number of grid squares that make up the grid.");
        }

        UIHelper.SimpleHeader("Actions", "");

        UIHelper.MultiButtonInput("gridActions",
            "setGridToPrimary", "View Primary Grid", "", ViewPrimaryGrid,
            "setGridToSecondary", "View Secondary Grid", "", ViewSecondaryGrid,
            "setGridToTertiary", "View Tertiary Grid", "", ViewTertiaryGrid);

        ImGui.EndChild();
    }

    public void ViewPrimaryGrid()
    {
        CurrentGridType = TargetMapGridType.Primary;
    }
    public void ViewSecondaryGrid()
    {
        CurrentGridType = TargetMapGridType.Secondary;
    }
    public void ViewTertiaryGrid()
    {
        CurrentGridType = TargetMapGridType.Tertiary;
    }

    public void TogglePrimaryGrid()
    {
        CFG.Current.MapEditor_DisplayPrimaryGrid = !CFG.Current.MapEditor_DisplayPrimaryGrid;
    }
    public void ToggleSecondaryGrid()
    {
        CFG.Current.MapEditor_DisplaySecondaryGrid = !CFG.Current.MapEditor_DisplaySecondaryGrid;
    }
    public void ToggleTertiaryGrid()
    {
        CFG.Current.MapEditor_DisplayTertiaryGrid = !CFG.Current.MapEditor_DisplayTertiaryGrid;
    }
}
