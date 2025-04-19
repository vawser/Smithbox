using Hexa.NET.ImGui;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Configuration.Settings;

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

            if (ImGui.Button("Reset to Default"))
            {
                CFG.Current.Gparam_QuickEdit_Chain = "+";
            }
        }

        if (ImGui.CollapsingHeader("Quick Edit - File Filter", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.InputText("File Filter: Match File", ref CFG.Current.Gparam_QuickEdit_File, 255);
            UIHelper.Tooltip("The text string to detect for the 'File' filter argument.");

            if (ImGui.Button("Reset to Default"))
            {
                CFG.Current.Gparam_QuickEdit_File = "file";
            }
        }

        if (ImGui.CollapsingHeader("Quick Edit - Group Filter", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.InputText("Group Filter: Match Group", ref CFG.Current.Gparam_QuickEdit_Group, 255);
            UIHelper.Tooltip("The text string to detect for the 'Group' filter argument.");

            if (ImGui.Button("Reset to Default"))
            {
                CFG.Current.Gparam_QuickEdit_Group = "group";
            }
        }

        if (ImGui.CollapsingHeader("Quick Edit - Field Filter", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.InputText("Field Filter: Match Field", ref CFG.Current.Gparam_QuickEdit_Field, 255);
            UIHelper.Tooltip("The text string to detect for the 'Field' filter argument.");

            if (ImGui.Button("Reset to Default"))
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

            if (ImGui.Button("Reset to Default"))
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

            if (ImGui.Button("Reset to Default"))
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
