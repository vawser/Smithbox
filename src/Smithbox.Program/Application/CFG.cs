using Microsoft.Extensions.Logging;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudioCore.Application;

public class CFG
{
    ///------------------------------------------------------------
    /// System
    ///------------------------------------------------------------
    public bool EnableDeveloperTools = false;
    public string SmithboxBuildFolder = "";

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

    ///------------------------------------------------------------
    /// Project
    ///------------------------------------------------------------

    public bool Project_Enable_Auto_Load = true;

    public bool EnableAutomaticSave = true;
    public bool EnableBackupSaves = true;

    public float AutomaticSaveIntervalTime = 300;

    public string DefaultModDirectory = "";
    public string DefaultDataDirectory = "";

    public string ModEngine3ProfileDirectory = "";
    public string ModEngine2Install = "";
    public string ModEngine2Dlls = "";

    public ProjectBackupBehaviorType BackupProcessType = ProjectBackupBehaviorType.Simple;

    public string Alias_Export_Delimiter = ";";
    public bool Alias_Editor_Export_Ignore_Empty = false;

    public bool PTDE_UseCollisionHack = true;
    public string PTDE_Collision_Root = "";
    public bool PTDE_Collision_Root_Warning = true;

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

    ///------------------------------------------------------------
    /// Interface
    ///------------------------------------------------------------
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

    public string System_English_Font = Path.Join("Assets","Fonts","RobotoMono-Light.ttf");
    public string System_Other_Font = Path.Join("Assets","Fonts","NotoSansCJKtc-Light.otf");

    public string SelectedTheme = "";

    public float Interface_FontSize = 14.0f;

    public bool Interface_Editor_Viewport = true;
    public bool Viewport_Profiling = true;

    public bool Allow_Window_Movement = true;
    public int GFX_Display_Width { get; set; } = 1920;
    public int GFX_Display_Height { get; set; } = 1057;
    public int GFX_Display_X { get; set; } = 0;
    public int GFX_Display_Y { get; set; } = 23;

    ///------------------------------------------------------------
    /// Map Editor
    ///------------------------------------------------------------
    // General
    public bool MapEditor_EnableMapUnload = true;
    public bool MapEditor_IgnoreSaveExceptions = false;

    public bool MapEditor_DisplayMapCategories = true;

    public EntityNameDisplayType MapEditor_MapContentList_EntryNameDisplayType = EntityNameDisplayType.Internal_FMG;

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
    public bool MapEditor_DisplayUnknownFields = true;
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

    public int MapEditor_Selection_Position_IncrementType = 0;
    public float MapEditor_Selection_Position_Increment_0 { get; set; } = 0.05f;
    public float MapEditor_Selection_Position_Increment_1 { get; set; } = 0.1f;
    public float MapEditor_Selection_Position_Increment_2 { get; set; } = 0.25f;
    public float MapEditor_Selection_Position_Increment_3 { get; set; } = 0.5f;
    public float MapEditor_Selection_Position_Increment_4 { get; set; } = 1.0f;

    public bool MapEditor_Selection_Position_Increment_DiscreteApplication = false;

    public bool Shortcuts_MapEditor_EnableSelectionGroupShortcuts = false;

    public bool GlobalMapSearch_CopyResults_IncludeHeader = true;
    public bool GlobalMapSearch_CopyResults_IncludeIndex = true;
    public bool GlobalMapSearch_CopyResults_IncludeEntityName = true;
    public bool GlobalMapSearch_CopyResults_IncludeEntityAlias = true;
    public bool GlobalMapSearch_CopyResults_IncludePropertyName = true;
    public bool GlobalMapSearch_CopyResults_IncludePropertyValue = true;
    public string GlobalMapSearch_CopyResults_Delimiter = ",";

    public bool MsbReference_DisplayName = true;
    public bool MsbReference_DisplayEntityID = true;
    public bool MsbReference_DisplayAlias = true;

    public bool Interface_MapEditor_WrapAliasDisplay = true;

    public bool Prefab_IncludeEntityID = true;
    public bool Prefab_IncludeEntityGroupIDs = true;

