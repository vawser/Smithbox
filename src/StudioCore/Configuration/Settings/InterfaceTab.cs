using Hexa.NET.ImGui;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Configuration.Settings;

public class InterfaceTab
{
    public Smithbox BaseEditor;
    private float _tempScale;

    public InterfaceTab(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;
        _tempScale = UI.Current.System_UI_Scale;
    }

    public void Display()
    {
        var buttonSize = new Vector2(200, 24);

        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Show tooltips", ref UI.Current.System_Show_UI_Tooltips);
            UIHelper.Tooltip("This is a tooltip.");

            ImGui.Checkbox("Wrap alias text", ref UI.Current.System_WrapAliasDisplay);
            UIHelper.Tooltip("Makes the alias text display wrap instead of being cut off.");

            ImGui.AlignTextToFramePadding();
            ImGui.SliderFloat("UI scale", ref _tempScale, 0.5f, 4.0f);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                // Round to 0.05
                UI.Current.System_UI_Scale = (float)Math.Round(_tempScale * 20) / 20;
                _tempScale = UI.Current.System_UI_Scale;
                DPI.UIScaleChanged?.Invoke(null, EventArgs.Empty);
            }
            UIHelper.Tooltip("Adjusts the scale of the user interface throughout all of Smithbox.");

            ImGui.SameLine();

            ImGui.AlignTextToFramePadding();
            if (ImGui.Button("Reset", buttonSize))
            {
                UI.Current.System_UI_Scale = UI.Default.System_UI_Scale;
                _tempScale = UI.Current.System_UI_Scale;
                DPI.UIScaleChanged?.Invoke(null, EventArgs.Empty);
                Smithbox.FontRebuildRequest = true;
            }

