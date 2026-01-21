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

            Title = "Interface Scale",
            Description = "Adjusts the scaling of the user interface throughout all of Smithbox.",

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

            Title = "Interface Scale affected by DPI",
            Description = "Multiplies the user interface scale by your monitor's DPI setting.",

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

            Title = "Font Size",
            Description = "Adjusts the size of the font in Smithbox.",

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

            Title = "Enable Alias Word-wrap",
            Description = "If enabled, aliases will word-wrap if they touch a window's boundry. Otherwise, they will truncate.",

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

            Title = "Enable Alias Word-wrap (Map Editor)",
            Description = "If enabled, aliases will word-wrap if they touch a window's boundry. Otherwise, they will truncate. This affects the Map Editor only.",

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

            Title = "Enable Alias Word-wrap (Model Editor)",
            Description = "If enabled, aliases will word-wrap if they touch a window's boundry. Otherwise, they will truncate. This affects the Model Editor only.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Interface_Alias_Wordwrap_Model_Editor);
            }
        };
    }
    public static PreferenceItem Interface_Allow_Window_Movement()
    {
        return new PreferenceItem
        {
            OrderID = 6,
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
    public static PreferenceItem Interface_Context_Menu_Width()
    {
        return new PreferenceItem
        {
            OrderID = 7,
            Category = PreferenceCategory.Interface,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.General,

            Title = "Popup Context Menu Width",
            Description = "The width of a popup context menu within an editor.",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.DragFloat("##inputValue", ref CFG.Current.Interface_Context_Menu_Width);
            }
        };
    }
    public static PreferenceItem Interface_Context_Menu_List_Height_Multiplier()
    {
        return new PreferenceItem
        {
            OrderID = 7,
            Category = PreferenceCategory.Interface,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.General,

            Title = "Listbox Line Height Multiplier",
            Description = "The line height multiple to use for a listbox per entry.",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.DragFloat("##inputValue", ref CFG.Current.Interface_Context_Menu_List_Height_Multiplier);
            }
        };
    }


    #endregion

    #region Fonts
    public static PreferenceItem Interface_English_Font_Path()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.Interface,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Fonts,

            Title = "English Font",
            Description = "Set the font used for English characters. .ttf and .otf expected.",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputText("##inputValue", ref CFG.Current.Interface_English_Font_Path, 255);

                ImGui.SameLine();

                if (ImGui.Button("Set Font", DPI.SelectorButtonSize))
                {
                    PlatformUtils.Instance.OpenFileDialog("Select Font", ["*"], out string path);
                    if (File.Exists(path))
                    {
                        CFG.Current.Interface_English_Font_Path = path;
                        Smithbox.FontRebuildRequest = true;
                    }
                }

                ImGui.SameLine();

                if (ImGui.Button("Reset", DPI.SelectorButtonSize))
                {
                    CFG.Current.Interface_English_Font_Path = Path.Join("Assets", "Fonts", "RobotoMono-Light.ttf");
                    Smithbox.FontRebuildRequest = true;
                }
            }
        };
    }

    public static PreferenceItem Interface_Non_English_Font_Path()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.Interface,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Fonts,

            Title = "Non-English Font",
            Description = "Set the font used for Non-English characters. .ttf and .otf expected.",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputText("##inputValue", ref CFG.Current.Interface_Non_English_Font_Path, 255);

                ImGui.SameLine();

                if (ImGui.Button("Set Font", DPI.SelectorButtonSize))
                {
                    PlatformUtils.Instance.OpenFileDialog("Select Font", ["*"], out string path);
                    if (File.Exists(path))
                    {
                        CFG.Current.Interface_Non_English_Font_Path = path;
                        Smithbox.FontRebuildRequest = true;
                    }
                }

                ImGui.SameLine();

                if (ImGui.Button("Reset", DPI.SelectorButtonSize))
                {
                    CFG.Current.Interface_Non_English_Font_Path = Path.Join("Assets", "Fonts", "NotoSansCJKtc-Light.otf");
                    Smithbox.FontRebuildRequest = true;
                }
            }
        };
    }

    #endregion

    #region Additional Font Symbols
    public static PreferenceItem Interface_Include_Chinese_Symbols()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.Interface,
            Spacer = true,

            Section = SectionCategory.AdditionalFontSymbols,

            Title = "Enable Chinese",
            Description = "Include Chinese font.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Interface_Include_Chinese_Symbols);
            }
        };
    }

    public static PreferenceItem Interface_Include_Korean_Symbols()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.Interface,
            Spacer = true,

            Section = SectionCategory.AdditionalFontSymbols,

            Title = "Enable Korean",
            Description = "Include Korean font.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Interface_Include_Korean_Symbols);
            }
        };
    }

    public static PreferenceItem Interface_Include_Thai_Symbols()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.Interface,
            Spacer = true,

            Section = SectionCategory.AdditionalFontSymbols,

            Title = "Enable Thai",
            Description = "Include Thai font.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Interface_Include_Thai_Symbols);
            }
        };
    }

    public static PreferenceItem Interface_Include_Vietnamese_Symbols()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.Interface,
            Spacer = true,

            Section = SectionCategory.AdditionalFontSymbols,

            Title = "Enable Vietnamese",
            Description = "Include Vietnamese font.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Interface_Include_Vietnamese_Symbols);
            }
        };
    }

    public static PreferenceItem Interface_Include_Cyrillic_Symbols()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.Interface,
            Spacer = true,

            Section = SectionCategory.AdditionalFontSymbols,

            Title = "Enable Cyrillic",
            Description = "Include Cyrillic font.",

            Draw = () => {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Interface_Include_Cyrillic_Symbols);
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

            Title = "Theme Selection",
            Description = "Select the interface theme to use for Smithbox.",

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

                if (ImGui.Button("Reset", DPI.SelectorButtonSize))
                {
                    UI.LoadDefault();
                }

                ImGui.SameLine();

                if (ImGui.Button("Open Folder", DPI.SelectorButtonSize))
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

            Title = "New Theme Name",
            Description = "",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.InputText("##themeName", ref PreferencesUtil.NewThemeName, 255);

                ImGui.SameLine();

                if (ImGui.Button("Save", DPI.SelectorButtonSize))
                {
                    UI.ExportTheme(PreferencesUtil.NewThemeName);
                }

                var flags = ImGuiColorEditFlags.AlphaOpaque;

                UIHelper.SimpleHeader("Editor Window", "");

                ImGui.ColorEdit4("Main Background##ImGui_MainBg", ref UI.Current.ImGui_MainBg, flags);
                ImGui.ColorEdit4("Child Background##ImGui_ChildBg", ref UI.Current.ImGui_ChildBg, flags);
                ImGui.ColorEdit4("Popup Background##ImGui_PopupBg", ref UI.Current.ImGui_PopupBg, flags);
                ImGui.ColorEdit4("Border##ImGui_Border", ref UI.Current.ImGui_Border, flags);
                ImGui.ColorEdit4("Title Bar Background##ImGui_TitleBarBg", ref UI.Current.ImGui_TitleBarBg, flags);
                ImGui.ColorEdit4("Title Bar Background (Active)##ImGui_TitleBarBg_Active", ref UI.Current.ImGui_TitleBarBg_Active, flags);
                ImGui.ColorEdit4("Menu Bar Background##ImGui_MenuBarBg", ref UI.Current.ImGui_MenuBarBg, flags);

                UIHelper.SimpleHeader("Moveable Window", "");

                ImGui.ColorEdit4("Main Background##Imgui_Moveable_MainBg", ref UI.Current.Imgui_Moveable_MainBg, flags);
                ImGui.ColorEdit4("Child Background##Imgui_Moveable_ChildBg", ref UI.Current.Imgui_Moveable_ChildBg, flags);
                ImGui.ColorEdit4("Child Background##Imgui_Moveable_ChildBgSecondary", ref UI.Current.Imgui_Moveable_ChildBgSecondary, flags);
                ImGui.ColorEdit4("Header##Imgui_Moveable_Header", ref UI.Current.Imgui_Moveable_Header, flags);
                ImGui.ColorEdit4("Title Bar Background##Imgui_Moveable_TitleBg", ref UI.Current.Imgui_Moveable_TitleBg, flags);
                ImGui.ColorEdit4("Title Bar Background (Active)##Imgui_Moveable_TitleBg_Active", ref UI.Current.Imgui_Moveable_TitleBg_Active, flags);

                UIHelper.SimpleHeader("Scrollbar", "");

                ImGui.ColorEdit4("Scrollbar Background", ref UI.Current.ImGui_ScrollbarBg, flags);
                ImGui.ColorEdit4("Scrollbar Grab", ref UI.Current.ImGui_ScrollbarGrab, flags);
                ImGui.ColorEdit4("Scrollbar Grab (Hover)", ref UI.Current.ImGui_ScrollbarGrab_Hover, flags);
                ImGui.ColorEdit4("Scrollbar Grab (Active)", ref UI.Current.ImGui_ScrollbarGrab_Active, flags);
                ImGui.ColorEdit4("Slider Grab", ref UI.Current.ImGui_SliderGrab, flags);
                ImGui.ColorEdit4("Slider Grab (Active)", ref UI.Current.ImGui_SliderGrab_Active, flags);

                UIHelper.SimpleHeader("Tab", "");

                ImGui.ColorEdit4("Tab", ref UI.Current.ImGui_Tab, flags);
                ImGui.ColorEdit4("Tab (Hover)", ref UI.Current.ImGui_Tab_Hover, flags);
                ImGui.ColorEdit4("Tab (Active)", ref UI.Current.ImGui_Tab_Active, flags);
                ImGui.ColorEdit4("Unfocused Tab", ref UI.Current.ImGui_UnfocusedTab, flags);
                ImGui.ColorEdit4("Unfocused Tab (Active)", ref UI.Current.ImGui_UnfocusedTab_Active, flags);

                UIHelper.SimpleHeader("Button", "");

                ImGui.ColorEdit4("Button", ref UI.Current.ImGui_Button, flags);
                ImGui.ColorEdit4("Button (Hover)", ref UI.Current.ImGui_Button_Hovered, flags);
                ImGui.ColorEdit4("Button (Active)", ref UI.Current.ImGui_ButtonActive, flags);

                UIHelper.SimpleHeader("Selection", "");

                ImGui.ColorEdit4("Selection", ref UI.Current.ImGui_Selection, flags);
                ImGui.ColorEdit4("Selection (Hover)", ref UI.Current.ImGui_Selection_Hover, flags);
                ImGui.ColorEdit4("Selection (Active)", ref UI.Current.ImGui_Selection_Active, flags);

                UIHelper.SimpleHeader("Inputs", "");

                ImGui.ColorEdit4("Input Background", ref UI.Current.ImGui_Input_Background, flags);
                ImGui.ColorEdit4("Input Background (Hover)", ref UI.Current.ImGui_Input_Background_Hover, flags);
                ImGui.ColorEdit4("Input Background (Active)", ref UI.Current.ImGui_Input_Background_Active, flags);
                ImGui.ColorEdit4("Input Checkmark", ref UI.Current.ImGui_Input_CheckMark, flags);
                ImGui.ColorEdit4("Input Conflict Background", ref UI.Current.ImGui_Input_Conflict_Background, flags);
                ImGui.ColorEdit4("Input Vanilla Background", ref UI.Current.ImGui_Input_Vanilla_Background, flags);
                ImGui.ColorEdit4("Input Default Background", ref UI.Current.ImGui_Input_Default_Background, flags);
                ImGui.ColorEdit4("Input Auxillary Vanilla Background", ref UI.Current.ImGui_Input_AuxVanilla_Background, flags);
                ImGui.ColorEdit4("Input Difference Comparison Background", ref UI.Current.ImGui_Input_DiffCompare_Background, flags);

                UIHelper.SimpleHeader("Text", "");

                ImGui.ColorEdit4("Default Text", ref UI.Current.ImGui_Default_Text_Color, flags);
                ImGui.ColorEdit4("Warning Text", ref UI.Current.ImGui_Warning_Text_Color, flags);
                ImGui.ColorEdit4("Beneficial Text", ref UI.Current.ImGui_Benefit_Text_Color, flags);
                ImGui.ColorEdit4("Invalid Text", ref UI.Current.ImGui_Invalid_Text_Color, flags);

                ImGui.ColorEdit4("Param Reference Text", ref UI.Current.ImGui_ParamRef_Text, flags);
                ImGui.ColorEdit4("Param Reference Missing Text", ref UI.Current.ImGui_ParamRefMissing_Text, flags);
                ImGui.ColorEdit4("Param Reference Inactive Text", ref UI.Current.ImGui_ParamRefInactive_Text, flags);
                ImGui.ColorEdit4("Enum Name Text", ref UI.Current.ImGui_EnumName_Text, flags);
                ImGui.ColorEdit4("Enum Value Text", ref UI.Current.ImGui_EnumValue_Text, flags);
                ImGui.ColorEdit4("FMG Link Text", ref UI.Current.ImGui_FmgLink_Text, flags);
                ImGui.ColorEdit4("FMG Reference Text", ref UI.Current.ImGui_FmgRef_Text, flags);
                ImGui.ColorEdit4("FMG Reference Inactive Text", ref UI.Current.ImGui_FmgRefInactive_Text, flags);
                ImGui.ColorEdit4("Is Reference Text", ref UI.Current.ImGui_IsRef_Text, flags);
                ImGui.ColorEdit4("Virtual Reference Text", ref UI.Current.ImGui_VirtualRef_Text, flags);
                ImGui.ColorEdit4("Reference Text", ref UI.Current.ImGui_Ref_Text, flags);
                ImGui.ColorEdit4("Auxiliary Conflict Text", ref UI.Current.ImGui_AuxConflict_Text, flags);
                ImGui.ColorEdit4("Auxiliary Added Text", ref UI.Current.ImGui_AuxAdded_Text, flags);
                ImGui.ColorEdit4("Primary Changed Text", ref UI.Current.ImGui_PrimaryChanged_Text, flags);
                ImGui.ColorEdit4("Param Row Text", ref UI.Current.ImGui_ParamRow_Text, flags);
                ImGui.ColorEdit4("Aliased Name Text", ref UI.Current.ImGui_AliasName_Text, flags);

                ImGui.ColorEdit4("Text Editor: Modified Row", ref UI.Current.ImGui_TextEditor_ModifiedTextEntry_Text, flags);
                ImGui.ColorEdit4("Text Editor: Unique Row", ref UI.Current.ImGui_TextEditor_UniqueTextEntry_Text, flags);

                ImGui.ColorEdit4("Logger: Information", ref UI.Current.ImGui_Logger_Information_Color, flags);
                ImGui.ColorEdit4("Logger: Warning", ref UI.Current.ImGui_Logger_Warning_Color, flags);
                ImGui.ColorEdit4("Logger: Error", ref UI.Current.ImGui_Logger_Error_Color, flags);

                UIHelper.SimpleHeader("Miscellaneous", "");

                ImGui.ColorEdit4("Display Group: Border Highlight", ref UI.Current.DisplayGroupEditor_Border_Highlight, flags);
                ImGui.ColorEdit4("Display Group: Active Input Background", ref UI.Current.DisplayGroupEditor_DisplayActive_Frame, flags);
                ImGui.ColorEdit4("Display Group: Active Checkbox", ref UI.Current.DisplayGroupEditor_DisplayActive_Checkbox, flags);
                ImGui.ColorEdit4("Draw Group: Active Input Background", ref UI.Current.DisplayGroupEditor_DrawActive_Frame, flags);
                ImGui.ColorEdit4("Draw Group: Active Checkbox", ref UI.Current.DisplayGroupEditor_DrawActive_Checkbox, flags);
                ImGui.ColorEdit4("Combined Group: Active Input Background", ref UI.Current.DisplayGroupEditor_CombinedActive_Frame, flags);
                ImGui.ColorEdit4("Combined Group: Active Checkbox", ref UI.Current.DisplayGroupEditor_CombinedActive_Checkbox, flags);
            }
        };
    }
    #endregion
}