    public bool Prefab_ApplyUniqueInstanceID = true;
    public bool Prefab_ApplyUniqueEntityID = false;
    public bool Prefab_ApplySpecificEntityGroupID = false;
    public int Prefab_SpecificEntityGroupID = 0;
    public bool Prefab_PlaceAtPlacementOrb = true;

    public bool Prefab_ApplyOverrideName = false;
    public string Prefab_OverrideName = "";

    public bool WorldMapDisplayTiles = false;
    public bool WorldMapDisplaySmallTiles = true;
    public bool WorldMapDisplayMediumTiles = false;
    public bool WorldMapDisplayLargeTiles = false;
    public bool WorldMapLockMovement = false;

    public bool QuickView_DisplayTooltip = false;
    public List<string> QuickView_TargetProperties = new List<string>() { "Name" };

    public bool DisplayPlacementOrb = false;
    public float PlacementOrb_Distance = 5.0f;

    public bool MapEditor_MapContentList_DisplayVisibilityIcon = true;

    public bool MapEditor_ModelLoad_MapPieces = true;
    public bool MapEditor_ModelLoad_Objects = true;
    public bool MapEditor_ModelLoad_Characters = true;
    public bool MapEditor_ModelLoad_Collisions = true;
    public bool MapEditor_ModelLoad_Navmeshes = true;

    public bool MapEditor_TextureLoad_MapPieces = true;
    public bool MapEditor_TextureLoad_Objects = true;
    public bool MapEditor_TextureLoad_Characters = true;
    public bool MapEditor_TextureLoad_Misc = true;

    public bool MapEditor_ModelDataExtraction_IncludeFolder = true;
    public ResourceExtractionType MapEditor_ModelDataExtraction_Type = ResourceExtractionType.Loose;
    public string MapEditor_ModelDataExtraction_DefaultOutputFolder = ".output";

    public bool MapEditor_LightAtlas_AutomaticAdd = true;
    public bool MapEditor_LightAtlas_AutomaticDelete = false;
    public bool MapEditor_LightAtlas_AutomaticAdjust = true;

    public HavokCollisionType CurrentHavokCollisionType = HavokCollisionType.Low;

    // Toolbar
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

    // Windows
    public bool Interface_MapEditor_Viewport_Grid = true;
    public bool Interface_MapEditor_MapList = true;
    public bool Interface_MapEditor_MapContents = true;
    public bool Interface_MapEditor_Properties = true;
    public bool Interface_MapEditor_RenderGroups = true;
    public bool Interface_MapEditor_AssetBrowser = true;
    public bool Interface_MapEditor_ToolWindow = true;
    public bool Interface_MapEditor_ResourceList = true;

    // Tools
    public bool Interface_MapEditor_Tool_Create = true;
    public bool Interface_MapEditor_Tool_Duplicate = true;
    public bool Interface_MapEditor_Tool_DuplicateToMap = true;
    public bool Interface_MapEditor_Tool_PullToCamera = true;
    public bool Interface_MapEditor_Tool_Rotate = true;
    public bool Interface_MapEditor_Tool_Scramble = true;
    public bool Interface_MapEditor_Tool_Replicate = true;
    public bool Interface_MapEditor_Tool_Prefab = true;
    public bool Interface_MapEditor_Tool_SelectionGroups = true;
    public bool Interface_MapEditor_Tool_MovementIncrements = true;
    public bool Interface_MapEditor_Tool_RotationIncrements = true;
    public bool Interface_MapEditor_Tool_LocalPropertySearch = true;
    public bool Interface_MapEditor_Tool_GlobalPropertySearch = true;
    public bool Interface_MapEditor_Tool_PropertyMassEdit = true;
    public bool Interface_MapEditor_Tool_TreasureMaker = false;
    public bool Interface_MapEditor_Tool_WorldMapLayoutGenerator = false;
    public bool Interface_MapEditor_Tool_GridConfiguration = true;
    public bool Interface_MapEditor_Tool_ModelSelector = true;
    public bool Interface_MapEditor_Tool_DisplayGroups = true;
    public bool Interface_MapEditor_Tool_EntityIdentifier = true;
    public bool Interface_MapEditor_Tool_MapValidator = true;
    public bool Interface_MapEditor_Tool_MapModelInsight = true;

    // Saving
    public bool AutomaticSave_MapEditor = true;

