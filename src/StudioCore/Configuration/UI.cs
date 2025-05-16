using Microsoft.Extensions.Logging;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text.Json;

namespace StudioCore.Configuration;

/// <summary>
/// This is for all ImGui coloring and presentation adjustments
/// ONLY those elements should be in here, to allow interface themes to be easily handled
/// -- The theme is simply an alternative UI JSON that is loaded in.
/// </summary>
public class UI
{
    ///------------------------------------------------------------
    /// Interface: General Coloring
    ///------------------------------------------------------------
    // Fixed Window
    public Vector4 ImGui_MainBg = new Vector4(0.176f, 0.176f, 0.188f, 1.0f);
    public Vector4 ImGui_ChildBg = new Vector4(0.145f, 0.145f, 0.149f, 1.0f);
    public Vector4 ImGui_PopupBg = new Vector4(0.106f, 0.106f, 0.110f, 1.0f);
    public Vector4 ImGui_Border = new Vector4(0.247f, 0.247f, 0.275f, 1.0f);
    public Vector4 ImGui_TitleBarBg = new Vector4(0.176f, 0.176f, 0.188f, 1.0f);
    public Vector4 ImGui_TitleBarBg_Active = new Vector4(0.176f, 0.176f, 0.188f, 1.0f);
    public Vector4 ImGui_MenuBarBg = new Vector4(0.176f, 0.176f, 0.188f, 1.0f);

    // Moveable Window
    public Vector4 Imgui_Moveable_MainBg = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
    public Vector4 Imgui_Moveable_ChildBg = new Vector4(0.145f, 0.145f, 0.149f, 1.0f);
    public Vector4 Imgui_Moveable_ChildBgSecondary = new Vector4(0.1f, 0.1f, 0.1f, 1.0f);
    public Vector4 Imgui_Moveable_TitleBg = new Vector4(0.176f, 0.176f, 0.188f, 1.0f);
    public Vector4 Imgui_Moveable_TitleBg_Active = new Vector4(0.25f, 0.25f, 0.25f, 1.0f);
    public Vector4 Imgui_Moveable_Header = new Vector4(0.3f, 0.3f, 0.6f, 0.4f);

    // Scroll
    public Vector4 ImGui_ScrollbarBg = new Vector4(0.243f, 0.243f, 0.249f, 1.0f);
    public Vector4 ImGui_ScrollbarGrab = new Vector4(0.408f, 0.408f, 0.408f, 1.0f);
    public Vector4 ImGui_ScrollbarGrab_Hover = new Vector4(0.635f, 0.635f, 0.635f, 1.0f);
    public Vector4 ImGui_ScrollbarGrab_Active = new Vector4(1.000f, 1.000f, 1.000f, 1.0f);
    public Vector4 ImGui_SliderGrab = new Vector4(0.635f, 0.635f, 0.635f, 1.0f);
    public Vector4 ImGui_SliderGrab_Active = new Vector4(1.000f, 1.000f, 1.000f, 1.0f);

    // Tab
    public Vector4 ImGui_Tab = new Vector4(0.176f, 0.176f, 0.188f, 1.0f);
    public Vector4 ImGui_Tab_Hover = new Vector4(0.110f, 0.592f, 0.918f, 1.0f);
    public Vector4 ImGui_Tab_Active = new Vector4(0.200f, 0.600f, 1.000f, 1.0f);
    public Vector4 ImGui_UnfocusedTab = new Vector4(0.176f, 0.176f, 0.188f, 1.0f);
    public Vector4 ImGui_UnfocusedTab_Active = new Vector4(0.247f, 0.247f, 0.275f, 1.0f);

    // Button
    public Vector4 ImGui_Button = new Vector4(0.176f, 0.176f, 0.188f, 1.0f);
    public Vector4 ImGui_Button_Hovered = new Vector4(0.247f, 0.247f, 0.275f, 1.0f);
    public Vector4 ImGui_ButtonActive = new Vector4(0.200f, 0.600f, 1.000f, 1.0f);

    // Selection
    public Vector4 ImGui_Selection = new Vector4(0.087f, 0.296f, 0.437f, 1.000f);
    public Vector4 ImGui_Selection_Hover = new Vector4(0.247f, 0.247f, 0.275f, 1.0f);
    public Vector4 ImGui_Selection_Active = new Vector4(0.161f, 0.550f, 0.939f, 1.0f);

