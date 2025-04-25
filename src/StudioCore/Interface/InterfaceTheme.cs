using StudioCore.Banks.FormatBank;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Veldrid;
using System.IO;
using StudioCore.Banks.AliasBank;
using Google.Protobuf.WellKnownTypes;
using StudioCore.Configuration.Interface;
using System.Windows.Forms;

namespace StudioCore.Interface
{
    public static class InterfaceTheme
    {
        public static Dictionary<int, InterfaceThemeConfig> LoadedThemes { get; set; }
        public static string[] LoadedThemeNames { get; set; }

        public static void ResetInterface()
        {
            // Fixed Window
            UI.Current.ImGui_MainBg = UI.Default.ImGui_MainBg;
            UI.Current.ImGui_ChildBg = UI.Default.ImGui_ChildBg;
            UI.Current.ImGui_PopupBg = UI.Default.ImGui_PopupBg;
            UI.Current.ImGui_Border = UI.Default.ImGui_Border;
            UI.Current.ImGui_TitleBarBg = UI.Default.ImGui_TitleBarBg;
            UI.Current.ImGui_TitleBarBg_Active = UI.Default.ImGui_TitleBarBg_Active;
            UI.Current.ImGui_MenuBarBg = UI.Default.ImGui_MenuBarBg;

            // Moveable Window
            UI.Current.Imgui_Moveable_MainBg = UI.Default.Imgui_Moveable_MainBg;
            UI.Current.Imgui_Moveable_TitleBg = UI.Default.Imgui_Moveable_TitleBg;
            UI.Current.Imgui_Moveable_TitleBg_Active = UI.Default.Imgui_Moveable_TitleBg_Active;
            UI.Current.Imgui_Moveable_ChildBg = UI.Default.Imgui_Moveable_ChildBg;
            UI.Current.Imgui_Moveable_ChildBgSecondary = UI.Default.Imgui_Moveable_ChildBgSecondary;
            UI.Current.Imgui_Moveable_Header = UI.Default.Imgui_Moveable_Header;

            // Scroll
            UI.Current.ImGui_ScrollbarBg = UI.Default.ImGui_ScrollbarBg;
            UI.Current.ImGui_ScrollbarGrab = UI.Default.ImGui_ScrollbarGrab;
            UI.Current.ImGui_ScrollbarGrab_Hover = UI.Default.ImGui_ScrollbarGrab_Hover;
            UI.Current.ImGui_ScrollbarGrab_Active = UI.Default.ImGui_ScrollbarGrab_Active;
            UI.Current.ImGui_SliderGrab = UI.Default.ImGui_SliderGrab;
            UI.Current.ImGui_SliderGrab_Active = UI.Default.ImGui_SliderGrab_Active;

            // Tab
            UI.Current.ImGui_Tab = UI.Default.ImGui_Tab;
            UI.Current.ImGui_Tab_Hover = UI.Default.ImGui_Tab_Hover;
            UI.Current.ImGui_Tab_Active = UI.Default.ImGui_Tab_Active;
            UI.Current.ImGui_UnfocusedTab = UI.Default.ImGui_UnfocusedTab;
            UI.Current.ImGui_UnfocusedTab_Active = UI.Default.ImGui_UnfocusedTab_Active;

            // Button
            UI.Current.ImGui_Button = UI.Default.ImGui_Button;
            UI.Current.ImGui_Button_Hovered = UI.Default.ImGui_Button_Hovered;
            UI.Current.ImGui_ButtonActive = UI.Default.ImGui_ButtonActive;

            // Selection
            UI.Current.ImGui_Selection = UI.Default.ImGui_Selection;
            UI.Current.ImGui_Selection_Hover = UI.Default.ImGui_Selection_Hover;
            UI.Current.ImGui_Selection_Active = UI.Default.ImGui_Selection_Active;

            // Input
            UI.Current.ImGui_Input_Background = UI.Default.ImGui_Input_Background;
            UI.Current.ImGui_Input_Background_Hover = UI.Default.ImGui_Input_Background_Hover;
            UI.Current.ImGui_Input_Background_Active = UI.Default.ImGui_Input_Background_Active;
            UI.Current.ImGui_Input_CheckMark = UI.Default.ImGui_Input_CheckMark;
            UI.Current.ImGui_Input_Conflict_Background = UI.Default.ImGui_Input_Conflict_Background;
            UI.Current.ImGui_Input_Vanilla_Background = UI.Default.ImGui_Input_Vanilla_Background;
            UI.Current.ImGui_Input_Default_Background = UI.Default.ImGui_Input_Default_Background;
            UI.Current.ImGui_Input_AuxVanilla_Background = UI.Default.ImGui_Input_AuxVanilla_Background;
            UI.Current.ImGui_Input_DiffCompare_Background = UI.Default.ImGui_Input_DiffCompare_Background;

            // Text
            UI.Current.ImGui_Default_Text_Color = UI.Default.ImGui_Default_Text_Color;
            UI.Current.ImGui_Warning_Text_Color = UI.Default.ImGui_Warning_Text_Color;
            UI.Current.ImGui_Benefit_Text_Color = UI.Default.ImGui_Benefit_Text_Color;
            UI.Current.ImGui_Invalid_Text_Color = UI.Default.ImGui_Invalid_Text_Color;

            UI.Current.ImGui_TimeAct_InfoText_1_Color = UI.Default.ImGui_TimeAct_InfoText_1_Color;
            UI.Current.ImGui_TimeAct_InfoText_2_Color = UI.Default.ImGui_TimeAct_InfoText_2_Color;
            UI.Current.ImGui_TimeAct_InfoText_3_Color = UI.Default.ImGui_TimeAct_InfoText_3_Color;
            UI.Current.ImGui_TimeAct_InfoText_4_Color = UI.Default.ImGui_TimeAct_InfoText_4_Color;

            UI.Current.ImGui_ParamRef_Text = UI.Default.ImGui_ParamRef_Text;
            UI.Current.ImGui_ParamRefMissing_Text = UI.Default.ImGui_ParamRefMissing_Text;
            UI.Current.ImGui_ParamRefInactive_Text = UI.Default.ImGui_ParamRefInactive_Text;
            UI.Current.ImGui_EnumName_Text = UI.Default.ImGui_EnumName_Text;
            UI.Current.ImGui_EnumValue_Text = UI.Default.ImGui_EnumValue_Text;
            UI.Current.ImGui_FmgLink_Text = UI.Default.ImGui_FmgLink_Text;
            UI.Current.ImGui_FmgRef_Text = UI.Default.ImGui_FmgRef_Text;
            UI.Current.ImGui_FmgRefInactive_Text = UI.Default.ImGui_FmgRefInactive_Text;
            UI.Current.ImGui_IsRef_Text = UI.Default.ImGui_IsRef_Text;
            UI.Current.ImGui_VirtualRef_Text = UI.Default.ImGui_VirtualRef_Text;
            UI.Current.ImGui_Ref_Text = UI.Default.ImGui_Ref_Text;
            UI.Current.ImGui_AuxConflict_Text = UI.Default.ImGui_AuxConflict_Text;
            UI.Current.ImGui_AuxAdded_Text = UI.Default.ImGui_AuxAdded_Text;
            UI.Current.ImGui_PrimaryChanged_Text = UI.Default.ImGui_PrimaryChanged_Text;
            UI.Current.ImGui_ParamRow_Text = UI.Default.ImGui_ParamRow_Text;
            UI.Current.ImGui_AliasName_Text = UI.Default.ImGui_AliasName_Text;
            UI.Current.ImGui_TextEditor_ModifiedRow_Text = UI.Default.ImGui_TextEditor_ModifiedRow_Text;
            UI.Current.ImGui_TextEditor_UniqueRow_Text = UI.Default.ImGui_TextEditor_UniqueRow_Text;

            UI.Current.ImGui_Logger_Information_Color = UI.Default.ImGui_Logger_Information_Color;
            UI.Current.ImGui_Logger_Warning_Color = UI.Default.ImGui_Logger_Warning_Color;
            UI.Current.ImGui_Logger_Error_Color = UI.Default.ImGui_Logger_Error_Color;

            UI.Current.ImGui_Havok_Header = UI.Default.ImGui_Havok_Header;
            UI.Current.ImGui_Havok_Reference = UI.Default.ImGui_Havok_Reference;
            UI.Current.ImGui_Havok_Highlight = UI.Default.ImGui_Havok_Highlight;
            UI.Current.ImGui_Havok_Warning = UI.Default.ImGui_Havok_Warning;

            // Misc
            UI.Current.DisplayGroupEditor_Border_Highlight = UI.Default.DisplayGroupEditor_Border_Highlight;
            UI.Current.DisplayGroupEditor_DisplayActive_Frame = UI.Default.DisplayGroupEditor_DisplayActive_Frame;
            UI.Current.DisplayGroupEditor_DisplayActive_Checkbox = UI.Default.DisplayGroupEditor_DisplayActive_Checkbox;
            UI.Current.DisplayGroupEditor_DrawActive_Frame = UI.Default.DisplayGroupEditor_DrawActive_Frame;
            UI.Current.DisplayGroupEditor_DrawActive_Checkbox = UI.Default.DisplayGroupEditor_DrawActive_Checkbox;
            UI.Current.DisplayGroupEditor_CombinedActive_Frame = UI.Default.DisplayGroupEditor_CombinedActive_Frame;
            UI.Current.DisplayGroupEditor_CombinedActive_Checkbox = UI.Default.DisplayGroupEditor_CombinedActive_Checkbox;
        }

