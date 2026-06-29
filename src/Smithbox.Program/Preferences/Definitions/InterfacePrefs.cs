using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace StudioCore.Preferences;

public class InterfacePrefs
{
    public static Type GetPrefType()
    {
        return typeof(InterfacePrefs);
    }

    #region General
    public static PreferenceItem Interface_UI_Scale()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.Interface,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.General,

            Title = "PREF_Interface_UI_Scale",
            Description = "PREF_Interface_UI_Scale_TT",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.SliderFloat("##inputValue", ref PreferencesUtil.TempScale, 0.5f, 4.0f);

                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.Interface_UI_Scale = (float)Math.Round(PreferencesUtil.TempScale * 20) / 20;
                    PreferencesUtil.TempScale = CFG.Current.Interface_UI_Scale;
                    DPI.UIScaleChanged?.Invoke(null, EventArgs.Empty);
                }

                ImGui.SameLine();

                if (ImGui.Button("Reset", DPI.SelectorButtonSize))
                {
                    CFG.Current.Interface_UI_Scale = CFG.Default.Interface_UI_Scale;
                    PreferencesUtil.TempScale = CFG.Current.Interface_UI_Scale;
                    DPI.UIScaleChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        };
    }

    public static PreferenceItem Interface_Scale_by_DPI()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.Interface,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "PREF_Interface_Scale_by_DPI",
            Description = "PREF_Interface_Scale_by_DPI_TT",

            Draw = () => {
                ImGui.Checkbox($"##inputValue", ref CFG.Current.Interface_Scale_by_DPI);

                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    DPI.UIScaleChanged?.Invoke(null, EventArgs.Empty);
                }

            },
            PostDraw = () =>
            {
                ImGui.TextDisabled($"DPI: {(DPI.Dpi / 96).ToString("P0", new NumberFormatInfo { PercentPositivePattern = 1, PercentNegativePattern = 1 })}");
            }
        };
    }

    public static PreferenceItem Interface_Font_Size()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.Interface,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.General,

            Title = "PREF_Interface_Font_Size",
            Description = "PREF_Interface_Font_Size_TT",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.SliderFloat("##inputValue", ref CFG.Current.Interface_Font_Size, 8.0f, 32.0f);

                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    CFG.Current.Interface_Font_Size = (float)Math.Round(CFG.Current.Interface_Font_Size);
                    DPI.UIScaleChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        };
    }

    public static PreferenceItem Interface_Alias_Wordwrap_General()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.Interface,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "PREF_Interface_Alias_Wordwrap_General",
            Description = "PREF_Interface_Alias_Wordwrap_General_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Interface_Alias_Wordwrap_General);
            }
        };
    }

    public static PreferenceItem Interface_Alias_Wordwrap_Map_Editor()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.Interface,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "PREF_Interface_Alias_Wordwrap_Map_Editor",
            Description = "PREF_Interface_Alias_Wordwrap_Map_Editor_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Interface_Alias_Wordwrap_Map_Editor);
            }
        };
    }
    public static PreferenceItem Interface_Alias_Wordwrap_Model_Editor()
    {
        return new PreferenceItem
        {
            OrderID = 5,
            Category = PreferenceCategory.Interface,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "PREF_Interface_Alias_Wordwrap_Model_Editor",
            Description = "PREF_Interface_Alias_Wordwrap_Model_Editor_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Interface_Alias_Wordwrap_Model_Editor);
            }
        };
    }
    public static PreferenceItem Interface_Alias_Wordwrap_Animation_Editor()
    {
        return new PreferenceItem
        {
            OrderID = 6,
            Category = PreferenceCategory.Interface,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "PREF_Interface_Alias_Wordwrap_Animation_Editor",
            Description = "PREF_Interface_Alias_Wordwrap_Animation_Editor_TT",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Interface_Alias_Wordwrap_Animation_Editor);
            }
        };
    }
    public static PreferenceItem Interface_Allow_Window_Movement()
    {
        return new PreferenceItem
        {
            OrderID = 7,
            Category = PreferenceCategory.Interface,
            Spacer = true,

            Section = SectionCategory.General,

            Title = "Enable Window Movement",
            Description = "If enabled, the internal windows can be moved and docked freely.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Interface_Allow_Window_Movement);
            }
        };
    }

    #endregion

    #region Fonts
    public static PreferenceItem Interface_English_Font()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.Interface,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Fonts,

            Title = "PREF_English_Font",
            Description = "PREF_English_Font_TT",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputText("##inputValue", ref CFG.Current.English_Font, 255);

                ImGui.SameLine();

                if (ImGui.Button($"{LOC.Get("PREF_Set_Font")}##setFontAction", DPI.SelectorButtonSize))
                {
                    PlatformUtils.Instance.OpenFileDialog(LOC.Get("PREF_Select_Font"), ["*"], out string path);
                    if (File.Exists(path))
                    {
                        CFG.Current.English_Font = path;
                        Smithbox.FontRebuildRequest = true;
                    }
                }
            }
        };
    }

    public static PreferenceItem Interface_Additional_Font_1()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.Interface,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Fonts,

            Title = "PREF_Additional_Font_1",
            Description = "PREF_Additional_Font_1_TT",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputText("##inputValue", ref CFG.Current.Additional_Font_1, 255);

                ImGui.SameLine();

                if (ImGui.Button($"{LOC.Get("PREF_Set_Font")}##setFontAction", DPI.SelectorButtonSize))
                {
                    PlatformUtils.Instance.OpenFileDialog(LOC.Get("PREF_Select_Font"), ["*"], out string path);
                    if (File.Exists(path))
                    {
                        CFG.Current.Additional_Font_1 = path;
                        Smithbox.FontRebuildRequest = true;
                    }
                }
            }
        };
    }

    public static PreferenceItem Interface_Additional_Font_2()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.Interface,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Fonts,

            Title = "PREF_Additional_Font_2",
            Description = "PREF_Additional_Font_2_TT",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputText("##inputValue", ref CFG.Current.Additional_Font_2, 255);

                ImGui.SameLine();

                if (ImGui.Button($"{LOC.Get("PREF_Set_Font")}##setFontAction", DPI.SelectorButtonSize))
                {
                    PlatformUtils.Instance.OpenFileDialog(LOC.Get("PREF_Select_Font"), ["*"], out string path);
                    if (File.Exists(path))
                    {
                        CFG.Current.Additional_Font_2 = path;
                        Smithbox.FontRebuildRequest = true;
                    }
                }
            }
        };
    }

    public static PreferenceItem Interface_Additional_Font_3()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.Interface,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Fonts,

            Title = "PREF_Additional_Font_3",
            Description = "PREF_Additional_Font_3_TT",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputText("##inputValue", ref CFG.Current.Additional_Font_3, 255);

                ImGui.SameLine();

                if (ImGui.Button($"{LOC.Get("PREF_Set_Font")}##setFontAction", DPI.SelectorButtonSize))
                {
                    PlatformUtils.Instance.OpenFileDialog(LOC.Get("PREF_Select_Font"), ["*"], out string path);
                    if (File.Exists(path))
                    {
                        CFG.Current.Additional_Font_3 = path;
                        Smithbox.FontRebuildRequest = true;
                    }
                }
            }
        };
    }

    public static PreferenceItem Interface_Additional_Font_4()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.Interface,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Fonts,

            Title = "PREF_Additional_Font_4",
            Description = "PREF_Additional_Font_4_TT",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputText("##inputValue", ref CFG.Current.Additional_Font_4, 255);

                ImGui.SameLine();

                if (ImGui.Button($"{LOC.Get("PREF_Set_Font")}##setFontAction", DPI.SelectorButtonSize))
                {
                    PlatformUtils.Instance.OpenFileDialog(LOC.Get("PREF_Select_Font"), ["*"], out string path);
                    if (File.Exists(path))
                    {
                        CFG.Current.Additional_Font_4 = path;
                        Smithbox.FontRebuildRequest = true;
                    }
                }
            }
        };
    }

    public static PreferenceItem Interface_Additional_Font_5()
    {
        return new PreferenceItem
        {
            OrderID = 5,
            Category = PreferenceCategory.Interface,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Fonts,

            Title = "PREF_Additional_Font_5",
            Description = "PREF_Additional_Font_5_TT",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputText("##inputValue", ref CFG.Current.Additional_Font_5, 255);

                ImGui.SameLine();

                if (ImGui.Button($"{LOC.Get("PREF_Set_Font")}##setFontAction", DPI.SelectorButtonSize))
                {
                    PlatformUtils.Instance.OpenFileDialog(LOC.Get("PREF_Select_Font"), ["*"], out string path);
                    if (File.Exists(path))
                    {
                        CFG.Current.Additional_Font_5 = path;
                        Smithbox.FontRebuildRequest = true;
                    }
                }
            }
        };
    }

    #endregion

    #region Theme
    public static PreferenceItem ThemeSelector()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.Interface,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Theme,

            Title = "PREF_Theme_Selection",
            Description = "PREF_Theme_Selection_TT",

            Draw = () => {
                var folder = ProjectUtils.GetThemeFolder();

                var files = Directory.EnumerateFiles(folder);

                var themeFiles = new List<string>();
                foreach (var file in files)
                {
                    var filename = Path.GetFileNameWithoutExtension(file);
                    themeFiles.Add(filename);
                }

                DPI.ApplyInputWidth();
                if (ImGui.BeginCombo("##themeSelect", PreferencesUtil.CurrentThemeName))
                {
                    foreach (var entry in themeFiles)
                    {
                        if (ImGui.Selectable(entry, entry == PreferencesUtil.CurrentThemeName))
                        {
                            PreferencesUtil.CurrentThemeName = entry;
                            CFG.Current.Interface_Selected_Theme = PreferencesUtil.CurrentThemeName;
                            UI.LoadTheme(entry);
                        }
                    }

                    ImGui.EndCombo();
                }

                if (ImGui.Button($"{LOC.Get("PREF_Theme_Reset")}##themeReset", DPI.SelectorButtonSize))
                {
                    UI.LoadDefault();
                }

                ImGui.SameLine();

                if (ImGui.Button($"{LOC.Get("PREF_Theme_Open_Folder")}##themeOpenFolder", DPI.SelectorButtonSize))
                {
                    Process.Start("explorer.exe", Path.Join(AppContext.BaseDirectory, "Assets", "Themes")); 
                }
            }
        };
    }

    // NB: not split up for user ease-of-use, as otherwise the ordering would be messy.
    // However, this does mean the search doesn't see the colorEdit names
    public static PreferenceItem ThemeBuilder()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.Interface,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.ThemeBuilder,

            Title = "PREF_Theme_Builder",
            Description = "",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputText("##themeName", ref PreferencesUtil.NewThemeName, 255);

                ImGui.SameLine();

                if (ImGui.Button($"{LOC.Get("PREF_Theme_Builder_Save")}##themeBuilderSave", DPI.SelectorButtonSize))
                {
                    UI.ExportTheme(PreferencesUtil.NewThemeName);
                }

                var flags = ImGuiColorEditFlags.AlphaOpaque;

                // Theme Editor
                UIHelper.SimpleHeader(
                    LOC.Get("PREF_Theme_Editor"),
                    LOC.Get("PREF_Theme_Editor_TT"));

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_MainBg")}##ImGui_MainBg", 
                    ref UI.Current.ImGui_MainBg, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_ChildBg")}##ImGui_ChildBg", 
                    ref UI.Current.ImGui_ChildBg, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_PopupBg")}##ImGui_PopupBg", 
                    ref UI.Current.ImGui_PopupBg, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Border")}##ImGui_Border", 
                    ref UI.Current.ImGui_Border, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_TitleBarBg")}##ImGui_TitleBarBg", 
                    ref UI.Current.ImGui_TitleBarBg, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_TitleBarBg_Active")}##ImGui_TitleBarBg_Active", 
                    ref UI.Current.ImGui_TitleBarBg_Active, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_MenuBarBg")}##ImGui_MenuBarBg", 
                    ref UI.Current.ImGui_MenuBarBg, flags);

                // Moveable Window
                UIHelper.SimpleHeader(
                    LOC.Get("PREF_Theme_Editor_Moveable_Window"),
                    LOC.Get("PREF_Theme_Editor_Moveable_Window_TT"));

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_Imgui_Moveable_MainBg")}##Imgui_Moveable_MainBg", 
                    ref UI.Current.Imgui_Moveable_MainBg, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_Imgui_Moveable_ChildBg")}##Imgui_Moveable_ChildBg", 
                    ref UI.Current.Imgui_Moveable_ChildBg, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_Imgui_Moveable_ChildBgSecondary")}##Imgui_Moveable_ChildBgSecondary", 
                    ref UI.Current.Imgui_Moveable_ChildBgSecondary, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_Imgui_Moveable_Header")}##Imgui_Moveable_Header", 
                    ref UI.Current.Imgui_Moveable_Header, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_Imgui_Moveable_TitleBg")}##Imgui_Moveable_TitleBg", 
                    ref UI.Current.Imgui_Moveable_TitleBg, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_Imgui_Moveable_TitleBg_Active")}##Imgui_Moveable_TitleBg_Active", 
                    ref UI.Current.Imgui_Moveable_TitleBg_Active, flags);

                // Scrollbar
                UIHelper.SimpleHeader(
                    LOC.Get("PREF_Theme_Editor_Scrollbar"),
                    LOC.Get("PREF_Theme_Editor_Scrollbar_TT"));

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_ScrollbarBg")}##ImGui_ScrollbarBg", 
                    ref UI.Current.ImGui_ScrollbarBg, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_ScrollbarGrab")}##ImGui_ScrollbarGrab", 
                    ref UI.Current.ImGui_ScrollbarGrab, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_ScrollbarGrab_Hover")}##ImGui_ScrollbarGrab_Hover", 
                    ref UI.Current.ImGui_ScrollbarGrab_Hover, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_ScrollbarGrab_Active")}##ImGui_ScrollbarGrab_Active", 
                    ref UI.Current.ImGui_ScrollbarGrab_Active, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_SliderGrab")}##ImGui_SliderGrab", 
                    ref UI.Current.ImGui_SliderGrab, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_SliderGrab_Active")}##ImGui_SliderGrab_Active", 
                    ref UI.Current.ImGui_SliderGrab_Active, flags);

                // Tab
                UIHelper.SimpleHeader(
                    LOC.Get("PREF_Theme_Editor_Tab"),
                    LOC.Get("PREF_Theme_Editor_Tab_TT"));

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Tab")}##ImGui_Tab",
                    ref UI.Current.ImGui_Tab, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Tab_Hover")}##ImGui_Tab_Hover",
                    ref UI.Current.ImGui_Tab_Hover, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Tab_Active")}##ImGui_Tab_Active",
                    ref UI.Current.ImGui_Tab_Active, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_UnfocusedTab")}##ImGui_UnfocusedTab",
                    ref UI.Current.ImGui_UnfocusedTab, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_UnfocusedTab_Active")}##ImGui_UnfocusedTab_Active",
                    ref UI.Current.ImGui_UnfocusedTab_Active, flags);

                // Button
                UIHelper.SimpleHeader(
                    LOC.Get("PREF_Theme_Editor_Button"),
                    LOC.Get("PREF_Theme_Editor_Button_TT"));

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Button")}##ImGui_Button",
                    ref UI.Current.ImGui_Button, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Button_Hovered")}##ImGui_Button_Hovered",
                    ref UI.Current.ImGui_Button_Hovered, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_ButtonActive")}##ImGui_ButtonActive",
                    ref UI.Current.ImGui_ButtonActive, flags);

                // Selection
                UIHelper.SimpleHeader(
                    LOC.Get("PREF_Theme_Editor_Selection"),
                    LOC.Get("PREF_Theme_Editor_Selection_TT"));

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Selection")}##ImGui_Selection",
                    ref UI.Current.ImGui_Selection, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Selection_Hover")}##ImGui_Selection_Hover",
                    ref UI.Current.ImGui_Selection_Hover, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Selection_Active")}##ImGui_Selection_Active",
                    ref UI.Current.ImGui_Selection_Active, flags);

                UIHelper.SimpleHeader(
                    LOC.Get("PREF_Theme_Editor_Inputs"),
                    LOC.Get("PREF_Theme_Editor_Inputs_TT"));

                // Inputs
                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Input_Background")}##ImGui_Input_Background",
                    ref UI.Current.ImGui_Input_Background, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Input_Background_Hover")}##ImGui_Input_Background_Hover",
                    ref UI.Current.ImGui_Input_Background_Hover, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Input_Background_Active")}##ImGui_Input_Background_Active",
                    ref UI.Current.ImGui_Input_Background_Active, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Input_CheckMark")}##ImGui_Input_CheckMark",
                    ref UI.Current.ImGui_Input_CheckMark, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Input_Conflict_Background")}##ImGui_Input_Conflict_Background",
                    ref UI.Current.ImGui_Input_Conflict_Background, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Input_Vanilla_Background")}##ImGui_Input_Vanilla_Background",
                    ref UI.Current.ImGui_Input_Vanilla_Background, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Input_Default_Background")}##ImGui_Input_Default_Background",
                    ref UI.Current.ImGui_Input_Default_Background, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Input_AuxVanilla_Background")}##ImGui_Input_AuxVanilla_Background",
                    ref UI.Current.ImGui_Input_AuxVanilla_Background, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Input_DiffCompare_Background")}##ImGui_Input_DiffCompare_Background",
                    ref UI.Current.ImGui_Input_DiffCompare_Background, flags);

                // Backgrounds
                UIHelper.SimpleHeader(
                    LOC.Get("PREF_Theme_Editor_Backgrounds"),
                    LOC.Get("PREF_Theme_Editor_Backgrounds_TT"));

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ParamRowDiffBackgroundColor")}##ParamRowDiffBackgroundColor",
                    ref UI.Current.ParamRowDiffBackgroundColor, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ParamDiffBackgroundColor")}##ParamDiffBackgroundColor",
                    ref UI.Current.ParamDiffBackgroundColor, flags);

                // Text
                UIHelper.SimpleHeader(
                    LOC.Get("PREF_Theme_Editor_Text"),
                    LOC.Get("PREF_Theme_Editor_Text_TT"));

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Default_Text_Color")}##ImGui_Default_Text_Color",
                    ref UI.Current.ImGui_Default_Text_Color, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Warning_Text_Color")}##ImGui_Warning_Text_Color",
                    ref UI.Current.ImGui_Warning_Text_Color, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Benefit_Text_Color")}##ImGui_Benefit_Text_Color",
                    ref UI.Current.ImGui_Benefit_Text_Color, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Invalid_Text_Color")}##ImGui_Invalid_Text_Color",
                    ref UI.Current.ImGui_Invalid_Text_Color, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_ParamRef_Text")}##ImGui_ParamRef_Text",
                    ref UI.Current.ImGui_ParamRef_Text, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_ParamRefMissing_Text")}##ImGui_ParamRefMissing_Text",
                    ref UI.Current.ImGui_ParamRefMissing_Text, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_ParamRefInactive_Text")}##ImGui_ParamRefInactive_Text",
                    ref UI.Current.ImGui_ParamRefInactive_Text, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_EnumName_Text")}##ImGui_EnumName_Text",
                    ref UI.Current.ImGui_EnumName_Text, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_EnumValue_Text")}##ImGui_EnumValue_Text",
                    ref UI.Current.ImGui_EnumValue_Text, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_FmgLink_Text")}##ImGui_FmgLink_Text",
                    ref UI.Current.ImGui_FmgLink_Text, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_FmgRef_Text")}##ImGui_FmgRef_Text",
                    ref UI.Current.ImGui_FmgRef_Text, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_FmgRefInactive_Text")}##ImGui_FmgRefInactive_Text",
                    ref UI.Current.ImGui_FmgRefInactive_Text, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_IsRef_Text")}##ImGui_IsRef_Text",
                    ref UI.Current.ImGui_IsRef_Text, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_VirtualRef_Text")}##ImGui_VirtualRef_Text",
                    ref UI.Current.ImGui_VirtualRef_Text, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Ref_Text")}##ImGui_Ref_Text",
                    ref UI.Current.ImGui_Ref_Text, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_AuxConflict_Text")}##ImGui_AuxConflict_Text",
                    ref UI.Current.ImGui_AuxConflict_Text, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_AuxAdded_Text")}##ImGui_AuxAdded_Text",
                    ref UI.Current.ImGui_AuxAdded_Text, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_PrimaryChanged_Text")}##ImGui_PrimaryChanged_Text",
                    ref UI.Current.ImGui_PrimaryChanged_Text, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_ParamRow_Text")}##ImGui_ParamRow_Text",
                    ref UI.Current.ImGui_ParamRow_Text, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_AliasName_Text")}##ImGui_AliasName_Text",
                    ref UI.Current.ImGui_AliasName_Text, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_TextEditor_ModifiedTextEntry_Text")}##ImGui_TextEditor_ModifiedTextEntry_Text",
                    ref UI.Current.ImGui_TextEditor_ModifiedTextEntry_Text, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_TextEditor_UniqueTextEntry_Text")}##ImGui_TextEditor_UniqueTextEntry_Text",
                    ref UI.Current.ImGui_TextEditor_UniqueTextEntry_Text, flags);

                // Logger
                UIHelper.SimpleHeader(
                    LOC.Get("PREF_Theme_Editor_Logger"),
                    LOC.Get("PREF_Theme_Editor_Logger_TT"));

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Logger_Information_Color")}##ImGui_Logger_Information_Color",
                    ref UI.Current.ImGui_Logger_Information_Color, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Logger_Warning_Color")}##ImGui_Logger_Warning_Color",
                    ref UI.Current.ImGui_Logger_Warning_Color, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_ImGui_Logger_Error_Color")}##ImGui_Logger_Error_Color",
                    ref UI.Current.ImGui_Logger_Error_Color, flags);

                // Display Group Tool
                UIHelper.SimpleHeader(
                    LOC.Get("PREF_Theme_Editor_Display_Group"),
                    LOC.Get("PREF_Theme_Editor_Display_Group_TT"));

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_DisplayGroupEditor_Border_Highlight")}##DisplayGroupEditor_Border_Highlight",
                    ref UI.Current.DisplayGroupEditor_Border_Highlight, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_DisplayGroupEditor_DisplayActive_Frame")}##DisplayGroupEditor_DisplayActive_Frame",
                    ref UI.Current.DisplayGroupEditor_DisplayActive_Frame, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_DisplayGroupEditor_DisplayActive_Checkbox")}##DisplayGroupEditor_DisplayActive_Checkbox",
                    ref UI.Current.DisplayGroupEditor_DisplayActive_Checkbox, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_DisplayGroupEditor_DrawActive_Frame")}##DisplayGroupEditor_DrawActive_Frame",
                    ref UI.Current.DisplayGroupEditor_DrawActive_Frame, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_DisplayGroupEditor_DrawActive_Checkbox")}##DisplayGroupEditor_DrawActive_Checkbox",
                    ref UI.Current.DisplayGroupEditor_DrawActive_Checkbox, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_DisplayGroupEditor_CombinedActive_Frame")}##DisplayGroupEditor_CombinedActive_Frame",
                    ref UI.Current.DisplayGroupEditor_CombinedActive_Frame, flags);

                ImGui.ColorEdit4(
                    $"{LOC.Get("PREF_CE_DisplayGroupEditor_CombinedActive_Checkbox")}##DisplayGroupEditor_CombinedActive_Checkbox",
                    ref UI.Current.DisplayGroupEditor_CombinedActive_Checkbox, flags);
            }
        };
    }
    #endregion
}