    public bool MapEditor_AutomaticSave_IncludeMSB = true;
    public bool MapEditor_AutomaticSave_IncludeBTL = false;
    public bool MapEditor_AutomaticSave_IncludeAIP = false;
    public bool MapEditor_AutomaticSave_IncludeNVA = false;
    public bool MapEditor_AutomaticSave_IncludeBTAB = false;
    public bool MapEditor_AutomaticSave_IncludeBTPB = false;

    public bool MapEditor_ManualSave_IncludeMSB = true;
    public bool MapEditor_ManualSave_IncludeBTL = true;
    public bool MapEditor_ManualSave_IncludeAIP = true;
    public bool MapEditor_ManualSave_IncludeNVA = true;
    public bool MapEditor_ManualSave_IncludeBTAB = true;
    public bool MapEditor_ManualSave_IncludeBTPB = true;

    // Scene Filters
    public RenderFilter LastSceneFilter { get; set; } = RenderFilter.All ^ RenderFilter.Light;
    public RenderFilterPreset SceneFilter_Preset_01 { get; set; } = new("Map", RenderFilter.MapPiece | RenderFilter.Object | RenderFilter.Character | RenderFilter.Region);
    public RenderFilterPreset SceneFilter_Preset_02 { get; set; } = new("Collision", RenderFilter.Collision | RenderFilter.Object | RenderFilter.Character | RenderFilter.Region);
    public RenderFilterPreset SceneFilter_Preset_03 { get; set; } = new("Collision & Navmesh", RenderFilter.Collision | RenderFilter.Navmesh | RenderFilter.Object | RenderFilter.Character | RenderFilter.Region);
    public RenderFilterPreset SceneFilter_Preset_04 { get; set; } = new("Lighting (Map)", RenderFilter.MapPiece | RenderFilter.Object | RenderFilter.Character | RenderFilter.Light);
    public RenderFilterPreset SceneFilter_Preset_05 { get; set; } = new("Lighting (Collision)", RenderFilter.Collision | RenderFilter.Object | RenderFilter.Character | RenderFilter.Light);
    public RenderFilterPreset SceneFilter_Preset_06 { get; set; } = new("All", RenderFilter.All);

    ///------------------------------------------------------------
    /// Model Editor
    ///------------------------------------------------------------
    // General
    public bool ModelEditor_AutoLoadSingles = true;
    public bool ModelEditor_IncludeAliasInSearch = true;

    public bool Interface_ModelEditor_WrapAliasDisplay = true;

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


    public bool ModelEditor_ModelLoad_MapPieces = true;
    public bool ModelEditor_ModelLoad_Objects = true;
    public bool ModelEditor_ModelLoad_Parts = true;
    public bool ModelEditor_ModelLoad_Characters = true;
    public bool ModelEditor_ModelLoad_Collisions = true;
    public bool ModelEditor_ModelLoad_Navmeshes = true;

    public bool ModelEditor_TextureLoad_MapPieces = true;
    public bool ModelEditor_TextureLoad_Objects = true;
    public bool ModelEditor_TextureLoad_Parts = true;
    public bool ModelEditor_TextureLoad_Characters = true;
    public bool ModelEditor_TextureLoad_Misc = true;

    public float DummyMeshSize = 0.05f;
    public float NodeMeshSize = 0.05f;

    // Windows
    public bool Interface_ModelEditor_Viewport_Grid = true;
    public bool Interface_ModelEditor_ModelSourceList = true;
    public bool Interface_ModelEditor_ModelSelectList = true;
    public bool Interface_ModelEditor_ModelContents = true;
    public bool Interface_ModelEditor_Properties = true;
    public bool Interface_ModelEditor_ResourceList = true;

    // Tools
    public bool Interface_ModelEditor_Tool_CreateAction = true;
    public bool Interface_ModelEditor_Tool_ModelGridConfiguration = true;
    public bool Interface_ModelEditor_Tool_ModelInsight = true;
    public bool Interface_ModelEditor_Tool_ModelInstanceFinder = true;
    public bool Interface_ModelEditor_Tool_ModelMaskToggler = true;

    // Saving
    public bool ModelEditor_AutomaticSave_IncludeFLVER = true;
    public bool ModelEditor_ManualSave_IncludeFLVER = true;
    public bool AutomaticSave_ModelEditor = true;

