using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using static StudioCore.Editors.MapEditor.DuplicateAction;

namespace StudioCore.Application;

public class CFG
{
    // Types:
    // Preferences: shown in the Preferences menu
    // Options: exposes via ImGui elements such as checkboxes or combo-lists
    // Internal: used for persistent state

    #region Developer Kit
    public bool DEVKIT_DisplayDataSources = true;
    public string DEVKIT_DataPath_DataFolder = "";
    public string DEVKIT_DataPath_OutputFolder = "";
    public string DEVKIT_DataPath_DES = "";
    public string DEVKIT_DataPath_DS1 = "";
    public string DEVKIT_DataPath_DS1R = "";
    public string DEVKIT_DataPath_DS2 = "";
    public string DEVKIT_DataPath_DS2S = "";
    public string DEVKIT_DataPath_DS3 = "";
    public string DEVKIT_DataPath_BB = "";
    public string DEVKIT_DataPath_SDT = "";
    public string DEVKIT_DataPath_ER = "";
    public string DEVKIT_DataPath_AC6 = "";
    public string DEVKIT_DataPath_NR = "";
    public string DEVKIT_DataPath_DB_NT = "";
    public string DEVKIT_DataPath_DB = "";

    #endregion

    #region Updater

    #endregion

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
    public bool System_Ignore_Read_Asserts = false;
    public bool System_Apply_DCX_Heuristic = false;

    public bool Logger_Enable_Action_Log = true;
    public bool Logger_Enable_Warning_Log = true;
    public int Logger_Action_Fade_Time = 1500;
    public int Logger_Warning_Fade_Time = 1500;
    public bool Logger_Enable_Color_Fade = true;
    public bool Logger_Enable_Log_Popups = true;
    public bool Logger_Enable_Scope_Logging = false;
    public bool Logger_Hide_File_Dictionary_Warnings = true;

    public bool Developer_Enable_Tools = false;
    public string Developer_Smithbox_Build_Folder = "";

    public bool Block_OpenGL_Warning = false;

    #endregion

    #region Project
    ///------------------------------------------------------------
    /// Project
    ///------------------------------------------------------------
    // Preferences
    public bool Project_Enable_Auto_Load = true;
    public bool Project_Enable_Automatic_Auto_Load_Assignment = true;

    public bool Project_Enable_Backup_Saves = true;
    public bool Project_Enable_Comprehensive_Backups = false;

    public bool Project_Enable_Automatic_Save = true;
    public float Project_Automatic_Save_Interval = 300;

    public string Project_Default_Mod_Directory = "";
    public string Project_Default_Data_Directory = "";
    public string Project_ME3_Profile_Directory = "";


    public bool Project_Scan_Directory_For_Additions = true;
    public bool Project_Scan_Directory_Strict_Mode = false;
    public bool Project_VFS_Prefer_Loose_Files = false;

    // Options
    public string Project_Alias_Export_Delimiter = ";";
    public bool Project_Alias_Editor_Export_Ignore_Empty = false;

    public bool Project_Alias_Editor_Use_Base_Source = true;
    public bool Project_Alias_Editor_Use_Project_Source = true;
    public bool Project_Alias_Editor_Save_Applies_To_Base = false;
    public bool Project_Alias_Editor_Add_Insert_At_Top = false;

    public bool Project_Enum_Editor_Use_Base_Source = true;
    public bool Project_Enum_Editor_Use_Project_Source = true;
    public bool Project_Enum_Editor_Save_Applies_To_Base = false;
    public bool Project_Enum_Editor_Add_Insert_At_Top = false;

    // Metadata (Param Editor)
    public bool Param_Editor_Enable_Param_Meta_Override = false;
    public bool Param_Editor_Enable_Param_Meta_Addition = true;

    public bool Param_Editor_Enable_Param_Enum_Override = false;
    public bool Param_Editor_Enable_Param_Enum_Addition = true;

    public bool Param_Editor_Enable_Param_Category_Override = false;
    public bool Param_Editor_Enable_Param_Category_Addition = true;

    public bool Param_Editor_Enable_Param_Commutativity_Group_Override = false;
    public bool Param_Editor_Enable_Param_Commutativity_Group_Addition = true;

    public bool Param_Editor_Enable_Param_Field_Reference_Group_Override = false;
    public bool Param_Editor_Enable_Param_Field_Reference_Group_Addition = true;

    public bool Param_Editor_Enable_Param_Field_Layout_Override = false;
    public bool Param_Editor_Enable_Field_Layout_Addition = true;

    public bool Param_Editor_Enable_Icon_Configuration_Override = false;
    public bool Param_Editor_Enable_Icon_Configuration_Addition = true;

    public bool Param_Editor_Enable_Graph_Annotation_Override = false;
    public bool Param_Editor_Enable_Graph_Annotation_Addition = true;

    public bool Param_Editor_Enable_Row_FMG_Annotation_Override = false;
    public bool Param_Editor_Enable_Row_FMG_Annotation_Addition = true;

    public bool ProjectEditor_Show_Tool_ProjectMerge = true;
    #endregion

    #region Interface
    ///------------------------------------------------------------
    /// Interface
    ///------------------------------------------------------------
    public float Interface_UI_Scale = 1.0f;
    public bool Interface_Scale_by_DPI = true;
    public float Interface_Font_Size = 14.0f;
    public bool Interface_Allow_Window_Movement = true;
    public bool Interface_Tool_Window_Dropdown_Navigation = false;

    public bool Interface_Alias_Wordwrap_General = true;
    public bool Interface_Alias_Wordwrap_Map_Editor = true;
    public bool Interface_Alias_Wordwrap_Model_Editor = true;
    public bool Interface_Alias_Wordwrap_Animation_Editor = true;

    public string English_Font = Path.Join("Assets","Fonts","RobotoMono-Light.ttf");
    public string Additional_Font_1 = Path.Join("Assets","Fonts","NotoSansCJKtc-Light.otf");
    public string Additional_Font_2 = "";
    public string Additional_Font_3 = "";
    public string Additional_Font_4 = "";
    public string Additional_Font_5 = "";

    public string Interface_Selected_Theme = "";

    // Editor Presentation
    public bool Interface_Display_Title_Headers = false;
    public bool Interface_Display_Searchbar_Minimize = true;

    public EditorDisplayType Interface_MapEditor_EditorDisplayType = EditorDisplayType.Windows;
    public EditorDisplayType Interface_ModelEditor_EditorDisplayType = EditorDisplayType.Windows;

    #endregion

    #region Map Editor
    ///------------------------------------------------------------
    /// Map Editor
    ///------------------------------------------------------------
    // Preferences
    public bool MapEditor_IgnoreSaveExceptions = false;
    public bool MapEditor_SkipHavokLoad = false;

    public float MapEditor_Display_MapList_Percentage = 0.25f;
    public float MapEditor_Display_Contents_Percentage = 0.75f;

    public bool MapEditor_Map_List_Display_Map_Aliases = true;
    public bool MapEditor_Map_List_Enable_Load_on_Double_Click = false;

    // Tile outlines (ER, NR): toggled in the Map List header, colours by tile size (0-255)
    public bool MapEditor_Display_Tile_Outlines = false;
    public Vector3 MapEditor_Tile_Outline_Small_Color = new Vector3(60, 200, 255);
    public Vector3 MapEditor_Tile_Outline_Medium_Color = new Vector3(255, 210, 60);
    public Vector3 MapEditor_Tile_Outline_Large_Color = new Vector3(235, 90, 200);

    public bool MapEditor_Map_Contents_Display_Character_Aliases = true;
    public bool MapEditor_Map_Contents_Display_Asset_Aliases = true;
    public bool MapEditor_Map_Contents_Display_Map_Piece_Aliases = true;
    public bool MapEditor_Map_Contents_Display_Treasure_Aliases = true;

    public bool MapEditor_DisplayGroups_Show_Unused = false;

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
    public bool MapEditor_Model_Selector_Use_Previews = false;

    public int Interface_Model_Preview_Size = 150;
    public bool Interface_Asset_Browser_Use_Previews = true;

    public bool MapEditor_Enable_Character_Substitution = false;
    public string MapEditor_Character_Substitution_ID = "c0000";
    public bool MapEditor_Enable_Character_Composition = true;
    public bool MapEditor_Hold_Idle_Poses_On_Load = false;

    public bool MapEditor_Selection_Group_Frame_Selection_On_Use = true;
    public bool MapEditor_Selection_Group_Enable_Quick_Creation = false;
    public bool MapEditor_Selection_Group_Confirm_Delete = true;
    public bool MapEditor_Selection_Group_Show_Keybind = true;
    public bool MapEditor_Selection_Group_Show_Tags = false;

    public bool MapEditor_MapContentGroup_AutoAdjustName = true;

    public bool MapEditor_HavokEdit_Display_Type_Column = false;
    public bool MapEditor_CollisionEdit_Display_Raw_Data_Fields = true;

    // Options
    public bool MapEditor_Model_Selector_Update_Name = true;
    public bool MapEditor_Model_Selector_Update_Instance_ID = true;
    public bool MapEditor_Model_Selector_Update_NpcParams = true;

    // Model Browser: other maps' map pieces, ported into the selected map when picked
    public bool MapEditor_Model_Selector_Show_Other_Maps = true;
    public bool MapEditor_Model_Selector_Prompt_Map_Piece_Port = true;
    public bool MapEditor_Model_Selector_Port_Textures = true;

    public MapObjectNameDisplayType MapEditor_MapObjectName_DisplayType = MapObjectNameDisplayType.Internal_Community_FMG;

    public int MapEditor_Selection_Position_IncrementType = 0;
    public float MapEditor_Selection_Position_Increment_0 { get; set; } = 0.05f;
    public float MapEditor_Selection_Position_Increment_1 { get; set; } = 0.1f;
    public float MapEditor_Selection_Position_Increment_2 { get; set; } = 0.25f;
    public float MapEditor_Selection_Position_Increment_3 { get; set; } = 0.5f;
    public float MapEditor_Selection_Position_Increment_4 { get; set; } = 1.0f;

    public bool MapEditor_Selection_Position_Increment_DiscreteApplication = false;

    public bool MapEditor_Selection_Group_Enable_Shortcuts = false;

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

    public bool WorldMapFilter_IncludeSmallTiles = true;
    public bool WorldMapFilter_IncludeMediumTiles = true;
    public bool WorldMapFilter_IncludeLargeTiles = true;
    public bool WorldMapFilter_RestrictBySource = false;
    public bool WorldMapDisplayUnderground = false;

    public int MapInfo_BrushSize = 4;
    public float MapInfo_Opacity = 0.6f;
    public bool MapInfo_HighlightSelected = false;
    public bool MapInfo_ShowTileOutlines = true;

    public bool QuickView_DisplayTooltip = false;
    public List<string> QuickView_TargetProperties = new List<string>() { "Name" };

    public bool DisplayPlacementOrb = false;
    public float PlacementOrb_Distance = 5.0f;

    public bool DisplayCompass = true;
    public int CompassSize = 120;
    public int CompassOffset = 64;
    public float CompassNorthOffset = -90f;

