using ImGuiNET;
using StudioCore.Core.Project;
using StudioCore.Editors.MapEditor.Tools;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Configuration.Settings;

public class MapEditorTab
{
    public MapEditorTab() { }

    public void Display()
    {
        // General
        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Allow map unload", ref CFG.Current.MapEditor_EnableMapUnload);
            UIHelper.ShowHoverTooltip("When enabled, a map's resources will be unloaded and released when a map is unloaded. If disabled, they are kept in memory until Smithbox closes.");

            ImGui.Checkbox("Enable map load on double-click", ref CFG.Current.MapEditor_Enable_Map_Load_on_Double_Click);
            UIHelper.ShowHoverTooltip("This option will cause double-clicking on a map in the map object list to load it.");

            ImGui.Checkbox("Exclude loaded maps from search filter", ref CFG.Current.MapEditor_Always_List_Loaded_Maps);
            UIHelper.ShowHoverTooltip("This option will cause loaded maps to always be visible within the map list, ignoring the search filter.");

            if (Smithbox.ProjectHandler.CurrentProject.Config != null)
            {
                if (Smithbox.ProjectType is ProjectType.ER)
                {
                    ImGui.Checkbox("Enable Elden Ring auto map offset", ref CFG.Current.Viewport_Enable_ER_Auto_Map_Offset);
                    UIHelper.ShowHoverTooltip("");

                    ImGui.Checkbox("Enable Elden Ring collisions", ref CFG.Current.MapEditor_LoadCollisions_ER);
                    UIHelper.ShowHoverTooltip("Enables the viewing of Elden Ring collisions. Note this will add delay to map loading if enabled.");
                }
            }

            ImGui.Checkbox("Enable global property search", ref CFG.Current.MapEditor_LoadMapQueryData);
            UIHelper.ShowHoverTooltip("This option will allow the global property search to be used. Note, this will load all map files into memory.\nYou need to restart Smithbox after enabling this.");

        }

        // Scene View
        if (ImGui.CollapsingHeader("Map Object List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display list sorting type", ref CFG.Current.MapEditor_MapObjectList_ShowListSortingType);
            UIHelper.ShowHoverTooltip("Display the list sorting type combo box.");

            ImGui.Checkbox("Display map id search", ref CFG.Current.MapEditor_MapObjectList_ShowMapIdSearch);
            UIHelper.ShowHoverTooltip("Display the map id search text box.");

            ImGui.Checkbox("Display map content search", ref CFG.Current.MapEditor_MapObjectList_ShowMapContentSearch);
            UIHelper.ShowHoverTooltip("Display the map object list search text box.");

            ImGui.Checkbox("Display map groups interface", ref CFG.Current.MapEditor_ShowMapGroups);
            UIHelper.ShowHoverTooltip("Display the map group drop-downs.");

            ImGui.Checkbox("Display world map interface", ref CFG.Current.MapEditor_ShowWorldMapButtons);
            UIHelper.ShowHoverTooltip("Display the world map buttons.");

            ImGui.Checkbox("Display map categories", ref CFG.Current.MapEditor_DisplayMapCategories);
            UIHelper.ShowHoverTooltip("If defined, display maps in their assigned map category groupings.");

            ImGui.Separator();

            ImGui.Checkbox("Display map names", ref CFG.Current.MapEditor_MapObjectList_ShowMapNames);
            UIHelper.ShowHoverTooltip("Map names will be displayed within the scene view list.");

            ImGui.Checkbox("Display character names", ref CFG.Current.MapEditor_MapObjectList_ShowCharacterNames);
            UIHelper.ShowHoverTooltip("Characters names will be displayed within the scene view list.");

            ImGui.Checkbox("Display asset names", ref CFG.Current.MapEditor_MapObjectList_ShowAssetNames);
            UIHelper.ShowHoverTooltip("Asset/object names will be displayed within the scene view list.");

            ImGui.Checkbox("Display map piece names", ref CFG.Current.MapEditor_MapObjectList_ShowMapPieceNames);
            UIHelper.ShowHoverTooltip("Map piece names will be displayed within the scene view list.");

            ImGui.Checkbox("Display treasure names", ref CFG.Current.MapEditor_MapObjectList_ShowTreasureNames);
            UIHelper.ShowHoverTooltip("Treasure itemlot names will be displayed within the scene view list.");
        }

