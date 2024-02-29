using StudioCore.Platform;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Drawing;
using StudioCore.MsbEditor;
using StudioCore.UserProject;
using Veldrid;

namespace StudioCore;

[JsonSourceGenerationOptions(WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata, IncludeFields = true)]
[JsonSerializable(typeof(CFG))]
internal partial class CfgSerializerContext : JsonSerializerContext
{
}

public class CFG
{
    public const string FolderName = "Smithbox";
    public const string Config_FileName = "Smithbox_Config.json";
    public const string Keybinds_FileName = "Smithbox_Keybinds.json";

    public const int MAX_RECENT_PROJECTS = 20;
    public static bool IsEnabled = true;

    private static readonly object _lock_SaveLoadCFG = new();

    //private string _Param_Export_Array_Delimiter = "|";
    private string _Param_Export_Delimiter = ",";

    // JsonExtensionData stores info in config file not present in class in order to retain settings between versions.
#pragma warning disable IDE0051
    [JsonExtensionData] public IDictionary<string, JsonElement> AdditionalData;
#pragma warning restore IDE0051

    //**************
    // Debug
    //**************
    public bool Debug_FireOnce = false;

    //**************
    // Actions
    //**************
    public Vector3 SavedPosition = new Vector3();
    public Vector3 SavedRotation = new Vector3();
    public Vector3 SavedScale = new Vector3();

    //**************
    // Interface
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
    public Vector4 ImGui_Selection = new Vector4(0.000f, 0.478f, 0.800f, 1.0f);
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

    // Misc
    public Vector4 DisplayGroupEditor_Border_Highlight = new Vector4(1.0f, 0.2f, 0.2f, 1.0f);
    public Vector4 DisplayGroupEditor_DisplayActive_Frame = new Vector4(0.4f, 0.06f, 0.06f, 1.0f);
    public Vector4 DisplayGroupEditor_DisplayActive_Checkbox = new Vector4(1.0f, 0.2f, 0.2f, 1.0f);
    public Vector4 DisplayGroupEditor_DrawActive_Frame = new Vector4(0.02f, 0.3f, 0.02f, 1.0f);
    public Vector4 DisplayGroupEditor_DrawActive_Checkbox = new Vector4(0.2f, 1.0f, 0.2f, 1.0f);
    public Vector4 DisplayGroupEditor_CombinedActive_Frame = new Vector4(0.4f, 0.4f, 0.06f, 1.0f);
    public Vector4 DisplayGroupEditor_CombinedActive_Checkbox = new Vector4(1f, 1f, 0.02f, 1.0f);

    // Setup
    public string SelectedThemeName = "";
    public int SelectedTheme = 0;
    public string NewThemeName = "";

    //****************************
    // Settings: System
    //****************************
    public bool System_Check_Program_Update = true;
    public bool System_Show_UI_Tooltips = true;
    public float System_UI_Scale = 1.0f;
    public bool System_Enable_Soapstone_Server = true;
    public bool System_Font_Chinese = false;
    public bool System_Font_Cyrillic = false;
    public bool System_Font_Korean = false;
    public bool System_Font_Thai = false;
    public bool System_Font_Vietnamese = false;
    public float System_Frame_Rate = 60.0f;

    public bool System_FlexibleUnpack = true;

    // Resource Banks
    public bool AutoLoadBank_TimeAct = false;
    public bool AutoLoadBank_Cutscene = false;
    public bool AutoLoadBank_Gparam = true;
    public bool AutoLoadBank_Material = false;
    public bool AutoLoadBank_Particle = false;
    public bool AutoLoadBank_EventScript = false;
    public bool AutoLoadBank_TalkScript = false;
    public bool AutoLoadBank_Textures = false;
    public bool AutoLoadBank_Behavior = false;

    //****************************
    // Settings: Viewport
    //****************************
    // View Grid
    public bool Viewport_EnableGrid = true;

    public int Viewport_GridType = 0;
    public int Viewport_Grid_Size = 1000;
    public int Viewport_Grid_Square_Size = 10;

    public float Viewport_Grid_Height = 0;
    public float Viewport_Grid_Height_Increment = 1;

    public bool Viewport_RegenerateMapGrid = false;
    public bool Viewport_Frustum_Culling = true;
    public bool Viewport_Enable_Texturing = false;
    public bool Viewport_Enable_ER_Auto_Map_Offset = true;
    public bool Viewport_Enable_Selection_Outline = true;

