using ImGuiNET;
using StudioCore.Banks.GameOffsetBank;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Configuration.Settings;

public class ParamEditorTab
{
    public ParamEditorTab()
    {
        SelectedGameOffsetData = null;
    }

    private GameOffsetResource SelectedGameOffsetData { get; set; }

    public void Display()
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
            UIHelper.ShowHoverTooltip("Use project-specific PARAM meta instead of Smithbox's base version.");

            ImGui.Checkbox("Use compact param editor", ref CFG.Current.UI_CompactParams);
            UIHelper.ShowHoverTooltip("Reduces the line height within the the Param Editor screen.");

            ImGui.Checkbox("Show advanced options in massedit popup", ref CFG.Current.Param_AdvancedMassedit);
            UIHelper.ShowHoverTooltip("Show additional options for advanced users within the massedit popup.");

            ImGui.Checkbox("Pinned rows stay visible", ref CFG.Current.Param_PinnedRowsStayVisible);
            UIHelper.ShowHoverTooltip("Pinned rows will stay visible when you scroll instead of only being pinned to the top of the list.");
        }

        if (ImGui.CollapsingHeader("Regulation Data", ImGuiTreeNodeFlags.DefaultOpen))
        {
            switch(Smithbox.ProjectType)
            {
                case ProjectType.DES:
                    ImGui.Checkbox("Strip row names on save", ref CFG.Current.Param_StripRowNamesOnSave_DES);
                    break;

                case ProjectType.DS1:
                case ProjectType.DS1R:
                    ImGui.Checkbox("Strip row names on save", ref CFG.Current.Param_StripRowNamesOnSave_DS1);
                    break;

                case ProjectType.DS2:
                case ProjectType.DS2S:
                    ImGui.Checkbox("Strip row names on save", ref CFG.Current.Param_StripRowNamesOnSave_DS2);
                    break;

                case ProjectType.BB:
                    ImGui.Checkbox("Strip row names on save", ref CFG.Current.Param_StripRowNamesOnSave_BB);
                    break;

                case ProjectType.DS3:
                    ImGui.Checkbox("Strip row names on save", ref CFG.Current.Param_StripRowNamesOnSave_DS3);
                    break;

                case ProjectType.SDT:
                    ImGui.Checkbox("Strip row names on save", ref CFG.Current.Param_StripRowNamesOnSave_SDT);
                    break;

                case ProjectType.ER:
                    ImGui.Checkbox("Strip row names on save", ref CFG.Current.Param_StripRowNamesOnSave_ER);
                    break;

                case ProjectType.AC6:
                    ImGui.Checkbox("Strip row names on save", ref CFG.Current.Param_StripRowNamesOnSave_AC6);
                    break;
            }
            UIHelper.ShowHoverTooltip("If enabled, row names are stripped upon save, meaning no row names will be stored in the regulation.\n\nThe row names are saved in the /.smithbox/Workflow/Stripped Row Names/ folder within your project folder.\n\nIf this folder is present when a project is loaded, the row names will be restored automatically.");

            if (Smithbox.ProjectType is ProjectType.ER && ParamBank.PrimaryBank.ParamVersion >= 11210015L)
            {
                ImGui.Checkbox("Save regulation.bin as DCX.DFLT", ref CFG.Current.Param_SaveERAsDFLT);
                UIHelper.ShowHoverTooltip("If enabled, the regulation will be saved with the DCX.DFLT compression instead of the ZSTD compression that Elden Ring uses post patch 1.12.1.\n\nEnable if you want to load the regulation in an older tool that doesn't support ZSTD compression.");
            }
        }

        // Params
        if (ImGui.CollapsingHeader("Params", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (ImGui.Checkbox("Sort params alphabetically", ref CFG.Current.Param_AlphabeticalParams))
                UICache.ClearCaches();
            UIHelper.ShowHoverTooltip("Sort the Param View list alphabetically.");

            if (ImGui.Checkbox("Show community param names", ref CFG.Current.Param_ShowParamCommunityName))
                UICache.ClearCaches();
            UIHelper.ShowHoverTooltip("Show the community name for a param instead of its raw filename in the list.");

            if (ImGui.Checkbox("Display param categories", ref CFG.Current.Param_DisplayParamCategories))
                UICache.ClearCaches();
            UIHelper.ShowHoverTooltip("If defined, display params in their assigned param category groupings.");
        }

        // Rows
        if (ImGui.CollapsingHeader("Rows", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Disable line wrapping", ref CFG.Current.Param_DisableLineWrapping);
            UIHelper.ShowHoverTooltip("Disable the row names from wrapping within the Row View list.");

            ImGui.Checkbox("Disable row grouping", ref CFG.Current.Param_DisableRowGrouping);
            UIHelper.ShowHoverTooltip("Disable the grouping of connected rows in certain params, such as ItemLotParam within the Row View list.");
        }

        // Fields
        if (ImGui.CollapsingHeader("Field Layout", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Allow field reordering", ref CFG.Current.Param_AllowFieldReorder);
            UIHelper.ShowHoverTooltip("Allow the field order to be changed by an alternative order as defined within the PARAM META file.");

            ImGui.Separator();

            ImGui.Checkbox("Show community field names first", ref CFG.Current.Param_MakeMetaNamesPrimary);
            UIHelper.ShowHoverTooltip("Crowd-sourced names will appear before the canonical name in the Field View list.");

            ImGui.Checkbox("Show secondary field names", ref CFG.Current.Param_ShowSecondaryNames);
            UIHelper.ShowHoverTooltip("The crowd-sourced name (or the canonical name if the above option is enabled) will appear after the initial name in the Field View list.");

            ImGui.Checkbox("Show field data offsets", ref CFG.Current.Param_ShowFieldOffsets);
            UIHelper.ShowHoverTooltip("The field offset within the .PARAM file will be shown to the left in the Field View List.");

            ImGui.Checkbox("Show color preview", ref CFG.Current.Param_ShowColorPreview);
            UIHelper.ShowHoverTooltip("Show color preview in field column if applicable.");

            ImGui.Checkbox("Show graph visualisation", ref CFG.Current.Param_ShowGraphVisualisation);
            UIHelper.ShowHoverTooltip("Show graph visualisation in field column if applicable.");

            ImGui.Checkbox("Show view in map button", ref CFG.Current.Param_ViewInMapOption);
            UIHelper.ShowHoverTooltip("Show the view in map if applicable.");

            ImGui.Checkbox("Show view model button", ref CFG.Current.Param_ViewModelOption);
            UIHelper.ShowHoverTooltip("Show the view model if applicable.");

            ImGui.Separator();

            ImGui.Checkbox("Hide field references", ref CFG.Current.Param_HideReferenceRows);
            UIHelper.ShowHoverTooltip("Hide the generated param references for fields that link to other params.");

            ImGui.Checkbox("Hide field enums", ref CFG.Current.Param_HideEnums);
            UIHelper.ShowHoverTooltip("Hide the crowd-sourced namelist for index-based enum fields.");

            ImGui.Checkbox("Hide padding fields", ref CFG.Current.Param_HidePaddingFields);
            UIHelper.ShowHoverTooltip("Hides fields that are considered 'padding' in the property editor view.");

            ImGui.Checkbox("Hide obsolete fields", ref CFG.Current.Param_HideObsoleteFields);
            UIHelper.ShowHoverTooltip("Hides fields that are obsolete in the property editor view.");

            ImGui.Separator();

            ImGui.Checkbox("Show field param labels", ref CFG.Current.Param_ShowFieldParamLabels);
            UIHelper.ShowHoverTooltip("The field param labels will be shown below the field name.");

            ImGui.Checkbox("Show field enum labels", ref CFG.Current.Param_ShowFieldEnumLabels);
            UIHelper.ShowHoverTooltip("The field enum labels will be shown below the field name.");

            ImGui.Checkbox("Show field text labels", ref CFG.Current.Param_ShowFieldFmgLabels);
            UIHelper.ShowHoverTooltip("The field fmg reference labels will be shown below the field name.");

            ImGui.Checkbox("Show field image labels", ref CFG.Current.Param_ShowFieldTextureLabels);
            UIHelper.ShowHoverTooltip("The field texture reference labels will be shown below the field name.");
        }

        // Field Information
        if (ImGui.CollapsingHeader("Field Information", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Help Icon: Show field description", ref CFG.Current.Param_ShowFieldDescription_onIcon);
            UIHelper.ShowHoverTooltip("Display the description for the field when hovering over the help icon.");

            ImGui.Checkbox("Help Icon: Show field limits", ref CFG.Current.Param_ShowFieldLimits_onIcon);
            UIHelper.ShowHoverTooltip("Display the minimum and maximum limits for the field when hovering over the help icon.");

            ImGui.Checkbox("Name: Show field description", ref CFG.Current.Param_ShowFieldDescription_onName);
            UIHelper.ShowHoverTooltip("Display the description for the field when hovering over the name.");

            ImGui.Checkbox("Name: Show field limits", ref CFG.Current.Param_ShowFieldLimits_onName);
            UIHelper.ShowHoverTooltip("Display the minimum and maximum limits for the field when hovering over the name.");

        }

        // Values
        if (ImGui.CollapsingHeader("Values", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Show inverted percentages as traditional percentages", ref CFG.Current.Param_ShowTraditionalPercentages);
            UIHelper.ShowHoverTooltip("Displays field values that utilise the (1 - x) pattern as traditional percentages (e.g. -20 instead of 1.2).");
        }

        // Param Context Menu
        if (ImGui.CollapsingHeader("Param Context Menu", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Text("Menu Width");
            ImGui.DragFloat("##paramContextMenuWidth", ref CFG.Current.Param_ParamContextMenu_Width);
        }

        // Row Context Menu
        if (ImGui.CollapsingHeader("Row Context Menu", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Text("Menu Width");
            ImGui.DragFloat("##rowContextMenuWidth", ref CFG.Current.Param_RowContextMenu_Width);

            ImGui.Checkbox("Display row name input", ref CFG.Current.Param_RowContextMenu_NameInput);
            UIHelper.ShowHoverTooltip("Display a row name input within the right-click context menu.");

            ImGui.Checkbox("Display row shortcut tools", ref CFG.Current.Param_RowContextMenu_ShortcutTools);
            UIHelper.ShowHoverTooltip("Show the shortcut tools in the right-click row context menu.");

            ImGui.Checkbox("Display row pin options", ref CFG.Current.Param_RowContextMenu_PinOptions);
            UIHelper.ShowHoverTooltip("Show the pin options in the right-click row context menu.");

            ImGui.Checkbox("Display row compare options", ref CFG.Current.Param_RowContextMenu_CompareOptions);
            UIHelper.ShowHoverTooltip("Show the compare options in the right-click row context menu.");

            ImGui.Checkbox("Display row reverse lookup option", ref CFG.Current.Param_RowContextMenu_ReverseLoopup);
            UIHelper.ShowHoverTooltip("Show the reverse lookup option in the right-click row context menu.");
        }

        // Field Context Menu
        if (ImGui.CollapsingHeader("Field Context Menu", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Text("Menu Width");
            ImGui.DragFloat("##fieldContextMenuWidth", ref CFG.Current.Param_FieldContextMenu_Width);

            ImGui.Checkbox("Split context menu", ref CFG.Current.Param_FieldContextMenu_Split);
            UIHelper.ShowHoverTooltip("Split the field context menu into separate menus for separate right-click locations.");

            ImGui.Checkbox("Display field name", ref CFG.Current.Param_FieldContextMenu_Name);
            UIHelper.ShowHoverTooltip("Display the field name in the context menu.");

            ImGui.Checkbox("Display field description", ref CFG.Current.Param_FieldContextMenu_Description);
            UIHelper.ShowHoverTooltip("Display the field description in the context menu.");

            ImGui.Checkbox("Display field property info", ref CFG.Current.Param_FieldContextMenu_PropertyInfo);
            UIHelper.ShowHoverTooltip("Display the field property info in the context menu.");

            ImGui.Checkbox("Display field pin options", ref CFG.Current.Param_FieldContextMenu_PinOptions);
            UIHelper.ShowHoverTooltip("Display the field pin options in the context menu.");

            ImGui.Checkbox("Display field compare options", ref CFG.Current.Param_FieldContextMenu_CompareOptions);
            UIHelper.ShowHoverTooltip("Display the field compare options in the context menu.");

            ImGui.Checkbox("Display field value distribution option", ref CFG.Current.Param_FieldContextMenu_ValueDistribution);
            UIHelper.ShowHoverTooltip("Display the field value distribution option in the context menu.");

            ImGui.Checkbox("Display field add options", ref CFG.Current.Param_FieldContextMenu_AddOptions);
            UIHelper.ShowHoverTooltip("Display the field add to searchbar and mass edit options in the context menu.");

            ImGui.Checkbox("Display field references", ref CFG.Current.Param_FieldContextMenu_References);
            UIHelper.ShowHoverTooltip("Display the field references in the context menu.");

            ImGui.Checkbox("Display field reference search", ref CFG.Current.Param_FieldContextMenu_ReferenceSearch);
            UIHelper.ShowHoverTooltip("Display the field reference search in the context menu.");

            ImGui.Checkbox("Display field mass edit options", ref CFG.Current.Param_FieldContextMenu_MassEdit);
            UIHelper.ShowHoverTooltip("Display the field mass edit options in the context menu.");

            ImGui.Checkbox("Display full mass edit submenu", ref CFG.Current.Param_FieldContextMenu_FullMassEdit);
            UIHelper.ShowHoverTooltip("If enabled, the right-click context menu for fields shows a comprehensive editing popup for the massedit feature.\nIf disabled, simply shows a shortcut to the manual massedit entry element.\n(The full menu is still available from the manual popup)");
        }

        // Image Preview
        if (ImGui.CollapsingHeader("Image Preview", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Text("Image Preview Scale:");
            ImGui.DragFloat("##imagePreviewScale", ref CFG.Current.Param_FieldContextMenu_ImagePreviewScale, 0.1f, 0.1f, 10.0f);
            UIHelper.ShowHoverTooltip("Scale of the previewed image.");

            if(ImGui.Checkbox("Display image preview in field context menu", ref CFG.Current.Param_FieldContextMenu_ImagePreview_ContextMenu))
            {
                if (!TextureFolderBank.IsLoaded)
                {
                    TextureFolderBank.LoadTextureFolders();
                }
            }
            UIHelper.ShowHoverTooltip("Display image preview of any image index fields if possible within the field context menu.");

            ImGui.Checkbox("Display image preview in field column", ref CFG.Current.Param_FieldContextMenu_ImagePreview_FieldColumn);
            {
                if (!TextureFolderBank.IsLoaded)
                {
                    TextureFolderBank.LoadTextureFolders();
                }
            }
            UIHelper.ShowHoverTooltip("Display image preview of any image index fields if possible at the bottom of the field column.");
        }

        if (SelectedGameOffsetData != null)
        {
            // Ignore if no game offsets exist for the project type
            if (Smithbox.BankHandler.GameOffsets.Offsets.list != null)
            {
                if (ImGui.CollapsingHeader("Param Reloader", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Text("Param Reloader Version");
                    UIHelper.ShowHoverTooltip("This should match the executable version you wish to target, otherwise the memory offsets will be incorrect.");

                    var index = CFG.Current.SelectedGameOffsetData;
                    string[] options = Smithbox.BankHandler.GameOffsets.Offsets.list.Select(entry => entry.exeVersion).ToArray();

                    if (ImGui.Combo("##GameOffsetVersion", ref index, options, options.Length))
                    {
                        CFG.Current.SelectedGameOffsetData = index;
                    }
                }
            }
        }
        else
        {
            if (Smithbox.ProjectType != ProjectType.Undefined)
            {
                SelectedGameOffsetData = Smithbox.BankHandler.GameOffsets.Offsets;
            }
        }
    }
}
