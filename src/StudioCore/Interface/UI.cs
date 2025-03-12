using StudioCore.Platform;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Drawing;
using static StudioCore.Configuration.Settings.TimeActEditorTab;
using StudioCore.Core.Project;
using Silk.NET.SDL;
using StudioCore.Graphics;
using ImGuiNET;

namespace StudioCore.Interface;

[JsonSourceGenerationOptions(WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata, IncludeFields = true)]
[JsonSerializable(typeof(UI))]
internal partial class UISerializerContext : JsonSerializerContext
{
}

public class UI
{
    public const string FolderName = "Smithbox";
    public const string Config_FileName = "Smithbox_Interface.json";

    public static bool IsEnabled = true;
    private static readonly object _lock_SaveLoadCFG = new();

    public static UI Current { get; private set; }
    public static UI Default { get; } = new();

    //**************
    // Common Size Definitions
    //**************
    public static Vector2 MenuButtonSize = new Vector2(200, 24);
    public static Vector2 MenuButtonWideSize = new Vector2(350, 24);

    public static Vector2 ModalButtonThirdSize = new Vector2(172, 24);
    public static Vector2 ModalButtonHalfSize = new Vector2(260, 24);
    public static Vector2 ModalButtonSize = new Vector2(520, 24);

    //**************
    // System
    //**************
    public bool System_Show_UI_Tooltips = true;
    public bool System_ShowActionLogger = true;
    public bool System_ShowWarningLogger = true;

    public bool System_WrapAliasDisplay = true;
    public float System_UI_Scale = 1.0f;
    public bool System_ScaleByDPI = true;
    public bool System_Font_Chinese = false;
    public bool System_Font_Cyrillic = false;
    public bool System_Font_Korean = false;
    public bool System_Font_Thai = false;
    public bool System_Font_Vietnamese = false;
    public string System_English_Font = "Assets\\Fonts\\RobotoMono-Light.ttf";
    public string System_Other_Font = "Assets\\Fonts\\NotoSansCJKtc-Light.otf";

    public bool System_DisplayResourceLoadingWindow = true;

    //**************
    // Theme
    //**************
    public string SelectedThemeName = "";
    public int SelectedTheme = 0;
    public string NewThemeName = "";

    //**************
    // Interface
    //**************
    public float Interface_FontSize = 14.0f;

    public float Interface_ButtonHeight = 32.0f;
    public float Interface_ThinButtonHeight = 24.0f;

    public float Interface_ModalHeight = 600.0f;
    public float Interface_ModalWidth = 800.0f;

    //**************
    // View Toggles
    //**************
    public bool Interface_Editor_Viewport = true;
    public bool Interface_Editor_Profiling = true;

    // Map Editor
    public bool Interface_MapEditor_MapList = true;
    public bool Interface_MapEditor_MapContents = true;
    public bool Interface_MapEditor_Properties = true;
    public bool Interface_MapEditor_PropertySearch = true;
    public bool Interface_MapEditor_RenderGroups = true;
    public bool Interface_MapEditor_AssetBrowser = true;
    public bool Interface_MapEditor_ToolWindow = true;
    public bool Interface_MapEditor_ResourceList = true;
    public bool Interface_MapEditor_Selection_Groups = true;
    public bool Interface_MapEditor_Viewport_Grid = true;
    public bool Interface_MapEditor_Viewport_LightmapAtlas = true;
    public bool Interface_MapEditor_EntityInformation = false;
    public bool Interface_MapEditor_EntityIdentifierOverview = false;

    // Model Editor
    public bool Interface_ModelEditor_ModelHierarchy = true;
    public bool Interface_ModelEditor_Properties = true;
    public bool Interface_ModelEditor_AssetBrowser = true;
    public bool Interface_ModelEditor_ToolConfigurationWindow = true;
    public bool Interface_ModelEditor_ResourceList = true;
    public bool Interface_ModelEditor_Viewport_Grid = true;

    // Param Editor
    public bool Interface_ParamEditor_Table = true;
    public bool Interface_ParamEditor_MassEdit = true;
    public bool Interface_ParamEditor_ToolConfiguration = true;

    // Text Editor
    public bool Interface_TextEditor_FileContainerList = true;
    public bool Interface_TextEditor_FmgList = true;
    public bool Interface_TextEditor_FmgEntryList = true;
    public bool Interface_TextEditor_FmgEntryProperties = true;
    public bool Interface_TextEditor_ToolConfigurationWindow = true;

