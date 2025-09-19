using Hexa.NET.ImGui;
using StudioCore;
using StudioCore.Configuration;
using StudioCore.Editors.MapEditor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Program.Editors.MapEditor.Tools;

public class MapGridConfiguration
{
    private MapEditorScreen Editor;

    public MapGridConfiguration(MapEditorScreen screen)
    {
        Editor = screen;
    }

    public void Display()
    {
        if (ImGui.CollapsingHeader("Map Grid Configuration"))
        {
            if(ImGui.Button("Toggle Grid Visibility", DPI.StandardButtonSize))
            {
                CFG.Current.Interface_MapEditor_Viewport_Grid = !CFG.Current.Interface_MapEditor_Viewport_Grid;
            }

            UIHelper.SimpleHeader("positionHeader", "Position", "The position configuration for the grid.", UI.Current.ImGui_Default_Text_Color);

            ImGui.InputFloat("Grid Position: X", ref CFG.Current.MapEditor_Viewport_Grid_Position_X);
            UIHelper.Tooltip("The position of the grid on the X-axis.");

            ImGui.InputFloat("Grid Position: Y", ref CFG.Current.MapEditor_Viewport_Grid_Position_Y);
            UIHelper.Tooltip("The position of the grid on the Y-axis.");

            ImGui.InputFloat("Grid Position: Z", ref CFG.Current.MapEditor_Viewport_Grid_Position_Z);
            UIHelper.Tooltip("The position of the grid on the Z-axis.");

            UIHelper.SimpleHeader("rotationHeader", "Rotation", "The rotation configuration for the grid.", UI.Current.ImGui_Default_Text_Color);

            ImGui.InputFloat("Grid Rotation: X", ref CFG.Current.MapEditor_Viewport_Grid_Rotation_X);
            UIHelper.Tooltip("The rotation of the grid on the X-axis.");

            ImGui.InputFloat("Grid Rotation: Y", ref CFG.Current.MapEditor_Viewport_Grid_Rotation_Y);
            UIHelper.Tooltip("The rotation of the grid on the Y-axis.");

            ImGui.InputFloat("Grid Rotation: Z", ref CFG.Current.MapEditor_Viewport_Grid_Rotation_Z);
            UIHelper.Tooltip("The rotation of the grid on the Z-axis.");

            UIHelper.SimpleHeader("colorHeader", "Color", "The color configuration for the grid.", UI.Current.ImGui_Default_Text_Color);

            ImGui.ColorEdit3("Grid Color", ref CFG.Current.MapEditor_Viewport_Grid_Color);
            if(ImGui.IsItemDeactivatedAfterEdit())
            {
                CFG.Current.MapEditor_Viewport_RegenerateMapGrid = true;
            }
            UIHelper.Tooltip("The color of the grid.");

            UIHelper.SimpleHeader("sizeHeader", "Size", "The size configuration for the grid.", UI.Current.ImGui_Default_Text_Color);

            ImGui.InputInt("Square Size", ref CFG.Current.MapEditor_Viewport_Grid_Square_Size);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                CFG.Current.MapEditor_Viewport_RegenerateMapGrid = true;
            }
            UIHelper.Tooltip("The size of an individual grid square.");

            ImGui.InputInt("Grid Size", ref CFG.Current.MapEditor_Viewport_Grid_Size);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                CFG.Current.MapEditor_Viewport_RegenerateMapGrid = true;
            }
            UIHelper.Tooltip("The number of grid squares that make up the grid.");
        }
    }
}
