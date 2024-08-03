using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface.Settings;

public class TimeActEditorTab
{
    public TimeActEditorTab() { }

    public void Display()
    {
        if (ImGui.BeginTabItem("Time Act Editor"))
        {
            // Animation List
            if (ImGui.CollapsingHeader("Animation List", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Display every valid generator in animation alias", ref CFG.Current.TimeActEditor_DisplayAllGenerators);
                ImguiUtils.ShowHoverTooltip("By default only the first generator is the list is displayed, this will display them all.");
            }
            ImGui.EndTabItem();

            // Grid
            if (ImGui.CollapsingHeader("Viewport Grid"))
            {
                ImGui.SliderInt("Grid size", ref CFG.Current.TimeActEditor_Viewport_Grid_Size, 100, 1000);
                ImguiUtils.ShowHoverTooltip("The overall maximum size of the grid.\nThe grid will only update upon restarting Smithbox after changing this value.");

                ImGui.SliderInt("Grid increment", ref CFG.Current.TimeActEditor_Viewport_Grid_Square_Size, 1, 100);
                ImguiUtils.ShowHoverTooltip("The increment size of the grid.");

                var height = CFG.Current.TimeActEditor_Viewport_Grid_Height;

                ImGui.InputFloat("Grid height", ref height);
                ImguiUtils.ShowHoverTooltip("The height at which the horizontal grid sits.");

                if (height < -10000)
                    height = -10000;

                if (height > 10000)
                    height = 10000;

                CFG.Current.TimeActEditor_Viewport_Grid_Height = height;

                ImGui.ColorEdit3("Grid color", ref CFG.Current.TimeActEditor_Viewport_Grid_Color);

                if (ImGui.Button("Regenerate"))
                {
                    CFG.Current.TimeActEditor_Viewport_RegenerateMapGrid = true;
                }
                ImGui.SameLine();
                if (ImGui.Button("Reset"))
                {
                    CFG.Current.TimeActEditor_Viewport_Grid_Color = Utils.GetDecimalColor(Color.Red);
                    CFG.Current.TimeActEditor_Viewport_Grid_Size = 1000;
                    CFG.Current.TimeActEditor_Viewport_Grid_Square_Size = 10;
                    CFG.Current.TimeActEditor_Viewport_Grid_Height = 0;
                }
                ImguiUtils.ShowHoverTooltip("Resets all of the values within this section to their default values.");
            }
        }
    }
}
