using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.ProjectileEditor;
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
using System.Text.Json.Serialization.Metadata;
using static StudioCore.Editors.MapEditor.DuplicateAction;

namespace StudioCore.Application;

/// <summary>
///     The Map Editor's settings (<c>CFG.Current.MapEditor</c>), including its viewport grids. Saved in
///     Configuration.json under the same MapEditor_ keys as before they moved here.
/// </summary>
public sealed class MapEditorSettings : ICfgSection
{
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

    // Toggled in the Map List header: a map with several BTL files (_0000, _0001, ...) lights the viewport with the
    // _0000 one only, as the game uses one of them
    public bool MapEditor_Lights_Default_BTL_Variant_Only = true;

    // Tile outlines (ER, NR): toggled in the Map List header, colours by tile size (0-255)
    public bool MapEditor_Display_Tile_Outlines = false;
    public Vector3 MapEditor_Tile_Outline_Small_Color = new Vector3(60, 200, 255);
    public Vector3 MapEditor_Tile_Outline_Medium_Color = new Vector3(255, 210, 60);
    public Vector3 MapEditor_Tile_Outline_Large_Color = new Vector3(235, 90, 200);

    // Forbidden regions (ER .mfr boxes): toggled in the Map List header, per-type visibility and colours (0-255)
    public bool MapEditor_Display_Forbidden_Regions = false;
    public bool MapEditor_Forbidden_Region_Show_Save_Limit = true;
    public bool MapEditor_Forbidden_Region_Show_Multiplay_Limit = true;
    public Vector3 MapEditor_Forbidden_Region_Save_Limit_Color = new Vector3(255, 80, 60);
    public Vector3 MapEditor_Forbidden_Region_Multiplay_Limit_Color = new Vector3(170, 100, 255);
    public Vector3 MapEditor_Forbidden_Region_Selected_Color = new Vector3(255, 255, 255);
    public Vector3 MapEditor_Forbidden_Region_Hovered_Color = new Vector3(255, 230, 80);

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

    public bool MapEditor_Enable_Character_Substitution = false;
    public string MapEditor_Character_Substitution_ID = "c0000";
    public bool MapEditor_Enable_Character_Composition = true;
    public bool MapEditor_Hold_Idle_Poses_On_Load = false;

    // Map element effect playback (SFX, WindSFX and Message regions), shown from Viewport Filters
    public bool MapEditor_Display_Effects = false;
    public bool MapEditor_Effects_Sfx = true;
    public bool MapEditor_Effects_WindSfx = true;
    public bool MapEditor_Effects_MessageSfx = false;
    public bool MapEditor_Effects_Assets = true;
    public bool MapEditor_Effects_Selected_Only = false;
    public float MapEditor_Effects_Max_Distance = 150.0f;
    public int MapEditor_Effects_Max_Count = 48;

    // Regions drawn as a clickable icon at their centre while unselected, instead of their wireframe
    public bool MapEditor_Region_Icons = false;
    public bool MapEditor_Region_Icon_Occlusion = true;
    public float MapEditor_Region_Icon_Max_Distance = 500.0f;
    public Vector3 MapEditor_Region_Icon_Color = Utils.GetDecimalColor(Color.Blue);

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
    public bool MapEditor_Model_Selector_Load_Textures_On_Create = true;

    // Make Variant (ER, NR, AC6 assets): Claude/Documentation/Model Variants Plan.md §6
    public bool MapEditor_AssetVariant_Swap_Parts = true;
    public bool MapEditor_AssetVariant_Copy_Param_Rows = true;
    public bool MapEditor_AssetVariant_Use_Project_Group = false;
    public int MapEditor_AssetVariant_Project_Group = 900;

    // Variant tool (Tools window): which sections are open
    public bool MapEditor_VariantTool_Show_Make = true;
    public bool MapEditor_VariantTool_Show_Tint = true;
    public bool MapEditor_VariantTool_Show_Colour_Shift = true;
    public bool MapEditor_VariantTool_Show_Collision = true;

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

    public bool MapEditor_MapContentList_Auto_Open_Tree = false;

    // Map Contents colours objects that differ from, or aren't in, the vanilla map
    public bool MapEditor_Vanilla_Highlight = true;
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
    // Collision Editor, Material tab: the material painted, the brush radius (0 paints the face under the cursor
    // alone), the steepest angle from the hit face a painted face may have, and the overlay of each face's material
    public int MapEditor_CollisionEditor_Paint_Material = 1;
    public float MapEditor_CollisionEditor_Paint_Radius = 0f;
    public float MapEditor_CollisionEditor_Paint_Facing_Limit = 180f;
    public bool MapEditor_CollisionEditor_Material_Overlay = true;
    public float MapEditor_CollisionEditor_Material_Overlay_Alpha = 55f;
    public bool MapEditor_CollisionEditor_Show_Paint_Material = true;
    public bool MapEditor_CollisionEditor_Show_Material_Legend = true;
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
    public bool MapEditor_CollisionEditor_Show_Create_Preview = true;
    public bool MapEditor_CollisionEditor_Create_High = true;
    public bool MapEditor_CollisionEditor_Create_Low = true;
    public bool MapEditor_CollisionEditor_Create_Fall = false;
    public bool MapEditor_CollisionEditor_Show_Create_Fall = true;
    public HavokCollisionType MapEditor_CollisionEditor_Fall_Source = HavokCollisionType.High;
    public FallProtectionDirection MapEditor_CollisionEditor_Fall_Direction = FallProtectionDirection.All;
    public float MapEditor_CollisionEditor_Fall_Height = 1.8f;
    public float MapEditor_CollisionEditor_Fall_Height_AC6 = 10f;
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