    public bool MapEditor_MapContentList_Auto_Open_Tree = false;

    // Map Contents colours objects that differ from, or aren't in, the vanilla map
    public bool MapEditor_Vanilla_Highlight = true;
    public bool Interface_MapEditor_Tool_VanillaCompare = true;
    public bool Interface_MapEditor_Tool_History = true;
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
    public string MapEditor_ModelDataExtraction_DefaultOutputFolder = "";

    public string MapEditor_MapExtraction_OutputDirectory = "";
    public string MapEditor_MapExtraction_FileName = "map_extraction";
    public bool MapEditor_MapExtraction_IncludeTextures = true;
    public bool MapEditor_MapExtraction_IncludeMaterialMaps = true;
    public MapExtractionTextureFormat MapEditor_MapExtraction_TextureFormat = MapExtractionTextureFormat.PNG;

    public bool MapEditor_TextureTransfer_SelectionOnly = false;

    // Part Transfer
    public bool MapEditor_PartTransfer_Move_To_Orb = false;
    public bool MapEditor_PartTransfer_Rename_Parts = true;
    public bool MapEditor_PartTransfer_Copy_Textures = true;
    public bool MapEditor_MapExtraction_FlipUVs = true;
    public bool MapEditor_MapExtraction_MirrorX = false;
    public bool MapEditor_MapExtraction_CentreAtOrigin = false;

    public bool MapEditor_LightAtlas_AutomaticAdd = true;
    public bool MapEditor_LightAtlas_AutomaticDelete = true;
    public bool MapEditor_LightAtlas_AutomaticAdjust = true;


    public HavokCollisionType CurrentHavokCollisionType = HavokCollisionType.Low;

    // Map collision types displayed in the Map Editor, any combination of them can be shown at once
    public bool MapEditor_Display_Low_Collision = true;
    public bool MapEditor_Display_High_Collision = false;
    public bool MapEditor_Display_FallProtection_Collision = false;


    // Map Visibility Filters
    public bool MapEditor_BasicFilter_MapPiece = true;
    public bool MapEditor_BasicFilter_Object = true;
    public bool MapEditor_BasicFilter_Character = true;
    public bool MapEditor_BasicFilter_DummyEnemy = true;
    public bool MapEditor_BasicFilter_DummyAsset = true;
    public bool MapEditor_BasicFilter_Region = true;
    public bool MapEditor_BasicFilter_Light = false;
    public bool MapEditor_BasicFilter_Collision = true;
    public bool MapEditor_BasicFilter_ConnectCollision = true;
    public bool MapEditor_BasicFilter_ObjectCollision = false;
    public bool MapEditor_BasicFilter_Grass = false;
    public float MapEditor_Grass_Density = 1f;
    public bool MapEditor_BasicFilter_EdgePoint = false;
    public bool MapEditor_BasicFilter_Wall = false;
    public bool MapEditor_Wall_Display_Cells = true;
    public bool MapEditor_Wall_Display_NoJump = true;
    public float MapEditor_CollisionEditor_Step = 0.25f;
    public CollisionBrushMode MapEditor_CollisionEditor_Brush_Mode = CollisionBrushMode.Raise;
    public float MapEditor_CollisionEditor_Brush_Radius = 2f;
    public float MapEditor_CollisionEditor_Brush_Interval = 0.1f;
    public float MapEditor_CollisionEditor_Smooth_Strength = 0.25f;
    public bool MapEditor_CollisionEditor_Show_Cursor_Display = false;
    public bool MapEditor_CollisionEditor_Show_Controls = true;
    public bool MapEditor_CollisionEditor_Show_Cursor = true;
    public bool MapEditor_CollisionEditor_Show_Create_Material = true;
    public bool MapEditor_CollisionEditor_Show_Create_Parameters = true;
    public float MapEditor_CollisionEditor_Cursor_Fill_Alpha = 20f;
    public Vector3 MapEditor_CollisionEditor_Cursor_Color = new Vector3(0, 200, 255);
    public Vector3 MapEditor_CollisionEditor_Vertex_Color = new Vector3(255, 220, 0);
    public float MapEditor_Ruler_Point_Size = 0.15f;
    public Vector3 MapEditor_Ruler_Color = new Vector3(255, 200, 40);
    public Vector3 MapEditor_Ruler_Selected_Color = new Vector3(255, 255, 255);
    public bool MapEditor_Ruler_Show_Label = true;
    // First person mode: what the player collides with (Low or High; fall protection and assets on top)
    public HavokCollisionType MapEditor_FirstPerson_Layer = HavokCollisionType.Low;
    public bool MapEditor_FirstPerson_Fall_Protection = false;
    public bool MapEditor_FirstPerson_Assets = true;
    public bool MapEditor_FirstPerson_Dummy_Assets = false;
    // The player: eye height above the feet, capsule height and radius, steepest walkable slope (degrees)
    public float MapEditor_FirstPerson_Eye_Height = 1.5f;
    public float MapEditor_FirstPerson_Height = 1.7f;
    public float MapEditor_FirstPerson_Radius = 0.35f;
    public float MapEditor_FirstPerson_Max_Slope = 50f;
    // Movement: speeds (m/s), gravity (m/s²), jump and step heights (m)
    public float MapEditor_FirstPerson_Walk_Speed = 4f;
    public float MapEditor_FirstPerson_Run_Speed = 7f;
    public float MapEditor_FirstPerson_Gravity = 20f;
    public float MapEditor_FirstPerson_Jump_Height = 1f;
    public float MapEditor_FirstPerson_Step_Height = 0.45f;
    // While walking: hide what the game doesn't draw, show a crosshair; on leaving, keep the camera where the player was
    public bool MapEditor_FirstPerson_Hide_Helpers = true;
    public bool MapEditor_FirstPerson_Crosshair = true;
    public bool MapEditor_FirstPerson_Keep_Camera = false;
    public bool MapEditor_FirstPerson_Show_Controls = true;
    // Collision materials: Slide ground (Slide1/2) can't be stood on; the overlay names the surface underfoot
    public bool MapEditor_FirstPerson_Slide_Not_Standable = true;
    public bool MapEditor_FirstPerson_Show_Surface = true;
    public Vector3 MapEditor_FirstPerson_Spawn_Color = new Vector3(40, 230, 120);
    public Vector3 MapEditor_FirstPerson_Blocked_Color = new Vector3(255, 80, 60);
    public HavokCollisionType MapEditor_CollisionEditor_Layer = HavokCollisionType.Low;
    public bool MapEditor_CollisionEditor_Link_Layers = true;
    public float MapEditor_CollisionEditor_Link_Distance = 0.5f;
    public bool MapEditor_CollisionEditor_Show_Flver_Meshes = true;
    public bool MapEditor_CollisionEditor_Show_Flver_Simplify = true;
    public bool MapEditor_CollisionEditor_Flver_Simplify = false;
    public float MapEditor_CollisionEditor_Flver_Simplify_Target = 25f;
    public float MapEditor_CollisionEditor_Flver_Simplify_Max_Error = 0.25f;
    public bool MapEditor_CollisionEditor_Flver_Simplify_Lock_Borders = true;
    public bool MapEditor_CollisionEditor_Show_Create_Preview = true;    public HavokCollisionType MapEditor_CollisionEditor_Fall_Source = HavokCollisionType.High;
    public FallProtectionDirection MapEditor_CollisionEditor_Fall_Direction = FallProtectionDirection.All;
    public float MapEditor_CollisionEditor_Fall_Height = 1.8f;
    public int MapEditor_CollisionEditor_Fall_Filter = 1;
    public bool MapEditor_CollisionEditor_Fall_Keep_Existing = false;
    public bool MapEditor_CollisionEditor_Fall_Show_Preview = true;
    public bool MapEditor_CollisionEditor_Fall_Drop_Only = true;
    public float MapEditor_CollisionEditor_Fall_Min_Drop = 1.0f;
    public CollisionUserDefinedMode MapEditor_CollisionEditor_User_Mode = CollisionUserDefinedMode.Line;
    public CollisionUserDefinedPlacement MapEditor_CollisionEditor_User_Placement = CollisionUserDefinedPlacement.Raycast;
    public float MapEditor_CollisionEditor_User_Spawn_Spacing = 2.0f;
    public float MapEditor_CollisionEditor_User_Height = 2.0f;
    public bool MapEditor_CollisionEditor_User_Closed = false;
    public bool MapEditor_CollisionEditor_User_Flip = false;
    public float MapEditor_CollisionEditor_User_Point_Size = 0.15f;
    public bool MapEditor_CollisionEditor_User_Show_Points = true;
    public bool MapEditor_CollisionEditor_User_Show_Preview = true;
    public bool MapEditor_BasicFilter_Navmesh = true;
    public bool MapEditor_BasicFilter_NavVolume = false;
    public bool MapEditor_BasicFilter_SpeedTree = true;
    public bool MapEditor_BasicFilter_InvasionPoint = true;
    public bool MapEditor_BasicFilter_MultiPlayerWall = true;
    public bool MapEditor_BasicFilter_MapLimit = true;
    public bool MapEditor_BasicFilter_FogVolume = true;

    // Toolbar
    public bool Toolbar_Duplicate_Increment_Entity_ID = false;
    public bool Toolbar_Duplicate_Increment_PartNames = false;
    public bool Toolbar_Duplicate_Increment_InstanceID = true;
    public bool Toolbar_Duplicate_Place_at_List_End = false;
    public bool MapEditor_Paste_At_Placement_Orb = true;

    // Duplicate to Map, Move to Map and Paste keep objects where they are in the world, rather than at the same
    // local coordinates in the other map (which in ER and NR is another tile, 256 m away)
    public bool MapEditor_Keep_World_Position_Across_Maps = true;

    public bool Toolbar_Duplicate_Clear_Entity_ID = false;
    public bool Toolbar_Duplicate_Clear_Entity_Group_IDs = false;

    public bool Toolbar_Duplicate_Randomise_Rotation = false;
    public RandomiseRotationDirection Toolbar_Duplicate_Randomise_Rotation_Dir = RandomiseRotationDirection.Y;
    public float Toolbar_Duplicate_Randomise_Rotation_Range = 360;

    public bool Toolbar_Presence_Dummy_Type_ER = false;
    public bool Toolbar_Presence_Dummify = true;
    public bool Toolbar_Presence_Undummify = false;

    public bool Toolbar_Rotate_X = true;
    public bool Toolbar_Rotate_Y = false;
    public bool Toolbar_Set_Rotate = false;
    public Vector3 Toolbar_Rotate_FixedAngle = new Vector3(0, 0, 0);

    public bool Toolbar_Move_to_Camera_Offset_Specific_Input = false;
    public float Toolbar_Move_to_Camera_Offset = 3.0f;

    // Snap to Mesh
    public int MapEditor_SnapToMesh_Direction = 0;
    public float MapEditor_SnapToMesh_Offset = 0.0f;
    public float MapEditor_SnapToMesh_MaxDistance = 500.0f;
    public bool MapEditor_SnapToMesh_Individually = false;

    public bool Toolbar_Rotate_Specific_Input = false;

    public bool Toolbar_Rotate_HandleAsSingleObject = true;
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

