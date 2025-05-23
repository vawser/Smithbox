using Microsoft.Extensions.Logging;
using StudioCore.Core;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Editors.TextEditor;
using StudioCore.Formats.JSON;
using StudioCore.Scene.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using static StudioCore.Configuration.Settings.TimeActEditorTab;

namespace StudioCore;

public class CFG
{
    ///------------------------------------------------------------
    /// Developer
    ///------------------------------------------------------------
    /// <summary>
    /// If true, the DokuWiki tools for generating the Param page contents will be visible.
    /// </summary>
    public bool EnableWikiTools = false;

    ///------------------------------------------------------------
    /// Project
    ///------------------------------------------------------------
    /// <summary>
    /// The default directory to use for selecting a project directory during Project Creation.
    /// </summary>
    public string DefaultModDirectory = "";

    /// <summary>
    /// The default directory to use for selecting a data directory during Project Creation.
    /// </summary>
    public string DefaultDataDirectory = "";

    /// <summary>
    /// The path to the user's Mod Engine 2 exe.
    /// </summary>
    public string ModEngineInstall = "";

    /// <summary>
    /// The dll arguments to use with the Mod Engine 2 launch.
    /// </summary>
    public string ModEngineDlls = "";

    ///------------------------------------------------------------
    /// Interface
    ///------------------------------------------------------------
    /// <summary>
    /// If true, the Action Logger elements are visible.
    /// </summary>
    public bool System_ShowActionLogger = true;

    /// <summary>
    /// If true, the Warning Logger elements are visible.
    /// </summary>
    public bool System_ShowWarningLogger = true;

    /// <summary>
    /// If true, the alias text will be wrapped if required.
    /// </summary>
    public bool System_WrapAliasDisplay = true;

    /// <summary>
    /// The scale value to apply to interface sizing.
    /// </summary>
    public float System_UI_Scale = 1.0f;

    /// <summary>
    /// If true, the interface sizing is affected by DPI.
    /// </summary>
    public bool System_ScaleByDPI = true;

    /// <summary>
    /// If true, Chinese characters are supported by the ImGui font.
    /// </summary>
    public bool System_Font_Chinese = false;

    /// <summary>
    /// If true, Russian characters are supported by the ImGui font.
    /// </summary>
    public bool System_Font_Cyrillic = false;

    /// <summary>
    /// If true, Korean characters are supported by the ImGui font.
    /// </summary>
    public bool System_Font_Korean = false;

    /// <summary>
    /// If true, Thai characters are supported by the ImGui font.
    /// </summary>
    public bool System_Font_Thai = false;

    /// <summary>
    /// If true, Vietnamese characters are supported by the ImGui font.
    /// </summary>
    public bool System_Font_Vietnamese = false;

    /// <summary>
    /// The relative path (from the Smithbox.exe) to the font used for English text.
    /// </summary>
    public string System_English_Font = "Assets\\Fonts\\RobotoMono-Light.ttf";

    /// <summary>
    /// The relative path (from the Smithbox.exe) to the font used for non-English text.
    /// </summary>
    public string System_Other_Font = "Assets\\Fonts\\NotoSansCJKtc-Light.otf";

    /// <summary>
    /// The current theme a user has selected to override the default.
    /// </summary>
    public string SelectedTheme = "";

    /// <summary>
    /// The size of the font used by ImGui (before scaling).
    /// </summary>
    public float Interface_FontSize = 14.0f;

    /// <summary>
    /// If true, the viewport window (in Map or Model Editor) is visible.
    /// </summary>
    public bool Interface_Editor_Viewport = true;

    /// <summary>
    /// If true, the profiling window (in Map or Model Editor) is visible.
    /// </summary>
    public bool Viewport_Profiling = true;

    /// <summary>
    /// If true, the project list window is visible.
    /// </summary>
    public bool Interface_Editor_ProjectList = true;

    ///------------------------------------------------------------
    /// Map Editor
    ///------------------------------------------------------------
    /// <summary>
    /// If true, the viewport grid is visible in the Map Editor.
    /// </summary>
    public bool Interface_MapEditor_Viewport_Grid = true;

    /// <summary>
    /// If true, the map list window is visible in the Map Editor.
    /// </summary>
    public bool Interface_MapEditor_MapList = true;

    /// <summary>
    /// If true, the map content window is visible in the Map Editor.
    /// </summary>
    public bool Interface_MapEditor_MapContents = true;

    /// <summary>
    /// If true, the properties window is visible in the Map Editor.
    /// </summary>
    public bool Interface_MapEditor_Properties = true;

    /// <summary>
    /// If true, the render groups window is visible in the Map Editor.
    /// </summary>
    public bool Interface_MapEditor_RenderGroups = true;

    /// <summary>
    /// If true, the asset browser window is visible in the Map Editor.
    /// </summary>
    public bool Interface_MapEditor_AssetBrowser = true;

    /// <summary>
    /// If true, the tool window is visible in the Map Editor.
    /// </summary>
    public bool Interface_MapEditor_ToolWindow = true;

    /// <summary>
    /// If true, the resource list window is visible in the Map Editor.
    /// </summary>
    public bool Interface_MapEditor_ResourceList = true;

    /// <summary>
    /// If true, the entity identifier window is visible in the Map Editor.
    /// </summary>
    public bool Interface_MapEditor_EntityIdentifierOverview = false;

    ///------------------------------------------------------------
    /// Model Editor
    ///------------------------------------------------------------
    /// <summary>
    /// If true, the viewport grid is visible in the Model Editor.
    /// </summary>
    public bool Interface_ModelEditor_Viewport_Grid = true;