    // Input 
    public Vector4 ImGui_Input_Background = new Vector4(0.200f, 0.200f, 0.216f, 1.0f);
    public Vector4 ImGui_Input_Background_Hover = new Vector4(0.247f, 0.247f, 0.275f, 1.0f);
    public Vector4 ImGui_Input_Background_Active = new Vector4(0.200f, 0.200f, 0.216f, 1.0f);
    public Vector4 ImGui_Input_CheckMark = new Vector4(1.000f, 1.000f, 1.000f, 1.0f);
    public Vector4 ImGui_Input_Conflict_Background = new Vector4(0.25f, 0.2f, 0.2f, 1.0f);
    public Vector4 ImGui_Input_Vanilla_Background = new Vector4(0.2f, 0.22f, 0.2f, 1.0f);
    public Vector4 ImGui_Input_Default_Background = new Vector4(0.180f, 0.180f, 0.196f, 1.0f);
    public Vector4 ImGui_Input_AuxVanilla_Background = new Vector4(0.2f, 0.2f, 0.35f, 1.0f);
    public Vector4 ImGui_Input_DiffCompare_Background = new Vector4(0.2f, 0.2f, 0.35f, 1.0f);
    public Vector4 ImGui_MultipleInput_Background = new Vector4(0.0f, 0.5f, 0.0f, 0.1f);
    public Vector4 ImGui_ErrorInput_Background = new Vector4(0.8f, 0.2f, 0.2f, 1.0f);

    // Text
    public Vector4 ImGui_Default_Text_Color = new Vector4(0.9f, 0.9f, 0.9f, 1.0f);
    public Vector4 ImGui_Warning_Text_Color = new Vector4(1.0f, 0f, 0f, 1.0f);
    public Vector4 ImGui_Benefit_Text_Color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
    public Vector4 ImGui_Invalid_Text_Color = new Vector4(1.0f, 0.3f, 0.3f, 1.0f);

