using Microsoft.Extensions.Logging;
using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
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
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudioCore.Application;

public class CFG
{
    // Types:
    // Preferences: shown in the Preferences menu
    // Options: exposes via ImGui elements such as checkboxes or combo-lists
    // Internal: used for persistent state

    #region GFX
    ///------------------------------------------------------------
    /// GFX
    ///------------------------------------------------------------
    public int GFX_Display_Width { get; set; } = 1920;
    public int GFX_Display_Height { get; set; } = 1057;
    public int GFX_Display_X { get; set; } = 0;
    public int GFX_Display_Y { get; set; } = 23;
    #endregion

    #region System
    ///------------------------------------------------------------
    /// System
    ///------------------------------------------------------------
    // Preferences
    public bool System_Check_Program_Update = true;
    public bool System_Enable_Soapstone_Server = true;
    public bool System_Ignore_Read_Asserts = false;
    public bool System_Apply_DCX_Heuristic = false;

    public bool Logger_Enable_Action_Log = true;
    public bool Logger_Enable_Warning_Log = true;
    public int Logger_Action_Fade_Time = 1500;
    public int Logger_Warning_Fade_Time = 1500;

    public bool Developer_Enable_Tools = false;
    public string Developer_Smithbox_Build_Folder = "";

    #endregion

    #region Project
    ///------------------------------------------------------------
    /// Project
    ///------------------------------------------------------------
    // Preferences
    public bool Project_Enable_Auto_Load = true;
    public bool Project_Enable_Automatic_Auto_Load_Assignment = true;

    public bool Project_Enable_Backup_Saves = true;
    public ProjectBackupBehaviorType Project_Backup_Type = ProjectBackupBehaviorType.Simple;

    public bool Project_Enable_Automatic_Save = true;
    public float Project_Automatic_Save_Interval = 300;

    public string Project_Default_Mod_Directory = "";
    public string Project_Default_Data_Directory = "";
    public string Project_ME3_Profile_Directory = "";

    public bool MapEditor_Use_PTDE_Collisions_In_DS1R_Projects = true;
    public string PTDE_Data_Path = "";

    public bool Project_Enable_Project_Metadata = false;

    // Options
    public string Project_Alias_Export_Delimiter = ";";
    public bool Project_Alias_Editor_Export_Ignore_Empty = false;

    #endregion

    #region Interface
    ///------------------------------------------------------------
    /// Interface
    ///------------------------------------------------------------
    public float Interface_UI_Scale = 1.0f;
    public bool Interface_Scale_by_DPI = true;
    public float Interface_Font_Size = 14.0f;
    public bool Interface_Allow_Window_Movement = true;

    public bool Interface_Alias_Wordwrap_General = true;
    public bool Interface_Alias_Wordwrap_Map_Editor = true;
    public bool Interface_Alias_Wordwrap_Model_Editor = true;

    public bool Interface_Include_Chinese_Symbols = false;
    public bool Interface_Include_Cyrillic_Symbols = false;
    public bool Interface_Include_Korean_Symbols = false;
    public bool Interface_Include_Thai_Symbols = false;
    public bool Interface_Include_Vietnamese_Symbols = false;

    public string Interface_English_Font_Path = Path.Join("Assets","Fonts","RobotoMono-Light.ttf");
    public string Interface_Non_English_Font_Path = Path.Join("Assets","Fonts","NotoSansCJKtc-Light.otf");

    public string Interface_Selected_Theme = "";

    public float Interface_Context_Menu_Width = 350f;
    public float Interface_Context_Menu_List_Height_Multiplier = 1f;

    #endregion

    #region Map Editor
    ///------------------------------------------------------------
    /// Map Editor
    ///------------------------------------------------------------
    // Preferences
    public bool MapEditor_IgnoreSaveExceptions = false;

    public bool MapEditor_Map_List_Display_Map_Aliases = true;
    public bool MapEditor_Map_List_Enable_Load_on_Double_Click = false;

