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
        if (ImGui.BeginTabItem("User Interface"))
        {
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Show tooltips", ref CFG.Current.System_Show_UI_Tooltips);
                ImguiUtils.ShowHoverTooltip("This is a tooltip.");

                ImGui.Checkbox("Wrap alias text", ref CFG.Current.System_WrapAliasDisplay);
                ImguiUtils.ShowHoverTooltip("Makes the alias text display wrap instead of being cut off.");

                ImGui.SliderFloat("UI scale", ref _tempScale, 0.5f, 4.0f);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    // Round to 0.05
                    CFG.Current.System_UI_Scale = (float)Math.Round(_tempScale * 20) / 20;
                    Smithbox.UIScaleChanged?.Invoke(null, EventArgs.Empty);
                    Smithbox.FontRebuildRequest = true;
                    _tempScale = CFG.Current.System_UI_Scale;
                }
                ImguiUtils.ShowHoverTooltip("Adjusts the scale of the user interface throughout all of Smithbox.");

                ImGui.Checkbox($"Multiply UI scale by DPI ({(Smithbox.Dpi / 96).ToString("P0", new NumberFormatInfo { PercentPositivePattern = 1, PercentNegativePattern = 1 })})", ref CFG.Current.System_ScaleByDPI);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    Smithbox.UIScaleChanged?.Invoke(null, EventArgs.Empty);
                    Smithbox.FontRebuildRequest = true;
                }
                ImguiUtils.ShowHoverTooltip("Multiplies the user interface scale by your monitor's DPI setting.");

                ImGui.SliderFloat("Font Size", ref CFG.Current.Interface_FontSize, 8.0f, 32.0f);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.Interface_FontSize = (float)Math.Round(CFG.Current.Interface_FontSize);
                    Smithbox.FontRebuildRequest = true;
                }
                ImguiUtils.ShowHoverTooltip("Adjusts the size of the font in Smithbox.");
            }

            // Fonts
            if (ImGui.CollapsingHeader("Fonts"))
            {
                ImGui.Text("Current English Font:");
                ImGui.SameLine();
                ImGui.Text(Path.GetFileName(CFG.Current.System_English_Font));

                if (ImGui.Button("Set English font"))
                {
                    PlatformUtils.Instance.OpenFileDialog("Select Font", ["*"], out string path);
                    if (File.Exists(path))
                    {
                        CFG.Current.System_English_Font = path;
                        Smithbox.FontRebuildRequest = true;
                    }
                }
                ImguiUtils.ShowHoverTooltip("Use the following font for English characters. .ttf and .otf expected.");

                ImGui.Text("Current Non-English Font:");
                ImGui.SameLine();
                ImGui.Text(Path.GetFileName(CFG.Current.System_Other_Font));

                if (ImGui.Button("Set Non-English font"))
                {
                    PlatformUtils.Instance.OpenFileDialog("Select Font", ["*"], out string path);
                    if (File.Exists(path))
                    {
                        CFG.Current.System_Other_Font = path;
                        Smithbox.FontRebuildRequest = true;
                    }
                }
                ImguiUtils.ShowHoverTooltip("Use the following font for Non-English characters. .ttf and .otf expected.");

                if (ImGui.Button("Restore Default Fonts"))
                {
                    CFG.Current.System_English_Font = "Assets\\Fonts\\RobotoMono-Light.ttf";
                    CFG.Current.System_Other_Font = "Assets\\Fonts\\NotoSansCJKtc-Light.otf";
                    Smithbox.FontRebuildRequest = true;
                }
            }

            // Additional Language Fonts
            if (ImGui.CollapsingHeader("Additional Language Fonts"))
            {
                if (ImGui.Checkbox("Chinese", ref CFG.Current.System_Font_Chinese))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHoverTooltip("Include Chinese font.\nAdditional fonts take more VRAM and increase startup time.");

                if (ImGui.Checkbox("Korean", ref CFG.Current.System_Font_Korean))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHoverTooltip("Include Korean font.\nAdditional fonts take more VRAM and increase startup time.");

                if (ImGui.Checkbox("Thai", ref CFG.Current.System_Font_Thai))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHoverTooltip("Include Thai font.\nAdditional fonts take more VRAM and increase startup time.");

                if (ImGui.Checkbox("Vietnamese", ref CFG.Current.System_Font_Vietnamese))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHoverTooltip("Include Vietnamese font.\nAdditional fonts take more VRAM and increase startup time.");

                if (ImGui.Checkbox("Cyrillic", ref CFG.Current.System_Font_Cyrillic))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHoverTooltip("Include Cyrillic font.\nAdditional fonts take more VRAM and increase startup time.");
            }


            if (ImGui.CollapsingHeader("Theme", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Text("Current Theme");

                if (ImGui.ListBox("##themeSelect", ref CFG.Current.SelectedTheme, UI.LoadedThemeNames, UI.LoadedThemeNames.Length))
                {
                    UI.SetTheme(false);
                }

                if (ImGui.Button("Reset to Default"))
                {
                    UI.ResetInterface();
                }
                ImGui.SameLine();
                if (ImGui.Button("Open Theme Folder"))
                {
                    Process.Start("explorer.exe", $"{AppContext.BaseDirectory}\\Assets\\Themes\\");
                }
                ImGui.SameLine();

                if (ImGui.Button("Export Theme"))
                {
                    UI.ExportThemeJson();
                }
                ImGui.SameLine();
                ImGui.InputText("##themeName", ref CFG.Current.NewThemeName, 255);

                if (ImGui.CollapsingHeader("Editor Window", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.ColorEdit4("Main Background##ImGui_MainBg", ref CFG.Current.ImGui_MainBg);
                    ImGui.ColorEdit4("Child Background##ImGui_ChildBg", ref CFG.Current.ImGui_ChildBg);
                    ImGui.ColorEdit4("Popup Background##ImGui_PopupBg", ref CFG.Current.ImGui_PopupBg);
                    ImGui.ColorEdit4("Border##ImGui_Border", ref CFG.Current.ImGui_Border);
                    ImGui.ColorEdit4("Title Bar Background##ImGui_TitleBarBg", ref CFG.Current.ImGui_TitleBarBg);
                    ImGui.ColorEdit4("Title Bar Background (Active)##ImGui_TitleBarBg_Active", ref CFG.Current.ImGui_TitleBarBg_Active);
                    ImGui.ColorEdit4("Menu Bar Background##ImGui_MenuBarBg", ref CFG.Current.ImGui_MenuBarBg);
                }

                if (ImGui.CollapsingHeader("Moveable Window", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.ColorEdit4("Main Background##Imgui_Moveable_MainBg", ref CFG.Current.Imgui_Moveable_MainBg);
                    ImGui.ColorEdit4("Child Background##Imgui_Moveable_ChildBg", ref CFG.Current.Imgui_Moveable_ChildBg);
                    ImGui.ColorEdit4("Header##Imgui_Moveable_Header", ref CFG.Current.Imgui_Moveable_Header);
                    ImGui.ColorEdit4("Title Bar Background##Imgui_Moveable_TitleBg", ref CFG.Current.Imgui_Moveable_TitleBg);
                    ImGui.ColorEdit4("Title Bar Background (Active)##Imgui_Moveable_TitleBg_Active", ref CFG.Current.Imgui_Moveable_TitleBg_Active);
                }

                if (ImGui.CollapsingHeader("Scrollbar", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.ColorEdit4("Scrollbar Background", ref CFG.Current.ImGui_ScrollbarBg);
                    ImGui.ColorEdit4("Scrollbar Grab", ref CFG.Current.ImGui_ScrollbarGrab);
                    ImGui.ColorEdit4("Scrollbar Grab (Hover)", ref CFG.Current.ImGui_ScrollbarGrab_Hover);
                    ImGui.ColorEdit4("Scrollbar Grab (Active)", ref CFG.Current.ImGui_ScrollbarGrab_Active);
                    ImGui.ColorEdit4("Slider Grab", ref CFG.Current.ImGui_SliderGrab);
                    ImGui.ColorEdit4("Slider Grab (Active)", ref CFG.Current.ImGui_SliderGrab_Active);
                }

                if (ImGui.CollapsingHeader("Tab", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.ColorEdit4("Tab", ref CFG.Current.ImGui_Tab);
                    ImGui.ColorEdit4("Tab (Hover)", ref CFG.Current.ImGui_Tab_Hover);
                    ImGui.ColorEdit4("Tab (Active)", ref CFG.Current.ImGui_Tab_Active);
                    ImGui.ColorEdit4("Unfocused Tab", ref CFG.Current.ImGui_UnfocusedTab);
                    ImGui.ColorEdit4("Unfocused Tab (Active)", ref CFG.Current.ImGui_UnfocusedTab_Active);
                }

                if (ImGui.CollapsingHeader("Button", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.ColorEdit4("Button", ref CFG.Current.ImGui_Button);
                    ImGui.ColorEdit4("Button (Hover)", ref CFG.Current.ImGui_Button_Hovered);
                    ImGui.ColorEdit4("Button (Active)", ref CFG.Current.ImGui_ButtonActive);
                }

                if (ImGui.CollapsingHeader("Selection", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.ColorEdit4("Selection", ref CFG.Current.ImGui_Selection);
                    ImGui.ColorEdit4("Selection (Hover)", ref CFG.Current.ImGui_Selection_Hover);
                    ImGui.ColorEdit4("Selection (Active)", ref CFG.Current.ImGui_Selection_Active);
                }

                if (ImGui.CollapsingHeader("Inputs", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.ColorEdit4("Input Background", ref CFG.Current.ImGui_Input_Background);
                    ImGui.ColorEdit4("Input Background (Hover)", ref CFG.Current.ImGui_Input_Background_Hover);
                    ImGui.ColorEdit4("Input Background (Active)", ref CFG.Current.ImGui_Input_Background_Active);
                    ImGui.ColorEdit4("Input Checkmark", ref CFG.Current.ImGui_Input_CheckMark);
                    ImGui.ColorEdit4("Input Conflict Background", ref CFG.Current.ImGui_Input_Conflict_Background);
                    ImGui.ColorEdit4("Input Vanilla Background", ref CFG.Current.ImGui_Input_Vanilla_Background);
                    ImGui.ColorEdit4("Input Default Background", ref CFG.Current.ImGui_Input_Default_Background);
                    ImGui.ColorEdit4("Input Auxillary Vanilla Background", ref CFG.Current.ImGui_Input_AuxVanilla_Background);
                    ImGui.ColorEdit4("Input Difference Comparison Background", ref CFG.Current.ImGui_Input_DiffCompare_Background);
                }

                if (ImGui.CollapsingHeader("Text", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.ColorEdit4("Default Text", ref CFG.Current.ImGui_Default_Text_Color);
                    ImGui.ColorEdit4("Warning Text", ref CFG.Current.ImGui_Warning_Text_Color);
                    ImGui.ColorEdit4("Beneficial Text", ref CFG.Current.ImGui_Benefit_Text_Color);
                    ImGui.ColorEdit4("Invalid Text", ref CFG.Current.ImGui_Invalid_Text_Color);
                    ImGui.ColorEdit4("Param Reference Text", ref CFG.Current.ImGui_ParamRef_Text);
                    ImGui.ColorEdit4("Param Reference Missing Text", ref CFG.Current.ImGui_ParamRefMissing_Text);
                    ImGui.ColorEdit4("Param Reference Inactive Text", ref CFG.Current.ImGui_ParamRefInactive_Text);
                    ImGui.ColorEdit4("Enum Name Text", ref CFG.Current.ImGui_EnumName_Text);
                    ImGui.ColorEdit4("Enum Value Text", ref CFG.Current.ImGui_EnumValue_Text);
                    ImGui.ColorEdit4("FMG Link Text", ref CFG.Current.ImGui_FmgLink_Text);
                    ImGui.ColorEdit4("FMG Reference Text", ref CFG.Current.ImGui_FmgRef_Text);
                    ImGui.ColorEdit4("FMG Reference Inactive Text", ref CFG.Current.ImGui_FmgRefInactive_Text);
                    ImGui.ColorEdit4("Is Reference Text", ref CFG.Current.ImGui_IsRef_Text);
                    ImGui.ColorEdit4("Virtual Reference Text", ref CFG.Current.ImGui_VirtualRef_Text);
                    ImGui.ColorEdit4("Reference Text", ref CFG.Current.ImGui_Ref_Text);
                    ImGui.ColorEdit4("Auxiliary Conflict Text", ref CFG.Current.ImGui_AuxConflict_Text);
                    ImGui.ColorEdit4("Auxiliary Added Text", ref CFG.Current.ImGui_AuxAdded_Text);
                    ImGui.ColorEdit4("Primary Changed Text", ref CFG.Current.ImGui_PrimaryChanged_Text);
                    ImGui.ColorEdit4("Param Row Text", ref CFG.Current.ImGui_ParamRow_Text);
                    ImGui.ColorEdit4("Aliased Name Text", ref CFG.Current.ImGui_AliasName_Text);
                }

                if (ImGui.CollapsingHeader("Miscellaneous", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.ColorEdit4("Display Group: Border Highlight", ref CFG.Current.DisplayGroupEditor_Border_Highlight);
                    ImGui.ColorEdit4("Display Group: Active Input Background", ref CFG.Current.DisplayGroupEditor_DisplayActive_Frame);
                    ImGui.ColorEdit4("Display Group: Active Checkbox", ref CFG.Current.DisplayGroupEditor_DisplayActive_Checkbox);
                    ImGui.ColorEdit4("Draw Group: Active Input Background", ref CFG.Current.DisplayGroupEditor_DrawActive_Frame);
                    ImGui.ColorEdit4("Draw Group: Active Checkbox", ref CFG.Current.DisplayGroupEditor_DrawActive_Checkbox);
                    ImGui.ColorEdit4("Combined Group: Active Input Background", ref CFG.Current.DisplayGroupEditor_CombinedActive_Frame);
                    ImGui.ColorEdit4("Combined Group: Active Checkbox", ref CFG.Current.DisplayGroupEditor_CombinedActive_Checkbox);
                }
            }

            ImGui.EndTabItem();
        }
    }
    }
