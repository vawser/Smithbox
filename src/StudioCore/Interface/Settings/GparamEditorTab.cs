using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface.Settings;

public class GparamEditorTab
{
    public GparamEditorTab() { }

    public void Display()
    {
        if (ImGui.BeginTabItem("Gparam Editor"))
        {
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Display aliases in file list", ref CFG.Current.Interface_Display_Alias_for_Gparam);
                ImguiUtils.ShowHoverTooltip("Toggle the display of the aliases in the file list.");
            }

            if (ImGui.CollapsingHeader("Groups", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Show add button for missing groups", ref CFG.Current.Gparam_DisplayAddGroups);
                ImguiUtils.ShowHoverTooltip("Show the Add button for groups that are not present.");

                ImGui.Checkbox("Show empty groups", ref CFG.Current.Gparam_DisplayEmptyGroups);
                ImguiUtils.ShowHoverTooltip("Display empty groups in the group list.");
            }

            if (ImGui.CollapsingHeader("Fields", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Show add button for missing fields", ref CFG.Current.Gparam_DisplayAddFields);
                ImguiUtils.ShowHoverTooltip("Show the Add button for fields that are not present.");
            }

            if (ImGui.CollapsingHeader("Values", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Show color edit for 4 digit properties", ref CFG.Current.Gparam_DisplayColorEditForVector4Fields);
                ImguiUtils.ShowHoverTooltip("Show the color edit tool for 4 digit properties.");
            }

            if (ImGui.CollapsingHeader("Color Edit", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.Checkbox("Show color as Integer RGB", ref CFG.Current.Gparam_ColorEdit_RGB))
                {
                    CFG.Current.Gparam_ColorEdit_Decimal = false;
                    CFG.Current.Gparam_ColorEdit_HSV = false;
                }
                ImguiUtils.ShowHoverTooltip("Show the color data as Integer RGB color (0 to 255)");

                if (ImGui.Checkbox("Show color as Decimal RGB", ref CFG.Current.Gparam_ColorEdit_Decimal))
                {
                    CFG.Current.Gparam_ColorEdit_RGB = false;
                    CFG.Current.Gparam_ColorEdit_HSV = false;
                }
                ImguiUtils.ShowHoverTooltip("Show the color data as Decimal RGB color (0.0 to 1.0)");

                if (ImGui.Checkbox("Show color as HSV", ref CFG.Current.Gparam_ColorEdit_HSV))
                {
                    CFG.Current.Gparam_ColorEdit_RGB = false;
                    CFG.Current.Gparam_ColorEdit_Decimal = false;
                }
                ImguiUtils.ShowHoverTooltip("Show the color data as Hue, Saturation, Value color (0.0 to 1.0)");
            }

            if (ImGui.CollapsingHeader("Quick Edit", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.InputText("Filter: ID", ref CFG.Current.Gparam_QuickEdit_ID, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'ID' filter argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("Filter: Time of Day", ref CFG.Current.Gparam_QuickEdit_TimeOfDay, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Time of Day' filter argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("Filter: Value", ref CFG.Current.Gparam_QuickEdit_Value, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Value' filter argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("Command: Set", ref CFG.Current.Gparam_QuickEdit_Set, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Set' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("Command: Addition", ref CFG.Current.Gparam_QuickEdit_Add, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Addition' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("Command: Subtract", ref CFG.Current.Gparam_QuickEdit_Subtract, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Subtract' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("Command: Multiply", ref CFG.Current.Gparam_QuickEdit_Multiply, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Multiply' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("Command: Set by Row", ref CFG.Current.Gparam_QuickEdit_SetByRow, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Set By Row' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("Delimiter", ref CFG.Current.Gparam_QuickEdit_Chain, 255);
                ImguiUtils.ShowHoverTooltip("The text string to split filter and commands.");

                if (ImGui.Button("Reset to Default"))
                {
                    CFG.Current.Gparam_QuickEdit_Chain = "+";

                    CFG.Current.Gparam_QuickEdit_ID = "id";
                    CFG.Current.Gparam_QuickEdit_TimeOfDay = "tod";
                    CFG.Current.Gparam_QuickEdit_Value = "value";

                    CFG.Current.Gparam_QuickEdit_Set = "set";
                    CFG.Current.Gparam_QuickEdit_Add = "add";
                    CFG.Current.Gparam_QuickEdit_Subtract = "sub";
                    CFG.Current.Gparam_QuickEdit_Multiply = "mult";
                    CFG.Current.Gparam_QuickEdit_SetByRow = "setbyrow";
                }
            }

            ImGui.EndTabItem();
        }
    }

}