    public bool MapEditor_Map_Contents_Display_Character_Aliases = true;
    public bool MapEditor_Map_Contents_Display_Asset_Aliases = true;
    public bool MapEditor_Map_Contents_Display_Map_Piece_Aliases = true;
    public bool MapEditor_Map_Contents_Display_Treasure_Aliases = true;

    public bool MapEditor_Properties_Enable_Commmunity_Names = true;
    public bool MapEditor_Properties_Display_Unknown_Properties = true;
    public bool MapEditor_Properties_Display_Property_Attributes = false;
    public bool MapEditor_Properties_Enable_Referenced_Rename = false;
    public bool MapEditor_Properties_Display_Additional_Information_at_Top = false;
    public bool MapEditor_Properties_Display_Behavior_Information = true;
    public bool MapEditor_Properties_Display_Reference_Information = true;

    public bool MapEditor_Model_Selector_Display_Aliases = true;
    public bool MapEditor_Model_Selector_Display_Tags = false;
    public bool MapEditor_Model_Selector_Display_Low_Detail_Entries = false;

    public bool MapEditor_Enable_Character_Substitution = false;
    public string MapEditor_Character_Substitution_ID = "c0000";

    public bool MapEditor_Selection_Group_Frame_Selection_On_Use = true;
    public bool MapEditor_Selection_Group_Enable_Quick_Creation = false;
    public bool MapEditor_Selection_Group_Confirm_Delete = true;
    public bool MapEditor_Selection_Group_Show_Keybind = true;
    public bool MapEditor_Selection_Group_Show_Tags = false;

    // Options
    public bool MapEditor_Model_Selector_Update_Name = true;
    public bool MapEditor_Model_Selector_Update_Instance_ID = true;

    public EntityNameDisplayType MapEditor_MapContentList_EntryNameDisplayType = EntityNameDisplayType.Internal_FMG;

    public int MapEditor_Selection_Position_IncrementType = 0;
    public float MapEditor_Selection_Position_Increment_0 { get; set; } = 0.05f;
    public float MapEditor_Selection_Position_Increment_1 { get; set; } = 0.1f;
    public float MapEditor_Selection_Position_Increment_2 { get; set; } = 0.25f;
    public float MapEditor_Selection_Position_Increment_3 { get; set; } = 0.5f;
    public float MapEditor_Selection_Position_Increment_4 { get; set; } = 1.0f;

    public bool MapEditor_Selection_Position_Increment_DiscreteApplication = false;

    public bool MapEditor_Selection_Group_Enable_Shortcuts = false;

    public bool GlobalMapSearch_CopyResults_IncludeHeader = true;
    public bool GlobalMapSearch_CopyResults_IncludeIndex = true;
    public bool GlobalMapSearch_CopyResults_IncludeEntityName = true;
    public bool GlobalMapSearch_CopyResults_IncludeEntityAlias = true;
    public bool GlobalMapSearch_CopyResults_IncludePropertyName = true;
    public bool GlobalMapSearch_CopyResults_IncludePropertyValue = true;
    public string GlobalMapSearch_CopyResults_Delimiter = ",";

    public bool MapEditor_Properties_Display_Reference_Name = true;
    public bool MapEditor_Properties_Display_Reference_Entity_ID = true;
    public bool MapEditor_Properties_Display_Reference_Alias = true;

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
    public bool Project_Automatic_Save_Include_Map_Editor = true;

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
    public RenderFilterPreset Viewport_Filter_Preset_1 { get; set; } = new("Map", RenderFilter.MapPiece | RenderFilter.Object | RenderFilter.Character | RenderFilter.Region);
    public RenderFilterPreset Viewport_Filter_Preset_2 { get; set; } = new("Collision", RenderFilter.Collision | RenderFilter.Object | RenderFilter.Character | RenderFilter.Region);
    public RenderFilterPreset Viewport_Filter_Preset_3 { get; set; } = new("Collision & Navmesh", RenderFilter.Collision | RenderFilter.Navmesh | RenderFilter.Object | RenderFilter.Character | RenderFilter.Region);
    public RenderFilterPreset Viewport_Filter_Preset_4 { get; set; } = new("Lighting (Map)", RenderFilter.MapPiece | RenderFilter.Object | RenderFilter.Character | RenderFilter.Light);
    public RenderFilterPreset Viewport_Filter_Preset_5 { get; set; } = new("Lighting (Collision)", RenderFilter.Collision | RenderFilter.Object | RenderFilter.Character | RenderFilter.Light);
    public RenderFilterPreset Viewport_Filter_Preset_6 { get; set; } = new("All", RenderFilter.All);

