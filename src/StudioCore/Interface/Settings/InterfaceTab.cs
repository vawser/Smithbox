using ImGuiNET;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface.Settings;

public class InterfaceTab
{
    private float _tempScale;

    public InterfaceTab()
    {
        _tempScale = CFG.Current.System_UI_Scale;
    }

    public void Display()
    {
        if (ImGui.BeginTabItem("用户界面 User Interface"))
        {
            if (ImGui.CollapsingHeader("总览 General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("显示工具栏 Show tooltips", ref CFG.Current.System_Show_UI_Tooltips);
                ImguiUtils.ShowHoverTooltip("This is a tooltip.");

                ImGui.Checkbox("格式化文本 Wrap alias text", ref CFG.Current.System_WrapAliasDisplay);
                ImguiUtils.ShowHoverTooltip("不会因截断而看不清文本 会推至下一行 Makes the alias text display wrap instead of being cut off.");

                ImGui.SliderFloat("缩放比例 UI scale", ref _tempScale, 0.5f, 4.0f);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    // Round to 0.05
                    CFG.Current.System_UI_Scale = (float)Math.Round(_tempScale * 20) / 20;
                    _tempScale = CFG.Current.System_UI_Scale;
                    Smithbox.UIScaleChanged?.Invoke(null, EventArgs.Empty);
                }
                ImguiUtils.ShowHoverTooltip("调整本工具的用户缩放比例 Adjusts the scale of the user interface throughout all of Smithbox.");

                ImGui.SameLine();
                if (ImGui.Button("重置 Reset"))
                {
                    CFG.Current.System_UI_Scale = CFG.Default.System_UI_Scale;
                    _tempScale = CFG.Current.System_UI_Scale;
                    Smithbox.UIScaleChanged?.Invoke(null, EventArgs.Empty);
                }

                ImGui.Checkbox($"将UI比例乘以DPI Multiply UI scale by DPI ({(Smithbox.Dpi / 96).ToString("P0", new NumberFormatInfo { PercentPositivePattern = 1, PercentNegativePattern = 1 })})", ref CFG.Current.System_ScaleByDPI);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    Smithbox.UIScaleChanged?.Invoke(null, EventArgs.Empty);
                }
                ImguiUtils.ShowHoverTooltip("Multiplies the user interface scale by your monitor's DPI setting.");

                ImGui.SliderFloat("字体大小 Font size", ref CFG.Current.Interface_FontSize, 8.0f, 32.0f);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.Interface_FontSize = (float)Math.Round(CFG.Current.Interface_FontSize);
                    Smithbox.UIScaleChanged?.Invoke(null, EventArgs.Empty);
                }
                ImguiUtils.ShowHoverTooltip("Adjusts the size of the font in Smithbox.");
            }

            // Fonts
            if (ImGui.CollapsingHeader("字体 Fonts"))
            {
                ImGui.Text("当前英文字体 Current English Font:");
                ImGui.SameLine();
                ImGui.Text(Path.GetFileName(CFG.Current.System_English_Font));

                if (ImGui.Button("设置英文字体 Set English font"))
                {
                    PlatformUtils.Instance.OpenFileDialog("选择字体 Select Font", ["*"], out string path);
                    if (File.Exists(path))
                    {
                        CFG.Current.System_English_Font = path;
                        Smithbox.FontRebuildRequest = true;
                    }
                }
                ImguiUtils.ShowHoverTooltip("Use the following font for English characters. .ttf and .otf expected.");

                ImGui.Text("当前非英字体 Current Non-English Font:");
                ImGui.SameLine();
                ImGui.Text(Path.GetFileName(CFG.Current.System_Other_Font));

                if (ImGui.Button("设置非英字体 Set Non-English font"))
                {
                    PlatformUtils.Instance.OpenFileDialog("选择字体 Select Font", ["*"], out string path);
                    if (File.Exists(path))
                    {
                        CFG.Current.System_Other_Font = path;
                        Smithbox.FontRebuildRequest = true;
                    }
                }
                ImguiUtils.ShowHoverTooltip("设置非英文字体 Use the following font for Non-English characters. .ttf and .otf expected.");

                if (ImGui.Button("还原默认字体 Restore Default Fonts"))
                {
                    CFG.Current.System_English_Font = "Assets\\Fonts\\RobotoMono-Light.ttf";
                    CFG.Current.System_Other_Font = "Assets\\Fonts\\NotoSansCJKtc-Light.otf";
                    Smithbox.FontRebuildRequest = true;
                }
            }