        public static void SetupThemes()
        {
            LoadedThemes = new Dictionary<int, InterfaceThemeConfig>();

            var files = Directory.GetFiles($"{AppContext.BaseDirectory}\\Assets\\Themes\\");
            int idx = 0;

            LoadedThemeNames = new string[files.Length];

            foreach (var file in files)
            {
                var jsonTup = LoadThemeJson(file);

                if (jsonTup.Item1 != -1 && jsonTup.Item2 != null)
                {
                    LoadedThemes.Add(jsonTup.Item1, jsonTup.Item2);
                    LoadedThemeNames[idx] = jsonTup.Item2.name;
                }

                ++idx;
            }
        }

        public static (int, InterfaceThemeConfig) LoadThemeJson(string path)
        {
            var jsonObj = new InterfaceThemeConfig();

            if (File.Exists(path))
            {
                using (var stream = File.OpenRead(path))
                {
                    jsonObj = JsonSerializer.Deserialize(stream, InterfaceThemeSerializationContext.Default.InterfaceThemeConfig);
                }

                return (jsonObj.id, jsonObj);
            }
            else
            {
                return (-1, null);
            }
        }

        public static void ExportThemeJson()
        {
            var fileName = UI.Current.NewThemeName;

            if (fileName == "")
            {
                MessageBox.Show("Filename cannot be blank.", "Warning", MessageBoxButtons.OK);
                return;
            }

            var path = $"{AppContext.BaseDirectory}\\Assets\\Themes\\{fileName}.json";

            if (File.Exists(path))
            {
                MessageBox.Show("Filename already exists.", "Warning", MessageBoxButtons.OK);
                return;
            }

            int newId = 0;

            foreach(var entry in LoadedThemes)
            {
                int id = entry.Value.id;
                if(id > newId)
                {
                    newId = id;
                }
            }
            newId = newId + 1;

            InterfaceThemeConfig theme = new InterfaceThemeConfig();
            theme.id = newId;
            theme.name = fileName;

            theme.ImGui_MainBg = InterfaceUtils.GetFloatList(UI.Current.ImGui_MainBg);
            theme.ImGui_ChildBg = InterfaceUtils.GetFloatList(UI.Current.ImGui_ChildBg);
            theme.ImGui_PopupBg = InterfaceUtils.GetFloatList(UI.Current.ImGui_PopupBg);
            theme.ImGui_Border = InterfaceUtils.GetFloatList(UI.Current.ImGui_Border);
            theme.ImGui_TitleBarBg = InterfaceUtils.GetFloatList(UI.Current.ImGui_TitleBarBg);
            theme.ImGui_TitleBarBg_Active = InterfaceUtils.GetFloatList(UI.Current.ImGui_TitleBarBg_Active);
            theme.ImGui_MenuBarBg = InterfaceUtils.GetFloatList(UI.Current.ImGui_MenuBarBg);

            theme.Imgui_Moveable_MainBg = InterfaceUtils.GetFloatList(UI.Current.Imgui_Moveable_MainBg);
            theme.Imgui_Moveable_ChildBg = InterfaceUtils.GetFloatList(UI.Current.Imgui_Moveable_ChildBg);
            theme.Imgui_Moveable_ChildBgSecondary = InterfaceUtils.GetFloatList(UI.Current.Imgui_Moveable_ChildBgSecondary);
            theme.Imgui_Moveable_TitleBg = InterfaceUtils.GetFloatList(UI.Current.Imgui_Moveable_TitleBg);
            theme.Imgui_Moveable_TitleBg_Active = InterfaceUtils.GetFloatList(UI.Current.Imgui_Moveable_TitleBg_Active);
            theme.Imgui_Moveable_Header = InterfaceUtils.GetFloatList(UI.Current.Imgui_Moveable_Header);

            theme.ImGui_ScrollbarBg = InterfaceUtils.GetFloatList(UI.Current.ImGui_ScrollbarBg);
            theme.ImGui_ScrollbarGrab = InterfaceUtils.GetFloatList(UI.Current.ImGui_ScrollbarGrab);
            theme.ImGui_ScrollbarGrab_Hover = InterfaceUtils.GetFloatList(UI.Current.ImGui_ScrollbarGrab_Hover);
            theme.ImGui_ScrollbarGrab_Active = InterfaceUtils.GetFloatList(UI.Current.ImGui_ScrollbarGrab_Active);
            theme.ImGui_SliderGrab = InterfaceUtils.GetFloatList(UI.Current.ImGui_SliderGrab);
            theme.ImGui_SliderGrab_Active = InterfaceUtils.GetFloatList(UI.Current.ImGui_SliderGrab_Active);

            theme.ImGui_Tab = InterfaceUtils.GetFloatList(UI.Current.ImGui_Tab);
            theme.ImGui_Tab_Hover = InterfaceUtils.GetFloatList(UI.Current.ImGui_Tab_Hover);
            theme.ImGui_Tab_Active = InterfaceUtils.GetFloatList(UI.Current.ImGui_Tab_Active);
            theme.ImGui_UnfocusedTab = InterfaceUtils.GetFloatList(UI.Current.ImGui_UnfocusedTab);
            theme.ImGui_UnfocusedTab_Active = InterfaceUtils.GetFloatList(UI.Current.ImGui_UnfocusedTab_Active);

            theme.ImGui_Button = InterfaceUtils.GetFloatList(UI.Current.ImGui_Button);
            theme.ImGui_Button_Hovered = InterfaceUtils.GetFloatList(UI.Current.ImGui_Button_Hovered);
            theme.ImGui_ButtonActive = InterfaceUtils.GetFloatList(UI.Current.ImGui_ButtonActive);

            theme.ImGui_Selection = InterfaceUtils.GetFloatList(UI.Current.ImGui_Selection);
            theme.ImGui_Selection_Hover = InterfaceUtils.GetFloatList(UI.Current.ImGui_Selection_Hover);
            theme.ImGui_Selection_Active = InterfaceUtils.GetFloatList(UI.Current.ImGui_Selection_Active);

            theme.ImGui_Input_Background = InterfaceUtils.GetFloatList(UI.Current.ImGui_Input_Background);
            theme.ImGui_Input_Background_Hover = InterfaceUtils.GetFloatList(UI.Current.ImGui_Input_Background_Hover);
            theme.ImGui_Input_Background_Active = InterfaceUtils.GetFloatList(UI.Current.ImGui_Input_Background_Active);
            theme.ImGui_Input_CheckMark = InterfaceUtils.GetFloatList(UI.Current.ImGui_Input_CheckMark);
            theme.ImGui_Input_Conflict_Background = InterfaceUtils.GetFloatList(UI.Current.ImGui_Input_Conflict_Background);
            theme.ImGui_Input_Vanilla_Background = InterfaceUtils.GetFloatList(UI.Current.ImGui_Input_Vanilla_Background);
            theme.ImGui_Input_Default_Background = InterfaceUtils.GetFloatList(UI.Current.ImGui_Input_Default_Background);
            theme.ImGui_Input_AuxVanilla_Background = InterfaceUtils.GetFloatList(UI.Current.ImGui_Input_AuxVanilla_Background);
            theme.ImGui_Input_DiffCompare_Background = InterfaceUtils.GetFloatList(UI.Current.ImGui_Input_DiffCompare_Background);
            theme.ImGui_MultipleInput_Background = InterfaceUtils.GetFloatList(UI.Current.ImGui_MultipleInput_Background);
            theme.ImGui_ErrorInput_Background = InterfaceUtils.GetFloatList(UI.Current.ImGui_ErrorInput_Background);

            theme.ImGui_Default_Text_Color = InterfaceUtils.GetFloatList(UI.Current.ImGui_Default_Text_Color);
            theme.ImGui_Warning_Text_Color = InterfaceUtils.GetFloatList(UI.Current.ImGui_Warning_Text_Color);
            theme.ImGui_Benefit_Text_Color = InterfaceUtils.GetFloatList(UI.Current.ImGui_Benefit_Text_Color);
            theme.ImGui_Invalid_Text_Color = InterfaceUtils.GetFloatList(UI.Current.ImGui_Invalid_Text_Color);

            theme.ImGui_TimeAct_InfoText_1_Color = InterfaceUtils.GetFloatList(UI.Current.ImGui_TimeAct_InfoText_1_Color);
            theme.ImGui_TimeAct_InfoText_2_Color = InterfaceUtils.GetFloatList(UI.Current.ImGui_TimeAct_InfoText_2_Color);
            theme.ImGui_TimeAct_InfoText_3_Color = InterfaceUtils.GetFloatList(UI.Current.ImGui_TimeAct_InfoText_3_Color);
            theme.ImGui_TimeAct_InfoText_4_Color = InterfaceUtils.GetFloatList(UI.Current.ImGui_TimeAct_InfoText_4_Color);

            theme.ImGui_ParamRef_Text = InterfaceUtils.GetFloatList(UI.Current.ImGui_ParamRef_Text);
            theme.ImGui_ParamRefMissing_Text = InterfaceUtils.GetFloatList(UI.Current.ImGui_ParamRefMissing_Text);
            theme.ImGui_ParamRefInactive_Text = InterfaceUtils.GetFloatList(UI.Current.ImGui_ParamRefInactive_Text);
            theme.ImGui_EnumName_Text = InterfaceUtils.GetFloatList(UI.Current.ImGui_EnumName_Text);
            theme.ImGui_EnumValue_Text = InterfaceUtils.GetFloatList(UI.Current.ImGui_EnumValue_Text);
            theme.ImGui_FmgLink_Text = InterfaceUtils.GetFloatList(UI.Current.ImGui_FmgLink_Text);
            theme.ImGui_FmgRef_Text = InterfaceUtils.GetFloatList(UI.Current.ImGui_FmgRef_Text);
            theme.ImGui_FmgRefInactive_Text = InterfaceUtils.GetFloatList(UI.Current.ImGui_FmgRefInactive_Text);
            theme.ImGui_IsRef_Text = InterfaceUtils.GetFloatList(UI.Current.ImGui_IsRef_Text);
            theme.ImGui_VirtualRef_Text = InterfaceUtils.GetFloatList(UI.Current.ImGui_VirtualRef_Text);
            theme.ImGui_Ref_Text = InterfaceUtils.GetFloatList(UI.Current.ImGui_Ref_Text);
            theme.ImGui_AuxConflict_Text = InterfaceUtils.GetFloatList(UI.Current.ImGui_AuxConflict_Text);
            theme.ImGui_AuxAdded_Text = InterfaceUtils.GetFloatList(UI.Current.ImGui_AuxAdded_Text);
            theme.ImGui_PrimaryChanged_Text = InterfaceUtils.GetFloatList(UI.Current.ImGui_PrimaryChanged_Text);
            theme.ImGui_ParamRow_Text = InterfaceUtils.GetFloatList(UI.Current.ImGui_ParamRow_Text);
            theme.ImGui_AliasName_Text = InterfaceUtils.GetFloatList(UI.Current.ImGui_AliasName_Text);
            theme.ImGui_TextEditor_ModifiedRow_Text = InterfaceUtils.GetFloatList(UI.Current.ImGui_TextEditor_ModifiedRow_Text);
            theme.ImGui_TextEditor_UniqueRow_Text = InterfaceUtils.GetFloatList(UI.Current.ImGui_TextEditor_UniqueRow_Text);

            theme.ImGui_Logger_Information_Color = InterfaceUtils.GetFloatList(UI.Current.ImGui_Logger_Information_Color);
            theme.ImGui_Logger_Warning_Color = InterfaceUtils.GetFloatList(UI.Current.ImGui_Logger_Warning_Color);
            theme.ImGui_Logger_Error_Color = InterfaceUtils.GetFloatList(UI.Current.ImGui_Logger_Error_Color);

            theme.ImGui_Havok_Header = InterfaceUtils.GetFloatList(UI.Current.ImGui_Havok_Header);
            theme.ImGui_Havok_Reference = InterfaceUtils.GetFloatList(UI.Current.ImGui_Havok_Reference);
            theme.ImGui_Havok_Highlight = InterfaceUtils.GetFloatList(UI.Current.ImGui_Havok_Highlight);
            theme.ImGui_Havok_Warning = InterfaceUtils.GetFloatList(UI.Current.ImGui_Havok_Warning);

            theme.DisplayGroupEditor_Border_Highlight = InterfaceUtils.GetFloatList(UI.Current.DisplayGroupEditor_Border_Highlight);
            theme.DisplayGroupEditor_DisplayActive_Frame = InterfaceUtils.GetFloatList(UI.Current.DisplayGroupEditor_DisplayActive_Frame);
            theme.DisplayGroupEditor_DisplayActive_Checkbox = InterfaceUtils.GetFloatList(UI.Current.DisplayGroupEditor_DisplayActive_Checkbox);
            theme.DisplayGroupEditor_DrawActive_Frame = InterfaceUtils.GetFloatList(UI.Current.DisplayGroupEditor_DrawActive_Frame);
            theme.DisplayGroupEditor_DrawActive_Checkbox = InterfaceUtils.GetFloatList(UI.Current.DisplayGroupEditor_DrawActive_Checkbox);
            theme.DisplayGroupEditor_CombinedActive_Frame = InterfaceUtils.GetFloatList(UI.Current.DisplayGroupEditor_CombinedActive_Frame);
            theme.DisplayGroupEditor_CombinedActive_Checkbox = InterfaceUtils.GetFloatList(UI.Current.DisplayGroupEditor_CombinedActive_Checkbox);

            if (!File.Exists(path))
            {
                string jsonString = JsonSerializer.Serialize(theme, typeof(InterfaceThemeConfig), InterfaceThemeSerializationContext.Default);

                var filename = Path.GetFileNameWithoutExtension(path);

                try
                {
                    var fs = new FileStream(path, System.IO.FileMode.Create);
                    var data = Encoding.ASCII.GetBytes(jsonString);
                    fs.Write(data, 0, data.Length);
                    fs.Flush();
                    fs.Dispose();

                    TaskLogs.AddLog($"UI: saved Theme resource file: {filename} at {path}");
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"UI: failed to save Theme resource file: {filename} at {path}\n{ex}");
                }
            }