    public Vector3 Viewport_Grid_Color = new Vector3(0.5f, 0.5f, 0.5f);

    // Camera
    public float Viewport_Camera_FOV { get; set; } = 60.0f;
    public float Viewport_Camera_MoveSpeed_Slow { get; set; } = 1.0f;
    public float Viewport_Camera_MoveSpeed_Normal { get; set; } = 20.0f;
    public float Viewport_Camera_MoveSpeed_Fast { get; set; } = 200.0f;
    public float Viewport_Camera_Sensitivity { get; set; } = 0.0160f;
    public float Viewport_RenderDistance_Max { get; set; } = 50000.0f;

    // Rendering Limits
    public uint Viewport_Limit_Buffer_Flver_Bone = 65536;
    public uint Viewport_Limit_Buffer_Indirect_Draw = 50000;
    public int Viewport_Limit_Renderables = 50000;

    // Wireframe Coloring
    public float GFX_Wireframe_Color_Variance = 0.11f;

    public Vector3 GFX_Renderable_Box_BaseColor = Utils.GetDecimalColor(Color.Blue);
    public Vector3 GFX_Renderable_Box_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

    public Vector3 GFX_Renderable_Cylinder_BaseColor = Utils.GetDecimalColor(Color.Blue);
    public Vector3 GFX_Renderable_Cylinder_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

    public Vector3 GFX_Renderable_Sphere_BaseColor = Utils.GetDecimalColor(Color.Blue);
    public Vector3 GFX_Renderable_Sphere_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

    public Vector3 GFX_Renderable_Point_BaseColor = Utils.GetDecimalColor(Color.Yellow);
    public Vector3 GFX_Renderable_Point_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

    public Vector3 GFX_Renderable_DummyPoly_BaseColor = Utils.GetDecimalColor(Color.Yellow);
    public Vector3 GFX_Renderable_DummyPoly_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

    public Vector3 GFX_Renderable_BonePoint_BaseColor = Utils.GetDecimalColor(Color.Blue);
    public Vector3 GFX_Renderable_BonePoint_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

    public Vector3 GFX_Renderable_ModelMarker_Chr_BaseColor = Utils.GetDecimalColor(Color.Firebrick);
    public Vector3 GFX_Renderable_ModelMarker_Chr_HighlightColor = Utils.GetDecimalColor(Color.Tomato);

    public Vector3 GFX_Renderable_ModelMarker_Object_BaseColor = Utils.GetDecimalColor(Color.MediumVioletRed);
    public Vector3 GFX_Renderable_ModelMarker_Object_HighlightColor = Utils.GetDecimalColor(Color.DeepPink);

    public Vector3 GFX_Renderable_ModelMarker_Player_BaseColor = Utils.GetDecimalColor(Color.DarkOliveGreen);
    public Vector3 GFX_Renderable_ModelMarker_Player_HighlightColor = Utils.GetDecimalColor(Color.OliveDrab);

    public Vector3 GFX_Renderable_ModelMarker_Other_BaseColor = Utils.GetDecimalColor(Color.Wheat);
    public Vector3 GFX_Renderable_ModelMarker_Other_HighlightColor = Utils.GetDecimalColor(Color.AntiqueWhite);

    public Vector3 GFX_Renderable_PointLight_BaseColor = Utils.GetDecimalColor(Color.YellowGreen);
    public Vector3 GFX_Renderable_PointLight_HighlightColor = Utils.GetDecimalColor(Color.Yellow);

    public Vector3 GFX_Renderable_SpotLight_BaseColor = Utils.GetDecimalColor(Color.Goldenrod);
    public Vector3 GFX_Renderable_SpotLight_HighlightColor = Utils.GetDecimalColor(Color.Violet);

    public Vector3 GFX_Renderable_DirectionalLight_BaseColor = Utils.GetDecimalColor(Color.Cyan);
    public Vector3 GFX_Renderable_DirectionalLight_HighlightColor = Utils.GetDecimalColor(Color.AliceBlue);

    public Vector3 GFX_Gizmo_X_BaseColor = new(0.952f, 0.211f, 0.325f);
    public Vector3 GFX_Gizmo_X_HighlightColor = new(1.0f, 0.4f, 0.513f);

    public Vector3 GFX_Gizmo_Y_BaseColor = new(0.525f, 0.784f, 0.082f);
    public Vector3 GFX_Gizmo_Y_HighlightColor = new(0.713f, 0.972f, 0.270f);