    public bool Toolbar_LowDetail_Use_Naming = true;
    public bool Toolbar_LowDetail_Use_Density = true;
    public float Toolbar_LowDetail_Max_Density = 2.0f;
    public float Toolbar_LowDetail_Min_Size = 80.0f;

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

    public bool Replicator_Mode_Line = false;
    public bool Replicator_Mode_Circle = false;
    public bool Replicator_Mode_Square = false;
    public bool Replicator_Mode_Sphere = false;
    public bool Replicator_Mode_Box = false;
    public bool Replicator_Mode_Group = true;

    public int Replicator_Group_Clone_Amount = 1;
    public int Replicator_Group_Position_Offset = 25;

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

    // Param Properties
    public bool MapEditor_Field_List_Display_Padding = false;
    public bool MapEditor_Field_List_Display_References = false;

    // Windows
    public bool Interface_MapEditor_Viewport_Grid = true;
    public bool Interface_MapEditor_MapList = true;
    public bool Interface_MapEditor_MapContents = true;
    public bool Interface_MapEditor_Properties = true;
    public bool Interface_MapEditor_RenderGroups = true;
    public bool Interface_MapEditor_AssetBrowser = true;
    public bool Interface_MapEditor_ToolWindow = true;
    public bool Interface_MapEditor_MapInformation = false;
    public bool Interface_MapEditor_ResourceList = true;

    // Tools
    public bool Interface_MapEditor_Tool_Create = true;
    public bool Interface_MapEditor_Tool_Duplicate = true;
    public bool Interface_MapEditor_Tool_DuplicateToMap = true;
    public bool Interface_MapEditor_Tool_Replicate = true;
    public bool Interface_MapEditor_Tool_Translate = true;
    public bool Interface_MapEditor_Tool_Rotate = true;
    public bool Interface_MapEditor_Tool_PullToCamera = true;
    public bool Interface_MapEditor_Tool_SnapToMesh = true;
    public bool Interface_MapEditor_Tool_Scramble = true;
    public bool Interface_MapEditor_Tool_ResetTransform = true;
    public bool Interface_MapEditor_Tool_Properties_Action = true;
    public bool Interface_MapEditor_Tool_Select_Action = true;
    public bool Interface_MapEditor_Tool_ViewportFilters = true;
    public bool Interface_MapEditor_Tool_EditorVisibility = true;
    public bool Interface_MapEditor_Tool_LowDetail = true;
    public bool Interface_MapEditor_Tool_Viewport = true;
    public bool Interface_MapEditor_Tool_Data_Transfer = true;

    public bool Interface_MapEditor_Tool_Prefab = true;
    public bool Interface_MapEditor_Tool_SelectionGroups = true;
    public bool Interface_MapEditor_Tool_LocalSearch = true;
    public bool Interface_MapEditor_Tool_GlobalSearch = true;
    public bool Interface_MapEditor_Tool_MassEdit = true;
    public bool Interface_MapEditor_Tool_TreasureMaker = false;
    public bool Interface_MapEditor_Tool_WorldMapLayoutGenerator = false;
    public bool Interface_MapEditor_Tool_GridConfiguration = true;
    public bool Interface_MapEditor_Tool_ViewportRendering = true;
    public bool Interface_MapEditor_Tool_ModelBrowser = true;
    public bool Interface_MapEditor_Tool_Animation = true;
    public bool Interface_MapEditor_Tool_DisplayGroups = true;
    public bool Interface_MapEditor_Tool_MsbValidation = true;
    public bool Interface_MapEditor_Tool_EntityIds = true;
    public bool Interface_MapEditor_Tool_IrradianceVolumeIds = true;
    public bool Interface_MapEditor_Tool_EnemyAIVisualization = true;
    public bool Interface_MapEditor_Tool_WallPaint = true;
    public bool Interface_MapEditor_Tool_CollisionEditor = true;
    public bool Interface_MapEditor_Tool_Navigation = true;
    public bool Interface_MapEditor_Tool_Ruler = true;
    public bool Interface_MapEditor_Tool_FirstPerson = true;
    public bool Interface_MapEditor_Tool_MapExtraction = true;
    public bool Interface_MapEditor_Tool_TextureTransfer = true;
    public bool Interface_MapEditor_Tool_PartTransfer = true;
    public bool Interface_MapEditor_Tool_DestructionGeneration = true;
    public bool Interface_MapEditor_Tool_LoadList = true;
    public bool Interface_MapEditor_Tool_EntryFileList = true;
    public bool Interface_MapEditor_Tool_PartsTokenIdList = true;
    public bool Interface_MapEditor_Tool_AssetBrowser = true;
    public bool Interface_MapEditor_Tool_ViewportConfig = true;

    // Saving
    public bool Project_Automatic_Save_Include_Map_Editor = true;

    public bool MapEditor_Allow_Unmodified_Save = false;

    // Save Check: File › Save checks the maps it will write first, and stops on errors (or warnings too)
    public bool MapEditor_SaveCheck_Enabled = true;
    public bool MapEditor_SaveCheck_Stop_On_Warnings = false;
    public bool MapEditor_Allow_Automatic_Save = true;

    // Navigation tool: the tabs whose "How this works" section has been shown once, so it starts closed on them
    public List<string> MapEditor_Navigation_Help_Seen = new List<string>();
    public List<string> MapEditor_CollisionEditor_Help_Seen = new List<string>();

    // Scene Filters
    public RenderFilter LastSceneFilter { get; set; } = RenderFilter.All ^ RenderFilter.Light;
    public RenderFilterPreset Viewport_Filter_Preset_1 { get; set; } = new("Map", RenderFilter.MapPiece | RenderFilter.Object | RenderFilter.Character | RenderFilter.DummyEnemy | RenderFilter.DummyAsset | RenderFilter.Region);
    public RenderFilterPreset Viewport_Filter_Preset_2 { get; set; } = new("Collision", RenderFilter.Collision | RenderFilter.Object | RenderFilter.Character | RenderFilter.DummyEnemy | RenderFilter.DummyAsset | RenderFilter.Region);
    public RenderFilterPreset Viewport_Filter_Preset_3 { get; set; } = new("Collision & Navmesh", RenderFilter.Collision | RenderFilter.Navmesh | RenderFilter.Object | RenderFilter.Character | RenderFilter.DummyEnemy | RenderFilter.DummyAsset | RenderFilter.Region);
    public RenderFilterPreset Viewport_Filter_Preset_4 { get; set; } = new("Lighting (Map)", RenderFilter.MapPiece | RenderFilter.Object | RenderFilter.Character | RenderFilter.DummyEnemy | RenderFilter.DummyAsset | RenderFilter.Light);
    public RenderFilterPreset Viewport_Filter_Preset_5 { get; set; } = new("Lighting (Collision)", RenderFilter.Collision | RenderFilter.Object | RenderFilter.Character | RenderFilter.DummyEnemy | RenderFilter.DummyAsset | RenderFilter.Light);
    public RenderFilterPreset Viewport_Filter_Preset_6 { get; set; } = new("All", RenderFilter.All);

    #endregion

    #region Model Editor
    ///------------------------------------------------------------
    /// Model Editor
    ///------------------------------------------------------------
    // General
    public bool ModelEditor_Containers_IncludeAliasInSearch = true;

    public bool ModelEditor_Files_AutoLoadFirstEntry = true;

    public bool ModelEditor_Contents_Auto_Open_Tree = false;
    public bool ModelEditor_Contents_NodeNameInMeshEntry = true;

    // Redraw the model from its edited data shortly after each edit, rather than from the stored file
    public bool ModelEditor_Viewport_Live_Preview = true;
    public bool ModelEditor_Contents_Node_Hierarchy = false;

    // Save Check: File › Save and Save All check the models they will write first, and stop on errors (or warnings too)
    public bool ModelEditor_SaveCheck_Enabled = true;
    public bool ModelEditor_SaveCheck_Stop_On_Warnings = false;

    // History: how many models switched away from keep their objects and undo history in each view (0 = none)
    public int ModelEditor_History_Parked_Limit = 4;

    public bool ModelEditor_ViewMeshes = true;
    public bool ModelEditor_ViewDummyPolys = true;
    public bool ModelEditor_ViewBones = true;
    public bool ModelEditor_ViewSkeleton = true;
    public bool ModelEditor_ViewHighCollision = false;
    public bool ModelEditor_ViewLowCollision = true;

    public bool ModelEditor_ExactSearch = false;

    public bool ModelEditor_Properties_Enable_Commmunity_Names = true;
    public bool ModelEditor_Properties_Enable_Type_Column = false;
    public bool ModelEditor_Properties_Auto_Open_Tree = true;
    public bool ModelEditor_Properties_Enable_Mesh_Fields = true;



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

    public string ModelExport_Output_Directory = "";
    public bool ModelExport_UseCollisionMaterial = false;
    public bool ModelExport_UseSingleCollisionMaterial = false;
    public bool ModelExport_FlipUVs = true;
    public bool ModelExport_MirrorX = false;
    public bool ModelExport_MirrorY = false;
    public bool ModelExport_MirrorZ = false;
    public ModelExportAxis ModelExport_ForwardAxis = ModelExportAxis.PosZ;
    public ModelExportAxis ModelExport_UpAxis = ModelExportAxis.PosY;

    public float DummyMeshSize = 0.05f;
    public float NodeMeshSize = 0.05f;
    public bool ModelEditor_DisplayDummyLabels = false;
    public bool ModelEditor_DisplayNodeLabels = false;
    public bool ModelEditor_DisplayDummies = true;
    public bool ModelEditor_DisplayNodes = true;
    public float ModelEditor_LabelScale = 1.0f;
    public bool DisplayEdgePoints = false;
    public Vector3 StartEdgePoint = new Vector3();
    public Vector3 EndEdgePoint = new Vector3();
    public Vector3 PullEdgePoint = new Vector3();

    public bool ModelEditor_ModelSourceList_RequireDoubleClick = false;

    // Windows
    public bool Interface_ModelEditor_Viewport_Grid = true;
    public bool Interface_ModelEditor_ResourceList = true;
    public bool Interface_ModelEditor_ToolWindow = true;
    public bool Interface_ModelEditor_ScreenshotMode = false;

    // Tools
    public bool Interface_ModelEditor_Tool_CreateAction = true;
    public bool Interface_ModelEditor_Tool_ModelGridConfiguration = true;
    public bool Interface_ModelEditor_Tool_ViewportRendering = true;
    public bool Interface_ModelEditor_Tool_ModelInsight = true;
    public bool Interface_ModelEditor_Tool_ModelInstanceFinder = true;
    public bool Interface_ModelEditor_Tool_ModelMaskToggler = true;
    public bool Interface_ModelEditor_Tool_Animation = true;
    public bool Interface_ModelEditor_Tool_CharacterPreview = true;
    public bool Interface_ModelEditor_Tool_DisplayRotation = true;
    public bool Interface_ModelEditor_Tool_History = true;
    public bool Interface_ModelEditor_Tool_ResourceMonitor = true;
    public bool Interface_ModelEditor_Tool_Data_Transfer = true;
    public bool Interface_ModelEditor_Tool_ModelExport = true;
    public bool Interface_ModelEditor_Tool_AssetBrowser = true;
    public bool Interface_ModelEditor_Tool_ViewportConfig = true;

