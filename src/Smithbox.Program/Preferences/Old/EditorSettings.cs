using Hexa.NET.ImGui;
using Octokit;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.TextEditor;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Application;

//------------------------------------------
// Text Editor
//------------------------------------------
#region Text Editor
public class TextEditorTab
{
    public TextEditorTab() { }

    public void Display()
    {
        // Data
        if (ImGui.CollapsingHeader("Data", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Include Vanilla Cache", ref CFG.Current.TextEditor_Include_Vanilla_Cache);
            UIHelper.Tooltip("If enabled, the vanilla cache is loaded, which enables the modified and unique difference features.");

            ImGui.Checkbox("Enable Obsolete Container Loading", ref CFG.Current.TextEditor_Container_List_Display_Obsolete_Containers);
            UIHelper.Tooltip("If enabled, obsolete containers will be loaded. Otherwise, they are ignored.");
        }

        // Primary Category
        if (ImGui.CollapsingHeader("Primary Category", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (Smithbox.Orchestrator.SelectedProject != null)
            {
                var curProject = Smithbox.Orchestrator.SelectedProject;

                if (ImGui.BeginCombo("Primary Category##primaryCategoryCombo", CFG.Current.TextEditor_Primary_Category.GetDisplayName()))
                {
                    foreach (var entry in Enum.GetValues(typeof(TextContainerCategory)))
                    {
                        var type = (TextContainerCategory)entry;

                        if (TextUtils.IsSupportedLanguage(curProject, (TextContainerCategory)entry))
                        {
                            if (ImGui.Selectable(type.GetDisplayName()))
                            {
                                CFG.Current.TextEditor_Primary_Category = (TextContainerCategory)entry;

                                // Refresh the param editor FMG decorators when the category changes.
                                if (curProject.Handler.ParamEditor != null)
                                {
                                    curProject.Handler.ParamEditor.DecoratorHandler.ClearFmgDecorators();
                                }
                            }
                        }
                    }
                    ImGui.EndCombo();
                }
                UIHelper.Tooltip("Change the primary category, this determines which text files are used for FMG references and other stuff.");
            }

            ImGui.Checkbox("Hide non-primary categories in list", ref CFG.Current.TextEditor_Container_List_Display_Primary_Category_Only);
            UIHelper.Tooltip("Hide the non-primary categories in the File List.");

        }

        // File List
        if (ImGui.CollapsingHeader("File List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Simple File List", ref CFG.Current.TextEditor_Container_List_Hide_Unused_Containers);
            UIHelper.Tooltip("Display the file list in a simple form: this means unused containers are hidden.");

            ImGui.Checkbox("Display Community File Name", ref CFG.Current.TextEditor_Container_List_Display_Community_Names);
            UIHelper.Tooltip("If enabled, the names in the File List will be given a community name.");

            ImGui.Checkbox("Display Source Path", ref CFG.Current.TextEditor_Container_List_Display_Source_Path);
            UIHelper.Tooltip("If enabled, the path of the source file will be displayed in the hover tooltip.");
        }

        // Text File List
        if (ImGui.CollapsingHeader("Text File List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Simple Text File List", ref CFG.Current.TextEditor_Text_File_List_Grouped_Display);
            UIHelper.Tooltip("Display the text file list in a simple form: this means non-title or standalone files are hidden.");

            ImGui.Checkbox("Display FMG ID", ref CFG.Current.TextEditor_Text_File_List_Display_ID);
            UIHelper.Tooltip("Display the FMG ID in the Text File List by the name.");

            ImGui.Checkbox("Display Community FMG Name", ref CFG.Current.TextEditor_Text_File_List_Display_Community_Names);
            UIHelper.Tooltip("Display the FMG community name instead of the internal form.");
        }

        // Text Entries List
        if (ImGui.CollapsingHeader("Text Entries List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display Empty Text Placeholder", ref CFG.Current.TextEditor_Text_Entry_List_Display_Null_Text);
            UIHelper.Tooltip("Display placeholder text for rows that have no text.");

            ImGui.Checkbox("Trucate Displayed Text", ref CFG.Current.TextEditor_Text_Entry_List_Truncate_Name);
            UIHelper.Tooltip("Truncate the displayed text so it is always one line (does not affect the contents of the entry).");

            ImGui.Checkbox("Ignore ID on Duplication", ref CFG.Current.TextEditor_Text_Entry_List_Ignore_ID_Check);
            UIHelper.Tooltip("Keep the Entry ID the same on duplication. Useful if you want to manually edit the IDs afterwards.");
        }

        // Entry Properties
        if (ImGui.CollapsingHeader("Text Entry Properties", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display Grouped Entries", ref CFG.Current.TextEditor_Text_Entry_Enable_Grouped_Entries);
            UIHelper.Tooltip("Include related entries in the Contents window, e.g. Title, Summary, Description, Effect entries that share the same ID.");

            ImGui.Checkbox("Allow Duplicate IDs", ref CFG.Current.TextEditor_Text_Entry_Allow_Duplicate_ID);
            UIHelper.Tooltip("Allow Entry ID input to apply change even if the ID is a duplicate of an existing entry row.");
        }

        // Text Export
        if (ImGui.CollapsingHeader("Text Export", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Include Grouped Entries", ref CFG.Current.TextEditor_Text_Export_Include_Grouped_Entries);
            UIHelper.Tooltip("When exporting Text Entries, if they are associated with a group, include the associated entries as well whilst exporting.");

            ImGui.Checkbox("Use Quick Export", ref CFG.Current.TextEditor_Text_Export_Enable_Quick_Export);
            UIHelper.Tooltip("Automatically name the export file instead of display the Export Text prompt. Will overwrite the existing quick export file each time.");
        }

        // Language Sync
        if (ImGui.CollapsingHeader("Language Sync", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display Primary Category only", ref CFG.Current.TextEditor_Language_Sync_Display_Primary_Only);
            UIHelper.Tooltip("Only show your primary category (language) in the selection dropdown.");

            ImGui.Checkbox("Apply Prefix", ref CFG.Current.TextEditor_Language_Sync_Apply_Prefix);
            UIHelper.Tooltip("Add a prefix to synced text in the target language container for all new entries.");

            ImGui.InputText("##prefixText", ref CFG.Current.TextEditor_Language_Sync_Prefix, 255);
            UIHelper.Tooltip("The prefix to apply.");
        }

        // Text Entry Copy
        if (ImGui.CollapsingHeader("Clipboard Action", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Include ID", ref CFG.Current.TextEditor_Text_Clipboard_Include_ID);
            UIHelper.Tooltip("Include the row ID when copying a Text Entry to the clipboard.");

            ImGui.Checkbox("Escape New Lines", ref CFG.Current.TextEditor_Text_Clipboard_Escape_New_Lines);
            UIHelper.Tooltip("Escape the new lines characters when copying a Text Entry to the clipboard.");
        }
    }
}

#endregion

//------------------------------------------
// Param Editor
//------------------------------------------
#region Param Editor
public class ParamEditorTab
{
    public ParamEditorTab() { }

    public void Display()
    {
        if (Smithbox.Orchestrator.SelectedProject != null)
        {
            var curProject = Smithbox.Orchestrator.SelectedProject;

            // General
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Use loose params", ref CFG.Current.UseLooseParams);
                UIHelper.Tooltip("If true, then loose params will be loaded over the packed versions.");

                ImGui.Checkbox("Use compact param editor", ref CFG.Current.UI_CompactParams);
                UIHelper.Tooltip("Reduces the line height within the the Param Editor screen.");

                ImGui.Checkbox("Show advanced options in massedit popup", ref CFG.Current.Param_AdvancedMassedit);
                UIHelper.Tooltip("Show additional options for advanced users within the massedit popup.");

                ImGui.Checkbox("Pinned params stay visible", ref CFG.Current.Param_PinnedParamsStayVisible);
                UIHelper.Tooltip("Pinned params will stay visible when you scroll instead of only being pinned to the top of the list.");

                ImGui.Checkbox("Pinned rows stay visible", ref CFG.Current.Param_PinnedRowsStayVisible);
                UIHelper.Tooltip("Pinned rows will stay visible when you scroll instead of only being pinned to the top of the list.");

                ImGui.Checkbox("Pinned fields stay visible", ref CFG.Current.Param_PinnedFieldsStayVisible);
                UIHelper.Tooltip("Pinned fields will stay visible when you scroll instead of only being pinned to the top of the list.");
            }

            if (ImGui.CollapsingHeader("Metadata", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Text("Configure whether the current project draws the param editor metadata from project files rather than base files.");

                if (ImGui.Button("Create Project Metadata##createProjectMetaData", DPI.StandardButtonSize))
                {
                    var dialog = PlatformUtils.Instance.MessageBox("This will overwrite any existing project-specific metadata. Are you sure?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (dialog is DialogResult.Yes)
                    {
                        curProject.Handler.ParamData.CreateProjectMetadata();
                    }
                }

                if (ImGui.Checkbox("Use project metadata", ref CFG.Current.Param_UseProjectMeta))
                {
                    curProject.Handler.ParamData.ParamMeta.Clear();
                    curProject.Handler.ParamData.ReloadMeta();
                }
                UIHelper.Tooltip("Use project-specific metadata instead of Smithbox's base versions.");
            }

            if (ImGui.CollapsingHeader("Regulation Data", ImGuiTreeNodeFlags.DefaultOpen))
            {
                switch (curProject.Descriptor.ProjectType)
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

                    case ProjectType.NR:
                        ImGui.Checkbox("Strip row names on save", ref CFG.Current.Param_StripRowNamesOnSave_NR);
                        break;
                }
                UIHelper.Tooltip("If enabled, row names are stripped upon save, meaning no row names will be stored in the regulation.\n\nThe row names are saved in the /.smithbox/Workflow/Stripped Row Names/ folder within your project folder.");


                switch (curProject.Descriptor.ProjectType)
                {
                    case ProjectType.DES:
                        ImGui.Checkbox("Restore stripped row names on load", ref CFG.Current.Param_RestoreStrippedRowNamesOnLoad_DES);
                        break;

                    case ProjectType.DS1:
                    case ProjectType.DS1R:
                        ImGui.Checkbox("Restore stripped row names on load", ref CFG.Current.Param_RestoreStrippedRowNamesOnLoad_DS1);
                        break;

                    case ProjectType.DS2:
                    case ProjectType.DS2S:
                        ImGui.Checkbox("Restore stripped row names on load", ref CFG.Current.Param_RestoreStrippedRowNamesOnLoad_DS2);
                        break;

                    case ProjectType.BB:
                        ImGui.Checkbox("Restore stripped row names on load", ref CFG.Current.Param_RestoreStrippedRowNamesOnLoad_BB);
                        break;

                    case ProjectType.DS3:
                        ImGui.Checkbox("Restore stripped row names on load", ref CFG.Current.Param_RestoreStrippedRowNamesOnLoad_DS3);
                        break;

                    case ProjectType.SDT:
                        ImGui.Checkbox("Restore stripped row names on load", ref CFG.Current.Param_RestoreStrippedRowNamesOnLoad_SDT);
                        break;

                    case ProjectType.ER:
                        ImGui.Checkbox("Restore stripped row names on load", ref CFG.Current.Param_RestoreStrippedRowNamesOnLoad_ER);
                        break;

                    case ProjectType.AC6:
                        ImGui.Checkbox("Restore stripped row names on load", ref CFG.Current.Param_RestoreStrippedRowNamesOnLoad_AC6);
                        break;

                    case ProjectType.NR:
                        ImGui.Checkbox("Restore stripped row names on load", ref CFG.Current.Param_RestoreStrippedRowNamesOnLoad_NR);
                        break;
                }
                UIHelper.Tooltip("If enabled, stripped row names that have been stored will be applied to the row names during param loading.\n\nThe row names are saved in the /.smithbox/Workflow/Stripped Row Names/ folder within your project folder.");

                if (curProject.Descriptor.ProjectType is ProjectType.ER && curProject.Handler.ParamData.PrimaryBank.ParamVersion >= 11210015L)
                {
                    ImGui.Checkbox("Save regulation.bin as DCX.DFLT", ref CFG.Current.Param_SaveERAsDFLT);
                    UIHelper.Tooltip("If enabled, the regulation will be saved with the DCX.DFLT compression instead of the ZSTD compression that Elden Ring uses post patch 1.12.1.\n\nEnable if you want to load the regulation in an older tool that doesn't support ZSTD compression.");
                }
            }

            // Params
            if (ImGui.CollapsingHeader("Params", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.Checkbox("Sort params alphabetically", ref CFG.Current.Param_AlphabeticalParams))
                    UICache.ClearCaches();
                UIHelper.Tooltip("Sort the Param View list alphabetically.");

                if (ImGui.Checkbox("Show community param names", ref CFG.Current.Param_ShowParamCommunityName))
                    UICache.ClearCaches();
                UIHelper.Tooltip("Show the community name for a param instead of its raw filename in the list.");

                if (ImGui.Checkbox("Display param categories", ref CFG.Current.Param_DisplayParamCategories))
                    UICache.ClearCaches();
                UIHelper.Tooltip("If defined, display params in their assigned param category groupings.");
            }

            // Rows
            if (ImGui.CollapsingHeader("Rows", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Disable line wrapping", ref CFG.Current.Param_DisableLineWrapping);
                UIHelper.Tooltip("Disable the row names from wrapping within the Row View list.");

                ImGui.Checkbox("Disable row grouping", ref CFG.Current.Param_DisableRowGrouping);
                UIHelper.Tooltip("Disable the grouping of connected rows in certain params, such as ItemLotParam within the Row View list.");
            }

            // Fields
            if (ImGui.CollapsingHeader("Field Layout", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Allow field reordering", ref CFG.Current.Param_AllowFieldReorder);
                UIHelper.Tooltip("Allow the field order to be changed by an alternative order as defined within the PARAM META file.");

                ImGui.Separator();

                ImGui.Checkbox("Show community field names first", ref CFG.Current.Param_MakeMetaNamesPrimary);
                UIHelper.Tooltip("Crowd-sourced names will appear before the canonical name in the Field View list.");

                ImGui.Checkbox("Show secondary field names", ref CFG.Current.Param_ShowSecondaryNames);
                UIHelper.Tooltip("The crowd-sourced name (or the canonical name if the above option is enabled) will appear after the initial name in the Field View list.");

                ImGui.Checkbox("Show field data offsets", ref CFG.Current.Param_ShowFieldOffsets);
                UIHelper.Tooltip("The field offset within the .PARAM file will be shown to the left in the Field View List.");

                ImGui.Checkbox("Show color preview", ref CFG.Current.Param_ShowColorPreview);
                UIHelper.Tooltip("Show color preview in field column if applicable.");

                ImGui.Checkbox("Show graph visualisation", ref CFG.Current.Param_ShowGraphVisualisation);
                UIHelper.Tooltip("Show graph visualisation in field column if applicable.");

                ImGui.Checkbox("Show view in map button", ref CFG.Current.Param_ViewInMapOption);
                UIHelper.Tooltip("Show the view in map if applicable.");

                ImGui.Checkbox("Show view model button", ref CFG.Current.Param_ViewModelOption);
                UIHelper.Tooltip("Show the view model if applicable.");

                ImGui.Separator();

                ImGui.Checkbox("Hide field references", ref CFG.Current.Param_HideReferenceRows);
                UIHelper.Tooltip("Hide the generated param references for fields that link to other params.");

                ImGui.Checkbox("Hide field enums", ref CFG.Current.Param_HideEnums);
                UIHelper.Tooltip("Hide the crowd-sourced namelist for index-based enum fields.");

                ImGui.Checkbox("Hide padding fields", ref CFG.Current.Param_HidePaddingFields);
                UIHelper.Tooltip("Hides fields that are considered 'padding' in the property editor view.");

                ImGui.Checkbox("Hide obsolete fields", ref CFG.Current.Param_HideObsoleteFields);
                UIHelper.Tooltip("Hides fields that are obsolete in the property editor view.");

                ImGui.Separator();

                ImGui.Checkbox("Show field param labels", ref CFG.Current.Param_ShowFieldParamLabels);
                UIHelper.Tooltip("The field param labels will be shown below the field name.");

                ImGui.Checkbox("Show field enum labels", ref CFG.Current.Param_ShowFieldEnumLabels);
                UIHelper.Tooltip("The field enum labels will be shown below the field name.");

                ImGui.Checkbox("Show field text labels", ref CFG.Current.Param_ShowFieldFmgLabels);
                UIHelper.Tooltip("The field fmg reference labels will be shown below the field name.");

                ImGui.Checkbox("Show field icon labels", ref CFG.Current.Param_ShowFieldTextureLabels);
                UIHelper.Tooltip("The field texture reference labels will be shown below the field name.");
            }

            // Field Information
            if (ImGui.CollapsingHeader("Field Information", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Help Icon: Show field description", ref CFG.Current.Param_ShowFieldDescription_onIcon);
                UIHelper.Tooltip("Display the description for the field when hovering over the help icon.");

                ImGui.Checkbox("Help Icon: Show field limits", ref CFG.Current.Param_ShowFieldLimits_onIcon);
                UIHelper.Tooltip("Display the minimum and maximum limits for the field when hovering over the help icon.");

                ImGui.Checkbox("Name: Show field description", ref CFG.Current.Param_ShowFieldDescription_onName);
                UIHelper.Tooltip("Display the description for the field when hovering over the name.");

                ImGui.Checkbox("Name: Show field limits", ref CFG.Current.Param_ShowFieldLimits_onName);
                UIHelper.Tooltip("Display the minimum and maximum limits for the field when hovering over the name.");

            }

            // Values
            if (ImGui.CollapsingHeader("Values", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Show inverted percentages as traditional percentages", ref CFG.Current.Param_ShowTraditionalPercentages);
                UIHelper.Tooltip("Displays field values that utilise the (1 - x) pattern as traditional percentages (e.g. -20 instead of 1.2).");
            }

            // Param Context Menu
            if (ImGui.CollapsingHeader("Param Context Menu", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.DragFloat("Context Menu Width##paramContextMenuWidth", ref CFG.Current.Param_ParamContextMenu_Width);
            }

            // Table Group Context Menu
            if (ImGui.CollapsingHeader("Table Group Context Menu", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.DragFloat("Context Menu Width##tableGroupContextMenuWidth", ref CFG.Current.Param_TableGroupContextMenu_Width);
            }

            // Row Context Menu
            if (ImGui.CollapsingHeader("Row Context Menu", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Display row name input", ref CFG.Current.Param_RowContextMenu_NameInput);
                UIHelper.Tooltip("Display a row name input within the right-click context menu.");

                ImGui.Checkbox("Display row shortcut tools", ref CFG.Current.Param_RowContextMenu_ShortcutTools);
                UIHelper.Tooltip("Show the shortcut tools in the right-click row context menu.");

                ImGui.Checkbox("Display row pin options", ref CFG.Current.Param_RowContextMenu_PinOptions);
                UIHelper.Tooltip("Show the pin options in the right-click row context menu.");

                ImGui.Checkbox("Display row compare options", ref CFG.Current.Param_RowContextMenu_CompareOptions);
                UIHelper.Tooltip("Show the compare options in the right-click row context menu.");

                ImGui.Checkbox("Display row reverse lookup option", ref CFG.Current.Param_RowContextMenu_ReverseLoopup);
                UIHelper.Tooltip("Show the reverse lookup option in the right-click row context menu.");

                ImGui.Checkbox("Display proliferate name option", ref CFG.Current.Param_RowContextMenu_ProliferateName);
                UIHelper.Tooltip("Show the proliferate name option in the right-click row context menu.");

                ImGui.Checkbox("Display inherit name option", ref CFG.Current.Param_RowContextMenu_InheritName);
                UIHelper.Tooltip("Show the inherit name option in the right-click row context menu.");

                ImGui.Checkbox("Display row name adjustment options", ref CFG.Current.Param_RowContextMenu_RowNameAdjustments);
                UIHelper.Tooltip("Show the row name adjustment options in the right-click row context menu.");

                ImGui.DragFloat("Context Menu Width##rowContextMenuWidth", ref CFG.Current.Param_RowContextMenu_Width);
            }

            // Field Context Menu
            if (ImGui.CollapsingHeader("Field Context Menu", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Split context menu", ref CFG.Current.Param_FieldContextMenu_Split);
                UIHelper.Tooltip("Split the field context menu into separate menus for separate right-click locations.");

                ImGui.Checkbox("Display field name", ref CFG.Current.Param_FieldContextMenu_Name);
                UIHelper.Tooltip("Display the field name in the context menu.");

                ImGui.Checkbox("Display field description", ref CFG.Current.Param_FieldContextMenu_Description);
                UIHelper.Tooltip("Display the field description in the context menu.");

                ImGui.Checkbox("Display field property info", ref CFG.Current.Param_FieldContextMenu_PropertyInfo);
                UIHelper.Tooltip("Display the field property info in the context menu.");

                ImGui.Checkbox("Display field pin options", ref CFG.Current.Param_FieldContextMenu_PinOptions);
                UIHelper.Tooltip("Display the field pin options in the context menu.");

                ImGui.Checkbox("Display field compare options", ref CFG.Current.Param_FieldContextMenu_CompareOptions);
                UIHelper.Tooltip("Display the field compare options in the context menu.");

                ImGui.Checkbox("Display field value distribution option", ref CFG.Current.Param_FieldContextMenu_ValueDistribution);
                UIHelper.Tooltip("Display the field value distribution option in the context menu.");

                ImGui.Checkbox("Display field add options", ref CFG.Current.Param_FieldContextMenu_AddOptions);
                UIHelper.Tooltip("Display the field add to searchbar and mass edit options in the context menu.");

                ImGui.Checkbox("Display field references", ref CFG.Current.Param_FieldContextMenu_References);
                UIHelper.Tooltip("Display the field references in the context menu.");

                ImGui.Checkbox("Display field reference search", ref CFG.Current.Param_FieldContextMenu_ReferenceSearch);
                UIHelper.Tooltip("Display the field reference search in the context menu.");

                ImGui.Checkbox("Display field mass edit options", ref CFG.Current.Param_FieldContextMenu_MassEdit);
                UIHelper.Tooltip("Display the field mass edit options in the context menu.");

                ImGui.Checkbox("Display full mass edit submenu", ref CFG.Current.Param_FieldContextMenu_FullMassEdit);
                UIHelper.Tooltip("If enabled, the right-click context menu for fields shows a comprehensive editing popup for the massedit feature.\nIf disabled, simply shows a shortcut to the manual massedit entry element.\n(The full menu is still available from the manual popup)");

                ImGui.DragFloat("Context Menu Width##fieldContextMenuWidth", ref CFG.Current.Param_FieldContextMenu_Width);
                UIHelper.Tooltip("Controls the width of the field context menu when enum or aliases lists are present.");

                ImGui.DragFloat("List Height Multiplier##fieldContextListHeightMultiplier", ref CFG.Current.Param_FieldContextMenu_ListHeightMultiplier);
                UIHelper.Tooltip("Controls the height of the field context menu when enum or aliases lists are present.");
            }

            // Icon Preview
            if (ImGui.CollapsingHeader("Icon Preview", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Display icon preview", ref CFG.Current.Param_FieldContextMenu_ImagePreview_FieldColumn);

                ImGui.Text("Icon Preview Scale:");
                ImGui.DragFloat("##imagePreviewScale", ref CFG.Current.Param_FieldContextMenu_ImagePreviewScale, 0.1f, 0.1f, 10.0f);
                UIHelper.Tooltip("Scale of the previewed image.");
            }

            // Ignore if no game offsets exist for the project type
            if (curProject.Handler.ParamData.ParamMemoryOffsets != null && curProject.Handler.ParamData.ParamMemoryOffsets.list != null)
            {
                // Auto-set to the latest version
                if (ImGui.CollapsingHeader("Param Reloader", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Checkbox("Set latest version on program start", ref CFG.Current.UseLatestGameOffset);
                    UIHelper.Tooltip("If enabled, the param reloader version will be set to the latest executable version whenever Smithbox is started.");

                    ImGui.Text("Param Reloader Version");
                    UIHelper.Tooltip("This should match the executable version you wish to target, otherwise the memory offsets will be incorrect.");

                    var index = CFG.Current.SelectedGameOffsetData;
                    string[] options = curProject.Handler.ParamData.ParamMemoryOffsets.list.Select(entry => entry.exeVersion).ToArray();

                    if (ImGui.Combo("##GameOffsetVersion", ref index, options, options.Length))
                    {
                        CFG.Current.SelectedGameOffsetData = index;
                    }
                }
            }
        }
    }
}

#endregion

//------------------------------------------
// Graphics Param Editor
//------------------------------------------
#region Graphics Param Editor
public class GparamEditorTab
{

    public GparamEditorTab() { }

    public void Display()
    {
        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display aliases in file list", ref CFG.Current.Interface_Display_Alias_for_Gparam);
            UIHelper.Tooltip("Toggle the display of the aliases in the file list.");
        }

        if (ImGui.CollapsingHeader("Groups", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display param group aliases", ref CFG.Current.Gparam_DisplayParamGroupAlias);
            UIHelper.Tooltip("Display the aliased name for param groups, instead of the internal key.");

            ImGui.Checkbox("Show add button for missing groups", ref CFG.Current.Gparam_DisplayAddGroups);
            UIHelper.Tooltip("Show the Add button for groups that are not present.");

            ImGui.Checkbox("Show empty groups", ref CFG.Current.Gparam_DisplayEmptyGroups);
            UIHelper.Tooltip("Display empty groups in the group list.");
        }

        if (ImGui.CollapsingHeader("Fields", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display param field aliases", ref CFG.Current.Gparam_DisplayParamFieldAlias);
            UIHelper.Tooltip("Display the aliased name for param fields, instead of the internal key.");

            ImGui.Checkbox("Show add button for missing fields", ref CFG.Current.Gparam_DisplayAddFields);
            UIHelper.Tooltip("Show the Add button for fields that are not present.");
        }

        if (ImGui.CollapsingHeader("Values", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Show color edit for 4 digit properties", ref CFG.Current.Gparam_DisplayColorEditForVector4Fields);
            UIHelper.Tooltip("Show the color edit tool for 4 digit properties.");
        }

        if (ImGui.CollapsingHeader("Color Edit", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (ImGui.Checkbox("Show color as Integer RGB", ref CFG.Current.Gparam_ColorEdit_RGB))
            {
                CFG.Current.Gparam_ColorEdit_Decimal = false;
                CFG.Current.Gparam_ColorEdit_HSV = false;
            }
            UIHelper.Tooltip("Show the color data as Integer RGB color (0 to 255)");

            if (ImGui.Checkbox("Show color as Decimal RGB", ref CFG.Current.Gparam_ColorEdit_Decimal))
            {
                CFG.Current.Gparam_ColorEdit_RGB = false;
                CFG.Current.Gparam_ColorEdit_HSV = false;
            }
            UIHelper.Tooltip("Show the color data as Decimal RGB color (0.0 to 1.0)");

            if (ImGui.Checkbox("Show color as HSV", ref CFG.Current.Gparam_ColorEdit_HSV))
            {
                CFG.Current.Gparam_ColorEdit_RGB = false;
                CFG.Current.Gparam_ColorEdit_Decimal = false;
            }
            UIHelper.Tooltip("Show the color data as Hue, Saturation, Value color (0.0 to 1.0)");
        }

        if (ImGui.CollapsingHeader("Quick Edit - General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.InputText("Delimiter", ref CFG.Current.Gparam_QuickEdit_Chain, 255);
            UIHelper.Tooltip("The text string to split filter and commands.");

            if (ImGui.Button("Reset to Default", DPI.StandardButtonSize))
            {
                CFG.Current.Gparam_QuickEdit_Chain = "+";
            }
        }

        if (ImGui.CollapsingHeader("Quick Edit - File Filter", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.InputText("File Filter: Match File", ref CFG.Current.Gparam_QuickEdit_File, 255);
            UIHelper.Tooltip("The text string to detect for the 'File' filter argument.");

            if (ImGui.Button("Reset to Default", DPI.StandardButtonSize))
            {
                CFG.Current.Gparam_QuickEdit_File = "file";
            }
        }

        if (ImGui.CollapsingHeader("Quick Edit - Group Filter", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.InputText("Group Filter: Match Group", ref CFG.Current.Gparam_QuickEdit_Group, 255);
            UIHelper.Tooltip("The text string to detect for the 'Group' filter argument.");

            if (ImGui.Button("Reset to Default", DPI.StandardButtonSize))
            {
                CFG.Current.Gparam_QuickEdit_Group = "group";
            }
        }

        if (ImGui.CollapsingHeader("Quick Edit - Field Filter", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.InputText("Field Filter: Match Field", ref CFG.Current.Gparam_QuickEdit_Field, 255);
            UIHelper.Tooltip("The text string to detect for the 'Field' filter argument.");

            if (ImGui.Button("Reset to Default", DPI.StandardButtonSize))
            {
                CFG.Current.Gparam_QuickEdit_Field = "field";
            }
        }

        if (ImGui.CollapsingHeader("Quick Edit - Value Filters", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.InputText("Value Filter: Match ID", ref CFG.Current.Gparam_QuickEdit_ID, 255);
            UIHelper.Tooltip("The text string to detect for the 'ID' filter argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            ImGui.InputText("Value Filter: Match Time of Day", ref CFG.Current.Gparam_QuickEdit_TimeOfDay, 255);
            UIHelper.Tooltip("The text string to detect for the 'Time of Day' filter argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            ImGui.InputText("Value Filter: Match Value", ref CFG.Current.Gparam_QuickEdit_Value, 255);
            UIHelper.Tooltip("The text string to detect for the 'Value' filter argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            ImGui.InputText("Value Filter: Match Index", ref CFG.Current.Gparam_QuickEdit_Index, 255);
            UIHelper.Tooltip("The text string to detect for the 'Index' filter argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            if (ImGui.Button("Reset to Default", DPI.StandardButtonSize))
            {
                CFG.Current.Gparam_QuickEdit_ID = "id";
                CFG.Current.Gparam_QuickEdit_TimeOfDay = "tod";
                CFG.Current.Gparam_QuickEdit_Value = "value";
                CFG.Current.Gparam_QuickEdit_Index = "index";
            }
        }

        if (ImGui.CollapsingHeader("Quick Edit - Value Commands", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.InputText("Value Command: Set", ref CFG.Current.Gparam_QuickEdit_Set, 255);
            UIHelper.Tooltip("The text string to detect for the 'Set' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            ImGui.InputText("Value Command: Addition", ref CFG.Current.Gparam_QuickEdit_Add, 255);
            UIHelper.Tooltip("The text string to detect for the 'Addition' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            ImGui.InputText("Value Command: Subtract", ref CFG.Current.Gparam_QuickEdit_Subtract, 255);
            UIHelper.Tooltip("The text string to detect for the 'Subtract' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            ImGui.InputText("Value Command: Multiply", ref CFG.Current.Gparam_QuickEdit_Multiply, 255);
            UIHelper.Tooltip("The text string to detect for the 'Multiply' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            ImGui.InputText("Value Command: Set by Row", ref CFG.Current.Gparam_QuickEdit_SetByRow, 255);
            UIHelper.Tooltip("The text string to detect for the 'Set By Row' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            ImGui.InputText("Value Command: Restore", ref CFG.Current.Gparam_QuickEdit_Restore, 255);
            UIHelper.Tooltip("The text string to detect for the 'Restore' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            ImGui.InputText("Value Command: Random", ref CFG.Current.Gparam_QuickEdit_Random, 255);
            UIHelper.Tooltip("The text string to detect for the 'Random' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

            if (ImGui.Button("Reset to Default", DPI.StandardButtonSize))
            {
                CFG.Current.Gparam_QuickEdit_Set = "set";
                CFG.Current.Gparam_QuickEdit_Add = "add";
                CFG.Current.Gparam_QuickEdit_Subtract = "sub";
                CFG.Current.Gparam_QuickEdit_Multiply = "mult";
                CFG.Current.Gparam_QuickEdit_SetByRow = "setbyrow";
                CFG.Current.Gparam_QuickEdit_Restore = "restore";
                CFG.Current.Gparam_QuickEdit_Random = "random";
            }
        }
    }

}
#endregion


//------------------------------------------
// Texture Viewer
//------------------------------------------
#region Texture Viewer
public class TextureViewerTab
{
    public TextureViewerTab() { }

    public void Display()
    {
        if (ImGui.CollapsingHeader("File List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Show character names", ref CFG.Current.TextureViewer_FileList_ShowAliasName_Characters);
            UIHelper.Tooltip("Show matching character aliases within the file list.");

            ImGui.Checkbox("Show asset names", ref CFG.Current.TextureViewer_FileList_ShowAliasName_Assets);
            UIHelper.Tooltip("Show matching asset/object aliases within the file list.");

            ImGui.Checkbox("Show part names", ref CFG.Current.TextureViewer_FileList_ShowAliasName_Parts);
            UIHelper.Tooltip("Show matching part aliases within the file list.");

            ImGui.Checkbox("Show low detail entries", ref CFG.Current.TextureViewer_FileList_ShowLowDetail_Entries);
            UIHelper.Tooltip("Show the low-detail texture containers.");
        }

        if (ImGui.CollapsingHeader("Texture List", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Show particle names", ref CFG.Current.TextureViewer_TextureList_ShowAliasName_Particles);
            UIHelper.Tooltip("Show matching particle aliases within the texture list.");
        }
    }
}

#endregion


