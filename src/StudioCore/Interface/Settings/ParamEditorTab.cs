using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface.Settings;

public class ParamEditorTab
{
    public ParamEditorTab() { }

    public void Display()
    {
        if (ImGui.BeginTabItem("Param Editor"))
        {
            // General
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.Checkbox("Use project meta", ref CFG.Current.Param_UseProjectMeta))
                {
                    if (CFG.Current.Param_UseProjectMeta)
                    {
                        ParamBank.CreateProjectMeta();
                    }

                    ParamBank.ReloadParams();
                }
                ImguiUtils.ShowHoverTooltip("Use project-specific Paramdex meta instead of Smithbox's base version.");

                ImGui.Checkbox("Use compact param editor", ref CFG.Current.UI_CompactParams);
                ImguiUtils.ShowHoverTooltip("Reduces the line height within the the Param Editor screen.");

                ImGui.Checkbox("Show advanced options in massedit popup", ref CFG.Current.Param_AdvancedMassedit);
                ImguiUtils.ShowHoverTooltip("Show additional options for advanced users within the massedit popup.");

                ImGui.Checkbox("Pinned rows stay visible", ref CFG.Current.Param_PinnedRowsStayVisible);
                ImguiUtils.ShowHoverTooltip("Pinned rows will stay visible when you scroll instead of only being pinned to the top of the list.");
            }

            // Params
            if (ImGui.CollapsingHeader("Params"))
            {
                if (ImGui.Checkbox("Sort params alphabetically", ref CFG.Current.Param_AlphabeticalParams))
                    UICache.ClearCaches();
                ImguiUtils.ShowHoverTooltip("Sort the Param View list alphabetically.");
            }

            // Rows
            if (ImGui.CollapsingHeader("Rows"))
            {
                ImGui.Checkbox("Disable line wrapping", ref CFG.Current.Param_DisableLineWrapping);
                ImguiUtils.ShowHoverTooltip("Disable the row names from wrapping within the Row View list.");

                ImGui.Checkbox("Disable row grouping", ref CFG.Current.Param_DisableRowGrouping);
                ImguiUtils.ShowHoverTooltip("Disable the grouping of connected rows in certain params, such as ItemLotParam within the Row View list.");
            }

            // Fields
            if (ImGui.CollapsingHeader("Fields"))
            {
                ImGui.Checkbox("Show community field names first", ref CFG.Current.Param_MakeMetaNamesPrimary);
                ImguiUtils.ShowHoverTooltip("Crowd-sourced names will appear before the canonical name in the Field View list.");

                ImGui.Checkbox("Show secondary field names", ref CFG.Current.Param_ShowSecondaryNames);
                ImguiUtils.ShowHoverTooltip("The crowd-sourced name (or the canonical name if the above option is enabled) will appear after the initial name in the Field View list.");

                ImGui.Checkbox("Show field data offsets", ref CFG.Current.Param_ShowFieldOffsets);
                ImguiUtils.ShowHoverTooltip("The field offset within the .PARAM file will be show to the left in the Field View List.");

                ImGui.Checkbox("Hide field references", ref CFG.Current.Param_HideReferenceRows);
                ImguiUtils.ShowHoverTooltip("Hide the generated param references for fields that link to other params.");

                ImGui.Checkbox("Hide field enums", ref CFG.Current.Param_HideEnums);
                ImguiUtils.ShowHoverTooltip("Hide the crowd-sourced namelist for index-based enum fields.");

                ImGui.Checkbox("Allow field reordering", ref CFG.Current.Param_AllowFieldReorder);
                ImguiUtils.ShowHoverTooltip("Allow the field order to be changed by an alternative order as defined within the Paramdex META file.");

                ImGui.Checkbox("Hide padding fields", ref CFG.Current.Param_HidePaddingFields);
                ImguiUtils.ShowHoverTooltip("Hides fields that are considered 'padding' in the property editor view.");

                ImGui.Checkbox("Show color preview", ref CFG.Current.Param_ShowColorPreview);
                ImguiUtils.ShowHoverTooltip("Show color preview in field column if applicable.");

                ImGui.Checkbox("Show graph visualisation", ref CFG.Current.Param_ShowGraphVisualisation);
                ImguiUtils.ShowHoverTooltip("Show graph visualisation in field column if applicable.");

                ImGui.Checkbox("Show view in map button", ref CFG.Current.Param_ViewInMapOption);
                ImguiUtils.ShowHoverTooltip("Show the view in map if applicable.");

                ImGui.Checkbox("Show view model button", ref CFG.Current.Param_ViewModelOption);
                ImguiUtils.ShowHoverTooltip("Show the view model if applicable.");
            }

            // Values
            if (ImGui.CollapsingHeader("Values"))
            {
                ImGui.Checkbox("Show inverted percentages as traditional percentages", ref CFG.Current.Param_ShowTraditionalPercentages);
                ImguiUtils.ShowHoverTooltip("Displays field values that utilise the (1 - x) pattern as traditional percentages (e.g. -20 instead of 1.2).");
            }