    #endregion

    #region Model Editor
    ///------------------------------------------------------------
    /// Model Editor
    ///------------------------------------------------------------
    // General
    public bool ModelEditor_AutoLoadSingles = true;
    public bool ModelEditor_IncludeAliasInSearch = true;

    public bool ModelEditor_ViewMeshes = true;
    public bool ModelEditor_ViewDummyPolys = true;
    public bool ModelEditor_ViewBones = true;
    public bool ModelEditor_ViewSkeleton = true;
    public bool ModelEditor_ViewHighCollision = false;
    public bool ModelEditor_ViewLowCollision = true;

    public bool ModelEditor_ExactSearch = false;

    public bool ModelEditor_Properties_Enable_Commmunity_Names = true;
    public bool ModelEditor_Properties_Enable_Commmunity_Descriptions = true;
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

    #endregion

    #region Param Editor
    ///------------------------------------------------------------
    /// Param Editor
    ///------------------------------------------------------------
    // General
    public bool ParamEditor_Enable_Compact_Mode = false;

    // Regulation
    public bool ParamEditor_Loose_Param_Mode_DS2 = false;
    public bool ParamEditor_Loose_Param_Mode_DS3 = false;
    public bool ParamEditor_Repack_Loose_Params_DS2 = false;

    public bool ParamEditor_Row_Name_Strip_DES = false;
    public bool ParamEditor_Row_Name_Strip_DS1 = false;
    public bool ParamEditor_Row_Name_Strip_DS2 = false;
    public bool ParamEditor_Row_Name_Strip_BB = false;
    public bool ParamEditor_Row_Name_Strip_DS3 = false;
    public bool ParamEditor_Row_Name_Strip_SDT = false;
    public bool ParamEditor_Row_Name_Strip_ER = false;
    public bool ParamEditor_Row_Name_Strip_AC6 = false;
    public bool ParamEditor_Row_Name_Strip_NR = false;

    public bool ParamEditor_Stripped_Row_Name_Load_DES = false;
    public bool ParamEditor_Stripped_Row_Name_Load_DS1 = false;
    public bool ParamEditor_Stripped_Row_Name_Load_DS2 = false;
    public bool ParamEditor_Stripped_Row_Name_Load_BB = false;
    public bool ParamEditor_Stripped_Row_Name_Load_DS3 = false;
    public bool ParamEditor_Stripped_Row_Name_Load_SDT = false;
    public bool ParamEditor_Stripped_Row_Name_Load_ER = false;
    public bool ParamEditor_Stripped_Row_Name_Load_AC6 = false;
    public bool ParamEditor_Stripped_Row_Name_Load_NR = false;

    // Param List
    public bool ParamEditor_Param_List_Pinned_Stay_Visible = true;
    public bool ParamEditor_Param_List_Sort_Alphabetically = true;
    public bool ParamEditor_Param_List_Display_Community_Names = false;
    public bool ParamEditor_Param_List_Display_Categories = true;

    // Table List
    public bool ParamEditor_Display_Table_List = true;
    public ParamTableRowDisplayType ParamEditor_Table_List_Row_Display_Type = ParamTableRowDisplayType.None;


    // Row List
    public bool ParamEditor_Row_List_Pinned_Stay_Visible = true;
    public bool ParamEditor_Row_List_Enable_Line_Wrapping = true;
    public bool ParamEditor_Row_List_Enable_Row_Grouping = false;
    public bool ParamEditor_Row_List_Display_Decorators = true;
    public bool ParamEditor_Row_List_Display_Modified_Row_Background = true;

