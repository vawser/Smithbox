using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.MapEditor;

namespace StudioCore.Editors.ModelEditor;

public class ModelGridConfiguration
{
    public ModelEditorView View;
    public ProjectEntry Project;

    private TargetMapGridType CurrentGridType = TargetMapGridType.Primary;

    public ModelGridConfiguration(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        // Model Grid Configuration
        if (ImGui.CollapsingHeader($"{LOC.Get("MODEL_GridConfig_ModelGridConfiguration_Header")}##modelGridConfigTool"))
        {
            ImGui.BeginChild("ModelGridToolSection", ImGuiChildFlags.Borders);

            // Primary Configuration
            if (CurrentGridType is TargetMapGridType.Primary)
            {
                // Primary Grid
                GUI.SimpleHeader(
                    LOC.Get("MODEL_GridConfig_Primary_Grid_Header"),
                    LOC.Get("MODEL_GridConfig_Primary_Grid_Header_TT"));

                GUI.MultiButtonInput("primaryActions",
                    "toggleGrid", 
                    LOC.Get("MODEL_GridConfig_Action_Toggle_Grid_Visibility"),
                    LOC.Get("MODEL_GridConfig_Action_Toggle_Grid_Visibility_TT"),
                    TogglePrimaryGrid);

                // Position
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("MODEL_GridConfig_Grid_Position_Header"),
                    LOC.Get("MODEL_GridConfig_Grid_Position_Header_TT"));

                // X
                ImGui.InputFloat($"{LOC.Get("MODEL_GridConfig_Position_X")}##gridPosX", ref CFG.Current.ModelEditor_PrimaryGrid_Position_X);
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Position_X_TT"));

                // Y
                ImGui.InputFloat($"{LOC.Get("MODEL_GridConfig_Position_Y")}##gridPosY", ref CFG.Current.ModelEditor_PrimaryGrid_Position_Y);
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Position_Y_TT"));

                // Z
                ImGui.InputFloat($"{LOC.Get("MODEL_GridConfig_Position_Z")}##gridPosZ", ref CFG.Current.ModelEditor_PrimaryGrid_Position_Z);
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Position_Z_TT"));

                // Rotation
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("MODEL_GridConfig_Grid_Rotation_Header"),
                    LOC.Get("MODEL_GridConfig_Grid_Rotation_Header_TT"));

                // X
                ImGui.InputFloat($"{LOC.Get("MODEL_GridConfig_Rotation_X")}##gridRotX", ref CFG.Current.ModelEditor_PrimaryGrid_Rotation_X);
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Rotation_X_TT"));

                // Y
                ImGui.InputFloat($"{LOC.Get("MODEL_GridConfig_Rotation_Y")}##gridRotY", ref CFG.Current.ModelEditor_PrimaryGrid_Rotation_Y);
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Rotation_Y_TT"));
                
                // Z
                ImGui.InputFloat($"{LOC.Get("MODEL_GridConfig_Rotation_Z")}##gridRotZ", ref CFG.Current.ModelEditor_PrimaryGrid_Rotation_Z);
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Rotation_Z_TT"));

                // Color
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("MODEL_GridConfig_Color_Header"),
                    LOC.Get("MODEL_GridConfig_Color_Header_TT"));

