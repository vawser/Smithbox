using ImGuiNET;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface.Settings;

public class MapEditorTab
{
    public MapEditorTab() { }

    public void Display()
    {
        if (ImGui.BeginTabItem("地图编辑器 Map Editor"))
        {
            // General
            if (ImGui.CollapsingHeader("总览 General", ImGuiTreeNodeFlags.DefaultOpen))
            {

                ImGui.Checkbox("双击加载地图 Enable map load on double-click", ref CFG.Current.MapEditor_Enable_Map_Load_on_Double_Click);
                ImguiUtils.ShowHoverTooltip("此选项将使加载的地图在地图列表中始终可见，忽略搜索过滤器。 This option will cause double-clicking on a map in the map object list to load it.");

                ImGui.Checkbox("排除搜索结果 Exclude loaded maps from search filter", ref CFG.Current.MapEditor_Always_List_Loaded_Maps);
                ImguiUtils.ShowHoverTooltip("此选项将导致加载的地图在地图列表中始终可见，忽略搜索过滤器。 This option will cause loaded maps to always be visible within the map list, ignoring the search filter.");

                if (Smithbox.ProjectHandler.CurrentProject.Config != null)
                {
                    if (Smithbox.ProjectType is ProjectType.ER)
                    {
                        ImGui.Checkbox("启用ER自动地图偏移 Enable Elden Ring auto map offset", ref CFG.Current.Viewport_Enable_ER_Auto_Map_Offset);
                        ImguiUtils.ShowHoverTooltip("");

                        ImGui.Checkbox("显示ER碰撞 Enable Elden Ring collisions", ref CFG.Current.MapEditor_LoadCollisions_ER);
                        ImguiUtils.ShowHoverTooltip("Enables the viewing of Elden Ring collisions. Note this will add delay to map loading if enabled.");
                    }
                }
            }

            // Scene View
            if (ImGui.CollapsingHeader("地图对象列表 Map Object List"))
            {
                ImGui.Checkbox("显示列表排序类型 Display list sorting type", ref CFG.Current.MapEditor_MapObjectList_ShowListSortingType);
                ImguiUtils.ShowHoverTooltip("显示列表排序类型组合框。 Display the list sorting type combo box.");

                ImGui.Checkbox("显示地图对象列表搜索 Display map object list search", ref CFG.Current.MapEditor_MapObjectList_ShowMapIdSearch);
                ImguiUtils.ShowHoverTooltip("显示地图对象列表搜索文本框。 Display the map object list search text box.");

                ImGui.Checkbox("显示地图组界面 Display map groups interface", ref CFG.Current.MapEditor_ShowMapGroups);
                ImguiUtils.ShowHoverTooltip("显示地图组下拉框。 Display the map group drop-downs.");

                ImGui.Checkbox("显示世界地图界面 Display world map interface", ref CFG.Current.MapEditor_ShowWorldMapButtons);
                ImguiUtils.ShowHoverTooltip("显示世界地图按钮。 Display the world map buttons.");

                ImGui.Separator();

                ImGui.Checkbox("显示地图名称 Display map names", ref CFG.Current.MapEditor_MapObjectList_ShowMapNames);
                ImguiUtils.ShowHoverTooltip("地图名称将在场景视图列表中显示。 Map names will be displayed within the scene view list.");

                ImGui.Checkbox("显示角色名称 Display character names", ref CFG.Current.MapEditor_MapObjectList_ShowCharacterNames);
                ImguiUtils.ShowHoverTooltip("角色名称将在场景视图列表中显示。 Characters names will be displayed within the scene view list.");

                ImGui.Checkbox("显示资产名称 Display asset names", ref CFG.Current.MapEditor_MapObjectList_ShowAssetNames);
                ImguiUtils.ShowHoverTooltip("资产/对象名称将在场景视图列表中显示。 Asset/object names will be displayed within the scene view list.");

                ImGui.Checkbox("显示地图片段名称 Display map piece names", ref CFG.Current.MapEditor_MapObjectList_ShowMapPieceNames);
                ImguiUtils.ShowHoverTooltip("地图片段名称将在场景视图列表中显示。 Map piece names will be displayed within the scene view list.");

                ImGui.Checkbox("显示宝藏名称 Display treasure names", ref CFG.Current.MapEditor_MapObjectList_ShowTreasureNames);
                ImguiUtils.ShowHoverTooltip("宝藏物品名将在场景视图列表中显示。 Treasure itemlot names will be displayed within the scene view list.");

            }

            // Property View
            if (ImGui.CollapsingHeader("属性 Properties"))
            {
                ImGui.Checkbox("显示俗称 Display community names", ref CFG.Current.MapEditor_Enable_Commmunity_Names);
                ImguiUtils.ShowHoverTooltip("MSB 属性字段将显示众包名称，而非规范名称\nThe MSB property fields will be given crowd-sourced names instead of the canonical name.");

                ImGui.Checkbox("显示描述 Display community descriptions", ref CFG.Current.MapEditor_Enable_Commmunity_Hints);
                ImguiUtils.ShowHoverTooltip("MSB 属性字段将显示众包描述\nThe MSB property fields will be given crowd-sourced descriptions.");

                ImGui.Checkbox("显示属性 Display property info", ref CFG.Current.MapEditor_Enable_Property_Info);
                ImguiUtils.ShowHoverTooltip("右键点击时，MSB 属性字段将显示属性信息，如最小值和最大值\nThe MSB property fields show the property info, such as minimum and maximum values, when right-clicked.");

                ImGui.Checkbox("显示属性筛选器 Display property filter", ref CFG.Current.MapEditor_Enable_Property_Filter);
                ImguiUtils.ShowHoverTooltip("MSB 属性筛选器组合框将可见\nThe MSB property filter combo-box will be visible.");
            }

            // Asset Browser
            if (ImGui.CollapsingHeader("资源浏览器 Asset Browser"))
            {
                ImGui.Checkbox("显示俗称 Display aliases in list", ref CFG.Current.MapEditor_AssetBrowser_ShowAliases);
                ImguiUtils.ShowHoverTooltip("Show the aliases for each entry within the browser list as part of their displayed name.");

                ImGui.Checkbox("显示标签 Display tags in list", ref CFG.Current.MapEditor_AssetBrowser_ShowTags);
                ImguiUtils.ShowHoverTooltip("Show the tags for each entry within the browser list as part of their displayed name.");

                ImGui.Checkbox("显示低级细节 Display low detail Parts in list", ref CFG.Current.MapEditor_AssetBrowser_ShowLowDetailParts);
                ImguiUtils.ShowHoverTooltip("Show the _l (low-detail) part entries in the Model Editor instance of the Asset Browser.");
            }

            // Additional Property Information
            if (ImGui.CollapsingHeader("附加属性信息 Additional Property Information"))
            {
                ImGui.Checkbox("显示到顶部 Display additional property information at the top", ref CFG.Current.MapEditor_Enable_Property_Property_TopDecoration);
                ImguiUtils.ShowHoverTooltip("The additional property information will be displayed at the top of the properties window. By default they will appear at the bottom.");

                ImGui.Checkbox("显示地图对象 Display information about Map Object class", ref CFG.Current.MapEditor_Enable_Property_Property_Class_Info);
                ImguiUtils.ShowHoverTooltip("The MSB property view will display additional information relating to the map object's class.");

                ImGui.Checkbox("显示特殊属性 Display information about specific properties", ref CFG.Current.MapEditor_Enable_Property_Property_SpecialProperty_Info);
                ImguiUtils.ShowHoverTooltip("The MSB property view will display additional information relating to specific properties, such as the alias for a Map ID a property or set of properties represents.");

                ImGui.Checkbox("显示被引用对象 Display references by for the Map Object", ref CFG.Current.MapEditor_Enable_Property_Property_ReferencesBy);
                ImguiUtils.ShowHoverTooltip("The MSB property view will display references by the selected map object.");

                ImGui.Checkbox("显示引用对象 Display references to the Map Object", ref CFG.Current.MapEditor_Enable_Property_Property_ReferencesTo);
                ImguiUtils.ShowHoverTooltip("The MSB property view will display references to the selected map object.");

                ImGui.Checkbox("显示被地图快速链接的参数 Display quick-links to params used by the Map Object", ref CFG.Current.MapEditor_Enable_Param_Quick_Links);
                ImguiUtils.ShowHoverTooltip("The MSB property view will display quick-links to related params pointed to within the properties for the selected map object.");
            }

            // Substitutions
            if (ImGui.CollapsingHeader("替代 Substitutions"))
            {
                ImGui.Checkbox("取代c0000实例 Substitute c0000 entity", ref CFG.Current.MapEditor_Substitute_PseudoPlayer_Model);
                ImguiUtils.ShowHoverTooltip("The c0000 enemy that represents the player-like enemies will be given a visual model substitution so it is visible.");

                ImGui.InputText("##modelString", ref CFG.Current.MapEditor_Substitute_PseudoPlayer_ChrID, 255);
                ImguiUtils.ShowHoverTooltip("The Chr ID of the model you want to use as the replacement.");
            }

            // Grid
            if (ImGui.CollapsingHeader("视图网格 Viewport Grid"))
            {
                ImGui.SliderInt("网格大小 Grid size", ref CFG.Current.MapEditor_Viewport_Grid_Size, 100, 1000);
                ImguiUtils.ShowHoverTooltip("The overall maximum size of the grid.\nThe grid will only update upon restarting DSMS after changing this value.");

                ImGui.SliderInt("网格增加 Grid increment", ref CFG.Current.MapEditor_Viewport_Grid_Square_Size, 1, 100);
                ImguiUtils.ShowHoverTooltip("The increment size of the grid.");

                var height = CFG.Current.MapEditor_Viewport_Grid_Height;

                ImGui.InputFloat("网格高度 Grid height", ref height);
                ImguiUtils.ShowHoverTooltip("The height at which the horizontal grid sits.");

                if (height < -10000)
                    height = -10000;

                if (height > 10000)
                    height = 10000;

                CFG.Current.MapEditor_Viewport_Grid_Height = height;

                ImGui.SliderFloat("网格增高 Grid height increment", ref CFG.Current.MapEditor_Viewport_Grid_Height_Increment, 0.1f, 100);
                ImguiUtils.ShowHoverTooltip("The amount to lower or raise the viewport grid height via the shortcuts.");

                ImGui.ColorEdit3("网格颜色 Grid color", ref CFG.Current.MapEditor_Viewport_Grid_Color);

                if (ImGui.Button("重置 Reset"))
                {
                    CFG.Current.MapEditor_Viewport_Grid_Color = Utils.GetDecimalColor(Color.Red);
                    CFG.Current.MapEditor_Viewport_Grid_Size = 1000;
                    CFG.Current.MapEditor_Viewport_Grid_Square_Size = 10;
                    CFG.Current.MapEditor_Viewport_Grid_Height = 0;
                }
                ImguiUtils.ShowHoverTooltip("重置到初始状态 Resets all of the values within this section to their default values.");
            }

            // Selection Groups
            if (ImGui.CollapsingHeader("可选组 Selection Groups", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("选择时选中帧选组 Frame selection group on select", ref CFG.Current.MapEditor_SelectionGroup_FrameSelection);
                ImguiUtils.ShowHoverTooltip("Frame the selection group entities automatically in the viewport when selecting a group.");

                ImGui.Checkbox("启用组自动创建 Enable group auto-creation", ref CFG.Current.MapEditor_SelectionGroup_AutoCreation);
                ImguiUtils.ShowHoverTooltip("The selection group will be given the name of the first entity within the selection as the group name and no tags, bypassing the creation prompt.");

                ImGui.Checkbox("启用删除提示 Enable group deletion prompt", ref CFG.Current.MapEditor_SelectionGroup_ConfirmDelete);
                ImguiUtils.ShowHoverTooltip("Display the confirmation dialog when deleting a group.");

                ImGui.Checkbox("显示按键 Show keybind in selection group name", ref CFG.Current.MapEditor_SelectionGroup_ShowKeybind);
                ImguiUtils.ShowHoverTooltip("Append the keybind hint to the selection group name.");

                ImGui.Checkbox("显示标签 Show tags in selection group name", ref CFG.Current.MapEditor_SelectionGroup_ShowTags);
                ImguiUtils.ShowHoverTooltip("Append the tags to the selection group name.");
            }

            // World Map
            if (ImGui.CollapsingHeader("世界地图 World Map"))
            {
                ImGui.Checkbox("启用单击自动过滤地图 Enable automatic map filtering on click", ref CFG.Current.WorldMap_EnableFilterOnClick);
                ImguiUtils.ShowHoverTooltip("Left-clicking on the world map will automatically filter the map list to the specific tiles you clicked.");

                ImGui.Checkbox("启用单击自动加载地图 Enable automatic map loading on click", ref CFG.Current.WorldMap_EnableLoadOnClick);
                ImguiUtils.ShowHoverTooltip("Right-clicking on the world map will automatically load the maps you clicked.");
            }

            ImGui.Unindent();
            ImGui.EndTabItem();
        }
    }
}