    // Field List
    public ParamFieldNameMode ParamEditor_FieldNameMode = ParamFieldNameMode.Source;
    public bool ParamEditor_Field_List_Allow_Rearrangement = true;
    public bool ParamEditor_Field_List_Display_Offsets = false;
    public bool ParamEditor_Field_List_Pinned_Stay_Visible = true;
    public bool ParamEditor_Field_List_Display_Padding = true;
    public bool ParamEditor_Field_List_Display_Color_Picker = true;
    public bool ParamEditor_Field_List_Display_Graph = true;
    public bool ParamEditor_Field_List_Display_Map_Link = true;
    public bool ParamEditor_Field_List_Display_Model_Link = true;
    public bool ParamEditor_Field_List_Display_Decorators = true;
    public bool ParamEditor_Field_List_Display_Enums = true;
    public bool ParamEditor_Field_List_Display_References = true;
    public bool ParamEditor_Field_List_Display_Field_Attributes = true;
    public bool ParamEditor_Field_List_Display_Icon_Preview = true;
    public float ParamEditor_Field_List_Icon_Preview_Scale = 1.0f;
    public bool ParamEditor_Field_List_Display_Modified_Field_Background = true;

    public ParamTooltipMode ParamEditor_Field_List_Tooltip_Mode = ParamTooltipMode.OnFieldName;

    // Mass Edit
    public bool ParamEditor_Show_Advanced_Mass_Edit_Commands = false;

    public ParamFieldMassEditMode ParamEditor_Field_List_Context_Mass_Edit_Display_Mode = ParamFieldMassEditMode.AutoFill;

    public bool Param_PasteAfterSelection = false;
    public bool Param_PasteThenSelect = true;
    public ParamRowCopyBehavior Param_RowCopyBehavior = ParamRowCopyBehavior.ID;

    public bool Param_ShowVanillaColumn = true;
    public bool Param_ShowAuxColumn = true;

    public bool ParamEditor_Field_Input_Display_Traditional_Percentage = false;

    public bool Param_MassEdit_ShowAddButtons = true;

    public bool ParamEditor_Row_Context_Display_Row_Name_Input = true;
    public bool ParamEditor_Row_Context_Display_Shortcut_Tools = true;
    public bool ParamEditor_Row_Context_Display_Pin_Options = true;
    public bool ParamEditor_Row_Context_Display_Comparison_Options = true;
    public bool ParamEditor_Row_Context_Display_Reverse_Lookup = true;
    public bool ParamEditor_Row_Context_Display_Proliferate_Name = true;
    public bool ParamEditor_Row_Context_Display_Inherit_Name = true;
    public bool ParamEditor_Row_Context_Display_Row_Name_Tools = true;
    public bool ParamEditor_Row_Context_Display_Finder_Quick_Option = false;
    public bool ParamEditor_Row_Context_Display_Advanced_Options = true;

    public bool ParamEditor_Field_Context_Split = false;
    public bool ParamEditor_Field_Context_Display_Field_Name = false;
    public bool ParamEditor_Field_Context_Display_Field_Description = false;
    public bool ParamEditor_Field_Context_Display_Field_Attributes = false;
    public bool ParamEditor_Field_Context_Display_Pin_Options = true;
    public bool ParamEditor_Field_Context_Display_Comparison_Options = true;
    public bool ParamEditor_Field_Context_Display_Field_Value_Distribution = true;
    public bool ParamEditor_Field_Context_Display_Searchbar_Options = true;
    public bool ParamEditor_Field_Context_Display_Reference_Search = true;
    public bool ParamEditor_Field_Context_Display_References = true;
    public bool Param_FieldContextMenu_ImagePreview_ContextMenu = false;


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

    public bool ParamReloader_Use_Latest_Offset = true;
    public int ParamReloader_Current_Offsets = 0;