        // Property View
        if (ImGui.CollapsingHeader("Properties", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display community names", ref CFG.Current.MapEditor_Enable_Commmunity_Names);
            UIHelper.ShowHoverTooltip("The MSB property fields will be given crowd-sourced names instead of the canonical name.");

            ImGui.Checkbox("Display padding fields", ref CFG.Current.MapEditor_Enable_Padding_Fields);
            UIHelper.ShowHoverTooltip("The MSB property fields that are considered padding.");

            ImGui.Checkbox("Display obsolete fields", ref CFG.Current.MapEditor_Enable_Obsolete_Fields);
            UIHelper.ShowHoverTooltip("The MSB property fields that are considered obsolete.");

            ImGui.Checkbox("Enable complex rename", ref CFG.Current.MapEditor_Enable_Referenced_Rename);
            UIHelper.ShowHoverTooltip("This option will allow renaming an object to also rename every reference to it, but will require a confirmation to apply a rename");

            ImGui.Checkbox("Display property info", ref CFG.Current.MapEditor_Enable_Property_Info);
            UIHelper.ShowHoverTooltip("The MSB property fields show the property info, such as minimum and maximum values, when right-clicked.");

            ImGui.Checkbox("Display property filter", ref CFG.Current.MapEditor_Enable_Property_Filter);
            UIHelper.ShowHoverTooltip("The MSB property filter combo-box will be visible.");
        }

        // Asset Browser
        if (ImGui.CollapsingHeader("Asset Browser", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display aliases in list", ref CFG.Current.MapEditor_AssetBrowser_ShowAliases);
            UIHelper.ShowHoverTooltip("Show the aliases for each entry within the browser list as part of their displayed name.");

            ImGui.Checkbox("Display tags in list", ref CFG.Current.MapEditor_AssetBrowser_ShowTags);
            UIHelper.ShowHoverTooltip("Show the tags for each entry within the browser list as part of their displayed name.");

            ImGui.Checkbox("Display low detail Parts in list", ref CFG.Current.MapEditor_AssetBrowser_ShowLowDetailParts);
            UIHelper.ShowHoverTooltip("Show the _l (low-detail) part entries in the Model Editor instance of the Asset Browser.");
        }

        // Additional Property Information
        if (ImGui.CollapsingHeader("Additional Property Information", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display additional property information at the top", ref CFG.Current.MapEditor_Enable_Property_Property_TopDecoration);
            UIHelper.ShowHoverTooltip("The additional property information will be displayed at the top of the properties window. By default they will appear at the bottom.");

            ImGui.Checkbox("Display information about Map Object class", ref CFG.Current.MapEditor_Enable_Property_Property_Class_Info);
            UIHelper.ShowHoverTooltip("The MSB property view will display additional information relating to the map object's class.");

            ImGui.Checkbox("Display information about specific properties", ref CFG.Current.MapEditor_Enable_Property_Property_SpecialProperty_Info);
            UIHelper.ShowHoverTooltip("The MSB property view will display additional information relating to specific properties, such as the alias for a Map ID a property or set of properties represents.");

            ImGui.Checkbox("Display references by for the Map Object", ref CFG.Current.MapEditor_Enable_Property_Property_ReferencesBy);
            UIHelper.ShowHoverTooltip("The MSB property view will display references by the selected map object.");

            ImGui.Checkbox("Display references to the Map Object", ref CFG.Current.MapEditor_Enable_Property_Property_ReferencesTo);
            UIHelper.ShowHoverTooltip("The MSB property view will display references to the selected map object.");

            ImGui.Checkbox("Display quick-links to params used by the Map Object", ref CFG.Current.MapEditor_Enable_Param_Quick_Links);
            UIHelper.ShowHoverTooltip("The MSB property view will display quick-links to related params pointed to within the properties for the selected map object.");
        }

        // Substitutions
        if (ImGui.CollapsingHeader("Substitutions", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Substitute c0000 entity", ref CFG.Current.MapEditor_Substitute_PseudoPlayer_Model);
            UIHelper.ShowHoverTooltip("The c0000 enemy that represents the player-like enemies will be given a visual model substitution so it is visible.");

            ImGui.InputText("##modelString", ref CFG.Current.MapEditor_Substitute_PseudoPlayer_ChrID, 255);
            UIHelper.ShowHoverTooltip("The Chr ID of the model you want to use as the replacement.");
        }

        // Grid
        if (ImGui.CollapsingHeader("Viewport Grid"))
        {
            ImGui.SliderInt("Grid size", ref CFG.Current.MapEditor_Viewport_Grid_Size, 100, 1000);
            UIHelper.ShowHoverTooltip("The overall maximum size of the grid.\nThe grid will only update upon restarting DSMS after changing this value.");

            ImGui.SliderInt("Grid increment", ref CFG.Current.MapEditor_Viewport_Grid_Square_Size, 1, 100);
            UIHelper.ShowHoverTooltip("The increment size of the grid.");

            var height = CFG.Current.MapEditor_Viewport_Grid_Height;

            ImGui.InputFloat("Grid height", ref height);
            UIHelper.ShowHoverTooltip("The height at which the horizontal grid sits.");

            if (height < -10000)
                height = -10000;

            if (height > 10000)
                height = 10000;

            CFG.Current.MapEditor_Viewport_Grid_Height = height;

            ImGui.SliderFloat("Grid height increment", ref CFG.Current.MapEditor_Viewport_Grid_Height_Increment, 0.1f, 100);
            UIHelper.ShowHoverTooltip("The amount to lower or raise the viewport grid height via the shortcuts.");

            ImGui.ColorEdit3("Grid color", ref CFG.Current.MapEditor_Viewport_Grid_Color);

            if (ImGui.Button("Reset"))
            {
                CFG.Current.MapEditor_Viewport_Grid_Color = Utils.GetDecimalColor(Color.Red);
                CFG.Current.MapEditor_Viewport_Grid_Size = 1000;
                CFG.Current.MapEditor_Viewport_Grid_Square_Size = 10;
                CFG.Current.MapEditor_Viewport_Grid_Height = 0;
            }
            UIHelper.ShowHoverTooltip("Resets all of the values within this section to their default values.");
        }

        // Selection Groups
        if (ImGui.CollapsingHeader("Selection Groups", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Frame selection group on select", ref CFG.Current.MapEditor_SelectionGroup_FrameSelection);
            UIHelper.ShowHoverTooltip("Frame the selection group entities automatically in the viewport when selecting a group.");

            ImGui.Checkbox("Enable group auto-creation", ref CFG.Current.MapEditor_SelectionGroup_AutoCreation);
            UIHelper.ShowHoverTooltip("The selection group will be given the name of the first entity within the selection as the group name and no tags, bypassing the creation prompt.");

            ImGui.Checkbox("Enable group deletion prompt", ref CFG.Current.MapEditor_SelectionGroup_ConfirmDelete);
            UIHelper.ShowHoverTooltip("Display the confirmation dialog when deleting a group.");

            ImGui.Checkbox("Show keybind in selection group name", ref CFG.Current.MapEditor_SelectionGroup_ShowKeybind);
            UIHelper.ShowHoverTooltip("Append the keybind hint to the selection group name.");

            ImGui.Checkbox("Show tags in selection group name", ref CFG.Current.MapEditor_SelectionGroup_ShowTags);
            UIHelper.ShowHoverTooltip("Append the tags to the selection group name.");
        }

        // World Map
        if (ImGui.CollapsingHeader("World Map", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Enable automatic map filtering on click", ref CFG.Current.WorldMap_EnableFilterOnClick);
            UIHelper.ShowHoverTooltip("Left-clicking on the world map will automatically filter the map list to the specific tiles you clicked.");

            ImGui.Checkbox("Enable automatic map loading on click", ref CFG.Current.WorldMap_EnableLoadOnClick);
            UIHelper.ShowHoverTooltip("Right-clicking on the world map will automatically load the maps you clicked.");
        }
    }
}