    // Saving
    public bool ModelEditor_Allow_Unmodified_Save = false;
    public bool ModelEditor_Allow_Automatic_Save = true;
    public bool AutomaticSave_ModelEditor = true;

    #endregion

    #region Time Act Editor
    ///------------------------------------------------------------
    /// Time Act Editor
    ///------------------------------------------------------------
    public bool Interface_TimeActEditor_ToolWindow = true;

    public bool TimeActEditor_Allow_Unmodified_Save = false;
    public bool TimeActEditor_Allow_Automatic_Save = true;
    public string TimeActEditor_Export_Directory = "";
    public bool AutomaticSave_TimeActEditor = true;

    public bool Interface_TimeActEditor_Tool_ResourceMonitor = true;
    public bool Interface_TimeActEditor_Tool_ViewportRendering = true;
    public bool Interface_TimeActEditor_Tool_GridConfiguration = true;
    public bool Interface_TimeActEditor_Tool_CharacterPreview = true;
    public bool Interface_TimeActEditor_Tool_ModelMaskToggler = true;
    public bool Interface_TimeActEditor_Tool_LocalSearch = true;
    public bool Interface_TimeActEditor_Tool_GlobalSearch = true;
    public bool Interface_TimeActEditor_Tool_MassEdit = true;
    public bool Interface_TimeActEditor_Tool_ComboBuilder = true;
    public bool Interface_TimeActEditor_Tool_History = true;

    public float TimeActEditor_Graph_Frame_Width = 10f;
    public bool TimeActEditor_DisplayDummies = true;
    public bool TimeActEditor_DisplayNodes = true;
    public bool TimeActEditor_DisplayDummyLabels = false;
    public bool TimeActEditor_DisplayNodeLabels = false;
    public bool TimeActEditor_Graph_Display_Outside_Playback = true;
    public bool TimeActEditor_Graph_Display_Event_Names = true;
    public bool TimeActEditor_Display_Vanilla_Markers = true;
    public bool TimeActEditor_Display_Reverse_Links = true;
    public bool TimeActEditor_Auto_Play_Animation = false;
    public bool TimeActEditor_Display_Event_Markers = true;
    public bool TimeActEditor_Display_Attack_Capsules = true;
    public bool TimeActEditor_Apply_Root_Motion = true;
    public bool TimeActEditor_Display_Root_Motion = true;
    public bool TimeActEditor_Play_Event_Sounds = true;
    public float TimeActEditor_Event_Marker_Size = 1f;
    public TimeActAliasDisplayMode TimeActEditor_Animation_List_Alias_Display = TimeActAliasDisplayMode.Wrap;
    public TimeActFieldNameMode TimeActEditor_Field_Name_Mode = TimeActFieldNameMode.Annotation;
    public bool TimeActEditor_Hide_Obsolete_Fields = false;

    #endregion

    #region Animation Editor
    public bool Interface_AnimEditor_Viewport_Grid = true;
    public bool Interface_AnimEditor_ResourceList = true;
    public bool Interface_AnimEditor_ToolWindow = true;

    public bool AnimEditor_AutomaticSave_IncludeTAE = true;
    public bool AnimEditor_ManualSave_IncludeTAE = true;
    public bool AnimEditor_AutomaticSave_IncludeBEH = true;
    public bool AnimEditor_ManualSave_IncludeBEH = true;
    public bool AutomaticSave_AnimlEditor = true;
    #endregion

    #region Param Editor
    ///------------------------------------------------------------
    /// Param Editor
    ///------------------------------------------------------------
    // General
    public string ParamEditor_Annotation_Language = "English";
    public bool ParamEditor_Enable_Compact_Mode = false;
    public bool ParamEditor_Enable_Table_Borders = true;

    public string ParamEditor_Import_Language = "English";

    // Regulation
    public bool ParamEditor_Loose_Param_Mode_DS2 = false;
    public bool ParamEditor_Loose_Param_Mode_DS3 = false;
    public bool ParamEditor_Repack_Loose_Params_DS2 = false;
    public ParamSaveCompressionType ParamEditor_CompressionOverride = ParamSaveCompressionType.Default;

    public bool ParamEditor_Row_Name_Strip_DES = false;
    public bool ParamEditor_Row_Name_Strip_DS1 = false;
    public bool ParamEditor_Row_Name_Strip_DS2 = true;
    public bool ParamEditor_Row_Name_Strip_BB = false;
    public bool ParamEditor_Row_Name_Strip_DS3 = false;
    public bool ParamEditor_Row_Name_Strip_SDT = false;
    public bool ParamEditor_Row_Name_Strip_ER = false;
    public bool ParamEditor_Row_Name_Strip_AC6 = false;
    public bool ParamEditor_Row_Name_Strip_NR = false;
    public bool ParamEditor_Row_Name_Strip_DB = false;

    public bool ParamEditor_Stripped_Row_Name_Load_DES = false;
    public bool ParamEditor_Stripped_Row_Name_Load_DS1 = false;
    public bool ParamEditor_Stripped_Row_Name_Load_DS2 = true;
    public bool ParamEditor_Stripped_Row_Name_Load_BB = false;
    public bool ParamEditor_Stripped_Row_Name_Load_DS3 = false;
    public bool ParamEditor_Stripped_Row_Name_Load_SDT = false;
    public bool ParamEditor_Stripped_Row_Name_Load_ER = false;
    public bool ParamEditor_Stripped_Row_Name_Load_AC6 = false;
    public bool ParamEditor_Stripped_Row_Name_Load_NR = false;
    public bool ParamEditor_Stripped_Row_Name_Load_DB = false;

    // Param List
    public bool ParamEditor_Param_List_Pinned_Stay_Visible = true;
    public bool ParamEditor_Param_List_Sort_Alphabetically = true;
    public bool ParamEditor_Param_List_Display_Community_Names = false;
    public bool ParamEditor_Param_List_Display_Categories = true;
    public bool ParamEditor_Param_List_Display_StayParams = true;
    public bool ParamEditor_Param_List_Display_FarModelParams = true;

    // Table List
    public bool ParamEditor_Display_Table_List = true;


    // Row List
    public bool ParamEditor_Row_List_Pinned_Stay_Visible = true;
    public bool ParamEditor_Row_List_Enable_Line_Wrapping = true;
    public bool ParamEditor_Row_List_Enable_Row_Grouping = false;
    public bool ParamEditor_Row_List_Display_Decorators = true;
    public bool ParamEditor_Row_List_Display_Modified_Row_Bg = false;
    public bool ParamEditor_Row_List_Row_FMG_Prefer_Base = true;

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
    public bool ParamEditor_Field_List_Display_Modified_Field_Bg = true;
    public bool ParamEditor_Field_List_GroupReference_DisplayCommunityName = true;

    public bool ParamEditor_Field_List_Enable_Field_Layouts = true;
    public bool ParamEditor_Field_List_Enable_Field_Layout_Category_Names = true;
    public bool ParamEditor_Field_List_Enable_Field_Layout_Chance_Hints = true;
    public FieldLayoutMode ParamEditor_Field_List_Field_Layout_Display_Type = FieldLayoutMode.Header;
    public FieldLayoutUnsortedPlacement ParamEditor_Field_List_Unsorted_Field_Placement = FieldLayoutUnsortedPlacement.Bottom;

    public ParamTooltipMode ParamEditor_Field_List_Tooltip_Mode = ParamTooltipMode.OnFieldName;

    // Mass Edit
    public bool ParamEditor_Show_Advanced_Mass_Edit_Commands = false;

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

    public bool ParamEditor_DeltaPatcher_Import_Display_All_Entries = false;
    public bool ParamEditor_DeltaPatcher_Import_Modified_Rows = true;
    public bool ParamEditor_DeltaPatcher_Import_Added_Rows = true;
    public bool ParamEditor_DeltaPatcher_Import_Deleted_Rows = true;

    public bool ParamEditor_DeltaPatcher_Import_Restrict_Row_Modify = false;
    public bool ParamEditor_DeltaPatcher_Import_Restrict_Row_Add = false;
    public bool ParamEditor_DeltaPatcher_Import_Allow_Row_Overwrite = false;

    public bool ParamEditor_DeltaPatcher_Export_Selected_Rows_Only = false;
    public bool ParamEditor_DeltaPatcher_Export_Ignore_Indexed_Rows = true;

    public List<string> ParamEditor_MassEdit_QuickCommands = new();
    public int ParamEditor_MassEdit_QuickCommands_PageCount = 10;

    // 
    public bool Interface_ParamEditor_Table = true;
    public bool Interface_ParamEditor_ToolWindow = true;

    // Tools
    public bool ParamEditor_Show_Tool_Sort_Rows = true;
    public bool ParamEditor_Show_Tool_Row_Names = true;
    public bool ParamEditor_Show_Tool_Data_Converter = true;
    public bool ParamEditor_Show_Tool_Data_Comparison = true;
    public bool ParamEditor_Show_Tool_Mass_Edit = true;
    public bool ParamEditor_Show_Tool_Data_Finders = true;
    public bool ParamEditor_Show_Tool_Param_List_Categories = true;
    public bool ParamEditor_Show_Tool_Item_Gib = true;
    public bool ParamEditor_Show_Tool_Param_Reloader = true;
    public bool ParamEditor_Show_Tool_Param_Merger = true;
    public bool ParamEditor_Show_Tool_Param_Upgrader = true;
    public bool ParamEditor_Show_Tool_Pin_Groups = true;
    public bool ParamEditor_Show_Tool_Param_Delta_Patcher = true;
    public bool ParamEditor_Show_Tool_Row_Name_Manipulation = true;
    public bool ParamEditor_Show_Tool_Projectile_Simulator = true;
    public bool ParamEditor_Show_Tool_Modified_Filter = true;
    public bool ParamEditor_Show_Projectile_Simulator_Window = false;

    // Projectile Simulator
    public float ParamEditor_ProjectileSimulator_LaunchHeight = 1.5f;
    public float ParamEditor_ProjectileSimulator_EmitHeight = 1.0f;
    public float ParamEditor_ProjectileSimulator_FixedEmitRadius = 6.0f;
    public float ParamEditor_ProjectileSimulator_BehindTargetHeight = 8.0f;
    public float ParamEditor_ProjectileSimulator_BehindTargetDistance = 2.0f;
    public CastOrientation ParamEditor_ProjectileSimulator_Orientation = CastOrientation.Forward;
    public bool ParamEditor_ProjectileSimulator_GroundCollision = true;
    public bool ParamEditor_ProjectileSimulator_SimulateWall = false;
    public float ParamEditor_ProjectileSimulator_WallDistance = 10.0f;
    public float ParamEditor_ProjectileSimulator_WallWidth = 12.0f;
    public float ParamEditor_ProjectileSimulator_WallHeight = 8.0f;
    public ImpactSurface ParamEditor_ProjectileSimulator_Surface = ImpactSurface.Map;
    public bool ParamEditor_ProjectileSimulator_SimulateTarget = false;
    public Vector3 ParamEditor_ProjectileSimulator_TargetPosition = new Vector3(5.0f, 1.5f, 15.0f);
    public float ParamEditor_ProjectileSimulator_TargetRadius = 0.5f;
    public TargetMotion ParamEditor_ProjectileSimulator_TargetMotion = TargetMotion.Still;
    public float ParamEditor_ProjectileSimulator_TargetSpeed = 5.0f;
    public float ParamEditor_ProjectileSimulator_TargetRange = 5.0f;
    public bool ParamEditor_ProjectileSimulator_UseEnemyAttacks = false;
    public int ParamEditor_ProjectileSimulator_Seed = 1;