    // 
    public bool Interface_ParamEditor_Table = true;
    public bool Interface_ParamEditor_ToolWindow = true;

    // Tools
    public bool ParamEditor_Show_Tool_Mass_Edit = true;
    public bool ParamEditor_Show_Tool_Data_Finders = true;
    public bool ParamEditor_Show_Tool_Param_List_Categories = true;
    public bool ParamEditor_Show_Tool_Item_Gib = true;
    public bool ParamEditor_Show_Tool_Param_Reloader = true;
    public bool ParamEditor_Show_Tool_Param_Merger = true;
    public bool ParamEditor_Show_Tool_Pin_Groups = true;

    // Saving
    public bool Project_Automatic_Save_Include_Param_Editor = true;

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

    #endregion

    #region Text Editor
    ///------------------------------------------------------------
    /// Text Editor
    ///------------------------------------------------------------
    // Preferences
    public TextContainerCategory TextEditor_Primary_Category = TextContainerCategory.English;
    public bool TextEditor_Include_Vanilla_Cache = true;

    public bool TextEditor_Container_List_Hide_Unused_Containers = true;
    public bool TextEditor_Container_List_Display_Obsolete_Containers = false;
    public bool TextEditor_Container_List_Display_Primary_Category_Only = false;
    public bool TextEditor_Container_List_Display_Community_Names = true;
    public bool TextEditor_Container_List_Display_Source_Path = true;

    public bool TextEditor_Text_File_List_Grouped_Display = true;
    public bool TextEditor_Text_File_List_Display_ID = false;
    public bool TextEditor_Text_File_List_Display_Community_Names = true;

    public bool TextEditor_Text_Entry_List_Display_Null_Text = true;
    public bool TextEditor_Text_Entry_List_Truncate_Name = true;

    public bool TextEditor_Text_Entry_Enable_Grouped_Entries = true;
    public bool TextEditor_Text_Entry_Allow_Duplicate_ID = false;

    public bool TextEditor_Text_Clipboard_Escape_New_Lines = true;
    public bool TextEditor_Text_Clipboard_Include_ID = true;

    public int TextEditor_CreationModal_CreationCount = 1;
    public int TextEditor_CreationModal_IncrementCount = 1;
    public bool TextEditor_CreationModal_UseIncrementalTitling = false;
    public string TextEditor_CreationModal_IncrementalTitling_Prefix = "+";
    public string TextEditor_CreationModal_IncrementalTitling_Postfix = "";

    public bool TextEditor_CreationModal_UseIncrementalNaming = false;
    public string TextEditor_CreationModal_IncrementalNaming_Template = "";

    public bool TextEditor_Text_Entry_List_Ignore_ID_Check = false;

    public bool TextEditor_Text_Export_Include_Grouped_Entries = true;
    public bool TextEditor_Text_Export_Enable_Quick_Export = false;
    public string TextEditor_TextExport_QuickExportPrefix = "quick_export";

    public bool TextEditor_Language_Sync_Display_Primary_Only = false;
    public bool TextEditor_Language_Sync_Apply_Prefix = true;
    public string TextEditor_Language_Sync_Prefix = "TRANSLATE: ";

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
    public bool Project_Automatic_Save_Include_Text_Editor = true;

    public bool TextEditor_AutomaticSave_IncludeFMG = true;
    public bool TextEditor_ManualSave_IncludeFMG = true;

    #endregion

    #region Graphics Param Editor
    ///------------------------------------------------------------
    /// Graphics Param Editor
    ///------------------------------------------------------------
    public bool GparamEditor_File_List_Display_Aliases = true;

    public bool GparamEditor_Group_List_Display_Aliases = true;
    public bool GparamEditor_Group_List_Display_Empty_Group = true;
    public bool GparamEditor_Group_List_Display_Group_Add = true;

    public bool GparamEditor_Field_List_Display_Field_Add = true;
    public bool GparamEditor_Field_List_Enable_Aliases = false;

    public bool GparamEditor_Value_List_Display_Color_Edit_V4 = true;