    /// <summary>
    /// If true, the model hierarchy window is visible in the Model Editor.
    /// </summary>
    public bool Interface_ModelEditor_ModelHierarchy = true;

    /// <summary>
    /// If true, the properties window is visible in the Model Editor.
    /// </summary>
    public bool Interface_ModelEditor_Properties = true;

    /// <summary>
    /// If true, the asset browser window is visible in the Model Editor.
    /// </summary>
    public bool Interface_ModelEditor_AssetBrowser = true;

    /// <summary>
    /// If true, the tool window is visible in the Model Editor.
    /// </summary>
    public bool Interface_ModelEditor_ToolWindow = true;

    /// <summary>
    /// If true, the resource list window is visible in the Model Editor.
    /// </summary>
    public bool Interface_ModelEditor_ResourceList = true;

    ///------------------------------------------------------------
    /// Text Editor
    ///------------------------------------------------------------
    /// <summary>
    /// If true, the file container window is visible in the Text Editor.
    /// </summary>
    public bool Interface_TextEditor_FileContainerList = true;

    /// <summary>
    /// If true, the text file window is visible in the Text Editor.
    /// </summary>
    public bool Interface_TextEditor_FmgList = true;

    /// <summary>
    /// If true, the text entries window is visible in the Text Editor.
    /// </summary>
    public bool Interface_TextEditor_TextEntryList = true;

    /// <summary>
    /// If true, the text entry contents window is visible in the Text Editor.
    /// </summary>
    public bool Interface_TextEditor_TextEntryContents = true;

    /// <summary>
    /// If true, the tool window is visible in the Text Editor.
    /// </summary>
    public bool Interface_TextEditor_ToolWindow = true;


    ///------------------------------------------------------------
    /// Param Editor
    ///------------------------------------------------------------
    /// <summary>
    /// If true, the project Meta files are used instead of the primary Meta files.
    /// </summary>
    public bool UseProjectMeta = false;

    /// <summary>
    /// If true, then loose params are prioitized over packed params (for games where it is relevant)
    /// </summary>
    public bool UseLooseParams = false;

    /// <summary>
    /// If true, then loose params are repacked into the enc_regulation for DS2 projects.
    /// </summary>
    public bool RepackLooseDS2Params = false;

    /// <summary>
    /// If true, then row name restore will use index matching. If false, it will use ID matching.
    /// </summary>
    public bool UseIndexMatchForRowNameRestore = true;

    /// <summary>
    /// The delimiter character to use when exporting param data.
    /// </summary>
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
    private string _Param_Export_Delimiter = ",";

    /// <summary>
    /// If true, the editor window is visible in the Param Editor.
    /// </summary>
    public bool Interface_ParamEditor_Table = true;

    /// <summary>
    /// If true, the tool window is visible in the Param Editor.
    /// </summary>
    public bool Interface_ParamEditor_ToolWindow = true;

    ///------------------------------------------------------------
    /// Graphics Param Editor
    ///------------------------------------------------------------
    /// <summary>
    /// If true, the file list window is visible in the Graphics Param Editor.
    /// </summary>
    public bool Interface_GparamEditor_FileList = true;

    /// <summary>
    /// If true, the group list window is visible in the Graphics Param Editor.
    /// </summary>
    public bool Interface_GparamEditor_GroupList = true;

    /// <summary>
    /// If true, the field list window is visible in the Graphics Param Editor.
    /// </summary>
    public bool Interface_GparamEditor_FieldList = true;

    /// <summary>
    /// If true, the field values window is visible in the Graphics Param Editor.
    /// </summary>
    public bool Interface_GparamEditor_FieldValues = true;

    /// <summary>
    /// If true, the tool window is visible in the Graphics Param Editor.
    /// </summary>
    public bool Interface_GparamEditor_ToolWindow = true;


    ///------------------------------------------------------------
    /// Texture Viewer
    ///------------------------------------------------------------
    /// <summary>
    /// If true, the file list window is visible in the Texture Viewer.
    /// </summary>
    public bool Interface_TextureViewer_Files = true;

    /// <summary>
    /// If true, the texture list window is visible in the Texture Viewer.
    /// </summary>
    public bool Interface_TextureViewer_Textures = true;

    /// <summary>
    /// If true, the texture view window is visible in the Texture Viewer.
    /// </summary>
    public bool Interface_TextureViewer_Viewer = true;

    /// <summary>
    /// If true, the texture properties window is visible in the Texture Viewer.
    /// </summary>
    public bool Interface_TextureViewer_Properties = true;

    /// <summary>
    /// If true, the tool window is visible in the Texture Viewer.
    /// </summary>
    public bool Interface_TextureViewer_ToolWindow = true;

    /// <summary>
    /// If true, the resource list window is visible in the Texture Viewer.
    /// </summary>
    public bool Interface_TextureViewer_ResourceList = true;

    ///------------------------------------------------------------
    /// Time Act Editor
    ///------------------------------------------------------------
    /// <summary>
    /// If true, the container file list window is visible in the Time Act Editor.
    /// </summary>
    public bool Interface_TimeActEditor_ContainerFileList = true;

    /// <summary>
    /// If true, the time act list window is visible in the Time Act Editor.
    /// </summary>
    public bool Interface_TimeActEditor_TimeActList = true;

    /// <summary>
    /// If true, the animation list window is visible in the Time Act Editor.
    /// </summary>
    public bool Interface_TimeActEditor_AnimationList = true;

    /// <summary>
    /// If true, the animation properties window is visible in the Time Act Editor.
    /// </summary>
    public bool Interface_TimeActEditor_AnimationProperties = true;

    /// <summary>
    /// If true, the event list window is visible in the Time Act Editor.
    /// </summary>
    public bool Interface_TimeActEditor_EventList = true;