    // Saving
    public bool Project_Automatic_Save_Include_Param_Editor = true;

    public bool ParamEditor_Allow_Unmodified_Save = false;
    public bool ParamEditor_Allow_Automatic_Save = true;

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

    public bool Param_CSV_Ignore_Existing_Rows = false;
    public bool Param_CSV_Append_Only = false;
    public bool Param_CSV_Replace_Row = false;

    #endregion

    #region Text Editor
    ///------------------------------------------------------------
    /// Text Editor
    ///------------------------------------------------------------
    // Preferences
    public TextContainerCategory TextEditor_Primary_Category = TextContainerCategory.English;

    public string TextEditor_Primary_Language_DES = "English";
    public string TextEditor_Primary_Language_DS1 = "English";
    public string TextEditor_Primary_Language_DS1R = "English";
    public string TextEditor_Primary_Language_DS2 = "English";
    public string TextEditor_Primary_Language_DS2S = "English";
    public string TextEditor_Primary_Language_BB = "English";
    public string TextEditor_Primary_Language_DS3 = "English";
    public string TextEditor_Primary_Language_SDT = "English";
    public string TextEditor_Primary_Language_ER = "English";
    public string TextEditor_Primary_Language_AC6 = "English";
    public string TextEditor_Primary_Language_NR = "English";
    public string TextEditor_Primary_Language_DB_NT = "English";
    public string TextEditor_Primary_Language_DB = "English";

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
    public bool TextEditor_Text_Entry_List_Display_Null_Entries = true;
    public bool TextEditor_Text_Entry_List_Allow_Select_All = false;

    public bool TextEditor_Text_Entry_Enable_Grouped_Entries = true;
    public bool TextEditor_Text_Entry_Allow_Duplicate_ID = false;

    // Wrapped, markup-highlighted preview under each Contents text box
    public bool TextEditor_Contents_Show_Text_Preview = true;

    public bool TextEditor_Text_Clipboard_Escape_New_Lines = true;
    public bool TextEditor_Text_Clipboard_Include_ID = true;

    public int TextEditor_CreationModal_CreationCount = 1;
    public int TextEditor_CreationModal_IncrementCount = 1;
    public bool TextEditor_CreationModal_UseIncrementalNaming = false;
    public string TextEditor_CreationModal_IncrementalTitling_Prefix = "+";
    public string TextEditor_CreationModal_IncrementalTitling_Postfix = "";

    public bool TextEditor_CreationModal_UseTemplateNaming = false;
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
    public bool Interface_TextEditor_ToolWindow = true;

    // Tools
    public bool Interface_TextEditor_Tool_EntryCreator = true;
    public bool Interface_TextEditor_Tool_DataTransfer = true;
    public bool Interface_TextEditor_Tool_LanguageSync = true;
    public bool Interface_TextEditor_Tool_TextSearch = true;
    public bool Interface_TextEditor_Tool_TextReplacement = true;
    public bool Interface_TextEditor_Tool_TextMerge = true;

    // Saving
    public bool Project_Automatic_Save_Include_Text_Editor = true;

    public bool TextEditor_Allow_Unmodified_Save = false;
    public bool TextEditor_Allow_Automatic_Save = true;

    #endregion

    #region Graphics Param Editor
    ///------------------------------------------------------------
    /// Graphics Param Editor
    ///------------------------------------------------------------
    public string GparamEditor_Annotation_Language = "English";

    public bool GparamEditor_File_List_Display_BB_BND_Files = true;
    public bool GparamEditor_File_List_Display_Aliases = true;

    public bool GparamEditor_Group_List_Display_Descriptions = true;
    public bool GparamEditor_Group_List_Display_Empty_Group = true;

    public bool GparamEditor_Field_List_Display_Descriptions = true;
    public bool GparamEditor_Highlight_Viewport_Fields = true;

    public bool GparamEditor_Value_List_Display_Color_Edit_V4 = true;
    public bool GparamEditor_Value_List_Display_Time_Of_Day_Column = true;
    public bool GparamEditor_Value_List_Display_Information_Column = true;

    public ColorEditDisplayMode GparamEditor_Color_Edit_Mode = ColorEditDisplayMode.RGB;

    // Transfer: the folder the last export or import used, where the file dialogs open
    public string GparamEditor_Transfer_Last_Folder = "";

    // Transfer: the paste mode for each payload kind (see GparamPasteModes)
    public GparamFilePasteMode GparamEditor_Transfer_File_Mode = GparamFilePasteMode.ReplaceAll;
    public GparamKeyedPasteMode GparamEditor_Transfer_Group_Mode = GparamKeyedPasteMode.MatchKey;
    public GparamKeyedPasteMode GparamEditor_Transfer_Field_Mode = GparamKeyedPasteMode.MatchKey;
    public GparamValuePasteMode GparamEditor_Transfer_Values_Mode = GparamValuePasteMode.InsertAfter;

    // Windows
    public bool Interface_GparamEditor_FileList = true;
    public bool Interface_GparamEditor_GroupList = true;
    public bool Interface_GparamEditor_FieldList = true;
    public bool Interface_GparamEditor_FieldValues = true;

    // Tools
    public bool Interface_GparamEditor_ToolWindow = true;
    public bool Interface_GparamEditor_Tool_DataTransfer = true;
    public bool Interface_GparamEditor_Tool_LocalSearch = true;
    public bool Interface_GparamEditor_Tool_GlobalSearch = true;
    public bool Interface_GparamEditor_Tool_MassEdit = true;
    public bool Interface_GparamEditor_Tool_ViewportPreview = true;
    public bool Interface_GparamEditor_Tool_ViewportRendering = true;
    public bool Interface_GparamEditor_Tool_GridConfiguration = true;

    // Viewport: the map shown is the selected GPARAM's (or the one chosen), lit by the editor's copy of the GPARAM.
    // The time of day and weather are the Map Editor's (Viewport Rendering: Map Lighting), unless following the value.
    public bool GparamEditor_Viewport_Auto_Load_Map = false;
    public bool GparamEditor_Viewport_Load_Assets = true;
    public bool GparamEditor_Viewport_Follow_Value_Time = false;

    // Saving
    public bool Project_Automatic_Save_Include_Gparam_Editor = false;

    public bool GparamEditor_Allow_Unmodified_Save = false;
    public bool GparamEditor_Allow_Automatic_Save = true;

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
    public TextureExportType TextureViewerToolbar_ExportTextureType = TextureExportType.DDS;

    // Windows
    public bool Interface_TextureViewer_ToolWindow = true;
    public bool Interface_TextureViewer_Properties = true;
    public bool Interface_TextureViewer_Layout = true;

    // Layout editor (T5)
    public bool TextureViewerLayout_ShowCells = true;
    public bool TextureViewerLayout_ShowNames = false;
    public int TextureViewerLayout_Snap = 1;

    // Tools
    public bool Interface_TextureViewer_Tool_ExportTexture = true;
    public bool Interface_TextureViewer_Tool_ImportTexture = true;

    // Texture Import: 0 = keep the replaced texture's format, otherwise the DXGI format to write
    public int TextureViewerImport_Format = 0;
    // 0 = match the replaced texture, 1 = full chain, 2 = top level only
    public int TextureViewerImport_MipMode = 0;
    // Resize imported images to the replaced texture's size
    public bool TextureViewerImport_MatchSize = false;
    public string TextureViewerImport_LastFolder = "";
    // New cells: 0 = the image's size, 1 = the selected cell's size
    public int TextureViewerImport_CellSizeMode = 0;
    // Double a full sheet to fit new cells
    public bool TextureViewerImport_GrowSheet = false;
    // Low twin size of new cells: 0 = guess from the name family, 1 = full size, 2 = half size
    public int TextureViewerTwin_ScaleMode = 0;

    #endregion

    #region Material Editor
    ///------------------------------------------------------------
    /// Material Editor
    ///------------------------------------------------------------
    // General
    public bool MaterialEditor_FileList_DisplayFullPath = false;

    public bool MaterialEditor_Properties_Display_Community_Names = false;
    public bool MaterialEditor_Properties_Display_Type_Column = false;

    // Windows
    public bool Interface_MaterialEditor_ToolWindow = true;

    // Tools
    public bool MaterialEditor_Tool_Data_Transfer = true;
    public bool Interface_MaterialEditor_Tool_MaterialCheck = true;
    public bool MaterialEditor_Live_Preview = true;
    public bool MaterialEditor_FileList_Modified_Only = false;
    public bool Interface_MaterialEditor_Tool_Search = true;
    public bool Interface_MaterialEditor_Tool_UsedBy = true;
    public bool Interface_MaterialEditor_Tool_ShaderInfo = true;

    public bool MaterialEditor_Textures_Display_Preview = true;

    // Saving
    public bool Project_Automatic_Save_Include_Material_Editor = false;

    public bool MaterialEditor_Allow_Unmodified_Save = false;
    public bool MaterialEditor_Allow_Automatic_Save = true;

    #endregion

    #region File Browser
    ///------------------------------------------------------------
    /// File Browser
    ///------------------------------------------------------------
    public bool Interface_FileBrowser_FileList = true;
    public bool Interface_FileBrowser_ItemViewer = true;
    public bool Interface_FileBrowser_ToolView = true;

    public bool Interface_FileBrowser_Tool_GameUnpacker = true;
    public bool Interface_FileBrowser_Tool_FileExtract = true;
    public bool Interface_FileBrowser_Tool_BinderSearch = true;

    public string UnpackDirectory = "";
    public string ExtractDirectory = "";

    // File Browser unpacker: how many files are unpacked at the same time
    public int FileBrowser_Unpack_Threads = 8;

    // File Browser exporter: replace files that already exist in the export directory (off = skip them)
    public bool FileBrowser_Export_Overwrite = false;

    // File Browser exporter: put inner files and textures in a folder named after their container, at the
    // container's path (e.g. chr/c0000-chrbnd-dcx/c0000.flver), instead of directly in the export directory
    public bool FileBrowser_Export_Keep_Container_Path = false;

    // File Browser exporter: write DCX-compressed files decompressed, without their ".dcx"
    public bool FileBrowser_Export_Decompress = false;

    // File Browser exporter: the format textures are written in
    public StudioCore.Editors.TextureViewer.TextureExportType FileBrowser_Export_Texture_Type = StudioCore.Editors.TextureViewer.TextureExportType.DDS;

    #endregion