    public ColorEditDisplayMode GparamEditor_Color_Edit_Mode = ColorEditDisplayMode.RGB;

    // Windows
    public bool Interface_GparamEditor_FileList = true;
    public bool Interface_GparamEditor_GroupList = true;
    public bool Interface_GparamEditor_FieldList = true;
    public bool Interface_GparamEditor_FieldValues = true;

    // Tools
    public bool Interface_GparamEditor_ToolWindow = true;
    public bool Interface_GparamEditor_Tool_QuickEdit = true;

    // Saving
    public bool Project_Automatic_Save_Include_Gparam_Editor = false;

    public bool GparamEditor_AutomaticSave_IncludeGPARAM = true;
    public bool GparamEditor_ManualSave_IncludeGPARAM = true;

    #endregion

    #region Texture Viewer
    ///------------------------------------------------------------
    /// Texture Viewer
    ///------------------------------------------------------------
    // General
    public bool TextureViewer_File_List_Display_Low_Detail_Entries = true;

    public bool TextureViewer_File_List_Display_Character_Aliases = true;
    public bool TextureViewer_File_List_Display_Asset_Aliases = true;
    public bool TextureViewer_File_List_Display_Part_Aliases = true;

    public bool TextureViewer_File_List_Display_Particle_Aliases = true;

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

    #endregion

    #region Material Editor
    ///------------------------------------------------------------
    /// Material Editor
    ///------------------------------------------------------------
    // General
    public bool MaterialEditor_Properties_Display_Community_Names = false;

    // Windows
    public bool Interface_MaterialEditor_SourceList = true;
    public bool Interface_MaterialEditor_FileList = true;
    public bool Interface_MaterialEditor_PropertyView = true;
    public bool Interface_MaterialEditor_ToolWindow = true;

    // Tools

    // Saving
    public bool Project_Automatic_Save_Include_Material_Editor = false;

    public bool MaterialEditor_AutomaticSave_IncludeMTD = true;
    public bool MaterialEditor_AutomaticSave_IncludeMATBIN = true;

    public bool MaterialEditor_ManualSave_IncludeMTD = true;
    public bool MaterialEditor_ManualSave_IncludeMATBIN = true;

    #endregion

    #region File Browser
    ///------------------------------------------------------------
    /// File Browser
    ///------------------------------------------------------------
    public bool Interface_FileBrowser_FileList = true;
    public bool Interface_FileBrowser_ItemViewer = true;
    public bool Interface_FileBrowser_ToolView = true;

    public bool Interface_FileBrowser_Tool_GameUnpacker = true;

    #endregion

    #region Viewport
    ///------------------------------------------------------------
    /// Viewport
    ///------------------------------------------------------------
    public float Viewport_Frame_Rate = 60.0f;
    public bool Viewport_Enable_Rendering = true;
    public bool Viewport_Enable_Texturing = true;
    public bool Viewport_Enable_Culling = true;

    public bool Viewport_Enable_Selection_Outline = true;
    public bool Viewport_Enable_Model_Masks = true;
    public bool Viewport_Enable_LOD_Facesets = false;

    public bool Viewport_Display = true;
    public bool Viewport_Display_Profiling = true;

    public Vector3 Viewport_Frame_Offset = new Vector3();
    public float Viewport_Frame_Distance = 1f;

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
    public float Viewport_Wireframe_Color_Variance = 0.11f;

    public float GFX_Renderable_Default_Wireframe_Alpha = 100.0f;


    public Vector3 Viewport_Collision_Color = new Vector3(53, 157, 255);
    public Vector3 Viewport_Connect_Collision_Color = new Vector3(146, 57, 158);
    public Vector3 Viewport_Navmesh_Color = new Vector3(157, 53, 255);
    public Vector3 Viewport_Navmesh_Gate_Color = new Vector3(50, 220, 0);