    ///------------------------------------------------------------
    /// Param Editor
    ///------------------------------------------------------------
    // General
    public bool UseLooseParams = false;
    public bool RepackLooseDS2Params = false;

    // Done per project so we can default ON for DS2 and ER without forcing stripping for the others
    public bool Param_StripRowNamesOnSave_DES = false;
    public bool Param_StripRowNamesOnSave_DS1 = false;
    public bool Param_StripRowNamesOnSave_DS2 = true;
    public bool Param_StripRowNamesOnSave_BB = false;
    public bool Param_StripRowNamesOnSave_DS3 = false;
    public bool Param_StripRowNamesOnSave_SDT = false;
    public bool Param_StripRowNamesOnSave_ER = true;
    public bool Param_StripRowNamesOnSave_AC6 = false;
    public bool Param_StripRowNamesOnSave_NR = false;

    public bool Param_RestoreStrippedRowNamesOnLoad_DES = false;
    public bool Param_RestoreStrippedRowNamesOnLoad_DS1 = false;
    public bool Param_RestoreStrippedRowNamesOnLoad_DS2 = true;
    public bool Param_RestoreStrippedRowNamesOnLoad_BB = false;
    public bool Param_RestoreStrippedRowNamesOnLoad_DS3 = false;
    public bool Param_RestoreStrippedRowNamesOnLoad_SDT = false;
    public bool Param_RestoreStrippedRowNamesOnLoad_ER = true;
    public bool Param_RestoreStrippedRowNamesOnLoad_AC6 = false;
    public bool Param_RestoreStrippedRowNamesOnLoad_NR = false;

    public bool Param_SaveERAsDFLT = false;

    public bool UI_CompactParams = false;

    public bool Param_UseProjectMeta = false;

    public bool Param_AlphabeticalParams = true;
    public bool Param_ShowParamCommunityName = false;
    public bool Param_ShowSecondaryNames = true;

    public bool Param_DisplayParamCategories = true;

    public bool Param_DisplayTableGroupColumn = true;
    public ParamTableGroupRowDisplayType Param_TableGroupRowDisplayType = ParamTableGroupRowDisplayType.None;

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
    public bool Param_ShowFmgDecorator = true;
    public ParamRowCopyBehavior Param_RowCopyBehavior = ParamRowCopyBehavior.ID;

    public bool Param_ShowVanillaColumn = true;
    public bool Param_ShowAuxColumn = true;

    public bool Param_HidePaddingFields = true;
    public bool Param_HideObsoleteFields = true;
    public bool Param_ShowColorPreview = true;
    public bool Param_ShowGraphVisualisation = true;
    public bool Param_PinnedParamsStayVisible = true;
    public bool Param_PinnedRowsStayVisible = true;
    public bool Param_PinnedFieldsStayVisible = true;
    public bool Param_ViewInMapOption = true;
    public bool Param_ViewModelOption = true;

    public bool Param_ShowFieldDescription_onIcon = true;
    public bool Param_ShowFieldLimits_onIcon = true;
    public bool Param_ShowFieldDescription_onName = true;
    public bool Param_ShowFieldLimits_onName = true;

    public bool Param_ShowTraditionalPercentages = false;

    public bool Param_MassEdit_ShowAddButtons = true;

    public float Param_ParamContextMenu_Width = 350f;

    public float Param_TableGroupContextMenu_Width = 350f;

    public float Param_RowContextMenu_Width = 350f;
    public bool Param_RowContextMenu_NameInput = true;
    public bool Param_RowContextMenu_ShortcutTools = true;
    public bool Param_RowContextMenu_PinOptions = true;
    public bool Param_RowContextMenu_CompareOptions = true;
    public bool Param_RowContextMenu_ReverseLoopup = true;
    public bool Param_RowContextMenu_ProliferateName = true;
    public bool Param_RowContextMenu_InheritName = true;
    public bool Param_RowContextMenu_RowNameAdjustments = true;

    public float Param_FieldContextMenu_Width = 350f;
    public float Param_FieldContextMenu_ListHeightMultiplier = 1f;

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

    public bool Param_RowNameImport_ReplaceEmptyNamesOnly = false;