    #region Map Data Editor
    ///------------------------------------------------------------
    /// Map Data Editor
    ///------------------------------------------------------------
    // General
    public bool MapDataEditor_CategoryView_Display_Empty_Base_Categories = true;
    public bool MapDataEditor_CategoryView_Display_Empty_Sub_Categories = true;

    #endregion

    #region Behavior Editor
    ///------------------------------------------------------------
    /// Behavior Editor (AC6 .bhv)
    ///------------------------------------------------------------
    public bool BehaviorEditor_Allow_Unmodified_Save = false;
    public bool BehaviorEditor_Allow_Automatic_Save = true;
    public bool BehaviorEditor_Properties_Show_Hex = true;
    public bool BehaviorEditor_Transitions_Game_Order = true;
    public bool Interface_BehaviorEditor_ToolWindow = true;
    public bool Interface_BehaviorEditor_Tool_Search = true;
    public bool Interface_BehaviorEditor_Tool_UsedBy = true;
    public bool Interface_BehaviorEditor_Tool_Problems = true;
    #endregion

    #region Sound Editor
    ///------------------------------------------------------------
    /// Sound Editor (FMOD / Wwise)
    ///------------------------------------------------------------
    public float SoundEditor_Volume = 0.8f;
    public bool SoundEditor_Loop = false;
    public bool SoundEditor_Play_On_Select = false;
    public bool Interface_SoundEditor_ToolWindow = true;
    public bool Interface_SoundEditor_Tool_Search = true;
    public string SoundEditor_Export_Folder = "";
    public string SoundEditor_Import_Folder = "";
    #endregion

    #region EMEVD Editor
    ///------------------------------------------------------------
    /// EMEVD Editor (event scripts)
    ///------------------------------------------------------------
    public bool EmevdEditor_Show_Arg_Names = true;
    public bool EmevdEditor_Structured_View = true;
    public bool EmevdEditor_Inline_MustSub_Calls = true;
    public bool EmevdEditor_Allow_Unmodified_Save = false;
    public bool EmevdEditor_Allow_Automatic_Save = true;
    public bool Interface_EmevdEditor_ToolWindow = true;
    public bool Interface_EmevdEditor_Tool_Palette = true;
    public bool Interface_EmevdEditor_Tool_Templates = true;
    public bool Interface_EmevdEditor_Tool_Problems = true;
    public bool Interface_EmevdEditor_Tool_Search = true;
    public bool Interface_EmevdEditor_Tool_MassEdit = true;
    public bool Interface_EmevdEditor_Tool_CalledBy = true;
    #endregion

    #region ESD Editor
    ///------------------------------------------------------------
    /// ESD Editor (EzState scripts)
    ///------------------------------------------------------------
    public bool EsdEditor_Code_View = true;
    public bool Interface_EsdEditor_ToolWindow = true;
    public bool Interface_EsdEditor_Tool_Palette = true;
    public bool Interface_EsdEditor_Tool_Search = true;
    public bool Interface_EsdEditor_Tool_CalledBy = true;
    #endregion

    #region FX Editor
    public bool FxEditor_Allow_Automatic_Save = true;

    // Effect List
    public bool FxEditor_EffectList_Display_Aliases = true;
    public bool FxEditor_EffectList_Group_By_Binder = false;

    // Action Inspector
    public bool FxEditor_EffectTree_Highlight_Selected = true;
    public bool FxEditor_EffectTree_Show_Summaries = true;
    public bool FxEditor_EffectTree_Show_Checks = true;
    public bool FxEditor_Inspector_Show_Raw = false;
    public bool FxEditor_Inspector_Show_Unknown = true;
    public bool FxEditor_Inspector_Show_Descriptions = true;

    // Viewport and playback
    public bool FxEditor_Viewport_Display_Emitter_Shapes = true;
    public bool FxEditor_Viewport_Display_Node_Axes = true;
    public bool FxEditor_Viewport_Display_Particle_Markers = false;
    public bool FxEditor_Viewport_Display_Billboards = true;
    public bool FxEditor_Viewport_Display_Screen_Effects = true;
    public bool FxEditor_Viewport_Display_Lens_Flares = true;
    public bool FxEditor_Viewport_Display_Gpu_Particles = true;
    public float FxEditor_Viewport_Billboard_Brightness = 1.0f;
    public bool FxEditor_Viewport_Display_Models = true;
    public bool FxEditor_Viewport_Display_Lights = true;
    public bool FxEditor_Viewport_Display_Light_Markers = true;
    public float FxEditor_Viewport_Light_Brightness = 1.0f;
    public float FxEditor_Playback_Speed = 1.0f;
    public bool FxEditor_Playback_Loop = true;
    public float FxEditor_Playback_Length = 5.0f;
    public int FxEditor_Preview_Motion = 0;
    public float FxEditor_Preview_Motion_Speed = 5.0f;
    public float FxEditor_Preview_Swing_Radius = 1.0f;
    public float FxEditor_Preview_Swing_Speed = 360.0f;
    public bool FxEditor_Preview_Motion_Follow = true;
    public bool FxEditor_Viewport_Auto_Frame = true;
    public bool FxEditor_Live_Preview = false;
    public bool FxEditor_Preview_Wind = false;
    public float FxEditor_Preview_Wind_Yaw = 0;
    public float FxEditor_Preview_Wind_Strength = 10;
    public bool FxEditor_Preview_Effect_Forces = true;

    // Tools window
    public bool Interface_FxEditor_ToolWindow = true;
    public bool Interface_FxEditor_CurveEditor = true;
    public bool Interface_FxEditor_Tool_Preview = true;
    public bool Interface_FxEditor_Tool_Search = true;
    public bool Interface_FxEditor_Tool_References = true;
    public bool Interface_FxEditor_Tool_Resources = true;
    public bool Interface_FxEditor_Tool_GridConfiguration = true;
    public bool Interface_FxEditor_Tool_ViewportRendering = true;
    #endregion

    #region Havok Editor
    public bool HavokEditor_Allow_Unmodified_Save = false;
    public bool HavokEditor_Allow_Automatic_Save = true;

    public bool Interface_HavokEditor_ToolWindow = true;

    public bool HavokEditor_BinderList_Display_Full_Path = false;
    public bool HavokEditor_BinderList_Display_Aliases = true;

    public bool HavokEditor_FileList_Display_Full_Path = false;
    public bool HavokEditor_FileList_Display_Aliases = true;
    public int HavokEditor_FileList_Collision_Type_Filter = 0;

    public bool HavokEditor_EntryList_Display_Aliases = true;

    public bool HavokEditor_Properties_Display_Community_Names = false;
    public bool HavokEditor_Properties_Display_Type_Column = false;
    public bool HavokEditor_Properties_Display_Raw_Data_Fields = true;
    public bool HavokEditor_Properties_Auto_Open_Tree = false;
    public bool HavokEditor_Properties_Display_Property_Bags = false;

    public bool HavokEditor_ToolVisibility_CollisionGenerator = true;
    public bool HavokEditor_ToolVisibility_ScriptReloader = true;

    // Whether the Collision Generator shows its FLVER mesh list and its 3D preview
    public bool HavokEditor_CollisionGen_Show_Flver_Meshes = true;
    public bool HavokEditor_CollisionGen_Show_Preview = true;
    public bool HavokEditor_CollisionGen_Show_Obj_Groups = true;

    // The axes an OBJ was exported with, matching the Model Editor's OBJ export settings, so it can be brought back
    // into the game's space
    public bool HavokEditor_CollisionGen_Obj_MirrorX = false;
    public bool HavokEditor_CollisionGen_Obj_MirrorY = false;
    public bool HavokEditor_CollisionGen_Obj_MirrorZ = false;
    public ModelExportAxis HavokEditor_CollisionGen_Obj_ForwardAxis = ModelExportAxis.PosZ;
    public ModelExportAxis HavokEditor_CollisionGen_Obj_UpAxis = ModelExportAxis.PosY;

    #endregion

    #region Viewport
    ///------------------------------------------------------------
    /// Viewport
    ///------------------------------------------------------------
    public float Viewport_Frame_Rate = 60.0f;
    public bool Viewport_Enable_Rendering = true;
    public bool Viewport_Enable_Texturing = true;

    // 0 Editor: fullbright, with the lighting, shadows, GI, fog and screen effects off. 1 Game: those follow the
    // Rendering tool's settings. See RenderMode.
    public int Viewport_Render_Mode = 0;

    // 0 Diffuse: the textured image. 1 Normal: the surface's normals in its place, the Normals surface view.
    public int Viewport_Texturing_Mode = 0;

    // The wireframe view: lines along the triangles of FLVER models, drawn by the shaders' _wire variants (see
    // MeshRenderables.cWireframePipelines). Only: the surfaces are left out, so only the lines show. The color is
    // a display color, 0 to 1; the opacity is how much the lines cover the surfaces when they are shown.
    public bool Viewport_Wireframe_Enable = false;
    public bool Viewport_Wireframe_Only = false;
    public Vector3 Viewport_Wireframe_Line_Color = new Vector3(0.85f, 0.9f, 1.0f);
    public float Viewport_Wireframe_Line_Opacity = 0.75f;
    public float Viewport_Wireframe_Line_Width = 1.0f;

    public bool Viewport_Enable_Material_Animation = true;
    public float Viewport_Wind_Strength = 1.0f;
    public float Viewport_Wind_Direction = 45.0f;
    public bool Viewport_Material_Wind = true;
    public bool Viewport_Material_Vertex_Animation = true;
    public bool Viewport_Material_Sway = true;
    public bool Viewport_Material_Birds = true;
    public bool Viewport_Material_Layer_Blending = true;
    public bool Viewport_Material_Overlays = true;
    public bool Viewport_Material_Magma = true;
    public bool Viewport_Material_Emissive = true;
    public bool Viewport_Material_UV_Scroll = true;
    public bool Viewport_Material_Unlit = true;
    public bool Viewport_Material_Water = true;
    public bool Viewport_Material_Fur = true;
    public bool Viewport_Enable_Culling = true;

    public bool Viewport_Enable_Selection_Tint = true;
    public bool Viewport_Enable_Model_Masks = true;
    public bool Viewport_Enable_LOD_Facesets = false;

    public bool Viewport_Display = true;
    public bool Viewport_Display_Profiling = true;

    public Vector3 Viewport_Frame_Offset = new Vector3();
    public float Viewport_Frame_Distance = 1f;

    public bool Viewport_Render_Gizmos = true;
    public float Viewport_Gizmo_Size_Distance_Scale = 0.04f;

    // Degrees per step while snapping a gizmo rotation (Ctrl held), 0 for none
    public float Viewport_Gizmo_Snap_Rotation_Step = 15f;

    // Viewport Grids (shared by every grid)
    public int Viewport_Grid_Major_Line_Interval = 10;
    public bool Viewport_Grid_Display_Axis_Lines = true;
    public float Viewport_Grid_Opacity = 1.0f;
    public float Viewport_Grid_Fade_Distance = 2000f;
    public float Viewport_Grid_Line_Width = 1f;
    public float Viewport_Grid_Hidden_Opacity = 0f;

