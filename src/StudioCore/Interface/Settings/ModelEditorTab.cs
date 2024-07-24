using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface.Settings;

public class ModelEditorTab
{
    public ModelEditorTab() { }

    public void Display()
    {
        if (ImGui.BeginTabItem("模型编辑器 Model Editor"))
        {
            // Scene View
            if (ImGui.CollapsingHeader("模型层次 Model Hierarchy", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("使用网格显示材质名称 Display material names with meshes", ref CFG.Current.ModelEditor_DisplayMatNameOnMesh);
                ImguiUtils.ShowHoverTooltip("Display the material name that a mesh uses by the scene tree name.");

                ImGui.Checkbox("显示虚拟多边形参考ID Display dummy polygon reference ids", ref CFG.Current.ModelEditor_DisplayDmyPolyReferenceID);
                ImguiUtils.ShowHoverTooltip("Display the reference ID of a dummy polygon by the scene tree name.");
            }

            // Property View
            if (ImGui.CollapsingHeader("属性 Properties", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("显示俗称 Display community names", ref CFG.Current.ModelEditor_Enable_Commmunity_Names);
                ImguiUtils.ShowHoverTooltip("The FLVER property fields will be given crowd-sourced names instead of the canonical name.");

                ImGui.Checkbox("显示描述 Display community descriptions", ref CFG.Current.ModelEditor_Enable_Commmunity_Hints);
                ImguiUtils.ShowHoverTooltip("The FLVER property fields will be given crowd-sourced descriptions.");

            }

            // Asset Browser
            if (ImGui.CollapsingHeader("资源浏览器 Asset Browser"))
            {
                ImGui.Checkbox("显示别称 Display aliases in list", ref CFG.Current.ModelEditor_AssetBrowser_ShowAliases);
                ImguiUtils.ShowHoverTooltip("Show the aliases for each entry within the browser list as part of their displayed name.");

                ImGui.Checkbox("显示标签 Display tags in list", ref CFG.Current.ModelEditor_AssetBrowser_ShowTags);
                ImguiUtils.ShowHoverTooltip("Show the tags for each entry within the browser list as part of their displayed name.");

                ImGui.Checkbox("显示低级细节部分 Display low detail Parts in list", ref CFG.Current.ModelEditor_AssetBrowser_ShowLowDetailParts);
                ImguiUtils.ShowHoverTooltip("Show the _l (low-detail) part entries in the Model Editor instance of the Asset Browser.");
            }

            // Grid
            if (ImGui.CollapsingHeader("视图网络 Viewport Grid"))
            {
                ImGui.SliderInt("网格大小 Grid size", ref CFG.Current.ModelEditor_Viewport_Grid_Size, 100, 1000);
                ImguiUtils.ShowHoverTooltip("The overall maximum size of the grid.\nThe grid will only update upon restarting DSMS after changing this value.");

                ImGui.SliderInt("网格增加 Grid increment", ref CFG.Current.ModelEditor_Viewport_Grid_Square_Size, 1, 100);
                ImguiUtils.ShowHoverTooltip("The increment size of the grid.");

                var height = CFG.Current.ModelEditor_Viewport_Grid_Height;

                ImGui.InputFloat("网格高 Grid height", ref height);
                ImguiUtils.ShowHoverTooltip("The height at which the horizontal grid sits.");

                if (height < -10000)
                    height = -10000;

                if (height > 10000)
                    height = 10000;

                CFG.Current.ModelEditor_Viewport_Grid_Height = height;

                ImGui.SliderFloat("网格增高 Grid height increment", ref CFG.Current.ModelEditor_Viewport_Grid_Height_Increment, 0.1f, 100);
                ImguiUtils.ShowHoverTooltip("The amount to lower or raise the viewport grid height via the shortcuts.");

                ImGui.ColorEdit3("网格颜色 Grid color", ref CFG.Current.ModelEditor_Viewport_Grid_Color);

                if (ImGui.Button("重置 Reset"))
                {
                    CFG.Current.ModelEditor_Viewport_Grid_Color = Utils.GetDecimalColor(Color.Red);
                    CFG.Current.ModelEditor_Viewport_Grid_Size = 1000;
                    CFG.Current.ModelEditor_Viewport_Grid_Square_Size = 10;
                    CFG.Current.ModelEditor_Viewport_Grid_Height = 0;
                }
                ImguiUtils.ShowHoverTooltip("重置所有数据到初始状态 Resets all of the values within this section to their default values.");
            }

            ImGui.EndTabItem();
        }
    }
}
