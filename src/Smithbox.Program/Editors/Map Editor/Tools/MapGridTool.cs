using Hexa.NET.ImGui;
using StudioCore.Application;

namespace StudioCore.Editors.MapEditor;

public class MapGridTool
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    private TargetMapGridType CurrentMapGridType = TargetMapGridType.Primary;

    public MapGridTool(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        if (ImGui.CollapsingHeader("Map Grid Configuration"))
        {
            // Primary
            if (CurrentMapGridType is TargetMapGridType.Primary)
            {
                ImGui.BeginDisabled();
                if (ImGui.Button("Primary", DPI.StandardButtonSize))
                {

                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("View the configuration options for the primary grid.");
            }
            else
            {
                if (ImGui.Button("Primary", DPI.StandardButtonSize))
                {
                    CurrentMapGridType = TargetMapGridType.Primary;
                }
                UIHelper.Tooltip("View the configuration options for the primary grid.");
            }

            ImGui.SameLine();

            // Secondary
            if (CurrentMapGridType is TargetMapGridType.Secondary)
            {
                ImGui.BeginDisabled();
                if (ImGui.Button("Secondary", DPI.StandardButtonSize))
                {

                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("View the configuration options for the secondary grid.");
            }
            else
            {
                if (ImGui.Button("Secondary", DPI.StandardButtonSize))
                {
                    CurrentMapGridType = TargetMapGridType.Secondary;
                }
                UIHelper.Tooltip("View the configuration options for the secondary grid.");
            }

            ImGui.SameLine();

            // Tertiary
            if (CurrentMapGridType is TargetMapGridType.Tertiary)
            {
                ImGui.BeginDisabled();
                if (ImGui.Button("Tertiary", DPI.StandardButtonSize))
                {

                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("View the configuration options for the tertiary grid.");
            }
            else
            {
                if (ImGui.Button("Tertiary", DPI.StandardButtonSize))
                {
                    CurrentMapGridType = TargetMapGridType.Tertiary;
                }
                UIHelper.Tooltip("View the configuration options for the tertiary grid.");
            }

            ImGui.Separator();

            // Primary Configuration
            if (CurrentMapGridType is TargetMapGridType.Primary)
            {
                if (ImGui.Button("Toggle Visibility", DPI.StandardButtonSize))
                {
                    CFG.Current.MapEditor_DisplayPrimaryGrid = !CFG.Current.MapEditor_DisplayPrimaryGrid;
                }
                UIHelper.Tooltip("Toggle the visibility of the primary grid.");

                UIHelper.SimpleHeader("positionHeader", "Position", "The position configuration for the grid.", UI.Current.ImGui_Default_Text_Color);

                ImGui.InputFloat("Grid Position: X", ref CFG.Current.MapEditor_PrimaryGrid_Position_X);
                UIHelper.Tooltip("The position of the grid on the X-axis.");

                ImGui.InputFloat("Grid Position: Y", ref CFG.Current.MapEditor_PrimaryGrid_Position_Y);
                UIHelper.Tooltip("The position of the grid on the Y-axis.");

                ImGui.InputFloat("Grid Position: Z", ref CFG.Current.MapEditor_PrimaryGrid_Position_Z);
                UIHelper.Tooltip("The position of the grid on the Z-axis.");

                UIHelper.SimpleHeader("rotationHeader", "Rotation", "The rotation configuration for the grid.", UI.Current.ImGui_Default_Text_Color);

                ImGui.InputFloat("Grid Rotation: X", ref CFG.Current.MapEditor_PrimaryGrid_Rotation_X);
                UIHelper.Tooltip("The rotation of the grid on the X-axis.");

                ImGui.InputFloat("Grid Rotation: Y", ref CFG.Current.MapEditor_PrimaryGrid_Rotation_Y);
                UIHelper.Tooltip("The rotation of the grid on the Y-axis.");

                ImGui.InputFloat("Grid Rotation: Z", ref CFG.Current.MapEditor_PrimaryGrid_Rotation_Z);
                UIHelper.Tooltip("The rotation of the grid on the Z-axis.");

                UIHelper.SimpleHeader("colorHeader", "Color", "The color configuration for the grid.", UI.Current.ImGui_Default_Text_Color);

                ImGui.ColorEdit3("Grid Color", ref CFG.Current.MapEditor_PrimaryGrid_Color);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.MapEditor_RegeneratePrimaryGrid = true;
                }
                UIHelper.Tooltip("The color of the grid.");

                UIHelper.SimpleHeader("sizeHeader", "Size", "The size configuration for the grid.", UI.Current.ImGui_Default_Text_Color);

                ImGui.InputFloat("Square Size", ref CFG.Current.MapEditor_PrimaryGrid_SectionSize);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.MapEditor_RegeneratePrimaryGrid = true;
                }
                UIHelper.Tooltip("The size of an individual grid square.");

                ImGui.InputInt("Grid Size", ref CFG.Current.MapEditor_PrimaryGrid_Size);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.MapEditor_RegeneratePrimaryGrid = true;
                }
                UIHelper.Tooltip("The number of grid squares that make up the grid.");
            }

            // Secondary Configuration
            if (CurrentMapGridType is TargetMapGridType.Secondary)
            {
                if (ImGui.Button("Toggle Visibility", DPI.StandardButtonSize))
                {
                    CFG.Current.MapEditor_DisplaySecondaryGrid = !CFG.Current.MapEditor_DisplaySecondaryGrid;
                }
                UIHelper.Tooltip("Toggle the visibility of the secondary grid.");

                UIHelper.SimpleHeader("positionHeader", "Position", "The position configuration for the grid.", UI.Current.ImGui_Default_Text_Color);

                ImGui.InputFloat("Grid Position: X", ref CFG.Current.MapEditor_SecondaryGrid_Position_X);
                UIHelper.Tooltip("The position of the grid on the X-axis.");

                ImGui.InputFloat("Grid Position: Y", ref CFG.Current.MapEditor_SecondaryGrid_Position_Y);
                UIHelper.Tooltip("The position of the grid on the Y-axis.");

                ImGui.InputFloat("Grid Position: Z", ref CFG.Current.MapEditor_SecondaryGrid_Position_Z);
                UIHelper.Tooltip("The position of the grid on the Z-axis.");

                UIHelper.SimpleHeader("rotationHeader", "Rotation", "The rotation configuration for the grid.", UI.Current.ImGui_Default_Text_Color);

                ImGui.InputFloat("Grid Rotation: X", ref CFG.Current.MapEditor_SecondaryGrid_Rotation_X);
                UIHelper.Tooltip("The rotation of the grid on the X-axis.");

                ImGui.InputFloat("Grid Rotation: Y", ref CFG.Current.MapEditor_SecondaryGrid_Rotation_Y);
                UIHelper.Tooltip("The rotation of the grid on the Y-axis.");

                ImGui.InputFloat("Grid Rotation: Z", ref CFG.Current.MapEditor_SecondaryGrid_Rotation_Z);
                UIHelper.Tooltip("The rotation of the grid on the Z-axis.");

                UIHelper.SimpleHeader("colorHeader", "Color", "The color configuration for the grid.", UI.Current.ImGui_Default_Text_Color);

                ImGui.ColorEdit3("Grid Color", ref CFG.Current.MapEditor_SecondaryGrid_Color);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.MapEditor_RegenerateSecondaryGrid = true;
                }
                UIHelper.Tooltip("The color of the grid.");

                UIHelper.SimpleHeader("sizeHeader", "Size", "The size configuration for the grid.", UI.Current.ImGui_Default_Text_Color);

                ImGui.InputFloat("Square Size", ref CFG.Current.MapEditor_SecondaryGrid_SectionSize);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.MapEditor_RegenerateSecondaryGrid = true;
                }
                UIHelper.Tooltip("The size of an individual grid square.");

                ImGui.InputInt("Grid Size", ref CFG.Current.MapEditor_SecondaryGrid_Size);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.MapEditor_RegenerateSecondaryGrid = true;
                }
                UIHelper.Tooltip("The number of grid squares that make up the grid.");
            }

            // Tertiary Configuration
            if (CurrentMapGridType is TargetMapGridType.Tertiary)
            {
                if (ImGui.Button("Toggle Visibility", DPI.StandardButtonSize))
                {
                    CFG.Current.MapEditor_DisplayTertiaryGrid = !CFG.Current.MapEditor_DisplayTertiaryGrid;
                }
                UIHelper.Tooltip("Toggle the visibility of the tertiary grid.");

                UIHelper.SimpleHeader("positionHeader", "Position", "The position configuration for the grid.", UI.Current.ImGui_Default_Text_Color);

                ImGui.InputFloat("Grid Position: X", ref CFG.Current.MapEditor_TertiaryGrid_Position_X);
                UIHelper.Tooltip("The position of the grid on the X-axis.");

                ImGui.InputFloat("Grid Position: Y", ref CFG.Current.MapEditor_TertiaryGrid_Position_Y);
                UIHelper.Tooltip("The position of the grid on the Y-axis.");

                ImGui.InputFloat("Grid Position: Z", ref CFG.Current.MapEditor_TertiaryGrid_Position_Z);
                UIHelper.Tooltip("The position of the grid on the Z-axis.");

                UIHelper.SimpleHeader("rotationHeader", "Rotation", "The rotation configuration for the grid.", UI.Current.ImGui_Default_Text_Color);

                ImGui.InputFloat("Grid Rotation: X", ref CFG.Current.MapEditor_TertiaryGrid_Rotation_X);
                UIHelper.Tooltip("The rotation of the grid on the X-axis.");

                ImGui.InputFloat("Grid Rotation: Y", ref CFG.Current.MapEditor_TertiaryGrid_Rotation_Y);
                UIHelper.Tooltip("The rotation of the grid on the Y-axis.");

                ImGui.InputFloat("Grid Rotation: Z", ref CFG.Current.MapEditor_TertiaryGrid_Rotation_Z);
                UIHelper.Tooltip("The rotation of the grid on the Z-axis.");

                UIHelper.SimpleHeader("colorHeader", "Color", "The color configuration for the grid.", UI.Current.ImGui_Default_Text_Color);

                ImGui.ColorEdit3("Grid Color", ref CFG.Current.MapEditor_TertiaryGrid_Color);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.MapEditor_RegenerateTertiaryGrid = true;
                }
                UIHelper.Tooltip("The color of the grid.");

                UIHelper.SimpleHeader("sizeHeader", "Size", "The size configuration for the grid.", UI.Current.ImGui_Default_Text_Color);

                ImGui.InputFloat("Square Size", ref CFG.Current.MapEditor_TertiaryGrid_SectionSize);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.MapEditor_RegenerateTertiaryGrid = true;
                }
                UIHelper.Tooltip("The size of an individual grid square.");

                ImGui.InputInt("Grid Size", ref CFG.Current.MapEditor_TertiaryGrid_Size);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.MapEditor_RegenerateTertiaryGrid = true;
                }
                UIHelper.Tooltip("The number of grid squares that make up the grid.");
            }
        }
    }
}