    public Vector3 GFX_Gizmo_Z_BaseColor = new(0.219f, 0.564f, 0.929f);
    public Vector3 GFX_Gizmo_Z_HighlightColor = new(0.407f, 0.690f, 1.0f);

    public float Viewport_DefaultRender_Brightness = 1.0f;
    public float Viewport_DefaultRender_Saturation = 0.5f;

    public Vector3 Viewport_DefaultRender_SelectColor = new(1.0f, 0.5f, 0.0f);

    //****************************
    // Settings: Map Editor
    //****************************
    public bool MapEditor_Always_List_Loaded_Maps = true;
    public bool MapEditor_Show_Character_Names_in_Scene_Tree = true;
    public bool MapEditor_Enable_Commmunity_Names = true;
    public bool MapEditor_Enable_Commmunity_Hints = true;
    public bool MapEditor_Enable_Property_Info = false;
    public bool MapEditor_Enable_Map_Load_on_Double_Click = false;

    // Scene Filters
    public RenderFilter LastSceneFilter { get; set; } = RenderFilter.All ^ RenderFilter.Light;
    public RenderFilterPreset SceneFilter_Preset_01 { get; set; } = new("Map", RenderFilter.MapPiece | RenderFilter.Object | RenderFilter.Character | RenderFilter.Region);
    public RenderFilterPreset SceneFilter_Preset_02 { get; set; } = new("Collision", RenderFilter.Collision | RenderFilter.Object | RenderFilter.Character | RenderFilter.Region);
    public RenderFilterPreset SceneFilter_Preset_03 { get; set; } = new("Collision & Navmesh", RenderFilter.Collision | RenderFilter.Navmesh | RenderFilter.Object | RenderFilter.Character | RenderFilter.Region);
    public RenderFilterPreset SceneFilter_Preset_04 { get; set; } = new("Lighting (Map)", RenderFilter.MapPiece | RenderFilter.Object | RenderFilter.Character | RenderFilter.Light);
    public RenderFilterPreset SceneFilter_Preset_05 { get; set; } = new("Lighting (Collision)", RenderFilter.Collision | RenderFilter.Object | RenderFilter.Character | RenderFilter.Light);
    public RenderFilterPreset SceneFilter_Preset_06 { get; set; } = new("All", RenderFilter.All);

    public bool Prefab_IncludeEntityID = true;
    public bool Prefab_IncludeEntityGroupIDs = true;

    public bool Prefab_ApplyUniqueInstanceID = true;
    public bool Prefab_ApplyUniqueEntityID = false;
    public bool Prefab_ApplySelfPartNames = true;
    public bool Prefab_ApplySpecificEntityGroupID = false;
    public int Prefab_SpecificEntityGroupID = 0;

    //****************************
    // Settings: Model Editor
    //****************************
    public bool ModelEditor_RenderingUpdate = false;
    public bool ModelEditor_ViewDummyPolys = true;
    public bool Model_ViewBones = true;
    public bool ModelEditor_Enable_Commmunity_Names = true;
    public bool ModelEditor_Enable_Commmunity_Hints = true;
    public bool ModelEditor_DisplayDmyPolyReferenceID = true;
    public bool ModelEditor_DisplayMatNameOnMesh = true;

    //****************************
    // Settings: Param Editor
    //****************************
    public bool Param_MasseditPopupInContextMenu = true;
    public bool Param_AdvancedMassedit = false;
    public bool Param_AllowFieldReorder = true;
    public bool Param_AlphabeticalParams = true;
    public bool Param_DisableLineWrapping = false;
    public bool Param_DisableRowGrouping = false;
    public bool Param_HideEnums = false;
    public bool Param_HideReferenceRows = false;
    public bool Param_MakeMetaNamesPrimary = true;
    public bool Param_PasteAfterSelection = false;
    public bool Param_ShowFieldOffsets = false;
    public bool Param_ShowHotkeysInContextMenu = true;
    public bool Param_ShowSecondaryNames = true;
    public bool Param_ShowVanillaParams = true;
    public bool Param_FieldNameInContextMenu = true;
    public bool Param_FieldDescriptionInContextMenu = true;
    public bool Param_SplitContextMenu = false;
    public bool UI_CompactParams = false;
    public bool Param_QuickNameEdit = false;
    public int Param_DuplicateAmount = 1;
    public bool Param_ShowTraditionalPercentages = false;