    public Vector4 ImGui_ParamRef_Text = new Vector4(1.0f, 0.5f, 0.5f, 1.0f);
    public Vector4 ImGui_ParamRefMissing_Text = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
    public Vector4 ImGui_ParamRefInactive_Text = new Vector4(0.7f, 0.7f, 0.7f, 1.0f);
    public Vector4 ImGui_EnumName_Text = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
    public Vector4 ImGui_EnumValue_Text = new Vector4(1.0f, 0.5f, 0.5f, 1.0f);
    public Vector4 ImGui_FmgLink_Text = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
    public Vector4 ImGui_FmgRef_Text = new Vector4(1.0f, 0.5f, 0.5f, 1.0f);
    public Vector4 ImGui_FmgRefInactive_Text = new Vector4(0.7f, 0.7f, 0.7f, 1.0f);
    public Vector4 ImGui_IsRef_Text = new Vector4(1.0f, 0.5f, 1.0f, 1.0f);
    public Vector4 ImGui_VirtualRef_Text = new Vector4(1.0f, 0.75f, 1.0f, 1.0f);
    public Vector4 ImGui_Ref_Text = new Vector4(1.0f, 0.75f, 0.75f, 1.0f);
    public Vector4 ImGui_AuxConflict_Text = new Vector4(1, 0.7f, 0.7f, 1);
    public Vector4 ImGui_AuxAdded_Text = new Vector4(0.7f, 0.7f, 1, 1);
    public Vector4 ImGui_PrimaryChanged_Text = new Vector4(0.7f, 1, 0.7f, 1);
    public Vector4 ImGui_ParamRow_Text = new Vector4(0.8f, 0.8f, 0.8f, 1.0f);
    public Vector4 ImGui_AliasName_Text = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);

    // Logger
    public Vector4 ImGui_Logger_Information_Color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
    public Vector4 ImGui_Logger_Warning_Color = new Vector4(1.0f, 0f, 0f, 1.0f);
    public Vector4 ImGui_Logger_Error_Color = new Vector4(1.0f, 0.5f, 0.5f, 1.0f);

    /// <summary>
    /// Coloring for the border highlight in the Render Group window.
    /// </summary>
    public Vector4 DisplayGroupEditor_Border_Highlight = new Vector4(1.0f, 0.2f, 0.2f, 1.0f);

    /// <summary>
    /// Coloring for the dispgroup active frame in the Render Group window.
    /// </summary>
    public Vector4 DisplayGroupEditor_DisplayActive_Frame = new Vector4(0.4f, 0.06f, 0.06f, 1.0f);

    /// <summary>
    /// Coloring for the dispgroup active checkbox in the Render Group window.
    /// </summary>
    public Vector4 DisplayGroupEditor_DisplayActive_Checkbox = new Vector4(1.0f, 0.2f, 0.2f, 1.0f);

    /// <summary>
    /// Coloring for the drawgroup active frame in the Render Group window.
    /// </summary>
    public Vector4 DisplayGroupEditor_DrawActive_Frame = new Vector4(0.02f, 0.3f, 0.02f, 1.0f);

    /// <summary>
    /// Coloring for the drawgroup active checkbox in the Render Group window.
    /// </summary>
    public Vector4 DisplayGroupEditor_DrawActive_Checkbox = new Vector4(0.2f, 1.0f, 0.2f, 1.0f);

    /// <summary>
    /// Coloring for the disp/drawgroup combined frame in the Render Group window.
    /// </summary>
    public Vector4 DisplayGroupEditor_CombinedActive_Frame = new Vector4(0.4f, 0.4f, 0.06f, 1.0f);

    /// <summary>
    /// Coloring for the disp/drawgroup combined checkbox in the Render Group window.
    /// </summary>
    public Vector4 DisplayGroupEditor_CombinedActive_Checkbox = new Vector4(1f, 1f, 0.02f, 1.0f);

    /// <summary>
    /// Coloring for a modified text entry in the Text Editor.
    /// </summary>
    public Vector4 ImGui_TextEditor_ModifiedTextEntry_Text = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);

    /// <summary>
    /// Coloring for an unique text entry in the Text Editor.
    /// </summary>
    public Vector4 ImGui_TextEditor_UniqueTextEntry_Text = new Vector4(0.409f, 0.967f, 0.693f, 1.0f);

    public Vector4 ImGui_TimeAct_InfoText_1_Color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f); // Green
    public Vector4 ImGui_TimeAct_InfoText_2_Color = new Vector4(0.409f, 0.967f, 0.693f, 1.0f); // Cyan
    public Vector4 ImGui_TimeAct_InfoText_3_Color = new Vector4(0.237f, 0.925f, 1.000f, 1.0f); // Light Blue
    public Vector4 ImGui_TimeAct_InfoText_4_Color = new Vector4(1f, 0.470f, 0.884f, 1.0f); // Purple

    //****************************
    // UI
    //****************************
    public static UI Current { get; private set; }
    public static UI Default { get; } = new();

    public static void Setup()
    {
        Current = new UI();
    }

    public static void Load()
    {
        if(CFG.Current.SelectedTheme != "")
        {
            LoadTheme(CFG.Current.SelectedTheme);
        }
        else
        {
            LoadDefault();
        }
    }

    public static void LoadDefault()
    {
        var folder = ProjectUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Interface.json");

        if (!File.Exists(file))
        {
            Current = new UI();
            Save();
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var options = new JsonSerializerOptions();
                Current = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.UI);

                if (Current == null)
                {
                    throw new Exception("JsonConvert returned null");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Interface Configuration failed to load, default configuration has been restored.", LogLevel.Error, Tasks.LogPriority.High, e);

                Current = new UI();
                Save();
            }
        }
    }

    public static void LoadTheme(string name)
    {
        var folder = ProjectUtils.GetThemeFolder();
        var file = Path.Combine(folder, $"{name}.json");

        if (!File.Exists(file))
        {
            Current = new UI();
            Save();
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var options = new JsonSerializerOptions();
                Current = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.UI);

                if (Current == null)
                {
                    throw new Exception("JsonConvert returned null");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Interface Configuration failed to load, default configuration has been restored.", LogLevel.Error, Tasks.LogPriority.High, e);

                Current = new UI();
                Save();
            }
        }
    }

    public static void ExportTheme(string name)
    {
        var folder = ProjectUtils.GetThemeFolder();
        var file = Path.Combine(folder, $"{name}.json");

        if (File.Exists(file))
        {
            var result = PlatformUtils.Instance.MessageBox("Theme with this name already exists. Overwrite?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if(result  == DialogResult.OK)
            {
                var json = JsonSerializer.Serialize(Current, SmithboxSerializerContext.Default.UI);

                File.WriteAllText(file, json);
            }
        }
        else if (name != "" && FilePathUtils.IsValidFileName(name))
        {
            var json = JsonSerializer.Serialize(Current, SmithboxSerializerContext.Default.UI);

            File.WriteAllText(file, json);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("Invalid filename.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static void Save()
    {
        var folder = ProjectUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Interface.json");

        var json = JsonSerializer.Serialize(Current, SmithboxSerializerContext.Default.UI);

        File.WriteAllText(file, json);
    }

    public static void ResetToDefault()
    {
        foreach (var field in typeof(UI).GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            field.SetValue(Current, field.GetValue(Default));
        }
    }
}