                ImGui.ColorEdit3("##Color", ref CFG.Current.ModelEditor_PrimaryGrid_Color);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.ModelEditor_RegeneratePrimaryGrid = true;
                }
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Color_Input_TT"));

                // Square Size
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("MODEL_GridConfig_Square_Size_Header"),
                    LOC.Get("MODEL_GridConfig_Square_Size_Header_TT"));

                ImGui.InputFloat("##SquareSize", ref CFG.Current.ModelEditor_PrimaryGrid_SectionSize);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.ModelEditor_RegeneratePrimaryGrid = true;
                }
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Square_Size_Input_TT"));

                // Grid Size
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("MODEL_GridConfig_Size"),
                    LOC.Get("MODEL_GridConfig_Size_TT"));

                ImGui.InputInt("##GridSize", ref CFG.Current.ModelEditor_PrimaryGrid_Size);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.ModelEditor_RegeneratePrimaryGrid = true;
                }
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Size_Input_TT"));
            }

            // Secondary Configuration
            if (CurrentGridType is TargetMapGridType.Secondary)
            {
                // Secondary Grid
                GUI.SimpleHeader(
                    LOC.Get("MODEL_GridConfig_Secondary_Grid_Header"),
                    LOC.Get("MODEL_GridConfig_Secondary_Grid_Header_TT"));

                GUI.MultiButtonInput("secondaryActions",
                    "toggleGrid",
                    LOC.Get("MODEL_GridConfig_Action_Toggle_Grid_Visibility"),
                    LOC.Get("MODEL_GridConfig_Action_Toggle_Grid_Visibility_TT"), 
                    ToggleSecondaryGrid);

                // Position
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("MODEL_GridConfig_Grid_Position_Header"),
                    LOC.Get("MODEL_GridConfig_Grid_Position_Header_TT"));

                // X
                ImGui.InputFloat($"{LOC.Get("MODEL_GridConfig_Position_X")}##gridPosX", ref CFG.Current.ModelEditor_SecondaryGrid_Position_X);
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Position_X_TT"));

                // Y
                ImGui.InputFloat($"{LOC.Get("MODEL_GridConfig_Position_Y")}##gridPosY", ref CFG.Current.ModelEditor_SecondaryGrid_Position_Y);
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Position_Y_TT"));

                // Z
                ImGui.InputFloat($"{LOC.Get("MODEL_GridConfig_Position_Z")}##gridPosZ", ref CFG.Current.ModelEditor_SecondaryGrid_Position_Z);
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Position_Z_TT"));

                // Rotation
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("MODEL_GridConfig_Grid_Rotation_Header"),
                    LOC.Get("MODEL_GridConfig_Grid_Rotation_Header_TT"));

                // X
                ImGui.InputFloat($"{LOC.Get("MODEL_GridConfig_Rotation_X")}##gridRotX", ref CFG.Current.ModelEditor_SecondaryGrid_Rotation_X);
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Rotation_X_TT"));

                // Y
                ImGui.InputFloat($"{LOC.Get("MODEL_GridConfig_Rotation_Y")}##gridRotY", ref CFG.Current.ModelEditor_SecondaryGrid_Rotation_Y);
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Rotation_Y_TT"));

                // Z
                ImGui.InputFloat($"{LOC.Get("MODEL_GridConfig_Rotation_Z")}##gridRotZ", ref CFG.Current.ModelEditor_SecondaryGrid_Rotation_Z);
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Rotation_Z_TT"));

                // Color
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("MODEL_GridConfig_Color_Header"),
                    LOC.Get("MODEL_GridConfig_Color_Header_TT"));

                ImGui.ColorEdit3("##Color", ref CFG.Current.ModelEditor_SecondaryGrid_Color);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.ModelEditor_RegenerateSecondaryGrid = true;
                }
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Color_Input_TT"));

                // Square Size
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("MODEL_GridConfig_Square_Size_Header"),
                    LOC.Get("MODEL_GridConfig_Square_Size_Header_TT"));

                ImGui.InputFloat("##SquareSize", ref CFG.Current.ModelEditor_SecondaryGrid_SectionSize);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.ModelEditor_RegenerateSecondaryGrid = true;
                }
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Square_Size_Input_TT"));

                // Grid Size
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("MODEL_GridConfig_Size"),
                    LOC.Get("MODEL_GridConfig_Size_TT"));

                ImGui.InputInt("##GridSize", ref CFG.Current.ModelEditor_SecondaryGrid_Size);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.ModelEditor_RegenerateSecondaryGrid = true;
                }
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Size_Input_TT"));
            }

            // Tertiary Configuration
            if (CurrentGridType is TargetMapGridType.Tertiary)
            {
                // Tertiary Grid
                GUI.SimpleHeader(
                    LOC.Get("MODEL_GridConfig_Tertiary_Grid_Header"),
                    LOC.Get("MODEL_GridConfig_Tertiary_Grid_Header_TT"));

                GUI.MultiButtonInput("tertiaryActions",
                    "toggleGrid",
                    LOC.Get("MODEL_GridConfig_Action_Toggle_Grid_Visibility"),
                    LOC.Get("MODEL_GridConfig_Action_Toggle_Grid_Visibility_TT"), 
                    ToggleTertiaryGrid);

                // Position
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("MODEL_GridConfig_Grid_Position_Header"),
                    LOC.Get("MODEL_GridConfig_Grid_Position_Header_TT"));

                // X
                ImGui.InputFloat($"{LOC.Get("MODEL_GridConfig_Position_X")}##gridPosX", ref CFG.Current.ModelEditor_TertiaryGrid_Position_X);
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Position_X_TT"));

                // Y
                ImGui.InputFloat($"{LOC.Get("MODEL_GridConfig_Position_Y")}##gridPosY", ref CFG.Current.ModelEditor_TertiaryGrid_Position_Y);
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Position_Y_TT"));

                // Z
                ImGui.InputFloat($"{LOC.Get("MODEL_GridConfig_Position_Z")}##gridPosZ", ref CFG.Current.ModelEditor_TertiaryGrid_Position_Z);
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Position_Z_TT"));

                // Rotation
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("MODEL_GridConfig_Grid_Rotation_Header"),
                    LOC.Get("MODEL_GridConfig_Grid_Rotation_Header_TT"));

                // X
                ImGui.InputFloat($"{LOC.Get("MODEL_GridConfig_Rotation_X")}##gridRotX", ref CFG.Current.ModelEditor_TertiaryGrid_Rotation_X);
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Rotation_X_TT"));

                // Y
                ImGui.InputFloat($"{LOC.Get("MODEL_GridConfig_Rotation_Y")}##gridRotY", ref CFG.Current.ModelEditor_TertiaryGrid_Rotation_Y);
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Rotation_Y_TT"));

                // Z
                ImGui.InputFloat($"{LOC.Get("MODEL_GridConfig_Rotation_Z")}##gridRotZ", ref CFG.Current.ModelEditor_TertiaryGrid_Rotation_Z);
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Rotation_Z_TT"));

                // Color
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("MODEL_GridConfig_Color_Header"),
                    LOC.Get("MODEL_GridConfig_Color_Header_TT"));

                ImGui.ColorEdit3("##Color", ref CFG.Current.ModelEditor_TertiaryGrid_Color);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.ModelEditor_RegenerateTertiaryGrid = true;
                }
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Color_Input_TT"));

                // Square Size
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("MODEL_GridConfig_Square_Size_Header"),
                    LOC.Get("MODEL_GridConfig_Square_Size_Header_TT"));

                ImGui.InputFloat("##SquareSize", ref CFG.Current.ModelEditor_TertiaryGrid_SectionSize);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.ModelEditor_RegenerateTertiaryGrid = true;
                }
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Square_Size_Input_TT"));

                // Grid Size
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("MODEL_GridConfig_Size"),
                    LOC.Get("MODEL_GridConfig_Size_TT"));

                ImGui.InputInt("##GridSize", ref CFG.Current.ModelEditor_TertiaryGrid_Size);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.ModelEditor_RegenerateTertiaryGrid = true;
                }
                GUI.Tooltip(LOC.Get("MODEL_GridConfig_Size_Input_TT"));
            }
            
            // Actions
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("MODEL_GridConfig_Actions_Header"),
                LOC.Get("MODEL_GridConfig_Actions_Header_TT"));

            GUI.MultiButtonInput("gridActions",
                "setGridToPrimary", 
                LOC.Get("MODEL_GridConfig_Toggle_Primary_Grid_Action"),
                LOC.Get("MODEL_GridConfig_Toggle_Primary_Grid_Action_TT"),
                ViewPrimaryGrid,

                "setGridToSecondary",
                LOC.Get("MODEL_GridConfig_Toggle_Secondary_Grid_Action"),
                LOC.Get("MODEL_GridConfig_Toggle_Secondary_Grid_Action_TT"), 
                ViewSecondaryGrid,

                "setGridToTertiary",
                LOC.Get("MODEL_GridConfig_Toggle_Tertiary_Grid_Action"),
                LOC.Get("MODEL_GridConfig_Toggle_Tertiary_Grid_Action_TT"), 
                ViewTertiaryGrid);

            ImGui.EndChild();
        }
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
        CFG.Current.ModelEditor_DisplayPrimaryGrid = !CFG.Current.ModelEditor_DisplayPrimaryGrid;
    }
    public void ToggleSecondaryGrid()
    {
        CFG.Current.ModelEditor_DisplaySecondaryGrid = !CFG.Current.ModelEditor_DisplaySecondaryGrid;
    }
    public void ToggleTertiaryGrid()
    {
        CFG.Current.ModelEditor_DisplayTertiaryGrid = !CFG.Current.ModelEditor_DisplayTertiaryGrid;
    }
}