    //****************************
    // Settings: Text Editor
    //****************************
    public bool FMG_NoFmgPatching = false;
    public bool FMG_NoGroupedFmgEntries = false;
    public bool FMG_ShowOriginalNames = false;
    public int FMG_DuplicateAmount = 1;
    public int FMG_DuplicateIncrement = 1;

    //****************************
    // Settings: Gparam Editor
    //****************************
    public bool Gparam_DisplayColorEditForVector4Fields = true;
    public bool Gparam_DisplayMapNames = true;
    public bool Gparam_DisplayEmptyGroups = true;
    public bool Gparam_DisplayAddGroups = true;
    public bool Gparam_DisplayAddFields = true;

    //****************************
    // Windows
    //****************************
    public bool AliasBank_EditorMode = false;
    public bool AssetBrowser_ShowTagsInBrowser = true;
    public bool AssetBrowser_ShowLowDetailParts = false;

    public bool EventFlagAtlas_ShowTags = true;

    public bool MapNameAtlas_ShowUnused = false;
    public bool MapNameAtlas_ShowTags = true;

    //****************************
    // Map Toolbar
    //****************************
    public bool Toolbar_Show_Check_Duplicate_Entity_ID = true;
    public bool Toolbar_Show_Render_Patrol_Routes = true;
    public bool Toolbar_Show_Navigation_Data = true;
    public bool Toolbar_Show_Toggle_Object_Visibility_by_Tag = true;

    public bool Toolbar_Show_Go_to_in_Object_List = true;
    public bool Toolbar_Show_Move_to_Camera = true;
    public bool Toolbar_Show_Frame_in_Viewport = true;
    public bool Toolbar_Show_Toggle_Visibility = true;
    public bool Toolbar_Show_Create = true;
    public bool Toolbar_Show_Duplicate = true;
    public bool Toolbar_Show_Rotate = true;
    public bool Toolbar_Show_Toggle_Presence = true;
    public bool Toolbar_Show_Scramble = true;
    public bool Toolbar_Show_Replicate = true;
    public bool Toolbar_Show_Move_to_Grid = true;

    public bool Toolbar_Duplicate_Increment_Entity_ID = false;
    public bool Toolbar_Duplicate_Increment_UnkPartNames = false;
    public bool Toolbar_Duplicate_Increment_InstanceID = true;

    public bool Toolbar_Presence_Dummy_Type_ER = false;
    public bool Toolbar_Presence_Dummify = true;
    public bool Toolbar_Presence_Undummify = false;

    public bool Toolbar_Rotate_X = true;
    public bool Toolbar_Rotate_Y = false;
    public bool Toolbar_Rotate_Y_Pivot = false;
    public bool Toolbar_Fixed_Rotate = false;
    public Vector3 Toolbar_Rotate_FixedAngle = new Vector3(0, 0, 0);

    public bool Toolbar_Move_to_Camera_Offset_Specific_Input = false;
    public float Toolbar_Move_to_Camera_Offset = 3.0f;

    public bool Toolbar_Rotate_Specific_Input = false;
    public float Toolbar_Rotate_Increment { get; set; } = 90.0f;

    public bool Toolbar_Visibility_Target_Selection = true;
    public bool Toolbar_Visibility_Target_All = false;

    public bool Toolbar_Visibility_State_Enabled = true;
    public bool Toolbar_Visibility_State_Disabled = false;
    public bool Toolbar_Visibility_State_Flip = false;

    public bool Toolbar_Move_to_Grid_X = false;
    public bool Toolbar_Move_to_Grid_Y = true;
    public bool Toolbar_Move_to_Grid_Z = false;

    public bool Toolbar_Move_to_Grid_Specific_Height_Input = false;

    public bool Toolbar_Create_Light = false;
    public bool Toolbar_Create_Part = true;
    public bool Toolbar_Create_Region = false;
    public bool Toolbar_Create_Event = false;

    public string Toolbar_Tag_Visibility_Target = "LOD";
    public bool Toolbar_Tag_Visibility_State_Enabled = false;
    public bool Toolbar_Tag_Visibility_State_Disabled = true;

    public bool Toolbar_ShowScramblerMenu = false;

    public bool Scrambler_RandomisePosition_X = false;
    public bool Scrambler_RandomisePosition_Y = false;
    public bool Scrambler_RandomisePosition_Z = false;