    // GPARM EDitor
    public bool Interface_GparamEditor_Files = true;
    public bool Interface_GparamEditor_Groups = true;
    public bool Interface_GparamEditor_Fields = true;
    public bool Interface_GparamEditor_Values = true;
    public bool Interface_GparamEditor_ToolConfiguration = true;

    // SFX Editor
    public bool Interface_ParticleEditor_Files = true;
    public bool Interface_ParticleEditor_Particles = true;
    public bool Interface_ParticleEditor_Data = true;
    public bool Interface_ParticleEditor_Toolbar = true;

    // TAE Editor
    public bool Interface_TimeActEditor_ContainerFileList = true;
    public bool Interface_TimeActEditor_TimeActList = true;
    public bool Interface_TimeActEditor_AnimationList = true;
    public bool Interface_TimeActEditor_AnimationProperties = true;
    public bool Interface_TimeActEditor_EventList = true;
    public bool Interface_TimeActEditor_EventProperties = true;
    public bool Interface_TimeActEditor_ToolConfiguration = true;

    // EMEVD Editor
    public bool Interface_EmevdEditor_Files = true;
    public bool Interface_EmevdEditor_Events = true;
    public bool Interface_EmevdEditor_Instructions = true;
    public bool Interface_EmevdEditor_EventProperties = true;
    public bool Interface_EmevdEditor_InstructionProperties = true;
    public bool Interface_EmevdEditor_ToolConfigurationWindow = true;

    // ESD Editor
    public bool Interface_EsdEditor_FileList = true;
    public bool Interface_EsdEditor_ScriptList = true;
    public bool Interface_EsdEditor_StateGroupList = true;
    public bool Interface_EsdEditor_StateNodeList = true;
    public bool Interface_EsdEditor_StateNodeContents = true;
    public bool Interface_EsdEditor_ToolConfigurationWindow = true;

    // Texture Viewer
    public bool Interface_TextureViewer_Files = true;
    public bool Interface_TextureViewer_Textures = true;
    public bool Interface_TextureViewer_Viewer = true;
    public bool Interface_TextureViewer_Properties = true;
    public bool Interface_TextureViewer_ToolConfiguration = true;
    public bool Interface_TextureViewer_ResourceList = true;

    public bool Interface_MapEditor_Toolbar_ActionList_TopToBottom = true;
    public bool Interface_ModelEditor_Toolbar_ActionList_TopToBottom = true;
    public bool Interface_ParamEditor_Toolbar_ActionList_TopToBottom = true;
    public bool Interface_TextEditor_Toolbar_ActionList_TopToBottom = true;
    public bool Interface_GparamEditor_Toolbar_ActionList_TopToBottom = true;
    public bool Interface_ParticleEditor_Toolbar_ActionList_TopToBottom = true;
    public bool Interface_TextureViewer_Toolbar_ActionList_TopToBottom = true;

    public bool Interface_MapEditor_PromptUser = true;
    public bool Interface_GparamEditor_PromptUser = true;
    public bool Interface_ModelEditor_PromptUser = true;
    public bool Interface_ParamEditor_PromptUser = true;
    public bool Interface_TextEditor_PromptUser = true;
    public bool Interface_ParticleEditor_PromptUser = true;
    public bool Interface_TextureViewer_PromptUser = true;

    //**************
    // Interface Colors
    //**************
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