    public Vector3 Viewport_Box_Region_Base_Color = Utils.GetDecimalColor(Color.Blue);
    public Vector3 Viewport_Box_Region_Highlight_Color = Utils.GetDecimalColor(Color.DarkViolet);
    public float Viewport_Box_Region_Alpha = 75.0f;

    public Vector3 Viewport_Cylinder_Region_Base_Color = Utils.GetDecimalColor(Color.Blue);
    public Vector3 Viewport_Cylinder_Region_Highlight_Color = Utils.GetDecimalColor(Color.DarkViolet);
    public float Viewport_Cylinder_Region_Alpha = 75.0f;

    public Vector3 Viewport_Sphere_Region_Base_Color = Utils.GetDecimalColor(Color.Blue);
    public Vector3 Viewport_Sphere_Region_Highlight_Color = Utils.GetDecimalColor(Color.DarkViolet);
    public float Viewport_Sphere_Region_Alpha = 75.0f;

    public Vector3 Viewport_Point_Region_Base_Color = Utils.GetDecimalColor(Color.Yellow);
    public Vector3 Viewport_Point_Region_Highlight_Color = Utils.GetDecimalColor(Color.DarkViolet);
    public float Viewport_Point_Region_Alpha = 75.0f;

    public Vector3 Viewport_Dummy_Polygon_Base_Color = Utils.GetDecimalColor(Color.Yellow);
    public Vector3 Viewport_Dummy_Polygon_Highlight_Color = Utils.GetDecimalColor(Color.DarkViolet);
    public float Viewport_Dummy_Polygon_Alpha = 75.0f;

    public Vector3 Viewport_Bone_Marker_Base_Color = Utils.GetDecimalColor(Color.Blue);
    public Vector3 Viewport_Bone_Marker_Highlight_Color = Utils.GetDecimalColor(Color.DarkViolet);
    public float Viewport_Bone_Marker_Alpha = 75.0f;

    public Vector3 Viewport_Character_Marker_Base_Color = Utils.GetDecimalColor(Color.Firebrick);
    public Vector3 Viewport_Character_Marker_Highlight_Color = Utils.GetDecimalColor(Color.Tomato);
    public float Viewport_Character_Marker_Alpha = 75.0f;

    public Vector3 Viewport_Object_Marker_Base_Color = Utils.GetDecimalColor(Color.MediumVioletRed);
    public Vector3 Viewport_Object_Marker_Highlight_Color = Utils.GetDecimalColor(Color.DeepPink);
    public float Viewport_Object_Marker_Alpha = 75.0f;

    public Vector3 Viewport_Player_Marker_Base_Color = Utils.GetDecimalColor(Color.DarkOliveGreen);
    public Vector3 Viewport_Player_Marker_Highlight_Color = Utils.GetDecimalColor(Color.OliveDrab);
    public float Viewport_Player_Marker_Alpha = 75.0f;

    public Vector3 Viewport_Other_Marker_Base_Color = Utils.GetDecimalColor(Color.Wheat);
    public Vector3 Viewport_Other_Marker_Highlight_Color = Utils.GetDecimalColor(Color.AntiqueWhite);
    public float Viewport_Other_Marker_Alpha = 75.0f;

    public Vector3 Viewport_Point_Light_Base_Color = Utils.GetDecimalColor(Color.YellowGreen);
    public Vector3 Viewport_Point_Light_Highlight_Color = Utils.GetDecimalColor(Color.Yellow);
    public float Viewport_Point_Light_Alpha = 75.0f;

    public Vector3 Viewport_Spot_Light_Base_Color = Utils.GetDecimalColor(Color.Goldenrod);
    public Vector3 Viewport_Splot_Light_Highlight_Color = Utils.GetDecimalColor(Color.Violet);
    public float Viewport_Spot_Light_Alpha = 75.0f;

    public Vector3 Viewport_Directional_Light_Base_Color = Utils.GetDecimalColor(Color.Cyan);
    public Vector3 Viewport_Directional_Light_Highlight_Color = Utils.GetDecimalColor(Color.AliceBlue);
    public float Viewport_Directional_Light_Alpha = 75.0f;