    public float Scrambler_OffsetMin_Position_X = -100.0f;
    public float Scrambler_OffsetMax_Position_X = 100.0f;
    public float Scrambler_OffsetMin_Position_Y = -100.0f;
    public float Scrambler_OffsetMax_Position_Y = 100.0f;
    public float Scrambler_OffsetMin_Position_Z = -100.0f;
    public float Scrambler_OffsetMax_Position_Z = 100.0f;

    public bool Scrambler_RandomiseRotation_X = false;
    public bool Scrambler_RandomiseRotation_Y = false;
    public bool Scrambler_RandomiseRotation_Z = false;

    public float Scrambler_OffsetMin_Rotation_X = 0.0f;
    public float Scrambler_OffsetMax_Rotation_X = 360.0f;
    public float Scrambler_OffsetMin_Rotation_Y = 0.0f;
    public float Scrambler_OffsetMax_Rotation_Y = 360.0f;
    public float Scrambler_OffsetMin_Rotation_Z = 0.0f;
    public float Scrambler_OffsetMax_Rotation_Z = 360.0f;

    public bool Scrambler_RandomiseScale_SharedScale = false;

    public bool Scrambler_RandomiseScale_X = false;
    public bool Scrambler_RandomiseScale_Y = false;
    public bool Scrambler_RandomiseScale_Z = false;

    public float Scrambler_OffsetMin_Scale_X = 1.0f;
    public float Scrambler_OffsetMax_Scale_X = 3.0f;
    public float Scrambler_OffsetMin_Scale_Y = 1.0f;
    public float Scrambler_OffsetMax_Scale_Y = 3.0f;
    public float Scrambler_OffsetMin_Scale_Z = 1.0f;
    public float Scrambler_OffsetMax_Scale_Z = 3.0f;

    public bool Toolbar_ShowReplicatorMenu = false;

    public bool Replicator_Circle_Radius_Specific_Input = false;
    public bool Replicator_Sphere_Horizontal_Radius_Specific_Input = false;
    public bool Replicator_Sphere_Vertical_Radius_Specific_Input = false;

    public bool Replicator_Apply_Scramble_Configuration = false;
    public bool Replicator_Increment_Entity_ID = false;
    public bool Replicator_Increment_UnkPartNames = false;
    public bool Replicator_Increment_InstanceID = true;

    public bool Replicator_Mode_Line = true;
    public bool Replicator_Mode_Circle = false;
    public bool Replicator_Mode_Square = false;
    public bool Replicator_Mode_Sphere = false;
    public bool Replicator_Mode_Box = false;

    public int Replicator_Line_Clone_Amount = 1;
    public int Replicator_Line_Position_Offset = 25;
    public bool Replicator_Line_Position_Offset_Axis_X = true;
    public bool Replicator_Line_Position_Offset_Axis_Y = false;
    public bool Replicator_Line_Position_Offset_Axis_Z = false;
    public bool Replicator_Line_Offset_Direction_Flipped = false;

    public int Replicator_Circle_Size = 1;
    public float Replicator_Circle_Radius = 1;

    public int Replicator_Square_Size = 4;
    public float Replicator_Square_Width = 4;
    public float Replicator_Square_Depth = 4;

    public int Replicator_Sphere_Size = 1;
    public float Replicator_Sphere_Horizontal_Radius = 1;
    public float Replicator_Sphere_Vertical_Radius = 1;

    //****************************
    // CFG
    //****************************
    public static CFG Current { get; private set; }
    public static CFG Default { get; } = new();

    public string LastProjectFile { get; set; } = "";
    public List<RecentProject> RecentProjects { get; set; } = new();

    public ProjectType Game_Type { get; set; } = ProjectType.Undefined;

    

    public int GFX_Display_Width { get; set; } = 1920;
    public int GFX_Display_Height { get; set; } = 1057;

    public int GFX_Display_X { get; set; } = 0;
    public int GFX_Display_Y { get; set; } = 23;

    public string Param_Export_Delimiter
    {
        get
        {
            if (_Param_Export_Delimiter.Length == 0)
            {
                _Param_Export_Delimiter = Default.Param_Export_Delimiter;
            }
            else if (_Param_Export_Delimiter == "|")
            {
                _Param_Export_Delimiter =
                    Default
                        .Param_Export_Delimiter; // Temporary measure to prevent conflicts with byte array delimiters. Will be removed later.
            }

            return _Param_Export_Delimiter;
        }
        set => _Param_Export_Delimiter = value;
    }