            // Context Menu
            if (ImGui.CollapsingHeader("Row Context Menu"))
            {
                ImGui.Checkbox("Display row name input", ref CFG.Current.Param_RowContextMenu_NameInput);
                ImguiUtils.ShowHoverTooltip("Display a row name input within the right-click context menu.");

                ImGui.Checkbox("Display row shortcut tools", ref CFG.Current.Param_RowContextMenu_ShortcutTools);
                ImguiUtils.ShowHoverTooltip("Show the shortcut tools in the right-click row context menu.");

                ImGui.Checkbox("Display row pin options", ref CFG.Current.Param_RowContextMenu_PinOptions);
                ImguiUtils.ShowHoverTooltip("Show the pin options in the right-click row context menu.");

                ImGui.Checkbox("Display row compare options", ref CFG.Current.Param_RowContextMenu_CompareOptions);
                ImguiUtils.ShowHoverTooltip("Show the compare options in the right-click row context menu.");

                ImGui.Checkbox("Display row reverse lookup option", ref CFG.Current.Param_RowContextMenu_ReverseLoopup);
                ImguiUtils.ShowHoverTooltip("Show the reverse lookup option in the right-click row context menu.");

                ImGui.Checkbox("Display row copy id option", ref CFG.Current.Param_RowContextMenu_CopyID);
                ImguiUtils.ShowHoverTooltip("Show the copy id option in the right-click row context menu.");
            }

            // Context Menu
            if (ImGui.CollapsingHeader("Field Context Menu"))
            {

                ImGui.Checkbox("Split context menu", ref CFG.Current.Param_FieldContextMenu_Split);
                ImguiUtils.ShowHoverTooltip("Split the field context menu into separate menus for separate right-click locations.");

                ImGui.Checkbox("Display field name", ref CFG.Current.Param_FieldContextMenu_Name);
                ImguiUtils.ShowHoverTooltip("Display the field name in the context menu.");

                ImGui.Checkbox("Display field description", ref CFG.Current.Param_FieldContextMenu_Description);
                ImguiUtils.ShowHoverTooltip("Display the field description in the context menu.");

                ImGui.Checkbox("Display field property info", ref CFG.Current.Param_FieldContextMenu_PropertyInfo);
                ImguiUtils.ShowHoverTooltip("Display the field property info in the context menu.");

                ImGui.Checkbox("Display field pin options", ref CFG.Current.Param_FieldContextMenu_PinOptions);
                ImguiUtils.ShowHoverTooltip("Display the field pin options in the context menu.");

                ImGui.Checkbox("Display field compare options", ref CFG.Current.Param_FieldContextMenu_CompareOptions);
                ImguiUtils.ShowHoverTooltip("Display the field compare options in the context menu.");

                ImGui.Checkbox("Display field value distribution option", ref CFG.Current.Param_FieldContextMenu_ValueDistribution);
                ImguiUtils.ShowHoverTooltip("Display the field value distribution option in the context menu.");

                ImGui.Checkbox("Display field add options", ref CFG.Current.Param_FieldContextMenu_AddOptions);
                ImguiUtils.ShowHoverTooltip("Display the field add to searchbar and mass edit options in the context menu.");

                ImGui.Checkbox("Display field references", ref CFG.Current.Param_FieldContextMenu_References);
                ImguiUtils.ShowHoverTooltip("Display the field references in the context menu.");

                ImGui.Checkbox("Display field reference search", ref CFG.Current.Param_FieldContextMenu_ReferenceSearch);
                ImguiUtils.ShowHoverTooltip("Display the field reference search in the context menu.");

                ImGui.Checkbox("Display field mass edit options", ref CFG.Current.Param_FieldContextMenu_MassEdit);
                ImguiUtils.ShowHoverTooltip("Display the field mass edit options in the context menu.");

                ImGui.Checkbox("Display full mass edit submenu", ref CFG.Current.Param_FieldContextMenu_FullMassEdit);
                ImguiUtils.ShowHoverTooltip("If enabled, the right-click context menu for fields shows a comprehensive editing popup for the massedit feature.\nIf disabled, simply shows a shortcut to the manual massedit entry element.\n(The full menu is still available from the manual popup)");
            }

            // Context Menu
            if (ImGui.CollapsingHeader("Image Preview"))
            {

                ImGui.Text("Image Preview Scale:");
                ImGui.DragFloat("##imagePreviewScale", ref CFG.Current.Param_FieldContextMenu_ImagePreviewScale, 0.1f, 0.1f, 10.0f);
                ImguiUtils.ShowHoverTooltip("Scale of the previewed image.");

                ImGui.Checkbox("Display image preview in field context menu", ref CFG.Current.Param_FieldContextMenu_ImagePreview_ContextMenu);
                ImguiUtils.ShowHoverTooltip("Display image preview of any image index fields if possible within the field context menu.");

                ImGui.Checkbox("Display image preview in field column", ref CFG.Current.Param_FieldContextMenu_ImagePreview_FieldColumn);
                ImguiUtils.ShowHoverTooltip("Display image preview of any image index fields if possible at the bottom of the field column.");
            }

            ImGui.EndTabItem();
        }
    }
}