    /// <summary>
    /// If true, the event properties window is visible in the Time Act Editor.
    /// </summary>
    public bool Interface_TimeActEditor_EventProperties = true;

    /// <summary>
    /// If true, the tool window is visible in the Time Act Editor.
    /// </summary>
    public bool Interface_TimeActEditor_ToolWindow = true;

    ///------------------------------------------------------------
    /// Material Editor
    ///------------------------------------------------------------

    ///------------------------------------------------------------
    /// Event Script Editor
    ///------------------------------------------------------------
    public bool Interface_EmevdEditor_Files = true;
    public bool Interface_EmevdEditor_Events = true;
    public bool Interface_EmevdEditor_Instructions = true;
    public bool Interface_EmevdEditor_EventProperties = true;
    public bool Interface_EmevdEditor_InstructionProperties = true;
    public bool Interface_EmevdEditor_ToolConfigurationWindow = true;

    ///------------------------------------------------------------
    /// EzState Script Editor
    ///------------------------------------------------------------
    public bool Interface_EsdEditor_FileList = true;
    public bool Interface_EsdEditor_ScriptList = true;
    public bool Interface_EsdEditor_StateGroupList = true;
    public bool Interface_EsdEditor_StateNodeList = true;
    public bool Interface_EsdEditor_StateNodeContents = true;
    public bool Interface_EsdEditor_ToolConfigurationWindow = true;

    ///------------------------------------------------------------
    /// File Browser
    ///------------------------------------------------------------
    public bool Interface_FileBrowser_FileList = true;
    public bool Interface_FileBrowser_ItemViewer = true;

    //**************
    // Actions
    //**************
    public Vector3 SavedPosition = new Vector3();
    public Vector3 SavedRotation = new Vector3();
    public Vector3 SavedScale = new Vector3();

    //**************
    // Project
    //**************
    public string PTDE_Collision_Root = "";
    public bool PTDE_Collision_Root_Warning = true;

    //****************************
    // Settings: System
    //****************************
    public bool System_Check_Program_Update = true;
    public float System_Frame_Rate = 60.0f;
    public bool System_Enable_Soapstone_Server = true;
    public bool System_IgnoreAsserts = false;
    public bool System_UseDCXHeuristicOnReadFailure = false;

    public bool System_EnableAutoSave = true;
    public int System_AutoSaveIntervalSeconds = 300;
    public bool System_EnableAutoSave_Project = true;
    public bool System_EnableAutoSave_MapEditor = false;
    public bool System_EnableAutoSave_ModelEditor = false;
    public bool System_EnableAutoSave_ParamEditor = false;
    public bool System_EnableAutoSave_TextEditor = false;
    public bool System_EnableAutoSave_GparamEditor = false;

    public bool System_EnableRecoveryFolder = true;

    public int System_ActionLogger_FadeTime = 1500;
    public int System_WarningLogger_FadeTime = 1500;

    //****************************
    // Settings: Viewport Grid
    //****************************
    public bool MapEditor_Viewport_RegenerateMapGrid = false;
    public int MapEditor_Viewport_GridType = 0;
    public int MapEditor_Viewport_Grid_Size = 1000;
    public int MapEditor_Viewport_Grid_Square_Size = 10;
    public float MapEditor_Viewport_Grid_Height = 0;
    public float MapEditor_Viewport_Grid_Height_Increment = 1;
    public Vector3 MapEditor_Viewport_Grid_Color = new Vector3(0.5f, 0.5f, 0.5f);

    public bool ModelEditor_Viewport_RegenerateMapGrid = false;
    public int ModelEditor_Viewport_GridType = 0;
    public int ModelEditor_Viewport_Grid_Size = 1000;
    public int ModelEditor_Viewport_Grid_Square_Size = 10;
    public float ModelEditor_Viewport_Grid_Height = 0;
    public float ModelEditor_Viewport_Grid_Height_Increment = 1;
    public Vector3 ModelEditor_Viewport_Grid_Color = new Vector3(0.5f, 0.5f, 0.5f);

    //****************************
    // Settings: Viewport
    //****************************
    public bool Viewport_Enable_Rendering = true;
    public bool Viewport_Enable_Texturing = true;
    public bool Viewport_Enable_Culling = true;

    public bool Viewport_Enable_ER_Auto_Map_Offset = true;
    public bool Viewport_Enable_Selection_Outline = true;
    public bool Viewport_Enable_Model_Masks = true;
    public bool Viewport_Enable_LOD_Facesets = false;


    public Vector3 Viewport_BackgroundColor = Utils.GetDecimalColor(Color.Gray);

    // Camera
    public float Viewport_Camera_FOV { get; set; } = 60.0f;
    public float Viewport_Camera_MoveSpeed_Slow { get; set; } = 1.0f;
    public float Viewport_Camera_MoveSpeed_Normal { get; set; } = 20.0f;
    public float Viewport_Camera_MoveSpeed_Fast { get; set; } = 200.0f;
    public float Viewport_Camera_Sensitivity { get; set; } = 0.0160f;
    public float Viewport_RenderDistance_Min { get; set; } = 0.1f;
    public float Viewport_RenderDistance_Max { get; set; } = 50000.0f;

    // Rendering Limits
    public uint Viewport_Limit_Buffer_Flver_Bone = 65536;
    public uint Viewport_Limit_Buffer_Indirect_Draw = 50000;
    public int Viewport_Limit_Renderables = 50000;

    // Wireframe Coloring
    public float GFX_Wireframe_Color_Variance = 0.11f;

    public float GFX_Renderable_Default_Wireframe_Alpha = 100.0f;