            // Additional Language Fonts
            if (ImGui.CollapsingHeader("添加语言 Additional Language Fonts"))
            {
                if (ImGui.Checkbox("中文 Chinese", ref CFG.Current.System_Font_Chinese))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHoverTooltip("Include Chinese font.\nAdditional fonts take more VRAM and increase startup time.");

                ImGui.SameLine();
                if (ImGui.Checkbox("韩语 Korean", ref CFG.Current.System_Font_Korean))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHoverTooltip("Include Korean font.\nAdditional fonts take more VRAM and increase startup time.");

                ImGui.SameLine();
                if (ImGui.Checkbox("泰语 Thai", ref CFG.Current.System_Font_Thai))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHoverTooltip("Include Thai font.\nAdditional fonts take more VRAM and increase startup time.");

                ImGui.SameLine();
                if (ImGui.Checkbox("越语 Vietnamese", ref CFG.Current.System_Font_Vietnamese))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHoverTooltip("Include Vietnamese font.\nAdditional fonts take more VRAM and increase startup time.");

                ImGui.SameLine();
                if (ImGui.Checkbox("希腊 Cyrillic", ref CFG.Current.System_Font_Cyrillic))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHoverTooltip("Include Cyrillic font.\nAdditional fonts take more VRAM and increase startup time.");
            }