            ImGui.Checkbox($"Multiply UI scale by DPI ({(DPI.Dpi / 96).ToString("P0", new NumberFormatInfo { PercentPositivePattern = 1, PercentNegativePattern = 1 })})", ref UI.Current.System_ScaleByDPI);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                DPI.UIScaleChanged?.Invoke(null, EventArgs.Empty);
            }
            UIHelper.Tooltip("Multiplies the user interface scale by your monitor's DPI setting.");
        }

        // Fonts
        if (ImGui.CollapsingHeader("Fonts", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.SliderFloat("Font size", ref UI.Current.Interface_FontSize, 8.0f, 32.0f);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                UI.Current.Interface_FontSize = (float)Math.Round(UI.Current.Interface_FontSize);
                DPI.UIScaleChanged?.Invoke(null, EventArgs.Empty);
            }
            UIHelper.Tooltip("Adjusts the size of the font in Smithbox.");

            ImGui.Text("Current English Font:");
            ImGui.SameLine();
            ImGui.Text(Path.GetFileName(UI.Current.System_English_Font));

            if (ImGui.Button("Set English font", buttonSize))
            {
                PlatformUtils.Instance.OpenFileDialog("Select Font", ["*"], out string path);
                if (File.Exists(path))
                {
                    UI.Current.System_English_Font = path;
                    Smithbox.FontRebuildRequest = true;
                }
            }
            UIHelper.Tooltip("Use the following font for English characters. .ttf and .otf expected.");

            ImGui.Text("Current Non-English Font:");
            ImGui.SameLine();
            ImGui.Text(Path.GetFileName(UI.Current.System_Other_Font));

            if (ImGui.Button("Set Non-English font", buttonSize))
            {
                PlatformUtils.Instance.OpenFileDialog("Select Font", ["*"], out string path);
                if (File.Exists(path))
                {
                    UI.Current.System_Other_Font = path;
                    Smithbox.FontRebuildRequest = true;
                }
            }
            UIHelper.Tooltip("Use the following font for Non-English characters. .ttf and .otf expected.");

            if (ImGui.Button("Restore Default Fonts", buttonSize))
            {
                UI.Current.System_English_Font = "Assets\\Fonts\\RobotoMono-Light.ttf";
                UI.Current.System_Other_Font = "Assets\\Fonts\\NotoSansCJKtc-Light.otf";
                Smithbox.FontRebuildRequest = true;
            }
        }

        // Additional Language Fonts
        if (ImGui.CollapsingHeader("Additional Language Fonts", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (ImGui.Checkbox("Chinese", ref UI.Current.System_Font_Chinese))
                Smithbox.FontRebuildRequest = true;
            UIHelper.Tooltip("Include Chinese font.\nAdditional fonts take more VRAM and increase startup time.");

            if (ImGui.Checkbox("Korean", ref UI.Current.System_Font_Korean))
                Smithbox.FontRebuildRequest = true;
            UIHelper.Tooltip("Include Korean font.\nAdditional fonts take more VRAM and increase startup time.");

            if (ImGui.Checkbox("Thai", ref UI.Current.System_Font_Thai))
                Smithbox.FontRebuildRequest = true;
            UIHelper.Tooltip("Include Thai font.\nAdditional fonts take more VRAM and increase startup time.");

            if (ImGui.Checkbox("Vietnamese", ref UI.Current.System_Font_Vietnamese))
                Smithbox.FontRebuildRequest = true;
            UIHelper.Tooltip("Include Vietnamese font.\nAdditional fonts take more VRAM and increase startup time.");

            if (ImGui.Checkbox("Cyrillic", ref UI.Current.System_Font_Cyrillic))
                Smithbox.FontRebuildRequest = true;
            UIHelper.Tooltip("Include Cyrillic font.\nAdditional fonts take more VRAM and increase startup time.");
        }

        // ImGui
        if (ImGui.CollapsingHeader("Interface Layout", ImGuiTreeNodeFlags.DefaultOpen))
        {
            var storedDir =$@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Smithbox\";
            var storedPath = $@"{storedDir}\imgui.ini";

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Store the current imgui.ini in the AppData folder for future usage.");

            ImGui.AlignTextToFramePadding();
            if (ImGui.Button("Store##storeImguiIni", buttonSize))
            {
                var curImgui = $@"{AppContext.BaseDirectory}\imgui.ini";

                if (Directory.Exists(storedDir))
                {
                    if (File.Exists(storedPath))
                    {
                        File.Copy(curImgui, storedPath, true);
                    }
                    else
                    {
                        File.Copy(curImgui, storedPath, true);
                    }
                }

                PlatformUtils.Instance.MessageBox($"Stored at {storedPath}.", "Information", MessageBoxButtons.OK);
            }
            ImGui.SameLine();
            if (ImGui.Button("Open Folder##openImguiIniFolder", buttonSize))
            {
                Process.Start("explorer.exe", storedDir);
            }

            if (File.Exists(storedPath))
            {
                ImGui.Separator();

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Set the current imgui.ini to the version you stored within the AppData folder.");

                ImGui.AlignTextToFramePadding();

                if (ImGui.Button("Set##setImguiIni", buttonSize))
                {
                    var curImgui = $@"{AppContext.BaseDirectory}\imgui.ini";

                    if (File.Exists(storedPath))
                    {
                        File.Copy(storedPath, curImgui, true);
                    }

                    PlatformUtils.Instance.MessageBox("Applied imgui.ini change. Restart Smithbox to apply changes.", "Information", MessageBoxButtons.OK);
                }
            }

            ImGui.Separator();

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Reset the imgui.ini to the default version.");

            ImGui.AlignTextToFramePadding();
            if (ImGui.Button("Reset##resetImguiIni", buttonSize))
            {
                var curImgui = $@"{AppContext.BaseDirectory}\imgui.ini";
                var defaultImgui = $@"{AppContext.BaseDirectory}\imgui.default";

                if (Directory.Exists(storedDir))
                {
                    File.Copy(defaultImgui, curImgui, true);
                }

                PlatformUtils.Instance.MessageBox("Applied imgui.ini change. Restart Smithbox to apply changes.", "Information", MessageBoxButtons.OK);
            }
        }

        if (ImGui.CollapsingHeader("Theme", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Text("Current Theme");

            if (ImGui.ListBox("##themeSelect", ref UI.Current.SelectedTheme, InterfaceTheme.LoadedThemeNames, InterfaceTheme.LoadedThemeNames.Length))
            {
                InterfaceTheme.SetTheme(false);
            }

            if (ImGui.Button("Reset to Default"))
            {
                InterfaceTheme.ResetInterface();
            }
            ImGui.SameLine();
            if (ImGui.Button("Open Theme Folder"))
            {
                Process.Start("explorer.exe", $"{AppContext.BaseDirectory}\\Assets\\Themes\\");
            }
            ImGui.SameLine();

            if (ImGui.Button("Export Theme"))
            {
                InterfaceTheme.ExportThemeJson();
            }
            ImGui.SameLine();
            ImGui.InputText("##themeName", ref UI.Current.NewThemeName, 255);

            if (ImGui.CollapsingHeader("Editor Window", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Main Background##ImGui_MainBg", ref UI.Current.ImGui_MainBg);
                ImGui.ColorEdit4("Child Background##ImGui_ChildBg", ref UI.Current.ImGui_ChildBg);
                ImGui.ColorEdit4("Popup Background##ImGui_PopupBg", ref UI.Current.ImGui_PopupBg);
                ImGui.ColorEdit4("Border##ImGui_Border", ref UI.Current.ImGui_Border);
                ImGui.ColorEdit4("Title Bar Background##ImGui_TitleBarBg", ref UI.Current.ImGui_TitleBarBg);
                ImGui.ColorEdit4("Title Bar Background (Active)##ImGui_TitleBarBg_Active", ref UI.Current.ImGui_TitleBarBg_Active);
                ImGui.ColorEdit4("Menu Bar Background##ImGui_MenuBarBg", ref UI.Current.ImGui_MenuBarBg);
            }

            if (ImGui.CollapsingHeader("Moveable Window", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Main Background##Imgui_Moveable_MainBg", ref UI.Current.Imgui_Moveable_MainBg);
                ImGui.ColorEdit4("Child Background##Imgui_Moveable_ChildBg", ref UI.Current.Imgui_Moveable_ChildBg);
                ImGui.ColorEdit4("Child Background##Imgui_Moveable_ChildBgSecondary", ref UI.Current.Imgui_Moveable_ChildBgSecondary);
                ImGui.ColorEdit4("Header##Imgui_Moveable_Header", ref UI.Current.Imgui_Moveable_Header);
                ImGui.ColorEdit4("Title Bar Background##Imgui_Moveable_TitleBg", ref UI.Current.Imgui_Moveable_TitleBg);
                ImGui.ColorEdit4("Title Bar Background (Active)##Imgui_Moveable_TitleBg_Active", ref UI.Current.Imgui_Moveable_TitleBg_Active);
            }

            if (ImGui.CollapsingHeader("Scrollbar", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Scrollbar Background", ref UI.Current.ImGui_ScrollbarBg);
                ImGui.ColorEdit4("Scrollbar Grab", ref UI.Current.ImGui_ScrollbarGrab);
                ImGui.ColorEdit4("Scrollbar Grab (Hover)", ref UI.Current.ImGui_ScrollbarGrab_Hover);
                ImGui.ColorEdit4("Scrollbar Grab (Active)", ref UI.Current.ImGui_ScrollbarGrab_Active);
                ImGui.ColorEdit4("Slider Grab", ref UI.Current.ImGui_SliderGrab);
                ImGui.ColorEdit4("Slider Grab (Active)", ref UI.Current.ImGui_SliderGrab_Active);
            }

            if (ImGui.CollapsingHeader("Tab", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Tab", ref UI.Current.ImGui_Tab);
                ImGui.ColorEdit4("Tab (Hover)", ref UI.Current.ImGui_Tab_Hover);
                ImGui.ColorEdit4("Tab (Active)", ref UI.Current.ImGui_Tab_Active);
                ImGui.ColorEdit4("Unfocused Tab", ref UI.Current.ImGui_UnfocusedTab);
                ImGui.ColorEdit4("Unfocused Tab (Active)", ref UI.Current.ImGui_UnfocusedTab_Active);
            }

            if (ImGui.CollapsingHeader("Button", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Button", ref UI.Current.ImGui_Button);
                ImGui.ColorEdit4("Button (Hover)", ref UI.Current.ImGui_Button_Hovered);
                ImGui.ColorEdit4("Button (Active)", ref UI.Current.ImGui_ButtonActive);
            }

            if (ImGui.CollapsingHeader("Selection", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Selection", ref UI.Current.ImGui_Selection);
                ImGui.ColorEdit4("Selection (Hover)", ref UI.Current.ImGui_Selection_Hover);
                ImGui.ColorEdit4("Selection (Active)", ref UI.Current.ImGui_Selection_Active);
            }

            if (ImGui.CollapsingHeader("Inputs", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Input Background", ref UI.Current.ImGui_Input_Background);
                ImGui.ColorEdit4("Input Background (Hover)", ref UI.Current.ImGui_Input_Background_Hover);
                ImGui.ColorEdit4("Input Background (Active)", ref UI.Current.ImGui_Input_Background_Active);
                ImGui.ColorEdit4("Input Checkmark", ref UI.Current.ImGui_Input_CheckMark);
                ImGui.ColorEdit4("Input Conflict Background", ref UI.Current.ImGui_Input_Conflict_Background);
                ImGui.ColorEdit4("Input Vanilla Background", ref UI.Current.ImGui_Input_Vanilla_Background);
                ImGui.ColorEdit4("Input Default Background", ref UI.Current.ImGui_Input_Default_Background);
                ImGui.ColorEdit4("Input Auxillary Vanilla Background", ref UI.Current.ImGui_Input_AuxVanilla_Background);
                ImGui.ColorEdit4("Input Difference Comparison Background", ref UI.Current.ImGui_Input_DiffCompare_Background);
            }

            if (ImGui.CollapsingHeader("Text", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Default Text", ref UI.Current.ImGui_Default_Text_Color);
                ImGui.ColorEdit4("Warning Text", ref UI.Current.ImGui_Warning_Text_Color);
                ImGui.ColorEdit4("Beneficial Text", ref UI.Current.ImGui_Benefit_Text_Color);
                ImGui.ColorEdit4("Invalid Text", ref UI.Current.ImGui_Invalid_Text_Color);

                ImGui.ColorEdit4("Param Reference Text", ref UI.Current.ImGui_ParamRef_Text);
                ImGui.ColorEdit4("Param Reference Missing Text", ref UI.Current.ImGui_ParamRefMissing_Text);
                ImGui.ColorEdit4("Param Reference Inactive Text", ref UI.Current.ImGui_ParamRefInactive_Text);
                ImGui.ColorEdit4("Enum Name Text", ref UI.Current.ImGui_EnumName_Text);
                ImGui.ColorEdit4("Enum Value Text", ref UI.Current.ImGui_EnumValue_Text);
                ImGui.ColorEdit4("FMG Link Text", ref UI.Current.ImGui_FmgLink_Text);
                ImGui.ColorEdit4("FMG Reference Text", ref UI.Current.ImGui_FmgRef_Text);
                ImGui.ColorEdit4("FMG Reference Inactive Text", ref UI.Current.ImGui_FmgRefInactive_Text);
                ImGui.ColorEdit4("Is Reference Text", ref UI.Current.ImGui_IsRef_Text);
                ImGui.ColorEdit4("Virtual Reference Text", ref UI.Current.ImGui_VirtualRef_Text);
                ImGui.ColorEdit4("Reference Text", ref UI.Current.ImGui_Ref_Text);
                ImGui.ColorEdit4("Auxiliary Conflict Text", ref UI.Current.ImGui_AuxConflict_Text);
                ImGui.ColorEdit4("Auxiliary Added Text", ref UI.Current.ImGui_AuxAdded_Text);
                ImGui.ColorEdit4("Primary Changed Text", ref UI.Current.ImGui_PrimaryChanged_Text);
                ImGui.ColorEdit4("Param Row Text", ref UI.Current.ImGui_ParamRow_Text);
                ImGui.ColorEdit4("Aliased Name Text", ref UI.Current.ImGui_AliasName_Text);

                ImGui.ColorEdit4("Text Editor: Modified Row", ref UI.Current.ImGui_TextEditor_ModifiedRow_Text);
                ImGui.ColorEdit4("Text Editor: Unique Row", ref UI.Current.ImGui_TextEditor_UniqueRow_Text);

                ImGui.ColorEdit4("Logger: Information", ref UI.Current.ImGui_Logger_Information_Color);
                ImGui.ColorEdit4("Logger: Warning", ref UI.Current.ImGui_Logger_Warning_Color);
                ImGui.ColorEdit4("Logger: Error", ref UI.Current.ImGui_Logger_Error_Color);
            }

            if (ImGui.CollapsingHeader("Havok Editor", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Header", ref UI.Current.ImGui_Havok_Header);
                ImGui.ColorEdit4("Reference", ref UI.Current.ImGui_Havok_Reference);
                ImGui.ColorEdit4("Highlight", ref UI.Current.ImGui_Havok_Highlight);
                ImGui.ColorEdit4("Warning", ref UI.Current.ImGui_Havok_Warning);
            }

            if (ImGui.CollapsingHeader("Miscellaneous", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Display Group: Border Highlight", ref UI.Current.DisplayGroupEditor_Border_Highlight);
                ImGui.ColorEdit4("Display Group: Active Input Background", ref UI.Current.DisplayGroupEditor_DisplayActive_Frame);
                ImGui.ColorEdit4("Display Group: Active Checkbox", ref UI.Current.DisplayGroupEditor_DisplayActive_Checkbox);
                ImGui.ColorEdit4("Draw Group: Active Input Background", ref UI.Current.DisplayGroupEditor_DrawActive_Frame);
                ImGui.ColorEdit4("Draw Group: Active Checkbox", ref UI.Current.DisplayGroupEditor_DrawActive_Checkbox);
                ImGui.ColorEdit4("Combined Group: Active Input Background", ref UI.Current.DisplayGroupEditor_CombinedActive_Frame);
                ImGui.ColorEdit4("Combined Group: Active Checkbox", ref UI.Current.DisplayGroupEditor_CombinedActive_Checkbox);
            }
        }
    }
}