    public bool Param_TableGroupView_AllowDuplicateInject = false;

    public bool UseLatestGameOffset = true;
    public int SelectedGameOffsetData = 0;

    // Windows
    public bool Interface_ParamEditor_Table = true;

    // Tools
    public bool Interface_ParamEditor_ToolWindow = true;
    public bool Interface_ParamEditor_Tool_PinGroups = true;
    public bool Interface_ParamEditor_Tool_ParamCategories = true;
    public bool Interface_ParamEditor_Tool_ParamMerge = true;
    public bool Interface_ParamEditor_Tool_ParamUpgrader = true;
    public bool Interface_ParamEditor_Tool_ParamReloader = true;
    public bool Interface_ParamEditor_Tool_ItemGib = true;
    public bool Interface_ParamEditor_Tool_MassEdit = true;
    public bool Interface_ParamEditor_Tool_MassEditScript = true;
    public bool Interface_ParamEditor_Tool_Duplicate = true;
    public bool Interface_ParamEditor_Tool_CommutativeDuplicate = true;
    public bool Interface_ParamEditor_Tool_RowNameTrimmer = true;
    public bool Interface_ParamEditor_Tool_RowNameSorter = true;
    public bool Interface_ParamEditor_Tool_FieldInstanceFinder = true;
    public bool Interface_ParamEditor_Tool_RowInstanceFinder = true;
    public bool Interface_ParamEditor_Tool_SetFinder = true;

    // Saving
    public bool AutomaticSave_ParamEditor = true;

    public bool ParamEditor_AutomaticSave_IncludePARAM = true;
    public bool ParamEditor_ManualSave_IncludePARAM = true;

    // Misc
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

    public string Param_Import_Delimiter = ",";


    ///------------------------------------------------------------
    /// Text Editor
    ///------------------------------------------------------------
    
    // General
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
    public bool TextEditor_EnableObsoleteContainerLoad = false;

    // OLD
    public string FMG_SearchAndReplace_SearchText = "";
    public string FMG_SearchAndReplace_ReplaceText = "";
    public bool FMG_SearchAndReplace_Regex_IgnoreCase = false;
    public bool FMG_SearchAndReplace_Regex_Multiline = false;
    public bool FMG_SearchAndReplace_Regex_Singleline = false;
    public bool FMG_SearchAndReplace_Regex_IgnorePatternWhitespace = false;

    public bool TextEditor_LanguageSync_IncludeDefaultEntries = false;
    public bool TextEditor_LanguageSync_IncludeModifiedEntries = true;
    public bool TextEditor_LanguageSync_IncludeUniqueEntries = true;

    // Windows
    public bool Interface_TextEditor_FileContainerList = true;
    public bool Interface_TextEditor_FmgList = true;
    public bool Interface_TextEditor_TextEntryList = true;
    public bool Interface_TextEditor_TextEntryContents = true;

    // Tools
    public bool Interface_TextEditor_ToolWindow = true;
    public bool Interface_TextEditor_Tool_TextSearch = true;
    public bool Interface_TextEditor_Tool_TextReplacement = true;
    public bool Interface_TextEditor_Tool_TextMerge = true;

    // Saving
    public bool AutomaticSave_TextEditor = true;

    public bool TextEditor_AutomaticSave_IncludeFMG = true;
    public bool TextEditor_ManualSave_IncludeFMG = true;

    ///------------------------------------------------------------
    /// Graphics Param Editor
    ///------------------------------------------------------------
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

    // Windows
    public bool Interface_GparamEditor_FileList = true;
    public bool Interface_GparamEditor_GroupList = true;
    public bool Interface_GparamEditor_FieldList = true;
    public bool Interface_GparamEditor_FieldValues = true;

    // Tools
    public bool Interface_GparamEditor_ToolWindow = true;
    public bool Interface_GparamEditor_Tool_QuickEdit = true;

    // Saving
    public bool AutomaticSave_GparamEditor = false;

    public bool GparamEditor_AutomaticSave_IncludeGPARAM = true;
    public bool GparamEditor_ManualSave_IncludeGPARAM = true;

    ///------------------------------------------------------------
    /// Texture Viewer
    ///------------------------------------------------------------
    // General
    public bool TextureViewer_FileList_ShowLowDetail_Entries = true;

