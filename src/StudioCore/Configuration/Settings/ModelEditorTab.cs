using ImGuiNET;
using SoulsFormats;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Configuration.Settings;

public class ModelEditorTab
{
    public ModelEditorTab() { }

    public void Display()
    {
        // Scene View
        if (ImGui.CollapsingHeader("Model Hierarchy", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display material names with meshes", ref CFG.Current.ModelEditor_DisplayMatNameOnMesh);
            UIHelper.ShowHoverTooltip("Display the material name that a mesh uses by the scene tree name.");

            ImGui.Checkbox("Display dummy polygon reference ids", ref CFG.Current.ModelEditor_DisplayDmyPolyReferenceID);
            UIHelper.ShowHoverTooltip("Display the reference ID of a dummy polygon by the scene tree name.");
        }

        // Property View
        if (ImGui.CollapsingHeader("Properties", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display community names", ref CFG.Current.ModelEditor_Enable_Commmunity_Names);
            UIHelper.ShowHoverTooltip("The FLVER property fields will be given crowd-sourced names instead of the canonical name.");

            ImGui.Checkbox("Display community descriptions", ref CFG.Current.ModelEditor_Enable_Commmunity_Hints);
            UIHelper.ShowHoverTooltip("The FLVER property fields will be given crowd-sourced descriptions.");

        }

        // Asset Browser
        if (ImGui.CollapsingHeader("Asset Browser"))
        {
            ImGui.Checkbox("Display aliases in list", ref CFG.Current.ModelEditor_AssetBrowser_ShowAliases);
            UIHelper.ShowHoverTooltip("Show the aliases for each entry within the browser list as part of their displayed name.");

            ImGui.Checkbox("Display tags in list", ref CFG.Current.ModelEditor_AssetBrowser_ShowTags);
            UIHelper.ShowHoverTooltip("Show the tags for each entry within the browser list as part of their displayed name.");

            ImGui.Checkbox("Display low detail Parts in list", ref CFG.Current.ModelEditor_AssetBrowser_ShowLowDetailParts);
            UIHelper.ShowHoverTooltip("Show the _l (low-detail) part entries in the Model Editor instance of the Asset Browser.");
        }

        // Grid
        if (ImGui.CollapsingHeader("Viewport Grid"))
        {
            ImGui.SliderInt("Grid size", ref CFG.Current.ModelEditor_Viewport_Grid_Size, 100, 1000);
            UIHelper.ShowHoverTooltip("The overall maximum size of the grid.\nThe grid will only update upon restarting Smithbox after changing this value.");

            ImGui.SliderInt("Grid increment", ref CFG.Current.ModelEditor_Viewport_Grid_Square_Size, 1, 100);
            UIHelper.ShowHoverTooltip("The increment size of the grid.");

            var height = CFG.Current.ModelEditor_Viewport_Grid_Height;

            ImGui.InputFloat("Grid height", ref height);
            UIHelper.ShowHoverTooltip("The height at which the horizontal grid sits.");

            if (height < -10000)
                height = -10000;

            if (height > 10000)
                height = 10000;

            CFG.Current.ModelEditor_Viewport_Grid_Height = height;

            ImGui.SliderFloat("Grid height increment", ref CFG.Current.ModelEditor_Viewport_Grid_Height_Increment, 0.1f, 100);
            UIHelper.ShowHoverTooltip("The amount to lower or raise the viewport grid height via the shortcuts.");

            ImGui.ColorEdit3("Grid color", ref CFG.Current.ModelEditor_Viewport_Grid_Color);

            if (ImGui.Button("Reset"))
            {
                CFG.Current.ModelEditor_Viewport_Grid_Color = Utils.GetDecimalColor(Color.Red);
                CFG.Current.ModelEditor_Viewport_Grid_Size = 1000;
                CFG.Current.ModelEditor_Viewport_Grid_Square_Size = 10;
                CFG.Current.ModelEditor_Viewport_Grid_Height = 0;
            }
            UIHelper.ShowHoverTooltip("Resets all of the values within this section to their default values.");
        }
    }
}