    public Vector3 GFX_Renderable_Box_BaseColor = Utils.GetDecimalColor(Color.Blue);
    public Vector3 GFX_Renderable_Box_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);
    public float GFX_Renderable_Box_Alpha = 75.0f;

    public Vector3 GFX_Renderable_Cylinder_BaseColor = Utils.GetDecimalColor(Color.Blue);
    public Vector3 GFX_Renderable_Cylinder_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);
    public float GFX_Renderable_Cylinder_Alpha = 75.0f;

    public Vector3 GFX_Renderable_Sphere_BaseColor = Utils.GetDecimalColor(Color.Blue);
    public Vector3 GFX_Renderable_Sphere_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);
    public float GFX_Renderable_Sphere_Alpha = 75.0f;

    public Vector3 GFX_Renderable_Point_BaseColor = Utils.GetDecimalColor(Color.Yellow);
    public Vector3 GFX_Renderable_Point_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);
    public float GFX_Renderable_Point_Alpha = 75.0f;

    public Vector3 GFX_Renderable_DummyPoly_BaseColor = Utils.GetDecimalColor(Color.Yellow);
    public Vector3 GFX_Renderable_DummyPoly_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);
    public float GFX_Renderable_DummyPoly_Alpha = 75.0f;

    public Vector3 GFX_Renderable_BonePoint_BaseColor = Utils.GetDecimalColor(Color.Blue);
    public Vector3 GFX_Renderable_BonePoint_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);
    public float GFX_Renderable_BonePoint_Alpha = 75.0f;

    public Vector3 GFX_Renderable_ModelMarker_Chr_BaseColor = Utils.GetDecimalColor(Color.Firebrick);
    public Vector3 GFX_Renderable_ModelMarker_Chr_HighlightColor = Utils.GetDecimalColor(Color.Tomato);
    public float GFX_Renderable_ModelMarker_Chr_Alpha = 75.0f;

    public Vector3 GFX_Renderable_ModelMarker_Object_BaseColor = Utils.GetDecimalColor(Color.MediumVioletRed);
    public Vector3 GFX_Renderable_ModelMarker_Object_HighlightColor = Utils.GetDecimalColor(Color.DeepPink);
    public float GFX_Renderable_ModelMarker_Object_Alpha = 75.0f;

    public Vector3 GFX_Renderable_ModelMarker_Player_BaseColor = Utils.GetDecimalColor(Color.DarkOliveGreen);
    public Vector3 GFX_Renderable_ModelMarker_Player_HighlightColor = Utils.GetDecimalColor(Color.OliveDrab);
    public float GFX_Renderable_ModelMarker_Player_Alpha = 75.0f;

    public Vector3 GFX_Renderable_ModelMarker_Other_BaseColor = Utils.GetDecimalColor(Color.Wheat);
    public Vector3 GFX_Renderable_ModelMarker_Other_HighlightColor = Utils.GetDecimalColor(Color.AntiqueWhite);
    public float GFX_Renderable_ModelMarker_Other_Alpha = 75.0f;

    public Vector3 GFX_Renderable_PointLight_BaseColor = Utils.GetDecimalColor(Color.YellowGreen);
    public Vector3 GFX_Renderable_PointLight_HighlightColor = Utils.GetDecimalColor(Color.Yellow);
    public float GFX_Renderable_PointLight_Alpha = 75.0f;

    public Vector3 GFX_Renderable_SpotLight_BaseColor = Utils.GetDecimalColor(Color.Goldenrod);
    public Vector3 GFX_Renderable_SpotLight_HighlightColor = Utils.GetDecimalColor(Color.Violet);
    public float GFX_Renderable_SpotLight_Alpha = 75.0f;

    public Vector3 GFX_Renderable_DirectionalLight_BaseColor = Utils.GetDecimalColor(Color.Cyan);
    public Vector3 GFX_Renderable_DirectionalLight_HighlightColor = Utils.GetDecimalColor(Color.AliceBlue);
    public float GFX_Renderable_DirectionalLight_Alpha = 75.0f;

    public Vector3 GFX_Gizmo_X_BaseColor = new(0.952f, 0.211f, 0.325f);
    public Vector3 GFX_Gizmo_X_HighlightColor = new(1.0f, 0.4f, 0.513f);

    public Vector3 GFX_Gizmo_Y_BaseColor = new(0.525f, 0.784f, 0.082f);
    public Vector3 GFX_Gizmo_Y_HighlightColor = new(0.713f, 0.972f, 0.270f);

    public Vector3 GFX_Gizmo_Z_BaseColor = new(0.219f, 0.564f, 0.929f);
    public Vector3 GFX_Gizmo_Z_HighlightColor = new(0.407f, 0.690f, 1.0f);

    public float Viewport_DefaultRender_Brightness = 1.0f;
    public float Viewport_DefaultRender_Saturation = 0.5f;

    public Vector3 Viewport_DefaultRender_SelectColor = new(1.0f, 0.5f, 0.0f);

    public bool Viewport_DisplayControls = true;
    public bool Viewport_DisplayRotationIncrement = true;
    public bool Viewport_DisplayMovementIncrement = true;
    public bool Viewport_Enable_BoxSelection = true;
    public float Viewport_BS_DistThresFactor = 1.2f;

    //****************************
    // Settings: Map Editor
    //****************************
    public bool MapEditor_EnableMapUnload = true;
    public bool MapEditor_IgnoreSaveExceptions = false;

    public bool MapEditor_DisplayMapCategories = true;

    public bool MapEditor_LoadMapQueryData = true;

    public bool MapEditor_AssetBrowser_ShowAliases = true;
    public bool MapEditor_AssetBrowser_ShowTags = false;
    public bool MapEditor_AssetBrowser_ShowLowDetailParts = false;

    public bool MapEditor_LoadCollisions_ER = true;

    public bool MapEditor_ShowMapGroups = true;
    public bool MapEditor_ShowWorldMapButtons = true;
    public bool MapEditor_MapObjectList_ShowMapNames = true;

    public bool MapEditor_Always_List_Loaded_Maps = true;

    public bool MapEditor_Enable_Commmunity_Names = true;
    public bool MapEditor_Enable_Padding_Fields = true;
    public bool MapEditor_Enable_Obsolete_Fields = true;

    public bool MapEditor_Enable_Property_Info = false;
    public bool MapEditor_Enable_Map_Load_on_Double_Click = false;
    public bool MapEditor_Enable_Property_Filter = true;
    public bool MapEditor_Enable_Param_Quick_Links = true;
    public bool MapEditor_Enable_Referenced_Rename = false;

    public bool MapEditor_Enable_Property_Property_TopDecoration = false;
    public bool MapEditor_Enable_Property_Property_Class_Info = true;
    public bool MapEditor_Enable_Property_Property_SpecialProperty_Info = true;
    public bool MapEditor_Enable_Property_Property_ReferencesTo = true;
    public bool MapEditor_Enable_Property_Property_ReferencesBy = true;

    public bool MapEditor_MapObjectList_ShowListSortingType = true;
    public bool MapEditor_MapObjectList_ShowMapIdSearch = true;
    public bool MapEditor_MapObjectList_ShowMapContentSearch = true;

    public bool MapEditor_MapObjectList_ShowCharacterNames = true;
    public bool MapEditor_MapObjectList_ShowAssetNames = true;
    public bool MapEditor_MapObjectList_ShowMapPieceNames = true;
    public bool MapEditor_MapObjectList_ShowPlayerCharacterNames = true;
    public bool MapEditor_MapObjectList_ShowSystemCharacterNames = true;
    public bool MapEditor_MapObjectList_ShowTreasureNames = true;

    public bool MapEditor_Substitute_PseudoPlayer_Model = false;
    public string MapEditor_Substitute_PseudoPlayer_ChrID = "c0000";

    public bool MapEditor_SelectionGroup_FrameSelection = true;
    public bool MapEditor_SelectionGroup_AutoCreation = false;
    public bool MapEditor_SelectionGroup_ConfirmDelete = true;
    public bool MapEditor_SelectionGroup_ShowKeybind = true;
    public bool MapEditor_SelectionGroup_ShowTags = false;

    public int MapEditor_Selection_Movement_IncrementType = 0;
    public float MapEditor_Selection_Movement_Increment_0 { get; set; } = 0.05f;
    public float MapEditor_Selection_Movement_Increment_1 { get; set; } = 0.1f;
    public float MapEditor_Selection_Movement_Increment_2 { get; set; } = 0.25f;
    public float MapEditor_Selection_Movement_Increment_3 { get; set; } = 0.5f;
    public float MapEditor_Selection_Movement_Increment_4 { get; set; } = 1.0f;


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
    public bool Prefab_ApplySpecificEntityGroupID = false;
    public int Prefab_SpecificEntityGroupID = 0;

    public bool Prefab_ApplyOverrideName = false;
    public string Prefab_OverrideName = "";

    public float WorldMapHeight = 1200f;
    public float WorldMapWidth = 1200f;
    public bool WorldMapDisplayTiles = true;
    public bool WorldMapDisplaySmallTiles = true;
    public bool WorldMapDisplayMediumTiles = false;
    public bool WorldMapDisplayLargeTiles = false;

    public bool QuickView_DisplayTooltip = false;
    public List<string> QuickView_TargetProperties = new List<string>() { "Name" };

    public bool DisplayPlacementOrb = false;
    public float PlacementOrb_Distance = 5.0f;

    //****************************
    // Settings: Model Editor
    //****************************
    public bool ModelEditor_AssetBrowser_ShowAliases = true;
    public bool ModelEditor_AssetBrowser_ShowTags = false;
    public bool ModelEditor_AssetBrowser_ShowLowDetailParts = false;

    public bool ModelEditor_ViewMeshes = true;
    public bool ModelEditor_ViewDummyPolys = true;
    public bool ModelEditor_ViewBones = true;
    public bool ModelEditor_ViewSkeleton = true;
    public bool ModelEditor_ViewHighCollision = false;
    public bool ModelEditor_ViewLowCollision = true;

    public bool ModelEditor_ExactSearch = false;

    public bool ModelEditor_Enable_Commmunity_Names = true;
    public bool ModelEditor_Enable_Commmunity_Hints = true;
    public bool ModelEditor_DisplayDmyPolyReferenceID = true;
    public bool ModelEditor_DisplayMatNameOnMesh = true;

    public string ModelEditor_Toolbar_DuplicateFile_NewName = "";

    public int ModelEditor_Toolbar_DuplicateProperty_Amount = 1;

    public bool ModelEditor_Toolbar_DeleteProperty_FaceSetsOnly = true;

    public ModelExportType ModelEditor_ExportType = ModelExportType.OBJ;

    //****************************
    // Settings: Param Editor
    //****************************
    public bool Param_UseLooseParams = false;

    // Done per project so we can default ON for DS2 and ER without forcing stripping for the others
    public bool Param_StripRowNamesOnSave_DES = false;
    public bool Param_StripRowNamesOnSave_DS1 = false;
    public bool Param_StripRowNamesOnSave_DS2 = true;
    public bool Param_StripRowNamesOnSave_BB = false;
    public bool Param_StripRowNamesOnSave_DS3 = false;
    public bool Param_StripRowNamesOnSave_SDT = false;
    public bool Param_StripRowNamesOnSave_ER = true;
    public bool Param_StripRowNamesOnSave_AC6 = false;

    public bool Param_RestoreStrippedRowNamesOnLoad_DES = false;
    public bool Param_RestoreStrippedRowNamesOnLoad_DS1 = false;
    public bool Param_RestoreStrippedRowNamesOnLoad_DS2 = true;
    public bool Param_RestoreStrippedRowNamesOnLoad_BB = false;
    public bool Param_RestoreStrippedRowNamesOnLoad_DS3 = false;
    public bool Param_RestoreStrippedRowNamesOnLoad_SDT = false;
    public bool Param_RestoreStrippedRowNamesOnLoad_ER = true;
    public bool Param_RestoreStrippedRowNamesOnLoad_AC6 = false;

    public bool Param_SaveERAsDFLT = false;

    public bool UI_CompactParams = false;

    public bool Param_UseProjectMeta = false;

    public bool Param_AlphabeticalParams = true;
    public bool Param_ShowParamCommunityName = false;
    public bool Param_ShowSecondaryNames = true;

    public bool Param_DisplayParamCategories = true;

    public bool Param_AdvancedMassedit = false;
    public bool Param_AllowFieldReorder = true;
    public bool Param_DisableLineWrapping = false;
    public bool Param_DisableRowGrouping = false;
    public bool Param_HideEnums = false;
    public bool Param_HideReferenceRows = false;
    public bool Param_MakeMetaNamesPrimary = true;
    public bool Param_PasteAfterSelection = false;
    public bool Param_PasteThenSelect = true;
    public bool Param_ShowFieldOffsets = false;
    public bool Param_ShowFieldEnumLabels = true;
    public bool Param_ShowFieldParamLabels = true;
    public bool Param_ShowFieldFmgLabels = true;
    public bool Param_ShowFieldTextureLabels = true;

    public bool Param_ShowVanillaColumn = true;
    public bool Param_ShowAuxColumn = true;

    public bool Param_HidePaddingFields = true;
    public bool Param_HideObsoleteFields = true;
    public bool Param_ShowColorPreview = true;
    public bool Param_ShowGraphVisualisation = true;
    public bool Param_PinnedRowsStayVisible = true;
    public bool Param_ViewInMapOption = true;
    public bool Param_ViewModelOption = true;

    public bool Param_ShowFieldDescription_onIcon = true;
    public bool Param_ShowFieldLimits_onIcon = true;
    public bool Param_ShowFieldDescription_onName = true;
    public bool Param_ShowFieldLimits_onName = true;

    public bool Param_ShowTraditionalPercentages = false;

    public bool Param_MassEdit_ShowAddButtons = true;

    public float Param_ParamContextMenu_Width = 300f;

    public float Param_RowContextMenu_Width = 300f;
    public bool Param_RowContextMenu_NameInput = true;
    public bool Param_RowContextMenu_ShortcutTools = true;
    public bool Param_RowContextMenu_PinOptions = true;
    public bool Param_RowContextMenu_CompareOptions = true;
    public bool Param_RowContextMenu_ReverseLoopup = true;
    public bool Param_RowContextMenu_CopyID = true;

    public float Param_FieldContextMenu_Width = 300f;
    public bool Param_FieldContextMenu_Split = false;
    public bool Param_FieldContextMenu_Name = false;
    public bool Param_FieldContextMenu_Description = false;
    public bool Param_FieldContextMenu_PropertyInfo = false;
    public bool Param_FieldContextMenu_PinOptions = true;
    public bool Param_FieldContextMenu_CompareOptions = true;
    public bool Param_FieldContextMenu_ValueDistribution = true;
    public bool Param_FieldContextMenu_AddOptions = true;
    public bool Param_FieldContextMenu_ReferenceSearch = true;
    public bool Param_FieldContextMenu_References = true;
    public bool Param_FieldContextMenu_MassEdit = true;
    public bool Param_FieldContextMenu_FullMassEdit = true;
    public bool Param_FieldContextMenu_ImagePreview_ContextMenu = false;
    public bool Param_FieldContextMenu_ImagePreview_FieldColumn = true;
    public float Param_FieldContextMenu_ImagePreviewScale = 1.0f;

    public int Param_Toolbar_Duplicate_Amount = 1;
    public int Param_Toolbar_Duplicate_Offset = 1;

    public string Param_Toolbar_CommutativeDuplicate_Target = "";
    public int Param_Toolbar_CommutativeDuplicate_Offset = 0;
    public bool Param_Toolbar_CommutativeDuplicate_ReplaceExistingRows = true;

    public bool Param_Toolbar_Duplicate_AffectAttackField = false;
    public bool Param_Toolbar_Duplicate_AffectBehaviorField = false;
    public bool Param_Toolbar_Duplicate_AffectBulletField = false;
    public bool Param_Toolbar_Duplicate_AffectSpEffectField = false;
    public bool Param_Toolbar_Duplicate_AffectSourceField = false;

    public bool Param_Toolbar_FindValueInstances_InitialMatchOnly = false;

    public bool Param_PinGroups_ShowOnlyPinnedParams = false;
    public bool Param_PinGroups_ShowOnlyPinnedRows = false;
    public bool Param_PinGroups_ShowOnlyPinnedFields = false;

    //****************************
    // Settings: Text Editor
    //****************************
    public TextContainerCategory TextEditor_PrimaryCategory = TextContainerCategory.English;

    public bool TextEditor_AdvancedPresentationMode = false;
    public bool TextEditor_SimpleFileList = true;
    public bool TextEditor_SimpleFmgList = true;

    public bool TextEditor_IncludeNonPrimaryContainers = true;
    public bool TextEditor_IncludeVanillaCache = true;
    public bool TextEditor_DisplayFmgPrecedenceHint = true;

    public bool TextEditor_DisplayPrimaryCategoryOnly = false;
    public bool TextEditor_DisplayCommunityContainerName = true;
    public bool TextEditor_DisplaySourcePath = true;
    public bool TextEditor_DisplayContainerPrecedenceHint = true;

    public bool TextEditor_DisplayFmgID = false;
    public bool TextEditor_DisplayFmgPrettyName = true;

    public bool TextEditor_DisplayNullEntries = true;
    public bool TextEditor_DisplayNullPlaceholder = true;
    public bool TextEditor_TruncateTextDisplay = true;

    public bool TextEditor_Entry_DisplayGroupedEntries = true;
    public bool TextEditor_Entry_AllowDuplicateIds = false;

    public bool TextEditor_TextCopy_EscapeNewLines = true;
    public bool TextEditor_TextCopy_IncludeID = true;

    public int TextEditor_CreationModal_CreationCount = 1;
    public int TextEditor_CreationModal_IncrementCount = 1;
    public bool TextEditor_CreationModal_UseIncrementalTitling = false;
    public string TextEditor_CreationModal_IncrementalTitling_Prefix = "+";
    public string TextEditor_CreationModal_IncrementalTitling_Postfix = "";

    public bool TextEditor_CreationModal_UseIncrementalNaming = false;
    public string TextEditor_CreationModal_IncrementalNaming_Template = "";

    public bool TextEditor_IgnoreIdOnDuplicate = false;

    public bool TextEditor_TextExport_IncludeGroupedEntries = true;
    public bool TextEditor_TextExport_UseQuickExport = false;
    public string TextEditor_TextExport_QuickExportPrefix = "quick_export";

    public bool TextEditor_LanguageSync_PrimaryOnly = false;
    public bool TextEditor_LanguageSync_AddPrefix = true;
    public string TextEditor_LanguageSync_Prefix = "TRANSLATE: ";

    public bool TextEditor_EnableAutomaticDifferenceCheck = false;

    // OLD
    public string FMG_SearchAndReplace_SearchText = "";
    public string FMG_SearchAndReplace_ReplaceText = "";
    public bool FMG_SearchAndReplace_Regex_IgnoreCase = false;
    public bool FMG_SearchAndReplace_Regex_Multiline = false;
    public bool FMG_SearchAndReplace_Regex_Singleline = false;
    public bool FMG_SearchAndReplace_Regex_IgnorePatternWhitespace = false;

    //****************************
    // Settings: Gparam Editor
    //****************************
    public bool Interface_Display_Alias_for_Gparam = true;

    public bool Gparam_DisplayParamGroupAlias = true;
    public bool Gparam_DisplayParamFieldAlias = false;

    public bool Gparam_DisplayColorEditForVector4Fields = true;
    public bool Gparam_DisplayEmptyGroups = true;
    public bool Gparam_DisplayAddGroups = true;
    public bool Gparam_DisplayAddFields = true;

    public bool Gparam_ColorEdit_RGB = true;
    public bool Gparam_ColorEdit_Decimal = false;
    public bool Gparam_ColorEdit_HSV = false;

    public string Gparam_QuickEdit_Chain = "+";

    public string Gparam_QuickEdit_File = "file";
    public string Gparam_QuickEdit_Group = "group";
    public string Gparam_QuickEdit_Field = "field";

    public string Gparam_QuickEdit_ID = "id";
    public string Gparam_QuickEdit_TimeOfDay = "tod";
    public string Gparam_QuickEdit_Value = "value";
    public string Gparam_QuickEdit_Index = "index";

    public string Gparam_QuickEdit_Set = "set";
    public string Gparam_QuickEdit_Add = "add";
    public string Gparam_QuickEdit_Subtract = "sub";
    public string Gparam_QuickEdit_Multiply = "mult";
    public string Gparam_QuickEdit_SetByRow = "setbyrow";
    public string Gparam_QuickEdit_Restore = "restore";
    public string Gparam_QuickEdit_Random = "random";

    //****************************
    // Settings: Particle Editor
    //****************************
    public bool Interface_Display_Alias_for_Particles = true;

    //****************************
    // Settings: Time Act Editor
    //****************************
    public TimeactCompressionType CurrentTimeActCompressionType = TimeactCompressionType.Default;

    public bool TimeActEditor_Load_CharacterTimeActs = true;
    public bool TimeActEditor_Load_ObjectTimeActs = false;
    public bool TimeActEditor_Load_VanillaCharacterTimeActs = true;
    public bool TimeActEditor_Load_VanillaObjectTimeActs = false;

    public bool TimeActEditor_DisplayTimeActRow_AliasInfo = true;

    public bool TimeActEditor_DisplayAnimFileName = true;
    public bool TimeActEditor_DisplayAnimRow_GeneratorInfo = true;

    public bool TimeActEditor_DisplayAllGenerators = false;

    public bool TimeActEditor_DisplayEventBank = true;
    public bool TimeActEditor_DisplayEventID = true;
    public bool TimeActEditor_DisplayEventRow_EnumInfo = true;
    public bool TimeActEditor_DisplayEventRow_ParamRefInfo = true;
    public bool TimeActEditor_DisplayEventRow_DataAliasInfo = true;
    public bool TimeActEditor_DisplayEventRow_DataAliasInfo_IncludeAliasName = true;
    public bool TimeActEditor_DisplayEventRow_ProjectEnumInfo = true;

    public bool TimeActEditor_DisplayPropertyType = false;

    public bool TimeActEditor_Viewport_Grid = true;

    public bool TimeActEditor_Viewport_RegenerateMapGrid = false;
    public int TimeActEditor_Viewport_GridType = 0;
    public int TimeActEditor_Viewport_Grid_Size = 100;
    public int TimeActEditor_Viewport_Grid_Square_Size = 10;
    public float TimeActEditor_Viewport_Grid_Height = 0;
    public Vector3 TimeActEditor_Viewport_Grid_Color = new Vector3(0.5f, 0.5f, 0.5f);

    //****************************
    // Settings: EMEVD Editor
    //****************************
    public bool EmevdEditor_PropagateFilterCommands = true;

    public bool EmevdEditor_DisplayInstructionCategory = false;
    public bool EmevdEditor_DisplayInstructionParameterNames = false;

    //****************************
    // Settings: ESD Editor
    //****************************
    public bool EsdEditor_PropagateFilterCommands = true;

    //****************************
    // Settings: Texture Viewer
    //****************************
    public bool TextureViewer_FileList_ShowLowDetail_Entries = true;

    public bool TextureViewer_FileList_ShowAliasName_Characters = true;
    public bool TextureViewer_FileList_ShowAliasName_Assets = true;
    public bool TextureViewer_FileList_ShowAliasName_Parts = true;

    public bool TextureViewer_TextureList_ShowAliasName_Particles = true;

    public string TextureViewerToolbar_ExportTextureLocation = "";
    public bool TextureViewerToolbar_ExportTexture_IncludeFolder = true;
    public bool TextureViewerToolbar_ExportTexture_DisplayConfirm = true;
    public int TextureViewerToolbar_ExportTextureType = 0;

    //****************************
    // Windows
    //****************************
    public bool AliasBank_EditorMode = false;

    public bool AssetBrowser_ShowAliasesInBrowser = true;
    public bool AssetBrowser_ShowTagsInBrowser = true;
    public bool AssetBrowser_ShowLowDetailParts = false;
    public bool AssetBrowser_UpdateName = true;
    public bool AssetBrowser_UpdateInstanceID = true;

    public bool MapAtlas_ShowTags = true;
    public bool CharacterAtlas_ShowTags = true;
    public bool AssetAtlas_ShowTags = true;
    public bool PartAtlas_ShowTags = true;
    public bool MapPieceAtlas_ShowTags = true;
    public bool EventFlagAtlas_ShowTags = true;
    public bool ParticleAtlas_ShowTags = true;
    public bool MapNameAtlas_ShowTags = true;
    public bool GparamNameAtlas_ShowTags = true;
    public bool SoundAtlas_ShowTags = true;
    public bool CutsceneAtlas_ShowTags = true;
    public bool MovieAtlas_ShowTags = true;
    public bool TimeActAtlas_ShowTags = true;
    public bool TalkAtlas_ShowTags = true;

    //****************************
    // Map Toolbar
    //****************************

    public bool Toolbar_Duplicate_Increment_Entity_ID = false;
    public bool Toolbar_Duplicate_Increment_PartNames = false;
    public bool Toolbar_Duplicate_Increment_InstanceID = true;

    public bool Toolbar_Duplicate_Clear_Entity_ID = false;
    public bool Toolbar_Duplicate_Clear_Entity_Group_IDs = false;

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

    public int Toolbar_Rotate_IncrementType = 0;
    public float Toolbar_Rotate_Increment_0 { get; set; } = 90.0f;
    public float Toolbar_Rotate_Increment_1 { get; set; } = 45.0f;
    public float Toolbar_Rotate_Increment_2 { get; set; } = 30.0f;
    public float Toolbar_Rotate_Increment_3 { get; set; } = 15.0f;
    public float Toolbar_Rotate_Increment_4 { get; set; } = 5.0f;

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
    public bool Replicator_Increment_PartNames = false;
    public bool Replicator_Increment_InstanceID = true;

    public bool Replicator_Clear_Entity_ID = false;
    public bool Replicator_Clear_Entity_Group_IDs = false;

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

    public int Toolbar_EntityGroupID = 0;
    public string Toolbar_EntityGroup_Attribute = "";

    public int SelectedGameOffsetData = 0;

    public int GFX_Display_Width { get; set; } = 1920;
    public int GFX_Display_Height { get; set; } = 1057;

    public int GFX_Display_X { get; set; } = 0;
    public int GFX_Display_Y { get; set; } = 23;

    // JsonExtensionData stores info in config file not present in class in order to retain settings between versions.
#pragma warning disable IDE0051
    [JsonExtensionData] public IDictionary<string, JsonElement> AdditionalData;
#pragma warning restore IDE0051

    //****************************
    // CFG
    //****************************
    public static CFG Current { get; private set; }
    public static CFG Default { get; } = new();

    public static void Setup()
    {
        Current = new CFG();
    }

    public static void Load()
    {
        var folder = ProjectUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Configuration.json");

        if (!File.Exists(file))
        {
            Current = new CFG();
            Save();
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var options = new JsonSerializerOptions();
                Current = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.CFG);

                if (Current == null)
                {
                    throw new Exception("JsonConvert returned null");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Configuration failed to load, default configuration has been restored.", LogLevel.Error, Tasks.LogPriority.High, e);

                Current = new CFG();
                Save();
            }
        }
    }

    public static void Save()
    {
        var folder = ProjectUtils.GetConfigurationFolder();
        var file = Path.Combine(folder, "Configuration.json");

        if(!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var json = JsonSerializer.Serialize(Current, SmithboxSerializerContext.Default.CFG);

        File.WriteAllText(file, json);
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