    public bool TextureViewer_FileList_ShowAliasName_Characters = true;
    public bool TextureViewer_FileList_ShowAliasName_Assets = true;
    public bool TextureViewer_FileList_ShowAliasName_Parts = true;

    public bool TextureViewer_TextureList_ShowAliasName_Particles = true;

    public string TextureViewerToolbar_ExportTextureLocation = "";
    public bool TextureViewerToolbar_ExportTexture_IncludeFolder = true;
    public bool TextureViewerToolbar_ExportTexture_DisplayConfirm = true;
    public int TextureViewerToolbar_ExportTextureType = 0;

    // Windows
    public bool Interface_TextureViewer_Files = true;
    public bool Interface_TextureViewer_Textures = true;
    public bool Interface_TextureViewer_Viewer = true;
    public bool Interface_TextureViewer_Properties = true;
    public bool Interface_TextureViewer_ToolWindow = true;
    public bool Interface_TextureViewer_ResourceList = true;

    // Tools
    public bool Interface_TextureViewer_Tool_ExportTexture = true;

    ///------------------------------------------------------------
    /// Material Editor
    ///------------------------------------------------------------
    // General
    public bool MaterialEditor_DisplayCommunityFieldNames = false;

    // Windows
    public bool Interface_MaterialEditor_SourceList = true;
    public bool Interface_MaterialEditor_FileList = true;
    public bool Interface_MaterialEditor_PropertyView = true;
    public bool Interface_MaterialEditor_ToolWindow = true;

    // Tools

    // Saving
    public bool AutomaticSave_MaterialEditor = false;

    public bool MaterialEditor_AutomaticSave_IncludeMTD = true;
    public bool MaterialEditor_AutomaticSave_IncludeMATBIN = true;

    public bool MaterialEditor_ManualSave_IncludeMTD = true;
    public bool MaterialEditor_ManualSave_IncludeMATBIN = true;

    ///------------------------------------------------------------
    /// File Browser
    ///------------------------------------------------------------
    public bool Interface_FileBrowser_FileList = true;
    public bool Interface_FileBrowser_ItemViewer = true;
    public bool Interface_FileBrowser_ToolView = true;

    public bool Interface_FileBrowser_Tool_GameUnpacker = true;

    ///------------------------------------------------------------
    /// Viewport
    ///------------------------------------------------------------
    public bool Viewport_Enable_Rendering = true;
    public bool Viewport_Enable_Texturing = true;
    public bool Viewport_Enable_Culling = true;

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


    public Vector3 GFX_Renderable_Collision_Color = new Vector3(53, 157, 255);
    public Vector3 GFX_Renderable_ConnectCollision_Color = new Vector3(146, 57, 158);
    public Vector3 GFX_Renderable_Navmesh_Color = new Vector3(157, 53, 255);
    public Vector3 GFX_Renderable_NavmeshGate_Color = new Vector3(50, 220, 0);

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

    public Vector3 GFX_Renderable_AutoInvadeSphere_BaseColor = Utils.GetDecimalColor(Color.Red);
    public Vector3 GFX_Renderable_AutoInvadeSphere_HighlightColor = Utils.GetDecimalColor(Color.DarkRed);

    public Vector3 GFX_Renderable_LightProbeSphere_BaseColor = Utils.GetDecimalColor(Color.Yellow);
    public Vector3 GFX_Renderable_LightProbeSphere_HighlightColor = Utils.GetDecimalColor(Color.YellowGreen);

    public Vector3 GFX_Renderable_LevelConnectorSphere_BaseColor = Utils.GetDecimalColor(Color.Turquoise);
    public Vector3 GFX_Renderable_LevelConnectorSphere_HighlightColor = Utils.GetDecimalColor(Color.DarkTurquoise);

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
    public bool Viewport_DisplayPositionIncrement = true;
    public bool Viewport_Enable_BoxSelection = true;
    public float Viewport_BS_DistThresFactor = 1.2f;

    public Vector3 Viewport_Background_Color = new(1.0f, 0.5f, 0.0f);