    public bool MapEditor_Paste_At_Placement_Orb = true;

    // Duplicate to Map, Move to Map and Paste keep objects where they are in the world, rather than at the same
    // local coordinates in the other map (which in ER and NR is another tile, 256 m away)
    public bool MapEditor_Keep_World_Position_Across_Maps = true;

    // Snap to Mesh
    public int MapEditor_SnapToMesh_Direction = 0;
    public float MapEditor_SnapToMesh_Offset = 0.0f;
    public float MapEditor_SnapToMesh_MaxDistance = 500.0f;
    public bool MapEditor_SnapToMesh_Individually = false;

    // Param Properties
    public bool MapEditor_Field_List_Display_Padding = false;
    public bool MapEditor_Field_List_Display_References = false;

    public bool MapEditor_Allow_Unmodified_Save = false;

    // Save Check: File › Save checks the maps it will write first, and stops on errors (or warnings too)
    public bool MapEditor_SaveCheck_Enabled = true;
    public bool MapEditor_SaveCheck_Stop_On_Warnings = false;
    public bool MapEditor_Allow_Automatic_Save = true;

    // Navigation tool: the tabs whose "How this works" section has been shown once, so it starts closed on them
    public List<string> MapEditor_Navigation_Help_Seen = new List<string>();
    public List<string> MapEditor_CollisionEditor_Help_Seen = new List<string>();

    // Viewport grids, see ViewportGridSettings
    public ViewportGridSet MapEditor_Grids = new();

    // Toolbar actions (Move to Camera and the other actions; the Model Editor also reads Toolbar_Move_to_Camera_Offset)
    // Toolbar
    public bool Toolbar_Duplicate_Increment_Entity_ID = false;
    public bool Toolbar_Duplicate_Increment_PartNames = false;
    public bool Toolbar_Duplicate_Increment_InstanceID = true;
    public bool Toolbar_Duplicate_Place_at_List_End = false;

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

    public bool Toolbar_ShowReplicatorMenu = false;

    public int Toolbar_EntityGroupID = 0;
    public string Toolbar_EntityGroup_Attribute = "";

    // Replicator tool
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

    // Scrambler tool
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

    // Prefabs
    public bool Prefab_IncludeEntityID = true;
    public bool Prefab_IncludeEntityGroupIDs = true;

    public bool Prefab_ApplyUniqueInstanceID = true;
    public bool Prefab_ApplyUniqueEntityID = false;
    public bool Prefab_ApplySpecificEntityGroupID = false;
    public int Prefab_SpecificEntityGroupID = 0;
    public bool Prefab_PlaceAtPlacementOrb = true;

    public bool Prefab_ApplyOverrideName = false;
    public string Prefab_OverrideName = "";

    // MapDataEditor_
    ///------------------------------------------------------------
    /// Map Data Editor
    ///------------------------------------------------------------
    // General
    public bool MapDataEditor_CategoryView_Display_Empty_Base_Categories = true;
    public bool MapDataEditor_CategoryView_Display_Empty_Sub_Categories = true;

    // Entry view: the field columns shown for each entry type, keyed by the type's full name.
    // A type without an entry shows the default columns.
    public Dictionary<string, List<string>> MapDataEditor_EntryView_Columns = new Dictionary<string, List<string>>();

    // Entry view: an entity selected elsewhere (viewport, Map Content) is shown, switching category if needed
    public bool MapDataEditor_EntryView_Follow_Selection = true;

    // MapInfo_
    public int MapInfo_BrushSize = 4;
    public float MapInfo_Opacity = 0.6f;
    public bool MapInfo_HighlightSelected = false;
    public bool MapInfo_ShowTileOutlines = true;

    // WorldMap
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

    // QuickView_
    public bool QuickView_DisplayTooltip = false;
    public List<string> QuickView_TargetProperties = new List<string>() { "Name" };

    // PlacementOrb_
    public float PlacementOrb_Distance = 5.0f;

    // DisplayPlacementOrb
    public bool DisplayPlacementOrb = false;

    // LastSceneFilter
    // Scene Filters
    public RenderFilter LastSceneFilter { get; set; } = RenderFilter.All ^ RenderFilter.Light;

    // Saved
    ///------------------------------------------------------------
    /// Misc
    ///------------------------------------------------------------
    public Vector3 SavedPosition = new Vector3();
    public Vector3 SavedRotation = new Vector3();
    public Vector3 SavedScale = new Vector3();
}