    public Vector4 ImGui_TimeAct_InfoText_1_Color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f); // Green
    public Vector4 ImGui_TimeAct_InfoText_2_Color = new Vector4(0.409f, 0.967f, 0.693f, 1.0f); // Cyan
    public Vector4 ImGui_TimeAct_InfoText_3_Color = new Vector4(0.237f, 0.925f, 1.000f, 1.0f); // Light Blue
    public Vector4 ImGui_TimeAct_InfoText_4_Color = new Vector4(1f, 0.470f, 0.884f, 1.0f); // Purple

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

    public Vector4 ImGui_TextEditor_ModifiedRow_Text = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
    public Vector4 ImGui_TextEditor_UniqueRow_Text = new Vector4(0.409f, 0.967f, 0.693f, 1.0f);

    public Vector4 ImGui_Logger_Information_Color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
    public Vector4 ImGui_Logger_Warning_Color = new Vector4(1.0f, 0f, 0f, 1.0f);
    public Vector4 ImGui_Logger_Error_Color = new Vector4(1.0f, 0.5f, 0.5f, 1.0f); 

    //**************
    // Interface Styles
    //**************
    public Vector4 DisplayGroupEditor_Border_Highlight = new Vector4(1.0f, 0.2f, 0.2f, 1.0f);
    public Vector4 DisplayGroupEditor_DisplayActive_Frame = new Vector4(0.4f, 0.06f, 0.06f, 1.0f);
    public Vector4 DisplayGroupEditor_DisplayActive_Checkbox = new Vector4(1.0f, 0.2f, 0.2f, 1.0f);
    public Vector4 DisplayGroupEditor_DrawActive_Frame = new Vector4(0.02f, 0.3f, 0.02f, 1.0f);
    public Vector4 DisplayGroupEditor_DrawActive_Checkbox = new Vector4(0.2f, 1.0f, 0.2f, 1.0f);
    public Vector4 DisplayGroupEditor_CombinedActive_Frame = new Vector4(0.4f, 0.4f, 0.06f, 1.0f);
    public Vector4 DisplayGroupEditor_CombinedActive_Checkbox = new Vector4(1f, 1f, 0.02f, 1.0f);

    //**************
    // Functions
    //**************
    public static string GetConfigFilePath()
    {
        return $@"{GetConfigFolderPath()}\{Config_FileName}";
    }

    public static string GetConfigFolderPath()
    {
        return $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\{FolderName}";
    }

    private static void LoadConfig()
    {
        if (!File.Exists(GetConfigFilePath()))
        {
            Current = new UI();
        }
        else
        {
            try
            {
                var options = new JsonSerializerOptions();
                Current = JsonSerializer.Deserialize(File.ReadAllText(GetConfigFilePath()),
                    UISerializerContext.Default.UI);

                if (Current == null)
                {
                    throw new Exception("JsonConvert returned null");
                }
            }
            catch (Exception e)
            {
                DialogResult result = PlatformUtils.Instance.MessageBox(
                    $"{e.Message}\n\nConfig could not be loaded. Reset settings?",
                    $"{Config_FileName} Load Error", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    throw new Exception($"{Config_FileName} could not be loaded.\n\n{e.Message}");
                }

                Current = new UI();
                SaveConfig();
            }
        }
        DPI.UIScaleChanged?.Invoke(null, EventArgs.Empty);
    }

    private static void SaveConfig()
    {
        var json = JsonSerializer.Serialize(
            Current, UISerializerContext.Default.UI);

        File.WriteAllText(GetConfigFilePath(), json);
    }

    public static void Save()
    {
        if (IsEnabled)
        {
            lock (_lock_SaveLoadCFG)
            {
                if (!Directory.Exists(GetConfigFolderPath()))
                {
                    Directory.CreateDirectory(GetConfigFolderPath());
                }

                SaveConfig();
            }
        }
    }

    public static void AttemptLoadOrDefault()
    {
        if (IsEnabled)
        {
            lock (_lock_SaveLoadCFG)
            {
                if (!Directory.Exists(GetConfigFolderPath()))
                {
                    Directory.CreateDirectory(GetConfigFolderPath());
                }

                LoadConfig();
            }
        }
    }

    /// <summary>
    /// Updates the common size definitions to account for current UI scale
    /// </summary>
    public static void OnGui()
    {
        // Menubar
        MenuButtonSize = new Vector2(200 * DPI.GetUIScale(), 24 * DPI.GetUIScale());
        MenuButtonWideSize = new Vector2(350 * DPI.GetUIScale(), 24 * DPI.GetUIScale());

        // Modal
        ModalButtonThirdSize = new Vector2(172 * DPI.GetUIScale(), 24 * DPI.GetUIScale());
        ModalButtonHalfSize = new Vector2(260 * DPI.GetUIScale(), 24 * DPI.GetUIScale());
        ModalButtonSize = new Vector2(520 * DPI.GetUIScale(), 24 * DPI.GetUIScale());
    }

    public static Vector2 GetStandardButtonSize()
    {
        var windowWidth = ImGui.GetWindowWidth() * 0.95f;
        return new Vector2(windowWidth, 32 * DPI.GetUIScale());
    }
    public static Vector2 GetStandardHalfButtonSize()
    {
        var windowWidth = ImGui.GetWindowWidth() * 0.95f;
        return new Vector2(windowWidth / 2, 32 * DPI.GetUIScale());
    }
}