    public Vector3 Viewport_Auto_Invade_Marker_Base_Color = Utils.GetDecimalColor(Color.Red);
    public Vector3 Viewport_Auto_Invade_Marker_Highlight_Color = Utils.GetDecimalColor(Color.DarkRed);

    public Vector3 GFX_Renderable_LightProbeSphere_BaseColor = Utils.GetDecimalColor(Color.Yellow);
    public Vector3 GFX_Renderable_LightProbeSphere_HighlightColor = Utils.GetDecimalColor(Color.YellowGreen);

    public Vector3 Viewport_Level_Connector_Marker_Base_Color = Utils.GetDecimalColor(Color.Turquoise);
    public Vector3 Viewport_Level_Connector_Marker_Highlight_Color = Utils.GetDecimalColor(Color.DarkTurquoise);

    public Vector3 Viewport_Gizmo_X_Base_Color = new(0.952f, 0.211f, 0.325f);
    public Vector3 Viewport_Gizmo_X_Highlight_Color = new(1.0f, 0.4f, 0.513f);

    public Vector3 Viewport_Gizmo_Y_Base_Color = new(0.525f, 0.784f, 0.082f);
    public Vector3 Viewport_Gizmo_Y_Highlight_Color = new(0.713f, 0.972f, 0.270f);

    public Vector3 Viewport_Gizmo_Z_Base_Color = new(0.219f, 0.564f, 0.929f);
    public Vector3 Viewport_Gizmo_Z_Highlight_Color = new(0.407f, 0.690f, 1.0f);

    public float Viewport_Untextured_Model_Brightness = 1.0f;
    public float Viewport_Untextured_Model_Saturation = 0.5f;

    public Vector3 Viewport_Selection_Outline_Color = new(1.0f, 0.5f, 0.0f);

    public bool Viewport_DisplayControls = true;
    public bool Viewport_DisplayRotationIncrement = true;
    public bool Viewport_DisplayPositionIncrement = true;
    public bool Viewport_Enable_Box_Selection = true;
    public float Viewport_Box_Selection_Distance_Threshold = 1.2f;

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

    #endregion

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
    public static CFG Current { get; set; }
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

public static class CFGHelpers
{
    public static void ResetCurrentToDefault()
    {
        if (CFG.Current == null)
        {
            CFG.Current = new CFG();
        }

        CopyValues(CFG.Default, CFG.Current);
    }

    private static void CopyValues(CFG source, CFG target)
    {
        var type = typeof(CFG);

        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            if (field.IsInitOnly)
                continue;

            var value = field.GetValue(source);
            field.SetValue(target, CloneIfNeeded(value));
        }

        foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!prop.CanRead || !prop.CanWrite)
                continue;

            if (prop.GetIndexParameters().Length > 0)
                continue;

            var value = prop.GetValue(source);
            prop.SetValue(target, CloneIfNeeded(value));
        }
    }

    private static object CloneIfNeeded(object value)
    {
        if (value == null)
            return null;

        var type = value.GetType();
        if (type.IsValueType || value is string)
            return value;

        if (value is IList<string> stringList)
            return new List<string>(stringList);

        if (value is IList<int> intList)
            return new List<int>(intList);

        if (value is IList<float> floatList)
            return new List<float>(floatList);

        return value;
    }

    public static bool ResetMemberToDefault(string memberName)
    {
        if (CFG.Current == null)
            return false;

        var type = typeof(CFG);

        var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public);
        if (field != null && !field.IsInitOnly)
        {
            var defaultValue = field.GetValue(CFG.Default);
            field.SetValue(CFG.Current, CloneIfNeeded(defaultValue));
            return true;
        }

        var prop = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public);
        if (prop != null && prop.CanRead && prop.CanWrite && prop.GetIndexParameters().Length == 0)
        {
            var defaultValue = prop.GetValue(CFG.Default);
            prop.SetValue(CFG.Current, CloneIfNeeded(defaultValue));
            return true;
        }

        return false;
    }

}