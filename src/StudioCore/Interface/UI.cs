using StudioCore.Banks.FormatBank;
using StudioCore.Platform;
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

namespace StudioCore.Interface
{
    public static class UI
    {
        public static Dictionary<int, InterfaceTheme> LoadedThemes { get; set; }
        public static string[] LoadedThemeNames { get; set; }

        public static void ResetInterface()
        {
            // Fixed Window
            CFG.Current.ImGui_MainBg = CFG.Default.ImGui_MainBg;
            CFG.Current.ImGui_ChildBg = CFG.Default.ImGui_ChildBg;
            CFG.Current.ImGui_PopupBg = CFG.Default.ImGui_PopupBg;
            CFG.Current.ImGui_Border = CFG.Default.ImGui_Border;
            CFG.Current.ImGui_TitleBarBg = CFG.Default.ImGui_TitleBarBg;
            CFG.Current.ImGui_TitleBarBg_Active = CFG.Default.ImGui_TitleBarBg_Active;
            CFG.Current.ImGui_MenuBarBg = CFG.Default.ImGui_MenuBarBg;

            // Moveable Window
            CFG.Current.Imgui_Moveable_MainBg = CFG.Default.Imgui_Moveable_MainBg;
            CFG.Current.Imgui_Moveable_TitleBg = CFG.Default.Imgui_Moveable_TitleBg;
            CFG.Current.Imgui_Moveable_TitleBg_Active = CFG.Default.Imgui_Moveable_TitleBg_Active;
            CFG.Current.Imgui_Moveable_ChildBg = CFG.Default.Imgui_Moveable_ChildBg;
            CFG.Current.Imgui_Moveable_Header = CFG.Default.Imgui_Moveable_Header;

            // Scroll
            CFG.Current.ImGui_ScrollbarBg = CFG.Default.ImGui_ScrollbarBg;
            CFG.Current.ImGui_ScrollbarGrab = CFG.Default.ImGui_ScrollbarGrab;
            CFG.Current.ImGui_ScrollbarGrab_Hover = CFG.Default.ImGui_ScrollbarGrab_Hover;
            CFG.Current.ImGui_ScrollbarGrab_Active = CFG.Default.ImGui_ScrollbarGrab_Active;
            CFG.Current.ImGui_SliderGrab = CFG.Default.ImGui_SliderGrab;
            CFG.Current.ImGui_SliderGrab_Active = CFG.Default.ImGui_SliderGrab_Active;

            // Tab
            CFG.Current.ImGui_Tab = CFG.Default.ImGui_Tab;
            CFG.Current.ImGui_Tab_Hover = CFG.Default.ImGui_Tab_Hover;
            CFG.Current.ImGui_Tab_Active = CFG.Default.ImGui_Tab_Active;
            CFG.Current.ImGui_UnfocusedTab = CFG.Default.ImGui_UnfocusedTab;
            CFG.Current.ImGui_UnfocusedTab_Active = CFG.Default.ImGui_UnfocusedTab_Active;

            // Button
            CFG.Current.ImGui_Button = CFG.Default.ImGui_Button;
            CFG.Current.ImGui_Button_Hovered = CFG.Default.ImGui_Button_Hovered;
            CFG.Current.ImGui_ButtonActive = CFG.Default.ImGui_ButtonActive;

            // Selection
            CFG.Current.ImGui_Selection = CFG.Default.ImGui_Selection;
            CFG.Current.ImGui_Selection_Hover = CFG.Default.ImGui_Selection_Hover;
            CFG.Current.ImGui_Selection_Active = CFG.Default.ImGui_Selection_Active;

            // Input
            CFG.Current.ImGui_Input_Background = CFG.Default.ImGui_Input_Background;
            CFG.Current.ImGui_Input_Background_Hover = CFG.Default.ImGui_Input_Background_Hover;
            CFG.Current.ImGui_Input_Background_Active = CFG.Default.ImGui_Input_Background_Active;
            CFG.Current.ImGui_Input_CheckMark = CFG.Default.ImGui_Input_CheckMark;
            CFG.Current.ImGui_Input_Conflict_Background = CFG.Default.ImGui_Input_Conflict_Background;
            CFG.Current.ImGui_Input_Vanilla_Background = CFG.Default.ImGui_Input_Vanilla_Background;
            CFG.Current.ImGui_Input_Default_Background = CFG.Default.ImGui_Input_Default_Background;
            CFG.Current.ImGui_Input_AuxVanilla_Background = CFG.Default.ImGui_Input_AuxVanilla_Background;
            CFG.Current.ImGui_Input_DiffCompare_Background = CFG.Default.ImGui_Input_DiffCompare_Background;

            // Text
            CFG.Current.ImGui_Default_Text_Color = CFG.Default.ImGui_Default_Text_Color;
            CFG.Current.ImGui_Warning_Text_Color = CFG.Default.ImGui_Warning_Text_Color;
            CFG.Current.ImGui_Benefit_Text_Color = CFG.Default.ImGui_Benefit_Text_Color;
            CFG.Current.ImGui_Invalid_Text_Color = CFG.Default.ImGui_Invalid_Text_Color;
            CFG.Current.ImGui_ParamRef_Text = CFG.Default.ImGui_ParamRef_Text;
            CFG.Current.ImGui_ParamRefMissing_Text = CFG.Default.ImGui_ParamRefMissing_Text;
            CFG.Current.ImGui_ParamRefInactive_Text = CFG.Default.ImGui_ParamRefInactive_Text;
            CFG.Current.ImGui_EnumName_Text = CFG.Default.ImGui_EnumName_Text;
            CFG.Current.ImGui_EnumValue_Text = CFG.Default.ImGui_EnumValue_Text;
            CFG.Current.ImGui_FmgLink_Text = CFG.Default.ImGui_FmgLink_Text;
            CFG.Current.ImGui_FmgRef_Text = CFG.Default.ImGui_FmgRef_Text;
            CFG.Current.ImGui_FmgRefInactive_Text = CFG.Default.ImGui_FmgRefInactive_Text;
            CFG.Current.ImGui_IsRef_Text = CFG.Default.ImGui_IsRef_Text;
            CFG.Current.ImGui_VirtualRef_Text = CFG.Default.ImGui_VirtualRef_Text;
            CFG.Current.ImGui_Ref_Text = CFG.Default.ImGui_Ref_Text;
            CFG.Current.ImGui_AuxConflict_Text = CFG.Default.ImGui_AuxConflict_Text;
            CFG.Current.ImGui_AuxAdded_Text = CFG.Default.ImGui_AuxAdded_Text;
            CFG.Current.ImGui_PrimaryChanged_Text = CFG.Default.ImGui_PrimaryChanged_Text;
            CFG.Current.ImGui_ParamRow_Text = CFG.Default.ImGui_ParamRow_Text;
            CFG.Current.ImGui_AliasName_Text = CFG.Default.ImGui_AliasName_Text;

            // Misc
            CFG.Current.DisplayGroupEditor_Border_Highlight = CFG.Default.DisplayGroupEditor_Border_Highlight;
            CFG.Current.DisplayGroupEditor_DisplayActive_Frame = CFG.Default.DisplayGroupEditor_DisplayActive_Frame;
            CFG.Current.DisplayGroupEditor_DisplayActive_Checkbox = CFG.Default.DisplayGroupEditor_DisplayActive_Checkbox;
            CFG.Current.DisplayGroupEditor_DrawActive_Frame = CFG.Default.DisplayGroupEditor_DrawActive_Frame;
            CFG.Current.DisplayGroupEditor_DrawActive_Checkbox = CFG.Default.DisplayGroupEditor_DrawActive_Checkbox;
            CFG.Current.DisplayGroupEditor_CombinedActive_Frame = CFG.Default.DisplayGroupEditor_CombinedActive_Frame;
            CFG.Current.DisplayGroupEditor_CombinedActive_Checkbox = CFG.Default.DisplayGroupEditor_CombinedActive_Checkbox;
        }