    // Camera
    public float Viewport_Camera_FOV { get; set; } = 60.0f;
    public float Viewport_Camera_MoveSpeed_Slow { get; set; } = 1.0f;
    public float Viewport_Camera_MoveSpeed_Normal { get; set; } = 20.0f;
    public float Viewport_Camera_MoveSpeed_Fast { get; set; } = 200.0f;
    public float Viewport_Camera_Sensitivity { get; set; } = 0.0160f;

    public float Viewport_Perspective_Near_Clip { get; set; } = 0.1f;
    public float Viewport_Perspective_Far_Clip { get; set; } = 50000.0f;

    // The clip round the camera, see SceneParam.CameraClip
    public bool Viewport_Camera_Clip_Enabled { get; set; } = false;
    public float Viewport_Camera_Clip_Radius { get; set; } = 5.0f;

    public float Viewport_Orthographic_Near_Clip { get; set; } = -50000.0f;
    public float Viewport_Orthographic_Far_Clip { get; set; } = 50000.0f;

    public float Viewport_DefaultOrthographicSize { get; set; } = 10.0f;
    public float Viewport_DefaultObliqueAngle { get; set; } = 45.0f;
    public float Viewport_DefaultObliqueScaling { get; set; } = 0.5f;
    public float Viewport_MousePan_Sensitivity { get; set; } = 25.0f;
    public bool Viewport_Orthographic_Zoom_To_Cursor { get; set; } = true;
    public bool Viewport_Preset_View_Switch_To_Orthographic { get; set; } = true;
    public bool Viewport_Oblique_Snap_To_Axis { get; set; } = true;
    public float Viewport_Slice_Half_Depth { get; set; } = 2.5f;
    public bool Viewport_DisplayScaleBar { get; set; } = true;

    // Rendering Limits
    public uint Viewport_Limit_Buffer_Flver_Bone = 65536;
    public uint Viewport_Limit_Buffer_Indirect_Draw = 50000;

    // Wireframe Coloring
    public float Viewport_Wireframe_Color_Variance = 0.11f;

    public float GFX_Renderable_Default_Wireframe_Alpha = 100.0f;

    public Vector3 Viewport_Collision_Color = new Vector3(53, 157, 255);
    public Vector3 Viewport_High_Collision_Color = new Vector3(255, 160, 40);
    public Vector3 Viewport_FallProtection_Collision_Color = new Vector3(70, 210, 90);
    public Vector3 Viewport_Wall_Fill_Color = new Vector3(60, 230, 120);
    public Vector3 Viewport_Wall_Other_Collision_Color = new Vector3(190, 90, 255);
    public Vector3 Viewport_Wall_None_Color = new Vector3(255, 60, 60);
    public Vector3 Viewport_Wall_NoJump_Color = new Vector3(255, 170, 30);
    public Vector3 Viewport_Wall_Hover_Color = new Vector3(255, 255, 255);
    public float Viewport_Wall_Alpha = 45.0f;
    public Vector3 Viewport_Connect_Collision_Color = new Vector3(146, 57, 158);
    public Vector3 Viewport_Navmesh_Color = new Vector3(157, 53, 255);
    public Vector3 Viewport_Navmesh_Gate_Color = new Vector3(50, 220, 0);
    public float Viewport_Collision_Opacity = 1.0f;

    // The bake of the irradiance volumes to light maps with (AC6, ER, NR), the NN of mAA_BB_CC_DD_envmap_NN
    public int Viewport_GI_Bake = 0;

    // Whether models are lit by the irradiance volumes, and how brightly
    public bool Viewport_GI_Enable = true;
    public bool Viewport_GI_Auto_Exposure = true;
    public float Viewport_GI_Intensity = 1.0f;

    // Blending towards a second bake, as the game does between times of day; its textures load with the map
    public bool Viewport_GI_Bake_Blend = false;
    public int Viewport_GI_Blend_Bake = 1;
    public float Viewport_GI_Blend_Ratio = 0.5f;

    // Whether the environment map's reflections are scaled by the volumes' light, as the game does
    public bool Viewport_GI_Normalize_Reflections = true;

    // Lighting map viewports from the map's GPARAMs (AC6, ER, NR): the file looked in first ("" for mAA_BB_0000),
    // and the time of day on the GPARAMs' TimeOfDay scale, whose first daytime key is 4
    public bool Viewport_Map_Lighting = true;
    public string Viewport_Map_Lighting_File = "";
    public float Viewport_Map_Lighting_Hours = 4.0f;

    // Scales the GPARAM's exposure, for maps where the game's auto exposure would set it differently
    public float Viewport_Map_Lighting_Exposure_Scale = 1.0f;

    // Demon's Souls lights its maps from DrawParam (DrawParamLightingBank): the LightBank and ToneMapBank rows to
    // use, -1 for the one most of the map's pieces use
    public int Viewport_Map_DrawParam_Light_Row = -1;
    public int Viewport_Map_DrawParam_ToneMap_Row = -1;
    public int Viewport_Map_DrawParam_Shadow_Row = -1;
    public int Viewport_Map_DrawParam_ToneCorrect_Row = -1;
    public int Viewport_Map_DrawParam_Fog_Row = -1;

    // Whether each part is lit by the LightBank row its own LightID picks, rather than all by one row
    public bool Viewport_Map_DrawParam_Per_Part = true;

    // Whether Demon's Souls' map materials are lit as the game's HemEnv shaders light them, by the hemisphere and the
    // LightBank row's env cubemaps instead of the sun; off, they take the sun as characters do
    public bool Viewport_Map_DrawParam_Env_Lighting = true;

    // Whether Demon's Souls' Light events light the viewport, at their region (PointLightID) as their PointLightBank
    // row (PointLightBankID) gives it
    public bool Viewport_Map_DrawParam_Point_Lights = true;

    // Whether the map lighting's colour correction (Demon's Souls' ToneCorrectBank) is drawn
    public bool Viewport_Post_Tone_Correction = true;

    // Whether the GPARAM's hemisphere light adds to the irradiance volumes' light, as well as lighting what they
    // don't cover; compared with the game, it does
    public bool Viewport_Map_Lighting_Hemi_Inside_Volumes = true;

    // DS3's lightmaps of its map pieces (the DOL textures of gi_env_mAA), loaded with the map and lighting the pieces
    // in place of the light probes, see DolLightmapBank
    public bool Viewport_Map_Lightmaps = true;

    // The viewport's scale on DS3's lightmaps, on top of the game's own (GparamProfile.LightmapScale), to match the
    // game while the game's scale of them isn't known
    public float Viewport_Map_Lightmap_Intensity = 1.0f;

    // A debug view of the map lighting in the flver shader: 0 off, 1 the source of the light from around (green
    // lightmap, blue light probes or volumes, red hemisphere, yellow a lightmapped material without its lightmap),
    // 2 that light alone, 3 the share of the sun let through
    public int Viewport_Lighting_Debug_View = 0;

    // Whether the viewport's background takes the GPARAM's BG color instead of Viewport_Bg_Color
    public bool Viewport_Map_Lighting_Background = true;

    // Whether characters are lit by the GPARAM LightSet's model set (its sun, fill lights and hemisphere) instead of
    // the scene's; off until compared with the game
    public bool Viewport_Map_Lighting_Model_Lights = false;

    // The weather to light with (WeatherParam's GparamId), 0 for none: the WW of the area's mAA_BB_CCWW files, and the
    // shared m00_00_WWWW_weatheroutdoor/_weatherbase overlays, which the game puts over outdoor areas
    public int Viewport_Map_Lighting_Weather = 0;

    // Whether the chosen weather's shared overlays are read, for an outdoor map; off for an interior
    public bool Viewport_Map_Lighting_Weather_Outdoor = true;

    // Wet surfaces in a weather that wets them, from the GPARAM's Wet group
    public bool Viewport_Post_Wet = true;

    // Whether wet surfaces are kept dry under roofs, by a depth map of the scene seen from above
    public bool Viewport_Post_Wet_Rain_Mask = true;

    // Elden Ring: whether the irradiance volumes' bakes follow the time of day (bake N is timezone N), loading the
    // bakes they need as the time moves, instead of the Global Illumination tab's bake settings
    public bool Viewport_Map_Lighting_Follow_Bakes = true;

    // The Model and Time Act Editors' viewport environment (AC6, ER, NR): the map whose GPARAM lighting and
    // irradiance bake light them ("" for the project type's default, see ViewportEnvironment), the GPARAM looked in
    // first ("" for the map's own), the volume of the bake (-1 for the one of median light), and the size of the
    // box it is stretched over, which is centred on the origin
    public string Viewport_Environment_Map = "";
    public string Viewport_Environment_Lighting_File = "";
    public int Viewport_Environment_Volume = -1;
    public float Viewport_Environment_Volume_Size = 30.0f;

    // How the viewports' HDR scene is taken to the screen, a ToneMapSetting: from the map's GPARAM Tone Map group,
    // or a fixed curve. The exposure is on top of the GPARAM's, and the white point is for a fixed Reinhard curve.
    public int Viewport_ToneMap = 0;
    public float Viewport_ToneMap_Exposure = 1.0f;
    public float Viewport_ToneMap_White_Point = 1.5f;

    // Whether the viewports' screen effects run (bloom, fog, light shafts... see PostProcessChain), from the map
    // GPARAM's groups. Each effect has its own switch as it is added.
    public bool Viewport_Post_Effects = true;

    // The GPARAM's auto exposure (Tone Map), bloom (Bloom's Glare fields), and FXAA after the tone map (Anti-Alias)
    public bool Viewport_Post_Auto_Exposure = true;
    public bool Viewport_Post_Bloom = true;

    // The viewport's scale on the bloom, over the GPARAM's GlareBloom Alpha, to match a game whose CPU-side bloom
    // scale isn't known (Sekiro)
    public float Viewport_Post_Bloom_Strength = 1.0f;

    // The GPARAM's Glow Filter, round what surfaces give off themselves, and the viewport's scale on it
    public bool Viewport_Post_Glow = true;
    public float Viewport_Post_Glow_Strength = 1.0f;
    public bool Viewport_Post_Fxaa = true;

    // The GPARAM's TAA (Anti-Alias Type 5, as Elden Ring's mapbase has), which replaces FXAA where it runs
    public bool Viewport_Post_Taa = true;

    // The GPARAM's VolumetricFog (Elden Ring's distance and height fog), with the sun's shadows through it and the
    // map's lights in it
    public bool Viewport_Post_Volumetric_Fog = true;

    // The older games' distance fog (Demon's Souls' FogBank), DistanceFogPass
    public bool Viewport_Post_Distance_Fog = true;
    public bool Viewport_Post_Fog_Shadows = true;
    public bool Viewport_Post_Fog_Local_Lights = true;

    // The viewport's scale on the fog's density, over the GPARAM's, to match a game whose CPU-side density scale
    // isn't known (Sekiro's GameScale)
    public float Viewport_Post_Fog_Density_Scale = 1.0f;

    // Whether transparent flvers (water, glass) are fogged at their own depth, as the opaque scene behind them is
    public bool Viewport_Post_Fog_Transparents = true;