    // Map Editor: Primary Grid
    public bool MapEditor_DisplayPrimaryGrid = false;
    public bool MapEditor_RegeneratePrimaryGrid = false;
    public int MapEditor_PrimaryGrid_Size = 1000;
    public float MapEditor_PrimaryGrid_SectionSize = 10;
    public Vector3 MapEditor_PrimaryGrid_Color = new Vector3(0.5f, 0.5f, 0.5f);

    public float MapEditor_PrimaryGrid_Position_X = 0;
    public float MapEditor_PrimaryGrid_Position_Y = 0;
    public float MapEditor_PrimaryGrid_Position_Z = 0;

    public float MapEditor_PrimaryGrid_Rotation_X = 0;
    public float MapEditor_PrimaryGrid_Rotation_Y = 0;
    public float MapEditor_PrimaryGrid_Rotation_Z = 0;

    public bool MapEditor_PrimaryGrid_Configure_ApplyPosition_X = true;
    public bool MapEditor_PrimaryGrid_Configure_ApplyPosition_Y = true;
    public bool MapEditor_PrimaryGrid_Configure_ApplyPosition_Z = true;

    public bool MapEditor_PrimaryGrid_Configure_ApplyRotation_X = true;
    public bool MapEditor_PrimaryGrid_Configure_ApplyRotation_Y = true;
    public bool MapEditor_PrimaryGrid_Configure_ApplyRotation_Z = true;

    public ViewportGridRootAxis MapEditor_PrimaryGrid_Configure_RootAxis = ViewportGridRootAxis.Y;

    // Map Editor: Secondary Grid
    public bool MapEditor_DisplaySecondaryGrid = false;
    public bool MapEditor_RegenerateSecondaryGrid = false;
    public int MapEditor_SecondaryGrid_Size = 1000;
    public float MapEditor_SecondaryGrid_SectionSize = 10;
    public Vector3 MapEditor_SecondaryGrid_Color = new Vector3(0.5f, 0.5f, 0.5f);

    public float MapEditor_SecondaryGrid_Position_X = 90;
    public float MapEditor_SecondaryGrid_Position_Y = 0;
    public float MapEditor_SecondaryGrid_Position_Z = 0;

    public float MapEditor_SecondaryGrid_Rotation_X = 0;
    public float MapEditor_SecondaryGrid_Rotation_Y = 0;
    public float MapEditor_SecondaryGrid_Rotation_Z = 0;

    public bool MapEditor_SecondaryGrid_Configure_ApplyPosition_X = true;
    public bool MapEditor_SecondaryGrid_Configure_ApplyPosition_Y = true;
    public bool MapEditor_SecondaryGrid_Configure_ApplyPosition_Z = true;

    public bool MapEditor_SecondaryGrid_Configure_ApplyRotation_X = true;
    public bool MapEditor_SecondaryGrid_Configure_ApplyRotation_Y = true;
    public bool MapEditor_SecondaryGrid_Configure_ApplyRotation_Z = true;

    public ViewportGridRootAxis MapEditor_SecondaryGrid_Configure_RootAxis = ViewportGridRootAxis.X;

    // Map Editor: Tertiary Grid
    public bool MapEditor_DisplayTertiaryGrid = false;
    public bool MapEditor_RegenerateTertiaryGrid = false;
    public int MapEditor_TertiaryGrid_Size = 1000;
    public float MapEditor_TertiaryGrid_SectionSize = 10;
    public Vector3 MapEditor_TertiaryGrid_Color = new Vector3(0.5f, 0.5f, 0.5f);

    public float MapEditor_TertiaryGrid_Position_X = 0;
    public float MapEditor_TertiaryGrid_Position_Y = 0;
    public float MapEditor_TertiaryGrid_Position_Z = 0;

    public float MapEditor_TertiaryGrid_Rotation_X = 0;
    public float MapEditor_TertiaryGrid_Rotation_Y = 0;
    public float MapEditor_TertiaryGrid_Rotation_Z = 90;

    public bool MapEditor_TertiaryGrid_Configure_ApplyPosition_X = true;
    public bool MapEditor_TertiaryGrid_Configure_ApplyPosition_Y = true;
    public bool MapEditor_TertiaryGrid_Configure_ApplyPosition_Z = true;