        public static void SetupThemes()
        {
            LoadedThemes = new Dictionary<int, InterfaceTheme>();

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

        public static (int, InterfaceTheme) LoadThemeJson(string path)
        {
            var jsonObj = new InterfaceTheme();

            if (File.Exists(path))
            {
                using (var stream = File.OpenRead(path))
                {
                    jsonObj = JsonSerializer.Deserialize(stream, InterfaceThemeSerializationContext.Default.InterfaceTheme);
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
            var fileName = CFG.Current.NewThemeName;

            if (fileName == "")
            {
                PlatformUtils.Instance.MessageBox("Filename cannot be blank.", "Warning", MessageBoxButtons.OK);
                return;
            }

            var path = $"{AppContext.BaseDirectory}\\Assets\\Themes\\{fileName}.json";

            if (File.Exists(path))
            {
                PlatformUtils.Instance.MessageBox("Filename already exists.", "Warning", MessageBoxButtons.OK);
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

            InterfaceTheme theme = new InterfaceTheme();
            theme.id = newId;
            theme.name = fileName;

            theme.ImGui_MainBg = GetFloatList(CFG.Current.ImGui_MainBg);
            theme.ImGui_ChildBg = GetFloatList(CFG.Current.ImGui_ChildBg);
            theme.ImGui_PopupBg = GetFloatList(CFG.Current.ImGui_PopupBg);
            theme.ImGui_Border = GetFloatList(CFG.Current.ImGui_Border);
            theme.ImGui_TitleBarBg = GetFloatList(CFG.Current.ImGui_TitleBarBg);
            theme.ImGui_TitleBarBg_Active = GetFloatList(CFG.Current.ImGui_TitleBarBg_Active);
            theme.ImGui_MenuBarBg = GetFloatList(CFG.Current.ImGui_MenuBarBg);

            theme.Imgui_Moveable_MainBg = GetFloatList(CFG.Current.Imgui_Moveable_MainBg);
            theme.Imgui_Moveable_ChildBg = GetFloatList(CFG.Current.Imgui_Moveable_ChildBg);
            theme.Imgui_Moveable_TitleBg = GetFloatList(CFG.Current.Imgui_Moveable_TitleBg);
            theme.Imgui_Moveable_TitleBg_Active = GetFloatList(CFG.Current.Imgui_Moveable_TitleBg_Active);
            theme.Imgui_Moveable_Header = GetFloatList(CFG.Current.Imgui_Moveable_Header);

            theme.ImGui_ScrollbarBg = GetFloatList(CFG.Current.ImGui_ScrollbarBg);
            theme.ImGui_ScrollbarGrab = GetFloatList(CFG.Current.ImGui_ScrollbarGrab);
            theme.ImGui_ScrollbarGrab_Hover = GetFloatList(CFG.Current.ImGui_ScrollbarGrab_Hover);
            theme.ImGui_ScrollbarGrab_Active = GetFloatList(CFG.Current.ImGui_ScrollbarGrab_Active);
            theme.ImGui_SliderGrab = GetFloatList(CFG.Current.ImGui_SliderGrab);
            theme.ImGui_SliderGrab_Active = GetFloatList(CFG.Current.ImGui_SliderGrab_Active);

            theme.ImGui_Tab = GetFloatList(CFG.Current.ImGui_Tab);
            theme.ImGui_Tab_Hover = GetFloatList(CFG.Current.ImGui_Tab_Hover);
            theme.ImGui_Tab_Active = GetFloatList(CFG.Current.ImGui_Tab_Active);
            theme.ImGui_UnfocusedTab = GetFloatList(CFG.Current.ImGui_UnfocusedTab);
            theme.ImGui_UnfocusedTab_Active = GetFloatList(CFG.Current.ImGui_UnfocusedTab_Active);

            theme.ImGui_Button = GetFloatList(CFG.Current.ImGui_Button);
            theme.ImGui_Button_Hovered = GetFloatList(CFG.Current.ImGui_Button_Hovered);
            theme.ImGui_ButtonActive = GetFloatList(CFG.Current.ImGui_ButtonActive);

            theme.ImGui_Selection = GetFloatList(CFG.Current.ImGui_Selection);
            theme.ImGui_Selection_Hover = GetFloatList(CFG.Current.ImGui_Selection_Hover);
            theme.ImGui_Selection_Active = GetFloatList(CFG.Current.ImGui_Selection_Active);

            theme.ImGui_Input_Background = GetFloatList(CFG.Current.ImGui_Input_Background);
            theme.ImGui_Input_Background_Hover = GetFloatList(CFG.Current.ImGui_Input_Background_Hover);
            theme.ImGui_Input_Background_Active = GetFloatList(CFG.Current.ImGui_Input_Background_Active);
            theme.ImGui_Input_CheckMark = GetFloatList(CFG.Current.ImGui_Input_CheckMark);
            theme.ImGui_Input_Conflict_Background = GetFloatList(CFG.Current.ImGui_Input_Conflict_Background);
            theme.ImGui_Input_Vanilla_Background = GetFloatList(CFG.Current.ImGui_Input_Vanilla_Background);
            theme.ImGui_Input_Default_Background = GetFloatList(CFG.Current.ImGui_Input_Default_Background);
            theme.ImGui_Input_AuxVanilla_Background = GetFloatList(CFG.Current.ImGui_Input_AuxVanilla_Background);
            theme.ImGui_Input_DiffCompare_Background = GetFloatList(CFG.Current.ImGui_Input_DiffCompare_Background);
            theme.ImGui_MultipleInput_Background = GetFloatList(CFG.Current.ImGui_MultipleInput_Background);
            theme.ImGui_ErrorInput_Background = GetFloatList(CFG.Current.ImGui_ErrorInput_Background);

            theme.ImGui_Default_Text_Color = GetFloatList(CFG.Current.ImGui_Default_Text_Color);
            theme.ImGui_Warning_Text_Color = GetFloatList(CFG.Current.ImGui_Warning_Text_Color);
            theme.ImGui_Benefit_Text_Color = GetFloatList(CFG.Current.ImGui_Benefit_Text_Color);
            theme.ImGui_Invalid_Text_Color = GetFloatList(CFG.Current.ImGui_Invalid_Text_Color);
            theme.ImGui_ParamRef_Text = GetFloatList(CFG.Current.ImGui_ParamRef_Text);
            theme.ImGui_ParamRefMissing_Text = GetFloatList(CFG.Current.ImGui_ParamRefMissing_Text);
            theme.ImGui_ParamRefInactive_Text = GetFloatList(CFG.Current.ImGui_ParamRefInactive_Text);
            theme.ImGui_EnumName_Text = GetFloatList(CFG.Current.ImGui_EnumName_Text);
            theme.ImGui_EnumValue_Text = GetFloatList(CFG.Current.ImGui_EnumValue_Text);
            theme.ImGui_FmgLink_Text = GetFloatList(CFG.Current.ImGui_FmgLink_Text);
            theme.ImGui_FmgRef_Text = GetFloatList(CFG.Current.ImGui_FmgRef_Text);
            theme.ImGui_FmgRefInactive_Text = GetFloatList(CFG.Current.ImGui_FmgRefInactive_Text);
            theme.ImGui_IsRef_Text = GetFloatList(CFG.Current.ImGui_IsRef_Text);
            theme.ImGui_VirtualRef_Text = GetFloatList(CFG.Current.ImGui_VirtualRef_Text);
            theme.ImGui_Ref_Text = GetFloatList(CFG.Current.ImGui_Ref_Text);
            theme.ImGui_AuxConflict_Text = GetFloatList(CFG.Current.ImGui_AuxConflict_Text);
            theme.ImGui_AuxAdded_Text = GetFloatList(CFG.Current.ImGui_AuxAdded_Text);
            theme.ImGui_PrimaryChanged_Text = GetFloatList(CFG.Current.ImGui_PrimaryChanged_Text);
            theme.ImGui_ParamRow_Text = GetFloatList(CFG.Current.ImGui_ParamRow_Text);
            theme.ImGui_AliasName_Text = GetFloatList(CFG.Current.ImGui_AliasName_Text);

            theme.DisplayGroupEditor_Border_Highlight = GetFloatList(CFG.Current.DisplayGroupEditor_Border_Highlight);
            theme.DisplayGroupEditor_DisplayActive_Frame = GetFloatList(CFG.Current.DisplayGroupEditor_DisplayActive_Frame);
            theme.DisplayGroupEditor_DisplayActive_Checkbox = GetFloatList(CFG.Current.DisplayGroupEditor_DisplayActive_Checkbox);
            theme.DisplayGroupEditor_DrawActive_Frame = GetFloatList(CFG.Current.DisplayGroupEditor_DrawActive_Frame);
            theme.DisplayGroupEditor_DrawActive_Checkbox = GetFloatList(CFG.Current.DisplayGroupEditor_DrawActive_Checkbox);
            theme.DisplayGroupEditor_CombinedActive_Frame = GetFloatList(CFG.Current.DisplayGroupEditor_CombinedActive_Frame);
            theme.DisplayGroupEditor_CombinedActive_Checkbox = GetFloatList(CFG.Current.DisplayGroupEditor_CombinedActive_Checkbox);

            if (!File.Exists(path))
            {
                string jsonString = JsonSerializer.Serialize(theme, typeof(InterfaceTheme), InterfaceThemeSerializationContext.Default);

                try
                {
                    var fs = new FileStream(path, System.IO.FileMode.Create);
                    var data = Encoding.ASCII.GetBytes(jsonString);
                    fs.Write(data, 0, data.Length);
                    fs.Flush();
                    fs.Dispose();
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"{ex}");
                }
            }

            PlatformUtils.Instance.MessageBox("Theme saved.", "Information", MessageBoxButtons.OK);

            // Update the theme
            SetupThemes();
        }

        public static void SetTheme(bool ignoreId)
        {
            var ThemeName = CFG.Current.SelectedThemeName;

            if (!ignoreId)
            {
                ThemeName = LoadedThemeNames[CFG.Current.SelectedTheme];
            }

            InterfaceTheme theme = null;

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
                CFG.Current.SelectedThemeName = "Dark";
                //PlatformUtils.Instance.MessageBox("Theme does not exist.", "Warning", MessageBoxButtons.OK);
                return;
            }

            CFG.Current.SelectedThemeName = theme.name;

            // Update CFG vars with values from json

            // Fixed Window
            CFG.Current.ImGui_MainBg = GetVectorValue(theme.ImGui_MainBg);
            CFG.Current.ImGui_ChildBg = GetVectorValue(theme.ImGui_ChildBg);
            CFG.Current.ImGui_PopupBg = GetVectorValue(theme.ImGui_PopupBg);
            CFG.Current.ImGui_Border = GetVectorValue(theme.ImGui_Border);
            CFG.Current.ImGui_TitleBarBg = GetVectorValue(theme.ImGui_TitleBarBg);
            CFG.Current.ImGui_TitleBarBg_Active = GetVectorValue(theme.ImGui_TitleBarBg_Active);
            CFG.Current.ImGui_MenuBarBg = GetVectorValue(theme.ImGui_MenuBarBg);

            // Moveable Window
            CFG.Current.Imgui_Moveable_MainBg = GetVectorValue(theme.Imgui_Moveable_MainBg);
            CFG.Current.Imgui_Moveable_TitleBg = GetVectorValue(theme.Imgui_Moveable_TitleBg);
            CFG.Current.Imgui_Moveable_TitleBg_Active = GetVectorValue(theme.Imgui_Moveable_TitleBg_Active);
            CFG.Current.Imgui_Moveable_ChildBg = GetVectorValue(theme.Imgui_Moveable_ChildBg);
            CFG.Current.Imgui_Moveable_Header = GetVectorValue(theme.Imgui_Moveable_Header);

            // Scroll
            CFG.Current.ImGui_ScrollbarBg = GetVectorValue(theme.ImGui_ScrollbarBg);
            CFG.Current.ImGui_ScrollbarGrab = GetVectorValue(theme.ImGui_ScrollbarGrab);
            CFG.Current.ImGui_ScrollbarGrab_Hover = GetVectorValue(theme.ImGui_ScrollbarGrab_Hover);
            CFG.Current.ImGui_ScrollbarGrab_Active = GetVectorValue(theme.ImGui_ScrollbarGrab_Active);
            CFG.Current.ImGui_SliderGrab = GetVectorValue(theme.ImGui_SliderGrab);
            CFG.Current.ImGui_SliderGrab_Active = GetVectorValue(theme.ImGui_SliderGrab_Active);

            // Tab
            CFG.Current.ImGui_Tab = GetVectorValue(theme.ImGui_Tab);
            CFG.Current.ImGui_Tab_Hover = GetVectorValue(theme.ImGui_Tab_Hover);
            CFG.Current.ImGui_Tab_Active = GetVectorValue(theme.ImGui_Tab_Active);
            CFG.Current.ImGui_UnfocusedTab = GetVectorValue(theme.ImGui_UnfocusedTab);
            CFG.Current.ImGui_UnfocusedTab_Active = GetVectorValue(theme.ImGui_UnfocusedTab_Active);

            // Button
            CFG.Current.ImGui_Button = GetVectorValue(theme.ImGui_Button);
            CFG.Current.ImGui_Button_Hovered = GetVectorValue(theme.ImGui_Button_Hovered);
            CFG.Current.ImGui_ButtonActive = GetVectorValue(theme.ImGui_ButtonActive);

            // Selection
            CFG.Current.ImGui_Selection = GetVectorValue(theme.ImGui_Selection);
            CFG.Current.ImGui_Selection_Hover = GetVectorValue(theme.ImGui_Selection_Hover);
            CFG.Current.ImGui_Selection_Active = GetVectorValue(theme.ImGui_Selection_Active);

            // Input
            CFG.Current.ImGui_Input_Background = GetVectorValue(theme.ImGui_Input_Background);
            CFG.Current.ImGui_Input_Background_Hover = GetVectorValue(theme.ImGui_Input_Background_Hover);
            CFG.Current.ImGui_Input_Background_Active = GetVectorValue(theme.ImGui_Input_Background_Active);
            CFG.Current.ImGui_Input_CheckMark = GetVectorValue(theme.ImGui_Input_CheckMark);
            CFG.Current.ImGui_Input_Conflict_Background = GetVectorValue(theme.ImGui_Input_Conflict_Background);
            CFG.Current.ImGui_Input_Vanilla_Background = GetVectorValue(theme.ImGui_Input_Vanilla_Background);
            CFG.Current.ImGui_Input_Default_Background = GetVectorValue(theme.ImGui_Input_Default_Background);
            CFG.Current.ImGui_Input_AuxVanilla_Background = GetVectorValue(theme.ImGui_Input_AuxVanilla_Background);
            CFG.Current.ImGui_Input_DiffCompare_Background = GetVectorValue(theme.ImGui_Input_DiffCompare_Background);

            // Text
            CFG.Current.ImGui_Default_Text_Color = GetVectorValue(theme.ImGui_Default_Text_Color);
            CFG.Current.ImGui_Warning_Text_Color = GetVectorValue(theme.ImGui_Warning_Text_Color);
            CFG.Current.ImGui_Benefit_Text_Color = GetVectorValue(theme.ImGui_Benefit_Text_Color);
            CFG.Current.ImGui_Invalid_Text_Color = GetVectorValue(theme.ImGui_Invalid_Text_Color);
            CFG.Current.ImGui_ParamRef_Text = GetVectorValue(theme.ImGui_ParamRef_Text);
            CFG.Current.ImGui_ParamRefMissing_Text = GetVectorValue(theme.ImGui_ParamRefMissing_Text);
            CFG.Current.ImGui_ParamRefInactive_Text = GetVectorValue(theme.ImGui_ParamRefInactive_Text);
            CFG.Current.ImGui_EnumName_Text = GetVectorValue(theme.ImGui_EnumName_Text);
            CFG.Current.ImGui_EnumValue_Text = GetVectorValue(theme.ImGui_EnumValue_Text);
            CFG.Current.ImGui_FmgLink_Text = GetVectorValue(theme.ImGui_FmgLink_Text);
            CFG.Current.ImGui_FmgRef_Text = GetVectorValue(theme.ImGui_FmgRef_Text);
            CFG.Current.ImGui_FmgRefInactive_Text = GetVectorValue(theme.ImGui_FmgRefInactive_Text);
            CFG.Current.ImGui_IsRef_Text = GetVectorValue(theme.ImGui_IsRef_Text);
            CFG.Current.ImGui_VirtualRef_Text = GetVectorValue(theme.ImGui_VirtualRef_Text);
            CFG.Current.ImGui_Ref_Text = GetVectorValue(theme.ImGui_Ref_Text);
            CFG.Current.ImGui_AuxConflict_Text = GetVectorValue(theme.ImGui_AuxConflict_Text);
            CFG.Current.ImGui_AuxAdded_Text = GetVectorValue(theme.ImGui_AuxAdded_Text);
            CFG.Current.ImGui_PrimaryChanged_Text = GetVectorValue(theme.ImGui_PrimaryChanged_Text);
            CFG.Current.ImGui_ParamRow_Text = GetVectorValue(theme.ImGui_ParamRow_Text);
            CFG.Current.ImGui_AliasName_Text = GetVectorValue(theme.ImGui_AliasName_Text);

            // Misc
            CFG.Current.DisplayGroupEditor_Border_Highlight = GetVectorValue(theme.DisplayGroupEditor_Border_Highlight);
            CFG.Current.DisplayGroupEditor_DisplayActive_Frame = GetVectorValue(theme.DisplayGroupEditor_DisplayActive_Frame);
            CFG.Current.DisplayGroupEditor_DisplayActive_Checkbox = GetVectorValue(theme.DisplayGroupEditor_DisplayActive_Checkbox);
            CFG.Current.DisplayGroupEditor_DrawActive_Frame = GetVectorValue(theme.DisplayGroupEditor_DrawActive_Frame);
            CFG.Current.DisplayGroupEditor_DrawActive_Checkbox = GetVectorValue(theme.DisplayGroupEditor_DrawActive_Checkbox);
            CFG.Current.DisplayGroupEditor_CombinedActive_Frame = GetVectorValue(theme.DisplayGroupEditor_CombinedActive_Frame);
            CFG.Current.DisplayGroupEditor_CombinedActive_Checkbox = GetVectorValue(theme.DisplayGroupEditor_CombinedActive_Checkbox);
        }

        public static Vector4 GetVectorValue(List<float> list)
        {
            if (list == null)
            {
                TaskLogs.AddLog($"Theme is missing entry, defaulted this vector to 1, 1, 1, 1");
                return new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            }

            return new Vector4(list[0], list[1], list[2], list[3]);
        }

        public static List<float> GetFloatList(Vector4 value)
        {
            return new List<float> { value.X, value.Y, value.Z, value.W };
        }
    }

    [JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
    [JsonSerializable(typeof(InterfaceTheme))]
    public partial class InterfaceThemeSerializationContext
    : JsonSerializerContext
    { }

    public class InterfaceTheme
    {
        public int id { get; set; }

        public string name { get; set; }

        public List<float> ImGui_MainBg { get; set; }
        public List<float> ImGui_ChildBg { get; set; }
        public List<float> ImGui_PopupBg { get; set; }
        public List<float> ImGui_Border { get; set; }
        public List<float> ImGui_TitleBarBg { get; set; }
        public List<float> ImGui_TitleBarBg_Active { get; set; }
        public List<float> ImGui_MenuBarBg { get; set; }
        public List<float> Imgui_Moveable_MainBg { get; set; }
        public List<float> Imgui_Moveable_ChildBg { get; set; }
        public List<float> Imgui_Moveable_TitleBg { get; set; }
        public List<float> Imgui_Moveable_TitleBg_Active { get; set; }
        public List<float> Imgui_Moveable_Header { get; set; }
        public List<float> ImGui_ScrollbarBg { get; set; }
        public List<float> ImGui_ScrollbarGrab { get; set; }
        public List<float> ImGui_ScrollbarGrab_Hover { get; set; }
        public List<float> ImGui_ScrollbarGrab_Active { get; set; }
        public List<float> ImGui_SliderGrab { get; set; }
        public List<float> ImGui_SliderGrab_Active { get; set; }
        public List<float> ImGui_Tab { get; set; }
        public List<float> ImGui_Tab_Hover { get; set; }
        public List<float> ImGui_Tab_Active { get; set; }
        public List<float> ImGui_UnfocusedTab { get; set; }
        public List<float> ImGui_UnfocusedTab_Active { get; set; }
        public List<float> ImGui_Button { get; set; }
        public List<float> ImGui_Button_Hovered { get; set; }
        public List<float> ImGui_ButtonActive { get; set; }
        public List<float> ImGui_Selection { get; set; }
        public List<float> ImGui_Selection_Hover { get; set; }
        public List<float> ImGui_Selection_Active { get; set; }
        public List<float> ImGui_Input_Background { get; set; }
        public List<float> ImGui_Input_Background_Hover { get; set; }
        public List<float> ImGui_Input_Background_Active { get; set; }
        public List<float> ImGui_Input_CheckMark { get; set; }
        public List<float> ImGui_Input_Conflict_Background { get; set; }
        public List<float> ImGui_Input_Vanilla_Background { get; set; }
        public List<float> ImGui_Input_Default_Background { get; set; }
        public List<float> ImGui_Input_AuxVanilla_Background { get; set; }
        public List<float> ImGui_Input_DiffCompare_Background { get; set; }
        public List<float> ImGui_MultipleInput_Background { get; set; }
        public List<float> ImGui_ErrorInput_Background { get; set; }
        public List<float> ImGui_Default_Text_Color { get; set; }
        public List<float> ImGui_Warning_Text_Color { get; set; }
        public List<float> ImGui_Benefit_Text_Color { get; set; }
        public List<float> ImGui_Invalid_Text_Color { get; set; }
        public List<float> ImGui_ParamRef_Text { get; set; }
        public List<float> ImGui_ParamRefMissing_Text { get; set; }
        public List<float> ImGui_ParamRefInactive_Text { get; set; }
        public List<float> ImGui_EnumName_Text { get; set; }
        public List<float> ImGui_EnumValue_Text { get; set; }
        public List<float> ImGui_FmgLink_Text { get; set; }
        public List<float> ImGui_FmgRef_Text { get; set; }
        public List<float> ImGui_FmgRefInactive_Text { get; set; }
        public List<float> ImGui_IsRef_Text { get; set; }
        public List<float> ImGui_VirtualRef_Text { get; set; }
        public List<float> ImGui_Ref_Text { get; set; }
        public List<float> ImGui_AuxConflict_Text { get; set; }
        public List<float> ImGui_AuxAdded_Text { get; set; }
        public List<float> ImGui_PrimaryChanged_Text { get; set; }
        public List<float> ImGui_ParamRow_Text { get; set; }
        public List<float> ImGui_AliasName_Text { get; set; }
        public List<float> DisplayGroupEditor_Border_Highlight { get; set; }
        public List<float> DisplayGroupEditor_DisplayActive_Frame { get; set; }
        public List<float> DisplayGroupEditor_DisplayActive_Checkbox { get; set; }
        public List<float> DisplayGroupEditor_DrawActive_Frame { get; set; }
        public List<float> DisplayGroupEditor_DrawActive_Checkbox { get; set; }
        public List<float> DisplayGroupEditor_CombinedActive_Frame { get; set; }
        public List<float> DisplayGroupEditor_CombinedActive_Checkbox { get; set; }
    }
}