    public static string GetConfigFilePath()
    {
        return $@"{GetConfigFolderPath()}\{Config_FileName}";
    }

    public static string GetBindingsFilePath()
    {
        return $@"{GetConfigFolderPath()}\{Keybinds_FileName}";
    }

    public static string GetConfigFolderPath()
    {
        return $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\{FolderName}";
    }

    private static void LoadConfig()
    {
        if (!File.Exists(GetConfigFilePath()))
        {
            Current = new CFG();
        }
        else
        {
            try
            {
                var options = new JsonSerializerOptions();
                Current = JsonSerializer.Deserialize(File.ReadAllText(GetConfigFilePath()),
                    CfgSerializerContext.Default.CFG);
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

                Current = new CFG();
                SaveConfig();
            }
        }
    }

    private static void LoadKeybinds()
    {
        if (!File.Exists(GetBindingsFilePath()))
        {
            KeyBindings.Current = new KeyBindings.Bindings();
        }
        else
        {
            try
            {
                KeyBindings.Current = JsonSerializer.Deserialize(File.ReadAllText(GetBindingsFilePath()),
                    KeybindingsSerializerContext.Default.Bindings);
                if (KeyBindings.Current == null)
                {
                    throw new Exception("JsonConvert returned null");
                }
            }
            catch (Exception e)
            {
                DialogResult result = PlatformUtils.Instance.MessageBox(
                    $"{e.Message}\n\nKeybinds could not be loaded. Reset keybinds?",
                    $"{Keybinds_FileName} Load Error", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    throw new Exception($"{Keybinds_FileName} could not be loaded.\n\n{e.Message}");
                }

                KeyBindings.Current = new KeyBindings.Bindings();
                SaveKeybinds();
            }
        }
    }

    private static void SaveConfig()
    {
        var json = JsonSerializer.Serialize(
            Current, CfgSerializerContext.Default.CFG);
        File.WriteAllText(GetConfigFilePath(), json);
    }

    private static void SaveKeybinds()
    {
        var json = JsonSerializer.Serialize(
            KeyBindings.Current, KeybindingsSerializerContext.Default.Bindings);
        File.WriteAllText(GetBindingsFilePath(), json);
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
                SaveKeybinds();
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
                LoadKeybinds();
            }
        }
    }

    /// <summary>
    /// Inserts a RecentProject to the top of the list of recent projects.
    /// Updates LastProjectFile and removes any project dupes in the list.
    /// </summary>
    public static void AddMostRecentProject(RecentProject proj)
    {
        foreach (var otherProj in Current.RecentProjects.ToArray())
        {
            if (proj.IsSameProjectLocation(otherProj))
            {
                Current.RecentProjects.Remove(otherProj);
            }
        }

        Current.RecentProjects.Insert(0, proj);

        if (Current.RecentProjects.Count > MAX_RECENT_PROJECTS)
        {
            Current.RecentProjects.RemoveAt(Current.RecentProjects.Count - 1);
        }

        Current.LastProjectFile = proj.ProjectFile;

        CFG.Save();
    }

    /// <summary>
    /// Removes a RecentProject from the list of recent projects.
    /// Also removes any dupes.
    /// </summary>
    public static void RemoveRecentProject(RecentProject proj)
    {
        foreach (var otherProj in Current.RecentProjects.ToArray())
        {
            if (proj.IsSameProjectLocation(otherProj))
            {
                Current.RecentProjects.Remove(otherProj);
            }
        }

        CFG.Save();
    }

    public class RecentProject
    {
        // JsonExtensionData stores info in config file not present in class in order to retain settings between versions.
#pragma warning disable IDE0051
        [JsonExtensionData] public IDictionary<string, JsonElement> AdditionalData { get; set; }
#pragma warning restore IDE0051

        public string Name { get; set; }
        public string ProjectFile { get; set; }
        public ProjectType GameType { get; set; }

        public bool IsSameProjectLocation(RecentProject otherProject)
        {
            if (ProjectFile == otherProject.ProjectFile)
            {
                return true;
            }
            return false;
        }
    }

    public class RenderFilterPreset
    {
        [JsonConstructor]
        public RenderFilterPreset()
        {
        }

        public RenderFilterPreset(string name, RenderFilter filters)
        {
            Name = name;
            Filters = filters;
        }

        public string Name { get; set; }
        public RenderFilter Filters { get; set; }
    }
}