    public bool MapEditor_TertiaryGrid_Configure_ApplyRotation_X = true;
    public bool MapEditor_TertiaryGrid_Configure_ApplyRotation_Y = true;
    public bool MapEditor_TertiaryGrid_Configure_ApplyRotation_Z = true;

    public ViewportGridRootAxis MapEditor_TertiaryGrid_Configure_RootAxis = ViewportGridRootAxis.Z;

    // Model Editor: Primary Grid
    public bool ModelEditor_DisplayPrimaryGrid = false;
    public bool ModelEditor_RegeneratePrimaryGrid = false;
    public int ModelEditor_PrimaryGrid_Size = 1000;
    public float ModelEditor_PrimaryGrid_SectionSize = 10;

    public Vector3 ModelEditor_PrimaryGrid_Color = new Vector3(0.5f, 0.5f, 0.5f);

    public float ModelEditor_PrimaryGrid_Position_X = 0;
    public float ModelEditor_PrimaryGrid_Position_Y = 0;
    public float ModelEditor_PrimaryGrid_Position_Z = 0;

    public float ModelEditor_PrimaryGrid_Rotation_X = 0;
    public float ModelEditor_PrimaryGrid_Rotation_Y = 0;
    public float ModelEditor_PrimaryGrid_Rotation_Z = 0;

    // Model Editor: Secondary Grid
    public bool ModelEditor_DisplaySecondaryGrid = false;
    public bool ModelEditor_RegenerateSecondaryGrid = false;
    public int ModelEditor_SecondaryGrid_Size = 1000;
    public float ModelEditor_SecondaryGrid_SectionSize = 10;

    public Vector3 ModelEditor_SecondaryGrid_Color = new Vector3(0.5f, 0.5f, 0.5f);

    public float ModelEditor_SecondaryGrid_Position_X = 0;
    public float ModelEditor_SecondaryGrid_Position_Y = 0;
    public float ModelEditor_SecondaryGrid_Position_Z = 0;

    public float ModelEditor_SecondaryGrid_Rotation_X = 90;
    public float ModelEditor_SecondaryGrid_Rotation_Y = 0;
    public float ModelEditor_SecondaryGrid_Rotation_Z = 0;

    // Model Editor: Tertiary Grid
    public bool ModelEditor_DisplayTertiaryGrid = false;
    public bool ModelEditor_RegenerateTertiaryGrid = false;
    public int ModelEditor_TertiaryGrid_Size = 1000;
    public float ModelEditor_TertiaryGrid_SectionSize = 10;

    public Vector3 ModelEditor_TertiaryGrid_Color = new Vector3(0.5f, 0.5f, 0.5f);

    public float ModelEditor_TertiaryGrid_Position_X = 0;
    public float ModelEditor_TertiaryGrid_Position_Y = 0;
    public float ModelEditor_TertiaryGrid_Position_Z = 0;

    public float ModelEditor_TertiaryGrid_Rotation_X = 0;
    public float ModelEditor_TertiaryGrid_Rotation_Y = 0;
    public float ModelEditor_TertiaryGrid_Rotation_Z = 90;

    public Vector3 ModelEditor_FrameInViewport_Offset = new Vector3();
    public float ModelEditor_FrameInViewport_Distance = 1f;
    public float ModelEditor_PullToCamera_Offset = 3f;

    ///------------------------------------------------------------
    /// Misc
    ///------------------------------------------------------------
    public Vector3 SavedPosition = new Vector3();
    public Vector3 SavedRotation = new Vector3();
    public Vector3 SavedScale = new Vector3();

    // JsonExtensionData stores info in config file not present in class in order to retain settings between versions.
#pragma warning disable IDE0051
    [JsonExtensionData] public IDictionary<string, JsonElement> AdditionalData;
#pragma warning restore IDE0051

    ///------------------------------------------------------------
    /// CFG
    ///------------------------------------------------------------
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
                Current = JsonSerializer.Deserialize(filestring, ProjectJsonSerializerContext.Default.CFG);

                if (Current == null)
                {
                    throw new Exception("JsonConvert returned null");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Configuration failed to load, default configuration has been restored.", LogLevel.Error, LogPriority.High, e);

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

        var json = JsonSerializer.Serialize(Current, ProjectJsonSerializerContext.Default.CFG);

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