            MessageBox.Show("Theme saved.", "Information", MessageBoxButtons.OK);

            // Update the theme
            SetupThemes();
        }

        public static void SetTheme(bool ignoreId)
        {
            var ThemeName = UI.Current.SelectedThemeName;

            if (!ignoreId)
            {
                ThemeName = LoadedThemeNames[UI.Current.SelectedTheme];
            }

            InterfaceThemeConfig theme = null;

            foreach(var entry in LoadedThemes)
            {
                if(entry.Value.name == ThemeName)
                {
                    theme = entry.Value;
                }
            }

            if(theme == null)
            {
                // Reset to Dark if theme is null
                UI.Current.SelectedThemeName = "Dark";
                //MessageBox.Show("Theme does not exist.", "Warning", MessageBoxButtons.OK);
                return;
            }

            UI.Current.SelectedThemeName = theme.name;

            // Update CFG vars with values from json

            // Fixed Window
            UI.Current.ImGui_MainBg = InterfaceUtils.GetVectorValue(theme.ImGui_MainBg);
            UI.Current.ImGui_ChildBg = InterfaceUtils.GetVectorValue(theme.ImGui_ChildBg);
            UI.Current.ImGui_PopupBg = InterfaceUtils.GetVectorValue(theme.ImGui_PopupBg);
            UI.Current.ImGui_Border = InterfaceUtils.GetVectorValue(theme.ImGui_Border);
            UI.Current.ImGui_TitleBarBg = InterfaceUtils.GetVectorValue(theme.ImGui_TitleBarBg);
            UI.Current.ImGui_TitleBarBg_Active = InterfaceUtils.GetVectorValue(theme.ImGui_TitleBarBg_Active);
            UI.Current.ImGui_MenuBarBg = InterfaceUtils.GetVectorValue(theme.ImGui_MenuBarBg);

            // Moveable Window
            UI.Current.Imgui_Moveable_MainBg = InterfaceUtils.GetVectorValue(theme.Imgui_Moveable_MainBg);
            UI.Current.Imgui_Moveable_TitleBg = InterfaceUtils.GetVectorValue(theme.Imgui_Moveable_TitleBg);
            UI.Current.Imgui_Moveable_TitleBg_Active = InterfaceUtils.GetVectorValue(theme.Imgui_Moveable_TitleBg_Active);
            UI.Current.Imgui_Moveable_ChildBg = InterfaceUtils.GetVectorValue(theme.Imgui_Moveable_ChildBg);
            UI.Current.Imgui_Moveable_ChildBgSecondary = InterfaceUtils.GetVectorValue(theme.Imgui_Moveable_ChildBgSecondary);
            UI.Current.Imgui_Moveable_Header = InterfaceUtils.GetVectorValue(theme.Imgui_Moveable_Header);

            // Scroll
            UI.Current.ImGui_ScrollbarBg = InterfaceUtils.GetVectorValue(theme.ImGui_ScrollbarBg);
            UI.Current.ImGui_ScrollbarGrab = InterfaceUtils.GetVectorValue(theme.ImGui_ScrollbarGrab);
            UI.Current.ImGui_ScrollbarGrab_Hover = InterfaceUtils.GetVectorValue(theme.ImGui_ScrollbarGrab_Hover);
            UI.Current.ImGui_ScrollbarGrab_Active = InterfaceUtils.GetVectorValue(theme.ImGui_ScrollbarGrab_Active);
            UI.Current.ImGui_SliderGrab = InterfaceUtils.GetVectorValue(theme.ImGui_SliderGrab);
            UI.Current.ImGui_SliderGrab_Active = InterfaceUtils.GetVectorValue(theme.ImGui_SliderGrab_Active);

            // Tab
            UI.Current.ImGui_Tab = InterfaceUtils.GetVectorValue(theme.ImGui_Tab);
            UI.Current.ImGui_Tab_Hover = InterfaceUtils.GetVectorValue(theme.ImGui_Tab_Hover);
            UI.Current.ImGui_Tab_Active = InterfaceUtils.GetVectorValue(theme.ImGui_Tab_Active);
            UI.Current.ImGui_UnfocusedTab = InterfaceUtils.GetVectorValue(theme.ImGui_UnfocusedTab);
            UI.Current.ImGui_UnfocusedTab_Active = InterfaceUtils.GetVectorValue(theme.ImGui_UnfocusedTab_Active);

            // Button
            UI.Current.ImGui_Button = InterfaceUtils.GetVectorValue(theme.ImGui_Button);
            UI.Current.ImGui_Button_Hovered = InterfaceUtils.GetVectorValue(theme.ImGui_Button_Hovered);
            UI.Current.ImGui_ButtonActive = InterfaceUtils.GetVectorValue(theme.ImGui_ButtonActive);

            // Selection
            UI.Current.ImGui_Selection = InterfaceUtils.GetVectorValue(theme.ImGui_Selection);
            UI.Current.ImGui_Selection_Hover = InterfaceUtils.GetVectorValue(theme.ImGui_Selection_Hover);
            UI.Current.ImGui_Selection_Active = InterfaceUtils.GetVectorValue(theme.ImGui_Selection_Active);

            // Input
            UI.Current.ImGui_Input_Background = InterfaceUtils.GetVectorValue(theme.ImGui_Input_Background);
            UI.Current.ImGui_Input_Background_Hover = InterfaceUtils.GetVectorValue(theme.ImGui_Input_Background_Hover);
            UI.Current.ImGui_Input_Background_Active = InterfaceUtils.GetVectorValue(theme.ImGui_Input_Background_Active);
            UI.Current.ImGui_Input_CheckMark = InterfaceUtils.GetVectorValue(theme.ImGui_Input_CheckMark);
            UI.Current.ImGui_Input_Conflict_Background = InterfaceUtils.GetVectorValue(theme.ImGui_Input_Conflict_Background);
            UI.Current.ImGui_Input_Vanilla_Background = InterfaceUtils.GetVectorValue(theme.ImGui_Input_Vanilla_Background);
            UI.Current.ImGui_Input_Default_Background = InterfaceUtils.GetVectorValue(theme.ImGui_Input_Default_Background);
            UI.Current.ImGui_Input_AuxVanilla_Background = InterfaceUtils.GetVectorValue(theme.ImGui_Input_AuxVanilla_Background);
            UI.Current.ImGui_Input_DiffCompare_Background = InterfaceUtils.GetVectorValue(theme.ImGui_Input_DiffCompare_Background);

            // Text
            UI.Current.ImGui_Default_Text_Color = InterfaceUtils.GetVectorValue(theme.ImGui_Default_Text_Color);
            UI.Current.ImGui_Warning_Text_Color = InterfaceUtils.GetVectorValue(theme.ImGui_Warning_Text_Color);
            UI.Current.ImGui_Benefit_Text_Color = InterfaceUtils.GetVectorValue(theme.ImGui_Benefit_Text_Color);
            UI.Current.ImGui_Invalid_Text_Color = InterfaceUtils.GetVectorValue(theme.ImGui_Invalid_Text_Color);

            UI.Current.ImGui_TimeAct_InfoText_1_Color = InterfaceUtils.GetVectorValue(theme.ImGui_TimeAct_InfoText_1_Color);
            UI.Current.ImGui_TimeAct_InfoText_2_Color = InterfaceUtils.GetVectorValue(theme.ImGui_TimeAct_InfoText_2_Color);
            UI.Current.ImGui_TimeAct_InfoText_3_Color = InterfaceUtils.GetVectorValue(theme.ImGui_TimeAct_InfoText_3_Color);
            UI.Current.ImGui_TimeAct_InfoText_4_Color = InterfaceUtils.GetVectorValue(theme.ImGui_TimeAct_InfoText_4_Color);

            UI.Current.ImGui_ParamRef_Text = InterfaceUtils.GetVectorValue(theme.ImGui_ParamRef_Text);
            UI.Current.ImGui_ParamRefMissing_Text = InterfaceUtils.GetVectorValue(theme.ImGui_ParamRefMissing_Text);
            UI.Current.ImGui_ParamRefInactive_Text = InterfaceUtils.GetVectorValue(theme.ImGui_ParamRefInactive_Text);
            UI.Current.ImGui_EnumName_Text = InterfaceUtils.GetVectorValue(theme.ImGui_EnumName_Text);
            UI.Current.ImGui_EnumValue_Text = InterfaceUtils.GetVectorValue(theme.ImGui_EnumValue_Text);
            UI.Current.ImGui_FmgLink_Text = InterfaceUtils.GetVectorValue(theme.ImGui_FmgLink_Text);
            UI.Current.ImGui_FmgRef_Text = InterfaceUtils.GetVectorValue(theme.ImGui_FmgRef_Text);
            UI.Current.ImGui_FmgRefInactive_Text = InterfaceUtils.GetVectorValue(theme.ImGui_FmgRefInactive_Text);
            UI.Current.ImGui_IsRef_Text = InterfaceUtils.GetVectorValue(theme.ImGui_IsRef_Text);
            UI.Current.ImGui_VirtualRef_Text = InterfaceUtils.GetVectorValue(theme.ImGui_VirtualRef_Text);
            UI.Current.ImGui_Ref_Text = InterfaceUtils.GetVectorValue(theme.ImGui_Ref_Text);
            UI.Current.ImGui_AuxConflict_Text = InterfaceUtils.GetVectorValue(theme.ImGui_AuxConflict_Text);
            UI.Current.ImGui_AuxAdded_Text = InterfaceUtils.GetVectorValue(theme.ImGui_AuxAdded_Text);
            UI.Current.ImGui_PrimaryChanged_Text = InterfaceUtils.GetVectorValue(theme.ImGui_PrimaryChanged_Text);
            UI.Current.ImGui_ParamRow_Text = InterfaceUtils.GetVectorValue(theme.ImGui_ParamRow_Text);
            UI.Current.ImGui_AliasName_Text = InterfaceUtils.GetVectorValue(theme.ImGui_AliasName_Text);
            UI.Current.ImGui_TextEditor_ModifiedRow_Text = InterfaceUtils.GetVectorValue(theme.ImGui_TextEditor_ModifiedRow_Text);
            UI.Current.ImGui_TextEditor_UniqueRow_Text = InterfaceUtils.GetVectorValue(theme.ImGui_TextEditor_UniqueRow_Text);

            UI.Current.ImGui_Logger_Information_Color = InterfaceUtils.GetVectorValue(theme.ImGui_Logger_Information_Color);
            UI.Current.ImGui_Logger_Warning_Color = InterfaceUtils.GetVectorValue(theme.ImGui_Logger_Warning_Color);
            UI.Current.ImGui_Logger_Error_Color = InterfaceUtils.GetVectorValue(theme.ImGui_Logger_Error_Color);

            UI.Current.ImGui_Havok_Header = InterfaceUtils.GetVectorValue(theme.ImGui_Havok_Header);
            UI.Current.ImGui_Havok_Reference = InterfaceUtils.GetVectorValue(theme.ImGui_Havok_Reference);
            UI.Current.ImGui_Havok_Highlight = InterfaceUtils.GetVectorValue(theme.ImGui_Havok_Highlight);
            UI.Current.ImGui_Havok_Warning = InterfaceUtils.GetVectorValue(theme.ImGui_Havok_Warning);

            // Misc
            UI.Current.DisplayGroupEditor_Border_Highlight = InterfaceUtils.GetVectorValue(theme.DisplayGroupEditor_Border_Highlight);
            UI.Current.DisplayGroupEditor_DisplayActive_Frame = InterfaceUtils.GetVectorValue(theme.DisplayGroupEditor_DisplayActive_Frame);
            UI.Current.DisplayGroupEditor_DisplayActive_Checkbox = InterfaceUtils.GetVectorValue(theme.DisplayGroupEditor_DisplayActive_Checkbox);
            UI.Current.DisplayGroupEditor_DrawActive_Frame = InterfaceUtils.GetVectorValue(theme.DisplayGroupEditor_DrawActive_Frame);
            UI.Current.DisplayGroupEditor_DrawActive_Checkbox = InterfaceUtils.GetVectorValue(theme.DisplayGroupEditor_DrawActive_Checkbox);
            UI.Current.DisplayGroupEditor_CombinedActive_Frame = InterfaceUtils.GetVectorValue(theme.DisplayGroupEditor_CombinedActive_Frame);
            UI.Current.DisplayGroupEditor_CombinedActive_Checkbox = InterfaceUtils.GetVectorValue(theme.DisplayGroupEditor_CombinedActive_Checkbox);
        }

        
    }
}