            if (ImGui.CollapsingHeader("配色 Theme", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Text("当前主题 Current Theme");

                if (ImGui.ListBox("##themeSelect", ref CFG.Current.SelectedTheme, UI.LoadedThemeNames, UI.LoadedThemeNames.Length))
                {
                    UI.SetTheme(false);
                }

                if (ImGui.Button("重置 Reset to Default"))
                {
                    UI.ResetInterface();
                }
                ImGui.SameLine();
                if (ImGui.Button("打开主题文件夹 Open Theme Folder"))
                {
                    Process.Start("explorer.exe", $"{AppContext.BaseDirectory}\\Assets\\Themes\\");
                }
                ImGui.SameLine();

                if (ImGui.Button("导出主题 Export Theme"))
                {
                    UI.ExportThemeJson();
                }
                ImGui.SameLine();
                ImGui.InputText("##themeName", ref CFG.Current.NewThemeName, 255);

                if (ImGui.CollapsingHeader("编辑器窗口 Editor Window", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.ColorEdit4("主背景 Main Background##ImGui_MainBg", ref CFG.Current.ImGui_MainBg);
                    ImGui.ColorEdit4("子背景 Child Background##ImGui_ChildBg", ref CFG.Current.ImGui_ChildBg);
                    ImGui.ColorEdit4("弹出背景 Popup Background##ImGui_PopupBg", ref CFG.Current.ImGui_PopupBg);
                    ImGui.ColorEdit4("边框 Border##ImGui_Border", ref CFG.Current.ImGui_Border);
                    ImGui.ColorEdit4("标题栏背景 Title Bar Background##ImGui_TitleBarBg", ref CFG.Current.ImGui_TitleBarBg);
                    ImGui.ColorEdit4("标题栏背景 (活动) Title Bar Background (Active)##ImGui_TitleBarBg_Active", ref CFG.Current.ImGui_TitleBarBg_Active);
                    ImGui.ColorEdit4("菜单栏背景 Menu Bar Background##ImGui_MenuBarBg", ref CFG.Current.ImGui_MenuBarBg);
                }

                if (ImGui.CollapsingHeader("可移动窗口 Moveable Window", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.ColorEdit4("主背景 Main Background##ImGui_MainBg", ref CFG.Current.ImGui_MainBg);
                    ImGui.ColorEdit4("子背景 Child Background##ImGui_ChildBg", ref CFG.Current.ImGui_ChildBg);
                    ImGui.ColorEdit4("弹出背景 Popup Background##ImGui_PopupBg", ref CFG.Current.ImGui_PopupBg);
                    ImGui.ColorEdit4("边框 Border##ImGui_Border", ref CFG.Current.ImGui_Border);
                    ImGui.ColorEdit4("标题栏背景 Title Bar Background##ImGui_TitleBarBg", ref CFG.Current.ImGui_TitleBarBg);
                    ImGui.ColorEdit4("标题栏背景 (活动) Title Bar Background (Active)##ImGui_TitleBarBg_Active", ref CFG.Current.ImGui_TitleBarBg_Active);
                    ImGui.ColorEdit4("菜单栏背景 Menu Bar Background##ImGui_MenuBarBg", ref CFG.Current.ImGui_MenuBarBg);
                }

                if (ImGui.CollapsingHeader("滚动条 Scrollbar", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.ColorEdit4("滚动条背景 Scrollbar Background", ref CFG.Current.ImGui_ScrollbarBg);
                    ImGui.ColorEdit4("滚动条抓手 Scrollbar Grab", ref CFG.Current.ImGui_ScrollbarGrab);
                    ImGui.ColorEdit4("滚动条抓手 (悬停) Scrollbar Grab (Hover)", ref CFG.Current.ImGui_ScrollbarGrab_Hover);
                    ImGui.ColorEdit4("滚动条抓手 (活动) Scrollbar Grab (Active)", ref CFG.Current.ImGui_ScrollbarGrab_Active);
                    ImGui.ColorEdit4("滑块抓手 Slider Grab", ref CFG.Current.ImGui_SliderGrab);
                    ImGui.ColorEdit4("滑块抓手 (活动) Slider Grab (Active)", ref CFG.Current.ImGui_SliderGrab_Active);
                }

                if (ImGui.CollapsingHeader("标签页 Tab", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.ColorEdit4("Tab", ref CFG.Current.ImGui_Tab);
                    ImGui.ColorEdit4("Tab (Hover 悬停)", ref CFG.Current.ImGui_Tab_Hover);
                    ImGui.ColorEdit4("Tab (Active 活动)", ref CFG.Current.ImGui_Tab_Active);
                    ImGui.ColorEdit4("未聚焦标签页 Unfocused Tab", ref CFG.Current.ImGui_UnfocusedTab);
                    ImGui.ColorEdit4("未聚焦标签页 Unfocused Tab (Active 活动)", ref CFG.Current.ImGui_UnfocusedTab_Active);
                }

                if (ImGui.CollapsingHeader("按钮 Button", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.ColorEdit4("Button", ref CFG.Current.ImGui_Button);
                    ImGui.ColorEdit4("Button (Hover 悬停)", ref CFG.Current.ImGui_Button_Hovered);
                    ImGui.ColorEdit4("Button (Active 活动)", ref CFG.Current.ImGui_ButtonActive);
                }

                if (ImGui.CollapsingHeader("选框 Selection", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.ColorEdit4("Selection", ref CFG.Current.ImGui_Selection);
                    ImGui.ColorEdit4("Selection (Hover 覆盖)", ref CFG.Current.ImGui_Selection_Hover);
                    ImGui.ColorEdit4("Selection (Active 活跃)", ref CFG.Current.ImGui_Selection_Active);
                }

                if (ImGui.CollapsingHeader("输入 Inputs", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.ColorEdit4("输入背景 Input Background", ref CFG.Current.ImGui_Input_Background);
                    ImGui.ColorEdit4("输入背景 (悬停) Input Background (Hover)", ref CFG.Current.ImGui_Input_Background_Hover);
                    ImGui.ColorEdit4("输入背景 (活动) Input Background (Active)", ref CFG.Current.ImGui_Input_Background_Active);
                    ImGui.ColorEdit4("输入复选标记 Input Checkmark", ref CFG.Current.ImGui_Input_CheckMark);
                    ImGui.ColorEdit4("输入冲突背景 Input Conflict Background", ref CFG.Current.ImGui_Input_Conflict_Background);
                    ImGui.ColorEdit4("输入默认背景 Input Vanilla Background", ref CFG.Current.ImGui_Input_Vanilla_Background);
                    ImGui.ColorEdit4("输入默认背景 Input Default Background", ref CFG.Current.ImGui_Input_Default_Background);
                    ImGui.ColorEdit4("输入辅助默认背景 Input Auxiliary Vanilla Background", ref CFG.Current.ImGui_Input_AuxVanilla_Background);
                    ImGui.ColorEdit4("输入差异比较背景 Input Difference Comparison Background", ref CFG.Current.ImGui_Input_DiffCompare_Background);
                }

                if (ImGui.CollapsingHeader("文本 Text", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.ColorEdit4("默认 Default Text", ref CFG.Current.ImGui_Default_Text_Color);
                    ImGui.ColorEdit4("警告 Warning Text", ref CFG.Current.ImGui_Warning_Text_Color);
                    ImGui.ColorEdit4("有利 Beneficial Text", ref CFG.Current.ImGui_Benefit_Text_Color);
                    ImGui.ColorEdit4("无效 Invalid Text", ref CFG.Current.ImGui_Invalid_Text_Color);
                    ImGui.ColorEdit4("参数引用 Param Reference Text", ref CFG.Current.ImGui_ParamRef_Text);
                    ImGui.ColorEdit4("缺失参数引用 Param Reference Missing Text", ref CFG.Current.ImGui_ParamRefMissing_Text);
                    ImGui.ColorEdit4("不活跃参数引用 Param Reference Inactive Text", ref CFG.Current.ImGui_ParamRefInactive_Text);
                    ImGui.ColorEdit4("枚举名 Enum Name Text", ref CFG.Current.ImGui_EnumName_Text);
                    ImGui.ColorEdit4("枚举值 Enum Value Text", ref CFG.Current.ImGui_EnumValue_Text);
                    ImGui.ColorEdit4("FMG 链接 FMG Link Text", ref CFG.Current.ImGui_FmgLink_Text);
                    ImGui.ColorEdit4("FMG 引用 FMG Reference Text", ref CFG.Current.ImGui_FmgRef_Text);
                    ImGui.ColorEdit4("不活跃 FMG 引用 FMG Reference Inactive Text", ref CFG.Current.ImGui_FmgRefInactive_Text);
                    ImGui.ColorEdit4("是引用 Is Reference Text", ref CFG.Current.ImGui_IsRef_Text);
                    ImGui.ColorEdit4("虚拟引用 Virtual Reference Text", ref CFG.Current.ImGui_VirtualRef_Text);
                    ImGui.ColorEdit4("引用 Reference Text", ref CFG.Current.ImGui_Ref_Text);
                    ImGui.ColorEdit4("辅助冲突 Auxiliary Conflict Text", ref CFG.Current.ImGui_AuxConflict_Text);
                    ImGui.ColorEdit4("辅助添加 Auxiliary Added Text", ref CFG.Current.ImGui_AuxAdded_Text);
                    ImGui.ColorEdit4("主要更改 Primary Changed Text", ref CFG.Current.ImGui_PrimaryChanged_Text);
                    ImGui.ColorEdit4("参数行 Param Row Text", ref CFG.Current.ImGui_ParamRow_Text);
                    ImGui.ColorEdit4("别名名称 Aliased Name Text", ref CFG.Current.ImGui_AliasName_Text);
                }

                if (ImGui.CollapsingHeader("其他 Miscellaneous", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.ColorEdit4("显示组: 边框高亮 Display Group: Border Highlight", ref CFG.Current.DisplayGroupEditor_Border_Highlight);
                    ImGui.ColorEdit4("显示组: 活动输入背景 Display Group: Active Input Background", ref CFG.Current.DisplayGroupEditor_DisplayActive_Frame);
                    ImGui.ColorEdit4("显示组: 活动复选框 Display Group: Active Checkbox", ref CFG.Current.DisplayGroupEditor_DisplayActive_Checkbox);
                    ImGui.ColorEdit4("绘制组: 活动输入背景 Draw Group: Active Input Background", ref CFG.Current.DisplayGroupEditor_DrawActive_Frame);
                    ImGui.ColorEdit4("绘制组: 活动复选框 Draw Group: Active Checkbox", ref CFG.Current.DisplayGroupEditor_DrawActive_Checkbox);
                    ImGui.ColorEdit4("组合组: 活动输入背景 Combined Group: Active Input Background", ref CFG.Current.DisplayGroupEditor_CombinedActive_Frame);
                    ImGui.ColorEdit4("组合组: 活动复选框 Combined Group: Active Checkbox", ref CFG.Current.DisplayGroupEditor_CombinedActive_Checkbox);
                }
            }

            ImGui.EndTabItem();
        }
    }
    }