    // The GPARAM's LightShaft group: rays from the sun where the sky shows through
    public bool Viewport_Post_Light_Shafts = true;

    // The GPARAM's SSAO, and the viewport's scale on how much it darkens
    public bool Viewport_Post_Ssao = true;
    public float Viewport_Post_Ssao_Strength = 1.0f;

    // The GPARAM's SSR, and the viewport's scale on how much of the environment's reflection it replaces
    public bool Viewport_Post_Ssr = true;
    public float Viewport_Post_Ssr_Strength = 1.0f;

    // The GPARAM's ColorGrading LUT, applied after the tone map
    public bool Viewport_Post_Color_Grading = true;

    // A debug view of the scene's surface attachments in place of the image, see SurfaceView: 0 for none
    public int Viewport_Post_Surface_View = 0;

    // Sun shadows: cascaded shadow maps whose splits, fade and bias come from the map's GPARAM Shadows group, or
    // Elden Ring's mapbase without map lighting. The resolution is that of one cascade; the map holds four.
    public bool Viewport_Shadows = true;
    public int Viewport_Shadow_Resolution = 2048;
    public float Viewport_Shadow_Bias_Scale = 1.0f;
    public float Viewport_Shadow_Strength = 1.0f;

    // Whether MSB parts cast and receive shadows as their IsShadowSrc and IsShadowDest flags say; all do otherwise
    public bool Viewport_Shadows_Use_Msb_Flags = true;

    // The Viewport Rendering tool's Scene Lighting tab: the sun used without map lighting, and the light multipliers.
    // Copied into every viewport's scene params each frame.
    public Vector3 Viewport_Scene_Light_Direction = new Vector3(1.0f, -0.5f, 0.0f);
    public Vector3 Viewport_Scene_Sun_Color = new Vector3(1.0f, 1.0f, 1.0f);
    public float Viewport_Scene_Sun_Intensity = 1.0f;
    public float Viewport_Scene_Direct_Light_Mult = 1.0f;
    public float Viewport_Scene_Indirect_Light_Mult = 1.0f;
    public float Viewport_Scene_Brightness = 1.0f;

    // The light that follows the camera, for lighting up dark interiors
    public bool Viewport_Camera_Light = false;
    public Vector3 Viewport_Camera_Light_Color = new Vector3(1.0f, 1.0f, 1.0f);
    public float Viewport_Camera_Light_Intensity = 8.0f;
    public float Viewport_Camera_Light_Range = 25.0f;

    // Whether the Viewport Rendering tool's tabs show their Status, Resolved Values and Debug sections
    public bool Viewport_Rendering_Show_Status = true;
    public bool Viewport_Rendering_Show_Resolved_Values = false;
    public bool Viewport_Rendering_Show_Debug = false;

    public Vector3 Viewport_Box_Region_Base_Color = Utils.GetDecimalColor(Color.Blue);
    public Vector3 Viewport_Box_Region_Highlight_Color = Utils.GetDecimalColor(Color.DarkViolet);

    public Vector3 Viewport_Cylinder_Region_Base_Color = Utils.GetDecimalColor(Color.Blue);
    public Vector3 Viewport_Cylinder_Region_Highlight_Color = Utils.GetDecimalColor(Color.DarkViolet);

    public Vector3 Viewport_Sphere_Region_Base_Color = Utils.GetDecimalColor(Color.Blue);
    public Vector3 Viewport_Sphere_Region_Highlight_Color = Utils.GetDecimalColor(Color.DarkViolet);

    public Vector3 Viewport_Point_Region_Base_Color = Utils.GetDecimalColor(Color.Yellow);
    public Vector3 Viewport_Point_Region_Highlight_Color = Utils.GetDecimalColor(Color.DarkViolet);

    public float Viewport_Region_Selection_Fill_Alpha = 20.0f;

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

    public Vector3 Viewport_Point_Light_Highlight_Color = Utils.GetDecimalColor(Color.White);

    public Vector3 Viewport_Spot_Light_Highlight_Color = Utils.GetDecimalColor(Color.Violet);

    public Vector3 Viewport_Directional_Light_Highlight_Color = Utils.GetDecimalColor(Color.AliceBlue);

    public float Viewport_Light_Intensity_Scale = 5.0f;

    // The Map Editor's BTL lights: those within this many metres of the camera light the viewports, nearest first,
    // up to the count; with the enable distance on, each also only within its own ViewDistance, as in game.
    // Selected lights always do.
    public bool Viewport_BTL_Lights_Near_Camera = true;
    public float Viewport_BTL_Light_Radius = 150.0f;
    public bool Viewport_BTL_Light_Use_Enable_Distance = true;
    public int Viewport_BTL_Light_Max_Count = 256;

    public Vector3 Viewport_Auto_Invade_Marker_Base_Color = Utils.GetDecimalColor(Color.Red);
    public Vector3 Viewport_Auto_Invade_Marker_Highlight_Color = Utils.GetDecimalColor(Color.DarkRed);
    public Vector3 Viewport_MultiPlayer_Wall_Base_Color = Utils.GetDecimalColor(Color.Gold);
    public Vector3 Viewport_MultiPlayer_Wall_Highlight_Color = Utils.GetDecimalColor(Color.DarkGoldenrod);
    public Vector3 Viewport_Map_Limit_Wall_Base_Color = Utils.GetDecimalColor(Color.OrangeRed);
    public Vector3 Viewport_Map_Limit_Wall_Highlight_Color = Utils.GetDecimalColor(Color.DarkOrange);
    public Vector3 Viewport_Fog_Volume_Base_Color = Utils.GetDecimalColor(Color.Gray);
    public Vector3 Viewport_Fog_Volume_Highlight_Color = Utils.GetDecimalColor(Color.LightGray);

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

    public Vector3 Viewport_Bg_Color = new(0.1f, 0.1f, 0.1f);

    public float Viewport_Flat_Model_Brightness = 1.0f;
    public float Viewport_Flat_Model_Saturation = 0.5f;

    public bool Viewport_Enable_Selection_Outline = false;
    public Vector3 Viewport_Selection_Outline_Color = new(1.0f, 0.5f, 0.0f);
    public Vector3 Viewport_Selection_Tint_Color = new(1.0f, 0.5f, 0.0f);
    public Vector3 Viewport_Untextured_Selection_Tint_Color = new(0.85f, 0.0f, 1.0f);
    public float Viewport_Selection_Tint_Strength = 1.0f;
    public bool Viewport_Enable_Selection_Dithering = true;
    public float Viewport_Selection_Dither_Opacity = 0.9f;

    public bool Viewport_DisplayControls = true;
    public bool Viewport_DisplayRotationIncrement = true;
    public bool Viewport_DisplayTranslationIncrement = true;
    public bool Viewport_DisplayActiveMap = true;

    public bool Viewport_Enable_Box_Selection = true;
    public bool Viewport_Enable_Box_Selection_MapPiece = true;
    public bool Viewport_Enable_Box_Selection_Asset = true;
    public bool Viewport_Enable_Box_Selection_Enemy = true;
    public bool Viewport_Enable_Box_Selection_Player = true;
    public bool Viewport_Enable_Box_Selection_Collision = true;
    public bool Viewport_Enable_Box_Selection_Light = true;
    public bool Viewport_Enable_Box_Selection_Region = true;

    // Viewport grids, see ViewportGridSettings
    public ViewportGridSet MapEditor_Grids = new();
    public ViewportGridSet ModelEditor_Grids = new();
    public ViewportGridSet TimeActEditor_Grids = new();
    public ViewportGridSet GparamEditor_Grids = new();

    // Effects are small, so the FX Editor shows a 1 m grid from the start
    public ViewportGridSet FxEditor_Grids = new()
    {
        Primary = new() { Display = true, Size = 50, SectionSize = 1, FollowCamera = true }
    };

    // Anim Editor: Primary Grid
    public bool AnimEditor_DisplayPrimaryGrid = false;
    public bool AnimEditor_RegeneratePrimaryGrid = false;
    public int AnimEditor_PrimaryGrid_Size = 1000;
    public float AnimEditor_PrimaryGrid_SectionSize = 10;

    public Vector3 AnimEditor_PrimaryGrid_Color = new Vector3(0.5f, 0.5f, 0.5f);

    public float AnimEditor_PrimaryGrid_Position_X = 0;
    public float AnimEditor_PrimaryGrid_Position_Y = 0;
    public float AnimEditor_PrimaryGrid_Position_Z = 0;

    public float AnimEditor_PrimaryGrid_Rotation_X = 0;
    public float AnimEditor_PrimaryGrid_Rotation_Y = 0;
    public float AnimEditor_PrimaryGrid_Rotation_Z = 0;

    // Anim Editor: Secondary Grid
    public bool AnimEditor_DisplaySecondaryGrid = false;
    public bool AnimEditor_RegenerateSecondaryGrid = false;
    public int AnimEditor_SecondaryGrid_Size = 1000;
    public float AnimEditor_SecondaryGrid_SectionSize = 10;

    public Vector3 AnimEditor_SecondaryGrid_Color = new Vector3(0.5f, 0.5f, 0.5f);

    public float AnimEditor_SecondaryGrid_Position_X = 0;
    public float AnimEditor_SecondaryGrid_Position_Y = 0;
    public float AnimEditor_SecondaryGrid_Position_Z = 0;

    public float AnimEditor_SecondaryGrid_Rotation_X = 90;
    public float AnimEditor_SecondaryGrid_Rotation_Y = 0;
    public float AnimEditor_SecondaryGrid_Rotation_Z = 0;

    // Anim Editor: Tertiary Grid
    public bool AnimEditor_DisplayTertiaryGrid = false;
    public bool AnimEditor_RegenerateTertiaryGrid = false;
    public int AnimEditor_TertiaryGrid_Size = 1000;
    public float AnimEditor_TertiaryGrid_SectionSize = 10;

    public Vector3 AnimEditor_TertiaryGrid_Color = new Vector3(0.5f, 0.5f, 0.5f);

    public float AnimEditor_TertiaryGrid_Position_X = 0;
    public float AnimEditor_TertiaryGrid_Position_Y = 0;
    public float AnimEditor_TertiaryGrid_Position_Z = 0;

    public float AnimEditor_TertiaryGrid_Rotation_X = 0;
    public float AnimEditor_TertiaryGrid_Rotation_Y = 0;
    public float AnimEditor_TertiaryGrid_Rotation_Z = 90;

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
                    throw new Exception($"{LOC.Get("SYS_CFG_Failed_To_Deserialize", file)}");
                }

                MigrateGridSettings(filestring);
            }
            catch (Exception e)
            {
                Smithbox.LogError<CFG>($"{LOC.Get("SYS_CFG_Failed_To_Read", file)}", e);

                Current = new CFG();
                Save();
            }
        }
    }

    /// <summary>
    /// Moves the grid settings of older configs into the grid sets. A failure only loses the old grid settings.
    /// </summary>
    private static void MigrateGridSettings(string json)
    {
        try
        {
            ViewportGridSettingsMigration.Migrate(Current, json);
        }
        catch (Exception e)
        {
            Smithbox.LogError<CFG>("Failed to migrate the viewport grid settings.", e);
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