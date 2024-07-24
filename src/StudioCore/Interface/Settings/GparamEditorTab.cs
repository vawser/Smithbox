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
        if (ImGui.BeginTabItem("G参数编辑器 Gparam Editor"))
        {
            if (ImGui.CollapsingHeader("总览 General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("在文件列表中显示别名 Display aliases in file list", ref CFG.Current.Interface_Display_Alias_for_Gparam);
                ImguiUtils.ShowHoverTooltip("切换文件列表中别名的显示\nToggle the display of the aliases in the file list.");
            }

            if (ImGui.CollapsingHeader("组 Groups", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("显示缺失组的添加按钮 Show add button for missing groups", ref CFG.Current.Gparam_DisplayAddGroups);
                ImguiUtils.ShowHoverTooltip("为缺失的组显示添加按钮\nShow the Add button for groups that are not present.");

                ImGui.Checkbox("显示空组 Show empty groups", ref CFG.Current.Gparam_DisplayEmptyGroups);
                ImguiUtils.ShowHoverTooltip("在组列表中显示空组\nDisplay empty groups in the group list.");
            }

            if (ImGui.CollapsingHeader("块 Fields", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("显示缺失字段的添加按钮 Show add button for missing fields", ref CFG.Current.Gparam_DisplayAddFields);
                ImguiUtils.ShowHoverTooltip("为缺失的字段显示添加按钮\nShow the Add button for fields that are not present.");
            }

            if (ImGui.CollapsingHeader("值 Values", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("显示四维属性的颜色编辑 Show color edit for 4 digit properties", ref CFG.Current.Gparam_DisplayColorEditForVector4Fields);
                ImguiUtils.ShowHoverTooltip("为四维属性显示颜色编辑工具\nShow the color edit tool for 4 digit properties.");
            }

            if (ImGui.CollapsingHeader("颜色编辑 Color Edit", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.Checkbox("显示为RGB Show color as Integer RGB", ref CFG.Current.Gparam_ColorEdit_RGB))
                {
                    CFG.Current.Gparam_ColorEdit_Decimal = false;
                    CFG.Current.Gparam_ColorEdit_HSV = false;
                }
                ImguiUtils.ShowHoverTooltip("显示0-255 Show the color data as Integer RGB color (0 to 255)");

                if (ImGui.Checkbox("显示为RPG数 Show color as Decimal RGB", ref CFG.Current.Gparam_ColorEdit_Decimal))
                {
                    CFG.Current.Gparam_ColorEdit_RGB = false;
                    CFG.Current.Gparam_ColorEdit_HSV = false;
                }
                ImguiUtils.ShowHoverTooltip("显示小数0-1 Show the color data as Decimal RGB color (0.0 to 1.0)");

                if (ImGui.Checkbox("显示为HSV浮点 Show color as HSV", ref CFG.Current.Gparam_ColorEdit_HSV))
                {
                    CFG.Current.Gparam_ColorEdit_RGB = false;
                    CFG.Current.Gparam_ColorEdit_Decimal = false;
                }
                ImguiUtils.ShowHoverTooltip("显示0-1 Show the color data as Hue, Saturation, Value color (0.0 to 1.0)");
            }

            if (ImGui.CollapsingHeader("快速编辑 Quick Edit", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.InputText("筛选 Filter: ID", ref CFG.Current.Gparam_QuickEdit_ID, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'ID' filter argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("筛选 Filter: Time of Day 时间", ref CFG.Current.Gparam_QuickEdit_TimeOfDay, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Time of Day' filter argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("筛选 Filter: Value 值", ref CFG.Current.Gparam_QuickEdit_Value, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Value' filter argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("指令 Command: Set 设置", ref CFG.Current.Gparam_QuickEdit_Set, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Set' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("指令 Command: Addition 补充", ref CFG.Current.Gparam_QuickEdit_Add, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Addition' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("指令 Command: Subtract 删减", ref CFG.Current.Gparam_QuickEdit_Subtract, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Subtract' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("指令 Command: Multiply 相乘", ref CFG.Current.Gparam_QuickEdit_Multiply, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Multiply' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("指令 Command: Set by Row 置为行", ref CFG.Current.Gparam_QuickEdit_SetByRow, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Set By Row' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("分割符 Delimiter", ref CFG.Current.Gparam_QuickEdit_Chain, 255);
                ImguiUtils.ShowHoverTooltip("The text string to split filter and commands.");

                if (ImGui.Button("重置为默认 Reset to Default"))
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
