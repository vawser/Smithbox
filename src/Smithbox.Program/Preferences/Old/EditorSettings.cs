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

            // Fields
            if (ImGui.CollapsingHeader("Field Layout", ImGuiTreeNodeFlags.DefaultOpen))
            {
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
                ImGui.Checkbox("Display icon preview", ref CFG.Current.Param_FieldContextMenu_ImagePreview_FieldColumn);

                ImGui.Text("Icon Preview Scale:");
                ImGui.DragFloat("##imagePreviewScale", ref CFG.Current.Param_FieldContextMenu_ImagePreviewScale, 0.1f, 0.1f, 10.0f);
                UIHelper.Tooltip("Scale of the previewed image.");

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


